using Microsoft.Extensions.Options;

namespace Musialize.Api.Branding;

/// <summary>The branding the web app reads as JSON, under /api.</summary>
internal static class BrandingApi
{
    /// <summary>Maps the branding API on its group, which Program.cs creates under /api.</summary>
    public static RouteGroupBuilder MapBrandingApi(this RouteGroupBuilder group)
    {
        group.MapGet("", GetBranding)
            .WithName("GetBranding");
        return group;
    }

    private static BrandingResponse GetBranding(IOptions<BrandingOptions> options) =>
        new(options.Value.AppName, options.Value.ShortName);
}

internal sealed record BrandingResponse(string AppName, string ShortName);
