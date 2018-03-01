using System;
using System.Collections.Generic;
using System.Linq;
using Git.hub;
using GitUIPluginInterfaces.RepositoryHosts;

namespace Github3
{
    public class GithubRepo : IHostedRepository
    {
        private Repository repo;

        public GithubRepo(Repository repo)
        {
            this.repo = repo;
        }

        public string Owner => repo.Owner?.Login;
        public string Name => repo.Name;
        public string Description => repo.Description;
        public bool IsAFork => repo.Fork;
        public bool IsMine => Owner == GithubLoginInfo.username;
        public bool IsPrivate => repo.Private;
        public int Forks => repo.Forks;
        public string Homepage => repo.Homepage;

        public string ParentReadOnlyUrl
        {
            get
            {
                if (!repo.Fork)
                    return null;

                if (!repo.Detailed)
                {
                    if (repo.Organization != null)
                        return null;

                    repo = Github3Plugin.github.getRepository(Owner, Name);
                }
                return repo.Parent?.GitUrl;
            }
        }

        public string ParentOwner
        {
            get
            {
                if (!repo.Fork)
                    return null;

                if (!repo.Detailed)
                {
                    if (repo.Organization != null)
                        return null;

                    repo = Github3Plugin.github.getRepository(Owner, Name);
                }

                return repo.Parent?.Owner.Login;
            }
        }

        public string CloneReadWriteUrl => repo.SshUrl;
        public string CloneReadOnlyUrl => repo.GitUrl;

        public List<IHostedBranch> Branches
        {
            get { return repo.GetBranches().Select(branch => (IHostedBranch)new GithubBranch(branch)).ToList(); }
        }

        public IHostedRepository Fork()
        {
            return new GithubRepo(repo.CreateFork());
        }

        public List<IPullRequestInformation> GetPullRequests()
        {
            var pullRequests = repo?.GetPullRequests();
            if (pullRequests != null)
            {
                return pullRequests
                    .Select(pr => (IPullRequestInformation)new GithubPullRequest(pr))
                    .ToList();
            }

            return new List<IPullRequestInformation>();
        }

        public int CreatePullRequest(string myBranch, string remoteBranch, string title, string body)
        {
            var pullrequest = repo.CreatePullRequest(GithubLoginInfo.username + ":" + myBranch, remoteBranch, title, body);
            if (pullrequest == null || pullrequest.Number == 0)
                throw new Exception("Failed to create pull request.");

            return pullrequest.Number;
        }

        public override string ToString()
        {
            return Owner + "/" + Name;
        }
    }
}
