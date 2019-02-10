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
using GitCommands.Submodules;
using GitCommands.UserRepositoryHistory;
using GitCommands.Utils;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.BranchTreePanel;
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

        private readonly TranslationString _warningMiddleOfBisect = new TranslationString("You are in the middle of a bisect");
        private readonly TranslationString _warningMiddleOfRebase = new TranslationString("You are in the middle of a rebase");
        private readonly TranslationString _warningMiddleOfPatchApply = new TranslationString("You are in the middle of a patch apply");

        private readonly TranslationString _hintUnresolvedMergeConflicts = new TranslationString("There are unresolved merge conflicts!");

        private readonly TranslationString _noBranchTitle = new TranslationString("no branch");
        private readonly TranslationString _noSubmodulesPresent = new TranslationString("No submodules");
        private readonly TranslationString _topProjectModuleFormat = new TranslationString("Top project: {0}");
        private readonly TranslationString _superprojectModuleFormat = new TranslationString("Superproject: {0}");
        private readonly TranslationString _goToSuperProject = new TranslationString("Go to superproject");

        private readonly TranslationString _indexLockCantDelete = new TranslationString("Failed to delete index.lock.");

        private readonly TranslationString _errorCaption = new TranslationString("Error");
        private readonly TranslationString _loading = new TranslationString("Loading...");

        private readonly TranslationString _noReposHostPluginLoaded = new TranslationString("No repository host plugin loaded.");
        private readonly TranslationString _noReposHostFound = new TranslationString("Could not find any relevant repository hosts for the currently open repository.");

        private readonly TranslationString _configureWorkingDirMenu = new TranslationString("Configure this menu");

        private readonly TranslationString _directoryIsNotAValidRepositoryCaption = new TranslationString("Open");
        private readonly TranslationString _directoryIsNotAValidRepositoryMainInstruction = new TranslationString("The selected item is not a valid git repository.");
        private readonly TranslationString _directoryIsNotAValidRepositoryRemoveSelectedRepoCommand = new TranslationString("Remove the selected invalid repository");
        private readonly TranslationString _directoryIsNotAValidRepositoryRemoveAllCommand = new TranslationString("Remove all {0} invalid repositories");

        private readonly TranslationString _updateCurrentSubmodule = new TranslationString("Update current submodule");

        private readonly TranslationString _pullFetch = new TranslationString("Fetch");
        private readonly TranslationString _pullFetchAll = new TranslationString("Fetch all");
        private readonly TranslationString _pullFetchPruneAll = new TranslationString("Fetch and prune all");
        private readonly TranslationString _pullMerge = new TranslationString("Pull - merge");
        private readonly TranslationString _pullRebase = new TranslationString("Pull - rebase");
        private readonly TranslationString _pullOpenDialog = new TranslationString("Open pull dialog");
        private readonly TranslationString _pullFetchPruneAllConfirmation = new TranslationString("Warning! The fetch with prune will remove all the remote-tracking references which no longer exist on remotes. Do you want to proceed?");

        private readonly TranslationString _buildReportTabCaption = new TranslationString("Build Report");
        private readonly TranslationString _consoleTabCaption = new TranslationString("Console");

        private readonly TranslationString _noWorkingFolderText = new TranslationString("No working directory");
        private readonly TranslationString _commitButtonText = new TranslationString("Commit");

        private readonly TranslationString _undoLastCommitText = new TranslationString("You will still be able to find all the commit's changes in the staging area\n\nDo you want to continue?");
        private readonly TranslationString _undoLastCommitCaption = new TranslationString("Undo last commit");
        #endregion

        private readonly SplitterManager _splitterManager = new SplitterManager(new AppSettingsPath("FormBrowse"));
        [NotNull]
        private readonly GitStatusMonitor _gitStatusMonitor;
        private readonly FilterRevisionsHelper _filterRevisionsHelper;
        private readonly FilterBranchHelper _filterBranchHelper;
        private readonly FormBrowseMenus _formBrowseMenus;
        private readonly IFormBrowseController _controller;
        private readonly ICommitDataManager _commitDataManager;
        private readonly IAppTitleGenerator _appTitleGenerator;
        [CanBeNull] private readonly IAheadBehindDataProvider _aheadBehindDataProvider;
        private readonly WindowsJumpListManager _windowsJumpListManager;
        private readonly SubmoduleStatusProvider _submoduleStatusProvider;

        [CanBeNull] private BuildReportTabPageExtension _buildReportTabPageExtension;
        private ConEmuControl _terminal;
        private Dashboard _dashboard;
        private ToolStripItem _rebase;
        private ToolStripItem _bisect;
        private ToolStripItem _warning;

        [CanBeNull] private TabPage _consoleTabPage;

        [Flags]
        private enum UpdateTargets
        {
            None = 1,
            DiffList = 2,
            FileTree = 4,
            CommitInfo = 8
        }

        private UpdateTargets _selectedRevisionUpdatedTargets = UpdateTargets.None;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormBrowse()
        {
            InitializeComponent();
            InitializeComplete();
        }

        public FormBrowse([NotNull] GitUICommands commands, string filter, ObjectId selectCommit = null)
            : base(commands)
        {
            InitializeComponent();

            commandsToolStripMenuItem.DropDownOpening += CommandsToolStripMenuItem_DropDownOpening;

            MainSplitContainer.Visible = false;
            MainSplitContainer.SplitterDistance = DpiUtil.Scale(260);

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
                    { nameof(Images.Key), Images.Key },
                    { nameof(Images.Console), Images.Console }
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

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;
                PluginRegistry.Initialize();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                RegisterPlugins();
            }).FileAndForget();

            _filterRevisionsHelper = new FilterRevisionsHelper(toolStripRevisionFilterTextBox, toolStripRevisionFilterDropDownButton, RevisionGrid, toolStripRevisionFilterLabel, ShowFirstParent, form: this);
            _filterBranchHelper = new FilterBranchHelper(toolStripBranchFilterComboBox, toolStripBranchFilterDropDownButton, RevisionGrid);
            _aheadBehindDataProvider = GitVersion.Current.SupportAheadBehindData ? new AheadBehindDataProvider(() => Module.GitExecutable) : null;

            repoObjectsTree.Initialize(_aheadBehindDataProvider, _filterBranchHelper);
            toolStripBranchFilterComboBox.DropDown += toolStripBranches_DropDown_ResizeDropDownWidth;
            revisionDiff.Bind(RevisionGrid, fileTree);

            var repositoryDescriptionProvider = new RepositoryDescriptionProvider(new GitDirectoryResolver());
            _appTitleGenerator = new AppTitleGenerator(repositoryDescriptionProvider);
            _windowsJumpListManager = new WindowsJumpListManager(repositoryDescriptionProvider);
            _windowsJumpListManager.CreateJumpList(
                Handle,
                new WindowsThumbnailToolbarButtons(
                    new WindowsThumbnailToolbarButton(toolStripButtonCommit.Text, toolStripButtonCommit.Image, CommitToolStripMenuItemClick),
                    new WindowsThumbnailToolbarButton(toolStripButtonPush.Text, toolStripButtonPush.Image, PushToolStripMenuItemClick),
                    new WindowsThumbnailToolbarButton(toolStripButtonPull.Text, toolStripButtonPull.Image, PullToolStripMenuItemClick)));

            InitCountArtificial(out _gitStatusMonitor);

            if (!EnvUtils.RunningOnWindows())
            {
                toolStripSeparator6.Visible = false;
                PuTTYToolStripMenuItem.Visible = false;
            }

            RevisionGrid.SelectionChanged += (sender, e) =>
            {
                _selectedRevisionUpdatedTargets = UpdateTargets.None;

                FillFileTree();
                FillDiff();
                FillCommitInfo();
                ThreadHelper.JoinableTaskFactory.RunAsync(() => FillGpgInfoAsync());
                FillBuildReport();
            };

            _filterRevisionsHelper.SetFilter(filter);

            HotkeysEnabled = true;
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            UICommandsChanged += (a, e) =>
            {
                var oldCommands = e.OldCommands;
                RefreshDefaultPullAction();
                oldCommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
                UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
                oldCommands.BrowseRepo = null;
                UICommands.BrowseRepo = this;
            };

            pullToolStripMenuItem1.Tag = AppSettings.PullAction.None;
            mergeToolStripMenuItem.Tag = AppSettings.PullAction.Merge;
            rebaseToolStripMenuItem1.Tag = AppSettings.PullAction.Rebase;
            fetchToolStripMenuItem.Tag = AppSettings.PullAction.Fetch;
            fetchAllToolStripMenuItem.Tag = AppSettings.PullAction.FetchAll;
            fetchPruneAllToolStripMenuItem.Tag = AppSettings.PullAction.FetchPruneAll;

            FillNextPullActionAsDefaultToolStripMenuItems();
            RefreshDefaultPullAction();
            UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
            UICommands.BrowseRepo = this;
            _controller = new FormBrowseController(new GitGpgController(() => Module));
            _commitDataManager = new CommitDataManager(() => Module);
            _submoduleStatusProvider = new SubmoduleStatusProvider();

            FillBuildReport(); // Ensure correct page visibility
            RevisionGrid.ShowBuildServerInfo = true;

            _formBrowseMenus = new FormBrowseMenus(menuStrip1);
            RevisionGrid.MenuCommands.MenuChanged += (sender, e) => _formBrowseMenus.OnMenuCommandsPropertyChanged();
            SystemEvents.SessionEnding += (sender, args) => SaveApplicationSettings();

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
            UpdateCommitButtonAndGetBrush(null, AppSettings.ShowGitStatusInBrowseToolbar);
            RestorePosition();

            // Populate terminal tab after translation within InitializeComplete
            FillTerminalTab();

            return;

            void ManageWorktreeSupport()
            {
                if (!GitVersion.Current.SupportWorktree)
                {
                    createWorktreeToolStripMenuItem.Enabled = false;
                }

                if (!GitVersion.Current.SupportWorktreeList)
                {
                    manageWorktreeToolStripMenuItem.Enabled = false;
                }
            }

            void InitCountArtificial(out GitStatusMonitor gitStatusMonitor)
            {
                Brush lastBrush = null;

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
                        TaskbarManager.Instance.SetOverlayIcon(null, "");
                        lastBrush = null;
                    }
                };

                gitStatusMonitor.GitWorkingDirectoryStatusChanged += (s, e) =>
                {
                    var status = e.ItemStatuses?.ToList();

                    bool countToolbar = AppSettings.ShowGitStatusInBrowseToolbar;
                    bool countArtificial = AppSettings.ShowGitStatusForArtificialCommits && AppSettings.RevisionGraphShowWorkingDirChanges;

                    var brush = UpdateCommitButtonAndGetBrush(status, countToolbar);

                    RevisionGrid.UpdateArtificialCommitCount(countArtificial ? status : null);

                    // The diff filelist is not updated, as the selected diff is unset
                    ////_revisionDiff.RefreshArtificial();

                    if (countToolbar || countArtificial)
                    {
                        if (!ReferenceEquals(brush, lastBrush))
                        {
                            lastBrush = brush;

                            const int imgDim = 32;
                            const int dotDim = 15;
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

                        if (AppSettings.ShowSubmoduleStatus)
                        {
                            if (_submoduleStatusProvider.HasChangedToNone(status))
                            {
                                UpdateSubmodulesStructure(updateStatus: false);
                            }
                            else if (_submoduleStatusProvider.HasStatusChanges(status))
                            {
                                UpdateSubmodulesStructure(updateStatus: true);
                            }
                        }
                    }
                };
            }

            Brush UpdateCommitButtonAndGetBrush(IReadOnlyList<GitItemStatus> status, bool showCount)
            {
                var repoStateVisualiser = new RepoStateVisualiser();
                var (image, brush) = repoStateVisualiser.Invoke(status);

                if (showCount)
                {
                    toolStripButtonCommit.Image = image;

                    if (status != null)
                    {
                        toolStripButtonCommit.Text = string.Format("{0} ({1})", _commitButtonText, status.Count);
                        toolStripButtonCommit.AutoSize = true;
                    }
                    else
                    {
                        int width = toolStripButtonCommit.Width;
                        toolStripButtonCommit.Text = _commitButtonText.Text;
                        if (width > toolStripButtonCommit.Width)
                        {
                            toolStripButtonCommit.AutoSize = false;
                            toolStripButtonCommit.Width = width;
                        }
                    }
                }
                else
                {
                    toolStripButtonCommit.Image = repoStateVisualiser.Invoke(new List<GitItemStatus>()).Item1;

                    toolStripButtonCommit.Text = _commitButtonText.Text;
                    toolStripButtonCommit.AutoSize = true;
                }

                return brush;
            }
        }

        private void FillNextPullActionAsDefaultToolStripMenuItems()
        {
            var setDefaultPullActionDropDown = (ToolStripDropDownMenu)setDefaultPullButtonActionToolStripMenuItem.DropDown;

            // Show both Check and Image margins in a menu
            setDefaultPullActionDropDown.ShowImageMargin = true;
            setDefaultPullActionDropDown.ShowCheckMargin = true;

            // Prevent submenu from closing while options are changed
            setDefaultPullActionDropDown.Closing += (sender, args) =>
            {
                if (args.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                {
                    args.Cancel = true;
                }
            };

            var setDefaultPullActionDropDownItems = toolStripButtonPull.DropDownItems
                .OfType<ToolStripMenuItem>()
                .Where(tsmi => tsmi.Tag is AppSettings.PullAction)
                .Select(tsmi =>
                {
                    ToolStripItem tsi = new ToolStripMenuItem
                    {
                        Name = $"{tsmi.Name}SetDefault",
                        Text = tsmi.Text,
                        CheckOnClick = true,
                        Image = tsmi.Image,
                        Tag = tsmi.Tag
                    };

                    tsi.Click += SetDefaultPullActionMenuItemClick;

                    return tsi;
                });

            setDefaultPullActionDropDown.Items.AddRange(setDefaultPullActionDropDownItems.ToArray());

            void SetDefaultPullActionMenuItemClick(object sender, EventArgs eventArgs)
            {
                var clickedMenuItem = (ToolStripMenuItem)sender;
                AppSettings.DefaultPullAction = (AppSettings.PullAction)clickedMenuItem.Tag;
                RefreshDefaultPullAction();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ReSharper disable ConstantConditionalAccessQualifier - these can be null if run from under the TranslatioApp

                _submoduleStatusProvider?.Dispose();
                _formBrowseMenus?.Dispose();
                _filterRevisionsHelper?.Dispose();
                _filterBranchHelper?.Dispose();
                components?.Dispose();
                _gitStatusMonitor?.Dispose();
                _windowsJumpListManager?.Dispose();

                // ReSharper restore ConstantConditionalAccessQualifier
            }

            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            SetSplitterPositions();
            HideVariableMainMenuItems();
            RefreshSplitViewLayout();
            LayoutRevisionInfo();
            InternalInitialize(false);

            if (!Module.IsValidGitWorkingDir())
            {
                base.OnLoad(e);
                return;
            }

            RevisionGrid.Load();
            _filterBranchHelper.InitToolStripBranchFilter();

            ActiveControl = RevisionGrid;
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
            UpdateSubmodulesStructure();
            UpdateStashCount();

            toolStripButtonPush.Initialize(_aheadBehindDataProvider);
            toolStripButtonPush.DisplayAheadBehindInformation(Module.GetSelectedBranch());

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
            PluginRegistry.Unregister(UICommands);
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

        private bool NeedsGitStatusMonitor()
        {
            return AppSettings.ShowGitStatusInBrowseToolbar || (AppSettings.ShowGitStatusForArtificialCommits && AppSettings.RevisionGraphShowWorkingDirChanges);
        }

        private void UICommands_PostRepositoryChanged(object sender, GitUIEventArgs e)
        {
            this.InvokeAsync(RefreshRevisions).FileAndForget();
            UpdateSubmodulesStructure();
            UpdateStashCount();
        }

        private void RefreshRevisions()
        {
            if (RevisionGrid.IsDisposed || IsDisposed || Disposing)
            {
                return;
            }

            _gitStatusMonitor.InvalidateGitWorkingDirectoryStatus();
            _gitStatusMonitor.RequestRefresh();

            if (_dashboard == null || !_dashboard.Visible)
            {
                revisionDiff.ForceRefreshRevisions();
                RevisionGrid.ForceRefreshRevisions();
                InternalInitialize(true);
            }

            toolStripButtonPush.DisplayAheadBehindInformation(Module.GetSelectedBranch());
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
            var pluginEntries = PluginRegistry.Plugins
                .OrderBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase);

            foreach (var plugin in pluginEntries)
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
            }

            // Allow the plugin to perform any self-registration actions
            PluginRegistry.Register(UICommands);

            UICommands.RaisePostRegisterPlugin(this);

            // Show "Repository hosts" menu item when there is at least 1 repository host plugin loaded
            _repositoryHostsToolStripMenuItem.Visible = PluginRegistry.GitHosters.Count > 0;
            if (PluginRegistry.GitHosters.Count == 1)
            {
                _repositoryHostsToolStripMenuItem.Text = PluginRegistry.GitHosters[0].Description;
            }

            UpdatePluginMenu(Module.IsValidGitWorkingDir());
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
                if (AppSettings.CheckForUpdates && AppSettings.LastUpdateCheck.AddDays(7) < DateTime.Now)
                {
                    AppSettings.LastUpdateCheck = DateTime.Now;
                    var updateForm = new FormUpdates(AppSettings.AppVersion);
                    updateForm.SearchForUpdatesAndShow(Owner, false);
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
                bool isDashboard = _dashboard != null && _dashboard.Visible;
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
                Text = _appTitleGenerator.Generate(Module.WorkingDir, validBrowseDir, branchName);

                OnActivate();

                LoadUserMenu();

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

            return;

            void SetShortcutKeyDisplayStringsFromHotkeySettings()
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

                // TODO: add more
            }

            void LoadUserMenu()
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

            void ShowRevisions()
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
        }

        private void OnActivate()
        {
            CheckForMergeConflicts();

            return;

            void CheckForMergeConflicts()
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

        private void FillFileTree()
        {
            var revision = RevisionGrid.GetSelectedRevisions().FirstOrDefault();

            // Don't show the "File Tree" tab for artificial commits
            var showFileTreeTab = revision?.IsArtificial != true;

            if (showFileTreeTab)
            {
                if (TreeTabPage.Parent == null)
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

            if (AppSettings.CommitInfoPosition == CommitInfoPosition.BelowList && CommitInfoTabControl.SelectedTab != CommitInfoTabPage)
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

            if (revision == null)
            {
                return;
            }

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
                _buildReportTabPageExtension = new BuildReportTabPageExtension(() => Module, CommitInfoTabControl, _buildReportTabCaption.Text);
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

        private void RefreshStatus()
        {
            UpdateSubmodulesStructure();
            UpdateStashCount();
        }

        private void RefreshToolStripMenuItemClick(object sender, EventArgs e)
        {
            RefreshRevisions();
            RefreshStatus();
            repoObjectsTree.RefreshTree();
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
            UpdateStashCount();
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

            _dashboard?.RefreshContent();

            _gitStatusMonitor.Active = NeedsGitStatusMonitor();
        }

        private void TagToolStripMenuItemClick(object sender, EventArgs e)
        {
            var revision = RevisionGrid.LatestSelectedRevision;

            UICommands.StartCreateTagDialog(this, revision);
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
                    from = revisions[0].ObjectId.ToShortString(8);
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
            FillFileTree();
            FillDiff();
            FillCommitInfo();
            ThreadHelper.JoinableTaskFactory.RunAsync(() => FillGpgInfoAsync());
            FillBuildReport();
            FillTerminalTab();
            if (CommitInfoTabControl.SelectedTab == DiffTabPage)
            {
                // workaround to avoid focusing the "filter files" combobox
                revisionDiff.SwitchFocus(alreadyContainedFocus: false);
            }
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
            UpdateSubmodulesStructure();
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

        private void ChangeWorkingDir(string path)
        {
            var module = new GitModule(path);
            if (module.IsValidGitWorkingDir())
            {
                SetGitModule(this, new GitModuleEventArgs(module));
                return;
            }

            int invalidPathCount = ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.LoadRecentHistoryAsync()).Count(repo => !GitModule.IsValidGitWorkingDir(repo.Path));
            string commandButtonCaptions = _directoryIsNotAValidRepositoryRemoveSelectedRepoCommand.Text;
            if (invalidPathCount > 1)
            {
                commandButtonCaptions =
                    string.Format("{0}|{1}", commandButtonCaptions, string.Format(_directoryIsNotAValidRepositoryRemoveAllCommand.Text, invalidPathCount));
            }

            int dialogResult = PSTaskDialog.cTaskDialog.ShowCommandBox(
                Title: _directoryIsNotAValidRepositoryCaption.Text,
                MainInstruction: _directoryIsNotAValidRepositoryMainInstruction.Text,
                Content: "",
                CommandButtons: commandButtonCaptions,
                ShowCancelButton: true);

            if (dialogResult < 0)
            {
                /* Cancel */
                return;
            }
            else if (PSTaskDialog.cTaskDialog.CommandButtonResult == 0)
            {
                /* Remove selected invalid repo */
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveRecentAsync(path));
            }
            else if (PSTaskDialog.cTaskDialog.CommandButtonResult == 1)
            {
                /* Remove all invalid repos */
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveInvalidRepositoriesAsync(repoPath => GitModule.IsValidGitWorkingDir(repoPath)));
            }
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
                Process.Start("http://git-extensions-documentation.readthedocs.org/en/master/");
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
            var repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync());
            if (repositoryHistory.Count < 1)
            {
                return;
            }

            PopulateFavouriteRepositoriesMenu(container, repositoryHistory);
        }

        private void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container, in IList<Repository> repositoryHistory)
        {
            var mostRecentRepos = new List<RecentRepoInfo>();
            var lessRecentRepos = new List<RecentRepoInfo>();

            using (var graphics = CreateGraphics())
            {
                var splitter = new RecentRepoSplitter
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
            PluginRegistry.Unregister(UICommands);
            RevisionGrid.InvalidateCount();
            _gitStatusMonitor.InvalidateGitWorkingDirectoryStatus();
            _submoduleStatusProvider.Init();

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
                RevisionInfo.SetRevisionWithChildren(null, Array.Empty<ObjectId>());
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
            Process.Start("https://github.com/gitextensions/gitextensions/wiki/Translations");
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
                var checkoutBranchItem = new ToolStripMenuItem(checkoutBranchToolStripMenuItem.Text, Images.BranchCheckout)
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
                        .GetRefs(tags: false, branches: true, noLocks: true)
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

        internal enum Command
        {
            GitBash = 0,
            GitGui = 1,
            GitGitK = 2,
            FocusRevisionGrid = 3,
            FocusCommitInfo = 4,
            FocusDiff = 5,
            FocusFileTree = 6,
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
            ToggleBranchTreePanel = 21,
            EditFile = 22,
            OpenAsTempFile = 23,
            OpenAsTempFileWith = 24,
            FocusBranchTree = 25,
            FocusGpgInfo = 26,
            GoToSuperproject = 27,
            GoToSubmodule = 28,
            FocusGitConsole = 29,
            FocusBuildServerStatus = 30,
            FocusNextTab = 31,
            FocusPrevTab = 32
        }

        internal Keys GetShortcutKeys(Command cmd)
        {
            return GetShortcutKeys((int)cmd);
        }

        private void AddNotes()
        {
            Module.EditNotes(RevisionGrid.GetSelectedRevisions().FirstOrDefault()?.ObjectId);
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

        protected override bool ExecuteCommand(int cmd)
        {
            switch ((Command)cmd)
            {
                case Command.GitBash: Module.RunBash(); break;
                case Command.GitGui: Module.RunGui(); break;
                case Command.GitGitK: Module.RunGitK(); break;
                case Command.FocusBranchTree: FocusBranchTree(); break;
                case Command.FocusRevisionGrid: RevisionGrid.Focus(); break;
                case Command.FocusCommitInfo: FocusCommitInfo(); break;
                case Command.FocusDiff: FocusTabOf(revisionDiff, (c, alreadyContainedFocus) => c.SwitchFocus(alreadyContainedFocus)); break;
                case Command.FocusFileTree: FocusTabOf(fileTree, (c, alreadyContainedFocus) => c.SwitchFocus(alreadyContainedFocus)); break;
                case Command.FocusGpgInfo when AppSettings.ShowGpgInformation.ValueOrDefault: FocusTabOf(revisionGpgInfo1, (c, alreadyContainedFocus) => c.Focus()); break;
                case Command.FocusGitConsole: FocusGitConsole(); break;
                case Command.FocusBuildServerStatus: FocusTabOf(_buildReportTabPageExtension.Control, (c, alreadyContainedFocus) => c.Focus()); break;
                case Command.FocusNextTab: FocusNextTab(); break;
                case Command.FocusPrevTab: FocusNextTab(forward: false); break;
                case Command.FocusFilter: FocusFilter(); break;
                case Command.Commit: CommitToolStripMenuItemClick(null, null); break;
                case Command.AddNotes: AddNotes(); break;
                case Command.FindFileInSelectedCommit: FindFileInSelectedCommit(); break;
                case Command.CheckoutBranch: CheckoutBranchToolStripMenuItemClick(null, null); break;
                case Command.QuickFetch: QuickFetch(); break;
                case Command.QuickPull: ToolStripButtonPullClick(null, null); break;
                case Command.QuickPush: UICommands.StartPushDialog(this, true); break;
                case Command.CloseRepository: CloseToolStripMenuItemClick(null, null); break;
                case Command.Stash: UICommands.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash); break;
                case Command.StashPop: UICommands.StashPop(this); break;
                case Command.OpenWithDifftool: OpenWithDifftool(); break;
                case Command.OpenSettings: OnShowSettingsClick(null, null); break;
                case Command.ToggleBranchTreePanel: toggleBranchTreePanel_Click(null, null); break;
                case Command.EditFile: EditFile(); break;
                case Command.OpenAsTempFile when fileTree.Visible: fileTree.ExecuteCommand(RevisionFileTreeControl.Command.OpenAsTempFile); break;
                case Command.OpenAsTempFileWith when fileTree.Visible: fileTree.ExecuteCommand(RevisionFileTreeControl.Command.OpenAsTempFileWith); break;
                case Command.GoToSuperproject: toolStripButtonLevelUp_ButtonClick(null, null); break;
                case Command.GoToSubmodule: toolStripButtonLevelUp.ShowDropDown(); break;
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

            void FocusTabOf<T>(T control, Action<T, bool> switchFocus) where T : Control
            {
                if (control != null)
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
                if (_consoleTabPage != null && CommitInfoTabControl.TabPages.Contains(_consoleTabPage))
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
        }

        internal bool ExecuteCommand(Command cmd)
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
            DoPull(pullAction: AppSettings.DefaultPullAction, isSilent: false);
        }

        private void ToolStripButtonPullClick(object sender, EventArgs e)
        {
            // Clicking on the Pull button toolbar button will perform the default selected action silently,
            // except if that action is to open the dialog (PullAction.None)
            bool isSilent = AppSettings.DefaultPullAction != AppSettings.PullAction.None;
            DoPull(pullAction: AppSettings.DefaultPullAction, isSilent: isSilent);
        }

        private void pullToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // "Open Pull Dialog..." toolbar menu item always open the dialog with the current default action
            DoPull(pullAction: AppSettings.DefaultPullAction, isSilent: false);
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
            // Special case for FetchPruneAll to make sure user confirms the action.
            // Notice, if action is not silent, we just show a regular Pull dialog without any confirmations.
            if (isSilent && pullAction == AppSettings.PullAction.FetchPruneAll)
            {
                bool isActionConfirmed = AppSettings.DontConfirmFetchAndPruneAll
                                         || MessageBox.Show(
                                             this,
                                             _pullFetchPruneAllConfirmation.Text,
                                             _pullFetchPruneAll.Text,
                                             MessageBoxButtons.YesNo) == DialogResult.Yes;

                if (!isActionConfirmed)
                {
                    return;
                }
            }

            if (isSilent)
            {
                UICommands.StartPullDialogAndPullImmediately(this, pullAction: pullAction);
            }
            else
            {
                UICommands.StartPullDialog(this, pullAction: pullAction);
            }
        }

        private void RefreshDefaultPullAction()
        {
            var defaultPullAction = AppSettings.DefaultPullAction;

            foreach (ToolStripMenuItem menuItem in setDefaultPullButtonActionToolStripMenuItem.DropDown.Items)
            {
                menuItem.Checked = (AppSettings.PullAction)menuItem.Tag == defaultPullAction;
            }

            switch (defaultPullAction)
            {
                case AppSettings.PullAction.Fetch:
                    toolStripButtonPull.Image = Images.PullFetch;
                    toolStripButtonPull.ToolTipText = _pullFetch.Text;
                    break;

                case AppSettings.PullAction.FetchAll:
                    toolStripButtonPull.Image = Images.PullFetchAll;
                    toolStripButtonPull.ToolTipText = _pullFetchAll.Text;
                    break;

                case AppSettings.PullAction.FetchPruneAll:
                    toolStripButtonPull.Image = Images.PullFetchPruneAll;
                    toolStripButtonPull.ToolTipText = _pullFetchPruneAll.Text;
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

        private void branchSelect_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                CheckoutBranchToolStripMenuItemClick(sender, e);
            }
        }

        private void RevisionInfo_CommandClicked(object sender, CommitInfo.CommandEventArgs e)
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
            PreventToolStripSplitButtonClosing(sender as ToolStripSplitButton);
        }

        #region Submodules

        private ToolStripMenuItem CreateSubmoduleMenuItem(CancellationToken cancelToken, SubmoduleInfo info, string textFormat = "{0}")
        {
            var item = new ToolStripMenuItem(string.Format(textFormat, info.Text))
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

            if (info.Detailed != null)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    var details = await info.Detailed.GetValueAsync(cancelToken);
                    if (details == null)
                    {
                        return;
                    }

                    await this.SwitchToMainThreadAsync(cancelToken);
                    item.Image = GetSubmoduleItemImage(details);
                    item.Text = string.Format(textFormat, info.Text + details.AddedAndRemovedText);
                }).FileAndForget();
            }

            return item;

            Image GetSubmoduleItemImage(DetailedSubmoduleInfo details)
            {
                if (details.Status == null)
                {
                    return Images.FolderSubmodule;
                }

                if (details.Status == SubmoduleStatus.FastForward)
                {
                    return details.IsDirty ? Images.SubmoduleRevisionUpDirty : Images.SubmoduleRevisionUp;
                }

                if (details.Status == SubmoduleStatus.Rewind)
                {
                    return details.IsDirty ? Images.SubmoduleRevisionDownDirty : Images.SubmoduleRevisionDown;
                }

                if (details.Status == SubmoduleStatus.NewerTime)
                {
                    return details.IsDirty ? Images.SubmoduleRevisionSemiUpDirty : Images.SubmoduleRevisionSemiUp;
                }

                if (details.Status == SubmoduleStatus.OlderTime)
                {
                    return details.IsDirty ? Images.SubmoduleRevisionSemiDownDirty : Images.SubmoduleRevisionSemiDown;
                }

                return details.IsDirty ? Images.SubmoduleDirty : Images.FileStatusModified;
            }
        }

        private void UpdateSubmodulesStructure(bool updateStatus = false)
        {
            // Submodule status is updated on git-status updates. To make sure supermodule status is updated, update immediately
            updateStatus = updateStatus || (AppSettings.ShowSubmoduleStatus && _gitStatusMonitor.Active && (Module.SuperprojectModule != null));

            toolStripButtonLevelUp.ToolTipText = "";
            _submoduleStatusProvider.UpdateSubmodulesStatus(
                updateStatus,
                Module.WorkingDir, _noBranchTitle.Text,
                () =>
                {
                    RemoveSubmoduleButtons();
                    toolStripButtonLevelUp.DropDownItems.Add(_loading.Text);
                },
                PopulateToolbarAsync);
        }

        private async Task PopulateToolbarAsync(SubmoduleInfoResult result, CancellationToken cancelToken)
        {
            // Second task: Populate toolbar menu on UI thread.  Note further tasks are created by
            // CreateSubmoduleMenuItem to update images with submodule status.
            await this.SwitchToMainThreadAsync(cancelToken);

            RemoveSubmoduleButtons();

            var newItems = result.OurSubmodules
                .Select(submodule => CreateSubmoduleMenuItem(cancelToken, submodule))
                .ToList<ToolStripItem>();

            if (result.OurSubmodules.Count == 0)
            {
                newItems.Add(new ToolStripMenuItem(_noSubmodulesPresent.Text));
            }

            if (result.SuperProject != null)
            {
                newItems.Add(new ToolStripSeparator());

                // Show top project only if it's not our super project
                if (result.TopProject != null && result.TopProject != result.SuperProject)
                {
                    newItems.Add(CreateSubmoduleMenuItem(cancelToken, result.TopProject, _topProjectModuleFormat.Text));
                }

                newItems.Add(CreateSubmoduleMenuItem(cancelToken, result.SuperProject, _superprojectModuleFormat.Text));
                newItems.AddRange(result.SuperSubmodules.Select(submodule => CreateSubmoduleMenuItem(cancelToken, submodule)));
                toolStripButtonLevelUp.ToolTipText = _goToSuperProject.Text;
            }

            newItems.Add(new ToolStripSeparator());

            var mi = new ToolStripMenuItem(updateAllSubmodulesToolStripMenuItem.Text, Images.SubmodulesUpdate);
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
            UserEnvironmentInformation.CopyInformation();
            Process.Start(@"https://github.com/gitextensions/gitextensions/issues");
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var updateForm = new FormUpdates(AppSettings.AppVersion);
            updateForm.SearchForUpdatesAndShow(Owner, true);
        }

        private void toolStripButtonPull_DropDownOpened(object sender, EventArgs e)
        {
            PreventToolStripSplitButtonClosing(sender as ToolStripSplitButton);
        }

        /// <summary>
        /// Adds a tab with console interface to Git over the current working copy. Recreates the terminal on tab activation if user exits the shell.
        /// </summary>
        private void FillTerminalTab()
        {
            if (!EnvUtils.RunningOnWindows() || !AppSettings.ShowConEmuTab.ValueOrDefault)
            {
                // ConEmu only works on WinNT
                return;
            }

            if (_terminal != null)
            {
                // Terminal already created; give it focus
                _terminal.Focus();
                return;
            }

            if (_consoleTabPage != null)
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

                if (_terminal == null)
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
            using (var formReflog = new FormReflog(UICommands))
            {
                formReflog.ShowDialog();
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
                RevisionGrid.Parent = RevisionsSplitContainer.Panel1;
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
                    RevisionGrid.Parent = RevisionsSplitContainer.Panel1;
                }
                else if (commitInfoPosition == CommitInfoPosition.LeftwardFromList)
                {
                    RevisionsSplitContainer.FixedPanel = FixedPanel.Panel1;
                    RevisionsSplitContainer.SplitterDistance = width;
                    RevisionInfo.Parent = RevisionsSplitContainer.Panel1;
                    RevisionGrid.Parent = RevisionsSplitContainer.Panel2;
                }
                else
                {
                    throw new NotSupportedException();
                }

                RevisionsSplitContainer.Panel2Collapsed = false;
            }

            RevisionInfo.Parent.BackColor = RevisionInfo.BackColor;
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
            if (toolStripBranchFilterComboBox.Items.Count == 0)
            {
                return;
            }

            toolStripBranchFilterComboBox.DroppedDown = true;
        }

        private void undoLastCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AppSettings.DontConfirmUndoLastCommit || MessageBox.Show(this, _undoLastCommitText.Text, _undoLastCommitCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                var args = GitCommandHelpers.ResetCmd(ResetMode.Soft, "HEAD~1");
                Module.GitExecutable.GetOutput(args);
                refreshToolStripMenuItem.PerformClick();
            }
        }

        internal TestAccessor GetTestAccessor()
            => new TestAccessor(this);

        public readonly struct TestAccessor
        {
            private readonly FormBrowse _form;

            public TestAccessor(FormBrowse form)
            {
                _form = form;
                RepoObjectsTree = _form.repoObjectsTree;
            }

            public RepoObjectsTree RepoObjectsTree { get; }

            public void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container, in IList<Repository> repositoryHistory)
            {
                _form.PopulateFavouriteRepositoriesMenu(container, repositoryHistory);
            }
        }
    }
}
