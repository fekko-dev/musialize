namespace Musialize.Domain.Groups;

/// <summary>A collective of musicians who pick Songs together. Every Song belongs to one Group.</summary>
public sealed class Group(Guid id, string name)
{
    public Guid Id { get; private set; } = id;

    public string Name { get; private set; } = name;
}
