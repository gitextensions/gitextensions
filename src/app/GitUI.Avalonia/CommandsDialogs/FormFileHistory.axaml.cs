using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Compat;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs;

public sealed partial class FormFileHistory : GitModuleForm, IRevisionGridFileUpdate
{
    private const string FormBrowseName = "FormBrowse";

    private readonly TranslationString _buildReportTabCaption = new("Build Report");
    private readonly TranslationString _fileNotFound = new(" - Git could not identify the file {0}");
    private readonly ICommitDataManager _commitDataManager;
    private readonly IFullPathResolver _fullPathResolver;
    private readonly CancellationTokenSequence _customDiffToolsSequence = new();
    private readonly CancellationTokenSequence _viewChangesSequence = new();

    // Avalonia's designer constructs views before the application initializes ThreadHelper.
    private readonly TaskManager _taskManager = GitUI.Compat.DesignTimeTaskManager.Create();
    private readonly List<Task> _viewTasks = [];
    private BuildReportTabPageExtension? _buildReportTabPageExtension;

    private string? _commitInfoTabPageText;

    private string FileName { get; init; } = string.Empty;

    public FormFileHistory()
    {
        InitializeComponent();
        _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
        _commitDataManager = new CommitDataManager(() => Module);
        ConfigureControls();
        InitializeComplete();
    }

    /// <summary>
    /// Open FileHistory form.
    /// </summary>
    /// <param name="commands">commands in the current form.</param>
    /// <param name="fileName">name in repo of file to view.</param>
    /// <param name="revision">initial selected commit.</param>
    /// <param name="filterByRevision">add filter.</param>
    /// <param name="showBlame">show blame initially instead of diff view.</param>
    public FormFileHistory(IGitUICommands commands, string fileName, GitRevision? revision = null, bool filterByRevision = false, bool showBlame = false)
        : base(commands, enablePositionRestore: true)
    {
        InitializeComponent();
        _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
        _commitDataManager = new CommitDataManager(() => Module);
        ConfigureControls();

        RevisionGrid.SelectedId = revision?.ObjectId ?? default;

        // Git paths always use forward slashes, including when the dialog is opened from
        // the file tree on Windows.
        // Replace Windows path separator to Linux path separator.
        // This is needed to keep the file history working when started from file tree in
        // browse dialog.
        FileName = fileName.RemoveQuotes().ToPosixPath();
        bool isSubmodule = GitModule.IsValidGitWorkingDir(_fullPathResolver.Resolve(FileName));
        BlameTab.IsVisible = !isSubmodule;
        toolStripBlameOptions.IsVisible = !isSubmodule;
        loadBlameOnShowToolStripMenuItem.IsEnabled = !isSubmodule;
        saveAsToolStripMenuItem.IsVisible = !isSubmodule;
        UpdateLoadMenuItems();

        if (filterByRevision && revision is not null)
        {
            ToolStripFilters.SetRevisionFilter(revision.Guid);
        }

        tabControl1.SelectedItem = !isSubmodule && showBlame ? BlameTab : DiffTab;
        InitializeComplete();
        Blame.ConfigureRepositoryHostPlugin(
            PluginRegistry.TryGetGitHosterForModule(Module));
        SetTitle();
    }

