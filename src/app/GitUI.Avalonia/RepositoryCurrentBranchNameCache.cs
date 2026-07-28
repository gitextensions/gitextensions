using System.Collections.Concurrent;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;

namespace GitUI;

public interface IRepositoryCurrentBranchNameCache : IRepositoryCurrentBranchNameProvider
{
    string? GetCachedBranchName(string repositoryPath);
    string GetUpdatedBranchName(string repositoryPath);
    void UpdateCache(string repositoryPath, string branchName);
    void InvalidateAll();
    bool IsEmpty { get; }
}

internal sealed class RepositoryCurrentBranchNameCache(IRepositoryCurrentBranchNameProvider inner)
    : IRepositoryCurrentBranchNameCache
{
    private readonly ConcurrentDictionary<string, string> _cache = new(GetPathComparer());

    public bool IsEmpty => _cache.IsEmpty;

    public string? GetCachedBranchName(string repositoryPath)
        => _cache.TryGetValue(repositoryPath, out string? branchName) ? branchName : null;

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

    private static StringComparer GetPathComparer()
        => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
}
