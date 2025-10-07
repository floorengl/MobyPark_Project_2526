using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
namespace MobyPark_api.tests
{
    //host = 127.0.0.1; Database = YourDatabaseName; Username = postgres; Password = XXXXX
    public class TestDatabaseFixture : IAsyncLifetime
    {
        //public TestDbContext(DbContextOptions<TestDbContext> options)
        //    : base(options) { }

        //public DbSet<User> Users { get; set; }
        private readonly PostgreSqlContainer _container;
        public DbContextOptions<AppDbContext> DbOptions { get; private set; } = default!;

        /// <summary>
        /// Create a new container for postgre in Docker. this container will be used to run our tests.
        /// </summary>
        public TestDatabaseFixture()
        {
            _container = new PostgreSqlBuilder()
                .WithUsername("postgres")
                .WithPassword("devpassword123")
                // .WithPortBinding(5434)
                .Build();
        }


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