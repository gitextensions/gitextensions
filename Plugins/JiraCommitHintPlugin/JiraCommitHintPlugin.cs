using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Atlassian.Jira;
using GitUIPluginInterfaces;
using ResourceManager;

namespace JiraCommitHintPlugin
{
    public class JiraCommitHintPlugin : GitPluginBase, IGitPluginForRepository
    {
        private const string description = "Jira Commit Hint";
        private Jira jira;
        private string query;
        private readonly StringSetting url = new StringSetting("Jira URL", @"https://jira.atlassian.com");
        private readonly StringSetting user = new StringSetting("Jira user", string.Empty);
        private readonly PasswordSetting password = new PasswordSetting("Jira password", string.Empty);
        private readonly StringSetting jdlQuery = new StringSetting("JDL Query", "assignee = currentUser() and resolution is EMPTY ORDER BY updatedDate DESC");
        private JiraTaskDTO[] currentMessages;

        public JiraCommitHintPlugin()
        {
            Description = description;
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            MessageBox.Show(string.Join(Environment.NewLine, GetMessageToCommit().Select(t => t.Text).ToArray()));
            return false;
        }

        public override IEnumerable<ISetting> GetSettings()
        {
            yield return url;
            yield return user;
            yield return password;
            yield return jdlQuery;
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            base.Register(gitUiCommands);
            gitUiCommands.PostSettings += gitUiCommands_PostSettings;
            gitUiCommands.PreCommit += gitUiCommands_PreCommit;
            gitUiCommands.PostRepositoryChanged += gitUiCommands_PostRepositoryChanged;
            UpdateJiraSettings();
        }

        private void UpdateJiraSettings()
        {
            jira = Jira.CreateRestClient(url[Settings], user[Settings], password[Settings]);
            query = jdlQuery[Settings];
        }

        private void gitUiCommands_PostSettings(object sender, GitUIPostActionEventArgs e)
        {
            UpdateJiraSettings();
        }

        public override void Unregister(IGitUICommands gitUiCommands)
        {
            base.Unregister(gitUiCommands);
            gitUiCommands.PreCommit -= gitUiCommands_PreCommit;
            gitUiCommands.PostSettings -= gitUiCommands_PostSettings;
            gitUiCommands.PostRepositoryChanged -= gitUiCommands_PostRepositoryChanged;
        }

        private void gitUiCommands_PreCommit(object sender, GitUIBaseEventArgs e)
        {
            currentMessages = GetMessageToCommit();
            foreach (var message in currentMessages)
            {
                e.GitUICommands.AddCommitTemplate(message.Title, () => message.Text);
            }
        }

        private void gitUiCommands_PostRepositoryChanged(object sender, GitUIBaseEventArgs e)
        {
            if (currentMessages == null)
                return;
            foreach (var message in currentMessages)
            {
                e.GitUICommands.RemoveCommitTemplate(message.Title);
            }
            currentMessages = null;
        }

        private JiraTaskDTO[] GetMessageToCommit()
        {
            try
            {
                return jira.Issues.GetIssuesFromJqlAsync(query).Result.Select(i => i.Key + " " + i.Summary).Select(i => new JiraTaskDTO(i, i)).ToArray();
            }
            catch (Exception ex)
            {
                return new[] { new JiraTaskDTO($"{description} error",  ex.Message )};
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
