using GitCommands;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

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
        chkRememberIgnoreWhiteSpacePreference.Checked = AppSettings.RememberIgnoreWhiteSpacePreference.Value;
        chkOmitUninterestingDiff.Checked = AppSettings.OmitUninterestingDiff.Value;
        chkRememberShowEntireFilePreference.Checked = AppSettings.RememberShowEntireFilePreference.Value;
        chkRememberDiffAppearancePreference.Checked = AppSettings.RememberDiffDisplayAppearance.Value;
        chkRememberShowNonPrintingCharsPreference.Checked = AppSettings.RememberShowNonPrintingCharsPreference.Value;
        chkRememberNumberOfContextLines.Checked = AppSettings.RememberNumberOfContextLines.Value;
        chkRememberShowSyntaxHighlightingInDiff.Checked = AppSettings.RememberShowSyntaxHighlightingInDiff.Value;
        chkOpenSubmoduleDiffInSeparateWindow.Checked = AppSettings.OpenSubmoduleDiffInSeparateWindow.Value;
        chkContScrollToNextFileOnlyWithAlt.Checked = AppSettings.AutomaticContinuousScroll.Value;
        chkShowDiffForAllParents.Checked = AppSettings.ShowDiffForAllParents.Value;
        chkShowAllCustomDiffTools.Checked = AppSettings.ShowAvailableDiffTools.Value;
        VerticalRulerPosition.Value = AppSettings.DiffVerticalRulerPosition.Value;
        chkUseGitColoring.Checked = AppSettings.UseGitColoring.Value;
        chkUseGEThemeGitColoring.Checked = AppSettings.ReverseGitColoring.Value;
        chkUseGEThemeGitColoring.Enabled = chkUseGitColoring.Checked;

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.RememberIgnoreWhiteSpacePreference.Value = chkRememberIgnoreWhiteSpacePreference.Checked;
        AppSettings.OmitUninterestingDiff.Value = chkOmitUninterestingDiff.Checked;
        AppSettings.RememberShowEntireFilePreference.Value = chkRememberShowEntireFilePreference.Checked;
        AppSettings.RememberDiffDisplayAppearance.Value = chkRememberDiffAppearancePreference.Checked;
        AppSettings.RememberShowNonPrintingCharsPreference.Value = chkRememberShowNonPrintingCharsPreference.Checked;
        AppSettings.RememberNumberOfContextLines.Value = chkRememberNumberOfContextLines.Checked;
        AppSettings.RememberShowSyntaxHighlightingInDiff.Value = chkRememberShowSyntaxHighlightingInDiff.Checked;
        AppSettings.OpenSubmoduleDiffInSeparateWindow.Value = chkOpenSubmoduleDiffInSeparateWindow.Checked;
        AppSettings.AutomaticContinuousScroll.Value = chkContScrollToNextFileOnlyWithAlt.Checked;
        AppSettings.ShowDiffForAllParents.Value = chkShowDiffForAllParents.Checked;
        AppSettings.ShowAvailableDiffTools.Value = chkShowAllCustomDiffTools.Checked;
        AppSettings.DiffVerticalRulerPosition.Value = (int)VerticalRulerPosition.Value;
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
