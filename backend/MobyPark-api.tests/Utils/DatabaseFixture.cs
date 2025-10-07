using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace MobyPark_api.tests
{
    public class DatabaseFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
            .WithUsername("postgres")
            .WithPassword("devpassword123")
            .Build();

        public DbContextOptions<AppDbContext> DbOptions { get; private set; } = default!;


        public async Task InitializeAsync()
        {
            await _container.StartAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_container.GetConnectionString())
                .Options;

            DbOptions = options;

            using var context = new AppDbContext(options);
            await context.Database.MigrateAsync();
        }

        /// <summary>
        /// Disposes Docker test container
        /// </summary>
        public async Task DisposeAsync()
        {
            await _container.DisposeAsync();
        }

        public AppDbContext CreateContext() => new AppDbContext(DbOptions);
    }
}