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

        public string Owner { get { return repo.Owner != null ? repo.Owner.Login : null; } }
        public string Name { get { return repo.Name; } }
        public string Description { get { return repo.Description; } }
        public bool IsAFork { get { return repo.Fork; } }
        public bool IsMine { get { return Owner == GithubLoginInfo.username; } }
        public bool IsPrivate { get { return repo.Private; } }
        public int Forks { get { return repo.Forks; } }
        public string Homepage { get { return repo.Homepage; } }
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

                    repo = Github3.github.getRepository(Owner, Name);
                }
                return repo.Parent == null ? null : repo.Parent.GitUrl;
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

                    repo = Github3.github.getRepository(Owner, Name);
                }

                return repo.Parent == null ? null : repo.Parent.Owner.Login;
            }
        }
        public string CloneReadWriteUrl { get { return repo.SshUrl; } }
        public string CloneReadOnlyUrl { get { return repo.GitUrl; } }

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
            if (repo == null)
                return new List<IPullRequestInformation>();
            return repo.GetPullRequests().Select(pullrequest => (IPullRequestInformation)new GithubPullRequest(pullrequest)).ToList();
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
