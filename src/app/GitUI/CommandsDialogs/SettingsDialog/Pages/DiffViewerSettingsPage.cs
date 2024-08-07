using GitCommands;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class DiffViewerSettingsPage : SettingsPageWithHeader
    {
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
            chkUseGEThemeGitColoring.Checked = AppSettings.ReverseGitColoring.Value;
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
            AppSettings.ReverseGitColoring.Value = chkUseGEThemeGitColoring.Checked;

            base.PageToSettings();
        }

        private void chkUseGitColoring_CheckedChanged(object sender, EventArgs e)
            => chkUseGEThemeGitColoring.Enabled = chkUseGitColoring.Checked;

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(DiffViewerSettingsPage));
        }
    }
}
