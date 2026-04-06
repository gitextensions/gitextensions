using GitCommands;
using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class AdvancedSettingsPage : SettingsPageWithHeader
{
    public AdvancedSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();

        var autoNormaliseSymbols = new[]
        {
            new { Key = "_", Value = "_" },
            new { Key = "-", Value = "-" },
            new { Key = "(none)", Value = "" },
        };
        cboAutoNormaliseSymbol.DisplayMember = "Key";
        cboAutoNormaliseSymbol.ValueMember = "Value";
        cboAutoNormaliseSymbol.DataSource = autoNormaliseSymbols;
        cboAutoNormaliseSymbol.SelectedIndex = 0;
    }

    protected override void SettingsToPage()
    {
        chkAlwaysShowCheckoutDlg.Checked = AppSettings.AlwaysShowCheckoutBranchDlg.Value;
        chkUseLocalChangesAction.Checked = AppSettings.UseDefaultCheckoutBranchAction.Value;
        chkDontSHowHelpImages.Checked = AppSettings.DontShowHelpImages.Value;
        chkAlwaysShowAdvOpt.Checked = AppSettings.AlwaysShowAdvOpt.Value;
        chkCheckForUpdates.Checked = AppSettings.CheckForUpdates.Value;
        chkCheckForRCVersions.Checked = AppSettings.CheckForReleaseCandidates.Value;
        chkConsoleEmulator.Checked = AppSettings.UseConsoleEmulatorForCommands.Value;
        chkAutoNormaliseBranchName.Checked = AppSettings.AutoNormaliseBranchName.Value;
        cboAutoNormaliseSymbol.Enabled = chkAutoNormaliseBranchName.Checked;
        cboAutoNormaliseSymbol.SelectedValue = AppSettings.AutoNormaliseSymbol;
        chkCommitAndPushForcedWhenAmend.Checked = AppSettings.CommitAndPushForcedWhenAmend.Value;

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.AlwaysShowCheckoutBranchDlg.Value = chkAlwaysShowCheckoutDlg.Checked;
        AppSettings.UseDefaultCheckoutBranchAction.Value = chkUseLocalChangesAction.Checked;
        AppSettings.DontShowHelpImages.Value = chkDontSHowHelpImages.Checked;
        AppSettings.AlwaysShowAdvOpt.Value = chkAlwaysShowAdvOpt.Checked;
        AppSettings.CheckForUpdates.Value = chkCheckForUpdates.Checked;
        AppSettings.CheckForReleaseCandidates.Value = chkCheckForRCVersions.Checked;
        AppSettings.UseConsoleEmulatorForCommands.Value = chkConsoleEmulator.Checked;
        AppSettings.AutoNormaliseBranchName.Value = chkAutoNormaliseBranchName.Checked;
        AppSettings.AutoNormaliseSymbol = (string)cboAutoNormaliseSymbol.SelectedValue;
        AppSettings.CommitAndPushForcedWhenAmend.Value = chkCommitAndPushForcedWhenAmend.Checked;

        base.PageToSettings();
    }

    public static SettingsPageReference GetPageReference()
    {
        return new SettingsPageReferenceByType(typeof(AdvancedSettingsPage));
    }

    private void chkAutoNormaliseBranchName_CheckedChanged(object sender, EventArgs e)
    {
        cboAutoNormaliseSymbol.Enabled = chkAutoNormaliseBranchName.Checked;
    }
}
