using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Musialize.Api.Tests.Logging;
using Npgsql;
using Serilog.Events;
using Shouldly;

namespace Musialize.Api.Tests.Health;

[Collection(UsesDatabase.Name)]
public sealed class HealthTests(ApiFactory factory) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await factory.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task HealthReportsHealthyWithTheVersionAndTheLatestMigration()
    {
        using var client = factory.CreateClient();

        var (statusCode, body) = await GetHealthAsync(client);

        statusCode.ShouldBe(HttpStatusCode.OK);
        body.GetProperty("status").GetString().ShouldBe("healthy");
        body.GetProperty("version").GetString().ShouldBe(BuiltVersion());
        body.GetProperty("databaseMigration").GetString().ShouldBe(factory.LatestMigration);
    }

    [Fact]
    public async Task HealthReportsUnhealthyWhileAMigrationIsPending()
    {
        var unmigrated = await CreateEmptyDatabaseAsync("unmigrated");
        await using var unmigratedFactory = FactoryFor(unmigrated);
        using var client = unmigratedFactory.CreateClient();

        var (statusCode, body) = await GetHealthAsync(client);

        statusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        body.GetProperty("status").GetString().ShouldBe("unhealthy");
        body.GetProperty("version").GetString().ShouldBe(BuiltVersion());
        body.GetProperty("databaseMigration").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task HealthReportsUnhealthyWhenTheDatabaseCantBeReached()
    {
        // Nothing listens on port 1.
        await using var unreachableFactory = FactoryFor("Host=localhost;Port=1;Database=musialize;Username=musialize;Password=musialize;Timeout=3");
        using var client = unreachableFactory.CreateClient();

        var (statusCode, body) = await GetHealthAsync(client);

        statusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        body.GetProperty("status").GetString().ShouldBe("unhealthy");
        body.GetProperty("databaseMigration").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    // Dokploy checks /health every 10 seconds; a healthy answer would flood the logs.
    [Fact]
    public async Task AHealthyHealthCheckIsNotLogged()
    {
        using var client = factory.CreateClient();
        var logs = factory.Services.GetRequiredService<CapturedLogs>();
        logs.Clear();

        var (statusCode, _) = await GetHealthAsync(client);

        statusCode.ShouldBe(HttpStatusCode.OK);
        LoggingTests.RequestLogFor(logs, "/health").ShouldBeEmpty();
    }

    [Fact]
    public async Task AnUnhealthyHealthCheckIsLoggedAsAWarning()
    {
        var unmigrated = await CreateEmptyDatabaseAsync("unmigrated_logged");
        await using var unmigratedFactory = FactoryFor(unmigrated);
        using var client = unmigratedFactory.CreateClient();

        var (statusCode, _) = await GetHealthAsync(client);

        statusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        LoggingTests.RequestLogFor(unmigratedFactory.Services.GetRequiredService<CapturedLogs>(), "/health")
            .ShouldHaveSingleItem().Level.ShouldBe(LogEventLevel.Warning);
    }

    private WebApplicationFactory<Program> FactoryFor(string connectionString) =>
        factory.WithWebHostBuilder(builder => builder.UseSetting("ConnectionStrings:Database", connectionString));

    private async Task<string> CreateEmptyDatabaseAsync(string name)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new NpgsqlConnection(factory.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS {name}; CREATE DATABASE {name}", connection);
        await command.ExecuteNonQueryAsync(cancellationToken);

        return new NpgsqlConnectionStringBuilder(factory.ConnectionString) { Database = name }.ConnectionString;
    }

    private static async Task<(HttpStatusCode StatusCode, JsonElement Body)> GetHealthAsync(HttpClient client)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var response = await client.GetAsync(new Uri("/health", UriKind.Relative), cancellationToken);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        return (response.StatusCode, body);
    }

    // The SHA passed in with -p:AppVersion=<sha>, or "dev" when nothing is passed in.
    private static string BuiltVersion()
    {
        var version = typeof(Program).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "AppVersion")
            .Value;
        version.ShouldNotBeNullOrWhiteSpace();
        return version;
    }
}
