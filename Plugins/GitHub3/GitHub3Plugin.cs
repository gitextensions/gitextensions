using System.ComponentModel.Composition;
using Git.hub;
using GitCommands;
using GitCommands.Config;
using GitCommands.Remotes;
using GitExtensions.Plugins.GitHub3.Properties;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;
using Microsoft;
using ResourceManager;

namespace GitExtensions.Plugins.GitHub3
{
    internal static class GitHubLoginInfo
    {
        private static string? _username;

        public static string? Username
        {
            get
            {
                if (_username == "")
                {
                    return null;
                }

                if (_username is not null)
                {
                    return _username;
                }

                try
                {
                    var user = GitHub3Plugin.GitHub.getCurrentUser();
                    if (user is not null)
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
            get => GitHub3Plugin.Instance.PersonalAccessToken.ValueOrDefault(GitHub3Plugin.Instance.Settings);
            set
            {
                _username = null;
                GitHub3Plugin.Instance.PersonalAccessToken[GitHub3Plugin.Instance.Settings] = value;
                GitHub3Plugin.GitHub.setOAuth2Token(value);
            }
        }
    }

    [Export(typeof(IGitPlugin))]
    public class GitHub3Plugin : GitPluginBase, IRepositoryHostPlugin
    {
        private readonly TranslationString _viewInWebSite = new("View in {0}");
        private readonly TranslationString _tokenAlreadyExist = new("You already have an personal access token. To get a new one, delete your old one in Plugins > Plugin Settings first.");
        private readonly TranslationString _generateToken = new("Generate a GitHub personal access token");
        private readonly TranslationString _manageToken = new("Manage GitHub personal access token");
        private readonly TranslationString _openLinkFailed = new("Fail to open the link. Reason: ");

        public static string GitHubAuthorizationRelativeUrl = "authorizations";
        public static string UpstreamConventionName = "upstream";
        public readonly StringSetting GitHubHost = new("GitHub (Enterprise) hostname", "github.com");
        public readonly StringSetting PersonalAccessToken = new("OAuth Token", "Personal Access Token", "");
        public string GitHubApiEndpoint => $"https://api.{GitHubHost.ValueOrDefault(Settings)}";
        public string GitHubEndpoint => $"https://{GitHubHost.ValueOrDefault(Settings)}";

        internal static GitHub3Plugin Instance = null!;
        internal static Client? _gitHub;
        internal static Client GitHub => _gitHub ??= new(Instance.GitHubApiEndpoint);

        private IGitUICommands? _currentGitUiCommands;
        private IReadOnlyList<IHostedRemote>? _hostedRemotesForModule;

        public GitHub3Plugin() : base(true)
        {
            Id = new Guid("2EC3E1F0-EF37-413F-BEA5-B8FE1F9C505C");
            Name = "GitHub";
            Translate();

            Instance ??= this;

            Icon = Resources.IconGitHub;
        }

        public override IEnumerable<ISetting> GetSettings()
        {
            yield return PersonalAccessToken;

            LinkLabel generateTokenLink = new() { Text = _generateToken.Text };
            generateTokenLink.Click += GenerateTokenLink_Click;
            yield return new PseudoSetting(generateTokenLink);

            LinkLabel manageTokenLink = new() { Text = _manageToken.Text };
            manageTokenLink.Click += ManageTokenLink_Click;
            yield return new PseudoSetting(manageTokenLink);
        }

        private void GenerateTokenLink_Click(object sender, EventArgs e)
        {
            OpenLink($"https://{GitHubHost.ValueOrDefault(Instance.Settings)}/settings/tokens/new?description=Token%20for%20GitExtensions&scopes=repo,public_repo");
        }

        private void ManageTokenLink_Click(object sender, EventArgs e)
        {
            OpenLink($"https://{GitHubHost.ValueOrDefault(Instance.Settings)}/settings/tokens");
        }

        private void OpenLink(string url)
        {
            try
            {
                OsShellUtil.OpenUrlInDefaultBrowser(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(_openLinkFailed.Text + ex.Message);
            }
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            _currentGitUiCommands = gitUiCommands;
            if (!string.IsNullOrEmpty(GitHubLoginInfo.OAuthToken))
            {
                GitHub.setOAuth2Token(GitHubLoginInfo.OAuthToken);
            }
        }

        public override bool Execute(GitUIEventArgs args)
        {
            if (string.IsNullOrEmpty(GitHubLoginInfo.OAuthToken))
            {
                args.GitUICommands.StartSettingsDialog(this);
            }
            else
            {
                MessageBox.Show(args.OwnerForm, _tokenAlreadyExist.Text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        public string? OwnerLogin => GitHub.getCurrentUser()?.Login;

        public async Task<string?> AddUpstreamRemoteAsync()
        {
            Validates.NotNull(_currentGitUiCommands);

            var gitModule = _currentGitUiCommands.GitModule;
            var hostedRemote = GetHostedRemotesForModule().FirstOrDefault(r => r.IsOwnedByMe);
            if (hostedRemote is null)
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
            if (_currentGitUiCommands?.GitModule is null)
            {
                return Array.Empty<IHostedRemote>();
            }

            var gitModule = _currentGitUiCommands.GitModule;
            return Remotes().ToList();

            IEnumerable<IHostedRemote> Remotes()
            {
                HashSet<IHostedRemote> set = new();

                foreach (string remote in gitModule.GetRemoteNames())
                {
                    var url = gitModule.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote));

                    if (string.IsNullOrEmpty(url))
                    {
                        continue;
                    }

                    if (new GitHubRemoteParser().TryExtractGitHubDataFromRemoteUrl(url, out var owner, out var repository))
                    {
                        GitHubHostedRemote hostedRemote = new(remote, owner, repository, url);

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

            ToolStripMenuItem toolStripMenuItem = new(string.Format(_viewInWebSite.Text, Name), Icon);
            contextMenu.Items.Add(toolStripMenuItem);

            foreach (IHostedRemote hostedRemote in _hostedRemotesForModule.OrderBy(r => r.Data))
            {
                ToolStripItem toolStripItem = toolStripMenuItem.DropDownItems.Add(hostedRemote.DisplayData);
                toolStripItem.Click += (s, e) =>
                {
                    if (contextMenu.Tag is GitBlameContext blameContext)
                    {
                        OsShellUtil.OpenUrlInDefaultBrowser(hostedRemote.GetBlameUrl(
                                blameContext.BlameId.ToString(),
                                blameContext.FileName,
                                blameContext.LineIndex + 1));
                    }
                };
            }
        }
    }
}
