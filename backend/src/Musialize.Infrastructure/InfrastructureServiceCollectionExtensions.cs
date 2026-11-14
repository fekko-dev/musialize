using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Musialize.Application.Groups;
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
        services.AddDbContext<MusializeDbContext>((provider, options) => options
            .UseNpgsql("Name=ConnectionStrings:Database")
            .UseSnakeCaseNamingConvention()
            .UseDemoGroupSeeding(provider.GetRequiredService<IOptions<DemoGroupOptions>>().Value));
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<MusializeDbContext>());
        services.AddScoped<IDatabaseHealth, DatabaseHealth>();
        return services;
    }
}
