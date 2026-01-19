using System.Text.Json;
using MobyPark_api.Data;
using MobyPark_api.Data.Models;
using MobyPark_api.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace MobyPark.DbSeeder.Importers;

public static class ReservationsImporter
{
    public static async Task ImportAsync(AppDbContext db, string basePath)
    {
        // Checking if the json file exists.
        var path = Path.Combine(basePath, "reservations.json"); 
        if (!File.Exists(path))
        {
            Console.WriteLine("Reservations file not found, skipping.");
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
            if (!o.TryGetProperty("parking_lot_id", out var lotProp) ||
                !o.TryGetProperty("vehicle_id", out var vehProp) ||
                !o.TryGetProperty("start_time", out var startProp) ||
                !o.TryGetProperty("end_time", out var endProp))
            {
                skipped++;
                skipReasons.Add("Missing required JSON properties (id, username, or password).");
                continue;
            }

            // Lookup vehicle to get the license plate string.
            var vehicleId = long.TryParse(vehProp.GetString(), out var vid) ? vid : 0;
            var vehicle = db.Vehicles.FirstOrDefault(v => v.Id == vehicleId);
            if (vehicle == null)
            {
                skipped++;
                skipReasons.Add($"Vehicle ID {vehicleId} not found.");
                continue;
            }
            var actualLicensePlate = vehicle.LicensePlate;

            // All fields.
            var lotId = long.TryParse(lotProp.GetString(), out var lid) ? lid : 0;
            var startTime = DateTime.TryParse(startProp.GetString(), out var st) ? st.ToUniversalTime() : DateTime.UtcNow;
            var endTime = DateTime.TryParse(endProp.GetString(), out var et) ? et.ToUniversalTime() : DateTime.UtcNow;
            var createdAt = o.TryGetProperty("created_at", out var c) && DateTime.TryParse(c.GetString(), out var ca)
                            ? ca.ToUniversalTime() : DateTime.UtcNow;
            var cost = o.TryGetProperty("cost", out var co) && co.TryGetDecimal(out var cv) ? cv : (decimal?)null;
            var statusStr = o.TryGetProperty("status", out var s) ? s.GetString()?.ToLower() : "";   // Helper for status field
            var status = statusStr switch
            {
                "confirmed" => ReservationStatus.UnUsed,
                "used"      => ReservationStatus.Used,
                "unpaid"    => ReservationStatus.Unpaid,
                _           => ReservationStatus.Unpaid 
            };

            var reservation = await db.Reservations.FirstOrDefaultAsync(r => 
                r.ParkingLotId == lotId && 
                r.LicensePlate == actualLicensePlate && 
                r.StartTime == startTime);

            // Inserting or updating user in the database.
            if (reservation == null)
            {
                db.Reservations.Add(new Reservation
                {
                    Id = Guid.NewGuid(),
                    ParkingLotId = lotId,
                    LicensePlate = actualLicensePlate,
                    StartTime = startTime,
                    EndTime = endTime,
                    CreatedAt = createdAt,
                    Cost = cost,
                    Status = status
                });
                inserted++;
            }
            else
            {
                reservation.EndTime = endTime;
                reservation.Cost = cost;
                reservation.Status = status;
                updated++;
            }
        }
        // Save changes of import to the database.
        await db.SaveChangesAsync();
        // Import Summary.
        Console.WriteLine($"Reservations → inserted={inserted}, updated={updated}, skipped={skipped}"); 
        skipReasons.Select(reason => $"- [{reason}").ToList().ForEach(Console.WriteLine); 
    }
}