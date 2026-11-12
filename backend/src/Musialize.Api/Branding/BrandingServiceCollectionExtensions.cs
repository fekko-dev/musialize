namespace Musialize.Api.Branding;

internal static class BrandingServiceCollectionExtensions
{
    /// <summary>Binds the <c>Branding</c> settings.</summary>
    public static IServiceCollection AddBranding(this IServiceCollection services)
    {
        services.AddOptions<BrandingOptions>().BindConfiguration(BrandingOptions.Section);
        return services;
    }
}
