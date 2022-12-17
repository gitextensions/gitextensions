using GitCommands;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class GitConfigAdvancedSettingsPage : ConfigFileSettingsPage
    {
        private record GitSettingUiMapping(string GitSettingKey, CheckBox MappedCheckbox);
        private readonly List<GitSettingUiMapping> _gitSettings;

        public GitConfigAdvancedSettingsPage()
        {
            InitializeComponent();
            Text = "Advanced";
            InitializeComplete();

            _gitSettings = new List<GitSettingUiMapping>
            {
                new("pull.rebase", checkBoxPullRebase),
                new("fetch.prune", checkBoxFetchPrune),
                new("rebase.autoStash", checkBoxRebaseAutostash),
                new("rebase.autosquash", checkBoxRebaseAutosquash),
                new("rebase.updateRefs", checkBoxUpdateRefs)
            };

            checkBoxUpdateRefs.Visible = GitVersion.Current.SupportUpdateRefs;

            Load += GitConfigAdvancedSettingsPage_Load;
        }

        private void GitConfigAdvancedSettingsPage_Load(object? sender, System.EventArgs e)
        {
            foreach (GitSettingUiMapping gitSetting in _gitSettings)
            {
                gitSetting.MappedCheckbox.Text += $" [{gitSetting.GitSettingKey}]";
            }
        }

        protected override void SettingsToPage()
        {
            Validates.NotNull(CurrentSettings);
            foreach (GitSettingUiMapping gitSetting in _gitSettings)
            {
                gitSetting.MappedCheckbox.Checked = CurrentSettings.GetValue(gitSetting.GitSettingKey) == "true";
            }

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            Validates.NotNull(CurrentSettings);
            foreach (GitSettingUiMapping gitSetting in _gitSettings)
            {
                CurrentSettings.SetValue(gitSetting.GitSettingKey, gitSetting.MappedCheckbox.Checked ? "true" : "false");
            }

            base.PageToSettings();
        }
    }
}
