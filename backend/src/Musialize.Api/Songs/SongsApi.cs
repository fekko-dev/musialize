using Microsoft.Extensions.Options;
using Musialize.Application.Groups;
using Musialize.Application.Songs;

namespace Musialize.Api.Songs;

/// <summary>The Group's Songs, under /api/songs.</summary>
internal static class SongsApi
{
    /// <summary>Maps the Songs API on its group, which Program.cs creates under /api.</summary>
    public static RouteGroupBuilder MapSongsApi(this RouteGroupBuilder group)
    {
        group.MapGet("", GetSuggestions).WithName("GetSuggestions");
        return group;
    }

    // The Group comes from configuration until Sign-in (#21) puts it in the session.
    private static Task<SongResponse[]> GetSuggestions(
        GetSuggestionsHandler handler, IOptions<DemoGroupOptions> group, CancellationToken cancellationToken) =>
        handler.HandleAsync(group.Value.Id, cancellationToken);
}
