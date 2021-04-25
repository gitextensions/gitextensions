using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Commands;
using GitExtUtils;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.HelperDialogs;
using GitUI.Hotkey;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class RevisionDiffControl : GitModuleControl
    {
        private readonly TranslationString _saveFileFilterCurrentFormat = new("Current format");
        private readonly TranslationString _saveFileFilterAllFiles = new("All files");
        private readonly TranslationString _deleteSelectedFilesCaption = new("Delete");
        private readonly TranslationString _deleteSelectedFiles =
            new TranslationString("Are you sure you want to delete the selected file(s)?");
        private readonly TranslationString _deleteFailed = new("Delete file failed");
        private readonly TranslationString _multipleDescription = new("<multiple>");
        private readonly TranslationString _selectedRevision = new("Second: b/");
        private readonly TranslationString _firstRevision = new("First: a/");

        private readonly TranslationString _resetSelectedChangesText =
            new TranslationString("Are you sure you want to reset all selected files to {0}?");

        private RevisionGridControl? _revisionGrid;
        private RevisionFileTreeControl? _revisionFileTree;
        private readonly IRevisionDiffController _revisionDiffController = new RevisionDiffController();
        private readonly IFileStatusListContextMenuController _revisionDiffContextMenuController;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly IFindFilePredicateProvider _findFilePredicateProvider;
        private readonly IGitRevisionTester _gitRevisionTester;
        private readonly RememberFileContextMenuController _rememberFileContextMenuController
            = RememberFileContextMenuController.Default;
        private Action? _refreshGitStatus;

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
            DiffText.LinePatchingBlocksUntilReload = true;
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

        public void UICommands_PostRepositoryChanged(object sender, GitUIEventArgs e)
        {
            _rememberFileContextMenuController.RememberedDiffFileItem = null;
        }

        public void RefreshArtificial()
        {
            if (!Visible)
            {
                return;
            }

            Validates.NotNull(_revisionGrid);
            var revisions = _revisionGrid.GetSelectedRevisions();
            if (!revisions.Any(r => r.IsArtificial))
            {
                return;
            }

            DiffFiles.StoreNextIndexToSelect();
            SetDiffs(revisions);
            if (DiffFiles.SelectedItem is null)
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
            OpenWithDifftoolSelectedToLocal = 8,
            ResetSelectedFiles = 9,
            StageSelectedFile = 10,
            UnStageSelectedFile = 11,
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
                case Command.OpenWithDifftool: OpenFilesWithDiffTool(RevisionDiffKind.DiffAB); break;
                case Command.OpenWithDifftoolFirstToLocal: OpenFilesWithDiffTool(RevisionDiffKind.DiffALocal); break;
                case Command.OpenWithDifftoolSelectedToLocal: OpenFilesWithDiffTool(RevisionDiffKind.DiffBLocal); break;
                case Command.EditFile: diffEditWorkingDirectoryFileToolStripMenuItem.PerformClick(); break;
                case Command.OpenAsTempFile: diffOpenRevisionFileToolStripMenuItem.PerformClick(); break;
                case Command.OpenAsTempFileWith: diffOpenRevisionFileWithToolStripMenuItem.PerformClick(); break;
                case Command.ResetSelectedFiles: return ResetSelectedFilesWithConfirmation();
                case Command.StageSelectedFile: return StageSelectedFiles();
                case Command.UnStageSelectedFile: return UnstageSelectedFiles();

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
            resetFileToParentToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.ResetSelectedFiles);
            stageFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.StageSelectedFile);
            unstageFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.UnStageSelectedFile);

            DiffText.ReloadHotkeys();
        }

        public void LoadCustomDifftools()
        {
            List<CustomDiffMergeTool> menus = new()
            {
                new(firstToSelectedToolStripMenuItem, firstToSelectedToolStripMenuItem_Click),
                new(selectedToLocalToolStripMenuItem, selectedToLocalToolStripMenuItem_Click),
                new(firstToLocalToolStripMenuItem, firstToLocalToolStripMenuItem_Click),
                new(diffWithRememberedDifftoolToolStripMenuItem, diffWithRememberedDiffToolToolStripMenuItem_Click),
                new(diffTwoSelectedDifftoolToolStripMenuItem, diffTwoSelectedDiffToolToolStripMenuItem_Click)
            };

            new CustomDiffMergeToolProvider().LoadCustomDiffMergeTools(Module, menus, components, isDiff: true);
        }

        private string GetShortcutKeyDisplayString(Command cmd)
        {
            return GetShortcutKeys((int)cmd).ToShortcutKeyDisplayString();
        }

        #endregion

        public void DisplayDiffTab(IReadOnlyList<GitRevision> revisions)
        {
            SetDiffs(revisions);
            if (DiffFiles.SelectedItem is null)
            {
                DiffFiles.SelectFirstVisibleItem();
            }
        }

        private void SetDiffs(IReadOnlyList<GitRevision> revisions)
        {
            Validates.NotNull(_revisionGrid);

            var item = DiffFiles.SelectedItem;
            var oldDiffItem = DiffFiles.FirstGroupItems.Contains(item) ? item : null;
            DiffFiles.SetDiffs(revisions, _revisionGrid.GetRevision);

            // Try to restore previous item
            if (oldDiffItem is not null && DiffFiles.FirstGroupItems.Any(i => i.Item.Name.Equals(oldDiffItem.Item.Name)))
            {
                DiffFiles.SelectedGitItem = oldDiffItem.Item;
            }
        }

        public void Bind(RevisionGridControl revisionGrid, RevisionFileTreeControl revisionFileTree, Action refreshGitStatus)
        {
            _revisionGrid = revisionGrid;
            _revisionFileTree = revisionFileTree;
            _refreshGitStatus = refreshGitStatus;
        }

        public void InitSplitterManager(SplitterManager splitterManager)
        {
            splitterManager.AddSplitter(DiffSplitContainer, "DiffSplitContainer");
        }

        protected override void OnRuntimeLoad()
        {
            DiffFiles.DescribeRevision = objectId => DescribeRevision(objectId);
            DiffText.SetFileLoader(GetNextPatchFile);
            DiffText.Font = AppSettings.FixedWidthFont;
            ReloadHotkeys();
            LoadCustomDifftools();

            base.OnRuntimeLoad();
        }

        private string DescribeRevision(ObjectId? objectId, int maxLength = 0)
        {
            if (objectId is null)
            {
                // No parent at all, present as working directory
                return ResourceManager.TranslatedStrings.Workspace;
            }

            Validates.NotNull(_revisionGrid);

            var revision = _revisionGrid.GetRevision(objectId);

            if (revision is null)
            {
                return objectId.ToShortString();
            }

            return _revisionGrid.DescribeRevision(revision, maxLength);
        }

        /// <summary>
        /// Provide a description for the first selected or parent to the "primary" selected last.
        /// </summary>
        /// <returns>A description of the selected parent.</returns>
        private string? DescribeRevision(List<GitRevision> parents)
        {
            return parents.Count switch
            {
                1 => DescribeRevision(parents.FirstOrDefault()?.ObjectId, 50),
                > 1 => _multipleDescription.Text,
                _ => null
            };
        }

        private bool GetNextPatchFile(bool searchBackward, bool loop, out int fileIndex, out Task loadFileContent)
        {
            fileIndex = -1;
            loadFileContent = Task.CompletedTask;
            if (DiffFiles.SelectedItem is null)
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
            var selectedRev = revisions.Count != 1 ? null : revisions.FirstOrDefault();

            // First (A) is parent if one revision selected or if parent, then selected
            var parentIds = selectedItems.FirstIds().ToList();

            // Combined diff, range diff etc are for display only, no manipulations
            bool isStatusOnly = selectedItems.Any(item => item.Item.IsRangeDiff || item.Item.IsStatusOnly);
            bool isDisplayOnlyDiff = parentIds.Contains(ObjectId.CombinedDiffId) || isStatusOnly;
            int selectedGitItemCount = selectedItems.Count();

            // No changes to files in bare repos
            bool isBareRepository = Module.IsBareRepository();
            bool isAnyTracked = selectedItems.Any(item => item.Item.IsTracked);
            bool isAnyIndex = selectedItems.Any(item => item.Item.Staged == StagedStatus.Index);
            bool isAnyWorkTree = selectedItems.Any(item => item.Item.Staged == StagedStatus.WorkTree);
            bool supportPatches = selectedGitItemCount == 1 && DiffText.SupportLinePatching;
            bool isDeleted = selectedItems.Any(item => item.Item.IsDeleted);
            bool isAnySubmodule = selectedItems.Any(item => item.Item.IsSubmodule);
            (bool allFilesExist, bool allDirectoriesExist, bool allFilesOrUntrackedDirectoriesExist) = FileOrUntrackedDirExists(selectedItems, _fullPathResolver);

            var selectionInfo = new ContextMenuSelectionInfo(
                selectedRevision: selectedRev,
                isDisplayOnlyDiff: isDisplayOnlyDiff,
                isStatusOnly: isStatusOnly,
                selectedGitItemCount: selectedGitItemCount,
                isAnyItemIndex: isAnyIndex,
                isAnyItemWorkTree: isAnyWorkTree,
                isBareRepository: isBareRepository,
                allFilesExist: allFilesExist,
                allDirectoriesExist: allDirectoriesExist,
                allFilesOrUntrackedDirectoriesExist: allFilesOrUntrackedDirectoriesExist,
                isAnyTracked: isAnyTracked,
                supportPatches: supportPatches,
                isDeleted: isDeleted,
                isAnySubmodule: isAnySubmodule);
            return selectionInfo;

            static (bool allFilesExist, bool allDirectoriesExist, bool allFilesOrUntrackedDirectoriesExist) FileOrUntrackedDirExists(List<FileStatusItem> items, IFullPathResolver fullPathResolver)
            {
                bool allFilesExist = items.Any();
                bool allDirectoriesExist = allFilesExist;
                bool allFilesOrUntrackedDirectoriesExist = allFilesExist;
                foreach (var item in items)
                {
                    var path = fullPathResolver.Resolve(item.Item.Name);
                    var fileExists = File.Exists(path);
                    var directoryExists = Directory.Exists(path);
                    allFilesExist &= fileExists;
                    allDirectoriesExist &= directoryExists;
                    var fileOrUntrackedDirectoryExists = fileExists || (!item.Item.IsTracked && allDirectoriesExist);
                    allFilesOrUntrackedDirectoriesExist &= fileOrUntrackedDirectoryExists;

                    if (!allFilesExist && !allDirectoriesExist && !allFilesOrUntrackedDirectoriesExist)
                    {
                        break;
                    }
                }

                return (allFilesExist, allDirectoriesExist, allFilesOrUntrackedDirectoriesExist);
            }
        }

        private void RequestRefresh()
        {
            // Request immediate update of commit count, no delay due to backoff
            // If a file system change was triggered too, the requests should be merged
            // (this will also update the count if only worktree<->index is changed)
            // This may trigger a second RefreshArtificial()
            _refreshGitStatus?.Invoke();

            RefreshArtificial();
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

            RequestRefresh();
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
            FileStatusItem? item = DiffFiles.SelectedItem;
            if (item is null || !item.Item.IsTracked)
            {
                return;
            }

            if (AppSettings.OpenSubmoduleDiffInSeparateWindow && item.Item.IsSubmodule)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        await DiffFiles.OpenSubmoduleAsync();
                    });
            }
            else
            {
                UICommands.StartFileHistoryDialog(this, item.Item.Name, item.SecondRevision);
            }
        }

        private void DiffFiles_DataSourceChanged(object sender, EventArgs e)
        {
            if (DiffFiles.GitItemStatuses is null || !DiffFiles.GitItemStatuses.Any())
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

        private void DiffText_PatchApplied(object sender, EventArgs e)
        {
            RequestRefresh();
        }

        private void diffShowInFileTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Validates.NotNull(_revisionFileTree);

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
            cherryPickSelectedDiffFileToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuCherryPick(selectionInfo);

            diffToolStripSeparator13.Visible = _revisionDiffController.ShouldShowDifftoolMenus(selectionInfo) ||
                                               _revisionDiffController.ShouldShowMenuDeleteFile(selectionInfo) ||
                                               _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo) ||
                                               _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);

            openWithDifftoolToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowDifftoolMenus(selectionInfo);
            diffOpenWorkingDirectoryFileWithToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo);
            diffOpenRevisionFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);
            diffOpenRevisionFileWithToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);
            saveAsToolStripMenuItem1.Visible = _revisionDiffController.ShouldShowMenuSaveAs(selectionInfo);
            openContainingFolderToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuShowInFolder(selectionInfo);
            diffEditWorkingDirectoryFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo);
            diffDeleteFileToolStripMenuItem.Text = ResourceManager.TranslatedStrings.GetDeleteFile(selectionInfo.SelectedGitItemCount);
            diffDeleteFileToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuDeleteFile(selectionInfo);
            diffDeleteFileToolStripMenuItem.Visible = diffDeleteFileToolStripMenuItem.Enabled;

            copyFilenameToClipboardToolStripMenuItem1.Enabled = _revisionDiffController.ShouldShowMenuCopyFileName(selectionInfo);
            openContainingFolderToolStripMenuItem.Enabled = false;

            foreach (var item in DiffFiles.SelectedItems)
            {
                string? filePath = _fullPathResolver.Resolve(item.Item.Name);
                if (filePath is not null && FormBrowseUtil.FileOrParentDirectoryExists(filePath))
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
            FileStatusItem? item = DiffFiles.SelectedItem;
            if (item is null || !item.Item.IsTracked)
            {
                return;
            }

            UICommands.StartFileHistoryDialog(this, item.Item.Name, item.SecondRevision, true, true);
        }

        private void StageFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            StageFiles();
        }

        private void StageFiles()
        {
            var files = DiffFiles.SelectedItems.Where(item => item.Item.Staged == StagedStatus.WorkTree).Select(i => i.Item).ToList();

            Module.StageFiles(files, out _);
            RequestRefresh();
        }

        private void UnstageFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            UnstageFiles();
        }

        private void UnstageFiles()
        {
            Module.BatchUnstageFiles(DiffFiles.SelectedItems.Where(item => item.Item.Staged == StagedStatus.Index).Select(i => i.Item).ToList());
            RequestRefresh();
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

            GitItemStatus? selectedItem;
            using (var searchWindow = new SearchWindow<GitItemStatus>(FindDiffFilesMatches)
            {
                Owner = FindForm()
            })
            {
                searchWindow.ShowDialog(this);
                selectedItem = searchWindow.SelectedItem;
            }

            if (selectedItem is not null)
            {
                DiffFiles.SelectedGitItem = selectedItem;
            }
        }

        private void fileHistoryDiffToolstripMenuItem_Click(object sender, EventArgs e)
        {
            FileStatusItem? item = DiffFiles.SelectedItem;
            if (item is null || !item.Item.IsTracked)
            {
                return;
            }

            UICommands.StartFileHistoryDialog(this, item.Item.Name, item.SecondRevision);
        }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormBrowse.OpenContainingFolder(DiffFiles, Module);
        }

        private void firstToSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFilesWithDiffTool(RevisionDiffKind.DiffAB, sender);
        }

        private void selectedToLocalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFilesWithDiffTool(RevisionDiffKind.DiffBLocal, sender);
        }

        private void firstToLocalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFilesWithDiffTool(RevisionDiffKind.DiffALocal, sender);
        }

        private void OpenFilesWithDiffTool(RevisionDiffKind diffKind, object sender)
        {
            var item = sender as ToolStripMenuItem;
            if (item?.DropDownItems != null)
            {
                // "main menu" clicked, cancel dropdown manually, invoke default mergetool
                item.HideDropDown();
                item.Owner.Hide();
            }

            var toolName = item?.Tag as string;
            OpenFilesWithDiffTool(diffKind, toolName);
        }

        private void OpenFilesWithDiffTool(RevisionDiffKind diffKind, string? toolName = null)
        {
            using (WaitCursorScope.Enter())
            {
                foreach (var item in DiffFiles.SelectedItems)
                {
                    if (item.FirstRevision?.ObjectId == ObjectId.CombinedDiffId)
                    {
                        // CombinedDiff cannot be viewed in a difftool
                        // Disabled in menus but can be activated from shortcuts, just ignore
                        continue;
                    }

                    // If item.FirstRevision is null, compare to root commit
                    GitRevision?[] revs = { item.SecondRevision, item.FirstRevision };
                    UICommands.OpenWithDifftool(this, revs, item.Item.Name, item.Item.OldName, diffKind, item.Item.IsTracked, customTool: toolName);
                }
            }
        }

        private void diffTwoSelectedDiffToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item?.DropDownItems != null)
            {
                // "main menu" clicked, cancel dropdown manually, invoke default difftool
                item.HideDropDown();
            }

            var toolName = item?.Tag as string;
            var diffFiles = DiffFiles.SelectedItems.ToList();
            if (diffFiles.Count != 2)
            {
                return;
            }

            // The order is always the order in the list, not clicked order, but the (last) selected is known
            var firstIndex = DiffFiles.SelectedItem == diffFiles[0] ? 1 : 0;

            // Fallback to first revision if second revision cannot be used
            var isFirstItemSecondRev = _rememberFileContextMenuController.ShouldEnableFirstItemDiff(diffFiles[firstIndex], isSecondRevision: true);
            var first = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash, diffFiles[firstIndex], isSecondRevision: isFirstItemSecondRev);
            var isSecondItemSecondRev = _rememberFileContextMenuController.ShouldEnableSecondItemDiff(DiffFiles.SelectedItem, isSecondRevision: true);
            var second = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash, DiffFiles.SelectedItem, isSecondRevision: isSecondItemSecondRev);

            Module.OpenFilesWithDifftool(first, second, customTool: toolName);
        }

        private void diffWithRememberedDiffToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item?.DropDownItems != null)
            {
                // "main menu" clicked, cancel dropdown manually, invoke default difftool
                item.HideDropDown();
            }

            string? toolName = item?.Tag as string;

            // For first item, the second revision is explicitly remembered
            var first = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash,
                _rememberFileContextMenuController.RememberedDiffFileItem, isSecondRevision: true);

            // Fallback to first revision if second cannot be used
            var isSecond = _rememberFileContextMenuController.ShouldEnableSecondItemDiff(DiffFiles.SelectedItem, isSecondRevision: true);
            var second = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash, DiffFiles.SelectedItem, isSecondRevision: isSecond);

            Module.OpenFilesWithDifftool(first, second, customTool: toolName);
        }

        private void rememberSecondDiffToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _rememberFileContextMenuController.RememberedDiffFileItem = DiffFiles.SelectedItem;
        }

        private void rememberFirstDiffToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem?.FirstRevision is null)
            {
                return;
            }

            var item = new FileStatusItem(
                firstRev: DiffFiles.SelectedItem.SecondRevision,
                secondRev: DiffFiles.SelectedItem.FirstRevision,
                item: DiffFiles.SelectedItem.Item);
            if (!Strings.IsNullOrWhiteSpace(DiffFiles.SelectedItem.Item.OldName))
            {
                var name = DiffFiles.SelectedItem.Item.OldName;
                DiffFiles.SelectedItem.Item.OldName = DiffFiles.SelectedItem.Item.Name;
                DiffFiles.SelectedItem.Item.Name = name;
            }

            _rememberFileContextMenuController.RememberedDiffFileItem = item;
        }

        private void diffEditWorkingDirectoryFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem is null)
            {
                return;
            }

            var fileName = _fullPathResolver.Resolve(DiffFiles.SelectedItem.Item.Name);
            UICommands.StartFileEditorDialog(fileName);
            RequestRefresh();
        }

        private void diffOpenWorkingDirectoryFileWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem is null)
            {
                return;
            }

            var fileName = _fullPathResolver.Resolve(DiffFiles.SelectedItem.Item.Name);

            if (fileName is not null)
            {
                OsShellUtil.OpenAs(fileName.ToNativePath());
            }
        }

        private void diffOpenRevisionFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveSelectedItemToTempFile(fileName => OsShellUtil.Open(fileName));
        }

        private void diffOpenRevisionFileWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveSelectedItemToTempFile(OsShellUtil.OpenAs);
        }

        private void SaveSelectedItemToTempFile(Action<string> onSaved)
        {
            var item = DiffFiles.SelectedItem;
            if (item?.Item.Name is null)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;

                var blob = Module.GetFileBlobHash(item.Item.Name, item.SecondRevision.ObjectId);

                if (blob is null)
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
                localExists: localExists);
        }

        private void openWithDifftoolToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ContextMenuDiffToolInfo selectionInfo = GetContextMenuDiffToolInfo();
            var revisions = DiffFiles.SelectedItems.SecondRevs().ToList();

            if (revisions.Any())
            {
                secondDiffCaptionMenuItem.Text = _selectedRevision + (DescribeRevision(revisions) ?? string.Empty);
                secondDiffCaptionMenuItem.Visible = true;
                MenuUtil.SetAsCaptionMenuItem(secondDiffCaptionMenuItem, DiffContextMenu);

                firstDiffCaptionMenuItem.Text = _firstRevision.Text +
                                                (DescribeRevision(DiffFiles.SelectedItems.FirstRevs().ToList()) ?? string.Empty);
                firstDiffCaptionMenuItem.Visible = true;
                MenuUtil.SetAsCaptionMenuItem(firstDiffCaptionMenuItem, DiffContextMenu);
            }
            else
            {
                firstDiffCaptionMenuItem.Visible = false;
                secondDiffCaptionMenuItem.Visible = false;
            }

            firstToSelectedToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuFirstToSelected(selectionInfo);
            firstToLocalToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuFirstToLocal(selectionInfo);
            selectedToLocalToolStripMenuItem.Enabled = _revisionDiffContextMenuController.ShouldShowMenuSelectedToLocal(selectionInfo);

            var diffFiles = DiffFiles.SelectedItems.ToList();
            diffRememberStripSeparator.Visible = diffFiles.Count == 1 || diffFiles.Count == 2;

            // The order is always the order in the list, not clicked order, but the (last) selected is known
            var firstIndex = diffFiles.Count == 2 && DiffFiles.SelectedItem == diffFiles[0] ? 1 : 0;

            diffTwoSelectedDifftoolToolStripMenuItem.Visible = diffFiles.Count == 2;
            diffTwoSelectedDifftoolToolStripMenuItem.Enabled =
                diffFiles.Count == 2
                && _rememberFileContextMenuController.ShouldEnableFirstItemDiff(diffFiles[firstIndex])
                && _rememberFileContextMenuController.ShouldEnableSecondItemDiff(DiffFiles.SelectedItem);

            diffWithRememberedDifftoolToolStripMenuItem.Visible = diffFiles.Count == 1 && _rememberFileContextMenuController.RememberedDiffFileItem is not null;
            diffWithRememberedDifftoolToolStripMenuItem.Enabled =
                diffFiles.Count == 1
                && diffFiles[0] != _rememberFileContextMenuController.RememberedDiffFileItem
                && _rememberFileContextMenuController.ShouldEnableSecondItemDiff(diffFiles[0]);
            diffWithRememberedDifftoolToolStripMenuItem.Text =
                _rememberFileContextMenuController.RememberedDiffFileItem is not null
                    ? string.Format(TranslatedStrings.DiffSelectedWithRememberedFile, _rememberFileContextMenuController.RememberedDiffFileItem.Item.Name)
                    : string.Empty;

            rememberSecondRevDiffToolStripMenuItem.Visible = diffFiles.Count == 1;
            rememberSecondRevDiffToolStripMenuItem.Enabled = diffFiles.Count == 1
                                                                 && _rememberFileContextMenuController.ShouldEnableFirstItemDiff(diffFiles[0], isSecondRevision: true);

            rememberFirstRevDiffToolStripMenuItem.Visible = diffFiles.Count == 1;
            rememberFirstRevDiffToolStripMenuItem.Enabled = diffFiles.Count == 1
                                                                && _rememberFileContextMenuController.ShouldEnableFirstItemDiff(diffFiles[0], isSecondRevision: false);
        }

        private void resetFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetSelectedItemsTo(sender == resetFileToSelectedToolStripMenuItem);
        }

        /// <summary>
        /// Checks if it is possible to reset to the revision.
        /// For artificial is Index is possible but not WorkTree or Combined.
        /// </summary>
        /// <param name="guid">The Git objectId.</param>
        /// <returns>If it is possible to reset to the revisions.</returns>
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
            FileStatusItem? item = DiffFiles.SelectedItem;
            if (item is null)
            {
                return;
            }

            var fullName = _fullPathResolver.Resolve(item.Item.Name);
            using var fileDialog =
                new SaveFileDialog
                {
                    InitialDirectory = Path.GetDirectoryName(fullName),
                    FileName = Path.GetFileName(fullName),
                    DefaultExt = Path.GetExtension(fullName),
                    AddExtension = true
                };
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

        private bool DeleteSelectedFiles()
        {
            try
            {
                var selected = DiffFiles.SelectedItem;
                if (selected is null || !selected.SecondRevision.IsArtificial ||
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
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, recursive: true);
                    }
                    else
                    {
                        File.Delete(path);
                    }
                }

                RequestRefresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _deleteFailed.Text + Environment.NewLine + ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            RequestRefresh();
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

            RequestRefresh();
       }

        private void diffUpdateSubmoduleMenuItem_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.Item.IsSubmodule).Select(it => it.Item.Name).Distinct().ToList();

            FormProcess.ShowDialog(FindForm() as FormBrowse, process: null, arguments: GitCommandHelpers.SubmoduleUpdateCmd(submodules), Module.WorkingDir, input: null, useDialogSettings: true);
            RequestRefresh();
        }

        private void diffStashSubmoduleChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.Item.IsSubmodule).Select(it => it.Item.Name).Distinct().ToList();

            foreach (var name in submodules)
            {
                var uiCmds = new GitUICommands(Module.GetSubmodule(name));
                uiCmds.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash);
            }

            RequestRefresh();
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

        /// <summary>
        /// Hotkey handler.
        /// </summary>
        /// <returns>true if hotkey handled.</returns>
        private bool StageSelectedFiles()
        {
            if (!DiffFiles.Focused)
            {
                return false;
            }

            var selectedIds = DiffFiles.SelectedItems.SecondIds().ToList();
            if (selectedIds.Count != 1 || selectedIds.FirstOrDefault() != ObjectId.WorkTreeId)
            {
                return true;
            }

            StageFiles();
            return true;
        }

        /// <summary>
        /// Hotkey handler.
        /// </summary>
        /// <returns>true if hotkey handled.</returns>
        private bool UnstageSelectedFiles()
        {
            if (!DiffFiles.Focused)
            {
                return false;
            }

            var selectedIds = DiffFiles.SelectedItems.SecondIds().ToList();
            if (selectedIds.Count != 1 || selectedIds.FirstOrDefault() != ObjectId.IndexId)
            {
                return true;
            }

            UnstageFiles();
            return true;
        }

        /// <summary>
        /// Hotkey handler.
        /// </summary>
        /// <returns>true if hotkey handled.</returns>
        private bool ResetSelectedFilesWithConfirmation()
        {
            if (!DiffFiles.Focused)
            {
                return false;
            }

            var parentIds = DiffFiles.SelectedItems.FirstIds().ToList();
            if (parentIds.Count != 1 || !CanResetToRevision(parentIds.FirstOrDefault()))
            {
                return true;
            }

            var rev = _firstRevision.Text + (DescribeRevision(DiffFiles.SelectedItems.FirstRevs().ToList()) ?? string.Empty);
            var text = string.Format(_resetSelectedChangesText.Text, rev);
            if (!MessageBoxes.ConfirmResetSelectedFiles(this, text))
            {
                return true;
            }

            // Reset to first (parent)
            ResetSelectedItemsTo(actsAsChild: false);
            return true;
        }
    }
}
