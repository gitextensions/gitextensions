using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Atlassian.Jira;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.Settings;
using ResourceManager;

namespace JiraComminTipPlugin
{
    public class JiraComminPlugin : GitPluginBase, IGitPluginForRepository
    {
        private const string description = "Jira Commit Tip";
        private Jira jira;
        private JiraNamedEntity filter;
        private readonly StringSetting url = new StringSetting("Jira URL", @"https://jira.atlassian.com");
        private readonly StringSetting user = new StringSetting("Jira user", string.Empty);
        private readonly PasswordSetting password = new PasswordSetting("Jira password");
        private readonly StringSetting filterName = new StringSetting("Filter name", "[Filter should be in your favorites]");

        public JiraComminPlugin()
        {
            Description = description;
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            MessageBox.Show(GetMessageToCommit());
            return false;
        }

        public override IEnumerable<ISetting> GetSettings()
        {
            yield return url;
            yield return user;
            yield return password;
            yield return filterName;
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            base.Register(gitUiCommands);
            gitUiCommands.PostSettings += gitUiCommands_PostSettings;
            gitUiCommands.PreCommit += gitUiCommands_PreCommit;
            UpdateJiraSettings();
        }

        private void UpdateJiraSettings()
        {
            jira = new Jira(url[Settings], user[Settings], password[Settings]);
            try
            {
                if (jira.GetAccessToken() == null)
                    return;
                filter = jira.GetFilters().FirstOrDefault(f => f.Name.Equals(filterName[Settings]));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            gitUiCommands.PostSettings -= gitUiCommands_PostSettings;
        }

        private void gitUiCommands_PreCommit(object sender, GitUIBaseEventArgs e)
        {
           e.GitUICommands.AddFormCommitInfoButton(description, GetMessageToCommit);
        }

        private string GetMessageToCommit()
        {
            if (filter == null)
                return "<! Your Jira Commit Tip Plugin not configured !> ";
            try
            {
                return string.Join(Environment.NewLine,
                    jira.GetIssuesFromFilter(filter.Name).Select(i => i.Key + " " + i.Summary));
            }
            catch (Exception)
            {
                return "<! No connection to Jira !> ";
            }
        }
    }
}
