using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class CommitDialogSettingsPage : SettingsPageWithHeader
{
    public CommitDialogSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();
    }

    protected override void SettingsToPage()
    {
        chkShowErrorsWhenStagingFiles.Checked = AppSettings.ShowErrorsWhenStagingFiles.Value;
        chkEnsureCommitMessageSecondLineEmpty.Checked = AppSettings.EnsureCommitMessageSecondLineEmpty.Value;
        chkWriteCommitMessageInCommitWindow.Checked = AppSettings.UseFormCommitMessage.Value;
        _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Value = AppSettings.CommitDialogNumberOfPreviousMessages.Value;
        chkShowCommitAndPush.Checked = AppSettings.ShowCommitAndPush.Value;
        chkShowResetWorkTreeChanges.Checked = AppSettings.ShowResetWorkTreeChanges.Value;
        chkShowResetAllChanges.Checked = AppSettings.ShowResetAllChanges.Value;
        chkAutocomplete.Checked = AppSettings.ProvideAutocompletion.Value;
        cbRememberAmendCommitState.Checked = AppSettings.RememberAmendCommitState.Value;

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.ShowErrorsWhenStagingFiles.Value = chkShowErrorsWhenStagingFiles.Checked;
        AppSettings.EnsureCommitMessageSecondLineEmpty.Value = chkEnsureCommitMessageSecondLineEmpty.Checked;
        AppSettings.UseFormCommitMessage.Value = chkWriteCommitMessageInCommitWindow.Checked;
        AppSettings.CommitDialogNumberOfPreviousMessages.Value = (int)_NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Value;
        AppSettings.ShowCommitAndPush.Value = chkShowCommitAndPush.Checked;
        AppSettings.ShowResetWorkTreeChanges.Value = chkShowResetWorkTreeChanges.Checked;
        AppSettings.ShowResetAllChanges.Value = chkShowResetAllChanges.Checked;
        AppSettings.ProvideAutocompletion.Value = chkAutocomplete.Checked;
        AppSettings.RememberAmendCommitState.Value = cbRememberAmendCommitState.Checked;

        base.PageToSettings();
    }
}
