namespace GitExtensions.Extensibility.Git;

/// <summary>
/// Provides a minimal git execution context for lightweight repository operations such as reading the current branch.
/// </summary>
public interface IGitExecutor
{
    /// <summary>
    /// Gets the directory which contains the git repository.
    /// </summary>
    string WorkingDir { get; }

    /// <summary>
    /// Gets the default Git executable associated with this module.
    /// This executable can be non-native (i.e. WSL).
    /// </summary>
    IExecutable GitExecutable { get; }

    /// <summary>
    /// Gets the access to the current git executable associated with this module.
    /// This command runner can be non-native (i.e. WSL).
    /// </summary>
    IGitCommandRunner GitCommandRunner { get; }

    /// <summary>
    /// Gets the name of the currently checked out branch.
    /// </summary>
    /// <param name="emptyIfDetached">Defines the value returned if HEAD is detached. <see langword="true"/> to return <see cref="string.Empty"/>; <see langword="false"/> to return "(no branch)".</param>
    /// <returns>
    /// The name of the branch (for example: "main"); the value requested by <paramref name="emptyIfDetached"/>, if HEAD is detached.
    /// </returns>
    string GetSelectedBranch(bool emptyIfDetached = false);
}
