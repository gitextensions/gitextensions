using System.Text;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.ScriptsEngine;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs;

public partial class RevisionDiffControl : GitModuleControl, IRevisionGridFileUpdate
{
    private IRevisionGridInfo? _revisionGridInfo;
    private IRevisionGridUpdate? _revisionGridUpdate;
    private Func<string>? _pathFilter;
    private RevisionDiffControl? _revisionFileTree;
    private readonly CancellationTokenSequence _viewChangesSequence = new();
    private readonly CancellationTokenSequence _setDiffSequence = new();
    private Action? _refreshGitStatus;
    private GitItemStatus? _selectedBlameItem;
    private RelativePath? _fallbackFollowedFile;
    private RelativePath? _lastExplicitlySelectedItem;
    private int? _lastExplicitlySelectedItemLine;
    private RelativePath? _prevDiffItem;
    private int? _toBeSelectedItemLine;
    private bool _isImplicitListSelection = false;
    private bool _updatingDiffs = false;

    public RevisionDiffControl()
    {
        InitializeComponent();
        InitializeComplete();
        HotkeysEnabled = true;
        DiffFiles.CanUseFindInCommitFilesGitGrep = true;
        DiffText.TopScrollReached += FileViewer_TopScrollReached;
        DiffText.BottomScrollReached += FileViewer_BottomScrollReached;
        DiffText.LinePatchingBlocksUntilReload = true;
        BlameControl.HideCommitInfo();
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

    public void RepositoryChanged() => DiffFiles.RepositoryChanged();

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

        if (!_updatingDiffs)
        {
            DiffFiles.StoreNextItemToSelect();
        }

        DiffFiles.InvokeAndForget(async () =>
        {
            await SetDiffsAsync(revisions);
            if (!DiffFiles.SelectedItems.Any())
            {
                DiffFiles.SelectStoredNextItem(orSelectFirst: true);
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
        FindInCommitFilesUsingGitGrep_DiffTab = 17,
        GoToFirstParent = 18,
        GoToLastParent = 19,
        OpenWorkingDirectoryFile = 20,
        OpenInVisualStudio = 21,
        AddFileToGitIgnore = 22,
        RenameMove = 23,
        FindInCommitFilesUsingGitGrep_FileTreeTab = 24,
    }

    public bool ExecuteCommand(Command cmd)
    {
        return ExecuteCommand((int)cmd);
    }

    protected override bool ExecuteCommand(int cmd)
    {
        if ((Command)cmd == Command.SelectFirstGroupChanges)
        {
            // If no other subcontrol than DiffFiles is focused, focus DiffFiles and let it select all changes of the first diff group
            if (ContainsFocus && !DiffFiles.Focused)
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
            if (DiffFiles.Focused
                && FindForm() is FormBrowse formBrowse
                && formBrowse.RevisionGridControl.ExecuteCommand(command))
            {
                DiffFiles.Focus();
                return true;
            }

            return false;
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

        DiffFiles.ReloadHotkeys();
        DiffText.ReloadHotkeys();
    }

    #endregion

    public void LoadCustomDifftools() => DiffFiles.LoadCustomDifftools();

    public void CancelLoadCustomDifftools() => DiffFiles.CancelLoadCustomDifftools();

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
            DiffFiles.tsmiBlame.Checked = requestBlame.Value;
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

        if (!_updatingDiffs)
        {
            _updatingDiffs = true;
            _prevDiffItem = DiffFiles.SelectedFolder
                ?? (DiffFiles.SelectedItem is FileStatusItem prevSelectedItem && DiffFiles.FirstGroupItems.Contains(prevSelectedItem) ? RelativePath.From(prevSelectedItem.Item.Name) : null);
        }

        try
        {
            _isImplicitListSelection = true;

            await DiffFiles.SetDiffsAsync(revisions, _revisionGridInfo.CurrentCheckout, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            // Warning: Do not directly *show* a file here by means of "notify: false" and ShowSelectedFile()!
            // Reason: There is a pending throttled SelectedIndexChanged notification (from clearing the selection in SetDiffsAsync) which will update a second time and can discard the line number.

            // First try the last item explicitly selected
            if (_lastExplicitlySelectedItem is not null && DiffFiles.SelectFileOrFolder(_lastExplicitlySelectedItem, firstGroupOnly: true, notify: true))
            {
                _toBeSelectedItemLine = _lastExplicitlySelectedItemLine;
                _lastExplicitlySelectedItemLine = null;
                return;
            }

            // Second go back to the filtered file
            if (FallbackFollowedFile is not null && DiffFiles.SelectFileOrFolder(FallbackFollowedFile, firstGroupOnly: true, notify: true))
            {
                return;
            }

            // Third try to restore the previous item
            if (_prevDiffItem is not null && DiffFiles.SelectFileOrFolder(_prevDiffItem, firstGroupOnly: true, notify: true))
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
        DiffFiles.tsmiBlame.Checked = requestBlame;
        DiffFiles.Bind(RefreshArtificial, canAutoRefresh: true, objectId => DescribeRevision(objectId), _revisionGridInfo.GetActualRevision, IsFileTreeMode);
        DiffFiles.BindContextMenu(
            blame: BlameFile,
            cherryPickChanges: DiffText.CherryPickAllChanges,
            filterFileInGrid: FilterFileInGrid,
            openInFileTreeTab_AsBlame: OpenInFileTreeTab,
            refreshParent: RequestRefresh,
            getCurrentRevision: () => _revisionGridInfo?.GetRevision(_revisionGridInfo.CurrentCheckout),
            getLineNumber: () => BlameControl.Visible ? BlameControl.CurrentFileLine : DiffText.CurrentFileLine,
            getSelectedText: DiffText.GetSelectedText,
            getSupportLinePatching: () => DiffText.SupportLinePatching);
    }

    public void InitSplitterManager(SplitterManager splitterManager)
    {
        NestedSplitterManager nested = new(splitterManager, Name);
        nested.AddSplitter(DiffSplitContainer);
        nested.AddSplitter(LeftSplitContainer);
        BlameControl.InitSplitterManager(nested);
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

        FileStatusItem? item = DiffFiles.SelectedItem;
        await DiffText.ViewChangesAsync(item,
            line: line,
            forceFileView: IsFileTreeMode && !DiffFiles.FindInCommitFilesGitGrepActive,
            openWithDiffTool: IsFileTreeMode ? null : DiffFiles.tsmiDiffFirstToSelected.PerformClick,
            additionalCommandInfo: (item?.Item?.IsRangeDiff is true) && Module.GitVersion.SupportRangeDiffPath ? _pathFilter() : "",
            cancellationToken: _viewChangesSequence.Next());
    }

    /// <summary>
    /// Show selected item as diff or blame
    /// </summary>
    private void ShowSelectedFile(bool ensureNoSwitchToFilter = false, int? line = null)
    {
        DiffText.InvokeAndForget(async () =>
        {
            await (DiffFiles.SelectedFolder is RelativePath relativePath
                ? ShowSelectedFolderAsync(relativePath)
                : DiffFiles.tsmiBlame.Checked
                    ? ShowSelectedFileBlameAsync(ensureNoSwitchToFilter, line)
                    : ShowSelectedFileDiffAsync(ensureNoSwitchToFilter, line));
            _toBeSelectedItemLine = null;
            _updatingDiffs = false;
        });
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
        if (DiffFiles.AllItemsCount == 0)
        {
            BlameControl.Visible = false;
            DiffText.Visible = true;
            DiffText.Clear();
            return;
        }

        // Switch to diff if the selection changes (but not for file tree mode)
        GitItemStatus? item = DiffFiles.SelectedGitItem;
        if (!IsFileTreeMode && DiffFiles.tsmiBlame.Checked && item is not null && item.Name != _selectedBlameItem?.Name)
        {
            DiffFiles.tsmiBlame.Checked = false;
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
        ShowSelectedFile(ensureNoSwitchToFilter: true);
    }

    private void DiffText_PatchApplied(object sender, EventArgs e)
    {
        RequestRefresh();
    }

    private void FilterFileInGrid()
    {
        string pathFilter = DiffFiles.SelectedFolder is RelativePath relativePath
            ? relativePath.Value
            : string.Join(" ", DiffFiles.SelectedItems.Select(f => f.Item.Name.ToPosixPath().QuoteNE()));
        (FindForm() as FormBrowse)?.SetPathFilter(pathFilter);
    }

    private void BlameFile()
    {
        FileStatusItem? item = DiffFiles.SelectedItem;
        if (item is null || !item.Item.IsTracked)
        {
            return;
        }

        if (IsFileTreeMode || AppSettings.UseDiffViewerForBlame.Value)
        {
            int? line = DiffText.Visible ? DiffText.CurrentFileLine : BlameControl.CurrentFileLine;
            DiffFiles.tsmiBlame.Checked = !DiffFiles.tsmiBlame.Checked;
            _selectedBlameItem = DiffFiles.tsmiBlame.Checked ? DiffFiles.SelectedItem.Item : null;
            ShowSelectedFile(ensureNoSwitchToFilter: true, line);
            return;
        }

        DiffFiles.tsmiBlame.Checked = false;
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

        public FileStatusList DiffFiles => _control.DiffFiles;
        public Editor.FileViewer DiffText => _control.DiffText;
        public SplitContainer DiffSplitContainer => _control.DiffSplitContainer;
    }
}
