using Microsoft.EntityFrameworkCore;
using Musialize.Application.Groups;
using Musialize.Domain.Groups;
using Musialize.Domain.Songs;

namespace Musialize.Infrastructure.Persistence;

/// <summary>
/// Adds the demo Group with three Suggestions after migrating, unless it exists already, through EF Core's
/// UseSeeding: the migration bundle and <c>dotnet ef database update</c> run it under the migration lock.
/// </summary>
internal static class DemoGroupSeeding
{
    public static DbContextOptionsBuilder UseDemoGroupSeeding(
        this DbContextOptionsBuilder options, DemoGroupOptions demoGroup)
    {
        if (!demoGroup.Seed)
        {
            return options;
        }

        // Only the sync one: the migration bundle and `dotnet ef database update` call nothing else. With the
        // seed on, MigrateAsync throws for want of UseAsyncSeeding; the app never migrates itself (ADR-0009).
        return options.UseSeeding((context, _) =>
        {
            var groupId = demoGroup.Id;
            if (context.Set<Group>().Any(g => g.Id == groupId))
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;
            context.AddRange(
                new Group(groupId, "Demo Group"),
                Song.Suggest(groupId, "Wonderwall", "Oasis", "https://www.youtube.com/watch?v=bx1Bh8ZvH84", now.AddDays(-21)),
                Song.Suggest(groupId, "Mr. Brightside", "The Killers", "https://www.youtube.com/watch?v=gGdGFtwCNBE", now.AddDays(-18)),
                Song.Suggest(groupId, "Africa", "Toto", "https://www.youtube.com/watch?v=FTQbiNvZqaY", now.AddDays(-11)));
            context.SaveChanges();
        });
    }
}
