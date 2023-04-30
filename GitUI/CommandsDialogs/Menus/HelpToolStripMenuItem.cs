using GitCommands;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs.BrowseDialog;

namespace GitUI.CommandsDialogs.Menus
{
    internal partial class HelpToolStripMenuItem : ToolStripMenuItemEx
    {
        public HelpToolStripMenuItem()
        {
            InitializeComponent();

            translateToolStripMenuItem.AdaptImageLightness();
        }

        private void this_DropDownOpening(object sender, EventArgs e)
        {
            tsmiTelemetryEnabled.Checked = AppSettings.TelemetryEnabled ?? false;
        }

        private void AboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            using FormAbout frm = new();
            frm.ShowDialog(OwnerForm);
        }

        private void ChangelogToolStripMenuItemClick(object sender, EventArgs e)
        {
            using FormChangeLog frm = new();
            frm.ShowDialog(OwnerForm);
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormUpdates updateForm = new(AppSettings.AppVersion);
            updateForm.SearchForUpdatesAndShow(Owner, true);
        }

        private void DonateToolStripMenuItemClick(object sender, EventArgs e)
        {
            using FormDonate frm = new();
            frm.ShowDialog(OwnerForm);
        }

        private void reportAnIssueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UserEnvironmentInformation.CopyInformation();
            OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/issues");
        }

        private void TranslateToolStripMenuItemClick(object sender, EventArgs e)
        {
            OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/wiki/Translations");
        }

        private void TsmiTelemetryEnabled_Click(object sender, EventArgs e)
        {
            UICommands.StartGeneralSettingsDialog(OwnerForm);
        }

        private void UserManualToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Point to the default documentation, will work also if the old doc version is removed
            OsShellUtil.OpenUrlInDefaultBrowser(AppSettings.DocumentationBaseUrl);
        }
    }
}
