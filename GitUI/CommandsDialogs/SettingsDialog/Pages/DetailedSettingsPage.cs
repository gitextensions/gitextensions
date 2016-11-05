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
            cboStyle.SelectedItem = DetailedSettings.ConEmuStyle.Value;
            cboTerminal.SelectedItem = DetailedSettings.ConEmuTerminal.Value;
            chkRemotesFromServer.SetNullableChecked(DetailedSettings.GetRemoteBranchesDirectlyFromRemote.Value);
        }

        protected override void PageToSettings()
        {
            DetailedSettings.ShowConEmuTab.Value = chkChowConsoleTab.GetNullableChecked();
            DetailedSettings.ConEmuStyle.Value = cboStyle.SelectedItem.ToString();
            DetailedSettings.ConEmuTerminal.Value = cboTerminal.SelectedItem.ToString();
            DetailedSettings.GetRemoteBranchesDirectlyFromRemote.Value = chkRemotesFromServer.GetNullableChecked();
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(DetailedSettingsPage));
        }

        private void chkChowConsoleTab_CheckedChanged(object sender, System.EventArgs e)
        {
            groupBoxConsoleSettings.Enabled = chkChowConsoleTab.Checked;
        }
    }
}
