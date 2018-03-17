using System.Collections.Generic;

namespace GitUIPluginInterfaces.RepositoryHosts
{
    public interface IHostedRepository
    {
        string Owner { get; }
        string Name { get; }
        string Description { get; }
        bool IsAFork { get; }
        bool IsMine { get; }
        bool IsPrivate { get; }
        int Forks { get; }

        string Homepage { get; }

        string ParentReadOnlyUrl { get; }
        string ParentOwner { get; }

        string CloneReadWriteUrl { get; }
        string CloneReadOnlyUrl { get; }

        // Slow op
        List<IHostedBranch> Branches { get; }

        /// <summary>
        /// Forks the repo owned by somebody else to "my" repos.
        /// </summary>
        /// <returns>The new repo, owne by me.</returns>
        IHostedRepository Fork();

        IReadOnlyList<IPullRequestInformation> GetPullRequests();

        /// <returns>Pull request number</returns>
        int CreatePullRequest(string myBranch, string remoteBranch, string title, string body);
    }

    public interface IHostedBranch
    {
        string Name { get; }
        string Sha { get; }
    }
}
