using System.ComponentModel;
using System.Text;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.HelperDialogs;
using GitUI.ScriptsEngine;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class RevisionDiffControl : GitModuleControl, IRevisionGridFileUpdate
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
        private RevisionDiffControl? _revisionFileTree;
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
        private GitItemStatus? _selectedBlameItem;
        private RelativePath? _fallbackFollowedFile;
        private RelativePath? _lastExplicitlySelectedItem;
        private int? _lastExplicitlySelectedItemLine;
        private bool _isImplicitListSelection = false;

        public RevisionDiffControl()
        {
            InitializeComponent();
            InitializeComplete();
            HotkeysEnabled = true;
            DiffFiles.CanUseFindInCommitFilesGitGrep = true;
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
            copyPathsToolStripMenuItem.Initialize(getUICommands: () => UICommands,
                getSelectedFilePaths: () => DiffFiles.SelectedFolder is RelativePath relativePath
                    ? [relativePath.Value]
                    : DiffFiles.SelectedItems.Select(fsi => fsi.Item.Name));

            showFindInCommitFilesGitGrepToolStripMenuItem.Checked = AppSettings.ShowFindInCommitFilesGitGrep.Value;
            DiffFiles.SetFindInCommitFilesGitGrepVisibility(AppSettings.ShowFindInCommitFilesGitGrep.Value);
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
            IReadOnlyList<GitRevision> revisions = _revisionGridInfo.GetSelectedRevisions();
            if (!revisions.Any(r => r.IsArtificial))
            {
                return;
            }

            DiffFiles.StoreNextItemToSelect();
            DiffFiles.InvokeAndForget(async () =>
            {
                await SetDiffsAsync(revisions);
                if (!DiffFiles.SelectedItems.Any())
                {
                    DiffFiles.SelectStoredNextItem();
                }
            });
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
            FindInCommitFilesUsingGitGrep = 17,
            GoToFirstParent = 18,
            GoToLastParent = 19,
        }

        public bool ExecuteCommand(Command cmd)
        {
            return ExecuteCommand((int)cmd);
        }

        protected override bool ExecuteCommand(int cmd)
        {
            if ((DiffFiles.FilterFilesByNameRegexFocused || DiffFiles.FindInCommitFilesGitGrepFocused) && IsTextEditKey(GetShortcutKeys(cmd)))
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
                case Command.FindInCommitFilesUsingGitGrep:
                    if (IsFileTreeMode)
                    {
                        return base.ExecuteCommand(cmd);
                    }

                    showFindInCommitFilesGitGrepDialogToolStripMenuItem.PerformClick();
                    break;
                case Command.GoToFirstParent: return ForwardToRevisionGrid(RevisionGridControl.Command.GoToFirstParent);
                case Command.GoToLastParent: return ForwardToRevisionGrid(RevisionGridControl.Command.GoToLastParent);
                default: return base.ExecuteCommand(cmd);
            }

            return true;

            bool ForwardToRevisionGrid(RevisionGridControl.Command command)
            {
                if (DiffFiles.Focused
                    && FindForm() is FormBrowse formBrowse
                    && formBrowse.RevisionGridControl.ExecuteCommand(command))
                {
                    DiffFiles.Focus();
                    return true;
                }

                return false;
            }

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

        internal IScriptOptionsProvider? ScriptOptionsProvider => GetScriptOptionsProvider();

        protected override IScriptOptionsProvider? GetScriptOptionsProvider()
        {
            return new ScriptOptionsProvider(DiffFiles, () => BlameControl.Visible ? BlameControl.CurrentFileLine : DiffText.CurrentFileLine);
        }

        public void ReloadHotkeys()
        {
            LoadHotkeys(HotkeySettingsName);
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
            showFindInCommitFilesGitGrepDialogToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.FindInCommitFilesUsingGitGrep);

            DiffText.ReloadHotkeys();
        }

        public void LoadCustomDifftools()
        {
            List<CustomDiffMergeTool> menus =
            [
                new(firstToSelectedToolStripMenuItem, firstToSelectedToolStripMenuItem_Click),
                new(selectedToLocalToolStripMenuItem, selectedToLocalToolStripMenuItem_Click),
                new(firstToLocalToolStripMenuItem, firstToLocalToolStripMenuItem_Click),
                new(diffWithRememberedDifftoolToolStripMenuItem, diffWithRememberedDiffToolToolStripMenuItem_Click),
                new(diffTwoSelectedDifftoolToolStripMenuItem, diffTwoSelectedDiffToolToolStripMenuItem_Click)
            ];

            new CustomDiffMergeToolProvider().LoadCustomDiffMergeTools(Module, menus, components, isDiff: true, cancellationToken: _customDiffToolsSequence.Next());
        }

        public void CancelLoadCustomDifftools()
        {
            _customDiffToolsSequence.CancelCurrent();
        }

        #endregion

        public void DisplayDiffTab(IReadOnlyList<GitRevision> revisions)
        {
            DiffFiles.InvokeAndForget(async () =>
            {
                await SetDiffsAsync(revisions);

                // Select something by default
                if (!DiffFiles.SelectedItems.Any())
                {
                    DiffFiles.SelectFirstVisibleItem();
                }
            });
        }

        /// <summary>
        ///  Selects the file or folder matching the passed relative path.
        /// </summary>
        /// <param name="relativePath">The relative POSIX path to the item or folder.</param>
        public void SelectFileOrFolder(Action focusView, RelativePath relativePath, int? line = null, bool? requestBlame = null)
        {
            if (requestBlame is not null)
            {
                blameToolStripMenuItem.Checked = requestBlame.Value;
            }

            bool found = DiffFiles.SelectFileOrFolder(relativePath, notify: false);
            _lastExplicitlySelectedItem = relativePath;
            _lastExplicitlySelectedItemLine = line;

            // Switch to view (and load file tree if not already done)
            focusView();

            if (found)
            {
                ShowSelectedFile(line: line);
                _lastExplicitlySelectedItemLine = null;
            }
        }

        /// <summary>
        /// Gets or sets the file in the list to select initially.
        /// When switching commits, the last selected file is "followed" if available in the new commit,
        /// this file is used as a fallback.
        /// </summary>
        public RelativePath? FallbackFollowedFile
        {
            get => _fallbackFollowedFile;
            set
            {
                _fallbackFollowedFile = value;
                _lastExplicitlySelectedItem = null;
                _lastExplicitlySelectedItemLine = null;
            }
        }

        /// <summary>
        ///  Gets whether this control is showing the file tree in contrast to showing diffs.
        /// </summary>
        // The RevisionDiff has a companion RevisionFileTree, but the latter has none.
        private bool IsFileTreeMode => _revisionFileTree is null;

        private async Task SetDiffsAsync(IReadOnlyList<GitRevision> revisions)
        {
            Validates.NotNull(_revisionGridInfo);
            CancellationToken cancellationToken = _setDiffSequence.Next();

            _viewChangesSequence.CancelCurrent();
            await this.SwitchToMainThreadAsync(cancellationToken);
            await DiffText.ClearAsync();

            RelativePath? prevDiffItem = DiffFiles.SelectedFolder
                ?? (DiffFiles.SelectedItem is FileStatusItem prevSelectedItem && DiffFiles.FirstGroupItems.Contains(prevSelectedItem) ? RelativePath.From(prevSelectedItem.Item.Name) : null);

            try
            {
                _isImplicitListSelection = true;

                await DiffFiles.SetDiffsAsync(revisions, _revisionGridInfo.CurrentCheckout, cancellationToken);

                // First try the last item explicitly selected
                if (_lastExplicitlySelectedItem is not null && DiffFiles.SelectFileOrFolder(_lastExplicitlySelectedItem, firstGroupOnly: true, notify: false))
                {
                    ShowSelectedFile(line: _lastExplicitlySelectedItemLine);
                    _lastExplicitlySelectedItemLine = null;
                    return;
                }

                // Second go back to the filtered file
                if (FallbackFollowedFile is not null && DiffFiles.SelectFileOrFolder(FallbackFollowedFile, firstGroupOnly: true, notify: true))
                {
                    return;
                }

                // Third try to restore the previous item
                if (prevDiffItem is not null && DiffFiles.SelectFileOrFolder(prevDiffItem, firstGroupOnly: true, notify: true))
                {
                    return;
                }
            }
            finally
            {
                ThreadHelper.FileAndForget(async () =>
                {
                    // DiffFiles_SelectedIndexChanged is called asynchronously with throttling. _isImplicitListSelection must not be reset before.
                    await Task.Delay(FileStatusList.SelectedIndexChangeThrottleDuration + TimeSpan.FromSeconds(1), cancellationToken);
                    await this.SwitchToMainThreadAsync(cancellationToken);
                    _isImplicitListSelection = false;
                });
            }
        }

        public void Bind(IRevisionGridInfo revisionGridInfo, IRevisionGridUpdate revisionGridUpdate, RevisionDiffControl? revisionFileTree, Func<string>? pathFilter, Action? refreshGitStatus, bool requestBlame = false)
        {
            _revisionGridInfo = revisionGridInfo;
            _revisionGridUpdate = revisionGridUpdate;
            _revisionFileTree = revisionFileTree;
            _pathFilter = pathFilter;
            _refreshGitStatus = refreshGitStatus;
            blameToolStripMenuItem.Checked = requestBlame;
            DiffFiles.Bind(RefreshArtificial, canAutoRefresh: true, objectId => DescribeRevision(objectId), _revisionGridInfo.GetActualRevision, IsFileTreeMode);
            if (IsFileTreeMode)
            {
                showFindInCommitFilesGitGrepToolStripMenuItem.Visible = false;
                showFindInCommitFilesGitGrepDialogToolStripMenuItem.Visible = false;
            }
        }

        public void InitSplitterManager(SplitterManager splitterManager)
        {
            NestedSplitterManager nested = new(splitterManager, Name);
            nested.AddSplitter(DiffSplitContainer);
            nested.AddSplitter(LeftSplitContainer);
        }

        public SplitContainer HorizontalSplitter => DiffSplitContainer;

        protected override void OnRuntimeLoad()
        {
            base.OnRuntimeLoad();

            DiffText.SetFileLoader(GetNextPatchFile);
            DiffText.Font = AppSettings.FixedWidthFont;

            ReloadHotkeys();
            LoadCustomDifftools();
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

            GitRevision revision = _revisionGridInfo.GetRevision(objectId);

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
                1 => DescribeRevision(parents[0]?.ObjectId, 50),
                > 1 => _multipleDescription.Text,
                _ => null
            };
        }

        private bool GetNextPatchFile(bool searchBackward, bool loop, out FileStatusItem? selectedItem, out Task loadFileContent)
        {
            selectedItem = null;
            loadFileContent = Task.CompletedTask;

            FileStatusItem prevItem = DiffFiles.SelectedItem;
            selectedItem = DiffFiles.SelectNextItem(searchBackward, loop, notify: false);
            if (selectedItem is null || (!loop && selectedItem == prevItem))
            {
                return false;
            }

            loadFileContent = ShowSelectedFileDiffAsync(ensureNoSwitchToFilter: false, line: 0);
            return true;
        }

        private static ContextMenuSelectionInfo GetSelectionInfo(FileStatusItem[] selectedItems, RelativePath? selectedFolder, bool isBareRepository, bool supportLinePatching, IFullPathResolver fullPathResolver)
        {
            // Some items are not supported if more than one revision is selected
            List<GitRevision> revisions = selectedItems.SecondRevs().ToList();
            GitRevision? selectedRev = revisions.Count == 1 ? revisions[0] : null;

            // First (A) is parent if one revision selected or if parent, then selected
            List<ObjectId> parentIds = selectedItems.FirstIds().ToList();

            // Combined diff, range diff etc are for display only, no manipulations
            bool isStatusOnly = selectedItems.Any(item => item.Item.IsRangeDiff || item.Item.IsStatusOnly);
            bool isDisplayOnlyDiff = parentIds.Contains(ObjectId.CombinedDiffId) || isStatusOnly;
            int selectedGitItemCount = selectedItems.Length;

            bool isAnyTracked = selectedItems.Any(item => item.Item.IsTracked);
            bool isAnyIndex = selectedItems.Any(item => item.Item.Staged == StagedStatus.Index);
            bool isAnyWorkTree = selectedItems.Any(item => item.Item.Staged == StagedStatus.WorkTree);
            bool supportPatches = selectedGitItemCount == 1 && supportLinePatching;
            bool isDeleted = selectedItems.Any(item => item.Item.IsDeleted);
            bool isAnySubmodule = selectedItems.Any(item => item.Item.IsSubmodule);
            (bool allFilesExist, bool allDirectoriesExist, bool allFilesOrUntrackedDirectoriesExist) = FileOrUntrackedDirExists(selectedItems, fullPathResolver);

            ContextMenuSelectionInfo selectionInfo = new(
                SelectedRevision: selectedRev,
                SelectedFolder: selectedFolder,
                IsDisplayOnlyDiff: isDisplayOnlyDiff,
                IsStatusOnly: isStatusOnly,
                SelectedGitItemCount: selectedGitItemCount,
                IsAnyItemIndex: isAnyIndex,
                IsAnyItemWorkTree: isAnyWorkTree,
                IsBareRepository: isBareRepository,
                AllFilesExist: allFilesExist,
                AllDirectoriesExist: allDirectoriesExist,
                AllFilesOrUntrackedDirectoriesExist: allFilesOrUntrackedDirectoriesExist,
                IsAnyTracked: isAnyTracked,
                SupportPatches: supportPatches,
                IsDeleted: isDeleted,
                IsAnySubmodule: isAnySubmodule);
            return selectionInfo;

            static (bool allFilesExist, bool allDirectoriesExist, bool allFilesOrUntrackedDirectoriesExist) FileOrUntrackedDirExists(FileStatusItem[] items, IFullPathResolver fullPathResolver)
            {
                bool allFilesExist = items.Length != 0;
                bool allDirectoriesExist = allFilesExist;
                bool allFilesOrUntrackedDirectoriesExist = allFilesExist;
                foreach (FileStatusItem item in items)
                {
                    string path = fullPathResolver.Resolve(item.Item.Name);
                    bool fileExists = File.Exists(path);
                    bool directoryExists = Directory.Exists(path);
                    allFilesExist &= fileExists;
                    allDirectoriesExist &= directoryExists;
                    bool fileOrUntrackedDirectoryExists = fileExists || (!item.Item.IsTracked && allDirectoriesExist);
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
            FileStatusItem[] selectedItems = [.. DiffFiles.SelectedItems];

            if (selectedItems.Length == 0)
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

                    GitItemStatus[] resetItems = [.. resetToParent
                        ? selectedItems.Items()
                        : selectedItems.Items().Select(item => item.InvertStatus())];
                    Module.ResetChanges(id, resetItems, resetAndDelete: resetAndDelete, _fullPathResolver, out StringBuilder output, progressAction: null);

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
            if (ensureNoSwitchToFilter && (DiffFiles.FilterFilesByNameRegexFocused || DiffFiles.FindInCommitFilesGitGrepFocused))
            {
                BlameControl.Focus();
            }

            GitRevision rev = DiffFiles.SelectedItem.SecondRevision.IsArtificial
                ? _revisionGridInfo.GetActualRevision(_revisionGridInfo.CurrentCheckout)
                : DiffFiles.SelectedItem.SecondRevision;
            await BlameControl.LoadBlameAsync(rev, children: null, DiffFiles.SelectedItem.Item.Name, _revisionGridInfo, revisionGridFileUpdate: this,
                controlToMask: null, DiffText.Encoding, line, cancellationTokenSequence: _viewChangesSequence);
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
            if (ensureNoSwitchToFilter && (DiffFiles.FilterFilesByNameRegexFocused || DiffFiles.FindInCommitFilesGitGrepFocused))
            {
                DiffText.Focus();
            }

            await DiffText.ViewChangesAsync(DiffFiles.SelectedItem,
                line: line,
                forceFileView: IsFileTreeMode,
                openWithDiffTool: IsFileTreeMode ? null : firstToSelectedToolStripMenuItem.PerformClick,
                additionalCommandInfo: (DiffFiles.SelectedItem?.Item?.IsRangeDiff is true) && Module.GitVersion.SupportRangeDiffPath ? _pathFilter() : "",
                cancellationToken: _viewChangesSequence.Next());
        }

        /// <summary>
        /// Show selected item as diff or blame
        /// </summary>
        private void ShowSelectedFile(bool ensureNoSwitchToFilter = false, int? line = null)
        {
            DiffText.InvokeAndForget(() =>
                DiffFiles.SelectedFolder is RelativePath relativePath
                    ? ShowSelectedFolderAsync(relativePath)
                    : blameToolStripMenuItem.Checked
                        ? ShowSelectedFileBlameAsync(ensureNoSwitchToFilter, line)
                        : ShowSelectedFileDiffAsync(ensureNoSwitchToFilter, line));
        }

        private Task ShowSelectedFolderAsync(RelativePath relativePath)
        {
            (string path, string description) = GetDescription(relativePath, DiffFiles.SelectedItems.ToArray());
            BlameControl.Visible = false;
            DiffText.Visible = true;
            return DiffText.ViewTextAsync(fileName: path, text: description);

            static (string Path, string Text) GetDescription(RelativePath relativePath, FileStatusItem[] items)
            {
                string path = relativePath.Value;
                int nameStartIndex = path.Length;
                if (!path.EndsWith(PathUtil.PosixDirectorySeparatorChar))
                {
                    path += PathUtil.PosixDirectorySeparatorChar;
                    if (path.Length > 1)
                    {
                        ++nameStartIndex;
                    }
                }

                StringBuilder description = new();
                description.Append('(').Append(items.Length).Append(") ").AppendLine(path);
                foreach (FileStatusItem item in items)
                {
                    description.AppendLine().Append(item.Item.Name[nameStartIndex..]);
                }

                return (path, description.ToString());
            }
        }

        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Switch to diff if the selection changes (but not for file tree mode)
            GitItemStatus? item = DiffFiles.SelectedGitItem;
            if (!IsFileTreeMode && blameToolStripMenuItem.Checked && item is not null && item.Name != _selectedBlameItem?.Name)
            {
                blameToolStripMenuItem.Checked = false;
            }

            // If this is not occurring after a revision change (implicit selection)
            // save the selected item so it can be the "preferred" selection
            if (!_isImplicitListSelection)
            {
                _lastExplicitlySelectedItem = DiffFiles.SelectedFolder
                    ?? (item is not null && !item.IsRangeDiff ? RelativePath.From(item.Name) : null);
                _lastExplicitlySelectedItemLine = null;
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
                DiffFiles.InvokeAndForget(DiffFiles.OpenSubmoduleAsync);
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
            int? line = DiffText.Visible ? DiffText.CurrentFileLine : BlameControl.CurrentFileLine;
            ShowSelectedFile(ensureNoSwitchToFilter: true, line);
        }

        private void DiffText_PatchApplied(object sender, EventArgs e)
        {
            RequestRefresh();
        }

        private void diffShowInFileTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!IsFileTreeMode)
            {
                OpenInFileTreeTab(requestBlame: false);
            }
        }

        private void diffFilterFileInGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string pathFilter = DiffFiles.SelectedFolder is RelativePath relativePath
                ? relativePath.Value
                : string.Join(" ", DiffFiles.SelectedItems.Select(f => f.Item.Name.ToPosixPath().QuoteNE()));
            (FindForm() as FormBrowse)?.SetPathFilter(pathFilter);
        }

        private void UpdateStatusOfMenuItems()
        {
            ContextMenuSelectionInfo selectionInfo = GetSelectionInfo(DiffFiles.SelectedItems.ToArray(), DiffFiles.SelectedFolder, isBareRepository: Module.IsBareRepository(), supportLinePatching: DiffText.SupportLinePatching, _fullPathResolver);

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

            foreach (FileStatusItem item in DiffFiles.SelectedItems)
            {
                string? filePath = _fullPathResolver.Resolve(item.Item.Name);
                if (filePath is not null && FormBrowseUtil.FileOrParentDirectoryExists(filePath))
                {
                    openContainingFolderToolStripMenuItem.Enabled = true;
                    break;
                }
            }

            // Visibility of FileTree is not known, assume (CommitInfoTabControl.Contains(TreeTabPage);)
            diffShowInFileTreeToolStripMenuItem.Visible = !IsFileTreeMode && _revisionDiffController.ShouldShowMenuShowInFileTree(selectionInfo);
            diffFilterFileInGridToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuFileHistory(selectionInfo);
            fileHistoryDiffToolstripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuFileHistory(selectionInfo);
            blameToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuBlame(selectionInfo);
            if (!blameToolStripMenuItem.Enabled)
            {
                blameToolStripMenuItem.Checked = false;
            }

            toolStripSeparatorScript.Visible = DiffContextMenu.AddUserScripts(runScriptToolStripMenuItem, ExecuteCommand, script => script.OnEvent == ScriptEvent.ShowInFileList, UICommands);

            showFindInCommitFilesGitGrepToolStripMenuItem.Checked = DiffFiles.FindInCommitFilesGitGrepVisible;
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

            if (IsFileTreeMode || AppSettings.UseDiffViewerForBlame.Value)
            {
                int? line = DiffText.Visible ? DiffText.CurrentFileLine : BlameControl.CurrentFileLine;
                blameToolStripMenuItem.Checked = !blameToolStripMenuItem.Checked;
                _selectedBlameItem = blameToolStripMenuItem.Checked ? DiffFiles.SelectedItem.Item : null;
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

            RelativePath name = DiffFiles.SelectedFolder ?? RelativePath.From(DiffFiles.SelectedItems.First().Item.Name);
            int line = DiffText.Visible ? DiffText.CurrentFileLine : BlameControl.CurrentFileLine;
            Action focusView = () => (FindForm() as FormBrowse)?.ExecuteCommand(FormBrowse.Command.FocusFileTree);
            _revisionFileTree.SelectFileOrFolder(focusView, name, line, requestBlame);
        }

        private void StageFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            StageFiles();
        }

        private void StageFiles()
        {
            List<GitItemStatus> files = DiffFiles.SelectedItems.Where(item => item.Item.Staged == StagedStatus.WorkTree).Select(i => i.Item).ToList();

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
            IReadOnlyList<GitItemStatus> candidates = DiffFiles.GitItemStatuses;

            IEnumerable<GitItemStatus> FindDiffFilesMatches(string name)
            {
                Func<string?, bool> predicate = _findFilePredicateProvider.Get(name, Module.WorkingDir);
                return candidates.Where(item => predicate(item.Name) || predicate(item.OldName));
            }

            GitItemStatus? selectedItem;
            using (SearchWindow<GitItemStatus> searchWindow = new(FindDiffFilesMatches)
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

        private void showFindInCommitFilesGitGrepDialogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DiffFiles.ShowFindInCommitFileGitGrepDialog(DiffText.GetSelectedText());
        }

        private void fileHistoryDiffToolstripMenuItem_Click(object sender, EventArgs e)
        {
            (string? fileName, GitRevision? revision) = DiffFiles.SelectedFolder is RelativePath relativePath
                ? (relativePath.Length == 0 ? null : relativePath.Value, _revisionGridInfo?.GetRevision(_revisionGridInfo.CurrentCheckout))
                : DiffFiles.SelectedItem is FileStatusItem item && item.Item.IsTracked
                    ? (item.Item.Name, item.SecondRevision)
                    : (null, null);
            if (fileName is null)
            {
                return;
            }

            UICommands.StartFileHistoryDialog(this, fileName, revision);
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
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item?.DropDownItems != null)
            {
                // "main menu" clicked, cancel dropdown manually, invoke default mergetool
                item.HideDropDown();
                item.Owner.Hide();
            }

            string toolName = item?.Tag as string;
            OpenFilesWithDiffTool(diffKind, toolName);
        }

        private void OpenFilesWithDiffTool(RevisionDiffKind diffKind, string? toolName = null)
        {
            using (WaitCursorScope.Enter())
            {
                foreach (FileStatusItem item in DiffFiles.SelectedItems)
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
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item?.DropDownItems != null)
            {
                // "main menu" clicked, cancel dropdown manually, invoke default difftool
                item.HideDropDown();
            }

            string toolName = item?.Tag as string;
            List<FileStatusItem> diffFiles = DiffFiles.SelectedItems.ToList();
            if (diffFiles.Count != 2)
            {
                return;
            }

            // The order is always the order in the list, not clicked order, but the (last) selected is known
            int firstIndex = DiffFiles.FocusedItem == diffFiles[0] ? 1 : 0;
            int secondIndex = 1 - firstIndex;

            // Fallback to first revision if second revision cannot be used
            bool isFirstItemSecondRev = _rememberFileContextMenuController.ShouldEnableFirstItemDiff(diffFiles[firstIndex], isSecondRevision: true);
            string first = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash, diffFiles[firstIndex], isSecondRevision: isFirstItemSecondRev);
            bool isSecondItemSecondRev = _rememberFileContextMenuController.ShouldEnableSecondItemDiff(diffFiles[secondIndex], isSecondRevision: true);
            string second = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash, diffFiles[secondIndex], isSecondRevision: isSecondItemSecondRev);

            Module.OpenFilesWithDifftool(first, second, customTool: toolName);
        }

        private void diffWithRememberedDiffToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item?.DropDownItems != null)
            {
                // "main menu" clicked, cancel dropdown manually, invoke default difftool
                item.HideDropDown();
            }

            string? toolName = item?.Tag as string;

            // For first item, the second revision is explicitly remembered
            string first = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash,
                _rememberFileContextMenuController.RememberedDiffFileItem, isSecondRevision: true);

            // Fallback to first revision if second cannot be used
            bool isSecond = _rememberFileContextMenuController.ShouldEnableSecondItemDiff(DiffFiles.SelectedItem, isSecondRevision: true);
            string second = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash, DiffFiles.SelectedItem, isSecondRevision: isSecond);

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
                string name = DiffFiles.SelectedItem.Item.OldName;
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

            string? fileName = _fullPathResolver.Resolve(DiffFiles.SelectedItem.Item.Name);
            int? lineNumber = BlameControl.Visible ? BlameControl.CurrentFileLine : DiffText.CurrentFileLine;
            UICommands.StartFileEditorDialog(fileName, lineNumber: lineNumber);
            RequestRefresh();
        }

        private void diffOpenWorkingDirectoryFileWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem is null)
            {
                return;
            }

            string fileName = _fullPathResolver.Resolve(DiffFiles.SelectedItem.Item.Name);

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
            FileStatusItem item = DiffFiles.SelectedItem;
            if (item?.Item.Name is null)
            {
                return;
            }

            ThreadHelper.FileAndForget(async () =>
            {
                ObjectId blob = Module.GetFileBlobHash(item.Item.Name, item.SecondRevision.ObjectId);

                if (blob is null)
                {
                    return;
                }

                string fileName = PathUtil.GetFileName(item.Item.Name);
                fileName = (Path.GetTempPath() + fileName).ToNativePath();
                await Module.SaveBlobAsAsync(fileName, blob.ToString());

                onSaved(fileName);
            });
        }

        private ContextMenuDiffToolInfo GetContextMenuDiffToolInfo()
        {
            // Some items are not supported if more than one revision is selected
            List<GitRevision> revisions = DiffFiles.SelectedItems.SecondRevs().ToList();
            GitRevision? selectedRev = revisions.Count == 1 ? revisions[0] : null;

            List<ObjectId> parentIds = DiffFiles.SelectedItems.FirstIds().ToList();
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
            List<GitRevision> revisions = DiffFiles.SelectedItems.SecondRevs().ToList();

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

            List<FileStatusItem> diffFiles = DiffFiles.SelectedItems.ToList();
            diffRememberStripSeparator.Visible = diffFiles.Count == 1 || diffFiles.Count == 2;

            // The order is always the order in the list, not clicked order, but the (last) selected is known
            int firstIndex = diffFiles.Count == 2 && DiffFiles.FocusedItem == diffFiles[0] ? 1 : 0;
            int secondIndex = 1 - firstIndex;

            diffTwoSelectedDifftoolToolStripMenuItem.Visible = diffFiles.Count == 2;
            diffTwoSelectedDifftoolToolStripMenuItem.Enabled =
                diffFiles.Count == 2
                && _rememberFileContextMenuController.ShouldEnableFirstItemDiff(diffFiles[firstIndex])
                && _rememberFileContextMenuController.ShouldEnableSecondItemDiff(diffFiles[secondIndex]);

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
                FileStatusItem[] selected = [.. DiffFiles.SelectedItems];
                if (selected.Length == 0 || !selected[0].SecondRevision.IsArtificial ||
                    MessageBox.Show(this, _deleteSelectedFiles.Text, _deleteSelectedFilesCaption.Text, MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) !=
                    DialogResult.Yes)
                {
                    return false;
                }

                DiffFiles.StoreNextItemToSelect();

                try
                {
                    DeleteFromFilesystem(selected);
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
            List<string> submodules = DiffFiles.SelectedItems.Where(it => it.Item.IsSubmodule).Select(it => it.Item.Name).Distinct().ToList();

            foreach (string name in submodules)
            {
                IGitUICommands submodulCommands = UICommands.WithWorkingDirectory(_fullPathResolver.Resolve(name.EnsureTrailingPathSeparator()));
                submodulCommands.StartCommitDialog(this);
            }

            RequestRefresh();
        }

        private void diffResetSubmoduleChanges_Click(object sender, EventArgs e)
        {
            List<string> submodules = DiffFiles.SelectedItems.Where(it => it.Item.IsSubmodule).Select(it => it.Item.Name).Distinct().ToList();

            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(this, true, true);
            if (resetType == FormResetChanges.ActionEnum.Cancel)
            {
                return;
            }

            foreach (string name in submodules)
            {
                IGitModule module = Module.GetSubmodule(name);
                module.ResetAllChanges(clean: resetType == FormResetChanges.ActionEnum.ResetAndDelete);
            }

            RequestRefresh();
        }

        private void diffUpdateSubmoduleMenuItem_Click(object sender, EventArgs e)
        {
            List<string> submodules = DiffFiles.SelectedItems.Where(it => it.Item.IsSubmodule).Select(it => it.Item.Name).Distinct().ToList();

            FormProcess.ShowDialog(FindForm() as FormBrowse, UICommands, arguments: Commands.SubmoduleUpdate(submodules), Module.WorkingDir, input: null, useDialogSettings: true);
            RequestRefresh();
        }

        private void diffStashSubmoduleChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> submodules = DiffFiles.SelectedItems.Where(it => it.Item.IsSubmodule).Select(it => it.Item.Name).Distinct().ToList();

            foreach (string name in submodules)
            {
                IGitUICommands uiCmds = UICommands.WithGitModule(Module.GetSubmodule(name));
                uiCmds.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash);
            }

            RequestRefresh();
        }

        private void showFindInCommitFilesGitGrepToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.ShowFindInCommitFilesGitGrep.Value = showFindInCommitFilesGitGrepToolStripMenuItem.Checked;
            DiffFiles.SetFindInCommitFilesGitGrepVisibility(AppSettings.ShowFindInCommitFilesGitGrep.Value);
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
                    && ((DiffText.Visible && DiffText.ProcessHotkey(keyData))
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
            FileStatusItem[] items = [.. DiffFiles.SelectedItems];

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

        bool IRevisionGridFileUpdate.SelectFileInRevision(ObjectId commitId, RelativePath filename)
        {
            _lastExplicitlySelectedItem = filename;
            _lastExplicitlySelectedItemLine = null;
            return _revisionGridUpdate.SetSelectedRevision(commitId);
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
