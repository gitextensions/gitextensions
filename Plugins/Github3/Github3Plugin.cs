using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Git.hub;
using GitCommands.Config;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;
using ResourceManager;

namespace Github3
{
    internal static class GithubAPIInfo
    {
        internal static string client_id = "ebc0e8947c206610d737";
        internal static string client_secret = "c993907df3f45145bf638842692b69c56d1ace4d";
    }

    internal static class GithubLoginInfo
    {
        private static string _username;
        public static string Username
        {
            get
            {
                if (_username == "")
                {
                    return null;
                }

                if (_username != null)
                {
                    return _username;
                }

                try
                {
                    var user = Github3Plugin.github.getCurrentUser();
                    if (user != null)
                    {
                        _username = user.Login;
                        ////MessageBox.Show("Github username: " + _username);
                        return _username;
                    }
                    else
                    {
                        _username = "";
                    }

                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public static string OAuthToken
        {
            get => Github3Plugin.instance.OAuthToken.ValueOrDefault(Github3Plugin.instance.Settings);
            set
            {
                _username = null;
                Github3Plugin.instance.OAuthToken[Github3Plugin.instance.Settings] = value;
                Github3Plugin.github.setOAuth2Token(value);
            }
        }
    }

    [Export(typeof(IGitPlugin))]
    public class Github3Plugin : GitPluginBase, IRepositoryHostPlugin
    {
        public readonly StringSetting OAuthToken = new StringSetting("OAuth Token", "");

        internal static Github3Plugin instance;
        internal static Client github;

        public Github3Plugin()
        {
            SetNameAndDescription("Github");
            Translate();

            if (instance == null)
            {
                instance = this;
            }

            github = new Client();
        }

        public override IEnumerable<ISetting> GetSettings()
        {
            yield return OAuthToken;
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            if (!string.IsNullOrEmpty(GithubLoginInfo.OAuthToken))
            {
                github.setOAuth2Token(GithubLoginInfo.OAuthToken);
            }
        }

        public override bool Execute(GitUIEventArgs args)
        {
            if (string.IsNullOrEmpty(GithubLoginInfo.OAuthToken))
            {
                using (var frm = new OAuth())
                {
                    frm.ShowDialog(args.OwnerForm);
                }
            }
            else
            {
                MessageBox.Show(args.OwnerForm, "You already have an OAuth token. To get a new one, delete your old one in Plugins > Settings first.");
            }

            return false;
        }

        // --

        public IReadOnlyList<IHostedRepository> SearchForRepository(string search)
        {
            return github.searchRepositories(search).Select(repo => (IHostedRepository)new GithubRepo(repo)).ToList();
        }

        public IReadOnlyList<IHostedRepository> GetRepositoriesOfUser(string user)
        {
            return github.getRepositories(user).Select(repo => (IHostedRepository)new GithubRepo(repo)).ToList();
        }

        public IHostedRepository GetRepository(string user, string repositoryName)
        {
            return new GithubRepo(github.getRepository(user, repositoryName));
        }

        public IReadOnlyList<IHostedRepository> GetMyRepos()
        {
            return github.getRepositories().Select(repo => (IHostedRepository)new GithubRepo(repo)).ToList();
        }

        public bool ConfigurationOk => true;

        public bool GitModuleIsRelevantToMe(IGitModule module)
        {
            return GetHostedRemotesForModule(module).Count > 0;
        }

        /// <summary>
        /// Returns all relevant github-remotes for the current working directory
        /// </summary>
        public IReadOnlyList<IHostedRemote> GetHostedRemotesForModule(IGitModule module)
        {
            var repoInfos = new List<IHostedRemote>();

            string[] remotes = module.GetRemotes(false);
            foreach (string remote in remotes)
            {
                var url = module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote));
                if (string.IsNullOrEmpty(url))
                {
                    continue;
                }

                var m = Regex.Match(url, @"git(?:@|://)github.com[:/]([^/]+)/([\w_\.\-]+)\.git");
                if (!m.Success)
                {
                    m = Regex.Match(url, @"https?://(?:[^@:]+)?(?::[^/@:]+)?@?github.com/([^/]+)/([\w_\.\-]+)(?:.git)?");
                }

                if (m.Success)
                {
                    var hostedRemote = new GithubHostedRemote(remote, m.Groups[1].Value, m.Groups[2].Value.Replace(".git", ""));
                    if (!repoInfos.Contains(hostedRemote))
                    {
                        repoInfos.Add(hostedRemote);
                    }
                }
            }

            return repoInfos;
        }
    }
}
