namespace GitExtensions.Extensibility.Git;

/// <summary>
/// A lazy provider for GitRefs() that can be shared for instance when a repository is changed.
/// </summary>
public interface IFilteredGitRefsProvider
{
    /// <summary>
    /// Returns the IGitRefs matching the filter.
    /// </summary>
    /// <param name="filter">The filter</param>
    /// <returns>The filtered GitRefs</returns>
    IReadOnlyList<IGitRef> GetRefs(RefsFilter filter);
}
