using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;

namespace GitUI;

public interface IRepositoryCurrentBranchNameProvider
{
    /// <summary>
        ///  Gets the name of the currently checked out branch for the specified repository.
        /// </summary>
        /// <param name="repositoryPath">The path to the repository.</param>
        /// <returns>The current branch name.</returns>
    string GetCurrentBranchName(string repositoryPath);
}

internal sealed class RepositoryCurrentBranchNameProvider(IGitExecutorProvider executorProvider)
    : IRepositoryCurrentBranchNameProvider
{
    public string GetCurrentBranchName(string repositoryPath)
        => AppSettings.ShowRepoCurrentBranch
            ? Commands.GetSelectedBranch(executorProvider.GetExecutor(repositoryPath))
            : string.Empty;
}
