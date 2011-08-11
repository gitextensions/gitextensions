﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using GitUI.Plugin;
using GitUI.Statistics;
using GitUIPluginInterfaces;
using ICSharpCode.TextEditor.Util;
using GitUI.RepoHosting;
using System.Threading;
using GitUI.Hotkey;
using System.Drawing;
using System.Collections.Specialized;
using Microsoft.WindowsAPICodePack.Taskbar;
using GitUI.Script;

namespace GitUI
{
    public partial class FormBrowse : GitExtensionsForm
    {
        private readonly SynchronizationContext syncContext;
        private readonly IndexWatcher _indexWatcher = new IndexWatcher();

        private Dashboard _dashboard;
        private ToolStripItem _rebase;
        private ToolStripItem _bisect;
        private ToolStripItem _warning;

        private ThumbnailToolBarButton _commitButton;
        private ThumbnailToolBarButton _pushButton;
        private ThumbnailToolBarButton _pullButton;
        private bool _toolbarButtonsCreated;
        private bool _dontUpdateOnIndexChange;

        public FormBrowse(string filter)
        {
            syncContext = SynchronizationContext.Current;

            InitializeComponent();
            Translate();

            if (Settings.ShowGitStatusInBrowseToolbar)
            {
                var status = new ToolStripGitStatus
                                 {
                                     ImageTransparentColor = System.Drawing.Color.Magenta
                                 };
                status.Click += StatusClick;
                ToolStrip.Items.Insert(1, status);
            }

            RevisionGrid.SelectionChanged += RevisionGridSelectionChanged;
            DiffText.ExtraDiffArgumentsChanged += DiffTextExtraDiffArgumentsChanged;
            SetFilter(filter);
            DiffText.SetFileLoader(getNextPatchFile);

            GitTree.ImageList = new ImageList();
            GitTree.ImageList.Images.Add(Properties.Resources._21); //File
            GitTree.ImageList.Images.Add(Properties.Resources._40); //Folder
            GitTree.ImageList.Images.Add(Properties.Resources._39); //Submodule

            GitTree.MouseDown += new MouseEventHandler(GitTree_MouseDown);
            GitTree.MouseMove += new MouseEventHandler(GitTree_MouseMove);

            this.HotkeysEnabled = true;
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            this.toolPanel.SplitterDistance = this.ToolStrip.Height;
            this._dontUpdateOnIndexChange = false;
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
            IndexWatcher.Clear();
            RevisionGrid.ForceRefreshRevisions();
            InternalInitialize(false);
            IndexWatcher.Reset();
        }

        private void TreeView1AfterSelect(object sender, TreeViewEventArgs e)
        {
            var item = e.Node.Tag as GitItem;
            if (item == null)
                return;

            if (item.ItemType == "blob")
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
                if (indexChanged && Settings.UseFastChecks && Settings.ValidWorkingDir())
                    this.RefreshButton.Image = GitUI.Properties.Resources.arrow_refresh_dirty;
                else
                    this.RefreshButton.Image = GitUI.Properties.Resources.arrow_refresh;
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
            }
        }

        /// <summary>
        ///   Execute plugin
        /// </summary>
        private static void ItemClick(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem == null)
                return;

            var plugin = menuItem.Tag as IGitPlugin;
            if (plugin == null)
                return;

            var eventArgs = new GitUIEventArgs(GitUICommands.Instance);
            plugin.Execute(eventArgs);
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
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InternalInitialize(bool hard)
        {
            Cursor.Current = Cursors.WaitCursor;
            bool validWorkingDir = Settings.ValidWorkingDir();
            bool hasWorkingDir = !string.IsNullOrEmpty(Settings.WorkingDir);
            branchSelect.Text = validWorkingDir ? GitCommandHelpers.GetSelectedBranch() : "";
            if (hasWorkingDir)
                HideDashboard();
            else
                ShowDashboard();
            tabControl1.Visible = validWorkingDir;
            commandsToolStripMenuItem.Enabled = validWorkingDir;
            manageRemoteRepositoriesToolStripMenuItem1.Enabled = validWorkingDir;
            branchSelect.Enabled = validWorkingDir;
            toolStripButton1.Enabled = validWorkingDir;
            toolStripButtonPull.Enabled = validWorkingDir;
            toolStripButtonPush.Enabled = validWorkingDir;
            submodulesToolStripMenuItem.Enabled = validWorkingDir;
            gitMaintenanceToolStripMenuItem.Enabled = validWorkingDir;
            editgitignoreToolStripMenuItem1.Enabled = validWorkingDir;
            editmailmapToolStripMenuItem.Enabled = validWorkingDir;
            toolStripSplitStash.Enabled = validWorkingDir;
            commitcountPerUserToolStripMenuItem.Enabled = validWorkingDir;
            _createPullRequestsToolStripMenuItem.Enabled = validWorkingDir;
            _viewPullRequestsToolStripMenuItem.Enabled = validWorkingDir;
            //Only show "Repository hosts" menu item when there is at least 1 repository host plugin loaded
            _repositoryHostsToolStripMenuItem.Visible = RepoHosts.GitHosters.Count > 0;
            if (RepoHosts.GitHosters.Count == 1)
                _repositoryHostsToolStripMenuItem.Text = RepoHosts.GitHosters[0].Description;
            InitToolStripBranchFilter(localToolStripMenuItem.Checked, remoteToolStripMenuItem.Checked);
            if (hard)
                ShowRevisions();
            _NO_TRANSLATE_Workingdir.Text = Settings.WorkingDir;
            Text = GenerateWindowTitle(Settings.WorkingDir, validWorkingDir, branchSelect.Text);
            DiffText.Font = Settings.DiffFont;
            UpdateJumplist(validWorkingDir);

            CheckForMergeConflicts();
            UpdateStashCount();
            // load custom user menu
            LoadUserMenu();
            Cursor.Current = Cursors.Default;
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
                return ReadRepositoryDescription(repositoryDir) ?? dirInfo.Name;
            else
                return dirInfo.Name;
        }