    private void ConfigureControls()
    {
        ToolStripFilters.Bind(() => Module, RevisionGrid);
        RevisionGrid.MultiSelect = true;
        RevisionGrid.ShowBuildServerInfo = true;
        RevisionGrid.FilePathByObjectId = [];
        copyToClipboardToolStripMenuItem.SetRevisionFunc(RevisionGrid.GetSelectedRevisions);

        ContextMenu? revisionContextMenu = RevisionGrid.RevisionContextMenu;
        revisionContextMenu?.Items.Remove(RevisionGrid.NavigateMenuItem);
        revisionContextMenu?.Items.Remove(RevisionGrid.ViewMenuItem);
        FileHistoryContextMenu.Items.Add(RevisionGrid.NavigateMenuItem);
        FileHistoryContextMenu.Items.Add(RevisionGrid.ViewMenuItem);
        MainPanel.ContextMenu = null;
        RevisionGrid.RevisionContextMenu = FileHistoryContextMenu;

        RevisionGrid.SelectionChanged += FileChangesSelectionChanged;
        RevisionGrid.DoubleTapped += FileChangesDoubleClick;
        tabControl1.SelectionChanged += TabControl1SelectedIndexChanged;
        CommitDiff.EscapePressed += Close;
        Diff.EscapePressed += Close;
        View.EscapePressed += Close;
        Blame.EscapePressed += Close;
        Blame.CommandClick += new System.EventHandler<ResourceManager.CommandEventArgs>(Blame_CommandClick);
        Diff.ExtraDiffArgumentsChanged += (_, _) => UpdateSelectedFileViewers();

        FileHistoryContextMenu.Opening += FileHistoryContextMenuOpening;
        openWithDifftoolToolStripMenuItem.Click += OpenWithDifftoolToolStripMenuItem_Click;
        diffToolRemoteLocalStripMenuItem.Click += diffToolRemoteLocalStripMenuItem_Click;
        saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
        revertCommitToolStripMenuItem.Click += revertCommitToolStripMenuItem_Click;
        cherryPickThisCommitToolStripMenuItem.Click += cherryPickThisCommitToolStripMenuItem_Click;
        followFileHistoryToolStripMenuItem.Click += followFileHistoryToolStripMenuItem_Click;
        followFileHistoryRenamesToolStripMenuItem.Click += followFileHistoryRenamesToolStripMenuItem_Click;
        toolStripSplitLoad.Click += toolStripSplitLoad_ButtonClick;
        loadHistoryOnShowToolStripMenuItem.Click += loadHistoryOnShowToolStripMenuItem_Click;
        loadBlameOnShowToolStripMenuItem.Click += loadBlameOnShowToolStripMenuItem_Click;
        showFullHistoryToolStripMenuItem.Click += showFullHistoryToolStripMenuItem_Click;
        simplifyMergesToolStripMenuItem.Click += simplifyMergesToolStripMenuItem_Click;
        ignoreWhitespaceToolStripMenuItem.Click += ignoreWhitespaceToolStripMenuItem_Click;
        detectMoveAndCopyInThisFileToolStripMenuItem.Click += detectMoveAndCopyInThisFileToolStripMenuItem_Click;
        detectMoveAndCopyInAllFilesToolStripMenuItem.Click += detectMoveAndCopyInAllFilesToolStripMenuItem_Click;
        displayAuthorFirstToolStripMenuItem.Click += displayAuthorFirstToolStripMenuItem_Click;
        showAuthorAvatarToolStripMenuItem.Click += showAuthorAvatarToolStripMenuItem_Click;
        showAuthorToolStripMenuItem.Click += showAuthorToolStripMenuItem_Click;
        showAuthorDateToolStripMenuItem.Click += showAuthorDateToolStripMenuItem_Click;
        showAuthorTimeToolStripMenuItem.Click += showAuthorTimeToolStripMenuItem_Click;
        showLineNumbersToolStripMenuItem.Click += showLineNumbersToolStripMenuItem_Click;
        showOriginalFilePathToolStripMenuItem.Click += showOriginalFilePathToolStripMenuItem_Click;
        gitcommandLogToolStripMenuItem.Click += GitcommandLogToolStripMenuItemClick;

        UpdateFollowHistoryMenuItems();
        UpdateHistoryMenuItems();
        UpdateLoadMenuItems();
        UpdateBlameMenuItems();
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        bool autoLoad = (ReferenceEquals(tabControl1.SelectedItem, BlameTab) && AppSettings.LoadBlameOnShow)
            || AppSettings.LoadFileHistoryOnShow;
        if (autoLoad)
        {
            LoadFileHistory();
        }
        else
        {
            RevisionGrid.IsVisible = false;
        }

        LoadCustomDifftools();
    }

