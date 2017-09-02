using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Atlassian.Jira;
using GitUIPluginInterfaces;
using NString;
using ResourceManager;

namespace JiraCommitHintPlugin
{
    public class JiraCommitHintPlugin : GitPluginBase, IGitPluginForRepository
    {
        private const string description = "Jira Commit Hint";
        private const string defaultFormat = "{Key} {Summary}";
        private Jira jira;
        private string query;
        private string stringTemplate = defaultFormat;
        private readonly BoolSetting enabledSettings = new BoolSetting("Jira hint plugin enabled", false);
        private readonly StringSetting urlSettings = new StringSetting("Jira URL", @"https://jira.atlassian.com");
        private readonly StringSetting userSettings = new StringSetting("Jira user", string.Empty);
        private readonly PasswordSetting passwordSettings = new PasswordSetting("Jira password", string.Empty);
        private readonly StringSetting jdlQuerySettings = new StringSetting("JDL Query", "assignee = currentUser() and resolution is EMPTY ORDER BY updatedDate DESC");
        private readonly StringSetting stringTemplateSetting = new StringSetting("Jira Message Template", "Message Template", defaultFormat);
        private readonly StringSetting jiraFields = new StringSetting("Jira fields", $"{{{string.Join("} {", typeof(Issue).GetProperties().Where(i => i.CanRead).Select(i => i.Name).OrderBy(i => i).ToArray())}}}");
        private JiraTaskDTO[] currentMessages;
        private Button btnPreview;

        public JiraCommitHintPlugin()
        {
            Description = description;
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            if (enabledSettings.ValueOrDefault(Settings))
                return false;
            if (jira == null)
                return false;
            MessageBox.Show(string.Join(Environment.NewLine, GetMessageToCommit(jira, query, stringTemplate).Select(t => t.Text).ToArray()));
            return false;
        }

        public override IEnumerable<ISetting> GetSettings()
        {
            yield return enabledSettings;
            jiraFields.CustomControl = new TextBox { ReadOnly = true, Multiline = true, Height = 55, BorderStyle = BorderStyle.None };
            yield return jiraFields;
            urlSettings.CustomControl = new TextBox();
            yield return urlSettings;
            userSettings.CustomControl = new TextBox();
            yield return userSettings;
            passwordSettings.CustomControl = new TextBox { UseSystemPasswordChar = true };
            yield return passwordSettings;
            jdlQuerySettings.CustomControl = new TextBox();
            yield return jdlQuerySettings;
            var txtTemplate = new TextBox { Height = 75, Multiline = true, ScrollBars = ScrollBars.Horizontal };
            btnPreview = new Button { Text = "Preview", Top = 45, Anchor = AnchorStyles.Right };
            btnPreview.Click += btnPreviewClick;
            txtTemplate.Controls.Add(btnPreview);
            stringTemplateSetting.CustomControl = txtTemplate;
            yield return stringTemplateSetting;
        }

        private void btnPreviewClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var localJira = Jira.CreateRestClient(urlSettings.CustomControl.Text, userSettings.CustomControl.Text, passwordSettings.CustomControl.Text);
                var localQuery = jdlQuerySettings.CustomControl.Text;
                var localStringTemplate = stringTemplateSetting.CustomControl.Text;
                var preview = GetMessageToCommit(localJira, localQuery, localStringTemplate).FirstOrDefault();
                MessageBox.Show($"First Task Preview:{Environment.NewLine}{Environment.NewLine}{(preview == null ? "[Empty Jira Query Result]" : preview.Text)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            base.Register(gitUiCommands);
            gitUiCommands.PostSettings += gitUiCommands_PostSettings;
            gitUiCommands.PreCommit += gitUiCommands_PreCommit;
            gitUiCommands.PostCommit += gitUiCommands_PostRepositoryChanged;
            gitUiCommands.PostRepositoryChanged += gitUiCommands_PostRepositoryChanged;
            UpdateJiraSettings();
        }

        private void UpdateJiraSettings()
        {
            if (!enabledSettings.ValueOrDefault(Settings))
                return;

            var url = urlSettings.ValueOrDefault(Settings);
            var userName = userSettings.ValueOrDefault(Settings);
            var password = passwordSettings.ValueOrDefault(Settings);

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(userName))
                return;

            jira = Jira.CreateRestClient(url, userName, password);
            query = jdlQuerySettings.ValueOrDefault(Settings);
            stringTemplate = stringTemplateSetting.ValueOrDefault(Settings);
            if (btnPreview == null)
                return;
            btnPreview.Click -= btnPreviewClick;
            btnPreview = null;
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

        private void gitUiCommands_PreCommit(object sender, GitUIBaseEventArgs e)
        {
            if (!enabledSettings.ValueOrDefault(Settings))
                return;
            if (jira?.Issues == null)
                return;

            currentMessages = GetMessageToCommit(jira, query, stringTemplate);
            foreach (var message in currentMessages)
            {
                e.GitUICommands.AddCommitTemplate(message.Title, () => message.Text);
            }
        }

        private void gitUiCommands_PostRepositoryChanged(object sender, GitUIBaseEventArgs e)
        {
            if (!enabledSettings.ValueOrDefault(Settings))
                return;
            if (currentMessages == null)
                return;
            foreach (var message in currentMessages)
            {
                e.GitUICommands.RemoveCommitTemplate(message.Title);
            }
            currentMessages = null;
        }

        private static JiraTaskDTO[] GetMessageToCommit(Jira jira, string query, string stringTemplate)
        {
            try
            {
                return jira.Issues.GetIssuesFromJqlAsync(query).Result
                    .Select(issue => new { title = issue.Key + " " + issue.Summary, message = StringTemplate.Format(stringTemplate, issue) })
                    .Select(i => new JiraTaskDTO(i.title, i.message))
                    .ToArray();
            }
            catch (Exception ex)
            {
                return new[] { new JiraTaskDTO($"{description} error", ex.ToString()) };
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
