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

        protected override void Init(ISettingsPageHost aPageHost)
        {
            base.Init(aPageHost);
            BindSettingsWithControls();
        }

        private DetailedGroup DetailedSettings
        {
            get
            {
                return CurrentSettings.Detailed;
            }
        }

        private void BindSettingsWithControls()
        {
            AddSettingBinding(DetailedSettings.ShowConEmuTab, chkChowConsoleTab);
            cboStyle.SelectedItem = DetailedSettings.ConEmuStyle.Value;
            cboTerminal.SelectedItem = DetailedSettings.ConEmuTerminal.Value;
            cboFontSize.Text = DetailedSettings.ConEmuFontSize.Value;
            DetailedSettings.ConEmuStyle.Value = cboStyle.SelectedItem.ToString();
            DetailedSettings.ConEmuTerminal.Value = cboTerminal.SelectedItem.ToString();
            int newFontSize;
            if (int.TryParse(cboFontSize.Text, out newFontSize))
            {
                DetailedSettings.ConEmuFontSize.Value = newFontSize.ToString();
            }
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