    protected override void OnClosed(EventArgs e)
    {
        RevisionGrid.CancelBackgroundTasks();
        Blame.CancelBackgroundTasks();
        _customDiffToolsSequence.CancelCurrent();
        _taskManager.JoinPendingOperations();
        _viewChangesSequence.CancelCurrent();
        DispatcherPump.Wait(async () =>
        {
            await CompleteViewTasksAsync();
            return true;
        });
        _customDiffToolsSequence.Dispose();
        _viewChangesSequence.Dispose();
        base.OnClosed(e);
    }

    public void LoadCustomDifftools()
    {
        List<CustomDiffMergeTool> menus =
        [
            new(openWithDifftoolToolStripMenuItem, OpenWithDifftoolToolStripMenuItem_Click),
            new(diffToolRemoteLocalStripMenuItem, diffToolRemoteLocalStripMenuItem_Click),
        ];

        CustomDiffMergeToolProvider provider = new();
        CancellationToken cancellationToken = _customDiffToolsSequence.Next();
        _taskManager.FileAndForget(() => provider.LoadCustomDiffMergeToolsAsync(
            Module,
            menus,
            isDiff: true,
            delay: CustomDiffMergeToolProvider.FormBrowseToolDelay,
            _taskManager.JoinableTaskFactory,
            cancellationToken));
    }

    private void LoadFileHistory()
    {
        RevisionGrid.IsVisible = true;
        if (!string.IsNullOrEmpty(FileName))
        {
            bool startInitialLoad = !RevisionGrid.HasRevisionSource;
            RevisionGrid.SetAndApplyPathFilter(FileName.Quote());
            if (startInitialLoad)
            {
                RevisionGrid.ReloadRevisions(Module, selectedObjectId: RevisionGrid.SelectedId);
            }
        }
    }

    private string? GetFileNameForRevision(GitRevision rev)
    {
        ObjectId objectId = rev.IsArtificial ? RevisionGrid.CurrentCheckout : rev.ObjectId;
        return RevisionGrid.GetRevisionFileName(FileName, objectId);
    }

    private void FileChangesSelectionChanged(object? sender, EventArgs e)
        => UpdateSelectedFileViewers();

    private void SetTitle(string? alternativeFileName = null)
    {
        StringBuilder str = new StringBuilder()
            .Append("File History - ")
            .Append(FileName);

        if (!string.IsNullOrEmpty(alternativeFileName) && alternativeFileName != FileName)
        {
            str.Append(" (").Append(alternativeFileName).Append(')');
        }

        str.Append(" - ").Append(PathUtil.GetDisplayPath(Module.WorkingDir));
        Text = str.ToString();
    }

