using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extended;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Compat;
using GitUI.HelperDialogs;
using GitUI.ScriptsEngine;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI;

partial class FileStatusList
{
    private Action? _blame;
    private Action? _cherryPickChanges;
    private Action? _filterFileInGrid;
    private Func<GitRevision?>? _getCurrentRevision;
    private Func<int>? _getLineNumber;
    private Func<string>? _getSelectedText;
    private Func<bool>? _getSupportLinePatching;
    private Action<bool>? _openInFileTreeTab_AsBlame;
    private Action? _refreshParent;
    private Action? _stage;
    private Action? _unstage;
    private readonly CancellationTokenSequence _interactiveAddResetChunkSequence = new();
    private readonly IFileStatusListContextMenuController _itemContextMenuController = new FileStatusListContextMenuController();
    private readonly IFindFilePredicateProvider _findFilePredicateProvider = new FindFilePredicateProvider();
    private readonly RememberFileContextMenuController _rememberFileContextMenuController = RememberFileContextMenuController.Default;
    private readonly TranslationString _deleteSelectedFilesCaption = new("Delete");
    private readonly TranslationString _deleteSelectedFiles = new("Are you sure you want to delete the selected file(s)?");
    private readonly TranslationString _deleteFailed = new("Delete file failed");
    private readonly TranslationString _firstRevision = new("First: A ");
    private readonly TranslationString _multipleDescription = new("<multiple>");
    private readonly TranslationString _newName = new("New name");
    private readonly TranslationString _resetSelectedChangesText = new("Are you sure you want to reset all selected files to {0}?");
    private readonly TranslationString _selectedRevision = new("Second: B ");
    private readonly TranslationString _stopTrackingFail = new("Fail to stop tracking the file '{0}'.");
    private readonly TranslationString _skipWorktreeToolTip = new("Hide already tracked files that will change but that you don\'t want to commit."
        + Environment.NewLine + "Suitable for some config files modified locally.");
    private readonly TranslationString _assumeUnchangedToolTip = new("Tell git to not check the status of this file for performance benefits."
        + Environment.NewLine + "Use this feature when a file is big and never change."
        + Environment.NewLine + "Git will never check if the file has changed that will improve status check performance.");
    private readonly FullPathResolver _fullPathResolver;
    private readonly RevisionDiffController _revisionDiffController;

    /// <summary>
    ///  Binds worktree/index actions supplied by a staging consumer.
    /// </summary>
    public void BindContextMenu(Action refreshParent, bool canAutoRefresh, Action? stage, Action? unstage)
    {
        _refreshParent = refreshParent;
        _stage = stage;
        _unstage = unstage;
        btnRefresh.IsVisible = canAutoRefresh;
    }

    /// <summary>
    ///  Binds the stash-level cherry-pick action.
    /// </summary>
    public void BindContextMenu(Action cherryPickChanges, Func<bool> getSupportLinePatching)
    {
        _cherryPickChanges = cherryPickChanges;
        _getSupportLinePatching = getSupportLinePatching;
    }

    /// <summary>
    ///  Binds revision-diff actions supplied by the owning view.
    /// </summary>
    public void BindContextMenu(
        Action? blame,
        Action? cherryPickChanges,
        Action filterFileInGrid,
        Action refreshParent,
        Action<bool>? openInFileTreeTab_AsBlame,
        Func<GitRevision?>? getCurrentRevision,
        Func<int> getLineNumber,
        Func<string>? getSelectedText,
        Func<bool> getSupportLinePatching)
    {
        _blame = blame;
        _cherryPickChanges = cherryPickChanges;
        _getSupportLinePatching = getSupportLinePatching;
        _filterFileInGrid = filterFileInGrid;
        _openInFileTreeTab_AsBlame = openInFileTreeTab_AsBlame;
        _refreshParent = refreshParent;
        _getCurrentRevision = getCurrentRevision;
        _getLineNumber = getLineNumber;
        _getSelectedText = getSelectedText;
    }

    private void WireContextMenu()
    {
        ItemContextMenu.Opening += ItemContextMenu_Opening;
        tsmiStageFile.Click += StageFile_Click;
        tsmiUnstageFile.Click += UnstageFile_Click;
        tsmiResetChunkOfFile.Click += ResetChunkOfFile_Click;
        tsmiInteractiveAdd.Click += InteractiveAdd_Click;
        tsmiCherryPickChanges.Click += CherryPickChanges_Click;
        btnRefresh.Click += (_, _) => RequestRefresh();
        tsmiOpenWorkingDirectoryFile.Click += OpenWorkingDirectoryFile_Click;
        tsmiShowInFolder.Click += ShowInFolder_Click;
        tsmiShowInFileTree.Click += ShowInFileTree_Click;
        tsmiFilterFileInGrid.Click += FilterFileInGrid_Click;
        tsmiFileHistory.Click += FileHistory_Click;
        tsmiBlame.Click += Blame_Click;
        tsmiFindFile.Click += FindFile_Click;
        tsmiUpdateSubmodule.Click += UpdateSubmodule_Click;
        tsmiResetSubmoduleChanges.Click += ResetSubmoduleChanges_Click;
        tsmiStashSubmoduleChanges.Click += StashSubmoduleChanges_Click;
        tsmiCommitSubmoduleChanges.Click += CommitSubmoduleChanges_Click;
        tsmiResetFileTo.Click += ResetFile_Click;
        tsmiResetFileToSelected.Click += ResetFile_Click;
        tsmiResetFileToParent.Click += ResetFile_Click;
        tsmiOpenWithDifftool.SubmenuOpened += (_, _) => OpenWithDifftool_DropDownOpening();
        tsmiDiffFirstToSelected.Click += DiffFirstToSelected_Click;
        tsmiDiffSelectedToLocal.Click += DiffSelectedToLocal_Click;
        tsmiDiffFirstToLocal.Click += DiffFirstToLocal_Click;
        tsmiDiffTwoSelected.Click += DiffTwoSelected_Click;
        tsmiDiffWithRemembered.Click += DiffWithRemembered_Click;
        tsmiRememberSecondRevDiff.Click += RememberSecondRevDiff_Click;
        tsmiRememberFirstRevDiff.Click += RememberFirstRevDiff_Click;
        tsmiOpenWorkingDirectoryFileWith.Click += OpenWorkingDirectoryFileWith_Click;
        tsmiOpenRevisionFile.Click += (_, _) => SaveSelectedItemToTempFile(OsShellUtil.Open);
        tsmiOpenRevisionFileWith.Click += (_, _) => SaveSelectedItemToTempFile(OsShellUtil.OpenAs);
        tsmiEditWorkingDirectoryFile.Click += EditWorkingDirectoryFile_Click;
        tsmiOpenInVisualStudio.Click += OpenInVisualStudio_Click;
        tsmiSaveAs.Click += (_, _) => this.InvokeAndForget(SaveAsAsync);
        tsmiMove.Click += Move_Click;
        tsmiDeleteFile.Click += DeleteFile_Click;
        tsmiAddFileToGitIgnore.Click += AddFileToGitIgnore_Click;
        tsmiAddFileToGitInfoExclude.Click += AddFileToGitInfoExclude_Click;
        tsmiSkipWorktree.Click += SkipWorktree_Click;
        tsmiAssumeUnchanged.Click += AssumeUnchanged_Click;
        tsmiStopTracking.Click += StopTracking_Click;
        tsmiOpenFindInCommitFilesGitGrepDialog.Click += OpenFindInCommitFilesGitGrepDialog_Click;
        tsmiShowFindInCommitFilesGitGrep.Click += ShowFindInCommitFilesGitGrep_Click;
        CreateTreeContextMenuItems();
    }

