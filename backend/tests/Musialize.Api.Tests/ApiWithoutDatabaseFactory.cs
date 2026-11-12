using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Musialize.Application.Validation;

namespace Musialize.Api.Tests;

/// <summary>
/// The API without a database, for tests of cross-cutting behaviour. It captures every log entry
/// and maps <see cref="TestEndpoints"/> behind the app's own middleware.
/// </summary>
public sealed class ApiWithoutDatabaseFactory : WebApplicationFactory<Program>
{
    public CapturedLogs Logs => Services.GetRequiredService<CapturedLogs>();

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureServices(services =>
        {
            services.AddCapturedLogs();
            services.AddTransient<IStartupFilter, TestEndpoints>();
        });
}

/// <summary>Endpoints that exist only in tests.</summary>
internal sealed class TestEndpoints : IStartupFilter
{
    /// <summary>A path that no endpoint handles.</summary>
    public const string Unknown = "/api/does-not-exist";
    public const string Throw = "/api/test/throw";
    public const string ExceptionMessage = "Something only a developer should see.";
    public const string Validate = "/api/test/validate";

    // Added after the app's pipeline, so they run inside its middleware, like real endpoints.
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        next(app);
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet(Throw, ThrowException);
            endpoints.MapPost(Validate, (ValidationProbe _) => TypedResults.NoContent());
        });
    };

    private static IResult ThrowException() => throw new InvalidOperationException(ExceptionMessage);
}
