using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtUtils;
using GitUI.ScriptsEngine;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using Keys = GitExtensions.Shims.WinForms.Keys;

namespace GitUI.CommandsDialogs;

public partial class RevisionDiffControl : GitModuleControl, IRevisionGridFileUpdate
{
    private IRevisionGridInfo? _revisionGridInfo;
    private IRevisionGridUpdate? _revisionGridUpdate;
    private Func<string>? _pathFilter;

    private readonly FileStatusDiffCalculator _diffCalculator;
    private RevisionDiffControl? _revisionFileTree;

    // Avalonia's designer constructs views before the application initializes ThreadHelper.
    private readonly TaskManager _taskManager = GitUI.Compat.DesignTimeTaskManager.Create();
    private readonly CancellationTokenSequence _viewChangesSequence = new();
    private readonly CancellationTokenSequence _setDiffSequence = new();
    private Action? _refreshGitStatus;
    private GitItemStatus? _selectedBlameItem;
    private RelativePath? _fallbackFollowedFile;
    private RelativePath? _lastExplicitlySelectedItem;
    private int? _lastExplicitlySelectedItemLine;
    private RelativePath? _prevDiffItem;
    private int? _toBeSelectedItemLine;
    private bool _isImplicitListSelection;
    private bool _updatingDiffs;
    private bool _nestedViewerRuntimeStateApplied;

    public RevisionDiffControl()
    {
        InitializeComponent();

        _diffCalculator = new FileStatusDiffCalculator(() => Module);
        DiffFiles.SelectionMode = SelectionMode.Multiple;
        DiffFiles.CanUseFindInCommitFilesGitGrep = true;
        DiffFiles.SelectedIndexChanged += DiffFiles_SelectedIndexChanged;
        DiffFiles.DoubleClick += DiffFiles_DoubleClick;
        DiffFiles.DataSourceChanged += DiffFiles_DataSourceChanged;
        DiffText.LinePatchingBlocksUntilReload = true;
        DiffText.ExtraDiffArgumentsChanged += DiffText_ExtraDiffArgumentsChanged;
        DiffText.PatchApplied += DiffText_PatchApplied;
        DiffText.TopScrollReached += FileViewer_TopScrollReached;
        DiffText.BottomScrollReached += FileViewer_BottomScrollReached;
        BlameControl.HideCommitInfo();
        AttachedToVisualTree += (_, _) => InitializeNestedViewerRuntimeState();
        LayoutUpdated += (_, _) => InitializeNestedViewerRuntimeState();

        InitializeComplete();
    }

    private void FileViewer_TopScrollReached(object? sender, EventArgs e)
    {
        DiffFiles.SelectPreviousVisibleItem();
        DiffText.ScrollToBottom();
    }

    private void FileViewer_BottomScrollReached(object? sender, EventArgs e)
    {
        DiffFiles.SelectNextVisibleItem();
        DiffText.ScrollToTop();
    }

    private void InitializeNestedViewerRuntimeState()
    {
        if (_nestedViewerRuntimeStateApplied || !IsEffectivelyVisible || !TryGetUICommandsDirect(out _))
        {
            return;
        }

        // WinForms loads the nested editors when this tab page loads, including its hidden
        // blame pane. Avalonia can keep the tab detached until selected, so pass the owning
        // commands source at that boundary rather than when the Browse form is constructed.
        IGitUICommandsSource source = UICommandsSource;
        if (!DiffText.TryGetUICommandsDirect(out _))
        {
            DiffText.UICommandsSource = source;
        }

        if (!BlameControl.TryGetUICommandsDirect(out _))
        {
            BlameControl.UICommandsSource = source;
        }

        DiffText.InitializeRuntimeState();
        if (IsFileTreeMode)
        {
            // The file-tree page loads both hidden blame editors with its diff viewer;
            // the ordinary Diff page leaves its blame pane in the pre-load state.
            BlameControl.InitializeNestedViewerRuntimeState();
        }

        OnRuntimeLoad();
        _nestedViewerRuntimeStateApplied = true;
    }

    public void RepositoryChanged() => DiffFiles.RepositoryChanged();

    private IReadOnlyList<GitRevision> _displayedRevisions = [];
    private bool _showBlame;

