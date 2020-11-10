using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Atlassian.Jira;
using GitExtensions.Core.Commands;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Core.Module;
using GitExtensions.Core.Utils.UI;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Events;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Settings.UserControls;
using JiraCommitHintPlugin.Properties;
using NString;

namespace JiraCommitHintPlugin
{
    [Export(typeof(IGitPlugin))]
    public sealed class Plugin : IGitPlugin,
        IGitPluginForRepository,
        IGitPluginConfigurable,
        IGitPluginExecutable,
        ILoadHandler,
        IPostSettingsHandler,
        IPreCommitHandler,
        IPostCommitHandler,
        IPostRepositoryChangedHandler
    {
        private const string DefaultFormat = "{Key} {Summary}";
        private Jira _jira;
        private string _query;
        private string _stringTemplate = DefaultFormat;
        private readonly BoolSetting _enabledSettings = new BoolSetting("Jira hint plugin enabled", Strings.EnabledSettings, false);
        private readonly StringSetting _urlSettings = new StringSetting("Jira URL", Strings.UrlSettings, @"https://jira.atlassian.com");
        private readonly CredentialsSetting _credentialsSettings;

        // For compatibility reason, the setting key is kept to "JDL Query" even if the label is, rightly, "JQL Query" (for "Jira Query Language")
        private readonly StringSetting _jqlQuerySettings = new StringSetting("JDL Query", Strings.JqlQuerySettings, "assignee = currentUser() and resolution is EMPTY ORDER BY updatedDate DESC", true);
        private readonly StringSetting _stringTemplateSetting = new StringSetting("Jira Message Template", Strings.StringTemplateSetting, DefaultFormat, true);
        private readonly string _jiraFields = $"{{{string.Join("} {", typeof(Issue).GetProperties().Where(i => i.CanRead).Select(i => i.Name).OrderBy(i => i).ToArray())}}}";
        private IGitModule _gitModule;
        private JiraTaskDTO[] _currentMessages;
        private Button _btnPreview;

        public Plugin()
        {
            _credentialsSettings = new CredentialsSetting("JiraCredentials", Strings.CredentialsSettings, () => _gitModule?.WorkingDir);
        }

        public string Name => "Jira Commit Hint";

        public string Description => Strings.Description;

        public Image Icon => Images.IconJira;

        public IGitPluginSettingsContainer SettingsContainer { get; set; }

        public bool Execute(GitUIEventArgs args)
        {
            if (!_enabledSettings.ValueOrDefault(SettingsContainer.GetSettingsSource()))
            {
                args.GitUICommands.StartSettingsDialog(GetType());
                return false;
            }

            if (_jira == null)
            {
                return false;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    var message = await GetMessageToCommitAsync(_jira, _query, _stringTemplate);
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    MessageBox.Show(string.Join(Environment.NewLine, message.Select(jt => jt.Text).ToArray()), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                });

            return false;
        }

        public IEnumerable<ISetting> GetSettings()
        {
            yield return _enabledSettings;

            _urlSettings.CustomControl = new TextBox();
            yield return _urlSettings;

            _credentialsSettings.CustomControl = new CredentialsControl();
            yield return _credentialsSettings;

            _jqlQuerySettings.CustomControl = new TextBox();
            yield return _jqlQuerySettings;

            var queryHelperLink = new LinkLabel { Text = Strings.QueryHelperLinkText };
            queryHelperLink.Click += QueryHelperLink_Click;
            yield return new PseudoSetting(queryHelperLink);

            yield return new PseudoSetting(_jiraFields, Strings.JiraFieldsLabel, DpiUtil.Scale(55));

            var txtTemplate = new TextBox
            {
                Height = DpiUtil.Scale(75),
                Multiline = true,
                ScrollBars = ScrollBars.Horizontal
            };
            _btnPreview = new Button
            {
                Text = Strings.PreviewButtonText,
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
            if (string.IsNullOrWhiteSpace(_urlSettings.CustomControl.Text))
            {
                MessageBox.Show(null, Strings.InvalidUrlMessage, Strings.InvalidUrlCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Process.Start(_urlSettings.CustomControl.Text + "/secure/IssueNavigator.jspa?mode=show&createNew=true");
            }
            catch (Exception ex)
            {
                MessageBox.Show(null, ex.Message, Strings.QueryHelperOpenErrorText, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPreviewClick(object sender, EventArgs eventArgs)
        {
            try
            {
                _btnPreview.Enabled = false;

                var localJira = Jira.CreateRestClient(_urlSettings.CustomControl.Text, _credentialsSettings.CustomControl.UserName,
                    _credentialsSettings.CustomControl.Password);
                var localQuery = _jqlQuerySettings.CustomControl.Text;
                var localStringTemplate = _stringTemplateSetting.CustomControl.Text;

                ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        var message = await GetMessageToCommitAsync(localJira, localQuery, localStringTemplate);
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        var preview = message.FirstOrDefault();

                        MessageBox.Show(null, preview == null ? Strings.EmptyQueryResultMessage : preview.Text, Strings.EmptyQueryResultCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);

                        _btnPreview.Enabled = true;
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _btnPreview.Enabled = true;
            }
        }

        public void OnLoad(IGitUICommands gitUiCommands)
        {
            _gitModule = gitUiCommands.GitModule;

            UpdateJiraSettings();
        }

        private void UpdateJiraSettings()
        {
            if (!_enabledSettings.ValueOrDefault(SettingsContainer.GetSettingsSource()))
            {
                return;
            }

            var url = _urlSettings.ValueOrDefault(SettingsContainer.GetSettingsSource());
            var credentials = _credentialsSettings.GetValueOrDefault(SettingsContainer.GetSettingsSource());

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(credentials.UserName))
            {
                return;
            }

            _jira = Jira.CreateRestClient(url, credentials.UserName, credentials.Password);
            _query = _jqlQuerySettings.ValueOrDefault(SettingsContainer.GetSettingsSource());
            _stringTemplate = _stringTemplateSetting.ValueOrDefault(SettingsContainer.GetSettingsSource());
            if (_btnPreview == null)
            {
                return;
            }

            _btnPreview.Click -= btnPreviewClick;
            _btnPreview = null;
        }

        public void OnPostSettings(GitUIPostActionEventArgs e)
        {
            UpdateJiraSettings();
        }

        public void OnPreCommit(GitUIEventArgs e)
        {
            if (!_enabledSettings.ValueOrDefault(SettingsContainer.GetSettingsSource()))
            {
                return;
            }

            if (_jira?.Issues == null)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                var currentMessages = await GetMessageToCommitAsync(_jira, _query, _stringTemplate);

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                _currentMessages = currentMessages;
                foreach (var message in _currentMessages)
                {
                    e.GitUICommands.AddCommitTemplate(message.Title, () => message.Text, Icon);
                }
            });
        }

        public void OnPostCommit(GitUIPostActionEventArgs e)
        {
            OnPostRepositoryChanged(e);
        }

        public void OnPostRepositoryChanged(GitUIEventArgs e)
        {
            if (!_enabledSettings.ValueOrDefault(SettingsContainer.GetSettingsSource()))
            {
                return;
            }

            if (_currentMessages == null)
            {
                return;
            }

            foreach (var message in _currentMessages)
            {
                e.GitUICommands.RemoveCommitTemplate(message.Title);
            }

            _currentMessages = null;
        }

        private async Task<JiraTaskDTO[]> GetMessageToCommitAsync(Jira jira, string query, string stringTemplate)
        {
            try
            {
                var results = await jira.Issues.GetIssuesFromJqlAsync(query);
                return results
                    .Select(issue => new JiraTaskDTO(issue.Key + ": " + issue.Summary, StringTemplate.Format(stringTemplate, issue)))
                    .ToArray();
            }
            catch (Exception ex)
            {
                return new[] { new JiraTaskDTO($"{Description} error", ex.ToString()) };
            }
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
    }
}
