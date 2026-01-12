using System.Globalization;
using System.Text.Json;
using MobyPark_api.Data;
using MobyPark_api.Data.Models;

namespace MobyPark.DbSeeder.Importers;

public static class ParkingLotImporter
{
    public static void Import(AppDbContext db, string basePath)
    {
        var path = Path.Combine(basePath, "parking-lots.json");
        if (!File.Exists(path))
        {
            Console.WriteLine("ParkingLot file not found, skipping.");
            return;
        }

        var doc = JsonDocument.Parse(File.ReadAllText(path));

        int inserted = 0, updated = 0, skipped = 0;

        foreach (var item in doc.RootElement.EnumerateObject())
        {
            var o = item.Value;

            if (!o.TryGetProperty("id", out var idProp) ||
                !o.TryGetProperty("name", out var nameProp) ||
                !o.TryGetProperty("location", out var locationProp))
            {
                skipped++;
                continue;
            }

            if (!long.TryParse(idProp.ToString(), out var id))
            {
                skipped++;
                continue;
            }

            var name = nameProp.ToString();
            var location = locationProp.ToString();

            // read optional fields safely
            var address = o.TryGetProperty("address", out var a) ? a.ToString() : null;
            var capacity = (o.TryGetProperty("capacity", out var c) && c.TryGetInt64(out var cap)) ? cap : 0;

            var tariff = ReadFloat(o, "tariff");
            var dayTariff = ReadFloat(o, "day_tariff", "daytariff"); // supports both keys
            var coordinates = ReadCoordinates(o); // converting {lat,lng} to "lat,lng"

            var lot = db.ParkingLots.FirstOrDefault(p => p.Id == id);

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

        db.SaveChanges();
        Console.WriteLine($"ParkingLots → inserted={inserted}, updated={updated}, skipped={skipped}");
    }

    private static float? ReadFloat(JsonElement o, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!o.TryGetProperty(key, out var v)) continue;

            // number: 1.9 or 11
            if (v.ValueKind == JsonValueKind.Number && v.TryGetDouble(out var d))
                return (float)d;

            // string: "1.9"
            if (v.ValueKind == JsonValueKind.String &&
                float.TryParse(v.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                return f;
        }
        return null;
    }

    private static string? ReadCoordinates(JsonElement o)
    {
        if (!o.TryGetProperty("coordinates", out var v)) return null;

        // coordinates object: {"lat":52.3,"lng":5.2}
        if (v.ValueKind == JsonValueKind.Object &&
            v.TryGetProperty("lat", out var lat) &&
            v.TryGetProperty("lng", out var lng) &&
            lat.TryGetDouble(out var la) &&
            lng.TryGetDouble(out var ln))
        {
            return $"{la.ToString(CultureInfo.InvariantCulture)},{ln.ToString(CultureInfo.InvariantCulture)}";
        }

        // fallback: store raw json/text
        return v.ValueKind == JsonValueKind.Object ? v.GetRawText() : v.ToString();
    }
}
