namespace GitExtensions.Extensibility.Git;

public interface INamedGitItem : IGitItem
{
    /// <summary>
    ///  The name of the item.
    ///  This is generally not a unique name in global Git namespace.
    ///  For instance, the derived interface <c>IGitRef</c> removes
    ///  the initial <c>refs/*/</c> from the names.
    /// </summary>
    string Name { get; }
}
