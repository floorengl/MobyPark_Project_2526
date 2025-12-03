using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Net.Http.Headers;
using MobyPark_api.Data.Models;

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
        b.HasPostgresExtension("uuid-ossp");
        base.OnModelCreating(b);
        b.Entity<User>()
        .HasMany(u => u.Vehicles)
        .WithOne(v => v.User)
        .HasForeignKey(v => v.UserId);
        b.Entity<Reservation>()
        .Property(r => r.Id)
        .HasDefaultValueSql("uuid_generate_v4()");
    }
}
