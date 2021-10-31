using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConEmu.WinForms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Git.Commands;
using GitCommands.Gpg;
using GitCommands.Submodules;
using GitCommands.UserRepositoryHistory;
using GitCommands.Utils;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.BranchTreePanel;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.CommandsDialogs.WorktreeDialog;
using GitUI.HelperDialogs;
using GitUI.Hotkey;
using GitUI.Infrastructure.Telemetry;
using GitUI.NBugReports;
using GitUI.Properties;
using GitUI.Script;
using GitUI.Shells;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;
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
                F   Favourite Repositories
                L   Clone Repository...
                O   Open...
                R   Recent Repositories
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
                M   User Manual
                R   Report an issue
                T   Translate
                U   Check for updates
                Y   Yes, I allow telemetry
        */
        #endregion

        #region Translation

        private readonly TranslationString _noSubmodulesPresent = new("No submodules");
        private readonly TranslationString _topProjectModuleFormat = new("Top project: {0}");
        private readonly TranslationString _superprojectModuleFormat = new("Superproject: {0}");
        private readonly TranslationString _goToSuperProject = new("Go to superproject");

        private readonly TranslationString _indexLockCantDelete = new("Failed to delete index.lock");

        private readonly TranslationString _loading = new("Loading...");

        private readonly TranslationString _noReposHostPluginLoaded = new("No repository host plugin loaded.");
        private readonly TranslationString _noReposHostFound = new("Could not find any relevant repository hosts for the currently open repository.");

        private readonly TranslationString _configureWorkingDirMenu = new("&Configure this menu");

        private readonly TranslationString _updateCurrentSubmodule = new("Update current submodule");

        private readonly TranslationString _pullFetch = new("Fetch");
        private readonly TranslationString _pullFetchAll = new("Fetch all");
        private readonly TranslationString _pullFetchPruneAll = new("Fetch and prune all");
        private readonly TranslationString _pullMerge = new("Pull - merge");
        private readonly TranslationString _pullRebase = new("Pull - rebase");
        private readonly TranslationString _pullOpenDialog = new("Open pull dialog");

        private readonly TranslationString _buildReportTabCaption = new("Build Report");
        private readonly TranslationString _consoleTabCaption = new("Console");

        private readonly TranslationString _noWorkingFolderText = new("No working directory");
        private readonly TranslationString _commitButtonText = new("Commit");

        private readonly TranslationString _undoLastCommitText = new("You will still be able to find all the commit's changes in the staging area\n\nDo you want to continue?");
        private readonly TranslationString _undoLastCommitCaption = new("Undo last commit");

        #endregion

        private readonly SplitterManager _splitterManager = new(new AppSettingsPath("FormBrowse"));
        private readonly GitStatusMonitor _gitStatusMonitor;
        private readonly FormBrowseMenus _formBrowseMenus;
        private readonly IFormBrowseController _controller;
        private readonly ICommitDataManager _commitDataManager;
        private readonly IAppTitleGenerator _appTitleGenerator;
        private readonly IAheadBehindDataProvider? _aheadBehindDataProvider;
        private readonly IWindowsJumpListManager _windowsJumpListManager;
        private readonly ISubmoduleStatusProvider _submoduleStatusProvider;
        private List<ToolStripItem>? _currentSubmoduleMenuItems;
        private readonly FormBrowseDiagnosticsReporter _formBrowseDiagnosticsReporter;
        private BuildReportTabPageExtension? _buildReportTabPageExtension;
        private readonly ShellProvider _shellProvider = new();
        private ConEmuControl? _terminal;
        private Dashboard? _dashboard;

        private TabPage? _consoleTabPage;

        private readonly Dictionary<Brush, Icon> _overlayIconByBrush = new();

        private UpdateTargets _selectedRevisionUpdatedTargets = UpdateTargets.None;

        public override RevisionGridControl RevisionGridControl { get => RevisionGrid; }

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormBrowse()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
            InitializeComplete();
        }

        /// <summary>
        /// Open Browse - main GUI including dashboard.
        /// </summary>
        /// <param name="commands">The commands in the current form.</param>
        /// <param name="args">The start up arguments.</param>
        public FormBrowse(GitUICommands commands, BrowseArguments args)
            : base(commands)
        {
            SystemEvents.SessionEnding += (sender, args) => SaveApplicationSettings();

            InitializeComponent();
            BackColor = OtherColors.BackgroundColor;

            WorkaroundPaddingIncreaseBug();

            _appTitleGenerator = ManagedExtensibility.GetExport<IAppTitleGenerator>().Value;
            _windowsJumpListManager = ManagedExtensibility.GetExport<IWindowsJumpListManager>().Value;

            _formBrowseDiagnosticsReporter = new FormBrowseDiagnosticsReporter(this);

            MainSplitContainer.Visible = false;
            MainSplitContainer.SplitterDistance = DpiUtil.Scale(260);

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;
                PluginRegistry.Initialize();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                RegisterPlugins();
            }).FileAndForget();

            InitCountArtificial(out _gitStatusMonitor);

            _formBrowseMenus = new FormBrowseMenus(mainMenuStrip);

            RevisionGrid.SuspendRefreshRevisions();

            ToolStripFilters.Bind(() => Module, RevisionGrid);
            InitMenusAndToolbars(args.RevFilter, args.PathFilter);

            InitRevisionGrid(args.SelectedId, args.FirstId);
            InitCommitDetails();

            InitializeComplete();

            HotkeysEnabled = true;
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);

            UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
            UICommands.BrowseRepo = this;

            _controller = new FormBrowseController(new GitGpgController(() => Module), new RepositoryCurrentBranchNameProvider(), new InvalidRepositoryRemover());
            _commitDataManager = new CommitDataManager(() => Module);

            _submoduleStatusProvider = SubmoduleStatusProvider.Default;
            _submoduleStatusProvider.StatusUpdating += SubmoduleStatusProvider_StatusUpdating;
            _submoduleStatusProvider.StatusUpdated += SubmoduleStatusProvider_StatusUpdated;

            foreach (var control in this.FindDescendants())
            {
                control.AllowDrop = true;
                control.DragEnter += FormBrowse_DragEnter;
                control.DragDrop += FormBrowse_DragDrop;
            }

            _aheadBehindDataProvider = new AheadBehindDataProvider(() => Module.GitExecutable);
            toolStripButtonPush.Initialize(_aheadBehindDataProvider);
            repoObjectsTree.Initialize(_aheadBehindDataProvider, branchFilterAction: ToolStripFilters.SetBranchFilter, RevisionGrid, RevisionGrid, RevisionGrid);
            revisionDiff.Bind(RevisionGrid, fileTree, RequestRefresh);
            fileTree.Bind(RevisionGrid, RequestRefresh);

            // We're launching the main form, and whilst the module has been set for us we still need to formally switch to it
            SetGitModule(this, new GitModuleEventArgs(new GitModule(Module.WorkingDir)));

            RevisionGrid.ResumeRefreshRevisions();

            return;

            void InitCountArtificial(out GitStatusMonitor gitStatusMonitor)
            {
                Brush? lastBrush = null;

                gitStatusMonitor = new GitStatusMonitor(this);
                if (!NeedsGitStatusMonitor())
                {
                    gitStatusMonitor.Active = false;
                }

                gitStatusMonitor.GitStatusMonitorStateChanged += (s, e) =>
                {
                    var status = e.State;
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

                    var brush = UpdateCommitButtonAndGetBrush(status, countToolbar);

                    RevisionGrid.UpdateArtificialCommitCount(countArtificial ? status : null);
                    toolStripButtonLevelUp.Image = Module.SuperprojectModule is not null ? Images.NavigateUp : Images.SubmodulesManage;

                    if (countToolbar || countArtificial)
                    {
                        if (!ReferenceEquals(brush, lastBrush)
                            && EnvUtils.RunningOnWindowsWithMainWindow())
                        {
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

                            RepoStateVisualiser repoStateVisualiser = new();
                            var (image, _) = repoStateVisualiser.Invoke(status);
                            _windowsJumpListManager.UpdateCommitIcon(image);
                        }

                        if (AppSettings.ShowSubmoduleStatus)
                        {
                            Validates.NotNull(_submoduleStatusProvider);

                            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                            {
                                try
                                {
                                    await TaskScheduler.Default;
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
                };
            }

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

        // Contains app init logics ONLY. All repo-specific logic must be placed UICommands.PostRepositoryChanged handler.
        protected override void OnLoad(EventArgs e)
        {
            _formBrowseMenus.CreateToolbarsMenus(ToolStripMain, ToolStripFilters, ToolStripScripts);

            HideVariableMainMenuItems();
            RefreshSplitViewLayout();
            LayoutRevisionInfo();
            SetSplitterPositions();
            InternalInitialize(false);

            base.OnLoad(e);

            _formBrowseDiagnosticsReporter.Report();
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
                        new WindowsThumbnailToolbarButton(toolStripButtonPull.Text, toolStripButtonPull.Image, PullToolStripMenuItemClick)));
            }

            this.InvokeAsync(OnActivate).FileAndForget();
            base.OnActivated(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveApplicationSettings();

            foreach (var control in this.FindDescendants())
            {
                control.DragEnter -= FormBrowse_DragEnter;
                control.DragDrop -= FormBrowse_DragDrop;
            }

            base.OnFormClosing(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _splitterManager.SaveSplitters();
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            PluginRegistry.Unregister(UICommands);
            base.OnClosed(e);
        }

        protected override void OnUICommandsChanged(GitUICommandsChangedEventArgs e)
        {
            var oldCommands = e.OldCommands;
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
            if (RevisionGrid.FilterIsApplied(false))
            {
                // Clear filter
                ToolStripFilters.SetRevisionFilter(string.Empty);
            }

            // If a branch filter is applied by text or using the menus "Show current branch only"
            else if (RevisionGrid.FilterIsApplied(true) || AppSettings.BranchFilterEnabled)
            {
                // Clear branch filter
                ToolStripFilters.SetBranchFilter(string.Empty);

                // Execute the "Show all branches" menu option
                RevisionGrid.ShowAllBranches();
            }
        }

        private bool NeedsGitStatusMonitor()
        {
            return AppSettings.ShowGitStatusInBrowseToolbar || (AppSettings.ShowGitStatusForArtificialCommits && AppSettings.RevisionGraphShowArtificialCommits);
        }

        private void UICommands_PostRepositoryChanged(object sender, GitUIEventArgs e)
        {
            this.InvokeAsync(RefreshRevisions).FileAndForget();

            ToolStripFilters.UpdateBranchFilterItems();
            UpdateSubmodulesStructure();
            UpdateStashCount();

            revisionDiff.UICommands_PostRepositoryChanged(sender, e);
        }

        private void RefreshRevisions()
        {
            if (RevisionGrid.IsDisposed || IsDisposed || Disposing)
            {
                return;
            }

            _gitStatusMonitor.InvalidateGitWorkingDirectoryStatus();
            RequestRefresh();

            if (_dashboard is null || !_dashboard.Visible)
            {
                revisionDiff.RefreshArtificial();
                RevisionGrid.ForceRefreshRevisions();
                RevisionGrid.IndexWatcher.Reset();

                InternalInitialize(true);
            }

            toolStripButtonPush.DisplayAheadBehindInformation(Module.GetSelectedBranch());
        }

        private void RequestRefresh() => _gitStatusMonitor?.RequestRefresh();

        private void RefreshSelection()
        {
            var selectedRevisions = RevisionGrid.GetSelectedRevisions();
            var selectedRevision = selectedRevisions.FirstOrDefault();

            FillFileTree(selectedRevision);
            FillDiff(selectedRevisions);

            var oldBody = selectedRevision?.Body;
            FillCommitInfo(selectedRevision);

            // If the revision's body has been updated then the grid needs to be refreshed to display it
            if (AppSettings.ShowCommitBodyInRevisionGrid && selectedRevision is not null && selectedRevision.HasMultiLineMessage && oldBody != selectedRevision.Body)
            {
                RevisionGrid.Refresh();
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(() => FillGpgInfoAsync(selectedRevision));
            FillBuildReport(selectedRevision);
            repoObjectsTree.SelectionChanged(selectedRevisions);
        }

        #region IBrowseRepo

        public void GoToRef(string refName, bool showNoRevisionMsg, bool toggleSelection = false)
        {
            using (WaitCursorScope.Enter())
            {
                RevisionGrid.GoToRef(refName, showNoRevisionMsg, toggleSelection);
            }
        }

        #endregion

        public void SetPathFilter(string pathFilter)
        {
            revisionDiff.FallbackFollowedFile = pathFilter;
            fileTree.FallbackFollowedFile = pathFilter;
            RevisionGrid.SetAndApplyPathFilter(pathFilter.QuoteNE());
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

            DiagnosticsClient.TrackPageView("Dashboard");
        }

        private void HideDashboard()
        {
            MainSplitContainer.Visible = true;
            if (_dashboard is null || !_dashboard.Visible)
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
                item.Enabled = !(item.Tag is IGitPluginForRepository) || validWorkingDir;
            }
        }

        private void RegisterPlugins()
        {
            const string PluginManagerName = "Plugin Manager";
            var existingPluginMenus = pluginsToolStripMenuItem.DropDownItems.OfType<ToolStripMenuItem>().ToLookup(c => c.Tag);

            lock (PluginRegistry.Plugins)
            {
                var pluginEntries = PluginRegistry.Plugins
                    .OrderByDescending(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase);

                // pluginsToolStripMenuItem.DropDownItems menu already contains at least 2 items:
                //    [1] Separator
                //    [0] Plugin Settings
                // insert all plugins except 'Plugin Manager' above the separator
                foreach (var plugin in pluginEntries)
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
                            RefreshRevisions();
                        }
                    };

                    if (plugin.Name == PluginManagerName)
                    {
                        // insert Plugin Manager below the separator
                        pluginsToolStripMenuItem.DropDownItems.Insert(pluginsToolStripMenuItem.DropDownItems.Count - 1, item);
                    }
                    else
                    {
                        pluginsToolStripMenuItem.DropDownItems.Insert(0, item);
                    }
                }

                if (_dashboard?.Visible ?? false)
                {
                    // now that plugins are registered, populate Git-host-plugin actions on Dashboard, like "Clone GitHub repository"
                    _dashboard.RefreshContent();
                }

                mainMenuStrip?.Refresh();
            }

            // Allow the plugin to perform any self-registration actions
            PluginRegistry.Register(UICommands);

            UICommands.RaisePostRegisterPlugin(this);

            // Show "Repository hosts" menu item when there is at least 1 repository host plugin loaded
            _repositoryHostsToolStripMenuItem.Visible = PluginRegistry.GitHosters.Count > 0;
            if (PluginRegistry.GitHosters.Count == 1)
            {
                _repositoryHostsToolStripMenuItem.Text = PluginRegistry.GitHosters[0].Name;
            }

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
            _repositoryHostsToolStripMenuItem.Visible = false;
            _formBrowseMenus.RemoveRevisionGridMainMenuItems();
            mainMenuStrip.Refresh();
        }

        private void InternalInitialize(bool hard)
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
                bool isDashboard = _dashboard is not null && _dashboard.Visible;
                bool validBrowseDir = !isDashboard && Module.IsValidGitWorkingDir();

                branchSelect.Text = validBrowseDir ? Module.GetSelectedBranch() : "";
                toolStripButtonLevelUp.Enabled = hasWorkingDir && !bareRepository;
                CommitInfoTabControl.Visible = validBrowseDir;
                fileExplorerToolStripMenuItem.Enabled = validBrowseDir;
                manageRemoteRepositoriesToolStripMenuItem1.Enabled = validBrowseDir;
                branchSelect.Enabled = validBrowseDir;
                toolStripButtonCommit.Enabled = validBrowseDir && !bareRepository;

                toolStripButtonPull.Enabled = validBrowseDir;
                toolStripButtonPush.Enabled = validBrowseDir;
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

                if (repositoryToolStripMenuItem.Visible)
                {
                    manageSubmodulesToolStripMenuItem.Enabled = !bareRepository;
                    updateAllSubmodulesToolStripMenuItem.Enabled = !bareRepository;
                    synchronizeAllSubmodulesToolStripMenuItem.Enabled = !bareRepository;
                    editgitignoreToolStripMenuItem1.Enabled = !bareRepository;
                    editGitAttributesToolStripMenuItem.Enabled = !bareRepository;
                    editmailmapToolStripMenuItem.Enabled = !bareRepository;
                }

                if (commandsToolStripMenuItem.Visible)
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
                gitGUIToolStripMenuItem.Enabled = !bareRepository;

                SetShortcutKeyDisplayStringsFromHotkeySettings();

                if (hard && hasWorkingDir)
                {
                    ShowRevisions();
                }

                RefreshWorkingDirComboText();

                OnActivate();

                LoadUserMenu();

                if (validBrowseDir)
                {
                    _windowsJumpListManager.AddToRecent(Module.WorkingDir);

                    // add Navigate and View menu
                    _formBrowseMenus.ResetMenuCommandSets();
                    _formBrowseMenus.AddMenuCommandSet(MainMenuItem.NavigateMenu, RevisionGrid.MenuCommands.NavigateMenuCommands);
                    _formBrowseMenus.AddMenuCommandSet(MainMenuItem.ViewMenu, RevisionGrid.MenuCommands.ViewMenuCommands);

                    _formBrowseMenus.InsertRevisionGridMainMenuItems(repositoryToolStripMenuItem);

                    toolStripButtonPush.DisplayAheadBehindInformation(Module.GetSelectedBranch());

                    ActiveControl = RevisionGrid;
                    RevisionGrid.IndexWatcher.Reset();
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
                var scripts = ScriptManager.GetScripts()
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
                foreach (var script in scripts)
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

                    button.Click += delegate
                    {
                        if (ScriptRunner.RunScript(this, Module, script.Name, UICommands, RevisionGrid).NeedsGridRefresh)
                        {
                            RevisionGrid.RefreshRevisions();
                        }
                    };

                    // add to toolstrip
                    ToolStripScripts.Items.Add(button);
                }
            }

            void ShowRevisions()
            {
                if (RevisionGrid.IndexWatcher.IndexChanged)
                {
                    RefreshSelection();
                }

                RevisionGrid.IndexWatcher.Reset();
            }
        }

        private void SetShortcutKeyDisplayStringsFromHotkeySettings()
        {
            // Add shortcuts to the menu items
            gitBashToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.GitBash).ToShortcutKeyDisplayString();
            commitToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.Commit).ToShortcutKeyDisplayString();
            stashChangesToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.Stash).ToShortcutKeyDisplayString();
            stashPopToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.StashPop).ToShortcutKeyDisplayString();
            closeToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.CloseRepository).ToShortcutKeyDisplayString();
            gitGUIToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.GitGui).ToShortcutKeyDisplayString();
            kGitToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.GitGitK).ToShortcutKeyDisplayString();
            checkoutBranchToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.CheckoutBranch).ToShortcutKeyDisplayString();
            settingsToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.OpenSettings).ToShortcutKeyDisplayString();
            branchToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.CreateBranch).ToShortcutKeyDisplayString();
            tagToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.CreateTag).ToShortcutKeyDisplayString();
            mergeBranchToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.MergeBranches).ToShortcutKeyDisplayString();
            pullToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.PullOrFetch).ToShortcutKeyDisplayString();
            pullToolStripMenuItem1.ShortcutKeyDisplayString = GetShortcutKeys(Command.PullOrFetch).ToShortcutKeyDisplayString();
            pushToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.Push).ToShortcutKeyDisplayString();
            rebaseToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.Rebase).ToShortcutKeyDisplayString();
            rebaseToolStripMenuItem1.ShortcutKeyDisplayString = GetShortcutKeys(Command.Rebase).ToShortcutKeyDisplayString();

            // TODO: add more
        }

        private void OnActivate()
        {
            // check if we are in the middle of bisect
            notificationBarBisectInProgress.RefreshBisect();

            // check if we are in the middle of an action (merge/rebase/etc.)
            notificationBarGitActionInProgress.RefreshGitAction();
        }

        private void UpdateStashCount()
        {
            if (AppSettings.ShowStashCount)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    // Add a delay to not interfere with GUI updates when switching repository
                    await Task.Delay(500);
                    await TaskScheduler.Default;

                    var result = Module.GetStashes(noLocks: true).Count;

                    await this.SwitchToMainThreadAsync();

                    toolStripSplitStash.Text = $"({result})";
                }).FileAndForget();
            }
            else
            {
                toolStripSplitStash.Text = string.Empty;
            }
        }

        #region Working directory combo box

        /// <summary>Updates the text shown on the combo button itself.</summary>
        private void RefreshWorkingDirComboText()
        {
            var path = Module.WorkingDir;

            // it appears at times Module.WorkingDir path is an empty string, this caused issues like #4874
            if (string.IsNullOrWhiteSpace(path))
            {
                _NO_TRANSLATE_WorkingDir.Text = _noWorkingFolderText.Text;
                return;
            }

            var recentRepositoryHistory = ThreadHelper.JoinableTaskFactory.Run(
                () => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(path));

            List<RecentRepoInfo> mostRecentRepos = new();
            using var graphics = CreateGraphics();
            RecentRepoSplitter splitter = new()
            {
                MeasureFont = _NO_TRANSLATE_WorkingDir.Font,
                Graphics = graphics
            };

            splitter.SplitRecentRepos(recentRepositoryHistory, mostRecentRepos, mostRecentRepos);

            var ri = mostRecentRepos.Find(e => e.Repo.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase));

            _NO_TRANSLATE_WorkingDir.Text = PathUtil.GetDisplayPath(ri?.Caption ?? path);

            if (AppSettings.RecentReposComboMinWidth > 0)
            {
                _NO_TRANSLATE_WorkingDir.AutoSize = false;
                var captionWidth = graphics.MeasureString(_NO_TRANSLATE_WorkingDir.Text, _NO_TRANSLATE_WorkingDir.Font).Width;
                captionWidth = captionWidth + _NO_TRANSLATE_WorkingDir.DropDownButtonWidth + 5;
                _NO_TRANSLATE_WorkingDir.Width = Math.Max(AppSettings.RecentReposComboMinWidth, (int)captionWidth);
            }
            else
            {
                _NO_TRANSLATE_WorkingDir.AutoSize = true;
            }
        }

        private void WorkingDirDropDownOpening(object sender, EventArgs e)
        {
            _NO_TRANSLATE_WorkingDir.DropDownItems.Clear();

            ToolStripMenuItem tsmiCategorisedRepos = new(tsmiFavouriteRepositories.Text, tsmiFavouriteRepositories.Image);
            PopulateFavouriteRepositoriesMenu(tsmiCategorisedRepos);
            if (tsmiCategorisedRepos.DropDownItems.Count > 0)
            {
                _NO_TRANSLATE_WorkingDir.DropDownItems.Add(tsmiCategorisedRepos);
            }

            PopulateRecentRepositoriesMenu(_NO_TRANSLATE_WorkingDir);

            _NO_TRANSLATE_WorkingDir.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem mnuOpenLocalRepository = new(openToolStripMenuItem.Text, openToolStripMenuItem.Image) { ShortcutKeys = openToolStripMenuItem.ShortcutKeys };
            mnuOpenLocalRepository.Click += OpenToolStripMenuItemClick;
            _NO_TRANSLATE_WorkingDir.DropDownItems.Add(mnuOpenLocalRepository);

            ToolStripMenuItem mnuRecentReposSettings = new(_configureWorkingDirMenu.Text);
            mnuRecentReposSettings.Click += (hs, he) =>
            {
                using (FormRecentReposSettings frm = new())
                {
                    frm.ShowDialog(this);
                }

                RefreshWorkingDirComboText();
            };

            _NO_TRANSLATE_WorkingDir.DropDownItems.Add(mnuRecentReposSettings);

            ToolStripFilters.PreventToolStripSplitButtonClosing((ToolStripSplitButton)sender);
        }

        private void WorkingDirClick(object sender, EventArgs e)
        {
            _NO_TRANSLATE_WorkingDir.ShowDropDown();
        }

        private void _NO_TRANSLATE_WorkingDir_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                OpenToolStripMenuItemClick(sender, e);
            }
        }

        #endregion

        private void FillFileTree(GitRevision revision)
        {
            // Don't show the "File Tree" tab for artificial commits
            var showFileTreeTab = revision?.IsArtificial != true;

            if (showFileTreeTab)
            {
                if (TreeTabPage.Parent is null)
                {
                    var index = CommitInfoTabControl.TabPages.IndexOf(DiffTabPage);
                    Debug.Assert(index != -1, "TabControl should contain diff tab page");
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

            var children = RevisionGrid.GetRevisionChildren(revision.ObjectId);
            RevisionInfo.SetRevisionWithChildren(revision, children);
        }

        private async Task FillGpgInfoAsync(GitRevision? revision)
        {
            if (!AppSettings.ShowGpgInformation.Value || CommitInfoTabControl.SelectedTab != GpgInfoTabPage)
            {
                return;
            }

            if (revision is null)
            {
                return;
            }

            var info = await _controller.LoadGpgInfoAsync(revision);
            revisionGpgInfo1.DisplayGpgInfo(info);
        }

        private void OpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitModule? module = FormOpenDirectory.OpenModule(this, Module);
            if (module is not null)
            {
                SetGitModule(this, new GitModuleEventArgs(module));
            }
        }

        private void CheckoutToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCheckoutRevisionDialog(this);
        }

        private void CloneToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCloneDialog(this, string.Empty, false, SetGitModule);
        }

        private void CommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCommitDialog(this);
        }

        private void InitNewRepositoryToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartInitializeDialog(this, gitModuleChanged: SetGitModule);
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

        private void AboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            using FormAbout frm = new();
            frm.ShowDialog(this);
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

        private void GitGuiToolStripMenuItemClick(object sender, EventArgs e)
        {
            Module.RunGui();
        }

        private void FormatPatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartFormatPatchDialog(this);
        }

        private void GitcommandLogToolStripMenuItemClick(object sender, EventArgs e)
        {
            FormGitCommandLog.ShowOrActivate(this);
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
            UICommands.StartResetChangesDialog(this);
            RequestRefresh();
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
            var revisions = RevisionGrid.GetSelectedRevisions(SortDirection.Descending);

            UICommands.StartCherryPickDialog(this, revisions);
        }

        private void MergeBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartMergeBranchDialog(this, null);
        }

        private void OnShowSettingsClick(object sender, EventArgs e)
        {
            var translation = AppSettings.Translation;
            var commitInfoPosition = AppSettings.CommitInfoPosition;

            UICommands.StartSettingsDialog(this);

            if (translation != AppSettings.Translation)
            {
                Translator.Translate(this, AppSettings.CurrentTranslation);
            }

            if (commitInfoPosition != AppSettings.CommitInfoPosition)
            {
                LayoutRevisionInfo();
            }

            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            RevisionGrid.ReloadHotkeys();
            RevisionGrid.ReloadTranslation();
            fileTree.ReloadHotkeys();
            revisionDiff.ReloadHotkeys();
            SetShortcutKeyDisplayStringsFromHotkeySettings();

            // Clear the separate caches for diff/merge tools
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await new CustomDiffMergeToolProvider().ClearAsync(isDiff: false);
            }).FileAndForget();
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                revisionDiff.CancelLoadCustomDifftools();
                RevisionGrid.CancelLoadCustomDifftools();

                await new CustomDiffMergeToolProvider().ClearAsync(isDiff: true);

                // The tool lists are created when the forms are init, must be redone after clearing the cache
                revisionDiff.LoadCustomDifftools();
                RevisionGrid.LoadCustomDifftools();
            }).FileAndForget();

            _dashboard?.RefreshContent();

            _gitStatusMonitor.Active = NeedsGitStatusMonitor() && Module.IsValidGitWorkingDir();

            RefreshDefaultPullAction();
        }

        private void TagToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCreateTagDialog(this, RevisionGrid.LatestSelectedRevision);
        }

        private void KGitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Module.RunGitK();
        }

        private void DonateToolStripMenuItemClick(object sender, EventArgs e)
        {
            using FormDonate frm = new();
            frm.ShowDialog(this);
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
            var revisions = RevisionGrid.GetSelectedRevisions();
            if (revisions.Count < 1 || revisions.Count > 2)
            {
                MessageBoxes.SelectOnlyOneOrTwoRevisions(this);
                return;
            }

            GitRevision mainRevision = revisions.First();
            GitRevision? diffRevision = null;
            if (revisions.Count == 2)
            {
                diffRevision = revisions.Last();
            }

            UICommands.StartArchiveDialog(this, mainRevision, diffRevision);
        }

        private void EditMailMapToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartMailMapDialog(this);
        }

        private void EditLocalGitConfigToolStripMenuItemClick(object sender, EventArgs e)
        {
            var fileName = Path.Combine(Module.ResolveGitInternalPath("config"));
            UICommands.StartFileEditorDialog(fileName, true);
        }

        private void CompressGitDatabaseToolStripMenuItemClick(object sender, EventArgs e)
        {
            FormProcess.ReadDialog(this, arguments: "gc", Module.WorkingDir, input: null, useDialogSettings: true);
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
            var revisions = RevisionGrid.GetSelectedRevisions();

            if (revisions.Count == 0)
            {
                return;
            }

            if (revisions.Count == 2)
            {
                string? to = null;
                string? from = null;

                string currentBranch = Module.GetSelectedBranch();
                var currentCheckout = RevisionGrid.CurrentCheckout;

                if (revisions[0].ObjectId == currentCheckout)
                {
                    from = revisions[1].ObjectId.ToShortString();
                    to = currentBranch;
                }
                else if (revisions[1].ObjectId == currentCheckout)
                {
                    from = revisions[0].ObjectId.ToShortString();
                    to = currentBranch;
                }

                UICommands.StartRebaseDialog(this, from, to, null, interactive: false, startRebaseImmediately: false);
            }
            else
            {
                UICommands.StartRebaseDialog(this, revisions.First().Guid);
            }
        }

        private void StartAuthenticationAgentToolStripMenuItemClick(object sender, EventArgs e)
        {
            new Executable(AppSettings.Pageant, Module.WorkingDir).Start();
        }

        private void GenerateOrImportKeyToolStripMenuItemClick(object sender, EventArgs e)
        {
            new Executable(AppSettings.Puttygen, Module.WorkingDir).Start();
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
        }

        private void ChangelogToolStripMenuItemClick(object sender, EventArgs e)
        {
            using FormChangeLog frm = new();
            frm.ShowDialog(this);
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
                var submodule = toolStripMenuItem.Tag as string;
                Validates.NotNull(Module.SuperprojectModule);
                FormProcess.ShowDialog(this, arguments: GitCommandHelpers.SubmoduleUpdateCmd(submodule), Module.SuperprojectModule.WorkingDir, input: null, useDialogSettings: true);
            }

            UICommands.RepoChangedNotifier.Notify();
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

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Close();
        }

        private void tsmiFavouriteRepositories_DropDownOpening(object sender, EventArgs e)
        {
            tsmiFavouriteRepositories.DropDownItems.Clear();
            PopulateFavouriteRepositoriesMenu(tsmiFavouriteRepositories);
        }

        private void tsmiRecentRepositories_DropDownOpening(object sender, EventArgs e)
        {
            tsmiRecentRepositories.DropDownItems.Clear();
            PopulateRecentRepositoriesMenu(tsmiRecentRepositories);
            if (tsmiRecentRepositories.DropDownItems.Count < 1)
            {
                return;
            }

            tsmiRecentRepositories.DropDownItems.Add(clearRecentRepositoriesListToolStripMenuItem);
            TranslateItem(tsmiRecentRepositoriesClear.Name, tsmiRecentRepositoriesClear);
            tsmiRecentRepositories.DropDownItems.Add(tsmiRecentRepositoriesClear);
        }

        private void tsmiRecentRepositoriesClear_Click(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var repositoryHistory = Array.Empty<Repository>();
                await RepositoryHistoryManager.Locals.SaveRecentHistoryAsync(repositoryHistory);

                await this.SwitchToMainThreadAsync();
                _dashboard?.RefreshContent();
            });
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

        private void UserManualToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Point to the default documentation, will work also if the old doc version is removed
            OsShellUtil.OpenUrlInDefaultBrowser(AppSettings.DocumentationBaseUrl);
        }

        private void CleanupToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCleanupRepositoryDialog(this);
        }

        private void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container)
        {
            var repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync());
            if (repositoryHistory.Count < 1)
            {
                return;
            }

            PopulateFavouriteRepositoriesMenu(container, repositoryHistory);
        }

        private void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container, in IList<Repository> repositoryHistory)
        {
            List<RecentRepoInfo> mostRecentRepos = new();
            List<RecentRepoInfo> lessRecentRepos = new();

            using (var graphics = CreateGraphics())
            {
                RecentRepoSplitter splitter = new()
                {
                    MeasureFont = container.Font,
                    Graphics = graphics
                };

                splitter.SplitRecentRepos(repositoryHistory, mostRecentRepos, lessRecentRepos);
            }

            foreach (var repo in mostRecentRepos.Union(lessRecentRepos).GroupBy(k => k.Repo.Category).OrderBy(k => k.Key))
            {
                AddFavouriteRepositories(repo.Key, repo.ToList());
            }

            void AddFavouriteRepositories(string? category, IList<RecentRepoInfo> repos)
            {
                ToolStripMenuItem menuItemCategory;
                if (!container.DropDownItems.ContainsKey(category))
                {
                    menuItemCategory = new ToolStripMenuItem(category);
                    container.DropDownItems.Add(menuItemCategory);
                }
                else
                {
                    menuItemCategory = (ToolStripMenuItem)container.DropDownItems[category];
                }

                foreach (var r in repos)
                {
                    _controller.AddRecentRepositories(menuItemCategory, r.Repo, r.Caption, SetGitModule);
                }
            }
        }

        private void PopulateRecentRepositoriesMenu(ToolStripDropDownItem container)
        {
            List<RecentRepoInfo> mostRecentRepos = new();
            List<RecentRepoInfo> lessRecentRepos = new();

            var repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.LoadRecentHistoryAsync());
            if (repositoryHistory.Count < 1)
            {
                return;
            }

            using (var graphics = CreateGraphics())
            {
                RecentRepoSplitter splitter = new()
                {
                    MeasureFont = container.Font,
                    Graphics = graphics
                };

                splitter.SplitRecentRepos(repositoryHistory, mostRecentRepos, lessRecentRepos);
            }

            foreach (var repo in mostRecentRepos)
            {
                _controller.AddRecentRepositories(container, repo.Repo, repo.Caption, SetGitModule);
            }

            if (lessRecentRepos.Count > 0)
            {
                if (mostRecentRepos.Count > 0 && (AppSettings.SortMostRecentRepos || AppSettings.SortLessRecentRepos))
                {
                    container.DropDownItems.Add(new ToolStripSeparator());
                }

                foreach (var repo in lessRecentRepos)
                {
                    _controller.AddRecentRepositories(container, repo.Repo, repo.Caption, SetGitModule);
                }
            }
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

            var module = e.GitModule;
            HideVariableMainMenuItems();
            PluginRegistry.Unregister(UICommands);
            _gitStatusMonitor.InvalidateGitWorkingDirectoryStatus();
            _submoduleStatusProvider.Init();

            UICommands = new GitUICommands(module);
            if (Module.IsValidGitWorkingDir())
            {
                var path = Module.WorkingDir;
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(path));
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
                    RevisionGrid.DisableFilters();
                    SetPathFilter(pathFilter: null);
                    ToolStripFilters.ClearFilters();
                    AppSettings.BranchFilterEnabled = AppSettings.BranchFilterEnabled && AppSettings.ShowCurrentBranchOnly;
                }

                RevisionInfo.SetRevisionWithChildren(revision: null, children: Array.Empty<ObjectId>());

                // This will raise UICommands.PostRepositoryChanged event
                UICommands.RepoChangedNotifier.Notify();
            }
            else
            {
                dashboardToolStripMenuItem.Visible = true;

                RevisionGrid.IndexWatcher.Reset();
                MainSplitContainer.Visible = false;
                ShowDashboard();
            }

            RegisterPlugins();
        }

        private void TranslateToolStripMenuItemClick(object sender, EventArgs e)
        {
            OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/wiki/Translations");
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

        public static void CopyFullPathToClipboard(FileStatusList diffFiles, GitModule module)
        {
            if (!diffFiles.SelectedItems.Any())
            {
                return;
            }

            StringBuilder fileNames = new();
            foreach (var item in diffFiles.SelectedItems)
            {
                var path = Path.Combine(module.WorkingDir, item.Item.Name);
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                // Only use append line when multiple items are selected.
                // This to make it easier to use the text from clipboard when 1 file is selected.
                if (fileNames.Length > 0)
                {
                    fileNames.AppendLine();
                }

                fileNames.Append(path.ToNativePath());
            }

            ClipboardUtil.TrySetText(fileNames.ToString());
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

            UICommands.RepoChangedNotifier.Notify();
        }

        private void CurrentBranchDropDownOpening(object sender, EventArgs e)
        {
            branchSelect.DropDownItems.Clear();

            AddCheckoutBranchMenuItem();
            branchSelect.DropDownItems.Add(new ToolStripSeparator());
            AddBranchesMenuItems();

            ToolStripFilters.PreventToolStripSplitButtonClosing(sender as ToolStripSplitButton);

            void AddCheckoutBranchMenuItem()
            {
                ToolStripMenuItem checkoutBranchItem = new(checkoutBranchToolStripMenuItem.Text, Images.BranchCheckout)
                {
                    ShortcutKeys = GetShortcutKeys(Command.CheckoutBranch),
                    ShortcutKeyDisplayString = GetShortcutKeys(Command.CheckoutBranch).ToShortcutKeyDisplayString()
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
                UICommands.RepoChangedNotifier.Notify();
            }
            else
            {
                MessageBox.Show(this, _noReposHostPluginLoaded.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void _viewPullRequestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TryGetRepositoryHost(out var repoHost))
            {
                return;
            }

            UICommands.StartPullRequestsDialog(this, repoHost);
        }

        private void _createPullRequestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TryGetRepositoryHost(out var repoHost))
            {
                return;
            }

            UICommands.StartCreatePullRequest(this, repoHost);
        }

        private void _addUpstreamRemoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                if (!TryGetRepositoryHost(out var repoHost))
                {
                    return;
                }

                var remoteName = await repoHost.AddUpstreamRemoteAsync();
                if (!string.IsNullOrEmpty(remoteName))
                {
                    UICommands.StartPullDialogAndPullImmediately(this, null, remoteName, AppSettings.PullAction.Fetch);
                }
            }).FileAndForget();
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
            ToggleBranchTreePanel = 21,
            FocusBranchTree = 25,
            FocusGpgInfo = 26,
            FocusGitConsole = 29,
            FocusBuildServerStatus = 30,
            FocusNextTab = 31,
            FocusPrevTab = 32,

            // START menu

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
            QuickFetch = 11,
            QuickPull = 12,
            QuickPush = 13,
            Stash = 16,
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

        internal Keys GetShortcutKeys(Command cmd)
        {
            return GetShortcutKeys((int)cmd);
        }

        private void AddNotes()
        {
            var revision = RevisionGrid.GetSelectedRevisions().FirstOrDefault();
            var objectId = revision?.ObjectId;

            if (objectId is null || objectId.IsArtificial)
            {
                return;
            }

            Module.EditNotes(objectId);
            FillCommitInfo(revision);
        }

        private void FindFileInSelectedCommit()
        {
            CommitInfoTabControl.SelectedTab = TreeTabPage;

            AppSettings.ShowSplitViewLayout = true;
            RefreshSplitViewLayout();

            fileTree.InvokeFindFileDialog();
        }

        private void QuickFetch()
        {
            bool success = ScriptManager.RunEventScripts(this, ScriptEvent.BeforeFetch);
            if (!success)
            {
                return;
            }

            success = FormProcess.ShowDialog(this, arguments: Module.FetchCmd(string.Empty, string.Empty, string.Empty), Module.WorkingDir, input: null, useDialogSettings: true);
            if (!success)
            {
                return;
            }

            ScriptManager.RunEventScripts(this, ScriptEvent.AfterFetch);
            UICommands.RepoChangedNotifier.Notify();
        }

        protected override CommandStatus ExecuteCommand(int cmd)
        {
            switch ((Command)cmd)
            {
                case Command.GitBash: userShell.PerformButtonClick(); break;
                case Command.GitGui: Module.RunGui(); break;
                case Command.GitGitK: Module.RunGitK(); break;
                case Command.FocusBranchTree: FocusBranchTree(); break;
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
                case Command.Commit: UICommands.StartCommitDialog(this); break;
                case Command.AddNotes: AddNotes(); break;
                case Command.FindFileInSelectedCommit: FindFileInSelectedCommit(); break;
                case Command.CheckoutBranch: UICommands.StartCheckoutBranch(this); break;
                case Command.QuickFetch: QuickFetch(); break;
                case Command.QuickPull: DoPull(pullAction: AppSettings.PullAction.Merge, isSilent: true); break;
                case Command.QuickPush: UICommands.StartPushDialog(this, true); break;
                case Command.CloseRepository: SetWorkingDir(""); break;
                case Command.Stash: UICommands.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash); break;
                case Command.StashPop: UICommands.StashPop(this); break;
                case Command.OpenCommitsWithDifftool: RevisionGrid.DiffSelectedCommitsWithDifftool(); break;
                case Command.OpenWithDifftool: OpenWithDifftool(); break;
                case Command.OpenWithDifftoolFirstToLocal: OpenWithDifftoolFirstToLocal(); break;
                case Command.OpenWithDifftoolSelectedToLocal: OpenWithDifftoolSelectedToLocal(); break;
                case Command.OpenSettings: EditSettings.PerformClick(); break;
                case Command.ToggleBranchTreePanel: toggleBranchTreePanel.PerformClick(); break;
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

            void FocusBranchTree()
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
                    var tabPage = control.Parent as TabPage;
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

        internal CommandStatus ExecuteCommand(Command cmd)
        {
            return ExecuteCommand((int)cmd);
        }

        #endregion

        public static void OpenContainingFolder(FileStatusList diffFiles, GitModule module)
        {
            if (!diffFiles.SelectedItems.Any())
            {
                return;
            }

            foreach (var item in diffFiles.SelectedItems)
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

            // Since #8849 and #8557 we have a geometry bug, which pushes the splitter up by 4px.
            // Account for this shift. This is a workaround at best in the same way as for FormCommit.
            if (!RevisionsSplitContainer.Panel2Collapsed)
            {
                RevisionsSplitContainer.SplitterDistance -= 4;
            }
        }

        private void CommandsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            // Most options do not make sense for artificial commits or no revision selected at all
            var selectedRevisions = RevisionGrid.GetSelectedRevisions();
            bool singleNormalCommit = selectedRevisions.Count == 1 && !selectedRevisions[0].IsArtificial;

            // Some commands like stash, undo commit etc has no relation to selections

            // Require that a single commit is selected
            // Some commands like delete branch could be available for artificial as no default is used,
            // but hide for consistency
            branchToolStripMenuItem.Enabled =
            deleteBranchToolStripMenuItem.Enabled =
            mergeBranchToolStripMenuItem.Enabled =
            rebaseToolStripMenuItem.Enabled =
            checkoutBranchToolStripMenuItem.Enabled =
            cherryPickToolStripMenuItem.Enabled =
            checkoutToolStripMenuItem.Enabled =
            bisectToolStripMenuItem.Enabled =
                singleNormalCommit && !Module.IsBareRepository();

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
            bool isSilent = AppSettings.DefaultPullAction != AppSettings.PullAction.None;
            var pullAction = AppSettings.DefaultPullAction != AppSettings.PullAction.None ?
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
            DoPull(pullAction: AppSettings.PullAction.Merge, isSilent: true);
        }

        private void rebaseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DoPull(pullAction: AppSettings.PullAction.Rebase, isSilent: true);
        }

        private void fetchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoPull(pullAction: AppSettings.PullAction.Fetch, isSilent: true);
        }

        private void fetchAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoPull(pullAction: AppSettings.PullAction.FetchAll, isSilent: true);
        }

        private void fetchPruneAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoPull(pullAction: AppSettings.PullAction.FetchPruneAll, isSilent: true);
        }

        private void DoPull(AppSettings.PullAction pullAction, bool isSilent)
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

        private void RevisionInfo_CommandClicked(object sender, ResourceManager.CommandEventArgs e)
        {
            // TODO this code duplicated in FormFileHistory.Blame_CommandClick
            switch (e.Command)
            {
                case "gotocommit":
                    Validates.NotNull(e.Data);
                    var found = Module.TryResolvePartialCommitId(e.Data, out var revision);

                    if (found)
                    {
                        found = RevisionGrid.SetSelectedRevision(revision);
                    }

                    // When 'git log --first-parent' filtration is used, user can click on child commit
                    // that is not present in the shown git log. User still wants to see the child commit
                    // and to make it possible we add explicit branch filter and refresh.
                    if (AppSettings.ShowFirstParent && !found)
                    {
                        ToolStripFilters.SetBranchFilter(revision?.ToString());
                        RevisionGrid.SetSelectedRevision(revision);
                    }

                    break;
                case "gotobranch":
                case "gototag":
                    Validates.NotNull(e.Data);
                    CommitData? commit = _commitDataManager.GetCommitData(e.Data, out _);
                    if (commit is not null)
                    {
                        RevisionGrid.SetSelectedRevision(commit.ObjectId);
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
            if (sender is ToolStripMenuItem menuSender)
            {
                SetWorkingDir(menuSender.Tag as string);
            }
        }

        private void toolStripButtonLevelUp_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripFilters.PreventToolStripSplitButtonClosing(sender as ToolStripSplitButton);
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

        private void UpdateSubmoduleMenuItemStatus(ToolStripItem item, SubmoduleInfo info, string textFormat = "{0}")
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
            var updateStatus = AppSettings.ShowSubmoduleStatus && _gitStatusMonitor.Active;

            toolStripButtonLevelUp.ToolTipText = "";

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    await TaskScheduler.Default;
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
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await this.SwitchToMainThreadAsync();
                RemoveSubmoduleButtons();
                toolStripButtonLevelUp.DropDownItems.Add(_loading.Text);
            }).FileAndForget();
        }

        private void SubmoduleStatusProvider_StatusUpdated(object sender, SubmoduleStatusEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                if (e.StructureUpdated || _currentSubmoduleMenuItems is null)
                {
                    _currentSubmoduleMenuItems = await PopulateToolbarAsync(e.Info, e.Token);
                }

                await UpdateSubmoduleMenuStatusAsync(e.Info, e.Token);
            }).FileAndForget();
        }

        private async Task<List<ToolStripItem>> PopulateToolbarAsync(SubmoduleInfoResult result, CancellationToken cancelToken)
        {
            // Second task: Populate submodule toolbar menu on UI thread.
            await this.SwitchToMainThreadAsync(cancelToken);

            RemoveSubmoduleButtons();

            var newItems = result.OurSubmodules
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

            return newItems;
        }

        private async Task UpdateSubmoduleMenuStatusAsync(SubmoduleInfoResult result, CancellationToken cancelToken)
        {
            if (_currentSubmoduleMenuItems is null)
            {
                return;
            }

            await this.SwitchToMainThreadAsync(cancelToken);

            Validates.NotNull(result.TopProject);
            var infos = result.AllSubmodules.ToDictionary(info => info.Path, info => info);
            infos[result.TopProject.Path] = result.TopProject;
            foreach (var item in _currentSubmoduleMenuItems)
            {
                var path = item.Tag as string;
                if (string.IsNullOrWhiteSpace(path))
                {
                    // not a submodule
                    continue;
                }

                if (infos.ContainsKey(path))
                {
                    UpdateSubmoduleMenuItemStatus(item, infos[path]);
                }
                else
                {
                    Debug.Fail($"Status info for {path} ({1 + result.AllSubmodules.Count} records) has no match in current nodes ({_currentSubmoduleMenuItems.Count})");
                }
            }
        }

        private void RemoveSubmoduleButtons()
        {
            foreach (var item in toolStripButtonLevelUp.DropDownItems)
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

        private void reportAnIssueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UserEnvironmentInformation.CopyInformation();
            OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/issues");
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormUpdates updateForm = new(AppSettings.AppVersion);
            updateForm.SearchForUpdatesAndShow(Owner, true);
        }

        private void toolStripButtonPull_DropDownOpened(object sender, EventArgs e)
        {
            ToolStripFilters.PreventToolStripSplitButtonClosing(sender as ToolStripSplitButton);
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

        private void toggleBranchTreePanel_Click(object sender, EventArgs e)
        {
            MainSplitContainer.Panel1Collapsed = !MainSplitContainer.Panel1Collapsed;
            DiagnosticsClient.TrackEvent("Layout change",
                new Dictionary<string, string> { { "ShowLeftPanel", MainSplitContainer.Panel1Collapsed.ToString() } });

            RefreshLayoutToggleButtonStates();
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
            toggleBranchTreePanel.Checked = !MainSplitContainer.Panel1Collapsed;
            toggleSplitViewLayout.Checked = AppSettings.ShowSplitViewLayout;

            int commitInfoPositionNumber = (int)AppSettings.CommitInfoPosition;
            var selectedMenuItem = menuCommitInfoPosition.DropDownItems[commitInfoPositionNumber];
            menuCommitInfoPosition.Image = selectedMenuItem.Image;
            menuCommitInfoPosition.ToolTipText = selectedMenuItem.Text;
        }

        private void LayoutRevisionInfo()
        {
            // Handle must be created prior to insertion
            _ = CommitInfoTabControl.Handle;

            RevisionInfo.SuspendLayout();
            CommitInfoTabControl.SuspendLayout();
            RevisionsSplitContainer.SuspendLayout();

            var commitInfoPosition = AppSettings.CommitInfoPosition;
            if (commitInfoPosition == CommitInfoPosition.BelowList)
            {
                CommitInfoTabControl.InsertIfNotExists(0, CommitInfoTabPage);
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
                CommitInfoTabControl.RemoveIfExists(CommitInfoTabPage);

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
        }

        private void createWorktreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using FormCreateWorktree formCreateWorktree = new(UICommands);
            DialogResult dialogResult = formCreateWorktree.ShowDialog(this);
            if (dialogResult != DialogResult.OK || !formCreateWorktree.OpenWorktree)
            {
                return;
            }

            GitModule newModule = new(formCreateWorktree.WorktreeDirectory);
            if (newModule.IsValidGitWorkingDir())
            {
                SetGitModule(this, new GitModuleEventArgs(newModule));
            }
        }

        private void toolStripSplitStash_DropDownOpened(object sender, EventArgs e)
        {
            ToolStripFilters.PreventToolStripSplitButtonClosing(sender as ToolStripSplitButton);
        }

        private void undoLastCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AppSettings.DontConfirmUndoLastCommit || MessageBox.Show(this, _undoLastCommitText.Text, _undoLastCommitCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var args = GitCommandHelpers.ResetCmd(ResetMode.Soft, "HEAD~1");
                Module.GitExecutable.GetOutput(args);
                refreshToolStripMenuItem.PerformClick();
                RequestRefresh();
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
            public TabPage TreeTabPage => _form.TreeTabPage;
            public FilterToolBar ToolStripFilters => _form.ToolStripFilters;

            public void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container, IList<Repository> repositoryHistory)
            {
                _form.PopulateFavouriteRepositoriesMenu(container, repositoryHistory);
            }
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

            var itemPath = (e.Data.GetData(DataFormats.Text) ?? e.Data.GetData(DataFormats.UnicodeText)) as string;
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

        private void FormBrowse_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)
                || e.Data.GetDataPresent(DataFormats.Text)
                || e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void TsmiTelemetryEnabled_Click(object sender, EventArgs e)
        {
            UICommands.StartGeneralSettingsDialog(this);
        }

        private void HelpToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            tsmiTelemetryEnabled.Checked = AppSettings.TelemetryEnabled ?? false;
        }
    }
}
