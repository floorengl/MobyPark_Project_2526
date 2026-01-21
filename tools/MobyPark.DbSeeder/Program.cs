using Microsoft.EntityFrameworkCore;
using MobyPark_api.Data;
using MobyPark.DbSeeder.Importers;

// Connection string
var conn = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrWhiteSpace(conn))
{
    Console.WriteLine("DATABASE_URL not set");
    return;
}

// DbContext
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(conn)
    .Options;

using var db = new AppDbContext(options);

// Base path for JSON files
var rawPath = Path.Combine(AppContext.BaseDirectory, "raw");

// Run imports (ORDER MATTERS).
Console.WriteLine("\n--- IMPORT SUMMARY ---");
await ParkingLotImporter.ImportAsync(db, rawPath);
await UserImporter.ImportAsync(db, rawPath);
await VehicleImporter.ImportAsync(db, rawPath);
await ReservationsImporter.ImportAsync(db, rawPath);
await PaymentsImporter.ImportAsync(db, rawPath);
await SessionsImporter.ImportAsync(db, rawPath);
// Returned message in CLI
Console.WriteLine("ALL IMPORTS DONE");
