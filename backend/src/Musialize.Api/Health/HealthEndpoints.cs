using System.Reflection;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Musialize.Api.Health;

internal static class HealthEndpoints
{
    private static readonly string Version = typeof(HealthEndpoints).Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(attribute => attribute.Key == "AppVersion")
        .Value ?? throw new InvalidOperationException("The build must set AppVersion.");

    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        var health = app.MapGroup("/health").WithTags("Health");

        health.MapGet("", GetHealth).WithName("GetHealth");
    }

    private static Ok<HealthResponse> GetHealth() =>
        TypedResults.Ok(new HealthResponse(HealthStatus.Healthy, Version));
}

internal sealed record HealthResponse(HealthStatus Status, string Version);

internal enum HealthStatus
{
    Healthy,
}
