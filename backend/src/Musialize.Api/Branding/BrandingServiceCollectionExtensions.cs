using Microsoft.Extensions.Options;
using Musialize.Api.Branding;

// In this namespace, as ASP.NET Core's DI docs recommend, so callers need no extra using.
namespace Microsoft.Extensions.DependencyInjection;

internal static class BrandingServiceCollectionExtensions
{
    /// <summary>Binds the <c>Branding</c> settings, and stops the app at startup when one is invalid or its folder misses a file.</summary>
    public static IServiceCollection AddBranding(this IServiceCollection services)
    {
        services.AddOptions<BrandingOptions>()
            .BindConfiguration(BrandingOptions.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<BrandingOptions>, BrandingOptionsValidation>();
        return services;
    }
}
