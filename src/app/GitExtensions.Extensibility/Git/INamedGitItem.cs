namespace GitExtensions.Extensibility.Git;

public interface INamedGitItem : IGitItem
{
    /// <summary>
    ///  The Git full ref name.
    ///  Note that <see cref="IGitRef"/> excludes the ref prefix <c>refs/*/</c>,
    ///  (remote branches keep the name of the remote).
    /// </summary>
    string Name { get; }
}
