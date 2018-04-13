using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConEmu.WinForms;
using GitCommands;
using GitCommands.Git;
using GitCommands.Gpg;
using GitCommands.UserRepositoryHistory;
using GitCommands.Utils;
using GitExtUtils.GitUI;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.CommandsDialogs.WorktreeDialog;
using GitUI.Hotkey;
using GitUI.Properties;
using GitUI.Script;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGridClasses;
using GitUI.UserControls.ToolStripClasses;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Taskbar;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormBrowse : GitModuleForm, IBrowseRepo
    {
        #region Translation

        private readonly TranslationString _stashCount = new TranslationString("{0} saved {1}");
        private readonly TranslationString _stashPlural = new TranslationString("stashes");
        private readonly TranslationString _stashSingular = new TranslationString("stash");

        private readonly TranslationString _warningMiddleOfBisect = new TranslationString("You are in the middle of a bisect");
        private readonly TranslationString _warningMiddleOfRebase = new TranslationString("You are in the middle of a rebase");
        private readonly TranslationString _warningMiddleOfPatchApply = new TranslationString("You are in the middle of a patch apply");

        private readonly TranslationString _hintUnresolvedMergeConflicts = new TranslationString("There are unresolved merge conflicts!");

        private readonly TranslationString _noBranchTitle = new TranslationString("no branch");
        private readonly TranslationString _noSubmodulesPresent = new TranslationString("No submodules");
        private readonly TranslationString _topProjectModuleFormat = new TranslationString("Top project: {0}");
        private readonly TranslationString _superprojectModuleFormat = new TranslationString("Superproject: {0}");

        private readonly TranslationString _indexLockCantDelete = new TranslationString("Failed to delete index.lock.");

        private readonly TranslationString _errorCaption = new TranslationString("Error");
        private readonly TranslationString _loading = new TranslationString("Loading...");

        private readonly TranslationString _noReposHostPluginLoaded = new TranslationString("No repository host plugin loaded.");
        private readonly TranslationString _noReposHostFound = new TranslationString("Could not find any relevant repository hosts for the currently open repository.");

        private readonly TranslationString _configureWorkingDirMenu = new TranslationString("Configure this menu");

        private readonly TranslationString _directoryIsNotAValidRepositoryCaption = new TranslationString("Open");
        private readonly TranslationString _directoryIsNotAValidRepository = new TranslationString("The selected item is not a valid git repository.\n\nDo you want to abort and remove it from the recent repositories list?");

        private readonly TranslationString _updateCurrentSubmodule = new TranslationString("Update current submodule");

        private readonly TranslationString _pullFetch = new TranslationString("Pull - fetch");
        private readonly TranslationString _pullFetchAll = new TranslationString("Pull - fetch all");
        private readonly TranslationString _pullMerge = new TranslationString("Pull - merge");
        private readonly TranslationString _pullRebase = new TranslationString("Pull - rebase");
        private readonly TranslationString _pullOpenDialog = new TranslationString("Open pull dialog");

        private readonly TranslationString _buildReportTabCaption = new TranslationString("Build Report");
        private readonly TranslationString _consoleTabCaption = new TranslationString("Console");

        private readonly TranslationString _noWorkingFolderText = new TranslationString("No working directory");
        private readonly TranslationString _commitButtonText = new TranslationString("Commit");

        private readonly TranslationString _undoLastCommitText = new TranslationString("You will still be able to find all the commit's changes in the staging area\n\nDo you want to continue?");
        private readonly TranslationString _undoLastCommitCaption = new TranslationString("Undo last commit");
        #endregion

        private Dashboard _dashboard;
        private ToolStripItem _rebase;
        private ToolStripItem _bisect;
        private ToolStripItem _warning;

        private ThumbnailToolBarButton _commitButton;
        private ThumbnailToolBarButton _pushButton;
        private ThumbnailToolBarButton _pullButton;
        private bool _toolbarButtonsCreated;
        private readonly ToolStripMenuItem _toolStripGitStatus;
        private readonly GitStatusMonitor _gitStatusMonitor;
        private readonly FilterRevisionsHelper _filterRevisionsHelper;
        private readonly FilterBranchHelper _filterBranchHelper;

        private readonly CancellationTokenSequence _submodulesStatusSequence = new CancellationTokenSequence();
        private BuildReportTabPageExtension _buildReportTabPageExtension;

        private readonly FormBrowseMenus _formBrowseMenus;
        private ConEmuControl _terminal;
        private readonly SplitterManager _splitterManager = new SplitterManager(new AppSettingsPath("FormBrowse"));
        private readonly IFormBrowseController _controller;
        private readonly ICommitDataManager _commitDataManager;
        private readonly IRepositoryDescriptionProvider _repositoryDescriptionProvider;
        private readonly IAppTitleGenerator _appTitleGenerator;
        private readonly ILongShaProvider _longShaProvider;
        private static bool _showRevisionInfoNextToRevisionGrid;
        private bool _startWithDashboard = false;

        /// <summary>
        /// For VS designer
        /// </summary>
        private FormBrowse()
        {
            InitializeComponent();
            Translate();
        }

        public FormBrowse(GitUICommands commands, string filter)
            : base(true, commands)
        {
            // Save value for commit info panel, may be changed
            _showRevisionInfoNextToRevisionGrid = AppSettings.ShowRevisionInfoNextToRevisionGrid;
            InitializeComponent();

            MainSplitContainer.Visible = false;

            // set tab page images
            CommitInfoTabControl.ImageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth8Bit,
                ImageSize = DpiUtil.Scale(new Size(16, 16)),
                Images =
                {
                    Resources.IconCommit,
                    Resources.IconFileTree,
                    Resources.IconDiff,
                    Resources.IconKey
                }
            };
            CommitInfoTabControl.TabPages[0].ImageIndex = 0;
            CommitInfoTabControl.TabPages[1].ImageIndex = 1;
            CommitInfoTabControl.TabPages[2].ImageIndex = 2;
            CommitInfoTabControl.TabPages[3].ImageIndex = 3;

            if (!AppSettings.ShowGpgInformation.ValueOrDefault)
            {
                CommitInfoTabControl.RemoveIfExists(GpgInfoTabPage);
            }

            if (commands != null)
            {
                RevisionGrid.UICommandsSource = this;
                repoObjectsTree.UICommandsSource = this;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;

                try
                {
                    PluginRegistry.Initialize();
                }
                finally
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    RegisterPlugins();
                }
            }).FileAndForget();

            RevisionGrid.GitModuleChanged += SetGitModule;
            RevisionGrid.OnToggleBranchTreePanelRequested = () => toggleBranchTreePanel_Click(null, null);
            _filterRevisionsHelper = new FilterRevisionsHelper(toolStripRevisionFilterTextBox, toolStripRevisionFilterDropDownButton, RevisionGrid, toolStripRevisionFilterLabel, ShowFirstParent, form: this);
            _filterBranchHelper = new FilterBranchHelper(toolStripBranchFilterComboBox, toolStripBranchFilterDropDownButton, RevisionGrid);
            repoObjectsTree.FilterBranchHelper = _filterBranchHelper;
            toolStripBranchFilterComboBox.DropDown += toolStripBranches_DropDown_ResizeDropDownWidth;
            revisionDiff.Bind(RevisionGrid, fileTree);

            Translate();

            // Activation of the control requires restart of Browse, limit also deactivation for consistency
            bool countToolbar = AppSettings.ShowGitStatusInBrowseToolbar;
            bool countArtificial = AppSettings.ShowGitStatusForArtificialCommits && AppSettings.RevisionGraphShowWorkingDirChanges;

            if (countToolbar || countArtificial)
            {
                _toolStripGitStatus = new ToolStripMenuItem
                {
                    ImageTransparentColor = Color.Magenta,
                    ImageScaling = ToolStripItemImageScaling.SizeToFit,
                    Margin = new Padding(0, 1, 0, 2)
                };
                ICommitIconProvider commitIconProvider = new CommitIconProvider();

                _gitStatusMonitor = new GitStatusMonitor();
                _gitStatusMonitor.Init(this);

                _gitStatusMonitor.GitStatusMonitorStateChanged += (s, e) =>
                {
                    var status = e.State;
                    if (status == GitStatusMonitorState.Stopped)
                    {
                        _toolStripGitStatus.Visible = false;
                        _toolStripGitStatus.Text = string.Empty;
                    }
                    else if (status == GitStatusMonitorState.Running)
                    {
                        _toolStripGitStatus.Visible = true;
                    }
                };

                _gitStatusMonitor.GitWorkingDirectoryStatusChanged += (s, e) =>
                {
                    var status = e.ItemStatuses.ToList();

                    _toolStripGitStatus.Image = commitIconProvider.GetCommitIcon(status);

                    _toolStripGitStatus.Text = countToolbar && status.Count != 0
                        ? string.Format(_commitButtonText + " ({0})", status.Count.ToString())
                        : _commitButtonText.Text;

                    if (countArtificial)
                    {
                        RevisionGrid.UpdateArtificialCommitCount(status);
                    }

                    // The diff filelist is not updated, as the selected diff is unset
                    ////_revisionDiff.RefreshArtificial();
                };

                _toolStripGitStatus.Click += StatusClick;
                ToolStrip.Items.Insert(ToolStrip.Items.IndexOf(toolStripButton1), _toolStripGitStatus);
                ToolStrip.Items.Remove(toolStripButton1);
            }

            if (!EnvUtils.RunningOnWindows())
            {
                toolStripSeparator6.Visible = false;
                PuTTYToolStripMenuItem.Visible = false;
            }

            RevisionGrid.SelectionChanged += RevisionGridSelectionChanged;
            _filterRevisionsHelper.SetFilter(filter);

            HotkeysEnabled = true;
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            GitUICommandsChanged += (a, e) =>
            {
                var oldcommands = e.OldCommands;
                RefreshPullIcon();
                oldcommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
                UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
                oldcommands.BrowseRepo = null;
                UICommands.BrowseRepo = this;
            };
            if (commands != null)
            {
                RefreshPullIcon();
                UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
                UICommands.BrowseRepo = this;
                _controller = new FormBrowseController(new GitGpgController(() => Module));
                _commitDataManager = new CommitDataManager(() => Module);
                _longShaProvider = new LongShaProvider(() => Module);
            }

            _repositoryDescriptionProvider = new RepositoryDescriptionProvider(new GitDirectoryResolver());
            _appTitleGenerator = new AppTitleGenerator(_repositoryDescriptionProvider);

            FillBuildReport();  // Ensure correct page visibility
            RevisionGrid.ShowBuildServerInfo = true;

            _formBrowseMenus = new FormBrowseMenus(menuStrip1);
            RevisionGrid.MenuCommands.MenuChanged += (sender, e) => _formBrowseMenus.OnMenuCommandsPropertyChanged();
            SystemEvents.SessionEnding += (sender, args) => SaveApplicationSettings();

            FillTerminalTab();
            ManageWorktreeSupport();

            this.AdjustForDpiScaling();
        }

        public FormBrowse(GitUICommands commands, string filter, string selectCommit, bool startWithDashboard = false)
            : this(commands, filter)
        {
            if (!string.IsNullOrEmpty(selectCommit))
            {
                RevisionGrid.SetInitialRevision(_longShaProvider.Get(selectCommit));
            }

            _startWithDashboard = startWithDashboard;
        }

        private void LayoutRevisionInfo()
        {
            // Handle must be created prior to insertion
            IntPtr h = CommitInfoTabControl.Handle;

            if (_showRevisionInfoNextToRevisionGrid)
            {
                RevisionInfo.Parent = RevisionsSplitContainer.Panel2;
                RevisionsSplitContainer.SplitterDistance = RevisionsSplitContainer.Width - 420;
                RevisionInfo.DisplayAvatarOnRight();
                CommitInfoTabControl.SuspendLayout();
                CommitInfoTabControl.RemoveIfExists(CommitInfoTabPage);

                // Move difftab to left
                CommitInfoTabControl.RemoveIfExists(DiffTabPage);
                CommitInfoTabControl.TabPages.Insert(0, DiffTabPage);
                CommitInfoTabControl.SelectedTab = DiffTabPage;
                CommitInfoTabControl.ResumeLayout(true);
                RevisionsSplitContainer.Panel2Collapsed = false;
            }
            else
            {
                RevisionInfo.DisplayAvatarOnLeft();
                CommitInfoTabControl.SuspendLayout();
                CommitInfoTabControl.InsertIfNotExists(0, CommitInfoTabPage);
                CommitInfoTabControl.ResumeLayout(true);
                RevisionInfo.Parent = CommitInfoTabControl.Controls[0];
                RevisionsSplitContainer.Panel2Collapsed = true;
            }
        }

        private void ManageWorktreeSupport()
        {
            if (!GitCommandHelpers.VersionInUse.SupportWorktree)
            {
                createWorktreeToolStripMenuItem.Enabled = false;
            }

            if (!GitCommandHelpers.VersionInUse.SupportWorktreeList)
            {
                manageWorktreeToolStripMenuItem.Enabled = false;
            }
        }

        private void UICommands_PostRepositoryChanged(object sender, GitUIEventArgs e)
        {
            this.InvokeAsync(RefreshRevisions).FileAndForget();
        }

        private void RefreshRevisions()
        {
            if (RevisionGrid.IsDisposed || IsDisposed || Disposing)
            {
                return;
            }

            if (_dashboard == null || !_dashboard.Visible)
            {
                revisionDiff.ForceRefreshRevisions();
                RevisionGrid.ForceRefreshRevisions();
                InternalInitialize(true);
            }
        }

        #region IBrowseRepo
        public void GoToRef(string refName, bool showNoRevisionMsg)
        {
            RevisionGrid.GoToRef(refName, showNoRevisionMsg);
        }

        #endregion

        private void ShowDashboard()
        {
            toolPanel.TopToolStripPanelVisible = false;
            toolPanel.BottomToolStripPanelVisible = false;
            toolPanel.LeftToolStripPanelVisible = false;
            toolPanel.RightToolStripPanelVisible = false;

            MainSplitContainer.Visible = false;

            if (_dashboard == null)
            {
                _dashboard = new Dashboard();
                _dashboard.GitModuleChanged += SetGitModule;
                toolPanel.ContentPanel.Controls.Add(_dashboard);
                _dashboard.Dock = DockStyle.Fill;
            }

            Text = _appTitleGenerator.Generate(string.Empty, false, string.Empty);

            _dashboard.RefreshContent();
            _dashboard.Visible = true;
            _dashboard.BringToFront();
        }

        private void HideDashboard()
        {
            MainSplitContainer.Visible = true;
            if (_dashboard == null || !_dashboard.Visible)
            {
                return;
            }

            _dashboard.Visible = false;
            toolPanel.TopToolStripPanelVisible = true;
            toolPanel.BottomToolStripPanelVisible = true;
            toolPanel.LeftToolStripPanelVisible = true;
            toolPanel.RightToolStripPanelVisible = true;
        }

        private void BrowseLoad(object sender, EventArgs e)
        {
            if (EnvUtils.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
            {
                TaskbarManager.Instance.ApplicationId = "GitExtensions";
            }

            SetSplitterPositions();
            HideVariableMainMenuItems();

            RevisionGrid.Load();
            _filterBranchHelper.InitToolStripBranchFilter();

            using (WaitCursorScope.Enter())
            {
                LayoutRevisionInfo();
                InternalInitialize(false);
                RevisionGrid.Focus();
                RevisionGrid.IndexWatcher.Reset();

                RevisionGrid.IndexWatcher.Changed += _indexWatcher_Changed;
            }

            try
            {
                if (AppSettings.PlaySpecialStartupSound)
                {
                    using (var cowMoo = Resources.cow_moo)
                    {
                        new System.Media.SoundPlayer(cowMoo).Play();
                    }
                }
            }
            catch
            {
                // This code is just for fun, we do not want the program to crash because of it.
            }
        }

        private void _indexWatcher_Changed(object sender, IndexChangedEventArgs e)
        {
            bool indexChanged = e.IsIndexChanged;
            this.InvokeAsync(() =>
            {
                RefreshButton.Image = indexChanged && AppSettings.UseFastChecks && Module.IsValidGitWorkingDir()
                                          ? Resources.arrow_refresh_dirty
                                          : Resources.arrow_refresh;
            })
                .FileAndForget();
        }

        /// <summary>
        ///   Execute plugin
        /// </summary>
        private void ItemClick(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;

            if (menuItem?.Tag is IGitPlugin plugin)
            {
                var eventArgs = new GitUIEventArgs(this, UICommands);

                bool refresh = plugin.Execute(eventArgs);
                if (refresh)
                {
                    RefreshRevisions();
                }
            }
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
            foreach (var plugin in PluginRegistry.Plugins)
            {
                // Add the plugin to the Plugins menu
                var item = new ToolStripMenuItem { Text = plugin.Description, Tag = plugin };
                item.Click += ItemClick;
                pluginsToolStripMenuItem.DropDownItems.Insert(pluginsToolStripMenuItem.DropDownItems.Count - 2, item);

                // Allow the plugin to perform any self-registration actions
                plugin.Register(UICommands);
            }

            UICommands.RaisePostRegisterPlugin(this);

            // Show "Repository hosts" menu item when there is at least 1 repository host plugin loaded
            _repositoryHostsToolStripMenuItem.Visible = PluginRegistry.GitHosters.Count > 0;
            if (PluginRegistry.GitHosters.Count == 1)
            {
                _repositoryHostsToolStripMenuItem.Text = PluginRegistry.GitHosters[0].Description;
            }

            UpdatePluginMenu(Module.IsValidGitWorkingDir());
        }

        private void UnregisterPlugins()
        {
            foreach (var plugin in PluginRegistry.Plugins)
            {
                plugin.Unregister(UICommands);
            }
        }

        /// <summary>
        /// to avoid showing menu items that should not be there during
        /// the transition from dashboard to repo browser and vice versa
        ///
        /// and reset hotkeys that are shared between mutual exclusive menu items
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
            _formBrowseMenus.RemoveAdditionalMainMenuItems();
            menuStrip1.Refresh();
        }

        private void InternalInitialize(bool hard)
        {
            toolPanel.SuspendLayout();
            using (WaitCursorScope.Enter())
            {
                // check for updates
                if (AppSettings.LastUpdateCheck.AddDays(7) < DateTime.Now)
                {
                    AppSettings.LastUpdateCheck = DateTime.Now;
                    var updateForm = new FormUpdates(Module.AppVersion);
                    updateForm.SearchForUpdatesAndShow(Owner, false);
                }

                bool hasWorkingDir = !string.IsNullOrEmpty(Module.WorkingDir);
                if (hasWorkingDir && !_startWithDashboard)
                {
                    HideDashboard();
                }
                else
                {
                    // Consume the startup arguments, no specific directory was requested
                    _startWithDashboard = false;
                    ShowDashboard();
                }

                bool bareRepository = Module.IsBareRepository();
                bool isDashboard = _dashboard != null && _dashboard.Visible;
                bool validBrowseDir = !isDashboard && Module.IsValidGitWorkingDir();

                branchSelect.Text = validBrowseDir ? Module.GetSelectedBranch() : "";
                toolStripButtonLevelUp.Enabled = hasWorkingDir && !bareRepository;
                CommitInfoTabControl.Visible = validBrowseDir;
                fileExplorerToolStripMenuItem.Enabled = validBrowseDir;
                manageRemoteRepositoriesToolStripMenuItem1.Enabled = validBrowseDir;
                branchSelect.Enabled = validBrowseDir;
                toolStripButton1.Enabled = validBrowseDir && !bareRepository;
                if (_toolStripGitStatus != null)
                {
                    _toolStripGitStatus.Enabled = validBrowseDir && !Module.IsBareRepository();
                }

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
                editgitattributesToolStripMenuItem.Enabled = validBrowseDir;
                editmailmapToolStripMenuItem.Enabled = validBrowseDir;
                toolStripSplitStash.Enabled = validBrowseDir && !bareRepository;
                commitcountPerUserToolStripMenuItem.Enabled = validBrowseDir;
                _createPullRequestsToolStripMenuItem.Enabled = validBrowseDir;
                _viewPullRequestsToolStripMenuItem.Enabled = validBrowseDir;

                _filterBranchHelper.InitToolStripBranchFilter();

                if (repositoryToolStripMenuItem.Visible)
                {
                    manageSubmodulesToolStripMenuItem.Enabled = !bareRepository;
                    updateAllSubmodulesToolStripMenuItem.Enabled = !bareRepository;
                    synchronizeAllSubmodulesToolStripMenuItem.Enabled = !bareRepository;
                    editgitignoreToolStripMenuItem1.Enabled = !bareRepository;
                    editgitattributesToolStripMenuItem.Enabled = !bareRepository;
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

                RefreshWorkingDirCombo();
                var branchName = !string.IsNullOrEmpty(branchSelect.Text) ? branchSelect.Text : _noBranchTitle.Text;
                Text = _appTitleGenerator.Generate(Module.WorkingDir, validBrowseDir, branchName);
                UpdateJumplist(validBrowseDir);

                OnActivate();

                // load custom user menu
                LoadUserMenu();
                ReloadRepoObjectsTree();

                if (validBrowseDir)
                {
                    // add Navigate and View menu
                    _formBrowseMenus.ResetMenuCommandSets();
                    _formBrowseMenus.AddMenuCommandSet(MainMenuItem.NavigateMenu, RevisionGrid.MenuCommands.GetNavigateMenuCommands());
                    _formBrowseMenus.AddMenuCommandSet(MainMenuItem.ViewMenu, RevisionGrid.MenuCommands.GetViewMenuCommands());

                    _formBrowseMenus.InsertAdditionalMainMenuItems(repositoryToolStripMenuItem);
                }

                UICommands.RaisePostBrowseInitialize(this);
            }

            toolPanel.ResumeLayout();
        }

        private void ReloadRepoObjectsTree()
        {
            if (IsRepoObjectsTreeVisible())
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(() => repoObjectsTree.ReloadAsync()).FileAndForget();
            }

            return;

            bool IsRepoObjectsTreeVisible()
            {
                return !MainSplitContainer.Panel1Collapsed;
            }
        }

        private void OnActivate()
        {
            CheckForMergeConflicts();
            UpdateStashCount();
            UpdateSubmodulesList();
        }

        internal Keys GetShortcutKeys(Commands cmd)
        {
            return GetShortcutKeys((int)cmd);
        }

        /// <summary>
        /// Add shortcuts to the menu items
        /// </summary>
        private void SetShortcutKeyDisplayStringsFromHotkeySettings()
        {
            gitBashToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Commands.GitBash).ToShortcutKeyDisplayString();
            commitToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Commands.Commit).ToShortcutKeyDisplayString();
            stashChangesToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Commands.Stash).ToShortcutKeyDisplayString();
            stashPopToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Commands.StashPop).ToShortcutKeyDisplayString();
            closeToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Commands.CloseRepository).ToShortcutKeyDisplayString();
            gitGUIToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Commands.GitGui).ToShortcutKeyDisplayString();
            kGitToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Commands.GitGitK).ToShortcutKeyDisplayString();
            checkoutBranchToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Commands.CheckoutBranch).ToShortcutKeyDisplayString();

            // TODO: add more
        }

        private void RefreshWorkingDirCombo()
        {
            var path = Module.WorkingDir;

            // it appears at times Module.WorkingDir path is an empty string, this caused issues like #4874
            if (string.IsNullOrWhiteSpace(path))
            {
                _NO_TRANSLATE_Workingdir.Text = _noWorkingFolderText.Text;
                return;
            }

            var recentRepositoryHistory = ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(path));
            List<RecentRepoInfo> mostRecentRepos = new List<RecentRepoInfo>();
            using (var graphics = CreateGraphics())
            {
                var splitter = new RecentRepoSplitter
                {
                    MeasureFont = _NO_TRANSLATE_Workingdir.Font,
                    Graphics = graphics
                };
                splitter.SplitRecentRepos(recentRepositoryHistory, mostRecentRepos, mostRecentRepos);

                RecentRepoInfo ri = mostRecentRepos.Find((e) => e.Repo.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase));

                if (ri == null)
                {
                    _NO_TRANSLATE_Workingdir.Text = path;
                }
                else
                {
                    _NO_TRANSLATE_Workingdir.Text = ri.Caption;
                }

                if (AppSettings.RecentReposComboMinWidth > 0)
                {
                    _NO_TRANSLATE_Workingdir.AutoSize = false;
                    var captionWidth = graphics.MeasureString(_NO_TRANSLATE_Workingdir.Text, _NO_TRANSLATE_Workingdir.Font).Width;
                    captionWidth = captionWidth + _NO_TRANSLATE_Workingdir.DropDownButtonWidth + 5;
                    _NO_TRANSLATE_Workingdir.Width = Math.Max(AppSettings.RecentReposComboMinWidth, (int)captionWidth);
                }
                else
                {
                    _NO_TRANSLATE_Workingdir.AutoSize = true;
                }
            }
        }

        private void LoadUserMenu()
        {
            var scripts = ScriptManager.GetScripts().Where(script => script.Enabled
                && script.OnEvent == ScriptEvent.ShowInUserMenuBar).ToList();

            for (int i = ToolStrip.Items.Count - 1; i >= 0; i--)
            {
                if (ToolStrip.Items[i].Tag != null &&
                    ToolStrip.Items[i].Tag as string == "userscript")
                {
                    ToolStrip.Items.RemoveAt(i);
                }
            }

            if (scripts.Count == 0)
            {
                return;
            }

            ToolStrip.Items.Add(new ToolStripSeparator { Tag = "userscript" });

            foreach (ScriptInfo scriptInfo in scripts)
            {
                var tempButton = new ToolStripButton
                {
                    // store scriptname
                    Text = scriptInfo.Name,
                    Tag = "userscript",
                    Enabled = true,
                    Visible = true,
                    Image = scriptInfo.GetIcon(),
                    DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
                    ////Image = GitUI.Properties.Resources.bug,
                    ////Icon = "Cow"
                };

                // add handler
                tempButton.Click += UserMenu_Click;

                // add to toolstrip
                ToolStrip.Items.Add(tempButton);
            }
        }

        private void UserMenu_Click(object sender, EventArgs e)
        {
            if (ScriptRunner.RunScript(this, Module, ((ToolStripButton)sender).Text, RevisionGrid))
            {
                RevisionGrid.RefreshRevisions();
            }
        }

        private void UpdateJumplist(bool validWorkingDir)
        {
            if (!EnvUtils.RunningOnWindows() || !TaskbarManager.IsPlatformSupported)
            {
                return;
            }

            try
            {
                if (validWorkingDir)
                {
                    string repositoryDescription = _repositoryDescriptionProvider.Get(Module.WorkingDir);
                    string baseFolder = Path.Combine(AppSettings.ApplicationDataPath.Value, "Recent");
                    if (!Directory.Exists(baseFolder))
                    {
                        Directory.CreateDirectory(baseFolder);
                    }

                    // Remove InvalidPathChars
                    StringBuilder sb = new StringBuilder(repositoryDescription);
                    foreach (char c in Path.GetInvalidFileNameChars())
                    {
                        sb.Replace(c, '_');
                    }

                    string path = Path.Combine(baseFolder, string.Format("{0}.{1}", sb, "gitext"));
                    File.WriteAllText(path, Module.WorkingDir);
                    JumpList.AddToRecent(path);

                    var jumpList = JumpList.CreateJumpListForIndividualWindow(TaskbarManager.Instance.ApplicationId, Handle);
                    jumpList.ClearAllUserTasks();

                    // to control which category Recent/Frequent is displayed
                    jumpList.KnownCategoryToDisplay = JumpListKnownCategoryType.Recent;

                    jumpList.Refresh();
                }

                CreateOrUpdateTaskBarButtons(validWorkingDir);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Trace.WriteLine(ex.Message, "UpdateJumplist");
            }
        }

        private void CreateOrUpdateTaskBarButtons(bool validRepo)
        {
            if (EnvUtils.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
            {
                if (!_toolbarButtonsCreated)
                {
                    _commitButton = new ThumbnailToolBarButton(MakeIcon(toolStripButton1.Image, 48, true), toolStripButton1.Text);
                    _commitButton.Click += ToolStripButton1Click;

                    _pushButton = new ThumbnailToolBarButton(MakeIcon(toolStripButtonPush.Image, 48, true), toolStripButtonPush.Text);
                    _pushButton.Click += PushToolStripMenuItemClick;

                    _pullButton = new ThumbnailToolBarButton(MakeIcon(toolStripButtonPull.Image, 48, true), toolStripButtonPull.Text);
                    _pullButton.Click += PullToolStripMenuItemClick;

                    _toolbarButtonsCreated = true;
                    ThumbnailToolBarButton[] buttons = { _commitButton, _pullButton, _pushButton };

                    // Call this method using reflection.  This is a workaround to *not* reference WPF libraries, becuase of how the WindowsAPICodePack was implimented.
                    TaskbarManager.Instance.ThumbnailToolBars.AddButtons(Handle, buttons);
                }

                _commitButton.Enabled = validRepo;
                _pushButton.Enabled = validRepo;
                _pullButton.Enabled = validRepo;
            }
        }

        /// <summary>
        /// Converts an image into an icon.  This was taken off of the interwebs.
        /// It's on a billion different sites and forum posts, so I would say its creative commons by now. -tekmaven
        /// </summary>
        /// <param name="img">The image that shall become an icon</param>
        /// <param name="size">The width and height of the icon. Standard
        /// sizes are 16x16, 32x32, 48x48, 64x64.</param>
        /// <param name="keepAspectRatio">Whether the image should be squashed into a
        /// square or whether whitespace should be put around it.</param>
        /// <returns>An icon!!</returns>
        private static Icon MakeIcon(Image img, int size, bool keepAspectRatio)
        {
            Bitmap square = new Bitmap(size, size); // create new bitmap
            Graphics g = Graphics.FromImage(square); // allow drawing to it

            int x, y, w, h; // dimensions for new image

            if (!keepAspectRatio || img.Height == img.Width)
            {
                // just fill the square
                x = y = 0; // set x and y to 0
                w = h = size; // set width and height to size
            }
            else
            {
                // work out the aspect ratio
                float r = img.Width / (float)img.Height;

                // set dimensions accordingly to fit inside size^2 square
                if (r > 1)
                {
                    // w is bigger, so divide h by r
                    w = size;
                    h = (int)(size / r);
                    x = 0;
                    y = (size - h) / 2; // center the image
                }
                else
                {
                    // h is bigger, so multiply w by r
                    w = (int)(size * r);
                    h = size;
                    y = 0;
                    x = (size - w) / 2; // center the image
                }
            }

            // make the image shrink nicely by using HighQualityBicubic mode
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(img, x, y, w, h); // draw image with specified dimensions
            g.Flush(); // make sure all drawing operations complete before we get the icon

            // following line would work directly on any image, but then
            // it wouldn't look as nice.
            return Icon.FromHandle(square.GetHicon());
        }

        private void UpdateStashCount()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (AppSettings.ShowStashCount)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    await TaskScheduler.Default;

                    var result = Module.GetStashes().Count;

                    await this.SwitchToMainThreadAsync();

                    toolStripSplitStash.Text = string.Format(_stashCount.Text, result,
                            result != 1 ? _stashPlural.Text : _stashSingular.Text);
                }).FileAndForget();
            }
            else
            {
                toolStripSplitStash.Text = string.Empty;
            }
        }

        private void CheckForMergeConflicts()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            bool validWorkingDir = Module.IsValidGitWorkingDir();

            if (validWorkingDir && Module.InTheMiddleOfBisect())
            {
                if (_bisect == null)
                {
                    _bisect = new WarningToolStripItem { Text = _warningMiddleOfBisect.Text };
                    _bisect.Click += BisectClick;
                    statusStrip.Items.Add(_bisect);
                }
            }
            else
            {
                if (_bisect != null)
                {
                    _bisect.Click -= BisectClick;
                    statusStrip.Items.Remove(_bisect);
                    _bisect = null;
                }
            }

            if (validWorkingDir &&
                (Module.InTheMiddleOfRebase() || Module.InTheMiddleOfPatch()))
            {
                if (_rebase == null)
                {
                    _rebase = new WarningToolStripItem
                    {
                        Text = Module.InTheMiddleOfRebase()
                            ? _warningMiddleOfRebase.Text
                            : _warningMiddleOfPatchApply.Text
                    };
                    _rebase.Click += RebaseClick;
                    statusStrip.Items.Add(_rebase);
                }
            }
            else
            {
                if (_rebase != null)
                {
                    _rebase.Click -= RebaseClick;
                    statusStrip.Items.Remove(_rebase);
                    _rebase = null;
                }
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;

                var result = validWorkingDir
                    && Module.InTheMiddleOfConflictedMerge()
                    && !Directory.Exists(Module.WorkingDirGitDir + "rebase-apply\\");

                await this.SwitchToMainThreadAsync();

                if (result)
                {
                    if (_warning == null)
                    {
                        _warning = new WarningToolStripItem { Text = _hintUnresolvedMergeConflicts.Text };
                        _warning.Click += WarningClick;
                        statusStrip.Items.Add(_warning);
                    }
                }
                else
                {
                    if (_warning != null)
                    {
                        _warning.Click -= WarningClick;
                        statusStrip.Items.Remove(_warning);
                        _warning = null;
                    }
                }

                // Only show status strip when there are status items on it.
                // There is always a close (x) button, do not count first item.
                if (statusStrip.Items.Count > 1)
                {
                    statusStrip.Show();
                }
                else
                {
                    statusStrip.Hide();
                }
            }).FileAndForget();
        }

        private void RebaseClick(object sender, EventArgs e)
        {
            if (Module.InTheMiddleOfRebase())
            {
                UICommands.StartTheContinueRebaseDialog(this);
            }
            else
            {
                UICommands.StartApplyPatchDialog(this);
            }
        }

        private void ShowRevisions()
        {
            if (RevisionGrid.IndexWatcher.IndexChanged)
            {
                FillFileTree();
                FillDiff();
                FillCommitInfo();
                ThreadHelper.JoinableTaskFactory.RunAsync(() => FillGpgInfoAsync());
                FillBuildReport();
            }

            RevisionGrid.IndexWatcher.Reset();
        }

        private void FillFileTree()
        {
            if (CommitInfoTabControl.SelectedTab != TreeTabPage || _selectedRevisionUpdatedTargets.HasFlag(UpdateTargets.FileTree))
            {
                return;
            }

            _selectedRevisionUpdatedTargets |= UpdateTargets.FileTree;
            fileTree.LoadRevision(RevisionGrid.GetSelectedRevisions().FirstOrDefault());
        }

        private void FillDiff()
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
            revisionDiff.DisplayDiffTab();
        }

        private void FillCommitInfo()
        {
            if (!_showRevisionInfoNextToRevisionGrid && CommitInfoTabControl.SelectedTab != CommitInfoTabPage)
            {
                return;
            }

            if (_selectedRevisionUpdatedTargets.HasFlag(UpdateTargets.CommitInfo))
            {
                return;
            }

            _selectedRevisionUpdatedTargets |= UpdateTargets.CommitInfo;

            if (RevisionGrid.GetSelectedRevisions().Count == 0)
            {
                return;
            }

            var revision = RevisionGrid.GetSelectedRevisions()[0];

            var children = RevisionGrid.GetRevisionChildren(revision.Guid);
            RevisionInfo.SetRevisionWithChildren(revision, children);
        }

        private async Task FillGpgInfoAsync()
        {
            if (!AppSettings.ShowGpgInformation.ValueOrDefault || CommitInfoTabControl.SelectedTab != GpgInfoTabPage)
            {
                return;
            }

            var revisions = RevisionGrid.GetSelectedRevisions();
            var revision = revisions.FirstOrDefault();
            if (revision == null)
            {
                return;
            }

            var info = await _controller.LoadGpgInfoAsync(revision);
            revisionGpgInfo1.DisplayGpgInfo(info);
        }

        private void FillBuildReport()
        {
            var selectedRevisions = RevisionGrid.GetSelectedRevisions();
            var revision = selectedRevisions.Count == 1 ? selectedRevisions[0] : null;

            if (_buildReportTabPageExtension == null)
            {
                _buildReportTabPageExtension = new BuildReportTabPageExtension(CommitInfoTabControl, _buildReportTabCaption.Text);
            }

            // Note: FillBuildReport will check if tab is visible and revision is OK
            _buildReportTabPageExtension.FillBuildReport(revision);
        }

        [Flags]
        internal enum UpdateTargets
        {
            None = 1,
            DiffList = 2,
            FileTree = 4,
            CommitInfo = 8
        }

        private UpdateTargets _selectedRevisionUpdatedTargets = UpdateTargets.None;
        private void RevisionGridSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                _selectedRevisionUpdatedTargets = UpdateTargets.None;

                FillFileTree();
                FillDiff();
                FillCommitInfo();
                ThreadHelper.JoinableTaskFactory.RunAsync(() => FillGpgInfoAsync());
                FillBuildReport();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void OpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitModule module = FormOpenDirectory.OpenModule(this, Module);
            if (module != null)
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
            bool isSilent = (ModifierKeys & Keys.Shift) != 0;
            UICommands.StartPushDialog(this, isSilent);
        }

        private void PullToolStripMenuItemClick(object sender, EventArgs e)
        {
            bool isSilent;
            if (sender == toolStripButtonPull || sender == pullToolStripMenuItem)
            {
                if (Module.LastPullAction == AppSettings.PullAction.None)
                {
                    isSilent = (ModifierKeys & Keys.Shift) != 0;
                }
                else if (Module.LastPullAction == AppSettings.PullAction.FetchAll)
                {
                    fetchAllToolStripMenuItem_Click(sender, e);
                    return;
                }
                else
                {
                    isSilent = sender == toolStripButtonPull;
                    Module.LastPullActionToFormPullAction();
                }
            }
            else
            {
                isSilent = sender != pullToolStripMenuItem1;

                Module.LastPullActionToFormPullAction();
            }

            if (isSilent)
            {
                UICommands.StartPullDialogAndPullImmediately(this);
            }
            else
            {
                UICommands.StartPullDialog(this);
            }
        }

        private void RefreshToolStripMenuItemClick(object sender, EventArgs e)
        {
            RefreshRevisions();
        }

        private void RefreshDashboardToolStripMenuItemClick(object sender, EventArgs e)
        {
            _dashboard.RefreshContent();
        }

        private void AboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            using (var frm = new AboutBox())
            {
                frm.ShowDialog(this);
            }
        }

        private void PatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartViewPatchDialog(this);
        }

        private void ApplyPatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartApplyPatchDialog(this);
        }

        private void GitBashToolStripMenuItemClick1(object sender, EventArgs e)
        {
            Module.RunBash();
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
            FormGitLog.ShowOrActivate(this);
        }

        private void CheckoutBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCheckoutBranch(this);
        }

        private void StashToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartStashDialog(this);
        }

        private void ResetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UICommands.StartResetChangesDialog(this);
            revisionDiff.RefreshArtificial();
        }

        private void RunMergetoolToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartResolveConflictsDialog(this);
        }

        private void WarningClick(object sender, EventArgs e)
        {
            UICommands.StartResolveConflictsDialog(this);
        }

        private void WorkingdirClick(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Workingdir.ShowDropDown();
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
            var revisions = RevisionGrid.GetSelectedRevisions(System.DirectoryServices.SortDirection.Descending);

            UICommands.StartCherryPickDialog(this, revisions);
        }

        private void MergeBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartMergeBranchDialog(this, null);
        }

        private void ToolStripButton1Click(object sender, EventArgs e)
        {
            CommitToolStripMenuItemClick(sender, e);
        }

        private void SettingsClick(object sender, EventArgs e)
        {
            var translation = AppSettings.Translation;
            UICommands.StartSettingsDialog(this);
            if (translation != AppSettings.Translation)
            {
                Translate();
            }

            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            RevisionGrid.ReloadHotkeys();
            RevisionGrid.ReloadTranslation();
            fileTree.ReloadHotkeys();
            revisionDiff.ReloadHotkeys();
            if (_showRevisionInfoNextToRevisionGrid != AppSettings.ShowRevisionInfoNextToRevisionGrid)
            {
                _showRevisionInfoNextToRevisionGrid = AppSettings.ShowRevisionInfoNextToRevisionGrid;
                LayoutRevisionInfo();
            }

            _dashboard?.RefreshContent();
        }

        private void TagToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCreateTagDialog(this);
        }

        private void RefreshButtonClick(object sender, EventArgs e)
        {
            RefreshRevisions();
        }

        private void CommitcountPerUserToolStripMenuItemClick(object sender, EventArgs e)
        {
            using (var frm = new FormCommitCount(UICommands))
            {
                frm.ShowDialog(this);
            }
        }

        private void KGitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Module.RunGitK();
        }

        private void DonateToolStripMenuItemClick(object sender, EventArgs e)
        {
            using (var frm = new FormDonate())
            {
                frm.ShowDialog(this);
            }
        }

        private void FormBrowseFormClosing(object sender, FormClosingEventArgs e)
        {
            SaveApplicationSettings();
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
            if (revisions.Count > 2)
            {
                MessageBox.Show(this, @"Select only one or two revisions. Abort.", @"Archive revision");
                return;
            }

            GitRevision mainRevision = revisions.First();
            GitRevision diffRevision = null;
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
            FormProcess.ShowModeless(this, "gc");
        }

        private void VerifyGitDatabaseToolStripMenuItemClick(object sender, EventArgs e)
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

            if (revisions.Count == 2)
            {
                string to = null;
                string from = null;

                string currentBranch = Module.GetSelectedBranch();
                string currentCheckout = RevisionGrid.CurrentCheckout;

                if (revisions[0].Guid == currentCheckout)
                {
                    from = revisions[1].Guid.Substring(0, 8);
                    to = currentBranch;
                }
                else if (revisions[1].Guid == currentCheckout)
                {
                    from = revisions[0].Guid.Substring(0, 8);
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
            Module.RunExternalCmdDetached(AppSettings.Pageant, "");
        }

        private void GenerateOrImportKeyToolStripMenuItemClick(object sender, EventArgs e)
        {
            Module.RunExternalCmdDetached(AppSettings.Puttygen, "");
        }

        private void CommitInfoTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillFileTree();
            FillDiff();
            FillCommitInfo();
            ThreadHelper.JoinableTaskFactory.RunAsync(() => FillGpgInfoAsync());
            FillBuildReport();
            FillTerminalTab();
        }

        private void ChangelogToolStripMenuItemClick(object sender, EventArgs e)
        {
            using (var frm = new FormChangeLog())
            {
                frm.ShowDialog(this);
            }
        }

        private void ToolStripButtonPushClick(object sender, EventArgs e)
        {
            PushToolStripMenuItemClick(sender, e);
        }

        private void ManageSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartSubmodulesDialog(this);
        }

        private void UpdateSubmoduleToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem toolStripMenuItem)
            {
                var submodule = toolStripMenuItem.Tag as string;
                FormProcess.ShowDialog(this, Module.SuperprojectModule, GitCommandHelpers.SubmoduleUpdateCmd(submodule));
            }

            UICommands.RepoChangedNotifier.Notify();
        }

        private void UpdateAllSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartUpdateSubmodulesDialog(this);
        }

        private void SynchronizeAllSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartSyncSubmodulesDialog(this);
        }

        private void ToolStripSplitStashButtonClick(object sender, EventArgs e)
        {
            UICommands.StartStashDialog(this);
        }

        private void StashChangesToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash);
        }

        private void StashPopToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StashPop(this);
        }

        private void ManageStashesToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartStashDialog(this);
        }

        private void CreateStashToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartStashDialog(this, false);
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Close();
        }

        private void ChangeWorkingDir(string path)
        {
            var module = new GitModule(path);
            if (module.IsValidGitWorkingDir())
            {
                SetGitModule(this, new GitModuleEventArgs(module));
                return;
            }

            DialogResult dialogResult = MessageBox.Show(this, _directoryIsNotAValidRepository.Text,
                                                        _directoryIsNotAValidRepositoryCaption.Text,
                                                        MessageBoxButtons.YesNoCancel,
                                                        MessageBoxIcon.Exclamation,
                                                        MessageBoxDefaultButton.Button1);
            if (dialogResult != DialogResult.Yes)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveRecentAsync(path));
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

        public override void CancelButtonClick(object sender, EventArgs e)
        {
            // If a filter is applied, clear it
            if (RevisionGrid.FilterIsApplied(false))
            {
                // Clear filter
                _filterRevisionsHelper.SetFilter(string.Empty);
            }

            // If a branch filter is applied by text or using the menus "Show current branch only"
            else if (RevisionGrid.FilterIsApplied(true) || AppSettings.BranchFilterEnabled)
            {
                // Clear branch filter
                _filterBranchHelper.SetBranchFilter(string.Empty, true);

                // Execute the "Show all branches" menu option
                RevisionGrid.ShowAllBranches_ToolStripMenuItemClick(sender, e);
            }
        }

        private void UserManualToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://git-extensions-documentation.readthedocs.org/en/release-2.51/");
            }
            catch (System.ComponentModel.Win32Exception)
            {
            }
        }

        private void CleanupToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCleanupRepositoryDialog(this);
        }

        private void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container)
        {
            var mostRecentRepos = new List<RecentRepoInfo>();
            var lessRecentRepos = new List<RecentRepoInfo>();

            var repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync());
            if (repositoryHistory.Count < 1)
            {
                return;
            }

            using (var graphics = CreateGraphics())
            {
                var splitter = new RecentRepoSplitter
                {
                    MeasureFont = container.Font,
                    Graphics = graphics
                };
                splitter.SplitRecentRepos(repositoryHistory, mostRecentRepos, lessRecentRepos);
            }

            foreach (var repo in mostRecentRepos.Union(lessRecentRepos).GroupBy(k => k.Repo.Category))
            {
                AddFavouriteRepositories(repo.Key, repo.ToList());
            }

            void AddFavouriteRepositories(string category, IList<RecentRepoInfo> repos)
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

                repos.ForEach(r =>
                {
                    var item = new ToolStripMenuItem(r.Caption)
                    {
                        DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                    };
                    menuItemCategory.DropDownItems.Add(item);

                    item.Click += (hs, he) => ChangeWorkingDir(r.Repo.Path);

                    if (!r.Repo.Path.Equals(r.Caption))
                    {
                        item.ToolTipText = r.Repo.Path;
                    }
                });
            }
        }

        private void PopulateRecentRepositoriesMenu(ToolStripDropDownItem container)
        {
            var mostRecentRepos = new List<RecentRepoInfo>();
            var lessRecentRepos = new List<RecentRepoInfo>();

            var repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.LoadRecentHistoryAsync());
            if (repositoryHistory.Count < 1)
            {
                return;
            }

            using (var graphics = CreateGraphics())
            {
                var splitter = new RecentRepoSplitter
                {
                    MeasureFont = container.Font,
                    Graphics = graphics
                };
                splitter.SplitRecentRepos(repositoryHistory, mostRecentRepos, lessRecentRepos);
            }

            foreach (var repo in mostRecentRepos)
            {
                AddRecentRepositories(repo.Repo, repo.Caption);
            }

            if (lessRecentRepos.Count > 0)
            {
                if (mostRecentRepos.Count > 0 && (AppSettings.SortMostRecentRepos || AppSettings.SortLessRecentRepos))
                {
                    container.DropDownItems.Add(new ToolStripSeparator());
                }

                foreach (var repo in lessRecentRepos)
                {
                    AddRecentRepositories(repo.Repo, repo.Caption);
                }
            }

            void AddRecentRepositories(Repository repo, string caption)
            {
                if (!string.IsNullOrEmpty(repo.Category))
                {
                    caption = $"{caption} ({repo.Category})";
                }

                var item = new ToolStripMenuItem(caption)
                {
                    DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                };
                container.DropDownItems.Add(item);

                item.Click += (hs, he) => ChangeWorkingDir(repo.Path);

                if (!repo.Path.Equals(caption))
                {
                    item.ToolTipText = repo.Path;
                }
            }
        }

        private void WorkingdirDropDownOpening(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Workingdir.DropDownItems.Clear();

            var tsmiCategorisedRepos = new ToolStripMenuItem(tsmiFavouriteRepositories.Text, tsmiFavouriteRepositories.Image);
            PopulateFavouriteRepositoriesMenu(tsmiCategorisedRepos);
            if (tsmiCategorisedRepos.DropDownItems.Count > 0)
            {
                _NO_TRANSLATE_Workingdir.DropDownItems.Add(tsmiCategorisedRepos);
            }

            PopulateRecentRepositoriesMenu(_NO_TRANSLATE_Workingdir);

            _NO_TRANSLATE_Workingdir.DropDownItems.Add(new ToolStripSeparator());

            var toolStripItem = new ToolStripMenuItem(openToolStripMenuItem.Text, openToolStripMenuItem.Image);
            toolStripItem.ShortcutKeys = openToolStripMenuItem.ShortcutKeys;
            _NO_TRANSLATE_Workingdir.DropDownItems.Add(toolStripItem);
            toolStripItem.Click += (hs, he) => OpenToolStripMenuItemClick(hs, he);

            toolStripItem = new ToolStripMenuItem(_configureWorkingDirMenu.Text);
            _NO_TRANSLATE_Workingdir.DropDownItems.Add(toolStripItem);
            toolStripItem.Click += (hs, he) =>
            {
                using (var frm = new FormRecentReposSettings())
                {
                    frm.ShowDialog(this);
                }

                RefreshWorkingDirCombo();
            };

            PreventToolStripSplitButtonClosing(sender as ToolStripSplitButton);
        }

        public void SetWorkingDir(string path)
        {
            SetGitModule(this, new GitModuleEventArgs(new GitModule(path)));
        }

        private void SetGitModule(object sender, GitModuleEventArgs e)
        {
            var module = e.GitModule;
            HideVariableMainMenuItems();
            UnregisterPlugins();

            UICommands = new GitUICommands(module);
            if (Module.IsValidGitWorkingDir())
            {
                var path = Module.WorkingDir;
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(path));
                AppSettings.RecentWorkingDir = path;
                ChangeTerminalActiveFolder(path);

#if DEBUG
                // Current encodings
                Debug.WriteLine("Encodings for " + path);
                Debug.WriteLine("Files content encoding: " + module.FilesEncoding.EncodingName);
                Debug.WriteLine("Commit encoding: " + module.CommitEncoding.EncodingName);
                if (module.LogOutputEncoding.CodePage != module.CommitEncoding.CodePage)
                {
                    Debug.WriteLine("Log output encoding: " + module.LogOutputEncoding.EncodingName);
                }
#endif

                HideDashboard();
                UICommands.RepoChangedNotifier.Notify();
                RevisionGrid.IndexWatcher.Reset();
                RegisterPlugins();
            }
            else
            {
                dashboardToolStripMenuItem.Visible = true;

                RevisionGrid.IndexWatcher.Reset();
                MainSplitContainer.Visible = false;
                ShowDashboard();
            }
        }

        private void TranslateToolStripMenuItemClick(object sender, EventArgs e)
        {
            Process.Start("https://www.transifex.com/git-extensions/git-extensions/translate/");
        }

        private void FileExplorerToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Module.WorkingDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void StatusClick(object sender, EventArgs e)
        {
            // TODO: Replace with a status page?
            CommitToolStripMenuItemClick(sender, e);
        }

        private void CreateBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCreateBranchDialog(this, RevisionGrid.GetSelectedRevisions().FirstOrDefault());
        }

        private void GitBashClick(object sender, EventArgs e)
        {
            GitBashToolStripMenuItemClick1(sender, e);
        }

        private void ToolStripButtonPullClick(object sender, EventArgs e)
        {
            PullToolStripMenuItemClick(sender, e);
        }

        private void editgitattributesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UICommands.StartEditGitAttributesDialog(this);
        }

        public static void CopyFullPathToClipboard(FileStatusList diffFiles, GitModule module)
        {
            if (!diffFiles.SelectedItems.Any())
            {
                return;
            }

            var fileNames = new StringBuilder();
            foreach (var item in diffFiles.SelectedItems)
            {
                // Only use append line when multiple items are selected.
                // This to make it easier to use the text from clipboard when 1 file is selected.
                if (fileNames.Length > 0)
                {
                    fileNames.AppendLine();
                }

                fileNames.Append(Path.Combine(module.WorkingDir, item.Name).ToNativePath());
            }

            Clipboard.SetText(fileNames.ToString());
        }

        private void deleteIndexlockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Module.UnlockIndex(true);
            }
            catch (FileDeleteException ex)
            {
                MessageBox.Show(this, $@"{_indexLockCantDelete.Text}: {ex.FileName}{Environment.NewLine}{ex.Message}");
            }
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            statusStrip.Hide();
        }

        private void BisectClick(object sender, EventArgs e)
        {
            using (var frm = new FormBisect(RevisionGrid))
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

            PreventToolStripSplitButtonClosing(sender as ToolStripSplitButton);
        }

        private void AddCheckoutBranchMenuItem()
        {
            var checkoutBranchItem = new ToolStripMenuItem(checkoutBranchToolStripMenuItem.Text)
            {
                ShortcutKeys = checkoutBranchToolStripMenuItem.ShortcutKeys,
                ShortcutKeyDisplayString = checkoutBranchToolStripMenuItem.ShortcutKeyDisplayString
            };
            branchSelect.DropDownItems.Add(checkoutBranchItem);
            checkoutBranchItem.Click += CheckoutBranchToolStripMenuItemClick;
        }

        private void AddBranchesMenuItems()
        {
            foreach (string branchName in GetBranchNames())
            {
                ToolStripItem toolStripItem = branchSelect.DropDownItems.Add(branchName);
                toolStripItem.Click += BranchSelectToolStripItem_Click;
            }
        }

        private IEnumerable<string> GetBranchNames()
        {
            IEnumerable<string> branchNames = Module.GetRefs(false).Select(b => b.Name);
            if (AppSettings.BranchOrderingCriteria == BranchOrdering.Alphabetically)
            {
                branchNames = branchNames.OrderBy(b => b);
            }

            // Make sure there are never more than a 100 branches added to the menu
            // GitExtensions will hang when the drop down is too large...
            branchNames = branchNames.Take(100);

            return branchNames;
        }

        private void BranchSelectToolStripItem_Click(object sender, EventArgs e)
        {
            var toolStripItem = (ToolStripItem)sender;
            UICommands.StartCheckoutBranch(this, toolStripItem.Text);
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
                MessageBox.Show(this, _noReposHostPluginLoaded.Text, _errorCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void _viewPullRequestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var repoHost = PluginRegistry.TryGetGitHosterForModule(Module);
            if (repoHost == null)
            {
                MessageBox.Show(this, _noReposHostFound.Text, _errorCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            UICommands.StartPullRequestsDialog(this, repoHost);
        }

        private void _createPullRequestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var repoHost = PluginRegistry.TryGetGitHosterForModule(Module);
            if (repoHost == null)
            {
                MessageBox.Show(this, _noReposHostFound.Text, _errorCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            UICommands.StartCreatePullRequest(this, repoHost);
        }

        #region Hotkey commands

        public static readonly string HotkeySettingsName = "Browse";

        internal enum Commands
        {
            GitBash = 0,
            GitGui = 1,
            GitGitK = 2,
            FocusRevisionGrid = 3,
            FocusCommitInfo = 4,
            FocusFileTree = 5,
            FocusDiff = 6,
            FocusFilter = 18,
            Commit = 7,
            AddNotes = 8,
            FindFileInSelectedCommit = 9,
            CheckoutBranch = 10,
            QuickFetch = 11,
            QuickPull = 12,
            QuickPush = 13,
            RotateApplicationIcon = 14,
            CloseRepository = 15,
            Stash = 16,
            StashPop = 17,
            OpenWithDifftool = 19
        }

        private void AddNotes()
        {
            Module.EditNotes(RevisionGrid.GetSelectedRevisions().Count > 0 ? RevisionGrid.GetSelectedRevisions()[0].Guid : string.Empty);
            FillCommitInfo();
        }

        private void FocusFilter()
        {
            ToolStripControlHost filterToFocus = toolStripRevisionFilterTextBox.Focused ? (ToolStripControlHost)toolStripBranchFilterComboBox : (ToolStripControlHost)toolStripRevisionFilterTextBox;
            filterToFocus.Focus();
        }

        private void FindFileInSelectedCommit()
        {
            CommitInfoTabControl.SelectedTab = TreeTabPage;
            EnabledSplitViewLayout(true);

            fileTree.InvokeFindFileDialog();
        }

        private void QuickFetch()
        {
            FormProcess.ShowDialog(this, Module.FetchCmd(string.Empty, string.Empty, string.Empty));
            UICommands.RepoChangedNotifier.Notify();
        }

        private void OpenWithDifftool()
        {
            if (revisionDiff.Visible)
            {
                revisionDiff.ExecuteCommand(RevisionDiff.Command.OpenWithDifftool);
            }
            else if (fileTree.Visible)
            {
                fileTree.ExecuteCommand(RevisionFileTree.Command.OpenWithDifftool);
            }
        }

        protected override bool ExecuteCommand(int cmd)
        {
            switch ((Commands)cmd)
            {
                case Commands.GitBash: Module.RunBash(); break;
                case Commands.GitGui: Module.RunGui(); break;
                case Commands.GitGitK: Module.RunGitK(); break;
                case Commands.FocusRevisionGrid: RevisionGrid.Focus(); break;
                case Commands.FocusCommitInfo: CommitInfoTabControl.SelectedTab = CommitInfoTabPage; break;
                case Commands.FocusFileTree: CommitInfoTabControl.SelectedTab = TreeTabPage; fileTree.Focus(); break;
                case Commands.FocusDiff: CommitInfoTabControl.SelectedTab = DiffTabPage; revisionDiff.Focus(); break;
                case Commands.FocusFilter: FocusFilter(); break;
                case Commands.Commit: CommitToolStripMenuItemClick(null, null); break;
                case Commands.AddNotes: AddNotes(); break;
                case Commands.FindFileInSelectedCommit: FindFileInSelectedCommit(); break;
                case Commands.CheckoutBranch: CheckoutBranchToolStripMenuItemClick(null, null); break;
                case Commands.QuickFetch: QuickFetch(); break;
                case Commands.QuickPull: UICommands.StartPullDialogAndPullImmediately(this); break;
                case Commands.QuickPush: UICommands.StartPushDialog(this, true); break;
                case Commands.RotateApplicationIcon: RotateApplicationIcon(); break;
                case Commands.CloseRepository: CloseToolStripMenuItemClick(null, null); break;
                case Commands.Stash: UICommands.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash); break;
                case Commands.StashPop: UICommands.StashPop(this); break;
                case Commands.OpenWithDifftool: OpenWithDifftool(); break;
                default: return base.ExecuteCommand(cmd);
            }

            return true;
        }

        internal bool ExecuteCommand(Commands cmd)
        {
            return ExecuteCommand((int)cmd);
        }

        #endregion

        private void toggleSplitViewLayout_Click(object sender, EventArgs e)
        {
            EnabledSplitViewLayout(RightSplitContainer.Panel2Collapsed);
        }

        private void EnabledSplitViewLayout(bool enabled)
        {
            RightSplitContainer.Panel2Collapsed = !enabled;
        }

        public static void OpenContainingFolder(FileStatusList diffFiles, GitModule module)
        {
            if (!diffFiles.SelectedItems.Any())
            {
                return;
            }

            foreach (var item in diffFiles.SelectedItems)
            {
                string filePath = Path.Combine(module.WorkingDir, item.Name.ToNativePath());
                FormBrowseUtil.ShowFileOrParentFolderInFileExplorer(filePath);
            }
        }

        protected void SetSplitterPositions()
        {
            _splitterManager.AddSplitter(RevisionsSplitContainer, "RevisionsSplitContainer");
            _splitterManager.AddSplitter(MainSplitContainer, "MainSplitContainer");
            _splitterManager.AddSplitter(RightSplitContainer, nameof(RightSplitContainer));
            revisionDiff.InitSplitterManager(_splitterManager);
            fileTree.InitSplitterManager(_splitterManager);

            // hide status in order to restore splitters against the full height (the most common case)
            statusStrip.Hide();
            _splitterManager.RestoreSplitters();
            RefreshBranchTreeToggleButtonState();
        }

        protected void SaveSplitterPositions()
        {
            _splitterManager.SaveSplitters();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            SaveSplitterPositions();
        }

        protected override void OnClosed(EventArgs e)
        {
            UnregisterPlugins();

            base.OnClosed(e);
        }

        private void CommandsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            // Most options do not make sense for artificial commits or no revision selected at all
            var selectedRevisions = RevisionGrid.GetSelectedRevisions();
            bool enabled = selectedRevisions.Count == 1 && !selectedRevisions[0].IsArtificial;

            branchToolStripMenuItem.Enabled =
            deleteBranchToolStripMenuItem.Enabled =
            mergeBranchToolStripMenuItem.Enabled =
            rebaseToolStripMenuItem.Enabled =
            stashToolStripMenuItem.Enabled =
              selectedRevisions.Count > 0 && !Module.IsBareRepository();

            undoLastCommitToolStripMenuItem.Enabled =
            resetToolStripMenuItem.Enabled =
            checkoutBranchToolStripMenuItem.Enabled =
            runMergetoolToolStripMenuItem.Enabled =
            cherryPickToolStripMenuItem.Enabled =
            checkoutToolStripMenuItem.Enabled =
            toolStripMenuItemReflog.Enabled =
            bisectToolStripMenuItem.Enabled =
              enabled && !Module.IsBareRepository();

            tagToolStripMenuItem.Enabled =
            deleteTagToolStripMenuItem.Enabled =
            archiveToolStripMenuItem.Enabled =
              enabled;
        }

        public override void AddTranslationItems(ITranslation translation)
        {
            base.AddTranslationItems(translation);
            TranslationUtils.AddTranslationItemsFromFields(Name, _filterRevisionsHelper, translation);
            TranslationUtils.AddTranslationItemsFromFields(Name, _filterBranchHelper, translation);
        }

        public override void TranslateItems(ITranslation translation)
        {
            base.TranslateItems(translation);
            TranslationUtils.TranslateItemsFromFields(Name, _filterRevisionsHelper, translation);
            TranslationUtils.TranslateItemsFromFields(Name, _filterBranchHelper, translation);
        }

        private void dontSetAsDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.SetNextPullActionAsDefault = !setNextPullActionAsDefaultToolStripMenuItem.Checked;
            setNextPullActionAsDefaultToolStripMenuItem.Checked = AppSettings.SetNextPullActionAsDefault;
        }

        private void DoPullAction(Action action)
        {
            var actLactPullAction = Module.LastPullAction;
            try
            {
                action();
            }
            finally
            {
                if (!AppSettings.SetNextPullActionAsDefault)
                {
                    Module.LastPullAction = actLactPullAction;
                    Module.LastPullActionToFormPullAction();
                }

                AppSettings.SetNextPullActionAsDefault = false;
                RefreshPullIcon();
            }
        }

        private void mergeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoPullAction(() =>
                {
                    Module.LastPullAction = AppSettings.PullAction.Merge;
                    PullToolStripMenuItemClick(sender, e);
                });
        }

        private void rebaseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DoPullAction(() =>
            {
                Module.LastPullAction = AppSettings.PullAction.Rebase;
                PullToolStripMenuItemClick(sender, e);
            });
        }

        private void fetchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoPullAction(() =>
            {
                Module.LastPullAction = AppSettings.PullAction.Fetch;
                PullToolStripMenuItemClick(sender, e);
            });
        }

        private void pullToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (AppSettings.SetNextPullActionAsDefault)
            {
                Module.LastPullAction = AppSettings.PullAction.None;
            }

            PullToolStripMenuItemClick(sender, e);

            // restore AppSettings.FormPullAction value
            if (!AppSettings.SetNextPullActionAsDefault)
            {
                Module.LastPullActionToFormPullAction();
            }

            AppSettings.SetNextPullActionAsDefault = false;
        }

        private void RefreshPullIcon()
        {
            switch (Module.LastPullAction)
            {
                case AppSettings.PullAction.Fetch:
                    toolStripButtonPull.Image = Resources.PullFetch;
                    toolStripButtonPull.ToolTipText = _pullFetch.Text;
                    break;

                case AppSettings.PullAction.FetchAll:
                    toolStripButtonPull.Image = Resources.PullFetchAll;
                    toolStripButtonPull.ToolTipText = _pullFetchAll.Text;
                    break;

                case AppSettings.PullAction.Merge:
                    toolStripButtonPull.Image = Resources.PullMerge;
                    toolStripButtonPull.ToolTipText = _pullMerge.Text;
                    break;

                case AppSettings.PullAction.Rebase:
                    toolStripButtonPull.Image = Resources.PullRebase;
                    toolStripButtonPull.ToolTipText = _pullRebase.Text;
                    break;

                default:
                    toolStripButtonPull.Image = Resources.IconPull;
                    toolStripButtonPull.ToolTipText = _pullOpenDialog.Text;
                    break;
            }
        }

        private void fetchAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AppSettings.SetNextPullActionAsDefault)
            {
                Module.LastPullAction = AppSettings.PullAction.FetchAll;
            }

            RefreshPullIcon();

            UICommands.StartPullDialogAndPullImmediately(this, fetchAll: true);

            // restore AppSettings.FormPullAction value
            if (!AppSettings.SetNextPullActionAsDefault)
            {
                Module.LastPullActionToFormPullAction();
            }

            AppSettings.SetNextPullActionAsDefault = false;
        }

        private void _NO_TRANSLATE_Workingdir_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                OpenToolStripMenuItemClick(sender, e);
            }
        }

        private void branchSelect_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                CheckoutBranchToolStripMenuItemClick(sender, e);
            }
        }

        private void RevisionInfo_CommandClick(object sender, CommitInfo.CommandEventArgs e)
        {
            if (e.Command == "gotocommit")
            {
                var revision = _longShaProvider.Get(e.Data);
                var found = RevisionGrid.SetSelectedRevision(revision);

                // When 'git log --first-parent' filtration is used, user can click on child commit
                // that is not present in the shown git log. User still wants to see the child commit
                // and to make it possible we add explicit branch filter and refresh.
                if (AppSettings.ShowFirstParent && !found)
                {
                    _filterBranchHelper.SetBranchFilter(revision, refresh: true);
                    RevisionGrid.SetSelectedRevision(revision);
                }
            }
            else if (e.Command == "gotobranch" || e.Command == "gototag")
            {
                CommitData commit = _commitDataManager.GetCommitData(e.Data, out _);
                if (commit != null)
                {
                    RevisionGrid.SetSelectedRevision(new GitRevision(commit.Guid));
                }
            }
            else if (e.Command == "navigatebackward")
            {
                RevisionGrid.NavigateBackward();
            }
            else if (e.Command == "navigateforward")
            {
                RevisionGrid.NavigateForward();
            }
        }

        private void SubmoduleToolStripButtonClick(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuSender)
            {
                SetWorkingDir(menuSender.Tag as string);
            }
        }

        private void PreventToolStripSplitButtonClosing(ToolStripSplitButton control)
        {
            if (control == null || toolStripBranchFilterComboBox.Focused || toolStripRevisionFilterTextBox.Focused)
            {
                return;
            }

            control.Tag = this.FindFocusedControl();
            control.DropDownClosed += ToolStripSplitButtonDropDownClosed;
            toolStripBranchFilterComboBox.Focus();
        }

        private static void ToolStripSplitButtonDropDownClosed(object sender, EventArgs e)
        {
            if (sender is ToolStripSplitButton control)
            {
                control.DropDownClosed -= ToolStripSplitButtonDropDownClosed;

                if (control.Tag is Control controlToFocus)
                {
                    controlToFocus.Focus();
                    control.Tag = null;
                }
            }
        }

        private void toolStripButtonLevelUp_DropDownOpening(object sender, EventArgs e)
        {
            LoadSubmodulesIntoDropDownMenu();
            PreventToolStripSplitButtonClosing(sender as ToolStripSplitButton);
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

        private string GetModuleBranch(string path)
        {
            string branch = GitModule.GetSelectedBranchFast(path);
            return string.Format("[{0}]", DetachedHeadParser.IsDetachedHead(branch) ? _noBranchTitle.Text : branch);
        }

        private ToolStripMenuItem CreateSubmoduleMenuItem(SubmoduleInfo info, string textFormat = "{0}")
        {
            var spmenu = new ToolStripMenuItem(string.Format(textFormat, info.Text));
            spmenu.Click += SubmoduleToolStripButtonClick;
            spmenu.Width = 200;
            spmenu.Tag = info.Path;
            if (info.Bold)
            {
                spmenu.Font = new Font(spmenu.Font, FontStyle.Bold);
            }

            spmenu.Image = GetItemImage(info);
            return spmenu;
        }

        private DateTime _previousUpdateTime;

        private void LoadSubmodulesIntoDropDownMenu()
        {
            TimeSpan elapsed = DateTime.Now - _previousUpdateTime;
            if (elapsed.TotalSeconds > 15)
            {
                UpdateSubmodulesList();
            }
        }

        /// <summary>Holds submodule information that is gathered asynchronously.</summary>
        private class SubmoduleInfo
        {
            public string Text; // User-friendly display text
            public string Path; // Full path to submodule
            public SubmoduleStatus? Status;
            public bool IsDirty;
            public bool Bold;
        }

        /// <summary>Complete set of gathered submodule information.</summary>
        private class SubmoduleInfoResult
        {
            public readonly List<SubmoduleInfo> OurSubmodules = new List<SubmoduleInfo>();
            public readonly List<SubmoduleInfo> SuperSubmodules = new List<SubmoduleInfo>();
            public SubmoduleInfo TopProject;
            public SubmoduleInfo Superproject;
            public string CurrentSubmoduleName;
        }

        private static Image GetItemImage(SubmoduleInfo info)
        {
            if (info.Status == null)
            {
                return Resources.IconFolderSubmodule;
            }

            if (info.Status == SubmoduleStatus.FastForward)
            {
                return info.IsDirty ? Resources.IconSubmoduleRevisionUpDirty : Resources.IconSubmoduleRevisionUp;
            }

            if (info.Status == SubmoduleStatus.Rewind)
            {
                return info.IsDirty ? Resources.IconSubmoduleRevisionDownDirty : Resources.IconSubmoduleRevisionDown;
            }

            if (info.Status == SubmoduleStatus.NewerTime)
            {
                return info.IsDirty ? Resources.IconSubmoduleRevisionSemiUpDirty : Resources.IconSubmoduleRevisionSemiUp;
            }

            if (info.Status == SubmoduleStatus.OlderTime)
            {
                return info.IsDirty ? Resources.IconSubmoduleRevisionSemiDownDirty : Resources.IconSubmoduleRevisionSemiDown;
            }

            return info.IsDirty ? Resources.IconSubmoduleDirty : Resources.Modified;
        }

        private static async Task GetSubmoduleStatusAsync(SubmoduleInfo info, CancellationToken cancelToken)
        {
            await TaskScheduler.Default;
            cancelToken.ThrowIfCancellationRequested();

            var submodule = new GitModule(info.Path);
            var supermodule = submodule.SuperprojectModule;
            var submoduleName = submodule.GetCurrentSubmoduleLocalPath();

            info.Status = null;

            if (string.IsNullOrEmpty(submoduleName) || supermodule == null)
            {
                return;
            }

            var submoduleStatus = GitCommandHelpers.GetCurrentSubmoduleChanges(supermodule, submoduleName);
            if (submoduleStatus != null && submoduleStatus.Commit != submoduleStatus.OldCommit)
            {
                submoduleStatus.CheckSubmoduleStatus(submoduleStatus.GetSubmodule(supermodule));
            }

            if (submoduleStatus != null)
            {
                info.Status = submoduleStatus.Status;
                info.IsDirty = submoduleStatus.IsDirty;
                info.Text += submoduleStatus.AddedAndRemovedString();
            }
        }

        private void UpdateSubmodulesList()
        {
            _previousUpdateTime = DateTime.Now;

            // Cancel any previous async activities:
            var cancelToken = _submodulesStatusSequence.Next();

            RemoveSubmoduleButtons();
            toolStripButtonLevelUp.DropDownItems.Add(_loading.Text);

            // Start gathering new submodule information asynchronously.  This makes a significant difference in UI
            // responsiveness if there are numerous submodules (e.g. > 100).
            string thisModuleDir = Module.WorkingDir;

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                // First task: Gather list of submodules on a background thread.

                // Don't access Module directly because it's not thread-safe.  Use a thread-local version:
                GitModule threadModule = new GitModule(thisModuleDir);
                SubmoduleInfoResult result = new SubmoduleInfoResult();

                // Add all submodules inside the current repository:
                foreach (var submodule in threadModule.GetSubmodulesLocalPaths().OrderBy(submoduleName => submoduleName))
                {
                    cancelToken.ThrowIfCancellationRequested();
                    var name = submodule;
                    string path = threadModule.GetSubmoduleFullPath(submodule);
                    if (AppSettings.DashboardShowCurrentBranch && !GitModule.IsBareRepository(path))
                    {
                        name = name + " " + GetModuleBranch(path);
                    }

                    var smi = new SubmoduleInfo { Text = name, Path = path };
                    result.OurSubmodules.Add(smi);
                    GetSubmoduleStatusAsync(smi, cancelToken).FileAndForget();
                }

                if (threadModule.SuperprojectModule != null)
                {
                    GitModule supersuperproject = threadModule.FindTopProjectModule();
                    if (threadModule.SuperprojectModule.WorkingDir != supersuperproject.WorkingDir)
                    {
                        var name = Path.GetFileName(Path.GetDirectoryName(supersuperproject.WorkingDir));
                        string path = supersuperproject.WorkingDir;
                        if (AppSettings.DashboardShowCurrentBranch && !GitModule.IsBareRepository(path))
                        {
                            name = name + " " + GetModuleBranch(path);
                        }

                        result.TopProject = new SubmoduleInfo { Text = name, Path = supersuperproject.WorkingDir };
                        GetSubmoduleStatusAsync(result.TopProject, cancelToken).FileAndForget();
                    }

                    {
                        string name;
                        if (threadModule.SuperprojectModule.WorkingDir != supersuperproject.WorkingDir)
                        {
                            var localpath = threadModule.SuperprojectModule.WorkingDir.Substring(supersuperproject.WorkingDir.Length);
                            localpath = PathUtil.GetDirectoryName(localpath.ToPosixPath());
                            name = localpath;
                        }
                        else
                        {
                            name = Path.GetFileName(Path.GetDirectoryName(supersuperproject.WorkingDir));
                        }

                        string path = threadModule.SuperprojectModule.WorkingDir;
                        if (AppSettings.DashboardShowCurrentBranch && !GitModule.IsBareRepository(path))
                        {
                            name = name + " " + GetModuleBranch(path);
                        }

                        result.Superproject = new SubmoduleInfo { Text = name, Path = threadModule.SuperprojectModule.WorkingDir };
                        GetSubmoduleStatusAsync(result.Superproject, cancelToken).FileAndForget();
                    }

                    var submodules = supersuperproject.GetSubmodulesLocalPaths().OrderBy(submoduleName => submoduleName);
                    if (submodules.Any())
                    {
                        string localpath = threadModule.WorkingDir.Substring(supersuperproject.WorkingDir.Length);
                        localpath = PathUtil.GetDirectoryName(localpath.ToPosixPath());

                        foreach (var submodule in submodules)
                        {
                            cancelToken.ThrowIfCancellationRequested();
                            var name = submodule;
                            string path = supersuperproject.GetSubmoduleFullPath(submodule);
                            if (AppSettings.DashboardShowCurrentBranch && !GitModule.IsBareRepository(path))
                            {
                                name = name + " " + GetModuleBranch(path);
                            }

                            bool bold = false;
                            if (submodule == localpath)
                            {
                                result.CurrentSubmoduleName = threadModule.GetCurrentSubmoduleLocalPath();
                                bold = true;
                            }

                            var smi = new SubmoduleInfo { Text = name, Path = path, Bold = bold };
                            result.SuperSubmodules.Add(smi);
                            GetSubmoduleStatusAsync(smi, cancelToken).FileAndForget();
                        }
                    }
                }

                // Second task: Populate toolbar menu on UI thread.  Note further tasks are created by
                // CreateSubmoduleMenuItem to update images with submodule status.
                await this.SwitchToMainThreadAsync(cancelToken);

                if (result == null)
                {
                    return;
                }

                RemoveSubmoduleButtons();
                var newItems = new List<ToolStripItem>();

                result.OurSubmodules.ForEach(submodule => newItems.Add(CreateSubmoduleMenuItem(submodule)));
                if (result.OurSubmodules.Count == 0)
                {
                    newItems.Add(new ToolStripMenuItem(_noSubmodulesPresent.Text));
                }

                if (result.Superproject != null)
                {
                    newItems.Add(new ToolStripSeparator());
                    if (result.TopProject != null)
                    {
                        newItems.Add(CreateSubmoduleMenuItem(result.TopProject, _topProjectModuleFormat.Text));
                    }

                    newItems.Add(CreateSubmoduleMenuItem(result.Superproject, _superprojectModuleFormat.Text));
                    result.SuperSubmodules.ForEach(submodule => newItems.Add(CreateSubmoduleMenuItem(submodule)));
                }

                newItems.Add(new ToolStripSeparator());

                var mi = new ToolStripMenuItem(updateAllSubmodulesToolStripMenuItem.Text);
                mi.Click += UpdateAllSubmodulesToolStripMenuItemClick;
                newItems.Add(mi);

                if (result.CurrentSubmoduleName != null)
                {
                    var usmi = new ToolStripMenuItem(_updateCurrentSubmodule.Text);
                    usmi.Tag = result.CurrentSubmoduleName;
                    usmi.Click += UpdateSubmoduleToolStripMenuItemClick;
                    newItems.Add(usmi);
                }

                // Using AddRange is critical: if you used Add to add menu items one at a
                // time, performance would be extremely slow with many submodules (> 100).
                toolStripButtonLevelUp.DropDownItems.AddRange(newItems.ToArray());

                _previousUpdateTime = DateTime.Now;
            });
        }

        private void toolStripButtonLevelUp_ButtonClick(object sender, EventArgs e)
        {
            if (Module.SuperprojectModule != null)
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
            Process.Start(@"https://github.com/gitextensions/gitextensions/issues/new");
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var updateForm = new FormUpdates(Module.AppVersion);
            updateForm.SearchForUpdatesAndShow(Owner, true);
        }

        private void toolStripButtonPull_DropDownOpened(object sender, EventArgs e)
        {
            setNextPullActionAsDefaultToolStripMenuItem.Checked = AppSettings.SetNextPullActionAsDefault;
            PreventToolStripSplitButtonClosing(sender as ToolStripSplitButton);
        }

        private void FormBrowse_Activated(object sender, EventArgs e)
        {
            this.InvokeAsyncDoNotUseInNewCode(OnActivate);
        }

        /// <summary>
        /// Adds a tab with console interface to Git over the current working copy. Recreates the terminal on tab activation if user exits the shell.
        /// </summary>
        private void FillTerminalTab()
        {
            if (!EnvUtils.RunningOnWindows() || !AppSettings.ShowConEmuTab.ValueOrDefault)
            {
                return; // ConEmu only works on WinNT
            }

            if (_terminal != null)
            {
                // if terminal is already created, then give it focus
                _terminal.Focus();
                return;
            }

            var tabpageCaption = _consoleTabCaption.Text;
            var tabpageCreated = CommitInfoTabControl.TabPages.ContainsKey(tabpageCaption);
            TabPage tabpage;
            if (tabpageCreated)
            {
                tabpage = CommitInfoTabControl.TabPages[tabpageCaption];
            }
            else
            {
                const string imageKey = "Resources.IconConsole";
                CommitInfoTabControl.ImageList.Images.Add(imageKey, Resources.IconConsole);
                CommitInfoTabControl.Controls.Add(tabpage = new TabPage(tabpageCaption));
                tabpage.Name = tabpageCaption;
                tabpage.ImageKey = imageKey;
            }

            // Delay-create the terminal window when the tab is first selected
            CommitInfoTabControl.Selecting += (sender, args) =>
            {
                if (args.TabPage != tabpage)
                {
                    return;
                }

                if (_terminal == null)
                {
                    // Lazy-create on first opening the tab
                    tabpage.Controls.Clear();
                    tabpage.Controls.Add(
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
                var startinfo = new ConEmuStartInfo
                {
                    StartupDirectory = Module.WorkingDir,
                    WhenConsoleProcessExits = WhenConsoleProcessExits.CloseConsoleEmulator
                };

                var startinfoBaseConfiguration = startinfo.BaseConfiguration;
                if (!string.IsNullOrWhiteSpace(AppSettings.ConEmuFontSize.ValueOrDefault))
                {
                    if (int.TryParse(AppSettings.ConEmuFontSize.ValueOrDefault, out var fontSize))
                    {
                        var nodeFontSize =
                            startinfoBaseConfiguration.SelectSingleNode("/key/key/key/value[@name='FontSize']");
                        if (nodeFontSize?.Attributes != null)
                        {
                            nodeFontSize.Attributes["data"].Value = fontSize.ToString("X8");
                        }
                    }
                }

                startinfo.BaseConfiguration = startinfoBaseConfiguration;

                string[] exeList;
                switch (AppSettings.ConEmuTerminal.ValueOrDefault)
                {
                    case "cmd":
                        exeList = new[] { "cmd.exe" };
                        break;
                    case "powershell":
                        exeList = new[] { "powershell.exe" };
                        break;
                    default:
                        // Choose the console: bash from git with fallback to cmd
                        const string justBash = "bash.exe"; // Generic bash, should generally be in the git dir, less configured than the specific git-bash
                        const string justSh = "sh.exe"; // Fallback to SH
                        exeList = new[] { justBash, justSh };
                        break;
                }

                string cmdPath = exeList.
                      Select(shell => PathUtil.TryFindShellPath(shell, out var shellPath) ? shellPath : null).
                      FirstOrDefault(shellPath => shellPath != null);

                if (cmdPath == null)
                {
                    startinfo.ConsoleProcessCommandLine = ConEmuConstants.DefaultConsoleCommandLine;
                }
                else
                {
                    cmdPath = cmdPath.Quote();
                    if (AppSettings.ConEmuTerminal.ValueOrDefault == "bash")
                    {
                        startinfo.ConsoleProcessCommandLine = cmdPath + " --login -i";
                    }
                    else
                    {
                        startinfo.ConsoleProcessCommandLine = cmdPath;
                    }
                }

                if (AppSettings.ConEmuStyle.ValueOrDefault != "Default")
                {
                    startinfo.ConsoleProcessExtraArgs = " -new_console:P:\"" + AppSettings.ConEmuStyle.ValueOrDefault + "\"";
                }

                // Set path to git in this window (actually, effective with CMD only)
                if (!string.IsNullOrEmpty(AppSettings.GitCommandValue))
                {
                    string dirGit = Path.GetDirectoryName(AppSettings.GitCommandValue);
                    if (!string.IsNullOrEmpty(dirGit))
                    {
                        startinfo.SetEnv("PATH", dirGit + ";" + "%PATH%");
                    }
                }

                _terminal.Start(startinfo, ThreadHelper.JoinableTaskFactory);
            };
        }

        public void ChangeTerminalActiveFolder(string path)
        {
            if (_terminal?.RunningSession == null || string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (AppSettings.ConEmuTerminal.ValueOrDefault == "bash")
            {
                if (PathUtil.TryConvertWindowsPathToPosix(path, out var posixPath))
                {
                    ClearTerminalCommandLineAndRunCommand("cd " + posixPath);
                }
            }
            else if (AppSettings.ConEmuTerminal.ValueOrDefault == "powershell")
            {
                ClearTerminalCommandLineAndRunCommand("cd \"" + path + "\"");
            }
            else
            {
                ClearTerminalCommandLineAndRunCommand("cd /D \"" + path + "\"");
            }
        }

        private void ClearTerminalCommandLineAndRunCommand(string command)
        {
            if (_terminal?.RunningSession == null || string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            // Clear terminal line by sending 'backspace' characters
            for (int i = 0; i < 10000; i++)
            {
                _terminal.RunningSession.WriteInputTextAsync("\b");
            }

            _terminal.RunningSession.WriteInputTextAsync(command + Environment.NewLine);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_commitButton != null)
                {
                    _commitButton.Dispose();
                }

                if (_pushButton != null)
                {
                    _pushButton.Dispose();
                }

                if (_pullButton != null)
                {
                    _pullButton.Dispose();
                }

                if (_submodulesStatusSequence != null)
                {
                    _submodulesStatusSequence.Dispose();
                }

                if (_formBrowseMenus != null)
                {
                    _formBrowseMenus.Dispose();
                }

                if (_filterRevisionsHelper != null)
                {
                    _filterRevisionsHelper.Dispose();
                }

                if (_filterBranchHelper != null)
                {
                    _filterBranchHelper.Dispose();
                }

                if (components != null)
                {
                    components.Dispose();
                }

                if (_gitStatusMonitor != null)
                {
                    _gitStatusMonitor.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private void menuitemSparseWorkingCopy_Click(object sender, EventArgs e)
        {
            UICommands.StartSparseWorkingCopyDialog(this);
        }

        private void toolStripBranches_DropDown_ResizeDropDownWidth(object sender, EventArgs e)
        {
            ComboBoxHelper.ResizeComboBoxDropDownWidth(toolStripBranchFilterComboBox.ComboBox, AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);
        }

        private void toolStripMenuItemReflog_Click(object sender, EventArgs e)
        {
            var formReflog = new FormReflog(UICommands);
            formReflog.ShowDialog();
            if (formReflog.ShouldRefresh)
            {
                RefreshRevisions();
            }
        }

        private void toggleBranchTreePanel_Click(object sender, EventArgs e)
        {
            MainSplitContainer.Panel1Collapsed = !MainSplitContainer.Panel1Collapsed;
            ReloadRepoObjectsTree();
            RefreshBranchTreeToggleButtonState();
        }

        private void RefreshBranchTreeToggleButtonState()
        {
            toggleBranchTreePanel.Checked = !MainSplitContainer.Panel1Collapsed;
        }

        private void manageWorktreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var formManageWorktree = new FormManageWorktree(UICommands);
            formManageWorktree.ShowDialog(this);
        }

        private void createWorktreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var formCreateWorktree = new FormCreateWorktree(UICommands);
            var dialogResult = formCreateWorktree.ShowDialog(this);
            if (dialogResult == DialogResult.OK && formCreateWorktree.OpenWorktree)
            {
                var newModule = new GitModule(formCreateWorktree.WorktreeDirectory);
                SetGitModule(this, new GitModuleEventArgs(newModule));
            }
        }

        private void toolStripSplitStash_DropDownOpened(object sender, EventArgs e)
        {
            PreventToolStripSplitButtonClosing(sender as ToolStripSplitButton);
        }

        private void toolStripBranchFilterComboBox_Click(object sender, EventArgs e)
        {
            toolStripBranchFilterComboBox.DroppedDown = true;
        }

        private void undoLastCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AppSettings.DontConfirmUndoLastCommit || MessageBox.Show(this, _undoLastCommitText.Text, _undoLastCommitCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Module.RunGitCmd("reset --soft HEAD~1");
                RefreshRevisions();
            }
        }
    }
}
