using System.Collections.Concurrent;
using GitCommands.Git;

namespace GitUI;

/// <summary>
///  Wraps <see cref="IRepositoryCurrentBranchNameProvider"/> with a shared in-memory cache so that
///  multiple UI surfaces (menu bar, Dashboard list) avoid redundant git reads for the same path.
/// </summary>
public interface IRepositoryCurrentBranchNameCache : IRepositoryCurrentBranchNameProvider
{
    /// <summary>Returns the cached branch name for <paramref name="repositoryPath"/>, or <see langword="null"/> if not yet cached.</summary>
    public string? GetCachedBranchName(string repositoryPath);

    /// <summary>Writes the resolved branch name into the cache, or removes the entry when the name is blank or detached.</summary>
    public void UpdateCache(string repositoryPath, string branchName);

    /// <summary>Clears all cached branch names, forcing fresh reads on the next access.</summary>
    public void InvalidateAll();
}

public sealed class RepositoryCurrentBranchNameCache(IRepositoryCurrentBranchNameProvider inner) : IRepositoryCurrentBranchNameCache
{
    private readonly ConcurrentDictionary<string, string> _cache = new(StringComparer.InvariantCulture);

    public string? GetCachedBranchName(string repositoryPath)
        => _cache.TryGetValue(repositoryPath, out string? cached) ? cached : null;

    /// <summary>Gets the current branch name, reading from the cache when available.</summary>
    public string GetCurrentBranchName(string repositoryPath)
    {
        if (_cache.TryGetValue(repositoryPath, out string? cached))
        {
            return cached;
        }

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
}
