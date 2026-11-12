namespace Musialize.Api.Branding;

/// <summary>
/// The Group's name and logo, from the <c>Branding</c> deployment settings (docs/deployment.md#branding),
/// so the pilot's branding never lives in the repository.
/// </summary>
internal sealed class BrandingOptions
{
    public const string Section = "Branding";

    public string AppName { get; set; } = "Musialize";

    public string ShortName { get; set; } = "Musialize";
}
