using Avalonia.Controls;
using Avalonia.Input;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Gpg;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.Avatars;
using GitUI.Compat;
using GitUI.ConsoleEmulation;
using GitUI.Models;
using GitUI.UserControls;
using GitUIPluginInterfaces;

using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Avalonia twin of the repository browser. Controls owned by later port phases are added only
// when their commands are functional, rather than presenting inert toolbar entries.
public sealed partial class FormBrowse : GitModuleForm
{
    private readonly TranslationString _consoleTabCaption = new("Console");
    private readonly TranslationString _outputHistoryTabCaption = new("Output");

    private readonly IAheadBehindDataProvider? _aheadBehindDataProvider;
    private readonly IConsoleEmulatorsRegistry? _consoleEmulatorsRegistry;
    private readonly IGpgInfoProvider? _controller;
    private readonly CancellationTokenSequence _gpgInfoLoadSequence = new();
    private readonly CancellationTokenSource _loadOperationsCancellationTokenSource = new();
    private readonly TaskManager _loadOperations = ThreadHelper.CreateTaskManager();
    private readonly SplitterManager? _splitterManager;
    private GridLength _commitInfoWidth = new(490);
    private GpgInfo? _gpgInfo;
    private GitRevision? _gpgInfoLoadingRevision;
    private GitRevision? _gpgInfoRevision;
    private GridLength _leftPanelWidth = new(260);
    private GridLength _splitViewBottomHeight = new(2, GridUnitType.Star);
    private GridLength _splitViewTopHeight = new(3, GridUnitType.Star);
    private GitRevision? _fileTreeRevision;
    private bool _gpgInfoLoaded;
    private bool _hasRuntimeCommands;
    private int _gpgInfoLoadVersion;
    private bool _updatingWorkingDirectories;
    private IConsoleShellRunner? _terminal;
    private TabItem? _consoleTabPage;
    private OutputHistoryControllerBase? _outputHistoryController;

    public static readonly string HotkeySettingsName = "Browse";

    internal enum Command
    {
        GitBash = 0,
        FocusRevisionGrid = 3,
        FocusCommitInfo = 4,
        FocusDiff = 5,
        FocusFileTree = 6,
        FocusGpgInfo = 26,
        FocusGitConsole = 29,
        FocusOutputHistoryAndToggleIfPanel = 47,
        Commit = 7,
        CheckoutBranch = 10,
        FocusFilter = 18,
        OpenSettings = 20,
        ToggleLeftPanel = 21,
        FocusNextTab = 31,
        FocusPrevTab = 32,
        PullOrFetch = 39,
        Push = 40,
        CreateBranch = 41,
        MergeBranches = 42,
        CreateTag = 43,
        Rebase = 44,

        // WinForms routes F5 through ToolStripItem.ShortcutKeys. Avalonia has no ToolStrip,
        // so refresh joins the same command dispatcher without changing persisted upstream IDs.
        Refresh = 50,
    }

    public FormBrowse()
    {
        InitializeComponent();
        InitializeWorkspaceLayout();
        InitializeComplete();
    }

    public FormBrowse(IServiceProvider serviceProvider, GitModule module)
        : this(new GitUICommands(serviceProvider, module))
    {
    }

    public FormBrowse(IGitUICommands commands)
        : this(commands, gpgInfoProvider: null)
    {
    }

