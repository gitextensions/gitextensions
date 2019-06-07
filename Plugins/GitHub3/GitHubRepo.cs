using System;
using System.Collections.Generic;
using System.Linq;
using Git.hub;
using GitUIPluginInterfaces.RepositoryHosts;

namespace GitHub3
{
    public class GitHubRepo : IHostedRepository
    {
        private Repository _repo;

        public GitHubRepo(Repository repo)
        {
            _repo = repo;
        }

        public string Owner => _repo.Owner?.Login;
        public string Name => _repo.Name;
        public string Description => _repo.Description;
        public bool IsAFork => _repo.Fork;
        public bool IsMine => Owner == GitHubLoginInfo.Username;
        public bool IsPrivate => _repo.Private;
        public int Forks => _repo.Forks;
        public string Homepage => _repo.Homepage;

        public string ParentReadOnlyUrl
        {
            get
            {
                if (!_repo.Fork)
                {
                    return null;
                }

                if (!_repo.Detailed)
                {
                    if (_repo.Organization != null)
                    {
                        return null;
                    }

                    _repo = GitHub3Plugin.GitHub.getRepository(Owner, Name);
                }

                return CloneProtocol == GitProtocol.Ssh ? _repo.Parent?.GitUrl : _repo.Parent?.CloneUrl;
            }
        }

        public string ParentOwner
        {
            get
            {
                if (!_repo.Fork)
                {
                    return null;
                }

                if (!_repo.Detailed)
                {
                    if (_repo.Organization != null)
                    {
                        return null;
                    }

                    _repo = GitHub3Plugin.GitHub.getRepository(Owner, Name);
                }

                return _repo.Parent?.Owner.Login;
            }
        }

        public string CloneReadWriteUrl => CloneProtocol == GitProtocol.Ssh ? _repo.SshUrl : _repo.CloneUrl;

        public string CloneReadOnlyUrl => CloneProtocol == GitProtocol.Ssh ? _repo.GitUrl : _repo.CloneUrl;

        public IReadOnlyList<IHostedBranch> GetBranches()
        {
            return _repo.GetBranches()
                .Select(branch => new GitHubBranch(branch))
                .OrderBy(branch => branch.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public string GetDefaultBranch()
        {
            return _repo.GetDefaultBranch();
        }

        public IHostedRepository Fork()
        {
            return new GitHubRepo(_repo.CreateFork());
        }

        public IReadOnlyList<IPullRequestInformation> GetPullRequests()
        {
            var pullRequests = _repo?.GetPullRequests();

            if (pullRequests != null)
            {
                return pullRequests
                    .Select(pr => new GitHubPullRequest(pr))
                    .ToList();
            }

            return Array.Empty<IPullRequestInformation>();
        }

        public int CreatePullRequest(string myBranch, string remoteBranch, string title, string body)
        {
            var pullRequest = _repo.CreatePullRequest(GitHubLoginInfo.Username + ":" + myBranch, remoteBranch, title, body);

            if (pullRequest == null || pullRequest.Number == 0)
            {
                throw new Exception("Failed to create pull request.");
            }

            return pullRequest.Number;
        }

        public GitProtocol CloneProtocol { get; set; } = GitProtocol.Ssh;

        public IReadOnlyList<GitProtocol> SupportedCloneProtocols { get; set; } = new[] { GitProtocol.Ssh, GitProtocol.Https };

        public override string ToString() => $"{Owner}/{Name}";
    }
}
