using System.ComponentModel.DataAnnotations;

namespace Musialize.Application.Validation;

/// <summary>
/// An absolute http or https URL. Unlike <see cref="UrlAttribute"/>, which only checks the prefix and also
/// accepts ftp://, it parses the whole value.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class AbsoluteHttpUrlAttribute() : ValidationAttribute("The {0} field is not an absolute http(s) URL.")
{
    // Null passes, like every other attribute but [Required].
    public override bool IsValid(object? value) =>
        value is null
        || (value is string text
            && Uri.TryCreate(text, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps));
}
