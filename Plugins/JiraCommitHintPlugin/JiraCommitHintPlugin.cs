using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Atlassian.Jira;
using GitUIPluginInterfaces;
using NString;
using ResourceManager;

namespace JiraCommitHintPlugin
{
    public class JiraCommitHintPlugin : GitPluginBase, IGitPluginForRepository
    {
        private static readonly string EnablePluginLabel = new TranslationString("Jira hint plugin enabled").Text;
        private static readonly string JiraUrlLabel = new TranslationString("Jira URL").Text;
        private static readonly string JiraUserLabel = new TranslationString("Jira user").Text;
        private static readonly string JiraPasswordLabel = new TranslationString("Jira password").Text;
        private static readonly string JiraQueryLabel = new TranslationString("JQL Query").Text;
        private static readonly string MessageTemplateLabel = new TranslationString("Message Template").Text;
        private static readonly string JiraFieldsLabel = new TranslationString("Jira fields").Text;
        private static readonly string QueryHelperLinkText = new TranslationString("Open the query helper inside Jira").Text;
        private static readonly string InvalidUrlMessage = new TranslationString("A valid url is required to launch the Jira query helper").Text;
        private static readonly string InvalidUrlCaption = new TranslationString("Invalid Jira url").Text;
        private static readonly string PreviewButtionText = new TranslationString("Preview").Text;
        private static readonly string QueryHelperOpenErrorText = new TranslationString("Unable to open Jira query helper").Text;
        private static readonly string EmptyQueryResultMessage = new TranslationString("[Empty Jira Query Result]").Text;
        private static readonly string EmptyQueryResultCaption = new TranslationString("First Task Preview").Text;

        private const string description = "Jira Commit Hint";
        private const string defaultFormat = "{Key} {Summary}";
        private Jira jira;
        private string query;
        private string stringTemplate = defaultFormat;
        private readonly BoolSetting enabledSettings = new BoolSetting("Jira hint plugin enabled", EnablePluginLabel, false);
        private readonly StringSetting urlSettings = new StringSetting("Jira URL", JiraUrlLabel, @"https://jira.atlassian.com");
        private readonly StringSetting userSettings = new StringSetting("Jira user", JiraUserLabel, string.Empty);
        private readonly PasswordSetting passwordSettings = new PasswordSetting("Jira password", JiraPasswordLabel, string.Empty);
        // For compatibility reason, the setting key is kept to "JDL Query" even if the label is, rightly, "JQL Query" (for "Jira Query Language")
        private readonly StringSetting jqlQuerySettings = new StringSetting("JDL Query", JiraQueryLabel, "assignee = currentUser() and resolution is EMPTY ORDER BY updatedDate DESC");
        private readonly StringSetting stringTemplateSetting = new StringSetting("Jira Message Template", MessageTemplateLabel, defaultFormat);
        private readonly StringSetting jiraFields = new StringSetting("Jira fields", JiraFieldsLabel, $"{{{string.Join("} {", typeof(Issue).GetProperties().Where(i => i.CanRead).Select(i => i.Name).OrderBy(i => i).ToArray())}}}");
        private readonly StringSetting jiraQueryHelpLink = new StringSetting("    ", "");
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
            GetMessageToCommit(jira, query, stringTemplate).ContinueWith(t =>
            {
                MessageBox.Show(string.Join(Environment.NewLine, t.Result.Select(jt => jt.Text).ToArray()));
            });
            return false;
        }

        public override IEnumerable<ISetting> GetSettings()
        {
            yield return enabledSettings;

            urlSettings.CustomControl = new TextBox();
            yield return urlSettings;

            userSettings.CustomControl = new TextBox();
            yield return userSettings;

            passwordSettings.CustomControl = new TextBox { UseSystemPasswordChar = true };
            yield return passwordSettings;

            jqlQuerySettings.CustomControl = new TextBox();
            yield return jqlQuerySettings;

            var queryHelperLink = new LinkLabel { Text = QueryHelperLinkText, Width = 300 };
            queryHelperLink.Click += QueryHelperLink_Click;
            var txtJiraQueryHelpLink = new TextBox { ReadOnly = true, BorderStyle = BorderStyle.None, Width = 300 };
            txtJiraQueryHelpLink.Controls.Add(queryHelperLink);
            jiraQueryHelpLink.CustomControl = txtJiraQueryHelpLink;
            yield return jiraQueryHelpLink;

            jiraFields.CustomControl = new TextBox { ReadOnly = true, Multiline = true, Height = 55, BorderStyle = BorderStyle.None };
            yield return jiraFields;

            var txtTemplate = new TextBox { Height = 75, Multiline = true, ScrollBars = ScrollBars.Horizontal };
            btnPreview = new Button { Text = PreviewButtionText, Top = 45, Anchor = AnchorStyles.Right };
            btnPreview.Click += btnPreviewClick;
            txtTemplate.Controls.Add(btnPreview);
            stringTemplateSetting.CustomControl = txtTemplate;
            yield return stringTemplateSetting;
        }

        private void QueryHelperLink_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(urlSettings.CustomControl.Text))
            {
                MessageBox.Show(null, InvalidUrlMessage, InvalidUrlCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                Process.Start(urlSettings.CustomControl.Text + "/secure/IssueNavigator.jspa?mode=show&createNew=true");
            }
            catch (Exception ex)
            {
                MessageBox.Show(null, ex.Message, QueryHelperOpenErrorText, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPreviewClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var localJira = Jira.CreateRestClient(urlSettings.CustomControl.Text, userSettings.CustomControl.Text, passwordSettings.CustomControl.Text);
                var localQuery = jqlQuerySettings.CustomControl.Text;
                var localStringTemplate = stringTemplateSetting.CustomControl.Text;
                GetMessageToCommit(localJira, localQuery, localStringTemplate).ContinueWith(t =>
                {
                    var preview = t.Result.FirstOrDefault();
                    MessageBox.Show(null, preview == null ? EmptyQueryResultMessage : preview.Text, EmptyQueryResultCaption);
                });
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
            query = jqlQuerySettings.ValueOrDefault(Settings);
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

            GetMessageToCommit(jira, query, stringTemplate).ContinueWith(t =>
            {
                currentMessages = t.Result;
                foreach (var message in currentMessages)
                {
                    e.GitUICommands.AddCommitTemplate(message.Title, () => message.Text);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
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

        private static async Task<JiraTaskDTO[]> GetMessageToCommit(Jira jira, string query, string stringTemplate)
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
