using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Musialize.Infrastructure.Persistence;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Musialize.Api.Tests;

/// <summary>
/// One PostgreSQL container per test run, migrated once. Tests that use it belong to
/// <see cref="UsesDatabase"/>, so they never run in parallel, and reset the data before each test.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18").Build();
    private Respawner? _respawner;

    public string ConnectionString => _postgres.GetConnectionString();

    /// <summary>The newest migration in the build, which <see cref="InitializeAsync"/> applies to the test database.</summary>
    public string LatestMigration { get; private set; } = "";

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        using var scope = Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<MusializeDbContext>().Database;
        await database.MigrateAsync();
        LatestMigration = database.GetMigrations().Last();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        _respawner ??= await CreateRespawnerAsync(connection);
        if (_respawner is not null)
        {
            await _respawner.ResetAsync(connection);
        }
    }

    // Respawn refuses a database without tables to reset. Until a migration adds a table,
    // there is nothing to reset, so this returns null and is tried again on the next reset.
    private static async Task<Respawner?> CreateRespawnerAsync(NpgsqlConnection connection)
    {
        try
        {
            return await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                // Without the history table, /health would report every migration as pending.
                TablesToIgnore = [HistoryRepository.DefaultTableName],
            });
        }
        catch (InvalidOperationException exception) when (exception.Message.StartsWith("No tables found", StringComparison.Ordinal))
        {
            return null;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder
            .UseSetting("ConnectionStrings:Musialize", ConnectionString)
            .ConfigureServices(services => services.AddCapturedLogs());

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class UsesDatabase : ICollectionFixture<ApiFactory>
{
    public const string Name = "Database";
}
