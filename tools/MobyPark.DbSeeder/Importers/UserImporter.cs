using System.Globalization;
using System.Text.Json;
using Microsoft.VisualBasic;
using MobyPark_api.Data;
using MobyPark_api.Data.Models;
using Microsoft.EntityFrameworkCore;


namespace MobyPark.DbSeeder.Importers;

public static class UserImporter
{
    public static async Task ImportAsync(AppDbContext db, string basePath)
    {
        // Checking if the json file exists.
        var path = Path.Combine(basePath, "users.json");
        if (!File.Exists(path))
        {
            Console.WriteLine("User file not found, skipping.");
            return;
        }

        // Variables.
        await using var fs = File.OpenRead(path);
        var doc = await JsonDocument.ParseAsync(fs);
        int inserted = 0, updated = 0, skipped = 0;
        var skipReasons = new List<string>();

        // Loop through all in json file.
        foreach (var o in doc.RootElement.EnumerateArray())
        {
            // Check required fields are converted.
            if (!o.TryGetProperty("id", out var idProp) || 
                !o.TryGetProperty("username", out var nameProp) || 
                !o.TryGetProperty("password", out var passwordProp))
            {
                skipped++;
                skipReasons.Add("Missing required JSON properties (id, username, or password).");
                continue;
            }
            // Check id can be converted into long.
            if (!long.TryParse(idProp.ToString(), out var id))
            {
                skipped++;
                skipReasons.Add("ID is not a valid number.");
                continue;
            }

            // All fields.
            var username = nameProp.GetString() ?? "";
            var password = passwordProp.GetString() ?? "";
            if (string.IsNullOrWhiteSpace(username)) { skipped++; continue; }
            var fullName = o.TryGetProperty("name", out var n) ? n.GetString() : null;
            var email = o.TryGetProperty("email", out var e) ? e.GetString() : null;
            var phone = o.TryGetProperty("phone", out var p) ? p.GetString() : null;
            var role = o.TryGetProperty("role", out var r) ? r.GetString() : "USER";
            var createdAt = o.TryGetProperty("created_at", out var c) && DateTimeOffset.TryParse(c.GetString(), out var ca)
                            ? ca.ToUniversalTime()
                            : DateTimeOffset.UtcNow;
            short? birthYear = o.TryGetProperty("birth_year", out var b) && b.TryGetInt16(out var by) ? by : null;
            var active = o.TryGetProperty("active", out var a) && a.ValueKind == JsonValueKind.False ? false : true;

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id) ?? db.Users.FirstOrDefault(u => u.Username == username);

            // Inserting or updating user in the database.
            if (user == null)
            {
                db.Users.Add(new User
                {
                    Username = username,
                    Password = password,
                    FullName = fullName,
                    Email = email,
                    Phone = phone,
                    Role = role,
                    CreatedAtUtc = createdAt,
                    BirthYear = birthYear,
                    Active = active
                });
                inserted++;
            }
            else
            {
                user.Username = username;

                if (!string.IsNullOrWhiteSpace(password))
                    user.Password = password;

                user.FullName = fullName ?? user.FullName;
                user.Email = email ?? user.Email;
                user.Phone = phone ?? user.Phone;
                user.Role = string.IsNullOrWhiteSpace(role) ? user.Role : role;
                user.BirthYear = birthYear ?? user.BirthYear;
                user.Active = active;
                user.CreatedAtUtc = createdAt;

                updated++;
            }
        }
        // Save changes of import to the database.
        await db.SaveChangesAsync();
        // Import Summary.
        Console.WriteLine($"Users → inserted={inserted}, updated={updated}, skipped={skipped}"); 
        skipReasons.Select(reason => $"- [{reason}").ToList().ForEach(Console.WriteLine);
    }
}