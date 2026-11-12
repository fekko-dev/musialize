using Microsoft.Extensions.Options;

namespace Musialize.Api.Branding;

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
