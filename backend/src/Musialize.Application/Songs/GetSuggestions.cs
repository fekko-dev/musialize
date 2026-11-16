using Microsoft.EntityFrameworkCore;
using Musialize.Application.Persistence;

namespace Musialize.Application.Songs;

/// <summary>The Group's Suggestions for the Overview, oldest first: a queue to review in order.</summary>
public sealed class GetSuggestionsHandler(IAppDbContext db)
{
    public Task<SongResponse[]> HandleAsync(Guid groupId, CancellationToken cancellationToken) =>
        db.Songs.AsNoTracking()
            .Where(s => s.GroupId == groupId)
            .OrderBy(s => s.SuggestedAt)
            // Version 7 Ids follow creation time, so Songs suggested at the same moment keep a fixed order.
            .ThenBy(s => s.Id)
            .Select(s => SongResponse.From(s))
            .ToArrayAsync(cancellationToken);
}
