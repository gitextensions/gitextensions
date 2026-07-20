using Avalonia.Controls;
using Avalonia.Interactivity;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public sealed partial class GeneralSettingsPage : SettingsPageWithHeader
{
    private readonly TranslationString _openPullDialog = new("Open pull dialog");
    private readonly TranslationString _pullMerge = new("Pull - merge");
    private readonly TranslationString _pullRebase = new("Pull - rebase");
    private readonly TranslationString _fetch = new("Fetch");
    private readonly TranslationString _fetchAll = new("Fetch all");
    private readonly TranslationString _fetchAndPruneAll = new("Fetch and prune all");
    private readonly PullActionItem[] _pullActions;

    public GeneralSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public GeneralSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        WireEvents();
        InitializeComplete();

        _pullActions =
        [
            new(_openPullDialog, GitPullAction.None),
            new(_pullMerge, GitPullAction.Merge),
            new(_pullRebase, GitPullAction.Rebase),
            new(_fetch, GitPullAction.Fetch),
            new(_fetchAll, GitPullAction.FetchAll),
            new(_fetchAndPruneAll, GitPullAction.FetchPruneAll),
        ];
        cboDefaultPullAction.ItemsSource = _pullActions;
        cboDefaultPullAction.SelectedIndex = 0;

        if (serviceProvider is IGitUICommands)
        {
            IList<Repository> repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(
                RepositoryHistoryManager.Locals.LoadRecentHistoryAsync);
            cbDefaultCloneDestination.ItemsSource = repositoryHistory
                .Select(repository => repository.GetParentPath())
                .Where(path => !string.IsNullOrEmpty(path))
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .ToArray();
        }
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(GeneralSettingsPage));

    protected override void SettingsToPage()
    {
        chkCheckForUncommittedChangesInCheckoutBranch.IsChecked = AppSettings.CheckForUncommittedChangesInCheckoutBranch;
        chkStartWithRecentWorkingDir.IsChecked = AppSettings.StartWithRecentWorkingDir;
        chkUseHistogramDiffAlgorithm.IsChecked = AppSettings.UseHistogramDiffAlgorithm;
        RevisionGridQuickSearchTimeout.Value = AppSettings.RevisionGridQuickSearchTimeout;
        chkFollowRenamesInFileHistory.IsChecked = AppSettings.FollowRenamesInFileHistory;
        chkStashUntrackedFiles.IsChecked = AppSettings.IncludeUntrackedFilesInAutoStash;
        chkUpdateModules.IsChecked = AppSettings.UpdateSubmodulesOnCheckout;
        chkShowStashCountInBrowseWindow.IsChecked = AppSettings.ShowStashCount;
        chkShowAheadBehindDataInBrowseWindow.IsChecked = AppSettings.ShowAheadBehindData;
        chkShowGitStatusInToolbar.IsChecked = AppSettings.ShowGitStatusInBrowseToolbar;
        chkShowGitStatusForArtificialCommits.IsChecked = AppSettings.ShowGitStatusForArtificialCommits;
        chkShowSubmoduleStatusInBrowse.IsChecked = AppSettings.ShowSubmoduleStatus;
        lblCommitsLimit.IsChecked = AppSettings.MaxRevisionGraphCommits != 0;
        _NO_TRANSLATE_MaxCommits.Value = AppSettings.MaxRevisionGraphCommits;
        _NO_TRANSLATE_MaxCommits.IsEnabled = AppSettings.MaxRevisionGraphCommits != 0;
        chkCloseProcessDialog.IsChecked = AppSettings.CloseProcessDialog;
        chkShowGitCommandLine.IsChecked = AppSettings.ShowGitCommandLine;
        cbDefaultCloneDestination.Text = AppSettings.DefaultCloneDestinationPath;
        GitPullAction pullAction = AppSettings.DefaultPullAction == GitPullAction.Default
            ? GitPullAction.None
            : AppSettings.DefaultPullAction;
        cboDefaultPullAction.SelectedItem = _pullActions.FirstOrDefault(item => item.Value == pullAction)
            ?? _pullActions[0];
        chkFollowRenamesInFileHistoryExact.IsChecked = AppSettings.FollowRenamesInFileHistoryExactOnly;
        chkTelemetry.IsChecked = AppSettings.TelemetryEnabled ?? false;
        SetSubmoduleStatus();

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.CheckForUncommittedChangesInCheckoutBranch = chkCheckForUncommittedChangesInCheckoutBranch.IsChecked == true;
        AppSettings.StartWithRecentWorkingDir = chkStartWithRecentWorkingDir.IsChecked == true;
        AppSettings.UseHistogramDiffAlgorithm = chkUseHistogramDiffAlgorithm.IsChecked == true;
        AppSettings.IncludeUntrackedFilesInAutoStash = chkStashUntrackedFiles.IsChecked == true;
        AppSettings.UpdateSubmodulesOnCheckout = chkUpdateModules.IsChecked;
        AppSettings.FollowRenamesInFileHistory = chkFollowRenamesInFileHistory.IsChecked == true;
        AppSettings.ShowGitStatusInBrowseToolbar = chkShowGitStatusInToolbar.IsChecked == true;
        AppSettings.ShowGitStatusForArtificialCommits = chkShowGitStatusForArtificialCommits.IsChecked == true;
        AppSettings.CloseProcessDialog = chkCloseProcessDialog.IsChecked == true;
        AppSettings.ShowGitCommandLine = chkShowGitCommandLine.IsChecked == true;
        AppSettings.MaxRevisionGraphCommits = lblCommitsLimit.IsChecked == true ? (int)(_NO_TRANSLATE_MaxCommits.Value ?? 0) : 0;
        AppSettings.RevisionGridQuickSearchTimeout = (int)(RevisionGridQuickSearchTimeout.Value ?? 100);
        AppSettings.ShowStashCount = chkShowStashCountInBrowseWindow.IsChecked == true;
        AppSettings.ShowAheadBehindData = chkShowAheadBehindDataInBrowseWindow.IsChecked == true;
        AppSettings.ShowSubmoduleStatus = chkShowSubmoduleStatusInBrowse.IsChecked == true;
        AppSettings.DefaultCloneDestinationPath = cbDefaultCloneDestination.Text ?? string.Empty;
        AppSettings.DefaultPullAction = (cboDefaultPullAction.SelectedItem as PullActionItem)?.Value ?? GitPullAction.None;
        AppSettings.FollowRenamesInFileHistoryExactOnly = chkFollowRenamesInFileHistoryExact.IsChecked == true;
        AppSettings.TelemetryEnabled = chkTelemetry.IsChecked == true;

        base.PageToSettings();
    }

    private void WireEvents()
    {
        chkShowGitStatusInToolbar.IsCheckedChanged += (_, _) => SetSubmoduleStatus();
        chkShowGitStatusForArtificialCommits.IsCheckedChanged += (_, _) => SetSubmoduleStatus();
        lblCommitsLimit.IsCheckedChanged += (_, _) =>
            _NO_TRANSLATE_MaxCommits.IsEnabled = lblCommitsLimit.IsChecked == true;
        btnDefaultDestinationBrowse.Click += DefaultCloneDestinationBrowseClick;
        llblTelemetryPrivacyLink.Click += (_, _) => OsShellUtil.OpenUrlInDefaultBrowser(
            "https://github.com/gitextensions/gitextensions/blob/master/setup/assets/PrivacyPolicy.md");
    }

    private void SetSubmoduleStatus()
    {
        chkShowSubmoduleStatusInBrowse.IsEnabled =
            chkShowGitStatusInToolbar.IsChecked == true || chkShowGitStatusForArtificialCommits.IsChecked == true;
        if (!chkShowSubmoduleStatusInBrowse.IsEnabled)
        {
            chkShowSubmoduleStatusInBrowse.IsChecked = false;
        }
    }

    private void DefaultCloneDestinationBrowseClick(object? sender, RoutedEventArgs e)
    {
        WinFormsShims.IWin32Window owner = (TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window)!;
        string? userSelectedPath = OsShellUtil.PickFolder(owner, cbDefaultCloneDestination.Text);
        if (userSelectedPath is not null)
        {
            cbDefaultCloneDestination.Text = userSelectedPath;
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(GeneralSettingsPage page)
    {
        public CheckBox ShowGitStatusInToolbar => page.chkShowGitStatusInToolbar;

        public CheckBox ShowGitStatusForArtificialCommits => page.chkShowGitStatusForArtificialCommits;

        public CheckBox ShowSubmoduleStatusInBrowse => page.chkShowSubmoduleStatusInBrowse;

        public CheckBox LimitCommits => page.lblCommitsLimit;

        public NumericUpDown MaxCommits => page._NO_TRANSLATE_MaxCommits;

        public CheckBox UpdateModules => page.chkUpdateModules;

        public ComboBox DefaultPullAction => page.cboDefaultPullAction;

        public ComboBox DefaultCloneDestination => page.cbDefaultCloneDestination;
    }

    private sealed record PullActionItem(TranslationString Key, GitPullAction Value)
    {
        public override string ToString() => Key.Text;
    }
}
