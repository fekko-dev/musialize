using Microsoft.Extensions.DependencyInjection;

namespace Musialize.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // The validation source generator only covers the assembly that calls AddValidation,
        // so Application calls it for its own request types, which are marked [ValidatableType].
        services.AddValidation();
        return services;
    }
}
