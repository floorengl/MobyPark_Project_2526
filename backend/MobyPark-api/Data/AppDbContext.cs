using Microsoft.EntityFrameworkCore;
using MobyPark_api.Enums;
using MobyPark_api.Data.Models;
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

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<TransactionData> Transactions => Set<TransactionData>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.HasPostgresExtension("uuid-ossp");
        // For config classes.
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        // User.
        b.Entity<User>()
            .HasMany(u => u.Vehicles)
            .WithOne(v => v.User)
            .HasForeignKey(v => v.UserId);
        // Reservation.
        b.Entity<Reservation>()
            .Property(r => r.Id)
            .HasDefaultValueSql("uuid_generate_v4()");
        // Payment.
        b.Entity<Payment>(e =>
        {
            e.ToTable("payments");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasDefaultValueSql("uuid_generate_v4()");
            e.Property(p => p.Hash)
                .IsRequired()
                .HasMaxLength(64);
            e.HasOne(p => p.TransactionData)
                .WithOne()
                .HasForeignKey<Payment>(p => p.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        // Transaction Data
        b.Entity<TransactionData>(e =>
        {
            e.ToTable("transactions");
            e.HasKey(t => t.TransactionId);
            e.Property(t => t.TransactionId).HasDefaultValueSql("uuid_generate_v4()");
        });
    }
}
