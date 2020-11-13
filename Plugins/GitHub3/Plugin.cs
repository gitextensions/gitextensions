using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Git.hub;
using GitExtensions.Core.Commands;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Events;
using GitExtensions.Extensibility.RepositoryHosts;
using GitExtensions.Extensibility.Settings;
using GitHub3.Properties;

namespace GitHub3
{
    [Export(typeof(IGitPlugin))]
    public sealed class Plugin : IGitPlugin,
        IRepositoryHostPlugin,
        IGitPluginConfigurable,
        ILoadHandler
    {
        // Extract from GitCommands.Config.SettingKeyString
        private const string RemoteUrl = "remote.{0}.url";

        public static string GitHubAuthorizationRelativeUrl = "authorizations";
        public static string UpstreamConventionName = "upstream";
        public readonly StringSetting GitHubHost = new StringSetting("GitHub (Enterprise) hostname", Strings.GitHubHost, "github.com");
        public readonly StringSetting OAuthToken = new StringSetting("OAuth Token", Strings.OAuthToken, string.Empty);
        public string GitHubApiEndpoint => $"https://api.{GitHubHost.ValueOrDefault(SettingsContainer.GetSettingsSource())}";
        public string GitHubEndpoint => $"https://{GitHubHost.ValueOrDefault(SettingsContainer.GetSettingsSource())}";

        internal static Plugin Instance;
        internal static Client _gitHub;
        internal static Client GitHub => _gitHub ??= new Client(Instance.GitHubApiEndpoint);

        private IGitUICommands _currentGitUiCommands;
        private IReadOnlyList<IHostedRemote> _hostedRemotesForModule;

        public Plugin()
        {
            Instance ??= this;
        }

        public string Name => "GitHub";

        public string Description => Strings.Description;

        public Image Icon => Images.IconGitHub;

        public IGitPluginSettingsContainer SettingsContainer { get; set; }

        public IEnumerable<ISetting> GetSettings()
        {
            yield return OAuthToken;
        }

        public void OnLoad(IGitUICommands gitUiCommands)
        {
            _currentGitUiCommands = gitUiCommands;

            if (!string.IsNullOrEmpty(GitHubLoginInfo.OAuthToken))
            {
                GitHub.setOAuth2Token(GitHubLoginInfo.OAuthToken);
            }
        }

        public bool Execute(GitUIEventArgs args)
        {
            if (string.IsNullOrEmpty(GitHubLoginInfo.OAuthToken))
            {
                var authorizationApiUrl = new Uri(new Uri(GitHubApiEndpoint), GitHubAuthorizationRelativeUrl).ToString();
                using (var gitHubCredentialsPrompt = new GitHubCredentialsPrompt(authorizationApiUrl))
                {
                    gitHubCredentialsPrompt.ShowDialog(args.OwnerForm);
                }
            }
            else
            {
                MessageBox.Show(args.OwnerForm, Strings.TokenAlreadyExist, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        // --

        public IReadOnlyList<IHostedRepository> SearchForRepository(string search)
        {
            return GitHub.searchRepositories(search).Select(repo => (IHostedRepository)new GitHubRepo(repo)).ToList();
        }

        public IReadOnlyList<IHostedRepository> GetRepositoriesOfUser(string user)
        {
            return GitHub.getRepositories(user).Select(repo => (IHostedRepository)new GitHubRepo(repo)).ToList();
        }

        public IHostedRepository GetRepository(string user, string repositoryName)
        {
            return new GitHubRepo(GitHub.getRepository(user, repositoryName));
        }

        public IReadOnlyList<IHostedRepository> GetMyRepos()
        {
            return GitHub.getRepositories().Select(repo => (IHostedRepository)new GitHubRepo(repo)).ToList();
        }

        public bool ConfigurationOk => !string.IsNullOrEmpty(GitHubLoginInfo.OAuthToken);

        public string OwnerLogin => GitHub.getCurrentUser()?.Login;

        public async Task<string> AddUpstreamRemoteAsync()
        {
            var gitModule = _currentGitUiCommands.GitModule;
            var hostedRemote = GetHostedRemotesForModule().FirstOrDefault(r => r.IsOwnedByMe);
            if (hostedRemote == null)
            {
                return null;
            }

            var hostedRepository = hostedRemote.GetHostedRepository();
            if (!hostedRepository.IsAFork)
            {
                return null;
            }

            if ((await gitModule.GetRemotesAsync()).Any(r => r.Name == UpstreamConventionName || r.FetchUrl == hostedRepository.ParentReadOnlyUrl))
            {
                return null;
            }

            gitModule.AddRemote(UpstreamConventionName, hostedRepository.ParentReadOnlyUrl);
            return UpstreamConventionName;
        }

        public bool GitModuleIsRelevantToMe()
        {
            return GetHostedRemotesForModule().Count > 0;
        }

        /// <summary>
        /// Returns all relevant github-remotes for the current working directory
        /// </summary>
        public IReadOnlyList<IHostedRemote> GetHostedRemotesForModule()
        {
            if (_currentGitUiCommands?.GitModule == null)
            {
                return Array.Empty<IHostedRemote>();
            }

            var gitModule = _currentGitUiCommands.GitModule;
            return Remotes().ToList();

            IEnumerable<IHostedRemote> Remotes()
            {
                var set = new HashSet<IHostedRemote>();

                foreach (string remote in gitModule.GetRemoteNames())
                {
                    var url = gitModule.GetSetting(string.Format(RemoteUrl, remote));

                    if (string.IsNullOrEmpty(url))
                    {
                        continue;
                    }

                    if (new GitHubRemoteParser().TryExtractGitHubDataFromRemoteUrl(url, out var owner, out var repository))
                    {
                        var hostedRemote = new GitHubHostedRemote(remote, owner, repository, url);

                        if (set.Add(hostedRemote))
                        {
                            yield return hostedRemote;
                        }
                    }
                }
            }
        }

        public void ConfigureContextMenu(ContextMenuStrip contextMenu)
        {
            _hostedRemotesForModule = GetHostedRemotesForModule();
            if (_hostedRemotesForModule.Count == 0)
            {
                return;
            }

            var toolStripMenuItem = new ToolStripMenuItem(string.Format(Strings.ViewInWebSite, Name), Icon);
            contextMenu.Items.Add(toolStripMenuItem);
            toolStripMenuItem.Click += (s, e) => Process.Start(_hostedRemotesForModule.First().Data);

            foreach (IHostedRemote hostedRemote in _hostedRemotesForModule.OrderBy(r => r.Data))
            {
                ToolStripItem toolStripItem = toolStripMenuItem.DropDownItems.Add(hostedRemote.DisplayData);
                toolStripItem.Click += (s, e) =>
                {
                    var blameContext = contextMenu?.Tag as GitBlameContext;
                    if (blameContext == null)
                    {
                        return;
                    }

                    Process.Start(hostedRemote.GetBlameUrl(blameContext.BlameId.ToString(), blameContext.FileName,
                        blameContext.LineIndex + 1));
                };
            }
        }
    }

    internal static class GitHubLoginInfo
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
                    var user = Plugin.GitHub.getCurrentUser();
                    if (user != null)
                    {
                        _username = user.Login;
                        return _username;
                    }
                    else
                    {
                        _username = "";
                    }

                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public static string OAuthToken
        {
            get => Plugin.Instance.OAuthToken.ValueOrDefault(Plugin.Instance.SettingsContainer.GetSettingsSource());
            set
            {
                _username = null;
                Plugin.Instance.OAuthToken[Plugin.Instance.SettingsContainer.GetSettingsSource()] = value;
                Plugin.GitHub.setOAuth2Token(value);
            }
        }
    }
}
