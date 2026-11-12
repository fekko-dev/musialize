using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Musialize.Api.Branding;

/// <summary>
/// The Group's name and logo, from the <c>Branding</c> deployment settings (docs/deployment.md#branding),
/// so the pilot's branding never lives in the repository.
/// </summary>
internal sealed class BrandingOptions
{
    public const string Section = "Branding";

    /// <summary>The files a branding folder holds, by name, with the content type each is served as.</summary>
    public static readonly IReadOnlyDictionary<string, string> AssetFiles = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["logo.svg"] = "image/svg+xml",
        ["favicon.ico"] = "image/x-icon",
        ["icon-192.png"] = "image/png",
        ["icon-512.png"] = "image/png",
        ["icon-maskable-512.png"] = "image/png",
        ["apple-touch-icon.png"] = "image/png",
    };

    // Musialize's own logo and icons, copied next to the app by the build.
    private static readonly string DefaultAssetsPath = Path.Combine(AppContext.BaseDirectory, "Branding", "Defaults");

    /// <summary>The name in the header, the browser tab and the installed app.</summary>
    [Required]
    public string AppName { get; set; } = "Musialize";

    /// <summary>The name under the icon on a home screen, where a long name gets cut off.</summary>
    [Required]
    public string ShortName { get; set; } = "Musialize";

    /// <summary>The installed app's status and title bar on Android. Used in the manifest only.</summary>
    [RegularExpression("^#[0-9a-fA-F]{6}$", ErrorMessage = "Use a colour like #ffffff.")]
    public string ThemeColor { get; set; } = "#ffffff";

    /// <summary>The folder with the <see cref="AssetFiles"/>, as set. Read <see cref="ResolvedAssetsPath"/> instead.</summary>
    public string? AssetsPath { get; set; }

    /// <summary>The folder the <see cref="AssetFiles"/> are served from: <see cref="AssetsPath"/>, or Musialize's own when it is unset.</summary>
    public string ResolvedAssetsPath => string.IsNullOrEmpty(AssetsPath) ? DefaultAssetsPath : AssetsPath;
}

/// <summary>Stops the app at startup when the branding folder misses a file, so a broken mount is never deployed.</summary>
internal sealed class BrandingOptionsValidation : IValidateOptions<BrandingOptions>
{
    public ValidateOptionsResult Validate(string? name, BrandingOptions options)
    {
        var missing = BrandingOptions.AssetFiles.Keys
            .Where(file => !File.Exists(Path.Combine(options.ResolvedAssetsPath, file)))
            .ToList();

        return missing.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail($"The branding folder {options.ResolvedAssetsPath} misses {string.Join(", ", missing)}.");
    }
}
