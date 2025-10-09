using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
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
        private Respawner _respawn = null!;

        /// <summary>
        /// Startup the container and create a connection to it.
        /// </summary>
        public async Task InitializeAsync()
        {
            await _container.StartAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_container.GetConnectionString())
                .Options;

            DbOptions = options;

            using var context = new AppDbContext(options);
            await context.Database.MigrateAsync();

            await using var conn = new NpgsqlConnection(_container.GetConnectionString());
            await conn.OpenAsync();

            // setup respawn. respawn is used to quickly reset the database between tests
            _respawn = await Respawner.CreateAsync(conn, new RespawnerOptions
            {
                SchemasToInclude = new[]
                {
                    "public"
                },
                DbAdapter = DbAdapter.Postgres
            });
        }

        /// <summary>
        /// Disposes Docker test container
        /// </summary>
        public async Task DisposeAsync()
        {
            await _container.DisposeAsync();
        }

        public AppDbContext CreateContext() => new AppDbContext(DbOptions);

        public async Task ResetDB()
        {
            DbConnection con = new NpgsqlConnection(_container.GetConnectionString());
            await con.OpenAsync();
            await _respawn.ResetAsync(con);
        }
    }
}