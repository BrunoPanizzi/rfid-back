using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;

using Backend.Database;
using System.Data.Common;
using Respawn;
using Npgsql;

namespace tests.Setup;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Define the PostgreSQL container
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("rfid_test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithLogger(NullLogger.Instance)
        .Build();

    private DbConnection? _dbConnection;
    private Respawner? _respawner;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Clear verbose logging
        builder.ConfigureLogging(logging => logging.ClearProviders());

        // Use the created PostgreSQL container
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.Migrate();
        });
    }

    public async Task ResetDatabaseAsync()
    {
        if (_dbConnection == null)
        {
            // Create a fresh connection directly using the connection string
            _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
            await _dbConnection.OpenAsync();
        }

        _respawner ??= await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"]
        });

        await _respawner.ResetAsync(_dbConnection);
    }
}
