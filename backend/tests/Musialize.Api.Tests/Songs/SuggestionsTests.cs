using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Musialize.Domain.Groups;
using Musialize.Domain.Songs;
using Musialize.Infrastructure.Persistence;
using Shouldly;

namespace Musialize.Api.Tests.Songs;

[Collection(UsesDatabase.Name)]
public sealed class SuggestionsTests(ApiFactory factory) : IAsyncLifetime
{
    private static readonly Uri Suggestions = new("/api/songs", UriKind.Relative);

    public async ValueTask InitializeAsync() => await factory.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task SuggestionsAreListedOldestFirst()
    {
        var now = DateTimeOffset.UtcNow;
        await AddAsync(
            new Group(ApiFactory.GroupId, "Test Group"),
            Suggest("Newest", now),
            Suggest("Oldest", now.AddDays(-2)),
            Suggest("Middle", now.AddDays(-1)));
        using var client = factory.CreateClient();

        var suggestions = await client.GetFromJsonAsync<SongResponse[]>(Suggestions, TestContext.Current.CancellationToken);

        suggestions.ShouldNotBeNull().Select(song => song.Title).ShouldBe(["Oldest", "Middle", "Newest"]);
    }

    [Fact]
    public async Task AnotherGroupsSongsAreNotListed()
    {
        var otherGroupId = Guid.NewGuid();
        await AddAsync(
            new Group(ApiFactory.GroupId, "Test Group"),
            new Group(otherGroupId, "Other Group"),
            Song.Suggest(otherGroupId, "Wonderwall", "Oasis", "https://example.com/wonderwall", DateTimeOffset.UtcNow));
        using var client = factory.CreateClient();

        var suggestions = await client.GetFromJsonAsync<SongResponse[]>(Suggestions, TestContext.Current.CancellationToken);

        suggestions.ShouldNotBeNull().ShouldBeEmpty();
    }

    // Every restart of a development or staging app runs the seeder again.
    [Fact]
    public async Task TheDemoGroupIsSeededOnceWithItsSuggestionsOldestFirst()
    {
        for (var start = 0; start < 2; start++)
        {
            await using var seeded = factory.WithWebHostBuilder(builder => builder.UseSetting("DemoGroup:Seed", "true"));
            using var client = seeded.CreateClient();

            var suggestions = await client.GetFromJsonAsync<SongResponse[]>(Suggestions, TestContext.Current.CancellationToken);

            suggestions.ShouldNotBeNull().Select(song => song.Title).ShouldBe(["Wonderwall", "Mr. Brightside", "Africa"]);
        }
    }

    private static Song Suggest(string title, DateTimeOffset at) =>
        Song.Suggest(ApiFactory.GroupId, title, "Artist", "https://example.com/song", at);

    private async Task AddAsync(params object[] entities)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MusializeDbContext>();
        db.AddRange(entities);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private sealed record SongResponse(Guid Id, string Title, string Artist, string Link, DateTimeOffset SuggestedAt);
}
