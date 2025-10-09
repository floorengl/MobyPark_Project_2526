using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Licenseplate> LicensePlates => Set<Licenseplate>();
    public DbSet<Session> Sessions => Set<Session>();
    

    // Finds and applies all configurations.
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
