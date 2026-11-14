namespace Musialize.Application.Groups;

/// <summary>
/// The one Group the app shows until Sign-in (#21) decides it from the session. Configured under "DemoGroup".
/// </summary>
public sealed class DemoGroupOptions
{
    public const string Section = "DemoGroup";

    public Guid Id { get; set; }
}