    private void UpdateSelectedFileViewers(bool force = false)
    {
        IReadOnlyList<GitRevision> selectedRevisions = RevisionGrid.GetSelectedRevisions();
        if (selectedRevisions.Count == 0)
        {
            return;
        }

        GitRevision revision = selectedRevisions[0];
        string fileName = GetFileNameForRevision(revision) ?? FileName;
        bool isFolder = fileName.EndsWith('/');
        bool fileAvailable = !isFolder && (revision.IsArtificial
            ? File.Exists(_fullPathResolver.Resolve(fileName))
            : !Module.GetFileBlobHash(fileName, revision.ObjectId).IsZero);

        SetTitle(alternativeFileName: fileName);
        _commitInfoTabPageText ??= CommitInfoTabPage.Header as string ?? "Commit";
        CommitInfoTabPage.Header = _commitInfoTabPageText
            + (isFolder || fileAvailable ? string.Empty : string.Format(_fileNotFound.Text, fileName.Quote()));

        CommitInfoTabPage.IsVisible = !revision.IsArtificial;
        DiffTab.IsVisible = fileAvailable;
        ViewTab.IsVisible = !revision.IsArtificial && fileAvailable;
        BlameTab.IsVisible = !revision.IsArtificial && fileAvailable && toolStripBlameOptions.IsVisible;

        if (tabControl1.SelectedItem is TabItem selectedTab && !selectedTab.IsVisible)
        {
            tabControl1.SelectedItem = DiffTab.IsVisible ? DiffTab : CommitInfoTabPage;
        }

        Encoding encoding = Diff.Encoding ?? Module.FilesEncoding;
        if (ReferenceEquals(tabControl1.SelectedItem, BlameTab))
        {
            TrackViewTask(Blame.LoadBlameAsync(
                revision,
                fileName,
                RevisionGrid,
                this,
                encoding,
                force: force,
                cancellationTokenSequence: _viewChangesSequence,
                joinableTaskFactory: _taskManager.JoinableTaskFactory));
        }
        else if (ReferenceEquals(tabControl1.SelectedItem, ViewTab))
        {
            View.Encoding = encoding;
            GitItemStatus file = new(name: fileName) { IsTracked = true };
            CancellationToken cancellationToken = _viewChangesSequence.Next();
            View.GetUpdateTreeId(file, revision.ObjectId, cancellationToken);
            TrackViewTask(View.ViewGitItemAsync(file, revision.ObjectId, cancellationToken: cancellationToken));
        }
        else if (ReferenceEquals(tabControl1.SelectedItem, DiffTab))
        {
            GitItemStatus file = new(name: fileName) { IsTracked = true };
            FileStatusItem item = new(
                firstRev: selectedRevisions.Count > 1 ? selectedRevisions[^1] : null,
                secondRev: revision,
                file);
            TrackViewTask(Diff.ViewChangesAsync(item, _viewChangesSequence.Next()));
        }
        else if (ReferenceEquals(tabControl1.SelectedItem, CommitInfoTabPage))
        {
            CommitDiff.SetRevision(revision.ObjectId, fileName);
        }

        _buildReportTabPageExtension ??= new BuildReportTabPageExtension(
            () => Module,
            tabControl1,
            _buildReportTabCaption.Text);
        _buildReportTabPageExtension.FillBuildReport(selectedRevisions.Count == 1 ? revision : null);
    }