    internal FormBrowse(IGitUICommands commands, IGpgInfoProvider? gpgInfoProvider)
        : base(commands, enablePositionRestore: true)
    {
        InitializeComponent();

        _hasRuntimeCommands = true;
        RevisionGrid.UICommandsSource = this;
        revisionDiff.UICommandsSource = this;
        fileTree.UICommandsSource = this;
        _consoleEmulatorsRegistry = UICommands.GetService(typeof(IConsoleEmulatorsRegistry)) as IConsoleEmulatorsRegistry;
        _controller = gpgInfoProvider ?? new GpgInfoProvider(new GitGpgController(() => Module));
        _aheadBehindDataProvider = new AheadBehindDataProvider(() => Module.GitExecutable);
        RevisionGrid.SetAheadBehindDataProvider(_aheadBehindDataProvider);
        RevisionGrid.SelectionChanged += RevisionGrid_SelectionChanged;
        RevisionGrid.RevisionFilterRequested += (_, _) => ToolStripFilters.SetFocus();
        ToolStripFilters.Bind(() => Module, RevisionGrid);
        revisionDiff.Bind(RevisionGrid, RevisionGrid, fileTree, () => string.Empty, refreshGitStatus: null);
        fileTree.Bind(RevisionGrid, RevisionGrid, revisionFileTree: null, () => string.Empty, refreshGitStatus: null);
        _splitterManager = new SplitterManager(new AppSettingsPath("FormBrowse.Avalonia"));
        revisionDiff.InitSplitterManager(_splitterManager);
        fileTree.InitSplitterManager(_splitterManager);
        _splitterManager.RestoreSplitters();
        RevisionGrid.FilterChanged += RevisionGrid_FilterChanged;
        CommitInfoTabControl.SelectionChanged += CommitInfoTabControl_SelectionChanged;
        repoObjectsTree.SelectionChanged += RepoObjectsTree_SelectionChanged;
        refreshToolStripMenuItem.Click += RefreshToolStripMenuItemClick;
        commitToolStripMenuItem.Click += CommitToolStripMenuItemClick;
        checkoutBranchToolStripMenuItem.Click += CheckoutBranchToolStripMenuItemClick;
        branchToolStripMenuItem.Click += CreateBranchToolStripMenuItemClick;
        deleteBranchToolStripMenuItem.Click += DeleteBranchToolStripMenuItemClick;
        pullToolStripMenuItem.Click += PullToolStripMenuItemClick;
        fetchAllToolStripMenuItem.Click += fetchAllToolStripMenuItem_Click;
        mergeBranchToolStripMenuItem.Click += MergeBranchToolStripMenuItemClick;
        rebaseToolStripMenuItem.Click += RebaseToolStripMenuItemClick;
        tagToolStripMenuItem.Click += TagToolStripMenuItemClick;
        deleteTagToolStripMenuItem.Click += DeleteTagToolStripMenuItemClick;
        stashToolStripMenuItem.Click += StashToolStripMenuItemClick;
        patchToolStripMenuItem.Click += PatchToolStripMenuItemClick;
        RefreshButton.Click += RefreshToolStripMenuItemClick;
        toggleLeftPanel.Click += ToggleLeftPanelClick;
        InitializeWorkspaceLayout();
        FillTerminalTab();
        InitializeOutputHistory();
        branchSelect.Click += BranchSelectClick;
        BranchSelectFlyout.Opening += (_, _) => PopulateBranchSelector();
        toolStripButtonPull.Click += ToolStripButtonPullClick;
        toolStripButtonPush.Click += (_, _) => UICommands.StartPushDialog(this, pushOnShow: false);
        toolStripButtonCommit.Click += CommitToolStripMenuItemClick;
        toolStripSplitStash.Click += StashToolStripMenuItemClick;
        toolStripFileExplorer.Click += FileExplorerToolStripMenuItemClick;
        userShell.Click += userShell_Click;
        EditSettings.Click += OnShowSettingsClick;
        UserShellToolStripMenuItem.Click += userShell_Click;
        pullToolStripMenuItem1.Click += (_, _) => DoPull(AppSettings.FormPullAction, isSilent: false);
        mergeToolStripMenuItem.Click += (_, _) => DoPull(GitPullAction.Merge, isSilent: true);
        rebaseToolStripMenuItem1.Click += (_, _) => DoPull(GitPullAction.Rebase, isSilent: true);
        fetchToolStripMenuItem.Click += (_, _) => DoPull(GitPullAction.Fetch, isSilent: true);
        FetchAllToolbarMenuItem.Click += (_, _) => DoPull(GitPullAction.FetchAll, isSilent: true);
        fetchPruneAllToolStripMenuItem.Click += (_, _) => DoPull(GitPullAction.FetchPruneAll, isSilent: true);
        defaultPullDialogToolStripMenuItem.Click += (_, _) => SetDefaultPullAction(GitPullAction.None);
        defaultPullMergeToolStripMenuItem.Click += (_, _) => SetDefaultPullAction(GitPullAction.Merge);
        defaultPullRebaseToolStripMenuItem.Click += (_, _) => SetDefaultPullAction(GitPullAction.Rebase);
        defaultPullFetchToolStripMenuItem.Click += (_, _) => SetDefaultPullAction(GitPullAction.Fetch);
        defaultPullFetchAllToolStripMenuItem.Click += (_, _) => SetDefaultPullAction(GitPullAction.FetchAll);
        defaultPullFetchPruneAllToolStripMenuItem.Click += (_, _) => SetDefaultPullAction(GitPullAction.FetchPruneAll);
        stashChangesToolStripMenuItem.Click += (_, _) => UICommands.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash);
        stashStagedToolStripMenuItem.Click += (_, _) => UICommands.StashStaged(this);
        stashPopToolStripMenuItem.Click += (_, _) => UICommands.StashPop(this);
        manageStashesToolStripMenuItem.Click += StashToolStripMenuItemClick;
        createAStashToolStripMenuItem.Click += (_, _) => UICommands.StartStashDialog(this, manageStashes: false);
        _NO_TRANSLATE_WorkingDir.SelectionChanged += WorkingDirectorySelectionChanged;
        _NO_TRANSLATE_WorkingDir.KeyUp += WorkingDirectoryKeyUp;
        UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;

        ReloadRepository();

