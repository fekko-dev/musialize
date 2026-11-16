using Musialize.Application.Songs;

// In this namespace, as ASP.NET Core's DI docs recommend, so callers need no extra using.
namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // The validation source generator only covers the assembly that calls AddValidation,
        // so Application calls it for its own request types, which are marked [ValidatableType].
        services.AddValidation();

        services.AddScoped<GetSuggestionsHandler>();
        return services;
    }
}
