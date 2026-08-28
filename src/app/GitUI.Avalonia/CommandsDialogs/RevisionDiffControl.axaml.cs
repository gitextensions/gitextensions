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

namespace GitUI.CommandsDialogs;

public sealed partial class RevisionDiffControl : GitModuleControl, IRevisionGridFileUpdate
{
    private IRevisionGridInfo? _revisionGridInfo;
    private IRevisionGridUpdate? _revisionGridUpdate;

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

    public RevisionDiffControl()
    {
        InitializeComponent();

        _diffCalculator = new FileStatusDiffCalculator(() => Module);
        DiffFiles.SelectionMode = SelectionMode.Multiple;
        DiffFiles.Bind(RefreshArtificial);
        DiffFiles.SelectedIndexChanged += DiffFiles_SelectedIndexChanged;
        DiffFiles.DoubleClick += (_, _) => ShowSelectedFile();
        DiffText.LinePatchingBlocksUntilReload = true;
        DiffText.ExtraDiffArgumentsChanged += (_, _) => ShowSelectedFile();
        DiffText.PatchApplied += (_, _) => RequestRefresh();
        DiffText.TopScrollReached += (_, _) =>
        {
            DiffFiles.SelectPreviousVisibleItem();
            DiffText.ScrollToBottom();
        };
        DiffText.BottomScrollReached += (_, _) =>
        {
            DiffFiles.SelectNextVisibleItem();
            DiffText.ScrollToTop();
        };
        BlameControl.HideCommitInfo();

        InitializeComplete();
    }

    private RelativePath? _previousItem;

    public void RepositoryChanged()
    {
        if (_displayedRevisions.Count > 0)
        {
            DisplayDiffTab(_displayedRevisions);
        }
    }

    private IReadOnlyList<GitRevision> _displayedRevisions = [];
    private bool _showBlame;

    public void RefreshArtificial()
    {
        if (_displayedRevisions.Any(revision => revision.IsArtificial))
        {
            DisplayDiffTab(_displayedRevisions);
        }
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

    public bool ExecuteCommand(Command command)
        => command is Command.GoToFirstParent or Command.GoToLastParent
            ? false
            : DiffFiles.ExecuteCommand(command);

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
        _previousItem = DiffFiles.SelectedRelativePath;
        DiffFiles.Clear();
        DiffText.ViewPatch(string.Empty);
        BlameControl.IsVisible = false;
        DiffText.IsVisible = true;
        DisplayedRevision = null;

        if (revisions.Count == 0 || _revisionGridInfo is null)
        {
            return;
        }

        CancellationToken cancellationToken = _setDiffSequence.Next();
        _taskManager.FileAndForget(async () =>
        {
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
            RelativePath? itemToSelect = _lastExplicitlySelectedItem ?? FallbackFollowedFile ?? _previousItem;

            // Select something by default
            if (itemToSelect is null || !DiffFiles.SelectFileOrFolder(itemToSelect, notify: true))
            {
                DiffFiles.SelectFirstVisibleItem();
            }
        });
    }

