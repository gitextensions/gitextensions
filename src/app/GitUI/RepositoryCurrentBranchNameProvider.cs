using GitCommands;
using GitCommands.Git;
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

internal sealed class RepositoryCurrentBranchNameProvider : IRepositoryCurrentBranchNameProvider
{
    private readonly Func<string, IGitModule> _getModule;

    public RepositoryCurrentBranchNameProvider(Func<string, IGitModule> getModule)
    {
        _getModule = getModule;
    }

    public string GetCurrentBranchName(string repositoryPath)
    {
        if (!AppSettings.ShowRepoCurrentBranch)
        {
            return string.Empty;
        }

        string branchName = GetModule(repositoryPath).GetSelectedBranch();
        if (string.IsNullOrWhiteSpace(branchName) || branchName == DetachedHeadParser.DetachedBranch)
        {
            branchName = $"({TranslatedStrings.NoBranch})";
        }

        return branchName;
    }

    private IGitModule GetModule(string path)
    {
        IGitModule module = _getModule(path);
        if (module is null)
        {
            throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
        }

        return module;
    }
}
