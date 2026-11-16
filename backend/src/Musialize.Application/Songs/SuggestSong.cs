using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Validation;
using Musialize.Application.Persistence;
using Musialize.Application.Validation;
using Musialize.Domain.Songs;

namespace Musialize.Application.Songs;

// On the properties, as the OpenAPI docs show for records: on the parameters, maxLength is missing from
// openapi.json (dotnet/aspnetcore#57486), and the web app checks its limits against it.
[ValidatableType]
public sealed record SuggestSongRequest(
    [property: Required, MaxLength(Song.TitleMaxLength)] string Title,
    [property: Required, MaxLength(Song.ArtistMaxLength)] string Artist,
    [property: Required, MaxLength(Song.LinkMaxLength), AbsoluteHttpUrl] string Link);

/// <summary>Adds a Suggestion to the Group.</summary>
public sealed class SuggestSongHandler(IAppDbContext db)
{
    public async Task<SongResponse> HandleAsync(Guid groupId, SuggestSongRequest request, CancellationToken cancellationToken)
    {
        var song = Song.Suggest(groupId, request.Title, request.Artist, request.Link, DateTimeOffset.UtcNow);
        db.Songs.Add(song);
        await db.SaveChangesAsync(cancellationToken);
        return SongResponse.From(song);
    }
}
