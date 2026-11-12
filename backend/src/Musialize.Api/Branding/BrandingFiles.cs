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
        Icon("icon-192.png", "192x192", "any"),
        Icon("icon-512.png", "512x512", "any"),
        Icon("icon-maskable-512.png", "512x512", "maskable"),
    ];

    /// <summary>Maps the logo, icons and manifest. They stay outside /api, because the browser loads them directly.</summary>
    public static void MapBrandingFiles(this IEndpointRouteBuilder app)
    {
        app.MapGet($"{AssetsRoute}/{{file}}", GetAsset)
            .ExcludeFromDescription();

        // A file for the browser, not part of the API the web app calls.
        app.MapGet("/manifest.webmanifest", GetManifest)
            .ExcludeFromDescription();
    }

    private static Results<PhysicalFileHttpResult, NotFound> GetAsset(string file, IOptions<BrandingOptions> options, HttpContext httpContext)
    {
        if (!BrandingOptions.AssetFiles.TryGetValue(file, out var contentType))
        {
            return TypedResults.NotFound();
        }

        // Revalidated on every load (cheap, with Last-Modified), so a new logo shows up without a cache to wait out.
        httpContext.Response.Headers.CacheControl = "no-cache";
        var path = Path.Combine(options.Value.ResolvedAssetsPath, file);
        return TypedResults.PhysicalFile(path, contentType, lastModified: File.GetLastWriteTimeUtc(path));
    }

    // What "Add to home screen" installs: the name and icon under the app, and no browser bar.
    private static JsonHttpResult<WebManifest> GetManifest(IOptions<BrandingOptions> options) =>
        TypedResults.Json(
            new WebManifest(options.Value.AppName, options.Value.ShortName, options.Value.ThemeColor, BackgroundColor, "standalone", "/", Icons),
            ManifestJson,
            "application/manifest+json");

    // Looks the file up among the served ones, so the manifest never points at a file that isn't served.
    private static ManifestIcon Icon(string file, string sizes, string purpose) =>
        new($"{AssetsRoute}/{file}", sizes, BrandingOptions.AssetFiles[file], purpose);
}

internal sealed record WebManifest(string Name, string ShortName, string ThemeColor, string BackgroundColor, string Display, string StartUrl, ManifestIcon[] Icons);

internal sealed record ManifestIcon(string Src, string Sizes, string Type, string Purpose);
