using GitCommands;
using GitCommands.Properties;

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
            chkCheckForUncommittedChangesInCheckoutBranch.Checked = Settings.Default.CheckForUncommittedChangesInCheckoutBranch;
            chkStartWithRecentWorkingDir.Checked = Settings.Default.StartWithRecentWorkingDir;
            chkPlaySpecialStartupSound.Checked = Settings.Default.PlaySpecialStartupSound;
            chkWriteCommitMessageInCommitWindow.Checked = Settings.Default.UseFormCommitMessage;
            chkUsePatienceDiffAlgorithm.Checked = Settings.Default.UsePatienceDiffAlgorithm;
            RevisionGridQuickSearchTimeout.Value = Settings.Default.RevisionGridQuickSearchTimeout;
            chkFollowRenamesInFileHistory.Checked = Settings.Default.FollowRenamesInFileHistory;
            chkShowErrorsWhenStagingFiles.Checked = Settings.Default.ShowErrorsWhenStagingFiles;
            chkStashUntrackedFiles.Checked = Settings.Default.IncludeUntrackedFilesInAutoStash;
            chkShowCurrentChangesInRevisionGraph.Checked = Settings.Default.RevisionGraphShowWorkingDirChanges;
            chkShowStashCountInBrowseWindow.Checked = Settings.Default.ShowStashCount;
            chkShowGitStatusInToolbar.Checked = Settings.Default.ShowGitStatusInBrowseToolbar;
            SmtpServer.Text = Settings.Default.Smtp;
            _NO_TRANSLATE_MaxCommits.Value = Settings.Default.MaxRevisionGraphCommits;
            chkCloseProcessDialog.Checked = Settings.Default.CloseProcessDialog;
            chkShowGitCommandLine.Checked = Settings.Default.ShowGitCommandLine;
            chkUseFastChecks.Checked = Settings.Default.UseFastChecks;
        }

        public override void SaveSettings()
        {
            Settings.Default.CheckForUncommittedChangesInCheckoutBranch = chkCheckForUncommittedChangesInCheckoutBranch.Checked;
            Settings.Default.StartWithRecentWorkingDir = chkStartWithRecentWorkingDir.Checked;
            Settings.Default.PlaySpecialStartupSound = chkPlaySpecialStartupSound.Checked;
            Settings.Default.UseFormCommitMessage = chkWriteCommitMessageInCommitWindow.Checked;
            Settings.Default.UsePatienceDiffAlgorithm = chkUsePatienceDiffAlgorithm.Checked;
            Settings.Default.ShowErrorsWhenStagingFiles = chkShowErrorsWhenStagingFiles.Checked;
            Settings.Default.IncludeUntrackedFilesInAutoStash = chkStashUntrackedFiles.Checked;
            Settings.Default.FollowRenamesInFileHistory = chkFollowRenamesInFileHistory.Checked;
            Settings.Default.ShowGitStatusInBrowseToolbar = chkShowGitStatusInToolbar.Checked;
            Settings.Default.Smtp = SmtpServer.Text;
            Settings.Default.CloseProcessDialog = chkCloseProcessDialog.Checked;
            Settings.Default.ShowGitCommandLine = chkShowGitCommandLine.Checked;
            Settings.Default.UseFastChecks = chkUseFastChecks.Checked;
            Settings.Default.MaxRevisionGraphCommits = (int)_NO_TRANSLATE_MaxCommits.Value;
            Settings.Default.RevisionGridQuickSearchTimeout = (int)RevisionGridQuickSearchTimeout.Value;
            Settings.Default.RevisionGraphShowWorkingDirChanges = chkShowCurrentChangesInRevisionGraph.Checked;
            Settings.Default.ShowStashCount = chkShowStashCountInBrowseWindow.Checked;
        }
    }
}
