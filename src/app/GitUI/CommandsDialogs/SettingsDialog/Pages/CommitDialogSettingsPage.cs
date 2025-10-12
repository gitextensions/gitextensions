using GitCommands;
using GitUI.GitComments;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class CommitDialogSettingsPage : SettingsPageWithHeader
    {
        private int _selectedCommentStrategyId;

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

            _selectedCommentStrategyId = AppSettings.CommentStrategyId;

            // Select the item matching _selectedCommentStrategyId
            SelectStrategyById(_selectedCommentStrategyId);

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

            AppSettings.CommentStrategyId = _selectedCommentStrategyId;

            base.PageToSettings();
        }

        private void SelectStrategyById(int selectedCommentStrategyId)
        {
            var strategies = CommentStrategyFactory.GetAll();
            ICommentStrategy? selectedStrategy = strategies
                .FirstOrDefault(s => s.Id == selectedCommentStrategyId);

            if (selectedStrategy is not null)
            {
                cbCommentStrategy.SelectedItem = selectedStrategy;
            }
        }

        private void CommitDialogSettingsPage_Load(object sender, EventArgs e)
        {
            var strategies = CommentStrategyFactory.GetAll();
            cbCommentStrategy.DataSource = strategies;
            cbCommentStrategy.DisplayMember = nameof(ICommentStrategy.Name);
        }

        private void cbCommentStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
            ICommentStrategy? selectedItem = cbCommentStrategy.SelectedItem as ICommentStrategy;
            if (selectedItem != null)
            {
                _selectedCommentStrategyId = selectedItem.Id;
                lblCommentStrategyDescription.Text = selectedItem.Description;
            }
        }
    }
}
