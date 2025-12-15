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
        // Licenseplate.
        b.Entity<Licenseplate>(l =>
        {
            l.ToTable("licenseplates");
            l.HasKey(x => x.Id);

            l.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("bigint");

            l.Property(x => x.LicensePlateName)
                .HasColumnName("license_plate_name")
                .HasMaxLength(10)
                .IsRequired();

            l.HasIndex(x => x.LicensePlateName).IsUnique();
        });
        // Session.
        b.Entity<Session>(s =>
        {
            s.ToTable("sessions");
            s.HasKey(x => x.Id);

            s.Property(x => x.Started).IsRequired();

            s.HasOne(x => x.LicensePlate)
                .WithMany(p => p.Sessions)
                .HasForeignKey(x => x.LicensePlateId)
                .OnDelete(DeleteBehavior.SetNull);
        });

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

        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
                .HasColumnName("id")
                .HasColumnType("bigint");

            e.Property(x => x.Username)
                .HasColumnName("username")
                .HasColumnType("text")
                .IsRequired();

            e.Property(x => x.Password)
                .HasColumnName("password")
                .HasColumnType("text")
                .IsRequired();

            e.Property(x => x.FullName)
                .HasColumnName("name")
                .HasColumnType("text");

            e.Property(x => x.Email)
                .HasColumnName("email")
                .HasColumnType("text");

            e.Property(x => x.Phone)
                .HasColumnName("phone")
                .HasColumnType("text");

            e.Property(x => x.Role)
                .HasColumnName("role")
                .HasColumnType("text");

            e.Property(x => x.CreatedAtUtc)
                .HasColumnName("created-at")
                .HasColumnType("timestamptz");

            e.Property(x => x.BirthYear)
                .HasColumnName("birth-year")
                .HasColumnType("smallint");

            e.Property(x => x.Active)
                .HasColumnName("active")
                .HasColumnType("boolean")
                .HasDefaultValue(true);

            e.HasIndex(x => x.Username)
                .IsUnique()
                .HasDatabaseName("ux_users_username");

            e.HasIndex(x => x.Email)
                .HasDatabaseName("ix_users_email");
        });
    }
}
