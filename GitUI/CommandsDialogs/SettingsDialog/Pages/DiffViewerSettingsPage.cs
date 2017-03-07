using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class DiffViewerSettingsPage : SettingsPageWithHeader
    {
        public DiffViewerSettingsPage()
        {
            InitializeComponent();
            Text = "Diff Viewer";
            Translate();
        }

        protected override void SettingsToPage()
        {
            chkRememberIgnoreWhiteSpacePreference.Checked = AppSettings.RememberIgnoreWhiteSpacePreference;
            chkOmitUninterestingDiff.Checked = AppSettings.OmitUninterestingDiff;
            chkRememberShowEntireFilePreference.Checked = AppSettings.RememberShowEntireFilePreference;
            chkRememberShowNonPrintingCharsPreference.Checked = AppSettings.RememberShowNonPrintingCharsPreference;
            chkRememberNumberOfContextLines.Checked = AppSettings.RememberNumberOfContextLines;
        }

        protected override void PageToSettings()
        {
            AppSettings.RememberIgnoreWhiteSpacePreference = chkRememberIgnoreWhiteSpacePreference.Checked;
            AppSettings.OmitUninterestingDiff = chkOmitUninterestingDiff.Checked;
            AppSettings.RememberShowEntireFilePreference = chkRememberShowEntireFilePreference.Checked;
            AppSettings.RememberShowNonPrintingCharsPreference = chkRememberShowNonPrintingCharsPreference.Checked;
            AppSettings.RememberNumberOfContextLines = chkRememberNumberOfContextLines.Checked;
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(DiffViewerSettingsPage));
        }
    }
}