    /// <summary>
    /// Selects a repository file or folder and then focuses this view.
    /// </summary>
    public void SelectFileOrFolder(Action focusView, RelativePath relativePath, int? line = null, bool? requestBlame = null)
    {
        _lastExplicitlySelectedItem = relativePath;
        if (requestBlame.HasValue)
        {
            _showBlame = requestBlame.Value;
            DiffFiles.tsmiBlame.IsChecked = _showBlame;
        }

        bool found = DiffFiles.SelectFileOrFolder(relativePath, notify: false);

        // Switch to view (and load file tree if not already done)
        focusView();
        if (found)
        {
            ShowSelectedFile(line);
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
    internal bool IsFileTreeMode => _revisionFileTree is null;

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
        _refreshGitStatus = refreshGitStatus;
        _showBlame = requestBlame;
        DiffFiles.tsmiBlame.ToggleType = MenuItemToggleType.CheckBox;
        DiffFiles.tsmiBlame.IsChecked = _showBlame;
        _diffCalculator.DescribeRevision = objectId => DescribeRevision(objectId);
        _diffCalculator.GetActualRevision = revisionGridInfo.GetActualRevision;
        DiffFiles.BindContextMenu(
            blame: BlameFile,
            cherryPickChanges: DiffText.CherryPickAllChanges,
            filterFileInGrid: FilterFileInGrid,
            refreshParent: RequestRefresh,
            openInFileTreeTab_AsBlame: revisionFileTree is null ? null : OpenInFileTreeTab,
            getCurrentRevision: () => DisplayedRevision,
            getLineNumber: () => BlameControl.IsVisible ? BlameControl.CurrentFileLine : DiffText.CurrentFileLine,
            getSelectedText: null,
            getSupportLinePatching: () => DiffText.SupportLinePatching);
    }

    public void InitSplitterManager(SplitterManager splitterManager)
    {
        NestedSplitterManager nested = new(splitterManager, Name ?? nameof(RevisionDiffControl));
        nested.AddSplitter(DiffSplitContainer);
        BlameControl.InitSplitterManager(nested);
    }

    private string DescribeRevision(ObjectId objectId)
    {
        if (_revisionGridInfo is null)
        {
            return objectId.ToShortString();
        }

        GitRevision? revision = _revisionGridInfo.GetRevision(objectId);
        return revision is null ? objectId.ToShortString() : _revisionGridInfo.DescribeRevision(revision);
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
    private async Task ShowSelectedFileBlameAsync(FileStatusItem selectedItem, int? line)
    {
        BlameControl.IsVisible = true;
        DiffText.IsVisible = false;

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

    private void ShowSelectedFile(int? line = null)
    {
        FileStatusItem? selectedItem = DiffFiles.SelectedFileStatusItem;
        RelativePath? selectedFolder = DiffFiles.SelectedFolder;
        if (selectedFolder is not null)
        {
            CancellationToken folderCancellationToken = _viewChangesSequence.Next();
            BlameControl.IsVisible = false;
            DiffText.IsVisible = true;
            string prefix = selectedFolder.Value + PathUtil.PosixDirectorySeparatorChar;
            string description = string.Join(
                Environment.NewLine,
                DiffFiles.GitItemStatuses
                    .Where(item => item.Name.StartsWith(prefix, StringComparison.Ordinal))
                    .Select(item => item.Name));
            _taskManager.FileAndForget(() => DiffText.ViewTextAsync(selectedFolder.Value, description, folderCancellationToken));
            return;
        }

        if (selectedItem is null)
        {
            _viewChangesSequence.CancelCurrent();
            BlameControl.IsVisible = false;
            DiffText.IsVisible = true;
            DiffText.ViewPatch(string.Empty);
            return;
        }

        if (_showBlame)
        {
            _taskManager.FileAndForget(() => ShowSelectedFileBlameAsync(selectedItem, line));
            return;
        }

        CancellationToken cancellationToken = _viewChangesSequence.Next();
        BlameControl.IsVisible = false;
        DiffText.IsVisible = true;
        _taskManager.FileAndForget(async () =>
        {
            if (IsFileTreeMode)
            {
                await DiffText.ViewGitItemAsync(selectedItem, cancellationToken: cancellationToken);
            }
            else
            {
                await DiffText.ViewChangesAsync(selectedItem, cancellationToken);
            }

            if (line is > 0)
            {
                await _taskManager.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                DiffText.GoToLine(line.Value);
            }
        });
    }

    private void DiffFiles_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Switch to diff if the selection changes (but not for file tree mode)
        GitItemStatus? item = DiffFiles.SelectedGitItem;

        // If this is not occurring after a revision change (implicit selection)
        // save the selected item so it can be the "preferred" selection
        if (!IsFileTreeMode && _showBlame && item is not null && item.Name != _selectedBlameItem?.Name)
        {
            _showBlame = false;
            DiffFiles.tsmiBlame.IsChecked = false;
        }

        _selectedBlameItem = null;
        ShowSelectedFile();
    }

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
            ShowSelectedFile(line);
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
        return _revisionGridUpdate!.SetSelectedRevision(commitId);
    }
}
