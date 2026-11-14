using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Musialize.Application.Health;
using Musialize.Application.Persistence;
using Musialize.Infrastructure.Health;
using Musialize.Infrastructure.Persistence;

namespace Musialize.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, bool seedDemoGroup)
    {
        // A named connection string is read from configuration when the first DbContext is created,
        // so startup, and build-time OpenAPI generation, never need a database or connection string.
        services.AddDbContext<MusializeDbContext>(options => options
            .UseNpgsql("Name=ConnectionStrings:Database")
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<MusializeDbContext>());
        services.AddScoped<IDatabaseHealth, DatabaseHealth>();

        if (seedDemoGroup)
        {
            services.AddHostedService<DemoGroupSeeder>();
        }

        return services;
    }
}
