using GitCommands.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class DetailedSettingsPage : RepoDistSettingsPage
    {
        public DetailedSettingsPage()
        {
            InitializeComponent();
            Text = "Detailed";
            InitializeComplete();
        }

        protected override void Init(ISettingsPageHost pageHost)
        {
            base.Init(pageHost);
            BindSettingsWithControls();
        }

        private DetailedGroup DetailedSettings => CurrentSettings.Detailed;

        private void BindSettingsWithControls()
        {
            AddSettingBinding(DetailedSettings.GetRemoteBranchesDirectlyFromRemote, chkRemotesFromServer);
            AddSettingBinding(DetailedSettings.AddMergeLogMessages, addLogMessages);
            AddSettingBinding(DetailedSettings.MergeLogMessagesCount, nbMessages);
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(DetailedSettingsPage));
        }
    }
}
