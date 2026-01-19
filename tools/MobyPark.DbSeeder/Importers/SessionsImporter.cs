using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MobyPark_api.Data;
using MobyPark_api.Data.Models;

namespace MobyPark.DbSeeder.Importers;

public static class SessionsImporter
{
    public static async Task ImportAsync(AppDbContext db, string basePath)
    {
        // Checking if the sessions folder exists.
        var sessionsPath = Path.Combine(basePath, "sessions");
        if (!Directory.Exists(sessionsPath))
        {
            Console.WriteLine("Sessions folder not found, skipping.");
            return;
        }
        // Find all session files.
        var files = Directory.EnumerateFiles(sessionsPath, "p*-sessions.json", SearchOption.TopDirectoryOnly)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Ensure session files exist.
        if (files.Count == 0)
        {
            Console.WriteLine("No session files found (p*-sessions.json), skipping.");
            return;
        }

        // Variables
        const int maxFilesToImport = 5;
        const int batchSize = 10_000;
        int insertedSessions = 0, updatedSessions = 0, insertedPlates = 0, skipped = 0, filesProcessed = 0, sessionsProcessed = 0, sinceSave = 0;
        var plateCache = new Dictionary<string, Licenseplate>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            filesProcessed++;
            if (maxFilesToImport > 0 && filesProcessed > maxFilesToImport) break;
            // Parsing into objects.
            await using var fs = File.OpenRead(file);
            using var doc = await JsonDocument.ParseAsync(fs);
            foreach (var item in doc.RootElement.EnumerateObject())
            {
                sessionsProcessed++;
                var o = item.Value;
                // Variables for Sessions.
                var sessionId =
                    o.TryGetProperty("id", out var idProp) &&
                    long.TryParse(idProp.GetString() ?? idProp.ToString(), out var sid)
                        ? sid
                        : (long?)null;
                var parkingLotId =
                    o.TryGetProperty("parking_lot_id", out var lotProp) &&
                    long.TryParse(lotProp.GetString() ?? lotProp.ToString(), out var pid)
                        ? pid
                        : (long?)null;
                var plateText =
                    o.TryGetProperty("licenseplate", out var plateProp)
                        ? (plateProp.GetString() ?? plateProp.ToString())
                        : null;
                var startedUtc =
                    o.TryGetProperty("started", out var startedProp) &&
                    DateTime.TryParse(
                        startedProp.GetString(),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var sdt)
                        ? DateTime.SpecifyKind(sdt, DateTimeKind.Utc)
                        : (DateTime?)null;
                var stoppedUtc =
                    o.TryGetProperty("stopped", out var stoppedProp) &&
                    DateTime.TryParse(
                        stoppedProp.GetString(),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var edt)
                        ? DateTime.SpecifyKind(edt, DateTimeKind.Utc)
                        : (DateTime?)null;
                short ds;
                var durationMinutes =
                    o.TryGetProperty("duration_minutes", out var durProp) &&
                    short.TryParse(durProp.ToString(), out ds)
                        ? (short?)ds
                        : null;
                var cost =
                    o.TryGetProperty("cost", out var costProp) &&
                    (
                        (costProp.ValueKind == JsonValueKind.Number && costProp.TryGetDecimal(out var c))
                        || decimal.TryParse(costProp.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out c)
                    )
                        ? (decimal?)c
                        : null;

                // Validation.
                if (sessionId == null || parkingLotId == null || string.IsNullOrWhiteSpace(plateText) || startedUtc == null)
                {
                    skipped++;
                    continue;
                }
                // Ensure Licenseplate exists once.
                if (!plateCache.TryGetValue(plateText!, out var licensePlate))
                {
                    licensePlate = await db.LicensePlates.FirstOrDefaultAsync(lp => lp.LicensePlateName == plateText);
                    if (licensePlate == null)
                    {
                        licensePlate = new Licenseplate { LicensePlateName = plateText };
                        db.LicensePlates.Add(licensePlate);
                        insertedPlates++;
                    }
                    plateCache[plateText!] = licensePlate;
                }

                // Inserting or updating session in the database.
                var existingSession = db.Sessions.Local.FirstOrDefault(s => s.Id == sessionId.Value)
                     ?? await db.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId.Value);

                if (existingSession == null)
                {
                    db.Sessions.Add(new Session
                    {
                        ParkingLotId = parkingLotId.Value,
                        PlateText = plateText!,
                        Started = startedUtc.Value,
                        Stopped = stoppedUtc,
                        DurationMinutes = durationMinutes,
                        Cost = cost,
                        LicensePlate = licensePlate
                    });
                    insertedSessions++;
                }
                else
                {
                    existingSession.ParkingLotId = parkingLotId.Value;
                    existingSession.PlateText = plateText!;
                    existingSession.Started = startedUtc.Value;
                    existingSession.Stopped = stoppedUtc;
                    existingSession.DurationMinutes = durationMinutes;
                    existingSession.Cost = cost;
                    existingSession.LicensePlate = licensePlate;
                    updatedSessions++;
                }
                // Save the batch and clear.
                sinceSave++;
                if (sinceSave >= batchSize)
                {
                    await db.SaveChangesAsync();
                    db.ChangeTracker.Clear();
                    plateCache.Clear();
                    sinceSave = 0;
                }
            }
        }
        // Save changes and clear.
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        plateCache.Clear();
        // Import Summary.
        Console.WriteLine($"Sessions → inserted={insertedSessions}, updated={updatedSessions}, Plates inserted={insertedPlates}, skipped={skipped}");
    }
}
