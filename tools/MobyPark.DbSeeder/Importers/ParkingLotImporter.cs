using System.Globalization;
using System.Text.Json;
using MobyPark_api.Data;
using MobyPark_api.Data.Models;
using Microsoft.EntityFrameworkCore;


namespace MobyPark.DbSeeder.Importers;

public static class ParkingLotImporter
{
    public static async Task ImportAsync(AppDbContext db, string basePath)
    {
        // Checking if the json file exists.
        var path = Path.Combine(basePath, "parking-lots.json");
        if (!File.Exists(path))
        {
            Console.WriteLine("ParkingLot file not found, skipping.");
            return;
        }

        // Variables.
        await using var fs = File.OpenRead(path);
        var doc = await JsonDocument.ParseAsync(fs);
        int inserted = 0, updated = 0, skipped = 0;
        var skipReasons = new List<string>();

        // Loop through all in json file.
        foreach (var item in doc.RootElement.EnumerateObject())
        {
            var o = item.Value;
            // Check required fields are converted.
            if (!o.TryGetProperty("id", out var idProp) ||
                !o.TryGetProperty("name", out var nameProp) ||
                !o.TryGetProperty("location", out var locationProp))
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
            var name = nameProp.ToString();
            var location = locationProp.ToString();
            var address = o.TryGetProperty("address", out var a) ? a.ToString() : null;
            var capacity = (o.TryGetProperty("capacity", out var c) && c.TryGetInt64(out var cap)) ? cap : 0;
            var tariff = (o.TryGetProperty("tariff", out var tProp) && 
                (tProp.ValueKind == JsonValueKind.Number ? tProp.TryGetDecimal(out var tv) : decimal.TryParse(tProp.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out tv))) 
                ? tv : (decimal?)null;
            var dayTariff = ((o.TryGetProperty("day_tariff", out var dtProp) || o.TryGetProperty("daytariff", out dtProp)) && 
                (dtProp.ValueKind == JsonValueKind.Number ? dtProp.TryGetDecimal(out var dtv) : decimal.TryParse(dtProp.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out dtv))) 
                ? dtv : (decimal?)null;
            var coordinates = o.TryGetProperty("coordinates", out var v) && v.ValueKind == JsonValueKind.Object 
                ? $"{v.GetProperty("lat")},{v.GetProperty("lng")}" 
                : null;
            
            var lot = await db.ParkingLots.FirstOrDefaultAsync(p => p.Id == id);

            // Inserting or updating parkinglot in the database.
            if (lot == null)
            {
                db.ParkingLots.Add(new ParkingLot
                {
                    Id = id,
                    Name = name,
                    Location = location,
                    Address = address,
                    Capacity = capacity,
                    Tariff = tariff,
                    DayTariff = dayTariff,
                    Coordinates = coordinates,
                    CreatedAt = DateTime.UtcNow
                });

                inserted++;
            }
            else
            {
                lot.Name = name;
                lot.Location = location;
                lot.Address = address ?? lot.Address;
                lot.Capacity = capacity;
                lot.Tariff = tariff ?? lot.Tariff;
                lot.DayTariff = dayTariff ?? lot.DayTariff;
                lot.Coordinates = coordinates ?? lot.Coordinates;

                updated++;
            }
        }
        // Save changes of import to the database.
        await db.SaveChangesAsync();
        // Import Summary.
        Console.WriteLine($"ParkingLots → inserted={inserted}, updated={updated}, skipped={skipped}");
        skipReasons.Select(reason => $"- [{reason}").ToList().ForEach(Console.WriteLine);
    }
}