    private void Blame_Click(object? sender, EventArgs e)
    {
        if (_blame is not null)
        {
            _blame();
        }
        else
        {
            StartFileHistoryDialog(showBlame: true);
        }
    }

    private string? GetSelectedAbsolutePath()
    {
        string? relativePath = SelectedFolder?.Value;
        if (relativePath is null && SelectedGitItems is [GitItemStatus item])
        {
            relativePath = item.Name;
        }

        if (relativePath is null || !TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            return null;
        }

        return new FullPathResolver(() => commands.Module.WorkingDir).Resolve(relativePath)?.ToNativePath();
    }

    private WinFormsShims.IWin32Window? GetOwner()
        => TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window;

    private void ItemContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (sender is null || (SelectedFileStatusItem?.Item.IsStatusOnly ?? false))
        {
            e.Cancel = true;
            return;
        }

        System.Collections.IList items = ItemContextMenu.Items;
        InsertTreeContextMenuItems(items, index: 0);
        UpdateStatusOfTreeContextMenuItems();

        UpdateStatusOfMenuItems();

        // TODO The handling of _NO_TRANSLATE_openSubmoduleMenuItem need to be revised
        // This code handles the 'bold' in the menu for submodules. Other default actions are not set to bold.
        // The actual implementation of the default handling with doubleclick is in each form,
        // separate from this menu item
        if (!items.Contains(_NO_TRANSLATE_openSubmoduleMenuItem))
        {
            items.Insert(0, _NO_TRANSLATE_openSubmoduleMenuItem);
        }

        bool isSubmoduleSelected = SelectedFileStatusItem?.Item.IsSubmodule ?? false;
        _NO_TRANSLATE_openSubmoduleMenuItem.IsVisible = isSubmoduleSelected;
        _NO_TRANSLATE_openSubmoduleMenuItem.FontWeight = isSubmoduleSelected && !DisableSubmoduleMenuItemBold
            ? FontWeight.Bold
            : FontWeight.Normal;

        _sortBySeparator.IsVisible = !_isFileTreeMode;
        _sortByContextMenu.IsVisible = !_isFileTreeMode;
        if (!items.Contains(_sortByContextMenu))
        {
            items.Add(_sortBySeparator);
            items.Add(_sortByContextMenu);
        }

