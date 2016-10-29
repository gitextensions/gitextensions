using GitCommands.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class DetailedSettingsPage : RepoDistSettingsPage
    {
        public DetailedSettingsPage()
        {
            InitializeComponent();
            Text = "Detailed";
            Translate();
        }

        private DetailedGroup DetailedSettings
        {
            get
            {
                return CurrentSettings.Detailed;
            }
        }

        protected override void SettingsToPage()
        {
            chkChowConsoleTab.SetNullableChecked(DetailedSettings.ShowConEmuTab.Value);
            chkRemotesFromServer.SetNullableChecked(DetailedSettings.GetRemoteBranchesDirectlyFromRemote.Value);
        }

        protected override void PageToSettings()
        {
            DetailedSettings.ShowConEmuTab.Value = chkChowConsoleTab.GetNullableChecked();
            DetailedSettings.GetRemoteBranchesDirectlyFromRemote.Value = chkRemotesFromServer.GetNullableChecked();
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(DetailedSettingsPage));
        }
    }
}
