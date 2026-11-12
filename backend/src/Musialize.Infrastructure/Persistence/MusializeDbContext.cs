using Microsoft.EntityFrameworkCore;
using Musialize.Application.Persistence;
using Musialize.Domain.Groups;
using Musialize.Domain.Songs;

namespace Musialize.Infrastructure.Persistence;

public sealed class MusializeDbContext(DbContextOptions<MusializeDbContext> options) : DbContext(options), IAppDbContext
{
    /// <summary>Compares text ignoring case, so "wonderwall" equals "Wonderwall" in queries and unique indexes.</summary>
    internal const string CaseInsensitive = "case_insensitive";

    public DbSet<Group> Groups => Set<Group>();

    public DbSet<Song> Songs => Set<Song>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ICU, level 2: accents still count, case doesn't. Nondeterministic, so = compares that way too.
        modelBuilder.HasCollation(CaseInsensitive, locale: "und-u-ks-level2", provider: "icu", deterministic: false);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MusializeDbContext).Assembly);
    }
}
