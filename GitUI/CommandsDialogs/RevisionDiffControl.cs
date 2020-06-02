using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.HelperDialogs;
using GitUI.Hotkey;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class RevisionDiffControl : GitModuleControl
    {
        private readonly TranslationString _saveFileFilterCurrentFormat = new TranslationString("Current format");
        private readonly TranslationString _saveFileFilterAllFiles = new TranslationString("All files");
        private readonly TranslationString _deleteSelectedFilesCaption = new TranslationString("Delete");
        private readonly TranslationString _deleteSelectedFiles =
            new TranslationString("Are you sure you want to delete the selected file(s)?");
        private readonly TranslationString _deleteFailed = new TranslationString("Delete file failed");
        private readonly TranslationString _multipleDescription = new TranslationString("<multiple>");
        private readonly TranslationString _selectedRevision = new TranslationString("Selected: b/");
        private readonly TranslationString _firstRevision = new TranslationString("First: a/");

        private RevisionGridControl _revisionGrid;
        private RevisionFileTreeControl _revisionFileTree;
        private IRevisionDiffController _revisionDiffController;
        private readonly IFileStatusListContextMenuController _revisionDiffContextMenuController;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly IFindFilePredicateProvider _findFilePredicateProvider;
        private readonly IGitRevisionTester _gitRevisionTester;

        public RevisionDiffControl()
        {
            InitializeComponent();
            DiffFiles.GroupByRevision = true;
            InitializeComplete();
            HotkeysEnabled = true;
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
            _findFilePredicateProvider = new FindFilePredicateProvider();
            _gitRevisionTester = new GitRevisionTester(_fullPathResolver);
            _revisionDiffContextMenuController = new FileStatusListContextMenuController();
            DiffText.TopScrollReached += FileViewer_TopScrollReached;
            DiffText.BottomScrollReached += FileViewer_BottomScrollReached;
        }

        private void FileViewer_TopScrollReached(object sender, EventArgs e)
        {
            DiffFiles.SelectPreviousVisibleItem();
            DiffText.ScrollToBottom();
        }

        private void FileViewer_BottomScrollReached(object sender, EventArgs e)
        {
            DiffFiles.SelectNextVisibleItem();
            DiffText.ScrollToTop();
        }

        public void RefreshArtificial()
        {
            if (!Visible)
            {
                return;
            }

            var revisions = _revisionGrid.GetSelectedRevisions();
            if (!revisions.Any(r => r.IsArtificial))
            {
                return;
            }

            DiffFiles.StoreNextIndexToSelect();
            SetDiffs(revisions);
            if (DiffFiles.SelectedItem == null)
            {
                DiffFiles.SelectStoredNextIndex();
            }
        }

        #region Hotkey commands

        public static readonly string HotkeySettingsName = "BrowseDiff";

        public enum Command
        {
            DeleteSelectedFiles = 0,
            ShowHistory = 1,
            Blame = 2,
            OpenWithDifftool = 3,
            EditFile = 4,
            OpenAsTempFile = 5,
            OpenAsTempFileWith = 6,
            OpenWithDifftoolFirstToLocal = 7,
            OpenWithDifftoolSelectedToLocal = 8
        }

        public CommandStatus ExecuteCommand(Command cmd)
        {
            return ExecuteCommand((int)cmd);
        }

        protected override CommandStatus ExecuteCommand(int cmd)
        {
            if (DiffFiles.FilterFocused && IsTextEditKey(GetShortcutKeys(cmd)))
            {
                return false;
            }

            UpdateStatusOfMenuItems();

            switch ((Command)cmd)
            {
                case Command.DeleteSelectedFiles: diffDeleteFileToolStripMenuItem.PerformClick(); break;
                case Command.ShowHistory: fileHistoryDiffToolstripMenuItem.PerformClick(); break;
                case Command.Blame: blameToolStripMenuItem.PerformClick(); break;
                case Command.OpenWithDifftool: firstToSelectedToolStripMenuItem.PerformClick(); break;
                case Command.OpenWithDifftoolFirstToLocal: firstToLocalToolStripMenuItem.PerformClick(); break;
                case Command.OpenWithDifftoolSelectedToLocal: selectedToLocalToolStripMenuItem.PerformClick(); break;
                case Command.EditFile: diffEditWorkingDirectoryFileToolStripMenuItem.PerformClick(); break;
                case Command.OpenAsTempFile: diffOpenRevisionFileToolStripMenuItem.PerformClick(); break;
                case Command.OpenAsTempFileWith: diffOpenRevisionFileWithToolStripMenuItem.PerformClick(); break;

                default: return base.ExecuteCommand(cmd);
            }

            return true;
        }

        public void ReloadHotkeys()
        {
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            diffDeleteFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.DeleteSelectedFiles);
            fileHistoryDiffToolstripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.ShowHistory);
            blameToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.Blame);
            firstToSelectedToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenWithDifftool);
            firstToLocalToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenWithDifftoolFirstToLocal);
            selectedToLocalToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenWithDifftoolSelectedToLocal);
            diffEditWorkingDirectoryFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.EditFile);
            diffOpenRevisionFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenAsTempFile);
            diffOpenRevisionFileWithToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenAsTempFileWith);

            DiffText.ReloadHotkeys();
        }

        private string GetShortcutKeyDisplayString(Command cmd)
        {
            return GetShortcutKeys((int)cmd).ToShortcutKeyDisplayString();
        }

        #endregion

        public void DisplayDiffTab(IReadOnlyList<GitRevision> revisions)
        {
            SetDiffs(revisions);
            if (DiffFiles.SelectedItem == null)
            {
                DiffFiles.SelectFirstVisibleItem();
            }
        }

        private void SetDiffs(IReadOnlyList<GitRevision> revisions)
        {
            var item = DiffFiles.SelectedItem;
            var oldDiffItem = DiffFiles.FirstGroupItems.Contains(item) ? item : null;
            DiffFiles.SetDiffs(revisions, _revisionGrid.GetRevision);

            // Try to restore previous item
            if (oldDiffItem != null && DiffFiles.FirstGroupItems.Any(i => i.Item.Name.Equals(oldDiffItem.Item.Name)))
            {
                DiffFiles.SelectedGitItem = oldDiffItem.Item;
            }
        }

        public void Bind(RevisionGridControl revisionGrid, RevisionFileTreeControl revisionFileTree)
        {
            _revisionGrid = revisionGrid;
            _revisionFileTree = revisionFileTree;
        }

        public void InitSplitterManager(SplitterManager splitterManager)
        {
            splitterManager.AddSplitter(DiffSplitContainer, "DiffSplitContainer");
        }

        protected override void OnRuntimeLoad()
        {
            _revisionDiffController = new RevisionDiffController(_gitRevisionTester);

            DiffFiles.DescribeRevision = objectId => DescribeRevision(objectId);
            DiffText.SetFileLoader(GetNextPatchFile);
            DiffText.Font = AppSettings.FixedWidthFont;
            ReloadHotkeys();

            base.OnRuntimeLoad();
        }

        private string DescribeRevision([CanBeNull] ObjectId objectId, int maxLength = 0)
        {
            if (objectId == null)
            {
                // No parent at all, present as working directory
                return ResourceManager.Strings.Workspace;
            }

            var revision = _revisionGrid.GetRevision(objectId);

            if (revision == null)
            {
                return objectId.ToShortString();
            }

            return _revisionGrid.DescribeRevision(revision, maxLength);
        }

        /// <summary>
        /// Provide a description for the first selected or parent to the "primary" selected last
        /// </summary>
        /// <returns>A description of the selected parent</returns>
        [CanBeNull]
        private string DescribeRevision(List<GitRevision> parents)
        {
            if (parents.Count == 1)
            {
                return DescribeRevision(parents.FirstOrDefault()?.ObjectId, 50);
            }

            if (parents.Count > 1)
            {
                return _multipleDescription.Text;
            }

            return null;
        }

        private bool GetNextPatchFile(bool searchBackward, bool loop, out int fileIndex, out Task loadFileContent)
        {
            fileIndex = -1;
            loadFileContent = Task.CompletedTask;
            if (DiffFiles.SelectedItem == null)
            {
                return false;
            }

            int idx = DiffFiles.SelectedIndex;
            if (idx == -1)
            {
                return false;
            }

            fileIndex = DiffFiles.GetNextIndex(searchBackward, loop);
            if (fileIndex == idx)
            {
                if (!loop)
                {
                    return false;
                }
            }
            else
            {
                DiffFiles.SetSelectedIndex(fileIndex, notify: false);
            }

            loadFileContent = ShowSelectedFileDiffAsync();
            return true;
        }

        private ContextMenuSelectionInfo GetSelectionInfo()
        {
            var selectedItems = DiffFiles.SelectedItems.ToList();

            // Some items are not supported if more than one revision is selected
            var revisions = selectedItems.SecondRevs().ToList();
            var selectedRev = revisions.Count() != 1 ? null : revisions.FirstOrDefault();

            // First (A) is parent if one revision selected or if parent, then selected
            var parentIds = selectedItems.FirstIds().ToList();

            // Combined diff is a display only diff, no manipulations
            bool isAnyCombinedDiff = parentIds.Contains(ObjectId.CombinedDiffId);
            int selectedGitItemCount = selectedItems.Count();

            // No changes to files in bare repos
            bool isBareRepository = Module.IsBareRepository();
            bool isAnyTracked = selectedItems.Any(item => item.Item.IsTracked);
            bool isAnyIndex = selectedItems.Any(item => item.Item.Staged == StagedStatus.Index);
            bool isAnyWorkTree = selectedItems.Any(item => item.Item.Staged == StagedStatus.WorkTree);
            bool isAnySubmodule = selectedItems.Any(item => item.Item.IsSubmodule);
            (bool allFilesExist, bool allFilesOrUntrackedDirectoriesExist) = FileOrUntrackedDirExists(selectedItems, _fullPathResolver);

            var selectionInfo = new ContextMenuSelectionInfo(
                selectedRevision: selectedRev,
                isAnyCombinedDiff: isAnyCombinedDiff,
                selectedGitItemCount: selectedGitItemCount,
                isAnyItemIndex: isAnyIndex,
                isAnyItemWorkTree: isAnyWorkTree,
                isBareRepository: isBareRepository,
                allFilesExist: allFilesExist,
                allFilesOrUntrackedDirectoriesExist: allFilesOrUntrackedDirectoriesExist,
                isAnyTracked: isAnyTracked,
                isAnySubmodule: isAnySubmodule);
            return selectionInfo;

            static (bool allFilesExist, bool allFilesOrUntrackedDirectoriesExist) FileOrUntrackedDirExists(List<FileStatusItem> items, IFullPathResolver fullPathResolver)
            {
                bool allFilesExist = items.Any();
                bool allFilesOrUntrackedDirectoriesExist = items.Any();
                foreach (var item in items)
                {
                    var path = fullPathResolver.Resolve(item.Item.Name);
                    var fileExists = File.Exists(path);
                    allFilesExist = allFilesExist && fileExists;
                    var fileOrUntrackedDirectoryExists = fileExists || (!item.Item.IsTracked && Directory.Exists(path));
                    allFilesOrUntrackedDirectoriesExist = allFilesOrUntrackedDirectoriesExist && fileOrUntrackedDirectoryExists;

                    if (allFilesExist == false && allFilesOrUntrackedDirectoriesExist == false)
                    {
                        break;
                    }
                }

                return (allFilesExist, allFilesOrUntrackedDirectoriesExist);
            }
        }

        private void ResetSelectedItemsTo(bool actsAsChild)
        {
            var selectedItems = DiffFiles.SelectedItems.ToList();
            if (!selectedItems.Any())
            {
                return;
            }

            if (actsAsChild)
            {
                // selected revisions
                var deletedItems = selectedItems
                    .Where(item => item.Item.IsDeleted)
                    .Select(item => item.Item.Name).ToList();
                Module.RemoveFiles(deletedItems, false);

                foreach (var childId in selectedItems.SecondIds())
                {
                    var itemsToCheckout = selectedItems
                        .Where(item => !item.Item.IsDeleted && item.SecondRevision.ObjectId == childId)
                        .Select(item => item.Item.Name).ToList();
                    Module.CheckoutFiles(itemsToCheckout, childId, force: false);
                }
            }
            else
            {
                // acts as parent
                // if file is new to the parent or is copied, it has to be removed
                var addedItems = selectedItems
                    .Where(item => item.Item.IsNew || item.Item.IsCopied)
                    .Select(item => item.Item.Name).ToList();
                Module.RemoveFiles(addedItems, false);

                foreach (var parentId in selectedItems.FirstIds())
                {
                    var itemsToCheckout = selectedItems
                        .Where(item => !item.Item.IsNew && item.FirstRevision?.ObjectId == parentId)
                        .Select(item => item.Item.Name).ToList();
                    Module.CheckoutFiles(itemsToCheckout, parentId, force: false);
                }
            }

            RefreshArtificial();
        }

        private async Task ShowSelectedFileDiffAsync()
        {
            await DiffText.ViewChangesAsync(DiffFiles.SelectedItem,
                openWithDiffTool: () => firstToSelectedToolStripMenuItem.PerformClick());
        }

        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ShowSelectedFileDiffAsync();
            }).FileAndForget();
        }

        private void DiffFiles_DoubleClick(object sender, EventArgs e)
        {
            FileStatusItem item = DiffFiles.SelectedItem;
            if (item == null || !item.Item.IsTracked)
            {
                return;
            }

            if (AppSettings.OpenSubmoduleDiffInSeparateWindow && item.Item.IsSubmodule)
            {
                var submoduleName = item.Item.Name;

                ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        var status = await item.Item.GetSubmoduleStatusAsync().ConfigureAwait(false);

                        var process = new Process
                        {
                            StartInfo =
                            {
                                FileName = Application.ExecutablePath,
                                Arguments = "browse -commit=" + status.Commit,
                                WorkingDirectory = _fullPathResolver.Resolve(submoduleName.EnsureTrailingPathSeparator())
                            }
                        };

                        process.Start();
                    });
            }
            else
            {
                UICommands.StartFileHistoryDialog(this, item.Item.Name, item.SecondRevision);
            }
        }

        private void DiffFiles_DataSourceChanged(object sender, EventArgs e)
        {
            if (DiffFiles.GitItemStatuses == null || !DiffFiles.GitItemStatuses.Any())
            {
                DiffText.Clear();
            }
        }

        private void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ShowSelectedFileDiffAsync();
            }).FileAndForget();
        }

        private void diffShowInFileTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // switch to view (and fills the first level of file tree data model if not already done)
            (FindForm() as FormBrowse)?.ExecuteCommand(FormBrowse.Command.FocusFileTree);
            _revisionFileTree.ExpandToFile(DiffFiles.SelectedItems.First().Item.Name);
        }

        private void UpdateStatusOfMenuItems()
        {
            var selectionInfo = GetSelectionInfo();

            // Many options have no meaning for artificial commits or submodules
            // Hide the obviously no action options when single selected, handle them in actions if multi select

            // open submodule is added in FileStatusList
            fileHistoryDiffToolstripMenuItem.Font = (DiffFiles.SelectedItem?.Item.IsSubmodule ?? false) && AppSettings.OpenSubmoduleDiffInSeparateWindow
                ? new Font(fileHistoryDiffToolstripMenuItem.Font, FontStyle.Regular)
                : new Font(fileHistoryDiffToolstripMenuItem.Font, FontStyle.Bold);

            diffUpdateSubmoduleMenuItem.Visible =
                diffResetSubmoduleChanges.Visible =
                    diffStashSubmoduleChangesToolStripMenuItem.Visible =
                        diffCommitSubmoduleChanges.Visible =
                            submoduleStripSeparator.Visible = _revisionDiffController.ShouldShowSubmoduleMenus(selectionInfo);

            stageFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuStage(selectionInfo);
            unstageFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuUnstage(selectionInfo);
            resetFileToToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowResetFileMenus(selectionInfo);
            cherryPickSelectedDiffFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuCherryPick(selectionInfo);

            diffToolStripSeparator13.Visible = _revisionDiffController.ShouldShowDifftoolMenus(selectionInfo) ||
                                               _revisionDiffController.ShouldShowMenuDeleteFile(selectionInfo) ||
                                               _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo) ||
                                               _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);

            openWithDifftoolToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowDifftoolMenus(selectionInfo);
            diffOpenWorkingDirectoryFileWithToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo);
            diffOpenRevisionFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);
            diffOpenRevisionFileWithToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);
            saveAsToolStripMenuItem1.Visible = _revisionDiffController.ShouldShowMenuSaveAs(selectionInfo);
            diffEditWorkingDirectoryFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo);
            diffDeleteFileToolStripMenuItem.Text = ResourceManager.Strings.GetDeleteFile(selectionInfo.SelectedGitItemCount);
            diffDeleteFileToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuDeleteFile(selectionInfo);
            diffDeleteFileToolStripMenuItem.Visible = diffDeleteFileToolStripMenuItem.Enabled;

            copyFilenameToClipboardToolStripMenuItem1.Enabled = _revisionDiffController.ShouldShowMenuCopyFileName(selectionInfo);
            openContainingFolderToolStripMenuItem.Enabled = false;

            foreach (var item in DiffFiles.SelectedItems)
            {
                string filePath = _fullPathResolver.Resolve(item.Item.Name);
                if (FormBrowseUtil.FileOrParentDirectoryExists(filePath))
                {
                    openContainingFolderToolStripMenuItem.Enabled = true;
                    break;
                }
            }

            // Visibility of FileTree is not known, assume (CommitInfoTabControl.Contains(TreeTabPage);)
            diffShowInFileTreeToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuShowInFileTree(selectionInfo);
            fileHistoryDiffToolstripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuFileHistory(selectionInfo);
            blameToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuBlame(selectionInfo);
        }

        private void DiffContextMenu_Opening(object sender, CancelEventArgs e)
        {
            UpdateStatusOfMenuItems();
        }

        private void blameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileStatusItem item = DiffFiles.SelectedItem;
            if (item == null || !item.Item.IsTracked)
            {
                return;
            }

            UICommands.StartFileHistoryDialog(this, item.Item.Name, item.SecondRevision, true, true);
        }

        private void StageFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            var files = DiffFiles.SelectedItems.Where(item => item.Item.Staged == StagedStatus.WorkTree).Select(i => i.Item).ToList();

            Module.StageFiles(files, out _);
            RefreshArtificial();
        }

        private void UnstageFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            Module.BatchUnstageFiles(DiffFiles.SelectedItems.Where(item => item.Item.Staged == StagedStatus.Index).Select(i => i.Item).ToList());
            RefreshArtificial();
        }

        private void cherryPickSelectedDiffFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DiffText.CherryPickAllChanges();
        }

        private void copyFilenameToClipboardToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FormBrowse.CopyFullPathToClipboard(DiffFiles, Module);
        }

        private void findInDiffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var candidates = DiffFiles.GitItemStatuses;

            IEnumerable<GitItemStatus> FindDiffFilesMatches(string name)
            {
                var predicate = _findFilePredicateProvider.Get(name, Module.WorkingDir);
                return candidates.Where(item => predicate(item.Name) || predicate(item.OldName));
            }

            GitItemStatus selectedItem;
            using (var searchWindow = new SearchWindow<GitItemStatus>(FindDiffFilesMatches)
            {
                Owner = FindForm()
            })
            {
                searchWindow.ShowDialog(this);
                selectedItem = searchWindow.SelectedItem;
            }

            if (selectedItem != null)
            {
                DiffFiles.SelectedGitItem = selectedItem;
            }
        }

        private void fileHistoryDiffToolstripMenuItem_Click(object sender, EventArgs e)
        {
            FileStatusItem item = DiffFiles.SelectedItem;
            if (item == null || !item.Item.IsTracked)
            {
                return;
            }

            UICommands.StartFileHistoryDialog(this, item.Item.Name, item.SecondRevision);
        }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormBrowse.OpenContainingFolder(DiffFiles, Module);
        }

        private void openWithDifftoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RevisionDiffKind diffKind;

            if (sender == firstToLocalToolStripMenuItem)
            {
                diffKind = RevisionDiffKind.DiffALocal;
            }
            else if (sender == selectedToLocalToolStripMenuItem)
            {
                diffKind = RevisionDiffKind.DiffBLocal;
            }
            else if (sender == firstParentToLocalToolStripMenuItem)
            {
                diffKind = RevisionDiffKind.DiffAParentLocal;
            }
            else if (sender == selectedParentToLocalToolStripMenuItem)
            {
                diffKind = RevisionDiffKind.DiffBParentLocal;
            }
            else
            {
                diffKind = RevisionDiffKind.DiffAB;
            }

            foreach (var item in DiffFiles.SelectedItems)
            {
                if (item.FirstRevision?.ObjectId == ObjectId.CombinedDiffId)
                {
                    // CombinedDiff cannot be viewed in a difftool
                    // Disabled in menus but can be activated from shortcuts, just ignore
                    continue;
                }

                // If item.FirstRevision is null, compare to root commit
                GitRevision[] revs = new[] { item.SecondRevision, item.FirstRevision };
                UICommands.OpenWithDifftool(this, revs, item.Item.Name, item.Item.OldName, diffKind, item.Item.IsTracked);
            }
        }

        private void diffEditWorkingDirectoryFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null)
            {
                return;
            }

            var fileName = _fullPathResolver.Resolve(DiffFiles.SelectedItem.Item.Name);
            UICommands.StartFileEditorDialog(fileName);
            RefreshArtificial();
        }

        private void diffOpenWorkingDirectoryFileWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null)
            {
                return;
            }

            var fileName = _fullPathResolver.Resolve(DiffFiles.SelectedItem.Item.Name);
            OsShellUtil.OpenAs(fileName.ToNativePath());
        }

        private void diffOpenRevisionFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveSelectedItemToTempFile(fileName => Process.Start(fileName));
        }

        private void diffOpenRevisionFileWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveSelectedItemToTempFile(OsShellUtil.OpenAs);
        }

        private void SaveSelectedItemToTempFile(Action<string> onSaved)
        {
            var item = DiffFiles.SelectedItem;
            if (item?.Item?.Name == null || item.SecondRevision == null)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;

                var blob = Module.GetFileBlobHash(item.Item.Name, item.SecondRevision.ObjectId);

                if (blob == null)
                {
                    return;
                }

                var fileName = PathUtil.GetFileName(item.Item.Name);
                fileName = (Path.GetTempPath() + fileName).ToNativePath();
                Module.SaveBlobAs(fileName, blob.ToString());

                onSaved(fileName);
            }).FileAndForget();
        }

        private ContextMenuDiffToolInfo GetContextMenuDiffToolInfo()
        {
            // Some items are not supported if more than one revision is selected
            var revisions = DiffFiles.SelectedItems.SecondRevs().ToList();
            var selectedRev = revisions.Count() != 1 ? null : revisions.FirstOrDefault();

            var parentIds = DiffFiles.SelectedItems.FirstIds().ToList();
            bool firstIsParent = _gitRevisionTester.AllFirstAreParentsToSelected(parentIds, selectedRev);
            bool localExists = _gitRevisionTester.AnyLocalFileExists(DiffFiles.SelectedItems.Select(i => i.Item));

            bool allAreNew = DiffFiles.SelectedItems.All(i => i.Item.IsNew);
            bool allAreDeleted = DiffFiles.SelectedItems.All(i => i.Item.IsDeleted);

            return new ContextMenuDiffToolInfo(
                selectedRevision: selectedRev,
                selectedItemParentRevs: parentIds,
                allAreNew: allAreNew,
                allAreDeleted: allAreDeleted,
                firstIsParent: firstIsParent,
                firstParentsValid: _revisionGrid.IsFirstParentValid(),
                localExists: localExists);
        }

        private void openWithDifftoolToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ContextMenuDiffToolInfo selectionInfo = GetContextMenuDiffToolInfo();
            var revisions = DiffFiles.SelectedItems.Select(item => item.SecondRevision).Distinct().ToList();

            if (revisions.Any())
            {
                selectedDiffCaptionMenuItem.Text = _selectedRevision + (DescribeRevision(revisions) ?? string.Empty);
                selectedDiffCaptionMenuItem.Visible = true;
                MenuUtil.SetAsCaptionMenuItem(selectedDiffCaptionMenuItem, DiffContextMenu);

                firstDiffCaptionMenuItem.Text = _firstRevision.Text +
                                                (DescribeRevision(DiffFiles.SelectedItems.FirstRevs().ToList()) ?? string.Empty);
                firstDiffCaptionMenuItem.Visible = true;
                MenuUtil.SetAsCaptionMenuItem(firstDiffCaptionMenuItem, DiffContextMenu);
            }
            else
            {
                firstDiffCaptionMenuItem.Visible = false;
                selectedDiffCaptionMenuItem.Visible = false;
            }

            firstToSelectedToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuFirstToSelected(selectionInfo);
            firstToLocalToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuFirstToLocal(selectionInfo);
            selectedToLocalToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuSelectedToLocal(selectionInfo);
            firstParentToLocalToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuFirstParentToLocal(selectionInfo);
            selectedParentToLocalToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuSelectedParentToLocal(selectionInfo);
            firstParentToLocalToolStripMenuItem.Visible = _revisionDiffContextMenuController.ShouldDisplayMenuFirstParentToLocal(selectionInfo);
            selectedParentToLocalToolStripMenuItem.Visible = _revisionDiffContextMenuController.ShouldDisplayMenuSelectedParentToLocal(selectionInfo);
        }

        private void resetFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetSelectedItemsTo(sender == resetFileToSelectedToolStripMenuItem);
        }

        /// <summary>
        /// Checks if it is possible to reset to the revision.
        /// For artificial is Index is possible but not WorkTree or Combined
        /// </summary>
        /// <param name="guid">The Git objectId</param>
        /// <returns>If it is possible to reset to the revisions</returns>
        private bool CanResetToRevision(ObjectId guid)
        {
            return guid != ObjectId.WorkTreeId
                   && guid != ObjectId.CombinedDiffId;
        }

        private void resetFileToToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var items = DiffFiles.SelectedItems;
            var selectedIds = items.SecondIds().ToList();
            if (selectedIds.Count == 0)
            {
                resetFileToSelectedToolStripMenuItem.Visible = false;
                resetFileToParentToolStripMenuItem.Visible = false;
                return;
            }

            if (selectedIds.Count != 1 || !CanResetToRevision(selectedIds.FirstOrDefault()))
            {
                resetFileToSelectedToolStripMenuItem.Visible = false;
            }
            else
            {
                resetFileToSelectedToolStripMenuItem.Visible = true;
                resetFileToSelectedToolStripMenuItem.Text =
                    _selectedRevision + DescribeRevision(selectedIds.FirstOrDefault(), 50);
            }

            var parentIds = DiffFiles.SelectedItems.FirstIds().ToList();
            if (parentIds.Count != 1 || !CanResetToRevision(parentIds.FirstOrDefault()))
            {
                resetFileToParentToolStripMenuItem.Visible = false;
            }
            else
            {
                resetFileToParentToolStripMenuItem.Visible = true;
                resetFileToParentToolStripMenuItem.Text =
                    _firstRevision + DescribeRevision(parentIds.FirstOrDefault(), 50);
            }
        }

        private void saveAsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FileStatusItem item = DiffFiles.SelectedItem;
            if (item == null)
            {
                return;
            }

            var fullName = _fullPathResolver.Resolve(item.Item.Name);
            using (var fileDialog =
                new SaveFileDialog
                {
                    InitialDirectory = Path.GetDirectoryName(fullName),
                    FileName = Path.GetFileName(fullName),
                    DefaultExt = Path.GetExtension(fullName),
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
                    Module.SaveBlobAs(fileDialog.FileName, $"{item.SecondRevision.Guid}:\"{item.Item.Name}\"");
                }
            }
        }

        private bool DeleteSelectedFiles()
        {
            try
            {
                var selected = DiffFiles.SelectedItem;
                if (selected == null || !selected.SecondRevision.IsArtificial ||
                    MessageBox.Show(this, _deleteSelectedFiles.Text, _deleteSelectedFilesCaption.Text, MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) !=
                    DialogResult.Yes)
                {
                    return false;
                }

                // If any file is staged, it must be unstaged
                Module.BatchUnstageFiles(DiffFiles.SelectedItems.Where(item => item.Item.Staged == StagedStatus.Index).Select(item => item.Item));

                DiffFiles.StoreNextIndexToSelect();
                var items = DiffFiles.SelectedItems.Where(item => !item.Item.IsSubmodule);
                foreach (var item in items)
                {
                    var path = _fullPathResolver.Resolve(item.Item.Name);
                    bool isDir = (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
                    if (isDir)
                    {
                        Directory.Delete(path, recursive: true);
                    }
                    else
                    {
                        File.Delete(path);
                    }
                }

                RefreshArtificial();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _deleteFailed.Text + Environment.NewLine + ex.Message, Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void diffDeleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelectedFiles();
        }

        private void diffCommitSubmoduleChanges_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.Item.IsSubmodule).Select(it => it.Item.Name).Distinct().ToList();

            foreach (var name in submodules)
            {
                var submodulCommands = new GitUICommands(_fullPathResolver.Resolve(name.EnsureTrailingPathSeparator()));
                submodulCommands.StartCommitDialog(this);
            }

            RefreshArtificial();
        }

        private void diffResetSubmoduleChanges_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.Item.IsSubmodule).Select(it => it.Item.Name).Distinct().ToList();

            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(this, true, true);
            if (resetType == FormResetChanges.ActionEnum.Cancel)
            {
                return;
            }

            foreach (var name in submodules)
            {
                GitModule module = Module.GetSubmodule(name);

                // Reset all changes.
                module.Reset(ResetMode.Hard);

                // Also delete new files, if requested.
                if (resetType == FormResetChanges.ActionEnum.ResetAndDelete)
                {
                    module.Clean(CleanMode.OnlyNonIgnored, directories: true);
                }
            }

            RefreshArtificial();
        }

        private void diffUpdateSubmoduleMenuItem_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.Item.IsSubmodule).Select(it => it.Item.Name).Distinct().ToList();

            FormProcess.ShowDialog(FindForm() as FormBrowse, GitCommandHelpers.SubmoduleUpdateCmd(submodules));
            RefreshArtificial();
        }

        private void diffStashSubmoduleChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.Item.IsSubmodule).Select(it => it.Item.Name).Distinct().ToList();

            foreach (var name in submodules)
            {
                var uiCmds = new GitUICommands(Module.GetSubmodule(name));
                uiCmds.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash);
            }

            RefreshArtificial();
        }

        public void SwitchFocus(bool alreadyContainedFocus)
        {
            if (alreadyContainedFocus && DiffFiles.Focused)
            {
                DiffText.Focus();
            }
            else
            {
                DiffFiles.Focus();
            }
        }
    }
}
