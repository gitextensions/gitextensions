using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Hotkey;
using GitUI.Properties;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class RevisionFileTreeControl : GitModuleControl
    {
        private readonly TranslationString _resetFileCaption = new TranslationString("Reset");
        private readonly TranslationString _resetFileText = new TranslationString("Are you sure you want to reset this file or directory?");
        private readonly TranslationString _saveFileFilterCurrentFormat = new TranslationString("Current format");
        private readonly TranslationString _saveFileFilterAllFiles = new TranslationString("All files");
        private readonly TranslationString _nodeNotFoundNextAvailableParentSelected = new TranslationString("Node not found. The next available parent node will be selected.");
        private readonly TranslationString _nodeNotFoundSelectionNotChanged = new TranslationString("Node not found. File tree selection was not changed.");

        private readonly TranslationString _assumeUnchangedMessage = new TranslationString(@"This feature should be used for performance purpose when it is costly for git to check the state of a big file.


Are you sure to assume this file won't change ?");
        private readonly TranslationString _assumeUnchangedCaption = new TranslationString("Assume this file won't change");
        private readonly TranslationString _assumeUnchangedFail = new TranslationString("Fail to assume unchanged the file '{0}'.");
        private readonly TranslationString _assumeUnchangedSuccess = new TranslationString("File successfully assumed unchanged.");

        private readonly TranslationString _stopTrackingMessage = new TranslationString(@"Are you sure you want to stop tracking the file
'{0}'?");
        private readonly TranslationString _stopTrackingCaption = new TranslationString("Stop tracking the file");
        private readonly TranslationString _stopTrackingFail = new TranslationString("Fail to stop tracking the file '{0}'.");
        private readonly TranslationString _stopTrackingSuccess = new TranslationString(@"File successfully untracked. Removal has been added to the staging area.

See the changes in the commit form.");

        private readonly TranslationString _success = new TranslationString("Success");
        private readonly TranslationString _error = new TranslationString("Error");

        // store strings to not keep references to nodes
        private readonly Stack<string> _lastSelectedNodes = new Stack<string>();
        private readonly IRevisionFileTreeController _revisionFileTreeController;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly IFindFilePredicateProvider _findFilePredicateProvider;
        private GitRevision _revision;

        public RevisionFileTreeControl()
        {
            InitializeComponent();
            InitializeComplete();
            HotkeysEnabled = true;
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
            _findFilePredicateProvider = new FindFilePredicateProvider();
            _revisionFileTreeController = new RevisionFileTreeController(() => Module.WorkingDir,
                                                                         new GitRevisionInfoProvider(() => Module),
                                                                         new FileAssociatedIconProvider());
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
                    if (a.Tag is GitItem treeGitItem)
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

                // if not the last path part...
                if (i < pathParts.Length - 1)
                {
                    // load more data
                    foundNode.Expand();
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
                    _lastSelectedNodes.Clear();
                    while (node != null)
                    {
                        _lastSelectedNodes.Push(node.Text);
                        node = node.Parent;
                    }
                }

                // Refresh tree
                tvGitTree.Nodes.Clear();

                // restore selected file and scroll position when new selection is done
                if (_revision != null && !_revision.IsArtificial && tvGitTree.ImageList != null)
                {
                    _revisionFileTreeController.LoadChildren(_revision, tvGitTree.Nodes, tvGitTree.ImageList.Images);
                    ////GitTree.Sort();
                    TreeNode lastMatchedNode = null;

                    // Load state
                    var currentNodes = tvGitTree.Nodes;
                    TreeNode matchedNode = null;
                    while (_lastSelectedNodes.Count > 0 && currentNodes != null)
                    {
                        var next = _lastSelectedNodes.Pop();
                        foreach (TreeNode node in currentNodes)
                        {
                            if (node.Text != next && next.Length != 40)
                            {
                                continue;
                            }

                            node.Expand();
                            matchedNode = node;
                            break;
                        }

                        if (matchedNode == null)
                        {
                            currentNodes = null;
                        }
                        else
                        {
                            lastMatchedNode = matchedNode;
                            currentNodes = matchedNode.Nodes;
                        }
                    }

                    tvGitTree.SelectedNode = lastMatchedNode;
                }

                if (tvGitTree.SelectedNode == null)
                {
                    ThreadHelper.JoinableTaskFactory.Run(() => FileText.ViewTextAsync("", ""));
                }
            }
            finally
            {
                tvGitTree.ResumeLayout();
            }
        }

        #region Hotkey commands

        public static readonly string HotkeySettingsName = "RevisionFileTree";

        public enum Command
        {
            ShowHistory = 0,
            Blame = 1,
            OpenWithDifftool = 2,
            OpenAsTempFile = 3,
            OpenAsTempFileWith = 4,
            EditFile = 5
        }

        public bool ExecuteCommand(Command cmd)
        {
            return ExecuteCommand((int)cmd);
        }

        protected override bool ExecuteCommand(int cmd)
        {
            switch ((Command)cmd)
            {
                case Command.ShowHistory: fileHistoryToolStripMenuItem.PerformClick(); break;
                case Command.Blame: blameToolStripMenuItem1.PerformClick(); break;
                case Command.OpenWithDifftool: openWithDifftoolToolStripMenuItem.PerformClick(); break;
                case Command.OpenAsTempFile: openFileToolStripMenuItem.PerformClick(); break;
                case Command.OpenAsTempFileWith: openFileWithToolStripMenuItem.PerformClick(); break;
                case Command.EditFile: editCheckedOutFileToolStripMenuItem.PerformClick(); break;
                default: return base.ExecuteCommand(cmd);
            }

            return true;
        }

        public void ReloadHotkeys()
        {
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            fileHistoryToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.ShowHistory);
            blameToolStripMenuItem1.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.Blame);
            openWithDifftoolToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenWithDifftool);
            openFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenAsTempFile);
            openFileWithToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenAsTempFileWith);
            editCheckedOutFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.EditFile);
            FileText.ReloadHotkeys();
        }

        private string GetShortcutKeyDisplayString(Command cmd)
        {
            return GetShortcutKeys((int)cmd).ToShortcutKeyDisplayString();
        }

        #endregion

        protected override void OnRuntimeLoad()
        {
            tvGitTree.ImageList = new ImageList(components)
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = DpiUtil.Scale(new Size(16, 16)), // Scale ImageSize and images scale automatically
                Images =
                {
                    Images.File, // File
                    Images.FolderClosed, // Folder
                    Images.FolderSubmodule // Submodule
                }
            };

            ReloadHotkeys();

            base.OnRuntimeLoad();
        }

        private IEnumerable<string> FindFileMatches(string name)
        {
            var candidates = Module.GetFullTree(_revision.TreeGuid.ToString());
            var predicate = _findFilePredicateProvider.Get(name, Module.WorkingDir);

            return candidates.Where(predicate);
        }

        private void OnItemActivated()
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem gitItem)
            {
                switch (gitItem.ObjectType)
                {
                    case GitObjectType.Blob:
                    {
                        UICommands.StartFileHistoryDialog(this, gitItem.FileName, _revision);
                        break;
                    }

                    case GitObjectType.Commit:
                    {
                        SpawnCommitBrowser(gitItem);
                        break;
                    }
                }
            }
        }

        [CanBeNull]
        private string SaveSelectedItemToTempFile()
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem gitItem &&
                gitItem.ObjectType == GitObjectType.Blob &&
                !string.IsNullOrWhiteSpace(gitItem.FileName))
            {
                var fileName = gitItem.FileName.SubstringAfterLast('/').SubstringAfterLast('\\');

                fileName = (Path.GetTempPath() + fileName).ToNativePath();
                Module.SaveBlobAs(fileName, gitItem.Guid);
                return fileName;
            }

            return null;
        }

        private void SpawnCommitBrowser(GitItem item)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = Application.ExecutablePath,
                    Arguments = "browse",
                    WorkingDirectory = _fullPathResolver.Resolve(item.FileName.EnsureTrailingPathSeparator())
                }
            };
            if (item.Guid.IsNotNullOrWhitespace())
            {
                process.StartInfo.Arguments += " -commit=" + item.Guid;
            }

            process.Start();
        }

        private void tvGitTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(ViewItem);

            Task ViewItem()
            {
                return e.Node?.Tag is GitItem gitItem
                    ? ViewGitItemAsync(gitItem)
                    : Task.CompletedTask;
            }

            Task ViewGitItemAsync(GitItem gitItem)
            {
                switch (gitItem.ObjectType)
                {
                    case GitObjectType.Blob:
                    {
                        return FileText.ViewGitItemAsync(gitItem.FileName, gitItem.ObjectId);
                    }

                    case GitObjectType.Commit:
                    {
                        return FileText.ViewTextAsync(gitItem.FileName, LocalizationHelpers.GetSubmoduleText(Module, gitItem.FileName, gitItem.Guid));
                    }

                    default:
                    {
                        return FileText.ViewTextAsync("", "");
                    }
                }
            }
        }

        private void tvGitTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.IsExpanded)
            {
                return;
            }

            if (e.Node?.Tag is GitItem gitItem)
            {
                e.Node.Nodes.Clear();
                _revisionFileTreeController.LoadChildren(gitItem, e.Node.Nodes, tvGitTree.ImageList.Images);
            }
        }

        private void tvGitTree_DoubleClick(object sender, EventArgs e)
        {
            OnItemActivated();
        }

        private void tvGitTree_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Item is TreeNode treeNode &&
                treeNode.Tag is GitItem gitItem)
            {
                var fileList = new StringCollection();
                var fileName = _fullPathResolver.Resolve(gitItem.FileName);

                fileList.Add(fileName.ToNativePath());

                var obj = new DataObject();
                obj.SetFileDropList(fileList);

                DoDragDrop(obj, DragDropEffects.Copy);
            }
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
            if (tvGitTree.SelectedNode?.Tag is GitItem gitItem)
            {
                UICommands.StartFileHistoryDialog(this, gitItem.FileName, _revision, true, true);
            }
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tvGitTree.CollapseAll();
        }

        private void copyFilenameToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem gitItem)
            {
                var fileName = _fullPathResolver.Resolve(gitItem.FileName);
                ClipboardUtil.TrySetText(fileName.ToNativePath());
            }
        }

        private void fileHistoryItem_Click(object sender, EventArgs e)
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem gitItem)
            {
                UICommands.StartFileHistoryDialog(this, gitItem.FileName, _revision);
            }
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
                    return; // Item does not exist in the tree
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
            if (tvGitTree.SelectedNode?.Tag is GitItem gitItem && gitItem.ObjectType == GitObjectType.Blob)
            {
                var fileName = _fullPathResolver.Resolve(gitItem.FileName);
                UICommands.StartFileEditorDialog(fileName);
            }
        }

        private void expandAllStripMenuItem_Click(object sender, EventArgs e)
        {
            tvGitTree.ExpandAll();
        }

        private void expandSubtreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tvGitTree.SelectedNode?.ExpandAll();
        }

        private void fileTreeArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem gitItem)
            {
                UICommands.StartArchiveDialog(this, _revision, null, gitItem.FileName);
            }
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
                // No item selected is handled as the repo source
                filePath = Module.WorkingDir;
            }

            UICommands.StartCleanupRepositoryDialog(this, filePath);
        }

        private void FileTreeContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var gitItem = tvGitTree.SelectedNode?.Tag as GitItem;
            var itemSelected = gitItem != null;
            var isFile = itemSelected && gitItem.ObjectType == GitObjectType.Blob;
            var isFolder = itemSelected && gitItem.ObjectType == GitObjectType.Tree;
            var isFileOrFolder = isFile || isFolder;
            var isExistingFileOrDirectory = itemSelected && FormBrowseUtil.IsFileOrDirectory(_fullPathResolver.Resolve(gitItem.FileName));

            if (itemSelected && gitItem.ObjectType == GitObjectType.Commit)
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

            saveAsToolStripMenuItem.Visible = isFile;
            resetToThisRevisionToolStripMenuItem.Visible = isFileOrFolder && !Module.IsBareRepository();
            toolStripSeparatorFileSystemActions.Visible = isFileOrFolder;

            copyFilenameToClipboardToolStripMenuItem.Visible = itemSelected;
            fileTreeOpenContainingFolderToolStripMenuItem.Enabled = isExistingFileOrDirectory;
            fileTreeArchiveToolStripMenuItem.Enabled = itemSelected;
            fileTreeCleanWorkingTreeToolStripMenuItem.Visible = isFileOrFolder;
            fileTreeCleanWorkingTreeToolStripMenuItem.Enabled = isExistingFileOrDirectory;

            fileHistoryToolStripMenuItem.Enabled = itemSelected;
            blameToolStripMenuItem1.Visible = isFile;

            editCheckedOutFileToolStripMenuItem.Visible = isFile;
            editCheckedOutFileToolStripMenuItem.Enabled = isExistingFileOrDirectory;
            openWithToolStripMenuItem.Visible = isFile;
            openWithToolStripMenuItem.Enabled = isExistingFileOrDirectory;
            openWithDifftoolToolStripMenuItem.Visible = isFile;
            openWithDifftoolToolStripMenuItem.Enabled = FileText.OpenWithDifftool != null;
            openFileToolStripMenuItem.Visible = isFile;
            openFileWithToolStripMenuItem.Visible = isFile;

            toolStripSeparatorGitActions.Visible = isFile;
            stopTrackingThisFileToolStripMenuItem.Visible = isFile;
            stopTrackingThisFileToolStripMenuItem.Enabled = isExistingFileOrDirectory;
            assumeUnchangedTheFileToolStripMenuItem.Visible = isFile;
            assumeUnchangedTheFileToolStripMenuItem.Enabled = isExistingFileOrDirectory;
            findToolStripMenuItem.Enabled = tvGitTree.Nodes.Count > 0;

            toolStripSeparatorFileTreeActions.Visible = isFile;
            expandSubtreeToolStripMenuItem.Visible = isFolder;
        }

        private void fileTreeOpenContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string filePath = tvGitTree.SelectedNode?.Tag is GitItem gitItem
                ? _fullPathResolver.Resolve(gitItem.FileName)
                : Module.WorkingDir;
            FormBrowseUtil.ShowFileOrFolderInFileExplorer(filePath);
        }

        private void openFileWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileName = SaveSelectedItemToTempFile();
            if (fileName != null)
            {
                OsShellUtil.OpenAs(fileName);
            }
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var fileName = SaveSelectedItemToTempFile();
                if (fileName != null)
                {
                    Process.Start(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void openSubmoduleMenuItem_Click(object sender, EventArgs e)
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem gitItem && gitItem.ObjectType == GitObjectType.Commit)
            {
                SpawnCommitBrowser(gitItem);
            }
        }

        private void openWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem gitItem && gitItem.ObjectType == GitObjectType.Blob)
            {
                var fileName = _fullPathResolver.Resolve(gitItem.FileName);
                OsShellUtil.OpenAs(fileName.ToNativePath());
            }
        }

        private void openWithDifftoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileText.OpenWithDifftool?.Invoke();
        }

        private void resetToThisRevisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem gitItem)
            {
                if (MessageBox.Show(_resetFileText.Text, _resetFileCaption.Text, MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    Module.CheckoutFiles(new[] { gitItem.FileName }, _revision.ObjectId, false);
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem gitItem && gitItem.ObjectType == GitObjectType.Blob)
            {
                var fullName = _fullPathResolver.Resolve(gitItem.FileName);
                using (var fileDialog =
                    new SaveFileDialog
                    {
                        InitialDirectory = Path.GetDirectoryName(fullName),
                        FileName = Path.GetFileName(fullName),
                        DefaultExt = PathUtil.GetFileExtension(fullName),
                        AddExtension = true
                    })
                {
                    var extension = PathUtil.GetFileExtension(fileDialog.FileName);

                    fileDialog.Filter = $@"{_saveFileFilterCurrentFormat.Text}(*.{extension})|*.{extension}| {_saveFileFilterAllFiles.Text} (*.*)|*.*";
                    if (fileDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        Module.SaveBlobAs(fileDialog.FileName, gitItem.Guid);
                    }
                }
            }
        }

        private string GetSelectedFile()
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem item && item.ObjectType != GitObjectType.Tree)
            {
                var filename = _fullPathResolver.Resolve(item.FileName);
                if (File.Exists(filename))
                {
                    return filename;
                }
            }

            return null;
        }

        private void assumeUnchangedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedFile = GetSelectedFile();

            if (selectedFile == null)
            {
                return;
            }

            var itemStatus = new GitItemStatus { Name = selectedFile };

            var answer = MessageBox.Show(_assumeUnchangedMessage.Text, _assumeUnchangedCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (answer == DialogResult.No)
            {
                return;
            }

            Module.AssumeUnchangedFiles(new List<GitItemStatus> { itemStatus }, true, out var wereErrors);

            if (wereErrors)
            {
                MessageBox.Show(string.Format(_assumeUnchangedFail.Text, itemStatus.Name), _error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(_assumeUnchangedSuccess.Text, _success.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void stopTrackingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var filename = GetSelectedFile();
            if (filename == null)
            {
                return;
            }

            var answer = MessageBox.Show(string.Format(_stopTrackingMessage.Text, filename), _stopTrackingCaption.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer == DialogResult.No)
            {
                return;
            }

            if (Module.StopTrackingFile(filename))
            {
                MessageBox.Show(_stopTrackingSuccess.Text, _success.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(string.Format(_stopTrackingFail.Text, filename), _error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tvGitTree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                tvGitTree.SelectedNode = e.Node;
            }
        }

        public void SwitchFocus(bool alreadyContainedFocus)
        {
            if (alreadyContainedFocus && tvGitTree.Focused)
            {
                FileText.Focus();
            }
            else
            {
                tvGitTree.Focus();
            }
        }
    }
}
