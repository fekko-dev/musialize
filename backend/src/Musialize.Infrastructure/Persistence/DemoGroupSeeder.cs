using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Musialize.Application.Groups;
using Musialize.Domain.Groups;
using Musialize.Domain.Songs;

namespace Musialize.Infrastructure.Persistence;

/// <summary>
/// Adds the demo Group with three Suggestions at startup, unless it exists already. Registered only when
/// <see cref="DemoGroupOptions.Seed"/> is on: in development, the end-to-end tests and staging.
/// </summary>
internal sealed class DemoGroupSeeder(IServiceScopeFactory scopes, IOptions<DemoGroupOptions> options, TimeProvider clock) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MusializeDbContext>();
        var groupId = options.Value.Id;
        if (await db.Groups.AnyAsync(g => g.Id == groupId, cancellationToken))
        {
            return;
        }

        var now = clock.GetUtcNow();
        db.Groups.Add(new Group(groupId, "Demo Group"));
        db.Songs.AddRange(
            Song.Suggest(groupId, "Wonderwall", "Oasis", "https://www.youtube.com/watch?v=bx1Bh8ZvH84", now.AddDays(-21)),
            Song.Suggest(groupId, "Mr. Brightside", "The Killers", "https://www.youtube.com/watch?v=gGdGFtwCNBE", now.AddDays(-18)),
            Song.Suggest(groupId, "Africa", "Toto", "https://www.youtube.com/watch?v=FTQbiNvZqaY", now.AddDays(-11)));
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
