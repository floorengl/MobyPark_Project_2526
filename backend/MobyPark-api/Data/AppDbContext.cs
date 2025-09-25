using Microsoft.EntityFrameworkCore;
using MobyPark_api.Data.Models;

namespace MobyPark_api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets
        public DbSet<User> Users => Set<User>();
        public DbSet<ParkingLot> ParkingLots => Set<ParkingLot>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Apply all IEntityTypeConfiguration<> in this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // If you still have User config in OnModelCreating, keep it or move to a separate cfg class later
            modelBuilder.Entity<User>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Email).IsUnique();
                e.Property(x => x.Email).IsRequired().HasMaxLength(200);
                e.Property(x => x.Name).IsRequired().HasMaxLength(120);
            });
        }
    }
}