        InitializeComplete();
        HotkeysEnabled = true;
        LoadHotkeys(HotkeySettingsName);
    }

    private void ReloadRepository()
    {
        IGitModule module = Module;

        bool isValidWorkingDir = module.IsValidGitWorkingDir();
        string branchName = isValidWorkingDir ? module.GetSelectedBranch() : string.Empty;

        IAppTitleGenerator appTitleGenerator = UICommands.GetRequiredService<IAppTitleGenerator>();
        Title = appTitleGenerator.Generate(module.WorkingDir, isValidWorkingDir, branchName);

        refreshToolStripMenuItem.IsEnabled = isValidWorkingDir;
        commitToolStripMenuItem.IsEnabled = isValidWorkingDir && !module.IsBareRepository();
        checkoutBranchToolStripMenuItem.IsEnabled = isValidWorkingDir;
        branchToolStripMenuItem.IsEnabled = isValidWorkingDir;
        deleteBranchToolStripMenuItem.IsEnabled = isValidWorkingDir;
        pullToolStripMenuItem.IsEnabled = isValidWorkingDir;
        fetchAllToolStripMenuItem.IsEnabled = isValidWorkingDir;
        mergeBranchToolStripMenuItem.IsEnabled = isValidWorkingDir && !module.IsBareRepository();
        rebaseToolStripMenuItem.IsEnabled = false;
        tagToolStripMenuItem.IsEnabled = isValidWorkingDir;
        deleteTagToolStripMenuItem.IsEnabled = isValidWorkingDir;
        stashToolStripMenuItem.IsEnabled = isValidWorkingDir && !module.IsBareRepository();
        RefreshButton.IsEnabled = isValidWorkingDir;
        branchSelect.IsEnabled = isValidWorkingDir;
        toolStripButtonPull.IsEnabled = isValidWorkingDir;
        toolStripButtonPush.IsEnabled = isValidWorkingDir;
        toolStripButtonCommit.IsEnabled = isValidWorkingDir && !module.IsBareRepository();
        toolStripSplitStash.IsEnabled = isValidWorkingDir && !module.IsBareRepository();
        toolStripFileExplorer.IsEnabled = Directory.Exists(module.WorkingDir);
        userShell.IsEnabled = Directory.Exists(module.WorkingDir);
        ToolStripFilters.IsEnabled = isValidWorkingDir;
        branchSelect.Content = string.IsNullOrEmpty(branchName) ? "Branch" : branchName;
        RefreshDefaultPullAction();

        if (isValidWorkingDir)
        {
            LoadWorkingDirectories();
            _aheadBehindDataProvider?.ResetCache();
            lblRepoPath.Text = $"{module.WorkingDir}  —  {branchName}";
            lblStatus.Text = $"git: {GitVersion.Current}";
            RevisionGrid.ReloadRevisions(module);

            CancellationToken cancellationToken = _loadOperationsCancellationTokenSource.Token;
            _loadOperations.FileAndForget(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                IReadOnlyList<IGitRef> refs = module.GetRefs(RefsFilter.NoFilter);
                await _loadOperations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                repoObjectsTree.SetRefs(refs);
            });
        }
        else
        {
            _NO_TRANSLATE_WorkingDir.Text = module.WorkingDir;
            lblRepoPath.Text = "No git repository";
            lblStatus.Text = "Start the app inside a repository or pass one on the command line: GitExtensions.Avalonia browse <path>";
        }
    }

    private void LoadWorkingDirectories()
    {
        string workingDirectory = Module.WorkingDir;
        CancellationToken cancellationToken = _loadOperationsCancellationTokenSource.Token;
        _loadOperations.FileAndForget(async () =>
        {
            IList<Repository> history = await RepositoryHistoryManager.Locals.LoadRecentHistoryAsync().WaitAsync(cancellationToken);
            string[] directories =
            [
                workingDirectory,
                .. history.Select(repository => repository.Path)
                    .Where(path => !string.Equals(path, workingDirectory, StringComparison.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase),
            ];

            await _loadOperations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            if (!string.Equals(Module.WorkingDir, workingDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _updatingWorkingDirectories = true;
            _NO_TRANSLATE_WorkingDir.ItemsSource = directories;
            _NO_TRANSLATE_WorkingDir.Text = workingDirectory;
            _updatingWorkingDirectories = false;
        });
    }

    private void WorkingDirectorySelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_updatingWorkingDirectories && _NO_TRANSLATE_WorkingDir.SelectedItem is string path)
        {
            ChangeWorkingDirectory(path);
        }
    }

    private void WorkingDirectoryKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(_NO_TRANSLATE_WorkingDir.Text))
        {
            ChangeWorkingDirectory(_NO_TRANSLATE_WorkingDir.Text);
        }
    }

    private void ChangeWorkingDirectory(string path)
    {
        string normalizedPath;
        try
        {
            normalizedPath = Path.GetFullPath(path);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            MessageBoxes.ShowError(this, exception.Message);
            return;
        }

        if (string.Equals(normalizedPath, Path.GetFullPath(Module.WorkingDir), StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        UICommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
        UICommands = UICommands.WithWorkingDirectory(normalizedPath);
        UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
        ChangeTerminalActiveFolder(normalizedPath);
        ReloadRepository();
        CancellationToken cancellationToken = _loadOperationsCancellationTokenSource.Token;
        _loadOperations.FileAndForget(() => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(normalizedPath).WaitAsync(cancellationToken));
    }

    private void ToggleLeftPanelClick(object? sender, EventArgs e)
    {
        ColumnDefinition leftColumn = mainContentGrid.ColumnDefinitions[0];
        bool hide = leftColumn.Width.Value > 0;
        if (hide)
        {
            _leftPanelWidth = leftColumn.Width;
            leftColumn.Width = new GridLength(0);
            leftPanel.IsVisible = false;
            leftPanelSplitter.IsVisible = false;
        }
        else
        {
            leftColumn.Width = _leftPanelWidth.Value > 0 ? _leftPanelWidth : new GridLength(260);
            leftPanel.IsVisible = true;
            leftPanelSplitter.IsVisible = true;
        }
    }

    private void InitializeWorkspaceLayout()
    {
        toggleSplitViewLayout.Click += ToggleSplitViewLayoutClick;
        menuCommitInfoPosition.Click += CommitInfoPositionClick;
        commitInfoBelowMenuItem.Click += (_, _) => SetCommitInfoPosition(CommitInfoPosition.BelowList);
        commitInfoLeftwardMenuItem.Click += (_, _) => SetCommitInfoPosition(CommitInfoPosition.LeftwardFromList);
        commitInfoRightwardMenuItem.Click += (_, _) => SetCommitInfoPosition(CommitInfoPosition.RightwardFromList);
        RefreshWorkspaceLayout();
    }

    private void ToggleSplitViewLayoutClick(object? sender, EventArgs e)
    {
        RememberWorkspaceDimensions();
        AppSettings.ShowSplitViewLayout = !AppSettings.ShowSplitViewLayout;
        RefreshWorkspaceLayout(selectCommitInfoTab: false);
    }

    private void CommitInfoPositionClick(object? sender, EventArgs e)
    {
        CommitInfoPosition[] positions = Enum.GetValues<CommitInfoPosition>();
        int next = ((int)AppSettings.CommitInfoPosition + 1) % positions.Length;
        SetCommitInfoPosition((CommitInfoPosition)next);
    }

    private void SetCommitInfoPosition(CommitInfoPosition position)
    {
        RememberWorkspaceDimensions();
        AppSettings.CommitInfoPosition = position;
        RefreshWorkspaceLayout(refreshCommitInfoPositionToolTip: true);
    }

    private void RememberWorkspaceDimensions()
    {
        CommitInfoPosition position = AppSettings.CommitInfoPosition;
        if (position == CommitInfoPosition.LeftwardFromList
            && RevisionsSplitContainer.ColumnDefinitions[0].Width.Value > 0)
        {
            _commitInfoWidth = RevisionsSplitContainer.ColumnDefinitions[0].Width;
        }
        else if (position == CommitInfoPosition.RightwardFromList
                 && RevisionsSplitContainer.ColumnDefinitions[4].Width.Value > 0)
        {
            _commitInfoWidth = RevisionsSplitContainer.ColumnDefinitions[4].Width;
        }

        if (AppSettings.ShowSplitViewLayout
            && RightSplitContainer.RowDefinitions[2].Height.Value > 0)
        {
            _splitViewTopHeight = RightSplitContainer.RowDefinitions[0].Height;
            _splitViewBottomHeight = RightSplitContainer.RowDefinitions[2].Height;
        }
    }

    private void RefreshWorkspaceLayout(
        bool selectCommitInfoTab = true,
        bool refreshCommitInfoPositionToolTip = false)
    {
        CommitInfoPosition position = AppSettings.CommitInfoPosition;
        bool below = position == CommitInfoPosition.BelowList;

        Border commitInfoHost = position switch
        {
            CommitInfoPosition.BelowList => commitInfoBelowHost,
            CommitInfoPosition.LeftwardFromList => commitInfoLeftHost,
            CommitInfoPosition.RightwardFromList => commitInfoRightHost,
            _ => throw new NotSupportedException(),
        };
        if (!ReferenceEquals(commitInfoHost.Child, RevisionInfo))
        {
            if (ReferenceEquals(commitInfoBelowHost.Child, RevisionInfo))
            {
                commitInfoBelowHost.Child = null;
            }
            else if (ReferenceEquals(commitInfoLeftHost.Child, RevisionInfo))
            {
                commitInfoLeftHost.Child = null;
            }
            else if (ReferenceEquals(commitInfoRightHost.Child, RevisionInfo))
            {
                commitInfoRightHost.Child = null;
            }

            commitInfoHost.Child = RevisionInfo;
        }

        ColumnDefinitions columns = RevisionsSplitContainer.ColumnDefinitions;
        columns[0].Width = position == CommitInfoPosition.LeftwardFromList ? _commitInfoWidth : new GridLength(0);
        columns[1].Width = new GridLength(0);
        columns[2].Width = new GridLength(1, GridUnitType.Star);
        columns[3].Width = new GridLength(0);
        columns[4].Width = position == CommitInfoPosition.RightwardFromList ? _commitInfoWidth : new GridLength(0);

        commitInfoLeftSplitter.IsVisible = position == CommitInfoPosition.LeftwardFromList;
        commitInfoRightSplitter.IsVisible = position == CommitInfoPosition.RightwardFromList;
        CommitInfoTabPage.IsVisible = below;

        if (below)
        {
            if (selectCommitInfoTab)
            {
                CommitInfoTabControl.SelectedItem = CommitInfoTabPage;
            }
        }
        else
        {
            if (CommitInfoTabControl.SelectedItem == CommitInfoTabPage)
            {
                CommitInfoTabControl.SelectedItem = DiffTabPage;
            }
        }

        bool showSplitView = AppSettings.ShowSplitViewLayout;
        RowDefinitions rows = RightSplitContainer.RowDefinitions;
        rows[0].Height = showSplitView ? _splitViewTopHeight : new GridLength(1, GridUnitType.Star);
        rows[1].Height = new GridLength(0);
        rows[2].Height = showSplitView ? _splitViewBottomHeight : new GridLength(0);
        splitViewSplitter.IsVisible = showSplitView;
        CommitInfoTabControl.IsVisible = showSplitView;

        toggleSplitViewLayout.Classes.Set("checked", showSplitView);
        menuCommitInfoPositionImage.Source = position switch
        {
            CommitInfoPosition.BelowList => Properties.Images.LayoutFooterTab,
            CommitInfoPosition.LeftwardFromList => Properties.Images.LayoutSidebarTopLeft,
            CommitInfoPosition.RightwardFromList => Properties.Images.LayoutSidebarTopRight,
            _ => throw new NotSupportedException(),
        };
        if (refreshCommitInfoPositionToolTip)
        {
            RefreshCommitInfoPositionToolTip();
        }
    }

    private void RefreshCommitInfoPositionToolTip()
    {
        MenuItem selectedItem = AppSettings.CommitInfoPosition switch
        {
            CommitInfoPosition.BelowList => commitInfoBelowMenuItem,
            CommitInfoPosition.LeftwardFromList => commitInfoLeftwardMenuItem,
            CommitInfoPosition.RightwardFromList => commitInfoRightwardMenuItem,
            _ => throw new NotSupportedException(),
        };
        if (selectedItem.Header is string header)
        {
            ToolTip.SetTip(menuCommitInfoPosition, header.Replace("_", string.Empty));
        }
    }

    private void BranchSelectClick(object? sender, EventArgs e)
    {
        PopulateBranchSelector();
        branchSelect.Flyout?.ShowAt(branchSelect);
    }

    private void PopulateBranchSelector()
    {
        BranchSelectFlyout.Items.Clear();
        MenuItem checkout = new() { Header = checkoutBranchToolStripMenuItem.Header, Icon = checkoutBranchToolStripMenuItem.Icon };
        checkout.Click += CheckoutBranchToolStripMenuItemClick;
        BranchSelectFlyout.Items.Add(checkout);
        BranchSelectFlyout.Items.Add(new Separator());
        foreach (IGitRef branch in Module.GetRefs(RefsFilter.Heads).Take(100))
        {
            MenuItem item = new() { Header = branch.Name, IsEnabled = !branch.ObjectId.IsZero };
            item.Click += (_, _) => UICommands.StartCheckoutBranch(this, branch.Name);
            BranchSelectFlyout.Items.Add(item);
        }
    }

    private MenuFlyout BranchSelectFlyout => (MenuFlyout)branchSelect.Flyout!;

    private MenuItem UserShellToolStripMenuItem
        => toolsToolStripMenuItem.Items
            .OfType<MenuItem>()
            .Single(item => item.Tag as string == "userShell");

    private MenuItem FetchAllToolbarMenuItem
        => ((MenuFlyout)toolStripButtonPull.Flyout!).Items
            .OfType<MenuItem>()
            .Single(item => item.Tag as string == "fetchAllToolbar");

    private void RepoObjectsTree_SelectionChanged(object? sender, EventArgs e)
    {
        if (repoObjectsTree.SelectedRef is IGitRef gitRef)
        {
            RevisionGrid.SelectRevision(gitRef.ObjectId);
        }
    }

    private void RevisionGrid_SelectionChanged(object? sender, EventArgs e)
    {
        IReadOnlyList<GitRevision> selectedRevisions = RevisionGrid.GetSelectedRevisions();
        revisionDiff.DisplayDiffTab(selectedRevisions);
        fileTree.Clear();
        _fileTreeRevision = null;
        RefreshGpgInfo(RevisionGrid.SelectedRevision);
        RevisionInfo.Revision = RevisionGrid.SelectedRevision;
        rebaseToolStripMenuItem.IsEnabled =
            RevisionGrid.SelectedRevision is { IsArtificial: false }
            && !Module.IsBareRepository();

        if (CommitInfoTabControl.SelectedItem == TreeTabPage)
        {
            FillFileTree();
        }
    }

    private void RevisionGrid_FilterChanged(object? sender, FilterChangedEventArgs e)
    {
        string? path = e.PathFilter;
        if (path?.Length is > 1 && path[0] == '"' && path[^1] == '"')
        {
            path = path[1..^1];
        }

        if (!string.IsNullOrWhiteSpace(path))
        {
            RelativePath relativePath = RelativePath.From(path);
            revisionDiff.FallbackFollowedFile = relativePath;
            fileTree.FallbackFollowedFile = relativePath;
        }
    }

    private void CommitInfoTabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CommitInfoTabControl.SelectedItem == TreeTabPage)
        {
            FillFileTree();
            fileTree.SwitchFocus(alreadyContainedFocus: false);
        }
        else if (CommitInfoTabControl.SelectedItem == DiffTabPage)
        {
            revisionDiff.SwitchFocus(alreadyContainedFocus: false);
        }
        else if (CommitInfoTabControl.SelectedItem == GpgInfoTabPage)
        {
            FillGpgInfo();
            revisionGpgInfo1.FocusInfo();
        }
        else if (CommitInfoTabControl.SelectedItem == _consoleTabPage)
        {
            StartTerminal();
        }
    }

    private void FillFileTree()
    {
        if (RevisionGrid.SelectedRevision is not GitRevision revision
            || ReferenceEquals(_fileTreeRevision, revision))
        {
            return;
        }

        _fileTreeRevision = revision;
        fileTree.DisplayDiffTab([revision]);
    }

    internal void RefreshGpgInfo(GitRevision? revision)
    {
        _gpgInfoLoadSequence.CancelCurrent();
        _gpgInfoLoadVersion++;
        _gpgInfo = null;
        _gpgInfoLoadingRevision = null;
        _gpgInfoRevision = revision;
        _gpgInfoLoaded = false;
        revisionGpgInfo1.DisplayGpgInfo(null);

        bool showGpgInfoTab = revision?.IsArtificial is false && AppSettings.ShowGpgInformation.Value;
        GpgInfoTabPage.IsVisible = showGpgInfoTab;
        if (!showGpgInfoTab)
        {
            if (CommitInfoTabControl.SelectedItem == GpgInfoTabPage)
            {
                CommitInfoTabControl.SelectedItem = TreeTabPage;
            }

            return;
        }

        if (CommitInfoTabControl.SelectedItem == GpgInfoTabPage)
        {
            FillGpgInfo();
        }
    }

    private void FillGpgInfo()
    {
        if (!GpgInfoTabPage.IsVisible
            || CommitInfoTabControl.SelectedItem != GpgInfoTabPage
            || RevisionGrid.SelectedRevision is not GitRevision revision
            || revision.IsArtificial)
        {
            return;
        }

        if (_gpgInfoLoaded && ReferenceEquals(_gpgInfoRevision, revision))
        {
            revisionGpgInfo1.DisplayGpgInfo(_gpgInfo);
            return;
        }

        if (ReferenceEquals(_gpgInfoLoadingRevision, revision) || _controller is null)
        {
            return;
        }

        _gpgInfoLoadingRevision = revision;
        CancellationToken cancellationToken = _gpgInfoLoadSequence.Next();
        _loadOperations.FileAndForget(() => FillGpgInfoAsync(revision, cancellationToken));
    }

    private async Task FillGpgInfoAsync(GitRevision? revision, CancellationToken cancellationToken)
    {
        if (revision is null || _controller is null)
        {
            return;
        }

        int loadVersion = _gpgInfoLoadVersion;
        using CancellationTokenSource linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _loadOperationsCancellationTokenSource.Token);
        CancellationToken linkedCancellationToken = linkedCancellationTokenSource.Token;
        GpgInfo? info = await _controller.LoadGpgInfoAsync(revision).WaitAsync(linkedCancellationToken);
        await _loadOperations.JoinableTaskFactory.SwitchToMainThreadAsync(linkedCancellationToken);
        if (loadVersion != _gpgInfoLoadVersion
            || !ReferenceEquals(RevisionGrid.SelectedRevision, revision))
        {
            return;
        }

        _gpgInfo = info;
        _gpgInfoLoadingRevision = null;
        _gpgInfoRevision = revision;
        _gpgInfoLoaded = true;
        if (CommitInfoTabControl.SelectedItem == GpgInfoTabPage)
        {
            revisionGpgInfo1.DisplayGpgInfo(info);
        }
    }

    private void UICommands_PostRepositoryChanged(object? sender, GitUIEventArgs e)
    {
        CancellationToken cancellationToken = _loadOperationsCancellationTokenSource.Token;
        _loadOperations.FileAndForget(async () =>
        {
            await _loadOperations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            ReloadRepository();
        });
    }

    private void RefreshToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.RepoChangedNotifier.Notify();
    }

    private void CheckoutBranchToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartCheckoutBranch(this);
    }

    private void CommitToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartCommitDialog(this);
    }

    private void CreateBranchToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartCreateBranchDialog(this, RevisionGrid.SelectedRevision?.ObjectId ?? default);
    }

    private void DeleteBranchToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartDeleteBranchDialog(this, string.Empty);
    }

    private void PullToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartPullDialog(this);
    }

    private void ToolStripButtonPullClick(object? sender, EventArgs e)
    {
        GitPullAction action = AppSettings.DefaultPullAction == GitPullAction.None
            ? AppSettings.FormPullAction
            : AppSettings.DefaultPullAction;
        DoPull(action, isSilent: AppSettings.DefaultPullAction != GitPullAction.None);
    }

    private void DoPull(GitPullAction action, bool isSilent)
    {
        if (isSilent)
        {
            UICommands.StartPullDialogAndPullImmediately(this, pullAction: action);
        }
        else
        {
            UICommands.StartPullDialog(this, pullAction: action);
        }
    }

    private void SetDefaultPullAction(GitPullAction action)
    {
        AppSettings.DefaultPullAction = action;
        RefreshDefaultPullAction();
    }

    private void RefreshDefaultPullAction()
    {
        GitPullAction action = AppSettings.DefaultPullAction;
        defaultPullDialogToolStripMenuItem.IsChecked = action == GitPullAction.None;
        defaultPullMergeToolStripMenuItem.IsChecked = action == GitPullAction.Merge;
        defaultPullRebaseToolStripMenuItem.IsChecked = action == GitPullAction.Rebase;
        defaultPullFetchToolStripMenuItem.IsChecked = action == GitPullAction.Fetch;
        defaultPullFetchAllToolStripMenuItem.IsChecked = action == GitPullAction.FetchAll;
        defaultPullFetchPruneAllToolStripMenuItem.IsChecked = action == GitPullAction.FetchPruneAll;

        toolStripButtonPull.Icon = action switch
        {
            GitPullAction.Fetch => Properties.Images.PullFetch,
            GitPullAction.FetchAll => Properties.Images.PullFetchAll,
            GitPullAction.FetchPruneAll => Properties.Images.PullFetchPruneAll,
            GitPullAction.Rebase => Properties.Images.PullRebase,
            GitPullAction.Merge => Properties.Images.PullMerge,
            _ => Properties.Images.Pull,
        };
    }

    private void fetchAllToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        ArgumentString arguments = new GitArgumentBuilder("fetch")
        {
            "--all",
            "--progress",
        };

        if (UICommands.StartGitCommandProcessDialog(this, arguments))
        {
            UICommands.RepoChangedNotifier.Notify();
        }
    }

    private void StashToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartStashDialog(this);
    }

    private void PatchToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartViewPatchDialog(this);
    }

    private void FileExplorerToolStripMenuItemClick(object? sender, EventArgs e)
    {
        OsShellUtil.OpenWithFileExplorer(Module.WorkingDir);
    }

    private void MergeBranchToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartMergeBranchDialog(this, branch: null);
    }

    private void RebaseToolStripMenuItemClick(object? sender, EventArgs e)
    {
        if (RevisionGrid.SelectedRevision is not { IsArtificial: false } revision)
        {
            return;
        }

        UICommands.StartRebaseDialog(this, revision.ObjectId.ToString());
    }

    private void TagToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartCreateTagDialog(this, RevisionGrid.SelectedRevision);
    }

    private void DeleteTagToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartDeleteTagDialog(this, null);
    }

    private void userShell_Click(object? sender, EventArgs e)
    {
        try
        {
            UICommands.GetRequiredService<ITerminalLauncher>().Launch(Module.WorkingDir);
        }
        catch (Exception exception)
        {
            MessageBoxes.FailedToRunShell(this, "Git bash", exception);
        }
    }

    /// <summary>
    ///  Adds a tab with a console interface over the current working copy. Recreates the
    ///  terminal when the tab is activated again after the shell exits.
    /// </summary>
    private void FillTerminalTab()
    {
        if (!AppSettings.ShowConEmuTab.Value
            || _consoleEmulatorsRegistry is null
            || _consoleEmulatorsRegistry.AvailableConsoleEmulators.Count == 0
            || _consoleTabPage is not null)
        {
            return;
        }

        _consoleTabPage = new TabItem
        {
            Header = _consoleTabCaption.Text,
            Name = _consoleTabCaption.Text,
            Icon = Properties.Images.Console,
        };
        _consoleTabPage.Classes.Add("gitextensions-workspace-tab");
        CommitInfoTabControl.Items.Add(_consoleTabPage);
    }

    public void ChangeTerminalActiveFolder(string path)
    {
        if (_terminal?.IsShellRunning is true)
        {
            _terminal.ChangeWorkingDirectory(path);
        }
    }

    private void StartTerminal()
    {
        if (_consoleTabPage is null || _consoleEmulatorsRegistry is null)
        {
            return;
        }

        _terminal ??= _consoleEmulatorsRegistry.CreateShellRunner();
        if (_terminal is null)
        {
            return;
        }

        if (!ReferenceEquals(_consoleTabPage.Content, _terminal.Control))
        {
            _consoleTabPage.Content = _terminal.Control;
        }

        if (!_terminal.IsShellRunning)
        {
            _terminal.StartShell(Module.WorkingDir);
        }

        _terminal.FocusTerminal();
    }

    private void InitializeOutputHistory()
    {
        if (UICommands.GetService(typeof(IOutputHistoryProvider)) is not IOutputHistoryProvider outputHistoryProvider)
        {
            return;
        }

        OutputHistoryControl outputHistoryControl = new();
        _outputHistoryController = AppSettings.ShowOutputHistoryAsTab.Value
            ? new OutputHistoryTabController(
                outputHistoryProvider,
                outputHistoryControl,
                CommitInfoTabControl,
                _outputHistoryTabCaption.Text)
            : new OutputHistoryPanelController(
                outputHistoryProvider,
                outputHistoryControl,
                mainContentGrid,
                outputHistorySplitter,
                outputHistoryPanelHost);
    }

    protected override bool ExecuteCommand(int command)
    {
        switch ((Command)command)
        {
            case Command.GitBash: userShell_Click(this, EventArgs.Empty); break;
            case Command.FocusRevisionGrid: RevisionGrid.Focus(); break;
            case Command.FocusCommitInfo:
                if (AppSettings.CommitInfoPosition == CommitInfoPosition.BelowList)
                {
                    CommitInfoTabControl.SelectedItem = CommitInfoTabPage;
                }

                RevisionInfo.Focus();
                break;
            case Command.FocusDiff:
                bool diffAlreadyContainedFocus = revisionDiff.IsKeyboardFocusWithin;
                CommitInfoTabControl.SelectedItem = DiffTabPage;
                revisionDiff.SwitchFocus(diffAlreadyContainedFocus);
                break;
            case Command.FocusFileTree:
                bool fileTreeAlreadyContainedFocus = fileTree.IsKeyboardFocusWithin;
                CommitInfoTabControl.SelectedItem = TreeTabPage;
                fileTree.SwitchFocus(fileTreeAlreadyContainedFocus);
                break;
            case Command.FocusGpgInfo when GpgInfoTabPage.IsVisible:
                CommitInfoTabControl.SelectedItem = GpgInfoTabPage;
                revisionGpgInfo1.FocusInfo();
                break;
            case Command.FocusGitConsole:
                FillTerminalTab();
                if (_consoleTabPage is not null)
                {
                    CommitInfoTabControl.SelectedItem = _consoleTabPage;
                    StartTerminal();
                }

                break;
            case Command.FocusOutputHistoryAndToggleIfPanel:
                return _outputHistoryController?.FocusAndToggleIfPanel() ?? false;
            case Command.FocusFilter: ToolStripFilters.SetFocus(); break;
            case Command.ToggleLeftPanel: ToggleLeftPanelClick(this, EventArgs.Empty); break;
            case Command.OpenSettings: OnShowSettingsClick(this, EventArgs.Empty); break;
            case Command.FocusNextTab: FocusNextWorkspaceTab(forward: true); break;
            case Command.FocusPrevTab: FocusNextWorkspaceTab(forward: false); break;
            case Command.Refresh: RefreshToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.Commit: CommitToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.CheckoutBranch: CheckoutBranchToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.PullOrFetch: PullToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.Push: UICommands.StartPushDialog(this, pushOnShow: false); break;
            case Command.CreateBranch: CreateBranchToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.MergeBranches: MergeBranchToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.CreateTag: TagToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.Rebase: RebaseToolStripMenuItemClick(this, EventArgs.Empty); break;
            default: return base.ExecuteCommand(command);
        }

        return true;
    }

    internal bool ExecuteCommand(Command command)
        => ExecuteCommand((int)command);

    private void OnShowSettingsClick(object? sender, EventArgs e)
    {
        string translation = AppSettings.Translation;
        CommitInfoPosition commitInfoPosition = AppSettings.CommitInfoPosition;

        _loadOperations.JoinPendingOperations();
        UICommands.StartSettingsDialog(this);
        Module.InvalidateGitSettings();

        if (translation != AppSettings.Translation)
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
            AvaloniaTranslationUtils.RemoveTextBlockMnemonicMarkers(this);
        }

        if (commitInfoPosition != AppSettings.CommitInfoPosition)
        {
            RefreshWorkspaceLayout(refreshCommitInfoPositionToolTip: true);
        }

        LoadHotkeys(HotkeySettingsName);
        AvatarService.UpdateAvatarInitialFontsSettings();
        RevisionGrid.ApplyColumnSettings();
        RevisionGrid.RefreshRealizedRows();
        RevisionInfo.Revision = RevisionGrid.SelectedRevision;
        RefreshDefaultPullAction();
    }

    /// <summary>
    /// Set the path filter.
    /// </summary>
    /// <param name="pathFilter">Zero or more quoted paths, separated by spaces.</param>
    public void SetPathFilter(string pathFilter)
    {
        RevisionGrid.SetAndApplyPathFilter(pathFilter);
    }

    private void FocusNextWorkspaceTab(bool forward)
    {
        Control[] tabs =
        [
            .. CommitInfoTabControl.Items
                .OfType<Control>()
                .Where(tab => tab.IsVisible),
        ];
        if (tabs.Length == 0)
        {
            return;
        }

        int selectedIndex = Array.IndexOf(tabs, CommitInfoTabControl.SelectedItem);
        if (selectedIndex < 0)
        {
            selectedIndex = forward ? -1 : 0;
        }

        int offset = forward ? 1 : -1;
        CommitInfoTabControl.SelectedItem = tabs[(selectedIndex + offset + tabs.Length) % tabs.Length];
    }

    public override bool ProcessHotkey(WinFormsShims.Keys keyData)
    {
        if (base.ProcessHotkey(keyData))
        {
            return true;
        }

        object? focused = FocusManager?.GetFocusedElement();
        if (focused is TextBox && GitExtensionsControl.IsTextEditKey(keyData, multiLine: true))
        {
            return false;
        }

        return keyData != (WinFormsShims.Keys.Control | WinFormsShims.Keys.A)
            && RevisionGrid.ProcessHotkey(keyData);
    }

    protected override bool CloseOnEscape => false;

    protected override void OnClosed(EventArgs e)
    {
        if (_hasRuntimeCommands)
        {
            UICommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
        }

        _loadOperationsCancellationTokenSource.Cancel();
        _splitterManager?.SaveSplitters();
        _gpgInfoLoadSequence.Dispose();
        RevisionGrid.CancelBackgroundTasks();
        revisionDiff.CancelBackgroundTasks();
        fileTree.CancelBackgroundTasks();
        _loadOperations.JoinPendingOperations();
        _loadOperationsCancellationTokenSource.Dispose();
        (_terminal as IDisposable)?.Dispose();
        _terminal = null;
        _outputHistoryController?.Dispose();
        _outputHistoryController = null;
        base.OnClosed(e);
    }

    internal FileStatusList fileStatusList => revisionDiff.FileStatusList;
    internal Editor.FileViewer fileViewer => revisionDiff.FileViewer;

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(nameof(FormBrowse), nameof(RefreshButton), "ToolTipText", "Refresh");
        translation.AddTranslationItem(nameof(FormBrowse), nameof(toggleLeftPanel), "ToolTipText", "Toggle left panel");
        translation.AddTranslationItem(nameof(FormBrowse), nameof(toggleSplitViewLayout), "ToolTipText", "Toggle split view layout");
        translation.AddTranslationItem(nameof(FormBrowse), nameof(menuCommitInfoPosition), "ToolTipText", "Commit info position");
        translation.AddTranslationItem(nameof(FormBrowse), nameof(branchSelect), "ToolTipText", "Change current branch");
        translation.AddTranslationItem(nameof(FormBrowse), nameof(toolStripSplitStash), "ToolTipText", "Manage stashes");
        translation.AddTranslationItem(nameof(FormBrowse), nameof(toolStripFileExplorer), "ToolTipText", "File Explorer");
        translation.AddTranslationItem(nameof(FormBrowse), nameof(userShell), "ToolTipText", "Git bash");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        SetTranslatedToolTip(RefreshButton, nameof(RefreshButton), "Refresh");
        SetTranslatedToolTip(toggleLeftPanel, nameof(toggleLeftPanel), "Toggle left panel");
        SetTranslatedToolTip(toggleSplitViewLayout, nameof(toggleSplitViewLayout), "Toggle split view layout");
        SetTranslatedToolTip(menuCommitInfoPosition, nameof(menuCommitInfoPosition), "Commit info position");
        SetTranslatedToolTip(branchSelect, nameof(branchSelect), "Change current branch");
        SetTranslatedToolTip(toolStripSplitStash, nameof(toolStripSplitStash), "Manage stashes");
        SetTranslatedToolTip(toolStripFileExplorer, nameof(toolStripFileExplorer), "File Explorer");
        string terminalText = SetTranslatedToolTip(userShell, nameof(userShell), "Git bash");
        FetchAllToolbarMenuItem.Header = fetchAllToolStripMenuItem.Header;
        if (UserShellToolStripMenuItem.Header is TextBlock header)
        {
            header.Text = terminalText;
        }

        RefreshCommitInfoPositionToolTip();

        return;

        string SetTranslatedToolTip(Control control, string name, string source)
        {
            string translated = translation.TranslateItem(
                nameof(FormBrowse),
                name,
                "ToolTipText",
                () => source) ?? source;
            ToolTip.SetTip(control, translated);
            return translated;
        }
    }
}
