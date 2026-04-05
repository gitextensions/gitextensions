using System.ComponentModel;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class GeneralSettingsPage : SettingsPageWithHeader
{
    private readonly TranslationString _openPullDialog = new("Open pull dialog");
    private readonly TranslationString _pullMerge = new("Pull - merge");
    private readonly TranslationString _pullRebase = new("Pull - rebase");
    private readonly TranslationString _fetch = new("Fetch");
    private readonly TranslationString _fetchAll = new("Fetch all");
    private readonly TranslationString _fetchAndPruneAll = new("Fetch and prune all");

    public GeneralSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        InitializeComponent();
        InitializeComplete();

        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || GitModuleForm.IsUnitTestActive)
        {
            return;
        }

        IList<Repository> repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Locals.LoadRecentHistoryAsync);
        string[] historicPaths = [.. repositoryHistory.Select(x => x.GetParentPath())
                                                  .Where(x => !string.IsNullOrEmpty(x))
                                                  .Distinct(StringComparer.CurrentCultureIgnoreCase)];
        cbDefaultCloneDestination.Items.AddRange(historicPaths);

        var pullActions = new[]
        {
            new { Key = _openPullDialog, Value = GitPullAction.None },
            new { Key = _pullMerge, Value = GitPullAction.Merge },
            new { Key = _pullRebase, Value = GitPullAction.Rebase },
            new { Key = _fetch, Value = GitPullAction.Fetch },
            new { Key = _fetchAll, Value = GitPullAction.FetchAll },
            new { Key = _fetchAndPruneAll, Value = GitPullAction.FetchPruneAll },
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
        chkCheckForUncommittedChangesInCheckoutBranch.Checked = AppSettings.CheckForUncommittedChangesInCheckoutBranch.Value;
        chkStartWithRecentWorkingDir.Checked = AppSettings.StartWithRecentWorkingDir.Value;
        chkUseHistogramDiffAlgorithm.Checked = AppSettings.UseHistogramDiffAlgorithm.Value;
        RevisionGridQuickSearchTimeout.Value = AppSettings.RevisionGridQuickSearchTimeout.Value;
        chkFollowRenamesInFileHistory.Checked = AppSettings.FollowRenamesInFileHistory.Value;
        chkStashUntrackedFiles.Checked = AppSettings.IncludeUntrackedFilesInAutoStash.Value;
        chkUpdateModules.CheckState = ToCheckboxState(AppSettings.UpdateSubmodulesOnCheckout);
        chkShowStashCountInBrowseWindow.Checked = AppSettings.ShowStashCount.Value;
        chkShowAheadBehindDataInBrowseWindow.Checked = AppSettings.ShowAheadBehindData.Value;
        chkShowGitStatusInToolbar.Checked = AppSettings.ShowGitStatusInBrowseToolbar.Value;
        chkShowGitStatusForArtificialCommits.Checked = AppSettings.ShowGitStatusForArtificialCommits.Value;
        chkShowSubmoduleStatusInBrowse.Checked = AppSettings.ShowSubmoduleStatus.Value;
        lblCommitsLimit.Checked = AppSettings.MaxRevisionGraphCommits.Value != 0;
        _NO_TRANSLATE_MaxCommits.Value = AppSettings.MaxRevisionGraphCommits.Value;
        _NO_TRANSLATE_MaxCommits.Enabled = AppSettings.MaxRevisionGraphCommits.Value != 0;
        chkCloseProcessDialog.Checked = AppSettings.CloseProcessDialog.Value;
        chkShowGitCommandLine.Checked = AppSettings.ShowGitCommandLine.Value;
        cbDefaultCloneDestination.Text = AppSettings.DefaultCloneDestinationPath.Value;
        cboDefaultPullAction.SelectedValue
            = AppSettings.DefaultPullAction.Value != GitPullAction.Default ?
              AppSettings.DefaultPullAction.Value : GitPullAction.None;
        chkFollowRenamesInFileHistoryExact.Checked = AppSettings.FollowRenamesInFileHistoryExactOnly.Value;
        SetSubmoduleStatus();

        chkTelemetry.Checked = AppSettings.TelemetryEnabled ?? false;

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.CheckForUncommittedChangesInCheckoutBranch.Value = chkCheckForUncommittedChangesInCheckoutBranch.Checked;
        AppSettings.StartWithRecentWorkingDir.Value = chkStartWithRecentWorkingDir.Checked;
        AppSettings.UseHistogramDiffAlgorithm.Value = chkUseHistogramDiffAlgorithm.Checked;
        AppSettings.IncludeUntrackedFilesInAutoStash.Value = chkStashUntrackedFiles.Checked;
        AppSettings.UpdateSubmodulesOnCheckout = ToBoolean(chkUpdateModules.CheckState);
        AppSettings.FollowRenamesInFileHistory.Value = chkFollowRenamesInFileHistory.Checked;
        AppSettings.ShowGitStatusInBrowseToolbar.Value = chkShowGitStatusInToolbar.Checked;
        AppSettings.ShowGitStatusForArtificialCommits.Value = chkShowGitStatusForArtificialCommits.Checked;
        AppSettings.CloseProcessDialog.Value = chkCloseProcessDialog.Checked;
        AppSettings.ShowGitCommandLine.Value = chkShowGitCommandLine.Checked;
        AppSettings.MaxRevisionGraphCommits.Value = lblCommitsLimit.Checked ? (int)_NO_TRANSLATE_MaxCommits.Value : 0;
        AppSettings.RevisionGridQuickSearchTimeout.Value = (int)RevisionGridQuickSearchTimeout.Value;
        AppSettings.ShowStashCount.Value = chkShowStashCountInBrowseWindow.Checked;
        AppSettings.ShowAheadBehindData.Value = chkShowAheadBehindDataInBrowseWindow.Checked;
        AppSettings.ShowSubmoduleStatus.Value = chkShowSubmoduleStatusInBrowse.Checked;

        AppSettings.DefaultCloneDestinationPath.Value = cbDefaultCloneDestination.Text;
        AppSettings.DefaultPullAction.Value = (GitPullAction)cboDefaultPullAction.SelectedValue;
        AppSettings.FollowRenamesInFileHistoryExactOnly.Value = chkFollowRenamesInFileHistoryExact.Checked;

        AppSettings.TelemetryEnabled = chkTelemetry.Checked;

        base.PageToSettings();
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
        string userSelectedPath = OsShellUtil.PickFolder(this, cbDefaultCloneDestination.Text);

        if (userSelectedPath is not null)
        {
            cbDefaultCloneDestination.Text = userSelectedPath;
        }
    }

    private void ShowGitStatus_CheckedChanged(object sender, EventArgs e)
    {
        SetSubmoduleStatus();
    }

    private void LlblTelemetryPrivacyLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/blob/master/setup/assets/PrivacyPolicy.md");
    }

    private void lblCommitsLimit_CheckedChanged(object sender, EventArgs e)
    {
        _NO_TRANSLATE_MaxCommits.Enabled = lblCommitsLimit.Checked;
    }
}
