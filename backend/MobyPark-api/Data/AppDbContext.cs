using Microsoft.EntityFrameworkCore;
using MobyPark_api.Data.Models;
using System.Reflection.Emit;
using Microsoft.Net.Http.Headers;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Licenseplate> LicensePlates => Set<Licenseplate>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<ParkingLot> ParkingLots => Set<ParkingLot>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    // Finds and applies all configurations.
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(b);
        b.Entity<User>()
        .HasMany(u => u.Vehicles)
        .WithOne(v => v.User)
        .HasForeignKey(v => v.UserId);
    }
}
