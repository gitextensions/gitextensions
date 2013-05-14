using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class GitExtensionsSettingsPage : SettingsPageBase
    {
        public GitExtensionsSettingsPage(CommonLogic aCommonLogic)
            : base(aCommonLogic)
        {
            InitializeComponent();
            Text = "Git Extensions";
            Translate();
        }

        protected override void SettingsToPage()
        {
            chkCheckForUncommittedChangesInCheckoutBranch.Checked = AppSettings.CheckForUncommittedChangesInCheckoutBranch;
            chkStartWithRecentWorkingDir.Checked = AppSettings.StartWithRecentWorkingDir;
            chkPlaySpecialStartupSound.Checked = AppSettings.PlaySpecialStartupSound;
            chkWriteCommitMessageInCommitWindow.Checked = AppSettings.UseFormCommitMessage;
            chkUsePatienceDiffAlgorithm.Checked = AppSettings.UsePatienceDiffAlgorithm;
            RevisionGridQuickSearchTimeout.Value = AppSettings.RevisionGridQuickSearchTimeout;
            chkFollowRenamesInFileHistory.Checked = AppSettings.FollowRenamesInFileHistory;
            chkShowErrorsWhenStagingFiles.Checked = AppSettings.ShowErrorsWhenStagingFiles;
            chkStashUntrackedFiles.Checked = AppSettings.IncludeUntrackedFilesInAutoStash;
            chkShowCurrentChangesInRevisionGraph.Checked = AppSettings.RevisionGraphShowWorkingDirChanges;
            chkShowStashCountInBrowseWindow.Checked = AppSettings.ShowStashCount;
            chkShowGitStatusInToolbar.Checked = AppSettings.ShowGitStatusInBrowseToolbar;
            SmtpServer.Text = AppSettings.SmtpServer;
            SmtpServerPort.Text = AppSettings.SmtpPort.ToString();
            chkUseSSL.Checked = AppSettings.SmtpUseSsl;
            _NO_TRANSLATE_MaxCommits.Value = AppSettings.MaxRevisionGraphCommits;
            chkCloseProcessDialog.Checked = AppSettings.CloseProcessDialog;
            chkShowGitCommandLine.Checked = AppSettings.ShowGitCommandLine;
            chkUseFastChecks.Checked = AppSettings.UseFastChecks;
        }

        protected override void PageToSettings()
        {
            AppSettings.CheckForUncommittedChangesInCheckoutBranch = chkCheckForUncommittedChangesInCheckoutBranch.Checked;
            AppSettings.StartWithRecentWorkingDir = chkStartWithRecentWorkingDir.Checked;
            AppSettings.PlaySpecialStartupSound = chkPlaySpecialStartupSound.Checked;
            AppSettings.UseFormCommitMessage = chkWriteCommitMessageInCommitWindow.Checked;
            AppSettings.UsePatienceDiffAlgorithm = chkUsePatienceDiffAlgorithm.Checked;
            AppSettings.ShowErrorsWhenStagingFiles = chkShowErrorsWhenStagingFiles.Checked;
            AppSettings.IncludeUntrackedFilesInAutoStash = chkStashUntrackedFiles.Checked;
            AppSettings.FollowRenamesInFileHistory = chkFollowRenamesInFileHistory.Checked;
            AppSettings.ShowGitStatusInBrowseToolbar = chkShowGitStatusInToolbar.Checked;
            AppSettings.SmtpServer = SmtpServer.Text;
            int port;
            if (int.TryParse(SmtpServerPort.Text, out port))
                AppSettings.SmtpPort = port;
            AppSettings.SmtpUseSsl = chkUseSSL.Checked;
            AppSettings.CloseProcessDialog = chkCloseProcessDialog.Checked;
            AppSettings.ShowGitCommandLine = chkShowGitCommandLine.Checked;
            AppSettings.UseFastChecks = chkUseFastChecks.Checked;
            AppSettings.MaxRevisionGraphCommits = (int)_NO_TRANSLATE_MaxCommits.Value;
            AppSettings.RevisionGridQuickSearchTimeout = (int)RevisionGridQuickSearchTimeout.Value;
            AppSettings.RevisionGraphShowWorkingDirChanges = chkShowCurrentChangesInRevisionGraph.Checked;
            AppSettings.ShowStashCount = chkShowStashCountInBrowseWindow.Checked;
        }

        private void chkUseSSL_CheckedChanged(object sender, System.EventArgs e)
        {
            if (!chkUseSSL.Checked)
            {
                if (SmtpServerPort.Text == "587")
                    SmtpServerPort.Text = "465";
            }
            else
            {
                if (SmtpServerPort.Text == "465")
                    SmtpServerPort.Text = "587";
            }
        }
    }
}
