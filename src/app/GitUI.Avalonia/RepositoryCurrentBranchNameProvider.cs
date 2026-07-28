using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;

namespace GitUI;

public interface IRepositoryCurrentBranchNameProvider
{
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
