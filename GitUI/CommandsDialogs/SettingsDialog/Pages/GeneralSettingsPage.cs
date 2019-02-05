using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using GitCommands;
using GitCommands.UserRepositoryHistory;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class GeneralSettingsPage : SettingsPageWithHeader
    {
        public GeneralSettingsPage()
        {
            InitializeComponent();
            Text = "General";
            InitializeComplete();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || GitModuleForm.IsUnitTestActive)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var repositoryHistory = await RepositoryHistoryManager.Locals.LoadRecentHistoryAsync();

                await this.SwitchToMainThreadAsync();
                var historicPaths = repositoryHistory.Select(GetParentPath())
                                                     .Where(x => !string.IsNullOrEmpty(x))
                                                     .Distinct(StringComparer.CurrentCultureIgnoreCase)
                                                     .ToArray();
                cbDefaultCloneDestination.Items.AddRange(historicPaths);
            });
        }

        protected override void OnRuntimeLoad()
        {
            base.OnRuntimeLoad();

            // align 1st columns across all tables
            tlpnlBehaviour.AdjustWidthToSize(0, lblCommitsLimit, lblDefaultCloneDestination);
            tlpnlEmailSettings.AdjustWidthToSize(0, lblCommitsLimit, lblDefaultCloneDestination);
        }

        private void SetSubmoduleStatus()
        {
            chkShowSubmoduleStatusInBrowse.Enabled = chkShowGitStatusInToolbar.Checked || chkShowGitStatusForArtificialCommits.Checked;
            chkShowSubmoduleStatusInBrowse.Checked = chkShowSubmoduleStatusInBrowse.Enabled && chkShowSubmoduleStatusInBrowse.Checked;
        }

        protected override void SettingsToPage()
        {
            chkCheckForUncommittedChangesInCheckoutBranch.Checked = AppSettings.CheckForUncommittedChangesInCheckoutBranch;
            chkStartWithRecentWorkingDir.Checked = AppSettings.StartWithRecentWorkingDir;
            chkUsePatienceDiffAlgorithm.Checked = AppSettings.UsePatienceDiffAlgorithm;
            RevisionGridQuickSearchTimeout.Value = AppSettings.RevisionGridQuickSearchTimeout;
            chkFollowRenamesInFileHistory.Checked = AppSettings.FollowRenamesInFileHistory;
            chkStashUntrackedFiles.Checked = AppSettings.IncludeUntrackedFilesInAutoStash;
            chkShowCurrentChangesInRevisionGraph.Checked = AppSettings.RevisionGraphShowWorkingDirChanges;
            chkShowStashCountInBrowseWindow.Checked = AppSettings.ShowStashCount;
            chkShowAheadBehindDataInBrowseWindow.Checked = AppSettings.ShowAheadBehindData;
            chkShowAheadBehindDataInBrowseWindow.Enabled = GitVersion.Current.SupportAheadBehindData;
            chkShowGitStatusInToolbar.Checked = AppSettings.ShowGitStatusInBrowseToolbar;
            chkShowGitStatusForArtificialCommits.Checked = AppSettings.ShowGitStatusForArtificialCommits;
            chkShowSubmoduleStatusInBrowse.Checked = AppSettings.ShowSubmoduleStatus;
            SmtpServer.Text = AppSettings.SmtpServer;
            SmtpServerPort.Text = AppSettings.SmtpPort.ToString();
            chkUseSSL.Checked = AppSettings.SmtpUseSsl;
            _NO_TRANSLATE_MaxCommits.Value = AppSettings.MaxRevisionGraphCommits;
            chkCloseProcessDialog.Checked = AppSettings.CloseProcessDialog;
            chkShowGitCommandLine.Checked = AppSettings.ShowGitCommandLine;
            chkUseFastChecks.Checked = AppSettings.UseFastChecks;
            cbDefaultCloneDestination.Text = AppSettings.DefaultCloneDestinationPath;
            chkFollowRenamesInFileHistoryExact.Checked = AppSettings.FollowRenamesInFileHistoryExactOnly;
            SetSubmoduleStatus();
        }

        protected override void PageToSettings()
        {
            AppSettings.CheckForUncommittedChangesInCheckoutBranch = chkCheckForUncommittedChangesInCheckoutBranch.Checked;
            AppSettings.StartWithRecentWorkingDir = chkStartWithRecentWorkingDir.Checked;
            AppSettings.UsePatienceDiffAlgorithm = chkUsePatienceDiffAlgorithm.Checked;
            AppSettings.IncludeUntrackedFilesInAutoStash = chkStashUntrackedFiles.Checked;
            AppSettings.FollowRenamesInFileHistory = chkFollowRenamesInFileHistory.Checked;
            AppSettings.ShowGitStatusInBrowseToolbar = chkShowGitStatusInToolbar.Checked;
            AppSettings.ShowGitStatusForArtificialCommits = chkShowGitStatusForArtificialCommits.Checked;
            AppSettings.SmtpServer = SmtpServer.Text;
            if (int.TryParse(SmtpServerPort.Text, out var port))
            {
                AppSettings.SmtpPort = port;
            }

            AppSettings.SmtpUseSsl = chkUseSSL.Checked;
            AppSettings.CloseProcessDialog = chkCloseProcessDialog.Checked;
            AppSettings.ShowGitCommandLine = chkShowGitCommandLine.Checked;
            AppSettings.UseFastChecks = chkUseFastChecks.Checked;
            AppSettings.MaxRevisionGraphCommits = (int)_NO_TRANSLATE_MaxCommits.Value;
            AppSettings.RevisionGridQuickSearchTimeout = (int)RevisionGridQuickSearchTimeout.Value;
            AppSettings.RevisionGraphShowWorkingDirChanges = chkShowCurrentChangesInRevisionGraph.Checked;
            AppSettings.ShowStashCount = chkShowStashCountInBrowseWindow.Checked;
            AppSettings.ShowAheadBehindData = chkShowAheadBehindDataInBrowseWindow.Checked;
            AppSettings.ShowSubmoduleStatus = chkShowSubmoduleStatusInBrowse.Checked;

            AppSettings.DefaultCloneDestinationPath = cbDefaultCloneDestination.Text;
            AppSettings.FollowRenamesInFileHistoryExactOnly = chkFollowRenamesInFileHistoryExact.Checked;
        }

        private void chkUseSSL_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkUseSSL.Checked)
            {
                if (SmtpServerPort.Text == "587")
                {
                    SmtpServerPort.Text = "465";
                }
            }
            else
            {
                if (SmtpServerPort.Text == "465")
                {
                    SmtpServerPort.Text = "587";
                }
            }
        }

        private static Func<Repository, string> GetParentPath()
        {
            return x =>
            {
                if (x.Path.StartsWith(@"\\") || !Directory.Exists(x.Path))
                {
                    return string.Empty;
                }

                var dir = new DirectoryInfo(x.Path);
                if (dir.Parent == null)
                {
                    return x.Path;
                }

                return dir.Parent.FullName;
            };
        }

        private void DefaultCloneDestinationBrowseClick(object sender, EventArgs e)
        {
            var userSelectedPath = OsShellUtil.PickFolder(this, cbDefaultCloneDestination.Text);

            if (userSelectedPath != null)
            {
                cbDefaultCloneDestination.Text = userSelectedPath;
            }
        }

        private void ShowGitStatus_CheckedChanged(object sender, System.EventArgs e)
        {
            SetSubmoduleStatus();
        }
    }
}
