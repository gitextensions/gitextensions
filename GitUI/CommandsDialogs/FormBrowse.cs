using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Taskbar;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormBrowse : GitModuleForm, IBrowseRepo
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
        private readonly TranslationString _directoryIsNotAValidRepository = new TranslationString("The selected item is not a valid git repository.\n\nDo you want to remove it from the recent repositories list?");

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

        private readonly CancellationTokenSequence _submodulesStatusSequence = new CancellationTokenSequence();
        private readonly SplitterManager _splitterManager = new SplitterManager(new AppSettingsPath("FormBrowse"));
        private readonly ToolStripMenuItem _toolStripGitStatus;
        private readonly GitStatusMonitor _gitStatusMonitor;
        private readonly FilterRevisionsHelper _filterRevisionsHelper;
        private readonly FilterBranchHelper _filterBranchHelper;
        private readonly FormBrowseMenus _formBrowseMenus;
        private readonly IFormBrowseController _controller;
        private readonly ICommitDataManager _commitDataManager;
        private readonly IAppTitleGenerator _appTitleGenerator;
        private readonly WindowsJumpListManager _windowsJumpListManager;

        [CanBeNull] private BuildReportTabPageExtension _buildReportTabPageExtension;
        private ConEmuControl _terminal;
        private Dashboard _dashboard;
        private ToolStripItem _rebase;
        private ToolStripItem _bisect;
        private ToolStripItem _warning;
        private bool _startWithDashboard;

        [Flags]
        private enum UpdateTargets
        {
            None = 1,
            DiffList = 2,
            FileTree = 4,
            CommitInfo = 8
        }

        private UpdateTargets _selectedRevisionUpdatedTargets = UpdateTargets.None;

        /// <summary>
        /// For VS designer
        /// </summary>
        private FormBrowse()
        {
            InitializeComponent();
            InitializeComplete();
        }

        public FormBrowse([CanBeNull] GitUICommands commands, string filter, ObjectId selectCommit = null, bool startWithDashboard = false)
            : base(true, commands)
        {
            _startWithDashboard = startWithDashboard;

            InitializeComponent();

            commandsToolStripMenuItem.DropDownOpening += CommandsToolStripMenuItem_DropDownOpening;

            MainSplitContainer.Visible = false;
            MainSplitContainer.SplitterDistance = DpiUtil.Scale(120);

            // set tab page images
            CommitInfoTabControl.ImageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth8Bit,
                ImageSize = DpiUtil.Scale(new Size(16, 16)),
                Images =
                {
                    { nameof(Images.CommitSummary), Images.CommitSummary },
                    { nameof(Images.FileTree), Images.FileTree },
                    { nameof(Images.Diff), Images.Diff },
                    { nameof(Images.Key), Images.Key }
                }
            };
            CommitInfoTabPage.ImageKey = nameof(Images.CommitSummary);
            DiffTabPage.ImageKey = nameof(Images.Diff);
            TreeTabPage.ImageKey = nameof(Images.FileTree);
            GpgInfoTabPage.ImageKey = nameof(Images.Key);

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

            _filterRevisionsHelper = new FilterRevisionsHelper(toolStripRevisionFilterTextBox, toolStripRevisionFilterDropDownButton, RevisionGrid, toolStripRevisionFilterLabel, ShowFirstParent, form: this);
            _filterBranchHelper = new FilterBranchHelper(toolStripBranchFilterComboBox, toolStripBranchFilterDropDownButton, RevisionGrid);
            repoObjectsTree.SetBranchFilterer(_filterBranchHelper);
            toolStripBranchFilterComboBox.DropDown += toolStripBranches_DropDown_ResizeDropDownWidth;
            revisionDiff.Bind(RevisionGrid, fileTree);

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

                var repoStateVisualiser = new RepoStateVisualiser();

                _gitStatusMonitor = new GitStatusMonitor();
                _gitStatusMonitor.Init(this);

                _gitStatusMonitor.GitStatusMonitorStateChanged += (s, e) =>
                {
                    var status = e.State;
                    if (status == GitStatusMonitorState.Stopped)
                    {
                        _toolStripGitStatus.Visible = false;
                        _toolStripGitStatus.Text = "";
                        TaskbarManager.Instance.SetOverlayIcon(null, "");
                    }
                    else if (status == GitStatusMonitorState.Running)
                    {
                        _toolStripGitStatus.Visible = true;
                    }
                };

                Brush lastBrush = null;

                _gitStatusMonitor.GitWorkingDirectoryStatusChanged += (s, e) =>
                {
                    var status = e.ItemStatuses.ToList();

                    var (image, brush) = repoStateVisualiser.Invoke(status);

                    _toolStripGitStatus.Image = image;

                    _toolStripGitStatus.Text = countToolbar && status.Count != 0
                        ? string.Format(_commitButtonText + " ({0})", status.Count.ToString())
                        : _commitButtonText.Text;

                    if (countArtificial)
                    {
                        RevisionGrid.UpdateArtificialCommitCount(status);
                    }

                    // The diff filelist is not updated, as the selected diff is unset
                    ////_revisionDiff.RefreshArtificial();

                    if (!ReferenceEquals(brush, lastBrush))
                    {
                        lastBrush = brush;

                        const int imgDim = 32;
                        const int dotDim = 9;
                        const int pad = 2;
                        using (var bmp = new Bitmap(imgDim, imgDim))
                        {
                            using (var g = Graphics.FromImage(bmp))
                            {
                                g.SmoothingMode = SmoothingMode.AntiAlias;
                                g.Clear(Color.Transparent);
                                g.FillEllipse(brush, new Rectangle(imgDim - dotDim - pad, imgDim - dotDim - pad, dotDim, dotDim));
                            }

                            using (var overlay = Icon.FromHandle(bmp.GetHicon()))
                            {
                                TaskbarManager.Instance.SetOverlayIcon(overlay, "");
                            }
                        }
                    }
                };

                // TODO: Replace with a status page?
                _toolStripGitStatus.Click += CommitToolStripMenuItemClick;
                ToolStrip.Items.Insert(ToolStrip.Items.IndexOf(toolStripButtonCommit), _toolStripGitStatus);
                ToolStrip.Items.Remove(toolStripButtonCommit);
            }

            if (!EnvUtils.RunningOnWindows())
            {
                toolStripSeparator6.Visible = false;
                PuTTYToolStripMenuItem.Visible = false;
            }

            RevisionGrid.SelectionChanged += (sender, e) =>
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
            };
            _filterRevisionsHelper.SetFilter(filter);

            HotkeysEnabled = true;
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            UICommandsChanged += (a, e) =>
            {
                var oldCommands = e.OldCommands;
                RefreshPullIcon();
                oldCommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
                UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
                oldCommands.BrowseRepo = null;
                UICommands.BrowseRepo = this;
            };
            if (commands != null)
            {
                RefreshPullIcon();
                UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
                UICommands.BrowseRepo = this;
                _controller = new FormBrowseController(new GitGpgController(() => Module));
                _commitDataManager = new CommitDataManager(() => Module);
            }

            var repositoryDescriptionProvider = new RepositoryDescriptionProvider(new GitDirectoryResolver());
            _appTitleGenerator = new AppTitleGenerator(repositoryDescriptionProvider);
            _windowsJumpListManager = new WindowsJumpListManager(repositoryDescriptionProvider);

            FillBuildReport();  // Ensure correct page visibility
            RevisionGrid.ShowBuildServerInfo = true;

            _formBrowseMenus = new FormBrowseMenus(menuStrip1);
            RevisionGrid.MenuCommands.MenuChanged += (sender, e) => _formBrowseMenus.OnMenuCommandsPropertyChanged();
            SystemEvents.SessionEnding += (sender, args) => SaveApplicationSettings();

            FillTerminalTab();
            ManageWorktreeSupport();

            var toolBackColor = Color.FromArgb(218, 218, 218);
            BackColor = toolBackColor;
            ToolStrip.BackColor = toolBackColor;
            toolStripRevisionFilterDropDownButton.BackColor = toolBackColor;
            menuStrip1.BackColor = toolBackColor;
            toolPanel.TopToolStripPanel.BackColor = toolBackColor;
            statusStrip.BackColor = toolBackColor;

            var toolTextBoxBackColor = Color.FromArgb(235, 235, 235);
            toolStripBranchFilterComboBox.BackColor = toolTextBoxBackColor;
            toolStripRevisionFilterTextBox.BackColor = toolTextBoxBackColor;

            // Scale tool strip items according to DPI
            toolStripBranchFilterComboBox.Size = DpiUtil.Scale(toolStripBranchFilterComboBox.Size);
            toolStripRevisionFilterTextBox.Size = DpiUtil.Scale(toolStripRevisionFilterTextBox.Size);

            if (selectCommit != null)
            {
                RevisionGrid.InitialObjectId = selectCommit;
            }

            InitializeComplete();
            RestorePosition();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _submodulesStatusSequence?.Dispose();
                _formBrowseMenus?.Dispose();
                _filterRevisionsHelper?.Dispose();
                _filterBranchHelper?.Dispose();
                components?.Dispose();
                _gitStatusMonitor?.Dispose();
                _windowsJumpListManager?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            _windowsJumpListManager.CreateJumpList(
                Handle,
                new WindowsThumbnailToolbarButtons(
                    new WindowsThumbnailToolbarButton(toolStripButtonCommit.Text, toolStripButtonCommit.Image, CommitToolStripMenuItemClick),
                    new WindowsThumbnailToolbarButton(toolStripButtonPush.Text, toolStripButtonPush.Image, PushToolStripMenuItemClick),
                    new WindowsThumbnailToolbarButton(toolStripButtonPull.Text, toolStripButtonPull.Image, PullToolStripMenuItemClick)));

            SetSplitterPositions();
            HideVariableMainMenuItems();
            RefreshSplitViewLayout();

            RevisionGrid.Load();
            _filterBranchHelper.InitToolStripBranchFilter();

            LayoutRevisionInfo();
            InternalInitialize(false);
            RevisionGrid.Focus();
            RevisionGrid.IndexWatcher.Reset();

            RevisionGrid.IndexWatcher.Changed += (_, args) =>
            {
                bool indexChanged = args.IsIndexChanged;
                this.InvokeAsync(
                        () =>
                        {
                            RefreshButton.Image = indexChanged && AppSettings.UseFastChecks && Module.IsValidGitWorkingDir()
                                ? Images.ReloadRevisionsDirty
                                : Images.ReloadRevisions;
                        })
                    .FileAndForget();
            };

            base.OnLoad(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            this.InvokeAsyncDoNotUseInNewCode(OnActivate);
            base.OnActivated(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveApplicationSettings();
            base.OnFormClosing(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _splitterManager.SaveSplitters();
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            UnregisterPlugins();
            base.OnClosed(e);
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
                RevisionGrid.ShowAllBranches();
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
            using (WaitCursorScope.Enter())
            {
                RevisionGrid.GoToRef(refName, showNoRevisionMsg);
            }
        }

        #endregion

        private void ShowDashboard()
        {
            toolPanel.SuspendLayout();
            toolPanel.TopToolStripPanelVisible = false;
            toolPanel.BottomToolStripPanelVisible = false;
            toolPanel.LeftToolStripPanelVisible = false;
            toolPanel.RightToolStripPanelVisible = false;
            toolPanel.ResumeLayout();

            MainSplitContainer.Visible = false;

            if (_dashboard == null)
            {
                _dashboard = new Dashboard { Dock = DockStyle.Fill };
                _dashboard.GitModuleChanged += SetGitModule;
                toolPanel.ContentPanel.Controls.Add(_dashboard);
            }

            Text = _appTitleGenerator.Generate();

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
            toolPanel.SuspendLayout();
            toolPanel.TopToolStripPanelVisible = true;
            toolPanel.BottomToolStripPanelVisible = true;
            toolPanel.LeftToolStripPanelVisible = true;
            toolPanel.RightToolStripPanelVisible = true;
            toolPanel.ResumeLayout();
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
            var existingPluginMenus = pluginsToolStripMenuItem.DropDownItems.OfType<ToolStripMenuItem>().ToLookup(c => c.Tag);

            foreach (var plugin in PluginRegistry.Plugins)
            {
                // Add the plugin to the Plugins menu, if not already added
                if (!existingPluginMenus.Contains(plugin))
                {
                    var item = new ToolStripMenuItem { Text = plugin.Description, Image = plugin.Icon, Tag = plugin };
                    item.Click += delegate
                    {
                        if (plugin.Execute(new GitUIEventArgs(this, UICommands)))
                        {
                            RefreshRevisions();
                        }
                    };
                    pluginsToolStripMenuItem.DropDownItems.Insert(pluginsToolStripMenuItem.DropDownItems.Count - 2, item);
                }

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

            UpdatePluginMenu(Module?.IsValidGitWorkingDir() ?? false);
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
                toolStripButtonCommit.Enabled = validBrowseDir && !bareRepository;
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
                editGitAttributesToolStripMenuItem.Enabled = validBrowseDir;
                editmailmapToolStripMenuItem.Enabled = validBrowseDir;
                toolStripSplitStash.Enabled = validBrowseDir && !bareRepository;
                _createPullRequestsToolStripMenuItem.Enabled = validBrowseDir;
                _viewPullRequestsToolStripMenuItem.Enabled = validBrowseDir;

                _filterBranchHelper.InitToolStripBranchFilter();

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
                var branchName = !string.IsNullOrEmpty(branchSelect.Text) ? branchSelect.Text : _noBranchTitle.Text;
                Text = _appTitleGenerator.Generate(Module?.WorkingDir, validBrowseDir, branchName);

                OnActivate();

                // load custom user menu
                LoadUserMenu();
                ReloadRepoObjectsTree();

                if (validBrowseDir)
                {
                    _windowsJumpListManager.AddToRecent(Module.WorkingDir);

                    // add Navigate and View menu
                    _formBrowseMenus.ResetMenuCommandSets();
                    _formBrowseMenus.AddMenuCommandSet(MainMenuItem.NavigateMenu, RevisionGrid.MenuCommands.NavigateMenuCommands);
                    _formBrowseMenus.AddMenuCommandSet(MainMenuItem.ViewMenu, RevisionGrid.MenuCommands.ViewMenuCommands);

                    _formBrowseMenus.InsertAdditionalMainMenuItems(repositoryToolStripMenuItem);
                }
                else
                {
                    _windowsJumpListManager.DisableThumbnailToolbar();
                }

                UICommands.RaisePostBrowseInitialize(this);
            }

            toolPanel.ResumeLayout();
        }

        private void ReloadRepoObjectsTree()
        {
            if (MainSplitContainer.Panel1Collapsed)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(() => repoObjectsTree.ReloadAsync()).FileAndForget();
        }

        private void OnActivate()
        {
            CheckForMergeConflicts();
            UpdateStashCount();
            UpdateSubmodulesList();
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

            var mostRecentRepos = new List<RecentRepoInfo>();
            using (var graphics = CreateGraphics())
            {
                var splitter = new RecentRepoSplitter
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
        }

        private void WorkingDirDropDownOpening(object sender, EventArgs e)
        {
            _NO_TRANSLATE_WorkingDir.DropDownItems.Clear();

            var tsmiCategorisedRepos = new ToolStripMenuItem(tsmiFavouriteRepositories.Text, tsmiFavouriteRepositories.Image);
            PopulateFavouriteRepositoriesMenu(tsmiCategorisedRepos);
            if (tsmiCategorisedRepos.DropDownItems.Count > 0)
            {
                _NO_TRANSLATE_WorkingDir.DropDownItems.Add(tsmiCategorisedRepos);
            }

            PopulateRecentRepositoriesMenu(_NO_TRANSLATE_WorkingDir);

            _NO_TRANSLATE_WorkingDir.DropDownItems.Add(new ToolStripSeparator());

            var mnuOpenLocalRepository = new ToolStripMenuItem(openToolStripMenuItem.Text, openToolStripMenuItem.Image) { ShortcutKeys = openToolStripMenuItem.ShortcutKeys };
            mnuOpenLocalRepository.Click += OpenToolStripMenuItemClick;
            _NO_TRANSLATE_WorkingDir.DropDownItems.Add(mnuOpenLocalRepository);

            var mnuRecentReposSettings = new ToolStripMenuItem(_configureWorkingDirMenu.Text);
            mnuRecentReposSettings.Click += (hs, he) =>
            {
                using (var frm = new FormRecentReposSettings())
                {
                    frm.ShowDialog(this);
                }

                RefreshWorkingDirComboText();
            };
            _NO_TRANSLATE_WorkingDir.DropDownItems.Add(mnuRecentReposSettings);

            PreventToolStripSplitButtonClosing((ToolStripSplitButton)sender);
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

        private void LoadUserMenu()
        {
            var scripts = ScriptManager.GetScripts()
                .Where(script => script.Enabled && script.OnEvent == ScriptEvent.ShowInUserMenuBar)
                .ToList();

            for (int i = ToolStrip.Items.Count - 1; i >= 0; i--)
            {
                if (ToolStrip.Items[i].Tag as string == "userscript")
                {
                    ToolStrip.Items.RemoveAt(i);
                }
            }

            if (scripts.Count == 0)
            {
                return;
            }

            ToolStrip.Items.Add(new ToolStripSeparator { Tag = "userscript" });

            foreach (var script in scripts)
            {
                var button = new ToolStripButton
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
                    if (ScriptRunner.RunScript(this, Module, script.Name, RevisionGrid))
                    {
                        RevisionGrid.RefreshRevisions();
                    }
                };

                // add to toolstrip
                ToolStrip.Items.Add(button);
            }
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
            if (_selectedRevisionUpdatedTargets.HasFlag(UpdateTargets.CommitInfo))
            {
                return;
            }

            if (!AppSettings.ShowRevisionInfoNextToRevisionGrid && CommitInfoTabControl.SelectedTab != CommitInfoTabPage)
            {
                return;
            }

            _selectedRevisionUpdatedTargets |= UpdateTargets.CommitInfo;

            var selectedRevisions = RevisionGrid.GetSelectedRevisions();

            if (selectedRevisions.Count == 0)
            {
                return;
            }

            var revision = selectedRevisions[0];

            var children = RevisionGrid.GetRevisionChildren(revision.ObjectId);
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
            UICommands.StartPushDialog(this, pushOnShow: ModifierKeys.HasFlag(Keys.Shift));
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
            using (var frm = new FormAbout())
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
            FormGitCommandLog.ShowOrActivate(this);
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

        private void OnShowSettingsClick(object sender, EventArgs e)
        {
            var translation = AppSettings.Translation;
            var showRevisionInfoNextToRevisionGrid = AppSettings.ShowRevisionInfoNextToRevisionGrid;

            UICommands.StartSettingsDialog(this);

            if (translation != AppSettings.Translation)
            {
                Translator.Translate(this, AppSettings.CurrentTranslation);
            }

            if (showRevisionInfoNextToRevisionGrid != AppSettings.ShowRevisionInfoNextToRevisionGrid)
            {
                LayoutRevisionInfo();
            }

            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            RevisionGrid.ReloadHotkeys();
            RevisionGrid.ReloadTranslation();
            fileTree.ReloadHotkeys();
            revisionDiff.ReloadHotkeys();

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

            if (revisions.Count == 2)
            {
                string to = null;
                string from = null;

                string currentBranch = Module.GetSelectedBranch();
                var currentCheckout = RevisionGrid.CurrentCheckout;

                if (revisions[0].ObjectId == currentCheckout)
                {
                    from = revisions[1].ObjectId.ToShortString(8);
                    to = currentBranch;
                }
                else if (revisions[1].ObjectId == currentCheckout)
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
                                                        MessageBoxButtons.YesNo,
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

        private void UserManualToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://git-extensions-documentation.readthedocs.org/en/release-2.51/");
            }
            catch (Win32Exception)
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

                foreach (var r in repos)
                {
                    var item = new ToolStripMenuItem(r.Caption ?? "")
                    {
                        DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                    };
                    menuItemCategory.DropDownItems.Add(item);

                    item.Click += delegate { ChangeWorkingDir(r.Repo.Path); };

                    if (r.Repo.Path != r.Caption)
                    {
                        item.ToolTipText = r.Repo.Path;
                    }
                }
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

                if (repo.Path != caption)
                {
                    item.ToolTipText = repo.Path;
                }
            }
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
            RevisionGrid.InvalidateCount();

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

        private void CreateBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCreateBranchDialog(this, RevisionGrid.GetSelectedRevisions().FirstOrDefault()?.ObjectId);
        }

        private void GitBashClick(object sender, EventArgs e)
        {
            GitBashToolStripMenuItemClick1(sender, e);
        }

        private void ToolStripButtonPullClick(object sender, EventArgs e)
        {
            PullToolStripMenuItemClick(sender, e);
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

        private void deleteIndexLockToolStripMenuItem_Click(object sender, EventArgs e)
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

            void AddCheckoutBranchMenuItem()
            {
                var checkoutBranchItem = new ToolStripMenuItem(checkoutBranchToolStripMenuItem.Text)
                {
                    ShortcutKeys = checkoutBranchToolStripMenuItem.ShortcutKeys,
                    ShortcutKeyDisplayString = checkoutBranchToolStripMenuItem.ShortcutKeyDisplayString
                };
                branchSelect.DropDownItems.Add(checkoutBranchItem);
                checkoutBranchItem.Click += CheckoutBranchToolStripMenuItemClick;
            }

            void AddBranchesMenuItems()
            {
                foreach (var branchName in GetBranchNames())
                {
                    var toolStripItem = branchSelect.DropDownItems.Add(branchName);
                    toolStripItem.Click
                        += delegate { UICommands.StartCheckoutBranch(this, toolStripItem.Text); };
                }

                IEnumerable<string> GetBranchNames()
                {
                    // Make sure there are never more than a 100 branches added to the menu
                    // GitExtensions will hang when the drop down is too large...
                    return Module
                        .GetRefs(tags: false)
                        .Select(b => b.Name)
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
            Commit = 7,
            AddNotes = 8,
            FindFileInSelectedCommit = 9,
            CheckoutBranch = 10,
            QuickFetch = 11,
            QuickPull = 12,
            QuickPush = 13,
            /* deprecated: RotateApplicationIcon = 14, */
            CloseRepository = 15,
            Stash = 16,
            StashPop = 17,
            FocusFilter = 18,
            OpenWithDifftool = 19,
            OpenSettings = 20,
            ToggleBranchTreePanel = 21
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
            settingsToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Commands.OpenSettings).ToShortcutKeyDisplayString();

            // TODO: add more
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

            AppSettings.ShowSplitViewLayout = true;
            RefreshSplitViewLayout();

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
                revisionDiff.ExecuteCommand(RevisionDiffControl.Command.OpenWithDifftool);
            }
            else if (fileTree.Visible)
            {
                fileTree.ExecuteCommand(RevisionFileTreeControl.Command.OpenWithDifftool);
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
                case Commands.CloseRepository: CloseToolStripMenuItemClick(null, null); break;
                case Commands.Stash: UICommands.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash); break;
                case Commands.StashPop: UICommands.StashPop(this); break;
                case Commands.OpenWithDifftool: OpenWithDifftool(); break;
                case Commands.OpenSettings: OnShowSettingsClick(null, null); break;
                case Commands.ToggleBranchTreePanel: toggleBranchTreePanel_Click(null, null); break;
                default: return base.ExecuteCommand(cmd);
            }

            return true;
        }

        internal bool ExecuteCommand(Commands cmd)
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
                string filePath = Path.Combine(module.WorkingDir, item.Name.ToNativePath());
                FormBrowseUtil.ShowFileOrParentFolderInFileExplorer(filePath);
            }
        }

        private void SetSplitterPositions()
        {
            _splitterManager.AddSplitter(RevisionsSplitContainer, nameof(RevisionsSplitContainer));
            _splitterManager.AddSplitter(MainSplitContainer, nameof(MainSplitContainer));
            _splitterManager.AddSplitter(RightSplitContainer, nameof(RightSplitContainer));

            revisionDiff.InitSplitterManager(_splitterManager);
            fileTree.InitSplitterManager(_splitterManager);

            // hide status in order to restore splitters against the full height (the most common case)
            statusStrip.Hide();
            _splitterManager.RestoreSplitters();
            RefreshLayoutToggleButtonStates();
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

        private void dontSetAsDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.SetNextPullActionAsDefault = !setNextPullActionAsDefaultToolStripMenuItem.Checked;
            setNextPullActionAsDefaultToolStripMenuItem.Checked = AppSettings.SetNextPullActionAsDefault;
        }

        private void DoPullAction(Action action)
        {
            var actLastPullAction = Module.LastPullAction;
            try
            {
                action();
            }
            finally
            {
                if (!AppSettings.SetNextPullActionAsDefault)
                {
                    Module.LastPullAction = actLastPullAction;
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
                    toolStripButtonPull.Image = Images.PullFetch;
                    toolStripButtonPull.ToolTipText = _pullFetch.Text;
                    break;

                case AppSettings.PullAction.FetchAll:
                    toolStripButtonPull.Image = Images.PullFetchAll;
                    toolStripButtonPull.ToolTipText = _pullFetchAll.Text;
                    break;

                case AppSettings.PullAction.Merge:
                    toolStripButtonPull.Image = Images.PullMerge;
                    toolStripButtonPull.ToolTipText = _pullMerge.Text;
                    break;

                case AppSettings.PullAction.Rebase:
                    toolStripButtonPull.Image = Images.PullRebase;
                    toolStripButtonPull.ToolTipText = _pullRebase.Text;
                    break;

                default:
                    toolStripButtonPull.Image = Images.Pull;
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

        private void branchSelect_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                CheckoutBranchToolStripMenuItemClick(sender, e);
            }
        }

        private void RevisionInfo_CommandClick(object sender, CommitInfo.CommandEventArgs e)
        {
            // TODO this code duplicated in FormFileHistory.Blame_CommandClick
            switch (e.Command)
            {
                case "gotocommit":
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
                        _filterBranchHelper.SetBranchFilter(revision?.ToString(), refresh: true);
                        RevisionGrid.SetSelectedRevision(revision);
                    }

                    break;
                case "gotobranch":
                case "gototag":
                    CommitData commit = _commitDataManager.GetCommitData(e.Data, out _);
                    if (commit != null)
                    {
                        RevisionGrid.SetSelectedRevision(new GitRevision(commit.ObjectId));
                    }

                    break;
                case "navigatebackward":
                    RevisionGrid.NavigateBackward();
                    break;
                case "navigateforward":
                    RevisionGrid.NavigateForward();
                    break;
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

        #region Submodules

        private ToolStripMenuItem CreateSubmoduleMenuItem(SubmoduleInfo info, string textFormat = "{0}")
        {
            var item = new ToolStripMenuItem(string.Format(textFormat, info.Text))
            {
                Width = 200,
                Tag = info.Path,
                Image = GetSubmoduleItemImage()
            };

            if (info.Bold)
            {
                item.Font = new Font(item.Font, FontStyle.Bold);
            }

            item.Click += SubmoduleToolStripButtonClick;

            return item;

            Image GetSubmoduleItemImage()
            {
                if (info.Status == null)
                {
                    return Images.FolderSubmodule;
                }

                if (info.Status == SubmoduleStatus.FastForward)
                {
                    return info.IsDirty ? Images.SubmoduleRevisionUpDirty : Images.SubmoduleRevisionUp;
                }

                if (info.Status == SubmoduleStatus.Rewind)
                {
                    return info.IsDirty ? Images.SubmoduleRevisionDownDirty : Images.SubmoduleRevisionDown;
                }

                if (info.Status == SubmoduleStatus.NewerTime)
                {
                    return info.IsDirty ? Images.SubmoduleRevisionSemiUpDirty : Images.SubmoduleRevisionSemiUp;
                }

                if (info.Status == SubmoduleStatus.OlderTime)
                {
                    return info.IsDirty ? Images.SubmoduleRevisionSemiDownDirty : Images.SubmoduleRevisionSemiDown;
                }

                return info.IsDirty ? Images.SubmoduleDirty : Images.FileStatusModified;
            }
        }

        private DateTime _previousSubmoduleUpdateTime;

        private void LoadSubmodulesIntoDropDownMenu()
        {
            TimeSpan elapsed = DateTime.Now - _previousSubmoduleUpdateTime;
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
            _previousSubmoduleUpdateTime = DateTime.Now;

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
                var threadModule = new GitModule(thisModuleDir);
                var result = new SubmoduleInfoResult();

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
                            var localPath = threadModule.SuperprojectModule.WorkingDir.Substring(supersuperproject.WorkingDir.Length);
                            localPath = PathUtil.GetDirectoryName(localPath.ToPosixPath());
                            name = localPath;
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
                        string localPath = threadModule.WorkingDir.Substring(supersuperproject.WorkingDir.Length);
                        localPath = PathUtil.GetDirectoryName(localPath.ToPosixPath());

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
                            if (submodule == localPath)
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

                var newItems = result.OurSubmodules
                    .Select(submodule => CreateSubmoduleMenuItem(submodule))
                    .ToList<ToolStripItem>();

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
                    newItems.AddRange(result.SuperSubmodules.Select(submodule => CreateSubmoduleMenuItem(submodule)));
                }

                newItems.Add(new ToolStripSeparator());

                var mi = new ToolStripMenuItem(updateAllSubmodulesToolStripMenuItem.Text);
                mi.Click += UpdateAllSubmodulesToolStripMenuItemClick;
                newItems.Add(mi);

                if (result.CurrentSubmoduleName != null)
                {
                    var item = new ToolStripMenuItem(_updateCurrentSubmodule.Text) { Tag = result.CurrentSubmoduleName };
                    item.Click += UpdateSubmoduleToolStripMenuItemClick;
                    newItems.Add(item);
                }

                // Using AddRange is critical: if you used Add to add menu items one at a
                // time, performance would be extremely slow with many submodules (> 100).
                toolStripButtonLevelUp.DropDownItems.AddRange(newItems.ToArray());

                _previousSubmoduleUpdateTime = DateTime.Now;
            });

            void RemoveSubmoduleButtons()
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

            string GetModuleBranch(string path)
            {
                var branch = GitModule.GetSelectedBranchFast(path);
                var text = DetachedHeadParser.IsDetachedHead(branch) ? _noBranchTitle.Text : branch;
                return $"[{text}]";
            }
        }

        #endregion

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

            var tabPageCaption = _consoleTabCaption.Text;
            var tabPageCreated = CommitInfoTabControl.TabPages.ContainsKey(tabPageCaption);
            TabPage tabPage;
            if (tabPageCreated)
            {
                tabPage = CommitInfoTabControl.TabPages[tabPageCaption];
            }
            else
            {
                const string imageKey = "Resources.IconConsole";
                CommitInfoTabControl.ImageList.Images.Add(imageKey, Images.Console);
                CommitInfoTabControl.Controls.Add(tabPage = new TabPage(tabPageCaption));
                tabPage.Name = tabPageCaption;
                tabPage.ImageKey = imageKey;
            }

            // Delay-create the terminal window when the tab is first selected
            CommitInfoTabControl.Selecting += (sender, args) =>
            {
                if (args.TabPage != tabPage)
                {
                    return;
                }

                if (_terminal == null)
                {
                    // Lazy-create on first opening the tab
                    tabPage.Controls.Clear();
                    tabPage.Controls.Add(
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
                var startInfo = new ConEmuStartInfo
                {
                    StartupDirectory = Module.WorkingDir,
                    WhenConsoleProcessExits = WhenConsoleProcessExits.CloseConsoleEmulator
                };

                var startInfoBaseConfiguration = startInfo.BaseConfiguration;
                if (!string.IsNullOrWhiteSpace(AppSettings.ConEmuFontSize.ValueOrDefault))
                {
                    if (int.TryParse(AppSettings.ConEmuFontSize.ValueOrDefault, out var fontSize))
                    {
                        var nodeFontSize =
                            startInfoBaseConfiguration.SelectSingleNode("/key/key/key/value[@name='FontSize']");
                        if (nodeFontSize?.Attributes != null)
                        {
                            nodeFontSize.Attributes["data"].Value = fontSize.ToString("X8");
                        }
                    }
                }

                startInfo.BaseConfiguration = startInfoBaseConfiguration;

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
                    startInfo.ConsoleProcessCommandLine = ConEmuConstants.DefaultConsoleCommandLine;
                }
                else
                {
                    cmdPath = cmdPath.Quote();
                    if (AppSettings.ConEmuTerminal.ValueOrDefault == "bash")
                    {
                        startInfo.ConsoleProcessCommandLine = cmdPath + " --login -i";
                    }
                    else
                    {
                        startInfo.ConsoleProcessCommandLine = cmdPath;
                    }
                }

                if (AppSettings.ConEmuStyle.ValueOrDefault != "Default")
                {
                    startInfo.ConsoleProcessExtraArgs = " -new_console:P:\"" + AppSettings.ConEmuStyle.ValueOrDefault + "\"";
                }

                // Set path to git in this window (actually, effective with CMD only)
                if (!string.IsNullOrEmpty(AppSettings.GitCommandValue))
                {
                    string dirGit = Path.GetDirectoryName(AppSettings.GitCommandValue);
                    if (!string.IsNullOrEmpty(dirGit))
                    {
                        startInfo.SetEnv("PATH", dirGit + ";" + "%PATH%");
                    }
                }

                _terminal.Start(startInfo, ThreadHelper.JoinableTaskFactory);
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

        private void menuitemSparseWorkingCopy_Click(object sender, EventArgs e)
        {
            UICommands.StartSparseWorkingCopyDialog(this);
        }

        private void toolStripBranches_DropDown_ResizeDropDownWidth(object sender, EventArgs e)
        {
            toolStripBranchFilterComboBox.ComboBox.ResizeDropDownWidth(AppSettings.BranchDropDownMinWidth, AppSettings.BranchDropDownMaxWidth);
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

        #region Layout management

        private void toggleSplitViewLayout_Click(object sender, EventArgs e)
        {
            AppSettings.ShowSplitViewLayout = !AppSettings.ShowSplitViewLayout;
            RefreshSplitViewLayout();
        }

        private void toggleBranchTreePanel_Click(object sender, EventArgs e)
        {
            MainSplitContainer.Panel1Collapsed = !MainSplitContainer.Panel1Collapsed;
            ReloadRepoObjectsTree();
            RefreshLayoutToggleButtonStates();
        }

        private void toggleCommitInfoOnRight_Click(object sender, EventArgs e)
        {
            AppSettings.ShowRevisionInfoNextToRevisionGrid = !AppSettings.ShowRevisionInfoNextToRevisionGrid;
            LayoutRevisionInfo();
            RefreshLayoutToggleButtonStates();
        }

        private void RefreshSplitViewLayout()
        {
            RightSplitContainer.Panel2Collapsed = !AppSettings.ShowSplitViewLayout;
            RefreshLayoutToggleButtonStates();
        }

        private void RefreshLayoutToggleButtonStates()
        {
            toggleBranchTreePanel.Checked = !MainSplitContainer.Panel1Collapsed;
            toggleSplitViewLayout.Checked = AppSettings.ShowSplitViewLayout;
            toggleCommitInfoOnRight.Checked = AppSettings.ShowRevisionInfoNextToRevisionGrid;
        }

        private void LayoutRevisionInfo()
        {
            // Handle must be created prior to insertion
            _ = CommitInfoTabControl.Handle;

            RevisionInfo.SuspendLayout();
            CommitInfoTabControl.SuspendLayout();
            RevisionsSplitContainer.SuspendLayout();

            var isRight = AppSettings.ShowRevisionInfoNextToRevisionGrid;

            RevisionInfo.SetAvatarPosition(isRight);

            if (isRight)
            {
                RevisionInfo.Parent = RevisionsSplitContainer.Panel2;
                RevisionsSplitContainer.SplitterDistance = RevisionsSplitContainer.Width - DpiUtil.Scale(420);
                CommitInfoTabControl.RemoveIfExists(CommitInfoTabPage);
            }
            else
            {
                RevisionInfo.Parent = CommitInfoTabPage;
                CommitInfoTabControl.InsertIfNotExists(0, CommitInfoTabPage);
                CommitInfoTabControl.SelectedTab = CommitInfoTabPage;
            }

            RevisionsSplitContainer.Panel2Collapsed = !isRight;

            RevisionInfo.ResumeLayout(performLayout: true);
            CommitInfoTabControl.ResumeLayout(performLayout: true);
            RevisionsSplitContainer.ResumeLayout(performLayout: true);
        }

        #endregion

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
