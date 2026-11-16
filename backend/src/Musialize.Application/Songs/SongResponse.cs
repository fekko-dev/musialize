using Musialize.Domain.Songs;

namespace Musialize.Application.Songs;

public sealed record SongResponse(Guid Id, string Title, string Artist, string Link, DateTimeOffset SuggestedAt)
{
    // In a final Select, EF Core reads the columns and calls this on the client.
    public static SongResponse From(Song song) => new(song.Id, song.Title, song.Artist, song.Link, song.SuggestedAt);
}
