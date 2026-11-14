using Microsoft.EntityFrameworkCore;
using Musialize.Application.Persistence;
using Musialize.Domain.Songs;

namespace Musialize.Application.Songs;

public sealed record SongResponse(Guid Id, string Title, string Artist, string Link, DateTimeOffset SuggestedAt)
{
    public static SongResponse From(Song song) => new(song.Id, song.Title, song.Artist, song.Link, song.SuggestedAt);
}

/// <summary>The Group's Suggestions for the Overview, oldest first: a queue to review in order.</summary>
public sealed class GetSuggestionsHandler(IAppDbContext db)
{
    public Task<SongResponse[]> HandleAsync(Guid groupId, CancellationToken cancellationToken) =>
        db.Songs.AsNoTracking()
            .Where(s => s.GroupId == groupId)
            .OrderBy(s => s.SuggestedAt)
            .Select(s => new SongResponse(s.Id, s.Title, s.Artist, s.Link, s.SuggestedAt))
            .ToArrayAsync(cancellationToken);
}