        if (TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            sepScripts.IsVisible = ItemContextMenu.AddUserScripts(
                tsmiRunScript,
                ExecuteCommand,
                script => script.OnEvent == ScriptEvent.ShowInFileList,
                commands);
        }
        else
        {
            ItemContextMenu.RemoveUserScripts(tsmiRunScript);
            sepScripts.IsVisible = false;
        }
    }

    private void AddFileToGitIgnore_Click(object? sender, EventArgs e)
    {
        AddFileToIgnoreFile(localExclude: false);
    }

    private void AddFileToGitInfoExclude_Click(object? sender, EventArgs e)
    {
        AddFileToIgnoreFile(localExclude: true);
    }

    private static bool CanResetToFirst(ObjectId parentId, IEnumerable<FileStatusItem> selectedItems)
    {
        return CanResetToSecond(parentId) || (parentId == ObjectId.IndexId && selectedItems.SecondIds().All(i => i == ObjectId.WorkTreeId));
    }

    private static bool CanResetToSecond(ObjectId resetId) => !resetId.IsZeroOrArtificial;

    private void CherryPickChanges_Click(object? sender, EventArgs e)
    {
        _cherryPickChanges?.Invoke();
    }

    private void OpenWorkingDirectoryFile_Click(object? sender, EventArgs e)
    {
        if (GetSelectedAbsolutePath() is string path && File.Exists(path))
        {
            OsShellUtil.Open(path);
        }
    }

    private void ShowInFolder_Click(object? sender, EventArgs e)
    {
        if (GetSelectedAbsolutePath() is not string path)
        {
            return;
        }

        FormBrowseUtil.ShowFileOrParentFolderInFileExplorer(path);
    }

    private void StartFileHistoryDialog(bool showBlame)
    {
        (string? fileName, GitRevision? revision) = SelectedFolder is RelativePath relativePath
            ? (relativePath.Length == 0 ? null : relativePath.Value, _getCurrentRevision?.Invoke())
            : SelectedFileStatusItem is FileStatusItem item && item.Item.IsTracked
                ? (item.Item.Name, item.SecondRevision)
                : (null, null);
        if (fileName is null)
        {
            return;
        }

        UICommands.StartFileHistoryDialog(
            GetOwner(),
            fileName,
            revision,
            showBlame: showBlame);
    }

    private void FindFile_Click(object? sender, EventArgs e)
    {
        if (LoadingFiles.IsVisible)
        {
            this.InvokeAndForget(async () =>
            {
                while (LoadingFiles.IsVisible)
                {
                    await Task.Delay(100);
                }

                FindFile_Click(this, EventArgs.Empty);
            });
            return;
        }

        IReadOnlyList<GitItemStatus> candidates = GitItemStatuses;

        IEnumerable<GitItemStatus> FindDiffFilesMatches(string name)
        {
            Func<string?, bool> predicate = _findFilePredicateProvider.Get(name, Module.WorkingDir);
            return candidates.Where(item => predicate(item.Name) || predicate(item.OldName));
        }

        GitItemStatus? selectedItem;
        using (SearchWindow<GitItemStatus> searchWindow = new(FindDiffFilesMatches))
        {
            searchWindow.ShowDialog(GetOwner());
            selectedItem = searchWindow.SelectedItem;
        }

        if (selectedItem is not null)
        {
            SelectedGitItem = selectedItem;
        }
    }

    private void StageFile_Click(object? sender, EventArgs e)
    {
        if (_stage is not null)
        {
            _stage();
            return;
        }

        GitItemStatus[] files = [.. SelectedItems
            .Where(item => item.Item.Staged == StagedStatus.WorkTree)
            .Select(item => item.Item)];
        Module.StageFiles(files, out _);
        RequestRefresh();
    }

    private static ContextMenuSelectionInfo GetSelectionInfo(FileStatusItem[] selectedItems, RelativePath? selectedFolder, bool isBareRepository, bool supportLinePatching, IFullPathResolver fullPathResolver)
    {
        // Some items are not supported if more than one revision is selected
        List<GitRevision> revisions = [.. selectedItems.SecondRevs()];
        GitRevision? selectedRev = revisions.Count == 1 ? revisions[0] : null;

        // First (A) is parent if one revision selected or if parent, then selected
        List<ObjectId> parentIds = [.. selectedItems.FirstIds()];

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
                string? path = fullPathResolver.Resolve(item.Item.Name);
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

    private void FileHistory_Click(object? sender, EventArgs e)
    {
        StartFileHistoryDialog(showBlame: false);
    }

    private void FilterFileInGrid_Click(object? sender, EventArgs e)
    {
        _filterFileInGrid?.Invoke();
    }

    public void InitResetFileToToolStripMenuItem()
    {
        // Multiple parent/child can be selected, only the the first is shown.
        // The only artificial commit that can be reset to is Index<-WorkTree
        ObjectId selectedId = SelectedItems.SecondIds().FirstOrDefault();
        ObjectId parentId = SelectedItems.FirstIds().FirstOrDefault();

        bool canResetToSecond = CanResetToSecond(selectedId);
        tsmiResetFileToSelected.IsEnabled = canResetToSecond;
        tsmiResetFileToSelected.IsVisible = canResetToSecond;
        if (canResetToSecond)
        {
            tsmiResetFileToSelected.Header = _selectedRevision.Text + GetDescriptionForRevision(selectedId);
        }

        bool canResetToFirst = CanResetToFirst(parentId, SelectedItems);
        tsmiResetFileToParent.IsEnabled = canResetToFirst;
        tsmiResetFileToParent.IsVisible = canResetToFirst;
        if (canResetToFirst)
        {
            tsmiResetFileToParent.Header = _firstRevision.Text + GetDescriptionForRevision(parentId);
        }

        bool canReset = canResetToSecond || canResetToFirst;
        tsmiResetFileTo.IsEnabled = canReset;
    }

    private void UnstageFile_Click(object? sender, EventArgs e)
    {
        if (_unstage is not null)
        {
            _unstage();
            return;
        }

        GitItemStatus[] files = [.. SelectedItems
            .Where(item => item.Item.Staged == StagedStatus.Index)
            .Select(item => item.Item)];
        Module.BatchUnstageFiles(files);
        RequestRefresh();
    }

    public void UpdateStatusOfMenuItems()
    {
        FileStatusItem[] selectedItems = [.. SelectedItems];
        bool isBareRepository = TryGetUICommandsDirect(out IGitUICommands? commands) && commands.Module.IsBareRepository();
        ContextMenuSelectionInfo selectionInfo = GetSelectionInfo(selectedItems, SelectedFolder, isBareRepository, supportLinePatching: _getSupportLinePatching?.Invoke() ?? false, _fullPathResolver);

        // Many options have no meaning for artificial commits or submodules
        // Hide the obviously no action options when single selected, handle them in actions if multi select

        // open submodule is added in FileStatusList
        tsmiUpdateSubmodule.IsVisible
            = tsmiResetSubmoduleChanges.IsVisible
            = tsmiStashSubmoduleChanges.IsVisible
            = tsmiCommitSubmoduleChanges.IsVisible
            = sepSubmodule.IsVisible
            = _revisionDiffController.ShouldShowSubmoduleMenus(selectionInfo);

        tsmiStageFile.IsEnabled
            = tsmiStageFile.IsVisible
            = _revisionDiffController.ShouldShowMenuStage(selectionInfo);
        tsmiUnstageFile.IsEnabled
            = tsmiUnstageFile.IsVisible
            = _revisionDiffController.ShouldShowMenuUnstage(selectionInfo);
        InitResetFileToToolStripMenuItem();
        tsmiResetFileTo.IsVisible = _revisionDiffController.ShouldShowResetFileMenus(selectionInfo);
        if (!tsmiResetFileTo.IsVisible)
        {
            tsmiResetFileTo.IsEnabled = false;
        }

        tsmiCherryPickChanges.IsVisible = _revisionDiffController.ShouldShowMenuCherryPick(selectionInfo);
        tsmiCherryPickChanges.IsEnabled = tsmiCherryPickChanges.IsVisible;

        sepFile.IsVisible = _revisionDiffController.ShouldShowDifftoolMenus(selectionInfo)
            || _revisionDiffController.ShouldShowMenuDeleteFile(selectionInfo)
            || _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo)
            || _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);

        tsmiOpenWithDifftool.IsEnabled = _revisionDiffController.ShouldShowDifftoolMenus(selectionInfo);
        tsmiOpenWithDifftool.IsVisible = tsmiOpenWithDifftool.IsEnabled;
        tsmiOpenWorkingDirectoryFileWith.IsVisible = _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo);
        tsmiOpenRevisionFile.IsVisible = _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);
        tsmiOpenRevisionFile.IsEnabled = _revisionDiffController.ShouldShowMenuShowInFileTree(selectionInfo);
        tsmiOpenRevisionFileWith.IsVisible = _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);
        tsmiOpenRevisionFileWith.IsEnabled = _revisionDiffController.ShouldShowMenuShowInFileTree(selectionInfo);
        tsmiSaveAs.IsVisible = _revisionDiffController.ShouldShowMenuSaveAs(selectionInfo);
        tsmiShowInFolder.IsVisible = _revisionDiffController.ShouldShowMenuShowInFolder(selectionInfo);
        tsmiEditWorkingDirectoryFile.IsVisible = _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo);
        tsmiOpenInVisualStudio.IsVisible = OperatingSystem.IsWindows()
            && commands is not null
            && VisualStudioIntegration.IsVisualStudioInstalled
            && tsmiEditWorkingDirectoryFile.IsVisible;
        tsmiMove.IsVisible = _revisionDiffController.ShouldShowMenuMove(selectionInfo);
        tsmiDeleteFile.Header = ResourceManager.TranslatedStrings.GetDeleteFile(selectionInfo.SelectedGitItemCount);
        tsmiDeleteFile.IsEnabled = _revisionDiffController.ShouldShowMenuDeleteFile(selectionInfo);
        tsmiDeleteFile.IsVisible = tsmiDeleteFile.IsEnabled;

        tsmiCopyPaths.IsEnabled = _revisionDiffController.ShouldShowMenuCopyFileName(selectionInfo);
        tsmiShowInFolder.IsEnabled = selectedItems.Any(item => _fullPathResolver.Resolve(item.Item.Name) is string filePath && FormBrowseUtil.FileOrParentDirectoryExists(filePath));

        tsmiShowInFileTree.IsVisible = !_isFileTreeMode && _openInFileTreeTab_AsBlame is not null && _revisionDiffController.ShouldShowMenuShowInFileTree(selectionInfo);
        tsmiFilterFileInGrid.IsVisible = _filterFileInGrid is not null;
        tsmiFilterFileInGrid.IsEnabled = _filterFileInGrid is not null && _revisionDiffController.ShouldShowMenuFileHistory(selectionInfo);
        tsmiFileHistory.IsEnabled = _revisionDiffController.ShouldShowMenuFileHistory(selectionInfo);
        tsmiBlame.IsEnabled = AppSettings.UseDiffViewerForBlame.Value || _blame is null
            ? _revisionDiffController.ShouldShowMenuBlame(selectionInfo)
            : _revisionDiffController.ShouldShowMenuShowInFileTree(selectionInfo);
        if (!tsmiBlame.IsEnabled)
        {
            tsmiBlame.IsChecked = false;
        }

        tsmiFindFile.IsVisible = true;
        tsmiOpenFindInCommitFilesGitGrepDialog.IsVisible = CanUseFindInCommitFilesGitGrep;
        tsmiShowFindInCommitFilesGitGrep.IsVisible = CanUseFindInCommitFilesGitGrep;
        tsmiShowFindInCommitFilesGitGrep.IsChecked = FindInCommitFilesGitGrepVisible;

        bool isSubmodule = selectionInfo.SelectedGitItemCount == 1 && selectionInfo.IsAnySubmodule;
        bool isSingleFile = selectionInfo.SelectedGitItemCount == 1 && !isSubmodule;

        bool canResetAddInteractively = selectionInfo.IsAnyItemWorkTree && isSingleFile;
        tsmiResetChunkOfFile.IsVisible = canResetAddInteractively;
        tsmiInteractiveAdd.IsVisible = canResetAddInteractively;

        bool canOpenFile = selectionInfo.SelectedGitItemCount == 1 && selectionInfo.AllFilesExist;
        tsmiOpenWorkingDirectoryFile.IsVisible = canOpenFile;
        tsmiOpenWorkingDirectoryFileWith.IsVisible = canOpenFile;

        bool canIgnoreFiles = selectionInfo.IsAnyItemWorkTree && !isSubmodule;
        bool canStopTracking = isSingleFile && selectionInfo.IsAnyTracked;

        sepIgnore.IsVisible = canIgnoreFiles || canStopTracking;

        tsmiAddFileToGitIgnore.IsVisible = canIgnoreFiles;
        tsmiAddFileToGitInfoExclude.IsVisible = canIgnoreFiles;
        tsmiSkipWorktree.IsVisible = canIgnoreFiles && selectionInfo.IsAnyTracked;
        tsmiAssumeUnchanged.IsVisible = canIgnoreFiles && selectionInfo.IsAnyTracked;
        tsmiSkipWorktree.IsChecked = selectedItems.Any(item => item.Item.IsSkipWorktree);
        tsmiAssumeUnchanged.IsChecked = selectedItems.Any(item => item.Item.IsAssumeUnchanged);

        tsmiStopTracking.IsVisible = canStopTracking;

        ToolTip.SetTip(tsmiSkipWorktree, _skipWorktreeToolTip.Text);
        ToolTip.SetTip(tsmiAssumeUnchanged, _assumeUnchangedToolTip.Text);
    }

    public void RepositoryChanged()
        => _rememberFileContextMenuController.RememberedDiffFileItem = null;

    public void CancelLoadCustomDifftools()
    {
        _customDiffToolsSequence.CancelCurrent();
    }

    public void LoadCustomDifftools()
    {
        List<CustomDiffMergeTool> menus =
        [
            new(tsmiDiffFirstToSelected, DiffFirstToSelected_Click),
            new(tsmiDiffSelectedToLocal, DiffSelectedToLocal_Click),
            new(tsmiDiffFirstToLocal, DiffFirstToLocal_Click),
            new(tsmiDiffWithRemembered, DiffWithRemembered_Click),
            new(tsmiDiffTwoSelected, DiffTwoSelected_Click),
        ];

        new CustomDiffMergeToolProvider().LoadCustomDiffMergeTools(
            Module,
            menus,
            isDiff: true,
            cancellationToken: _customDiffToolsSequence.Next());
    }

    public void ReloadHotkeys()
    {
        HotkeysEnabled = true;
        LoadHotkeys(RevisionDiffControl.HotkeySettingsName);
        tsmiDeleteFile.InputGesture = GetGesture(RevisionDiffControl.Command.DeleteSelectedFiles);
        tsmiFileHistory.InputGesture = GetGesture(RevisionDiffControl.Command.ShowHistory);
        tsmiBlame.InputGesture = GetGesture(RevisionDiffControl.Command.Blame);
        tsmiDiffFirstToSelected.InputGesture = GetGesture(RevisionDiffControl.Command.OpenWithDifftool);
        tsmiEditWorkingDirectoryFile.InputGesture = GetGesture(RevisionDiffControl.Command.EditFile);
        tsmiOpenRevisionFile.InputGesture = GetGesture(RevisionDiffControl.Command.OpenAsTempFile);
        tsmiOpenRevisionFileWith.InputGesture = GetGesture(RevisionDiffControl.Command.OpenAsTempFileWith);
        tsmiDiffFirstToLocal.InputGesture = GetGesture(RevisionDiffControl.Command.OpenWithDifftoolFirstToLocal);
        tsmiDiffSelectedToLocal.InputGesture = GetGesture(RevisionDiffControl.Command.OpenWithDifftoolSelectedToLocal);
        tsmiResetFileToParent.InputGesture = GetGesture(RevisionDiffControl.Command.ResetSelectedFiles);
        tsmiStageFile.InputGesture = GetGesture(RevisionDiffControl.Command.StageSelectedFile);
        tsmiUnstageFile.InputGesture = GetGesture(RevisionDiffControl.Command.UnStageSelectedFile);
        tsmiShowInFileTree.InputGesture = GetGesture(RevisionDiffControl.Command.ShowFileTree);
        tsmiFilterFileInGrid.InputGesture = GetGesture(RevisionDiffControl.Command.FilterFileInGrid);
        tsmiFindFile.InputGesture = GetGesture(RevisionDiffControl.Command.FindFile);
        tsmiOpenWorkingDirectoryFileWith.InputGesture = GetGesture(RevisionDiffControl.Command.OpenWorkingDirectoryFileWith);
        tsmiOpenWorkingDirectoryFile.InputGesture = GetGesture(RevisionDiffControl.Command.OpenWorkingDirectoryFile);
        tsmiMove.InputGesture = GetGesture(RevisionDiffControl.Command.RenameMove);

        Avalonia.Input.KeyGesture? GetGesture(RevisionDiffControl.Command command)
            => KeysMapper.ToKeyGesture(Hotkeys.FirstOrDefault(hotkey => hotkey.CommandCode == (int)command)?.KeyData);
    }

    internal bool ExecuteCommand(RevisionDiffControl.Command cmd)
        => ExecuteCommand((int)cmd);

    protected override bool ExecuteCommand(int cmd)
    {
        WinFormsShims.Keys shortcutKeys = Hotkeys.FirstOrDefault(hotkey => hotkey.CommandCode == cmd)?.KeyData ?? WinFormsShims.Keys.None;
        if ((FilterFilesByNameRegexFocused || FindInCommitFilesGitGrepFocused) && IsTextEditKey(shortcutKeys))
        {
            return false;
        }

        UpdateStatusOfMenuItems();

        switch ((RevisionDiffControl.Command)cmd)
        {
            case RevisionDiffControl.Command.DeleteSelectedFiles: tsmiDeleteFile.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); break;
            case RevisionDiffControl.Command.ShowHistory: tsmiFileHistory.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); break;
            case RevisionDiffControl.Command.Blame: tsmiBlame.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); break;
            case RevisionDiffControl.Command.OpenWithDifftool: OpenFilesWithDiffTool(RevisionDiffKind.DiffAB); break;
            case RevisionDiffControl.Command.OpenWithDifftoolFirstToLocal: OpenFilesWithDiffTool(RevisionDiffKind.DiffALocal); break;
            case RevisionDiffControl.Command.OpenWithDifftoolSelectedToLocal: OpenFilesWithDiffTool(RevisionDiffKind.DiffBLocal); break;
            case RevisionDiffControl.Command.OpenWorkingDirectoryFile: tsmiOpenWorkingDirectoryFile.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); break;
            case RevisionDiffControl.Command.OpenWorkingDirectoryFileWith: tsmiOpenWorkingDirectoryFileWith.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); break;
            case RevisionDiffControl.Command.EditFile: tsmiEditWorkingDirectoryFile.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); break;
            case RevisionDiffControl.Command.OpenAsTempFile: tsmiOpenRevisionFile.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); break;
            case RevisionDiffControl.Command.OpenAsTempFileWith: tsmiOpenRevisionFileWith.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); break;
            case RevisionDiffControl.Command.ResetSelectedFiles: return ResetSelectedFilesWithConfirmation();
            case RevisionDiffControl.Command.StageSelectedFile: tsmiStageFile.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); break;
            case RevisionDiffControl.Command.UnStageSelectedFile: tsmiUnstageFile.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); break;
            case RevisionDiffControl.Command.ShowFileTree: tsmiShowInFileTree.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); break;
            case RevisionDiffControl.Command.FilterFileInGrid: tsmiFilterFileInGrid.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); break;
            case RevisionDiffControl.Command.SelectFirstGroupChanges: return SelectFirstGroupChangesIfFocused();
            case RevisionDiffControl.Command.FindFile: tsmiFindFile.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); break;
            case RevisionDiffControl.Command.FindInCommitFilesUsingGitGrep_DiffTab:
                if (_isFileTreeMode)
                {
                    return false;
                }

                tsmiOpenFindInCommitFilesGitGrepDialog.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
                break;
            case RevisionDiffControl.Command.FindInCommitFilesUsingGitGrep_FileTreeTab:
                if (!_isFileTreeMode)
                {
                    return false;
                }

                tsmiOpenFindInCommitFilesGitGrepDialog.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
                break;
            case RevisionDiffControl.Command.OpenInVisualStudio: tsmiOpenInVisualStudio.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); break;
            case RevisionDiffControl.Command.AddFileToGitIgnore: return AddFileToGitIgnore();
            case RevisionDiffControl.Command.RenameMove: tsmiMove.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent)); break;
            default:
                return base.ExecuteCommand(cmd);
        }

        return true;

        bool AddFileToGitIgnore()
        {
            if (!IsKeyboardFocusWithin)
            {
                return false;
            }

            tsmiAddFileToGitIgnore.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));
            return true;
        }

        bool ResetSelectedFilesWithConfirmation()
        {
            if (!IsKeyboardFocusWithin)
            {
                return false;
            }

            InitResetFileToToolStripMenuItem();
            if (!tsmiResetFileToParent.IsEnabled)
            {
                // Hotkey executed when menu is disabled
                return true;
            }

            // Reset to first (parent)
            ResetSelectedItemsWithConfirmation(resetToParent: true);
            return true;
        }

        bool SelectFirstGroupChangesIfFocused()
        {
            if (!IsKeyboardFocusWithin)
            {
                return false;
            }

            SelectedItems = FirstGroupItems;
            return true;
        }
    }

    private void OpenFindInCommitFilesGitGrepDialog_Click(object? sender, EventArgs e)
    {
        ShowFindInCommitFileGitGrepDialog(_getSelectedText?.Invoke() ?? string.Empty);
    }

    private void InteractiveAdd_Click(object? sender, EventArgs e)
    {
        if (SelectedGitItem is not GitItemStatus item)
        {
            return;
        }

        CancellationToken token = _interactiveAddResetChunkSequence.Next();
        ThreadHelper.FileAndForget(async () =>
        {
            await Module.AddInteractiveAsync(item);
            await this.SwitchToMainThreadAsync(token);
            RequestRefresh();
        });
    }

    private void ShowFindInCommitFilesGitGrep_Click(object? sender, EventArgs e)
    {
        bool visible = tsmiShowFindInCommitFilesGitGrep.IsChecked == true;
        AppSettings.ShowFindInCommitFilesGitGrep.Value = visible;
        SetFindInCommitFilesGitGrepVisibility(visible);
    }

    private void AddFileToIgnoreFile(bool localExclude)
    {
        string[] fileNames = SelectedFolder is { Length: > 0 } selectedFolder
            ? [$"/{selectedFolder}/"]
            : [.. SelectedItems.Select(item => "/" + item.Item.Name)];
        if (fileNames.Length > 0
            && TryGetUICommandsDirect(out IGitUICommands? commands)
            && commands.StartAddToGitIgnoreDialog(GetOwner(), localExclude, fileNames))
        {
            RequestRefresh();
        }
    }

    private void AssumeUnchanged_Click(object? sender, EventArgs e)
    {
        Module.AssumeUnchangedFiles([.. SelectedItems.Items()], tsmiAssumeUnchanged.IsChecked, out _);
        RequestRefresh();
    }

    private void CommitSubmoduleChanges_Click(object? sender, EventArgs e)
    {
        if (!TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            return;
        }

        string[] submodules = [.. SelectedItems.Where(item => item.Item.IsSubmodule).Select(item => item.Item.Name).Distinct()];
        foreach (string name in submodules)
        {
            commands.WithWorkingDirectory(name.EnsureTrailingPathSeparator()).StartCommitDialog(GetOwner());
        }

        RequestRefresh();
    }

    private void DeleteFile_Click(object? sender, EventArgs e)
    {
        FileStatusItem[] selected = [.. SelectedItems];
        if (selected.Length == 0
            || MessageBoxes.Show(
                GetOwner(),
                _deleteSelectedFiles.Text,
                _deleteSelectedFilesCaption.Text,
                WinFormsShims.MessageBoxButtons.YesNo,
                WinFormsShims.MessageBoxIcon.Warning) != WinFormsShims.DialogResult.Yes)
        {
            return;
        }

        StoreNextItemToSelect();
        try
        {
            Module.BatchUnstageFiles(selected
                .Where(item => item.Item.Staged == StagedStatus.Index)
                .Select(item => item.Item));
            foreach (FileStatusItem item in selected.Where(item => !item.Item.IsSubmodule))
            {
                string? path = _fullPathResolver.Resolve(item.Item.Name);
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                }
                else if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
        catch (Exception exception)
        {
            MessageBoxes.Show(
                GetOwner(),
                _deleteFailed.Text + Environment.NewLine + exception.Message,
                TranslatedStrings.Error,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
        }
        finally
        {
            RequestRefresh();
        }
    }

    private void DiffTwoSelected_Click(object? sender, EventArgs e)
    {
        List<FileStatusItem> diffFiles = [.. SelectedItems];
        if (diffFiles.Count != 2)
        {
            return;
        }

        // The order is always the order in the list, not clicked order, but the (last) selected is known
        int firstIndex = SelectedFileStatusItem == diffFiles[0] ? 1 : 0;
        int secondIndex = 1 - firstIndex;

        // Fallback to first revision if second revision cannot be used
        bool firstUsesSecondRevision = _rememberFileContextMenuController.ShouldEnableFirstItemDiff(diffFiles[firstIndex], isSecondRevision: true);
        string? first = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash, diffFiles[firstIndex], firstUsesSecondRevision);
        bool secondUsesSecondRevision = _rememberFileContextMenuController.ShouldEnableSecondItemDiff(diffFiles[secondIndex], isSecondRevision: true);
        string? second = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash, diffFiles[secondIndex], secondUsesSecondRevision);
        Module.OpenFilesWithDifftool(first, second, GetCustomTool(sender));
    }

    private void DiffWithRemembered_Click(object? sender, EventArgs e)
    {
        FileStatusItem? selected = SelectedFileStatusItem;
        string? first = _rememberFileContextMenuController.GetGitCommit(
            Module.GetFileBlobHash,
            _rememberFileContextMenuController.RememberedDiffFileItem,
            isSecondRevision: true);
        bool useSecondRevision = _rememberFileContextMenuController.ShouldEnableSecondItemDiff(selected, isSecondRevision: true);
        string? second = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash, selected, useSecondRevision);
        Module.OpenFilesWithDifftool(first, second, GetCustomTool(sender));
    }

    private void EditWorkingDirectoryFile_Click(object? sender, EventArgs e)
    {
        if (GetSelectedAbsolutePath() is string path && TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            commands.StartFileEditorDialog(path);
            RequestRefresh();
        }
    }

    private void DiffFirstToSelected_Click(object? sender, EventArgs e)
        => OpenFilesWithDiffTool(RevisionDiffKind.DiffAB, GetCustomTool(sender));

    private void DiffSelectedToLocal_Click(object? sender, EventArgs e)
        => OpenFilesWithDiffTool(RevisionDiffKind.DiffBLocal, GetCustomTool(sender));

    private void DiffFirstToLocal_Click(object? sender, EventArgs e)
        => OpenFilesWithDiffTool(RevisionDiffKind.DiffALocal, GetCustomTool(sender));

    private static string? GetCustomTool(object? sender)
        => (sender as MenuItem)?.Tag as string;

    private void OpenFilesWithDiffTool(RevisionDiffKind diffKind, string? customTool = null)
    {
        if (!TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            return;
        }

        foreach (FileStatusItem item in SelectedItems)
        {
            if (item.FirstRevision?.ObjectId == ObjectId.CombinedDiffId)
            {
                // CombinedDiff cannot be viewed in a difftool
                continue;
            }

            GitRevision?[] revisions = [item.SecondRevision, item.FirstRevision];
            commands.OpenWithDifftool(
                GetOwner(),
                revisions,
                item.Item.Name,
                item.Item.OldName,
                diffKind,
                item.Item.IsTracked,
                customTool);
        }
    }

    private void OpenWithDifftool_DropDownOpening()
    {
        List<FileStatusItem> selected = [.. SelectedItems];
        List<GitRevision> secondRevisions = [.. selected.SecondRevs()];
        List<GitRevision> firstRevisions = [.. selected.FirstRevs()];
        tsmiSecondDiffCaption.Header = _selectedRevision.Text + DescribeRevisions(secondRevisions);
        tsmiFirstDiffCaption.Header = _firstRevision.Text + DescribeRevisions(firstRevisions);
        MenuUtil.SetAsCaptionMenuItem(tsmiSecondDiffCaption, tsmiOpenWithDifftool);
        MenuUtil.SetAsCaptionMenuItem(tsmiFirstDiffCaption, tsmiOpenWithDifftool);

        ContextMenuDiffToolInfo info = GetContextMenuDiffToolInfo();
        tsmiDiffFirstToSelected.IsEnabled = _itemContextMenuController.ShouldShowMenuFirstToSelected(info);
        tsmiDiffFirstToLocal.IsEnabled = _itemContextMenuController.ShouldShowMenuFirstToLocal(info);
        tsmiDiffSelectedToLocal.IsEnabled = _itemContextMenuController.ShouldShowMenuSelectedToLocal(info);
        bool hideToLocal = _itemContextMenuController.ShouldHideToLocal(info);
        tsmiDiffFirstToLocal.IsVisible = !hideToLocal;
        tsmiDiffSelectedToLocal.IsVisible = !hideToLocal;

        sepDifftoolRemember.IsVisible = selected.Count is 1 or 2;

        // The order is always the order in the list, not clicked order, but the (last) selected is known
        int firstIndex = selected.Count == 2 && SelectedFileStatusItem == selected[0] ? 1 : 0;
        int secondIndex = 1 - firstIndex;

        tsmiDiffTwoSelected.IsVisible = selected.Count == 2;
        tsmiDiffTwoSelected.IsEnabled = selected.Count == 2
                                         && _rememberFileContextMenuController.ShouldEnableFirstItemDiff(selected[firstIndex])
                                         && _rememberFileContextMenuController.ShouldEnableSecondItemDiff(selected[secondIndex]);
        tsmiDiffWithRemembered.IsVisible = selected.Count == 1 && _rememberFileContextMenuController.RememberedDiffFileItem is not null;
        tsmiDiffWithRemembered.IsEnabled = tsmiDiffWithRemembered.IsVisible
                                           && selected[0] != _rememberFileContextMenuController.RememberedDiffFileItem
                                           && _rememberFileContextMenuController.ShouldEnableSecondItemDiff(selected[0]);
        tsmiDiffWithRemembered.Header = _rememberFileContextMenuController.RememberedDiffFileItem is FileStatusItem remembered
            ? string.Format(TranslatedStrings.DiffSelectedWithRememberedFile, remembered.Item.Name)
            : string.Empty;
        tsmiRememberSecondRevDiff.IsVisible = selected.Count == 1;
        tsmiRememberSecondRevDiff.IsEnabled = selected.Count == 1
                                              && _rememberFileContextMenuController.ShouldEnableFirstItemDiff(selected[0], isSecondRevision: true);
        tsmiRememberFirstRevDiff.IsVisible = selected.Count == 1;
        tsmiRememberFirstRevDiff.IsEnabled = selected.Count == 1
                                             && _rememberFileContextMenuController.ShouldEnableFirstItemDiff(selected[0], isSecondRevision: false);
    }

    /// <summary>
    /// Gets the description of the selected parents.
    /// </summary>
    /// <param name="parents">The selected parents.</param>
    /// <returns>A description of the selected parent.</returns>
    private string? DescribeRevisions(List<GitRevision> parents)
    {
        return parents.Count switch
        {
            1 => GetDescriptionForRevision(parents[0]?.ObjectId ?? default(ObjectId)),
            > 1 => _multipleDescription.Text,
            _ => null
        };
    }

    private ContextMenuDiffToolInfo GetContextMenuDiffToolInfo()
    {
        // Some items are not supported if more than one revision is selected
        List<GitRevision> revisions = [.. SelectedItems.SecondRevs()];
        GitRevision? selectedRev = revisions.Count == 1 ? revisions[0] : null;

        List<ObjectId> parentIds = [.. SelectedItems.FirstIds()];
        bool firstIsParent = _gitRevisionTester.AllFirstAreParentsToSelected(parentIds, selectedRev);
        bool localExists = _gitRevisionTester.AnyLocalFileExists(SelectedItems.Select(i => i.Item));

        bool allAreNew = SelectedItems.All(i => i.Item.IsNew);
        bool allAreDeleted = SelectedItems.All(i => i.Item.IsDeleted);

        return new ContextMenuDiffToolInfo(
            selectedRevision: selectedRev,
            selectedItemParentRevs: parentIds,
            allAreNew: allAreNew,
            allAreDeleted: allAreDeleted,
            firstIsParent: firstIsParent,
            localExists: localExists);
    }

    private void OpenWorkingDirectoryFileWith_Click(object? sender, EventArgs e)
    {
        if (GetSelectedAbsolutePath() is string path && File.Exists(path))
        {
            OsShellUtil.OpenAs(path);
        }
    }

    private void OpenInVisualStudio_Click(object? sender, EventArgs e)
    {
        if (OperatingSystem.IsWindows()
            && VisualStudioIntegration.IsVisualStudioInstalled
            && GetSelectedAbsolutePath() is string itemName)
        {
            VisualStudioIntegration.OpenFile(itemName, GetLineNumber());
        }
    }

    private int GetLineNumber()
        => _getLineNumber is not null
            ? _getLineNumber()
            : int.Parse(FindScriptOptionsProvider().GetValues(ScriptOptionsProvider._lineNumber).FirstOrDefault("0"));

    private void RememberFirstRevDiff_Click(object? sender, EventArgs e)
    {
        if (SelectedFileStatusItem is not { FirstRevision: not null } selected)
        {
            return;
        }

        _rememberFileContextMenuController.RememberedDiffFileItem = new FileStatusItem(
            selected.SecondRevision,
            selected.FirstRevision,
            selected.Item);
    }

    private void RememberSecondRevDiff_Click(object? sender, EventArgs e)
        => _rememberFileContextMenuController.RememberedDiffFileItem = SelectedFileStatusItem;

    private void ResetFile_Click(object? sender, EventArgs e)
    {
        if (ReferenceEquals(sender, tsmiResetFileTo))
        {
            sender = tsmiResetFileToParent;
            if (!tsmiResetFileToParent.IsEnabled)
            {
                return;
            }
        }

        ResetSelectedItemsWithConfirmation(resetToParent: ReferenceEquals(sender, tsmiResetFileToParent));
    }

    private void ResetChunkOfFile_Click(object? sender, EventArgs e)
    {
        if (SelectedGitItem is not GitItemStatus item)
        {
            return;
        }

        CancellationToken token = _interactiveAddResetChunkSequence.Next();
        ThreadHelper.FileAndForget(async () =>
        {
            await Module.ResetInteractiveAsync(item);
            await this.SwitchToMainThreadAsync(token);
            RequestRefresh();
        });
    }

    public void ResetSelectedItemsWithConfirmation(bool resetToParent)
    {
        FileStatusItem[] selected = [.. SelectedItems];
        if (selected.Length == 0)
        {
            return;
        }

        bool hasNewFiles = !selected.All(item => item.Item.IsChanged);
        bool hasExistingFiles = selected.Any(item => !item.Item.IsUncommittedAdded);
        string description = resetToParent ? _firstRevision.Text : _selectedRevision.Text;
        FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(
            GetOwner(),
            hasExistingFiles,
            hasNewFiles,
            string.Format(_resetSelectedChangesText.Text, description));
        if (resetType == FormResetChanges.ActionEnum.Cancel)
        {
            return;
        }

        foreach (ObjectId id in resetToParent ? selected.FirstIds() : selected.SecondIds())
        {
            if (id.IsZeroOrArtificial)
            {
                continue;
            }

            GitItemStatus[] items = resetToParent
                ? [.. selected.Items()]
                : [.. selected.Items().Select(item => item.InvertStatus())];
            Module.ResetChanges(
                id,
                items,
                resetAndDelete: resetType == FormResetChanges.ActionEnum.ResetAndDelete,
                _fullPathResolver,
                out System.Text.StringBuilder output,
                progressAction: null);
            if (output.Length > 0)
            {
                MessageBoxes.Show(
                    GetOwner(),
                    output.ToString(),
                    TranslatedStrings.ResetChangesCaption,
                    WinFormsShims.MessageBoxButtons.OK,
                    WinFormsShims.MessageBoxIcon.Error);
            }
        }

        RequestRefresh();
    }

    private void ResetSubmoduleChanges_Click(object? sender, EventArgs e)
    {
        FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(GetOwner(), true, true);
        if (resetType == FormResetChanges.ActionEnum.Cancel)
        {
            return;
        }

        foreach (string name in SelectedItems.Where(item => item.Item.IsSubmodule).Select(item => item.Item.Name).Distinct())
        {
            Module.GetSubmodule(name).ResetAllChanges(clean: resetType == FormResetChanges.ActionEnum.ResetAndDelete);
        }

        RequestRefresh();
    }

    private async Task SaveAsAsync()
    {
        FileStatusItem[] selected = [.. SelectedItems];
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (selected.Length == 0 || topLevel is null)
        {
            return;
        }

        if (!await PortalPickerGuard.IsAvailableAsync())
        {
            return;
        }

        if (selected.Length == 1)
        {
            FileStatusItem item = selected[0];
            IStorageFile? target = await PortalPickerGuard.SaveFilePickerAsync(topLevel.StorageProvider, new FilePickerSaveOptions
            {
                SuggestedFileName = Path.GetFileName(item.Item.Name),
            });
            if (target is not null)
            {
                await Module.SaveBlobAsAsync(target.Path.LocalPath, $"{item.SecondRevision.Guid}:\"{item.Item.Name}\"");
            }

            return;
        }

        IReadOnlyList<IStorageFolder> folders = await PortalPickerGuard.OpenFolderPickerAsync(topLevel.StorageProvider, new FolderPickerOpenOptions
        {
            AllowMultiple = false,
        });
        if (folders is not [IStorageFolder folder])
        {
            return;
        }

        foreach (FileStatusItem item in selected)
        {
            string target = Path.Combine(folder.Path.LocalPath, item.Item.Name.ToNativePath());
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            await Module.SaveBlobAsAsync(target, $"{item.SecondRevision.Guid}:\"{item.Item.Name}\"");
        }
    }

    private void SaveSelectedItemToTempFile(Action<string> onSaved)
    {
        if (SelectedFileStatusItem is not FileStatusItem item)
        {
            return;
        }

        ThreadHelper.FileAndForget(async () =>
        {
            ObjectId blob = Module.GetFileBlobHash(item.Item.Name, item.SecondRevision.ObjectId);
            if (blob.IsZero)
            {
                return;
            }

            string fileName = Path.Combine(Path.GetTempPath(), PathUtil.GetFileName(item.Item.Name)).ToNativePath();
            await Module.SaveBlobAsAsync(fileName, blob.ToString());
            onSaved(fileName);
        });
    }

    private void SkipWorktree_Click(object? sender, EventArgs e)
    {
        Module.SkipWorktreeFiles([.. SelectedItems.Items()], tsmiSkipWorktree.IsChecked, out _);
        RequestRefresh();
    }

    private void ShowInFileTree_Click(object? sender, EventArgs e)
    {
        if (!_isFileTreeMode)
        {
            _openInFileTreeTab_AsBlame?.Invoke(false);
        }
    }

    private void StashSubmoduleChanges_Click(object? sender, EventArgs e)
    {
        if (!TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            return;
        }

        foreach (string name in SelectedItems.Where(item => item.Item.IsSubmodule).Select(item => item.Item.Name).Distinct())
        {
            commands.WithGitModule(Module.GetSubmodule(name)).StashSave(GetOwner(), AppSettings.IncludeUntrackedFilesInManualStash);
        }

        RequestRefresh();
    }

    private void StopTracking_Click(object? sender, EventArgs e)
    {
        if (SelectedItem?.Name is not string fileName)
        {
            return;
        }

        if (Module.StopTrackingFile(fileName))
        {
            RequestRefresh();
            return;
        }

        MessageBoxes.Show(
            GetOwner(),
            string.Format(_stopTrackingFail.Text, fileName),
            TranslatedStrings.Error,
            WinFormsShims.MessageBoxButtons.OK,
            WinFormsShims.MessageBoxIcon.Error);
    }

    private void UpdateSubmodule_Click(object? sender, EventArgs e)
    {
        if (!TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            return;
        }

        string[] submodules = [.. SelectedItems.Where(item => item.Item.IsSubmodule).Select(item => item.Item.Name).Distinct()];
        FormProcess.ShowDialog(
            GetOwner(),
            commands,
            Commands.SubmoduleUpdate(submodules),
            Module.WorkingDir,
            input: null,
            useDialogSettings: true);
        RequestRefresh();
    }

    private void Move_Click(object? sender, EventArgs e)
    {
        string? oldName = SelectedItem?.Name ?? SelectedFolder?.Value;
        if (oldName is null || !TryGetUICommandsDirect(out IGitUICommands? commands))
        {
            return;
        }

        using IUserInputPrompt prompt = commands.GetRequiredService<ISimplePromptCreator>()
            .Create(tsmiMove.Header?.ToString(), _newName.Text, oldName);
        if (GetOwner() is not WinFormsShims.IWin32Window owner
            || prompt.ShowDialog(owner) != WinFormsShims.DialogResult.OK
            || prompt.UserInput == oldName)
        {
            return;
        }

        MoveCommand.Arguments arguments = new(SelectedItem is null, oldName, prompt.UserInput);
        MoveCommand command = new(Module.GitExecutable);
        if (command.Validate(arguments))
        {
            command.Execute(arguments);
            RequestRefresh();
        }
    }

    private void RequestRefresh()
    {
        if (_refreshParent is not null)
        {
            _refreshParent();
            return;
        }

        _refreshAction?.Invoke();
    }
}