        private void LoadUserMenu()
        {
            var scripts = ScriptManager.GetScripts().Where(script => script.Enabled
                && script.OnEvent == ScriptEvent.ShowInUserMenuBar).ToList();

            for (int i = this.ToolStrip.Items.Count - 1; i >= 0; i--)
                if (this.ToolStrip.Items[i].Tag != null &&
                    this.ToolStrip.Items[i].Tag as String == "userscript")
                    this.ToolStrip.Items.RemoveAt(i);


            if (scripts.Count == 0)
                return;

            ToolStripSeparator toolstripseparator = new ToolStripSeparator();
            toolstripseparator.Tag = "userscript";
            this.ToolStrip.Items.Add(toolstripseparator);

            foreach (ScriptInfo scriptInfo in scripts)
            {
                ToolStripButton tempButton = new ToolStripButton();
                //store scriptname
                tempButton.Text = scriptInfo.Name;                
                tempButton.Tag = "userscript";
                //add handler
                tempButton.Click += new EventHandler(UserMenu_Click);
                tempButton.Enabled = true;
                tempButton.Visible = true;
                tempButton.Image = GitUI.Properties.Resources.bug;
                tempButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                //add to toolstrip
                this.ToolStrip.Items.Add((ToolStripItem)tempButton);
            }
        }

        void UserMenu_Click(object sender, EventArgs e)
        {
            ScriptRunner.RunScript(( (ToolStripButton) sender).Text, null);
        }

        private void UpdateJumplist(bool validWorkingDir)
        {
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


                    string path = Path.Combine(baseFolder, String.Format("{0}.{1}", repositoryDescription, "gitext"));
                    File.WriteAllText(path, Settings.WorkingDir);
                    JumpList.AddToRecent(path);
                }

