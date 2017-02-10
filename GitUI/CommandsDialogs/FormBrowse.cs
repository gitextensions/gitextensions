using GitUI.UserControls.RevisionGridClasses;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using ConEmu.WinForms;

using GitCommands;
using GitCommands.Repository;
using GitCommands.Utils;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.Hotkey;
using GitUI.Plugin;
using GitUI.Properties;
using GitUI.Script;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft.Win32;
using ResourceManager;
using Settings = GitCommands.AppSettings;
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

        private readonly TranslationString _saveFileFilterCurrentFormat =
            new TranslationString("Current format");
        private readonly TranslationString _saveFileFilterAllFiles =
            new TranslationString("All files");

        private readonly TranslationString _indexLockDeleted =
            new TranslationString("index.lock deleted.");
        private readonly TranslationString _indexLockNotFound =
            new TranslationString("index.lock not found at:");

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

        private readonly TranslationString directoryIsNotAValidRepositoryCaption =
            new TranslationString("Open");

        private readonly TranslationString directoryIsNotAValidRepository =
            new TranslationString("The selected item is not a valid git repository.\n\nDo you want to abort and remove it from the recent repositories list?");

        private readonly TranslationString _updateCurrentSubmodule =
            new TranslationString("Update current submodule");

        private readonly TranslationString _nodeNotFoundNextAvailableParentSelected =
            new TranslationString("Node not found. The next available parent node will be selected.");

        private readonly TranslationString _nodeNotFoundSelectionNotChanged =
            new TranslationString("Node not found. File tree selection was not changed.");

        private readonly TranslationString _diffNoSelection =
            new TranslationString("Diff (no selection)");

        private readonly TranslationString _diffParentWithSelection =
            new TranslationString("Diff (A: parent --> B: selection)");

        private readonly TranslationString _diffTwoSelected =
            new TranslationString("Diff (A: first --> B: second)");

        private readonly TranslationString _diffNotSupported =
            new TranslationString("Diff (not supported)");

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

        private readonly TranslationString _resetFileCaption =
            new TranslationString("Reset");
        private readonly TranslationString _resetFileText =
            new TranslationString("Are you sure you want to reset this file or directory?");
        private readonly TranslationString _resetFileError =
            new TranslationString("Exactly one revision must be selected. Abort.");

        private readonly TranslationString _buildReportTabCaption =
            new TranslationString("Build Report");
        private readonly TranslationString _consoleTabCaption =
            new TranslationString("Console");
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
        private bool _dontUpdateOnIndexChange;
        private readonly ToolStripGitStatus _toolStripGitStatus;
        private readonly FilterRevisionsHelper _filterRevisionsHelper;
        private readonly FilterBranchHelper _filterBranchHelper;

        private string _diffTabPageTitleBase = "";

        private readonly FormBrowseMenus _formBrowseMenus;
        ConEmuControl terminal = null;
#pragma warning disable 0414
        private readonly FormBrowseMenuCommands _formBrowseMenuCommands;
