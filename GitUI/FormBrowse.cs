using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using GitUI.Hotkey;
using GitUI.Plugin;
using GitUI.RepoHosting;
using GitUI.Script;
using GitUI.Statistics;
using GitUIPluginInterfaces;
using ICSharpCode.TextEditor.Util;
#if !__MonoCS__
using Microsoft.WindowsAPICodePack.Taskbar;
#endif
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormBrowse : GitExtensionsForm
    {
        #region Translation

        private readonly TranslationString _stashCount =
            new TranslationString("{0} saved {1}");
        private readonly TranslationString _stashPlural =
            new TranslationString("stashes");
        private readonly TranslationString _stashSingular =
            new TranslationString("stash");

        private readonly TranslationString _warningMiddleOfBisect =
            new TranslationString("Your are in the middle of a bisect");
        private readonly TranslationString _warningMiddleOfRebase =
            new TranslationString("You are in the middle of a rebase");
        private readonly TranslationString _warningMiddleOfPatchApply =
            new TranslationString("You are in the middle of a patch apply");

        private readonly TranslationString _hintUnresolvedMergeConflicts =
            new TranslationString("There are unresolved merge conflicts!");

        private static readonly TranslationString _noBranchTitle =
            new TranslationString("no branch");

        private readonly TranslationString _noSubmodulesPresent =
            new TranslationString("No submodules");

        private readonly TranslationString _saveFileFilterCurrentFormat =
            new TranslationString("Current format");
        private readonly TranslationString _saveFileFilterAllFiles =
            new TranslationString("All files");

        private readonly TranslationString _indexLockDeleted =
            new TranslationString("index.lock deleted.");
        private readonly TranslationString _indexLockNotFound =
            new TranslationString("index.lock not found at:");

        private readonly TranslationString _noReposHostPluginLoaded =
            new TranslationString("No repository host plugin loaded.");
        private readonly TranslationString _noReposHostPluginLoadedCaption =
            new TranslationString("Error");

        private readonly TranslationString _noReposHostFound =
            new TranslationString("Could not find any relevant repository hosts for the currently open repository.");
        private readonly TranslationString _noReposHostFoundCaption =
            new TranslationString("Error");

        private readonly TranslationString _noRevisionFoundError =
            new TranslationString("No revision found.");

        private readonly TranslationString _configureWorkingDirMenu =
            new TranslationString("Configure this menu");

        private readonly TranslationString _UnsupportedMultiselectAction =
            new TranslationString("Operation not supported");

        #endregion

        private string _NoDiffFilesChangesText;

        private readonly SynchronizationContext syncContext;
        private readonly IndexWatcher _indexWatcher = new IndexWatcher();

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
        private ToolStripGitStatus _toolStripGitStatus;
        private FilterRevisionsHelper filterRevisionsHelper;
        private FilterBranchHelper _FilterBranchHelper;

        public FormBrowse(string filter)
        {
            syncContext = SynchronizationContext.Current;

            InitializeComponent();
            filterRevisionsHelper = new FilterRevisionsHelper(toolStripTextBoxFilter, toolStripDropDownButton1, RevisionGrid, toolStripLabel2, this);
            _FilterBranchHelper = new FilterBranchHelper(toolStripBranches, toolStripDropDownButton2, RevisionGrid);
            Translate();
            _NoDiffFilesChangesText = DiffFiles.GetNoFilesText();

#if !__MonoCS__
            if (Settings.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
            {
                TaskbarManager.Instance.ApplicationId = "HenkWesthuis.GitExtensions";
            }
#endif
            if (Settings.ShowGitStatusInBrowseToolbar)
            {
                _toolStripGitStatus = new ToolStripGitStatus
                                 {
                                     ImageTransparentColor = System.Drawing.Color.Magenta
                                 };
                _toolStripGitStatus.Click += StatusClick;
                ToolStrip.Items.Insert(ToolStrip.Items.IndexOf(toolStripButton1), _toolStripGitStatus);
                ToolStrip.Items.Remove(toolStripButton1);
                _toolStripGitStatus.CommitTranslatedString = toolStripButton1.Text;
            }
            RevisionGrid.SelectionChanged += RevisionGridSelectionChanged;
            DiffText.ExtraDiffArgumentsChanged += DiffTextExtraDiffArgumentsChanged;
            filterRevisionsHelper.SetFilter(filter);
            DiffText.SetFileLoader(getNextPatchFile);

            GitTree.ImageList = new ImageList();
            GitTree.ImageList.Images.Add(Properties.Resources.New); //File
            GitTree.ImageList.Images.Add(Properties.Resources.Folder); //Folder
            GitTree.ImageList.Images.Add(Properties.Resources.Submodule); //Submodule

            GitTree.MouseDown += GitTree_MouseDown;
            GitTree.MouseMove += GitTree_MouseMove;

            this.HotkeysEnabled = true;
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            this.toolPanel.SplitterDistance = this.ToolStrip.Height;
            this._dontUpdateOnIndexChange = false;
            Settings.WorkingDirChanged += (a, b, c) => RefreshPullIcon();
            RefreshPullIcon();
        }

        private void ShowDashboard()
        {
            if (_dashboard == null)
            {
                _dashboard = new Dashboard();
                _dashboard.WorkingDirChanged += DashboardWorkingDirChanged;
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
            if (_dashboard != null)
                _dashboard.Visible = false;
        }

        private void DashboardWorkingDirChanged(object sender, EventArgs e)
        {
            WorkingDirChanged(false);
        }

        private void GitTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var item = e.Node.Tag as GitItem;
            if (item == null)
                return;

            if (item.IsBlob)
                FileText.ViewGitItem(item.FileName, item.Guid);
            else
                FileText.ViewText("", "");
        }

        private void BrowseLoad(object sender, EventArgs e)
        {
            RevisionGrid.Load();

            RestorePosition("browse");

            Cursor.Current = Cursors.WaitCursor;
            InternalInitialize(false);
            RevisionGrid.Focus();
            RevisionGrid.ActionOnRepositoryPerformed += ActionOnRepositoryPerformed;
            IndexWatcher.Reset();

            IndexWatcher.Changed += _indexWatcher_Changed;

            Cursor.Current = Cursors.Default;
        }

        void _indexWatcher_Changed(bool indexChanged)
        {
            syncContext.Post(o =>
            {
                RefreshButton.Image = indexChanged && Settings.UseFastChecks && Settings.Module.ValidWorkingDir()
                                          ? GitUI.Properties.Resources.arrow_refresh_dirty
                                          : GitUI.Properties.Resources.arrow_refresh;
            }, this);
        }

        private bool pluginsLoaded;
        private void LoadPluginsInPluginMenu()
        {
            if (!pluginsLoaded)
            {
                foreach (var plugin in LoadedPlugins.Plugins)
                {
                    var item = new ToolStripMenuItem { Text = plugin.Description, Tag = plugin };
                    item.Click += ItemClick;
                    pluginsToolStripMenuItem.DropDownItems.Add(item);
                }
                pluginsLoaded = true;
                UpdatePluginMenu(Settings.Module.ValidWorkingDir());
            }
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

            var eventArgs = new GitUIEventArgs(this, GitUICommands.Instance);

            string workingDirBefore = Settings.WorkingDir;
            bool refresh = plugin.Execute(eventArgs);
            if (workingDirBefore != Settings.WorkingDir)
                WorkingDirChanged(false);
            else if (refresh)
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

        private void ActionOnRepositoryPerformed(object sender, EventArgs e)
        {
            InternalInitialize(false);
        }

        protected void Initialize()
        {
            try
            {
                InternalInitialize(true);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InternalInitialize(bool hard)
        {
            Cursor.Current = Cursors.WaitCursor;
            bool validWorkingDir = Settings.Module.ValidWorkingDir();
            bool hasWorkingDir = !string.IsNullOrEmpty(Settings.WorkingDir);
            branchSelect.Text = validWorkingDir ? Settings.Module.GetSelectedBranch() : "";
            if (hasWorkingDir)
                HideDashboard();
            else
                ShowDashboard();
            CommitInfoTabControl.Visible = validWorkingDir;
            fileExplorerToolStripMenuItem.Enabled = validWorkingDir;
            commandsToolStripMenuItem.Enabled = validWorkingDir;
            manageRemoteRepositoriesToolStripMenuItem1.Enabled = validWorkingDir;
            branchSelect.Enabled = validWorkingDir;
            toolStripButton1.Enabled = validWorkingDir;
            if (_toolStripGitStatus != null)
                _toolStripGitStatus.Enabled = validWorkingDir;
            toolStripButtonPull.Enabled = validWorkingDir;
            toolStripButtonPush.Enabled = validWorkingDir;
            submodulesToolStripMenuItem.Enabled = validWorkingDir;
            UpdatePluginMenu(validWorkingDir);
            gitMaintenanceToolStripMenuItem.Enabled = validWorkingDir;
            editgitignoreToolStripMenuItem1.Enabled = validWorkingDir;
            editgitattributesToolStripMenuItem.Enabled = validWorkingDir;
            editmailmapToolStripMenuItem.Enabled = validWorkingDir;
            toolStripSplitStash.Enabled = validWorkingDir;
            commitcountPerUserToolStripMenuItem.Enabled = validWorkingDir;
            _createPullRequestsToolStripMenuItem.Enabled = validWorkingDir;
            _viewPullRequestsToolStripMenuItem.Enabled = validWorkingDir;
            //Only show "Repository hosts" menu item when there is at least 1 repository host plugin loaded
            _repositoryHostsToolStripMenuItem.Visible = RepoHosts.GitHosters.Count > 0;
            if (RepoHosts.GitHosters.Count == 1)
                _repositoryHostsToolStripMenuItem.Text = RepoHosts.GitHosters[0].Description;
            _FilterBranchHelper.InitToolStripBranchFilter();
            if (hard)
                ShowRevisions();
            RefreshWorkingDirCombo();
            Text = GenerateWindowTitle(Settings.WorkingDir, validWorkingDir, branchSelect.Text);
            DiffText.Font = Settings.DiffFont;
            UpdateJumplist(validWorkingDir);

            CheckForMergeConflicts();
            UpdateStashCount();
            // load custom user menu
            LoadUserMenu();

            Cursor.Current = Cursors.Default;
        }

        private void RefreshWorkingDirCombo()
        {
            if (Settings.RecentReposComboMinWidth > 0)
            {
                _NO_TRANSLATE_Workingdir.AutoSize = false;
                _NO_TRANSLATE_Workingdir.Width = Settings.RecentReposComboMinWidth;
            }
            else
                _NO_TRANSLATE_Workingdir.AutoSize = true;

            Repository r = null;
            if (Repositories.RepositoryHistory.Repositories.Count > 0)
                r = Repositories.RepositoryHistory.Repositories[0];

            List<RecentRepoInfo> mostRecentRepos = new List<RecentRepoInfo>();

            if (r == null || !r.Path.Equals(Settings.WorkingDir, StringComparison.InvariantCultureIgnoreCase))
                Repositories.AddMostRecentRepository(Settings.WorkingDir);

            using (var graphics = CreateGraphics())
            {
                var splitter = new RecentRepoSplitter
                {
                    measureFont = _NO_TRANSLATE_Workingdir.Font,
                    graphics = graphics
                };
                splitter.SplitRecentRepos(Repositories.RepositoryHistory.Repositories, mostRecentRepos, mostRecentRepos);
            }

            RecentRepoInfo ri = mostRecentRepos.Find((e) => e.Repo.Path.Equals(Settings.WorkingDir, StringComparison.InvariantCultureIgnoreCase));

            if (ri == null)
                _NO_TRANSLATE_Workingdir.Text = Settings.WorkingDir;
            else
                _NO_TRANSLATE_Workingdir.Text = ri.Caption;
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
            ScriptRunner.RunScript(((ToolStripButton)sender).Text, null);
            RevisionGrid.RefreshRevisions();
        }

        private void UpdateJumplist(bool validWorkingDir)
        {
#if !__MonoCS__
            if (Settings.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
            {
                //Call this method using reflection.  This is a workaround to *not* reference WPF libraries, becuase of how the WindowsAPICodePack was implimented.
                TaskbarManager.Instance.GetType().InvokeMember("SetApplicationIdForSpecificWindow", System.Reflection.BindingFlags.InvokeMethod, null, TaskbarManager.Instance, new object[] { Handle, "GitExtensions" });

                if (validWorkingDir)
                {
                    string repositoryDescription = GetRepositoryShortName(Settings.WorkingDir);
                    string baseFolder = Path.Combine(Settings.ApplicationDataPath, "Recent");
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
                    File.WriteAllText(path, Settings.WorkingDir);
                    JumpList.AddToRecent(path);
                }

                CreateOrUpdateTaskBarButtons(validWorkingDir);
            }
#endif
        }

        private void CreateOrUpdateTaskBarButtons(bool validRepo)
        {
#if !__MonoCS__
            if (Settings.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
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
                    TaskbarManager.Instance.ThumbnailToolBars.GetType().InvokeMember("AddButtons", System.Reflection.BindingFlags.InvokeMethod, null, TaskbarManager.Instance.ThumbnailToolBars,
                                                                                     new object[] { Handle, buttons });
                }

                _commitButton.Enabled = validRepo;
                _pushButton.Enabled = validRepo;
                _pullButton.Enabled = validRepo;
            }
#endif
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
                int stashCount = Settings.Module.GetStashes().Count;
                toolStripSplitStash.Text = string.Format(_stashCount.Text, stashCount,
                                                         stashCount != 1 ? _stashPlural.Text : _stashSingular.Text);
            }
            else
            {
                toolStripSplitStash.Text = string.Empty;
            }
        }

        private void CheckForMergeConflicts()
        {
            bool validWorkingDir = Settings.Module.ValidWorkingDir();

            if (validWorkingDir && Settings.Module.InTheMiddleOfBisect())
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
                (Settings.Module.InTheMiddleOfRebase() || Settings.Module.InTheMiddleOfPatch()))
            {
                if (_rebase == null)
                {
                    _rebase = new WarningToolStripItem
                                  {
                                      Text = Settings.Module.InTheMiddleOfRebase()
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

            if (validWorkingDir && Settings.Module.InTheMiddleOfConflictedMerge() &&
                !Directory.Exists(Settings.Module.GetGitDirectory() + "rebase-apply\\"))
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
        }

        /// <summary>
        /// Generates main window title according to given repository.
        /// </summary>
        /// <param name="workingDir">Path to repository.</param>
        /// <param name="isWorkingDirValid">If the given path contains valid repository.</param>
        /// <param name="branchName">Current branch name.</param>
        private static string GenerateWindowTitle(string workingDir, bool isWorkingDirValid, string branchName)
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
                var repositoryDescription = File.ReadAllLines(repositoryDescriptionFilePath).FirstOrDefault();
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
            if (Settings.Module.InTheMiddleOfRebase())
                GitUICommands.Instance.StartRebaseDialog(this, null);
            else
                GitUICommands.Instance.StartApplyPatchDialog(this);
            Initialize();
        }


        private void ShowRevisions()
        {
            if (IndexWatcher.IndexChanged)
            {
                RevisionGrid.RefreshRevisions();
                FillFileTree();
                FillDiff();
                FillCommitInfo();
            }
            IndexWatcher.Reset();
        }

        //store strings to not keep references to nodes
        private readonly Stack<string> lastSelectedNodes = new Stack<string>();

        private void FillFileTree()
        {
            if (CommitInfoTabControl.SelectedTab != Tree)
                return;

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
            if (CommitInfoTabControl.SelectedTab != Diff)
                return;

            var revisions = RevisionGrid.GetSelectedRevisions();

            DiffText.SaveCurrentScrollPos();

            DiffFiles.SetNoFilesText(_NoDiffFilesChangesText);
            switch (revisions.Count)
            {
                case 2:
                    bool artificialRevSelected = revisions[0].IsArtificial() || revisions[1].IsArtificial();
                    if (artificialRevSelected)
                    {
                        DiffFiles.SetNoFilesText(_UnsupportedMultiselectAction.Text);
                        DiffFiles.GitItemStatuses = null;
                    }
                    else
                        DiffFiles.GitItemStatuses =
                            Settings.Module.GetDiffFiles(revisions[0].Guid, revisions[1].Guid);
                    break;
                case 0:
                    DiffFiles.GitItemStatuses = null;
                    return;
                default:
                    var revision = revisions[0];

                    if (revision != null &&
                        revision.ParentGuids != null &&
                        revision.ParentGuids.Length > 0)
                    {
                        if (revision.Guid == GitRevision.UncommittedWorkingDirGuid) //working dir changes
                            DiffFiles.GitItemStatuses = Settings.Module.GetAllChangedFiles();
                        else
                            if (revision.Guid == GitRevision.IndexGuid) //index
                                DiffFiles.GitItemStatuses = Settings.Module.GetStagedFiles();
                            else
                                DiffFiles.GitItemStatuses = Settings.Module.GetDiffFiles(revision.Guid, revision.ParentGuids[0]);
                        DiffFiles.Revision = revision;
                    }
                    else
                        DiffFiles.GitItemStatuses = null;
                    break;
            }
        }


        private void FillCommitInfo()
        {
            if (CommitInfoTabControl.SelectedTab != CommitInfo)
                return;

            if (RevisionGrid.GetSelectedRevisions().Count == 0)
                return;

            var revision = RevisionGrid.GetSelectedRevisions()[0];

            if (revision != null)
                RevisionInfo.SetRevision(revision.Guid);
        }

        public void FileHistoryOnClick(object sender, EventArgs e)
        {
            var item = GitTree.SelectedNode.Tag as GitItem;

            if (item == null)
                return;

            if (item.IsBlob || item.IsTree)
            {
                IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

                if (revisions.Count == 0)
                    GitUICommands.Instance.StartFileHistoryDialog(this, item.FileName);
                else
                    GitUICommands.Instance.StartFileHistoryDialog(this, item.FileName, revisions[0], false, false);
            }
        }

        public void FindFileOnClick(object sender, EventArgs e)
        {
            var searchWindow = new SearchWindow<string>(FindFileMatches)
            {
                Owner = this
            };
            searchWindow.ShowDialog(this);
            string selectedItem = searchWindow.SelectedItem;
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
            var candidates = Settings.Module.GetFullTree(RevisionGrid.GetSelectedRevisions()[0].TreeGuid);

            string nameAsLower = name.ToLower();

            return candidates.Where(fileName => fileName.ToLower().Contains(nameAsLower)).ToList();
        }

        public void OpenWithOnClick(object sender, EventArgs e)
        {
            var gitItem = GitTree.SelectedNode.Tag as GitItem;
            if (gitItem == null || !gitItem.IsBlob)
                return;

            var fileName = gitItem.FileName;
            if (fileName.Contains("\\") && fileName.LastIndexOf("\\") < fileName.Length)
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
            if (fileName.Contains("/") && fileName.LastIndexOf("/") < fileName.Length)
                fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);

            fileName = (Path.GetTempPath() + fileName).Replace(Settings.PathSeparatorWrong, Settings.PathSeparator);
            Settings.Module.SaveBlobAs(fileName, gitItem.Guid);
            OpenWith.OpenAs(fileName);
        }

        private void FileTreeContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var gitItem = (GitTree.SelectedNode != null) ? GitTree.SelectedNode.Tag as GitItem : null;
            var enableItems = gitItem != null && gitItem.IsBlob;

            saveAsToolStripMenuItem.Enabled = enableItems;
            openFileToolStripMenuItem.Enabled = enableItems;
            openFileWithToolStripMenuItem.Enabled = enableItems;
            openWithToolStripMenuItem.Enabled = enableItems;
            copyFilenameToClipboardToolStripMenuItem.Enabled = enableItems;
            fileHistoryToolStripMenuItem.Enabled = enableItems;
            editCheckedOutFileToolStripMenuItem.Enabled = enableItems;
        }

        public void OpenOnClick(object sender, EventArgs e)
        {
            try
            {
                var gitItem = GitTree.SelectedNode.Tag as GitItem;
                if (gitItem == null || !gitItem.IsBlob)
                    return;

                var fileName = gitItem.FileName;
                if (fileName.Contains("\\") && fileName.LastIndexOf("\\") < fileName.Length)
                    fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                if (fileName.Contains("/") && fileName.LastIndexOf("/") < fileName.Length)
                    fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);

                fileName = (Path.GetTempPath() + fileName).Replace(Settings.PathSeparatorWrong, Settings.PathSeparator);

                Settings.Module.SaveBlobAs(fileName, (gitItem).Guid);

                Process.Start(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        protected void LoadInTree(List<IGitItem> items, TreeNodeCollection node)
        {
            items.Sort(new GitFileTreeComparer());

            foreach (var item in items)
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

        private void RevisionGridSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (RevisionGrid.GetSelectedRevisions().Count > 0 &&
                    (RevisionGrid.GetSelectedRevisions()[0].Guid == GitRevision.UncommittedWorkingDirGuid ||
                     RevisionGrid.GetSelectedRevisions()[0].Guid == GitRevision.IndexGuid))
                {
                    if (CommitInfoTabControl.TabPages.Contains(CommitInfo))
                        CommitInfoTabControl.TabPages.Remove(CommitInfo);
                    if (CommitInfoTabControl.TabPages.Contains(Tree))
                        CommitInfoTabControl.TabPages.Remove(Tree);
                }
                else
                {
                    if (!CommitInfoTabControl.TabPages.Contains(CommitInfo))
                        CommitInfoTabControl.TabPages.Insert(0, CommitInfo);
                    if (!CommitInfoTabControl.TabPages.Contains(Tree))
                        CommitInfoTabControl.TabPages.Insert(1, Tree);
                }

                FillFileTree();
                FillDiff();
                FillCommitInfo();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void OpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (new Open().ShowDialog(this) == DialogResult.OK)
            {
                WorkingDirChanged(false);
            }
        }

        private void CheckoutToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCheckoutRevisionDialog(this))
                Initialize();
        }

        private void GitTreeDoubleClick(object sender, EventArgs e)
        {
            if (GitTree.SelectedNode == null || !(GitTree.SelectedNode.Tag is IGitItem))
                return;

            var item = GitTree.SelectedNode.Tag as GitItem;
            if (item == null)
                return;

            if (!item.IsBlob)
                return;

            if (GitUICommands.Instance.StartFileHistoryDialog(this, item.FileName, null))
                Initialize();
        }

        private void ViewDiffToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCompareRevisionsDialog(this))
                Initialize();
        }

        private void CloneToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCloneDialog(this))
                Initialize();
        }

        private void CommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCommitDialog(this))
                Initialize();
        }

        private void InitNewRepositoryToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartInitializeDialog(this))
                Initialize();
        }

        private void PushToolStripMenuItemClick(object sender, EventArgs e)
        {
            bool bSilent = (ModifierKeys & Keys.Shift) != 0;
            if (GitUICommands.Instance.StartPushDialog(this, bSilent))
                Initialize();
        }

        private void PullToolStripMenuItemClick(object sender, EventArgs e)
        {
            bool bSilent;
            if (sender == toolStripButtonPull || sender == pullToolStripMenuItem)
            {
                if (Settings.LastPullAction == Settings.PullAction.None)
                {
                    bSilent = (ModifierKeys & Keys.Shift) != 0;
                }
                else
                {
                    bSilent = true;
                    Settings.PullMerge = Settings.LastPullAction;
                }
            }
            else
            {
                bSilent = sender != pullToolStripMenuItem1;
                RefreshPullIcon();
            }

            if (GitUICommands.Instance.StartPullDialog(this, bSilent))
                Initialize();

        }

        private void RefreshToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_dashboard != null)
            {
                _dashboard.Refresh();
            }

            if (_dashboard == null || !_dashboard.Visible)
            {
                RevisionGrid.ForceRefreshRevisions();
                InternalInitialize(false);
                IndexWatcher.Reset();
            }
        }

        private void AboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog(this);
        }

        private void PatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartViewPatchDialog(this))
                Initialize();
        }

        private void ApplyPatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartApplyPatchDialog(this))
                Initialize();
        }

        private void GitBashToolStripMenuItemClick1(object sender, EventArgs e)
        {
            Settings.Module.RunBash();
        }

        private void GitGuiToolStripMenuItemClick(object sender, EventArgs e)
        {
            Settings.Module.RunGui();
        }

        private void FormatPatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartFormatPatchDialog(this))
                Initialize();
        }

        private void GitcommandLogToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitLogForm.ShowOrActivate(this);
        }

        private void CheckoutBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCheckoutBranchDialog(this))
                Initialize();
        }

        private void StashToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartStashDialog(this))
                Initialize();
        }

        private void RunMergetoolToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartResolveConflictsDialog(this))
                Initialize();
        }

        private void WarningClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartResolveConflictsDialog(this))
                Initialize();
        }

        private void WorkingdirClick(object sender, EventArgs e)
        {
            OpenToolStripMenuItemClick(sender, e);
        }

        private void CurrentBranchClick(object sender, EventArgs e)
        {
            CheckoutBranchToolStripMenuItemClick(sender, e);
        }

        private void DeleteBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartDeleteBranchDialog(this, null))
                Initialize();
        }

        private void CherryPickToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCherryPickDialog(this))
                Initialize();
        }

        private void MergeBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartMergeBranchDialog(this, null))
                Initialize();
        }

        private void ToolStripButton1Click(object sender, EventArgs e)
        {
            CommitToolStripMenuItemClick(sender, e);
        }

        private void SettingsClick(object sender, EventArgs e)
        {
            SettingsToolStripMenuItem2Click(sender, e);
        }

        private void TagToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCreateTagDialog(this))
                Initialize();
        }

        private void RefreshButtonClick(object sender, EventArgs e)
        {
            RefreshToolStripMenuItemClick(sender, e);
        }

        private void CommitcountPerUserToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormCommitCount().ShowDialog(this);
        }

        private void KGitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Settings.Module.RunGitK();
        }

        private void DonateToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormDonate().ShowDialog(this);
        }

        private void DeleteTagToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartDeleteTagDialog(this))
                Initialize();
        }

        private void FormBrowseFormClosing(object sender, FormClosingEventArgs e)
        {
            SaveUserMenuPosition();
            SavePosition("browse");
        }

        private void SaveUserMenuPosition()
        {
            GitCommands.Settings.UserMenuLocationX = UserMenuToolStrip.Location.X;
            GitCommands.Settings.UserMenuLocationY = UserMenuToolStrip.Location.Y;
        }

        private void EditGitignoreToolStripMenuItem1Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartEditGitIgnoreDialog(this))
                Initialize();
        }

        private void SettingsToolStripMenuItem2Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartSettingsDialog(this);

            Translate();
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            Initialize();
            RevisionGrid.ReloadHotkeys();
            RevisionGrid.ReloadTranslation();
        }

        private void ArchiveToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartArchiveDialog(this))
                Initialize();
        }

        private void EditMailMapToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartMailMapDialog(this))
                Initialize();
        }

        private void CompressGitDatabaseToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormProcess("gc").ShowDialog(this);
        }

        private void VerifyGitDatabaseToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartVerifyDatabaseDialog(this))
                Initialize();
        }

        private void ManageRemoteRepositoriesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartRemotesDialog(this))
                Initialize();
        }

        private void RebaseToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartRebaseDialog(this, null))
                Initialize();
        }

        private void StartAuthenticationAgentToolStripMenuItemClick(object sender, EventArgs e)
        {
            Settings.Module.StartExternalCommand(Settings.Pageant, "");
        }

        private void GenerateOrImportKeyToolStripMenuItemClick(object sender, EventArgs e)
        {
            Settings.Module.StartExternalCommand(Settings.Puttygen, "");
        }

        private void TabControl1SelectedIndexChanged(object sender, EventArgs e)
        {
            FillFileTree();
            FillDiff();
            FillCommitInfo();
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

            GitItemStatus selectedItem = DiffFiles.SelectedItem;

            DiffText.ViewPatch(RevisionGrid, selectedItem, String.Empty);
        }


        private void ChangelogToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormChangeLog().ShowDialog(this);
        }

        private void DiffFilesDoubleClick(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null)
                return;

            if (GitUICommands.Instance.StartFileHistoryDialog(this, (DiffFiles.SelectedItem).Name))
                Initialize();
        }

        private void ToolStripButtonPushClick(object sender, EventArgs e)
        {
            PushToolStripMenuItemClick(sender, e);
        }

        private void ManageSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartSubmodulesDialog(this))
                Initialize();
        }

        private void UpdateAllSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartUpdateSubmodulesDialog(this))
                Initialize();
        }

        private void SynchronizeAllSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartSyncSubmodulesDialog(this))
                Initialize();
        }

        private void ToolStripSplitStashButtonClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartStashDialog(this))
                Initialize();
        }

        private void StashChangesToolStripMenuItemClick(object sender, EventArgs e)
        {
            var arguments = GitCommandHelpers.StashSaveCmd(Settings.IncludeUntrackedFilesInManualStash);
            new FormProcess(arguments).ShowDialog(this);
            Initialize();
        }

        private void StashPopToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormProcess("stash pop").ShowDialog(this);
            Initialize();
        }

        private void ViewStashToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartStashDialog(this))
                Initialize();
        }

        private void OpenSubmoduleToolStripMenuItemDropDownOpening(object sender, EventArgs e)
        {
            LoadSubmodulesIntoDropDownMenu();
        }

        private void LoadSubmodulesIntoDropDownMenu()
        {
            Cursor.Current = Cursors.WaitCursor;

            RemoveSubmoduleButtons();

            foreach (var submodule in Settings.Module.GetSubmodulesNames().OrderBy(submoduleName => submoduleName))
            {
                var submenu = new ToolStripMenuItem(submodule);
                submenu.Click += SubmoduleToolStripButtonClick;
                submenu.Width = 200;
                openSubmoduleToolStripMenuItem.DropDownItems.Add(submenu);
            }

            if (openSubmoduleToolStripMenuItem.DropDownItems.Count == 0)
                openSubmoduleToolStripMenuItem.DropDownItems.Add(_noSubmodulesPresent.Text);
            Cursor.Current = Cursors.Default;
        }

        private void RemoveSubmoduleButtons()
        {
            foreach (var item in openSubmoduleToolStripMenuItem.DropDownItems)
            {
                var toolStripButton = item as ToolStripMenuItem;
                if (toolStripButton != null)
                    toolStripButton.Click -= SubmoduleToolStripButtonClick;
            }
            openSubmoduleToolStripMenuItem.DropDownItems.Clear();
        }

        private void SubmoduleToolStripButtonClick(object sender, EventArgs e)
        {
            var button = sender as ToolStripMenuItem;

            if (button == null)
                return;

            Settings.WorkingDir += Settings.Module.GetSubmoduleLocalPath(button.Text);

            if (Settings.Module.ValidWorkingDir())
                Repositories.AddMostRecentRepository(Settings.WorkingDir);

            InternalInitialize(true);
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

        private void HistoryItemMenuClick(object sender, EventArgs e)
        {
            var button = sender as ToolStripMenuItem;

            if (button == null)
                return;

            Settings.WorkingDir = button.Text;
            InternalInitialize(true);
        }

        private void SettingsToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartPluginSettingsDialog(this);
        }

        private void CloseToolStripMenuItemClick(object sender, EventArgs e)
        {
            Settings.WorkingDir = "";
            WorkingDirChanged(false);
        }

        public override void CancelButtonClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.WorkingDir))
            {
                Close();
                return;
            }
            Settings.WorkingDir = "";
            WorkingDirChanged(false);
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
                Process.Start(Settings.GetInstallDir() + "\\GitExtensionsUserManual.pdf");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void DiffTextExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ShowSelectedFileDiff();
        }

        private void CleanupToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormCleanupRepository().ShowDialog(this);
        }

        private void openWithDifftoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null)
                return;

            var selectedItem = (DiffFiles.SelectedItem).Name;
            GitUIExtensions.DiffWithRevisionKind diffKind;

            if (sender == diffBaseLocalToolStripMenuItem)
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffBaseLocal;
            else if (sender == difftoolRemoteLocalToolStripMenuItem)
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffRemoteLocal;
            else
                diffKind = GitUIExtensions.DiffWithRevisionKind.DiffAsSelected;

            RevisionGrid.OpenWithDifftool(selectedItem, diffKind);
        }

        private void AddWorkingdirDropDownItem(Repository repo, string caption)
        {
            ToolStripMenuItem toolStripItem = new ToolStripMenuItem(caption);
            _NO_TRANSLATE_Workingdir.DropDownItems.Add(toolStripItem);

            toolStripItem.Click += (hs, he) => SetWorkingDir(repo.Path);

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
                    measureFont = _NO_TRANSLATE_Workingdir.Font,
                    graphics = graphics
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
            ToolStripMenuItem toolStripItem = new ToolStripMenuItem(_configureWorkingDirMenu.Text);
            _NO_TRANSLATE_Workingdir.DropDownItems.Add(toolStripItem);

            toolStripItem.Click += (hs, he) =>
            {
                new FormRecentReposSettings().ShowDialog(this);
                RefreshWorkingDirCombo();
            };
        }

        private void SetWorkingDir(string path)
        {
            Settings.WorkingDir = path;
            Repositories.AddMostRecentRepository(Settings.WorkingDir);
            InternalInitialize(true);
        }

        private void TranslateToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormTranslate().ShowDialog(this);
        }

        private void FileExplorerToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Settings.WorkingDir);
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

            var fullName = Settings.WorkingDir + item.FileName;
            var fileDialog =
                new SaveFileDialog
                    {
                        InitialDirectory = Path.GetDirectoryName(fullName),
                        FileName = Path.GetFileName(fullName),
                        DefaultExt = GitCommandHelpers.GetFileExtension(fullName),
                        AddExtension = true
                    };
            fileDialog.Filter =
                _saveFileFilterCurrentFormat.Text + " (*." +
                GitCommandHelpers.GetFileExtension(fileDialog.FileName) + ")|*." +
                GitCommandHelpers.GetFileExtension(fileDialog.FileName) +
                "|" + _saveFileFilterAllFiles.Text + " (*.*)|*.*";

            if (fileDialog.ShowDialog(this) == DialogResult.OK)
            {
                Settings.Module.SaveBlobAs(fileDialog.FileName, item.Guid);
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

        private void BranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCreateBranchDialog(this))
                Initialize();
        }

        private void RevisionGridDoubleClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartCompareRevisionsDialog(this);
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
            if (GitUICommands.Instance.StartEditGitAttributesDialog(this))
                Initialize();
        }

        private void copyFilenameToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gitItem = GitTree.SelectedNode.Tag as GitItem;
            if (gitItem == null)
                return;

            var fileName = Settings.WorkingDir + (gitItem).FileName;
            Clipboard.SetText(fileName.Replace('/', '\\'));
        }

        private void copyFilenameToClipboardToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItems.Count == 0)
                return;

            var fileNames = new StringBuilder();
            foreach (var item in DiffFiles.SelectedItems)
            {
                //Only use appendline when multiple items are selected.
                //This to make it easier to use the text from clipboard when 1 file is selected.
                if (fileNames.Length > 0)
                    fileNames.AppendLine();

                fileNames.Append((Settings.WorkingDir + item.Name).Replace(Settings.PathSeparatorWrong, Settings.PathSeparator));
            }
            Clipboard.SetText(fileNames.ToString());
        }

        private void deleteIndexlockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = Path.Combine(Settings.Module.WorkingDirGitDir(), "index.lock");

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

            var fullName = Settings.WorkingDir + item.Name;
            var fileDialog =
                new SaveFileDialog
                {
                    InitialDirectory = Path.GetDirectoryName(fullName),
                    FileName = Path.GetFileName(fullName),
                    DefaultExt = GitCommandHelpers.GetFileExtension(fullName),
                    AddExtension = true
                };
            fileDialog.Filter =
                _saveFileFilterCurrentFormat.Text + " (*." +
                fileDialog.DefaultExt + ")|*." +
                fileDialog.DefaultExt +
                "|" + _saveFileFilterAllFiles.Text + " (*.*)|*.*";

            if (fileDialog.ShowDialog(this) == DialogResult.OK)
            {
                Settings.Module.SaveBlobAs(fileDialog.FileName, string.Format("{0}:\"{1}\"", revisions[0].Guid, item.Name));
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

            var fileName = Settings.WorkingDir + (gitItem).FileName;
            OpenWith.OpenAs(fileName.Replace(Settings.PathSeparatorWrong, Settings.PathSeparator));
        }

        private void pluginsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            LoadPluginsInPluginMenu();
        }

        private void BisectClick(object sender, EventArgs e)
        {
            new FormBisect(RevisionGrid).ShowDialog(this);
            Initialize();
        }

        private void fileHistoryDiffToolstripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedItem;

            if (item.IsTracked)
            {
                IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

                if (revisions.Count == 0)
                    GitUICommands.Instance.StartFileHistoryDialog(this, item.Name);
                else
                    GitUICommands.Instance.StartFileHistoryDialog(this, item.Name, revisions[0], false);
            }
        }

        private void CurrentBranchDropDownOpening(object sender, EventArgs e)
        {
            branchSelect.DropDownItems.Clear();
            foreach (var branch in Settings.Module.GetHeads(false))
            {
                var toolStripItem = branchSelect.DropDownItems.Add(branch.Name);
                toolStripItem.Click += BranchSelectToolStripItem_Click;
            }
        }

        void BranchSelectToolStripItem_Click(object sender, EventArgs e)
        {
            bool needRefresh;
            bool force;
            if (!GitUICommands.Instance.CheckForDirtyDir(this, out needRefresh, out force))
            {
                var toolStripItem = (ToolStripItem)sender;
                string args = force ? "-f": null;

                var command = "checkout".Join(" ", args).Join(" ", string.Format("\"{0}\"", toolStripItem.Text));
                var form = new FormProcess(command);
                form.ShowDialog(this);
                needRefresh = true;
            }

            if (needRefresh)
                Initialize();
        }

        private void _forkCloneMenuItem_Click(object sender, EventArgs e)
        {
            if (RepoHosts.GitHosters.Count > 0)
            {
                GitUICommands.Instance.StartCloneForkFromHoster(this, RepoHosts.GitHosters[0]); //FIXME: Works until we have > 1 repo hoster
                Initialize();
            }
            else
            {
                MessageBox.Show(this, _noReposHostPluginLoaded.Text, _noReposHostPluginLoadedCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void _viewPullRequestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var repoHost = RepoHosts.TryGetGitHosterForCurrentWorkingDir();
            if (repoHost == null)
            {
                MessageBox.Show(this, _noReposHostFound.Text, _noReposHostFoundCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            GitUICommands.Instance.StartPullRequestsDialog(this, repoHost);
            Initialize();
        }

        private void _createPullRequestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var repoHost = RepoHosts.TryGetGitHosterForCurrentWorkingDir();
            if (repoHost == null)
            {
                MessageBox.Show(this, _noReposHostFound.Text, _noReposHostFoundCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            GitUICommands.Instance.StartCreatePullRequest(this, repoHost);
            Initialize();
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
            SelectCurrentRevision,
            CheckoutBranch,
            QuickFetch,
            QuickPull,
            QuickPush,
            RotateApplicationIcon,
        }

        private void AddNotes()
        {
            Settings.Module.EditNotes(RevisionGrid.GetSelectedRevisions().Count > 0 ? RevisionGrid.GetSelectedRevisions()[0].Guid : string.Empty);
            FillCommitInfo();
        }

        private void FindFileInSelectedCommit()
        {
            CommitInfoTabControl.SelectedTab = Tree;
            EnabledSplitViewLayout(true);
            GitTree.Focus();
            FindFileOnClick(null, null);
        }

        private void QuickFetch()
        {
            new FormProcess(Settings.Module.FetchCmd(string.Empty, string.Empty, string.Empty)).ShowDialog(this);
            Initialize();
        }


        protected override bool ExecuteCommand(int cmd)
        {
            switch ((Commands)cmd)
            {
                case Commands.GitBash: Settings.Module.RunBash(); break;
                case Commands.GitGui: Settings.Module.RunGui(); break;
                case Commands.GitGitK: Settings.Module.RunGitK(); break;
                case Commands.FocusRevisionGrid: RevisionGrid.Focus(); break;
                case Commands.FocusCommitInfo: CommitInfoTabControl.SelectedTab = CommitInfo; break;
                case Commands.FocusFileTree: CommitInfoTabControl.SelectedTab = Tree; GitTree.Focus(); break;
                case Commands.FocusDiff: CommitInfoTabControl.SelectedTab = Diff; DiffFiles.Focus(); break;
                case Commands.Commit: CommitToolStripMenuItemClick(null, null); break;
                case Commands.AddNotes: AddNotes(); break;
                case Commands.FindFileInSelectedCommit: FindFileInSelectedCommit(); break;
                case Commands.SelectCurrentRevision: RevisionGrid.SetSelectedRevision(new GitRevision(RevisionGrid.CurrentCheckout)); break;
                case Commands.CheckoutBranch: CheckoutBranchToolStripMenuItemClick(null, null); break;
                case Commands.QuickFetch: QuickFetch(); break;
                case Commands.QuickPull:
                    if (GitUICommands.Instance.StartPullDialog(this, true))
                        Initialize();
                    break;
                case Commands.QuickPush:
                    if (GitUICommands.Instance.StartPushDialog(this, true))
                        Initialize();
                    break;
                case Commands.RotateApplicationIcon: RotateApplicationIcon(); break;
                default: ExecuteScriptCommand(cmd, Keys.None); break;
            }

            return true;
        }

        #endregion

        private void goToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormGoToCommit formGoToCommit = new FormGoToCommit();
            if (formGoToCommit.ShowDialog(this) == DialogResult.OK)
            {
                string revisionGuid = formGoToCommit.GetRevision();
                if (!string.IsNullOrEmpty(revisionGuid))
                {
                    RevisionGrid.SetSelectedRevision(new GitRevision(revisionGuid));
                }
                else
                {
                    MessageBox.Show(this, _noRevisionFoundError.Text);
                }
            }
        }

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

            var fileName = Settings.WorkingDir + (gitItem).FileName;
            new FormEditor(fileName).ShowDialog(this);
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
                        string fileName = GitCommands.Settings.WorkingDir + item.FileName;

                        fileList.Add(fileName.Replace('/', '\\'));
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

            idx = getNextIdx(idx, DiffFiles.GitItemStatuses.Count - 1, searchBackward);
            _dontUpdateOnIndexChange = true;
            DiffFiles.SelectedIndex = idx;
            _dontUpdateOnIndexChange = false;
            return new Tuple<int, string>(idx, DiffText.GetSelectedPatch(RevisionGrid, DiffFiles.SelectedItem));
        }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItems.Count == 0)
                return;

            foreach (var item in DiffFiles.SelectedItems)
            {
                var fileNames = new StringBuilder();
                fileNames.Append((Settings.WorkingDir + item.Name).Replace(Settings.PathSeparatorWrong, Settings.PathSeparator));

                string filePath = fileNames.ToString();
                if (File.Exists(filePath))
                {
                    Process.Start("explorer.exe", "/select, " + filePath);
                }
            }
        }

        private void DiffContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool artificialRevSelected;

            IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

            if (revisions.Count == 0)
                artificialRevSelected = false;
            else 
                artificialRevSelected = revisions[0].IsArtificial();
            if (revisions.Count > 1)
                artificialRevSelected = artificialRevSelected || revisions[revisions.Count - 1].IsArtificial();

            foreach (var item in DiffFiles.SelectedItems)
            {
                var fileNames = new StringBuilder();
                fileNames.Append((Settings.WorkingDir + item.Name).Replace(Settings.PathSeparatorWrong, Settings.PathSeparator));

                if (File.Exists(fileNames.ToString()))
                {
                    openContainingFolderToolStripMenuItem.Enabled = true;
                    diffBaseLocalToolStripMenuItem.Enabled = !artificialRevSelected;
                    difftoolRemoteLocalToolStripMenuItem.Enabled = !artificialRevSelected;
                    return;
                }
            }

            openContainingFolderToolStripMenuItem.Enabled = false;
            diffBaseLocalToolStripMenuItem.Enabled = false;
            difftoolRemoteLocalToolStripMenuItem.Enabled = false;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (_dashboard != null)
                _dashboard.SaveSplitterPositions();
        }

        private void CloneSvnToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartSvnCloneDialog(this))
                Initialize();
        }

        private void SvnRebaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartSvnRebaseDialog(this))
                Initialize();
        }

        private void SvnDcommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartSvnDcommitDialog(this))
                Initialize();
        }

        private void SvnFetchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartSvnFetchDialog(this))
                Initialize();
        }

        private void expandAllStripMenuItem_Click(object sender, EventArgs e)
        {
			GitTree.ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
			GitTree.CollapseAll();
        }

        private void WorkingDirChanged(bool internalInitialize)
        {
            IndexWatcher.Clear();
            RevisionGrid.ForceRefreshRevisions();
            InternalInitialize(internalInitialize);            
            IndexWatcher.Reset();
        }

        private void DiffFiles_DataSourceChanged(object sender, EventArgs e)
        {
            if (DiffFiles.GitItemStatuses == null || DiffFiles.GitItemStatuses.Count == 0)
                DiffText.ViewPatch(String.Empty);
        }

        private void blameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedItem;

            if (item.IsTracked)
            {
                IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

                if (revisions.Count == 0)
                    GitUICommands.Instance.StartFileHistoryDialog(this, item.Name, null, false, true);
                else
                    GitUICommands.Instance.StartFileHistoryDialog(this, item.Name, revisions[0], true, true);
            }

        }

        private void blameToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var item = GitTree.SelectedNode.Tag as GitItem;

            if (item == null)
                return;

            if (item.IsBlob || item.IsTree)
            {
                IList<GitRevision> revisions = RevisionGrid.GetSelectedRevisions();

                if (revisions.Count == 0)
                    GitUICommands.Instance.StartFileHistoryDialog(this, item.FileName, null, false, true);
                else
                    GitUICommands.Instance.StartFileHistoryDialog(this, item.FileName, revisions[0], true, true);
            }
        }

        public override void AddTranslationItems(Translation translation)
        {
            base.AddTranslationItems(translation);
            TranslationUtl.AddTranslationItemsFromFields(Name, filterRevisionsHelper, translation);
            TranslationUtl.AddTranslationItemsFromFields(Name, _FilterBranchHelper, translation);
        }

        public override void TranslateItems(Translation translation)
        {
            base.TranslateItems(translation);
            TranslationUtl.TranslateItemsFromFields(Name, filterRevisionsHelper, translation);
            TranslationUtl.TranslateItemsFromFields(Name, _FilterBranchHelper, translation);
        }


        private IList<GitItemStatus> FindDiffFilesMatches(string name)
        {
            var candidates = DiffFiles.GitItemStatuses;

            string nameAsLower = name.ToLower();

            return candidates.Where(item => 
                {
                    return item.Name != null && item.Name.ToLower().Contains(nameAsLower)
                        || item.OldName != null && item.OldName.ToLower().Contains(nameAsLower);
                }
                ).ToList();
        }



        private void findInDiffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var searchWindow = new SearchWindow<GitItemStatus>(FindDiffFilesMatches)
            {
                Owner = this
            };
            searchWindow.ShowDialog(this);
            GitItemStatus selectedItem = searchWindow.SelectedItem;
            if (selectedItem != null)
            {
                DiffFiles.SelectedItem = selectedItem;
            }

        }

        private void dontSetAsDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.DonSetAsLastPullAction = !dontSetAsDefaultToolStripMenuItem.Checked;
            dontSetAsDefaultToolStripMenuItem.Checked = Settings.DonSetAsLastPullAction;

        }

        private void mergeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.LastPullAction = Settings.PullAction.Merge;
            PullToolStripMenuItemClick(sender, e);
        }

        private void rebaseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Settings.LastPullAction = Settings.PullAction.Rebase;
            PullToolStripMenuItemClick(sender, e);
        }

        private void fetchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.LastPullAction = Settings.PullAction.Fetch;
            PullToolStripMenuItemClick(sender, e);
        }

        private void pullToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (!Settings.DonSetAsLastPullAction)
                Settings.LastPullAction = Settings.PullAction.None;
            PullToolStripMenuItemClick(sender, e);
        }

        private void RefreshPullIcon()
        {
            switch (Settings.LastPullAction)
            {
                case Settings.PullAction.Fetch:
                    toolStripButtonPull.Image = Properties.Resources.PullFetch;
                    toolStripButtonPull.ToolTipText = "Pull - fetch";
                    break;

                case Settings.PullAction.Merge:
                    toolStripButtonPull.Image = Properties.Resources.PullMerge;
                    toolStripButtonPull.ToolTipText = "Pull - merge";
                    break;

                case Settings.PullAction.Rebase:
                    toolStripButtonPull.Image = Properties.Resources.PullRebase;
                    toolStripButtonPull.ToolTipText = "Pull - rebase";
                    break;
                
                default:
                    toolStripButtonPull.Image = Properties.Resources._4;
                    toolStripButtonPull.ToolTipText = "Open pull dialog";
                    break;
            }
        }

    }

}
