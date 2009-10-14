using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using System.Text.RegularExpressions;
using PatchApply;
using System.IO;
using GitUI.Properties;
using Settings=GitCommands.Settings;
using GitUIPluginInterfaces;

namespace GitUI
{
    public partial class FormBrowse : GitExtensionsForm
    {
        public FormBrowse()
        {
            InitializeComponent();
            RevisionGrid.SelectionChanged += new EventHandler(RevisionGrid_SelectionChanged);
        }


        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            IGitItem item = (IGitItem)e.Node.Tag;
            if (item is GitItem)
                if (((GitItem)item).ItemType == "blob")
                {
                    FileText.ViewGitItem(((GitItem)item).FileName, item.Guid);
                }
                else
                {
                    FileText.ViewText("", "");
                }
        }

        private void Browse_Load(object sender, EventArgs e)
        {
            bool t = Application.MessageLoop;
			// Restore eventual saved Windows state
        	RestoreWindowsPositionAndState();

            Cursor.Current = Cursors.WaitCursor;
            InternalInitialize(false);
            RevisionGrid.Focus();
            RevisionGrid.ChangedCurrentBranch += RevisionGrid_ChangedCurrentBranch;
            indexWatcher.Changed += new EventHandler(indexWatcher_Changed);
            indexWatcher.Reset();

            foreach (IGitPlugin plugin in GitUIPluginCollection.Plugins)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = plugin.Description;
                item.Tag = plugin;
                item.Click += new EventHandler(item_Click);
                pluginsToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        void item_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem != null)
            {
                IGitPlugin plugin = menuItem.Tag as IGitPlugin;
                if (plugin != null)
                {
                    GitUIEventArgs eventArgs = new GitUIEventArgs(GitUICommands.Instance);
                    plugin.Execute(eventArgs);
                }
            }
        }

		private void RestoreWindowsPositionAndState()
		{
			// this is the default
			this.WindowState = FormWindowState.Normal;
			this.StartPosition = FormStartPosition.WindowsDefaultBounds;

			// check if the saved bounds are nonzero and visible on any screen
			if (GitUI.Properties.Settings.Default.WindowPosition != Rectangle.Empty &&
				IsVisibleOnAnyScreen(GitUI.Properties.Settings.Default.WindowPosition))
			{
				// first set the bounds
				this.StartPosition = FormStartPosition.Manual;
				this.DesktopBounds = GitUI.Properties.Settings.Default.WindowPosition;

				// afterwards set the window state to the saved value (which could be Maximized)
				this.WindowState = GitUI.Properties.Settings.Default.WindowState;
			}
			else
			{
				this.StartPosition = FormStartPosition.WindowsDefaultLocation;
			}
		}

		/// <summary>
		/// Check to see if a windows position is visible on any screen (multi-monitor setup)
		/// </summary>
		/// <param name="rect"></param>
		/// <returns></returns>
		private static bool IsVisibleOnAnyScreen(Rectangle rect)
		{
			foreach (Screen screen in Screen.AllScreens)
			{
				if (screen.WorkingArea.IntersectsWith(rect))
				{
					return true;
				}
			}
			return false;
		}

        void RevisionGrid_ChangedCurrentBranch(object sender, EventArgs e)
        {
            Initialize();
        }

        void indexWatcher_Changed(object sender, EventArgs e)
        {
            /*if (ToolStrip.InvokeRequired)
            {
                DoneCallback d = new DoneCallback(SetIndexDirty);
                this.Invoke(d, new object[] { });
            }
            else
            {
                SetIndexDirty();
            }*/
        }

        private void SetIndexDirty()
        {
            RefreshButton.Image = Resources.arrow_refresh_dirty;
        }

        private void SetIndexClean()
        {
            RefreshButton.Image = Resources.arrow_refresh;
        }


        private ToolStripItem warning;
        private ToolStripItem rebase;

        protected void Initialize()
        {
            try
            {
                InternalInitialize(true);
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        IndexWatcher indexWatcher = new IndexWatcher();

        private void InternalInitialize(bool hard)
        {
            SetIndexClean();

            Cursor.Current = Cursors.WaitCursor;
            string selectedHead = GitCommands.GitCommands.GetSelectedBranch();
            CurrentBranch.Text = selectedHead;

            bool validWorkingDir = GitCommands.Settings.ValidWorkingDir();
            NoGit.Visible = !validWorkingDir;
            tabControl1.Visible = validWorkingDir;
            commandsToolStripMenuItem.Enabled = validWorkingDir;
            manageRemoteRepositoriesToolStripMenuItem1.Enabled = validWorkingDir;
            CurrentBranch.Enabled = validWorkingDir;
            toolStripButton1.Enabled = validWorkingDir;
            toolStripButtonPull.Enabled = validWorkingDir;
            toolStripButtonPush.Enabled = validWorkingDir;
            submodulesToolStripMenuItem.Enabled = validWorkingDir;
            gitMaintenanceToolStripMenuItem.Enabled = validWorkingDir;
            editgitignoreToolStripMenuItem1.Enabled = validWorkingDir;
            editmailmapToolStripMenuItem.Enabled = validWorkingDir;
            toolStripSplitStash.Enabled = validWorkingDir;
            commitcountPerUserToolStripMenuItem.Enabled = validWorkingDir;

            if (NoGit.Visible)
            {
                int xStart = 10;
                int yStart = 25;
                RecentRepositoriesGroupBox.Controls.Clear();

                foreach (string historyItem in RepositoryHistory.MostRecentRepositories)
                {
                    LinkLabel label = new LinkLabel();
                    label.Text = historyItem;
                    label.Location = new Point(xStart, yStart);
                    label.Size = new Size(RecentRepositoriesGroupBox.Width - 20, 20);
                    label.Click += new EventHandler(label_Click);
                   

                    ToolTip toolTip = new ToolTip();
                    toolTip.InitialDelay = 0;
                    toolTip.AutomaticDelay = 0;
                    toolTip.AutoPopDelay = 0;
                    toolTip.UseFading = false;
                    toolTip.UseAnimation = false;
                    toolTip.ReshowDelay = 0;
                    toolTip.SetToolTip(label, label.Text);

                    RecentRepositoriesGroupBox.Controls.Add(label);
                    yStart += 20;
                }
            }

            if (hard)
                ShowRevisions();

            Workingdir.Text = GitCommands.Settings.WorkingDir;
            this.Text = GitCommands.Settings.WorkingDir + " - Git Extensions";

            if (validWorkingDir && (GitCommands.GitCommands.InTheMiddleOfRebase() || GitCommands.GitCommands.InTheMiddleOfPatch()))
            {
                if (rebase == null)
                {
                    if (GitCommands.GitCommands.InTheMiddleOfRebase())
                        rebase = ToolStrip.Items.Add("You are in the middle of a rebase");
                    else
                        rebase = ToolStrip.Items.Add("You are in the middle of a patch apply");

                    rebase.BackColor = Color.Salmon;
                    rebase.Click += new EventHandler(rebase_Click);
                }
            }
            else
            {
                if (rebase != null)
                {
                    rebase.Click -= new EventHandler(warning_Click);
                    ToolStrip.Items.Remove(rebase);
                    rebase = null;
                }
            }

            if (validWorkingDir && GitCommands.GitCommands.InTheMiddleOfConflictedMerge() && !Directory.Exists(GitCommands.Settings.WorkingDir + ".git\\rebase-apply\\"))
            {
                if (warning == null)
                {
                    warning = ToolStrip.Items.Add("There are unresolved merge conflicts!");
                    warning.BackColor = Color.Salmon;
                    warning.Click += new EventHandler(warning_Click);
                }
            }
            else
            {
                if (warning != null)
                {
                    warning.Click -= new EventHandler(warning_Click);
                    ToolStrip.Items.Remove(warning);
                    warning = null;
                }
            }

        }

        void label_Click(object sender, EventArgs e)
        {
            LinkLabel label = sender as LinkLabel;
            if (label != null && !string.IsNullOrEmpty(label.Text))
            {
                Settings.WorkingDir = label.Text;
                RepositoryHistory.AddMostRecentRepository(Settings.WorkingDir);

                indexWatcher.Clear();
                RevisionGrid.ForceRefreshRevisions();
                InternalInitialize(false);
                indexWatcher.Reset();
            }

        }

        void rebase_Click(object sender, EventArgs e)
        {
            if (GitCommands.GitCommands.InTheMiddleOfRebase())
                GitUICommands.Instance.StartRebaseDialog();
            else
                GitUICommands.Instance.StartApplyPatchDialog();
            Initialize();
        }


        private void ShowRevisions()
        {
            if (indexWatcher.IndexChanged)
            {
                RevisionGrid.RefreshRevisions();
                FillFileTree();
                FillDiff();
                FillCommitInfo();
            }
            indexWatcher.Reset();
        }
        
        private void FillFileTree()
        {
            if (tabControl1.SelectedTab == Tree)
            {
                //Save state
                Stack<TreeNode> lastSelectedNodes;
                lastSelectedNodes = new Stack<TreeNode>();
                lastSelectedNodes.Push(GitTree.SelectedNode);
                while (lastSelectedNodes.Peek() != null && lastSelectedNodes.Peek().Parent != null)
                    lastSelectedNodes.Push(((TreeNode)lastSelectedNodes.Peek()).Parent);

                int scrollPos = FileText.ScrollPos;

                //Refresh tree
                GitTree.Nodes.Clear();
                if (RevisionGrid.GetRevisions().Count > 0)
                    LoadInTreeSingle(RevisionGrid.GetRevisions()[0], GitTree.Nodes);
                GitTree.Sort();


                //Load state
                TreeNodeCollection currenNodes = GitTree.Nodes;
                while (lastSelectedNodes.Count > 0 && lastSelectedNodes.Peek() != null)
                {
                    //TreeNode[] nodes = currenNodes.Find(((TreeNode)lastSelectedNodes.Pop()).Text, false);
                    string next = ((TreeNode)lastSelectedNodes.Pop()).Text;
                    foreach (TreeNode node in currenNodes)
                    {
                        if (node.Text == next || next.Length == 40)
                        {
                            node.Expand();
                            GitTree.SelectedNode = node;
                            currenNodes = node.Nodes;
                            FileText.ScrollPos = scrollPos;
                        }
                    }
                }
            }
        }

        private void FillDiff()
        {
            if (tabControl1.SelectedTab == Commit)
            {
                DiffFiles.DataSource = null;
                DiffFiles.DisplayMember = "FileNameB"; 
                
                if (RevisionGrid.GetRevisions().Count == 0)
                    return;

                if (RevisionGrid.GetRevisions().Count == 2)
                {
                    DiffFiles.DataSource = GitCommands.GitCommands.GetDiffFiles(((GitRevision)RevisionGrid.GetRevisions()[0]).Guid, ((GitRevision)RevisionGrid.GetRevisions()[1]).Guid);
                }
                else
                {
                    GitRevision revision = RevisionGrid.GetRevisions()[0];

                    if (revision.ParentGuids.Count > 0)
                        DiffFiles.DataSource = GitCommands.GitCommands.GetDiffFiles(revision.Guid, revision.ParentGuids[0]);
                    else
                        DiffFiles.DataSource = null;
                }
            }
        }


        private void FillCommitInfo()
        {
            if (tabControl1.SelectedTab == CommitInfo)
            {
                if (RevisionGrid.GetRevisions().Count == 0)
                    return;

                GitRevision revision = RevisionGrid.GetRevisions()[0];

                RevisionInfo.Text = GitCommands.GitCommands.GetCommitInfo(revision.Guid);
            }
        }

        protected void LoadInTreeSingle(IGitItem item, TreeNodeCollection node)
        {
            List<IGitItem> list = new List<IGitItem>();
            list.Add(item);
            LoadInTree(list, node);
            if (node.Count > 0)
                node[0].Expand();
        }

        private ContextMenu treeContextMenu;
        private ContextMenu GetTreeContextMenu()
        {
            if (treeContextMenu == null)
            {
                treeContextMenu = new ContextMenu();
                treeContextMenu.MenuItems.Add(new MenuItem("Save as", new EventHandler(saveAsOnClick)));
                treeContextMenu.MenuItems.Add(new MenuItem("Open", new EventHandler(OpenOnClick)));
                treeContextMenu.MenuItems.Add(new MenuItem("Open With", new EventHandler(OpenWithOnClick)));
                treeContextMenu.MenuItems.Add(new MenuItem("File History", new EventHandler(FileHistoryOnClick)));

            }
            return treeContextMenu;
        }

        public void FileHistoryOnClick(object sender, EventArgs e)
        {
            object item = GitTree.SelectedNode.Tag;

            if (item is GitItem)
                if (((GitItem)item).ItemType == "blob")
                {
                    GitUICommands.Instance.StartFileHistoryDialog(((GitItem)item).FileName);
                }
        }

        public void saveAsOnClick(object sender, EventArgs e)
        {
            object item = GitTree.SelectedNode.Tag;

            if (item is GitItem)
            if (((GitItem)item).ItemType == "blob")
            {
                SaveFileDialog fileDialog = new SaveFileDialog();
                fileDialog.FileName = Settings.WorkingDir + ((GitItem)item).FileName;
                fileDialog.AddExtension = true;
                fileDialog.DefaultExt = GitCommands.GitCommands.GetFileExtension(fileDialog.FileName);
                fileDialog.Filter = "Current format (*." + GitCommands.GitCommands.GetFileExtension(fileDialog.FileName) + ")|*." + GitCommands.GitCommands.GetFileExtension(fileDialog.FileName) + "|All files (*.*)|*.*";
                //GitCommands.GitCommands.GetFileExtension(fileDialog.FileName);

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    GitCommands.GitCommands.RunCmd(Settings.GitDir + "git.cmd", "cat-file blob \"" + ((GitItem)item).Guid + "\" > \"" + fileDialog.FileName + "\"");
                }
            }
        }

        public void OpenWithOnClick(object sender, EventArgs e)
        {
            object item = GitTree.SelectedNode.Tag;

            if (item is GitItem)
                if (((GitItem)item).ItemType == "blob")
                {
                    string fileName = ((GitItem)item).FileName;
                    if (fileName.Contains("\\") && fileName.LastIndexOf("\\") < fileName.Length)
                        fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);

                    fileName = Path.GetTempPath() + fileName;
                    GitCommands.GitCommands.RunCmd(Settings.GitDir + "git.cmd", "cat-file blob \"" + ((GitItem)item).Guid + "\" > \"" + fileName + "\"");
                    OpenWith.OpenAs(fileName);
                }            
        }

        public void OpenOnClick(object sender, EventArgs e)
        {
            object item = GitTree.SelectedNode.Tag;

            if (item is GitItem)
                if (((GitItem)item).ItemType == "blob")
                {
                    string fileName = ((GitItem)item).FileName;
                    if (fileName.Contains("\\") && fileName.LastIndexOf("\\") < fileName.Length)
                        fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);

                    fileName = Path.GetTempPath() + fileName;
                    GitCommands.GitCommands.RunCmd(Settings.GitDir + "git.cmd", "cat-file blob \"" + ((GitItem)item).Guid + "\" > \"" + fileName + "\"");
                    System.Diagnostics.Process.Start(fileName);
                }
        }

        protected void LoadInTree(List<IGitItem> items, TreeNodeCollection node)
        {
            foreach (IGitItem item in items)
            {
                TreeNode subNode = node.Add(item.Name);
                subNode.Tag = item;
                
                if (item is GitItem)
                {
                    if (((GitItem)item).ItemType == "tree")
                        subNode.Nodes.Add(new TreeNode());
                    if (((GitItem)item).ItemType == "commit")
                        subNode.Text = item.Name + " (Submodule)";
                    if (((GitItem)item).ItemType == "blob")
                        subNode.ContextMenu = GetTreeContextMenu();
                }
                else
                {
                    subNode.Nodes.Add(new TreeNode());
                }
            }
        }

        private void GitTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (!e.Node.IsExpanded)
            {
                IGitItem item = (IGitItem)e.Node.Tag;

                e.Node.Nodes.Clear();
                //item.SubItems = GitCommands.GitCommands.GetTree(item.Guid);
                LoadInTree(item.SubItems, e.Node.Nodes);
            }
        }

        private void RevisionGrid_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                FillFileTree();
                FillDiff();
                FillCommitInfo();
            }
            catch
            { 
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Open open = new Open();
            open.ShowDialog();
            indexWatcher.Clear();
            RevisionGrid.ForceRefreshRevisions();
            InternalInitialize(false);
            indexWatcher.Reset();

        }

        private void checkoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCheckoutRevisionDialog())
                Initialize();
        }

        private void FileText_TextChanged(object sender, EventArgs e)
        {
        }

        private void FileChanges_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void GitTree_DoubleClick(object sender, EventArgs e)
        {
            if (GitTree.SelectedNode == null || !(GitTree.SelectedNode.Tag is IGitItem)) return;

            IGitItem item = (IGitItem)GitTree.SelectedNode.Tag;
            if (item is GitItem)
                if (((GitItem)item).ItemType == "blob")
                {
                    if (GitUICommands.Instance.StartFileHistoryDialog(((GitItem)item).FileName))
                        Initialize();

                }

        }

        private void viewDiffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCompareRevisionsDialog())
                Initialize();
        }

        private void addFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartAddFilesDialog())
                Initialize();
        }

        private void branchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCreateBranchDialog())
                Initialize();
        }

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCloneDialog())
                Initialize();
        }

        private void commitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCommitDialog())
                Initialize();
        }

        private void initNewRepositoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartInitializeDialog())
                Initialize();
        }

        private void pushToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartPushDialog());
                Initialize();
        }

        private void pullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartPullDialog());
                Initialize();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RevisionGrid.ForceRefreshRevisions();
            InternalInitialize(false);
            SetIndexClean();
            indexWatcher.Reset();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox a = new AboutBox();
            a.ShowDialog();
        }

        private void patchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartViewPatchDialog())
                Initialize();
        }


        private void applyPatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartApplyPatchDialog())
                Initialize();
        }

        private void gitBashToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunBash();

        }

        private void gitGUIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunGui();
        }

        private void formatPatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartFormatPatchDialog())
                Initialize();
        }

        private void gitcommandLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new GitLogForm().ShowDialog();
        }

        private void RevisionGrid_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void RevisionGrid_DoubleClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartCompareRevisionsDialog();
        }

        private void checkoutBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCheckoutBranchDialog())
                Initialize();
        }

        private void stashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartStashDialog())
                Initialize();
        }

        private void runMergetoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartResolveConflictsDialog())
                Initialize();
        }

        void warning_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartResolveConflictsDialog())
                Initialize();
        }


        private void WarningText_Click(object sender, EventArgs e)
        {

        }

        private void Workingdir_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(sender, e);
        }

        private void CurrentBranch_Click(object sender, EventArgs e)
        {
            checkoutBranchToolStripMenuItem_Click(sender, e);
        }

        private void deleteBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartDeleteBranchDialog())
                Initialize();
        }

        private void splitContainer2_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cherryPickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCherryPickDialog())
                Initialize();
        }

        private void mergeBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartMergeBranchDialog()) 
                Initialize();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            commitToolStripMenuItem_Click(sender, e);
        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void Workingdir_Click_1(object sender, EventArgs e)
        {
            openToolStripMenuItem_Click(sender, e);
        }

        private void CurrentBranch_Click_1(object sender, EventArgs e)
        {
            checkoutBranchToolStripMenuItem_Click(sender, e);
        }

        private void AddFiles_Click(object sender, EventArgs e)
        {
            addFilesToolStripMenuItem_Click(sender, e);
        }

        private void CreateBranch_Click(object sender, EventArgs e)
        {
            branchToolStripMenuItem_Click(sender, e);
        }

        private void GitBash_Click(object sender, EventArgs e)
        {
            gitBashToolStripMenuItem_Click_1(sender, e);
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            settingsToolStripMenuItem2_Click(sender, e);
        }

        private void tagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCreateTagDialog())
                Initialize();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            refreshToolStripMenuItem_Click(sender, e);
        }

        private void commitcountPerUserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormCommitCount().ShowDialog();
        }

        private void kGitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunGitK();
        }

        private void donateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormDonate().ShowDialog();
        }

        private void deleteTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartDeleteTagDialog())
                Initialize();
        }

        private void FormBrowse_FormClosing(object sender, FormClosingEventArgs e)
        {
			// Save current window position
        	SaveWindowsPositionAndState();
        }

		/// <summary>
		/// Save the current form position and state to user settings.
		/// </summary>
		private void SaveWindowsPositionAndState()
		{
			switch (WindowState)
			{
				case FormWindowState.Normal:
				case FormWindowState.Maximized:
				{
					GitUI.Properties.Settings.Default.WindowState = this.WindowState;
					break;
				}

				default:
				{
					GitUI.Properties.Settings.Default.WindowState = FormWindowState.Normal;
					break;					
				}
			}

			this.Visible = false;
			this.WindowState = FormWindowState.Normal;

			GitUI.Properties.Settings.Default.WindowPosition = this.DesktopBounds;
			GitUI.Properties.Settings.Default.Save();
		}

        private void editgitignoreToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartEditGitIgnoreDialog())
                Initialize();
        }

        private void settingsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartSettingsDialog())
                Initialize();
        }

        private void archiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartArchiveDialog())
                Initialize();
        }

        private void editmailmapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartMailMapDialog())
                Initialize();
        }

        private void compressGitDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormProcess("gc");
        }

        private void verifyGitDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartVerifyDatabaseDialog())
                Initialize();
        }

        private void removeDanglingObjecsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormProcess("prune");
        }

        private void manageRemoteRepositoriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartRemotesDialog())
                Initialize();
        }

        private void rebaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartRebaseDialog())
                Initialize();
        }

        private void startAuthenticationAgentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands.Run(GitCommands.Settings.Pageant, "");
        }

        private void generateOrImportKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {   
            GitCommands.GitCommands.Run(GitCommands.Settings.Puttygen, "");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCloneDialog())
                Initialize();
        }

        private void ToolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripLabel2_Click(object sender, EventArgs e)
        {
            if (RevisionGrid.Filter != RevisionGrid.FormatQuickFilter(toolStripTextBoxFilter.Text))
            {
                RevisionGrid.Filter = RevisionGrid.FormatQuickFilter(toolStripTextBoxFilter.Text);
                RevisionGrid.ForceRefreshRevisions();
            }
        }

        private void toolStripTextBoxFilter_Leave(object sender, EventArgs e)
        {
            toolStripLabel2_Click(sender, e);
        }

        private void FormBrowse_Shown(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(GitCommands.Settings.WorkingDir))
            {
                //openToolStripMenuItem_Click(sender, e);
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillFileTree();
            FillDiff();
            FillCommitInfo();
        }

        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem is string)
            {
                if (RevisionGrid.GetRevisions().Count == 0)
                    return;

                Patch selectedPatch = null;

                if (RevisionGrid.GetRevisions().Count == 2)
                {
                    selectedPatch = GitCommands.GitCommands.GetSingleDiff(((GitRevision)RevisionGrid.GetRevisions()[0]).Guid, ((GitRevision)RevisionGrid.GetRevisions()[1]).Guid, (string)DiffFiles.SelectedItem);
                }
                else
                {
                    GitRevision revision = RevisionGrid.GetRevisions()[0];
                    selectedPatch = GitCommands.GitCommands.GetSingleDiff(revision.Guid, revision.ParentGuids[0], (string)DiffFiles.SelectedItem);
                }

                if (selectedPatch != null)
                {
                    DiffText.ViewPatch(selectedPatch.Text);
                }
                else
                {
                    DiffText.ViewText("", "");
                }
            }
        }

        private void changelogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormChangeLog1().ShowDialog();
        }

        private void DiffFiles_DoubleClick(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem is string)
            {
                {
                    if (GitUICommands.Instance.StartFileHistoryDialog((string)DiffFiles.SelectedItem))
                        Initialize();
                }
            }
        }

        private void toolStripButtonPull_Click(object sender, EventArgs e)
        {
            pullToolStripMenuItem_Click(sender, e);
        }

        private void toolStripButtonPush_Click(object sender, EventArgs e)
        {
            pushToolStripMenuItem_Click(sender, e);
        }


        private void Open_Click(object sender, EventArgs e)
        {
            Open open = new Open();
            open.ShowDialog();
            indexWatcher.Clear();
            RevisionGrid.ForceRefreshRevisions();
            InternalInitialize(false);
            indexWatcher.Reset();

        }

        private void Clone_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCloneDialog())
                Initialize();
        }
        
        private void Init_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartInitializeDialog(GitCommands.Settings.WorkingDir);

            indexWatcher.Clear();
            RevisionGrid.ForceRefreshRevisions();
            InternalInitialize(false);
            indexWatcher.Reset();
        }

        private void RevisionInfo_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.EnableRaisingEvents = false;
                proc.StartInfo.FileName = e.LinkText;

                proc.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void toolStripTextBoxFilter_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                toolStripLabel2_Click(null, null);
            }
        }

        private void NoGit_Paint(object sender, PaintEventArgs e)
        {

        }

        private void manageSubmodulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartSubmodulesDialog())
                Initialize();
        }

        private void updateAllSubmodulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartUpdateSubmodulesDialog())
                Initialize();
        }

        private void updateAllSubmodulesRecursiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            if (GitUICommands.Instance.StartUpdateSubmodulesRecursiveDialog())
                Initialize();


        }

        private void initializeAllSubmodulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormProcess process = new FormProcess(GitCommands.GitCommands.SubmoduleInitCmd(""));
            Initialize();
        }

        private void initializeAllSubmodulesRecursiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            FormProcess process = new FormProcess(GitCommands.GitCommands.SubmoduleInitCmd(""));
            InitSubmodulesRecursive();
            Initialize();
        }

        private static void InitSubmodulesRecursive()
        {
            string oldworkingdir = Settings.WorkingDir;

            foreach (GitSubmodule submodule in (new GitCommands.GitCommands()).GetSubmodules())
            {
                if (!string.IsNullOrEmpty(submodule.LocalPath))
                {
                    Settings.WorkingDir = oldworkingdir + submodule.LocalPath;

                    if (Settings.WorkingDir != oldworkingdir && File.Exists(GitCommands.Settings.WorkingDir + ".gitmodules"))
                    {
                        FormProcess process = new FormProcess(GitCommands.GitCommands.SubmoduleInitCmd(""));

                        InitSubmodulesRecursive();
                    }

                    Settings.WorkingDir = oldworkingdir;
                }
            }

            Settings.WorkingDir = oldworkingdir;
        }

        private void syncronizeAllSubmodulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormProcess process = new FormProcess(GitCommands.GitCommands.SubmoduleSyncCmd(""));
            Initialize();
        }

        private void synchronizeAlSubmodulesRecursiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            FormProcess process = new FormProcess(GitCommands.GitCommands.SubmoduleSyncCmd(""));
            SyncSubmodulesRecursive();
            Initialize();
        }

        private static void SyncSubmodulesRecursive()
        {
            string oldworkingdir = Settings.WorkingDir;

            foreach (GitSubmodule submodule in (new GitCommands.GitCommands()).GetSubmodules())
            {
                if (!string.IsNullOrEmpty(submodule.LocalPath))
                {
                    Settings.WorkingDir = oldworkingdir + submodule.LocalPath;

                    if (Settings.WorkingDir != oldworkingdir && File.Exists(GitCommands.Settings.WorkingDir + ".gitmodules"))
                    {
                        FormProcess process = new FormProcess(GitCommands.GitCommands.SubmoduleSyncCmd(""));

                        SyncSubmodulesRecursive();
                    }

                    Settings.WorkingDir = oldworkingdir;
                }
            }

            Settings.WorkingDir = oldworkingdir;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            
        }

        private void toolStripSplitStash_ButtonClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartStashDialog())
                Initialize();
        }

        private void stashChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormProcess("stash save");
            Initialize();
        }

        private void stashPopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormProcess("stash pop");
            Initialize();
        }

        private void viewStashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartStashDialog())
                Initialize();
        }

        private void openSubmoduleToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            openSubmoduleToolStripMenuItem.DropDownItems.Clear();

            IList<IGitSubmodule> submodules = (new GitCommands.GitCommands()).GetSubmodules();

            foreach (IGitSubmodule submodule in submodules)
            {
                ToolStripButton submenu = new ToolStripButton(submodule.Name);
                submenu.Click += submenu_Click;
                openSubmoduleToolStripMenuItem.DropDownItems.Add(submenu);
            }

            if (openSubmoduleToolStripMenuItem.DropDownItems.Count == 0)
                openSubmoduleToolStripMenuItem.DropDownItems.Add("No submodules");
        }

        private void submenu_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;

            if (button == null)
                return;


            GitCommands.Settings.WorkingDir += GitCommands.GitCommands.GetSubmoduleLocalPath(button.Text);
            InternalInitialize(true);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void openSubmoduleToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            recentToolStripMenuItem.DropDownItems.Clear();

            foreach (string historyItem in RepositoryHistory.MostRecentRepositories)
            {
                if (!string.IsNullOrEmpty(historyItem))
                {
                    ToolStripButton historyItemMenu = new ToolStripButton(historyItem);
                    historyItemMenu.Click += new EventHandler(historyItemMenu_Click);
                    recentToolStripMenuItem.DropDownItems.Add(historyItemMenu);
                }
            }
        }

        void historyItemMenu_Click(object sender, EventArgs e)
        {
            ToolStripButton button = sender as ToolStripButton;

            if (button == null)
                return;

            GitCommands.Settings.WorkingDir = button.Text;
            InternalInitialize(true);
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartPluginSettingsDialog();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.WorkingDir = "";

            indexWatcher.Clear();
            RevisionGrid.ForceRefreshRevisions();
            InternalInitialize(false);
            indexWatcher.Reset();
        }

        private void Donate_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=WAL2SSDV8ND54&lc=US&item_name=GitExtensions&no_note=1&no_shipping=1&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted");
        }

        public override void cancelButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.WorkingDir))
            {
                Close();
            } else
            {
                Settings.WorkingDir = "";

                indexWatcher.Clear();
                RevisionGrid.ForceRefreshRevisions();
                InternalInitialize(false);
                indexWatcher.Reset();
            }
        }

        private void GitTree_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                GitTree.SelectedNode = GitTree.GetNodeAt(e.X, e.Y);
            }
        }

        private void userManualToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Settings.GetInstallDir() + "\\GitExtensionsUserManual.pdf");
        }

    }
}
