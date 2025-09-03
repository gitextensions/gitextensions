using GitCommands;
using GitCommands.Git;

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

internal sealed class RepositoryCurrentBranchNameProvider : IRepositoryCurrentBranchNameProvider
{
    public string GetCurrentBranchName(string repositoryPath)
    {
        if (!AppSettings.ShowRepoCurrentBranch)
        {
            return string.Empty;
        }

        string branchName = GitModule.GetSelectedBranchFast(repositoryPath);
        if (string.IsNullOrWhiteSpace(branchName) || branchName == DetachedHeadParser.DetachedBranch)
        {
            branchName = $"({TranslatedStrings.NoBranch})";
        }

        return branchName;
    }
}
