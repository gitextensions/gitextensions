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
using GitCommands.Repository;
using GitCommands.Utils;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.CommandsDialogs.WorktreeDialog;
using GitUI.Hotkey;
using GitUI.Plugin;
using GitUI.Properties;
using GitUI.Script;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGridClasses;
using GitUI.UserControls.ToolStripClasses;
using GitUIPluginInterfaces;
using Microsoft.Win32;
using ResourceManager;
#if !__MonoCS__
using Microsoft.WindowsAPICodePack.Taskbar;
#endif

namespace GitUI.CommandsDialogs
{
    public partial class FormBrowse : GitModuleForm, IBrowseRepo
    {
        #region Translation

        private readonly TranslationString _stashCount =
            new TranslationString("{0} saved {1}");
        private readonly TranslationString _stashPlural =
            new TranslationString("stashes");
        private readonly TranslationString _stashSingular =
            new TranslationString("stash");

        private readonly TranslationString _warningMiddleOfBisect =
            new TranslationString("You are in the middle of a bisect");
        private readonly TranslationString _warningMiddleOfRebase =
            new TranslationString("You are in the middle of a rebase");
        private readonly TranslationString _warningMiddleOfPatchApply =
            new TranslationString("You are in the middle of a patch apply");

        private readonly TranslationString _hintUnresolvedMergeConflicts =
            new TranslationString("There are unresolved merge conflicts!");

        private readonly TranslationString _noBranchTitle =
            new TranslationString("no branch");

        private readonly TranslationString _noSubmodulesPresent =
            new TranslationString("No submodules");
        private readonly TranslationString _topProjectModuleFormat =
            new TranslationString("Top project: {0}");
        private readonly TranslationString _superprojectModuleFormat =
            new TranslationString("Superproject: {0}");

        private readonly TranslationString _indexLockCantDelete =
            new TranslationString("Failed to delete index.lock.");

        private readonly TranslationString _errorCaption =
            new TranslationString("Error");
        private readonly TranslationString _loading =
            new TranslationString("Loading...");

        private readonly TranslationString _noReposHostPluginLoaded =
            new TranslationString("No repository host plugin loaded.");

        private readonly TranslationString _noReposHostFound =
            new TranslationString("Could not find any relevant repository hosts for the currently open repository.");

        private readonly TranslationString _configureWorkingDirMenu =
            new TranslationString("Configure this menu");

        // ReSharper disable InconsistentNaming
        private readonly TranslationString directoryIsNotAValidRepositoryCaption =
            new TranslationString("Open");

        private readonly TranslationString directoryIsNotAValidRepository =
            new TranslationString("The selected item is not a valid git repository.\n\nDo you want to abort and remove it from the recent repositories list?");
        // ReSharper restore InconsistentNaming

        private readonly TranslationString _updateCurrentSubmodule =
            new TranslationString("Update current submodule");

        private readonly TranslationString _pullFetch =
            new TranslationString("Pull - fetch");
        private readonly TranslationString _pullFetchAll =
            new TranslationString("Pull - fetch all");
        private readonly TranslationString _pullMerge =
            new TranslationString("Pull - merge");
        private readonly TranslationString _pullRebase =
            new TranslationString("Pull - rebase");
        private readonly TranslationString _pullOpenDialog =
            new TranslationString("Open pull dialog");

        private readonly TranslationString _buildReportTabCaption =
            new TranslationString("Build Report");
        private readonly TranslationString _consoleTabCaption =
            new TranslationString("Console");

        private readonly TranslationString _commitButtonText =
            new TranslationString("Commit");
        #endregion

        private Dashboard _dashboard;
        private ToolStripItem _rebase;
        private ToolStripItem _bisect;
        private ToolStripItem _warning;

#if !__MonoCS__
        private ThumbnailToolBarButton _commitButton;
        private ThumbnailToolBarButton _pushButton;
        private ThumbnailToolBarButton _pullButton;
        private bool _toolbarButtonsCreated;
#endif
        private readonly ToolStripMenuItem _toolStripGitStatus;
        private readonly GitStatusMonitor _gitStatusMonitor;
        private readonly FilterRevisionsHelper _filterRevisionsHelper;
        private readonly FilterBranchHelper _filterBranchHelper;

        private CancellationTokenSource _submodulesStatusCts = new CancellationTokenSource();
        private BuildReportTabPageExtension _buildReportTabPageExtension;
        private string _diffTabPageTitleBase = "";

        private readonly FormBrowseMenus _formBrowseMenus;
        private ConEmuControl _terminal;
        private readonly SplitterManager _splitterManager = new SplitterManager(new AppSettingsPath("FormBrowse"));
        private readonly IFormBrowseController _controller;
        private readonly ICommitDataManager _commitDataManager;
        private readonly IRepositoryDescriptionProvider _repositoryDescriptionProvider;
        private readonly IAppTitleGenerator _appTitleGenerator;
        private static bool _showRevisionInfoNextToRevisionGrid;

        /// <summary>
        /// For VS designer
        /// </summary>
        private FormBrowse()
        {
            InitializeComponent();
            Translate();
        }

        public FormBrowse(GitUICommands aCommands, string filter)
            : base(true, aCommands)
        {
            //Save value for commit info panel, may be changed
            _showRevisionInfoNextToRevisionGrid = AppSettings.ShowRevisionInfoNextToRevisionGrid;
            InitializeComponent();

            // set tab page images
            {
                var imageList = new ImageList();
                CommitInfoTabControl.ImageList = imageList;
                imageList.ColorDepth = ColorDepth.Depth8Bit;
                imageList.Images.Add(Resources.IconCommit);
                imageList.Images.Add(Resources.IconFileTree);
                imageList.Images.Add(Resources.IconDiff);
                imageList.Images.Add(Resources.IconKey);
                CommitInfoTabControl.TabPages[0].ImageIndex = 0;
                CommitInfoTabControl.TabPages[1].ImageIndex = 1;
                CommitInfoTabControl.TabPages[2].ImageIndex = 2;
                CommitInfoTabControl.TabPages[3].ImageIndex = 3;
            }

            if (!AppSettings.ShowGpgInformation.ValueOrDefault)
            {
                CommitInfoTabControl.RemoveIfExists(GpgInfoTabPage);
            }

            RevisionGrid.UICommandsSource = this;
            Repositories.LoadRepositoryHistoryAsync();
            Task.Factory.StartNew(PluginLoader.Load)
                .ContinueWith((task) => RegisterPlugins(), TaskScheduler.FromCurrentSynchronizationContext());
            RevisionGrid.GitModuleChanged += SetGitModule;
            _filterRevisionsHelper = new FilterRevisionsHelper(toolStripRevisionFilterTextBox, toolStripRevisionFilterDropDownButton, RevisionGrid, toolStripRevisionFilterLabel, ShowFirstParent, form: this);
            _filterBranchHelper = new FilterBranchHelper(toolStripBranchFilterComboBox, toolStripBranchFilterDropDownButton, RevisionGrid);
            toolStripBranchFilterComboBox.DropDown += toolStripBranches_DropDown_ResizeDropDownWidth;
            revisionDiff.Bind(RevisionGrid, fileTree);

            Translate();

            if (AppSettings.ShowGitStatusInBrowseToolbar)
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
                        _toolStripGitStatus.Text = String.Empty;
                    }
                    else if(status == GitStatusMonitorState.Running)
                    {
                        _toolStripGitStatus.Visible = true;
                    }
                };

