using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class DiffViewerSettingsPage : SettingsPageWithHeader
    {
        public DiffViewerSettingsPage()
        {
            InitializeComponent();
            Text = "Diff viewer";
            InitializeComplete();

            chkShowDiffForAllParents.Text = TranslatedStrings.ShowDiffForAllParentsText;
            chkShowDiffForAllParents.ToolTipText = TranslatedStrings.ShowDiffForAllParentsTooltip;
            chkContScrollToNextFileOnlyWithAlt.Text = TranslatedStrings.ContScrollToNextFileOnlyWithAlt;

            _NO_TRANSLATE_cboEditor.Items.Add(string.Empty);
            _NO_TRANSLATE_cboEditor.Items.AddRange(EditorHelper.GetEditors());
        }

        protected override void SettingsToPage()
        {
            chkRememberIgnoreWhiteSpacePreference.Checked = AppSettings.RememberIgnoreWhiteSpacePreference;
            chkOmitUninterestingDiff.Checked = AppSettings.OmitUninterestingDiff;
            chkRememberShowEntireFilePreference.Checked = AppSettings.RememberShowEntireFilePreference;
            chkRememberShowNonPrintingCharsPreference.Checked = AppSettings.RememberShowNonPrintingCharsPreference;
            chkRememberNumberOfContextLines.Checked = AppSettings.RememberNumberOfContextLines;
            chkRememberShowSyntaxHighlightingInDiff.Checked = AppSettings.RememberShowSyntaxHighlightingInDiff;
            chkOpenSubmoduleDiffInSeparateWindow.Checked = AppSettings.OpenSubmoduleDiffInSeparateWindow;
            chkContScrollToNextFileOnlyWithAlt.Checked = AppSettings.AutomaticContinuousScroll;
            chkShowDiffForAllParents.Checked = AppSettings.ShowDiffForAllParents;
            chkShowAllCustomDiffTools.Checked = AppSettings.ShowAvailableDiffTools;
            VerticalRulerPosition.Value = AppSettings.DiffVerticalRulerPosition;
            _NO_TRANSLATE_cboEditor.Text = AppSettings.LocalFileEditor.Value;

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            AppSettings.RememberIgnoreWhiteSpacePreference = chkRememberIgnoreWhiteSpacePreference.Checked;
            AppSettings.OmitUninterestingDiff = chkOmitUninterestingDiff.Checked;
            AppSettings.RememberShowEntireFilePreference = chkRememberShowEntireFilePreference.Checked;
            AppSettings.RememberShowNonPrintingCharsPreference = chkRememberShowNonPrintingCharsPreference.Checked;
            AppSettings.RememberNumberOfContextLines = chkRememberNumberOfContextLines.Checked;
            AppSettings.RememberShowSyntaxHighlightingInDiff = chkRememberShowSyntaxHighlightingInDiff.Checked;
            AppSettings.OpenSubmoduleDiffInSeparateWindow = chkOpenSubmoduleDiffInSeparateWindow.Checked;
            AppSettings.AutomaticContinuousScroll = chkContScrollToNextFileOnlyWithAlt.Checked;
            AppSettings.ShowDiffForAllParents = chkShowDiffForAllParents.Checked;
            AppSettings.ShowAvailableDiffTools = chkShowAllCustomDiffTools.Checked;
            AppSettings.DiffVerticalRulerPosition = (int)VerticalRulerPosition.Value;
            AppSettings.LocalFileEditor.Value = _NO_TRANSLATE_cboEditor.Text;

            base.PageToSettings();
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(DiffViewerSettingsPage));
        }
    }
}
