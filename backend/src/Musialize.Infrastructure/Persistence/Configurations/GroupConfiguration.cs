using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musialize.Domain.Groups;

namespace Musialize.Infrastructure.Persistence.Configurations;

internal sealed class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> group)
    {
        group.Property(g => g.Id).ValueGeneratedNever();
        group.Property(g => g.Name).HasMaxLength(100);
    }
}
