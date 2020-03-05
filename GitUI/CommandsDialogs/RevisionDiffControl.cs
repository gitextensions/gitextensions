using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.HelperDialogs;
using GitUI.Hotkey;
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

            switch ((Command)cmd)
            {
                case Command.DeleteSelectedFiles: return DeleteSelectedFiles();
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

        public void DisplayDiffTab()
        {
            var revisions = _revisionGrid.GetSelectedRevisions();
            SetDiffs(revisions);
            if (DiffFiles.SelectedItem == null)
            {
                DiffFiles.SelectFirstVisibleItem();
            }
        }

        private void SetDiffs(IReadOnlyList<GitRevision> revisions)
        {
            GitItemStatus oldDiffItem = DiffFiles.SelectedItem;
            DiffFiles.SetDiffs(revisions, _revisionGrid.GetRevision);

            // Try to restore previous item
            if (oldDiffItem != null && DiffFiles.GitItemStatuses.Any(i => i.Name.Equals(oldDiffItem.Name)))
            {
                DiffFiles.SelectedItem = oldDiffItem;
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
            if (DiffFiles.Revision == null)
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
            // First (A) is parent if one revision selected or if parent, then selected
            bool firstIsParent = _gitRevisionTester.AllFirstAreParentsToSelected(DiffFiles.SelectedItemParents, DiffFiles.Revision);

            // Combined diff is a display only diff, no manipulations
            bool isAnyCombinedDiff = DiffFiles.SelectedItemParents.Any(item => item.ObjectId == ObjectId.CombinedDiffId);
            bool isExactlyOneItemSelected = DiffFiles.SelectedItems.Count() == 1;
            bool isAnyItemSelected = DiffFiles.SelectedItems.Any();

            // No changes to files in bare repos
            bool isBareRepository = Module.IsBareRepository();
            bool isAnyTracked = DiffFiles.SelectedItems.Any(item => item.IsTracked);
            bool isAnyIndex = DiffFiles.SelectedItems.Any(item => item.Staged == StagedStatus.Index);
            bool isAnyWorkTree = DiffFiles.SelectedItems.Any(item => item.Staged == StagedStatus.WorkTree);
            bool isAnySubmodule = DiffFiles.SelectedItems.Any(item => item.IsSubmodule);
            bool singleFileExists = isExactlyOneItemSelected && File.Exists(_fullPathResolver.Resolve(DiffFiles.SelectedItem.Name));

            var selectionInfo = new ContextMenuSelectionInfo(DiffFiles.Revision,
                firstIsParent: firstIsParent,
                isAnyCombinedDiff: isAnyCombinedDiff,
                isSingleGitItemSelected: isExactlyOneItemSelected,
                isAnyItemSelected: isAnyItemSelected,
                isAnyItemIndex: isAnyIndex,
                isAnyItemWorkTree: isAnyWorkTree,
                isBareRepository: isBareRepository,
                singleFileExists: singleFileExists,
                isAnyTracked: isAnyTracked,
                isAnySubmodule: isAnySubmodule);
            return selectionInfo;
        }

        private void ResetSelectedItemsTo(bool actsAsChild)
        {
            if (!DiffFiles.SelectedItems.Any())
            {
                return;
            }

            var selectedItems = DiffFiles.SelectedItems;
            if (actsAsChild)
            {
                // selected, all are the same
                var deletedItems = selectedItems.Where(item => item.IsDeleted);
                Module.RemoveFiles(deletedItems.Select(item => item.Name).ToList(), false);

                var itemsToCheckout = selectedItems.Where(item => !item.IsDeleted);
                Module.CheckoutFiles(itemsToCheckout.Select(item => item.Name).ToList(), DiffFiles.Revision.ObjectId, force: false);
            }
            else
            {
                // acts as parent
                // if file is new to the parent or is copied, it has to be removed
                var addedItems = selectedItems.Where(item => item.IsNew || item.IsCopied);
                Module.RemoveFiles(addedItems.Select(item => item.Name).ToList(), false);

                foreach (var parent in DiffFiles.SelectedItemParents)
                {
                    var itemsToCheckout = DiffFiles.SelectedItemsWithParent.Where(item => !item.Item.IsNew && item.ParentRevision.ObjectId == parent.ObjectId);
                    Module.CheckoutFiles(itemsToCheckout.Select(item => item.Item.Name).ToList(), parent.ObjectId, force: false);
                }
            }

            RefreshArtificial();
        }

        private async Task ShowSelectedFileDiffAsync()
        {
            if (DiffFiles.SelectedItem == null || DiffFiles.Revision == null)
            {
                DiffText.Clear();
                return;
            }

            if (DiffFiles.SelectedItemParent?.ObjectId == ObjectId.CombinedDiffId)
            {
                var diffOfConflict = Module.GetCombinedDiffContent(DiffFiles.Revision, DiffFiles.SelectedItem.Name,
                    DiffText.GetExtraDiffArguments(), DiffText.Encoding);

                if (string.IsNullOrWhiteSpace(diffOfConflict))
                {
                    diffOfConflict = Strings.UninterestingDiffOmitted;
                }

                await DiffText.ViewPatchAsync(DiffFiles.SelectedItem.Name,
                    text: diffOfConflict,
                    openWithDifftool: () => firstToSelectedToolStripMenuItem.PerformClick(),
                    isText: DiffFiles.SelectedItem.IsSubmodule);

                return;
            }

            await DiffText.ViewChangesAsync(DiffFiles.SelectedItemParent?.ObjectId, DiffFiles.Revision, DiffFiles.SelectedItem, string.Empty,
                openWithDifftool: () => firstToSelectedToolStripMenuItem.PerformClick());
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
            if (DiffFiles.SelectedItem == null)
            {
                return;
            }

            if (AppSettings.OpenSubmoduleDiffInSeparateWindow && DiffFiles.SelectedItem.IsSubmodule)
            {
                var submoduleName = DiffFiles.SelectedItem.Name;

                ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        var status = await DiffFiles.SelectedItem.GetSubmoduleStatusAsync().ConfigureAwait(false);

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
                UICommands.StartFileHistoryDialog(this, DiffFiles.SelectedItem.Name, DiffFiles.Revision);
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
            _revisionFileTree.ExpandToFile(DiffFiles.SelectedItems.First().Name);
        }

        private void DiffContextMenu_Opening(object sender, CancelEventArgs e)
        {
            var selectionInfo = GetSelectionInfo();

            // Many options have no meaning for artificial commits or submodules
            // Hide the obviously no action options when single selected, handle them in actions if multi select

            openWithDifftoolToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowDifftoolMenus(selectionInfo);
            saveAsToolStripMenuItem1.Visible = _revisionDiffController.ShouldShowMenuSaveAs(selectionInfo);
            copyFilenameToClipboardToolStripMenuItem1.Enabled = _revisionDiffController.ShouldShowMenuCopyFileName(selectionInfo);

            stageFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuStage(selectionInfo);
            unstageFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuUnstage(selectionInfo);

            cherryPickSelectedDiffFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuCherryPick(selectionInfo);

            // Visibility of FileTree is not known, assume (CommitInfoTabControl.Contains(TreeTabPage);)
            diffShowInFileTreeToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuShowInFileTree(selectionInfo);
            fileHistoryDiffToolstripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuFileHistory(selectionInfo);
            blameToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuBlame(selectionInfo);
            resetFileToToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowResetFileMenus(selectionInfo);

            diffDeleteFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuDeleteFile(selectionInfo);
            diffEditWorkingDirectoryFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo);
            diffOpenWorkingDirectoryFileWithToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo);
            diffOpenRevisionFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);
            diffOpenRevisionFileWithToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);

            diffCommitSubmoduleChanges.Visible =
                diffResetSubmoduleChanges.Visible =
                diffStashSubmoduleChangesToolStripMenuItem.Visible =
                diffSubmoduleSummaryMenuItem.Visible =
                diffUpdateSubmoduleMenuItem.Visible = _revisionDiffController.ShouldShowSubmoduleMenus(selectionInfo);

            diffToolStripSeparator13.Visible = _revisionDiffController.ShouldShowMenuDeleteFile(selectionInfo) ||
                                               _revisionDiffController.ShouldShowSubmoduleMenus(selectionInfo) ||
                                               _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo) ||
                                               _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);

            // openContainingFolderToolStripMenuItem.Enabled or not
            {
                openContainingFolderToolStripMenuItem.Enabled = false;

                foreach (var item in DiffFiles.SelectedItems)
                {
                    string filePath = _fullPathResolver.Resolve(item.Name);
                    if (FormBrowseUtil.FileOrParentDirectoryExists(filePath))
                    {
                        openContainingFolderToolStripMenuItem.Enabled = true;
                        break;
                    }
                }
            }
        }

        private void blameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedItem;

            if (item != null && item.IsTracked)
            {
                UICommands.StartFileHistoryDialog(this, item.Name, DiffFiles.Revision, true, true);
            }
        }

        private void StageFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            var files = DiffFiles.SelectedItems.Where(item => item.Staged == StagedStatus.WorkTree).ToList();

            Module.StageFiles(files, out _);
            RefreshArtificial();
        }

        private void UnstageFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            Module.BatchUnstageFiles(DiffFiles.SelectedItems.Where(item => item.Staged == StagedStatus.Index));
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
                DiffFiles.SelectedItem = selectedItem;
            }
        }

        private void fileHistoryDiffToolstripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedItem;

            if (item != null && item.IsTracked)
            {
                UICommands.StartFileHistoryDialog(this, item.Name, DiffFiles.Revision);
            }
        }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormBrowse.OpenContainingFolder(DiffFiles, Module);
        }

        private void openWithDifftoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null || DiffFiles.Revision == null)
            {
                return;
            }

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

            foreach (var itemWithParent in DiffFiles.SelectedItemsWithParent)
            {
                if (itemWithParent.ParentRevision.ObjectId == ObjectId.CombinedDiffId)
                {
                    // CombinedDiff cannot be viewed in a difftool
                    // Disabled in menues but can be activated from shortcuts, just ignore
                    continue;
                }

                var revs = new[] { DiffFiles.Revision, itemWithParent.ParentRevision };
                UICommands.OpenWithDifftool(this, revs, itemWithParent.Item.Name, itemWithParent.Item.OldName, diffKind, itemWithParent.Item.IsTracked);
            }
        }

        private void diffEditWorkingDirectoryFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null)
            {
                return;
            }

            var fileName = _fullPathResolver.Resolve(DiffFiles.SelectedItem.Name);
            UICommands.StartFileEditorDialog(fileName);
            RefreshArtificial();
        }

        private void diffOpenWorkingDirectoryFileWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null)
            {
                return;
            }

            var fileName = _fullPathResolver.Resolve(DiffFiles.SelectedItem.Name);
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
            var gitItemStatus = DiffFiles.SelectedItem;
            var revisionId = DiffFiles.Revision?.ObjectId;

            if (gitItemStatus?.Name == null || revisionId == null || revisionId == ObjectId.CombinedDiffId)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;

                var blob = Module.GetFileBlobHash(gitItemStatus.Name, revisionId);

                if (blob == null)
                {
                    return;
                }

                var fileName = PathUtil.GetFileName(gitItemStatus.Name);
                fileName = (Path.GetTempPath() + fileName).ToNativePath();
                Module.SaveBlobAs(fileName, blob.ToString());

                onSaved(fileName);
            }).FileAndForget();
        }

        private ContextMenuDiffToolInfo GetContextMenuDiffToolInfo()
        {
            bool firstIsParent = _gitRevisionTester.AllFirstAreParentsToSelected(DiffFiles.SelectedItemParents, DiffFiles.Revision);
            bool localExists = _gitRevisionTester.AnyLocalFileExists(DiffFiles.SelectedItemsWithParent.Select(i => i.Item));

            var selectedItemParentRevs = DiffFiles.SelectedItemParents.Select(i => i.ObjectId).ToList();
            bool allAreNew = DiffFiles.SelectedItemsWithParent.All(i => i.Item.IsNew);
            bool allAreDeleted = DiffFiles.SelectedItemsWithParent.All(i => i.Item.IsDeleted);

            return new ContextMenuDiffToolInfo(
                DiffFiles.Revision,
                selectedItemParentRevs,
                allAreNew: allAreNew,
                allAreDeleted: allAreDeleted,
                firstIsParent: firstIsParent,
                firstParentsValid: _revisionGrid.IsFirstParentValid(),
                localExists: localExists);
        }

        private void openWithDifftoolToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ContextMenuDiffToolInfo selectionInfo = GetContextMenuDiffToolInfo();

            if (DiffFiles.SelectedItemsWithParent.Any())
            {
                selectedDiffCaptionMenuItem.Text = _selectedRevision + DescribeRevision(DiffFiles.Revision?.ObjectId, 50);
                selectedDiffCaptionMenuItem.Visible = true;
                MenuUtil.SetAsCaptionMenuItem(selectedDiffCaptionMenuItem, DiffContextMenu);

                firstDiffCaptionMenuItem.Text = _firstRevision.Text;
                var parentDesc = DescribeRevision(DiffFiles.SelectedItemParents.ToList());
                if (parentDesc.IsNotNullOrWhitespace())
                {
                    firstDiffCaptionMenuItem.Text += parentDesc;
                }

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
        /// <param name="rev">The GitRevision</param>
        /// <returns>If it is possible to reset to the revisions</returns>
        private bool CanResetToRevision(GitRevision rev)
        {
            return rev.ObjectId != ObjectId.WorkTreeId
                   && rev.ObjectId != ObjectId.CombinedDiffId;
        }

        private void resetFileToToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (DiffFiles.Revision == null)
            {
                resetFileToSelectedToolStripMenuItem.Visible = false;
                resetFileToParentToolStripMenuItem.Visible = false;
                return;
            }

            if (!CanResetToRevision(DiffFiles.Revision))
            {
                resetFileToSelectedToolStripMenuItem.Visible = false;
            }
            else
            {
                resetFileToSelectedToolStripMenuItem.Visible = true;
                resetFileToSelectedToolStripMenuItem.Text =
                    _selectedRevision + DescribeRevision(DiffFiles.Revision?.ObjectId, 50);
            }

            var parents = DiffFiles.SelectedItemParents.ToList();
            if (parents.Count != 1 || !CanResetToRevision(parents.FirstOrDefault()))
            {
                resetFileToParentToolStripMenuItem.Visible = false;
            }
            else
            {
                resetFileToParentToolStripMenuItem.Visible = true;
                resetFileToParentToolStripMenuItem.Text =
                    _firstRevision + DescribeRevision(parents.FirstOrDefault()?.ObjectId, 50);
            }
        }

        private void saveAsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (DiffFiles.Revision == null || DiffFiles.SelectedItem == null)
            {
                return;
            }

            GitItemStatus item = DiffFiles.SelectedItem;

            var fullName = _fullPathResolver.Resolve(item.Name);
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
                    Module.SaveBlobAs(fileDialog.FileName, $"{DiffFiles.Revision?.Guid}:\"{item.Name}\"");
                }
            }
        }

        private bool DeleteSelectedFiles()
        {
            try
            {
                if (DiffFiles.SelectedItem == null || DiffFiles.Revision == null || !DiffFiles.Revision.IsArtificial ||
                    MessageBox.Show(this, _deleteSelectedFiles.Text, _deleteSelectedFilesCaption.Text, MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning) !=
                    DialogResult.Yes)
                {
                    return false;
                }

                var selectedItems = DiffFiles.SelectedItems;

                // If any file is staged, it must be unstaged
                Module.BatchUnstageFiles(selectedItems.Where(item => item.Staged == StagedStatus.Index));

                DiffFiles.StoreNextIndexToSelect();
                var items = DiffFiles.SelectedItems.Where(item => !item.IsSubmodule);
                foreach (var item in items)
                {
                    var path = _fullPathResolver.Resolve(item.Name);
                    bool isDir = (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
                    if (isDir)
                    {
                        Directory.Delete(path, true);
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
                MessageBox.Show(this, _deleteFailed.Text + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            var submodules = DiffFiles.SelectedItems.Where(it => it.IsSubmodule).Select(it => it.Name).Distinct().ToList();

            foreach (var name in submodules)
            {
                var submodulCommands = new GitUICommands(_fullPathResolver.Resolve(name.EnsureTrailingPathSeparator()));
                submodulCommands.StartCommitDialog(this);
            }

            RefreshArtificial();
        }

        private void diffResetSubmoduleChanges_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.IsSubmodule).Select(it => it.Name).Distinct().ToList();

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
            var submodules = DiffFiles.SelectedItems.Where(it => it.IsSubmodule).Select(it => it.Name).Distinct();

            FormProcess.ShowDialog(FindForm() as FormBrowse, GitCommandHelpers.SubmoduleUpdateCmd(submodules));
            RefreshArtificial();
        }

        private void diffStashSubmoduleChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.IsSubmodule).Select(it => it.Name).Distinct().ToList();

            foreach (var name in submodules)
            {
                var uiCmds = new GitUICommands(Module.GetSubmodule(name));
                uiCmds.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash);
            }

            RefreshArtificial();
        }

        private void diffSubmoduleSummaryMenuItem_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.IsSubmodule).Select(it => it.Name).Distinct().ToList();

            string summary = "";
            foreach (var name in submodules)
            {
                summary += Module.GetSubmoduleSummary(name);
            }

            using (var frm = new FormEdit(UICommands, summary))
            {
                frm.ShowDialog(this);
            }
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
