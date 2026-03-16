using System.Collections.Concurrent;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;

namespace GitCommands;

/// <summary>
///  Creates and caches <see cref="GitExecutor"/> instances by repository path.
/// </summary>
internal sealed class GitExecutorProvider : IGitExecutorProvider
{
    private readonly IGitDirectoryResolver _gitDirectoryResolver;
    private readonly ConcurrentDictionary<string, IGitExecutor> _cache = new(StringComparer.Ordinal);

    public GitExecutorProvider(IGitDirectoryResolver gitDirectoryResolver)
    {
        _gitDirectoryResolver = gitDirectoryResolver;
    }

    public IGitExecutor GetExecutor(string repositoryPath)
        => _cache.GetOrAdd(repositoryPath, path => new GitExecutor(_gitDirectoryResolver, path));

    public IGitExecutor GetNewExecutor(string repositoryPath)
    {
        _cache.TryRemove(repositoryPath, out _);
        return _cache.GetOrAdd(repositoryPath, path => new GitExecutor(_gitDirectoryResolver, path));
    }
}
