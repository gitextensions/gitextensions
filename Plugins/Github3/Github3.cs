using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Git.hub;
using GitCommands.Config;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;

namespace Github3
{
    class GithubAPIInfo
    {
        internal static string client_id = "ebc0e8947c206610d737";
        internal static string client_secret = "c993907df3f45145bf638842692b69c56d1ace4d";
    }

    class GithubLoginInfo
    {
        private static string _username;
        public static string username
        {
            get
            {
                if (_username == "")
                    return null;
                if (_username != null)
                    return _username;

                try
                {
                    var user = Github3.github.getCurrentUser();
                    if (user != null)
                    {
                        _username = user.Login;
                        //MessageBox.Show("Github username: " + _username);
                        return _username;
                    }
                    else
                        _username = "";

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
            get
            {
                return Github3.instance.OAuthToken[Github3.instance.Settings];
            }
            set
            {
                _username = null;
                Github3.instance.OAuthToken[Github3.instance.Settings] = value;
                Github3.github.setOAuth2Token(value);
            }
        }
    }

    public class Github3 : GitPluginBase, IRepositoryHostPlugin
    {
        public StringSetting OAuthToken = new StringSetting("OAuth Token", "");
        
        internal static Github3 instance;
        internal static Client github;
        public Github3()
        {
            if (instance != null)
                throw new InvalidOperationException("tried to create second instance");

            instance = this;

            github = new Client();
        }

        public override string Description
        {
            get { return "Github"; }
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

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            if (string.IsNullOrEmpty(GithubLoginInfo.OAuthToken))
            {
                using (var frm = new OAuth())
                    frm.ShowDialog(gitUiCommands.OwnerForm);
            }
            else
            {
                MessageBox.Show(gitUiCommands.OwnerForm, "You already have an OAuth token. To get a new one, delete your old one in Plugins > Settings first.");
            }
            return false;
        }

        // --

        public IList<IHostedRepository> SearchForRepository(string search)
        {
            return github.searchRepositories(search).Select(repo => (IHostedRepository)new GithubRepo(repo)).ToList();
        }

        public IList<IHostedRepository> GetRepositoriesOfUser(string user)
        {
            return github.getRepositories(user).Select(repo => (IHostedRepository)new GithubRepo(repo)).ToList();
        }

        public IHostedRepository GetRepository(string user, string repositoryName)
        {
            return new GithubRepo(github.getRepository(user, repositoryName));
        }

        public IList<IHostedRepository> GetMyRepos()
        {
            return github.getRepositories().Select(repo => (IHostedRepository)new GithubRepo(repo)).ToList();
        }

        public bool ConfigurationOk { get { return true; } }
        public bool GitModuleIsRelevantToMe(IGitModule aModule)
        {
            return GetHostedRemotesForModule(aModule).Count > 0;
        }

        /// <summary>
        /// Returns all relevant github-remotes for the current working directory
        /// </summary>
        /// <returns></returns>
        public List<IHostedRemote> GetHostedRemotesForModule(IGitModule aModule)
        {
            var repoInfos = new List<IHostedRemote>();

            string[] remotes = aModule.GetRemotes(false);
            foreach (string remote in remotes)
            {
                var url = aModule.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote));
                if (string.IsNullOrEmpty(url))
                    continue;

                var m = Regex.Match(url, @"git(?:@|://)github.com[:/]([^/]+)/([\w_\.\-]+)\.git");
                if (!m.Success)
                    m = Regex.Match(url, @"https?://(?:[^@:]+)?(?::[^/@:]+)?@?github.com/([^/]+)/([\w_\.\-]+)(?:.git)?");
                if (m.Success)
                {
                    var hostedRemote = new GithubHostedRemote(remote, m.Groups[1].Value, m.Groups[2].Value.Replace(".git", ""));
                    if (!repoInfos.Contains(hostedRemote))
                        repoInfos.Add(hostedRemote);
                }
            }

            return repoInfos;
        }
    }
}
