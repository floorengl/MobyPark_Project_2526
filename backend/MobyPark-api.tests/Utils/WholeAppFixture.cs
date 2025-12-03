using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Microsoft.Extensions.DependencyInjection;
using MobyPark_api.tests;

public class WholeAppFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private DatabaseFixture _databaseFixture = new DatabaseFixture();
    public HttpClient HttpClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();


        HttpClient = CreateClient();
    }

    public new async Task DisposeAsync()
    {
        await _databaseFixture.DisposeAsync();
    }

    public async Task ResetDB()
    {
        await _databaseFixture.ResetDB();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("IsXUnitTesting", "True");
        // Environment.SetEnvironmentVariable("ConnectionStrings:Postgres", _dbContainer.GetConnectionString());
        builder.ConfigureServices(services =>
        {
            services.Remove(services.SingleOrDefault(service => typeof(DbContextOptions<AppDbContext>) == service.ServiceType));

            services.Remove(services.SingleOrDefault(service => typeof(DbConnection) == service.ServiceType));

            services.AddDbContext<AppDbContext>((_, option) => option.UseNpgsql(_databaseFixture.GetConnectionString()));
        });
    }
}