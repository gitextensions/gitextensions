using System.Collections.Concurrent;
using GitExtensions.Extensibility.Git;

namespace GitCommands;

/// <summary>
/// Creates and caches <see cref="GitExecutor"/> instances by repository path.
/// </summary>
public sealed class GitExecutorProvider : IGitExecutorProvider
{
    private readonly ConcurrentDictionary<string, IGitExecutor> _cache = new(StringComparer.OrdinalIgnoreCase);

    public IGitExecutor GetExecutor(string repositoryPath)
        => _cache.GetOrAdd(repositoryPath, static path => new GitExecutor(path));
}
