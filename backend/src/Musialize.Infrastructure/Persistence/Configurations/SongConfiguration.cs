using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musialize.Domain.Groups;
using Musialize.Domain.Songs;

namespace Musialize.Infrastructure.Persistence.Configurations;

internal sealed class SongConfiguration : IEntityTypeConfiguration<Song>
{
    public void Configure(EntityTypeBuilder<Song> song)
    {
        song.Property(s => s.Id).ValueGeneratedNever();
        song.Property(s => s.Title).HasMaxLength(200).UseCollation(MusializeDbContext.CaseInsensitive);
        song.Property(s => s.Artist).HasMaxLength(200).UseCollation(MusializeDbContext.CaseInsensitive);
        song.Property(s => s.Link).HasMaxLength(2048);

        song.HasOne<Group>().WithMany().HasForeignKey(s => s.GroupId);

        // The duplicate check (#45): one Song per title and artist in a Group, ignoring case through the collation.
        song.HasIndex(s => new { s.GroupId, s.Title, s.Artist }).IsUnique();
        // The Overview lists Suggestions oldest first.
        song.HasIndex(s => new { s.GroupId, s.SuggestedAt });
    }
}
