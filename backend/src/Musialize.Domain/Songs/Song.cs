namespace Musialize.Domain.Songs;

/// <summary>
/// A piece of music a Group is considering or playing. Until the Organizers decide on it, a Song is a Suggestion.
/// </summary>
public sealed class Song
{
    // For EF Core.
    private Song()
    {
    }

    public Guid Id { get; private set; }

    public Guid GroupId { get; private set; }

    public string Title { get; private set; } = "";

    public string Artist { get; private set; } = "";

    /// <summary>A recording or video, so everyone knows which version is meant.</summary>
    public string Link { get; private set; } = "";

    public DateTimeOffset SuggestedAt { get; private set; }

    /// <summary>
    /// A new Suggestion. Title and artist are trimmed, because they decide whether a Song is a duplicate.
    /// </summary>
    public static Song Suggest(Guid groupId, string title, string artist, string link, DateTimeOffset now) => new()
    {
        Id = Guid.CreateVersion7(now),
        GroupId = groupId,
        Title = title.Trim(),
        Artist = artist.Trim(),
        Link = link.Trim(),
        SuggestedAt = now,
    };
}
