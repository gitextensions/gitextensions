using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using System.Globalization;
using ConEmu.WinForms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Gpg;
using GitCommands.Submodules;
using GitCommands.UserRepositoryHistory;
using GitCommands.Utils;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Avatars;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.CommandsDialogs.WorktreeDialog;
using GitUI.HelperDialogs;
using GitUI.Infrastructure.Telemetry;
using GitUI.LeftPanel;
using GitUI.NBugReports;
using GitUI.Properties;
using GitUI.ScriptsEngine;
using GitUI.Shells;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Taskbar;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormBrowse : GitModuleForm, IBrowseRepo
    {
        #region Mnemonics
        /*
            MENUS
            ═════
                    ABEFGIJKLMOQUWXYZ
                C   Commands
                D   Dashboard
                H   Help
                N   Navigate (inserted by FormBrowseMenus)
                P   Plugins
                R   Repository
                S   Start
                T   Tools
                V   View (inserted by FormBrowseMenus)
                    GitHub (inserted dynamically)

            START menu
            ══════════
                    ABDEGHIJKMNPQSTUVWYZ
                C   Create new repository...
                F   Favorite repositories
                L   Clone repository...
                O   Open...
                R   Recent repositories
                X   Exit

            DASHBOARD menu
            ══════════════
                R   Refresh
                S   Recent repositories settings

            REPOSITORY menu
            ═══════════════
                    DFHJLNPQVYZ
                A   Edit .gitattributes
                B   Synchronize all submodules
                C   Close (go to Dashboard)
                E   Edit .git/info/exclude
                G   Git maintenance
                I   Edit .gitignore
                K   Sparse Working Copy
                M   Edit .mailmap
                O   Repository settings
                R   Refresh
                S   Manage submodules...
                T   Remote repositories...
                U   Update all submodules
                W   Worktrees
                X   File Explorer

            REPOSITORY ▷ WORKTREES submenu
            ══════════════════════════════
                C   Create a worktree...
                M   Manage worktrees...

            REPOSITORY ▷ GIT MAINTENANCE submenu
            ════════════════════════════════════
                C   Compress git database
                R   Recover lost objects...
                D   Delete index.lock
                E   Edit .git/config

            COMMANDS menu
            ═════════════
                    JQXZ
                /   Pull/Fetch...
                A   Apply patch...
                B   Create branch...
                C   Commit...
                D   Delete tag...
                E   Rebase...
                F   Format patch...
                G   Show reflog...
                H   View patch file...
                I   Bisect...
                K   Checkout branch...
                L   Delete branch...
                M   Merge branches...
                N   Manage stashes...
                O   Checkout revision...
                P   Push...
                R   Reset changes...
                S   Solve merge conflicts...
                T   Create tag...
                U   Undo last commit
                V   Archive revision...
                W   Clean working directory...
                Y   Cherry pick...

            GITHUB (Repository hosts) menu
            ══════════════════════════════
                    BDEGHIJKLMNOQRSTUVWXYZ
                A   Add upstream remote
                C   Create pull requests...
                F   Fork/Clone repository
                P   View pull requests...

            PLUGINS menu
            ════════════
                S   Plugin Settings

            TOOLS menu
            ══════════
                    ADEFHIJLMNOQRTUVWXYZ
                B   Git bash
                C   Git command log
                G   Git GUI
                K   GitK
                P   PuTTY
                S   Settings

            HELP menu
            ═════════
                    BEFGHIJKLNOPQSVWXZ
                A   About
                C   Changelog
                D   Donate
                M   User manual
                R   Report an issue
                T   Translate
                U   Check for updates
                Y   Yes, I allow telemetry
        */
        #endregion

        #region Translation

        private readonly TranslationString _closeAll = new("Close all windows");

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

        private readonly TranslationString _commitButtonText = new("Commit");

        private readonly TranslationString _undoLastCommitText = new("You will still be able to find all the commit's changes in the staging area\n\nDo you want to continue?");
        private readonly TranslationString _undoLastCommitCaption = new("Undo last commit");

        #endregion

        private readonly uint _closeAllMessage = NativeMethods.RegisterWindowMessageW("Global.GitExtensions.CloseAllInstances");
        private readonly SplitterManager _splitterManager;
        private readonly GitStatusMonitor _gitStatusMonitor;
        private readonly FormBrowseMenus _formBrowseMenus;
        private readonly IFormBrowseController _controller;
        private readonly ICommitDataManager _commitDataManager;
        private readonly IAppTitleGenerator _appTitleGenerator;
        private readonly IAheadBehindDataProvider? _aheadBehindDataProvider;
        private readonly IWindowsJumpListManager _windowsJumpListManager;
        private readonly ISubmoduleStatusProvider _submoduleStatusProvider;
        private readonly IScriptsManager _scriptsManager;
        private readonly IRepositoryHistoryUIService _repositoryHistoryUIService;
        private List<ToolStripItem>? _currentSubmoduleMenuItems;
        private readonly FormBrowseDiagnosticsReporter _formBrowseDiagnosticsReporter;
        private BuildReportTabPageExtension? _buildReportTabPageExtension;
        private readonly ShellProvider _shellProvider = new();
        private ConEmuControl? _terminal;
        private Dashboard? _dashboard;
        private bool _isFileHistoryMode;
        private bool _fileBlameHistoryLeftPanelStartupState;

        private TabPage? _consoleTabPage;

        private readonly Dictionary<Brush, Icon> _overlayIconByBrush = [];

        private UpdateTargets _selectedRevisionUpdatedTargets = UpdateTargets.None;

        public RevisionGridControl RevisionGridControl => RevisionGrid;

        /// <summary>
        /// Open Browse - main GUI including dashboard.
        /// </summary>
        /// <param name="commands">The commands in the current form.</param>
        /// <param name="args">The start up arguments.</param>
        public FormBrowse(IGitUICommands commands, BrowseArguments args)
#pragma warning disable CS0618 // Type or member is obsolete
            : this(commands, args, new AppSettingsPath("FormBrowse"))
#pragma warning restore CS0618 // Type or member is obsolete
        {
        }

        [Obsolete("Test only!")]
        internal FormBrowse(IGitUICommands commands, BrowseArguments args, SettingsSource settingsSource)
            : base(commands)
        {
            _splitterManager = new(settingsSource);

            SystemEvents.SessionEnding += (sender, args) => SaveApplicationSettings();

            _isFileHistoryMode = args.IsFileHistoryMode;
            InitializeComponent();

            _repositoryHistoryUIService = commands.GetRequiredService<IRepositoryHistoryUIService>();

            fileToolStripMenuItem.Initialize(() => UICommands);
            helpToolStripMenuItem.Initialize(() => UICommands);
            toolsToolStripMenuItem.Initialize(() => UICommands);
            _NO_TRANSLATE_WorkingDir.Initialize(() => UICommands, _repositoryHistoryUIService, fileToolStripMenuItem, closeToolStripMenuItem);

            _repositoryHistoryUIService.GitModuleChanged += SetGitModule;

            BackColor = OtherColors.BackgroundColor;

            WorkaroundPaddingIncreaseBug();

            _appTitleGenerator = commands.GetRequiredService<IAppTitleGenerator>();
            _windowsJumpListManager = commands.GetRequiredService<IWindowsJumpListManager>();
            _scriptsManager = commands.GetRequiredService<IScriptsManager>();

            _formBrowseDiagnosticsReporter = new FormBrowseDiagnosticsReporter(this);

            _repositoryHostsToolStripMenuItem.Visible = false;

            MainSplitContainer.Visible = false;
            MainSplitContainer.SplitterDistance = DpiUtil.Scale(260);

            InitCountArtificial(out _gitStatusMonitor);

            _formBrowseMenus = new FormBrowseMenus(mainMenuStrip);

            RevisionGrid.SuspendRefreshRevisions();

            ToolStripFilters.Bind(() => Module, RevisionGrid);

            InitRevisionGrid(args.SelectedId, args.FirstId, args.IsFileHistoryMode);
            InitCommitDetails();

            InitializeComplete();

            HotkeysEnabled = true;
            LoadHotkeys(HotkeySettingsName);
            SetShortcutKeyDisplayStringsFromHotkeySettings();
            InitMenusAndToolbars(args.RevFilter, args.PathFilter.ToPosixPath());

            UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
            UICommands.BrowseRepo = this;

            _controller = new FormBrowseController(new GitGpgController(() => Module));
            _commitDataManager = new CommitDataManager(() => Module);

            _submoduleStatusProvider = commands.GetRequiredService<ISubmoduleStatusProvider>();
            _submoduleStatusProvider.StatusUpdating += SubmoduleStatusProvider_StatusUpdating;
            _submoduleStatusProvider.StatusUpdated += SubmoduleStatusProvider_StatusUpdated;

            foreach (Control control in this.FindDescendants())
            {
                control.AllowDrop = true;
                control.DragEnter += FormBrowse_DragEnter;
                control.DragDrop += FormBrowse_DragDrop;
            }

            _aheadBehindDataProvider = new AheadBehindDataProvider(() => Module.GitExecutable);
            toolStripButtonPush.ResetToDefaultState();
            repoObjectsTree.Initialize(_aheadBehindDataProvider, filterRevisionGridBySpaceSeparatedRefs: ToolStripFilters.SetBranchFilter, refsSource: RevisionGrid, revisionGridInfo: RevisionGrid);
            revisionDiff.Bind(revisionGridInfo: RevisionGrid, revisionGridUpdate: RevisionGrid, revisionFileTree: fileTree, () => RevisionGrid.CurrentFilter.PathFilter, RefreshGitStatusMonitor);

            // Show blame by default in file tree if not started from command line
            fileTree.Bind(revisionGridInfo: RevisionGrid, revisionGridUpdate: RevisionGrid, RefreshGitStatusMonitor, _isFileHistoryMode);
            RevisionGrid.ResumeRefreshRevisions();

            // Application is init, the repo related operations are triggered in OnLoad()
            return;

            void InitCountArtificial(out GitStatusMonitor gitStatusMonitor)
            {
                Brush? lastBrush = null;

                gitStatusMonitor = new GitStatusMonitor(this, () => IsMinimized());
                if (!NeedsGitStatusMonitor())
                {
                    gitStatusMonitor.Active = false;
                }

                gitStatusMonitor.GitStatusMonitorStateChanged += (s, e) =>
                {
                    GitStatusMonitorState status = e.State;
                    if (status == GitStatusMonitorState.Stopped)
                    {
                        // fall back to operation without info in the button
                        UpdateCommitButtonAndGetBrush(null, showCount: false);
                        RevisionGrid.UpdateArtificialCommitCount(null);
                        if (EnvUtils.RunningOnWindowsWithMainWindow())
                        {
                            TaskbarManager.Instance.SetOverlayIcon(null, "");
                        }

                        lastBrush = null;
                    }
                };

                gitStatusMonitor.GitWorkingDirectoryStatusChanged += (s, e) =>
                {
                    IReadOnlyList<GitItemStatus>? status = e?.ItemStatuses;

                    bool countToolbar = AppSettings.ShowGitStatusInBrowseToolbar;
                    bool countArtificial = AppSettings.ShowGitStatusForArtificialCommits && AppSettings.RevisionGraphShowArtificialCommits;

                    Brush brush = UpdateCommitButtonAndGetBrush(status, countToolbar);

                    RevisionGrid.UpdateArtificialCommitCount(countArtificial ? status : null);

                    if (countToolbar || countArtificial)
                    {
                        UpdateStatusInTaskbar();

                        if (AppSettings.ShowSubmoduleStatus)
                        {
                            Validates.NotNull(_submoduleStatusProvider);

                            ThreadHelper.FileAndForget(async () =>
                            {
                                try
                                {
                                    await _submoduleStatusProvider.UpdateSubmodulesStatusAsync(Module.WorkingDir, status);
                                }
                                catch (GitConfigurationException ex)
                                {
                                    await this.SwitchToMainThreadAsync();
                                    MessageBoxes.ShowGitConfigurationExceptionMessage(this, ex);
                                }
                            });
                        }
                    }

                    return;

                    void UpdateStatusInTaskbar()
                    {
                        if (!EnvUtils.RunningOnWindowsWithMainWindow())
                        {
                            return;
                        }

                        if (ReferenceEquals(brush, lastBrush))
                        {
                            TaskbarManager.Instance.SetOverlayIcon(_overlayIconByBrush[lastBrush], "");
                            return;
                        }

                        lastBrush = brush;

                        if (!_overlayIconByBrush.TryGetValue(brush, out Icon overlay))
                        {
                            const int imgDim = 32;
                            const int dotDim = 15;
                            const int pad = 2;
                            using Bitmap bmp = new(imgDim, imgDim);
                            using Graphics g = Graphics.FromImage(bmp);
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.Clear(Color.Transparent);
                            g.FillEllipse(brush, new Rectangle(imgDim - dotDim - pad, imgDim - dotDim - pad, dotDim, dotDim));

                            overlay = bmp.ToIcon();
                            _overlayIconByBrush.Add(brush, overlay);
                        }

                        TaskbarManager.Instance.SetOverlayIcon(overlay, "");

                        _windowsJumpListManager.UpdateCommitIcon(toolStripButtonCommit.Image);
                    }
                };
            }

            bool IsMinimized() => WindowState == FormWindowState.Minimized;

            void WorkaroundPaddingIncreaseBug()
            {
                MainSplitContainer.Panel1.Padding = new Padding(1);
                RevisionsSplitContainer.Panel1.Padding = new Padding(1);
                RevisionsSplitContainer.Panel2.Padding = new Padding(1);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_repositoryHistoryUIService is not null)
                {
                    _repositoryHistoryUIService.GitModuleChanged -= SetGitModule;
                }

                _formBrowseMenus?.Dispose();
                components?.Dispose();
                _gitStatusMonitor?.Dispose();
                _windowsJumpListManager?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnApplicationActivated()
        {
            if (AppSettings.RefreshArtificialCommitOnApplicationActivated && CommitInfoTabControl.SelectedTab == DiffTabPage)
            {
                revisionDiff.RefreshArtificial();
            }

            base.OnApplicationActivated();
        }

        protected override void OnLoad(EventArgs e)
        {
            _formBrowseMenus.CreateToolbarsMenus(ToolStripMain, ToolStripFilters, ToolStripScripts);

            RefreshSplitViewLayout();
            LayoutRevisionInfo();
            SetSplitterPositions();

            base.OnLoad(e);

            _formBrowseDiagnosticsReporter.Report();

            // All app init is done, make all repo related similar to switching repos
            SetGitModule(this, new GitModuleEventArgs(new GitModule(Module.WorkingDir)));
            bool isDashboard = _dashboard?.Visible ?? false;
            ThreadHelper.FileAndForget(async () =>
            {
                if (isDashboard)
                {
                    // Load only the git hoster plugin to quickly provide related features in dashboard
                    PluginRegistry.InitializeGitHostersOnly();

                    await this.SwitchToMainThreadAsync();

                    // Now that hoster plugin is registered, populate Git-host-plugin actions on Dashboard, like "Clone GitHub repository"
                    UpdateRepositoryHostsMenu();

                    // Check if during plugin loading user left dashboard
                    if (_dashboard.Visible)
                    {
                        _dashboard.RefreshContent();
                    }

                    // Come back to background to load all the other plugins (that could be long...)
                    await TaskScheduler.Default;
                }

                await InitializeAndRegisterAllPluginsAsync();
            });

            return;

            async Task InitializeAndRegisterAllPluginsAsync()
            {
                PluginRegistry.InitializeAll();
                await this.SwitchToMainThreadAsync();
                RegisterPlugins();
                UpdateRepositoryHostsMenu();
            }

            void UpdateRepositoryHostsMenu()
            {
                if (PluginRegistry.GitHosters.Count != 0)
                {
                    // TODO: support more than one Git hosting provider plugin (ADO, GitLab,...)
                    _repositoryHostsToolStripMenuItem.Text = PluginRegistry.GitHosters[0].Name;
                }

                // Show "Repository hosts" menu item when there is at least 1 repository host plugin loaded
                _repositoryHostsToolStripMenuItem.Visible = PluginRegistry.GitHosters.Count != 0;
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            // wait for windows to really be displayed, which isn't necessarily the case in OnLoad()
            if (_windowsJumpListManager.NeedsJumpListCreation)
            {
                _windowsJumpListManager.CreateJumpList(
                    Handle,
                    new WindowsThumbnailToolbarButtons(
                        new WindowsThumbnailToolbarButton(toolStripButtonCommit.Text, toolStripButtonCommit.Image, CommitToolStripMenuItemClick),
                        new WindowsThumbnailToolbarButton(toolStripButtonPush.Text, toolStripButtonPush.Image, PushToolStripMenuItemClick),
                        new WindowsThumbnailToolbarButton(toolStripButtonPull.Text, toolStripButtonPull.Image, PullToolStripMenuItemClick),
                        new WindowsThumbnailToolbarButton(_closeAll.Text, Images.DeleteFile, (s, e) => NativeMethods.PostMessageW(NativeMethods.HWND_BROADCAST, _closeAllMessage))));
            }

            this.InvokeAndForget(OnActivate);
            base.OnActivated(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveApplicationSettings();

            foreach (Control control in this.FindDescendants())
            {
                control.DragEnter -= FormBrowse_DragEnter;
                control.DragDrop -= FormBrowse_DragDrop;
            }

            base.OnFormClosing(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Restore state at startup if file history mode, ignore the forced setting
            if (_isFileHistoryMode)
            {
                MainSplitContainer.Panel1Collapsed = _fileBlameHistoryLeftPanelStartupState;
            }

            _splitterManager.SaveSplitters();
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            PluginRegistry.Unregister(UICommands);
            RevisionGrid.CancelBackgroundTasks();
            base.OnClosed(e);
        }

        protected override void OnUICommandsChanged(GitUICommandsChangedEventArgs e)
        {
            IGitUICommands oldCommands = e.OldCommands;
            RefreshDefaultPullAction();

            if (oldCommands is not null)
            {
                oldCommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
                oldCommands.BrowseRepo = null;
            }

            UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
            UICommands.BrowseRepo = this;

            base.OnUICommandsChanged(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == _closeAllMessage || m is { Msg: NativeMethods.WM_SYSCOMMAND, WParam: NativeMethods.SC_CLOSE })
            {
                // Application close is requested, e.g. using the Taskbar context menu.
                // This request is directed to the main form also if a modal form like FormCommit is on top.
                // So forward the request and try to close the modal form.
                Form? modalForm = Application.OpenForms.Cast<Form>().FirstOrDefault(form => form.Modal);
                if (modalForm is not null)
                {
                    modalForm.Close();
                }

                Close();
            }

            base.WndProc(ref m);
        }

        public override void AddTranslationItems(ITranslation translation)
        {
            base.AddTranslationItems(translation);
            TranslationUtils.AddTranslationItemsFromFields(Name, ToolStripFilters, translation);
        }

        public override void TranslateItems(ITranslation translation)
        {
            base.TranslateItems(translation);
            TranslationUtils.TranslateItemsFromFields(Name, ToolStripFilters, translation);
        }

        public override void CancelButtonClick(object sender, EventArgs e)
        {
            // If a filter is applied, clear it
            if (RevisionGrid.FilterIsApplied())
            {
                // Clear filter
                ToolStripFilters.SetRevisionFilter(string.Empty);
            }
        }

        private static bool NeedsGitStatusMonitor()
        {
            return AppSettings.ShowGitStatusInBrowseToolbar || (AppSettings.ShowGitStatusForArtificialCommits && AppSettings.RevisionGraphShowArtificialCommits);
        }

        private void UICommands_PostRepositoryChanged(object sender, GitUIEventArgs e)
        {
            // Note that this called in most FormBrowse context to "be sure"
            // that the repo has not been updated externally.

            // It can also be called from background tasks, e.g. from BackgroundFetchPlugin.
            if (!ThreadHelper.JoinableTaskContext.IsOnMainThread)
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                    {
                        await this.SwitchToMainThreadAsync();
                        RefreshRevisions(e.GetRefs);
                    });
                return;
            }

            RefreshRevisions(e.GetRefs);
        }

        /// <summary>
        /// Refresh revisions, also handling changes external to GE.
        /// </summary>
        private void RefreshRevisions()
        {
            RefreshRevisions(new FilteredGitRefsProvider(UICommands.Module).GetRefs);
        }

        /// <summary>
        /// Refresh revisions, also handling changes external to GE.
        /// </summary>
        /// <param name="getRefs">(Lazy) func to get refs.</param>
        private void RefreshRevisions(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
        {
            if (RevisionGrid.IsDisposed || IsDisposed || Disposing)
            {
                return;
            }

            _aheadBehindDataProvider.ResetCache();
            bool isDashboard = string.IsNullOrEmpty(Module.WorkingDir) || (_dashboard?.Visible ?? false);
            if (isDashboard)
            {
                // Explicit call: Title is normally updated on RevisionGrid filter change
                Text = _appTitleGenerator.Generate();

                // "Repo" related methods, creates _dashboard
                InternalInitialize();

                return;
            }

            RevisionGrid.PerformRefreshRevisions(getRefs, forceRefresh: true);

            InternalInitialize();
            ToolStripFilters.RefreshRevisionFunction(getRefs);
            UpdateSubmodulesStructure();

            RefreshGitStatusMonitor();
            UpdateStashCount();
        }

        private void RefreshGitStatusMonitor() => _gitStatusMonitor?.RequestRefresh();

        private void RefreshSelection()
        {
            IReadOnlyList<GitRevision> selectedRevisions = RevisionGrid.GetSelectedRevisions();
            GitRevision? selectedRevision = selectedRevisions.Count > 0 ? selectedRevisions[0] : null;

            FillFileTree(selectedRevision);
            FillDiff(selectedRevisions);

            string oldBody = selectedRevision?.Body;
            FillCommitInfo(selectedRevision);

            // If the revision's body has been updated then the grid needs to be refreshed to display it
            if (AppSettings.ShowCommitBodyInRevisionGrid && selectedRevision?.HasMultiLineMessage is true && oldBody != selectedRevision.Body)
            {
                RevisionGrid.Refresh();
            }

            revisionGpgInfo1.InvokeAndForget(() => FillGpgInfoAsync(selectedRevision));
            FillBuildReport(selectedRevision);
            repoObjectsTree.SelectionChanged(selectedRevisions);
        }

        #region IBrowseRepo

        public GitRevision? GetLatestSelectedRevision() => RevisionGrid.LatestSelectedRevision;
        public IReadOnlyList<GitRevision> GetSelectedRevisions() => RevisionGrid.GetSelectedRevisions();
        public Point GetQuickItemSelectorLocation() => RevisionGrid.GetQuickItemSelectorLocation();
        public void GoToRef(string refName, bool showNoRevisionMsg, bool toggleSelection = false) => RevisionGrid.GoToRef(refName, showNoRevisionMsg, toggleSelection);

        #endregion

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
            toolPanel.SuspendLayout();
            toolPanel.TopToolStripPanelVisible = false;
            toolPanel.BottomToolStripPanelVisible = false;
            toolPanel.LeftToolStripPanelVisible = false;
            toolPanel.RightToolStripPanelVisible = false;
            toolPanel.ResumeLayout();

            MainSplitContainer.Visible = false;

            if (_dashboard is null)
            {
                _dashboard = new Dashboard { Dock = DockStyle.Fill };
                _dashboard.GitModuleChanged += SetGitModule;
                toolPanel.ContentPanel.Controls.Add(_dashboard);
            }

            Text = _appTitleGenerator.Generate(branchName: TranslatedStrings.NoBranch);

            _dashboard.RefreshContent();
            _dashboard.Visible = true;
            _dashboard.BringToFront();

            _createPullRequestsToolStripMenuItem.Enabled = false;
            _viewPullRequestsToolStripMenuItem.Enabled = false;
            _addUpstreamRemoteToolStripMenuItem.Enabled = false;

            DiagnosticsClient.TrackPageView("Dashboard");
        }

        private void HideDashboard()
        {
            MainSplitContainer.Visible = true;
            if (!_dashboard?.Visible ?? true)
            {
                return;
            }

            _dashboard.Visible = false;
            toolPanel.SuspendLayout();
            toolPanel.TopToolStripPanelVisible = true;
            toolPanel.BottomToolStripPanelVisible = true;
            toolPanel.LeftToolStripPanelVisible = true;
            toolPanel.RightToolStripPanelVisible = true;
            toolPanel.ResumeLayout();

            DiagnosticsClient.TrackPageView("Revision graph");
        }

        private void UpdatePluginMenu(bool validWorkingDir)
        {
            foreach (ToolStripItem item in pluginsToolStripMenuItem.DropDownItems)
            {
                if (item == pluginsLoadingToolStripMenuItem)
                {
                    continue;
                }

                item.Enabled = !(item.Tag is IGitPluginForRepository) || validWorkingDir;
            }
        }

        private void RegisterPlugins()
        {
            const string PluginManagerName = "Plugin Manager";
            ILookup<object, ToolStripMenuItem> existingPluginMenus = pluginsToolStripMenuItem.DropDownItems.OfType<ToolStripMenuItem>().ToLookup(c => c.Tag);

            lock (PluginRegistry.Plugins)
            {
                if (PluginRegistry.Plugins.Count == 0)
                {
                    return;
                }

                if (pluginsToolStripMenuItem.DropDownItems.Contains(pluginsLoadingToolStripMenuItem))
                {
                    pluginsToolStripMenuItem.DropDownItems.Remove(pluginsLoadingToolStripMenuItem);
                }

                IOrderedEnumerable<IGitPlugin> pluginEntries = PluginRegistry.Plugins
                    .OrderByDescending(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase);

                // pluginsToolStripMenuItem.DropDownItems menu already contains at least 2 items:
                //    [1] Separator
                //    [0] Plugin Settings
                // insert all plugins except 'Plugin Manager' above the separator
                foreach (IGitPlugin plugin in pluginEntries)
                {
                    // don't add the plugin to the Plugins menu, if already added
                    if (existingPluginMenus.Contains(plugin))
                    {
                        continue;
                    }

                    ToolStripMenuItem item = new()
                    {
                        Text = plugin.Name,
                        Image = plugin.Icon,
                        Tag = plugin
                    };
                    item.Click += delegate
                    {
                        if (plugin.Execute(new GitUIEventArgs(this, UICommands)))
                        {
                            UICommands.RepoChangedNotifier.Notify();
                        }
                    };

                    if (plugin.Name == PluginManagerName)
                    {
                        // insert Plugin Manager below the separator
                        pluginsToolStripMenuItem.DropDownItems.Insert(pluginsToolStripMenuItem.DropDownItems.Count - 1, item);
                    }
                    else
                    {
                        // Handle special case where Git Hosting plugin has already been loaded
                        ToolStripItem first = pluginsToolStripMenuItem.DropDownItems[0];
                        int index = first is ToolStripSeparator || string.CompareOrdinal(first.Text, item.Text) >= 0 ? 0 : 1;

                        pluginsToolStripMenuItem.DropDownItems.Insert(index, item);
                    }
                }

                mainMenuStrip?.Refresh();
            }

            // Allow the plugin to perform any self-registration actions
            PluginRegistry.Register(UICommands);

            UICommands.RaisePostRegisterPlugin(this);

            UpdatePluginMenu(Module.IsValidGitWorkingDir());
        }

        /// <summary>
        /// to avoid showing menu items that should not be there during
        /// the transition from dashboard to repo browser and vice versa
        ///
        /// and reset hotkeys that are shared between mutual exclusive menu items.
        /// </summary>
        private void HideVariableMainMenuItems()
        {
            dashboardToolStripMenuItem.Visible = false;
            repositoryToolStripMenuItem.Visible = false;
            commandsToolStripMenuItem.Visible = false;
            pluginsToolStripMenuItem.Visible = false;
            refreshToolStripMenuItem.ShortcutKeys = Keys.None;
            refreshDashboardToolStripMenuItem.ShortcutKeys = Keys.None;
            _formBrowseMenus.RemoveRevisionGridMainMenuItems();
            mainMenuStrip.Refresh();
        }

        private void InternalInitialize()
        {
            toolPanel.SuspendLayout();
            toolPanel.TopToolStripPanel.SuspendLayout();

            using (WaitCursorScope.Enter())
            {
                // check for updates
                if (AppSettings.CheckForUpdates && AppSettings.LastUpdateCheck.AddDays(7) < DateTime.Now)
                {
                    AppSettings.LastUpdateCheck = DateTime.Now;
                    FormUpdates updateForm = new(AppSettings.AppVersion);
                    updateForm.SearchForUpdatesAndShow(ownerWindow: this, alwaysShow: false);
                }

                bool hasWorkingDir = !string.IsNullOrEmpty(Module.WorkingDir);
                if (hasWorkingDir)
                {
                    HideDashboard();
                }
                else
                {
                    ShowDashboard();
                }

                bool bareRepository = Module.IsBareRepository();
                bool isDashboard = _dashboard?.Visible ?? false;
                bool validBrowseDir = !isDashboard && Module.IsValidGitWorkingDir();

                branchSelect.Text = validBrowseDir
                    ? !string.IsNullOrWhiteSpace(RevisionGrid.CurrentBranch.Value)
                        ? RevisionGrid.CurrentBranch.Value
                        : DetachedHeadParser.DetachedBranch
                    : "";
                toolStripButtonLevelUp.Enabled = hasWorkingDir && !bareRepository;
                CommitInfoTabControl.Visible = validBrowseDir;
                fileExplorerToolStripMenuItem.Enabled = validBrowseDir;
                manageRemoteRepositoriesToolStripMenuItem1.Enabled = validBrowseDir;
                branchSelect.Enabled = validBrowseDir;
                toolStripButtonCommit.Enabled = validBrowseDir && !bareRepository;

                toolStripButtonPull.Enabled = validBrowseDir;
                toolStripButtonPush.Enabled = validBrowseDir;
                toolStripButtonPush.ResetBeforeUpdate();
                dashboardToolStripMenuItem.Visible = isDashboard;
                pluginsToolStripMenuItem.Visible = validBrowseDir;
                repositoryToolStripMenuItem.Visible = validBrowseDir;
                commandsToolStripMenuItem.Visible = validBrowseDir;
                toolStripFileExplorer.Enabled = validBrowseDir;
                if (!isDashboard)
                {
                    refreshToolStripMenuItem.ShortcutKeys = Keys.F5;
                }
                else
                {
                    refreshDashboardToolStripMenuItem.ShortcutKeys = Keys.F5;
                }

                UpdatePluginMenu(validBrowseDir);
                gitMaintenanceToolStripMenuItem.Enabled = validBrowseDir;
                editgitignoreToolStripMenuItem1.Enabled = validBrowseDir;
                editGitAttributesToolStripMenuItem.Enabled = validBrowseDir;
                editmailmapToolStripMenuItem.Enabled = validBrowseDir;
                toolStripSplitStash.Enabled = validBrowseDir && !bareRepository;
                _createPullRequestsToolStripMenuItem.Enabled = validBrowseDir;
                _viewPullRequestsToolStripMenuItem.Enabled = validBrowseDir;
                _addUpstreamRemoteToolStripMenuItem.Enabled = validBrowseDir;

                // repositoryToolStripMenuItem.Visible
                if (validBrowseDir)
                {
                    manageSubmodulesToolStripMenuItem.Enabled = !bareRepository;
                    updateAllSubmodulesToolStripMenuItem.Enabled = !bareRepository;
                    synchronizeAllSubmodulesToolStripMenuItem.Enabled = !bareRepository;
                    editgitignoreToolStripMenuItem1.Enabled = !bareRepository;
                    editGitAttributesToolStripMenuItem.Enabled = !bareRepository;
                    editmailmapToolStripMenuItem.Enabled = !bareRepository;
                }

                // commandsToolStripMenuItem.Visible
                if (validBrowseDir)
                {
                    commitToolStripMenuItem.Enabled = !bareRepository;
                    mergeToolStripMenuItem.Enabled = !bareRepository;
                    rebaseToolStripMenuItem1.Enabled = !bareRepository;
                    pullToolStripMenuItem1.Enabled = !bareRepository;
                    cleanupToolStripMenuItem.Enabled = !bareRepository;
                    stashToolStripMenuItem.Enabled = !bareRepository;
                    checkoutBranchToolStripMenuItem.Enabled = !bareRepository;
                    mergeBranchToolStripMenuItem.Enabled = !bareRepository;
                    rebaseToolStripMenuItem.Enabled = !bareRepository;
                    applyPatchToolStripMenuItem.Enabled = !bareRepository;
                }

                stashChangesToolStripMenuItem.Enabled = !bareRepository;
                stashStagedToolStripMenuItem.Visible = Module.GitVersion.SupportStashStaged;

                toolsToolStripMenuItem.RefreshState(bareRepository);

                _NO_TRANSLATE_WorkingDir.RefreshContent();

                OnActivate();

                LoadUserMenu();
                toolStripButtonLevelUp.Image = validBrowseDir && Module.SuperprojectModule is not null ? Images.NavigateUp : Images.SubmodulesManage;

                if (validBrowseDir)
                {
                    _windowsJumpListManager.AddToRecent(Module.WorkingDir);

                    // add Navigate and View menu
                    _formBrowseMenus.ResetMenuCommandSets();
                    _formBrowseMenus.AddMenuCommandSet(MainMenuItem.NavigateMenu, RevisionGrid.MenuCommands.NavigateMenuCommands);
                    _formBrowseMenus.AddMenuCommandSet(MainMenuItem.ViewMenu, RevisionGrid.MenuCommands.ViewMenuCommands);

                    _formBrowseMenus.InsertRevisionGridMainMenuItems(repositoryToolStripMenuItem);

                    if (AppSettings.ShowAheadBehindData)
                    {
                        string currentBranch = RevisionGrid.CurrentBranch.Value;
                        ThreadHelper.FileAndForget(async () =>
                        {
                            // Always query only current branch here
                            // because, due to race condition with left panel async refresh:
                            // * when there are a lot of branches, we end up here 1st (and so, we want only the current branch data
                            // because getting ahead - behind data for all branches will be (very ?) long
                            // * when there are few branches, we will end up here not in 1st
                            // and the data will be taken from cache (so what we pass as argument is kind of useless)
                            IDictionary<string, AheadBehindData> aheadBehindData = _aheadBehindDataProvider?.GetData(currentBranch);
                            await this.SwitchToMainThreadAsync();
                            toolStripButtonPush.DisplayAheadBehindInformation(aheadBehindData, currentBranch, GetShortcutKeyTooltipString(Command.Push));
                        });
                    }

                    ActiveControl = RevisionGrid;
                }
                else
                {
                    _windowsJumpListManager.DisableThumbnailToolbar();
                }

                UICommands.RaisePostBrowseInitialize(this);
            }

            toolPanel.TopToolStripPanel.ResumeLayout();
            toolPanel.ResumeLayout();

            return;

            void LoadUserMenu()
            {
                List<ScriptInfo> scripts = _scriptsManager.GetScripts()
                    .Where(script => script.Enabled && script.OnEvent == ScriptEvent.ShowInUserMenuBar)
                    .ToList();

                for (int i = ToolStripScripts.Items.Count - 1; i >= 0; i--)
                {
                    if (ToolStripScripts.Items[i].Tag as string == "userscript")
                    {
                        ToolStripScripts.Items.RemoveAt(i);
                    }
                }

                if (scripts.Count == 0)
                {
                    ToolStripScripts.GripStyle = ToolStripGripStyle.Hidden;
                    return;
                }

                ToolStripScripts.GripStyle = ToolStripGripStyle.Visible;
                foreach (ScriptInfo script in scripts)
                {
                    ToolStripButton button = new()
                    {
                        // store scriptname
                        Text = script.Name,
                        Tag = "userscript",
                        Enabled = true,
                        Visible = true,
                        Image = script.GetIcon(),
                        DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
                    };

                    UpdateTooltipWithShortcut(button, (Command)script.HotkeyCommandIdentifier);

                    button.Click += (s, e) => ExecuteCommand(script.HotkeyCommandIdentifier);

                    // add to toolstrip
                    ToolStripScripts.Items.Add(button);
                }
            }
        }

        private void SetShortcutKeyDisplayStringsFromHotkeySettings()
        {
            // Add shortcuts to the menu items
            commitToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.Commit);
            stashChangesToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.Stash);
            stashStagedToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.StashStaged);
            stashPopToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.StashPop);
            closeToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.CloseRepository);
            checkoutBranchToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.CheckoutBranch);
            branchToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.CreateBranch);
            tagToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.CreateTag);
            mergeBranchToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.MergeBranches);
            pullToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.PullOrFetch);
            pullToolStripMenuItem1.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.PullOrFetch);
            pushToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.Push);
            rebaseToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.Rebase);

            fileToolStripMenuItem.RefreshShortcutKeys(Hotkeys);
            helpToolStripMenuItem.RefreshShortcutKeys(Hotkeys);
            toolsToolStripMenuItem.RefreshShortcutKeys(Hotkeys);
            ToolStripFilters.RefreshBrowseDialogShortcutKeys(Hotkeys);
            ToolStripFilters.RefreshRevisionGridShortcutKeys(GetHotkeys(RevisionGridControl.HotkeySettingsName));

            // Set shortcuts on the Browse toolbar with commands in RevGrid
            RevisionGrid.SetFilterShortcutKeys(ToolStripFilters);

            // TODO: add more
            UpdateTooltipWithShortcut(toggleLeftPanel, Command.ToggleLeftPanel);
            UpdateTooltipWithShortcut(toolStripButtonCommit, Command.Commit);
            UpdateTooltipWithShortcut(EditSettings, Command.OpenSettings);
            UpdateTooltipWithShortcut(branchSelect, Command.CheckoutBranch);
            UpdateTooltipWithShortcut(toolStripFileExplorer, fileExplorerToolStripMenuItem.ShortcutKeys);
            UpdateTooltipWithShortcut(RefreshButton, Keys.F5);
            UpdateTooltipWithShortcut(userShell, Command.GitBash);
        }

        private void OnActivate()
        {
            // check if we are in the middle of bisect
            notificationBarBisectInProgress.RefreshBisect();

            // check if we are in the middle of an action (merge/rebase/etc.)
            notificationBarGitActionInProgress.RefreshGitAction(
                checkForConflicts: AppSettings.GitAsyncWhenMinimized || (WindowState != FormWindowState.Minimized));
        }

        private void UpdateStashCount()
        {
            if (AppSettings.ShowStashCount && !Module.IsBareRepository())
            {
                ThreadHelper.FileAndForget(async () =>
                {
                    // Add a delay to not interfere with GUI updates when switching repository
                    await Task.Delay(500);

                    int result = Module.GetStashes(noLocks: true).Count;

                    await this.SwitchToMainThreadAsync();

                    toolStripSplitStash.Text = $"({result})";
                });
            }
            else
            {
                toolStripSplitStash.Text = string.Empty;
            }
        }

        public override IScriptOptionsProvider? GetScriptOptionsProvider()
        {
            if (fileTree.Visible)
            {
                return fileTree.ScriptOptionsProvider;
            }

            if (revisionDiff.Visible)
            {
                return revisionDiff.ScriptOptionsProvider;
            }

            return base.GetScriptOptionsProvider();
        }

        private void FillFileTree(GitRevision revision)
        {
            // Don't show the "File Tree" tab for artificial commits
            bool showFileTreeTab = revision?.IsArtificial != true;

            if (showFileTreeTab)
            {
                if (TreeTabPage.Parent is null)
                {
                    int index = CommitInfoTabControl.TabPages.IndexOf(DiffTabPage);
                    DebugHelpers.Assert(index != -1, "TabControl should contain diff tab page");
                    CommitInfoTabControl.TabPages.Insert(index + 1, TreeTabPage);
                }
            }
            else
            {
                TreeTabPage.Parent = null;
            }

            if (CommitInfoTabControl.SelectedTab != TreeTabPage || _selectedRevisionUpdatedTargets.HasFlag(UpdateTargets.FileTree))
            {
                return;
            }

            _selectedRevisionUpdatedTargets |= UpdateTargets.FileTree;
            fileTree.LoadRevision(revision);
        }

        private void FillDiff(IReadOnlyList<GitRevision> revisions)
        {
            if (CommitInfoTabControl.SelectedTab != DiffTabPage)
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

            if (AppSettings.CommitInfoPosition == CommitInfoPosition.BelowList && CommitInfoTabControl.SelectedTab != CommitInfoTabPage)
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

        private async Task FillGpgInfoAsync(GitRevision? revision)
        {
            // Don't show the "GPG" tab for artificial commits
            bool showGpgInfoTab = revision?.IsArtificial is false && AppSettings.ShowGpgInformation.Value;

            if (!showGpgInfoTab)
            {
                GpgInfoTabPage.Parent = null;
                return;
            }

            if (GpgInfoTabPage.Parent is null)
            {
                int index = CommitInfoTabControl.TabPages.IndexOf(TreeTabPage);
                Debug.Assert(index != -1, "TabControl should contain file tree tab page");
                CommitInfoTabControl.TabPages.Insert(index + 1, GpgInfoTabPage);
            }

            if (!AppSettings.ShowGpgInformation.Value || CommitInfoTabControl.SelectedTab != GpgInfoTabPage)
            {
                return;
            }

            if (revision is null)
            {
                return;
            }

            GpgInfo info = await _controller.LoadGpgInfoAsync(revision);
            revisionGpgInfo1.DisplayGpgInfo(info);
        }

        private void RefreshLeftPanel(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs, Lazy<IReadOnlyCollection<GitRevision>> getStashRevs, bool forceRefresh)
        {
            repoObjectsTree.RefreshRevisionsLoading(getRefs, getStashRevs, forceRefresh);
        }

        private void CheckoutToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCheckoutRevisionDialog(this);
        }

        private void CommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCommitDialog(this);
        }

        private void PushToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartPushDialog(this, pushOnShow: ModifierKeys.HasFlag(Keys.Shift));
        }

        private void RefreshToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Broadcast RepoChanged in case repo was changed outside of GE
            UICommands.RepoChangedNotifier.Notify();
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

        private void userShell_Click(object sender, EventArgs e)
        {
            if (userShell.DropDownButtonPressed)
            {
                return;
            }

            if ((sender as ToolStripItem)?.Tag is not IShellDescriptor shell)
            {
                return;
            }

            try
            {
                Validates.NotNull(shell.ExecutablePath);

                Executable executable = new(shell.ExecutablePath, Module.WorkingDir);
                executable.Start(createWindow: true, throwOnErrorExit: false); // throwOnErrorExit would redirect the output
            }
            catch (Exception exception)
            {
                MessageBoxes.FailedToRunShell(this, shell.Name, exception);
            }
        }

        private void FormatPatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartFormatPatchDialog(this);
        }

        private void CheckoutBranchToolStripMenuItemClick(object sender, EventArgs e)
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

        private void CurrentBranchClick(object sender, EventArgs e)
        {
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

        private void CherryPickToolStripMenuItemClick(object sender, EventArgs e)
        {
            IReadOnlyList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions(SortDirection.Descending);

            UICommands.StartCherryPickDialog(this, revisions);
        }

        private void MergeBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartMergeBranchDialog(this, null);
        }

        private void toolsToolStripMenuItem_SettingsChanged(object sender, Menus.SettingsChangedEventArgs e)
        {
            HandleSettingsChanged(e.OldTranslation, e.OldCommitInfoPosition);
        }

        private void OnShowSettingsClick(object sender, EventArgs e)
        {
            string translation = AppSettings.Translation;
            CommitInfoPosition commitInfoPosition = AppSettings.CommitInfoPosition;

            UICommands.StartSettingsDialog(this);

            HandleSettingsChanged(translation, commitInfoPosition);
        }

        private void HandleSettingsChanged(string oldTranslation, CommitInfoPosition oldCommitInfoPosition)
        {
            if (oldTranslation != AppSettings.Translation)
            {
                Translator.Translate(this, AppSettings.CurrentTranslation);
            }

            if (oldCommitInfoPosition != AppSettings.CommitInfoPosition)
            {
                LayoutRevisionInfo();
            }

            LoadHotkeys(HotkeySettingsName);
            RevisionGrid.ReloadHotkeys();
            RevisionGrid.ReloadTranslation();
            RevisionInfo.ReloadHotkeys();
            fileTree.ReloadHotkeys();
            revisionDiff.ReloadHotkeys();
            repoObjectsTree.ReloadHotkeys();
            SetShortcutKeyDisplayStringsFromHotkeySettings();
            AvatarService.UpdateAvatarInitialFontsSettings();

            // Clear the separate caches for diff/merge tools
            ThreadHelper.FileAndForget(() => new CustomDiffMergeToolProvider().ClearAsync(isDiff: false));
            ThreadHelper.FileAndForget(async () =>
            {
                revisionDiff.CancelLoadCustomDifftools();
                RevisionGrid.CancelLoadCustomDifftools();

                await new CustomDiffMergeToolProvider().ClearAsync(isDiff: true);

                // The tool lists are created when the forms are init, must be redone after clearing the cache
                await this.SwitchToMainThreadAsync();
                revisionDiff.LoadCustomDifftools();
                RevisionGrid.LoadCustomDifftools();
            });

            _dashboard?.RefreshContent();

            _gitStatusMonitor.Active = NeedsGitStatusMonitor() && Module.IsValidGitWorkingDir();

            RefreshDefaultPullAction();
        }

        private void TagToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCreateTagDialog(this, RevisionGrid.LatestSelectedRevision);
        }

        private static void SaveApplicationSettings()
        {
            AppSettings.SaveSettings();
        }

        private void EditGitignoreToolStripMenuItem1Click(object sender, EventArgs e)
        {
            UICommands.StartEditGitIgnoreDialog(this, false);
        }

        private void EditGitInfoExcludeToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartEditGitIgnoreDialog(this, true);
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
            UICommands.StartFileEditorDialog(fileName, true);
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

        private void RebaseToolStripMenuItemClick(object sender, EventArgs e)
        {
            IReadOnlyList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

            if (revisions.Count == 0 || revisions[0].IsArtificial)
            {
                return;
            }

            string onto = revisions[0].ObjectId.ToString(); // 2nd selected commit
            if (revisions.Count == 2)
            {
                // Set defaults in rebase form to rebase commits defined by the range *from* first selected commit *to* HEAD
                // *onto* 2nd selected commit
                string from = revisions[1].ObjectId.ToShortString(); // 1st selected commit (excluded from rebase)
                string to = RevisionGrid.CurrentBranch.Value; // current branch checked out (HEAD)

                UICommands.StartRebaseDialog(this, from, to, onto, interactive: false, startRebaseImmediately: false);
            }
            else
            {
                UICommands.StartRebaseDialog(this, onto);
            }
        }

        private void CommitInfoTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshSelection();
            FillTerminalTab();
            if (CommitInfoTabControl.SelectedTab == DiffTabPage)
            {
                // workaround to avoid focusing the "filter files" combobox
                revisionDiff.SwitchFocus(alreadyContainedFocus: false);
            }
            else if (CommitInfoTabControl.SelectedTab == TreeTabPage)
            {
                fileTree.SwitchFocus(alreadyContainedFocus: false);
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

        private void UpdateSubmoduleToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem toolStripMenuItem)
            {
                string submodule = toolStripMenuItem.Tag as string;
                Validates.NotNull(Module.SuperprojectModule);
                FormProcess.ShowDialog(this, UICommands, arguments: Commands.SubmoduleUpdate(submodule), Module.SuperprojectModule.WorkingDir, input: null, useDialogSettings: true);
            }

            RefreshRevisions();
        }

        private void UpdateAllSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
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
            UICommands.StartStashDialog(this, false);
            UpdateStashCount();
        }

        private void PluginSettingsToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartPluginSettingsDialog(this);
        }

        private void RepoSettingsToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartRepoSettingsDialog(this);
        }

        private void CloseToolStripMenuItemClick(object sender, EventArgs e)
        {
            SetWorkingDir("");
        }

        private void CleanupToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCleanupRepositoryDialog(this);
        }

        public void SetWorkingDir(string? path, ObjectId? selectedId = null, ObjectId? firstId = null)
        {
            RevisionGrid.SelectedId = selectedId;
            RevisionGrid.FirstId = firstId;
            SetGitModule(this, new GitModuleEventArgs(new GitModule(path)));
        }

        private void SetGitModule(object sender, GitModuleEventArgs e)
        {
            string originalWorkingDir = Module.WorkingDir;

            HideVariableMainMenuItems();
            PluginRegistry.Unregister(UICommands);
            RevisionGrid.OnRepositoryChanged();
            _gitStatusMonitor.InvalidateGitWorkingDirectoryStatus();
            _submoduleStatusProvider.Init();

            repoObjectsTree.ClearTrees();

            UICommands = UICommands.WithGitModule(e.GitModule);
            if (Module.IsValidGitWorkingDir())
            {
                RevisionGrid.SuspendRefreshRevisions();
                string path = Module.WorkingDir;
                AppSettings.RecentWorkingDir = path;

                HideDashboard();

                if (!string.Equals(originalWorkingDir, Module.WorkingDir, StringComparison.Ordinal))
                {
                    ChangeTerminalActiveFolder(Module.WorkingDir);

#if DEBUG
                    // Current encodings
                    Debug.WriteLine($"Encodings for {Module.WorkingDir}");
                    Debug.WriteLine($"Files content encoding: {Module.FilesEncoding.EncodingName}");
                    Debug.WriteLine($"Commit encoding: {Module.CommitEncoding.EncodingName}");
                    if (Module.LogOutputEncoding.CodePage != Module.CommitEncoding.CodePage)
                    {
                        Debug.WriteLine($"Log output encoding: {Module.LogOutputEncoding.EncodingName}");
                    }
#endif

                    // Reset the filter when switching repos

                    // If we're applying custom branch or revision filters - reset them
                    RevisionGrid.ResetAllFilters();
                    ToolStripFilters.ClearQuickFilters();
                    revisionDiff.RepositoryChanged();
                }

                RevisionInfo.SetRevisionWithChildren(revision: null, children: Array.Empty<ObjectId>());
                RevisionGrid.ResumeRefreshRevisions();

                RefreshRevisions();

                SetShortcutKeyDisplayStringsFromHotkeySettings();
            }
            else
            {
                dashboardToolStripMenuItem.Visible = true;

                MainSplitContainer.Visible = false;
                ShowDashboard();
            }

            RegisterPlugins();

            revisionDiff.RegisterGitHostingPluginInBlameControl();
            fileTree.RegisterGitHostingPluginInBlameControl();
        }

        private void FileExplorerToolStripMenuItemClick(object sender, EventArgs e)
        {
            OsShellUtil.OpenWithFileExplorer(Module.WorkingDir);
        }

        private void CreateBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCreateBranchDialog(this, RevisionGrid.LatestSelectedRevision?.ObjectId);
        }

        private void editGitAttributesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UICommands.StartEditGitAttributesDialog(this);
        }

        private void deleteIndexLockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Module.UnlockIndex(true);
            }
            catch (FileDeleteException ex)
            {
                throw new UserExternalOperationException(_indexLockCantDelete.Text,
                    new ExternalOperationException(arguments: ex.FileName, workingDirectory: Module.WorkingDir, innerException: ex));
            }
        }

        private void BisectClick(object sender, EventArgs e)
        {
            using (FormBisect frm = new(RevisionGrid))
            {
                frm.ShowDialog(this);
            }

            RefreshRevisions();
        }

        private void CurrentBranchDropDownOpening(object sender, EventArgs e)
        {
            branchSelect.DropDown.SuspendLayout();
            branchSelect.DropDownItems.Clear();

            AddCheckoutBranchMenuItem();
            branchSelect.DropDownItems.Add(new ToolStripSeparator());
            AddBranchesMenuItems();

            branchSelect.DropDown.ResumeLayout();

            void AddCheckoutBranchMenuItem()
            {
                ToolStripMenuItem checkoutBranchItem = new(checkoutBranchToolStripMenuItem.Text, Images.BranchCheckout)
                {
                    ShortcutKeys = GetShortcutKeys(Command.CheckoutBranch),
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.CheckoutBranch)
                };

                branchSelect.DropDownItems.Add(checkoutBranchItem);
                checkoutBranchItem.Click += CheckoutBranchToolStripMenuItemClick;
            }

            void AddBranchesMenuItems()
            {
                foreach (IGitRef branch in GetBranches())
                {
                    Validates.NotNull(branch.ObjectId);
                    bool isBranchVisible = ((ICheckRefs)RevisionGridControl).Contains(branch.ObjectId);

                    ToolStripItem toolStripItem = branchSelect.DropDownItems.Add(branch.Name);
                    toolStripItem.ForeColor = isBranchVisible ? branchSelect.ForeColor : Color.Silver.AdaptTextColor();
                    toolStripItem.Image = isBranchVisible ? Images.Branch : Images.EyeClosed;
                    toolStripItem.Click += (s, e) => UICommands.StartCheckoutBranch(this, toolStripItem.Text);
                    toolStripItem.AdaptImageLightness();
                }

                IEnumerable<IGitRef> GetBranches()
                {
                    // Make sure there are never more than a 100 branches added to the menu
                    // Git Extensions will hang when the drop down is too large...
                    return Module
                        .GetRefs(RefsFilter.Heads)
                        .Take(100);
                }
            }
        }

        private void _forkCloneMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginRegistry.GitHosters.Count > 0)
            {
                UICommands.StartCloneForkFromHoster(this, PluginRegistry.GitHosters[0], SetGitModule);
                RefreshRevisions();
            }
            else
            {
                MessageBox.Show(this, _noReposHostPluginLoaded.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void _viewPullRequestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TryGetRepositoryHost(out IRepositoryHostPlugin? repoHost))
            {
                return;
            }

            UICommands.StartPullRequestsDialog(this, repoHost);
        }

        private void _createPullRequestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TryGetRepositoryHost(out IRepositoryHostPlugin? repoHost))
            {
                return;
            }

            UICommands.StartCreatePullRequest(this, repoHost);
        }

        private void _addUpstreamRemoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TryGetRepositoryHost(out IRepositoryHostPlugin? repoHost))
            {
                return;
            }

            UICommands.AddUpstreamRemote(this, repoHost);
        }

        private bool TryGetRepositoryHost([NotNullWhen(returnValue: true)] out IRepositoryHostPlugin? repoHost)
        {
            repoHost = PluginRegistry.TryGetGitHosterForModule(Module);
            if (repoHost is null)
            {
                MessageBox.Show(this, _noReposHostFound.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        #region Hotkey commands

        public static readonly string HotkeySettingsName = "Browse";

        internal enum Command
        {
            // Focus or visuals
            FocusRevisionGrid = 3,
            FocusCommitInfo = 4,
            FocusDiff = 5,
            FocusFileTree = 6,
            FocusFilter = 18,
            ToggleLeftPanel = 21,
            FocusLeftPanel = 25,
            FocusGpgInfo = 26,
            FocusGitConsole = 29,
            FocusBuildServerStatus = 30,
            FocusNextTab = 31,
            FocusPrevTab = 32,

            // START menu
            OpenRepo = 45,

            // DASHBOARD menu

            // REPOSITORY menu
            CloseRepository = 15,

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
            QuickPullOrFetch = 48, // Default user action configured in toolbar
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
            GoToParent = 38

            /* deprecated: RotateApplicationIcon = 14, */
        }

        private void AddNotes()
        {
            GitRevision? revision = RevisionGrid.GetSelectedRevisionOrDefault();
            if (revision?.IsArtificial is not false)
            {
                return;
            }

            Module.EditNotes(revision.ObjectId);
            FillCommitInfo(revision);
        }

        private void FindFileInSelectedCommit()
        {
            IReadOnlyList<GitRevision> selectedRevisions = RevisionGrid.GetSelectedRevisions();
            if (selectedRevisions.Count > 1 || (selectedRevisions.Count == 1 && selectedRevisions[0].IsArtificial))
            {
                GitRevision potentialRevision = selectedRevisions[0];
                ObjectId? targetCommit = potentialRevision.IsArtificial ? RevisionGrid.CurrentCheckout : potentialRevision.ObjectId;
                RevisionGrid.SetSelectedRevision(targetCommit);
            }

            CommitInfoTabControl.SelectedTab = TreeTabPage;

            AppSettings.ShowSplitViewLayout = true;
            RefreshSplitViewLayout();

            fileTree.InvokeFindFileDialog();
        }

        private void QuickFetch()
        {
            bool success = ScriptsRunner.RunEventScripts(ScriptEvent.BeforeFetch, this);
            if (!success)
            {
                return;
            }

            success = FormProcess.ShowDialog(this, UICommands, arguments: Module.FetchCmd(string.Empty, string.Empty, string.Empty), Module.WorkingDir, input: null, useDialogSettings: true);
            if (!success)
            {
                return;
            }

            ScriptsRunner.RunEventScripts(ScriptEvent.AfterFetch, this);
            RefreshRevisions();
        }

        public override bool ProcessHotkey(Keys keyData)
        {
            if (IsDesignMode || !HotkeysEnabled)
            {
                return false;
            }

            // generic handling of this form's hotkeys (upstream)
            if (base.ProcessHotkey(keyData))
            {
                return true;
            }

            // downstream (without keys for quick search and without keys for text selection and copy e.g. in CommitInfo)
            // but allow routing Ctrl+A away from RevisionGridControl in order to not select all revisions
            if (GitExtensionsControl.IsTextEditKey(keyData)
                && !(keyData == (Keys.Control | Keys.A) && RevisionGridControl.ContainsFocus))
            {
                return false;
            }

            // route to visible controls which have their own hotkeys
            return (keyData != (Keys.Control | Keys.A) && RevisionGridControl.ProcessHotkey(keyData))
                || (CommitInfoTabControl.SelectedTab == DiffTabPage && revisionDiff.ProcessHotkey(keyData))
                || (CommitInfoTabControl.SelectedTab == TreeTabPage && fileTree.ProcessHotkey(keyData));
        }

        protected override bool ExecuteCommand(int cmd)
        {
            switch ((Command)cmd)
            {
                case Command.GitBash: userShell.PerformButtonClick(); break;
                case Command.GitGui: Module.RunGui(); break;
                case Command.GitGitK: Module.RunGitK(); break;
                case Command.FocusLeftPanel: FocusLeftPanel(); break;
                case Command.FocusRevisionGrid: RevisionGrid.Focus(); break;
                case Command.FocusCommitInfo: FocusCommitInfo(); break;
                case Command.FocusDiff: FocusTabOf(revisionDiff, (c, alreadyContainedFocus) => c.SwitchFocus(alreadyContainedFocus)); break;
                case Command.FocusFileTree: FocusTabOf(fileTree, (c, alreadyContainedFocus) => c.SwitchFocus(alreadyContainedFocus)); break;
                case Command.FocusGpgInfo when AppSettings.ShowGpgInformation.Value: FocusTabOf(revisionGpgInfo1, (c, alreadyContainedFocus) => c.Focus()); break;
                case Command.FocusGitConsole: FocusGitConsole(); break;
                case Command.FocusBuildServerStatus: FocusTabOf(_buildReportTabPageExtension?.Control, (c, alreadyContainedFocus) => c.Focus()); break;
                case Command.FocusNextTab: FocusNextTab(); break;
                case Command.FocusPrevTab: FocusNextTab(forward: false); break;
                case Command.FocusFilter: ToolStripFilters.SetFocus(); break;
                case Command.OpenRepo: fileToolStripMenuItem.OpenRepositoryMenuItem.PerformClick(); break;
                case Command.Commit: UICommands.StartCommitDialog(this); break;
                case Command.AddNotes: AddNotes(); break;
                case Command.FindFileInSelectedCommit: FindFileInSelectedCommit(); break;
                case Command.CheckoutBranch: UICommands.StartCheckoutBranch(this); break;
                case Command.QuickFetch: QuickFetch(); break;
                case Command.QuickPull: DoPull(pullAction: GitPullAction.Merge, isSilent: true); break;
                case Command.QuickPullOrFetch: toolStripButtonPull.PerformButtonClick(); break;
                case Command.QuickPush: UICommands.StartPushDialog(this, true); break;
                case Command.CloseRepository: SetWorkingDir(""); break;
                case Command.Stash: UICommands.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash); break;
                case Command.StashStaged: UICommands.StashStaged(this); break;
                case Command.StashPop: UICommands.StashPop(this); break;
                case Command.OpenCommitsWithDifftool: RevisionGrid.DiffSelectedCommitsWithDifftool(); break;
                case Command.OpenWithDifftool: OpenWithDifftool(); break;
                case Command.OpenWithDifftoolFirstToLocal: OpenWithDifftoolFirstToLocal(); break;
                case Command.OpenWithDifftoolSelectedToLocal: OpenWithDifftoolSelectedToLocal(); break;
                case Command.OpenSettings: EditSettings.PerformClick(); break;
                case Command.ToggleLeftPanel: toggleLeftPanel.PerformClick(); break;
                case Command.EditFile: EditFile(); break;
                case Command.OpenAsTempFile when fileTree.Visible: fileTree.ExecuteCommand(RevisionFileTreeControl.Command.OpenAsTempFile); break;
                case Command.OpenAsTempFileWith when fileTree.Visible: fileTree.ExecuteCommand(RevisionFileTreeControl.Command.OpenAsTempFileWith); break;
                case Command.GoToSuperproject: toolStripButtonLevelUp.PerformClick(); break;
                case Command.GoToSubmodule: toolStripButtonLevelUp.ShowDropDown(); break;
                case Command.ToggleBetweenArtificialAndHeadCommits: RevisionGrid?.ExecuteCommand(RevisionGridControl.Command.ToggleBetweenArtificialAndHeadCommits); break;
                case Command.GoToChild: RestoreFileStatusListFocus(() => RevisionGrid?.ExecuteCommand(RevisionGridControl.Command.GoToChild)); break;
                case Command.GoToParent: RestoreFileStatusListFocus(() => RevisionGrid?.ExecuteCommand(RevisionGridControl.Command.GoToParent)); break;
                case Command.PullOrFetch: DoPull(pullAction: AppSettings.FormPullAction, isSilent: false); break;
                case Command.Push: UICommands.StartPushDialog(this, pushOnShow: ModifierKeys.HasFlag(Keys.Shift)); break;
                case Command.CreateBranch: UICommands.StartCreateBranchDialog(this, RevisionGrid.LatestSelectedRevision?.ObjectId); break;
                case Command.MergeBranches: UICommands.StartMergeBranchDialog(this, null); break;
                case Command.CreateTag: UICommands.StartCreateTagDialog(this, RevisionGrid.LatestSelectedRevision); break;
                case Command.Rebase: rebaseToolStripMenuItem.PerformClick(); break;
                default: return base.ExecuteCommand(cmd);
            }

            return true;

            void FocusLeftPanel()
            {
                if (!MainSplitContainer.Panel1Collapsed)
                {
                    repoObjectsTree.Focus();
                }
            }

            void FocusCommitInfo()
            {
                if (AppSettings.CommitInfoPosition == CommitInfoPosition.BelowList)
                {
                    CommitInfoTabControl.SelectedTab = CommitInfoTabPage;
                }

                RevisionInfo.Focus();
            }

            void FocusTabOf<T>(T? control, Action<T, bool> switchFocus) where T : Control
            {
                if (control is not null)
                {
                    TabPage tabPage = control.Parent as TabPage;
                    if (CommitInfoTabControl.TabPages.IndexOf(tabPage) >= 0)
                    {
                        bool alreadyContainedFocus = control.ContainsFocus;

                        if (CommitInfoTabControl.SelectedTab != tabPage)
                        {
                            CommitInfoTabControl.SelectedTab = tabPage;
                        }

                        switchFocus(control, alreadyContainedFocus);
                    }
                }
            }

            void FocusGitConsole()
            {
                FillTerminalTab();
                if (_consoleTabPage is not null && CommitInfoTabControl.TabPages.Contains(_consoleTabPage))
                {
                    CommitInfoTabControl.SelectedTab = _consoleTabPage;
                }
            }

            void FocusNextTab(bool forward = true)
            {
                int tabIndex = CommitInfoTabControl.SelectedIndex;
                tabIndex += forward ? 1 : (CommitInfoTabControl.TabCount - 1);
                CommitInfoTabControl.SelectedIndex = tabIndex % CommitInfoTabControl.TabCount;
            }

            void OpenWithDifftool()
            {
                if (revisionDiff.Visible)
                {
                    revisionDiff.ExecuteCommand(RevisionDiffControl.Command.OpenWithDifftool);
                }
                else if (fileTree.Visible)
                {
                    fileTree.ExecuteCommand(RevisionFileTreeControl.Command.OpenWithDifftool);
                }
            }

            void OpenWithDifftoolFirstToLocal()
            {
                if (revisionDiff.Visible)
                {
                    revisionDiff.ExecuteCommand(RevisionDiffControl.Command.OpenWithDifftoolFirstToLocal);
                }
            }

            void OpenWithDifftoolSelectedToLocal()
            {
                if (revisionDiff.Visible)
                {
                    revisionDiff.ExecuteCommand(RevisionDiffControl.Command.OpenWithDifftoolSelectedToLocal);
                }
            }

            void EditFile()
            {
                if (revisionDiff.Visible)
                {
                    revisionDiff.ExecuteCommand(RevisionDiffControl.Command.EditFile);
                }
                else if (fileTree.Visible)
                {
                    fileTree.ExecuteCommand(RevisionFileTreeControl.Command.EditFile);
                }
            }

            void RestoreFileStatusListFocus(Action action)
            {
                bool restoreFocus = revisionDiff.ContainsFocus;

                action();

                if (restoreFocus)
                {
                    revisionDiff.SwitchFocus(alreadyContainedFocus: false);
                }
            }
        }

        internal bool ExecuteCommand(Command cmd)
        {
            return ExecuteCommand((int)cmd);
        }

        #endregion

        public static void OpenContainingFolder(FileStatusList diffFiles, IGitModule module)
        {
            if (!diffFiles.SelectedItems.Any())
            {
                return;
            }

            foreach (FileStatusItem item in diffFiles.SelectedItems)
            {
                string filePath = Path.Combine(module.WorkingDir, item.Item.Name.ToNativePath());

                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    FormBrowseUtil.ShowFileOrParentFolderInFileExplorer(filePath);
                }
            }
        }

        private void SetSplitterPositions()
        {
            _splitterManager.AddSplitter(RevisionsSplitContainer, nameof(RevisionsSplitContainer));
            _splitterManager.AddSplitter(MainSplitContainer, nameof(MainSplitContainer));
            _splitterManager.AddSplitter(RightSplitContainer, nameof(RightSplitContainer));

            revisionDiff.InitSplitterManager(_splitterManager);
            fileTree.InitSplitterManager(_splitterManager);

            _splitterManager.RestoreSplitters();
            RefreshLayoutToggleButtonStates();
            if (_isFileHistoryMode)
            {
                _fileBlameHistoryLeftPanelStartupState = MainSplitContainer.Panel1Collapsed;
                MainSplitContainer.Panel1Collapsed = true;
            }

            // Since #8849 and #8557 we have a geometry bug, which pushes the splitter up by 4px.
            // Account for this shift. This is a workaround at best in the same way as for FormCommit.
            if (!RevisionsSplitContainer.Panel2Collapsed && RevisionsSplitContainer.FixedPanel == FixedPanel.Panel2)
            {
                try
                {
                    RevisionsSplitContainer.SplitterDistance -= 4;
                }
                catch (Exception)
                {
                    // Catching because bad value can raise an exception
                }
            }
        }

        private void CommandsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            // Most options do not make sense for artificial commits or no revision selected at all
            IReadOnlyList<GitRevision> selectedRevisions = RevisionGrid.GetSelectedRevisions();
            bool singleNormalCommit = selectedRevisions.Count == 1 && !selectedRevisions[0].IsArtificial;

            // Some commands like stash, undo commit etc has no relation to selections

            // Require that a single commit is selected
            // Some commands like delete branch could be available for artificial as no default is used,
            // but hide for consistency
            branchToolStripMenuItem.Enabled =
            deleteBranchToolStripMenuItem.Enabled =
            mergeBranchToolStripMenuItem.Enabled =
            checkoutBranchToolStripMenuItem.Enabled =
            cherryPickToolStripMenuItem.Enabled =
            checkoutToolStripMenuItem.Enabled =
            bisectToolStripMenuItem.Enabled =
                singleNormalCommit && !Module.IsBareRepository();

            rebaseToolStripMenuItem.Enabled = selectedRevisions.Count is (1 or 2) && selectedRevisions.All(r => !r.IsArtificial) && !Module.IsBareRepository();

            tagToolStripMenuItem.Enabled =
            deleteTagToolStripMenuItem.Enabled =
            archiveToolStripMenuItem.Enabled =
                singleNormalCommit;

            // Not operating on selected revision
            commitToolStripMenuItem.Enabled =
            undoLastCommitToolStripMenuItem.Enabled =
            runMergetoolToolStripMenuItem.Enabled =
            stashToolStripMenuItem.Enabled =
            resetToolStripMenuItem.Enabled =
            cleanupToolStripMenuItem.Enabled =
            toolStripMenuItemReflog.Enabled =
            applyPatchToolStripMenuItem.Enabled =
                !Module.IsBareRepository();
        }

        private void PullToolStripMenuItemClick(object sender, EventArgs e)
        {
            // "Pull/Fetch..." menu item always opens the dialog
            DoPull(pullAction: AppSettings.FormPullAction, isSilent: false);
        }

        private void ToolStripButtonPullClick(object sender, EventArgs e)
        {
            // Clicking on the Pull button toolbar button will perform the default selected action silently,
            // except if that action is to open the dialog (PullAction.None)
            bool isSilent = AppSettings.DefaultPullAction != GitPullAction.None;
            GitPullAction pullAction = AppSettings.DefaultPullAction != GitPullAction.None ?
                AppSettings.DefaultPullAction : AppSettings.FormPullAction;
            DoPull(pullAction: pullAction, isSilent: isSilent);
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

        private void branchSelect_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                CheckoutBranchToolStripMenuItemClick(sender, e);
            }
        }

        private void RevisionInfo_CommandClicked(object sender, CommandEventArgs e)
        {
            // TODO this code duplicated in FormFileHistory.Blame_CommandClick
            switch (e.Command)
            {
                case "gotocommit":
                    Validates.NotNull(e.Data);
                    if (!Module.TryResolvePartialCommitId(e.Data, out ObjectId commitId) || !RevisionGrid.SetSelectedRevision(commitId))
                    {
                        if (commitId is null)
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

        private void SubmoduleToolStripButtonClick(object sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem menuSender)
            {
                return;
            }

            string path = menuSender.Tag as string;
            if (!Directory.Exists(path))
            {
                MessageBoxes.SubmoduleDirectoryDoesNotExist(this, path);
                return;
            }

            SetWorkingDir(path);
        }

        #region Submodules

        private ToolStripItem CreateSubmoduleMenuItem(SubmoduleInfo info, string textFormat = "{0}")
        {
            ToolStripMenuItem item = new(string.Format(textFormat, info.Text))
            {
                Width = 200,
                Tag = info.Path,
                Image = Images.FolderSubmodule
            };

            if (info.Bold)
            {
                item.Font = new Font(item.Font, FontStyle.Bold);
            }

            item.Click += SubmoduleToolStripButtonClick;

            return item;
        }

        private static void UpdateSubmoduleMenuItemStatus(ToolStripItem item, SubmoduleInfo info, string textFormat = "{0}")
        {
            if (info.Detailed is not null)
            {
                item.Image = GetSubmoduleItemImage(info.Detailed);
                item.Text = string.Format(textFormat, info.Text + info.Detailed.AddedAndRemovedText);
            }

            return;

            static Image GetSubmoduleItemImage(DetailedSubmoduleInfo details)
            {
                return (details.Status, details.IsDirty) switch
                {
                    (null, _) => Images.FolderSubmodule,
                    (SubmoduleStatus.FastForward, true) => Images.SubmoduleRevisionUpDirty,
                    (SubmoduleStatus.FastForward, false) => Images.SubmoduleRevisionUp,
                    (SubmoduleStatus.Rewind, true) => Images.SubmoduleRevisionDownDirty,
                    (SubmoduleStatus.Rewind, false) => Images.SubmoduleRevisionDown,
                    (SubmoduleStatus.NewerTime, true) => Images.SubmoduleRevisionSemiUpDirty,
                    (SubmoduleStatus.NewerTime, false) => Images.SubmoduleRevisionSemiUp,
                    (SubmoduleStatus.OlderTime, true) => Images.SubmoduleRevisionSemiDownDirty,
                    (SubmoduleStatus.OlderTime, false) => Images.SubmoduleRevisionSemiDown,
                    (_, true) => Images.SubmoduleDirty,
                    (_, false) => Images.FileStatusModified
                };
            }
        }

        private void UpdateSubmodulesStructure()
        {
            // Submodule status is updated on git-status updates. To make sure supermodule status is updated, update immediately (once)
            bool updateStatus = AppSettings.ShowSubmoduleStatus && _gitStatusMonitor.Active;

            toolStripButtonLevelUp.ToolTipText = "";

            ThreadHelper.FileAndForget(async () =>
            {
                try
                {
                    await _submoduleStatusProvider.UpdateSubmodulesStructureAsync(Module.WorkingDir, TranslatedStrings.NoBranch, updateStatus);
                }
                catch (GitConfigurationException ex)
                {
                    await this.SwitchToMainThreadAsync();
                    MessageBoxes.ShowGitConfigurationExceptionMessage(this, ex);
                }
            });
        }

        private void SubmoduleStatusProvider_StatusUpdating(object sender, EventArgs e)
        {
            this.InvokeAndForget(() =>
            {
                RemoveSubmoduleButtons();
                toolStripButtonLevelUp.DropDownItems.Add(_loading.Text);
            });
        }

        private void SubmoduleStatusProvider_StatusUpdated(object sender, SubmoduleStatusEventArgs e)
        {
            this.InvokeAndForget(() =>
            {
                if (e.StructureUpdated || _currentSubmoduleMenuItems is null)
                {
                    _currentSubmoduleMenuItems = PopulateToolbar(e.Info);
                }

                UpdateSubmoduleMenuStatus(e.Info);
            },
            cancellationToken: e.Token);
        }

        private List<ToolStripItem> PopulateToolbar(SubmoduleInfoResult result)
        {
            // Second task: Populate submodule toolbar menu on UI thread.
            // Suspend before clearing dropdowns to show loading text until updated
            toolStripButtonLevelUp.DropDown.SuspendLayout();
            RemoveSubmoduleButtons();

            List<ToolStripItem> newItems = result.OurSubmodules
                .Select(submodule => CreateSubmoduleMenuItem(submodule))
                .ToList();

            if (result.OurSubmodules.Count == 0)
            {
                newItems.Add(new ToolStripMenuItem(_noSubmodulesPresent.Text));
            }

            if (result.SuperProject is not null)
            {
                newItems.Add(new ToolStripSeparator());

                // Show top project only if it's not our super project
                if (result.TopProject is not null && result.TopProject != result.SuperProject)
                {
                    newItems.Add(CreateSubmoduleMenuItem(result.TopProject, _topProjectModuleFormat.Text));
                }

                newItems.Add(CreateSubmoduleMenuItem(result.SuperProject, _superprojectModuleFormat.Text));
                newItems.AddRange(result.AllSubmodules.Select(submodule => CreateSubmoduleMenuItem(submodule)));
                toolStripButtonLevelUp.ToolTipText = _goToSuperProject.Text;
            }

            newItems.Add(new ToolStripSeparator());

            ToolStripMenuItem mi = new(updateAllSubmodulesToolStripMenuItem.Text, Images.SubmodulesUpdate);
            mi.Click += UpdateAllSubmodulesToolStripMenuItemClick;
            newItems.Add(mi);

            if (result.CurrentSubmoduleName is not null)
            {
                ToolStripMenuItem item = new(_updateCurrentSubmodule.Text)
                {
                    Width = 200,
                    Tag = Module.WorkingDir,
                    Image = Images.FolderSubmodule
                };
                item.Click += UpdateSubmoduleToolStripMenuItemClick;
                newItems.Add(item);
            }

            // Using AddRange is critical: if you used Add to add menu items one at a
            // time, performance would be extremely slow with many submodules (> 100).
            toolStripButtonLevelUp.DropDownItems.AddRange(newItems.ToArray());
            toolStripButtonLevelUp.DropDown.ResumeLayout();

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
            foreach (ToolStripItem item in _currentSubmoduleMenuItems)
            {
                string path = item.Tag as string;
                if (string.IsNullOrWhiteSpace(path))
                {
                    // not a submodule
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
            foreach (object item in toolStripButtonLevelUp.DropDownItems)
            {
                if (item is ToolStripMenuItem toolStripButton)
                {
                    toolStripButton.Click -= SubmoduleToolStripButtonClick;
                }
            }

            toolStripButtonLevelUp.DropDownItems.Clear();
        }

        #endregion

        private void toolStripButtonLevelUp_ButtonClick(object sender, EventArgs e)
        {
            if (Module.SuperprojectModule is not null)
            {
                SetGitModule(this, new GitModuleEventArgs(Module.SuperprojectModule));
            }
            else
            {
                toolStripButtonLevelUp.ShowDropDown();
            }
        }

        /// <summary>
        /// Adds a tab with console interface to Git over the current working copy. Recreates the terminal on tab activation if user exits the shell.
        /// </summary>
        private void FillTerminalTab()
        {
            if (!EnvUtils.RunningOnWindows() || !AppSettings.ShowConEmuTab.Value)
            {
                // ConEmu only works on WinNT
                return;
            }

            if (_terminal is not null)
            {
                // Terminal already created; give it focus
                _terminal.Focus();
                return;
            }

            if (_consoleTabPage is not null)
            {
                // Tab page already created
                return;
            }

            _consoleTabPage = new TabPage
            {
                Text = _consoleTabCaption.Text,
                Name = _consoleTabCaption.Text
            };
            CommitInfoTabControl.Controls.Add(_consoleTabPage);

            // We have to set ImageKey after it's added to the tab control
            _consoleTabPage.ImageKey = nameof(Images.Console);

            // Delay-create the terminal window when the tab is first selected
            CommitInfoTabControl.Selecting += (sender, args) =>
            {
                if (args.TabPage != _consoleTabPage)
                {
                    return;
                }

                if (_terminal is null)
                {
                    // Lazy-create on first opening the tab
                    _consoleTabPage.Controls.Clear();
                    _consoleTabPage.Controls.Add(
                        _terminal = new ConEmuControl
                        {
                            Dock = DockStyle.Fill,
                            IsStatusbarVisible = false
                        });
                }

                // If user has typed "exit" in there, restart the shell; otherwise just return
                if (_terminal.IsConsoleEmulatorOpen)
                {
                    return;
                }

                // Create the terminal
                ConEmuStartInfo startInfo = new()
                {
                    StartupDirectory = Module.WorkingDir,
                    WhenConsoleProcessExits = WhenConsoleProcessExits.CloseConsoleEmulator
                };

                string shellType = AppSettings.ConEmuTerminal.Value;
                startInfo.ConsoleProcessCommandLine = _shellProvider.GetShellCommandLine(shellType);

                // Set path to git in this window (actually, effective with CMD only)
                if (!string.IsNullOrEmpty(AppSettings.GitCommandValue))
                {
                    string? dirGit = Path.GetDirectoryName(AppSettings.GitCommandValue);
                    if (!string.IsNullOrEmpty(dirGit))
                    {
                        startInfo.SetEnv("PATH", dirGit + ";" + "%PATH%");
                    }
                }

                try
                {
                    _terminal.Start(startInfo, ThreadHelper.JoinableTaskFactory, AppSettings.ConEmuStyle.Value, AppSettings.ConEmuConsoleFont.Name, AppSettings.ConEmuConsoleFont.Size.ToString(CultureInfo.InvariantCulture));
                }
                catch (InvalidOperationException)
                {
#if DEBUG
                    MessageBox.Show(@"ConEmu appears to be missing. Please perform a full rebuild and try again.", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
#else
                    throw;
#endif
                }
            };
        }

        public void ChangeTerminalActiveFolder(string path)
        {
            string? shellType = AppSettings.ConEmuTerminal.Value;
            IShellDescriptor shell = _shellProvider.GetShell(shellType);
            _terminal?.ChangeFolder(shell, path);
        }

        private void menuitemSparseWorkingCopy_Click(object sender, EventArgs e)
        {
            UICommands.StartSparseWorkingCopyDialog(this);
        }

        private void toolStripMenuItemReflog_Click(object sender, EventArgs e)
        {
            using FormReflog formReflog = new(UICommands);
            formReflog.ShowDialog();
        }

        #region Layout management

        private void toggleSplitViewLayout_Click(object sender, EventArgs e)
        {
            AppSettings.ShowSplitViewLayout = !AppSettings.ShowSplitViewLayout;
            DiagnosticsClient.TrackEvent("Layout change",
                new Dictionary<string, string> { { nameof(AppSettings.ShowSplitViewLayout), AppSettings.ShowSplitViewLayout.ToString() } });

            RefreshSplitViewLayout();
        }

        private void toggleLeftPanel_Click(object sender, EventArgs e)
        {
            MainSplitContainer.Panel1Collapsed = !MainSplitContainer.Panel1Collapsed;
            DiagnosticsClient.TrackEvent("Layout change",
                new Dictionary<string, string> { { "ShowLeftPanel", MainSplitContainer.Panel1Collapsed.ToString() } });

            RefreshLayoutToggleButtonStates();

            if (!MainSplitContainer.Panel1Collapsed)
            {
                // Refresh the left panel, update visibility of objects separately
                // Get the "main" stash commit, including the reflog selector
                Lazy<IReadOnlyCollection<GitRevision>> getStashRevs = new(() =>
                    !AppSettings.ShowStashes
                    ? Array.Empty<GitRevision>()
                    : new RevisionReader(new GitModule(UICommands.Module.WorkingDir)).GetStashes(CancellationToken.None));

                RefreshLeftPanel(new FilteredGitRefsProvider(UICommands.Module).GetRefs, getStashRevs, forceRefresh: true);
                repoObjectsTree.RefreshRevisionsLoaded();
            }
        }

        private void CommitInfoPositionClick(object sender, EventArgs e)
        {
            if (!menuCommitInfoPosition.DropDownButtonPressed)
            {
                SetCommitInfoPosition((CommitInfoPosition)(
                    ((int)AppSettings.CommitInfoPosition + 1) %
                    Enum.GetValues(typeof(CommitInfoPosition)).Length));
            }
        }

        private void CommitInfoBelowClick(object sender, EventArgs e) =>
            SetCommitInfoPosition(CommitInfoPosition.BelowList);

        private void CommitInfoLeftwardClick(object sender, EventArgs e) =>
            SetCommitInfoPosition(CommitInfoPosition.LeftwardFromList);

        private void CommitInfoRightwardClick(object sender, EventArgs e) =>
            SetCommitInfoPosition(CommitInfoPosition.RightwardFromList);

        private void SetCommitInfoPosition(CommitInfoPosition position)
        {
            AppSettings.CommitInfoPosition = position;
            DiagnosticsClient.TrackEvent("Layout change",
                new Dictionary<string, string> { { nameof(AppSettings.CommitInfoPosition), AppSettings.CommitInfoPosition.ToString() } });

            LayoutRevisionInfo();
            RefreshLayoutToggleButtonStates();
        }

        private void RefreshSplitViewLayout()
        {
            RightSplitContainer.Panel2Collapsed = !AppSettings.ShowSplitViewLayout;
            DiagnosticsClient.TrackEvent("Layout change",
                new Dictionary<string, string> { { nameof(AppSettings.ShowSplitViewLayout), AppSettings.ShowSplitViewLayout.ToString() } });

            RefreshLayoutToggleButtonStates();
        }

        private void RefreshLayoutToggleButtonStates()
        {
            toggleLeftPanel.Checked = !MainSplitContainer.Panel1Collapsed;
            toggleSplitViewLayout.Checked = AppSettings.ShowSplitViewLayout;

            int commitInfoPositionNumber = (int)AppSettings.CommitInfoPosition;
            ToolStripItem selectedMenuItem = menuCommitInfoPosition.DropDownItems[commitInfoPositionNumber];
            menuCommitInfoPosition.Image = selectedMenuItem.Image;
            menuCommitInfoPosition.ToolTipText = selectedMenuItem.Text?.Replace("&", string.Empty);
        }

        private void LayoutRevisionInfo()
        {
            // Handle must be created prior to insertion
            _ = CommitInfoTabControl.Handle;

            RevisionInfo.SuspendLayout();
            CommitInfoTabControl.SuspendLayout();
            RevisionsSplitContainer.SuspendLayout();

            CommitInfoPosition commitInfoPosition = AppSettings.CommitInfoPosition;
            if (commitInfoPosition == CommitInfoPosition.BelowList)
            {
                CommitInfoTabControl.SelectedIndexChanged -= CommitInfoTabControl_SelectedIndexChanged;
                CommitInfoTabControl.InsertIfNotExists(0, CommitInfoTabPage);
                CommitInfoTabControl.SelectedIndexChanged += CommitInfoTabControl_SelectedIndexChanged;
                CommitInfoTabControl.SelectedTab = CommitInfoTabPage;

                RevisionsSplitContainer.FixedPanel = FixedPanel.Panel2;
                RevisionInfo.Parent = CommitInfoTabPage;
                RevisionGridContainer.Parent = RevisionsSplitContainer.Panel1;
                RevisionsSplitContainer.Panel2Collapsed = true;
            }
            else
            {
                // enough to fit CommitInfoHeader in most cases, when the width is (avatar + commit hash)
                int width = DpiUtil.Scale(490) + SystemInformation.VerticalScrollBarWidth;
                CommitInfoTabControl.SelectedIndexChanged -= CommitInfoTabControl_SelectedIndexChanged;
                CommitInfoTabControl.RemoveIfExists(CommitInfoTabPage);
                CommitInfoTabControl.SelectedIndexChanged += CommitInfoTabControl_SelectedIndexChanged;

                if (commitInfoPosition == CommitInfoPosition.RightwardFromList)
                {
                    RevisionsSplitContainer.FixedPanel = FixedPanel.Panel2;
                    RevisionsSplitContainer.SplitterDistance = Math.Max(0, RevisionsSplitContainer.Width - width);
                    RevisionInfo.Parent = RevisionsSplitContainer.Panel2;
                    RevisionGridContainer.Parent = RevisionsSplitContainer.Panel1;
                }
                else if (commitInfoPosition == CommitInfoPosition.LeftwardFromList)
                {
                    RevisionsSplitContainer.FixedPanel = FixedPanel.Panel1;
                    RevisionsSplitContainer.SplitterDistance = width;
                    RevisionInfo.Parent = RevisionsSplitContainer.Panel1;
                    RevisionGridContainer.Parent = RevisionsSplitContainer.Panel2;
                }
                else
                {
                    throw new NotSupportedException();
                }

                RevisionsSplitContainer.Panel2Collapsed = false;
            }

            RevisionInfo.Parent.BackColor = RevisionInfo.BackColor;
            RevisionInfo.ResumeLayout(performLayout: true);

            MainSplitContainer.Panel1.BackColor = OtherColors.PanelBorderColor;
            RevisionsSplitContainer.Panel1.BackColor = OtherColors.PanelBorderColor;
            RevisionsSplitContainer.Panel2.BackColor = OtherColors.PanelBorderColor;

            CommitInfoTabControl.ResumeLayout(performLayout: true);
            RevisionsSplitContainer.ResumeLayout(performLayout: true);
        }

        #endregion

        private void manageWorktreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using FormManageWorktree formManageWorktree = new(UICommands);
            formManageWorktree.ShowDialog(this);
            if (formManageWorktree.ShouldRefreshRevisionGrid)
            {
                RefreshRevisions();
            }
        }

        private void undoLastCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AppSettings.DontConfirmUndoLastCommit || MessageBox.Show(this, _undoLastCommitText.Text, _undoLastCommitCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                ArgumentString args = Commands.Reset(ResetMode.Soft, "HEAD~1");
                Module.GitExecutable.GetOutput(args);
                refreshToolStripMenuItem.PerformClick();
                RefreshGitStatusMonitor();
            }
        }

        internal TestAccessor GetTestAccessor()
            => new(this);

        internal readonly struct TestAccessor
        {
            private readonly FormBrowse _form;

            public TestAccessor(FormBrowse form)
            {
                _form = form;
            }

            public FullBleedTabControl CommitInfoTabControl => _form.CommitInfoTabControl;
            public TabPage DiffTabPage => _form.DiffTabPage;
            public RepoObjectsTree RepoObjectsTree => _form.repoObjectsTree;
            public RevisionDiffControl RevisionDiffControl => _form.revisionDiff;
            public RevisionFileTreeControl RevisionFileTreeControl => _form.fileTree;
            public RevisionGridControl RevisionGrid => _form.RevisionGridControl;
            public SplitContainer RevisionsSplitContainer => _form.RevisionsSplitContainer;
            public SplitContainer RightSplitContainer => _form.RightSplitContainer;
            public SplitterManager SplitterManager => _form._splitterManager;
            public TabPage TreeTabPage => _form.TreeTabPage;
            public FilterToolBar ToolStripFilters => _form.ToolStripFilters;
        }

        private void FormBrowse_DragDrop(object sender, DragEventArgs e)
        {
            HandleDrop(e);
        }

        private void HandleDrop(DragEventArgs e)
        {
            if (TreeTabPage.Parent is null)
            {
                return;
            }

            string itemPath = (e.Data.GetData(DataFormats.Text) ?? e.Data.GetData(DataFormats.UnicodeText)) as string;
            if (IsFileExistingInRepo(itemPath))
            {
                CommitInfoTabControl.SelectedTab = TreeTabPage;
                fileTree.SelectFileOrFolder(itemPath);
                return;
            }

            if (e.Data.GetData(DataFormats.FileDrop) is not string[] paths)
            {
                return;
            }

            foreach (string path in paths)
            {
                if (!IsFileExistingInRepo(path))
                {
                    continue;
                }

                if (CommitInfoTabControl.SelectedTab != TreeTabPage)
                {
                    CommitInfoTabControl.SelectedTab = TreeTabPage;
                }

                if (fileTree.SelectFileOrFolder(path))
                {
                    return;
                }
            }

            bool IsPathExists([NotNullWhen(returnValue: true)] string? path) => path is not null && (File.Exists(path) || Directory.Exists(path));

            bool IsFileExistingInRepo([NotNullWhen(returnValue: true)] string? path) => IsPathExists(path) && path.StartsWith(Module.WorkingDir, StringComparison.InvariantCultureIgnoreCase);
        }

        private static void FormBrowse_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)
                || e.Data.GetDataPresent(DataFormats.Text)
                || e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void fileToolStripMenuItem_RecentRepositoriesCleared(object sender, EventArgs e)
        {
            _dashboard?.RefreshContent();
        }
    }
}
