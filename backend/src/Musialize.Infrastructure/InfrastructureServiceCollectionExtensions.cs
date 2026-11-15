using Microsoft.EntityFrameworkCore;
using Musialize.Application.Health;
using Musialize.Application.Persistence;
using Musialize.Infrastructure.Health;
using Musialize.Infrastructure.Persistence;

// In this namespace, as ASP.NET Core's DI docs recommend, so callers need no extra using.
namespace Microsoft.Extensions.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // A named connection string is read from configuration when the first DbContext is created,
        // so startup, and build-time OpenAPI generation, never need a database or connection string.
        services.AddDbContext<MusializeDbContext>(options => options
            .UseNpgsql("Name=ConnectionStrings:Database")
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<MusializeDbContext>());
        services.AddScoped<IDatabaseHealth, DatabaseHealth>();
        return services;
    }
}