                _gitStatusMonitor.GitWorkingDirectoryStatusChanged += (s, e) => {
                    var status = e.ItemStatuses.ToList();
                    _toolStripGitStatus.Image = commitIconProvider.GetCommitIcon(status);

                    if (status.Count == 0)
                        _toolStripGitStatus.Text = _commitButtonText.Text;
                    else
                        _toolStripGitStatus.Text = string.Format(_commitButtonText + " ({0})", status.Count.ToString());

                    RevisionGrid.UpdateArtificialCommitCount(status);
                    //The diff filelist is not updated, as the selected diff is unset
                    //_revisionDiff.RefreshArtificial();
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
            if (aCommands != null)
            {
                RefreshPullIcon();
                UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;
                UICommands.BrowseRepo = this;
                _controller = new FormBrowseController(new GitGpgController(() => Module));
                _commitDataManager = new CommitDataManager(() => Module);
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
        }

        private void LayoutRevisionInfo()
        {
            if (_showRevisionInfoNextToRevisionGrid)
            {
                RevisionInfo.Parent = RevisionsSplitContainer.Panel2;
                RevisionsSplitContainer.SplitterDistance = RevisionsSplitContainer.Width - 420;
                RevisionInfo.DisplayAvatarOnRight();
                CommitInfoTabControl.SuspendLayout();
                CommitInfoTabControl.RemoveIfExists(CommitInfoTabPage);
                //Move difftab to left
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

        public FormBrowse(GitUICommands aCommands, string filter, string selectCommit) : this(aCommands, filter)
        {
            if (!string.IsNullOrEmpty(selectCommit))
            {
                RevisionGrid.SetInitialRevision(GitRevision.CreateForShortSha1(Module, selectCommit));
            }
        }

        private new void Translate()
        {
            base.Translate();
            _diffTabPageTitleBase = DiffTabPage.Text;
        }

        void UICommands_PostRepositoryChanged(object sender, GitUIBaseEventArgs e)
        {
            this.InvokeAsync(RefreshRevisions);
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
            if (_dashboard == null)
            {
                _dashboard = new Dashboard();
                _dashboard.GitModuleChanged += SetGitModule;
                toolPanel.ContentPanel.Controls.Add(_dashboard);
                _dashboard.Dock = DockStyle.Fill;
                _dashboard.SetSplitterPositions();
            }
            else
                _dashboard.Refresh();
            _dashboard.Visible = true;
            _dashboard.BringToFront();
            _dashboard.ShowRecentRepositories();
        }

        private void HideDashboard()
        {
            if (_dashboard != null && _dashboard.Visible)
            {
                _dashboard.SaveSplitterPositions();
                _dashboard.Visible = false;
            }
        }

        private void BrowseLoad(object sender, EventArgs e)
        {
#if !__MonoCS__
            if (EnvUtils.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
            {
                TaskbarManager.Instance.ApplicationId = "GitExtensions";
            }
#endif
            SetSplitterPositions();
            HideVariableMainMenuItems();

            RevisionGrid.Load();
            _filterBranchHelper.InitToolStripBranchFilter();

            Cursor.Current = Cursors.WaitCursor;
            LayoutRevisionInfo();
            InternalInitialize(false);
            RevisionGrid.Focus();
            RevisionGrid.IndexWatcher.Reset();

            RevisionGrid.IndexWatcher.Changed += _indexWatcher_Changed;

            Cursor.Current = Cursors.Default;


            try
            {
                if (AppSettings.PlaySpecialStartupSound)
                {
                    using (var cowMoo = Resources.cow_moo)
                        new System.Media.SoundPlayer(cowMoo).Play();
                }
            }
            catch
            {
                // This code is just for fun, we do not want the program to crash because of it.
            }
        }

        void _indexWatcher_Changed(object sender, IndexChangedEventArgs e)
        {
            bool indexChanged = e.IsIndexChanged;
            this.InvokeAsync(() =>
            {
                RefreshButton.Image = indexChanged && AppSettings.UseFastChecks && Module.IsValidGitWorkingDir()
                                          ? Resources.arrow_refresh_dirty
                                          : Resources.arrow_refresh;
            });
        }

        private bool _pluginsLoaded;
        private void LoadPluginsInPluginMenu()
        {
            if (_pluginsLoaded)
                return;
            foreach (var plugin in LoadedPlugins.Plugins)
            {
                var item = new ToolStripMenuItem { Text = plugin.Description, Tag = plugin };
                item.Click += ItemClick;
                pluginsToolStripMenuItem.DropDownItems.Insert(pluginsToolStripMenuItem.DropDownItems.Count - 2, item);
            }
            _pluginsLoaded = true;
            UpdatePluginMenu(Module.IsValidGitWorkingDir());
        }

        /// <summary>
        ///   Execute plugin
        /// </summary>
        private void ItemClick(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem == null)
                return;

            var plugin = menuItem.Tag as IGitPlugin;
            if (plugin == null)
                return;

            var eventArgs = new GitUIEventArgs(this, UICommands);

            bool refresh = plugin.Execute(eventArgs);
            if (refresh)
                RefreshToolStripMenuItemClick(null, null);
        }

        private void UpdatePluginMenu(bool validWorkingDir)
        {
            foreach (ToolStripItem item in pluginsToolStripMenuItem.DropDownItems)
            {
                var plugin = item.Tag as IGitPluginForRepository;

                item.Enabled = plugin == null || validWorkingDir;
            }
        }

        private void RegisterPlugins()
        {
            foreach (var plugin in LoadedPlugins.Plugins)
                plugin.Register(UICommands);

            UICommands.RaisePostRegisterPlugin(this);
        }

        private void UnregisterPlugins()
        {
            foreach (var plugin in LoadedPlugins.Plugins)
                plugin.Unregister(UICommands);
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
            refreshToolStripMenuItem.ShortcutKeys = Keys.None;
            refreshDashboardToolStripMenuItem.ShortcutKeys = Keys.None;
            _repositoryHostsToolStripMenuItem.Visible = false;
            _formBrowseMenus.RemoveAdditionalMainMenuItems();
            menuStrip1.Refresh();
        }

        private void InternalInitialize(bool hard)
        {
            Cursor.Current = Cursors.WaitCursor;

            UICommands.RaisePreBrowseInitialize(this);

            // check for updates
            if (AppSettings.LastUpdateCheck.AddDays(7) < DateTime.Now)
            {
                AppSettings.LastUpdateCheck = DateTime.Now;
                var updateForm = new FormUpdates(Module.AppVersion);
                updateForm.SearchForUpdatesAndShow(Owner, false);
            }

            bool bareRepository = Module.IsBareRepository();
            bool validWorkingDir = Module.IsValidGitWorkingDir();
            bool hasWorkingDir = !string.IsNullOrEmpty(Module.WorkingDir);
            branchSelect.Text = validWorkingDir ? Module.GetSelectedBranch() : "";
            if (hasWorkingDir)
                HideDashboard();
            else
                ShowDashboard();
            toolStripButtonLevelUp.Enabled = hasWorkingDir && !bareRepository;
            CommitInfoTabControl.Visible = validWorkingDir;
            fileExplorerToolStripMenuItem.Enabled = validWorkingDir;
            manageRemoteRepositoriesToolStripMenuItem1.Enabled = validWorkingDir;
            branchSelect.Enabled = validWorkingDir;
            toolStripButton1.Enabled = validWorkingDir && !bareRepository;
            if (_toolStripGitStatus != null)
                _toolStripGitStatus.Enabled = validWorkingDir && !Module.IsBareRepository();
            toolStripButtonPull.Enabled = validWorkingDir;
            toolStripButtonPush.Enabled = validWorkingDir;
            dashboardToolStripMenuItem.Visible = !validWorkingDir;
            repositoryToolStripMenuItem.Visible = validWorkingDir;
            commandsToolStripMenuItem.Visible = validWorkingDir;
            toolStripFileExplorer.Enabled = validWorkingDir;
            if (validWorkingDir)
            {
                refreshToolStripMenuItem.ShortcutKeys = Keys.F5;
            }
            else
            {
                refreshDashboardToolStripMenuItem.ShortcutKeys = Keys.F5;
            }
            UpdatePluginMenu(validWorkingDir);
            gitMaintenanceToolStripMenuItem.Enabled = validWorkingDir;
            editgitignoreToolStripMenuItem1.Enabled = validWorkingDir;
            editgitattributesToolStripMenuItem.Enabled = validWorkingDir;
            editmailmapToolStripMenuItem.Enabled = validWorkingDir;
            toolStripSplitStash.Enabled = validWorkingDir && !bareRepository;
            commitcountPerUserToolStripMenuItem.Enabled = validWorkingDir;
            _createPullRequestsToolStripMenuItem.Enabled = validWorkingDir;
            _viewPullRequestsToolStripMenuItem.Enabled = validWorkingDir;
            //Only show "Repository hosts" menu item when there is at least 1 repository host plugin loaded
            _repositoryHostsToolStripMenuItem.Visible = RepoHosts.GitHosters.Count > 0;
            if (RepoHosts.GitHosters.Count == 1)
                _repositoryHostsToolStripMenuItem.Text = RepoHosts.GitHosters[0].Description;
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
                SvnRebaseToolStripMenuItem.Enabled = !bareRepository;
                SvnDcommitToolStripMenuItem.Enabled = !bareRepository;
            }

            stashChangesToolStripMenuItem.Enabled = !bareRepository;
            gitGUIToolStripMenuItem.Enabled = !bareRepository;

            SetShortcutKeyDisplayStringsFromHotkeySettings();

            if (hard && hasWorkingDir)
                ShowRevisions();
            RefreshWorkingDirCombo();
            var branchName = !string.IsNullOrEmpty(branchSelect.Text) ? branchSelect.Text : _noBranchTitle.Text;
            Text = _appTitleGenerator.Generate(Module.WorkingDir, validWorkingDir, branchName);
            UpdateJumplist(validWorkingDir);

            OnActivate();
            // load custom user menu
            LoadUserMenu();

            if (validWorkingDir)
            {
                // add Navigate and View menu
                _formBrowseMenus.ResetMenuCommandSets();
                _formBrowseMenus.AddMenuCommandSet(MainMenuItem.NavigateMenu, RevisionGrid.MenuCommands.GetNavigateMenuCommands());
                _formBrowseMenus.AddMenuCommandSet(MainMenuItem.ViewMenu, RevisionGrid.MenuCommands.GetViewMenuCommands());

                _formBrowseMenus.InsertAdditionalMainMenuItems(repositoryToolStripMenuItem);
            }

            UICommands.RaisePostBrowseInitialize(this);

            Cursor.Current = Cursors.Default;
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
            Repository r = null;
            if (Repositories.RepositoryHistory.Repositories.Count > 0)
                r = Repositories.RepositoryHistory.Repositories[0];

            List<RecentRepoInfo> mostRecentRepos = new List<RecentRepoInfo>();

            if (r == null || !r.Path.Equals(Module.WorkingDir, StringComparison.InvariantCultureIgnoreCase))
                Repositories.AddMostRecentRepository(Module.WorkingDir);

            using (var graphics = CreateGraphics())
            {
                var splitter = new RecentRepoSplitter
                {
                    MeasureFont = _NO_TRANSLATE_Workingdir.Font,
                    Graphics = graphics
                };
                splitter.SplitRecentRepos(Repositories.RepositoryHistory.Repositories, mostRecentRepos, mostRecentRepos);

                RecentRepoInfo ri = mostRecentRepos.Find((e) => e.Repo.Path.Equals(Module.WorkingDir, StringComparison.InvariantCultureIgnoreCase));

                if (ri == null)
                    _NO_TRANSLATE_Workingdir.Text = Module.WorkingDir;
                else
                    _NO_TRANSLATE_Workingdir.Text = ri.Caption;

                if (AppSettings.RecentReposComboMinWidth > 0)
                {
                    _NO_TRANSLATE_Workingdir.AutoSize = false;
                    var captionWidth = graphics.MeasureString(_NO_TRANSLATE_Workingdir.Text, _NO_TRANSLATE_Workingdir.Font).Width;
                    captionWidth = captionWidth + _NO_TRANSLATE_Workingdir.DropDownButtonWidth + 5;
                    _NO_TRANSLATE_Workingdir.Width = Math.Max(AppSettings.RecentReposComboMinWidth, (int)captionWidth);
                }
                else
                    _NO_TRANSLATE_Workingdir.AutoSize = true;
            }
        }


        private void LoadUserMenu()
        {
            var scripts = ScriptManager.GetScripts().Where(script => script.Enabled
                && script.OnEvent == ScriptEvent.ShowInUserMenuBar).ToList();

            for (int i = ToolStrip.Items.Count - 1; i >= 0; i--)
                if (ToolStrip.Items[i].Tag != null &&
                    ToolStrip.Items[i].Tag as string == "userscript")
                    ToolStrip.Items.RemoveAt(i);

            if (scripts.Count == 0)
                return;

            ToolStripSeparator toolstripseparator = new ToolStripSeparator();
            toolstripseparator.Tag = "userscript";
            ToolStrip.Items.Add(toolstripseparator);

            foreach (ScriptInfo scriptInfo in scripts)
            {
                ToolStripButton tempButton = new ToolStripButton();
                //store scriptname
                tempButton.Text = scriptInfo.Name;
                tempButton.Tag = "userscript";
                //add handler
                tempButton.Click += UserMenu_Click;
                tempButton.Enabled = true;
                tempButton.Visible = true;
                //tempButton.Image = GitUI.Properties.Resources.bug;
                //scriptInfo.Icon = "Cow";
                tempButton.Image = scriptInfo.GetIcon();
                tempButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                //add to toolstrip
                ToolStrip.Items.Add(tempButton);
            }
        }

        private void UserMenu_Click(object sender, EventArgs e)
        {
            if (ScriptRunner.RunScript(this, Module, ((ToolStripButton)sender).Text, RevisionGrid))
                RevisionGrid.RefreshRevisions();
        }

        private void UpdateJumplist(bool validWorkingDir)
        {
#if !__MonoCS__
            if (!EnvUtils.RunningOnWindows() || !TaskbarManager.IsPlatformSupported)
                return;

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

                    //Remove InvalidPathChars
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

                    //to control which category Recent/Frequent is displayed
                    jumpList.KnownCategoryToDisplay = JumpListKnownCategoryType.Recent;

                    jumpList.Refresh();
                }

                CreateOrUpdateTaskBarButtons(validWorkingDir);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                Trace.WriteLine(ex.Message, "UpdateJumplist");
            }
#endif
        }

#if !__MonoCS__
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
                    ThumbnailToolBarButton[] buttons = new[] { _commitButton, _pullButton, _pushButton };

