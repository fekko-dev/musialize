using System.Reflection;
using Microsoft.AspNetCore.Http.HttpResults;
using Musialize.Application.Health;

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

        health.MapGet("", GetHealth)
            .WithName("GetHealth")
            .Produces<HealthResponse>(StatusCodes.Status503ServiceUnavailable);
    }

    // Unhealthy while the database can't be reached or a migration is pending (ADR-0009),
    // so a container is never marked healthy against a database it doesn't match.
    private static async Task<Results<Ok<HealthResponse>, JsonHttpResult<HealthResponse>>> GetHealth(
        IDatabaseHealth database,
        CancellationToken cancellationToken)
    {
        var report = await database.CheckAsync(cancellationToken);
        var status = report.IsHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy;
        var response = new HealthResponse(status, Version, report.LatestMigration);

        return report.IsHealthy
            ? TypedResults.Ok(response)
            : TypedResults.Json(response, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}

internal sealed record HealthResponse(HealthStatus Status, string Version, string? DatabaseMigration);

internal enum HealthStatus
{
    Healthy,
    Unhealthy,
}
