using Microsoft.EntityFrameworkCore;

namespace MobyPark_api.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            // apply any pending migrations
            await db.Database.MigrateAsync();

            if (!await db.ParkingLots.AnyAsync())
            {
                db.ParkingLots.AddRange(
                    new Data.Models.ParkingLot { Name = "Central", Location= "Rotterdam", Capacity = 120 },
                    new Data.Models.ParkingLot { Name = "Airport", Location ="Rotterdam", Capacity = 350 }
                );
            }

            if (!await db.Users.AnyAsync())
            {
                db.Users.AddRange(
                    new Data.Models.User { Email = "admin@mobypark.local", Name = "Admin" },
                    new Data.Models.User { Email = "user@mobypark.local", Name = "Test User" }
                );
            }

            await db.SaveChangesAsync();
        }
    }
}
