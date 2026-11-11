using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Musialize.Application.Health;
using Musialize.Infrastructure.Persistence;

namespace Musialize.Infrastructure.Health;

internal sealed partial class DatabaseHealth(MusializeDbContext db, ILogger<DatabaseHealth> logger) : IDatabaseHealth
{
    public async Task<DatabaseHealthReport> CheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            var applied = (await db.Database.GetAppliedMigrationsAsync(cancellationToken)).ToList();
            var isPending = db.Database.GetMigrations().Except(applied).Any();
            return new DatabaseHealthReport(!isPending, applied.LastOrDefault());
        }
        // DbException: the database can't be reached. InvalidOperationException: EF Core can't
        // connect at all, for example because the connection string is missing.
        catch (Exception exception) when (exception is DbException or InvalidOperationException)
        {
            LogUnreachable(exception);
            return new DatabaseHealthReport(IsHealthy: false, LatestMigration: null);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "The database can't be reached.")]
    private partial void LogUnreachable(Exception exception);
}
