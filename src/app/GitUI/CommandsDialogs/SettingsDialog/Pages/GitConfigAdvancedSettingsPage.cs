using GitCommands;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class GitConfigAdvancedSettingsPage : ConfigFileSettingsPage
    {
        private record GitSettingUiMapping(string GitSettingKey, CheckBox MappedCheckbox);
        private readonly List<GitSettingUiMapping> _gitSettings;

        public GitConfigAdvancedSettingsPage(IServiceProvider serviceProvider)
           : base(serviceProvider)
        {
            InitializeComponent();
            InitializeComplete();

            _gitSettings =
            [
                new("pull.rebase", checkBoxPullRebase),
                new("fetch.prune", checkBoxFetchPrune),
                new("rebase.autoStash", checkBoxRebaseAutostash),
                new("rebase.autosquash", checkBoxRebaseAutosquash),
                new("rebase.updateRefs", checkBoxUpdateRefs)
            ];

            checkBoxUpdateRefs.Visible = GitVersion.Current.SupportUpdateRefs;

            Load += GitConfigAdvancedSettingsPage_Load;
        }

        private void GitConfigAdvancedSettingsPage_Load(object? sender, EventArgs e)
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
                gitSetting.MappedCheckbox.CheckState = CurrentSettings.GetValue(gitSetting.GitSettingKey) switch
                    {
                        "true" or "yes" or "on" or "1" => CheckState.Checked,
                        "false" or "no" or "off" or "0" or "" => CheckState.Unchecked,
                        _ => CheckState.Indeterminate
                    };
            }

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            Validates.NotNull(CurrentSettings);
            foreach (GitSettingUiMapping gitSetting in _gitSettings.Where(s => s.MappedCheckbox.CheckState != CheckState.Indeterminate))
            {
                CurrentSettings.SetValue(gitSetting.GitSettingKey, gitSetting.MappedCheckbox.Checked ? "true" : "false");
            }

            base.PageToSettings();
        }
    }
}