#pragma warning restore 0414

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
            InitializeComponent();

            toolPanel.SplitterDistance = ToolStrip.PreferredSize.Height;

            // set tab page images
            {
                var imageList = new ImageList();
                CommitInfoTabControl.ImageList = imageList;
                imageList.ColorDepth = ColorDepth.Depth8Bit;
                imageList.Images.Add(global::GitUI.Properties.Resources.IconCommit);
                imageList.Images.Add(global::GitUI.Properties.Resources.IconFileTree);
                imageList.Images.Add(global::GitUI.Properties.Resources.IconDiff);
                CommitInfoTabControl.TabPages[0].ImageIndex = 0;
                CommitInfoTabControl.TabPages[1].ImageIndex = 1;
                CommitInfoTabControl.TabPages[2].ImageIndex = 2;
            }
            this.DiffFiles.FilterVisible = true;
            RevisionGrid.UICommandsSource = this;
            Repositories.LoadRepositoryHistoryAsync();
            Task.Factory.StartNew(PluginLoader.Load)
                .ContinueWith((task) => RegisterPlugins(), TaskScheduler.FromCurrentSynchronizationContext());
            RevisionGrid.GitModuleChanged += SetGitModule;
            _filterRevisionsHelper = new FilterRevisionsHelper(toolStripRevisionFilterTextBox, toolStripRevisionFilterDropDownButton, RevisionGrid, toolStripRevisionFilterLabel, ShowFirstParent, form: this);
            _filterBranchHelper = new FilterBranchHelper(toolStripBranchFilterComboBox, toolStripBranchFilterDropDownButton, RevisionGrid);
            toolStripBranchFilterComboBox.DropDown += toolStripBranches_DropDown_ResizeDropDownWidth;

            Translate();

            if (Settings.ShowGitStatusInBrowseToolbar)
            {
                _toolStripGitStatus = new ToolStripGitStatus
                {
                    ImageTransparentColor = Color.Magenta
                };
                if (aCommands != null)
                    _toolStripGitStatus.UICommandsSource = this;
                _toolStripGitStatus.Click += StatusClick;
                ToolStrip.Items.Insert(ToolStrip.Items.IndexOf(toolStripButton1), _toolStripGitStatus);
                ToolStrip.Items.Remove(toolStripButton1);
                _toolStripGitStatus.CommitTranslatedString = toolStripButton1.Text;
            }

            if (!EnvUtils.RunningOnWindows())
            {
                toolStripSeparator6.Visible = false;
                PuTTYToolStripMenuItem.Visible = false;
            }

            RevisionGrid.SelectionChanged += RevisionGridSelectionChanged;
            DiffText.ExtraDiffArgumentsChanged += DiffTextExtraDiffArgumentsChanged;
            _filterRevisionsHelper.SetFilter(filter);
            DiffText.SetFileLoader(getNextPatchFile);

            GitTree.ImageList = new ImageList();
            GitTree.ImageList.Images.Add(Properties.Resources.New); //File
            GitTree.ImageList.Images.Add(Properties.Resources.Folder); //Folder
            GitTree.ImageList.Images.Add(Properties.Resources.IconFolderSubmodule); //Submodule

            GitTree.MouseDown += GitTree_MouseDown;
            GitTree.MouseMove += GitTree_MouseMove;

            this.HotkeysEnabled = true;
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            this.toolPanel.SplitterDistance = this.ToolStrip.Height;
            this._dontUpdateOnIndexChange = false;
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
            }

            FillBuildReport();  // Ensure correct page visibility
            RevisionGrid.ShowBuildServerInfo = true;

            _formBrowseMenuCommands = new FormBrowseMenuCommands(this);
            _formBrowseMenus = new FormBrowseMenus(menuStrip1);
            RevisionGrid.MenuCommands.MenuChanged += (sender, e) => _formBrowseMenus.OnMenuCommandsPropertyChanged();
            SystemEvents.SessionEnding += (sender, args) => SaveApplicationSettings();

            FillTerminalTab();
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

        private string _oldRevision;
        private GitItemStatus _oldDiffItem;
        private void RefreshRevisions()
        {
            if (RevisionGrid.IsDisposed || DiffFiles.IsDisposed || IsDisposed || Disposing)
            {
                return;
            }

            if (_dashboard == null || !_dashboard.Visible)
            {
                var revisions = RevisionGrid.GetSelectedRevisions();
                if (revisions.Count != 0)
                {
                    _oldRevision = revisions[0].Guid;
                    _oldDiffItem = DiffFiles.SelectedItem;
                }
                else
                {
                    _oldRevision = null;
                    _oldDiffItem = null;
                }
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
                toolPanel.Panel2.Controls.Add(_dashboard);
                _dashboard.Dock = DockStyle.Fill;
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
                _dashboard.Visible = false;
        }

        private void GitTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var item = e.Node.Tag as GitItem;
            if (item == null)
                return;

            if (item.IsBlob)
                FileText.ViewGitItem(item.FileName, item.Guid);
            else if (item.IsCommit)
                FileText.ViewText(item.FileName,
                    LocalizationHelpers.GetSubmoduleText(Module, item.FileName, item.Guid));
            else
                FileText.ViewText("", "");
        }

        private void BrowseLoad(object sender, EventArgs e)
        {
#if !__MonoCS__
            if (EnvUtils.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
            {
                TaskbarManager.Instance.ApplicationId = "GitExtensions";
            }
#endif
            HideVariableMainMenuItems();

            RevisionGrid.Load();
            _filterBranchHelper.InitToolStripBranchFilter();

            Cursor.Current = Cursors.WaitCursor;
            InternalInitialize(false);
            RevisionGrid.Focus();
            RevisionGrid.IndexWatcher.Reset();

            RevisionGrid.IndexWatcher.Changed += _indexWatcher_Changed;

            Cursor.Current = Cursors.Default;


            try
            {
                if (Settings.PlaySpecialStartupSound)
                {
                    using (var cowMoo = Resources.cow_moo)
                        new System.Media.SoundPlayer(cowMoo).Play();
                }
            }
            catch // This code is just for fun, we do not want the program to crash because of it.
            {
            }
        }

        void _indexWatcher_Changed(object sender, IndexChangedEventArgs e)
        {
            bool indexChanged = e.IsIndexChanged;
            this.InvokeAsync(() =>
            {
                RefreshButton.Image = indexChanged && Settings.UseFastChecks && Module.IsValidGitWorkingDir()
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
            if (Settings.LastUpdateCheck.AddDays(7) < DateTime.Now)
            {
                Settings.LastUpdateCheck = DateTime.Now;
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
                _toolStripGitStatus.Enabled = validWorkingDir;
            toolStripButtonPull.Enabled = validWorkingDir;
            toolStripButtonPush.Enabled = validWorkingDir;
            dashboardToolStripMenuItem.Visible = !validWorkingDir;
            repositoryToolStripMenuItem.Visible = validWorkingDir;
            commandsToolStripMenuItem.Visible = validWorkingDir;
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
                resetToolStripMenuItem.Enabled = !bareRepository;
                cleanupToolStripMenuItem.Enabled = !bareRepository;
                stashToolStripMenuItem.Enabled = !bareRepository;
                checkoutBranchToolStripMenuItem.Enabled = !bareRepository;
                mergeBranchToolStripMenuItem.Enabled = !bareRepository;
                rebaseToolStripMenuItem.Enabled = !bareRepository;
                runMergetoolToolStripMenuItem.Enabled = !bareRepository;
                cherryPickToolStripMenuItem.Enabled = !bareRepository;
                checkoutToolStripMenuItem.Enabled = !bareRepository;
                bisectToolStripMenuItem.Enabled = !bareRepository;
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
            Text = GenerateWindowTitle(Module.WorkingDir, validWorkingDir, branchSelect.Text);
            DiffText.Font = Settings.DiffFont;
            UpdateJumplist(validWorkingDir);

            OnActivate();
            // load custom user menu
            LoadUserMenu();

            if (validWorkingDir)
            {
                // add Navigate and View menu
                _formBrowseMenus.ResetMenuCommandSets();
                //// _formBrowseMenus.AddMenuCommandSet(MainMenuItem.NavigateMenu, _formBrowseMenuCommands.GetNavigateMenuCommands()); // not used at the moment
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
        ///
        /// </summary>
        private void SetShortcutKeyDisplayStringsFromHotkeySettings()
        {
            gitBashToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Commands.GitBash).ToShortcutKeyDisplayString();
            commitToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Commands.Commit).ToShortcutKeyDisplayString();
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

                if (Settings.RecentReposComboMinWidth > 0)
                {
                    _NO_TRANSLATE_Workingdir.AutoSize = false;
                    var captionWidth = graphics.MeasureString(_NO_TRANSLATE_Workingdir.Text, _NO_TRANSLATE_Workingdir.Font).Width;
                    captionWidth = captionWidth + _NO_TRANSLATE_Workingdir.DropDownButtonWidth + 5;
                    _NO_TRANSLATE_Workingdir.Width = Math.Max(Settings.RecentReposComboMinWidth, (int)captionWidth);
                }
                else
                    _NO_TRANSLATE_Workingdir.AutoSize = true;
            }
        }

        /// <summary>
        /// Returns a short name for repository.
        /// If the repository contains a description it is returned,
        /// otherwise the last part of path is returned.
        /// </summary>
        /// <param name="repositoryDir">Path to repository.</param>
        /// <returns>Short name for repository</returns>
        private static String GetRepositoryShortName(string repositoryDir)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(repositoryDir);
            if (dirInfo.Exists)
            {
                string desc = ReadRepositoryDescription(repositoryDir);
                if (desc.IsNullOrEmpty())
                {
                    desc = Repositories.RepositoryHistory.Repositories
                        .Where(repo => repo.Path.Equals(repositoryDir, StringComparison.CurrentCultureIgnoreCase)).Select(repo => repo.Title)
                        .FirstOrDefault();
                }
                return desc ?? dirInfo.Name;
            }
            return dirInfo.Name;
        }

        private void LoadUserMenu()
        {
            var scripts = ScriptManager.GetScripts().Where(script => script.Enabled
                && script.OnEvent == ScriptEvent.ShowInUserMenuBar).ToList();

            for (int i = ToolStrip.Items.Count - 1; i >= 0; i--)
                if (ToolStrip.Items[i].Tag != null &&
                    ToolStrip.Items[i].Tag as String == "userscript")
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
            if (ScriptRunner.RunScript(this, Module, ((ToolStripButton)sender).Text, this.RevisionGrid))
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
                    string repositoryDescription = GetRepositoryShortName(Module.WorkingDir);
                    string baseFolder = Path.Combine(Settings.ApplicationDataPath.Value, "Recent");
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

                    string path = Path.Combine(baseFolder, String.Format("{0}.{1}", sb, "gitext"));
                    File.WriteAllText(path, Module.WorkingDir);
                    JumpList.AddToRecent(path);

                    var JList = JumpList.CreateJumpListForIndividualWindow(TaskbarManager.Instance.ApplicationId, Handle);
                    JList.ClearAllUserTasks();

                    //to control which category Recent/Frequent is displayed
                    JList.KnownCategoryToDisplay = JumpListKnownCategoryType.Recent;

                    JList.Refresh();
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
                float r = (float)img.Width / (float)img.Height;

                // set dimensions accordingly to fit inside size^2 square
                if (r > 1)
                { // w is bigger, so divide h by r
                    w = size;
                    h = (int)((float)size / r);
                    x = 0; y = (size - h) / 2; // center the image
                }
                else
                { // h is bigger, so multiply w by r
                    w = (int)((float)size * r);
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
            if (Settings.ShowStashCount)
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
                      !Directory.Exists(Module.GetGitDirectory() + "rebase-apply\\"),
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

        /// <summary>
        /// Generates main window title according to given repository.
        /// </summary>
        /// <param name="workingDir">Path to repository.</param>
        /// <param name="isWorkingDirValid">If the given path contains valid repository.</param>
        /// <param name="branchName">Current branch name.</param>
        private string GenerateWindowTitle(string workingDir, bool isWorkingDirValid, string branchName)
        {
#if DEBUG
            const string defaultTitle = "Git Extensions -> DEBUG <-";
            const string repositoryTitleFormat = "{0} ({1}) - Git Extensions -> DEBUG <-";
#else
            const string defaultTitle = "Git Extensions";
            const string repositoryTitleFormat = "{0} ({1}) - Git Extensions";
#endif
            if (!isWorkingDirValid)
                return defaultTitle;
            string repositoryDescription = GetRepositoryShortName(workingDir);
            if (string.IsNullOrEmpty(branchName))
                branchName = _noBranchTitle.Text;
            return string.Format(repositoryTitleFormat, repositoryDescription, branchName.Trim('(', ')'));
        }

        /// <summary>
        /// Reads repository description's first line from ".git\description" file.
        /// </summary>
        /// <param name="workingDir">Path to repository.</param>
        /// <returns>If the repository has description, returns that description, else returns <c>null</c>.</returns>
        private static string ReadRepositoryDescription(string workingDir)
        {
            const string repositoryDescriptionFileName = "description";
            const string defaultDescription = "Unnamed repository; edit this file 'description' to name the repository.";

            var repositoryPath = GitModule.GetGitDirectory(workingDir);
            var repositoryDescriptionFilePath = Path.Combine(repositoryPath, repositoryDescriptionFileName);
            if (!File.Exists(repositoryDescriptionFilePath))
                return null;
            try
            {
                var repositoryDescription = File.ReadLines(repositoryDescriptionFilePath).FirstOrDefault();
                return string.Equals(repositoryDescription, defaultDescription, StringComparison.CurrentCulture)
                           ? null
                           : repositoryDescription;
            }
            catch (IOException)
            {
                return null;
            }
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
                FillBuildReport();
            }
            RevisionGrid.IndexWatcher.Reset();
        }

        //store strings to not keep references to nodes
        private readonly Stack<string> lastSelectedNodes = new Stack<string>();

        private void FillFileTree()
        {
            if (CommitInfoTabControl.SelectedTab != TreeTabPage)
                return;

            if (_selectedRevisionUpdatedTargets.HasFlag(UpdateTargets.FileTree))
                return;

            _selectedRevisionUpdatedTargets |= UpdateTargets.FileTree;

            try
            {
                GitTree.SuspendLayout();
                // Save state only when there is selected node
                if (GitTree.SelectedNode != null)
                {
                    TreeNode node = GitTree.SelectedNode;
                    FileText.SaveCurrentScrollPos();
                    lastSelectedNodes.Clear();
                    while (node != null)
                    {
                        lastSelectedNodes.Push(node.Text);
                        node = node.Parent;
                    }
                }

                // Refresh tree
                GitTree.Nodes.Clear();
                //restore selected file and scroll position when new selection is done
                if (RevisionGrid.GetSelectedRevisions().Count > 0)
                {
                    LoadInTree(RevisionGrid.GetSelectedRevisions()[0].SubItems, GitTree.Nodes);
                    //GitTree.Sort();
                    TreeNode lastMatchedNode = null;
                    // Load state
                    var currenNodes = GitTree.Nodes;
                    TreeNode matchedNode = null;
                    while (lastSelectedNodes.Count > 0 && currenNodes != null)
                    {
                        var next = lastSelectedNodes.Pop();
                        foreach (TreeNode node in currenNodes)
                        {
                            if (node.Text != next && next.Length != 40)
                                continue;

                            node.Expand();
                            matchedNode = node;
                            break;
                        }
                        if (matchedNode == null)
                            currenNodes = null;
                        else
                        {
                            lastMatchedNode = matchedNode;
                            currenNodes = matchedNode.Nodes;
                        }
                    }
                    //if there is no exact match, don't restore scroll position
                    if (lastMatchedNode != matchedNode)
                        FileText.ResetCurrentScrollPos();
                    GitTree.SelectedNode = lastMatchedNode;
                }
                if (GitTree.SelectedNode == null)
                {
                    FileText.ViewText("", "");
                }
            }
            finally
            {
                GitTree.ResumeLayout();
            }
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

            var revisions = RevisionGrid.GetSelectedRevisions();

            DiffText.SaveCurrentScrollPos();

            DiffFiles.SetDiffs(revisions);
            if (_oldDiffItem != null && revisions.Count > 0 && revisions[0].Guid == _oldRevision)
            {
                DiffFiles.SelectedItem = _oldDiffItem;
                _oldDiffItem = null;
                _oldRevision = null;
            }

            switch (revisions.Count)
            {
                case 0:
                    DiffTabPage.Text = _diffNoSelection.Text;
                    break;

                case 1: // diff "parent" --> "selected revision"
                    var revision = revisions[0];
                    if (revision != null && revision.ParentGuids != null && revision.ParentGuids.Length != 0)
                        DiffTabPage.Text = _diffParentWithSelection.Text;
                    break;

                case 2: // diff "first clicked revision" --> "second clicked revision"
                    bool artificialRevSelected = revisions[0].IsArtificial() || revisions[1].IsArtificial();
                    if (!artificialRevSelected)
                        DiffTabPage.Text = _diffTwoSelected.Text;
                    break;

                default: // more than 2 revisions selected => no diff
                    DiffTabPage.Text = _diffNotSupported.Text;
                    break;
            }
        }

        private void FillCommitInfo()
        {
            if (CommitInfoTabControl.SelectedTab != CommitInfoTabPage)
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

        private BuildReportTabPageExtension BuildReportTabPageExtension;

        private void FillBuildReport()
        {
            if (EnvUtils.IsMonoRuntime())
                return;

            var selectedRevisions = RevisionGrid.GetSelectedRevisions();
            var revision = selectedRevisions.Count == 1 ? selectedRevisions.Single() : null;

            if (BuildReportTabPageExtension == null)
                BuildReportTabPageExtension = new BuildReportTabPageExtension(CommitInfoTabControl, _buildReportTabCaption.Text);

            BuildReportTabPageExtension.FillBuildReport(revision);
        }

        public void fileHistoryItem_Click(object sender, EventArgs e)
        {
            var item = GitTree.SelectedNode.Tag as GitItem;

            if (item == null)
                return;

            IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

            if (revisions.Count == 0 || GitRevision.IsArtificial(revisions[0].Guid))
                UICommands.StartFileHistoryDialog(this, item.FileName);
            else
                UICommands.StartFileHistoryDialog(this, item.FileName, revisions[0], false, false);
        }

        private void blameMenuItem_Click(object sender, EventArgs e)
        {
            var item = GitTree.SelectedNode.Tag as GitItem;

            if (item == null)
                return;

            IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

            if (revisions.Count == 0 || GitRevision.IsArtificial(revisions[0].Guid))
                UICommands.StartFileHistoryDialog(this, item.FileName, null, false, true);
            else
                UICommands.StartFileHistoryDialog(this, item.FileName, revisions[0], true, true);
        }

        public void FindFileOnClick(object sender, EventArgs e)
        {
            string selectedItem;
            using (var searchWindow = new SearchWindow<string>(FindFileMatches)
            {
                Owner = this
            })
            {
                searchWindow.ShowDialog(this);
                selectedItem = searchWindow.SelectedItem;
            }
            if (string.IsNullOrEmpty(selectedItem))
            {
                return;
            }

            string[] items = selectedItem.Split(new[] { '/' });
            TreeNodeCollection nodes = GitTree.Nodes;

            for (int i = 0; i < items.Length - 1; i++)
            {
                TreeNode selectedNode = Find(nodes, items[i]);

                if (selectedNode == null)
                {
                    return; //Item does not exist in the tree
                }

                selectedNode.Expand();
                nodes = selectedNode.Nodes;
            }

            var lastItem = Find(nodes, items[items.Length - 1]);
            if (lastItem != null)
            {
                GitTree.SelectedNode = lastItem;
            }
        }

        private static TreeNode Find(TreeNodeCollection nodes, string label)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Text == label)
                {
                    return nodes[i];
                }
            }
            return null;
        }

        private IList<string> FindFileMatches(string name)
        {
            var candidates = Module.GetFullTree(RevisionGrid.GetSelectedRevisions()[0].TreeGuid);

            string nameAsLower = name.ToLower();

            return candidates.Where(fileName => fileName.ToLower().Contains(nameAsLower)).ToList();
        }

        private string SaveSelectedItemToTempFile()
        {
            var gitItem = GitTree.SelectedNode.Tag as GitItem;
            if (gitItem == null || !gitItem.IsBlob)
                return null;

            var fileName = gitItem.FileName;
            if (fileName.Contains("\\") && fileName.LastIndexOf("\\") < fileName.Length)
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
            if (fileName.Contains("/") && fileName.LastIndexOf("/") < fileName.Length)
                fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);

            fileName = (Path.GetTempPath() + fileName).ToNativePath();
            Module.SaveBlobAs(fileName, gitItem.Guid);
            return fileName;
        }

        public void OpenWithOnClick(object sender, EventArgs e)
        {
            var fileName = SaveSelectedItemToTempFile();
            if (fileName != null)
                OsShellUtil.OpenAs(fileName);
        }

        public void OpenOnClick(object sender, EventArgs e)
        {
            try
            {
                var fileName = SaveSelectedItemToTempFile();
                if (fileName != null)
                    Process.Start(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void FileTreeContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var gitItem = (GitTree.SelectedNode != null) ? GitTree.SelectedNode.Tag as GitItem : null;
            var enableItems = gitItem != null && gitItem.IsBlob;

            saveAsToolStripMenuItem.Enabled = enableItems;
            openFileToolStripMenuItem.Enabled = enableItems;
            openFileWithToolStripMenuItem.Enabled = enableItems;
            openWithToolStripMenuItem.Enabled = enableItems;
            copyFilenameToClipboardToolStripMenuItem.Enabled = gitItem != null && FormBrowseUtil.IsFileOrDirectory(FormBrowseUtil.GetFullPathFromGitItem(Module, gitItem));
            editCheckedOutFileToolStripMenuItem.Enabled = enableItems;
        }

        protected void LoadInTree(IEnumerable<IGitItem> items, TreeNodeCollection node)
        {
            var sortedItems = items.OrderBy(gi => gi, new GitFileTreeComparer());

            foreach (var item in sortedItems)
            {
                var subNode = node.Add(item.Name);
                subNode.Tag = item;

                var gitItem = item as GitItem;

                if (gitItem == null)
                    subNode.Nodes.Add(new TreeNode());
                else
                {
                    if (gitItem.IsTree)
                    {
                        subNode.ImageIndex = 1;
                        subNode.SelectedImageIndex = 1;
                        subNode.Nodes.Add(new TreeNode());
                    }
                    else
                        if (gitItem.IsCommit)
                    {
                        subNode.ImageIndex = 2;
                        subNode.SelectedImageIndex = 2;
                        subNode.Text = item.Name + " (Submodule)";
                    }
                }
            }
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

                var revisions = RevisionGrid.GetSelectedRevisions();

                if (revisions.Any() && GitRevision.IsArtificial(revisions[0].Guid))
                {
                    CommitInfoTabControl.RemoveIfExists(CommitInfoTabPage);
                    CommitInfoTabControl.RemoveIfExists(TreeTabPage);
                }
                else
                {
                    CommitInfoTabControl.InsertIfNotExists(0, CommitInfoTabPage);
                    CommitInfoTabControl.InsertIfNotExists(1, TreeTabPage);
                }

                //RevisionGrid.HighlightSelectedBranch();

                FillFileTree();
                FillDiff();
                FillCommitInfo();
                FillBuildReport();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void OpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitModule module = FormOpenDirectory.OpenModule(this);
            if (module != null)
                SetGitModule(this, new GitModuleEventArgs(module));
        }

        private void CheckoutToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCheckoutRevisionDialog(this);
        }

        private void GitTreeDoubleClick(object sender, EventArgs e)
        {
            OnItemActivated();
        }

        private void OnItemActivated()
        {
            if (GitTree.SelectedNode == null || !(GitTree.SelectedNode.Tag is IGitItem))
                return;

            var item = GitTree.SelectedNode.Tag as GitItem;
            if (item == null)
                return;

            if (item.IsBlob)
            {
                UICommands.StartFileHistoryDialog(this, item.FileName, null);
            }
            else if (item.IsCommit)
            {
                Process process = new Process();
                process.StartInfo.FileName = Application.ExecutablePath;
                process.StartInfo.Arguments = "browse";
                process.StartInfo.WorkingDirectory = Path.Combine(Module.WorkingDir, item.FileName.EnsureTrailingPathSeparator());
                process.Start();
            }
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
                if (Module.LastPullAction == Settings.PullAction.None)
                {
                    bSilent = (ModifierKeys & Keys.Shift) != 0;
                }
                else if (Module.LastPullAction == Settings.PullAction.FetchAll)
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
            var revisions = RevisionGrid.GetSelectedRevisions();
            if (revisions.Count != 1)
            {
                MessageBox.Show("Select exactly one revision.");
                return;
            }

            UICommands.StartCherryPickDialog(this, revisions.First());
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
            var translation = Settings.Translation;
            UICommands.StartSettingsDialog(this);
            if (translation != Settings.Translation)
                Translate();

            this.Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            RevisionGrid.ReloadHotkeys();
            RevisionGrid.ReloadTranslation();
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
            SaveUserMenuPosition();
            SaveApplicationSettings();
        }

        private static void SaveApplicationSettings()
        {
            Properties.Settings.Default.Save();
        }

        private void SaveUserMenuPosition()
        {
            GitCommands.AppSettings.UserMenuLocationX = UserMenuToolStrip.Location.X;
            GitCommands.AppSettings.UserMenuLocationY = UserMenuToolStrip.Location.Y;
        }

        private void EditGitignoreToolStripMenuItem1Click(object sender, EventArgs e)
        {
            UICommands.StartEditGitIgnoreDialog(this);
        }

        private void ArchiveToolStripMenuItemClick(object sender, EventArgs e)
        {
            var revisions = RevisionGrid.GetSelectedRevisions();
            if (revisions.Count > 2)
            {
                MessageBox.Show(this, "Select only one or two revisions. Abort.", "Archive revision");
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
            var fileName = Path.Combine(Module.GetGitDirectory(), "config");
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
            Module.RunExternalCmdDetached(Settings.Pageant, "");
        }

        private void GenerateOrImportKeyToolStripMenuItemClick(object sender, EventArgs e)
        {
            Module.RunExternalCmdDetached(Settings.Puttygen, "");
        }

        private void TabControl1SelectedIndexChanged(object sender, EventArgs e)
        {
            FillFileTree();
            FillDiff();
            FillCommitInfo();
            FillBuildReport();
        }

        private void DiffFilesSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_dontUpdateOnIndexChange)
                ShowSelectedFileDiff();
        }

        private void ShowSelectedFileDiff()
        {
            if (DiffFiles.SelectedItem == null)
            {
                DiffText.ViewPatch("");
                return;
            }

            IList<GitRevision> items = RevisionGrid.GetSelectedRevisions();
            if (items.Count() == 1)
            {
                items.Add(new GitRevision(Module, DiffFiles.SelectedItemParent));

                if (!string.IsNullOrWhiteSpace(DiffFiles.SelectedItemParent)
                    && DiffFiles.SelectedItemParent == DiffFiles.CombinedDiff.Text)
                {
                    var diffOfConflict = Module.GetCombinedDiffContent(items.First(), DiffFiles.SelectedItem.Name,
                        DiffText.GetExtraDiffArguments(), DiffText.Encoding);

                    if (string.IsNullOrWhiteSpace(diffOfConflict))
                    {
                        diffOfConflict = Strings.GetUninterestingDiffOmitted();
                    }

                    DiffText.ViewPatch(diffOfConflict);
                    return;
                }
            }
            DiffText.ViewChanges(items, DiffFiles.SelectedItem, String.Empty);
        }

        private void ChangelogToolStripMenuItemClick(object sender, EventArgs e)
        {
            using (var frm = new FormChangeLog()) frm.ShowDialog(this);
        }

        private void DiffFilesDoubleClick(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null)
                return;

            UICommands.StartFileHistoryDialog(this, (DiffFiles.SelectedItem).Name);
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
            var submodule = (sender as ToolStripMenuItem).Tag as string;
            FormProcess.ShowDialog(this, Module.SuperprojectModule,
                GitCommandHelpers.SubmoduleUpdateCmd(submodule));
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

        private void GitTreeMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                GitTree.SelectedNode = GitTree.GetNodeAt(e.X, e.Y);
        }

        private void UserManualToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://git-extensions-documentation.readthedocs.org/en/release-2.49/");
            }
            catch (System.ComponentModel.Win32Exception)
            {
            }
        }

        private void DiffTextExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ShowSelectedFileDiff();
        }

        private void CleanupToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCleanupRepositoryDialog(this);
        }

        private void openWithDifftoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null)
                return;

            var selectedItem = DiffFiles.SelectedItem;
            GitUIExtensions.DiffWithRevisionKind diffKind;

            if (sender == aLocalToolStripMenuItem)
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffALocal;
            else if (sender == bLocalToolStripMenuItem)
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffBLocal;
            else if (sender == parentOfALocalToolStripMenuItem)
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffAParentLocal;
            else if (sender == parentOfBLocalToolStripMenuItem)
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffBParentLocal;
            else
            {
                Debug.Assert(sender == aBToolStripMenuItem, "Not implemented DiffWithRevisionKind: " + sender);
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffAB;
            }

            string parentGuid = RevisionGrid.GetSelectedRevisions().Count() == 1 ? DiffFiles.SelectedItemParent : null;

            RevisionGrid.OpenWithDifftool(selectedItem.Name, selectedItem.OldName, diffKind, parentGuid);
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
                if (mostRecentRepos.Count > 0 && (Settings.SortMostRecentRepos || Settings.SortLessRecentRepos))
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

        }

        private void SetWorkingDir(string path)
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
                Settings.RecentWorkingDir = module.WorkingDir;
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
            Process.Start(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "TranslationApp.exe"));
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

        public void SaveAsOnClick(object sender, EventArgs e)
        {
            var item = GitTree.SelectedNode.Tag as GitItem;

            if (item == null)
                return;
            if (!item.IsBlob)
                return;

            var fullName = Path.Combine(Module.WorkingDir, item.FileName);
            using (var fileDialog =
                new SaveFileDialog
                {
                    InitialDirectory = Path.GetDirectoryName(fullName),
                    FileName = Path.GetFileName(fullName),
                    DefaultExt = GitCommandHelpers.GetFileExtension(fullName),
                    AddExtension = true
                })
            {
                fileDialog.Filter =
                    _saveFileFilterCurrentFormat.Text + " (*." +
                    GitCommandHelpers.GetFileExtension(fileDialog.FileName) + ")|*." +
                    GitCommandHelpers.GetFileExtension(fileDialog.FileName) +
                    "|" + _saveFileFilterAllFiles.Text + " (*.*)|*.*";

                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    Module.SaveBlobAs(fileDialog.FileName, item.Guid);
                }
            }
        }

        private void ResetToThisRevisionOnClick(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

            if (!revisions.Any() || revisions.Count != 1)
            {
                MessageBox.Show(_resetFileError.Text, _resetFileCaption.Text);
                return;
            }

            if (MessageBox.Show(_resetFileText.Text, _resetFileCaption.Text, MessageBoxButtons.OKCancel)
                == System.Windows.Forms.DialogResult.OK)
            {
                var item = GitTree.SelectedNode.Tag as GitItem;
                var files = new List<string> { item.FileName };
                Module.CheckoutFiles(files, revisions.First().Guid, false);
            }
        }

        private void GitTreeBeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.IsExpanded)
                return;

            var item = (IGitItem)e.Node.Tag;

            e.Node.Nodes.Clear();
            LoadInTree(item.SubItems, e.Node.Nodes);
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

        private void copyFilenameToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gitItem = GitTree.SelectedNode.Tag as GitItem;
            if (gitItem == null)
                return;

            var fileName = Path.Combine(Module.WorkingDir, (gitItem).FileName);
            Clipboard.SetText(fileName.ToNativePath());
        }

        private void copyFilenameToClipboardToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CopyFullPathToClipboard(DiffFiles, Module);
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
            string fileName = Path.Combine(Module.GetGitDirectory(), "index.lock");

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
                MessageBox.Show(this, _indexLockDeleted.Text);
            }
            else
                MessageBox.Show(this, _indexLockNotFound.Text + " " + fileName);
        }

        private void saveAsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

            if (revisions.Count == 0)
                return;

            if (DiffFiles.SelectedItem == null)
                return;

            GitItemStatus item = DiffFiles.SelectedItem;

            var fullName = Path.Combine(Module.WorkingDir, item.Name);
            using (var fileDialog =
                new SaveFileDialog
                {
                    InitialDirectory = Path.GetDirectoryName(fullName),
                    FileName = Path.GetFileName(fullName),
                    DefaultExt = GitCommandHelpers.GetFileExtension(fullName),
                    AddExtension = true
                })
            {
                fileDialog.Filter =
                    _saveFileFilterCurrentFormat.Text + " (*." +
                    fileDialog.DefaultExt + ")|*." +
                    fileDialog.DefaultExt +
                    "|" + _saveFileFilterAllFiles.Text + " (*.*)|*.*";

                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    Module.SaveBlobAs(fileDialog.FileName, string.Format("{0}:\"{1}\"", revisions[0].Guid, item.Name));
                }
            }
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            statusStrip.Hide();
        }

        private void openWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = GitTree.SelectedNode.Tag;

            var gitItem = item as GitItem;
            if (gitItem == null || !(gitItem).IsBlob)
                return;

            var fileName = Path.Combine(Module.WorkingDir, (gitItem).FileName);
            OsShellUtil.OpenAs(fileName.ToNativePath());
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

        private void fileHistoryDiffToolstripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedItem;

            if (item.IsTracked)
            {
                IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

                if (revisions.Count == 0 || GitRevision.IsArtificial(revisions[0].Guid))
                    UICommands.StartFileHistoryDialog(this, item.Name);
                else
                    UICommands.StartFileHistoryDialog(this, item.Name, revisions[0], false);
            }
        }

        private void blameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedItem;

            if (item.IsTracked)
            {
                IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

                if (revisions.Count == 0 || GitRevision.IsArtificial(revisions[0].Guid))
                    UICommands.StartFileHistoryDialog(this, item.Name, null, false, true);
                else
                    UICommands.StartFileHistoryDialog(this, item.Name, revisions[0], true, true);
            }
        }

        private void CurrentBranchDropDownOpening(object sender, EventArgs e)
        {
            branchSelect.DropDownItems.Clear();

            ToolStripMenuItem item = new ToolStripMenuItem(checkoutBranchToolStripMenuItem.Text);
            item.ShortcutKeys = checkoutBranchToolStripMenuItem.ShortcutKeys;
            item.ShortcutKeyDisplayString = checkoutBranchToolStripMenuItem.ShortcutKeyDisplayString;
            branchSelect.DropDownItems.Add(item);
            item.Click += (hs, he) => CheckoutBranchToolStripMenuItemClick(hs, he);

            branchSelect.DropDownItems.Add(new ToolStripSeparator());

            foreach (var branch in Module.GetRefs(false))
            {
                var toolStripItem = branchSelect.DropDownItems.Add(branch.Name);
                toolStripItem.Click += BranchSelectToolStripItem_Click;

                //Make sure there are never more than 100 branches added to the menu
                //GitExtensions will hang when the drop down is to large...
                if (branchSelect.DropDownItems.Count > 100)
                    break;
            }

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

        public const string HotkeySettingsName = "Browse";

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
            CloseRepositry,
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
            GitTree.Focus();
            FindFileOnClick(null, null);
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
                case Commands.FocusFileTree: CommitInfoTabControl.SelectedTab = TreeTabPage; GitTree.Focus(); break;
                case Commands.FocusDiff: CommitInfoTabControl.SelectedTab = DiffTabPage; DiffFiles.Focus(); break;
                case Commands.Commit: CommitToolStripMenuItemClick(null, null); break;
                case Commands.AddNotes: AddNotes(); break;
                case Commands.FindFileInSelectedCommit: FindFileInSelectedCommit(); break;
                case Commands.CheckoutBranch: CheckoutBranchToolStripMenuItemClick(null, null); break;
                case Commands.QuickFetch: QuickFetch(); break;
                case Commands.QuickPull:
                    UICommands.StartPullDialog(this, true);
                    break;
                case Commands.QuickPush:
                    UICommands.StartPushDialog(this, true);
                    break;
                case Commands.RotateApplicationIcon: RotateApplicationIcon(); break;
                case Commands.CloseRepositry: CloseToolStripMenuItemClick(null, null); break;
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
            EnabledSplitViewLayout(MainSplitContainer.Panel2.Height == 0 && MainSplitContainer.Height > 0);
        }

        private void EnabledSplitViewLayout(bool enabled)
        {
            if (enabled)
                MainSplitContainer.SplitterDistance = (MainSplitContainer.Height / 5) * 2;
            else
                MainSplitContainer.SplitterDistance = MainSplitContainer.Height;
        }

        private void editCheckedOutFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = GitTree.SelectedNode.Tag;

            var gitItem = item as GitItem;
            if (gitItem == null || !gitItem.IsBlob)
                return;

            var fileName = Path.Combine(Module.WorkingDir, (gitItem).FileName);
            UICommands.StartFileEditorDialog(fileName);
        }

        #region Git file tree drag-drop
        private Rectangle gitTreeDragBoxFromMouseDown;

        private void GitTree_MouseDown(object sender, MouseEventArgs e)
        {
            //DRAG
            if (e.Button == MouseButtons.Left)
            {
                // Remember the point where the mouse down occurred.
                // The DragSize indicates the size that the mouse can move
                // before a drag event should be started.
                Size dragSize = SystemInformation.DragSize;

                // Create a rectangle using the DragSize, with the mouse position being
                // at the center of the rectangle.
                gitTreeDragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                                e.Y - (dragSize.Height / 2)),
                                                                dragSize);
            }
        }

        void GitTree_MouseMove(object sender, MouseEventArgs e)
        {
            TreeView gitTree = (TreeView)sender;

            //DRAG
            // If the mouse moves outside the rectangle, start the drag.
            if (gitTreeDragBoxFromMouseDown != Rectangle.Empty &&
                !gitTreeDragBoxFromMouseDown.Contains(e.X, e.Y))
            {
                StringCollection fileList = new StringCollection();

                //foreach (GitItemStatus item in SelectedItems)
                if (gitTree.SelectedNode != null)
                {
                    GitItem item = gitTree.SelectedNode.Tag as GitItem;
                    if (item != null)
                    {
                        string fileName = Path.Combine(Module.WorkingDir, item.FileName);

                        fileList.Add(fileName.ToNativePath());
                    }

                    DataObject obj = new DataObject();
                    obj.SetFileDropList(fileList);

                    // Proceed with the drag and drop, passing in the list item.
                    DoDragDrop(obj, DragDropEffects.Copy);
                    gitTreeDragBoxFromMouseDown = Rectangle.Empty;
                }
            }
        }
        #endregion

        private int getNextIdx(int curIdx, int maxIdx, bool searchBackward)
        {
            if (searchBackward)
            {
                if (curIdx == 0)
                {
                    curIdx = maxIdx;
                }
                else
                {
                    curIdx--;
                }
            }
            else
            {
                if (curIdx == maxIdx)
                {
                    curIdx = 0;
                }
                else
                {
                    curIdx++;
                }
            }
            return curIdx;
        }

        private Tuple<int, string> getNextPatchFile(bool searchBackward)
        {
            var revisions = RevisionGrid.GetSelectedRevisions();
            if (revisions.Count == 0)
                return null;
            int idx = DiffFiles.SelectedIndex;
            if (idx == -1)
                return new Tuple<int, string>(idx, null);

            idx = getNextIdx(idx, DiffFiles.GitItemStatuses.Count() - 1, searchBackward);
            _dontUpdateOnIndexChange = true;
            DiffFiles.SelectedIndex = idx;
            _dontUpdateOnIndexChange = false;
            return new Tuple<int, string>(idx, DiffText.GetSelectedPatch(RevisionGrid, DiffFiles.SelectedItem));
        }

        //
        // diff context menu
        //
        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenContainingFolder(DiffFiles, Module);
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

        /// <summary>
        /// TODO: move logic to other source file?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void diffShowInFileTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var diffGitItemStatus = DiffFiles.SelectedItems.First();

            ExecuteCommand((int)Commands.FocusFileTree); // switch to view (and fills the first level of file tree data model if not already done)

            var currentNodes = GitTree.Nodes;
            TreeNode foundNode = null;
            bool isIncompleteMatch = false;
            var pathParts = UtilGetPathParts(diffGitItemStatus.Name);
            for (int i = 0; i < pathParts.Length; i++)
            {
                string pathPart = pathParts[i];
                string diffPathPart = pathPart.ToNativePath();

                var currentFoundNode = currentNodes.Cast<TreeNode>().FirstOrDefault(a =>
                {
                    var treeGitItem = a.Tag as GitItem;
                    if (treeGitItem != null)
                    {
                        // TODO: what about case(in)sensitive handling?
                        return treeGitItem.Name == diffPathPart;
                    }
                    else
                    {
                        return false;
                    }
                });

                if (currentFoundNode == null)
                {
                    isIncompleteMatch = true;
                    break;
                }

                foundNode = currentFoundNode;

                if (i < pathParts.Length - 1) // if not the last path part...
                {
                    foundNode.Expand(); // load more data

                    if (currentFoundNode.Nodes == null)
                    {
                        isIncompleteMatch = true;
                        break;
                    }

                    currentNodes = currentFoundNode.Nodes;
                }
            }

            if (foundNode != null)
            {
                if (isIncompleteMatch)
                {
                    MessageBox.Show(_nodeNotFoundNextAvailableParentSelected.Text);
                }

                GitTree.SelectedNode = foundNode;
                GitTree.SelectedNode.EnsureVisible();
            }
            else
            {
                MessageBox.Show(_nodeNotFoundSelectionNotChanged.Text);
            }
        }

        private string[] UtilGetPathParts(string path)
        {
            return path.Split('/');
        }

        private void fileTreeOpenContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gitItem = GitTree.SelectedNode.Tag as GitItem;
            if (gitItem == null)
            {
                return;
            }

            var filePath = FormBrowseUtil.GetFullPathFromGitItem(Module, gitItem);
            FormBrowseUtil.ShowFileOrFolderInFileExplorer(filePath);
        }

        private void fileTreeArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedRevisions = RevisionGrid.GetSelectedRevisions();
            if (selectedRevisions.Count != 1)
            {
                MessageBox.Show("Select exactly one revision.");
                return;
            }

            var gitItem = (GitItem)GitTree.SelectedNode.Tag;
            UICommands.StartArchiveDialog(this, selectedRevisions.First(), null, gitItem.FileName);
        }

        private void fileTreeCleanWorkingTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gitItem = (GitItem)GitTree.SelectedNode.Tag;
            UICommands.StartCleanupRepositoryDialog(this, gitItem.FileName + "/"); // the trailing / marks a directory
        }

        private void DiffContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool artificialRevSelected;

            IList<GitRevision> selectedRevisions = RevisionGrid.GetSelectedRevisions();

            if (selectedRevisions.Count == 0)
                artificialRevSelected = false;
            else
                artificialRevSelected = selectedRevisions[0].IsArtificial();
            if (selectedRevisions.Count > 1)
                artificialRevSelected = artificialRevSelected || selectedRevisions[selectedRevisions.Count - 1].IsArtificial();

            // disable items that need exactly one selected item
            bool isExcactlyOneItemSelected = DiffFiles.SelectedItems.Count() == 1;
            var isCombinedDiff = isExcactlyOneItemSelected &&
                DiffFiles.CombinedDiff.Text == DiffFiles.SelectedItemParent;
            var enabled = isExcactlyOneItemSelected && !isCombinedDiff;
            openWithDifftoolToolStripMenuItem.Enabled = enabled;
            saveAsToolStripMenuItem1.Enabled = enabled;
            cherryPickSelectedDiffFileToolStripMenuItem.Enabled = enabled;
            diffShowInFileTreeToolStripMenuItem.Enabled = isExcactlyOneItemSelected;
            fileHistoryDiffToolstripMenuItem.Enabled = isExcactlyOneItemSelected;
            blameToolStripMenuItem.Enabled = isExcactlyOneItemSelected;
            resetFileToToolStripMenuItem.Enabled = !isCombinedDiff;

            // openContainingFolderToolStripMenuItem.Enabled or not
            {
                openContainingFolderToolStripMenuItem.Enabled = false;

                foreach (var item in DiffFiles.SelectedItems)
                {
                    string filePath = FormBrowseUtil.GetFullPathFromGitItemStatus(Module, item);
                    if (FormBrowseUtil.FileOrParentDirectoryExists(filePath))
                    {
                        openContainingFolderToolStripMenuItem.Enabled = true;
                        break;
                    }
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (_dashboard != null)
                _dashboard.SaveSplitterPositions();
        }

        protected override void OnClosed(EventArgs e)
        {
            SetWorkingDir("");

            base.OnClosed(e);
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

        private void expandAllStripMenuItem_Click(object sender, EventArgs e)
        {
            GitTree.ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitTree.CollapseAll();
        }

        private void DiffFiles_DataSourceChanged(object sender, EventArgs e)
        {
            if (DiffFiles.GitItemStatuses == null || !DiffFiles.GitItemStatuses.Any())
                DiffText.ViewPatch(String.Empty);
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

        private void findInDiffToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var candidates = DiffFiles.GitItemStatuses;

            Func<string, IList<GitItemStatus>> FindDiffFilesMatches = (string name) =>
            {

                string nameAsLower = name.ToLower();

                return candidates.Where(item =>
                {
                    return item.Name != null && item.Name.ToLower().Contains(nameAsLower)
                        || item.OldName != null && item.OldName.ToLower().Contains(nameAsLower);
                }
                    ).ToList();
            };

            GitItemStatus selectedItem;
            using (var searchWindow = new SearchWindow<GitItemStatus>(FindDiffFilesMatches)
            {
                Owner = this
            })
            {
                searchWindow.ShowDialog(this);
                selectedItem = searchWindow.SelectedItem;
            }
            if (selectedItem != null)
            {
                DiffFiles.SelectedItem = selectedItem;
            }
        }

        private void dontSetAsDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.SetNextPullActionAsDefault = !setNextPullActionAsDefaultToolStripMenuItem.Checked;
            setNextPullActionAsDefaultToolStripMenuItem.Checked = Settings.SetNextPullActionAsDefault;
        }

        private void doPullAction(Action action)
        {
            var actLactPullAction = Module.LastPullAction;
            try
            {
                action();
            }
            finally
            {
                if (!Settings.SetNextPullActionAsDefault)
                {
                    Module.LastPullAction = actLactPullAction;
                    Module.LastPullActionToFormPullAction();
                }
                Settings.SetNextPullActionAsDefault = false;
                RefreshPullIcon();
            }
        }

        private void mergeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doPullAction(() =>
                {
                    Module.LastPullAction = Settings.PullAction.Merge;
                    PullToolStripMenuItemClick(sender, e);
                }
            );
        }

        private void rebaseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            doPullAction(() =>
            {
                Module.LastPullAction = Settings.PullAction.Rebase;
                PullToolStripMenuItemClick(sender, e);
            }
            );
        }

        private void fetchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doPullAction(() =>
            {
                Module.LastPullAction = Settings.PullAction.Fetch;
                PullToolStripMenuItemClick(sender, e);
            }
            );
        }

        private void pullToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Settings.SetNextPullActionAsDefault)
                Module.LastPullAction = Settings.PullAction.None;
            PullToolStripMenuItemClick(sender, e);

            //restore Settings.FormPullAction value
            if (!Settings.SetNextPullActionAsDefault)
                Module.LastPullActionToFormPullAction();

            Settings.SetNextPullActionAsDefault = false;
        }

        private void RefreshPullIcon()
        {
            switch (Module.LastPullAction)
            {
                case Settings.PullAction.Fetch:
                    toolStripButtonPull.Image = Properties.Resources.PullFetch;
                    toolStripButtonPull.ToolTipText = _pullFetch.Text;
                    break;

                case Settings.PullAction.FetchAll:
                    toolStripButtonPull.Image = Properties.Resources.PullFetchAll;
                    toolStripButtonPull.ToolTipText = _pullFetchAll.Text;
                    break;

                case Settings.PullAction.Merge:
                    toolStripButtonPull.Image = Properties.Resources.PullMerge;
                    toolStripButtonPull.ToolTipText = _pullMerge.Text;
                    break;

                case Settings.PullAction.Rebase:
                    toolStripButtonPull.Image = Properties.Resources.PullRebase;
                    toolStripButtonPull.ToolTipText = _pullRebase.Text;
                    break;

                default:
                    toolStripButtonPull.Image = Properties.Resources.Icon_4;
                    toolStripButtonPull.ToolTipText = _pullOpenDialog.Text;
                    break;
            }
        }

        private void fetchAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Settings.SetNextPullActionAsDefault)
                Module.LastPullAction = Settings.PullAction.FetchAll;

            RefreshPullIcon();
            bool pullCompelted;

            UICommands.StartPullDialog(this, true, out pullCompelted, true);

            //restore Settings.FormPullAction value
            if (!Settings.SetNextPullActionAsDefault)
                Module.LastPullActionToFormPullAction();

            Settings.SetNextPullActionAsDefault = false;
        }

        private void resetFileToToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();
            int selectedRevsCount = revisions.Count;

            if (selectedRevsCount == 1)
            {
                resetFileToSelectedToolStripMenuItem.Visible = true;
                TranslateItem(resetFileToSelectedToolStripMenuItem.Name, resetFileToSelectedToolStripMenuItem);
                resetFileToSelectedToolStripMenuItem.Text += " (" + revisions[0].Subject.ShortenTo(50) + ")";

                if (revisions[0].HasParent())
                {
                    resetFileToParentToolStripMenuItem.Visible = true;
                    TranslateItem(resetFileToParentToolStripMenuItem.Name, resetFileToParentToolStripMenuItem);
                    GitRevision parentRev = RevisionGrid.GetRevision(revisions[0].ParentGuids[0]);
                    if (parentRev != null)
                    {
                        resetFileToParentToolStripMenuItem.Text += " (" + parentRev.Subject.ShortenTo(50) + ")";
                    }
                }
                else
                {
                    resetFileToParentToolStripMenuItem.Visible = false;
                }
            }
            else
            {
                resetFileToSelectedToolStripMenuItem.Visible = false;
                resetFileToParentToolStripMenuItem.Visible = false;
            }

            if (selectedRevsCount == 2)
            {
                resetFileToFirstToolStripMenuItem.Visible = true;
                TranslateItem(resetFileToFirstToolStripMenuItem.Name, resetFileToFirstToolStripMenuItem);
                resetFileToFirstToolStripMenuItem.Text += " (" + revisions[1].Subject.ShortenTo(50) + ")";

                resetFileToSecondToolStripMenuItem.Visible = true;
                TranslateItem(resetFileToSecondToolStripMenuItem.Name, resetFileToSecondToolStripMenuItem);
                resetFileToSecondToolStripMenuItem.Text += " (" + revisions[0].Subject.ShortenTo(50) + ")";
            }
            else
            {
                resetFileToFirstToolStripMenuItem.Visible = false;
                resetFileToSecondToolStripMenuItem.Visible = false;
            }
        }

        private void ResetSelectedItemsTo(string revision, bool actsAsChild)
        {
            var selectedItems = DiffFiles.SelectedItems;
            IEnumerable<GitItemStatus> itemsToCheckout;
            if (actsAsChild)
            {
                var deletedItems = selectedItems.Where(item => item.IsDeleted);
                Module.RemoveFiles(deletedItems.Select(item => item.Name), false);
                itemsToCheckout = selectedItems.Where(item => !item.IsDeleted);
            }
            else //acts as parent
            {
                //if file is new to the parent, it has to be removed
                var addedItems = selectedItems.Where(item => item.IsNew);
                Module.RemoveFiles(addedItems.Select(item => item.Name), false);
                itemsToCheckout = selectedItems.Where(item => !item.IsNew);
            }

            Module.CheckoutFiles(itemsToCheckout.Select(item => item.Name), revision, false);
        }

        private void resetFileToFirstToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

            if (revisions.Count != 2 || !DiffFiles.SelectedItems.Any())
            {
                return;
            }

            ResetSelectedItemsTo(revisions[1].Guid, false);
        }

        private void resetFileToSecondToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

            if (revisions.Count != 2 || !DiffFiles.SelectedItems.Any())
            {
                return;
            }

            ResetSelectedItemsTo(revisions[0].Guid, true);
        }

        private void resetFileToParentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

            if (revisions.Count != 1 || !DiffFiles.SelectedItems.Any())
            {
                return;
            }

            if (!revisions[0].HasParent())
            {
                throw new ApplicationException("This menu should be disabled for revisions that don't have a parent.");
            }

            ResetSelectedItemsTo(revisions[0].ParentGuids[0], false);
        }

        private void resetFileToSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

            if (revisions.Count != 1 || !DiffFiles.SelectedItems.Any())
            {
                return;
            }

            ResetSelectedItemsTo(revisions[0].Guid, true);
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
                var revision = new GitRevision(Module, e.Data);
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
                CommitData commit = CommitData.GetCommitData(Module, e.Data, ref error);
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

        private void toolStripButtonLevelUp_DropDownOpening(object sender, EventArgs e)
        {
            LoadSubmodulesIntoDropDownMenu();
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

        private CancellationTokenSource _submodulesStatusCTS = new CancellationTokenSource();

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
            public List<SubmoduleInfo> OurSubmodules = new List<SubmoduleInfo>();
            public List<SubmoduleInfo> SuperSubmodules = new List<SubmoduleInfo>();
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

                if (String.IsNullOrEmpty(submoduleName) || supermodule == null)
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
            _submodulesStatusCTS.Cancel();
            _submodulesStatusCTS.Dispose();
            _submodulesStatusCTS = new CancellationTokenSource();

            RemoveSubmoduleButtons();
            toolStripButtonLevelUp.DropDownItems.Add(_loading.Text);

            // Start gathering new submodule information asynchronously.  This makes a significant difference in UI
            // responsiveness if there are numerous submodules (e.g. > 100).
            var cancelToken = _submodulesStatusCTS.Token;
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
                    if (Settings.DashboardShowCurrentBranch && !GitModule.IsBareRepository(path))
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
                        if (Settings.DashboardShowCurrentBranch && !GitModule.IsBareRepository(path))
                            name = name + " " + GetModuleBranch(path);

                        result.TopProject = new SubmoduleInfo { Text = name, Path = supersuperproject.WorkingDir };
                        GetSubmoduleStatusAsync(result.TopProject, cancelToken);
                    }

                    {
                        string name;
                        GitModule parentModule = threadModule.SuperprojectModule;
                        string localpath = "";
                        if (threadModule.SuperprojectModule.WorkingDir != supersuperproject.WorkingDir)
                        {
                            parentModule = supersuperproject;
                            localpath = threadModule.SuperprojectModule.WorkingDir.Substring(supersuperproject.WorkingDir.Length);
                            localpath = PathUtil.GetDirectoryName(localpath.ToPosixPath());
                            name = localpath;
                        }
                        else
                            name = Path.GetFileName(Path.GetDirectoryName(supersuperproject.WorkingDir));
                        string path = threadModule.SuperprojectModule.WorkingDir;
                        if (Settings.DashboardShowCurrentBranch && !GitModule.IsBareRepository(path))
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
                            if (Settings.DashboardShowCurrentBranch && !GitModule.IsBareRepository(path))
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

        private void openWithDifftoolToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            bool artificialRevSelected = false;
            bool enableDiffDropDown = true;
            bool showParentItems = false;

            IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

            if (revisions.Count > 0)
            {
                artificialRevSelected = revisions[0].IsArtificial();

                if (revisions.Count == 2)
                {
                    artificialRevSelected = artificialRevSelected || revisions[revisions.Count - 1].IsArtificial();
                    showParentItems = true;
                }
                else
                    enableDiffDropDown = revisions.Count == 1;
            }

            aBToolStripMenuItem.Enabled = enableDiffDropDown;
            bLocalToolStripMenuItem.Enabled = enableDiffDropDown;
            aLocalToolStripMenuItem.Enabled = enableDiffDropDown;
            parentOfALocalToolStripMenuItem.Enabled = enableDiffDropDown;
            parentOfBLocalToolStripMenuItem.Enabled = enableDiffDropDown;

            parentOfALocalToolStripMenuItem.Visible = showParentItems;
            parentOfBLocalToolStripMenuItem.Visible = showParentItems;

            if (!enableDiffDropDown)
                return;
            //enable *<->Local items only when local file exists
            foreach (var item in DiffFiles.SelectedItems)
            {
                string filePath = FormBrowseUtil.GetFullPathFromGitItemStatus(Module, item);
                if (File.Exists(filePath))
                {
                    bLocalToolStripMenuItem.Enabled = !artificialRevSelected;
                    aLocalToolStripMenuItem.Enabled = !artificialRevSelected;
                    parentOfALocalToolStripMenuItem.Enabled = !artificialRevSelected;
                    parentOfBLocalToolStripMenuItem.Enabled = !artificialRevSelected;
                    return;
                }
            }
        }

        private string GetMonoVersion()
        {
            Type type = Type.GetType("Mono.Runtime");
            if (type != null)
            {
                MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                if (displayName != null)
                    return (string)displayName.Invoke(null, null);
            }
            return null;
        }

        private void reportAnIssueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string issueData = "--- GitExtensions";
            try
            {
                issueData += Settings.ProductVersion;
                issueData += ", Git " + GitCommandHelpers.VersionInUse.Full;
                issueData += ", " + Environment.OSVersion;
                var monoVersion = GetMonoVersion();
                if (monoVersion != null)
                    issueData += ", Mono " + monoVersion;
            }
            catch (Exception) { }

            Process.Start(@"https://github.com/gitextensions/gitextensions/issues/new?body=" + WebUtility.HtmlEncode(issueData));
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var updateForm = new FormUpdates(Module.AppVersion);
            updateForm.SearchForUpdatesAndShow(Owner, true);
        }

        private void toolStripButtonPull_DropDownOpened(object sender, EventArgs e)
        {
            setNextPullActionAsDefaultToolStripMenuItem.Checked = Settings.SetNextPullActionAsDefault;
        }

        private void FormBrowse_Activated(object sender, EventArgs e)
        {
            this.InvokeAsync(OnActivate);
        }

        private void cherryPickSelectedDiffFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DiffText.CherryPickAllChanges();
        }

        private void GitTreeKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                if (GitTree.SelectedNode != null)
                {
                    OnItemActivated();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Adds a tab with console interface to Git over the current working copy. Recreates the terminal on tab activation if user exits the shell.
        /// </summary>
        private void FillTerminalTab()
        {
            if (!EnvUtils.RunningOnWindows() || !Module.EffectiveSettings.Detailed.ShowConEmuTab.ValueOrDefault)
                return; // ConEmu only works on WinNT
            TabPage tabpage;
            string sImageKey = "Resources.IconConsole";
            CommitInfoTabControl.ImageList.Images.Add(sImageKey, Resources.IconConsole);
            CommitInfoTabControl.Controls.Add(tabpage = new TabPage(_consoleTabCaption.Text));
            tabpage.ImageKey = sImageKey; // After adding page

            // Delay-create the terminal window when the tab is first selected
            CommitInfoTabControl.Selecting += (sender, args) =>
            {
                if (args.TabPage != tabpage)
                    return;
                if (terminal == null) // Lazy-create on first opening the tab
                {
                    tabpage.Controls.Clear();
                    tabpage.Controls.Add(
                        terminal = new ConEmuControl()
                        {
                            Dock = DockStyle.Fill,
                            AutoStartInfo = null,
                            IsStatusbarVisible = false
                        }
                    );
                }
                if (terminal.IsConsoleEmulatorOpen) // If user has typed "exit" in there, restart the shell; otherwise just return
                    return;

                // Create the terminal
                var startinfo = new ConEmuStartInfo();
                startinfo.StartupDirectory = Module.WorkingDir;
                startinfo.WhenConsoleProcessExits = WhenConsoleProcessExits.CloseConsoleEmulator;

                // Choose the console: bash from git with fallback to cmd
                string sJustBash = "bash.exe"; // Generic bash, should generally be in the git dir, less configured than the specific git-bash
                string sJustSh = "sh.exe"; // Fallback to SH

                string cmdPath = new[] { sJustBash, sJustSh }.
                    Select(shell =>
                      {
                          string shellPath;
                          if (PathUtil.TryFindShellPath(shell, out shellPath))
                              return shellPath;
                          return null;
                      }).
                      Where(shellPath => shellPath != null).
                      FirstOrDefault();

                if (cmdPath == null)
                {
                    startinfo.ConsoleProcessCommandLine = ConEmuConstants.DefaultConsoleCommandLine;
                }
                else
                {
                    startinfo.ConsoleProcessCommandLine = cmdPath + " --login -i";
                }
                startinfo.ConsoleProcessExtraArgs = " -new_console:P:\"<Solarized Light>\"";

                // Set path to git in this window (actually, effective with CMD only)
                if (!string.IsNullOrEmpty(AppSettings.GitCommandValue))
                {
                    string dirGit = Path.GetDirectoryName(AppSettings.GitCommandValue);
                    if (!string.IsNullOrEmpty(dirGit))
                        startinfo.SetEnv("PATH", dirGit + ";" + "%PATH%");
                }

                terminal.Start(startinfo);
            };
        }

        public void ChangeTerminalActiveFolder(string path)
        {
            if (terminal == null || terminal.RunningSession == null || string.IsNullOrWhiteSpace(path))
                return;

            string posixPath;
            if (PathUtil.TryConvertWindowsPathToPosix(path, out posixPath))
            {
                //Clear terminal line by sending 'backspace' characters
                for (int i = 0; i < 10000; i++)
                {
                    terminal.RunningSession.WriteInputText("\b");
                }
                terminal.RunningSession.WriteInputText(@"cd " + posixPath + Environment.NewLine);
            }
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
                _submodulesStatusCTS.Dispose();
                if (_formBrowseMenus != null)
                    _formBrowseMenus.Dispose();
                if (_filterRevisionsHelper != null)
                    _filterRevisionsHelper.Dispose();
                if (_filterBranchHelper != null)
                    _filterBranchHelper.Dispose();

                if (components != null)
                    components.Dispose();
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
            var formReflog = new FormReflog(this.UICommands);
            formReflog.ShowDialog();
            if (formReflog.ShouldRefresh)
            {
                RefreshRevisions();
            }
        }
    }
}