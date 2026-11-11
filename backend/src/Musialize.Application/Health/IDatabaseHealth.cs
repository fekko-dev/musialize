namespace Musialize.Application.Health;

/// <summary>Checks whether the database can be reached and has every migration this build expects.</summary>
public interface IDatabaseHealth
{
    Task<DatabaseHealthReport> CheckAsync(CancellationToken cancellationToken);
}

/// <summary>The result of <see cref="IDatabaseHealth.CheckAsync"/>.</summary>
/// <param name="IsHealthy">True when the database can be reached and no migration is pending.</param>
/// <param name="LatestMigration">The newest applied migration, or null when none is applied or the database can't be reached.</param>
public sealed record DatabaseHealthReport(bool IsHealthy, string? LatestMigration);
