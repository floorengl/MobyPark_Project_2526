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
// Run imports (ORDER MATTERS)
Console.WriteLine("\n--- IMPORT SUMMARY ---");
ParkingLotImporter.Import(db, rawPath);
UserImporter.Import(db, rawPath);
// Returned message in CLI
Console.WriteLine("ALL IMPORTS DONE");
