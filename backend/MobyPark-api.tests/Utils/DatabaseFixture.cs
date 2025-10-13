using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace MobyPark_api.tests
{
    /// <summary>
    /// This class is used to connect to the test database that is made as a docker container.
    /// </summary>
    public class DatabaseFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container = new PostgreSqlBuilder().Build();

        public DbContextOptions<AppDbContext> DbOptions { get; private set; } = default!;
        private Respawner _respawn = null!;

        /// <summary>
        /// Starts the database container and respawn
        /// </summary>
        public async Task InitializeAsync()
        {
            // setup database container
            // note that we are using testcontainers so we create a new temporary docker container (you can see them coming by in docker if you open it during tests :) )
            await _container.StartAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_container.GetConnectionString())
                .Options;

            DbOptions = options;

            using var context = new AppDbContext(options);
            await context.Database.MigrateAsync();

            // setup respawn. respawn is used to quickly reset the database between tests
            await using var conn = new NpgsqlConnection(_container.GetConnectionString());
            await conn.OpenAsync();

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

        /// <summary>
        /// Create a new AppDbcontext. That's a connection to the database. NOTE it's a new CONNECTION. Not a new database, so state changes can bleed to other tests.
        /// </summary>
        /// <returns></returns>
        public AppDbContext CreateContext() => new AppDbContext(DbOptions);

        /// <summary>
        /// Reset the database to a empty state
        /// </summary>
        /// <returns></returns>
        public async Task ResetDB()
        {
            using DbConnection con = new NpgsqlConnection(GetConnectionString());
            await con.OpenAsync();
            await _respawn.ResetAsync(con);
        }

        internal string GetConnectionString()
        {
            return _container.GetConnectionString();
        }
    }
}