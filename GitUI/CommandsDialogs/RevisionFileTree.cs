using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class RevisionFileTree : GitModuleControl
    {
        private readonly TranslationString _resetFileCaption = new TranslationString("Reset");
        private readonly TranslationString _resetFileText = new TranslationString("Are you sure you want to reset this file or directory?");
        private readonly TranslationString _saveFileFilterCurrentFormat = new TranslationString("Current format");
        private readonly TranslationString _saveFileFilterAllFiles = new TranslationString("All files");
        private readonly TranslationString _nodeNotFoundNextAvailableParentSelected = new TranslationString("Node not found. The next available parent node will be selected.");
        private readonly TranslationString _nodeNotFoundSelectionNotChanged = new TranslationString("Node not found. File tree selection was not changed.");

        //store strings to not keep references to nodes
        private readonly Stack<string> _lastSelectedNodes = new Stack<string>();
        private Rectangle _gitTreeDragBoxFromMouseDown;
        private GitRevision _revision;


        public RevisionFileTree()
        {
            InitializeComponent();
            Translate();

            tvGitTree.ImageList = new ImageList();
            tvGitTree.ImageList.Images.Add(Properties.Resources.New); //File
            tvGitTree.ImageList.Images.Add(Properties.Resources.Folder); //Folder
            tvGitTree.ImageList.Images.Add(Properties.Resources.IconFolderSubmodule); //Submodule

            GotFocus += (s, e) => tvGitTree.Focus();
        }


        public void ExpandToFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            var pathParts = filePath.Split('/');

            var currentNodes = tvGitTree.Nodes;
            TreeNode foundNode = null;
            bool isIncompleteMatch = false;
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
                    return false;
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
                    currentNodes = currentFoundNode.Nodes;
                }
            }

            if (foundNode != null)
            {
                if (isIncompleteMatch)
                {
                    MessageBox.Show(_nodeNotFoundNextAvailableParentSelected.Text);
                }

                tvGitTree.SelectedNode = foundNode;
                tvGitTree.SelectedNode.EnsureVisible();
            }
            else
            {
                MessageBox.Show(_nodeNotFoundSelectionNotChanged.Text);
            }
        }

        public void Init(SplitterManager splitterManager)
        {
            splitterManager.AddSplitter(FileTreeSplitContainer, "FileTreeSplitContainer");
        }

        public void InvokeFindFileDialog()
        {
            tvGitTree.Focus();
            findToolStripMenuItem_Click(null, null);
        }

        public void LoadRevision(GitRevision revision)
        {
            _revision = revision;

            try
            {
                tvGitTree.SuspendLayout();
                // Save state only when there is selected node
                if (tvGitTree.SelectedNode != null)
                {
                    var node = tvGitTree.SelectedNode;
                    FileText.SaveCurrentScrollPos();
                    _lastSelectedNodes.Clear();
                    while (node != null)
                    {
                        _lastSelectedNodes.Push(node.Text);
                        node = node.Parent;
                    }
                }

                // Refresh tree
                tvGitTree.Nodes.Clear();
                //restore selected file and scroll position when new selection is done
                if (_revision != null)
                {
                    LoadInTree(_revision.SubItems, tvGitTree.Nodes);
                    //GitTree.Sort();
                    TreeNode lastMatchedNode = null;
                    // Load state
                    var currenNodes = tvGitTree.Nodes;
                    TreeNode matchedNode = null;
                    while (_lastSelectedNodes.Count > 0 && currenNodes != null)
                    {
                        var next = _lastSelectedNodes.Pop();
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
                    tvGitTree.SelectedNode = lastMatchedNode;
                }
                if (tvGitTree.SelectedNode == null)
                {
                    FileText.ViewText("", "");
                }
            }
            finally
            {
                tvGitTree.ResumeLayout();
            }
        }

        public void ReloadHotkeys()
        {
            FileText.ReloadHotkeys();
        }


        private static TreeNode Find(TreeNodeCollection nodes, string label)
        {
            for (var i = 0; i < nodes.Count; i++)
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
            var candidates = Module.GetFullTree(_revision.TreeGuid);

            var nameAsLower = name.ToLower();

            return candidates.Where(fileName => fileName.ToLower().Contains(nameAsLower)).ToList();
        }

        private void LoadInTree(IEnumerable<IGitItem> items, TreeNodeCollection node)
        {
            var sortedItems = items.OrderBy(gi => gi, new GitFileTreeComparer());

            foreach (var item in sortedItems)
            {
                var subNode = node.Add(item.Name);
                subNode.Tag = item;

                var gitItem = item as GitItem;
                if (gitItem == null)
                {
                    subNode.Nodes.Add(new TreeNode());
                }
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
                        subNode.Text = $@"{item.Name} (Submodule)";
                    }
                }
            }
        }

        private void OnItemActivated()
        {
            var item = tvGitTree.SelectedNode?.Tag as GitItem;
            if (item == null)
                return;

            if (item.IsBlob)
            {
                UICommands.StartFileHistoryDialog(this, item.FileName, null);
            }
            else if (item.IsCommit)
            {
                SpawnCommitBrowser(item);
            }
        }

        private string SaveSelectedItemToTempFile()
        {
            var gitItem = tvGitTree.SelectedNode.Tag as GitItem;
            if (gitItem == null || !gitItem.IsBlob || string.IsNullOrWhiteSpace(gitItem.FileName))
            {
                return null;
            }

            var fileName = gitItem.FileName;
            if (fileName.Contains("\\") && fileName.LastIndexOf("\\", StringComparison.Ordinal) < fileName.Length)
                fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
            if (fileName.Contains("/") && fileName.LastIndexOf("/", StringComparison.Ordinal) < fileName.Length)
                fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);

            fileName = (Path.GetTempPath() + fileName).ToNativePath();
            Module.SaveBlobAs(fileName, gitItem.Guid);
            return fileName;
        }

        private void SpawnCommitBrowser(GitItem item)
        {
            var process = new Process();
            process.StartInfo.FileName = Application.ExecutablePath;
            process.StartInfo.Arguments = "browse";
            process.StartInfo.WorkingDirectory = Path.Combine(Module.WorkingDir, item.FileName.EnsureTrailingPathSeparator());
            process.Start();
        }


        private void GitTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var item = e.Node.Tag as GitItem;
            if (item == null)
                return;

            if (item.IsBlob)
                FileText.ViewGitItem(item.FileName, item.Guid);
            else if (item.IsCommit)
                FileText.ViewText(item.FileName, LocalizationHelpers.GetSubmoduleText(Module, item.FileName, item.Guid));
            else
                FileText.ViewText("", "");
        }

        private void GitTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.IsExpanded)
                return;

            var item = (IGitItem)e.Node.Tag;

            e.Node.Nodes.Clear();
            LoadInTree(item.SubItems, e.Node.Nodes);
        }

        private void GitTree_DoubleClick(object sender, EventArgs e)
        {
            OnItemActivated();
        }

        private void GitTree_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                if (tvGitTree.SelectedNode != null)
                {
                    OnItemActivated();
                    e.Handled = true;
                }
            }
        }

        private void GitTree_MouseDown(object sender, MouseEventArgs e)
        {
            //DRAG
            if (e.Button == MouseButtons.Left)
            {
                // Remember the point where the mouse down occurred.
                // The DragSize indicates the size that the mouse can move
                // before a drag event should be started.
                var dragSize = SystemInformation.DragSize;

                // Create a rectangle using the DragSize, with the mouse position being
                // at the center of the rectangle.
                _gitTreeDragBoxFromMouseDown = new Rectangle(new Point(e.X - dragSize.Width / 2, e.Y - dragSize.Height / 2), dragSize);
            }
            else if (e.Button == MouseButtons.Right)
            {
                tvGitTree.SelectedNode = tvGitTree.GetNodeAt(e.X, e.Y);
            }
        }

        private void GitTree_MouseMove(object sender, MouseEventArgs e)
        {
            var gitTree = (TreeView)sender;

            //DRAG
            // If the mouse moves outside the rectangle, start the drag.
            if (_gitTreeDragBoxFromMouseDown != Rectangle.Empty &&
                !_gitTreeDragBoxFromMouseDown.Contains(e.X, e.Y))
            {
                var fileList = new StringCollection();

                //foreach (GitItemStatus item in SelectedItems)
                if (gitTree.SelectedNode != null)
                {
                    var item = gitTree.SelectedNode.Tag as GitItem;
                    if (item != null)
                    {
                        var fileName = Path.Combine(Module.WorkingDir, item.FileName);

                        fileList.Add(fileName.ToNativePath());
                    }

                    var obj = new DataObject();
                    obj.SetFileDropList(fileList);

                    // Proceed with the drag and drop, passing in the list item.
                    DoDragDrop(obj, DragDropEffects.Copy);
                    _gitTreeDragBoxFromMouseDown = Rectangle.Empty;
                }
            }
        }


        private void blameMenuItem_Click(object sender, EventArgs e)
        {
            var item = tvGitTree.SelectedNode.Tag as GitItem;

            if (item == null)
                return;

            if (GitRevision.IsArtificial(_revision.Guid))
                UICommands.StartFileHistoryDialog(this, item.FileName, null, false, true);
            else
                UICommands.StartFileHistoryDialog(this, item.FileName, _revision, true, true);
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tvGitTree.CollapseAll();
        }

        private void copyFilenameToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gitItem = tvGitTree.SelectedNode.Tag as GitItem;
            if (gitItem == null)
                return;

            var fileName = Path.Combine(Module.WorkingDir, gitItem.FileName);
            Clipboard.SetText(fileName.ToNativePath());
        }

        private void fileHistoryItem_Click(object sender, EventArgs e)
        {
            var item = tvGitTree.SelectedNode.Tag as GitItem;
            if (item == null)
                return;

            if (GitRevision.IsArtificial(_revision.Guid))
                UICommands.StartFileHistoryDialog(this, item.FileName);
            else
                UICommands.StartFileHistoryDialog(this, item.FileName, _revision, false, false);
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string selectedItem;
            using (var searchWindow = new SearchWindow<string>(FindFileMatches) { Owner = FindForm() })
            {
                searchWindow.ShowDialog(this);
                selectedItem = searchWindow.SelectedItem;
            }
            if (string.IsNullOrEmpty(selectedItem))
            {
                return;
            }

            var items = selectedItem.Split('/');
            var nodes = tvGitTree.Nodes;

            for (var i = 0; i < items.Length - 1; i++)
            {
                var selectedNode = Find(nodes, items[i]);
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
                tvGitTree.SelectedNode = lastItem;
            }
        }

        private void editCheckedOutFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = tvGitTree.SelectedNode.Tag;

            var gitItem = item as GitItem;
            if (gitItem == null || !gitItem.IsBlob)
                return;

            var fileName = Path.Combine(Module.WorkingDir, gitItem.FileName);
            UICommands.StartFileEditorDialog(fileName);
        }

        private void expandAllStripMenuItem_Click(object sender, EventArgs e)
        {
            tvGitTree.ExpandAll();
        }

        private void fileTreeArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gitItem = (GitItem)tvGitTree.SelectedNode.Tag;
            UICommands.StartArchiveDialog(this, _revision, null, gitItem.FileName);
        }

        private void fileTreeCleanWorkingTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gitItem = (GitItem)tvGitTree.SelectedNode.Tag;
            UICommands.StartCleanupRepositoryDialog(this, gitItem.FileName + "/"); // the trailing / marks a directory
        }

        private void FileTreeContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var gitItem = tvGitTree.SelectedNode?.Tag as GitItem;
            var enableItems = gitItem != null && gitItem.IsBlob;

            if (gitItem != null && gitItem.IsCommit)
            {
                openSubmoduleMenuItem.Visible = true;
                if (!openSubmoduleMenuItem.Font.Bold)
                {
                    openSubmoduleMenuItem.Font = new Font(openSubmoduleMenuItem.Font, FontStyle.Bold);
                }
            }
            else
            {
                openSubmoduleMenuItem.Visible = false;
            }

            saveAsToolStripMenuItem.Visible = enableItems;
            openFileToolStripMenuItem.Visible = enableItems;
            openFileWithToolStripMenuItem.Visible = enableItems;
            openWithToolStripMenuItem.Visible = enableItems;
            copyFilenameToClipboardToolStripMenuItem.Visible = gitItem != null && FormBrowseUtil.IsFileOrDirectory(FormBrowseUtil.GetFullPathFromGitItem(Module, gitItem));
            editCheckedOutFileToolStripMenuItem.Visible = enableItems;
        }

        private void fileTreeOpenContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gitItem = tvGitTree.SelectedNode.Tag as GitItem;
            if (gitItem == null)
            {
                return;
            }

            var filePath = FormBrowseUtil.GetFullPathFromGitItem(Module, gitItem);
            FormBrowseUtil.ShowFileOrFolderInFileExplorer(filePath);
        }

        private void openFileWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileName = SaveSelectedItemToTempFile();
            if (fileName != null)
                OsShellUtil.OpenAs(fileName);
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void openSubmoduleMenuItem_Click(object sender, EventArgs e)
        {
            var item = tvGitTree.SelectedNode.Tag as GitItem;
            if (item != null && item.IsCommit)
            {
                SpawnCommitBrowser(item);
            }
        }

        private void openWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = tvGitTree.SelectedNode.Tag;

            var gitItem = item as GitItem;
            if (gitItem == null || !gitItem.IsBlob)
                return;

            var fileName = Path.Combine(Module.WorkingDir, gitItem.FileName);
            OsShellUtil.OpenAs(fileName.ToNativePath());
        }

        private void resetToThisRevisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == MessageBox.Show(_resetFileText.Text, _resetFileCaption.Text, MessageBoxButtons.OKCancel))
            {
                var item = tvGitTree.SelectedNode.Tag as GitItem;
                var files = new List<string> { item.FileName };
                Module.CheckoutFiles(files, _revision.Guid, false);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = tvGitTree.SelectedNode.Tag as GitItem;
            if (item == null || !item.IsBlob)
            {
                return;
            }

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
                var extension = GitCommandHelpers.GetFileExtension(fileDialog.FileName);

                fileDialog.Filter = $@"{_saveFileFilterCurrentFormat.Text}(*.{extension})|*.{extension }| {_saveFileFilterAllFiles.Text} (*.*)|*.*";
                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    Module.SaveBlobAs(fileDialog.FileName, item.Guid);
                }
            }
        }
    }
}
