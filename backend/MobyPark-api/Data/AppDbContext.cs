using Microsoft.EntityFrameworkCore;
using MobyPark_api.Data.Models;
using System.Reflection.Emit;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

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
