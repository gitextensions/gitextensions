using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Gpg;
using GitCommands.Remotes;
using GitCommands.Submodules;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.Avatars;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.CommandsDialogs.Menus;
using GitUI.CommandsDialogs.WorktreeDialog;
using GitUI.Compat;
using GitUI.ConsoleEmulation;
using GitUI.HelperDialogs;
using GitUI.Models;
using GitUI.Properties;
using GitUI.ScriptsEngine;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;

using ResourceManager;
using ResourceManager.CommitDataRenders;
using ResourceManager.Hotkey;
using Keys = GitExtensions.Shims.WinForms.Keys;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public sealed partial class FormBrowse : GitModuleForm, IBrowseRepo
{
    private readonly TranslationString _noSubmodulesPresent = new("No submodules");
    private readonly TranslationString _topProjectModuleFormat = new("Top project: {0}");
    private readonly TranslationString _superprojectModuleFormat = new("Superproject: {0}");
    private readonly TranslationString _goToSuperProject = new("Go to superproject");
    private readonly TranslationString _indexLockCantDelete = new("Failed to delete index.lock");
    private readonly TranslationString _loading = new("Loading...");
    private readonly TranslationString _noReposHostPluginLoaded = new("No repository host plugin loaded.");
    private readonly TranslationString _noReposHostFound = new("Could not find any relevant repository hosts for the currently open repository.");
    private readonly TranslationString _updateCurrentSubmodule = new("Update current submodule");
    private readonly TranslationString _pullFetch = new("Fetch");
    private readonly TranslationString _pullFetchAll = new("Fetch all");
    private readonly TranslationString _pullFetchPruneAll = new("Fetch and prune all");
    private readonly TranslationString _pullMerge = new("Pull - merge");
    private readonly TranslationString _pullRebase = new("Pull - rebase");
    private readonly TranslationString _pullOpenDialog = new("Open pull dialog");
    private readonly TranslationString _buildReportTabCaption = new("Build Report");
    private readonly TranslationString _consoleTabCaption = new("Console");
    private readonly TranslationString _outputHistoryTabCaption = new("Output");
    private readonly TranslationString _commitButtonText = new("Commit");
    private readonly TranslationString _undoLastCommitText = new("You will still be able to find all the commit's changes in the staging area\n\nDo you want to continue?");
    private readonly TranslationString _undoLastCommitCaption = new("Undo last commit");

    // Avalonia's designer constructs views before the application initializes ThreadHelper.
    private readonly TaskManager _loadOperations = GitUI.Compat.DesignTimeTaskManager.Create();
    private readonly SplitterManager? _splitterManager;
    private readonly GitStatusMonitor? _gitStatusMonitor;
    private FormBrowseMenus? _formBrowseMenus;
    private readonly IGpgInfoProvider? _controller;
    private readonly IUpdateCheckService? _updateCheckService;
    private readonly ICommitDataManager _commitDataManager;
    private readonly CancellationTokenSequence _gpgInfoLoadSequence = new();
    private readonly CancellationTokenSource _loadOperationsCancellationTokenSource = new();

    private readonly IAheadBehindDataProvider? _aheadBehindDataProvider;
    private readonly ISubmoduleStatusProvider? _submoduleStatusProvider;
    private readonly IScriptsManager? _scriptsManager;
    private List<MenuItem>? _currentSubmoduleMenuItems;
    private GridLength _commitInfoWidth = new(490);
    private GpgInfo? _gpgInfo;
    private GitRevision? _gpgInfoLoadingRevision;
    private GitRevision? _gpgInfoRevision;
    private GridLength _leftPanelWidth = new(260);
    private GridLength _splitViewBottomHeight = new(1, GridUnitType.Star);
    private GridLength _splitViewTopHeight = new(211);
    private bool _gpgInfoLoaded;
    private bool _hasRuntimeCommands;
    private int _gpgInfoLoadVersion;
    private IReadOnlyList<GitWorktree> _worktrees = [];
    private readonly IRepositoryHistoryUIService? _repositoryHistoryUIService;
    private readonly IConsoleEmulatorsRegistry? _consoleEmulatorsRegistry;
    private BuildReportTabPageExtension? _buildReportTabPageExtension;
    private IConsoleShellRunner? _terminal;
    private Dashboard? _dashboard;
    private TabItem? _consoleTabPage;
    private OutputHistoryControllerBase? _outputHistoryController;

    private UpdateTargets _selectedRevisionUpdatedTargets = UpdateTargets.None;

    public RevisionGridControl RevisionGridControl => RevisionGrid;

    public FormBrowse()
    {
        _commitDataManager = new CommitDataManager(() => Module);
        _commitDataManager.RevisionDetailsLoaded += (_, _) => RevisionGrid.InvalidateVisual();
        InitializeComponent();
        ApplySourceToolbarAutoSize();
        _formBrowseMenus = new FormBrowseMenus(mainMenuStrip, RevisionGrid, repositoryToolStripMenuItem);
        InitializeWorkspaceLayout();
        InitializeToolbarOverflow();
        InitializeComplete();
        InitMenusAndToolbars(revFilter: null, pathFilter: null);
        InitializeToolbarsMenus();
    }

    public FormBrowse(IGitUICommands commands)
        : this(commands, new BrowseArguments())
    {
    }

    /// <summary>
    /// Open Browse - main GUI including dashboard.
    /// </summary>
    /// <param name="commands">The commands in the current form.</param>
    /// <param name="args">The start up arguments.</param>
    public FormBrowse(IGitUICommands commands, BrowseArguments args)
        : this(commands, args, gpgInfoProvider: null)

    // Type or member is obsolete
    {
    }

    internal FormBrowse(IGitUICommands commands, IGpgInfoProvider? gpgInfoProvider)
        : this(commands, new BrowseArguments(), gpgInfoProvider)
    {
    }

    private FormBrowse(IGitUICommands commands, BrowseArguments args, IGpgInfoProvider? gpgInfoProvider)
        : base(commands, enablePositionRestore: true)
    {
        _commitDataManager = new CommitDataManager(() => Module);
        _commitDataManager.RevisionDetailsLoaded += (_, _) => RevisionGrid.InvalidateVisual();
        InitializeComponent();
        ApplySourceToolbarAutoSize();
        _formBrowseMenus = new FormBrowseMenus(mainMenuStrip, RevisionGrid, repositoryToolStripMenuItem);

        _hasRuntimeCommands = true;
        _scriptsManager = UICommands.GetService(typeof(IScriptsManager)) as IScriptsManager;
        _submoduleStatusProvider = UICommands.GetService(typeof(ISubmoduleStatusProvider)) as ISubmoduleStatusProvider;
        if (_submoduleStatusProvider is not null)
        {
            _submoduleStatusProvider.StatusUpdating += SubmoduleStatusProvider_StatusUpdating;
            _submoduleStatusProvider.StatusUpdated += SubmoduleStatusProvider_StatusUpdated;
        }

        _updateCheckService = UICommands.GetService(typeof(IUpdateCheckService)) as IUpdateCheckService;
        _repositoryHistoryUIService = UICommands.GetService(typeof(IRepositoryHistoryUIService)) as IRepositoryHistoryUIService;
        fileToolStripMenuItem.Initialize(() => UICommands);
        fileToolStripMenuItem.GitModuleChanged += (_, e) => ChangeWorkingDirectory(e.GitModule.WorkingDir);
        fileToolStripMenuItem.RecentRepositoriesCleared += fileToolStripMenuItem_RecentRepositoriesCleared;
        helpToolStripMenuItem.Initialize(() => UICommands);
        toolsToolStripMenuItem.Initialize(() => UICommands);
        toolsToolStripMenuItem.SettingsChanged += toolsToolStripMenuItem_SettingsChanged;
        RevisionGrid.UICommandsSource = this;
        RevisionInfo.UICommandsSource = this;
        RevisionGrid.ShowBuildServerInfo = true;
        revisionDiff.UICommandsSource = this;
        fileTree.UICommandsSource = this;
        repoObjectsTree.UICommandsSource = this;
        notificationBarBisectInProgress.UICommandsSource = this;
        notificationBarGitActionInProgress.UICommandsSource = this;
        Activated += (_, _) => Dispatcher.UIThread.Post(OnActivate);

        _consoleEmulatorsRegistry = UICommands.GetService(typeof(IConsoleEmulatorsRegistry)) as IConsoleEmulatorsRegistry;
        _controller = gpgInfoProvider ?? new GpgInfoProvider(new GitGpgController(() => Module));
        _aheadBehindDataProvider = new AheadBehindDataProvider(() => Module.GitExecutable);
        RevisionGrid.SetAheadBehindDataProvider(_aheadBehindDataProvider);
        repoObjectsTree.Initialize(_aheadBehindDataProvider, RevisionGrid.SetAndApplyBranchFilter, RevisionGrid, RevisionGrid);
        repoObjectsTree.Initialize(RevisionGrid.SetAndApplyBranchFilter, OpenRepository);
        ToolStripFilters.Bind(() => Module, RevisionGrid);
        revisionDiff.Bind(RevisionGrid, RevisionGrid, fileTree, () => string.Empty, RefreshGitStatusMonitor);
        fileTree.Bind(RevisionGrid, RevisionGrid, revisionFileTree: null, () => string.Empty, RefreshGitStatusMonitor);
        _splitterManager = new SplitterManager(new AppSettingsPath("FormBrowse.Avalonia"));
        revisionDiff.InitSplitterManager(_splitterManager);
        fileTree.InitSplitterManager(_splitterManager);
        _splitterManager.RestoreSplitters();
        InitRevisionGrid(args.SelectedId, args.FirstId, args.IsFileHistoryMode);
        InitCommitDetails();
        CommitInfoTabControl.SelectionChanged += CommitInfoTabControl_SelectedIndexChanged;
        repoObjectsTree.NodeSelectionChanged += RepoObjectsTree_SelectionChanged;
        refreshToolStripMenuItem.Click += RefreshToolStripMenuItemClick;
        refreshDashboardToolStripMenuItem.Click += RefreshDashboardToolStripMenuItemClick;
        fileExplorerToolStripMenuItem.Click += FileExplorerToolStripMenuItemClick;
        manageRemoteRepositoriesToolStripMenuItem1.Click += ManageRemoteRepositoriesToolStripMenuItemClick;
        manageSubmodulesToolStripMenuItem.Click += ManageSubmodulesToolStripMenuItemClick;
        updateAllSubmodulesToolStripMenuItem.Click += UpdateAllSubmodulesToolStripMenuItemClick;
        synchronizeAllSubmodulesToolStripMenuItem.Click += SynchronizeAllSubmodulesToolStripMenuItemClick;
        manageWorktreeToolStripMenuItem.Click += manageWorktreeToolStripMenuItem_Click;
        compressGitDatabaseToolStripMenuItem.Click += CompressGitDatabaseToolStripMenuItemClick;
        recoverLostObjectsToolStripMenuItem.Click += recoverLostObjectsToolStripMenuItemClick;
        deleteIndexLockToolStripMenuItem.Click += deleteIndexLockToolStripMenuItem_Click;
        editLocalGitConfigToolStripMenuItem.Click += EditLocalGitConfigToolStripMenuItemClick;
        repoSettingsToolStripMenuItem.Click += RepoSettingsToolStripMenuItemClick;
        editgitignoreToolStripMenuItem1.Click += EditGitignoreToolStripMenuItem1Click;
        editgitinfoexcludeToolStripMenuItem.Click += EditGitInfoExcludeToolStripMenuItemClick;
        editGitAttributesToolStripMenuItem.Click += editGitAttributesToolStripMenuItem_Click;
        editmailmapToolStripMenuItem.Click += EditMailMapToolStripMenuItemClick;
        menuitemSparse.Click += menuitemSparseWorkingCopy_Click;
        closeToolStripMenuItem.Click += CloseToolStripMenuItemClick;
        commitToolStripMenuItem.Click += CommitToolStripMenuItemClick;
        undoLastCommitToolStripMenuItem.Click += undoLastCommitToolStripMenuItem_Click;
        pushToolStripMenuItem.Click += PushToolStripMenuItemClick;
        resetToolStripMenuItem.Click += ResetToolStripMenuItem_Click;
        cleanupToolStripMenuItem.Click += CleanupToolStripMenuItemClick;
        checkoutBranchToolStripMenuItem.Click += CheckoutBranchToolStripMenuItemClick;
        branchToolStripMenuItem.Click += CreateBranchToolStripMenuItemClick;
        deleteBranchToolStripMenuItem.Click += DeleteBranchToolStripMenuItemClick;
        pullToolStripMenuItem.Click += PullToolStripMenuItemClick;
        mergeBranchToolStripMenuItem.Click += MergeBranchToolStripMenuItemClick;
        rebaseToolStripMenuItem.Click += RebaseToolStripMenuItemClick;
        runMergetoolToolStripMenuItem.Click += RunMergetoolToolStripMenuItemClick;
        tagToolStripMenuItem.Click += TagToolStripMenuItemClick;
        deleteTagToolStripMenuItem.Click += DeleteTagToolStripMenuItemClick;
        cherryPickToolStripMenuItem.Click += CherryPickToolStripMenuItemClick;
        archiveToolStripMenuItem.Click += ArchiveToolStripMenuItemClick;
        checkoutToolStripMenuItem.Click += CheckoutToolStripMenuItemClick;
        bisectToolStripMenuItem.Click += BisectClick;
        stashToolStripMenuItem.Click += StashToolStripMenuItemClick;
        toolStripMenuItemReflog.Click += toolStripMenuItemReflog_Click;
        formatPatchToolStripMenuItem.Click += FormatPatchToolStripMenuItemClick;
        applyPatchToolStripMenuItem.Click += ApplyPatchToolStripMenuItemClick;
        patchToolStripMenuItem.Click += PatchToolStripMenuItemClick;
        _forkCloneRepositoryToolStripMenuItem.Click += _forkCloneMenuItem_Click;
        _viewPullRequestsToolStripMenuItem.Click += _viewPullRequestsToolStripMenuItem_Click;
        _createPullRequestsToolStripMenuItem.Click += _createPullRequestToolStripMenuItem_Click;
        _addUpstreamRemoteToolStripMenuItem.Click += _addUpstreamRemoteToolStripMenuItem_Click;
        pluginSettingsToolStripMenuItem.Click += PluginSettingsToolStripMenuItemClick;
        RefreshButton.Click += RefreshToolStripMenuItemClick;
        toggleLeftPanel.Click += toggleLeftPanel_Click;
        InitializeWorkspaceLayout();
        InitializeOutputHistory();
        branchSelect.Click += CurrentBranchClick;
        branchSelect.AddHandler(
            PointerPressedEvent,
            BranchSelectPointerPressed,
            RoutingStrategies.Tunnel);
        branchSelect.AddHandler(
            PointerReleasedEvent,
            BranchSelectPointerReleased,
            RoutingStrategies.Tunnel);
        branchSelect.AddHandler(
            KeyDownEvent,
            BranchSelectKeyDown,
            RoutingStrategies.Tunnel);
        toolStripWorktrees.Click += manageWorktreeToolStripMenuItem_Click;
        WorktreeFlyout.Opening += (_, _) => PopulateWorktreeSelector();
        toolStripButtonLevelUp.Click += toolStripButtonLevelUp_ButtonClick;
        InitializeToolbarOverflow();
        toolStripButtonPull.Click += ToolStripButtonPullClick;
        toolStripButtonPush.Click += ToolStripButtonPushClick;
        toolStripButtonCommit.Click += CommitToolStripMenuItemClick;
        toolStripSplitStash.Click += ToolStripSplitStashButtonClick;
        toolStripFileExplorer.Click += FileExplorerToolStripMenuItemClick;
        userShell.Click += userShell_Click;
        EditSettings.Click += OnShowSettingsClick;
        pullToolStripMenuItem1.Click += pullToolStripMenuItem1_Click;
        mergeToolStripMenuItem.Click += mergeToolStripMenuItem_Click;
        rebaseToolStripMenuItem1.Click += rebaseToolStripMenuItem1_Click;
        fetchToolStripMenuItem.Click += fetchToolStripMenuItem_Click;
        fetchAllToolStripMenuItem.Click += fetchAllToolStripMenuItem_Click;
        fetchPruneAllToolStripMenuItem.Click += fetchPruneAllToolStripMenuItem_Click;
        stashChangesToolStripMenuItem.Click += StashChangesToolStripMenuItemClick;
        stashStagedToolStripMenuItem.Click += StashStagedToolStripMenuItemClick;
        stashPopToolStripMenuItem.Click += StashPopToolStripMenuItemClick;
        manageStashesToolStripMenuItem.Click += ManageStashesToolStripMenuItemClick;
        createAStashToolStripMenuItem.Click += CreateStashToolStripMenuItemClick;

        // The toolstrip and menu items must be initialised after InitializeComplete
        // which invokes the translation logic and applies the current language to the components.
        _NO_TRANSLATE_WorkingDir.Initialize(
            () => UICommands,
            _repositoryHistoryUIService
                ?? throw new InvalidOperationException($"{nameof(IRepositoryHistoryUIService)} is not registered."),
            ChangeWorkingDirectory,
            path => GitUICommands.LaunchBrowse(path),
            OpenRepositoryDialog,
            () => ChangeWorkingDirectory(string.Empty),
            ConfigureRecentRepositories);
        UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
        _gitStatusMonitor = new GitStatusMonitor(this, () => WindowState == WindowState.Minimized);
        _gitStatusMonitor.GitStatusMonitorStateChanged += GitStatusMonitorStateChanged;
        _gitStatusMonitor.GitWorkingDirectoryStatusChanged += GitWorkingDirectoryStatusChanged;

        if (args.IsFileHistoryMode)
        {
            toggleLeftPanel_Click(this, EventArgs.Empty);
        }

        InitializeComplete();
        HotkeysEnabled = true;
        LoadHotkeys(HotkeySettingsName);
        RefreshMenuShortcutKeys();
        _NO_TRANSLATE_WorkingDir.RefreshShortcutKeys(Hotkeys);
        ToolStripFilters.RefreshBrowseDialogShortcutKeys(Hotkeys ?? []);
        IReadOnlyList<HotkeyCommand> revisionGridHotkeys = UICommands
            .GetRequiredService<IHotkeySettingsLoader>()
            .LoadHotkeys(RevisionGridControl.HotkeySettingsName);
        ToolStripFilters.RefreshRevisionGridShortcutKeys(revisionGridHotkeys);
        RevisionGrid.RefreshMenuShortcutKeys(revisionGridHotkeys);
        InitMenusAndToolbars(args.RevFilter, args.PathFilter.ToPosixPath());
        InitializeToolbarsMenus();
        LoadUserMenu();
        ReloadRepository();
    }

    private void LoadUserMenu()
    {
        ReloadScriptHotkeys();
        ToolStripScripts.Children.Clear();
        if (_scriptsManager is null)
        {
            ToolStripScripts.IsVisible = true;
            return;
        }

        IReadOnlyList<HotkeyCommand> hotkeys = UICommands
            .GetRequiredService<IHotkeySettingsLoader>()
            .LoadHotkeys(FormSettings.HotkeySettingsName);
        foreach (ScriptInfo script in _scriptsManager.GetScripts()
                     .Where(script => script.Enabled && script.OnEvent == ScriptEvent.ShowInUserMenuBar))
        {
            IconButton button = new()
            {
                Content = script.Name,
                Icon = script.GetIcon(),
                Tag = "userscript",
            };
            button.Classes.Add("gitextensions-toolbar-button");
            KeyGesture? gesture = KeysMapper.ToKeyGesture(
                hotkeys.FirstOrDefault(hotkey => hotkey.Name == script.GetDisplayName())?.KeyData);
            ToolTip.SetTip(button, gesture is null ? script.Name : $"{script.Name} ({gesture})");
            button.Click += (_, _) => ExecuteCommand(script.HotkeyCommandIdentifier);
            ToolStripScripts.Children.Add(button);
        }

        ToolStripScripts.IsVisible = true;
    }

    private static void ToggleToolbarOverflow(Control content, ScrollViewer viewport, Button overflowButton)
    {
        double maximumOffset = Math.Max(0, content.Bounds.Width - viewport.Bounds.Width);
        bool returnToStart = content.RenderTransform is TranslateTransform { X: < 0 };
        content.RenderTransform = new TranslateTransform(returnToStart ? 0 : -maximumOffset, 0);
        overflowButton.Content = returnToStart ? "»" : "«";
    }

    private void InitializeToolbarOverflow()
    {
        toolStripMainOverflow.Click += (_, _) => ToggleToolbarOverflow(ToolStripMain, toolStripMainViewport, toolStripMainOverflow);
        toolStripFiltersOverflow.Click += (_, _) => ToggleToolbarOverflow(ToolStripFilters, toolStripFiltersViewport, toolStripFiltersOverflow);
        toolStripMainViewport.SizeChanged += (_, _) => UpdateToolbarOverflow(ToolStripMain, toolStripMainViewport, toolStripMainOverflow);
        toolStripFiltersViewport.SizeChanged += (_, _) => UpdateToolbarOverflow(ToolStripFilters, toolStripFiltersViewport, toolStripFiltersOverflow);
        ToolStripMain.LayoutUpdated += (_, _) => UpdateToolbarOverflow(ToolStripMain, toolStripMainViewport, toolStripMainOverflow);
        ToolStripFilters.LayoutUpdated += (_, _) => UpdateToolbarOverflow(ToolStripFilters, toolStripFiltersViewport, toolStripFiltersOverflow);
    }

    private static void UpdateToolbarOverflow(Control content, ScrollViewer viewport, Button overflowButton)
    {
        bool hasOverflow = content.Bounds.Width > viewport.Bounds.Width;
        overflowButton.IsVisible = hasOverflow;
        if (!hasOverflow && content.RenderTransform is TranslateTransform { X: not 0 })
        {
            content.RenderTransform = new TranslateTransform(0, 0);
            overflowButton.Content = "»";
        }
    }

    public FormBrowse(IServiceProvider serviceProvider, GitModule module)
        : this(new GitUICommands(serviceProvider, module))
    {
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(nameof(FormBrowse), nameof(RefreshButton), "ToolTipText", "Refresh");
        translation.AddTranslationItem(nameof(FormBrowse), nameof(toggleLeftPanel), "ToolTipText", "Toggle left panel");
        translation.AddTranslationItem(nameof(FormBrowse), nameof(toggleSplitViewLayout), "ToolTipText", "Toggle split view layout");
        translation.AddTranslationItem(nameof(FormBrowse), nameof(menuCommitInfoPosition), "ToolTipText", "Commit info position");
        translation.AddTranslationItem(nameof(FormBrowse), nameof(branchSelect), "ToolTipText", "Change current branch");
        translation.AddTranslationItem(nameof(FormBrowse), nameof(toolStripButtonLevelUp), "ToolTipText", "Submodules");
        translation.AddTranslationItem(nameof(FormBrowse), nameof(toolStripSplitStash), "ToolTipText", "Manage stashes");
        translation.AddTranslationItem(nameof(FormBrowse), nameof(toolStripWorktrees), "ToolTipText", "Worktrees");
        translation.AddTranslationItem(nameof(FormBrowse), nameof(toolStripFileExplorer), "ToolTipText", "File Explorer");
        translation.AddTranslationItem(nameof(FormBrowse), nameof(userShell), "ToolTipText", "Git bash");
        fileToolStripMenuItem.AddControlTranslationItems(translation);
        toolsToolStripMenuItem.AddControlTranslationItems(translation);
        helpToolStripMenuItem.AddControlTranslationItems(translation);
        _formBrowseMenus?.AddTranslationItems(translation);
        _NO_TRANSLATE_WorkingDir.AddControlTranslationItems(translation);
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        SetTranslatedToolTip(RefreshButton, nameof(RefreshButton), "Refresh");
        SetTranslatedToolTip(toggleLeftPanel, nameof(toggleLeftPanel), "Toggle left panel");
        SetTranslatedToolTip(toggleSplitViewLayout, nameof(toggleSplitViewLayout), "Toggle split view layout");
        SetTranslatedToolTip(menuCommitInfoPosition, nameof(menuCommitInfoPosition), "Commit info position");
        SetTranslatedToolTip(branchSelect, nameof(branchSelect), "Change current branch");
        SetTranslatedToolTip(toolStripButtonLevelUp, nameof(toolStripButtonLevelUp), "Submodules");
        SetTranslatedToolTip(toolStripSplitStash, nameof(toolStripSplitStash), "Manage stashes");
        SetTranslatedToolTip(toolStripWorktrees, nameof(toolStripWorktrees), "Worktrees");
        SetTranslatedToolTip(toolStripFileExplorer, nameof(toolStripFileExplorer), "File Explorer");
        SetTranslatedToolTip(userShell, nameof(userShell), "Git bash");
        fileToolStripMenuItem.TranslateControlItems(translation);
        toolsToolStripMenuItem.TranslateControlItems(translation);
        helpToolStripMenuItem.TranslateControlItems(translation);
        _formBrowseMenus?.TranslateItems(translation);
        _NO_TRANSLATE_WorkingDir.TranslateControlItems(translation);

        RefreshCommitInfoPositionToolTip();
        UpdateTooltipWithShortcut(toolStripButtonPull, Command.QuickPullOrFetch);
        UpdateTooltipWithShortcut(toolStripFileExplorer, fileExplorerToolStripMenuItem.InputGesture);

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

    public override void CancelButtonClick(object? sender, EventArgs e)
    {
        // If a filter is applied, clear it
        if (RevisionGrid.FilterIsApplied())
        {
            // Clear filter
            ToolStripFilters.SetRevisionFilter(string.Empty);
        }
    }

    private void ReloadRepository()
    {
        IGitModule module = Module;
        RevisionGrid.OnRepositoryChanged();

        bool isValidWorkingDir = module.IsValidGitWorkingDir();
        string branchName = isValidWorkingDir ? module.GetSelectedBranch() : string.Empty;

        IAppTitleGenerator appTitleGenerator = UICommands.GetRequiredService<IAppTitleGenerator>();
        Title = appTitleGenerator.Generate(module.WorkingDir, isValidWorkingDir, branchName);

        refreshToolStripMenuItem.IsEnabled = isValidWorkingDir;
        repositoryToolStripMenuItem.IsVisible = isValidWorkingDir;
        dashboardToolStripMenuItem.IsVisible = !isValidWorkingDir;
        _formBrowseMenus?.SetVisible(isValidWorkingDir);
        commandsToolStripMenuItem.IsVisible = isValidWorkingDir;
        fileExplorerToolStripMenuItem.IsEnabled = isValidWorkingDir;
        manageRemoteRepositoriesToolStripMenuItem1.IsEnabled = isValidWorkingDir;
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
        archiveToolStripMenuItem.IsEnabled = false;
        stashToolStripMenuItem.IsEnabled = isValidWorkingDir && !module.IsBareRepository();
        toolStripMenuItemReflog.IsEnabled = isValidWorkingDir && !module.IsBareRepository();
        manageWorktreeToolStripMenuItem.IsEnabled = isValidWorkingDir;
        gitMaintenanceToolStripMenuItem.IsEnabled = isValidWorkingDir;
        repoSettingsToolStripMenuItem.IsEnabled = isValidWorkingDir;
        editgitignoreToolStripMenuItem1.IsEnabled = isValidWorkingDir && !module.IsBareRepository();
        editgitinfoexcludeToolStripMenuItem.IsEnabled = isValidWorkingDir;
        editGitAttributesToolStripMenuItem.IsEnabled = isValidWorkingDir && !module.IsBareRepository();
        editmailmapToolStripMenuItem.IsEnabled = isValidWorkingDir && !module.IsBareRepository();
        menuitemSparse.IsEnabled = isValidWorkingDir && !module.IsBareRepository();
        bool enableWorkingTreeCommands = isValidWorkingDir && !module.IsBareRepository();
        manageSubmodulesToolStripMenuItem.IsEnabled = enableWorkingTreeCommands;
        updateAllSubmodulesToolStripMenuItem.IsEnabled = enableWorkingTreeCommands;
        synchronizeAllSubmodulesToolStripMenuItem.IsEnabled = enableWorkingTreeCommands;
        RefreshButton.IsEnabled = isValidWorkingDir;
        branchSelect.IsEnabled = isValidWorkingDir;
        toolStripButtonPull.IsEnabled = isValidWorkingDir;
        toolStripButtonPush.IsEnabled = isValidWorkingDir;
        toolStripButtonPush.ResetBeforeUpdate();
        toolStripButtonCommit.IsEnabled = isValidWorkingDir && !module.IsBareRepository();
        toolStripSplitStash.IsEnabled = isValidWorkingDir && !module.IsBareRepository();
        toolStripFileExplorer.IsEnabled = Directory.Exists(module.WorkingDir);
        userShell.IsEnabled = Directory.Exists(module.WorkingDir);
        ToolStripFilters.IsEnabled = isValidWorkingDir;
        branchSelect.Content = string.IsNullOrEmpty(branchName) ? "Branch" : branchName;
        pluginsToolStripMenuItem.IsVisible = isValidWorkingDir;
        UpdateRepositoryHostsMenu();
        UpdatePluginMenu(isValidWorkingDir);
        RefreshDefaultPullAction();

        if (isValidWorkingDir)
        {
            ShowRepository();
            _NO_TRANSLATE_WorkingDir.RefreshContent();
            _aheadBehindDataProvider?.ResetCache();
            lblRepoPath.Text = $"{module.WorkingDir}  —  {branchName}";
            lblStatus.Text = $"git: {GitVersion.Current}";
            RevisionGrid.ReloadRevisions(module, selectedObjectId: RevisionGrid.SelectedId);
            UpdateSubmodulesStructure();
            RefreshPushButton(module, branchName);
        }
        else
        {
            ShowDashboard();
            _worktrees = [];
            toolStripWorktrees.IsVisible = false;
            _NO_TRANSLATE_WorkingDir.RefreshContent();
            lblRepoPath.Text = "No git repository";
            lblStatus.Text = "Start the app inside a repository or pass one on the command line: GitExtensions.Avalonia browse <path>";
            toolStripButtonPush.ResetToDefaultState();
        }

        _gitStatusMonitor?.Active = isValidWorkingDir && NeedsGitStatusMonitor();
        UpdateStashCount();
    }

    private static bool NeedsGitStatusMonitor()
        => AppSettings.ShowGitStatusInBrowseToolbar
            || (AppSettings.ShowGitStatusForArtificialCommits
                && AppSettings.RevisionGraphShowArtificialCommits);

    private void ShowRepository()
    {
        if (_dashboard is not null)
        {
            _dashboard.IsVisible = false;
        }

        mainContentGrid.IsVisible = true;
        toolPanel.IsVisible = true;
        toolStripMainHost.IsVisible = true;
        toolStripFiltersHost.IsVisible = true;
        ToolStripScripts.IsVisible = true;
        _repositoryHistoryUIService?.TriggerBranchNameCacheUpdate(onlyIfEmpty: true);
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        if (_updateCheckService is not null
            && AppSettings.CheckForUpdates
            && AppSettings.LastUpdateCheck.AddDays(7) < DateTime.Now)
        {
            AppSettings.LastUpdateCheck = DateTime.Now;
            _updateCheckService.SearchForUpdatesAndShow(this, alwaysShow: false);
        }

        CancellationToken cancellationToken = _loadOperationsCancellationTokenSource.Token;
        _loadOperations.FileAndForget(async () =>
        {
            try
            {
                await TaskScheduler.Default;
                PluginRegistry.InitializeAll();
                await _loadOperations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                RegisterPlugins();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);
            }
        });
    }

    private void ChangeWorkingDirectory(string path)
    {
        string normalizedPath;
        try
        {
            normalizedPath = string.IsNullOrWhiteSpace(path)
                ? string.Empty
                : Path.GetFullPath(path);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            MessageBoxes.ShowError(this, exception.Message);
            return;
        }

        string currentPath = string.IsNullOrWhiteSpace(Module.WorkingDir)
            ? string.Empty
            : Path.GetFullPath(Module.WorkingDir);
        StringComparison pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (string.Equals(normalizedPath, currentPath, pathComparison))
        {
            return;
        }

        _submoduleStatusProvider?.Init();
        PluginRegistry.Unregister(UICommands);
        UICommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
        UICommands = UICommands.WithWorkingDirectory(normalizedPath);
        UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
        RegisterPlugins();
        ChangeTerminalActiveFolder(normalizedPath);
        if (Module.IsValidGitWorkingDir())
        {
            AppSettings.RecentWorkingDir = normalizedPath;
        }

        ReloadRepository();
    }

    private void OpenRepositoryDialog()
    {
        IGitModule? module = FormOpenDirectory.OpenModule(this, UICommands.GetRequiredService<IGitExecutorProvider>(), Module);
        if (module is not null)
        {
            ChangeWorkingDirectory(module.WorkingDir);
        }
    }

    private void ConfigureRecentRepositories()
    {
        using FormRecentReposSettings form = new();
        form.ShowDialog(this);
        _repositoryHistoryUIService?.Invalidate();
        _NO_TRANSLATE_WorkingDir.RefreshContent();
        if (_dashboard?.IsVisible is true)
        {
            _dashboard.RefreshContent();
        }
    }

    private void UICommands_PostRepositoryChanged(object? sender, GitUIEventArgs e)
    {
        CancellationToken cancellationToken = _loadOperationsCancellationTokenSource.Token;

        // Note that this called in most FormBrowse context to "be sure"
        // that the repo has not been updated externally.
        // It can also be called from background tasks, e.g. from BackgroundFetchPlugin.
        _loadOperations.FileAndForget(async () =>
        {
            await _loadOperations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            ReloadRepository();
        });
    }

    private void RefreshRevisions()
    {
        RevisionGrid.ReloadRevisions(Module, selectedObjectId: RevisionGrid.SelectedId);
    }

    private void RefreshGitStatusMonitor()
        => _gitStatusMonitor?.RequestRefresh();

    private void RefreshSelection()
    {
        IReadOnlyList<GitRevision> selectedRevisions = RevisionGrid.GetSelectedRevisions();
        GitRevision? selectedRevision = selectedRevisions.Count > 0 ? selectedRevisions[0] : null;

        FillFileTree(selectedRevision);
        FillDiff(selectedRevisions);

        (string? body, string? notes) old = (selectedRevision?.Body, selectedRevision?.Notes);
        FillCommitInfo(selectedRevision);

        // If the revision's body has been updated then the grid needs to be refreshed to display it
        if (AppSettings.ShowCommitBodyInRevisionGrid
            && selectedRevision?.HasMultiLineMessage is true
            && old != (selectedRevision.Body, selectedRevision.Notes))
        {
            RevisionGrid.InvalidateVisual();
        }

        RefreshGpgInfo(selectedRevision);
        FillBuildReport(selectedRevision);
        repoObjectsTree.SelectionChanged(selectedRevisions);
    }

    #region IBrowseRepo

    public GitRevision? GetLatestSelectedRevision() => RevisionGrid.LatestSelectedRevision;
    public IReadOnlyList<GitRevision> GetSelectedRevisions() => RevisionGrid.GetSelectedRevisions();
    public System.Drawing.Point GetQuickItemSelectorLocation() => RevisionGrid.GetQuickItemSelectorLocation();

    #endregion

    private void PopulatePluginMenu()
    {
        lock (PluginRegistry.Plugins)
        {
            if (PluginRegistry.Plugins.Count == 0)
            {
                return;
            }

            pluginsToolStripMenuItem.Items.Remove(pluginsLoadingToolStripMenuItem);
            MenuItem[] existingPluginItems = pluginsToolStripMenuItem.Items
                .OfType<MenuItem>()
                .Where(item => item.Tag is IGitPlugin)
                .ToArray();
            foreach (MenuItem existingPluginItem in existingPluginItems)
            {
                pluginsToolStripMenuItem.Items.Remove(existingPluginItem);
            }

            int insertIndex = 0;
            foreach (IGitPlugin plugin in PluginRegistry.Plugins
                         .OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase))
            {
                MenuItem item = new()
                {
                    Header = plugin.Name,
                    Tag = plugin,
                };
                Avalonia.Media.IImage? icon = PluginIconProvider.GetIcon(plugin);
                if (icon is not null)
                {
                    item.Icon = new Image { Width = 16, Height = 16, Source = icon };
                }

                item.Click += (_, _) =>
                {
                    if (plugin.Execute(new GitUIEventArgs(this, UICommands)))
                    {
                        UICommands.RepoChangedNotifier.Notify();
                    }
                };
                pluginsToolStripMenuItem.Items.Insert(insertIndex++, item);
            }
        }
    }

    public void GoToRef(string refName, bool showNoRevisionMsg, bool toggleSelection = false) => RevisionGrid.GoToRef(refName, showNoRevisionMsg, toggleSelection);

    private void UpdateRepositoryHostsMenu()
    {
        IRepositoryHostPlugin? firstHost = PluginRegistry.GitHosters.FirstOrDefault();
        _repositoryHostsToolStripMenuItem.IsVisible = firstHost is not null;
        if (firstHost is not null)
        {
            _repositoryHostsToolStripMenuItem.Header = firstHost.Name;
        }
    }

    /// <summary>
    /// Set the path filter.
    /// </summary>
    /// <param name="pathFilter">Zero or more quoted paths, separated by spaces.</param>
    public void SetPathFilter(string pathFilter)
    {
        RevisionGrid.SetAndApplyPathFilter(pathFilter);
    }

    private void ShowDashboard()
    {
        if (_dashboard is null)
        {
            _dashboard = new Dashboard
            {
                Name = "dashboard",
                UICommandsSource = this,
            };
            if (_repositoryHistoryUIService is not null)
            {
                _dashboard.Initialize(_repositoryHistoryUIService);
            }

            _dashboard.GitModuleChanged += (_, e) => ChangeWorkingDirectory(e.GitModule.WorkingDir);
            _dashboard.ConfigureRepositoriesRequested += (_, _) => ConfigureRecentRepositories();
            _dashboard.OpenRepositoryRequested += (_, _) => OpenRepositoryDialog();
            _contentPanel.Children.Add(_dashboard);
        }

        mainContentGrid.IsVisible = false;
        toolStripMainHost.IsVisible = false;
        toolStripFiltersHost.IsVisible = false;
        ToolStripScripts.IsVisible = false;
        toolPanel.IsVisible = true;
        _dashboard.IsVisible = true;
        _dashboard.RefreshContent();
        _dashboard.Focus();
    }

    private void UpdatePluginMenu(bool validWorkingDir)
    {
        foreach (MenuItem item in pluginsToolStripMenuItem.Items.OfType<MenuItem>())
        {
            if (item == pluginsLoadingToolStripMenuItem)
            {
                continue;
            }

            item.IsEnabled = item.Tag is not IGitPluginForRepository || validWorkingDir;
        }
    }

    private void RegisterPlugins()
    {
        bool werePluginsRegistered = PluginRegistry.PluginsRegistered;

        // Allow the plugin to perform any self-registration actions
        PluginRegistry.Register(UICommands);

        // pluginsToolStripMenuItem.DropDownItems menu already contains at least 2 items:
        //    [1] Separator
        //    [0] Plugin Settings
        // insert all plugins except 'Plugin Manager' above the separator
        if (!werePluginsRegistered && PluginRegistry.PluginsRegistered)
        {
            UICommands.RaisePostRegisterPlugin(this);
        }

        PopulatePluginMenu();
        UpdateRepositoryHostsMenu();
        UpdatePluginMenu(Module.IsValidGitWorkingDir());
        revisionDiff.RegisterGitHostingPluginInBlameControl();
        fileTree.RegisterGitHostingPluginInBlameControl();
    }

    private void OnActivate()
    {
        // check if we are in the middle of bisect
        notificationBarBisectInProgress.RefreshBisect();

        // check if we are in the middle of an action (merge/rebase/etc.)
        notificationBarGitActionInProgress.RefreshGitAction(
            checkForConflicts: AppSettings.GitAsyncWhenMinimized || WindowState != WindowState.Minimized);
    }

    private void UpdateStashCount()
    {
        if (!AppSettings.ShowStashCount || !Module.IsValidGitWorkingDir() || Module.IsBareRepository())
        {
            toolStripSplitStash.Content = string.Empty;
            return;
        }

        CancellationToken cancellationToken = _loadOperationsCancellationTokenSource.Token;
        _loadOperations.FileAndForget(async () =>
        {
            // Add a delay to not interfere with GUI updates when switching repository
            await Task.Delay(500, cancellationToken);
            int result = Module.GetStashes(noLocks: true).Count;
            await _loadOperations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            toolStripSplitStash.Content = $"({result})";
        });
    }

    internal void QueueRepositoryHostOperation(
        Func<JoinableTaskFactory, CancellationToken, Task> operation)
    {
        CancellationToken cancellationToken = _loadOperationsCancellationTokenSource.Token;
        _loadOperations.FileAndForget(
            () => operation(_loadOperations.JoinableTaskFactory, cancellationToken));
    }

    public override IScriptOptionsProvider GetScriptOptionsProvider()
    {
        if (CommitInfoTabControl.SelectedItem == TreeTabPage)
        {
            return fileTree.ScriptOptionsProvider;
        }

        if (CommitInfoTabControl.SelectedItem == DiffTabPage)
        {
            return revisionDiff.ScriptOptionsProvider;
        }

        return base.GetScriptOptionsProvider();
    }

    private void FillFileTree(GitRevision? revision)
    {
        // "File Tree" tab implemented using git-grep works for artificial commits, too
        bool showFileTreeTab = true;

        TreeTabPage.IsVisible = showFileTreeTab;
        if (!showFileTreeTab
            || CommitInfoTabControl.SelectedItem != TreeTabPage
            || _selectedRevisionUpdatedTargets.HasFlag(UpdateTargets.FileTree))
        {
            return;
        }

        _selectedRevisionUpdatedTargets |= UpdateTargets.FileTree;
        if (revision is not null)
        {
            fileTree.DisplayDiffTab([revision]);
        }
    }

    private void OpenRepository(string path, ObjectId selectedId, ObjectId firstId)
    {
        // The Avalonia grid currently retains one pending revision across repository changes.
        // Keep the original first diff endpoint in this boundary until multi-selection accepts it.
        RevisionGrid.SelectedId = selectedId;
        ChangeWorkingDirectory(path);
    }

    private void FillDiff(IReadOnlyList<GitRevision> revisions)
    {
        // Avalonia keeps the shared diff pane live while split view hosts it outside the selected tab.
        bool splitViewShowsDiff = AppSettings.ShowSplitViewLayout;
        if (!splitViewShowsDiff && CommitInfoTabControl.SelectedItem != DiffTabPage)
        {
            return;
        }

        if (_selectedRevisionUpdatedTargets.HasFlag(UpdateTargets.DiffList))
        {
            return;
        }

        _selectedRevisionUpdatedTargets |= UpdateTargets.DiffList;
        revisionDiff.DisplayDiffTab(revisions);
    }

    private void FillCommitInfo(GitRevision? revision)
    {
        if (_selectedRevisionUpdatedTargets.HasFlag(UpdateTargets.CommitInfo))
        {
            return;
        }

        if (AppSettings.CommitInfoPosition == CommitInfoPosition.BelowList
            && CommitInfoTabControl.SelectedItem != CommitInfoTabPage)
        {
            return;
        }

        _selectedRevisionUpdatedTargets |= UpdateTargets.CommitInfo;

        if (revision is null)
        {
            return;
        }

        IReadOnlyList<ObjectId> children = RevisionGrid.GetRevisionChildren(revision.ObjectId);
        RevisionInfo.SetRevisionWithChildren(revision, children);
    }

    private void ApplySourceToolbarAutoSize()
    {
        // WinForms ToolStrip item preferred widths include renderer-owned chrome which is
        // not part of Avalonia's content measurement.
        WinFormsAutoSizeContentControl.Attach(_NO_TRANSLATE_WorkingDir, 39, 22);
        WinFormsAutoSizeContentControl.Attach(branchSelect, 39, 22);
    }

    private void InitializeToolbarsMenus()
    {
        _formBrowseMenus!.CreateToolbarsMenus(
            (ToolStripMain, "Standard"),
            (ToolStripFilters, "Filters"),
            (ToolStripScripts, "Scripts"));
        toolPanel.ContextMenu = _formBrowseMenus.ToolStripContextMenu;
    }

    private void InitializeWorkspaceLayout()
    {
        toggleSplitViewLayout.Click += toggleSplitViewLayout_Click;
        menuCommitInfoPosition.Click += CommitInfoPositionClick;
        commitInfoBelowMenuItem.Click += (_, _) => SetCommitInfoPosition(CommitInfoPosition.BelowList);
        commitInfoLeftwardMenuItem.Click += (_, _) => SetCommitInfoPosition(CommitInfoPosition.LeftwardFromList);
        commitInfoRightwardMenuItem.Click += (_, _) => SetCommitInfoPosition(CommitInfoPosition.RightwardFromList);
        RefreshWorkspaceLayout();
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

    private void RefreshLeftPanel(
        Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs,
        Lazy<IReadOnlyCollection<GitRevision>> getStashRevs,
        bool forceRefresh)
    {
        ToolStripFilters.RefreshRevisionFunction(getRefs);
        IGitModule module = Module;
        string workingDirectory = module.WorkingDir;
        CancellationToken cancellationToken = _loadOperationsCancellationTokenSource.Token;
        _loadOperations.FileAndForget(async () =>
        {
            await TaskScheduler.Default;
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<IGitRef> refs = getRefs(RefsFilter.NoFilter);
            IReadOnlyCollection<GitRevision> stashes = getStashRevs.Value;
            string currentBranch = module.GetSelectedBranch();
            IReadOnlyList<Remote> enabledRemotes = await module.GetRemotesAsync();
            IReadOnlyList<GitWorktree> worktrees = module.GetWorktrees();
            ConfigFileRemoteSettingsManager remotesManager = new(() => module);
            IReadOnlyList<Remote> disabledRemotes = remotesManager.GetDisabledRemotes();
            cancellationToken.ThrowIfCancellationRequested();
            await _loadOperations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            if (!ReferenceEquals(Module, module))
            {
                return;
            }

            _worktrees = worktrees;
            toolStripWorktrees.IsVisible = worktrees.Count > 1;
            repoObjectsTree.SetRefs(
                refs,
                stashes,
                currentBranch,
                enabledRemotes,
                disabledRemotes,
                remotesManager,
                worktrees,
                workingDirectory);
        });
    }

    private void CheckoutToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartCheckoutRevisionDialog(this);
    }

    private void CommitToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartCommitDialog(this);
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
        rows[1].Height = showSplitView ? new GridLength(6) : new GridLength(0);
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

    private void PushToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartPushDialog(this, pushOnShow: false);
    }

    private void RefreshToolStripMenuItemClick(object sender, EventArgs e)
    {
        // Broadcast RepoChanged in case repo was changed outside of GE
        UICommands.RepoChangedNotifier.Notify();
        RefreshGitStatusMonitor();
    }

    private void RefreshDashboardToolStripMenuItemClick(object sender, EventArgs e)
    {
        _dashboard?.RefreshContent();
    }

    private void PatchToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartViewPatchDialog(this);
    }

    private void ApplyPatchToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartApplyPatchDialog(this);
    }

    private void userShell_Click(object? sender, EventArgs e)
    {
        try
        {
            UICommands.GetRequiredService<ITerminalLauncher>().Launch(Module.WorkingDir);
        }
        catch (PlatformNotSupportedException exception) when (FlatpakEnvironment.IsFlatpak())
        {
            // Cross-platform constraint: a confined app cannot execute a host terminal.
            MessageBoxes.FailedToRunShell(this, "Git bash", exception);
        }
        catch (Exception exception)
        {
            MessageBoxes.FailedToRunShell(this, "Git bash", exception);
        }
    }

    private void FormatPatchToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartFormatPatchDialog(this);
    }

    private void CheckoutBranchToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartCheckoutBranch(this);
    }

    private void StashToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartStashDialog(this);
        UpdateStashCount();
    }

    private void ResetToolStripMenuItem_Click(object sender, EventArgs e)
    {
        UICommands.StartResetChangesDialog(this, Module.GetWorkTreeFiles(), onlyWorkTree: false);
        RefreshGitStatusMonitor();
        revisionDiff.RefreshArtificial();
    }

    private void RunMergetoolToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartResolveConflictsDialog(this);
    }

    private void PopulateBranchSelector()
    {
        BranchSelectFlyout.Items.Clear();
        MenuItem checkout = new()
        {
            Header = checkoutBranchToolStripMenuItem.Header,
            Icon = new Image
            {
                Width = 16,
                Height = 16,
                Source = Images.BranchCheckout,
            },
        };
        checkout.Click += (_, _) => QueueBranchCheckout();
        BranchSelectFlyout.Items.Add(checkout);
        BranchSelectFlyout.Items.Add(new Separator());
        foreach (IGitRef branch in Module.GetRefs(RefsFilter.Heads).Take(100))
        {
            if (branch.ObjectId.IsZero)
            {
                throw new InvalidOperationException($"Branch '{branch.Name}' has no ObjectId.");
            }

            bool isBranchVisible = ((ICheckRefs)RevisionGrid).Contains(branch.ObjectId);
            MenuItem item = new()
            {
                Header = branch.Name,
                Icon = new Image
                {
                    Width = 16,
                    Height = 16,
                    Source = (isBranchVisible ? Images.Branch : Images.EyeClosed).AdaptLightness(),
                },
                Opacity = isBranchVisible ? 1 : 0.55,
            };
            item.Click += (_, _) => QueueBranchCheckout(branch.Name);
            BranchSelectFlyout.Items.Add(item);
        }
    }

    private void QueueBranchCheckout(string branch = "")
    {
        BranchSelectFlyout.Hide();

        // MenuFlyout raises Click before it finishes dismissing its popup. The checkout flow
        // may synchronously show a modal dialog, so let the popup complete first.
        Dispatcher.UIThread.Post(() => UICommands.StartCheckoutBranch(this, branch));
    }

    private void BranchSelectPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(branchSelect).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
        {
            PopulateBranchSelector();
        }
    }

    private void BranchSelectPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Right)
        {
            return;
        }

        CheckoutBranchToolStripMenuItemClick(sender, e);
        e.Handled = true;
    }

    private void BranchSelectKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F4 || (e.Key == Key.Down && (e.KeyModifiers & KeyModifiers.Alt) != 0))
        {
            PopulateBranchSelector();
        }
    }

    private MenuFlyout BranchSelectFlyout => (MenuFlyout)branchSelect.Flyout!;

    private MenuFlyout WorktreeFlyout => (MenuFlyout)toolStripWorktrees.Flyout!;

    private void RepoObjectsTree_SelectionChanged(object? sender, EventArgs e)
    {
        if (repoObjectsTree.SelectedRevisionObjectId is ObjectId objectId)
        {
            RevisionGrid.SelectRevision(objectId);
        }
    }

    private void CurrentBranchClick(object sender, EventArgs e)
    {
        PopulateBranchSelector();
        branchSelect.ShowDropDown();
    }

    private void DeleteBranchToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartDeleteBranchDialog(this, string.Empty);
    }

    private void DeleteTagToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartDeleteTagDialog(this, null);
    }

    private void PopulateWorktreeSelector()
    {
        WorktreeFlyout.Items.Clear();
        string currentWorkingDirectory = Module.WorkingDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        foreach (GitWorktree worktree in _worktrees)
        {
            bool isCurrent = string.Equals(
                worktree.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                currentWorkingDirectory,
                comparison);
            string displayName = worktree.GetDisplayName(
                Path.GetFileName(worktree.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)));
            MenuItem item = new()
            {
                Header = displayName,
                Tag = worktree.Path,
                Icon = new Image { Width = 16, Height = 16, Source = Properties.Images.WorkTree },
                ToggleType = MenuItemToggleType.CheckBox,
                IsChecked = isCurrent,
                IsEnabled = !isCurrent && !worktree.IsDeleted,
            };
            if (worktree.IsDeleted)
            {
                item.Classes.Add("worktree-deleted");
            }

            item.Click += WorktreeToolStripMenuItem_Click;
            WorktreeFlyout.Items.Add(item);
        }

        WorktreeFlyout.Items.Add(new Separator());
        MenuItem createItem = CreateWorktreeFlyoutItem(TranslatedStrings.CreateWorktree, Properties.Images.WorkTree);
        createItem.Click += (_, _) =>
        {
            string mainPath = _worktrees.Count > 0 ? _worktrees[0].Path : Module.WorkingDir;
            UICommands.WorktreeCreate(this, mainPath);
        };
        WorktreeFlyout.Items.Add(createItem);

        MenuItem pruneItem = CreateWorktreeFlyoutItem(TranslatedStrings.PruneWorktrees);
        pruneItem.Click += (_, _) =>
        {
            if (UICommands.StartCommandLineProcessDialog(this, command: null, "worktree prune"))
            {
                UICommands.RepoChangedNotifier.Notify();
            }
        };
        WorktreeFlyout.Items.Add(pruneItem);

        MenuItem manageItem = CreateWorktreeFlyoutItem(TranslatedStrings.ManageWorktrees, Properties.Images.WorkTree);
        manageItem.Click += manageWorktreeToolStripMenuItem_Click;
        WorktreeFlyout.Items.Add(manageItem);
    }

    private static MenuItem CreateWorktreeFlyoutItem(string header, Avalonia.Media.IImage? icon = null)
        => new()
        {
            Header = header,
            Icon = icon is null ? null : new Image { Width = 16, Height = 16, Source = icon },
        };

    private void CherryPickToolStripMenuItemClick(object sender, EventArgs e)
    {
        IReadOnlyList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions(SortDirection.Descending);
        UICommands.StartCherryPickDialog(this, revisions);
    }

    private void RevisionGrid_SelectionChanged(object? sender, EventArgs e)
    {
        RefreshSelection();

        IReadOnlyList<GitRevision> selectedRevisions = RevisionGrid.GetSelectedRevisions();
        rebaseToolStripMenuItem.IsEnabled =
            RevisionGrid.SelectedRevision is { IsArtificial: false }
            && !Module.IsBareRepository();
        archiveToolStripMenuItem.IsEnabled =
            selectedRevisions.Count == 1
            && selectedRevisions[0] is { IsArtificial: false };
    }

    private void MergeBranchToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartMergeBranchDialog(this, branch: null);
    }

    private void toolsToolStripMenuItem_SettingsChanged(object sender, Menus.SettingsChangedEventArgs e)
    {
        HandleSettingsChanged(e.OldTranslation, e.OldCommitInfoPosition);
    }

    private void OnShowSettingsClick(object sender, EventArgs e)
    {
        string translation = AppSettings.Translation;
        CommitInfoPosition commitInfoPosition = AppSettings.CommitInfoPosition;

        // Await plugin registration
        _loadOperations.JoinPendingOperations();
        UICommands.StartSettingsDialog(this);
        HandleSettingsChanged(translation, commitInfoPosition);
    }

    private void HandleSettingsChanged(string oldTranslation, CommitInfoPosition oldCommitInfoPosition)
    {
        Module.InvalidateGitSettings();

        if (oldTranslation != AppSettings.Translation)
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
            AvaloniaTranslationUtils.RemoveTextBlockMnemonicMarkers(this);
        }

        if (oldCommitInfoPosition != AppSettings.CommitInfoPosition)
        {
            RefreshWorkspaceLayout(refreshCommitInfoPositionToolTip: true);
        }

        LoadHotkeys(HotkeySettingsName);
        RefreshMenuShortcutKeys();
        _NO_TRANSLATE_WorkingDir.RefreshShortcutKeys(Hotkeys);
        ToolStripFilters.RefreshBrowseDialogShortcutKeys(Hotkeys ?? []);
        IReadOnlyList<HotkeyCommand> revisionGridHotkeys = UICommands
            .GetRequiredService<IHotkeySettingsLoader>()
            .LoadHotkeys(RevisionGridControl.HotkeySettingsName);
        ToolStripFilters.RefreshRevisionGridShortcutKeys(revisionGridHotkeys);
        RevisionGrid.RefreshMenuShortcutKeys(revisionGridHotkeys);
        LoadUserMenu();
        AvatarService.UpdateAvatarInitialFontsSettings();
        RevisionGrid.ApplyColumnSettings();
        RevisionGrid.RefreshRealizedRows();
        RevisionInfo.Revision = RevisionGrid.SelectedRevision;
        RefreshDefaultPullAction();
        _gitStatusMonitor?.Active = Module.IsValidGitWorkingDir() && NeedsGitStatusMonitor();
        RefreshGitStatusMonitor();
        RefreshPushButton(Module, Module.IsValidGitWorkingDir() ? Module.GetSelectedBranch() : string.Empty);
    }

    private void TagToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartCreateTagDialog(this, RevisionGrid.SelectedRevision);
    }

    private void EditGitignoreToolStripMenuItem1Click(object sender, EventArgs e)
    {
        UICommands.StartEditGitIgnoreDialog(this, false);
    }

    private void EditGitInfoExcludeToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartEditGitIgnoreDialog(this, true);
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
                GpgInfoTabPage.IsVisible = false;
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

    private void ArchiveToolStripMenuItemClick(object sender, EventArgs e)
    {
        IReadOnlyList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();
        if (revisions.Count is (< 1 or > 2))
        {
            MessageBoxes.SelectOnlyOneOrTwoRevisions(this);
            return;
        }

        GitRevision mainRevision = revisions[0];
        GitRevision? diffRevision = revisions.Count == 2 ? revisions[1] : null;
        UICommands.StartArchiveDialog(this, mainRevision, diffRevision);
    }

    private void EditMailMapToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartMailMapDialog(this);
    }

    private void EditLocalGitConfigToolStripMenuItemClick(object sender, EventArgs e)
    {
        string fileName = Path.Combine(Module.ResolveGitInternalPath("config"));
        UICommands.StartFileEditorDialog(fileName, showWarning: true);
    }

    private void CompressGitDatabaseToolStripMenuItemClick(object sender, EventArgs e)
    {
        FormProcess.ReadDialog(this, UICommands, arguments: "gc", Module.WorkingDir, input: null, useDialogSettings: true);
    }

    private void recoverLostObjectsToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartVerifyDatabaseDialog(this);
    }

    private void ManageRemoteRepositoriesToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartRemotesDialog(this);
    }

    private void GitStatusMonitorStateChanged(object? sender, GitStatusMonitorStateEventArgs e)
    {
        if (e.State != GitStatusMonitorState.Stopped)
        {
            return;
        }

        UpdateCommitButtonAndGetBrush(status: null, showCount: false);
        RevisionGrid.UpdateArtificialCommitCount(status: null);
    }

    private void GitWorkingDirectoryStatusChanged(object? sender, GitWorkingDirectoryStatusEventArgs? e)
    {
        IReadOnlyList<GitItemStatus>? status = e?.ItemStatuses;
        UpdateCommitButtonAndGetBrush(status, AppSettings.ShowGitStatusInBrowseToolbar);
        RevisionGrid.UpdateArtificialCommitCount(
            AppSettings.ShowGitStatusForArtificialCommits
                && AppSettings.RevisionGraphShowArtificialCommits
                    ? status
                    : null);
    }

    private void RefreshPushButton(IGitModule module, string branchName)
    {
        if (_aheadBehindDataProvider is null
            || !AppSettings.ShowAheadBehindData
            || string.IsNullOrWhiteSpace(branchName))
        {
            toolStripButtonPush.ResetToDefaultState();
            return;
        }

        CancellationToken cancellationToken = _loadOperationsCancellationTokenSource.Token;
        _loadOperations.FileAndForget(async () =>
        {
            IReadOnlyDictionary<string, AheadBehindData>? aheadBehindData = await Task.Run(
                () => _aheadBehindDataProvider.GetData(branchName),
                cancellationToken);
            await Dispatcher.UIThread.InvokeAsync(
                () =>
                {
                    if (!ReferenceEquals(module, Module)
                        || !string.Equals(branchName, Module.GetSelectedBranch(), StringComparison.Ordinal))
                    {
                        return;
                    }

                    toolStripButtonPush.DisplayAheadBehindInformation(
                        aheadBehindData,
                        branchName,
                        GetShortcutKeyTooltipString(Command.Push));
                },
                DispatcherPriority.Normal,
                cancellationToken);
        });
    }

    private void RebaseToolStripMenuItemClick(object sender, EventArgs e)
    {
        if (RevisionGrid.SelectedRevision is not { IsArtificial: false } revision)
        {
            return;
        }

        UICommands.StartRebaseDialog(this, revision.ObjectId.ToString());
    }

    private void CommitInfoTabControl_SelectedIndexChanged(object? sender, SelectionChangedEventArgs e)
    {
        bool selectingGpgInfo = CommitInfoTabControl.SelectedItem == GpgInfoTabPage;
        if (!selectingGpgInfo)
        {
            RefreshSelection();
        }

        if (CommitInfoTabControl.SelectedItem == TreeTabPage)
        {
            fileTree.SwitchFocus(alreadyContainedFocus: false);
        }
        else if (CommitInfoTabControl.SelectedItem == DiffTabPage)
        {
            revisionDiff.SwitchFocus(alreadyContainedFocus: false);
        }
        else if (selectingGpgInfo)
        {
            // Avalonia keeps the revision prepared by the selection event while the GPG tab receives focus.
            FillGpgInfo();
            revisionGpgInfo1.FocusInfo();
        }
        else if (CommitInfoTabControl.SelectedItem == _consoleTabPage)
        {
            StartTerminal();
        }
    }

    private void ToolStripButtonPushClick(object sender, EventArgs e)
    {
        PushToolStripMenuItemClick(sender, e);
    }

    private void ManageSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartSubmodulesDialog(this);
        UpdateSubmodulesStructure();
    }

    private void UpdateSubmoduleToolStripMenuItemClick(object? sender, EventArgs e)
    {
        if (sender is MenuItem { Tag: string submodule } && Module.SuperprojectModule is not null)
        {
            FormProcess.ShowDialog(
                this,
                UICommands,
                arguments: Commands.SubmoduleUpdate(submodule),
                Module.SuperprojectModule.WorkingDir,
                input: null,
                useDialogSettings: true);
        }

        RefreshRevisions();
    }

    private void UpdateAllSubmodulesToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartUpdateSubmodulesDialog(this);
        UpdateSubmodulesStructure();
    }

    private void SynchronizeAllSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartSyncSubmodulesDialog(this);
        UpdateSubmodulesStructure();
    }

    private void ToolStripSplitStashButtonClick(object sender, EventArgs e)
    {
        UICommands.StartStashDialog(this);
        UpdateStashCount();
    }

    private void StashChangesToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash);
        UpdateStashCount();
    }

    private void StashStagedToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StashStaged(this);
        UpdateStashCount();
    }

    private void StashPopToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StashPop(this);
        UpdateStashCount();
    }

    private void ManageStashesToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartStashDialog(this);
        UpdateStashCount();
    }

    private void CreateStashToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartStashDialog(this, manageStashes: false);
        UpdateStashCount();
    }

    private void PluginSettingsToolStripMenuItemClick(object sender, EventArgs e)
        => UICommands.StartPluginSettingsDialog(this);

    private void RepoSettingsToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartRepoSettingsDialog(this);
    }

    private void CloseToolStripMenuItemClick(object sender, EventArgs e)
    {
        SetWorkingDir(string.Empty);
    }

    private void CleanupToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartCleanupRepositoryDialog(this);
    }

    public void SetWorkingDir(string? path, ObjectId selectedId = default, ObjectId firstId = default)
    {
        RevisionGrid.SelectedId = selectedId.IsZero ? firstId : selectedId;
        ChangeWorkingDirectory(path ?? string.Empty);
    }

    private void FileExplorerToolStripMenuItemClick(object sender, EventArgs e)
    {
        OsShellUtil.OpenWithFileExplorer(Module.WorkingDir);
    }

    private void CreateBranchToolStripMenuItemClick(object sender, EventArgs e)
    {
        UICommands.StartCreateBranchDialog(this, RevisionGrid.SelectedRevision?.ObjectId ?? default);
    }

    private void editGitAttributesToolStripMenuItem_Click(object sender, EventArgs e)
    {
        UICommands.StartEditGitAttributesDialog(this);
    }

    private void deleteIndexLockToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            Module.UnlockIndex(includeSubmodules: true);
        }
        catch (FileDeleteException exception)
        {
            throw new UserExternalOperationException(
                _indexLockCantDelete.Text,
                new ExternalOperationException(
                    arguments: exception.FileName,
                    workingDirectory: Module.WorkingDir,
                    innerException: exception));
        }
    }

    private void BisectClick(object sender, EventArgs e)
    {
        using FormBisect form = new(RevisionGrid);
        form.ShowDialog(this);
        RefreshRevisions();
    }

    private void _forkCloneMenuItem_Click(object? sender, EventArgs e)
    {
        IRepositoryHostPlugin? repoHost = PluginRegistry.GitHosters.FirstOrDefault();
        if (repoHost is null)
        {
            MessageBoxes.ShowError(this, _noReposHostPluginLoaded.Text, TranslatedStrings.Error);
            return;
        }

        UICommands.StartCloneForkFromHoster(
            this,
            repoHost,
            (_, args) => SetWorkingDir(args.GitModule.WorkingDir));
    }

    private void _viewPullRequestsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (TryGetRepositoryHost(out IRepositoryHostPlugin? repoHost))
        {
            UICommands.StartPullRequestsDialog(this, repoHost);
        }
    }

    private void _createPullRequestToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (TryGetRepositoryHost(out IRepositoryHostPlugin? repoHost))
        {
            UICommands.StartCreatePullRequest(this, repoHost);
        }
    }

    private void _addUpstreamRemoteToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (TryGetRepositoryHost(out IRepositoryHostPlugin? repoHost))
        {
            UICommands.AddUpstreamRemote(this, repoHost);
        }
    }

    private bool TryGetRepositoryHost([NotNullWhen(returnValue: true)] out IRepositoryHostPlugin? repoHost)
    {
        repoHost = PluginRegistry.TryGetGitHosterForModule(Module);
        if (repoHost is not null)
        {
            return true;
        }

        MessageBoxes.Show(
            this,
            _noReposHostFound.Text,
            TranslatedStrings.Error,
            WinFormsShims.MessageBoxButtons.OK,
            WinFormsShims.MessageBoxIcon.Error);
        return false;
    }

    public static readonly string HotkeySettingsName = "Browse";

    internal enum Command
    {
        // Focus or visuals
        FocusLeftPanel = 25,
        FocusRevisionGrid = 3,
        FocusCommitInfo = 4,
        FocusDiff = 5,
        FocusFileTree = 6,
        FocusGpgInfo = 26,
        FocusGitConsole = 29,
        FocusBuildServerStatus = 30,
        FocusOutputHistoryAndToggleIfPanel = 47,
        FocusNextTab = 31,
        FocusPrevTab = 32,

        FocusFilter = 18,

        ToggleLeftPanel = 21,

        // START menu
        OpenRepo = 45,

        // DASHBOARD menu

        // REPOSITORY menu
        CloseRepository = 15,
        ManageWorkTrees = 49,

        // COMMANDS menu
        Commit = 7,
        CheckoutBranch = 10,
        PullOrFetch = 39,
        Push = 40,
        CreateBranch = 41,
        MergeBranches = 42,
        CreateTag = 43,
        Rebase = 44,

        // PLUGINS menu

        // TOOLS menu
        GitBash = 0,
        GitGui = 1,
        GitGitK = 2,
        OpenSettings = 20,

        // HELP menu

        // Toolbar
        AddNotes = 8,
        FindFileInSelectedCommit = 9,
        QuickPullOrFetch = 48,
        QuickFetch = 11,
        QuickPull = 12,
        QuickPush = 13,
        Stash = 16,
        StashStaged = 46,
        StashPop = 17,
        GoToSuperproject = 27,
        GoToSubmodule = 28,

        // Diff or File Tree tab
        OpenWithDifftool = 19,
        EditFile = 22,
        OpenAsTempFile = 23,
        OpenAsTempFileWith = 24,
        OpenWithDifftoolFirstToLocal = 33,
        OpenWithDifftoolSelectedToLocal = 34,

        // Revision grid
        OpenCommitsWithDifftool = 35,
        ToggleBetweenArtificialAndHeadCommits = 36,
        GoToChild = 37,
        GoToParent = 38,

        /* deprecated: RotateApplicationIcon = 14, */

        // WinForms routes F5 through ToolStripItem.ShortcutKeys. Avalonia has no ToolStrip,
        // so refresh joins the same command dispatcher without changing persisted upstream IDs.
        Refresh = 50,
    }

    private void AddNotes()
    {
        // Avalonia exposes the current row directly instead of WinForms' GetSelectedRevisionOrDefault helper.
        GitRevision? revision = RevisionGrid.SelectedRevision;
        if (revision?.IsArtificial is not false)
        {
            return;
        }

        Module.EditNotes(revision.ObjectId);
        FillCommitInfo(revision);
    }

    private void QuickFetch()
    {
        bool success = ScriptsRunner.RunEventScripts(ScriptEvent.BeforeFetch, this);
        if (!success)
        {
            return;
        }

        success = FormProcess.ShowDialog(
            this,
            UICommands,
            arguments: Module.FetchCmd(string.Empty, string.Empty, string.Empty),
            Module.WorkingDir,
            input: null,
            useDialogSettings: true);
        if (!success)
        {
            return;
        }

        ScriptsRunner.RunEventScripts(ScriptEvent.AfterFetch, this);
        UICommands.RepoChangedNotifier.Notify();
    }

    public override bool ProcessHotkey(Keys keyData)
    {
        // Avalonia resolves menu access keys after the window key handler; preserve WinForms menu precedence.
        string? gesture = KeysMapper.ToKeyGesture(keyData)?.ToString();
        MenuItem? accessKeyMenu = gesture is null
            ? null
            : mainMenuStrip.Items
                .OfType<MenuItem>()
                .FirstOrDefault(item => string.Equals(
                    AutomationProperties.GetAccessKey(item),
                    gesture,
                    StringComparison.OrdinalIgnoreCase));
        if (accessKeyMenu is not null)
        {
            accessKeyMenu.IsSubMenuOpen = true;
            return true;
        }

        if (repoObjectsTree.IsKeyboardFocusWithin && repoObjectsTree.ProcessHotkey(keyData))
        {
            return true;
        }

        // generic handling of this form's hotkeys (upstream)
        if (base.ProcessHotkey(keyData))
        {
            return true;
        }

        object? focused = FocusManager?.GetFocusedElement();

        // downstream (without keys for quick search and without keys for text selection and copy e.g. in CommitInfo)
        // but allow routing Ctrl+A away from RevisionGridControl in order to not select all revisions
        if (focused is TextBox && GitExtensionsControl.IsTextEditKey(keyData, multiLine: true))
        {
            return false;
        }

        // route to visible controls which have their own hotkeys
        return keyData != (WinFormsShims.Keys.Control | WinFormsShims.Keys.A)
            && RevisionGrid.ProcessHotkey(keyData);
    }

    protected override bool ExecuteCommand(int command)
    {
        switch ((Command)command)
        {
            case Command.GitBash: userShell_Click(this, EventArgs.Empty); break;
            case Command.GitGui: Module.RunGui(); break;
            case Command.GitGitK: Module.RunGitK(); break;
            case Command.FocusRevisionGrid: RevisionGrid.FocusRevisionGrid(); break;
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
            case Command.ToggleLeftPanel: toggleLeftPanel_Click(this, EventArgs.Empty); break;
            case Command.OpenSettings: OnShowSettingsClick(this, EventArgs.Empty); break;
            case Command.FocusNextTab: FocusNextWorkspaceTab(forward: true); break;
            case Command.FocusPrevTab: FocusNextWorkspaceTab(forward: false); break;
            case Command.Refresh: RefreshToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.Commit: CommitToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.AddNotes: AddNotes(); break;
            case Command.GoToSuperproject: toolStripButtonLevelUp_ButtonClick(toolStripButtonLevelUp, EventArgs.Empty); break;
            case Command.GoToSubmodule: toolStripButtonLevelUp.ShowDropDown(); break;
            case Command.CheckoutBranch: CheckoutBranchToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.QuickFetch: QuickFetch(); break;
            case Command.PullOrFetch: PullToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.Push: UICommands.StartPushDialog(this, pushOnShow: false); break;
            case Command.CreateBranch: CreateBranchToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.MergeBranches: MergeBranchToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.CreateTag: TagToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.Rebase: RebaseToolStripMenuItemClick(this, EventArgs.Empty); break;
            case Command.ManageWorkTrees: manageWorktreeToolStripMenuItem_Click(this, EventArgs.Empty); break;
            case Command.OpenRepo: OpenRepositoryDialog(); break;
            case Command.CloseRepository: ChangeWorkingDirectory(string.Empty); break;
            default: return base.ExecuteCommand(command);
        }

        return true;
    }

    internal bool ExecuteCommand(Command command)
        => ExecuteCommand((int)command);

    private void CommandsToolStripMenuItem_DropDownOpening(object? sender, EventArgs e)
    {
        // Most options do not make sense for artificial commits or no revision selected at all
        IReadOnlyList<GitRevision> selectedRevisions = RevisionGrid.GetSelectedRevisions();
        bool singleNormalCommit = selectedRevisions.Count == 1 && !selectedRevisions[0].IsArtificial;

        // Some commands like stash, undo commit etc has no relation to selections
        // Require that a single commit is selected
        // Some commands like delete branch could be available for artificial as no default is used,
        // but hide for consistency
        // Not operating on selected revision
        bool hasWorkingTree = !Module.IsBareRepository();

        branchToolStripMenuItem.IsEnabled =
        deleteBranchToolStripMenuItem.IsEnabled =
        mergeBranchToolStripMenuItem.IsEnabled =
        checkoutBranchToolStripMenuItem.IsEnabled =
        cherryPickToolStripMenuItem.IsEnabled =
        checkoutToolStripMenuItem.IsEnabled =
        bisectToolStripMenuItem.IsEnabled =
            singleNormalCommit && hasWorkingTree;

        rebaseToolStripMenuItem.IsEnabled =
            selectedRevisions.Count is (1 or 2)
            && selectedRevisions.All(revision => !revision.IsArtificial)
            && hasWorkingTree;

        tagToolStripMenuItem.IsEnabled =
        deleteTagToolStripMenuItem.IsEnabled =
        archiveToolStripMenuItem.IsEnabled =
            singleNormalCommit;

        commitToolStripMenuItem.IsEnabled =
        undoLastCommitToolStripMenuItem.IsEnabled =
        runMergetoolToolStripMenuItem.IsEnabled =
        stashToolStripMenuItem.IsEnabled =
        resetToolStripMenuItem.IsEnabled =
        cleanupToolStripMenuItem.IsEnabled =
        toolStripMenuItemReflog.IsEnabled =
        applyPatchToolStripMenuItem.IsEnabled =
            hasWorkingTree;
    }

    private void PullToolStripMenuItemClick(object? sender, EventArgs e)
    {
        UICommands.StartPullDialog(this);
    }

    private void ToolStripButtonPullClick(object sender, EventArgs e)
    {
        GitPullAction action = AppSettings.DefaultPullAction == GitPullAction.None
            ? AppSettings.FormPullAction
            : AppSettings.DefaultPullAction;

        // Clicking on the Pull button toolbar button will perform the default selected action silently,
        // except if that action is to open the dialog (PullAction.None)
        DoPull(action, isSilent: AppSettings.DefaultPullAction != GitPullAction.None);
    }

    private void pullToolStripMenuItem1_Click(object sender, EventArgs e)
    {
        // "Open Pull Dialog..." toolbar menu item always open the dialog with the current default action
        DoPull(pullAction: AppSettings.FormPullAction, isSilent: false);
    }

    private void mergeToolStripMenuItem_Click(object sender, EventArgs e)
    {
        DoPull(pullAction: GitPullAction.Merge, isSilent: true);
    }

    private void rebaseToolStripMenuItem1_Click(object sender, EventArgs e)
    {
        DoPull(pullAction: GitPullAction.Rebase, isSilent: true);
    }

    private void fetchToolStripMenuItem_Click(object sender, EventArgs e)
    {
        DoPull(pullAction: GitPullAction.Fetch, isSilent: true);
    }

    private void fetchAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
        DoPull(pullAction: GitPullAction.FetchAll, isSilent: true);
    }

    private void fetchPruneAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
        DoPull(pullAction: GitPullAction.FetchPruneAll, isSilent: true);
    }

    private void DoPull(GitPullAction pullAction, bool isSilent)
    {
        if (isSilent)
        {
            UICommands.StartPullDialogAndPullImmediately(this, pullAction: pullAction);
        }
        else
        {
            UICommands.StartPullDialog(this, pullAction: pullAction);
        }
    }

    private void UpdateSubmodulesStructure()
    {
        if (_submoduleStatusProvider is null)
        {
            return;
        }

        ToolTip.SetTip(toolStripButtonLevelUp, string.Empty);
        string workingDirectory = Module.WorkingDir;
        CancellationToken cancellationToken = _loadOperationsCancellationTokenSource.Token;
        _loadOperations.FileAndForget(async () =>
        {
            try
            {
                await _submoduleStatusProvider.UpdateSubmodulesStructureAsync(
                    workingDirectory,
                    TranslatedStrings.NoBranch,
                    updateStatus: AppSettings.ShowSubmoduleStatus);
                if (AppSettings.ShowSubmoduleStatus)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    IReadOnlyList<GitItemStatus> status = new GitModule(
                        UICommands.GetRequiredService<IGitExecutorProvider>(),
                        workingDirectory).GetAllChangedFilesWithSubmodulesStatus(cancellationToken);
                    await _submoduleStatusProvider.UpdateSubmodulesStatusAsync(workingDirectory, status, forceUpdate: true);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (GitCommands.Config.GitConfigurationException exception)
            {
                await _loadOperations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                MessageBoxes.ShowGitConfigurationExceptionMessage(this, exception);
            }
        });
    }

    private void RevisionInfo_CommandClicked(object? sender, CommandEventArgs e)
    {
        // TODO this code duplicated in FormFileHistory.Blame_CommandClick
        switch (e.Command)
        {
            case "gotocommit":
                Validates.NotNull(e.Data);
                if (!Module.TryResolvePartialCommitId(e.Data, out ObjectId commitId)
                    || !RevisionGrid.SetSelectedRevision(commitId))
                {
                    if (commitId.IsZero)
                    {
                        return;
                    }

                    // This may occur at various filters, like AppSettings.ShowOnlyFirstParent
                    // will hide other than the first parent.
                    MessageBoxes.RevisionFilteredInGrid(this, commitId);
                }

                break;
            case "gotobranch":
            case "gototag":
                Validates.NotNull(e.Data);
                CommitData? commit = _commitDataManager.GetCommitData(e.Data);
                if (commit is null)
                {
                    break;
                }

                if (!RevisionGrid.SetSelectedRevision(commit.ObjectId))
                {
                    MessageBoxes.RevisionFilteredInGrid(this, commit.ObjectId);
                }

                break;
            case "navigatebackward":
                RevisionGrid.NavigateBackward();
                break;
            case "navigateforward":
                RevisionGrid.NavigateForward();
                break;
            default:
                throw new InvalidOperationException($"unexpected internal link: {e.Command}/{e.Data}");
        }
    }

    private void SubmoduleToolStripButtonClick(object? sender, EventArgs e)
    {
        if (sender is not MenuItem { Tag: string path })
        {
            return;
        }

        if (!Directory.Exists(path))
        {
            MessageBoxes.SubmoduleDirectoryDoesNotExist(this, path);
            return;
        }

        SetWorkingDir(path);
    }

    private MenuItem CreateSubmoduleMenuItem(SubmoduleInfo info, string textFormat = "{0}")
    {
        MenuItem item = new()
        {
            Width = 200,
            Header = string.Format(textFormat, info.Text),
            Tag = info.Path,
            Icon = CreateSubmoduleMenuIcon(Properties.Images.FolderSubmodule),
            FontWeight = info.Bold ? FontWeight.Bold : FontWeight.Normal,
        };
        item.Click += SubmoduleToolStripButtonClick;
        return item;
    }

    private static void UpdateSubmoduleMenuItemStatus(MenuItem item, SubmoduleInfo info, string textFormat = "{0}")
    {
        if (info.Detailed is null)
        {
            return;
        }

        item.Icon = CreateSubmoduleMenuIcon(GetSubmoduleItemImage(info.Detailed));
        item.Header = string.Format(textFormat, info.Text + info.Detailed.AddedAndRemovedText);

        static IImage GetSubmoduleItemImage(DetailedSubmoduleInfo details)
        {
            return (details.Status, details.IsDirty) switch
            {
                (null, _) => Properties.Images.FolderSubmodule,
                (SubmoduleStatus.FastForward, true) => Properties.Images.SubmoduleRevisionUpDirty,
                (SubmoduleStatus.FastForward, false) => Properties.Images.SubmoduleRevisionUp,
                (SubmoduleStatus.Rewind, true) => Properties.Images.SubmoduleRevisionDownDirty,
                (SubmoduleStatus.Rewind, false) => Properties.Images.SubmoduleRevisionDown,
                (SubmoduleStatus.NewerTime, true) => Properties.Images.SubmoduleRevisionSemiUpDirty,
                (SubmoduleStatus.NewerTime, false) => Properties.Images.SubmoduleRevisionSemiUp,
                (SubmoduleStatus.OlderTime, true) => Properties.Images.SubmoduleRevisionSemiDownDirty,
                (SubmoduleStatus.OlderTime, false) => Properties.Images.SubmoduleRevisionSemiDown,
                (_, true) => Properties.Images.SubmoduleDirty,
                (_, false) => Properties.Images.FileStatusModified,
            };
        }
    }

    private static Image CreateSubmoduleMenuIcon(IImage image)
        => new() { Width = 16, Height = 16, Source = image };

    private void SubmoduleStatusProvider_StatusUpdating(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            RemoveSubmoduleButtons();
            SubmoduleFlyout.Items.Add(new MenuItem { Header = _loading.Text });
        });
    }

    private void SubmoduleStatusProvider_StatusUpdated(object? sender, SubmoduleStatusEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (e.Token.IsCancellationRequested)
            {
                return;
            }

            if (e.StructureUpdated || _currentSubmoduleMenuItems is null)
            {
                _currentSubmoduleMenuItems = PopulateToolbar(e.Info);
            }

            UpdateSubmoduleMenuStatus(e.Info);
        });
    }

    private List<MenuItem> PopulateToolbar(SubmoduleInfoResult result)
    {
        RemoveSubmoduleButtons();
        List<MenuItem> newItems = [.. result.OurSubmodules.Select(submodule => CreateSubmoduleMenuItem(submodule))];

        if (result.OurSubmodules.Count == 0)
        {
            newItems.Add(new MenuItem { Header = _noSubmodulesPresent.Text });
        }

        foreach (MenuItem item in newItems)
        {
            SubmoduleFlyout.Items.Add(item);
        }

        if (result.SuperProject is not null)
        {
            SubmoduleFlyout.Items.Add(new Separator());
            if (result.TopProject is not null && result.TopProject != result.SuperProject)
            {
                MenuItem topProjectItem = CreateSubmoduleMenuItem(result.TopProject, _topProjectModuleFormat.Text);
                newItems.Add(topProjectItem);
                SubmoduleFlyout.Items.Add(topProjectItem);
            }

            MenuItem superProjectItem = CreateSubmoduleMenuItem(result.SuperProject, _superprojectModuleFormat.Text);
            newItems.Add(superProjectItem);
            SubmoduleFlyout.Items.Add(superProjectItem);
            foreach (SubmoduleInfo submodule in result.AllSubmodules)
            {
                MenuItem item = CreateSubmoduleMenuItem(submodule);
                newItems.Add(item);
                SubmoduleFlyout.Items.Add(item);
            }

            ToolTip.SetTip(toolStripButtonLevelUp, _goToSuperProject.Text);
        }

        SubmoduleFlyout.Items.Add(new Separator());
        MenuItem updateAllItem = new()
        {
            Header = updateAllSubmodulesToolStripMenuItem.Header,
            Icon = CreateSubmoduleMenuIcon(Properties.Images.SubmodulesUpdate),
        };
        updateAllItem.Click += UpdateAllSubmodulesToolStripMenuItemClick;
        SubmoduleFlyout.Items.Add(updateAllItem);

        if (result.CurrentSubmoduleName is not null)
        {
            MenuItem updateCurrentItem = new()
            {
                Width = 200,
                Header = _updateCurrentSubmodule.Text,
                Tag = Module.WorkingDir,
                Icon = CreateSubmoduleMenuIcon(Properties.Images.FolderSubmodule),
            };
            updateCurrentItem.Click += UpdateSubmoduleToolStripMenuItemClick;
            SubmoduleFlyout.Items.Add(updateCurrentItem);
        }

        return newItems;
    }

    private void UpdateSubmoduleMenuStatus(SubmoduleInfoResult result)
    {
        if (_currentSubmoduleMenuItems is null)
        {
            return;
        }

        Validates.NotNull(result.TopProject);
        Dictionary<string, SubmoduleInfo> infos = result.AllSubmodules.ToDictionary(info => info.Path, info => info);
        infos[result.TopProject.Path] = result.TopProject;
        foreach (MenuItem item in _currentSubmoduleMenuItems)
        {
            if (item.Tag is not string path)
            {
                continue;
            }

            if (infos.TryGetValue(path, out SubmoduleInfo? info))
            {
                UpdateSubmoduleMenuItemStatus(item, info);
            }
            else
            {
                DebugHelpers.Fail($"Status info for {path} ({1 + result.AllSubmodules.Count} records) has no match in current nodes ({_currentSubmoduleMenuItems.Count})");
            }
        }
    }

    private void RemoveSubmoduleButtons()
    {
        foreach (MenuItem item in SubmoduleFlyout.Items.OfType<MenuItem>())
        {
            item.Click -= SubmoduleToolStripButtonClick;
            item.Click -= UpdateAllSubmodulesToolStripMenuItemClick;
            item.Click -= UpdateSubmoduleToolStripMenuItemClick;
        }

        SubmoduleFlyout.Items.Clear();
        _currentSubmoduleMenuItems = null;
    }

    private MenuFlyout SubmoduleFlyout => (MenuFlyout)toolStripButtonLevelUp.Flyout!;

    private void toolStripButtonLevelUp_ButtonClick(object? sender, EventArgs e)
    {
        if (Module.SuperprojectModule is not null)
        {
            SetWorkingDir(Module.SuperprojectModule.WorkingDir);
        }
        else
        {
            toolStripButtonLevelUp.ShowDropDown();
        }
    }

    /// <summary>
    ///  Adds a tab with a console interface over the current working copy. Recreates the
    ///  terminal when the tab is activated again after the shell exits.
    /// </summary>
    private void FillTerminalTab()
    {
        // If terminal control already exists, just focus it
        // Check if there are available console emulators
        // Delay-create the terminal window when the tab is first selected
        if (!AppSettings.ShowConEmuTab.Value
            || _consoleEmulatorsRegistry is null
            || _consoleEmulatorsRegistry.AvailableConsoleEmulators.Count == 0
            || _consoleTabPage is not null)
        {
            return;
        }

        // We have to set ImageKey after it's added to the tab control
        _consoleTabPage = new TabItem
        {
            Header = _consoleTabCaption.Text,
            Name = _consoleTabCaption.Text,
            Icon = Properties.Images.Console,
        };
        _consoleTabPage.Classes.Add("gitextensions-workspace-tab");
        CommitInfoTabControl.Items.Add(_consoleTabPage);
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

    public void ChangeTerminalActiveFolder(string path)
    {
        if (_terminal?.IsShellRunning is true)
        {
            _terminal.ChangeWorkingDirectory(path);
        }
    }

    private void menuitemSparseWorkingCopy_Click(object sender, EventArgs e)
    {
        UICommands.StartSparseWorkingCopyDialog(this);
    }

    private void toolStripMenuItemReflog_Click(object sender, EventArgs e)
    {
        using FormReflog formReflog = new(UICommands);
        formReflog.ShowDialog(this);
    }

    private void toggleSplitViewLayout_Click(object? sender, EventArgs e)
    {
        RememberWorkspaceDimensions();
        AppSettings.ShowSplitViewLayout = !AppSettings.ShowSplitViewLayout;
        RefreshWorkspaceLayout(selectCommitInfoTab: false);
    }

    private void toggleLeftPanel_Click(object? sender, EventArgs e)
    {
        ColumnDefinition leftColumn = MainSplitContainer.ColumnDefinitions[0];
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

    private void CommitInfoPositionClick(object sender, EventArgs e)
    {
        CommitInfoPosition[] positions = Enum.GetValues<CommitInfoPosition>();
        int next = ((int)AppSettings.CommitInfoPosition + 1) % positions.Length;
        SetCommitInfoPosition((CommitInfoPosition)next);
    }

    private void RefreshMenuShortcutKeys()
    {
        // Avalonia keeps display gestures separately from routed hotkeys. Preserve the
        // original ToolStrip menu text column as well as the form-level command route.
        SetShortcutKeyDisplayString(commitToolStripMenuItem, Command.Commit);
        SetShortcutKeyDisplayString(stashChangesToolStripMenuItem, Command.Stash);
        SetShortcutKeyDisplayString(stashStagedToolStripMenuItem, Command.StashStaged);
        SetShortcutKeyDisplayString(stashPopToolStripMenuItem, Command.StashPop);
        SetShortcutKeyDisplayString(closeToolStripMenuItem, Command.CloseRepository);
        SetShortcutKeyDisplayString(checkoutBranchToolStripMenuItem, Command.CheckoutBranch);
        SetShortcutKeyDisplayString(branchToolStripMenuItem, Command.CreateBranch);
        SetShortcutKeyDisplayString(tagToolStripMenuItem, Command.CreateTag);
        SetShortcutKeyDisplayString(mergeBranchToolStripMenuItem, Command.MergeBranches);
        SetShortcutKeyDisplayString(pullToolStripMenuItem, Command.PullOrFetch);
        SetShortcutKeyDisplayString(pullToolStripMenuItem1, Command.PullOrFetch);
        SetShortcutKeyDisplayString(pushToolStripMenuItem, Command.Push);
        SetShortcutKeyDisplayString(rebaseToolStripMenuItem, Command.Rebase);
        SetShortcutKeyDisplayString(manageWorktreeToolStripMenuItem, Command.ManageWorkTrees);

        fileToolStripMenuItem.RefreshShortcutKeys(Hotkeys);
        helpToolStripMenuItem.RefreshShortcutKeys(Hotkeys);
        toolsToolStripMenuItem.RefreshShortcutKeys(Hotkeys);

        void SetShortcutKeyDisplayString(MenuItem item, Command command)
        {
            item.InputGesture = KeysMapper.ToKeyGesture(GetShortcutKeys(command));
            WinFormsToolStripMenuSizer.SetShortcutDisplayString(item, GetShortcutKeyDisplayString(command));
        }
    }

    private void SetCommitInfoPosition(CommitInfoPosition position)
    {
        RememberWorkspaceDimensions();
        AppSettings.CommitInfoPosition = position;
        RefreshWorkspaceLayout(refreshCommitInfoPositionToolTip: true);
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

    private void manageWorktreeToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        using FormManageWorktree form = new(UICommands);
        form.ShowDialog(this);
        if (form.ShouldRefreshRevisionGrid)
        {
            RefreshToolStripMenuItemClick(this, EventArgs.Empty);
        }
    }

    protected override bool CloseOnEscape => false;

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // Avalonia does not execute a closed submenu item's InputGesture, so route the
        // original static F12 accelerator through the owning form.
        MenuItem gitCommandLog = toolsToolStripMenuItem.GetTestAccessor().GitCommandLogMenuItem;
        if (gitCommandLog.InputGesture?.Matches(e) == true)
        {
            gitCommandLog.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, gitCommandLog));
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_hasRuntimeCommands)
        {
            PluginRegistry.Unregister(UICommands);
            UICommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
        }

        _loadOperationsCancellationTokenSource.Cancel();
        if (_submoduleStatusProvider is not null)
        {
            _submoduleStatusProvider.StatusUpdating -= SubmoduleStatusProvider_StatusUpdating;
            _submoduleStatusProvider.StatusUpdated -= SubmoduleStatusProvider_StatusUpdated;
        }

        _submoduleStatusProvider?.Init();
        _splitterManager?.SaveSplitters();
        _gpgInfoLoadSequence.Dispose();
        _gitStatusMonitor?.Dispose();
        RevisionGrid.CancelBackgroundTasks();
        revisionDiff.CancelBackgroundTasks();
        fileTree.CancelBackgroundTasks();
        _loadOperations.JoinPendingOperations();
        _loadOperationsCancellationTokenSource.Dispose();
        (_terminal as IDisposable)?.Dispose();
        _terminal = null;
        _outputHistoryController?.Dispose();
        _outputHistoryController = null;
        _formBrowseMenus?.Dispose();
        _formBrowseMenus = null;
        base.OnClosed(e);
    }

    internal void PopulatePluginMenuForTest() => PopulatePluginMenu();
    internal void UpdateRepositoryHostsMenuForTest() => UpdateRepositoryHostsMenu();
    internal Task JoinLoadOperationsForTestAsync(CancellationToken cancellationToken = default)
        => _loadOperations.JoinPendingOperationsAsync(cancellationToken);

    internal FileStatusList fileStatusList => revisionDiff.FileStatusList;
    internal Editor.FileViewer fileViewer => revisionDiff.FileViewer;

    private void WorktreeToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (sender is not MenuItem { Tag: string path })
        {
            return;
        }

        if (!Directory.Exists(path))
        {
            MessageBoxes.ShowError(this, string.Format(TranslatedStrings.WorktreeDirectoryNotFound, path), TranslatedStrings.Error);
            return;
        }

        SetWorkingDir(Path.GetFullPath(path));
    }

    private void undoLastCommitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        bool confirmed = MessageBoxes.ConfirmSuppressible(this, _undoLastCommitText.Text, _undoLastCommitCaption.Text, AppSettings.DontConfirmUndoLastCommit, icon: TaskDialogIcon.Warning);

        if (!confirmed)
        {
            return;
        }

        ArgumentString arguments = Commands.Reset(ResetMode.Soft, "HEAD~1");
        Module.GitExecutable.RunCommand(arguments);
        RefreshToolStripMenuItemClick(refreshToolStripMenuItem, EventArgs.Empty);
        RefreshGitStatusMonitor();
    }

    private void fileToolStripMenuItem_RecentRepositoriesCleared(object sender, EventArgs e)
    {
        _dashboard?.RefreshContent();
    }
}
