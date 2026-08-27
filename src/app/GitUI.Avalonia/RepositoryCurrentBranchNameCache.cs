using System.Collections.Concurrent;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;

namespace GitUI;

public interface IRepositoryCurrentBranchNameCache : IRepositoryCurrentBranchNameProvider
{
    /// <summary>
        ///  Returns the cached branch name for <paramref name="repositoryPath"/>, or <see langword="null"/> if not yet cached.
        /// </summary>
    string? GetCachedBranchName(string repositoryPath);

    /// <summary>
        ///  Resolves the branch name and forwards it to <see cref="UpdateCache"/>.
        /// </summary>
        /// <returns>The branch name for <paramref name="repositoryPath"/>.</returns>
    string GetUpdatedBranchName(string repositoryPath);

    /// <summary>
        ///  Writes the resolved branch name into the cache, or removes the entry when the name is blank or error occurred.
        /// </summary>
    void UpdateCache(string repositoryPath, string branchName);

    /// <summary>
        ///  Clears all cached branch names, forcing fresh reads on the next access.
        /// </summary>
    void InvalidateAll();

    /// <summary>
        ///  Returns <see langword="true"/> when no branch names have been cached yet.
        /// </summary>
    bool IsEmpty { get; }
}

internal sealed class RepositoryCurrentBranchNameCache(IRepositoryCurrentBranchNameProvider inner)
    : IRepositoryCurrentBranchNameCache
{
    private readonly ConcurrentDictionary<string, string> _cache = new(GetPathComparer());

    public string? GetCachedBranchName(string repositoryPath)
        => _cache.TryGetValue(repositoryPath, out string? branchName) ? branchName : null;

    /// <summary>
        ///  Gets the current branch name, reading from the cache when available.
        /// </summary>
    public string GetCurrentBranchName(string repositoryPath)
        => GetCachedBranchName(repositoryPath) ?? GetUpdatedBranchName(repositoryPath);

    public string GetUpdatedBranchName(string repositoryPath)
    {
        string branchName = inner.GetCurrentBranchName(repositoryPath);
        UpdateCache(repositoryPath, branchName);
        return branchName;
    }

    public void UpdateCache(string repositoryPath, string branchName)
    {
        if (string.IsNullOrWhiteSpace(branchName) || branchName == DetachedHeadParser.UnknownBranchName)
        {
            _cache.TryRemove(repositoryPath, out _);
        }
        else
        {
            _cache[repositoryPath] = branchName;
        }
    }

    public void InvalidateAll() => _cache.Clear();

    public bool IsEmpty => _cache.IsEmpty;

    private static StringComparer GetPathComparer()
        => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
}
