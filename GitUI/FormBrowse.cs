using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using GitUI.Plugin;
using GitUI.Statistics;
using GitUIPluginInterfaces;
using PatchApply;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormBrowse : GitExtensionsForm
    {
        private readonly IndexWatcher _indexWatcher = new IndexWatcher();
        private readonly TranslationString _menuFileHistory = new TranslationString("File history");
        private readonly TranslationString _menuOpen = new TranslationString("Open");
        private readonly TranslationString _menuOpenWith = new TranslationString("Open with");
        private readonly TranslationString _menuSaveAs = new TranslationString("Save as");

        private Dashboard _dashboard;
        private ToolStripItem _rebase;
        private ContextMenu _treeContextMenu;
        private ToolStripItem _warning;

        public FormBrowse(string filter)
        {
            InitializeComponent();
            Translate();
            RevisionGrid.SelectionChanged += RevisionGridSelectionChanged;
            DiffText.ExtraDiffArgumentsChanged += DiffTextExtraDiffArgumentsChanged;
            SetFilter(filter);
        }

        private void ShowDashboard()
        {
            if (_dashboard == null)
            {
                _dashboard = new Dashboard();
                _dashboard.WorkingDirChanged += DashboardWorkingDirChanged;
                splitContainer2.Panel2.Controls.Add(_dashboard);
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
            _indexWatcher.Clear();
            RevisionGrid.ForceRefreshRevisions();
            InternalInitialize(false);
            _indexWatcher.Reset();
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
            RestorePosition("browse");

            Cursor.Current = Cursors.WaitCursor;
            InternalInitialize(false);
            RevisionGrid.Focus();
            RevisionGrid.ChangedCurrentBranch += RevisionGridChangedCurrentBranch;
            _indexWatcher.Reset();

            LoadPluginsInPluginMenu();
        }

        private void LoadPluginsInPluginMenu()
        {
            foreach (var plugin in LoadedPlugins.Plugins)
            {
                var item = new ToolStripMenuItem {Text = plugin.Description, Tag = plugin};
                item.Click += ItemClick;
                pluginsToolStripMenuItem.DropDownItems.Add(item);
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

        private void RevisionGridChangedCurrentBranch(object sender, EventArgs e)
        {
            Initialize();
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

            var validWorkingDir = Settings.ValidWorkingDir();


            _CurrentBranch.Text = validWorkingDir ? GitCommands.GitCommands.GetSelectedBranch() : "";

            if (validWorkingDir)
                HideDashboard();
            else
                ShowDashboard();

            tabControl1.Visible = validWorkingDir;
            commandsToolStripMenuItem.Enabled = validWorkingDir;
            manageRemoteRepositoriesToolStripMenuItem1.Enabled = validWorkingDir;
            _CurrentBranch.Enabled = validWorkingDir;
            toolStripButton1.Enabled = validWorkingDir;
            toolStripButtonPull.Enabled = validWorkingDir;
            toolStripButtonPush.Enabled = validWorkingDir;
            submodulesToolStripMenuItem.Enabled = validWorkingDir;
            gitMaintenanceToolStripMenuItem.Enabled = validWorkingDir;
            editgitignoreToolStripMenuItem1.Enabled = validWorkingDir;
            editmailmapToolStripMenuItem.Enabled = validWorkingDir;
            toolStripSplitStash.Enabled = validWorkingDir;
            commitcountPerUserToolStripMenuItem.Enabled = validWorkingDir;

            if (hard)
                ShowRevisions();

            _Workingdir.Text = Settings.WorkingDir;
            Text = Settings.WorkingDir + " - Git Extensions";

            if (validWorkingDir &&
                (GitCommands.GitCommands.InTheMiddleOfRebase() || GitCommands.GitCommands.InTheMiddleOfPatch()))
            {
                if (_rebase == null)
                {
                    _rebase =
                        ToolStrip.Items.Add(GitCommands.GitCommands.InTheMiddleOfRebase()
                                                ? "You are in the middle of a rebase"
                                                : "You are in the middle of a patch apply");

                    _rebase.BackColor = Color.Salmon;
                    _rebase.Click += RebaseClick;
                }
            }
            else
            {
                if (_rebase != null)
                {
                    _rebase.Click -= WarningClick;
                    ToolStrip.Items.Remove(_rebase);
                    _rebase = null;
                }
            }

            if (validWorkingDir && GitCommands.GitCommands.InTheMiddleOfConflictedMerge() &&
                !Directory.Exists(Settings.WorkingDir + ".git\\rebase-apply\\"))
            {
                if (_warning == null)
                {
                    _warning = ToolStrip.Items.Add("There are unresolved merge conflicts!");
                    _warning.BackColor = Color.Salmon;
                    _warning.Click += WarningClick;
                }
            }
            else
            {
                if (_warning != null)
                {
                    _warning.Click -= WarningClick;
                    ToolStrip.Items.Remove(_warning);
                    _warning = null;
                }
            }
        }

        private void RebaseClick(object sender, EventArgs e)
        {
            if (GitCommands.GitCommands.InTheMiddleOfRebase())
                GitUICommands.Instance.StartRebaseDialog(null);
            else
                GitUICommands.Instance.StartApplyPatchDialog();
            Initialize();
        }


        private void ShowRevisions()
        {
            if (_indexWatcher.IndexChanged)
            {
                RevisionGrid.RefreshRevisions();
                FillFileTree();
                FillDiff();
                FillCommitInfo();
            }
            _indexWatcher.Reset();
        }

        private void FillFileTree()
        {
            if (tabControl1.SelectedTab != Tree)
                return;

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
            GitTree.Sort();


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

        private void FillDiff()
        {
            if (tabControl1.SelectedTab != Diff)
                return;

            DiffFiles.GitItemStatusses = null;
            var revisions = RevisionGrid.GetRevisions();

            DiffText.SaveCurrentScrollPos();

            switch (revisions.Count)
            {
                case 2:
                    DiffFiles.GitItemStatusses =
                        GitCommands.GitCommands.GetDiffFiles(revisions[0].Guid, revisions[1].Guid);
                    break;
                case 0:
                    return;
                default:
                    var revision = revisions[0];

                    if (revision != null &&
                        revision.ParentGuids != null &&
                        revision.ParentGuids.Length > 0)
                    {
                        DiffFiles.GitItemStatusses =
                            GitCommands.GitCommands.GetDiffFiles(revision.Guid, revision.ParentGuids[0]);
                        DiffFiles.Revision = revision;
                    }
                    else
                        DiffFiles.GitItemStatusses = null;
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

        private ContextMenu GetTreeContextMenu()
        {
            if (_treeContextMenu == null)
            {
                _treeContextMenu = new ContextMenu();
                _treeContextMenu.MenuItems.Add(new MenuItem(_menuSaveAs.Text, SaveAsOnClick));
                _treeContextMenu.MenuItems.Add(new MenuItem(_menuOpen.Text, OpenOnClick));
                _treeContextMenu.MenuItems.Add(new MenuItem(_menuOpenWith.Text, OpenWithOnClick));
                _treeContextMenu.MenuItems.Add(new MenuItem(_menuFileHistory.Text, FileHistoryOnClick));
            }
            return _treeContextMenu;
        }

        public void FileHistoryOnClick(object sender, EventArgs e)
        {
            var item = GitTree.SelectedNode.Tag as GitItem;

            if (item == null)
                return;

            if (item.ItemType == "blob")
                GitUICommands.Instance.StartFileHistoryDialog(item.FileName);
        }

        public void OpenWithOnClick(object sender, EventArgs e)
        {
            var item = GitTree.SelectedNode.Tag;

            if (item is GitItem)
                if (((GitItem) item).ItemType == "blob")
                {
                    var fileName = ((GitItem) item).FileName;
                    if (fileName.Contains("\\") && fileName.LastIndexOf("\\") < fileName.Length)
                        fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                    if (fileName.Contains("/") && fileName.LastIndexOf("/") < fileName.Length)
                        fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);

                    fileName = Path.GetTempPath() + fileName;
                    GitCommands.GitCommands.RunCmd(Settings.GitCommand,
                                                   "cat-file blob \"" + ((GitItem) item).Guid + "\" > \"" + fileName +
                                                   "\"");
                    OpenWith.OpenAs(fileName);
                }
        }

        public void OpenOnClick(object sender, EventArgs e)
        {
            var item = GitTree.SelectedNode.Tag;

            if (item is GitItem)
                if (((GitItem) item).ItemType == "blob")
                {
                    var fileName = ((GitItem) item).FileName;
                    if (fileName.Contains("\\") && fileName.LastIndexOf("\\") < fileName.Length)
                        fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                    if (fileName.Contains("/") && fileName.LastIndexOf("/") < fileName.Length)
                        fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);

                    fileName = Path.GetTempPath() + fileName;
                    GitCommands.GitCommands.RunCmd(Settings.GitCommand,
                                                   "cat-file blob \"" + ((GitItem) item).Guid + "\" > \"" + fileName +
                                                   "\"");
                    Process.Start(fileName);
                }
        }

        protected void LoadInTree(List<IGitItem> items, TreeNodeCollection node)
        {
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
                        subNode.Nodes.Add(new TreeNode());
                    if (gitItem.ItemType == "commit")
                        subNode.Text = item.Name + " (Submodule)";
                    if (gitItem.ItemType == "blob")
                        subNode.ContextMenu = GetTreeContextMenu();
                }
            }
        }

        private void RevisionGridSelectionChanged(object sender, EventArgs e)
        {
            try
            {
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
            new Open().ShowDialog();

            _indexWatcher.Clear();
            RevisionGrid.ForceRefreshRevisions();
            InternalInitialize(false);
            _indexWatcher.Reset();
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
            RevisionGrid.ForceRefreshRevisions();
            InternalInitialize(false);
            _indexWatcher.Reset();

            if (_dashboard != null)
                _dashboard.Refresh();
        }

        private static void AboutToolStripMenuItemClick(object sender, EventArgs e)
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

        private static void GitBashToolStripMenuItemClick1(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunBash();
        }

        private static void GitGuiToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunGui();
        }

        private void FormatPatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartFormatPatchDialog())
                Initialize();
        }

        private static void GitcommandLogToolStripMenuItemClick(object sender, EventArgs e)
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

        private static void CommitcountPerUserToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormCommitCount().ShowDialog();
        }

        private static void KGitToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunGitK();
        }

        private static void DonateToolStripMenuItemClick(object sender, EventArgs e)
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
            SavePosition("browse");
        }

        private void EditGitignoreToolStripMenuItem1Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartEditGitIgnoreDialog())
                Initialize();
        }

        private void SettingsToolStripMenuItem2Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartSettingsDialog())
                Initialize();

            Translate();
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

        private static void CompressGitDatabaseToolStripMenuItemClick(object sender, EventArgs e)
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

        private static void StartAuthenticationAgentToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitCommands.GitCommands.Run(Settings.Pageant, "");
        }

        private static void GenerateOrImportKeyToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitCommands.GitCommands.Run(Settings.Puttygen, "");
        }

        private void SetFilter(string filter)
        {
            if (string.IsNullOrEmpty(filter)) return;
            toolStripTextBoxFilter.Text = filter;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (RevisionGrid.Filter == RevisionGrid.FormatQuickFilter(toolStripTextBoxFilter.Text)) return;
            RevisionGrid.Filter = RevisionGrid.FormatQuickFilter(toolStripTextBoxFilter.Text);
            RevisionGrid.ForceRefreshRevisions();
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
            ShowSelectedFileDiff();
        }

        private void ShowSelectedFileDiff()
        {
            if (DiffFiles.SelectedItem == null)
                return;

            var selectedItem = (DiffFiles.SelectedItem).Name;
            var revisions = RevisionGrid.GetRevisions();

            if (revisions.Count == 0)
                return;

            DiffText.ViewPatch(() =>
                                   {
                                       var selectedPatch = GetSelectedPatch(revisions, selectedItem);

                                       return selectedPatch == null ? String.Empty : selectedPatch.Text;
                                   });
        }

        private Patch GetSelectedPatch(IList<GitRevision> revisions, string fileName)
        {
            var secondRevision = revisions.Count == 2 ? revisions[1].Guid : revisions[0].ParentGuids[0];

            return GitCommands.GitCommands.GetSingleDiff(revisions[0].Guid, secondRevision, fileName,
                                                         DiffText.GetExtraDiffArguments());
        }

        private static void ChangelogToolStripMenuItemClick(object sender, EventArgs e)
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
            if (e.KeyChar == (char) Keys.Enter)
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
        }

        private void InitializeAllSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
        {
            var process = new FormProcess(GitCommands.GitCommands.SubmoduleInitCmd(""));
            process.ShowDialog();
            Initialize();
        }

        private void InitializeAllSubmodulesRecursiveToolStripMenuItemClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var process = new FormProcess(GitCommands.GitCommands.SubmoduleInitCmd(""));
            process.ShowDialog();
            InitSubmodulesRecursive();
            Initialize();
        }

        private static void InitSubmodulesRecursive()
        {
            var oldworkingdir = Settings.WorkingDir;

            foreach (GitSubmodule submodule in (new GitCommands.GitCommands()).GetSubmodules())
            {
                if (string.IsNullOrEmpty(submodule.LocalPath))
                    continue;

                Settings.WorkingDir = oldworkingdir + submodule.LocalPath;

                if (Settings.WorkingDir != oldworkingdir && File.Exists(Settings.WorkingDir + ".gitmodules"))
                {
                    var process = new FormProcess(GitCommands.GitCommands.SubmoduleInitCmd(""));
                    process.ShowDialog();

                    InitSubmodulesRecursive();
                }

                Settings.WorkingDir = oldworkingdir;
            }

            Settings.WorkingDir = oldworkingdir;
        }

        private void SyncronizeAllSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
        {
            var process = new FormProcess(GitCommands.GitCommands.SubmoduleSyncCmd(""));
            process.ShowDialog();
            Initialize();
        }

        private void SynchronizeAllSubmodulesRecursiveToolStripMenuItemClick(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            var process = new FormProcess(GitCommands.GitCommands.SubmoduleSyncCmd(""));
            process.ShowDialog();
            SyncSubmodulesRecursive();
            Initialize();
        }

        private static void SyncSubmodulesRecursive()
        {
            var oldworkingdir = Settings.WorkingDir;

            foreach (GitSubmodule submodule in (new GitCommands.GitCommands()).GetSubmodules())
            {
                if (string.IsNullOrEmpty(submodule.LocalPath))
                    continue;

                Settings.WorkingDir = oldworkingdir + submodule.LocalPath;

                if (Settings.WorkingDir != oldworkingdir && File.Exists(Settings.WorkingDir + ".gitmodules"))
                {
                    var process = new FormProcess(GitCommands.GitCommands.SubmoduleSyncCmd(""));
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

            var submodules = (new GitCommands.GitCommands()).GetSubmodules();

            foreach (var submodule in submodules)
            {
                var submenu = new ToolStripButton(submodule.Name);
                submenu.Click += SubmoduleToolStripButtonClick;
                submenu.Width = 200;
                openSubmoduleToolStripMenuItem.DropDownItems.Add(submenu);
            }

            if (openSubmoduleToolStripMenuItem.DropDownItems.Count == 0)
                openSubmoduleToolStripMenuItem.DropDownItems.Add("No submodules");
        }

        private void RemoveSubmoduleButtons()
        {
            foreach (var item in openSubmoduleToolStripMenuItem.DropDownItems)
            {
                var toolStripButton = item as ToolStripButton;
                if(toolStripButton != null)
                    toolStripButton.Click -= SubmoduleToolStripButtonClick;
            }
            openSubmoduleToolStripMenuItem.DropDownItems.Clear();
        }

        private void SubmoduleToolStripButtonClick(object sender, EventArgs e)
        {
            var button = sender as ToolStripButton;

            if (button == null)
                return;

            Settings.WorkingDir += GitCommands.GitCommands.GetSubmoduleLocalPath(button.Text);

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

        private static void SettingsToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartPluginSettingsDialog();
        }

        private void CloseToolStripMenuItemClick(object sender, EventArgs e)
        {
            Settings.WorkingDir = "";

            _indexWatcher.Clear();
            RevisionGrid.ForceRefreshRevisions();
            InternalInitialize(false);
            _indexWatcher.Reset();
        }

        public override void CancelButtonClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.WorkingDir))
            {
                Close();
                return;
            }
            Settings.WorkingDir = "";

            _indexWatcher.Clear();
            RevisionGrid.ForceRefreshRevisions();
            InternalInitialize(false);
            _indexWatcher.Reset();
        }

        private void GitTreeMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                GitTree.SelectedNode = GitTree.GetNodeAt(e.X, e.Y);
        }

        private static void UserManualToolStripMenuItemClick(object sender, EventArgs e)
        {
            Process.Start(Settings.GetInstallDir() + "\\GitExtensionsUserManual.pdf");
        }

        private void DiffTextExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ShowSelectedFileDiff();
        }

        private static void CleanupToolStripMenuItemClick(object sender, EventArgs e)
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


            var output = GitCommands.GitCommands.OpenWithDifftool(selectedItem, revisions[0].Guid,
                                                                  revisions[0].ParentGuids[0]);
            if (!string.IsNullOrEmpty(output))
                MessageBox.Show(output);
        }

        private void WorkingdirDropDownOpening(object sender, EventArgs e)
        {
            _Workingdir.DropDownItems.Clear();
            foreach (var repository in Repositories.RepositoryHistory.Repositories)
            {
                var toolStripItem = _Workingdir.DropDownItems.Add(repository.Path);
                toolStripItem.Click += ToolStripItemClick;
            }
        }

        private void ToolStripItemClick(object sender, EventArgs e)
        {
            var toolStripItem = (ToolStripItem) sender;
            if (toolStripItem == null)
                return;

            Settings.WorkingDir = toolStripItem.Text;
            Repositories.RepositoryHistory.AddMostRecentRepository(Settings.WorkingDir);
            InternalInitialize(true);
        }

        private static void TranslateToolStripMenuItemClick(object sender, EventArgs e)
        {
            new FormTranslate().ShowDialog();
        }

        private void FileExplorerToolStripMenuItemClick(object sender, EventArgs e)
        {
            Process.Start(_Workingdir.Text);
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
            fileDialog.DefaultExt = GitCommands.GitCommands.GetFileExtension(fileDialog.FileName);
            fileDialog.Filter =
                "Current format (*." +
                GitCommands.GitCommands.GetFileExtension(fileDialog.FileName) + ")|*." +
                GitCommands.GitCommands.GetFileExtension(fileDialog.FileName) +
                "|All files (*.*)|*.*";

            if (fileDialog.ShowDialog() == DialogResult.OK)
                GitCommands.GitCommands.RunCmd(
                    Settings.GitCommand,
                    string.Format("cat-file blob \"{0}\" > \"{1}\"", item.Guid, fileDialog.FileName));
        }

        private void GitTreeBeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.IsExpanded)
                return;

            var item = (IGitItem) e.Node.Tag;

            e.Node.Nodes.Clear();
            LoadInTree(item.SubItems, e.Node.Nodes);
        }

        private void BranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCreateBranchDialog())
                Initialize();
        }

        private static void RevisionGridDoubleClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartCompareRevisionsDialog();
        }

        private static void GitBashClick(object sender, EventArgs e)
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
    }
}