                CreateOrUpdateTaskBarButtons(validWorkingDir);
            }
        }

        private void CreateOrUpdateTaskBarButtons(bool validRepo)
        {
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
                int stashCount = GitCommandHelpers.GetStashes().Count;
                toolStripSplitStash.Text = string.Format("{0} saved {1}", stashCount,
                                                         stashCount != 1 ? "stashes" : "stash");
            }
            else
            {
                toolStripSplitStash.Text = string.Empty;
            }
        }

        private void CheckForMergeConflicts()
        {
            bool validWorkingDir = Settings.ValidWorkingDir();

            if (validWorkingDir && GitCommandHelpers.InTheMiddleOfBisect())
            {

                if (_bisect == null)
                {
                    _bisect = new WarningToolStripItem { Text = "Your are in the middle of a bisect" };
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
                (GitCommandHelpers.InTheMiddleOfRebase() || GitCommandHelpers.InTheMiddleOfPatch()))
            {
                if (_rebase == null)
                {
                    _rebase = new WarningToolStripItem
                                  {
                                      Text = GitCommandHelpers.InTheMiddleOfRebase()
                                                 ? "You are in the middle of a rebase"
                                                 : "You are in the middle of a patch apply"
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

            if (validWorkingDir && GitCommandHelpers.InTheMiddleOfConflictedMerge() &&
                !Directory.Exists(Settings.WorkingDir + ".git\\rebase-apply\\"))
            {
                if (_warning == null)
                {
                    _warning = new WarningToolStripItem { Text = "There are unresolved merge conflicts!" };
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
                branchName = "no branch";
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
            const string repositoryDirectoryName = ".git";
            const string defaultDescription = "Unnamed repository; edit this file 'description' to name the repository.";

            var repositoryPath = Path.Combine(workingDir, repositoryDirectoryName);
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

        private void InitToolStripBranchFilter(bool local, bool remote)
        {
            toolStripBranches.Items.Clear();
            IEnumerable<string> branches = GetBranchHeads(local, remote);
            foreach (var branch in branches)
                toolStripBranches.Items.Add(branch);
        }

        private static IEnumerable<string> GetBranchHeads(bool local, bool remote)
        {
            var list = new List<string>();
            if (local && remote)
            {
                var branches = GitCommandHelpers.GetHeads(true, true);
                list.AddRange(branches.Where(branch => !branch.IsTag).Select(branch => branch.Name));
            }
            else if (local)
            {
                var branches = GitCommandHelpers.GetHeads(false);
                list.AddRange(branches.Select(branch => branch.Name));
            }
            else if (remote)
            {
                var branches = GitCommandHelpers.GetHeads(true, true);
                list.AddRange(branches.Where(branch => branch.IsRemote && !branch.IsTag).Select(branch => branch.Name));
            }
            return list;
        }

        private void RebaseClick(object sender, EventArgs e)
        {
            if (GitCommandHelpers.InTheMiddleOfRebase())
                GitUICommands.Instance.StartRebaseDialog(null);
            else
                GitUICommands.Instance.StartApplyPatchDialog();
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

        private void FillFileTree()
        {
            if (tabControl1.SelectedTab != Tree)
                return;

            try
            {
                GitTree.SuspendLayout();

                // Save state
                var lastSelectedNodes = new Stack<TreeNode>();
                lastSelectedNodes.Push(GitTree.SelectedNode);
                while (lastSelectedNodes.Peek() != null && lastSelectedNodes.Peek().Parent != null)
                    lastSelectedNodes.Push((lastSelectedNodes.Peek()).Parent);

                FileText.SaveCurrentScrollPos();

                // Refresh tree
                GitTree.Nodes.Clear();
                if (RevisionGrid.GetRevisions().Count > 0)
                    LoadInTree(RevisionGrid.GetRevisions()[0].SubItems, GitTree.Nodes);
                //GitTree.Sort();

                // Load state
                var currenNodes = GitTree.Nodes;
                while (lastSelectedNodes.Count > 0 && lastSelectedNodes.Peek() != null)
                {
                    var next = (lastSelectedNodes.Pop()).Text;
                    foreach (TreeNode node in currenNodes)
                    {
                        if (node.Text != next && next.Length != 40)
                            continue;

                        node.Expand();
                        GitTree.SelectedNode = node;
                        currenNodes = node.Nodes;
                    }
                }
            }
            finally
            {
                GitTree.ResumeLayout();
            }
        }



        private void FillDiff()
        {
            if (tabControl1.SelectedTab != Diff)
                return;

            var revisions = RevisionGrid.GetRevisions();

            DiffText.SaveCurrentScrollPos();

            switch (revisions.Count)
            {
                case 2:
                    DiffFiles.GitItemStatuses =
                        GitCommandHelpers.GetDiffFiles(revisions[0].Guid, revisions[1].Guid);
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
                            DiffFiles.GitItemStatuses = GitCommandHelpers.GetAllChangedFiles();
                        else
                            if (revision.Guid == GitRevision.IndexGuid) //index
                                DiffFiles.GitItemStatuses = GitCommandHelpers.GetStagedFiles();
                            else
                                DiffFiles.GitItemStatuses = GitCommandHelpers.GetDiffFiles(revision.Guid, revision.ParentGuids[0]);
                        DiffFiles.Revision = revision;
                    }
                    else
                        DiffFiles.GitItemStatuses = null;
                    break;
            }
        }


        private void FillCommitInfo()
        {
            if (tabControl1.SelectedTab != CommitInfo)
                return;

            if (RevisionGrid.GetRevisions().Count == 0)
                return;

            var revision = RevisionGrid.GetRevisions()[0];

            if (revision != null)
                RevisionInfo.SetRevision(revision.Guid);
        }

        public void FileHistoryOnClick(object sender, EventArgs e)
        {
            var item = GitTree.SelectedNode.Tag as GitItem;

            if (item == null)
                return;

            if (item.ItemType == "blob" || item.ItemType == "tree")
                GitUICommands.Instance.StartFileHistoryDialog(item.FileName);
        }

        public void FindFileOnClick(object sender, EventArgs e)
        {
            var searchWindow = new SearchWindow<string>(FindFileMatches)
            {
                Owner = this
            };
            searchWindow.ShowDialog();
            string selectedItem = searchWindow.SelectedItem;
            if (string.IsNullOrEmpty(selectedItem))
            {
                return;
            }

            string[] items = selectedItem.Split(new[] { '/' });
            TreeNodeCollection nodes = GitTree.Nodes;

            TreeNode selectedNode;
            for (int i = 0; i < items.Length - 1; i++)
            {
                selectedNode = Find(nodes, items[i]);

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
            var candidates = GitCommandHelpers.GetFullTree(RevisionGrid.GetRevisions()[0].TreeGuid);

            string nameAsLower = name.ToLower();

            return candidates.Where(fileName => fileName.ToLower().Contains(nameAsLower)).ToList();
        }





        public void OpenWithOnClick(object sender, EventArgs e)
        {
            var item = GitTree.SelectedNode.Tag;

            if (item is GitItem)
                if (((GitItem)item).ItemType == "blob")
                {
                    var fileName = ((GitItem)item).FileName;
                    if (fileName.Contains("\\") && fileName.LastIndexOf("\\") < fileName.Length)
                        fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                    if (fileName.Contains("/") && fileName.LastIndexOf("/") < fileName.Length)
                        fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);

                    fileName = (Path.GetTempPath() + fileName).Replace(Settings.PathSeparatorWrong, Settings.PathSeparator);
                    GitCommandHelpers.SaveBlobAs(fileName, ((GitItem)item).Guid);
                    OpenWith.OpenAs(fileName);
                }
        }

        private void FileTreeContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool enableItems = false;
            var item = (GitTree.SelectedNode != null) ? GitTree.SelectedNode.Tag : null;

            if (item is GitItem)
                if (((GitItem)item).ItemType == "blob")
                    enableItems = true;

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
                var item = GitTree.SelectedNode.Tag;
                if (item is GitItem)
                    if (((GitItem)item).ItemType == "blob")
                    {
                        var fileName = ((GitItem)item).FileName;
                        if (fileName.Contains("\\") && fileName.LastIndexOf("\\") < fileName.Length)
                            fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                        if (fileName.Contains("/") && fileName.LastIndexOf("/") < fileName.Length)
                            fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);

                        fileName = (Path.GetTempPath() + fileName).Replace(Settings.PathSeparatorWrong, Settings.PathSeparator);

                        GitCommandHelpers.SaveBlobAs(fileName, ((GitItem)item).Guid);

                        Process.Start(fileName);
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
                    if (gitItem.ItemType == "tree")
                    {
                        subNode.ImageIndex = 1;
                        subNode.SelectedImageIndex = 1;
                        subNode.Nodes.Add(new TreeNode());
                    }
                    else
                        if (gitItem.ItemType == "commit")
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
                if (RevisionGrid.GetRevisions().Count > 0 &&
                    (RevisionGrid.GetRevisions()[0].Guid == GitRevision.UncommittedWorkingDirGuid ||
                     RevisionGrid.GetRevisions()[0].Guid == GitRevision.IndexGuid))
                {
                    if (tabControl1.TabPages.Contains(CommitInfo))
                        tabControl1.TabPages.Remove(CommitInfo);
                    if (tabControl1.TabPages.Contains(Tree))
                        tabControl1.TabPages.Remove(Tree);
                }
                else
                {
                    if (!tabControl1.TabPages.Contains(CommitInfo))
                        tabControl1.TabPages.Insert(0, CommitInfo);
                    if (!tabControl1.TabPages.Contains(Tree))
                        tabControl1.TabPages.Insert(1, Tree);
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
            if (new Open().ShowDialog() == DialogResult.OK)
            {
                IndexWatcher.Clear();
                RevisionGrid.ForceRefreshRevisions();
                InternalInitialize(false);
                IndexWatcher.Reset();
            }
        }

        private void CheckoutToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCheckoutRevisionDialog())
                Initialize();
        }

        private void GitTreeDoubleClick(object sender, EventArgs e)
        {
            if (GitTree.SelectedNode == null || !(GitTree.SelectedNode.Tag is IGitItem))
                return;

            var item = GitTree.SelectedNode.Tag as GitItem;
            if (item == null)
                return;

            if (item.ItemType != "blob")
                return;

            if (GitUICommands.Instance.StartFileHistoryDialog(item.FileName))
                Initialize();
        }

        private void ViewDiffToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCompareRevisionsDialog())
                Initialize();
        }

        private void CloneToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCloneDialog())
                Initialize();
        }

        private void CommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCommitDialog())
                Initialize();
        }

        private void InitNewRepositoryToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartInitializeDialog())
                Initialize();
        }

        private void PushToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartPushDialog())
                Initialize();
        }

        private void PullToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartPullDialog())
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
            new AboutBox().ShowDialog();
        }

        private void PatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartViewPatchDialog())
                Initialize();
        }

        private void ApplyPatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartApplyPatchDialog())
                Initialize();
        }

        private void GitBashToolStripMenuItemClick1(object sender, EventArgs e)
        {
            GitCommandHelpers.RunBash();
        }

        private void GitGuiToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitCommandHelpers.RunGui();
        }

        private void FormatPatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartFormatPatchDialog())
                Initialize();
        }

        private void GitcommandLogToolStripMenuItemClick(object sender, EventArgs e)
        {
            new GitLogForm().ShowDialog();
        }


        private void CheckoutBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCheckoutBranchDialog())
                Initialize();
        }

        private void StashToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartStashDialog())
                Initialize();
        }

        private void RunMergetoolToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartResolveConflictsDialog())
                Initialize();
        }

        private void WarningClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartResolveConflictsDialog())
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
            if (GitUICommands.Instance.StartDeleteBranchDialog(null))
                Initialize();
        }

        private void CherryPickToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCherryPickDialog())
                Initialize();
        }

        private void MergeBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartMergeBranchDialog(null))
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
            if (GitUICommands.Instance.StartCreateTagDialog())
                Initialize();
        }

        private void RefreshButtonClick(object sender, EventArgs e)
        {
            RefreshToolStripMenuItemClick(sender, e);
        }

        private void CommitcountPerUserToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormCommitCount().ShowDialog();
        }

        private void KGitToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitCommandHelpers.RunGitK();
        }

        private void DonateToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormDonate().ShowDialog();
        }

        private void DeleteTagToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartDeleteTagDialog())
                Initialize();
        }

        private void FormBrowseFormClosing(object sender, FormClosingEventArgs e)
        {
            SaveUserMenuPosition();
            SavePosition("browse");
        }

        private void SaveUserMenuPosition()
        {
            GitCommands.Settings.UserMenuLocationX = this.UserMenuToolStrip.Location.X;
            GitCommands.Settings.UserMenuLocationY = this.UserMenuToolStrip.Location.Y;
        }

        private void EditGitignoreToolStripMenuItem1Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartEditGitIgnoreDialog())
                Initialize();
        }

        private void SettingsToolStripMenuItem2Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartSettingsDialog();

            Translate();
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            Initialize();
            RevisionGrid.ReloadHotkeys();
            RevisionGrid.ReloadTranslation();
        }

        private void ArchiveToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartArchiveDialog())
                Initialize();
        }

        private void EditMailMapToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartMailMapDialog())
                Initialize();
        }

        private void CompressGitDatabaseToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormProcess("gc").ShowDialog();
        }

        private void VerifyGitDatabaseToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartVerifyDatabaseDialog())
                Initialize();
        }

        private void ManageRemoteRepositoriesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartRemotesDialog())
                Initialize();
        }

        private void RebaseToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartRebaseDialog(null))
                Initialize();
        }

        private void StartAuthenticationAgentToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitCommandHelpers.StartExternalCommand(Settings.Pageant, "");
        }

        private void GenerateOrImportKeyToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitCommandHelpers.StartExternalCommand(Settings.Puttygen, "");
        }

        private void SetFilter(string filter)
        {
            if (string.IsNullOrEmpty(filter)) return;
            toolStripTextBoxFilter.Text = filter;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string revListArgs;
            string inMemMessageFilter;
            string inMemCommitterFilter;
            string inMemAuthorFilter;
            var filterParams = new bool[4];
            filterParams[0] = commitToolStripMenuItem1.Checked;
            filterParams[1] = committerToolStripMenuItem.Checked;
            filterParams[2] = authorToolStripMenuItem.Checked;
            filterParams[3] = diffContainsToolStripMenuItem.Checked;
            try
            {
                RevisionGrid.FormatQuickFilter(toolStripTextBoxFilter.Text,
                                               filterParams,
                                               out revListArgs,
                                               out inMemMessageFilter,
                                               out inMemCommitterFilter,
                                               out inMemAuthorFilter);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Filter error");
                toolStripTextBoxFilter.Text = "";
                return;
            }

            if ((RevisionGrid.Filter == revListArgs) &&
                (RevisionGrid.InMemMessageFilter == inMemMessageFilter) &&
                (RevisionGrid.InMemCommitterFilter == inMemCommitterFilter) &&
                (RevisionGrid.InMemAuthorFilter == inMemAuthorFilter) &&
                (RevisionGrid.InMemFilterIgnoreCase))
                return;
            RevisionGrid.Filter = revListArgs;
            RevisionGrid.InMemMessageFilter = inMemMessageFilter;
            RevisionGrid.InMemCommitterFilter = inMemCommitterFilter;
            RevisionGrid.InMemAuthorFilter = inMemAuthorFilter;
            RevisionGrid.InMemFilterIgnoreCase = true;
            RevisionGrid.ForceRefreshRevisions();
        }

        private void ApplyBranchFilter()
        {
            bool success = RevisionGrid.SetAndApplyBranchFilter(toolStripBranches.Text);
            if (success) RevisionGrid.ForceRefreshRevisions();
        }

        private void UpdateBranchFilterItems()
        {
            string filter = toolStripBranches.Text;
            toolStripBranches.Items.Clear();
            var index = toolStripBranches.Text.Length;
            var branches = GetBranchHeads(localToolStripMenuItem.Checked, remoteToolStripMenuItem.Checked);
            foreach (var branch in branches)
                if (branch.Contains(filter))
                    toolStripBranches.Items.Add(branch);
            toolStripBranches.SelectionStart = index;
        }

        private void ToolStripTextBoxFilterLeave(object sender, EventArgs e)
        {
            ToolStripLabel2Click(sender, e);
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
            var revisions = RevisionGrid.GetRevisions();

            if (revisions.Count == 0)
                return;

            DiffText.ViewPatch(() =>
                                   {
                                       string selectedPatch = GetSelectedPatch(revisions, selectedItem);

                                       return selectedPatch ?? String.Empty;
                                   });
        }

        private string GetSelectedPatch(IList<GitRevision> revisions, GitItemStatus file)
        {
            if (revisions[0].Guid == GitRevision.UncommittedWorkingDirGuid) //working dir changes
            {
                if (file.IsTracked)
                    return GitCommandHelpers.GetCurrentChanges(file.Name, file.OldName, false, DiffText.GetExtraDiffArguments());
                return FileReader.ReadFileContent(Settings.WorkingDir + file.Name, Settings.Encoding);
            }
            if (revisions[0].Guid == GitRevision.IndexGuid) //index
            {
                return GitCommandHelpers.GetCurrentChanges(file.Name, file.OldName, true, DiffText.GetExtraDiffArguments());
            }
            var secondRevision = revisions.Count == 2 ? revisions[1].Guid : revisions[0].ParentGuids[0];

            PatchApply.Patch patch = GitCommandHelpers.GetSingleDiff(revisions[0].Guid, secondRevision, file.Name, file.OldName,
                                                    DiffText.GetExtraDiffArguments());

            if (patch == null)
                return string.Empty;

            return patch.Text;
        }

        private void ChangelogToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormChangeLog().ShowDialog();
        }

        private void DiffFilesDoubleClick(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null)
                return;

            if (GitUICommands.Instance.StartFileHistoryDialog((DiffFiles.SelectedItem).Name))
                Initialize();
        }

        private void ToolStripButtonPushClick(object sender, EventArgs e)
        {
            PushToolStripMenuItemClick(sender, e);
        }

        private void ToolStripTextBoxFilterKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                ToolStripLabel2Click(null, null);
        }

        private void ManageSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartSubmodulesDialog())
                Initialize();
        }

        private void UpdateAllSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartUpdateSubmodulesDialog())
                Initialize();
        }

        private void UpdateAllSubmodulesRecursiveToolStripMenuItemClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            if (GitUICommands.Instance.StartUpdateSubmodulesRecursiveDialog())
                Initialize();
            Cursor.Current = Cursors.Default;
        }

        private void InitializeAllSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
        {
            var process = new FormProcess(GitCommandHelpers.SubmoduleInitCmd(""));
            process.ShowDialog();
            Initialize();
        }

        private void InitializeAllSubmodulesRecursiveToolStripMenuItemClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var process = new FormProcess(GitCommandHelpers.SubmoduleInitCmd(""));
            process.ShowDialog();
            InitSubmodulesRecursive();
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private static void InitSubmodulesRecursive()
        {
            var oldworkingdir = Settings.WorkingDir;

            foreach (GitSubmodule submodule in (new GitCommandsInstance()).GetSubmodules())
            {
                if (string.IsNullOrEmpty(submodule.LocalPath))
                    continue;

                Settings.WorkingDir = oldworkingdir + submodule.LocalPath;

                if (Settings.WorkingDir != oldworkingdir && File.Exists(Settings.WorkingDir + ".gitmodules"))
                {
                    var process = new FormProcess(GitCommandHelpers.SubmoduleInitCmd(""));
                    process.ShowDialog();

                    InitSubmodulesRecursive();
                }

                Settings.WorkingDir = oldworkingdir;
            }

            Settings.WorkingDir = oldworkingdir;
        }

        private void SyncronizeAllSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
        {
            var process = new FormProcess(GitCommandHelpers.SubmoduleSyncCmd(""));
            process.ShowDialog();
            Initialize();
        }

        private void SynchronizeAllSubmodulesRecursiveToolStripMenuItemClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var process = new FormProcess(GitCommandHelpers.SubmoduleSyncCmd(""));
            process.ShowDialog();
            SyncSubmodulesRecursive();
            Initialize();
            Cursor.Current = Cursors.Default;
        }

        private static void SyncSubmodulesRecursive()
        {
            var oldworkingdir = Settings.WorkingDir;

            foreach (GitSubmodule submodule in (new GitCommandsInstance()).GetSubmodules())
            {
                if (string.IsNullOrEmpty(submodule.LocalPath))
                    continue;

                Settings.WorkingDir = oldworkingdir + submodule.LocalPath;

                if (Settings.WorkingDir != oldworkingdir && File.Exists(Settings.WorkingDir + ".gitmodules"))
                {
                    var process = new FormProcess(GitCommandHelpers.SubmoduleSyncCmd(""));
                    process.ShowDialog();

                    SyncSubmodulesRecursive();
                }

                Settings.WorkingDir = oldworkingdir;
            }

            Settings.WorkingDir = oldworkingdir;
        }


        private void ToolStripSplitStashButtonClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartStashDialog())
                Initialize();
        }

        private void StashChangesToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormProcess("stash save").ShowDialog();
            Initialize();
        }

        private void StashPopToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormProcess("stash pop").ShowDialog();
            Initialize();
        }

        private void ViewStashToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartStashDialog())
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

            var submodules = GitCommandHelpers.GetSubmodulesNames();

            foreach (var submodule in submodules)
            {
                var submenu = new ToolStripButton(submodule/*.Name*/);
                submenu.Click += SubmoduleToolStripButtonClick;
                submenu.Width = 200;
                openSubmoduleToolStripMenuItem.DropDownItems.Add(submenu);
            }

            if (openSubmoduleToolStripMenuItem.DropDownItems.Count == 0)
                openSubmoduleToolStripMenuItem.DropDownItems.Add("No submodules");
            Cursor.Current = Cursors.Default;
        }

        private void RemoveSubmoduleButtons()
        {
            foreach (var item in openSubmoduleToolStripMenuItem.DropDownItems)
            {
                var toolStripButton = item as ToolStripButton;
                if (toolStripButton != null)
                    toolStripButton.Click -= SubmoduleToolStripButtonClick;
            }
            openSubmoduleToolStripMenuItem.DropDownItems.Clear();
        }

        private void SubmoduleToolStripButtonClick(object sender, EventArgs e)
        {
            var button = sender as ToolStripButton;

            if (button == null)
                return;

            Settings.WorkingDir += GitCommandHelpers.GetSubmoduleLocalPath(button.Text);

            if (Settings.ValidWorkingDir())
                Repositories.RepositoryHistory.AddMostRecentRepository(Settings.WorkingDir);

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

                var historyItemMenu = new ToolStripButton(historyItem.Path);
                historyItemMenu.Click += HistoryItemMenuClick;
                historyItemMenu.Width = 225;
                recentToolStripMenuItem.DropDownItems.Add(historyItemMenu);
            }
        }

        private void HistoryItemMenuClick(object sender, EventArgs e)
        {
            var button = sender as ToolStripButton;

            if (button == null)
                return;

            Settings.WorkingDir = button.Text;
            InternalInitialize(true);
        }

        private void SettingsToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartPluginSettingsDialog();
        }

        private void CloseToolStripMenuItemClick(object sender, EventArgs e)
        {
            Settings.WorkingDir = "";

            IndexWatcher.Clear();
            RevisionGrid.ForceRefreshRevisions();
            InternalInitialize(false);
            IndexWatcher.Reset();
        }

        public override void CancelButtonClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.WorkingDir))
            {
                Close();
                return;
            }
            Settings.WorkingDir = "";

            IndexWatcher.Clear();
            RevisionGrid.ForceRefreshRevisions();
            InternalInitialize(false);
            IndexWatcher.Reset();
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
                MessageBox.Show(ex.Message);
            }
        }

        private void DiffTextExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ShowSelectedFileDiff();
        }

        private void CleanupToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormCleanupRepository().ShowDialog();
        }

        private void openWithDifftoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null)
                return;
            var selectedItem = (DiffFiles.SelectedItem).Name;

            IList<GitRevision> revisions = RevisionGrid.GetRevisions();

            if (revisions.Count == 0)
                return;

            string output;
            if (revisions.Count == 1)   // single item selected
                output = GitCommandHelpers.OpenWithDifftool(selectedItem, revisions[0].Guid,
                                                                  revisions[0].ParentGuids[0]);
            else                        // multiple items selected
                output = GitCommandHelpers.OpenWithDifftool(selectedItem, revisions[0].Guid,
                                                                  revisions[revisions.Count - 1].Guid);

            if (!string.IsNullOrEmpty(output))
                MessageBox.Show(output);
        }

        private void WorkingdirDropDownOpening(object sender, EventArgs e)
        {
            _NO_TRANSLATE_Workingdir.DropDownItems.Clear();
            foreach (var repository in Repositories.RepositoryHistory.Repositories)
            {
                var toolStripItem = _NO_TRANSLATE_Workingdir.DropDownItems.Add(repository.Path);
                toolStripItem.Click += ToolStripItemClick;
            }
        }

        private void ToolStripItemClick(object sender, EventArgs e)
        {
            var toolStripItem = (ToolStripItem)sender;
            if (toolStripItem == null)
                return;

            Settings.WorkingDir = toolStripItem.Text;
            Repositories.RepositoryHistory.AddMostRecentRepository(Settings.WorkingDir);
            InternalInitialize(true);
        }

        private void TranslateToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormTranslate().ShowDialog();
        }

        private void FileExplorerToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Process.Start(_NO_TRANSLATE_Workingdir.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            if (item.ItemType != "blob")
                return;

            var fileDialog =
                new SaveFileDialog
                    {
                        FileName = Settings.WorkingDir + item.FileName,
                        AddExtension = true
                    };
            fileDialog.DefaultExt = GitCommandHelpers.GetFileExtension(fileDialog.FileName);
            fileDialog.Filter =
                "Current format (*." +
                GitCommandHelpers.GetFileExtension(fileDialog.FileName) + ")|*." +
                GitCommandHelpers.GetFileExtension(fileDialog.FileName) +
                "|All files (*.*)|*.*";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                GitCommandHelpers.SaveBlobAs(fileDialog.FileName, item.Guid);
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
            if (GitUICommands.Instance.StartCreateBranchDialog())
                Initialize();
        }

        private void RevisionGridDoubleClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartCompareRevisionsDialog();
        }

        private void GitBashClick(object sender, EventArgs e)
        {
            GitBashToolStripMenuItemClick1(sender, e);
        }

        private void ToolStripLabel2Click(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void ToolStripButtonPullClick(object sender, EventArgs e)
        {
            PullToolStripMenuItemClick(sender, e);
        }

        private void editgitattributesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartEditGitAttributesDialog())
                Initialize();
        }

        private void copyFilenameToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = GitTree.SelectedNode.Tag;

            if (item is GitItem)
            {
                var fileName = Settings.WorkingDir + ((GitItem)item).FileName;

                Clipboard.SetText(fileName.Replace('/', '\\'));
            }
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
            string fileName = Path.Combine(Settings.WorkingDirGitDir(), "index.lock");

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
                MessageBox.Show("index.lock deleted.");
            }
            else
                MessageBox.Show("index.lock not found at: " + fileName);
        }

        private void saveAsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = RevisionGrid.GetRevisions();

            if (revisions.Count == 0)
                return;


            if (DiffFiles.SelectedItem == null)
                return;

            GitItemStatus item = DiffFiles.SelectedItem;

            var fileDialog =
                new SaveFileDialog
                {
                    InitialDirectory = Settings.WorkingDir,
                    FileName = item.Name.Replace(Settings.PathSeparatorWrong, Settings.PathSeparator),
                    AddExtension = true
                };
            fileDialog.DefaultExt = GitCommandHelpers.GetFileExtension(fileDialog.FileName);
            fileDialog.Filter =
                "Current format (*." +
                GitCommandHelpers.GetFileExtension(fileDialog.FileName) + ")|*." +
                GitCommandHelpers.GetFileExtension(fileDialog.FileName) +
                "|All files (*.*)|*.*";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                GitCommandHelpers.SaveBlobAs(fileDialog.FileName, string.Format("{0}:\"{1}\"", revisions[0].Guid, item.Name));
            }
        }

        private void toolStripBranches_TextUpdate(object sender, EventArgs e)
        {
            UpdateBranchFilterItems();
        }

        private void toolStripBranches_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == (char)Keys.Enter)
            {
                ApplyBranchFilter();
            }
        }

        private void toolStripBranches_DropDown(object sender, EventArgs e)
        {
            InitToolStripBranchFilter(localToolStripMenuItem.Checked, remoteToolStripMenuItem.Checked);
            UpdateBranchFilterItems();
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            statusStrip.Hide();
        }

        private void toolStripBranches_Leave(object sender, EventArgs e)
        {
            ApplyBranchFilter();
        }

        private void openWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = GitTree.SelectedNode.Tag;

            if (item is GitItem)
                if (((GitItem)item).ItemType == "blob")
                {
                    string fileName = ((GitItem)item).FileName;
                    fileName = Settings.WorkingDir + fileName;

                    OpenWith.OpenAs(fileName.Replace(Settings.PathSeparatorWrong, Settings.PathSeparator));
                }

        }

        private void pluginsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            LoadPluginsInPluginMenu();
        }

        private void bisectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormBisect().ShowDialog();
            Initialize();
        }

        private void BisectClick(object sender, EventArgs e)
        {
            new FormBisect().ShowDialog();
            Initialize();
        }

        private void fileHistoryDiffToolstripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedItem;

            if (item.IsTracked)
                GitUICommands.Instance.StartFileHistoryDialog(item.Name);
        }

        private void CurrentBranchDropDownOpening(object sender, EventArgs e)
        {
            branchSelect.DropDownItems.Clear();
            foreach (var branch in GitCommandHelpers.GetHeads(false))
            {
                var toolStripItem = branchSelect.DropDownItems.Add(branch.Name);
                toolStripItem.Click += BranchSelectToolStripItem_Click;
            }
        }

        void BranchSelectToolStripItem_Click(object sender, EventArgs e)
        {
            var toolStripItem = (ToolStripItem)sender;

            var command = string.Format("checkout \"{0}\"", toolStripItem.Text);
            var form = new FormProcess(command);
            form.ShowDialog(this);
            Initialize();
        }

        private void diffContainsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (diffContainsToolStripMenuItem.Checked)
            {
                commitToolStripMenuItem1.Checked = false;
                committerToolStripMenuItem.Checked = false;
                authorToolStripMenuItem.Checked = false;
            }
            else
                commitToolStripMenuItem1.Checked = true;
        }

        private void _forkCloneMenuItem_Click(object sender, EventArgs e)
        {
            if (RepoHosts.GitHosters.Count > 0)
            {
                GitUICommands.Instance.StartCloneForkFromHoster(RepoHosts.GitHosters[0]); //FIXME: Works untill we have > 1 repo hoster
                Initialize();
            }
            else
            {
                MessageBox.Show(this, "No repository host plugin loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void _viewPullRequestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var repoHost = RepoHosts.TryGetGitHosterForCurrentWorkingDir();
            if (repoHost == null)
            {
                MessageBox.Show(this, "Could not find any relevant repository hosts for the currently open repository.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            GitUICommands.Instance.StartPullRequestsDialog(repoHost);
            Initialize();
        }

        private void _createPullRequestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var repoHost = RepoHosts.TryGetGitHosterForCurrentWorkingDir();
            if (repoHost == null)
            {
                MessageBox.Show(this, "Could not find any relevant repository hosts for the currently open repository.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            GitUICommands.Instance.StartCreatePullRequest(repoHost);
            Initialize();
        }

        #region Hotkey commands

        public const string HotkeySettingsName = "Browse";

        internal enum Commands : int
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
            QuickPush,
            RotateApplicationIcon,
        }

        private void AddNotes()
        {
            if (RevisionGrid.GetRevisions().Count > 0)
                GitCommandHelpers.EditNotes(RevisionGrid.GetRevisions()[0].Guid);
            else
                GitCommandHelpers.EditNotes(string.Empty);

            FillCommitInfo();
        }

        private void FindFileInSelectedCommit()
        {
            tabControl1.SelectedTab = Tree;
            EnabledSplitViewLayout(true);
            GitTree.Focus();
            FindFileOnClick(null, null);
        }

        private void QuickFetch()
        {
            new FormProcess(GitCommandHelpers.FetchCmd(string.Empty, string.Empty, string.Empty)).ShowDialog();
            Initialize();
        }


        protected override bool ExecuteCommand(int cmd)
        {
            Commands command = (Commands)cmd;

            switch (command)
            {
                case Commands.GitBash: GitCommandHelpers.RunBash(); break;
                case Commands.GitGui: GitCommandHelpers.RunGui(); break;
                case Commands.GitGitK: GitCommandHelpers.RunGitK(); break;
                case Commands.FocusRevisionGrid: RevisionGrid.Focus(); break;
                case Commands.FocusCommitInfo: tabControl1.SelectedTab = CommitInfo; break;
                case Commands.FocusFileTree: tabControl1.SelectedTab = Tree; GitTree.Focus(); break;
                case Commands.FocusDiff: tabControl1.SelectedTab = Diff; DiffFiles.Focus(); break;
                case Commands.Commit: CommitToolStripMenuItemClick(null, null); break;
                case Commands.AddNotes: AddNotes(); break;
                case Commands.FindFileInSelectedCommit: FindFileInSelectedCommit(); break;
                case Commands.SelectCurrentRevision: RevisionGrid.SetSelectedRevision(new GitRevision(RevisionGrid.CurrentCheckout)); break;
                case Commands.CheckoutBranch: CheckoutBranchToolStripMenuItemClick(null, null); break;
                case Commands.QuickFetch: QuickFetch(); break;
                case Commands.QuickPush: GitUICommands.Instance.StartPushDialog(true); break;
                case Commands.RotateApplicationIcon: RotateApplicationIcon(); break;
                default: ExecuteScriptCommand(cmd, Keys.None); break;
            }

            return true;
        }

        #endregion

        private void goToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormGoToCommit formGoToCommit = new FormGoToCommit();
            formGoToCommit.ShowDialog();
            string revisionGuid = formGoToCommit.GetRevision();
            if (!string.IsNullOrEmpty(revisionGuid))
            {
                RevisionGrid.SetSelectedRevision(new GitRevision(revisionGuid));
            }
            else
            {
                MessageBox.Show("No revision found.");
            }
        }

        private void toggleSplitViewLayout_Click(object sender, EventArgs e)
        {
            EnabledSplitViewLayout(splitContainer3.Panel2.Height == 0 && splitContainer3.Height > 0);
        }

        private void EnabledSplitViewLayout(bool enabled)
        {
            if (enabled)
                splitContainer3.SplitterDistance = (splitContainer3.Height / 5) * 2;
            else
                splitContainer3.SplitterDistance = splitContainer3.Height;
        }

        private void editCheckedOutFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = GitTree.SelectedNode.Tag;

            if (item is GitItem)
                if (((GitItem)item).ItemType == "blob")
                {
                    string fileName = ((GitItem)item).FileName;
                    fileName = Settings.WorkingDir + fileName;

                    new FormEditor(fileName).ShowDialog();
                }
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

            if (!gitTree.Focused)
                gitTree.Focus();

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

        private Tuple<int, string> getNextPatchFile()
        {
            var revisions = RevisionGrid.GetRevisions();
            if (revisions.Count == 0)
                return null;
            int idx = DiffFiles.SelectedIndex;
            if(idx == -1){
                return new Tuple<int, string>(idx, null);
            } else {
                IList<GitItemStatus> items = DiffFiles.GitItemStatuses;
                if (idx == items.Count - 1)
                {
                    idx = 0;
                }
                else
                {
                    idx++;
                }
                _dontUpdateOnIndexChange = true;
                DiffFiles.SelectedIndex = idx;
                _dontUpdateOnIndexChange = false;
                Tuple<int, string> tuple = new Tuple<int, string>(idx, GetSelectedPatch(revisions, DiffFiles.SelectedItem));
                return tuple;
            }
        }

    }
}