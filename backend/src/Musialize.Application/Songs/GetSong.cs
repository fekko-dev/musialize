using Microsoft.EntityFrameworkCore;
using Musialize.Application.Persistence;

namespace Musialize.Application.Songs;

/// <summary>One of the Group's Songs for its Song page, or null when the Group has no such Song.</summary>
public sealed class GetSongHandler(IAppDbContext db)
{
    public Task<SongResponse?> HandleAsync(Guid groupId, Guid songId, CancellationToken cancellationToken) =>
        db.Songs.AsNoTracking()
            .Where(s => s.GroupId == groupId && s.Id == songId)
            .Select(s => SongResponse.From(s))
            .SingleOrDefaultAsync(cancellationToken);
}
