using System.ComponentModel.Composition;
using Atlassian.Jira;
using Atlassian.Jira.Remote;
using GitCommands;
using GitExtensions.Plugins.JiraCommitHintPlugin.Properties;
using GitExtUtils.GitUI;
using GitUI;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.Settings;
using GitUIPluginInterfaces.UserControls;
using Microsoft;
using NString;
using ResourceManager;
using RestSharp.Authenticators;

namespace GitExtensions.Plugins.JiraCommitHintPlugin
{
    [Export(typeof(IGitPlugin))]
    [Export(typeof(IGitPluginForCommit))]
    public class JiraCommitHintPlugin : GitPluginBase, IGitPluginForCommit
    {
        private static readonly TranslationString JiraFieldsLabel = new("Jira fields");
        private static readonly TranslationString QueryHelperLinkText = new("Open the query helper inside Jira");
        private static readonly TranslationString InvalidUrlMessage = new("A valid url is required to launch the Jira query helper");
        private static readonly TranslationString InvalidUrlCaption = new("Invalid Jira url");
        private static readonly TranslationString PreviewButtonText = new("Preview");
        private static readonly TranslationString QueryHelperOpenErrorText = new("Unable to open Jira query helper");
        private static readonly TranslationString EmptyQueryResultMessage = new("[Empty Jira Query Result]");
        private static readonly TranslationString EmptyQueryResultCaption = new("First Task Preview");
        private static readonly TranslationString JiraPluginConfigurationErrorText = new("Jira plugin configuration incomplete!");

        private const string DefaultFormat = "{Key} {Summary}";
        private const string AuthTypeUsernamePassword = "User name and password";
        private const string AuthTypePersonalAccessToken = "Personal access token";

        // It not a good idea use this user name for Personal access token, but credentials required.
        private const string AuthTypePersonalAccessTokenUserName = "#GitExtensions/jira.commit.hint:b0128e39-d312-47da-b18a-43f5ca726d7d/access.token$";
        private Jira? _jira;
        private string? _query;
        private string _stringTemplate = DefaultFormat;
        private readonly BoolSetting _enabledSettings = new("Jira hint plugin enabled", false);
        private readonly StringSetting _urlSettings = new("Jira URL", @"https://jira.atlassian.com");
        private readonly ChoiceSetting _authTypeSettings = new("Auth type", new List<string> { AuthTypeUsernamePassword, AuthTypePersonalAccessToken });
        private readonly CredentialsSetting _credentialsSettings;

