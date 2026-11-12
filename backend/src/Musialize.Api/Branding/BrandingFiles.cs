using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace Musialize.Api.Branding;

/// <summary>The logo, icons and manifest, which the browser and a phone's home screen load directly.</summary>
internal static class BrandingFiles
{
    /// <summary>Where the logo and icons are served, under one prefix so they never clash with the SPA's files.</summary>
    public const string AssetsRoute = "/branding";

    // Android's launch screen while the app starts, the same white as the page.
    private const string BackgroundColor = "#ffffff";

    // The Web App Manifest spec names its members in snake_case.
    private static readonly JsonSerializerOptions ManifestJson = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    private static readonly ManifestIcon[] Icons =
    [
        new($"{AssetsRoute}/icon-192.png", "192x192", "image/png", "any"),
        new($"{AssetsRoute}/icon-512.png", "512x512", "image/png", "any"),
        new($"{AssetsRoute}/icon-maskable-512.png", "512x512", "image/png", "maskable"),
    ];

    /// <summary>Maps the logo, icons and manifest. They stay outside /api, because the browser loads them directly.</summary>
    public static void MapBrandingFiles(this IEndpointRouteBuilder app)
    {
        // A file for the browser, not part of the API the web app calls.
        app.MapGet("/manifest.webmanifest", GetManifest)
            .ExcludeFromDescription();
    }

    // What "Add to home screen" installs: the name and icon under the app, and no browser bar.
    private static JsonHttpResult<WebManifest> GetManifest(IOptions<BrandingOptions> options) =>
        TypedResults.Json(
            new WebManifest(options.Value.AppName, options.Value.ShortName, options.Value.ThemeColor, BackgroundColor, "standalone", "/", Icons),
            ManifestJson,
            "application/manifest+json");
}

internal sealed record WebManifest(string Name, string ShortName, string ThemeColor, string BackgroundColor, string Display, string StartUrl, ManifestIcon[] Icons);

internal sealed record ManifestIcon(string Src, string Sizes, string Type, string Purpose);
