using System;
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

            AssignDefaultLabels();
        }

        private void AssignDefaultLabels()
        {
            _NO_TRANSLATE_consolStyleDefaultLable.Text = "(" + DetailedSettings.ConEmuStyle.DefaultValue + ")";
            _NO_TRANSLATE_ShellToRunLabelDefault.Text = "(" + DetailedSettings.ConEmuTerminal.DefaultValue + ")";
            _NO_TRANSLATE_FontSizeDefault.Text = "(" + DetailedSettings.ConEmuFontSize.DefaultValue + ")";
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
            cboFontSize.Text = DetailedSettings.ConEmuFontSize.Value;
            chkRemotesFromServer.SetNullableChecked(DetailedSettings.GetRemoteBranchesDirectlyFromRemote.Value);
        }

        protected override void PageToSettings()
        {
            DetailedSettings.ShowConEmuTab.Value = chkChowConsoleTab.GetNullableChecked();
            DetailedSettings.ConEmuStyle.Value = cboStyle.SelectedItem == null ? null : cboStyle.SelectedItem.ToString();
            DetailedSettings.ConEmuTerminal.Value = cboTerminal.SelectedItem == null ? null : cboTerminal.SelectedItem.ToString();
            int newFontSize;
            if (int.TryParse(cboFontSize.Text, out newFontSize))
            {
                DetailedSettings.ConEmuFontSize.Value = newFontSize.ToString();
            }
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
