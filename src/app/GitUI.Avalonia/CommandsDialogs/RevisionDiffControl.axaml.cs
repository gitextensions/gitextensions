using Avalonia.Controls;
using Avalonia.Controls.Selection;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitUI.CommandsDialogs;

// Functional Avalonia twin of RevisionDiffControl. The shared diff calculator and the
// WinForms-shaped list/viewer boundary are retained; blame, context commands, and the
// remaining FileViewer modes join in their owning follow-up phases.
public sealed partial class RevisionDiffControl : GitModuleControl, IRevisionGridFileUpdate
{
    private readonly FileStatusDiffCalculator _diffCalculator;
    private readonly CancellationTokenSequence _setDiffSequence = new();
    private readonly TaskManager _taskManager = ThreadHelper.CreateTaskManager();
    private readonly CancellationTokenSequence _viewChangesSequence = new();
    private IRevisionGridInfo? _revisionGridInfo;
    private IRevisionGridUpdate? _revisionGridUpdate;
    private RevisionDiffControl? _revisionFileTree;
    private Action? _refreshGitStatus;
    private RelativePath? _fallbackFollowedFile;
    private RelativePath? _lastExplicitlySelectedItem;
    private RelativePath? _previousItem;
    private IReadOnlyList<GitRevision> _displayedRevisions = [];

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

        InitializeComplete();
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

    internal FileStatusList FileStatusList => DiffFiles;
    internal Editor.FileViewer FileViewer => DiffText;
    internal bool IsFileTreeMode => _revisionFileTree is null;
    internal GitRevision? DisplayedRevision { get; private set; }

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
        _diffCalculator.DescribeRevision = objectId => DescribeRevision(objectId);
        _diffCalculator.GetActualRevision = revisionGridInfo.GetActualRevision;
        DiffFiles.BindContextMenu(
            blame: null,
            cherryPickChanges: DiffText.CherryPickAllChanges,
            filterFileInGrid: FilterFileInGrid,
            refreshParent: RequestRefresh,
            openInFileTreeTab_AsBlame: revisionFileTree is null ? null : OpenInFileTreeTab,
            getCurrentRevision: () => DisplayedRevision,
            getLineNumber: () => DiffText.CurrentFileLine,
            getSelectedText: null,
            getSupportLinePatching: () => DiffText.SupportLinePatching);
    }

    public void DisplayDiffTab(IReadOnlyList<GitRevision> revisions)
    {
        _displayedRevisions = revisions;
        _previousItem = DiffFiles.SelectedRelativePath;
        DiffFiles.Clear();
        DiffText.ViewPatch(string.Empty);
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

            await this.SwitchToMainThreadAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!_displayedRevisions.SequenceEqual(revisions))
            {
                return;
            }

            DiffFiles.SetDiffs(groups, IsFileTreeMode);
            DisplayedRevision = revisions[0];
            RelativePath? itemToSelect = _lastExplicitlySelectedItem ?? FallbackFollowedFile ?? _previousItem;
            if (itemToSelect is null || !DiffFiles.SelectFileOrFolder(itemToSelect, notify: true))
            {
                DiffFiles.SelectFirstVisibleItem();
            }
        });
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
        _taskManager.JoinPendingOperations();
    }

    public void RepositoryChanged()
    {
        if (_displayedRevisions.Count > 0)
        {
            DisplayDiffTab(_displayedRevisions);
        }
    }

    public void RefreshArtificial()
    {
        if (_displayedRevisions.Any(revision => revision.IsArtificial))
        {
            DisplayDiffTab(_displayedRevisions);
        }
    }

    /// <summary>
    /// Selects a repository file or folder and then focuses this view.
    /// </summary>
    public void SelectFileOrFolder(Action focusView, RelativePath relativePath, int? line = null, bool? requestBlame = null)
    {
        _lastExplicitlySelectedItem = relativePath;
        bool found = DiffFiles.SelectFileOrFolder(relativePath, notify: false);
        focusView();
        if (found)
        {
            ShowSelectedFile(line);
        }
    }

    public void SwitchFocus(bool alreadyContainedFocus)
    {
        if (alreadyContainedFocus && DiffFiles.IsKeyboardFocusWithin)
        {
            DiffText.FocusViewer();
        }
        else
        {
            DiffFiles.FocusFiles();
        }
    }

    bool IRevisionGridFileUpdate.SelectFileInRevision(ObjectId commitId, RelativePath filename)
    {
        _lastExplicitlySelectedItem = filename;
        return _revisionGridUpdate!.SetSelectedRevision(commitId);
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

    private void FilterFileInGrid()
    {
        string pathFilter = DiffFiles.SelectedFolder is RelativePath relativePath
            ? relativePath.Value
            : string.Join(" ", DiffFiles.SelectedItems.Select(item => item.Item.Name.ToPosixPath().QuoteNE()));
        (TopLevel.GetTopLevel(this) as FormBrowse)?.SetPathFilter(pathFilter);
    }

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

        int line = DiffText.CurrentFileLine;
        Action focusView = () => (TopLevel.GetTopLevel(this) as FormBrowse)?.ExecuteCommand(FormBrowse.Command.FocusFileTree);
        _revisionFileTree.SelectFileOrFolder(focusView, path, line, requestBlame);
    }

    private void RequestRefresh()
    {
        _refreshGitStatus?.Invoke();
        RefreshArtificial();
    }

    private void DiffFiles_SelectedIndexChanged(object? sender, EventArgs e)
        => ShowSelectedFile();

    private void ShowSelectedFile(int? line = null)
    {
        CancellationToken cancellationToken = _viewChangesSequence.Next();
        FileStatusItem? selectedItem = DiffFiles.SelectedFileStatusItem;
        RelativePath? selectedFolder = DiffFiles.SelectedFolder;
        if (selectedFolder is not null)
        {
            string prefix = selectedFolder.Value + PathUtil.PosixDirectorySeparatorChar;
            string description = string.Join(
                Environment.NewLine,
                DiffFiles.GitItemStatuses
                    .Where(item => item.Name.StartsWith(prefix, StringComparison.Ordinal))
                    .Select(item => item.Name));
            _taskManager.FileAndForget(() => DiffText.ViewTextAsync(selectedFolder.Value, description, cancellationToken));
            return;
        }

        if (selectedItem is null)
        {
            DiffText.ViewPatch(string.Empty);
            return;
        }

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
                await this.SwitchToMainThreadAsync(cancellationToken);
                DiffText.GoToLine(line.Value);
            }
        });
    }
}
