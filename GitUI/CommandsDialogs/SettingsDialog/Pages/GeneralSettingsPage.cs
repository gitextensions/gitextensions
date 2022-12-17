using System.ComponentModel;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class GeneralSettingsPage : SettingsPageWithHeader
    {
        private readonly TranslationString _openPullDialog = new("Open pull dialog");
        private readonly TranslationString _pullMerge = new("Pull - merge");
        private readonly TranslationString _pullRebase = new("Pull - rebase");
        private readonly TranslationString _fetch = new("Fetch");
        private readonly TranslationString _fetchAll = new("Fetch all");
        private readonly TranslationString _fetchAndPruneAll = new("Fetch and prune all");

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

            var pullActions = new[]
            {
                new { Key = _openPullDialog, Value = AppSettings.PullAction.None },
                new { Key = _pullMerge, Value = AppSettings.PullAction.Merge },
                new { Key = _pullRebase, Value = AppSettings.PullAction.Rebase },
                new { Key = _fetch, Value = AppSettings.PullAction.Fetch },
                new { Key = _fetchAll, Value = AppSettings.PullAction.FetchAll },
                new { Key = _fetchAndPruneAll, Value = AppSettings.PullAction.FetchPruneAll },
            };
            cboDefaultPullAction.DisplayMember = "Key";
            cboDefaultPullAction.ValueMember = "Value";
            cboDefaultPullAction.DataSource = pullActions;
            cboDefaultPullAction.SelectedIndex = 0;
        }

        public static SettingsPageReference GetPageReference()
        {
            return new SettingsPageReferenceByType(typeof(GeneralSettingsPage));
        }

        protected override void OnRuntimeLoad()
        {
            base.OnRuntimeLoad();

            // align 1st columns across all tables
            tlpnlBehaviour.AdjustWidthToSize(0, lblDefaultCloneDestination);
            tlpnlTelemetry.AdjustWidthToSize(0, lblDefaultCloneDestination);
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
            chkUseHistogramDiffAlgorithm.Checked = AppSettings.UseHistogramDiffAlgorithm;
            RevisionGridQuickSearchTimeout.Value = AppSettings.RevisionGridQuickSearchTimeout;
            chkFollowRenamesInFileHistory.Checked = AppSettings.FollowRenamesInFileHistory;
            chkStashUntrackedFiles.Checked = AppSettings.IncludeUntrackedFilesInAutoStash;
            chkUpdateModules.CheckState = ToCheckboxState(AppSettings.UpdateSubmodulesOnCheckout);
            chkShowStashCountInBrowseWindow.Checked = AppSettings.ShowStashCount;
            chkShowAheadBehindDataInBrowseWindow.Checked = AppSettings.ShowAheadBehindData;
            chkShowGitStatusInToolbar.Checked = AppSettings.ShowGitStatusInBrowseToolbar;
            chkShowGitStatusForArtificialCommits.Checked = AppSettings.ShowGitStatusForArtificialCommits;
            chkShowSubmoduleStatusInBrowse.Checked = AppSettings.ShowSubmoduleStatus;
            lblCommitsLimit.Checked = AppSettings.MaxRevisionGraphCommits != 0;
            _NO_TRANSLATE_MaxCommits.Value = AppSettings.MaxRevisionGraphCommits;
            _NO_TRANSLATE_MaxCommits.Enabled = AppSettings.MaxRevisionGraphCommits != 0;
            chkCloseProcessDialog.Checked = AppSettings.CloseProcessDialog;
            chkShowGitCommandLine.Checked = AppSettings.ShowGitCommandLine;
            cbDefaultCloneDestination.Text = AppSettings.DefaultCloneDestinationPath;
            cboDefaultPullAction.SelectedValue
                = AppSettings.DefaultPullAction != AppSettings.PullAction.Default ?
                  AppSettings.DefaultPullAction : AppSettings.PullAction.None;
            chkFollowRenamesInFileHistoryExact.Checked = AppSettings.FollowRenamesInFileHistoryExactOnly;
            SetSubmoduleStatus();

            chkTelemetry.Checked = AppSettings.TelemetryEnabled ?? false;

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            AppSettings.CheckForUncommittedChangesInCheckoutBranch = chkCheckForUncommittedChangesInCheckoutBranch.Checked;
            AppSettings.StartWithRecentWorkingDir = chkStartWithRecentWorkingDir.Checked;
            AppSettings.UseHistogramDiffAlgorithm = chkUseHistogramDiffAlgorithm.Checked;
            AppSettings.IncludeUntrackedFilesInAutoStash = chkStashUntrackedFiles.Checked;
            AppSettings.UpdateSubmodulesOnCheckout = ToBoolean(chkUpdateModules.CheckState);
            AppSettings.FollowRenamesInFileHistory = chkFollowRenamesInFileHistory.Checked;
            AppSettings.ShowGitStatusInBrowseToolbar = chkShowGitStatusInToolbar.Checked;
            AppSettings.ShowGitStatusForArtificialCommits = chkShowGitStatusForArtificialCommits.Checked;
            AppSettings.CloseProcessDialog = chkCloseProcessDialog.Checked;
            AppSettings.ShowGitCommandLine = chkShowGitCommandLine.Checked;
            AppSettings.MaxRevisionGraphCommits = lblCommitsLimit.Checked ? (int)_NO_TRANSLATE_MaxCommits.Value : 0;
            AppSettings.RevisionGridQuickSearchTimeout = (int)RevisionGridQuickSearchTimeout.Value;
            AppSettings.ShowStashCount = chkShowStashCountInBrowseWindow.Checked;
            AppSettings.ShowAheadBehindData = chkShowAheadBehindDataInBrowseWindow.Checked;
            AppSettings.ShowSubmoduleStatus = chkShowSubmoduleStatusInBrowse.Checked;

            AppSettings.DefaultCloneDestinationPath = cbDefaultCloneDestination.Text;
            AppSettings.DefaultPullAction = (AppSettings.PullAction)cboDefaultPullAction.SelectedValue;
            AppSettings.FollowRenamesInFileHistoryExactOnly = chkFollowRenamesInFileHistoryExact.Checked;

            AppSettings.TelemetryEnabled = chkTelemetry.Checked;

            base.PageToSettings();
        }

        private static Func<Repository, string> GetParentPath()
        {
            return x =>
            {
                if (x.Path.StartsWith(@"\\") || !Directory.Exists(x.Path))
                {
                    return string.Empty;
                }

                DirectoryInfo dir = new(x.Path);
                if (dir.Parent is null)
                {
                    return x.Path;
                }

                return dir.Parent.FullName;
            };
        }

        private static CheckState ToCheckboxState(bool? booleanValue)
        {
            if (!booleanValue.HasValue)
            {
                return CheckState.Indeterminate;
            }

            return booleanValue == true ? CheckState.Checked : CheckState.Unchecked;
        }

        private static bool? ToBoolean(CheckState state)
        {
            if (state == CheckState.Indeterminate)
            {
                return null;
            }

            return state == CheckState.Checked;
        }

        private void DefaultCloneDestinationBrowseClick(object sender, EventArgs e)
        {
            var userSelectedPath = OsShellUtil.PickFolder(this, cbDefaultCloneDestination.Text);

            if (userSelectedPath is not null)
            {
                cbDefaultCloneDestination.Text = userSelectedPath;
            }
        }

        private void ShowGitStatus_CheckedChanged(object sender, System.EventArgs e)
        {
            SetSubmoduleStatus();
        }

        private void LlblTelemetryPrivacyLink_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/blob/master/PrivacyPolicy.md");
        }

        private void lblCommitsLimit_CheckedChanged(object sender, EventArgs e)
        {
            _NO_TRANSLATE_MaxCommits.Enabled = lblCommitsLimit.Checked;
        }
    }
}