                    //Call this method using reflection.  This is a workaround to *not* reference WPF libraries, becuase of how the WindowsAPICodePack was implimented.
                    TaskbarManager.Instance.ThumbnailToolBars.AddButtons(Handle, buttons);
                }

                _commitButton.Enabled = validRepo;
                _pushButton.Enabled = validRepo;
                _pullButton.Enabled = validRepo;
            }
        }
#endif

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
                { // w is bigger, so divide h by r
                    w = size;
                    h = (int)(size / r);
                    x = 0; y = (size - h) / 2; // center the image
                }
                else
                { // h is bigger, so multiply w by r
                    w = (int)(size * r);
                    h = size;
                    y = 0; x = (size - w) / 2; // center the image
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
            if (AppSettings.ShowStashCount)
            {
                AsyncLoader.DoAsync(() => Module.GetStashes().Count,
                    (result) => toolStripSplitStash.Text = string.Format(_stashCount.Text, result,
                        result != 1 ? _stashPlural.Text : _stashSingular.Text));
            }
            else
            {
                toolStripSplitStash.Text = string.Empty;
            }
        }

        private void CheckForMergeConflicts()
        {
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

            AsyncLoader.DoAsync(
                () => validWorkingDir && Module.InTheMiddleOfConflictedMerge() &&
                      !Directory.Exists(Module.WorkingDirGitDir + "rebase-apply\\"),
                (result) =>
                {
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

                    //Only show status strip when there are status items on it.
                    //There is always a close (x) button, do not count first item.
                    if (statusStrip.Items.Count > 1)
                        statusStrip.Show();
                    else
                        statusStrip.Hide();
                });
        }

        private void RebaseClick(object sender, EventArgs e)
        {
            if (Module.InTheMiddleOfRebase())
                UICommands.StartRebaseDialog(this, null);
            else
                UICommands.StartApplyPatchDialog(this);
        }


        private void ShowRevisions()
        {
            if (RevisionGrid.IndexWatcher.IndexChanged)
            {
                FillFileTree();
                FillDiff();
                FillCommitInfo();
                FillGpgInfo();
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
            DiffTabPage.Text = _diffTabPageTitleBase;

            if (CommitInfoTabControl.SelectedTab != DiffTabPage)
            {
                return;
            }

            if (_selectedRevisionUpdatedTargets.HasFlag(UpdateTargets.DiffList))
                return;

            _selectedRevisionUpdatedTargets |= UpdateTargets.DiffList;

            DiffTabPage.Text = revisionDiff.GetTabText();
        }

        private void FillCommitInfo()
        {
            if (!_showRevisionInfoNextToRevisionGrid && CommitInfoTabControl.SelectedTab != CommitInfoTabPage)
                return;

            if (_selectedRevisionUpdatedTargets.HasFlag(UpdateTargets.CommitInfo))
                return;

            _selectedRevisionUpdatedTargets |= UpdateTargets.CommitInfo;

            if (RevisionGrid.GetSelectedRevisions().Count == 0)
                return;

            var revision = RevisionGrid.GetSelectedRevisions()[0];

            var children = RevisionGrid.GetRevisionChildren(revision.Guid);
            RevisionInfo.SetRevisionWithChildren(revision, children);
        }

        private async void FillGpgInfo()
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
            if (EnvUtils.IsMonoRuntime())
                return;

            var selectedRevisions = RevisionGrid.GetSelectedRevisions();
            var revision = selectedRevisions.Count == 1 ? selectedRevisions[0] : null;

            if (_buildReportTabPageExtension == null)
                _buildReportTabPageExtension = new BuildReportTabPageExtension(CommitInfoTabControl, _buildReportTabCaption.Text);

            //Note: FillBuildReport will check if tab is visible and revision is OK
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
                FillGpgInfo();
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
                SetGitModule(this, new GitModuleEventArgs(module));
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
            UICommands.StartInitializeDialog(this, SetGitModule);
        }

        private void PushToolStripMenuItemClick(object sender, EventArgs e)
        {
            bool bSilent = (ModifierKeys & Keys.Shift) != 0;
            UICommands.StartPushDialog(this, bSilent);
        }

        private void PullToolStripMenuItemClick(object sender, EventArgs e)
        {
            bool bSilent;
            if (sender == toolStripButtonPull || sender == pullToolStripMenuItem)
            {
                if (Module.LastPullAction == AppSettings.PullAction.None)
                {
                    bSilent = (ModifierKeys & Keys.Shift) != 0;
                }
                else if (Module.LastPullAction == AppSettings.PullAction.FetchAll)
                {
                    fetchAllToolStripMenuItem_Click(sender, e);
                    return;
                }
                else
                {
                    bSilent = (sender == toolStripButtonPull);
                    Module.LastPullActionToFormPullAction();
                }
            }
            else
            {
                bSilent = sender != pullToolStripMenuItem1;

                Module.LastPullActionToFormPullAction();
            }

            UICommands.StartPullDialog(this, bSilent);
        }

        private void RefreshToolStripMenuItemClick(object sender, EventArgs e)
        {
            RefreshRevisions();
        }

        private void RefreshDashboardToolStripMenuItemClick(object sender, EventArgs e)
        {
            _dashboard.Refresh();
        }

        private void AboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            using (var frm = new AboutBox()) frm.ShowDialog(this);
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
            UICommands.StartDeleteBranchDialog(this, null);
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
                Translate();

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
        }

        private void TagToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCreateTagDialog(this);
        }

        private void RefreshButtonClick(object sender, EventArgs e)
        {
            RefreshToolStripMenuItemClick(sender, e);
        }

        private void CommitcountPerUserToolStripMenuItemClick(object sender, EventArgs e)
        {
            using (var frm = new FormCommitCount(UICommands)) frm.ShowDialog(this);
        }

        private void KGitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Module.RunGitK();
        }

        private void DonateToolStripMenuItemClick(object sender, EventArgs e)
        {
            using (var frm = new FormDonate()) frm.ShowDialog(this);
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
                diffRevision = revisions.Last();

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
            IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();
            if (2 == revisions.Count)
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
                UICommands.StartRebaseDialog(this, from, to, null);
            }
            else
            {
                UICommands.StartRebaseDialog(this, null);
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
            FillGpgInfo();
            FillBuildReport();
            FillTerminalTab();
        }

        private void ChangelogToolStripMenuItemClick(object sender, EventArgs e)
        {
            using (var frm = new FormChangeLog()) frm.ShowDialog(this);
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
            var toolStripMenuItem = sender as ToolStripMenuItem;
            if (toolStripMenuItem != null)
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

        private void ViewStashToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartStashDialog(this);
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Close();
        }

        private void FileToolStripMenuItemDropDownOpening(object sender, EventArgs e)
        {
            if (Repositories.RepositoryHistory.Repositories.Count() == 0)
            {
                recentToolStripMenuItem.Enabled = false;
                return;
            }

            recentToolStripMenuItem.Enabled = true;
            recentToolStripMenuItem.DropDownItems.Clear();

            foreach (var historyItem in Repositories.RepositoryHistory.Repositories)
            {
                if (string.IsNullOrEmpty(historyItem.Path))
                    continue;

                var historyItemMenu = new ToolStripMenuItem(historyItem.Path);
                historyItemMenu.Click += HistoryItemMenuClick;
                historyItemMenu.Width = 225;
                recentToolStripMenuItem.DropDownItems.Add(historyItemMenu);
            }

            // Re-add controls.
            recentToolStripMenuItem.DropDownItems.Add(clearRecentRepositoriesListToolStripMenuItem);
            TranslateItem(clearRecentRepositoriesListMenuItem.Name, clearRecentRepositoriesListMenuItem);
            recentToolStripMenuItem.DropDownItems.Add(clearRecentRepositoriesListMenuItem);
        }

        private void ChangeWorkingDir(string path)
        {
            GitModule module = new GitModule(path);

            if (!module.IsValidGitWorkingDir())
            {
                DialogResult dialogResult = MessageBox.Show(this, directoryIsNotAValidRepository.Text,
                    directoryIsNotAValidRepositoryCaption.Text, MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                if (dialogResult == DialogResult.Yes)
                {
                    Repositories.RepositoryHistory.RemoveRecentRepository(path);
                    return;
                }
                else if (dialogResult == DialogResult.Cancel)
                    return;
            }

            SetGitModule(this, new GitModuleEventArgs(module));
        }

        private void HistoryItemMenuClick(object sender, EventArgs e)
        {
            var button = sender as ToolStripMenuItem;

            if (button == null)
                return;

            ChangeWorkingDir(button.Text);
        }

        private void ClearRecentRepositoriesListClick(object sender, EventArgs e)
        {
            Repositories.RepositoryHistory.Repositories.Clear();
            Repositories.SaveSettings();
            // Force clear recent repositories list from dashboard.
            if (_dashboard != null)
            {
                _dashboard.ShowRecentRepositories();
            }
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

        private void AddWorkingdirDropDownItem(Repository repo, string caption)
        {
            ToolStripMenuItem toolStripItem = new ToolStripMenuItem(caption);
            _NO_TRANSLATE_Workingdir.DropDownItems.Add(toolStripItem);

            toolStripItem.Click += (hs, he) => ChangeWorkingDir(repo.Path);

            if (repo.Title != null || !repo.Path.Equals(caption))
                toolStripItem.ToolTipText = repo.Path;
        }

        private void WorkingdirDropDownOpening(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Workingdir.DropDownItems.Clear();

            List<RecentRepoInfo> mostRecentRepos = new List<RecentRepoInfo>();
            List<RecentRepoInfo> lessRecentRepos = new List<RecentRepoInfo>();

            using (var graphics = CreateGraphics())
            {
                var splitter = new RecentRepoSplitter
                {
                    MeasureFont = _NO_TRANSLATE_Workingdir.Font,
                    Graphics = graphics
                };
                splitter.SplitRecentRepos(Repositories.RepositoryHistory.Repositories, mostRecentRepos, lessRecentRepos);
            }

            foreach (RecentRepoInfo repo in mostRecentRepos)
                AddWorkingdirDropDownItem(repo.Repo, repo.Caption);

            if (lessRecentRepos.Count > 0)
            {
                if (mostRecentRepos.Count > 0 && (AppSettings.SortMostRecentRepos || AppSettings.SortLessRecentRepos))
                    _NO_TRANSLATE_Workingdir.DropDownItems.Add(new ToolStripSeparator());

                foreach (RecentRepoInfo repo in lessRecentRepos)
                    AddWorkingdirDropDownItem(repo.Repo, repo.Caption);
            }

            _NO_TRANSLATE_Workingdir.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem toolStripItem = new ToolStripMenuItem(openToolStripMenuItem.Text);
            toolStripItem.ShortcutKeys = openToolStripMenuItem.ShortcutKeys;
            _NO_TRANSLATE_Workingdir.DropDownItems.Add(toolStripItem);
            toolStripItem.Click += (hs, he) => OpenToolStripMenuItemClick(hs, he);

            toolStripItem = new ToolStripMenuItem(_configureWorkingDirMenu.Text);
            _NO_TRANSLATE_Workingdir.DropDownItems.Add(toolStripItem);
            toolStripItem.Click += (hs, he) =>
            {
                using (var frm = new FormRecentReposSettings()) frm.ShowDialog(this);
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
                Repositories.AddMostRecentRepository(Module.WorkingDir);
                AppSettings.RecentWorkingDir = module.WorkingDir;
                ChangeTerminalActiveFolder(Module.WorkingDir);

#if DEBUG
                //Current encodings
                Debug.WriteLine("Encodings for " + module.WorkingDir);
                Debug.WriteLine("Files content encoding: " + module.FilesEncoding.EncodingName);
                Debug.WriteLine("Commit encoding: " + module.CommitEncoding.EncodingName);
                if (module.LogOutputEncoding.CodePage != module.CommitEncoding.CodePage)
                    Debug.WriteLine("Log output encoding: " + module.LogOutputEncoding.EncodingName);
#endif
            }

            HideDashboard();
            UICommands.RepoChangedNotifier.Notify();
            RevisionGrid.IndexWatcher.Reset();
            RegisterPlugins();
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
                return;

            var fileNames = new StringBuilder();
            foreach (var item in diffFiles.SelectedItems)
            {
                //Only use append line when multiple items are selected.
                //This to make it easier to use the text from clipboard when 1 file is selected.
                if (fileNames.Length > 0)
                    fileNames.AppendLine();

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

        private void pluginsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            LoadPluginsInPluginMenu();
        }

        private void BisectClick(object sender, EventArgs e)
        {
            using (var frm = new FormBisect(RevisionGrid)) frm.ShowDialog(this);
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
            IList<IGitRef> branches = Module.GetRefs(false);
            IEnumerable<string> branchNames = branches.Select(b => b.Name);
            if (AppSettings.BranchOrderingCriteria == BranchOrdering.Alphabetically)
            {
                branchNames = branchNames.OrderBy(b => b);
            }

            // Make sure there are never more than a 100 branches added to the menu
            // GitExtensions will hang when the drop down is too large...
            branchNames = branchNames.Take(100);

            return branchNames;
        }

        void BranchSelectToolStripItem_Click(object sender, EventArgs e)
        {
            var toolStripItem = (ToolStripItem)sender;
            UICommands.StartCheckoutBranch(this, toolStripItem.Text, false);
        }

        private void _forkCloneMenuItem_Click(object sender, EventArgs e)
        {
            if (RepoHosts.GitHosters.Count > 0)
            {
                UICommands.StartCloneForkFromHoster(this, RepoHosts.GitHosters[0], SetGitModule);
                UICommands.RepoChangedNotifier.Notify();
            }
            else
            {
                MessageBox.Show(this, _noReposHostPluginLoaded.Text, _errorCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void _viewPullRequestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var repoHost = RepoHosts.TryGetGitHosterForModule(Module);
            if (repoHost == null)
            {
                MessageBox.Show(this, _noReposHostFound.Text, _errorCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            UICommands.StartPullRequestsDialog(this, repoHost);
        }

        private void _createPullRequestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var repoHost = RepoHosts.TryGetGitHosterForModule(Module);
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
            GitBash,
            GitGui,
            GitGitK,
            FocusRevisionGrid,
            FocusCommitInfo,
            FocusFileTree,
            FocusDiff,
            Commit,
            AddNotes,
            FindFileInSelectedCommit,
            CheckoutBranch,
            QuickFetch,
            QuickPull,
            QuickPush,
            RotateApplicationIcon,
            CloseRepository,
            Stash,
            StashPop
        }

        private void AddNotes()
        {
            Module.EditNotes(RevisionGrid.GetSelectedRevisions().Count > 0 ? RevisionGrid.GetSelectedRevisions()[0].Guid : string.Empty);
            FillCommitInfo();
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
                case Commands.Commit: CommitToolStripMenuItemClick(null, null); break;
                case Commands.AddNotes: AddNotes(); break;
                case Commands.FindFileInSelectedCommit: FindFileInSelectedCommit(); break;
                case Commands.CheckoutBranch: CheckoutBranchToolStripMenuItemClick(null, null); break;
                case Commands.QuickFetch: QuickFetch(); break;
                case Commands.QuickPull: UICommands.StartPullDialog(this, true); break;
                case Commands.QuickPush: UICommands.StartPushDialog(this, true); break;
                case Commands.RotateApplicationIcon: RotateApplicationIcon(); break;
                case Commands.CloseRepository: CloseToolStripMenuItemClick(null, null); break;
                case Commands.Stash: UICommands.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash); break;
                case Commands.StashPop: UICommands.StashPop(this); break;
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
            EnabledSplitViewLayout(MainSplitContainer.Panel2Collapsed);
        }

        private void EnabledSplitViewLayout(bool enabled)
        {
            MainSplitContainer.Panel2Collapsed = !enabled;
        }

        public static void OpenContainingFolder(FileStatusList diffFiles, GitModule module)
        {
            if (!diffFiles.SelectedItems.Any())
                return;

            foreach (var item in diffFiles.SelectedItems)
            {
                string filePath = FormBrowseUtil.GetFullPathFromGitItemStatus(module, item);
                FormBrowseUtil.ShowFileOrParentFolderInFileExplorer(filePath);
            }
        }

        protected void SetSplitterPositions()
        {
            _splitterManager.AddSplitter(RevisionsSplitContainer, "RevisionsSplitContainer");
            _splitterManager.AddSplitter(MainSplitContainer, "MainSplitContainer");
            revisionDiff.InitSplitterManager(_splitterManager);
            fileTree.InitSplitterManager(_splitterManager);
            //hide status in order to restore splitters against the full height (the most common case)
            statusStrip.Hide();
            _splitterManager.RestoreSplitters();
        }

        protected void SaveSplitterPositions()
        {
            _splitterManager.SaveSplitters();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            SaveSplitterPositions();
            if (_dashboard != null && _dashboard.Visible)
                _dashboard.SaveSplitterPositions();
        }

        protected override void OnClosed(EventArgs e)
        {
            SetWorkingDir("");

            base.OnClosed(e);
        }

        private void CommandsToolStripMenuItem_DropDownOpening(object sender, System.EventArgs e)
        {
            //Most options do not make sense for artificial commits or no revision selected at all
            var selectedRevisions = RevisionGrid.GetSelectedRevisions();
            bool enabled = selectedRevisions.Count == 1 && !selectedRevisions[0].IsArtificial();

            this.branchToolStripMenuItem.Enabled =
            this.deleteBranchToolStripMenuItem.Enabled =
            this.mergeBranchToolStripMenuItem.Enabled =
            this.rebaseToolStripMenuItem.Enabled =
            this.stashToolStripMenuItem.Enabled =
              selectedRevisions.Count > 0 && !Module.IsBareRepository();

            this.resetToolStripMenuItem.Enabled =
            this.checkoutBranchToolStripMenuItem.Enabled =
            this.runMergetoolToolStripMenuItem.Enabled =
            this.cherryPickToolStripMenuItem.Enabled =
            this.checkoutToolStripMenuItem.Enabled =
            this.toolStripMenuItemReflog.Enabled = 
            this.bisectToolStripMenuItem.Enabled =
              enabled && !Module.IsBareRepository();

            this.tagToolStripMenuItem.Enabled =
            this.deleteTagToolStripMenuItem.Enabled =
            this.archiveToolStripMenuItem.Enabled =
              enabled;
        }

        private void CloneSvnToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartSvnCloneDialog(this, SetGitModule);
        }

        private void SvnRebaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UICommands.StartSvnRebaseDialog(this);
        }

        private void SvnDcommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UICommands.StartSvnDcommitDialog(this);
        }

        private void SvnFetchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UICommands.StartSvnFetchDialog(this);
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
                }
            );
        }

        private void rebaseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DoPullAction(() =>
            {
                Module.LastPullAction = AppSettings.PullAction.Rebase;
                PullToolStripMenuItemClick(sender, e);
            }
            );
        }

        private void fetchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoPullAction(() =>
            {
                Module.LastPullAction = AppSettings.PullAction.Fetch;
                PullToolStripMenuItemClick(sender, e);
            }
            );
        }

        private void pullToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (AppSettings.SetNextPullActionAsDefault)
                Module.LastPullAction = AppSettings.PullAction.None;
            PullToolStripMenuItemClick(sender, e);

            //restore AppSettings.FormPullAction value
            if (!AppSettings.SetNextPullActionAsDefault)
                Module.LastPullActionToFormPullAction();

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
                Module.LastPullAction = AppSettings.PullAction.FetchAll;

            RefreshPullIcon();
            bool pullCompelted;

            UICommands.StartPullDialog(this, true, out pullCompelted, true);

            //restore AppSettings.FormPullAction value
            if (!AppSettings.SetNextPullActionAsDefault)
                Module.LastPullActionToFormPullAction();

            AppSettings.SetNextPullActionAsDefault = false;
        }

        private void _NO_TRANSLATE_Workingdir_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                OpenToolStripMenuItemClick(sender, e);
        }

        private void branchSelect_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                CheckoutBranchToolStripMenuItemClick(sender, e);
        }

        private void RevisionInfo_CommandClick(object sender, CommitInfo.CommandEventArgs e)
        {
            if (e.Command == "gotocommit")
            {
                var revision = GitRevision.CreateForShortSha1(Module, e.Data);
                var found = RevisionGrid.SetSelectedRevision(revision);

                // When 'git log --first-parent' filtration is used, user can click on child commit
                // that is not present in the shown git log. User still wants to see the child commit
                // and to make it possible we add explicit branch filter and refresh.
                if (AppSettings.ShowFirstParent && !found)
                {
                    _filterBranchHelper.SetBranchFilter(revision.Guid, refresh: true);
                    RevisionGrid.SetSelectedRevision(revision);
                }
            }
            else if (e.Command == "gotobranch" || e.Command == "gototag")
            {
                string error = "";
                CommitData commit = _commitDataManager.GetCommitData(e.Data, ref error);
                if (commit != null)
                    RevisionGrid.SetSelectedRevision(new GitRevision(Module, commit.Guid));
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
            var menuSender = sender as ToolStripMenuItem;
            if (menuSender != null)
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

        private void ToolStripSplitButtonDropDownClosed(object sender, EventArgs e)
        {
            var control = sender as ToolStripSplitButton;

            if (control == null)
            {
                return;
            }

            control.DropDownClosed -= ToolStripSplitButtonDropDownClosed;

            var controlToFocus = control.Tag as Control;

            if (controlToFocus == null)
            {
                return;
            }

            controlToFocus.Focus();
            control.Tag = null;
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
                var toolStripButton = item as ToolStripMenuItem;
                if (toolStripButton != null)
                    toolStripButton.Click -= SubmoduleToolStripButtonClick;
            }
            toolStripButtonLevelUp.DropDownItems.Clear();
        }

        private string GetModuleBranch(string path)
        {
            string branch = GitModule.GetSelectedBranchFast(path);
            return string.Format("[{0}]", GitModule.IsDetachedHead(branch) ? _noBranchTitle.Text : branch);
        }

        private ToolStripMenuItem CreateSubmoduleMenuItem(SubmoduleInfo info, string textFormat)
        {
            var spmenu = new ToolStripMenuItem(string.Format(textFormat, info.Text));
            spmenu.Click += SubmoduleToolStripButtonClick;
            spmenu.Width = 200;
            spmenu.Tag = info.Path;
            if (info.Bold)
                spmenu.Font = new Font(spmenu.Font, FontStyle.Bold);
            spmenu.Image = GetItemImage(info);
            return spmenu;
        }

        private ToolStripMenuItem CreateSubmoduleMenuItem(SubmoduleInfo info)
        {
            return CreateSubmoduleMenuItem(info, "{0}");
        }

        DateTime _previousUpdateTime;

        private void LoadSubmodulesIntoDropDownMenu()
        {
            TimeSpan elapsed = DateTime.Now - _previousUpdateTime;
            if (elapsed.TotalSeconds > 15)
                UpdateSubmodulesList();
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
            public SubmoduleInfo TopProject, Superproject;
            public string CurrentSubmoduleName;
        }

        private static Image GetItemImage(SubmoduleInfo info)
        {
            if (info.Status == null)
                return Resources.IconFolderSubmodule;
            if (info.Status == SubmoduleStatus.FastForward)
                return info.IsDirty ? Resources.IconSubmoduleRevisionUpDirty : Resources.IconSubmoduleRevisionUp;
            if (info.Status == SubmoduleStatus.Rewind)
                return info.IsDirty ? Resources.IconSubmoduleRevisionDownDirty : Resources.IconSubmoduleRevisionDown;
            if (info.Status == SubmoduleStatus.NewerTime)
                return info.IsDirty ? Resources.IconSubmoduleRevisionSemiUpDirty : Resources.IconSubmoduleRevisionSemiUp;
            if (info.Status == SubmoduleStatus.OlderTime)
                return info.IsDirty ? Resources.IconSubmoduleRevisionSemiDownDirty : Resources.IconSubmoduleRevisionSemiDown;

            return info.IsDirty ? Resources.IconSubmoduleDirty : Resources.Modified;
        }

        private static void GetSubmoduleStatusAsync(SubmoduleInfo info, CancellationToken cancelToken)
        {
            Task.Factory.StartNew(() =>
            {
                var submodule = new GitModule(info.Path);
                var supermodule = submodule.SuperprojectModule;
                var submoduleName = submodule.GetCurrentSubmoduleLocalPath();

                info.Status = null;

                if (string.IsNullOrEmpty(submoduleName) || supermodule == null)
                    return;

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
            }, cancelToken, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
        }

        private void UpdateSubmodulesList()
        {
            _previousUpdateTime = DateTime.Now;

            // Cancel any previous async activities:
            _submodulesStatusCts?.Cancel();
            _submodulesStatusCts?.Dispose();
            _submodulesStatusCts = new CancellationTokenSource();

            RemoveSubmoduleButtons();
            toolStripButtonLevelUp.DropDownItems.Add(_loading.Text);

            // Start gathering new submodule information asynchronously.  This makes a significant difference in UI
            // responsiveness if there are numerous submodules (e.g. > 100).
            var cancelToken = _submodulesStatusCts.Token;
            string thisModuleDir = Module.WorkingDir;
            // First task: Gather list of submodules on a background thread.
            var updateTask = Task.Factory.StartNew(() =>
            {
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
                        name = name + " " + GetModuleBranch(path);

                    var smi = new SubmoduleInfo { Text = name, Path = path };
                    result.OurSubmodules.Add(smi);
                    GetSubmoduleStatusAsync(smi, cancelToken);
                }

                if (threadModule.SuperprojectModule != null)
                {
                    GitModule supersuperproject = threadModule.FindTopProjectModule();
                    if (threadModule.SuperprojectModule.WorkingDir != supersuperproject.WorkingDir)
                    {
                        var name = Path.GetFileName(Path.GetDirectoryName(supersuperproject.WorkingDir));
                        string path = supersuperproject.WorkingDir;
                        if (AppSettings.DashboardShowCurrentBranch && !GitModule.IsBareRepository(path))
                            name = name + " " + GetModuleBranch(path);

                        result.TopProject = new SubmoduleInfo { Text = name, Path = supersuperproject.WorkingDir };
                        GetSubmoduleStatusAsync(result.TopProject, cancelToken);
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
                            name = Path.GetFileName(Path.GetDirectoryName(supersuperproject.WorkingDir));
                        string path = threadModule.SuperprojectModule.WorkingDir;
                        if (AppSettings.DashboardShowCurrentBranch && !GitModule.IsBareRepository(path))
                            name = name + " " + GetModuleBranch(path);

                        result.Superproject = new SubmoduleInfo { Text = name, Path = threadModule.SuperprojectModule.WorkingDir };
                        GetSubmoduleStatusAsync(result.Superproject, cancelToken);
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
                                name = name + " " + GetModuleBranch(path);
                            bool bold = false;
                            if (submodule == localpath)
                            {
                                result.CurrentSubmoduleName = threadModule.GetCurrentSubmoduleLocalPath();
                                bold = true;
                            }
                            var smi = new SubmoduleInfo { Text = name, Path = path, Bold = bold };
                            result.SuperSubmodules.Add(smi);
                            GetSubmoduleStatusAsync(smi, cancelToken);
                        }
                    }
                }
                return result;
            }, cancelToken);

            // Second task: Populate toolbar menu on UI thread.  Note further tasks are created by
            // CreateSubmoduleMenuItem to update images with submodule status.
            updateTask.ContinueWith((task) =>
            {
                if (task.Result == null)
                    return;

                RemoveSubmoduleButtons();
                var newItems = new List<ToolStripItem>();

                task.Result.OurSubmodules.ForEach(submodule => newItems.Add(CreateSubmoduleMenuItem(submodule)));
                if (task.Result.OurSubmodules.Count == 0)
                    newItems.Add(new ToolStripMenuItem(_noSubmodulesPresent.Text));

                if (task.Result.Superproject != null)
                {
                    newItems.Add(new ToolStripSeparator());
                    if (task.Result.TopProject != null)
                        newItems.Add(CreateSubmoduleMenuItem(task.Result.TopProject, _topProjectModuleFormat.Text));
                    newItems.Add(CreateSubmoduleMenuItem(task.Result.Superproject, _superprojectModuleFormat.Text));
                    task.Result.SuperSubmodules.ForEach(submodule => newItems.Add(CreateSubmoduleMenuItem(submodule)));
                }

                newItems.Add(new ToolStripSeparator());

                var mi = new ToolStripMenuItem(updateAllSubmodulesToolStripMenuItem.Text);
                mi.Click += UpdateAllSubmodulesToolStripMenuItemClick;
                newItems.Add(mi);

                if (task.Result.CurrentSubmoduleName != null)
                {
                    var usmi = new ToolStripMenuItem(_updateCurrentSubmodule.Text);
                    usmi.Tag = task.Result.CurrentSubmoduleName;
                    usmi.Click += UpdateSubmoduleToolStripMenuItemClick;
                    newItems.Add(usmi);
                }

                // Using AddRange is critical: if you used Add to add menu items one at a
                // time, performance would be extremely slow with many submodules (> 100).
                toolStripButtonLevelUp.DropDownItems.AddRange(newItems.ToArray());

                _previousUpdateTime = DateTime.Now;
            },
                cancelToken,
                TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void toolStripButtonLevelUp_ButtonClick(object sender, EventArgs e)
        {
            if (Module.SuperprojectModule != null)
                SetGitModule(this, new GitModuleEventArgs(Module.SuperprojectModule));
            else
                toolStripButtonLevelUp.ShowDropDown();
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
            this.InvokeAsync(OnActivate);
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
                    return;
                if (_terminal == null) // Lazy-create on first opening the tab
                {
                    tabpage.Controls.Clear();
                    tabpage.Controls.Add(
                        _terminal = new ConEmuControl()
                        {
                            Dock = DockStyle.Fill,
                            AutoStartInfo = null,
                            IsStatusbarVisible = false
                        }
                    );
                }
                if (_terminal.IsConsoleEmulatorOpen) // If user has typed "exit" in there, restart the shell; otherwise just return
                    return;

                // Create the terminal
                var startinfo = new ConEmuStartInfo
                {
                    StartupDirectory = Module.WorkingDir,
                    WhenConsoleProcessExits = WhenConsoleProcessExits.CloseConsoleEmulator
                };

                var startinfoBaseConfiguration = startinfo.BaseConfiguration;
                if (!string.IsNullOrWhiteSpace(AppSettings.ConEmuFontSize.ValueOrDefault))
                {
                    int fontSize;
                    if (int.TryParse(AppSettings.ConEmuFontSize.ValueOrDefault, out fontSize))
                    {
                        var nodeFontSize =
                            startinfoBaseConfiguration.SelectSingleNode("/key/key/key/value[@name='FontSize']");
                        if (nodeFontSize?.Attributes != null)
                            nodeFontSize.Attributes["data"].Value = fontSize.ToString("X8");
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
                        string sJustBash = "bash.exe"; // Generic bash, should generally be in the git dir, less configured than the specific git-bash
                        string sJustSh = "sh.exe"; // Fallback to SH
                        exeList = new[] { sJustBash, sJustSh };
                        break;
                }

                string cmdPath = exeList.
                      Select(shell =>
                      {
                          string shellPath;
                          if (PathUtil.TryFindShellPath(shell, out shellPath))
                              return shellPath;
                          return null;
                      }).
                      FirstOrDefault(shellPath => shellPath != null);

                if (cmdPath == null)
                {
                    startinfo.ConsoleProcessCommandLine = ConEmuConstants.DefaultConsoleCommandLine;
                }
                else
                {
                    cmdPath = cmdPath.Quote();
                    if (AppSettings.ConEmuTerminal.ValueOrDefault == "bash")
                        startinfo.ConsoleProcessCommandLine = cmdPath + " --login -i";
                    else
                        startinfo.ConsoleProcessCommandLine = cmdPath;
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
                        startinfo.SetEnv("PATH", dirGit + ";" + "%PATH%");
                }

                _terminal.Start(startinfo);
            };
        }

        public void ChangeTerminalActiveFolder(string path)
        {
            if (_terminal == null || _terminal.RunningSession == null || string.IsNullOrWhiteSpace(path))
                return;

            if (AppSettings.ConEmuTerminal.ValueOrDefault == "bash")
            {
                string posixPath;
                if (PathUtil.TryConvertWindowsPathToPosix(path, out posixPath))
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
            if (_terminal == null || _terminal.RunningSession == null || string.IsNullOrWhiteSpace(command))
                return;

            //Clear terminal line by sending 'backspace' characters
            for (int i = 0; i < 10000; i++)
            {
                _terminal.RunningSession.WriteInputText("\b");
            }
            _terminal.RunningSession.WriteInputText(command + Environment.NewLine);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
#if !__MonoCS__
                if (_commitButton != null)
                    _commitButton.Dispose();
                if (_pushButton != null)
                    _pushButton.Dispose();
                if (_pullButton != null)
                    _pullButton.Dispose();
#endif
                if (_submodulesStatusCts != null)
                    _submodulesStatusCts.Dispose();
                if (_formBrowseMenus != null)
                    _formBrowseMenus.Dispose();
                if (_filterRevisionsHelper != null)
                    _filterRevisionsHelper.Dispose();
                if (_filterBranchHelper != null)
                    _filterBranchHelper.Dispose();

                if (components != null)
                    components.Dispose();
                if (_gitStatusMonitor != null)
                    _gitStatusMonitor.Dispose();
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
    }
}