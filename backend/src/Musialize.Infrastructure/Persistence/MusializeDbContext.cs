using Microsoft.EntityFrameworkCore;

namespace Musialize.Infrastructure.Persistence;

public sealed class MusializeDbContext(DbContextOptions<MusializeDbContext> options) : DbContext(options);
