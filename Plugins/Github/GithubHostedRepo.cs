using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces;

namespace Github
{
    public class GithubHostedRepo : IHostedGitRepo
    {
        private GithubPlugin _githubPlugin;

        public GithubHostedRepo(GithubPlugin githubPlugin)
        {
            _githubPlugin = githubPlugin;
        }

        public string Owner { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool IsAFork { get; private set; }
        public bool IsMine
        {
            get
            {
                if (_githubPlugin.Auth == null)
                    throw new InvalidOperationException("Information not set?");
                return Owner == _githubPlugin.Auth.Username;
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
                    var repoApi = _githubPlugin.GetRepositoryApi();
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
            else if (_githubPlugin.PreferredAccessMethod == "https")
                return string.Format("https://{0}:{2}@github.com/{0}/{1}.git", owner, repoName, password);
            else
                return string.Format("git@github.com:{0}/{1}.git", owner, repoName);
        }

        public string CloneReadWriteUrl
        {
            get
            {
                return CreateUrl(Owner, Name, false, _githubPlugin.Auth.Password);
            }
        }

        public string CloneReadOnlyUrl
        {
            get
            {
                return CreateUrl(Owner, Name, true);
            }
        }

        public IHostedGitRepo Fork()
        {
            if (IsMine)
                throw new InvalidOperationException("Can not fork a repo that is already yours");

            var repoApi = _githubPlugin.GetRepositoryApi();
            var tRepo = repoApi.Fork(Owner, Name);
            if (tRepo == null || tRepo.Owner != _githubPlugin.Auth.Username)
                throw new InvalidOperationException("Some part of the fork failed.");

            _githubPlugin.InvalidateCache();

            return Convert(_githubPlugin, tRepo);
        }

        public string Homepage { get; private set; }

        public static GithubHostedRepo Convert(GithubPlugin p, GithubSharp.Core.Models.Repository repo)
        {
            return new GithubHostedRepo(p)
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

        public static GithubHostedRepo Convert(GithubPlugin p, GithubSharp.Core.Models.RepositoryFromSearch repo)
        {
            return new GithubHostedRepo(p)
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
}
