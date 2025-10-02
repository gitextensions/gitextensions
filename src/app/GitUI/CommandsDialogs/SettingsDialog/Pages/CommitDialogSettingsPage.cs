using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
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
            chkShowErrorsWhenStagingFiles.Checked = AppSettings.ShowErrorsWhenStagingFiles;
            chkEnsureCommitMessageSecondLineEmpty.Checked = AppSettings.EnsureCommitMessageSecondLineEmpty;
            chkWriteCommitMessageInCommitWindow.Checked = AppSettings.UseFormCommitMessage;
            _NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Value = AppSettings.CommitDialogNumberOfPreviousMessages;
            chkShowCommitAndPush.Checked = AppSettings.ShowCommitAndPush;
            chkShowResetWorkTreeChanges.Checked = AppSettings.ShowResetWorkTreeChanges;
            chkShowResetAllChanges.Checked = AppSettings.ShowResetAllChanges;
            chkAutocomplete.Checked = AppSettings.ProvideAutocompletion;
            cbRememberAmendCommitState.Checked = AppSettings.RememberAmendCommitState;
            chkReadFromGitAfterAppStart.Checked = AppSettings.ReadFromGitAfterAppStart;
            tbTemplateComment.Text = AppSettings.Comment;

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            AppSettings.ShowErrorsWhenStagingFiles = chkShowErrorsWhenStagingFiles.Checked;
            AppSettings.EnsureCommitMessageSecondLineEmpty = chkEnsureCommitMessageSecondLineEmpty.Checked;
            AppSettings.UseFormCommitMessage = chkWriteCommitMessageInCommitWindow.Checked;
            AppSettings.CommitDialogNumberOfPreviousMessages = (int)_NO_TRANSLATE_CommitDialogNumberOfPreviousMessages.Value;
            AppSettings.ShowCommitAndPush = chkShowCommitAndPush.Checked;
            AppSettings.ShowResetWorkTreeChanges = chkShowResetWorkTreeChanges.Checked;
            AppSettings.ShowResetAllChanges = chkShowResetAllChanges.Checked;
            AppSettings.ProvideAutocompletion = chkAutocomplete.Checked;
            AppSettings.RememberAmendCommitState = cbRememberAmendCommitState.Checked;
            AppSettings.ReadFromGitAfterAppStart = chkReadFromGitAfterAppStart.Checked;

            // Validate template comment is not empty
            if (string.IsNullOrWhiteSpace(tbTemplateComment.Text))
            {
                MessageBox.Show(this,
                    "Comment/line prefix cannot be empty. Please enter a value.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                // Focus the textbox and select all text for easy editing
                tbTemplateComment.Focus();
                tbTemplateComment.SelectAll();
                return;
            }

            AppSettings.Comment = tbTemplateComment.Text;
            base.PageToSettings();
        }
    }
}
