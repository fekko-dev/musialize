using Microsoft.EntityFrameworkCore;
using Musialize.Domain.Songs;

namespace Musialize.Application.Persistence;

/// <summary>
/// How handlers reach PostgreSQL: one <see cref="DbSet{TEntity}"/> per aggregate root (ADR-0010).
/// <c>MusializeDbContext</c> in Infrastructure implements it.
/// </summary>
public interface IAppDbContext
{
    DbSet<Song> Songs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