    public void RefreshArtificial()
    {
        if (!IsEffectivelyVisible || _revisionGridInfo is null)
        {
            return;
        }

        IReadOnlyList<GitRevision> revisions = _revisionGridInfo.GetSelectedRevisions();
        if (!revisions.Any(revision => revision.IsArtificial))
        {
            return;
        }

        if (!_updatingDiffs)
        {
            DiffFiles.StoreNextItemToSelect();
        }

        _taskManager.FileAndForget(async () =>
        {
            await SetDiffsAsync(revisions);
            if (!DiffFiles.SelectedItems.Any())
            {
                DiffFiles.SelectStoredNextItem(orSelectFirst: true);
            }
        });
    }

    public static readonly string HotkeySettingsName = "BrowseDiff";

    internal FileStatusList FileStatusList => DiffFiles;
    internal Editor.FileViewer FileViewer => DiffText;

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
        FindInCommitFilesUsingGitGrep_DiffTab = 17,
        GoToFirstParent = 18,
        GoToLastParent = 19,
        OpenWorkingDirectoryFile = 20,
        OpenInVisualStudio = 21,
        AddFileToGitIgnore = 22,
        RenameMove = 23,
        FindInCommitFilesUsingGitGrep_FileTreeTab = 24,
    }

    internal GitRevision? DisplayedRevision { get; private set; }

    internal IScriptOptionsProvider ScriptOptionsProvider => GetScriptOptionsProvider();

    public void ReloadHotkeys()
    {
        LoadHotkeys(HotkeySettingsName);
        DiffFiles.ReloadHotkeys();
        DiffText.ReloadHotkeys();
    }

    public void LoadCustomDifftools() => DiffFiles.LoadCustomDifftools();

    public void CancelLoadCustomDifftools() => DiffFiles.CancelLoadCustomDifftools();

    public bool ExecuteCommand(Command cmd)
        => ExecuteCommand((int)cmd);

    protected override bool ExecuteCommand(int cmd)
    {
        if ((Command)cmd == Command.SelectFirstGroupChanges)
        {
            // If no other subcontrol than DiffFiles is focused, focus DiffFiles and let it select all changes of the first diff group
            if (IsKeyboardFocusWithin && !DiffFiles.IsKeyboardFocusWithin)
            {
                return false;
            }

            DiffFiles.Focus();
        }

        switch ((Command)cmd)
        {
            case Command.GoToFirstParent: return ForwardToRevisionGrid(RevisionGridControl.Command.GoToFirstParent);
            case Command.GoToLastParent: return ForwardToRevisionGrid(RevisionGridControl.Command.GoToLastParent);

            case Command.DeleteSelectedFiles:
            case Command.ShowHistory:
            case Command.Blame:
            case Command.OpenWithDifftool:
            case Command.EditFile:
            case Command.OpenAsTempFile:
            case Command.OpenAsTempFileWith:
            case Command.OpenWithDifftoolFirstToLocal:
            case Command.OpenWithDifftoolSelectedToLocal:
            case Command.ResetSelectedFiles:
            case Command.StageSelectedFile:
            case Command.UnStageSelectedFile:
            case Command.ShowFileTree:
            case Command.FilterFileInGrid:
            case Command.SelectFirstGroupChanges:
            case Command.FindFile:
            case Command.OpenWorkingDirectoryFileWith:
            case Command.FindInCommitFilesUsingGitGrep_DiffTab:
            case Command.FindInCommitFilesUsingGitGrep_FileTreeTab:
            case Command.OpenWorkingDirectoryFile:
            case Command.OpenInVisualStudio:
            case Command.AddFileToGitIgnore:
            case Command.RenameMove:
                return DiffFiles.ExecuteCommand((Command)cmd);

            default: return base.ExecuteCommand(cmd);
        }

        bool ForwardToRevisionGrid(RevisionGridControl.Command command)
        {
            if (DiffFiles.IsKeyboardFocusWithin
                && TopLevel.GetTopLevel(this) is FormBrowse formBrowse
                && formBrowse.RevisionGridControl.ExecuteCommand(command))
            {
                DiffFiles.Focus();
                return true;
            }

            return false;
        }
    }

    public override bool ProcessHotkey(Keys keyData)
    {
        // The Avalonia file list owns the shared BrowseDiff hotkey table.
        return DiffFiles.ProcessHotkey(keyData)
            || base.ProcessHotkey(keyData)
            || (!ResourceManager.GitExtensionsControl.IsTextEditKey(keyData)
                && ((DiffText.IsEffectivelyVisible && DiffText.ProcessHotkey(keyData))
                    || (BlameControl.IsEffectivelyVisible && BlameControl.ProcessHotkey(keyData))));
    }

    protected override IScriptOptionsProvider GetScriptOptionsProvider()
    {
        return new ScriptOptionsProvider(
            DiffFiles,
            () => BlameControl.IsVisible ? BlameControl.CurrentFileLine : DiffText.CurrentFileLine,
            () => BlameControl.IsVisible ? BlameControl.CurrentFileColumn : DiffText.CurrentFileColumn);
    }

    public void DisplayDiffTab(IReadOnlyList<GitRevision> revisions)
    {
        _displayedRevisions = revisions;
        _taskManager.FileAndForget(async () =>
        {
            await SetDiffsAsync(revisions);
            if (!DiffFiles.SelectedItems.Any())
            {
                DiffFiles.SelectFirstVisibleItem();
            }
        });
    }

    private async Task SetDiffsAsync(IReadOnlyList<GitRevision> revisions)
    {
        if (_revisionGridInfo is null)
        {
            return;
        }

        CancellationToken cancellationToken = _setDiffSequence.Next();
        _viewChangesSequence.CancelCurrent();
        await _taskManager.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        await DiffText.ClearAsync();

        if (!_updatingDiffs)
        {
            _updatingDiffs = true;
            _prevDiffItem = DiffFiles.SelectedFolder
                ?? (DiffFiles.SelectedItem is FileStatusItem previous
                    && DiffFiles.FirstGroupItems.Contains(previous)
                        ? RelativePath.From(previous.Item.Name)
                        : null);
        }

        try
        {
            _isImplicitListSelection = true;
            DiffFiles.Clear();
            BlameControl.IsVisible = false;
            DiffText.IsVisible = true;
            DisplayedRevision = null;
            if (revisions.Count == 0)
            {
                return;
            }

            await TaskScheduler.Default;
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<FileStatusWithDescription> groups;
            if (IsFileTreeMode)
            {
                _diffCalculator.SetDiff(revisions, _revisionGridInfo.CurrentCheckout, allowMultiDiff: false);
                _diffCalculator.SetGrep(string.Empty, fileTreeMode: true);
                groups = _diffCalculator.Calculate([], refreshDiff: false, refreshGrep: true, cancellationToken);
            }
            else
            {
                _diffCalculator.SetDiff(revisions, _revisionGridInfo.CurrentCheckout, allowMultiDiff: true);
                groups = _diffCalculator.Calculate([], refreshDiff: true, refreshGrep: false, cancellationToken);
            }

            await _taskManager.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!_displayedRevisions.SequenceEqual(revisions))
            {
                return;
            }

            DiffFiles.SetDiffs(groups, IsFileTreeMode);
            DisplayedRevision = revisions[0];

            // First try the last item explicitly selected.
            if (_lastExplicitlySelectedItem is not null
                && DiffFiles.SelectFileOrFolder(_lastExplicitlySelectedItem, firstGroupOnly: true, notify: true))
            {
                _toBeSelectedItemLine = _lastExplicitlySelectedItemLine;
                _lastExplicitlySelectedItemLine = null;
                return;
            }

            // Second go back to the filtered file.
            if (FallbackFollowedFile is not null
                && DiffFiles.SelectFileOrFolder(FallbackFollowedFile, firstGroupOnly: true, notify: true))
            {
                return;
            }

            // Third try to restore the previous item.
            if (_prevDiffItem is not null
                && DiffFiles.SelectFileOrFolder(_prevDiffItem, firstGroupOnly: true, notify: true))
            {
                return;
            }
        }
        finally
        {
            _taskManager.FileAndForget(async () =>
            {
                // Selection notifications are throttled; retain the implicit marker until they drain.
                await Task.Delay(FileStatusList.SelectedIndexChangeThrottleDuration + TimeSpan.FromSeconds(1), cancellationToken);
                await _taskManager.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                _isImplicitListSelection = false;
            });
        }
    }

    /// <summary>
    /// Selects a repository file or folder and then focuses this view.
    /// </summary>
    public void SelectFileOrFolder(Action focusView, RelativePath relativePath, int? line = null, bool? requestBlame = null)
    {
        if (requestBlame.HasValue)
        {
            _showBlame = requestBlame.Value;
            DiffFiles.tsmiBlame.IsChecked = _showBlame;
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
    /// Gets or sets the file selected when the previously followed file is unavailable.
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

    public void Clear()
    {
        _setDiffSequence.CancelCurrent();
        _viewChangesSequence.CancelCurrent();
        _displayedRevisions = [];
        DisplayedRevision = null;
        DiffFiles.Clear();
        DiffText.ViewPatch(string.Empty);
    }

    internal void CancelBackgroundTasks()
    {
        Clear();
        BlameControl.CancelBackgroundTasks();
        _taskManager.JoinPendingOperations();
    }

    /// <summary>
    ///  Gets whether this control is showing the file tree in contrast to showing diffs.
    /// </summary>
    // The RevisionDiff has a companion RevisionFileTree, but the latter has none.
    private bool IsFileTreeMode => _revisionFileTree is null;

    public void Bind(
        IRevisionGridInfo revisionGridInfo,
        IRevisionGridUpdate revisionGridUpdate,
        RevisionDiffControl? revisionFileTree,
        Func<string>? pathFilter,
        Action? refreshGitStatus,
        bool requestBlame = false)
    {
        _revisionGridInfo = revisionGridInfo;
        _revisionGridUpdate = revisionGridUpdate;
        _revisionFileTree = revisionFileTree;
        _pathFilter = pathFilter;
        _refreshGitStatus = refreshGitStatus;
        _showBlame = requestBlame;
        DiffFiles.tsmiBlame.ToggleType = MenuItemToggleType.CheckBox;
        DiffFiles.tsmiBlame.IsChecked = _showBlame;
        _diffCalculator.DescribeRevision = objectId => DescribeRevision(objectId);
        _diffCalculator.GetActualRevision = revisionGridInfo.GetActualRevision;
        DiffFiles.Bind(
            RefreshArtificial,
            canAutoRefresh: true,
            objectId => DescribeRevision(objectId),
            revisionGridInfo.GetActualRevision,
            IsFileTreeMode);
        DiffFiles.BindContextMenu(
            blame: BlameFile,
            cherryPickChanges: DiffText.CherryPickAllChanges,
            filterFileInGrid: FilterFileInGrid,
            refreshParent: RequestRefresh,
            openInFileTreeTab_AsBlame: revisionFileTree is null ? null : OpenInFileTreeTab,
            getCurrentRevision: () => DisplayedRevision,
            getLineNumber: () => BlameControl.IsVisible ? BlameControl.CurrentFileLine : DiffText.CurrentFileLine,
            getSelectedText: DiffText.GetSelectedText,
            getSupportLinePatching: () => DiffText.SupportLinePatching);
    }

    public void InitSplitterManager(SplitterManager splitterManager)
    {
        NestedSplitterManager nested = new(splitterManager, Name ?? nameof(RevisionDiffControl));
        nested.AddSplitter(DiffSplitContainer);
        BlameControl.InitSplitterManager(nested);
    }

    public Grid HorizontalSplitter => DiffSplitContainer;

    protected void OnRuntimeLoad()
    {
        DiffText.SetFileLoader(GetNextPatchFile);
        DiffText.Font = AppSettings.FixedWidthFont;

        ReloadHotkeys();
        LoadCustomDifftools();
    }

    private string DescribeRevision(ObjectId objectId, int maxLength = 0)
    {
        if (objectId.IsZero)
        {
            // No parent at all, present as working directory
            return ResourceManager.TranslatedStrings.Workspace;
        }

        if (_revisionGridInfo is null)
        {
            return objectId.ToShortString();
        }

        GitRevision? revision = _revisionGridInfo.GetRevision(objectId);
        return revision is null ? objectId.ToShortString() : _revisionGridInfo.DescribeRevision(revision, maxLength);
    }

    private bool GetNextPatchFile(bool searchBackward, bool loop, out FileStatusItem? selectedItem, out Task loadFileContent)
    {
        loadFileContent = Task.CompletedTask;

        FileStatusItem? previousItem = DiffFiles.SelectedItem;
        selectedItem = DiffFiles.SelectNextItem(searchBackward, loop, notify: false);
        if (selectedItem is null || (!loop && selectedItem == previousItem))
        {
            return false;
        }

        loadFileContent = ShowSelectedFileDiffAsync(ensureNoSwitchToFilter: false, line: 0);
        return true;
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

    /// <summary>
    /// Show the file in the BlameViewer if Blame is visible.
    /// </summary>
    /// <param name="line">The line to start at.</param>
    /// <returns>a task</returns>
    private async Task ShowSelectedFileBlameAsync(bool ensureNoSwitchToFilter, int? line)
    {
        FileStatusItem? selectedItem = DiffFiles.SelectedItem;
        if (selectedItem is null)
        {
            await ShowSelectedFileDiffAsync(ensureNoSwitchToFilter, line);
            return;
        }

        BlameControl.IsVisible = true;
        DiffText.IsVisible = false;

        // Avoid that focus is switched to the file filter after changing visibility.
        if (ensureNoSwitchToFilter && (DiffFiles.FilterFilesByNameRegexFocused || DiffFiles.FindInCommitFilesGitGrepFocused))
        {
            BlameControl.Focus();
        }

        GitRevision revision = selectedItem.SecondRevision.IsArtificial
            ? _revisionGridInfo!.GetActualRevision(_revisionGridInfo.CurrentCheckout)!
            : selectedItem.SecondRevision;
        Encoding encoding = DiffText.Encoding ?? Module.FilesEncoding;
        await BlameControl.LoadBlameAsync(
            revision,
            selectedItem.Item.Name,
            _revisionGridInfo,
            this,
            encoding,
            line,
            cancellationTokenSequence: _viewChangesSequence,
            joinableTaskFactory: _taskManager.JoinableTaskFactory);
    }

    /// <summary>
    /// Show selected item as a file diff.
    /// Activate diffviewer if Blame is visible.
    /// </summary>
    /// <returns>a task</returns>
    private async Task ShowSelectedFileDiffAsync(bool ensureNoSwitchToFilter, int? line)
    {
        BlameControl.IsVisible = false;
        DiffText.IsVisible = true;

        // Avoid that focus is switched to the file filter after changing visibility.
        if (ensureNoSwitchToFilter && (DiffFiles.FilterFilesByNameRegexFocused || DiffFiles.FindInCommitFilesGitGrepFocused))
        {
            DiffText.FocusViewer();
        }

        FileStatusItem? item = DiffFiles.SelectedItems.Contains(DiffFiles.FocusedItem)
            ? DiffFiles.FocusedItem
            : DiffFiles.SelectedItems.FirstOrDefault();
        string additionalCommandInfo = item?.Item.IsRangeDiff is true && Module.GitVersion.SupportRangeDiffPath
            ? _pathFilter?.Invoke() ?? string.Empty
            : string.Empty;
        await GitUIExtensions.ViewChangesAsync(
            DiffText,
            item,
            _viewChangesSequence.Next(),
            line,
            openWithDiffTool: IsFileTreeMode ? null : () => DiffFiles.ExecuteCommand(Command.OpenWithDifftool),
            additionalCommandInfo: additionalCommandInfo,
            forceFileView: IsFileTreeMode && !DiffFiles.FindInCommitFilesGitGrepActive);
    }

    /// <summary>
    /// Show selected item as diff or blame.
    /// </summary>
    private void ShowSelectedFile(bool ensureNoSwitchToFilter = false, int? line = null)
    {
        _taskManager.FileAndForget(async () =>
        {
            await (DiffFiles.SelectedFolder is RelativePath relativePath
                ? ShowSelectedFolderAsync(relativePath)
                : DiffFiles.tsmiBlame.IsChecked
                    ? ShowSelectedFileBlameAsync(ensureNoSwitchToFilter, line)
                    : ShowSelectedFileDiffAsync(ensureNoSwitchToFilter, line));
            _toBeSelectedItemLine = null;
            _updatingDiffs = false;
        });
    }

    private Task ShowSelectedFolderAsync(RelativePath relativePath)
    {
        (string path, string description) = GetDescription(relativePath, [.. DiffFiles.SelectedItems]);
        BlameControl.IsVisible = false;
        DiffText.IsVisible = true;
        return DiffText.ViewTextAsync(path, description, _viewChangesSequence.Next());

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

    private void DiffFiles_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (DiffFiles.AllItemsCount == 0)
        {
            BlameControl.IsVisible = false;
            DiffText.IsVisible = true;
            DiffText.ViewPatch(string.Empty);
            return;
        }

        // Switch to diff if the selection changes (but not for file tree mode)
        GitItemStatus? item = DiffFiles.SelectedGitItem;
        if (!IsFileTreeMode && _showBlame && item is not null && item.Name != _selectedBlameItem?.Name)
        {
            _showBlame = false;
            DiffFiles.tsmiBlame.IsChecked = false;
        }

        // If this is not occurring after a revision change (implicit selection),
        // save the selected item so it can be the preferred selection.
        if (!_isImplicitListSelection)
        {
            _lastExplicitlySelectedItem = DiffFiles.SelectedFolder
                ?? (item is not null && !item.IsRangeDiff ? RelativePath.From(item.Name) : null);
            _lastExplicitlySelectedItemLine = null;
            _selectedBlameItem = null;
        }

        _isImplicitListSelection = false;
        ShowSelectedFile(line: _toBeSelectedItemLine);
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
            _taskManager.FileAndForget(DiffFiles.OpenSubmoduleAsync);
        }
        else
        {
            UICommands.StartFileHistoryDialog(this, item.Item.Name, item.SecondRevision);
        }
    }

    private void DiffFiles_DataSourceChanged(object sender, EventArgs e)
    {
        if (!DiffFiles.GitItemStatuses.Any())
        {
            DiffText.ViewPatch(string.Empty);
        }
    }

    private void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        => ShowSelectedFile(ensureNoSwitchToFilter: true);

    private void DiffText_PatchApplied(object sender, EventArgs e)
        => RequestRefresh();

    private void FilterFileInGrid()
    {
        string pathFilter = DiffFiles.SelectedFolder is RelativePath relativePath
            ? relativePath.Value
            : string.Join(" ", DiffFiles.SelectedItems.Select(item => item.Item.Name.ToPosixPath().QuoteNE()));
        (TopLevel.GetTopLevel(this) as FormBrowse)?.SetPathFilter(pathFilter);
    }

    private void BlameFile()
    {
        GitItemStatus? item = DiffFiles.SelectedItem?.Item;
        if (item is null || !item.IsTracked)
        {
            return;
        }

        if (IsFileTreeMode || AppSettings.UseDiffViewerForBlame.Value)
        {
            int line = BlameControl.IsVisible ? BlameControl.CurrentFileLine : DiffText.CurrentFileLine;
            _showBlame = !_showBlame;
            DiffFiles.tsmiBlame.IsChecked = _showBlame;
            _selectedBlameItem = _showBlame ? item : null;
            ShowSelectedFile(ensureNoSwitchToFilter: true, line);
            return;
        }

        _showBlame = false;
        DiffFiles.tsmiBlame.IsChecked = false;
        OpenInFileTreeTab(requestBlame: true);
    }

    /// <summary>
    /// Open the selected item in the FileTree tab
    /// </summary>
    /// <param name="requestBlame">Request that Blame is shown in the FileTree</param>
    private void OpenInFileTreeTab(bool requestBlame)
    {
        if (_revisionFileTree is null)
        {
            return;
        }

        RelativePath? path = DiffFiles.SelectedFolder
            ?? DiffFiles.SelectedItems.Select(item => RelativePath.From(item.Item.Name)).FirstOrDefault();
        if (path is null)
        {
            return;
        }

        int line = BlameControl.IsVisible ? BlameControl.CurrentFileLine : DiffText.CurrentFileLine;
        Action focusView = () => (TopLevel.GetTopLevel(this) as FormBrowse)?.ExecuteCommand(FormBrowse.Command.FocusFileTree);
        _revisionFileTree.SelectFileOrFolder(focusView, path, line, requestBlame);
    }

    public void SwitchFocus(bool alreadyContainedFocus)
    {
        if (alreadyContainedFocus && DiffFiles.IsKeyboardFocusWithin)
        {
            if (BlameControl.IsVisible)
            {
                BlameControl.Focus();
            }
            else
            {
                DiffText.FocusViewer();
            }
        }
        else
        {
            DiffFiles.Focus();
        }
    }

    internal void RegisterGitHostingPluginInBlameControl()
    {
        BlameControl.ConfigureRepositoryHostPlugin(
            PluginRegistry.TryGetGitHosterForModule(Module));
    }

    bool IRevisionGridFileUpdate.SelectFileInRevision(ObjectId commitId, RelativePath filename)
    {
        _lastExplicitlySelectedItem = filename;
        _lastExplicitlySelectedItemLine = null;
        return _revisionGridUpdate!.SetSelectedRevision(commitId);
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor(RevisionDiffControl control)
    {
        public FileStatusList DiffFiles => control.DiffFiles;
        public Editor.FileViewer DiffText => control.DiffText;
        public Grid DiffSplitContainer => control.DiffSplitContainer;
    }
}
