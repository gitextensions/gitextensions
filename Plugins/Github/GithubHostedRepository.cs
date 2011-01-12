using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces.RepositoryHosts;

namespace Github
{
    public class GithubHostedRepository : IHostedRepository
    {
        private GithubPlugin _plugin;

        public GithubHostedRepository(GithubPlugin githubPlugin)
        {
            _plugin = githubPlugin;
        }

        public string Owner { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool IsAFork { get; private set; }
        public bool IsMine
        {
            get
            {
                if (_plugin.Auth == null)
                    throw new InvalidOperationException("Information not set?");
                return Owner == _plugin.Auth.Username;
            }
        }

        public bool IsPrivate { get; private set; }
        public int Forks { get; private set; }

        private string _parent; 
        public string Parent 
        { 
            get
            {
                if (_parent == null && IsAFork)
                {
                    var repoApi = _plugin.GetRepositoryApi();
                    var repo = repoApi.Get(Owner, Name);
                    if (!string.IsNullOrEmpty(repo.Parent))
                        _parent = repo.Parent;
                }

                return _parent;
            }
            private set { _parent = value;  } 
        }

        public string ParentOwner
        {
            get
            {
                if (string.IsNullOrEmpty(Parent))
                    return null;
                string[] s = Parent.Split('/');
                if (s.Length != 2 || s[0].Length == 0 || s[1].Length == 0)
                    return null;
                return s[0];
            }
        }

        public string ParentReadOnlyUrl
        {
            get
            {
                if (string.IsNullOrEmpty(Parent))
                    return null;
                string[] s = Parent.Split('/');
                if (s.Length != 2 || s[0].Length == 0 || s[1].Length == 0)
                    return null;
                return CreateUrl(s[0], s[1], true);
            }
        }

        public string CreateUrl(string owner, string repoName, bool readOnly)
        {
            return CreateUrl(owner, repoName, readOnly, null);
        }

        public string CreateUrl(string owner, string repoName, bool readOnly, string password)
        {
            if (readOnly)
                return string.Format("http://github.com/{0}/{1}.git", owner, repoName);
            else if (_plugin.PreferredAccessMethod == "https")
                return string.Format("https://{0}:{2}@github.com/{0}/{1}.git", owner, repoName, password);
            else
                return string.Format("git@github.com:{0}/{1}.git", owner, repoName);
        }

        public string CloneReadWriteUrl
        {
            get
            {
                return CreateUrl(Owner, Name, false, _plugin.Auth.Password);
            }
        }

        public string CloneReadOnlyUrl
        {
            get
            {
                return CreateUrl(Owner, Name, true);
            }
        }

        public IHostedRepository Fork()
        {
            if (IsMine)
                throw new InvalidOperationException("Can not fork a repo that is already yours");

            var repoApi = _plugin.GetRepositoryApi();
            var tRepo = repoApi.Fork(Owner, Name);
            if (tRepo == null || tRepo.Owner != _plugin.Auth.Username)
                throw new InvalidOperationException("Some part of the fork failed.");

            _plugin.InvalidateCache();

            return Convert(_plugin, tRepo);
        }

        private List<IHostedBranch> _branches;
        public List<IHostedBranch> Branches
        {
            get 
            {
                if (_branches == null)
                {
                    var repoApi = _plugin.GetRepositoryApi();
                    _branches = (from b in repoApi.Branches(Name, Owner) select (IHostedBranch)new GithubHostedBranch(b.Name, b.Sha)).ToList();
                }
                return _branches;
            }
        }

        public List<IPullRequestInformation> GetPullRequests()
        {
            var api = _plugin.GetPullRequestApi();
            var data = api.List(Owner, Name);
            if (data == null)
                throw new InvalidOperationException("Could not fetch data!" + _plugin.GetLoggerData());

            return (from el in data select (IPullRequestInformation)new GithubPullRequestInformation(Owner, Name, el, _plugin)).ToList();
        }

        public int CreatePullRequest(string myBranch, string remoteBranch, string title, string body)
        {
            var api = _plugin.GetPullRequestApi();
            var pr = api.Create(Owner, Name, remoteBranch, _plugin.Auth.Username + ":" + myBranch, title, body);
            if (pr == null || pr.Number == 0)
                throw new InvalidOperationException("CreatePullRequest failed! \r\n" + _plugin.GetLoggerData());
            return pr.Number;
        }

        public string Homepage { get; private set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Owner))
                return Owner + "/" + Name;
            return "GithubHostedRepository[NO DATA]";
        }

        public static GithubHostedRepository Convert(GithubPlugin p, GithubSharp.Core.Models.Repository repo)
        {
            return new GithubHostedRepository(p)
            {
                Owner = repo.Owner,
                Name = repo.Name,
                Description = repo.Description,
                IsAFork = repo.Fork,
                Forks = repo.Forks,
                IsPrivate = repo.Private,
                Homepage = repo.Homepage
            };
        }

        public static GithubHostedRepository Convert(GithubPlugin p, GithubSharp.Core.Models.RepositoryFromSearch repo)
        {
            return new GithubHostedRepository(p)
            {
                Owner = repo.Username,
                Name = repo.Name,
                Description = repo.Description,
                IsAFork = repo.Fork,
                Forks = repo.Forks,
                IsPrivate = false,
                Homepage = string.Format("https://github.com/{0}/{1}", repo.Username, repo.Name)
            };
        }
    }

    internal class GithubHostedBranch : IHostedBranch
    {
        public GithubHostedBranch(string name, string sha)
        {
            Name = name;
            Sha = sha;
        }

        public string Name { get; private set; }
        public string Sha { get; private set; }
    }
}
