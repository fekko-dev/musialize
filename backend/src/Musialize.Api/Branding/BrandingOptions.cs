namespace Musialize.Api.Branding;

/// <summary>
/// The Group's name and logo, from the <c>Branding</c> deployment settings (docs/deployment.md#branding),
/// so the pilot's branding never lives in the repository.
/// </summary>
internal sealed class BrandingOptions
{
    public const string Section = "Branding";

    public string AppName { get; set; } = "Musialize";

    /// <summary>The name under the icon on a home screen, where a long name gets cut off.</summary>
    public string ShortName { get; set; } = "Musialize";

    /// <summary>The installed app's status and title bar on Android. Used in the manifest only.</summary>
    public string ThemeColor { get; set; } = "#ffffff";
}
