using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class CommitDialogSettingsPage : SettingsPageWithHeader
    {
        public CommitDialogSettingsPage()
        {
            InitializeComponent();
            Text = "Commit dialog";
            Translate();
        }

        protected override void SettingsToPage()
        {
            chkShowErrorsWhenStagingFiles.Checked = AppSettings.ShowErrorsWhenStagingFiles;
            chkAddNewlineToCommitMessageWhenMissing.Checked = AppSettings.AddNewlineToCommitMessageWhenMissing;
            chkWriteCommitMessageInCommitWindow.Checked = AppSettings.UseFormCommitMessage;
            _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Value = AppSettings.CommitDialogNumberOfPreviousMessages;
            chkShowCommitAndPush.Checked = AppSettings.ShowCommitAndPush;
            chkShowResetUnstagedChanges.Checked = AppSettings.ShowResetUnstagedChanges;
            chkShowResetAllChanges.Checked = AppSettings.ShowResetAllChanges;
            chkAutocomplete.Checked = AppSettings.ProvideAutocompletion;
            cbRememberAmendCommitState.Checked = AppSettings.RememberAmendCommitState;
        }

        protected override void PageToSettings()
        {
            AppSettings.ShowErrorsWhenStagingFiles = chkShowErrorsWhenStagingFiles.Checked;
            AppSettings.AddNewlineToCommitMessageWhenMissing = chkAddNewlineToCommitMessageWhenMissing.Checked;
            AppSettings.UseFormCommitMessage = chkWriteCommitMessageInCommitWindow.Checked;
            AppSettings.CommitDialogNumberOfPreviousMessages = (int)_NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Value;
            AppSettings.ShowCommitAndPush = chkShowCommitAndPush.Checked;
            AppSettings.ShowResetUnstagedChanges = chkShowResetUnstagedChanges.Checked;
            AppSettings.ShowResetAllChanges = chkShowResetAllChanges.Checked;
            AppSettings.ProvideAutocompletion = chkAutocomplete.Checked;
            AppSettings.RememberAmendCommitState = cbRememberAmendCommitState.Checked;
        }
    }
}
