using GitCommands;
using GitCommands.Git;

namespace GitUI
{
    public interface IRepositoryCurrentBranchNameProvider
    {
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
                branchName = $"({Strings.NoBranch})";
            }

            return branchName;
        }
    }
}
