using Microsoft.EntityFrameworkCore;
using MobyPark_api.Data.Models;

namespace MobyPark_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<User> Users => Set<User>();
        public DbSet<ParkingLot> ParkingLots => Set<ParkingLot>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);
            b.Entity<User>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Email).IsUnique();
                e.Property(x => x.Email).IsRequired().HasMaxLength(200);
                e.Property(x => x.Name).IsRequired().HasMaxLength(120);
            });

            b.Entity<ParkingLot>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Name).IsRequired().HasMaxLength(120);
                e.Property(x => x.Capacity).IsRequired();
            });
        }
    }
}
