using System.ComponentModel.Composition;
using Git.hub;
using GitCommands;
using GitCommands.Config;
using GitCommands.Remotes;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Plugins.GitHub3.Properties;
using GitUI;
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
                    User user = GitHub3Plugin.GitHub.getCurrentUser();
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
    [Export(typeof(IRepositoryHostPlugin))]
    [Export(typeof(IGitPluginForCommit))]
    public class GitHub3Plugin : GitPluginBase, IRepositoryHostPlugin, IGitPluginForCommit
    {
        private readonly TranslationString _viewInWebSite = new("View in {0}");
        private readonly TranslationString _tokenAlreadyExist = new("You already have an personal access token. To get a new one, delete your old one in Plugins > Plugin Settings first.");
        private readonly TranslationString _generateToken = new("Generate a GitHub personal access token");
        private readonly TranslationString _manageToken = new("Manage GitHub personal access token");
        private readonly TranslationString _openLinkFailed = new("Fail to open the link. Reason: ");
        private readonly TranslationString _noteRestartNeeded = new("Note: Git Extensions need to be restarted so that the token is taken into account.");
        private readonly TranslationString _noTokenError = new("No GitHub personal access token (PAT) defined");
        private readonly TranslationString _noAssignedIssues = new("No assigned GitHub issues found");
        private readonly TranslationString _error = new("Error");

        public static string GitHubAuthorizationRelativeUrl = "authorizations";
        public static string UpstreamConventionName = "upstream";
        public readonly StringSetting GitHubHost = new("GitHub (Enterprise) hostname", "github.com");
        public readonly StringSetting PersonalAccessToken = new("OAuth Token", "Personal Access Token", "");
        private readonly BoolSetting _issueCommitMessageHelperEnabled = new("IssueCommitMessageHelperEnabled", "Enable commit message issue helper", true);
        private readonly NumberSetting<int> _issueCommitMessageHelperMaxCount = new("IssueCommitMessageHelperMaxCount", "Maximum number of issues retrieved", 10);
        public string GitHubApiEndpoint => $"https://api.{GitHubHost.ValueOrDefault(Settings)}";
        public string GitHubEndpoint => $"https://{GitHubHost.ValueOrDefault(Settings)}";

        internal static GitHub3Plugin Instance = null!;
        internal static Client? _gitHub;
        internal static Client GitHub => _gitHub ??= new(Instance.GitHubApiEndpoint);
        internal static GitHubRemoteParser _gitHubRemoteParser = new();

        private IGitUICommands? _currentGitUiCommands;
        private IReadOnlyList<IHostedRemote>? _hostedRemotesForModule;
        private List<string> _currentMessages = new();

        public GitHub3Plugin() : base(true)
        {
            Id = new Guid("2EC3E1F0-EF37-413F-BEA5-B8FE1F9C505C");
            Name = "GitHub";
            Translate(AppSettings.CurrentTranslation);

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

            yield return new PseudoSetting(_noteRestartNeeded.Text);

            yield return _issueCommitMessageHelperEnabled;

            yield return _issueCommitMessageHelperMaxCount;
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

            gitUiCommands.PreCommit += GitUiCommands_PreCommit;
            gitUiCommands.PostCommit += GitUiCommands_PostCommit;
        }

        private void GitUiCommands_PreCommit(object? sender, GitUIEventArgs e)
        {
            if (!_issueCommitMessageHelperEnabled.ValueOrDefault(Settings))
            {
                return;
            }

            if (string.IsNullOrEmpty(GitHubLoginInfo.OAuthToken))
            {
                e.GitUICommands.AddCommitTemplate(_noTokenError.Text, () => string.Empty, Icon);
                return;
            }

            ThreadHelper.FileAndForget(async () =>
            {
                IHostedRemote[] hostedRemotes = GetHostedRemotes().ToArray();
                if (hostedRemotes.Length == 0)
                {
                    return;
                }

                IReadOnlyList<Issue> issues = _gitHub.GetAssignedIssues();

                if (issues?.All(i => i.Number == 0) ?? true)
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    e.GitUICommands.AddCommitTemplate(_noAssignedIssues.Text, () => string.Empty, Icon);
                    return;
                }

                Issue[] recentUserIssues = issues.Where(i => i.Number != 0 && hostedRemotes.Any(r => r.Owner == i.Repository.Owner.Login && r.RemoteRepositoryName == i.Repository.Name))
                                                            .OrderByDescending(i => i.UpdatedAt)
                                                            .Take(_issueCommitMessageHelperMaxCount.ValueOrDefault(Settings))
                                                            .ToArray();

                bool multipleRemotes = hostedRemotes.Length > 1;
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                foreach (Issue issue in recentUserIssues)
                {
                    string remoteData = multipleRemotes ? $" ({issue.Repository.Owner.Login}/{issue.Repository.Name})" : string.Empty;
                    string key = $"{issue.Number}: {issue.Title}{remoteData}";
                    _currentMessages.Add(key);
                    e.GitUICommands.AddCommitTemplate(key, () => GetIssueDescription(issue), Icon);
                }

                string GetIssueDescription(Issue issue)
                    => $"""

                        Fixes #{issue.Number} : {issue.Title}

                        {issue.Body}

                        """;
            });
        }

        private void GitUiCommands_PostCommit(object sender, GitUIEventArgs e)
        {
            if (_currentMessages.Count == 0)
            {
                return;
            }

            foreach (string key in _currentMessages)
            {
                e.GitUICommands.RemoveCommitTemplate(key);
            }

            _currentMessages.Clear();
        }

        public override bool Execute(GitUIEventArgs args)
        {
            if (string.IsNullOrEmpty(GitHubLoginInfo.OAuthToken))
            {
                args.GitUICommands.StartSettingsDialog(this);
            }
            else
            {
                MessageBox.Show(args.OwnerForm, _tokenAlreadyExist.Text, _error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            IGitModule gitModule = _currentGitUiCommands.Module;
            IHostedRemote hostedRemote = GetHostedRemotesForModule().FirstOrDefault(r => r.IsOwnedByMe);
            if (hostedRemote is null)
            {
                return null;
            }

            IHostedRepository hostedRepository = hostedRemote.GetHostedRepository();
            if (!hostedRepository.IsAFork)
            {
                return null;
            }

            if ((await gitModule.GetRemotesAsync()).Any(r => r.Name == UpstreamConventionName || r.FetchUrl == hostedRepository.ParentUrl))
            {
                return null;
            }

            gitModule.AddRemote(UpstreamConventionName, hostedRepository.ParentUrl);
            return UpstreamConventionName;
        }

        public bool GitModuleIsRelevantToMe()
            => _currentGitUiCommands?.Module is null
                ? false
                : GetHostedRemotes().Any();

        /// <summary>
        /// Returns all relevant github-remotes for the current working directory
        /// </summary>
        public IReadOnlyList<IHostedRemote> GetHostedRemotesForModule()
            => _currentGitUiCommands?.Module is null ? [] : GetHostedRemotes().ToArray();

        private IEnumerable<IHostedRemote> GetHostedRemotes()
        {
            HashSet<IHostedRemote> set = [];

            IGitModule gitModule = _currentGitUiCommands.Module;
            foreach (string remote in gitModule.GetRemoteNames())
            {
                string url = gitModule.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote));

                if (string.IsNullOrEmpty(url))
                {
                    continue;
                }

                if (_gitHubRemoteParser.TryExtractGitHubDataFromRemoteUrl(url, out string? owner, out string? repository))
                {
                    GitHubHostedRemote hostedRemote = new(remote, owner, repository, url);

                    if (set.Add(hostedRemote))
                    {
                        yield return hostedRemote;
                    }
                }
            }
        }

        public void ConfigureContextMenu(ContextMenuStrip contextMenu)
        {
            const string HostedRemoteMenuItem = "HostedRemoteMenuItem";

            for (int i = contextMenu.Items.Count - 1; i >= 0; i--)
            {
                ToolStripItem item = contextMenu.Items[i];
                if (item is ToolStripMenuItem tsmi && (string)tsmi.Tag == HostedRemoteMenuItem)
                {
                    contextMenu.Items.RemoveAt(i);
                }
            }

            _hostedRemotesForModule = GetHostedRemotesForModule();
            if (_hostedRemotesForModule.Count == 0)
            {
                return;
            }

            ToolStripMenuItem toolStripMenuItem = new(string.Format(_viewInWebSite.Text, Name), Icon);
            toolStripMenuItem.Tag = HostedRemoteMenuItem;
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
