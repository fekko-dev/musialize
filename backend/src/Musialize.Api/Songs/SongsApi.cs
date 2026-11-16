using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Musialize.Application.Groups;
using Musialize.Application.Songs;

namespace Musialize.Api.Songs;

/// <summary>The Group's Songs, under /api/songs.</summary>
internal static class SongsApi
{
    // Every endpoint takes the Group from configuration until Sign-in (#21) puts it in the session.

    /// <summary>Maps the Songs API on its group, which Program.cs creates under /api.</summary>
    public static RouteGroupBuilder MapSongsApi(this RouteGroupBuilder group)
    {
        group.MapGet("", GetSuggestions).WithName("GetSuggestions");
        group.MapPost("", SuggestSong).WithName("SuggestSong").ProducesValidationProblem();
        // UseStatusCodePages gives the 404 a ProblemDetails body, which Results<…, NotFound> doesn't document.
        // nameof, unlike the other names: SuggestSong's CreatedAtRoute refers to it.
        group.MapGet("{id:guid}", GetSong).WithName(nameof(GetSong)).ProducesProblem(StatusCodes.Status404NotFound);
        return group;
    }

    private static Task<SongResponse[]> GetSuggestions(
        GetSuggestionsHandler handler, IOptions<DemoGroupOptions> demoGroup, CancellationToken cancellationToken) =>
        handler.HandleAsync(demoGroup.Value.Id, cancellationToken);

    private static async Task<CreatedAtRoute<SongResponse>> SuggestSong(
        SuggestSongRequest request, SuggestSongHandler handler, IOptions<DemoGroupOptions> demoGroup,
        CancellationToken cancellationToken)
    {
        var song = await handler.HandleAsync(demoGroup.Value.Id, request, cancellationToken);
        return TypedResults.CreatedAtRoute(song, nameof(GetSong), new { id = song.Id });
    }

    private static async Task<Results<Ok<SongResponse>, NotFound>> GetSong(
        Guid id, GetSongHandler handler, IOptions<DemoGroupOptions> demoGroup, CancellationToken cancellationToken) =>
        await handler.HandleAsync(demoGroup.Value.Id, id, cancellationToken) is { } song
            ? TypedResults.Ok(song)
            : TypedResults.NotFound();
}
