﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUI.CommandsDialogs.BrowseDialog;
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
        private IRevisionFileTreeController _revisionFileTreeController;
        private GitRevision _revision;


        public RevisionFileTree()
        {
            InitializeComponent();
            Translate();
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

        public void InitSplitterManager(SplitterManager splitterManager)
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
            _revisionFileTreeController.ResetCache();

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
                    _revisionFileTreeController.LoadChildren(_revision, tvGitTree.Nodes, tvGitTree.ImageList.Images);
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


        protected override void OnRuntimeLoad(EventArgs e)
        {
            _revisionFileTreeController = new RevisionFileTreeController(() => Module.WorkingDir,
                                                                         new GitRevisionInfoProvider(() => Module),
                                                                         new FileAssociatedIconProvider());

            tvGitTree.ImageList = new ImageList(components)
            {
                ColorDepth = ColorDepth.Depth32Bit
            };
            tvGitTree.ImageList.Images.Add(Properties.Resources.New); //File
            tvGitTree.ImageList.Images.Add(Properties.Resources.Folder); //Folder
            tvGitTree.ImageList.Images.Add(Properties.Resources.IconFolderSubmodule); //Submodule

            GotFocus += (s, e1) => tvGitTree.Focus();

            base.OnRuntimeLoad(e);
        }


        private IList<string> FindFileMatches(string name)
        {
            var candidates = Module.GetFullTree(_revision.TreeGuid);

            var nameAsLower = name.ToLower();

            return candidates.Where(fileName => fileName.ToLower().Contains(nameAsLower)).ToList();
        }

        private void OnItemActivated()
        {
            var gitItem = tvGitTree.SelectedNode?.Tag as GitItem;
            if (gitItem == null)
                return;

            switch (gitItem.ObjectType)
            {
                case GitObjectType.Blob:
                    {
                        UICommands.StartFileHistoryDialog(this, gitItem.FileName, null);
                        break;
                    }
                case GitObjectType.Commit:
                    {
                        SpawnCommitBrowser(gitItem);
                        break;
                    }
            }
        }

        private string SaveSelectedItemToTempFile()
        {
            var gitItem = tvGitTree.SelectedNode?.Tag as GitItem;
            if (gitItem == null || gitItem.ObjectType != GitObjectType.Blob || string.IsNullOrWhiteSpace(gitItem.FileName))
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


        private void tvGitTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var gitItem = e.Node?.Tag as GitItem;
            if (gitItem == null)
            {
                return;
            }

            switch (gitItem.ObjectType)
            {
                case GitObjectType.Blob:
                    {
                        FileText.ViewGitItem(gitItem.FileName, gitItem.Guid);
                        break;
                    }
                case GitObjectType.Commit:
                    {
                        FileText.ViewText(gitItem.FileName, LocalizationHelpers.GetSubmoduleText(Module, gitItem.FileName, gitItem.Guid));
                        break;
                    }
                default:
                    {
                        FileText.ViewText("", "");
                        break;
                    }
            }
        }

        private void tvGitTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.IsExpanded)
            {
                return;
            }
            var gitItem = e.Node?.Tag as GitItem;
            if (gitItem == null)
            {
                return;
            }

            e.Node.Nodes.Clear();
            _revisionFileTreeController.LoadChildren(gitItem, e.Node.Nodes, tvGitTree.ImageList.Images);
        }

        private void tvGitTree_DoubleClick(object sender, EventArgs e)
        {
            OnItemActivated();
        }

        private void tvGitTree_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var gitItem = (e.Item as TreeNode)?.Tag as GitItem;
            if (gitItem == null)
            {
                return;
            }

            var fileList = new StringCollection();
            var fileName = Path.Combine(Module.WorkingDir, gitItem.FileName);

            fileList.Add(fileName.ToNativePath());

            var obj = new DataObject();
            obj.SetFileDropList(fileList);

            DoDragDrop(obj, DragDropEffects.Copy);
        }

        private void tvGitTree_KeyDown(object sender, KeyEventArgs e)
        {
            if (tvGitTree.SelectedNode == null || e.KeyCode != Keys.Enter)
            {
                return;
            }
            OnItemActivated();
            e.Handled = true;
        }


        private void blameMenuItem_Click(object sender, EventArgs e)
        {
            var gitItem = tvGitTree.SelectedNode?.Tag as GitItem;

            if (gitItem == null)
                return;

            if (GitRevision.IsArtificial(_revision.Guid))
                UICommands.StartFileHistoryDialog(this, gitItem.FileName, null, false, true);
            else
                UICommands.StartFileHistoryDialog(this, gitItem.FileName, _revision, true, true);
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tvGitTree.CollapseAll();
        }

        private void copyFilenameToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gitItem = tvGitTree.SelectedNode?.Tag as GitItem;
            if (gitItem == null)
                return;

            var fileName = Path.Combine(Module.WorkingDir, gitItem.FileName);
            Clipboard.SetText(fileName.ToNativePath());
        }

        private void fileHistoryItem_Click(object sender, EventArgs e)
        {
            var gitItem = tvGitTree.SelectedNode?.Tag as GitItem;
            if (gitItem == null)
                return;

            if (GitRevision.IsArtificial(_revision.Guid))
                UICommands.StartFileHistoryDialog(this, gitItem.FileName);
            else
                UICommands.StartFileHistoryDialog(this, gitItem.FileName, _revision, false, false);
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
                var selectedNode = _revisionFileTreeController.Find(nodes, items[i]);
                if (selectedNode == null)
                {
                    return; //Item does not exist in the tree
                }

                selectedNode.Expand();
                nodes = selectedNode.Nodes;
            }

            var lastItem = _revisionFileTreeController.Find(nodes, items[items.Length - 1]);
            if (lastItem != null)
            {
                tvGitTree.SelectedNode = lastItem;
            }
        }

        private void editCheckedOutFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gitItem = tvGitTree.SelectedNode?.Tag as GitItem;
            if (gitItem == null || gitItem.ObjectType != GitObjectType.Blob)
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
            var gitItem = tvGitTree.SelectedNode?.Tag as GitItem;
            if (gitItem == null) { return; }
            UICommands.StartArchiveDialog(this, _revision, null, gitItem.FileName);
        }

        private void fileTreeCleanWorkingTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gitItem = tvGitTree.SelectedNode?.Tag as GitItem;
            string filePath;
            if (gitItem != null)
            {
                filePath = gitItem.FileName + "/"; // the trailing / marks a directory
            }
            else
            {
                //No item selected is handled as the repo source
                filePath = Module.WorkingDir;
            }
            UICommands.StartCleanupRepositoryDialog(this, filePath);
        }

        private void FileTreeContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var gitItem = tvGitTree.SelectedNode?.Tag as GitItem;
            var enableItems = gitItem != null && gitItem.ObjectType == GitObjectType.Blob;

            if (gitItem != null && gitItem.ObjectType == GitObjectType.Commit)
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
            this.fileHistoryToolStripMenuItem.Enabled = gitItem != null;

            this.resetToThisRevisionToolStripMenuItem.Enabled =
            this.blameToolStripMenuItem1.Enabled =
            this.fileTreeArchiveToolStripMenuItem.Enabled =
              enableItems;
            
            saveAsToolStripMenuItem.Visible = enableItems;
            openFileToolStripMenuItem.Visible = enableItems;
            openFileWithToolStripMenuItem.Visible = enableItems;
            openWithToolStripMenuItem.Visible = enableItems;
            copyFilenameToClipboardToolStripMenuItem.Visible = gitItem != null && FormBrowseUtil.IsFileOrDirectory(FormBrowseUtil.GetFullPathFromGitItem(Module, gitItem));
            editCheckedOutFileToolStripMenuItem.Visible = enableItems;
            toolStripSeparator18.Visible = enableItems;
        }

        private void fileTreeOpenContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gitItem = tvGitTree.SelectedNode?.Tag as GitItem;
            string filePath = gitItem == null ? Module.WorkingDir : FormBrowseUtil.GetFullPathFromGitItem(Module, gitItem);
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
            var gitItem = tvGitTree.SelectedNode?.Tag as GitItem;
            if (gitItem != null && gitItem.ObjectType == GitObjectType.Commit)
            {
                SpawnCommitBrowser(gitItem);
            }
        }

        private void openWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gitItem = tvGitTree.SelectedNode?.Tag as GitItem;
            if (gitItem == null || gitItem.ObjectType != GitObjectType.Blob)
                return;

            var fileName = Path.Combine(Module.WorkingDir, gitItem.FileName);
            OsShellUtil.OpenAs(fileName.ToNativePath());
        }

        private void resetToThisRevisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gitItem = tvGitTree.SelectedNode?.Tag as GitItem;
            if (gitItem == null)
            {
                return;
            }

            if (DialogResult.OK == MessageBox.Show(_resetFileText.Text, _resetFileCaption.Text, MessageBoxButtons.OKCancel))
            {
                var files = new List<string> { gitItem.FileName };
                Module.CheckoutFiles(files, _revision.Guid, false);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gitItem = tvGitTree.SelectedNode?.Tag as GitItem;
            if (gitItem == null || gitItem.ObjectType != GitObjectType.Blob)
            {
                return;
            }

            var fullName = Path.Combine(Module.WorkingDir, gitItem.FileName);
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
                    Module.SaveBlobAs(fileDialog.FileName, gitItem.Guid);
                }
            }
        }
    }
}