    private void TabControl1SelectedIndexChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ReferenceEquals(e.Source, tabControl1))
        {
            UpdateSelectedFileViewers();
        }
    }

    private void FileChangesDoubleClick(object? sender, TappedEventArgs e)
    {
        RevisionGrid.ViewSelectedRevisions();
    }

    private void OpenWithDifftoolToolStripMenuItem_Click(object? sender, EventArgs e)
        => OpenFilesWithDiffTool(RevisionDiffKind.DiffAB, sender);

    private void OpenFilesWithDiffTool(RevisionDiffKind diffKind, object? sender)
    {
        string? toolName = (sender as MenuItem)?.Tag as string;
        IReadOnlyList<GitRevision> selectedRevisions = RevisionGrid.GetSelectedRevisions();
        string? oldFileName = selectedRevisions.Count > 0 ? GetFileNameForRevision(selectedRevisions[0]) : null;
        UICommands.OpenWithDifftool(this, selectedRevisions, FileName, oldFileName, diffKind, isTracked: true, customTool: toolName);
    }

    private void saveAsToolStripMenuItem_Click(object? sender, EventArgs e)
        => ThreadHelper.FileAndForget(SaveSelectedRevisionAsAsync);

    private async Task SaveSelectedRevisionAsAsync()
    {
        GitRevision? selectedRevision = RevisionGrid.SelectedRevision;
        if (selectedRevision is null)
        {
            return;
        }

        string historicalFileName = GetFileNameForRevision(selectedRevision) ?? FileName;
        string? fullName = _fullPathResolver.Resolve(historicalFileName)?.ToNativePath();
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return;
        }

        string? initialDirectory = Path.GetDirectoryName(fullName);
        string extension = Path.GetExtension(fullName);
        if (!await PortalPickerGuard.IsAvailableAsync())
        {
            return;
        }

        IStorageFile? file = await PortalPickerGuard.SaveFilePickerAsync(StorageProvider, new FilePickerSaveOptions
        {
            SuggestedFileName = Path.GetFileName(fullName),
            SuggestedStartLocation = initialDirectory is null ? null : await StorageProvider.TryGetFolderFromPathAsync(initialDirectory),
            DefaultExtension = extension.TrimStart('.'),
            FileTypeChoices =
            [
                new FilePickerFileType($"Current format (*{extension})") { Patterns = [$"*{extension}"] },
                new FilePickerFileType("All files (*.*)") { Patterns = ["*.*"] },
            ],
        });
        string? targetPath = file?.TryGetLocalPath();
        if (targetPath is not null)
        {
            await Module.SaveBlobAsAsync(targetPath, $"{selectedRevision.Guid}:\"{historicalFileName}\"");
        }
    }

    private void followFileHistoryToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AppSettings.FollowRenamesInFileHistory = !AppSettings.FollowRenamesInFileHistory;
        UpdateFollowHistoryMenuItems();
        LoadFileHistory();
    }

    private void UpdateFollowHistoryMenuItems()
    {
        followFileHistoryToolStripMenuItem.IsChecked = AppSettings.FollowRenamesInFileHistory;
        followFileHistoryRenamesToolStripMenuItem.IsEnabled = AppSettings.FollowRenamesInFileHistory;
        followFileHistoryRenamesToolStripMenuItem.IsChecked = AppSettings.FollowRenamesInFileHistoryExactOnly;
    }

    private void showFullHistoryToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        ToggleFullHistoryFlag();
    }

    private void simplifyMergesToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        ToggleSimplifyMergesFlag();
    }

    private void ToggleSimplifyMergesFlag()
    {
        AppSettings.SimplifyMergesInFileHistory = !AppSettings.SimplifyMergesInFileHistory;
        UpdateHistoryMenuItems();
        if (AppSettings.FullHistoryInFileHistory)
        {
            LoadFileHistory();
        }
    }

    private void ToggleFullHistoryFlag()
    {
        AppSettings.FullHistoryInFileHistory = !AppSettings.FullHistoryInFileHistory;
        UpdateHistoryMenuItems();
        LoadFileHistory();
    }

    private void cherryPickThisCommitToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        IReadOnlyList<GitRevision> selectedRevisions = RevisionGrid.GetSelectedRevisions();
        if (selectedRevisions.Count == 1)
        {
            UICommands.StartCherryPickDialog(this, selectedRevisions[0]);
        }
    }

    private void revertCommitToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        IReadOnlyList<GitRevision> selectedRevisions = RevisionGrid.GetSelectedRevisions();
        if (selectedRevisions.Count == 1)
        {
            UICommands.StartRevertCommitDialog(this, selectedRevisions[0]);
        }
    }

    private void FileHistoryContextMenuOpening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        IReadOnlyList<GitRevision> selectedRevisions = RevisionGrid.GetSelectedRevisions();
        copyToClipboardToolStripMenuItem.RefreshItems();
        openWithDifftoolToolStripMenuItem.IsEnabled = selectedRevisions.Count is >= 1 and <= 2;
        diffToolRemoteLocalStripMenuItem.IsEnabled =
            selectedRevisions.Count == 1
            && selectedRevisions[0].ObjectId != ObjectId.WorkTreeId
            && File.Exists(_fullPathResolver.Resolve(FileName));
        manipulateCommitToolStripMenuItem.IsEnabled =
            selectedRevisions.Count == 1 && !selectedRevisions[0].IsArtificial;
        saveAsToolStripMenuItem.IsEnabled = selectedRevisions.Count == 1;
        copyToClipboardToolStripMenuItem.IsEnabled =
            selectedRevisions.Count >= 1 && !selectedRevisions[0].IsArtificial;
    }

    private void UpdateHistoryMenuItems()
    {
        showFullHistoryToolStripMenuItem.IsChecked = AppSettings.FullHistoryInFileHistory;
        simplifyMergesToolStripMenuItem.IsChecked = AppSettings.SimplifyMergesInFileHistory;
        simplifyMergesToolStripMenuItem.IsEnabled = AppSettings.FullHistoryInFileHistory;
    }

    private void diffToolRemoteLocalStripMenuItem_Click(object? sender, EventArgs e)
        => OpenFilesWithDiffTool(RevisionDiffKind.DiffBLocal, sender);

    private void toolStripSplitLoad_ButtonClick(object? sender, EventArgs e)
        => LoadFileHistory();

    private void loadHistoryOnShowToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AppSettings.LoadFileHistoryOnShow = !AppSettings.LoadFileHistoryOnShow;
        UpdateLoadMenuItems();
    }

    private void UpdateLoadMenuItems()
    {
        loadHistoryOnShowToolStripMenuItem.IsChecked = AppSettings.LoadFileHistoryOnShow;
        loadBlameOnShowToolStripMenuItem.IsChecked = AppSettings.LoadBlameOnShow && BlameTab.IsVisible;
    }

    private void loadBlameOnShowToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AppSettings.LoadBlameOnShow = !AppSettings.LoadBlameOnShow;
        UpdateLoadMenuItems();
    }

    private void Blame_CommandClick(object? sender, CommandEventArgs e)
    {
        if (e.Command == "gotocommit")
        {
            Validates.NotNull(e.Data);
            if (Module.TryResolvePartialCommitId(e.Data, out ObjectId commitId)
                && !RevisionGrid.SetSelectedRevision(commitId))
            {
                MessageBoxes.RevisionFilteredInGrid(this, commitId);
            }
        }
        else if (e.Command == "gotobranch" || e.Command == "gototag")
        {
            Validates.NotNull(e.Data);
            CommitData? commit = _commitDataManager.GetCommitData(e.Data);
            if (commit is not null && !RevisionGrid.SetSelectedRevision(commit.ObjectId))
            {
                MessageBoxes.RevisionFilteredInGrid(this, commit.ObjectId);
            }
        }
        else if (e.Command == "navigatebackward")
        {
            RevisionGrid.NavigateBackward();
        }
        else if (e.Command == "navigateforward")
        {
            RevisionGrid.NavigateForward();
        }
    }

    private void followFileHistoryRenamesToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AppSettings.FollowRenamesInFileHistoryExactOnly = !AppSettings.FollowRenamesInFileHistoryExactOnly;
        UpdateFollowHistoryMenuItems();
        LoadFileHistory();
    }

    private void ignoreWhitespaceToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AppSettings.IgnoreWhitespaceOnBlame = !AppSettings.IgnoreWhitespaceOnBlame;
        UpdateBlameMenuItems();
        UpdateSelectedFileViewers(force: true);
    }

    private void detectMoveAndCopyInAllFilesToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AppSettings.DetectCopyInAllOnBlame = !AppSettings.DetectCopyInAllOnBlame;
        UpdateBlameMenuItems();
        UpdateSelectedFileViewers(force: true);
    }

    private void detectMoveAndCopyInThisFileToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AppSettings.DetectCopyInFileOnBlame = !AppSettings.DetectCopyInFileOnBlame;
        UpdateBlameMenuItems();
        UpdateSelectedFileViewers(force: true);
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        AvaloniaTranslationUtils.AddTranslationItemsFromFields(FormBrowseName, ToolStripFilters, translation);
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        AvaloniaTranslationUtils.TranslateItemsFromFields(FormBrowseName, ToolStripFilters, translation);
    }

    private void displayAuthorFirstToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AppSettings.BlameDisplayAuthorFirst = !AppSettings.BlameDisplayAuthorFirst;
        UpdateBlameMenuItems();
        UpdateSelectedFileViewers(force: true);
    }

    private void showAuthorToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AppSettings.BlameShowAuthor = !AppSettings.BlameShowAuthor;
        if (!AppSettings.BlameShowAuthor)
        {
            AppSettings.BlameShowAuthorDate = true;
        }

        UpdateBlameMenuItems();
        UpdateSelectedFileViewers(force: true);
    }

    private void showAuthorDateToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AppSettings.BlameShowAuthorDate = !AppSettings.BlameShowAuthorDate;
        if (!AppSettings.BlameShowAuthorDate)
        {
            AppSettings.BlameShowAuthor = true;
        }

        UpdateBlameMenuItems();
        UpdateSelectedFileViewers(force: true);
    }

    private void showAuthorTimeToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AppSettings.BlameShowAuthorTime = !AppSettings.BlameShowAuthorTime;
        UpdateBlameMenuItems();
        UpdateSelectedFileViewers(force: true);
    }

    private void showLineNumbersToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AppSettings.BlameShowLineNumbers = !AppSettings.BlameShowLineNumbers;
        Blame.UpdateShowLineNumbers();
        UpdateBlameMenuItems();
        UpdateSelectedFileViewers(force: true);
    }

    private void showOriginalFilePathToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AppSettings.BlameShowOriginalFilePath = !AppSettings.BlameShowOriginalFilePath;
        UpdateBlameMenuItems();
        UpdateSelectedFileViewers(force: true);
    }

    private void UpdateBlameMenuItems()
    {
        ignoreWhitespaceToolStripMenuItem.IsChecked = AppSettings.IgnoreWhitespaceOnBlame;
        detectMoveAndCopyInThisFileToolStripMenuItem.IsChecked = AppSettings.DetectCopyInFileOnBlame;
        detectMoveAndCopyInAllFilesToolStripMenuItem.IsChecked = AppSettings.DetectCopyInAllOnBlame;
        displayAuthorFirstToolStripMenuItem.IsChecked = AppSettings.BlameDisplayAuthorFirst;
        showAuthorAvatarToolStripMenuItem.IsChecked = AppSettings.BlameShowAuthorAvatar;
        showAuthorToolStripMenuItem.IsChecked = AppSettings.BlameShowAuthor;
        showAuthorDateToolStripMenuItem.IsChecked = AppSettings.BlameShowAuthorDate;
        showAuthorTimeToolStripMenuItem.IsChecked = AppSettings.BlameShowAuthorTime;
        showAuthorTimeToolStripMenuItem.IsEnabled = AppSettings.BlameShowAuthorDate;
        showLineNumbersToolStripMenuItem.IsChecked = AppSettings.BlameShowLineNumbers;
        showOriginalFilePathToolStripMenuItem.IsChecked = AppSettings.BlameShowOriginalFilePath;
    }

    private void showAuthorAvatarToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AppSettings.BlameShowAuthorAvatar = !AppSettings.BlameShowAuthorAvatar;
        UpdateBlameMenuItems();
        UpdateSelectedFileViewers(force: true);
    }

    private void GitcommandLogToolStripMenuItemClick(object? sender, EventArgs e)
    {
        FormGitCommandLog.ShowOrActivate(this);
    }

    private void TrackViewTask(Task task)
    {
        _viewTasks.RemoveAll(viewTask => viewTask.IsCompleted);
        _viewTasks.Add(task);
    }

    private async Task CompleteViewTasksAsync()
    {
        try
        {
            await Task.WhenAll(_viewTasks);
        }
        catch (OperationCanceledException)
        {
        }
    }

    bool IRevisionGridFileUpdate.SelectFileInRevision(ObjectId commitId, RelativePath ignoredFilename)
        => RevisionGrid.SetSelectedRevision(commitId);

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor
    {
        private readonly FormFileHistory _form;

        public TestAccessor(FormFileHistory form)
        {
            _form = form;
        }

        public RevisionGridControl RevisionGrid => _form.RevisionGrid;

        public Editor.FileViewer FileViewer => _form.View;

        public MenuItem ManipulateCommitMenuItem => _form.manipulateCommitToolStripMenuItem;

        public void SelectViewTab() => _form.tabControl1.SelectedItem = _form.ViewTab;
    }
}