        // For compatibility reason, the setting key is kept to "JDL Query" even if the label is, rightly, "JQL Query" (for "Jira Query Language")
        private readonly StringSetting _jqlQuerySettings = new("JDL Query", "JQL Query", "assignee = currentUser() and resolution is EMPTY ORDER BY updatedDate DESC", true);
        private readonly StringSetting _stringTemplateSetting = new("Jira Message Template", "Message Template", DefaultFormat, true);
        private readonly string _jiraFields = $"{{{string.Join("} {", typeof(Issue).GetProperties().Where(i => i.CanRead).Select(i => i.Name).OrderBy(i => i).ToArray())}}}";
        private IGitModule? _gitModule;
        private JiraTaskDTO[]? _currentMessages;
        private Button? _btnPreview;

        public JiraCommitHintPlugin() : base(true)
        {
            Id = new Guid("B0128E39-D312-47DA-B18A-43F5CA726D7D");
            Name = "Jira Commit Hint";
            Translate();
            Icon = Resources.IconJira;

            _credentialsSettings = new CredentialsSetting("JiraCredentials", "Jira credentials", () => _gitModule?.WorkingDir);
        }

        public override bool Execute(GitUIEventArgs args)
        {
            if (!_enabledSettings.ValueOrDefault(Settings))
            {
                args.GitUICommands.StartSettingsDialog(this);
                return false;
            }

            if (_jira is null || _query is null)
            {
                return false;
            }

            ThreadHelper.FileAndForget(async () =>
                {
                    JiraTaskDTO[] message = await GetMessageToCommitAsync(_jira, _query, _stringTemplate);
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    MessageBox.Show(string.Join(Environment.NewLine, message.Select(jt => jt.Text).ToArray()), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });

            return false;
        }

        public override IEnumerable<ISetting> GetSettings()
        {
            yield return _enabledSettings;

            _urlSettings.CustomControl = new TextBox();
            yield return _urlSettings;

            _authTypeSettings.CustomControl = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            _authTypeSettings.Values.ForEach(value => _authTypeSettings.CustomControl.Items.Add(value));
            _authTypeSettings.CustomControl.SelectedIndexChanged += authTypeSetting_SelectedIndexChanged;
            yield return _authTypeSettings;

            _credentialsSettings.CustomControl = new CredentialsControl();
            yield return _credentialsSettings;

            _jqlQuerySettings.CustomControl = new TextBox();
            yield return _jqlQuerySettings;

            LinkLabel queryHelperLink = new() { Text = QueryHelperLinkText.Text };
            queryHelperLink.Click += QueryHelperLink_Click;
            yield return new PseudoSetting(queryHelperLink);

            yield return new PseudoSetting(_jiraFields, JiraFieldsLabel.Text, DpiUtil.Scale(55));

            TextBox txtTemplate = new()
            {
                Height = DpiUtil.Scale(75),
                Multiline = true,
                ScrollBars = ScrollBars.Horizontal
            };
            _btnPreview = new Button
            {
                Text = PreviewButtonText.Text,
                Top = DpiUtil.Scale(45),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            _btnPreview.Size = DpiUtil.Scale(_btnPreview.Size);
            _btnPreview.Click += btnPreviewClick;
            _btnPreview.Left = txtTemplate.Width - _btnPreview.Width - DpiUtil.Scale(8);
            txtTemplate.Controls.Add(_btnPreview);
            _stringTemplateSetting.CustomControl = txtTemplate;
            yield return _stringTemplateSetting;
        }

        private void QueryHelperLink_Click(object sender, EventArgs e)
        {
            Validates.NotNull(_urlSettings.CustomControl);
            if (string.IsNullOrWhiteSpace(_urlSettings.CustomControl.Text))
            {
                MessageBox.Show(null, InvalidUrlMessage.Text, InvalidUrlCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                OsShellUtil.OpenUrlInDefaultBrowser(_urlSettings.CustomControl.Text + "/secure/IssueNavigator.jspa?mode=show&createNew=true");
            }
            catch (Exception ex)
            {
                MessageBox.Show(null, ex.Message, QueryHelperOpenErrorText.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPreviewClick(object sender, EventArgs eventArgs)
        {
            Validates.NotNull(_btnPreview);

            try
            {
                Validates.NotNull(_urlSettings.CustomControl);
                Validates.NotNull(_authTypeSettings.CustomControl);
                Validates.NotNull(_credentialsSettings.CustomControl);
                Validates.NotNull(_jqlQuerySettings.CustomControl);
                Validates.NotNull(_stringTemplateSetting.CustomControl);

                _btnPreview.Enabled = false;

                Jira localJira = CreateJiraClient(_urlSettings.CustomControl.Text, _credentialsSettings.CustomControl.UserName, _credentialsSettings.CustomControl.Password, _authTypeSettings.CustomControl.SelectedItem.ToString());
                string localQuery = _jqlQuerySettings.CustomControl.Text;
                string localStringTemplate = _stringTemplateSetting.CustomControl.Text;

                ThreadHelper.FileAndForget(async () =>
                    {
                        JiraTaskDTO[] message = await GetMessageToCommitAsync(localJira, localQuery, localStringTemplate);
                        await _btnPreview.SwitchToMainThreadAsync();
                        string previewText = message.Length > 0 ? message[0].Text : EmptyQueryResultMessage.Text;

                        MessageBox.Show(null, previewText, EmptyQueryResultCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

                        _btnPreview.Enabled = true;
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _btnPreview.Enabled = true;
            }
        }

        private void authTypeSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox is null || _credentialsSettings.CustomControl is null)
            {
                return;
            }

            // Saved value last time, or default value.
            System.Net.NetworkCredential credentials = _credentialsSettings.GetValueOrDefault(Settings);

            if (comboBox.SelectedIndex == _authTypeSettings.Values.IndexOf(AuthTypePersonalAccessToken))
            {
                // If auth type PAT selected, we should change ui to PAT mode.
                _credentialsSettings.CustomControl.ChangeUIMode(showUserName: false, passwordLabelText: "Personal access token");
                _credentialsSettings.CustomControl.UserName = AuthTypePersonalAccessTokenUserName;
                if (credentials.UserName == AuthTypePersonalAccessTokenUserName)
                {
                    _credentialsSettings.CustomControl.Password = credentials.Password;
                }
                else
                {
                    _credentialsSettings.CustomControl.Password = "";
                }
            }
            else
            {
                // If auth type not PAT, we think it was PWD
                _credentialsSettings.CustomControl.ChangeUIMode(showUserName: true, passwordLabelText: "Password");
                if (credentials.UserName != AuthTypePersonalAccessTokenUserName)
                {
                    _credentialsSettings.CustomControl.UserName = credentials.UserName;
                    _credentialsSettings.CustomControl.Password = credentials.Password;
                }
                else
                {
                    _credentialsSettings.CustomControl.UserName = "";
                    _credentialsSettings.CustomControl.Password = "";
                }
            }
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            base.Register(gitUiCommands);
            _gitModule = gitUiCommands.Module;
            gitUiCommands.PostSettings += gitUiCommands_PostSettings;
            gitUiCommands.PreCommit += gitUiCommands_PreCommit;
            gitUiCommands.PostCommit += gitUiCommands_PostRepositoryChanged;
            gitUiCommands.PostRepositoryChanged += gitUiCommands_PostRepositoryChanged;
            UpdateJiraSettings();
        }

        private void UpdateJiraSettings()
        {
            if (!_enabledSettings.ValueOrDefault(Settings))
            {
                return;
            }

            string url = _urlSettings.ValueOrDefault(Settings);
            System.Net.NetworkCredential credentials = _credentialsSettings.GetValueOrDefault(Settings);
            string authType = _authTypeSettings.ValueOrDefault(Settings);

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(credentials.UserName) || string.IsNullOrWhiteSpace(credentials.Password))
            {
                return;
            }

            _jira = CreateJiraClient(url, credentials.UserName, credentials.Password, authType);
            _query = _jqlQuerySettings.ValueOrDefault(Settings);
            _stringTemplate = _stringTemplateSetting.ValueOrDefault(Settings);
            if (_btnPreview is null)
            {
                return;
            }

            _btnPreview.Click -= btnPreviewClick;
            _btnPreview = null;

            if (_authTypeSettings.CustomControl is not null)
            {
                _authTypeSettings.CustomControl.SelectedIndexChanged -= authTypeSetting_SelectedIndexChanged;
                _authTypeSettings.CustomControl = null;
            }
        }

        private void gitUiCommands_PostSettings(object sender, GitUIPostActionEventArgs e)
        {
            UpdateJiraSettings();
        }

        public override void Unregister(IGitUICommands gitUiCommands)
        {
            base.Unregister(gitUiCommands);
            gitUiCommands.PreCommit -= gitUiCommands_PreCommit;
            gitUiCommands.PostCommit -= gitUiCommands_PostRepositoryChanged;
            gitUiCommands.PostSettings -= gitUiCommands_PostSettings;
            gitUiCommands.PostRepositoryChanged -= gitUiCommands_PostRepositoryChanged;
        }

        private void gitUiCommands_PreCommit(object sender, GitUIEventArgs e)
        {
            if (!_enabledSettings.ValueOrDefault(Settings))
            {
                return;
            }

            if (_jira?.Issues is null || _query is null)
            {
                e.GitUICommands.AddCommitTemplate(JiraPluginConfigurationErrorText.Text, () => string.Empty, Icon);
                return;
            }

            ThreadHelper.FileAndForget(async () =>
            {
                JiraTaskDTO[] currentMessages = await GetMessageToCommitAsync(_jira, _query, _stringTemplate);

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                _currentMessages = currentMessages;
                foreach (JiraTaskDTO message in _currentMessages)
                {
                    e.GitUICommands.AddCommitTemplate(message.Title, () => message.Text, Icon);
                }
            });
        }

        private void gitUiCommands_PostRepositoryChanged(object sender, GitUIEventArgs e)
        {
            if (!_enabledSettings.ValueOrDefault(Settings))
            {
                return;
            }

            if (_currentMessages is null)
            {
                return;
            }

            foreach (JiraTaskDTO message in _currentMessages)
            {
                e.GitUICommands.RemoveCommitTemplate(message.Title);
            }

            _currentMessages = null;
        }

        private async Task<JiraTaskDTO[]> GetMessageToCommitAsync(Jira jira, string query, string stringTemplate)
        {
            try
            {
                IPagedQueryResult<Issue> results = await jira.Issues.GetIssuesFromJqlAsync(query);
                return results
                    .Select(issue => new JiraTaskDTO(issue.Key + ": " + issue.Summary, StringTemplate.Format(stringTemplate, issue)))
                    .ToArray();
            }
            catch (Exception ex)
            {
                return new[] { new JiraTaskDTO($"{Name} error", ex.ToString()) };
            }
        }

        private Jira CreateJiraClient(string url, string username, string password, string authType)
        {
            if (authType == AuthTypePersonalAccessToken)
            {
                JiraRestClientSettings settings = new();
                return Jira.CreateRestClient(new JiraJwtRestClient(url, password), settings.Cache);
            }

            return Jira.CreateRestClient(url, username, password);
        }

        private class JiraTaskDTO
        {
            public string Title { get; }
            public string Text { get; }

            public JiraTaskDTO(string title, string text)
            {
                Title = title;
                Text = text;
            }
        }

        private class JiraJwtRestClient : JiraRestClient
        {
            public JiraJwtRestClient(string url, string accessToken) : base(url, new JwtAuthenticator(accessToken))
            {
            }
        }
    }
}
