using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class FormBrowseRepoSettingsPage : SettingsPageWithHeader
    {
        public FormBrowseRepoSettingsPage()
        {
            InitializeComponent();
            Text = "Browse repository window";
            InitializeComplete();
        }

        protected override void Init(ISettingsPageHost pageHost)
        {
            base.Init(pageHost);

            // Bind settings with controls
            AddSettingBinding(AppSettings.ShowConEmuTab, chkChowConsoleTab);
            AddSettingBinding(AppSettings.ConEmuStyle, _NO_TRANSLATE_cboStyle);
            AddSettingBinding(AppSettings.ConEmuTerminal, cboTerminal);
            AddSettingBinding(AppSettings.ConEmuFontSize, cboFontSize);
            AddSettingBinding(AppSettings.ShowGpgInformation, chkShowGpgInformation);
        }

        private void chkChowConsoleTab_CheckedChanged(object sender, System.EventArgs e)
        {
            groupBoxConsoleSettings.Enabled = chkChowConsoleTab.Checked;
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(FormBrowseRepoSettingsPage));
        }
    }
}
