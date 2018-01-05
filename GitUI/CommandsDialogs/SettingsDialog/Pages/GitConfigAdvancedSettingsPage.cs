using System;
using System.IO;
using System.Windows.Forms;
using GitCommands.Config;
using GitCommands.Settings;
using GitCommands.Utils;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class GitConfigAdvancedSettingsPage : ConfigFileSettingsPage
    {
        public GitConfigAdvancedSettingsPage()
        {
            InitializeComponent();
            Text = "Advanced";
            Translate();
        }

        protected override void SettingsToPage()
        {
            checkBoxPullRebase.Checked = CurrentSettings.GetValue("pull.rebase").Equals("true");
            checkBoxFetchPrune.Checked = CurrentSettings.GetValue("fetch.prune").Equals("true");
            checkBoxRebaseAutostash.Checked = CurrentSettings.GetValue("rebase.autoStash").Equals("true");
        }

        protected override void PageToSettings()
        {
            CurrentSettings.SetValue("pull.rebase", checkBoxPullRebase.Checked ? "true" : "false");
            CurrentSettings.SetValue("fetch.prune", checkBoxFetchPrune.Checked ? "true" : "false");
            CurrentSettings.SetValue("rebase.autoStash", checkBoxRebaseAutostash.Checked ? "true" : "false");
        }
    }
}
