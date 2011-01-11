using System;
using System.Collections.Generic;
using System.Text;

namespace GitUIPluginInterfaces
{
    public interface IGitHostingPlugin : IGitPlugin
    {
        IList<IHostedGitRepo> SearchForRepo(string search);
        IList<IHostedGitRepo> GetReposOfUser(string user);

        IList<IHostedGitRepo> GetMyRepos();

        bool ConfigurationOk { get; }

        bool CurrentWorkingDirRepoIsRelevantToMe { get; }
        List<IHostedRemote> GetPullRequestTargetsForCurrentWorkingDirRepo();
    }

    public interface IHostedGitRepo
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

        /// <summary>
        /// Forks the repo owned by somebody else to "my" repos.
        /// </summary>
        /// <returns>The new repo, owne by me.</returns>
        IHostedGitRepo Fork();
    }
}
