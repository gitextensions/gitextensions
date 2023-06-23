using System.ComponentModel;
using System.Text;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Commands;
using GitUI.CommandDialogs;
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
            new("Are you sure you want to delete the selected file(s)?");
        private readonly TranslationString _deleteFailed = new("Delete file failed");
        private readonly TranslationString _multipleDescription = new("<multiple>");
        private readonly TranslationString _selectedRevision = new("Second: B ");
        private readonly TranslationString _firstRevision = new("First: A ");

        private readonly TranslationString _resetSelectedChangesText =
            new("Are you sure you want to reset all selected files to {0}?");

        private IRevisionGridInfo? _revisionGridInfo;
        private IRevisionGridUpdate? _revisionGridUpdate;
        private Func<string>? _pathFilter;
        private RevisionFileTreeControl? _revisionFileTree;
        private readonly IRevisionDiffController _revisionDiffController;
        private readonly IFileStatusListContextMenuController _revisionDiffContextMenuController;
        private readonly IFullPathResolver _fullPathResolver;
        private readonly IFindFilePredicateProvider _findFilePredicateProvider;
        private readonly IGitRevisionTester _gitRevisionTester;
        private readonly CancellationTokenSequence _customDiffToolsSequence = new();
        private readonly CancellationTokenSequence _viewChangesSequence = new();
        private readonly CancellationTokenSequence _setDiffSequence = new();
        private readonly RememberFileContextMenuController _rememberFileContextMenuController
            = RememberFileContextMenuController.Default;
        private Action? _refreshGitStatus;
        private FileStatusItem? _selectedBlameItem;
        private string? _fallbackFollowedFile;
        private FileStatusItem? _lastExplicitlySelectedItem;
        private bool _isImplicitListSelection = false;

        public RevisionDiffControl()
        {
            InitializeComponent();
            DiffFiles.GroupByRevision = true;
            InitializeComplete();
            HotkeysEnabled = true;
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
            _revisionDiffController = new RevisionDiffController(() => Module, _fullPathResolver);
            _findFilePredicateProvider = new FindFilePredicateProvider();
            _gitRevisionTester = new GitRevisionTester(_fullPathResolver);
            _revisionDiffContextMenuController = new FileStatusListContextMenuController();
            DiffText.TopScrollReached += FileViewer_TopScrollReached;
            DiffText.BottomScrollReached += FileViewer_BottomScrollReached;
            DiffText.LinePatchingBlocksUntilReload = true;
            BlameControl.HideCommitInfo();
            diffFilterFileInGridToolStripMenuItem.Text = TranslatedStrings.FilterFileInGrid;
            copyPathsToolStripMenuItem.Initialize(() => UICommands, () => DiffFiles.SelectedItems.Select(fsi => fsi.Item.Name));
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

        public void RepositoryChanged()
        {
            _rememberFileContextMenuController.RememberedDiffFileItem = null;
        }

        public void RefreshArtificial()
        {
            if (!Visible)
            {
                return;
            }

            Validates.NotNull(_revisionGridInfo);
            var revisions = _revisionGridInfo.GetSelectedRevisions();
            if (!revisions.Any(r => r.IsArtificial))
            {
                return;
            }

            DiffFiles.StoreNextIndexToSelect();
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await SetDiffsAsync(revisions);
                if (DiffFiles.SelectedItem is null)
                {
                    DiffFiles.SelectStoredNextIndex();
                }
            }).FileAndForget();
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
            ShowFileTree = 12,
            FilterFileInGrid = 13,
            SelectFirstGroupChanges = 14,
            FindFile = 15,
            OpenWorkingDirectoryFileWith = 16,
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
                case Command.OpenWorkingDirectoryFileWith: diffOpenWorkingDirectoryFileWithToolStripMenuItem.PerformClick(); break;
                case Command.EditFile: diffEditWorkingDirectoryFileToolStripMenuItem.PerformClick(); break;
                case Command.OpenAsTempFile: diffOpenRevisionFileToolStripMenuItem.PerformClick(); break;
                case Command.OpenAsTempFileWith: diffOpenRevisionFileWithToolStripMenuItem.PerformClick(); break;
                case Command.ResetSelectedFiles: return ResetSelectedFilesWithConfirmation();
                case Command.StageSelectedFile: return StageSelectedFiles();
                case Command.UnStageSelectedFile: return UnstageSelectedFiles();
                case Command.ShowFileTree: diffShowInFileTreeToolStripMenuItem.PerformClick(); break;
                case Command.FilterFileInGrid: diffFilterFileInGridToolStripMenuItem.PerformClick(); break;
                case Command.SelectFirstGroupChanges: return SelectFirstGroupChangesIfFileNotFocused();
                case Command.FindFile: findInDiffToolStripMenuItem.PerformClick(); break;
                default: return base.ExecuteCommand(cmd);
            }

            return true;

            bool SelectFirstGroupChangesIfFileNotFocused()
            {
                if (ContainsFocus && !DiffFiles.Focused)
                {
                    return false;
                }

                DiffFiles.SelectedItems = DiffFiles.FirstGroupItems;
                DiffFiles.Focus();
                return true;
            }
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
            diffOpenWorkingDirectoryFileWithToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenWorkingDirectoryFileWith);
            diffEditWorkingDirectoryFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.EditFile);
            diffOpenRevisionFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenAsTempFile);
            diffOpenRevisionFileWithToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.OpenAsTempFileWith);
            resetFileToParentToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.ResetSelectedFiles);
            stageFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.StageSelectedFile);
            unstageFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.UnStageSelectedFile);
            diffShowInFileTreeToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.ShowFileTree);
            diffFilterFileInGridToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.FilterFileInGrid);
            findInDiffToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.FindFile);

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

            new CustomDiffMergeToolProvider().LoadCustomDiffMergeTools(Module, menus, components, isDiff: true, cancellationToken: _customDiffToolsSequence.Next());
        }

        public void CancelLoadCustomDifftools()
        {
            _customDiffToolsSequence.CancelCurrent();
        }

        private string GetShortcutKeyDisplayString(Command cmd)
        {
            return GetShortcutKeys((int)cmd).ToShortcutKeyDisplayString();
        }

        #endregion

        public void DisplayDiffTab(IReadOnlyList<GitRevision> revisions)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await SetDiffsAsync(revisions);

                // Select something by default, except range diff
                if (DiffFiles.SelectedItem is null && !(DiffFiles.FirstGroupItems.Count() == 1 && DiffFiles.FirstGroupItems.FirstOrDefault().Item.IsRangeDiff))
                {
                    DiffFiles.SelectFirstVisibleItem();
                }
            }).FileAndForget();
        }

        /// <summary>
        /// Gets or sets the file in the list to select initially.
        /// When switching commits, the last selected file is "followed" if available in the new commit,
        /// this file is used as a fallback.
        /// </summary>
        public string? FallbackFollowedFile
        {
            get => _fallbackFollowedFile;
            set
            {
                _fallbackFollowedFile = value;
                _lastExplicitlySelectedItem = null;
            }
        }

        private async Task SetDiffsAsync(IReadOnlyList<GitRevision> revisions)
        {
            Validates.NotNull(_revisionGridInfo);
            CancellationToken cancellationToken = _setDiffSequence.Next();

            _viewChangesSequence.CancelCurrent();
            await this.SwitchToMainThreadAsync(cancellationToken);
            await DiffText.ClearAsync();

            FileStatusItem prevSelectedItem = DiffFiles.SelectedItem;
            FileStatusItem prevDiffItem = DiffFiles.FirstGroupItems.Contains(prevSelectedItem) ? prevSelectedItem : null;
            await DiffFiles.SetDiffsAsync(revisions, _revisionGridInfo.CurrentCheckout, cancellationToken);
            await this.SwitchToMainThreadAsync(cancellationToken);

            _isImplicitListSelection = true;

            // First try the last item explicitly selected
            if (_lastExplicitlySelectedItem is not null
                && DiffFiles.FirstGroupItems.FirstOrDefault(i => i.Item.Name.Equals(_lastExplicitlySelectedItem.Item.Name))?.Item is GitItemStatus explicitItem)
            {
                DiffFiles.SelectedGitItem = explicitItem;
                return;
            }

            // Second go back to the filtered file
            if (!string.IsNullOrWhiteSpace(FallbackFollowedFile)
                && DiffFiles.FirstGroupItems.FirstOrDefault(i => i.Item.Name.Equals(FallbackFollowedFile))?.Item is GitItemStatus fallbackItem)
            {
                DiffFiles.SelectedGitItem = fallbackItem;
                return;
            }

            // Third try to restore the previous item
            if (prevDiffItem is not null
                && DiffFiles.FirstGroupItems.FirstOrDefault(i => i.Item.Name.Equals(prevDiffItem.Item.Name))?.Item is GitItemStatus prevItem)
            {
                DiffFiles.SelectedGitItem = prevItem;
            }
        }

        public void Bind(IRevisionGridInfo revisionGridInfo, IRevisionGridUpdate revisionGridUpdate, RevisionFileTreeControl revisionFileTree, Func<string>? pathFilter, Action? refreshGitStatus)
        {
            _revisionGridInfo = revisionGridInfo;
            _revisionGridUpdate = revisionGridUpdate;
            _revisionFileTree = revisionFileTree;
            _pathFilter = pathFilter;
            _refreshGitStatus = refreshGitStatus;
            DiffFiles.Bind(objectId => DescribeRevision(objectId), _revisionGridInfo.GetActualRevision);
        }

        public void InitSplitterManager(SplitterManager splitterManager)
        {
            splitterManager.AddSplitter(DiffSplitContainer, "DiffSplitContainer");
        }

        protected override void OnRuntimeLoad()
        {
            DiffText.SetFileLoader(GetNextPatchFile);
            DiffText.Font = AppSettings.FixedWidthFont;
            ReloadHotkeys();
            LoadCustomDifftools();

            base.OnRuntimeLoad();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _customDiffToolsSequence.Dispose();
                _viewChangesSequence.Dispose();
                _setDiffSequence.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private string DescribeRevision(ObjectId? objectId, int maxLength = 0)
        {
            if (objectId is null)
            {
                // No parent at all, present as working directory
                return ResourceManager.TranslatedStrings.Workspace;
            }

            Validates.NotNull(_revisionGridInfo);

            var revision = _revisionGridInfo.GetRevision(objectId);

            if (revision is null)
            {
                return objectId.ToShortString();
            }

            return _revisionGridInfo.DescribeRevision(revision, maxLength);
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

            loadFileContent = ShowSelectedFileDiffAsync(ensureNoSwitchToFilter: false, line: 0);
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

            ContextMenuSelectionInfo selectionInfo = new(
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

        private void ResetSelectedItemsTo(bool resetToParent, bool resetAndDelete)
        {
            IReadOnlyList<FileStatusItem> selectedItems = DiffFiles.SelectedItems.ToList();

            if (!selectedItems.Any())
            {
                return;
            }

            try
            {
                foreach (ObjectId id in resetToParent ? selectedItems.FirstIds() : selectedItems.SecondIds())
                {
                    if (resetToParent ? !CanResetToFirst(id, selectedItems) : !CanResetToSecond(id))
                    {
                        // Cannot reset to artificial commit, may be included in multi selections
                        continue;
                    }

                    IReadOnlyList<GitItemStatus> resetItems = (resetToParent ? selectedItems.Items()
                        : selectedItems.Items().Select(item => item.InvertStatus())).ToList();
                    Module.ResetChanges(id, resetItems, resetAndDelete: resetAndDelete, _fullPathResolver, out StringBuilder output);

                    if (output.Length > 0)
                    {
                        MessageBox.Show(this, output.ToString(), TranslatedStrings.ResetChangesCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            finally
            {
                RequestRefresh();
            }
        }

        /// <summary>
        /// Return if it is possible to reset to the first commit.
        /// </summary>
        /// <param name="parentId">The parent commit id.</param>
        /// <param name="selectedItems">The selected file status items.</param>
        /// <returns><see langword="true"/> if it is possible to reset to first id.</returns>
        private static bool CanResetToFirst(ObjectId? parentId, IEnumerable<FileStatusItem> selectedItems)
        {
            return CanResetToSecond(parentId) || (parentId == ObjectId.IndexId && selectedItems.SecondIds().All(i => i == ObjectId.WorkTreeId));
        }

        /// <summary>
        /// Return if it is possible to reset to the second (selected) commit.
        /// </summary>
        /// <param name="resetId">The selected commit id.</param>
        /// <returns><see langword="true"/> if it is possible to reset to first id.</returns>
        private static bool CanResetToSecond(ObjectId? resetId) => resetId?.IsArtificial is false;

        /// <summary>
        /// Show the file in the BlameViewer if Blame is visible.
        /// </summary>
        /// <param name="line">The line to start at.</param>
        /// <returns>a task</returns>
        private async Task ShowSelectedFileBlameAsync(bool ensureNoSwitchToFilter, int? line)
        {
            if (DiffFiles.SelectedItem is null)
            {
                await ShowSelectedFileDiffAsync(ensureNoSwitchToFilter, line);
                return;
            }

            DiffText.Visible = false;
            BlameControl.Visible = true;

            // Avoid that focus is switched to the file filter after changing visibility
            if (ensureNoSwitchToFilter && DiffFiles.FilterFocused)
            {
                BlameControl.Focus();
            }

            GitRevision rev = DiffFiles.SelectedItem.SecondRevision.IsArtificial
                ? _revisionGridInfo.GetActualRevision(_revisionGridInfo.CurrentCheckout)
                : DiffFiles.SelectedItem.SecondRevision;
            await BlameControl.LoadBlameAsync(rev, children: null, DiffFiles.SelectedItem.Item.Name, _revisionGridInfo, _revisionGridUpdate,
                controlToMask: null, DiffText.Encoding, line, cancellationToken: _viewChangesSequence.Next());
        }

        /// <summary>
        /// Show selected item as a file diff
        /// Activate diffviewer if Blame is visible
        /// </summary>
        /// <returns>a task</returns>
        private async Task ShowSelectedFileDiffAsync(bool ensureNoSwitchToFilter, int? line)
        {
            Validates.NotNull(_pathFilter);

            BlameControl.Visible = false;
            DiffText.Visible = true;

            // Avoid that focus is switched to the file filter after changing visibility
            if (ensureNoSwitchToFilter && DiffFiles.FilterFocused)
            {
                DiffText.Focus();
            }

            await DiffText.ViewChangesAsync(DiffFiles.SelectedItem,
                line: line,
                openWithDiffTool: () => firstToSelectedToolStripMenuItem.PerformClick(),
                additionalCommandInfo: (DiffFiles.SelectedItem?.Item?.IsRangeDiff is true) && Module.GitVersion.SupportRangeDiffPath ? _pathFilter() : "",
                cancellationToken: _viewChangesSequence.Next());
        }

        /// <summary>
        /// Show selected item as diff or blame
        /// </summary>
        private void ShowSelectedFile(bool ensureNoSwitchToFilter = false, int? line = null) =>
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                await (!blameToolStripMenuItem.Checked
                    ? ShowSelectedFileDiffAsync(ensureNoSwitchToFilter, line)
                    : ShowSelectedFileBlameAsync(ensureNoSwitchToFilter, line)))
            .FileAndForget();

        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Switch to diff if the selection changes
            FileStatusItem? item = DiffFiles.SelectedItem;
            if (item is not null && blameToolStripMenuItem.Checked && item.Item.Name != _selectedBlameItem?.Item.Name)
            {
                blameToolStripMenuItem.Checked = false;
            }

            // If this is not occurring after a revision change (implicit selection)
            // save the selected item so it can be the "preferred" selection
            if (!_isImplicitListSelection && item is not null && !item.Item.IsRangeDiff)
            {
                _lastExplicitlySelectedItem = item;
                _selectedBlameItem = null;
            }

            _isImplicitListSelection = false;
            ShowSelectedFile();
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
                ThreadHelper.JoinableTaskFactory.RunAsync(DiffFiles.OpenSubmoduleAsync);
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
            ShowSelectedFile(ensureNoSwitchToFilter: true);
        }

        private void DiffText_PatchApplied(object sender, EventArgs e)
        {
            RequestRefresh();
        }

        private void diffShowInFileTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenInFileTreeTab(requestBlame: false);
        }

        private void diffFilterFileInGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (FindForm() as FormBrowse)?.SetPathFilter(string.Join(" ", DiffFiles.SelectedItems.Select(f => f.Item.Name.ToPosixPath().QuoteNE())));
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

            diffUpdateSubmoduleMenuItem.Visible
                = diffResetSubmoduleChanges.Visible
                = diffStashSubmoduleChangesToolStripMenuItem.Visible
                = diffCommitSubmoduleChanges.Visible
                = submoduleStripSeparator.Visible
                = _revisionDiffController.ShouldShowSubmoduleMenus(selectionInfo);

            stageFileToolStripMenuItem.Enabled
                = stageFileToolStripMenuItem.Visible
                = _revisionDiffController.ShouldShowMenuStage(selectionInfo);
            unstageFileToolStripMenuItem.Enabled
                = unstageFileToolStripMenuItem.Visible
                = _revisionDiffController.ShouldShowMenuUnstage(selectionInfo);
            resetFileToToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowResetFileMenus(selectionInfo);
            cherryPickSelectedDiffFileToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuCherryPick(selectionInfo);

            diffToolStripSeparator13.Visible = _revisionDiffController.ShouldShowDifftoolMenus(selectionInfo)
                || _revisionDiffController.ShouldShowMenuDeleteFile(selectionInfo)
                || _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo)
                || _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);

            openWithDifftoolToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowDifftoolMenus(selectionInfo);
            diffOpenWorkingDirectoryFileWithToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo);
            diffOpenRevisionFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);
            diffOpenRevisionFileToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuShowInFileTree(selectionInfo);
            diffOpenRevisionFileWithToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);
            diffOpenRevisionFileWithToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuShowInFileTree(selectionInfo);
            saveAsToolStripMenuItem1.Visible = _revisionDiffController.ShouldShowMenuSaveAs(selectionInfo);
            openContainingFolderToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuShowInFolder(selectionInfo);
            diffEditWorkingDirectoryFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo);
            diffDeleteFileToolStripMenuItem.Text = ResourceManager.TranslatedStrings.GetDeleteFile(selectionInfo.SelectedGitItemCount);
            diffDeleteFileToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuDeleteFile(selectionInfo);
            diffDeleteFileToolStripMenuItem.Visible = diffDeleteFileToolStripMenuItem.Enabled;

            copyPathsToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuCopyFileName(selectionInfo);
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
            diffFilterFileInGridToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowResetFileMenus(selectionInfo);
            fileHistoryDiffToolstripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuFileHistory(selectionInfo);
            blameToolStripMenuItem.Enabled = AppSettings.UseDiffViewerForBlame.Value
                ? _revisionDiffController.ShouldShowMenuBlame(selectionInfo)
                : _revisionDiffController.ShouldShowMenuShowInFileTree(selectionInfo);
            if (!blameToolStripMenuItem.Enabled)
            {
                blameToolStripMenuItem.Checked = false;
            }
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

            if (AppSettings.UseDiffViewerForBlame.Value)
            {
                int? line = DiffText.Visible ? DiffText.CurrentFileLine : BlameControl.CurrentFileLine;
                blameToolStripMenuItem.Checked = !blameToolStripMenuItem.Checked;
                _selectedBlameItem = blameToolStripMenuItem.Checked ? DiffFiles.SelectedItem : null;
                ShowSelectedFile(ensureNoSwitchToFilter: true, line);
                return;
            }

            blameToolStripMenuItem.Checked = false;
            OpenInFileTreeTab(requestBlame: true);
        }

        /// <summary>
        /// Open the selected item in the FileTree tab
        /// </summary>
        /// <param name="requestBlame">Request that Blame is shown in the FileTree</param>
        private void OpenInFileTreeTab(bool requestBlame)
        {
            Validates.NotNull(_revisionFileTree);

            // switch to view (and fills the first level of file tree data model if not already done)
            string name = DiffFiles.SelectedItems.First().Item.Name;
            int? line = DiffText.Visible ? DiffText.CurrentFileLine : BlameControl.CurrentFileLine;
            (FindForm() as FormBrowse)?.ExecuteCommand(FormBrowse.Command.FocusFileTree);
            _revisionFileTree.ExpandToFile(name, line, requestBlame);
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

            FileStatusItem item = new(
                firstRev: DiffFiles.SelectedItem.SecondRevision,
                secondRev: DiffFiles.SelectedItem.FirstRevision,
                item: DiffFiles.SelectedItem.Item);
            if (!string.IsNullOrWhiteSpace(DiffFiles.SelectedItem.Item.OldName))
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
            ResetSelectedItemsWithConfirmation(resetToParent: sender == resetFileToParentToolStripMenuItem);
        }

        private void resetFileToToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            InitResetFileToToolStripMenuItem();
        }

        private void InitResetFileToToolStripMenuItem()
        {
            // Multiple parent/child can be selected, only the the first is shown.
            // The only artificial commit that can be reset to is Index<-WorkTree
            ObjectId? selectedId = DiffFiles.SelectedItems.SecondIds().FirstOrDefault();
            ObjectId? parentId = DiffFiles.SelectedItems.FirstIds().FirstOrDefault();

            if (!CanResetToSecond(selectedId))
            {
                resetFileToSelectedToolStripMenuItem.Enabled = false;
                resetFileToSelectedToolStripMenuItem.Visible = false;
            }
            else
            {
                resetFileToSelectedToolStripMenuItem.Enabled = true;
                resetFileToSelectedToolStripMenuItem.Visible = true;
                resetFileToSelectedToolStripMenuItem.Text =
                    _selectedRevision + DescribeRevision(selectedId, 50);
            }

            if (!CanResetToFirst(parentId, DiffFiles.SelectedItems))
            {
                resetFileToParentToolStripMenuItem.Enabled = false;
                resetFileToParentToolStripMenuItem.Visible = false;
            }
            else
            {
                resetFileToParentToolStripMenuItem.Enabled = true;
                resetFileToParentToolStripMenuItem.Visible = true;
                resetFileToParentToolStripMenuItem.Text =
                    _firstRevision + DescribeRevision(parentId, 50);
            }
        }

        private void saveAsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            List<FileStatusItem> files = DiffFiles.SelectedItems.ToList();

            Func<string, string?> userSelection = default;
            if (files.Count == 1)
            {
                userSelection = (fullName) =>
                {
                    using SaveFileDialog dialog = new()
                    {
                        InitialDirectory = Path.GetDirectoryName(fullName),
                        FileName = Path.GetFileName(fullName),
                        DefaultExt = Path.GetExtension(fullName),
                        AddExtension = true
                    };
                    dialog.Filter = $"{_saveFileFilterCurrentFormat.Text}(*.{dialog.DefaultExt})|*.{dialog.DefaultExt}|{_saveFileFilterAllFiles.Text}(*.*)|*.*";

                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        return dialog.FileName;
                    }

                    return null;
                };
            }
            else if (files.Count > 1)
            {
                userSelection = (baseSourceDirectory) =>
                {
                    using FolderBrowserDialog dialog = new()
                    {
                        InitialDirectory = baseSourceDirectory,
                        ShowNewFolderButton = true,
                    };

                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        return dialog.SelectedPath;
                    }

                    return null;
                };
            }

            _revisionDiffController.SaveFiles(files, userSelection);
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

                DiffFiles.StoreNextIndexToSelect();

                try
                {
                    DeleteFromFilesystem(DiffFiles.SelectedItems);
                }
                finally
                {
                    RequestRefresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _deleteFailed.Text + Environment.NewLine + ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void DeleteFromFilesystem(IEnumerable<FileStatusItem> items)
        {
            items = items.Where(item => !item.Item.IsSubmodule);

            // If any file is staged, it must be unstaged
            Module.BatchUnstageFiles(items.Where(item => item.Item.Staged == StagedStatus.Index).Select(item => item.Item));

            foreach (FileStatusItem item in items)
            {
                string? path = _fullPathResolver.Resolve(item.Item.Name);
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                }
                else
                {
                    File.Delete(path);
                }
            }
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
                GitUICommands submodulCommands = new(_fullPathResolver.Resolve(name.EnsureTrailingPathSeparator()));
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
                module.ResetAllChanges(clean: resetType == FormResetChanges.ActionEnum.ResetAndDelete);
            }

            RequestRefresh();
        }

        private void diffUpdateSubmoduleMenuItem_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.Item.IsSubmodule).Select(it => it.Item.Name).Distinct().ToList();

            FormProcess.ShowDialog(FindForm() as FormBrowse, arguments: GitCommandHelpers.SubmoduleUpdateCmd(submodules), Module.WorkingDir, input: null, useDialogSettings: true);
            RequestRefresh();
        }

        private void diffStashSubmoduleChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var submodules = DiffFiles.SelectedItems.Where(it => it.Item.IsSubmodule).Select(it => it.Item.Name).Distinct().ToList();

            foreach (var name in submodules)
            {
                GitUICommands uiCmds = new(Module.GetSubmodule(name));
                uiCmds.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash);
            }

            RequestRefresh();
        }

        public void SwitchFocus(bool alreadyContainedFocus)
        {
            if (alreadyContainedFocus && DiffFiles.Focused)
            {
                if (BlameControl.Visible)
                {
                    BlameControl.Focus();
                }
                else
                {
                    DiffText.Focus();
                }
            }
            else
            {
                DiffFiles.Focus();
            }
        }

        public override bool ProcessHotkey(Keys keyData)
        {
            return base.ProcessHotkey(keyData) // generic handling of this controls's hotkeys (upstream)
                || (!GitExtensionsControl.IsTextEditKey(keyData) // downstream (without keys for quick search and filter)
                    && ((DiffFiles.Visible && DiffFiles.ProcessHotkey(keyData))
                        || (DiffText.Visible && DiffText.ProcessHotkey(keyData))
                        || (BlameControl.Visible && BlameControl.ProcessHotkey(keyData))));
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

            if (!stageFileToolStripMenuItem.Enabled)
            {
                // Hotkey executed when menu is disabled
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

            if (!unstageFileToolStripMenuItem.Enabled)
            {
                // Hotkey executed when menu is disabled
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

            InitResetFileToToolStripMenuItem();
            if (!resetFileToParentToolStripMenuItem.Enabled)
            {
                // Hotkey executed when menu is disabled
                return true;
            }

            // Reset to first (parent)
            ResetSelectedItemsWithConfirmation(resetToParent: true);
            return true;
        }

        private void ResetSelectedItemsWithConfirmation(bool resetToParent)
        {
            IEnumerable<FileStatusItem> items = DiffFiles.SelectedItems;

            // The "new" state could change when resetting, allow user to tick the checkbox.
            // If there are only changed files, it is safe to disable the checkboc (also for restting to selected).
            bool hasNewFiles = !items.All(item => item.Item.IsChanged);
            bool hasExistingFiles = items.Any(item => !(item.Item.IsUncommittedAdded || RenamedIndexItem(item)));

            string revDescription = resetToParent
                ? $"{_firstRevision.Text}{DescribeRevision(items.FirstRevs().ToList())}"
                : $"{_selectedRevision.Text}{DescribeRevision(items.SecondRevs().ToList())}";
            string confirmationMessage = string.Format(_resetSelectedChangesText.Text, revDescription);

            FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(ParentForm, hasExistingFiles, hasNewFiles, confirmationMessage);
            if (resetType == FormResetChanges.ActionEnum.Cancel)
            {
                return;
            }

            bool resetAndDelete = resetType == FormResetChanges.ActionEnum.ResetAndDelete;
            ResetSelectedItemsTo(resetToParent, resetAndDelete);
        }

        private static bool RenamedIndexItem(FileStatusItem item) => item.Item.IsRenamed && item.Item.Staged == StagedStatus.Index;

        internal void RegisterGitHostingPluginInBlameControl()
        {
            BlameControl.ConfigureRepositoryHostPlugin(PluginRegistry.TryGetGitHosterForModule(Module));
        }

        internal TestAccessor GetTestAccessor()
            => new(this);

        internal readonly struct TestAccessor
        {
            private readonly RevisionDiffControl _control;

            public TestAccessor(RevisionDiffControl control)
            {
                _control = control;
            }

            public SplitContainer DiffSplitContainer => _control.DiffSplitContainer;
        }
    }
}
