﻿using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using GitCommands;
using GitCommands.Git;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Hotkey;
using GitUI.Properties;
using GitUIPluginInterfaces;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class RevisionFileTreeControl : GitModuleControl
    {
        private readonly TranslationString _resetFileCaption = new("Reset");
        private readonly TranslationString _resetFileText = new("Are you sure you want to reset this file or directory?");
        private readonly TranslationString _saveFileFilterCurrentFormat = new("Current format");
        private readonly TranslationString _saveFileFilterAllFiles = new("All files");
        private readonly TranslationString _nodeNotFoundNextAvailableParentSelected = new("Node not found. The next available parent node will be selected.");
        private readonly TranslationString _nodeNotFoundSelectionNotChanged = new("Node not found. File tree selection was not changed.");

        private readonly TranslationString _assumeUnchangedMessage = new(@"This feature should be used for performance purpose when it is costly for git to check the state of a big file.


Are you sure to assume this file won't change ?");
        private readonly TranslationString _assumeUnchangedCaption = new("Assume this file won't change");
        private readonly TranslationString _assumeUnchangedFail = new("Fail to assume unchanged the file '{0}'.");
        private readonly TranslationString _assumeUnchangedSuccess = new("File successfully assumed unchanged.");

        private readonly TranslationString _stopTrackingMessage = new(@"Are you sure you want to stop tracking the file
'{0}'?");
        private readonly TranslationString _stopTrackingCaption = new("Stop tracking the file");
        private readonly TranslationString _stopTrackingFail = new("Fail to stop tracking the file '{0}'.");
        private readonly TranslationString _stopTrackingSuccess = new(@"File successfully untracked. Removal has been added to the staging area.

See the changes in the commit form.");

        private readonly TranslationString _success = new("Success");

        // store strings to not keep references to nodes
        private readonly IRevisionFileTreeController _revisionFileTreeController;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly IFindFilePredicateProvider _findFilePredicateProvider = new FindFilePredicateProvider();
        private GitRevision? _revision;
        private readonly RememberFileContextMenuController _rememberFileContextMenuController
            = RememberFileContextMenuController.Default;
        private Action? _refreshGitStatus;
        private RevisionGridControl? _revisionGrid;
        private readonly CancellationTokenSequence _viewBlameSequence = new();

        public RevisionFileTreeControl()
        {
            InitializeComponent();
            InitializeComplete();
            HotkeysEnabled = true;
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
            _revisionFileTreeController = new RevisionFileTreeController(() => Module.WorkingDir,
                                                                         new GitRevisionInfoProvider(() => Module),
                                                                         new FileAssociatedIconProvider());
            BlameControl.HideCommitInfo();
            blameToolStripMenuItem1.Checked = AppSettings.RevisionFileTreeShowBlame;
            filterFileInGridToolStripMenuItem.Text = TranslatedStrings.FilterFileInGrid;
        }

        public void Bind(RevisionGridControl revisionGrid, Action? refreshGitStatus)
        {
            _revisionGrid = revisionGrid;
            _refreshGitStatus = refreshGitStatus;
        }

        /// <summary>
        /// Expand the tree for the path and show the contents
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <param name="line">The optional line to open</param>
        /// <param name="requestBlame">Request that Blame is shown in the FileTree</param>
        public void ExpandToFile(string filePath, int? line, bool requestBlame)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            var pathParts = filePath.Split(Delimiters.ForwardSlash);

            var currentNodes = tvGitTree.Nodes;
            TreeNode? foundNode = null;
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

                if (currentFoundNode is null)
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

            if (foundNode is not null)
            {
                if (isIncompleteMatch || foundNode.Tag is not GitItem gitItem)
                {
                    MessageBox.Show(_nodeNotFoundNextAvailableParentSelected.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    BlameControl.Visible = false;
                    FileText.Visible = true;
                    FileText.Clear();

                    return;
                }

                if (requestBlame && !AppSettings.RevisionFileTreeShowBlame)
                {
                    blameToolStripMenuItem1.Checked = AppSettings.RevisionFileTreeShowBlame = true;
                }

                // AfterSelect will not fire when selecting again, show manually
                tvGitTree.AfterSelect -= new System.Windows.Forms.TreeViewEventHandler(tvGitTree_AfterSelect);
                tvGitTree.SelectedNode = foundNode;
                tvGitTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(tvGitTree_AfterSelect);
                tvGitTree.SelectedNode.EnsureVisible();

                ThreadHelper.JoinableTaskFactory.RunAsync(() => ShowGitItemAsync(gitItem, line));
            }
            else
            {
                MessageBox.Show(_nodeNotFoundSelectionNotChanged.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void InitSplitterManager(SplitterManager splitterManager)
        {
            splitterManager.AddSplitter(FileTreeSplitContainer, "FileTreeSplitContainer");
        }

        public void InvokeFindFileDialog()
        {
            tvGitTree.Focus();
            findToolStripMenuItem_Click(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets or sets the file in the list to select initially.
        /// When switching commits, the last selected file is "followed" if available in the new commit,
        /// this file is used as a fallback.
        /// </summary>
        public string? FallbackFollowedFile { get; set; } = null;

        public void LoadRevision(GitRevision? revision)
        {
            _revision = revision;
            _revisionFileTreeController.ResetCache();

            try
            {
                tvGitTree.SuspendLayout();
                tvGitTree.BeginUpdate();

                // Save state only when there is selected node
                List<string> tryNodes = new();

                // When blame control is visible, taking the filename from the revision selected to blame
                // because the file could have been renamed in between
                if (BlameControl.Visible && !string.IsNullOrWhiteSpace(BlameControl.PathToBlame))
                {
                    tryNodes.Add(BlameControl.PathToBlame);
                }
                else if (tvGitTree.SelectedNode is not null)
                {
                    TreeNode node = tvGitTree.SelectedNode;
                    string path = "";
                    while (node is not null)
                    {
                        path = string.IsNullOrWhiteSpace(path) ? node.Text : $"{node.Text}/{path}";
                        node = node.Parent;
                    }

                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        tryNodes.Add(path);
                    }
                }

                if (!string.IsNullOrWhiteSpace(FallbackFollowedFile))
                {
                    tryNodes.Add(FallbackFollowedFile);
                }

                // Refresh tree
                tvGitTree.Nodes.Clear();

                // restore selected file when new selection is done
                if (_revision is not null && !_revision.IsArtificial && tvGitTree.ImageList is not null)
                {
                    _revisionFileTreeController.LoadChildren(_revision, tvGitTree.Nodes, tvGitTree.ImageList.Images);

                    foreach (string path in tryNodes)
                    {
                        if (_revisionFileTreeController.SelectFileOrFolder(tvGitTree, path))
                        {
                            // Try scroll
                            tvGitTree.SelectedNode.EnsureVisible();
                            break;
                        }
                    }
                }

                if (tvGitTree.SelectedNode is null)
                {
                    BlameControl.Visible = false;
                    FileText.Visible = true;
                    FileText.Clear();
                }
            }
            finally
            {
                tvGitTree.EndUpdate();
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
            EditFile = 5,
            FilterFileInGrid = 6,
            FindFile = 7,
            OpenWorkingDirectoryFileWith = 8,
        }

        public CommandStatus ExecuteCommand(Command cmd)
        {
            return ExecuteCommand((int)cmd);
        }

        protected override CommandStatus ExecuteCommand(int cmd)
        {
            switch ((Command)cmd)
            {
                case Command.ShowHistory: fileHistoryToolStripMenuItem.PerformClick(); break;
                case Command.Blame: blameToolStripMenuItem1.PerformClick(); break;
                case Command.OpenWithDifftool: openWithDifftoolToolStripMenuItem.PerformClick(); break;
                case Command.OpenAsTempFile: openFileToolStripMenuItem.PerformClick(); break;
                case Command.OpenAsTempFileWith: openFileWithToolStripMenuItem.PerformClick(); break;
                case Command.OpenWorkingDirectoryFileWith: openWithToolStripMenuItem.PerformClick(); break;
                case Command.EditFile: editCheckedOutFileToolStripMenuItem.PerformClick(); break;
                case Command.FilterFileInGrid: filterFileInGridToolStripMenuItem.PerformClick(); break;
                case Command.FindFile: findToolStripMenuItem.PerformClick(); break;
                default: return base.ExecuteCommand(cmd);
            }

            return true;
        }

        public override bool ProcessHotkey(Keys keyData)
        {
            return base.ProcessHotkey(keyData)
                || (!GitExtensionsControl.IsTextEditKey(keyData) // downstream (without keys for quick search)
                    && ((FileText.Visible && FileText.ProcessHotkey(keyData))
                        || (BlameControl.Visible && BlameControl.ProcessHotkey(keyData))));
        }

        public void ReloadHotkeys()
        {
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            fileHistoryToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.ShowHistory);
            blameToolStripMenuItem1.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.Blame);
            openWithDifftoolToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenWithDifftool);
            openFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenAsTempFile);
            openFileWithToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenAsTempFileWith);
            openWithToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenWorkingDirectoryFileWith);
            editCheckedOutFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.EditFile);
            filterFileInGridToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.FilterFileInGrid);
            findToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.FindFile);
            FileText.ReloadHotkeys();
        }

        private string GetShortcutKeyDisplayString(Command cmd)
        {
            return GetShortcutKeys((int)cmd).ToShortcutKeyDisplayString();
        }

        #endregion

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _viewBlameSequence.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

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
            var candidates = _revision?.TreeGuid is null ? Enumerable.Empty<string>() : Module.GetFullTree(_revision.TreeGuid.ToString());
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

        private string? SaveSelectedItemToTempFile()
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem { ObjectType: GitObjectType.Blob } gitItem &&
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
            string path = _fullPathResolver.Resolve(item.FileName.EnsureTrailingPathSeparator()) ?? "";
            if (!Directory.Exists(path))
            {
                MessageBoxes.SubmoduleDirectoryDoesNotExist(this, path, item.Name);
                return;
            }

            GitUICommands.LaunchBrowse(workingDir: path, selectedId: item.ObjectId);
        }

        private void tvGitTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(ViewItem);

            Task ViewItem()
            {
                // new selection, start show at line 1
                return e.Node?.Tag is GitItem gitItem
                    ? ShowGitItemAsync(gitItem, 1)
                    : ClearOutputAsync();
            }
        }

        /// <summary>
        /// Show the selected item
        /// </summary>
        /// <param name="gitItem">The <see cref="GitItem"/> to show.</param>
        /// <param name="line">The line to show if Blame is selected, null to not change selection for existing files.</param>
        /// <returns>The Task from FileViewer or Blame.</returns>
        private Task ShowGitItemAsync(GitItem gitItem, int? line)
        {
            switch (gitItem.ObjectType)
            {
                case GitObjectType.Blob:
                    {
                        if (!blameToolStripMenuItem1.Checked)
                        {
                            return ViewGitItemAsync(gitItem);
                        }

                        FileText.Visible = false;
                        BlameControl.Visible = true;
                        return BlameControl.LoadBlameAsync(_revision, children: null, gitItem.FileName, _revisionGrid, controlToMask: null, FileText.Encoding, line, cancellationToken: _viewBlameSequence.Next());
                    }

                case GitObjectType.Commit:
                    {
                        return ViewGitItemAsync(gitItem);
                    }

                default:
                    return ClearOutputAsync();
            }

            Task ViewGitItemAsync(GitItem gitItem)
            {
                GitItemStatus file = new(name: gitItem.FileName)
                {
                    IsTracked = true,
                    TreeGuid = gitItem.ObjectId,
                    IsSubmodule = gitItem.ObjectType == GitObjectType.Commit
                };

                BlameControl.Visible = false;
                FileText.Visible = true;
                return FileText.ViewGitItemAsync(file, gitItem.ObjectId);
            }
        }

        private Task ClearOutputAsync()
        {
            BlameControl.Visible = false;
            FileText.Visible = true;
            return FileText.ClearAsync();
        }

        private void tvGitTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.IsExpanded)
            {
                return;
            }

            if (e.Node.Tag is GitItem gitItem)
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
            if (e.Item is TreeNode { Tag: GitItem gitItem })
            {
                StringCollection fileList = new();
                var fileName = _fullPathResolver.Resolve(gitItem.FileName);

                fileList.Add(fileName.ToNativePath());

                DataObject obj = new();
                obj.SetFileDropList(fileList);

                DoDragDrop(obj, DragDropEffects.Copy);
            }
        }

        private void tvGitTree_KeyDown(object sender, KeyEventArgs e)
        {
            if (tvGitTree.SelectedNode is null || e.KeyCode != Keys.Enter)
            {
                return;
            }

            OnItemActivated();
            e.Handled = true;
        }

        private void blameMenuItem_Click(object sender, EventArgs e)
        {
            if (tvGitTree.SelectedNode?.Tag is not GitItem gitItem)
            {
                return;
            }

            blameToolStripMenuItem1.Checked = !blameToolStripMenuItem1.Checked;
            AppSettings.RevisionFileTreeShowBlame = blameToolStripMenuItem1.Checked;
            int? line = FileText.Visible ? FileText.CurrentFileLine : null;

            ThreadHelper.JoinableTaskFactory.RunAsync(() => ShowGitItemAsync(gitItem, line));
        }

        private void copyFilenameToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem gitItem)
            {
                var fileName = _fullPathResolver.Resolve(gitItem.FileName);
                if (fileName is not null)
                {
                    ClipboardUtil.TrySetText(fileName.ToNativePath());
                }
            }
        }

        private bool TryGetSelectedName([NotNullWhen(returnValue: true)] out string? name)
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem gitItem)
            {
                name = gitItem.FileName;
                if (gitItem.ObjectType == GitObjectType.Tree)
                {
                    name += "/";
                }

                return true;
            }

            name = null;
            return false;
        }

        private void filterFileInGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TryGetSelectedName(out string name))
            {
                (FindForm() as FormBrowse)?.SetPathFilter(name.ToPosixPath());
            }
        }

        private void fileHistoryItem_Click(object sender, EventArgs e)
        {
            if (TryGetSelectedName(out string name))
            {
                UICommands.StartFileHistoryDialog(this, name, _revision);
            }
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string? selectedItem;
            using (var searchWindow = new SearchWindow<string>(FindFileMatches) { Owner = FindForm() })
            {
                searchWindow.ShowDialog(this);
                selectedItem = searchWindow.SelectedItem;
            }

            if (string.IsNullOrEmpty(selectedItem))
            {
                return;
            }

            var items = selectedItem.Split(Delimiters.ForwardSlash);
            var nodes = tvGitTree.Nodes;

            for (var i = 0; i < items.Length - 1; i++)
            {
                var selectedNode = _revisionFileTreeController.Find(nodes, items[i]);
                if (selectedNode is null)
                {
                    return; // Item does not exist in the tree
                }

                selectedNode.Expand();
                nodes = selectedNode.Nodes;
            }

            var lastItem = _revisionFileTreeController.Find(nodes, items[items.Length - 1]);
            if (lastItem is not null)
            {
                tvGitTree.SelectedNode = lastItem;
            }
        }

        private void editCheckedOutFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvGitTree.SelectedNode?.Tag is not GitItem gitItem || gitItem.ObjectType != GitObjectType.Blob)
            {
                return;
            }

            var fileName = _fullPathResolver.Resolve(gitItem.FileName);
            Validates.NotNull(fileName);
            UICommands.StartFileEditorDialog(fileName);
            _refreshGitStatus?.Invoke();
        }

        private void expandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tvGitTree.SelectedNode?.ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tvGitTree.CollapseAll();
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
            string filePath;
            if (tvGitTree.SelectedNode?.Tag is GitItem gitItem)
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
            var itemSelected = gitItem is not null;
            var isFile = gitItem?.ObjectType == GitObjectType.Blob;
            var isFolder = gitItem?.ObjectType == GitObjectType.Tree;
            var isFileOrFolder = isFile || isFolder;

            // Many items does not make sense if a local file does not exist, why this is used for Enabled
            var isExistingFileOrDirectory = gitItem is not null && FormBrowseUtil.IsFileOrDirectory(_fullPathResolver.Resolve(gitItem.FileName));

            var openSubVisible = gitItem?.ObjectType == GitObjectType.Commit && isExistingFileOrDirectory;
            openSubmoduleMenuItem.Visible = openSubVisible;
            if (openSubVisible)
            {
                if (!openSubmoduleMenuItem.Font.Bold)
                {
                    openSubmoduleMenuItem.Font = new Font(openSubmoduleMenuItem.Font, FontStyle.Bold);
                }

                if (fileHistoryToolStripMenuItem.Font.Bold)
                {
                    fileHistoryToolStripMenuItem.Font = new Font(fileHistoryToolStripMenuItem.Font, FontStyle.Regular);
                }
            }
            else if (!fileHistoryToolStripMenuItem.Font.Bold)
            {
                fileHistoryToolStripMenuItem.Font = new Font(fileHistoryToolStripMenuItem.Font, FontStyle.Bold);
            }

            // Diff with workTree (some tools like kdiff3 and meld allows diff to NUL)
            resetToThisRevisionToolStripMenuItem.Visible = itemSelected && !Module.IsBareRepository();
            toolStripSeparatorTopActions.Visible = gitItem is not null && ((gitItem.ObjectType == GitObjectType.Commit && isExistingFileOrDirectory)
                                                                    || !Module.IsBareRepository());

            // RememberFile diff can be done for folders too (as well as for submodules, but that is meaningless)
            // However diffs will open many windows that cannot be aborted, so it is blocked
            // Another reason is that file<->folder compare is not giving any result
            // (and diff is shared with Diff tab that has no notion of folders)
            openWithDifftoolToolStripMenuItem.Visible = isFile;
            openWithToolStripMenuItem.Visible = isFile;
            openWithToolStripMenuItem.Enabled = isExistingFileOrDirectory;
            Validates.NotNull(_revision);
            var fsi = _rememberFileContextMenuController.CreateFileStatusItem(gitItem?.FileName ?? "", _revision);
            diffWithRememberedFileToolStripMenuItem.Visible = _rememberFileContextMenuController.RememberedDiffFileItem is not null;
            diffWithRememberedFileToolStripMenuItem.Enabled = isFile && fsi != _rememberFileContextMenuController.RememberedDiffFileItem
                                                                         && _rememberFileContextMenuController.ShouldEnableSecondItemDiff(fsi);
            diffWithRememberedFileToolStripMenuItem.Text =
                _rememberFileContextMenuController.RememberedDiffFileItem is not null
                    ? string.Format(TranslatedStrings.DiffSelectedWithRememberedFile, _rememberFileContextMenuController.RememberedDiffFileItem.Item.Name)
                    : string.Empty;

            rememberFileStripMenuItem.Visible = isFile;
            rememberFileStripMenuItem.Enabled = _rememberFileContextMenuController.ShouldEnableFirstItemDiff(fsi, isSecondRevision: true);

            openFileToolStripMenuItem.Visible = isFile;
            openFileWithToolStripMenuItem.Visible = isFile;
            saveAsToolStripMenuItem.Visible = isFile;
            editCheckedOutFileToolStripMenuItem.Visible = isFile;
            editCheckedOutFileToolStripMenuItem.Enabled = isExistingFileOrDirectory;
            toolStripSeparatorFileSystemActions.Visible = isFile;

            copyFilenameToClipboardToolStripMenuItem.Visible = itemSelected;
            fileTreeOpenContainingFolderToolStripMenuItem.Visible = itemSelected;
            fileTreeOpenContainingFolderToolStripMenuItem.Enabled = isExistingFileOrDirectory;
            toolStripSeparatorFileNameActions.Visible = itemSelected;

            fileHistoryToolStripMenuItem.Enabled = itemSelected;
            blameToolStripMenuItem1.Visible = isFile;
            fileTreeArchiveToolStripMenuItem.Enabled = itemSelected;
            fileTreeCleanWorkingTreeToolStripMenuItem.Visible = isFileOrFolder;
            fileTreeCleanWorkingTreeToolStripMenuItem.Enabled = isExistingFileOrDirectory;
            toolStripSeparatorGitActions.Visible = itemSelected;

            stopTrackingThisFileToolStripMenuItem.Visible = isFile;
            stopTrackingThisFileToolStripMenuItem.Enabled = isExistingFileOrDirectory;
            assumeUnchangedTheFileToolStripMenuItem.Visible = isFile;
            assumeUnchangedTheFileToolStripMenuItem.Enabled = isExistingFileOrDirectory;
            toolStripSeparatorGitTrackingActions.Visible = isFile;

            findToolStripMenuItem.Enabled = tvGitTree.Nodes.Count > 0;
            expandToolStripMenuItem.Visible = isFolder;
            collapseAllToolStripMenuItem.Visible = isFolder;
        }

        private void fileTreeOpenContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string? filePath = tvGitTree.SelectedNode?.Tag is GitItem gitItem
                ? _fullPathResolver.Resolve(gitItem.FileName)
                : Module.WorkingDir;

            if (filePath is not null)
            {
                FormBrowseUtil.ShowFileOrFolderInFileExplorer(filePath);
            }
        }

        private void openFileWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileName = SaveSelectedItemToTempFile();
            if (fileName is not null)
            {
                OsShellUtil.OpenAs(fileName);
            }
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileName = SaveSelectedItemToTempFile();
            if (fileName is not null)
            {
                OsShellUtil.Open(fileName);
            }
        }

        private void openSubmoduleMenuItem_Click(object sender, EventArgs e)
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem { ObjectType: GitObjectType.Commit } gitItem)
            {
                SpawnCommitBrowser(gitItem);
            }
        }

        private void openWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem { ObjectType: GitObjectType.Blob } gitItem)
            {
                var fileName = _fullPathResolver.Resolve(gitItem.FileName);
                if (File.Exists(fileName))
                {
                    OsShellUtil.OpenAs(fileName.ToNativePath());
                }
            }
        }

        private void openWithDifftoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem gitItem)
            {
                Module.OpenWithDifftool(gitItem.FileName, firstRevision: _revision?.ObjectId?.ToString());
            }
        }

        private void diffWithRememberedFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvGitTree.SelectedNode?.Tag is not GitItem gitItem || _revision is null)
            {
                return;
            }

            var fsi = _rememberFileContextMenuController.CreateFileStatusItem(gitItem.FileName, _revision);
            var first = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash,
                _rememberFileContextMenuController.RememberedDiffFileItem, isSecondRevision: true);
            var second = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash, fsi, isSecondRevision: true);

            Module.OpenFilesWithDifftool(first, second);
        }

        private void rememberFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(tvGitTree.SelectedNode?.Tag is GitItem gitItem) || _revision is null)
            {
                return;
            }

            var fsi = _rememberFileContextMenuController.CreateFileStatusItem(gitItem.FileName, _revision);
            _rememberFileContextMenuController.RememberedDiffFileItem = fsi;
        }

        private void resetToThisRevisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem gitItem && _revision is not null)
            {
                if (MessageBox.Show(_resetFileText.Text, _resetFileCaption.Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    Module.CheckoutFiles(new[] { gitItem.FileName }, _revision.ObjectId, false);
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvGitTree.SelectedNode?.Tag is GitItem { ObjectType: GitObjectType.Blob } gitItem)
            {
                var fullName = _fullPathResolver.Resolve(gitItem.FileName);
                using SaveFileDialog fileDialog =
                    new()
                    {
                        InitialDirectory = Path.GetDirectoryName(fullName),
                        FileName = Path.GetFileName(fullName),
                        DefaultExt = Path.GetExtension(fullName),
                        AddExtension = true
                    };
                var extension = Path.GetExtension(fileDialog.FileName);

                fileDialog.Filter = $@"{_saveFileFilterCurrentFormat.Text}(*{extension})|*{extension}| {_saveFileFilterAllFiles.Text} (*.*)|*.*";
                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    Module.SaveBlobAs(fileDialog.FileName, gitItem.Guid);
                }
            }
        }

        private string? GetSelectedFile()
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

            if (selectedFile is null)
            {
                return;
            }

            GitItemStatus itemStatus = new(name: selectedFile);

            var answer = MessageBox.Show(_assumeUnchangedMessage.Text, _assumeUnchangedCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (answer == DialogResult.No)
            {
                return;
            }

            bool wereErrors = !Module.AssumeUnchangedFiles(new List<GitItemStatus> { itemStatus }, true, out _);

            if (wereErrors)
            {
                MessageBox.Show(string.Format(_assumeUnchangedFail.Text, itemStatus.Name), TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(_assumeUnchangedSuccess.Text, _success.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void stopTrackingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var filename = GetSelectedFile();
            if (filename is null)
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
                MessageBox.Show(string.Format(_stopTrackingFail.Text, filename), TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                if (BlameControl.Visible)
                {
                    BlameControl.Focus();
                }
                else
                {
                    FileText.Focus();
                }
            }
            else
            {
                tvGitTree.Focus();
            }
        }

        public bool SelectFileOrFolder(string filePath)
        {
            if (filePath is null || filePath.IndexOf(Module.WorkingDir) != 0)
            {
                return false;
            }

            return _revisionFileTreeController.SelectFileOrFolder(tvGitTree, filePath.Substring(Module.WorkingDir.Length));
        }

        internal TestAccessor GetTestAccessor()
             => new(this);

        internal readonly struct TestAccessor
        {
            private readonly RevisionFileTreeControl _control;

            public TestAccessor(RevisionFileTreeControl control)
            {
                _control = control;
            }

            public SplitContainer FileTreeSplitContainer => _control.FileTreeSplitContainer;
        }
    }
}
