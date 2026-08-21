namespace GitExtensions.Extensibility.Git;

public interface INamedGitItem : IGitItem
{
    /// <summary>
    ///  The name of the item.
    ///
    ///  For <see cref="IGitRef"/> this is a display name stripping the prefix
    ///  <c>refs/*/</c> from the full unique name
    ///  (<see cref="IGitRef.CompleteName"/> for the full name).
    /// </summary>
    string Name { get; }
}
