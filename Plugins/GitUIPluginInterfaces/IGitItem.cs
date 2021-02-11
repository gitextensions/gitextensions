#nullable enable

namespace GitUIPluginInterfaces
{
    public interface IGitItem
    {
        ObjectId? ObjectId { get; }

        string? Guid { get; }
    }

    public interface INamedGitItem : IGitItem
    {
        string Name { get; }
    }
}
