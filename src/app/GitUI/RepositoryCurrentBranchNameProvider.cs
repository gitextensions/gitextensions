using GitCommands;
using GitExtensions.Extensibility.Git;

namespace GitUI;

/// <summary>
///  Provides the name of the currently checked out branch in a repository.
/// </summary>
public interface IRepositoryCurrentBranchNameProvider
{
    /// <summary>
    ///  Gets the name of the currently checked out branch for the specified repository.
    /// </summary>
    /// <param name="repositoryPath">The path to the repository.</param>
    /// <returns>The current branch name.</returns>
    string GetCurrentBranchName(string repositoryPath);
}

internal sealed class RepositoryCurrentBranchNameProvider(Func<string, IGitExecutor> getExecutor) : IRepositoryCurrentBranchNameProvider
{
    private readonly Func<string, IGitExecutor> _getExecutor = getExecutor;

    public string GetCurrentBranchName(string repositoryPath)
    {
        if (!AppSettings.ShowRepoCurrentBranch)
        {
            return string.Empty;
        }

        return _getExecutor(repositoryPath).GetSelectedBranch();
    }
}
