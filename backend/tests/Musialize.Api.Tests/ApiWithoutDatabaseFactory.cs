using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Musialize.Api.Tests;

/// <summary>The API without a database, for tests of cross-cutting behaviour. It captures every log entry.</summary>
public sealed class ApiWithoutDatabaseFactory : WebApplicationFactory<Program>
{
    public CapturedLogs Logs => Services.GetRequiredService<CapturedLogs>();

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureServices(services => services.AddCapturedLogs());
}

/// <summary>Paths used in tests.</summary>
internal static class TestEndpoints
{
    /// <summary>A path that no endpoint handles.</summary>
    public const string Unknown = "/api/does-not-exist";
}
