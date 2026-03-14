namespace GitExtensions.Extensibility.Git;

/// <summary>
/// Provides cached <see cref="IGitExecutor"/> instances for repository paths.
/// </summary>
public interface IGitExecutorProvider
{
    /// <summary>
    /// Gets (or creates) a cached <see cref="IGitExecutor"/> for the specified repository path.
    /// </summary>
    /// <param name="repositoryPath">The path to the repository.</param>
    IGitExecutor GetExecutor(string repositoryPath);

    /// <summary>
    /// Gets a new <see cref="IGitExecutor"/> for the specified repository path, replacing any cached instance.
    /// </summary>
    /// <param name="repositoryPath">The path to the repository.</param>
    IGitExecutor GetNewExecutor(string repositoryPath);
}
