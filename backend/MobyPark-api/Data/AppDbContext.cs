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
            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("uuid_generate_v4()");

            e.Property(x => x.Amount)
                .HasColumnName("amount")
                .HasColumnType("numeric");

            e.Property(x => x.CreatedAt)
                .HasColumnName("createdat")
                .HasColumnType("timestamptz");

            e.Property(x => x.Status)
                .HasColumnName("status")
                .HasColumnType("int"); 

            e.Property(x => x.Hash)
                .HasColumnName("hash")
                .HasColumnType("text");

            e.Property(x => x.TransactionId)
                .HasColumnName("transaction_id")
                .HasColumnType("uuid");

            e.HasOne(x => x.TransactionData)
                .WithOne()
                .HasForeignKey<Payment>(x => x.TransactionId);
        });
        // Transaction data.
        b.Entity<TransactionData>(e =>
        {
            e.ToTable("transaction_data");
            e.HasKey(x => x.TransactionId);

            e.Property(x => x.TransactionId)
                .HasColumnName("transaction_id")
                .HasColumnType("uuid")
                .HasDefaultValueSql("uuid_generate_v4()");

            e.Property(x => x.Amount)
                .HasColumnName("amount")
                .HasColumnType("numeric");

            e.Property(x => x.Date)
                .HasColumnName("date")
                .HasColumnType("timestamptz");

            e.Property(x => x.Method)
                .HasColumnName("method")
                .HasColumnType("text");

            e.Property(x => x.Issuer)
                .HasColumnName("issuer")
                .HasColumnType("text");

            e.Property(x => x.Bank)
                .HasColumnName("bank")
                .HasColumnType("text");
        });
    }
}
