using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Atlassian.Jira;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using JiraCommitPlugin;
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
        private FormCommit commitForm;

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
                if (jira == null || jira.GetAccessToken() == null)
                    return;
                filter = jira.GetFilters().FirstOrDefault(f => f.Name.Equals(filterName[Settings]));
            }
            catch (Exception)
            {
                return;
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
            commitForm = e.OwnerForm as FormCommit;
            if (commitForm == null)
                return;
            var button = CreateAddInfoBitton();
            commitForm.AddInfoButton(button);
        }

        private ToolStripButton CreateAddInfoBitton()
        {
            var button = new ToolStripButton
            {
                ImageAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(1, 3, 3, 3),
                Name = "AddJiraInfo",
                Size = new Size(171, 26),
                Text = description
            };
            button.Click -= AddInfo;
            button.Click += AddInfo;
            return button;
        }

        private void AddInfo(object sender, EventArgs e)
        {
            if (commitForm == null)
                return;
            var message = GetMessageToCommit();
            commitForm.AppendToMessage(message);
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
