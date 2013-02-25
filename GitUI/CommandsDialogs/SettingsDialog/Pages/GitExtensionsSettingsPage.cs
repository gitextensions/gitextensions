using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class GitExtensionsSettingsPage : SettingsPageBase
    {
        public GitExtensionsSettingsPage()
        {
            InitializeComponent();
            Text = "Git Extensions";
            Translate();
        }

        protected override void OnLoadSettings()
        {
            chkCheckForUncommittedChangesInCheckoutBranch.Checked = Settings.CheckForUncommittedChangesInCheckoutBranch;
            chkStartWithRecentWorkingDir.Checked = Settings.StartWithRecentWorkingDir;
            chkPlaySpecialStartupSound.Checked = Settings.PlaySpecialStartupSound;
            chkWriteCommitMessageInCommitWindow.Checked = Settings.UseFormCommitMessage;
            chkUsePatienceDiffAlgorithm.Checked = Settings.UsePatienceDiffAlgorithm;
            RevisionGridQuickSearchTimeout.Value = Settings.RevisionGridQuickSearchTimeout;
            chkFollowRenamesInFileHistory.Checked = Settings.FollowRenamesInFileHistory;
            chkShowErrorsWhenStagingFiles.Checked = Settings.ShowErrorsWhenStagingFiles;
            chkStashUntrackedFiles.Checked = Settings.IncludeUntrackedFilesInAutoStash;
            chkShowCurrentChangesInRevisionGraph.Checked = Settings.RevisionGraphShowWorkingDirChanges;
            chkShowStashCountInBrowseWindow.Checked = Settings.ShowStashCount;
            chkShowGitStatusInToolbar.Checked = Settings.ShowGitStatusInBrowseToolbar;
            SmtpServer.Text = Settings.Smtp;
            _NO_TRANSLATE_MaxCommits.Value = Settings.MaxRevisionGraphCommits;
            chkCloseProcessDialog.Checked = Settings.CloseProcessDialog;
            chkShowGitCommandLine.Checked = Settings.ShowGitCommandLine;
            chkUseFastChecks.Checked = Settings.UseFastChecks;
        }

        public override void SaveSettings()
        {
            Settings.CheckForUncommittedChangesInCheckoutBranch = chkCheckForUncommittedChangesInCheckoutBranch.Checked;
            Settings.StartWithRecentWorkingDir = chkStartWithRecentWorkingDir.Checked;
            Settings.PlaySpecialStartupSound = chkPlaySpecialStartupSound.Checked;
            Settings.UseFormCommitMessage = chkWriteCommitMessageInCommitWindow.Checked;
            Settings.UsePatienceDiffAlgorithm = chkUsePatienceDiffAlgorithm.Checked;
            Settings.ShowErrorsWhenStagingFiles = chkShowErrorsWhenStagingFiles.Checked;
            Settings.IncludeUntrackedFilesInAutoStash = chkStashUntrackedFiles.Checked;
            Settings.FollowRenamesInFileHistory = chkFollowRenamesInFileHistory.Checked;
            Settings.ShowGitStatusInBrowseToolbar = chkShowGitStatusInToolbar.Checked;
            Settings.Smtp = SmtpServer.Text;
            Settings.CloseProcessDialog = chkCloseProcessDialog.Checked;
            Settings.ShowGitCommandLine = chkShowGitCommandLine.Checked;
            Settings.UseFastChecks = chkUseFastChecks.Checked;
            Settings.MaxRevisionGraphCommits = (int)_NO_TRANSLATE_MaxCommits.Value;
            Settings.RevisionGridQuickSearchTimeout = (int)RevisionGridQuickSearchTimeout.Value;
            Settings.RevisionGraphShowWorkingDirChanges = chkShowCurrentChangesInRevisionGraph.Checked;
            Settings.ShowStashCount = chkShowStashCountInBrowseWindow.Checked;
        }
    }
}
