using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
using MobyPark_api.Data;
using MobyPark_api.Data.Models;

namespace MobyPark.DbSeeder.Importers;

public static class VehicleImporter
{
    public static void Import(AppDbContext db, string basePath)
    {
        // Checking if the json file exists.
        var path = Path.Combine(basePath, "vehicles.json");
        if (!File.Exists(path))
        {
            Console.WriteLine("Vehicle file not found, skipping.");
            return;
        }

        // Variables.
        var doc = JsonDocument.Parse(File.ReadAllText(path));
        int inserted = 0, updated = 0, skipped = 0;
        var skipReasons = new List<string>();

        // Loop through all in json file.
        foreach (var o in doc.RootElement.EnumerateArray())
        {
            // Check required fields are converted.
            if (!o.TryGetProperty("id", out var idProp) || 
                !o.TryGetProperty("license_plate", out var licenseplateProp) || 
                !o.TryGetProperty("user_id", out var useridProp))
            {
                skipped++;
                skipReasons.Add("Missing required JSON properties (id, username, or password).");
                continue;
            }
            // Check id can be converted into long.
            if (!long.TryParse(idProp.GetString(), out var id))
            {
                skipped++;
                skipReasons.Add("ID is not a valid number.");
                continue;
            }

            // All fields.
            var licensePlate = licenseplateProp.GetString() ?? "";
            var userId = long.TryParse(useridProp.GetString(), out var usid) ? usid : 0;
            var make = o.TryGetProperty("make", out var ma) ? ma.GetString() : null;
            var model = o.TryGetProperty("model", out var mo) ? mo.GetString() : null;
            var color = o.TryGetProperty("color", out var c) ? c.GetString() : null;
            DateTime? year = o.TryGetProperty("year", out var y) && y.TryGetInt32(out var yr) 
                ? new DateTime(yr, 1, 1, 0, 0, 0, DateTimeKind.Utc) 
                : null;
            var createdAt = o.TryGetProperty("created_at", out var ca) && DateTime.TryParse(ca.GetString(), out var dt)
                ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                : DateTime.UtcNow;

            var vehicle = db.Vehicles.FirstOrDefault(v => v.Id == id);

            // Inserting or updating user in the database.
            if (vehicle == null)
            {
                db.Vehicles.Add(new Vehicle{
                    Id = id,
                    LicensePlate = licensePlate,
                    Make = make,
                    Model = model,
                    Color = color,
                    Year = year,
                    UserId = userId,
                    CreatedAt = createdAt
                });
                inserted++;
            }
            else
            {
                vehicle.LicensePlate = licensePlate;
                vehicle.Make = make;
                vehicle.Model = model;
                vehicle.Color = color;
                vehicle.Year = year;
                vehicle.UserId = userId;
                vehicle.CreatedAt = createdAt;
                updated++;
            }
        }
        // Save changes of import to the database.
        db.SaveChanges();
        // Import Summary.
        Console.WriteLine($"Vehicle → inserted={inserted}, updated={updated}, skipped={skipped}"); 
        skipReasons.Select(reason => $"- [{reason}").ToList().ForEach(Console.WriteLine);
    }
}