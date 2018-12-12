using System.Collections.Generic;
using JetBrains.Annotations;

namespace GitUIPluginInterfaces.RepositoryHosts
{
    public interface IHostedRepository
    {
        [CanBeNull]
        string Owner { get; }
        string Name { get; }
        string Description { get; }
        bool IsAFork { get; }
        bool IsMine { get; }
        bool IsPrivate { get; }
        int Forks { get; }

        string Homepage { get; }

        [CanBeNull]
        string ParentReadOnlyUrl { get; }
        [CanBeNull]
        string ParentOwner { get; }

        string CloneReadWriteUrl { get; }
        string CloneReadOnlyUrl { get; }

        /// <summary>
        /// Requests details of branches from the GitHub remote repository.
        /// </summary>
        /// <returns>A list of branches, ordered by name.</returns>
        IReadOnlyList<IHostedBranch> GetBranches();

        /// <summary>
        /// Forks the repo owned by somebody else to "my" repos.
        /// </summary>
        /// <returns>The new repo, owned by me.</returns>
        IHostedRepository Fork();

        IReadOnlyList<IPullRequestInformation> GetPullRequests();

        /// <returns>Pull request number</returns>
        int CreatePullRequest(string myBranch, string remoteBranch, string title, string body);

        string CloneProtocol { get; set; }

        string[] SupportedCloneProtocols { get; set; }
    }

    public interface IHostedBranch
    {
        string Name { get; }
        ObjectId Sha { get; }
    }
}
