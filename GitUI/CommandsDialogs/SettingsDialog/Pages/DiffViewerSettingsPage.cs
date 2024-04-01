using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class DiffViewerSettingsPage : SettingsPageWithHeader
    {
        private const string _diffAppearanceURL = "https://git-extensions-documentation.readthedocs.io/settings.html#diff-appearance";
        private const string _useGitColoringURL = "https://git-extensions-documentation.readthedocs.io/settings.html#diff-coloring";

        public DiffViewerSettingsPage(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            InitializeComponent();
            InitializeComplete();

            chkShowDiffForAllParents.Text = TranslatedStrings.ShowDiffForAllParentsText;
            chkShowDiffForAllParents.ToolTipText = TranslatedStrings.ShowDiffForAllParentsTooltip;
            chkContScrollToNextFileOnlyWithAlt.Text = TranslatedStrings.ContScrollToNextFileOnlyWithAlt;
        }

        protected override void SettingsToPage()
        {
            chkRememberIgnoreWhiteSpacePreference.Checked = AppSettings.RememberIgnoreWhiteSpacePreference;
            chkOmitUninterestingDiff.Checked = AppSettings.OmitUninterestingDiff;
            chkRememberShowEntireFilePreference.Checked = AppSettings.RememberShowEntireFilePreference;
            chkRememberDiffAppearancePreference.Checked = AppSettings.RememberDiffDisplayAppearance.Value;
            chkRememberShowNonPrintingCharsPreference.Checked = AppSettings.RememberShowNonPrintingCharsPreference;
            chkRememberNumberOfContextLines.Checked = AppSettings.RememberNumberOfContextLines;
            chkRememberShowSyntaxHighlightingInDiff.Checked = AppSettings.RememberShowSyntaxHighlightingInDiff;
            chkOpenSubmoduleDiffInSeparateWindow.Checked = AppSettings.OpenSubmoduleDiffInSeparateWindow;
            chkContScrollToNextFileOnlyWithAlt.Checked = AppSettings.AutomaticContinuousScroll;
            chkShowDiffForAllParents.Checked = AppSettings.ShowDiffForAllParents;
            chkShowAllCustomDiffTools.Checked = AppSettings.ShowAvailableDiffTools;
            VerticalRulerPosition.Value = AppSettings.DiffVerticalRulerPosition;
            chkUseGitColoring.Checked = AppSettings.UseGitColoring.Value;
            chkUseGEThemeGitColoring.Checked = AppSettings.UseGEThemeGitColoring.Value;
            chkUseGEThemeGitColoring.Enabled = chkUseGitColoring.Checked;

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            AppSettings.RememberIgnoreWhiteSpacePreference = chkRememberIgnoreWhiteSpacePreference.Checked;
            AppSettings.OmitUninterestingDiff = chkOmitUninterestingDiff.Checked;
            AppSettings.RememberShowEntireFilePreference = chkRememberShowEntireFilePreference.Checked;
            AppSettings.RememberDiffDisplayAppearance.Value = chkRememberDiffAppearancePreference.Checked;
            AppSettings.RememberShowNonPrintingCharsPreference = chkRememberShowNonPrintingCharsPreference.Checked;
            AppSettings.RememberNumberOfContextLines = chkRememberNumberOfContextLines.Checked;
            AppSettings.RememberShowSyntaxHighlightingInDiff = chkRememberShowSyntaxHighlightingInDiff.Checked;
            AppSettings.OpenSubmoduleDiffInSeparateWindow = chkOpenSubmoduleDiffInSeparateWindow.Checked;
            AppSettings.AutomaticContinuousScroll = chkContScrollToNextFileOnlyWithAlt.Checked;
            AppSettings.ShowDiffForAllParents = chkShowDiffForAllParents.Checked;
            AppSettings.ShowAvailableDiffTools = chkShowAllCustomDiffTools.Checked;
            AppSettings.DiffVerticalRulerPosition = (int)VerticalRulerPosition.Value;
            AppSettings.UseGitColoring.Value = chkUseGitColoring.Checked;
            AppSettings.UseGEThemeGitColoring.Value = chkUseGEThemeGitColoring.Checked;

            base.PageToSettings();
        }

        private void chkUseGitColoring_CheckedChanged(object sender, EventArgs e)
            => chkUseGEThemeGitColoring.Enabled = chkUseGitColoring.Checked;

        private void chkUseGitColoring_InfoClicked(object sender, EventArgs e)
            => OsShellUtil.OpenUrlInDefaultBrowser(_useGitColoringURL);
        private void diffAppearanceHelp_Click(object sender, EventArgs e)
            => OsShellUtil.OpenUrlInDefaultBrowser(_diffAppearanceURL);

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(DiffViewerSettingsPage));
        }
    }
}
