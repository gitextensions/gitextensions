using Avalonia.Controls;
using Avalonia.Platform.Storage;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extended;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.HelperDialogs;
using GitUI.ScriptsEngine;
using GitUI.UserControls;
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
    private readonly IFileStatusListContextMenuController _itemContextMenuController = new FileStatusListContextMenuController();
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
        tsmiCherryPickChanges.Click += (_, _) => _cherryPickChanges?.Invoke();
        btnRefresh.Click += (_, _) => RequestRefresh();
        tsmiOpenWorkingDirectoryFile.Click += OpenWorkingDirectoryFile_Click;
        tsmiCopyPaths.Click += CopyPaths_Click;
        tsmiShowInFolder.Click += ShowInFolder_Click;
        tsmiShowInFileTree.Click += (_, _) => _openInFileTreeTab_AsBlame?.Invoke(false);
        tsmiFilterFileInGrid.Click += (_, _) => _filterFileInGrid?.Invoke();
        tsmiFileHistory.Click += (_, _) => StartFileHistoryDialog(showBlame: false);
        tsmiBlame.Click += Blame_Click;
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
        tsmiSaveAs.Click += (_, _) => this.InvokeAndForget(SaveAsAsync);
        tsmiMove.Click += Move_Click;
        tsmiDeleteFile.Click += DeleteFile_Click;
        tsmiAddFileToGitIgnore.Click += (_, _) => AddFileToIgnoreFile(localExclude: false);
        tsmiAddFileToGitInfoExclude.Click += (_, _) => AddFileToIgnoreFile(localExclude: true);
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

    private void CopyPaths_Click(object? sender, EventArgs e)
    {
        string[] paths = SelectedFolder is RelativePath folder
            ? [folder.Value]
            : [.. SelectedGitItems.Select(item => item.Name)];
        if (paths.Length > 0)
        {
            ClipboardUtil.TrySetText(string.Join(Environment.NewLine, paths));
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

    private void ItemContextMenu_Opening(object? sender, EventArgs e)
    {
        UpdateStatusOfMenuItems();
        IReadOnlyList<GitItemStatus> selectedItems = SelectedGitItems;
        bool hasItems = selectedItems.Count > 0;
        bool hasSingleItem = selectedItems.Count == 1;
        bool hasPath = hasItems || SelectedFolder is not null;
        if (!hasPath && e is System.ComponentModel.CancelEventArgs cancelEventArgs)
        {
            cancelEventArgs.Cancel = true;
            return;
        }

        string? absolutePath = GetSelectedAbsolutePath();
        bool workingFileExists = absolutePath is not null && File.Exists(absolutePath);

        tsmiStageFile.IsVisible = _stage is not null;
        tsmiStageFile.IsEnabled = _stage is not null
                                  && selectedItems.Any(item => !item.IsAssumeUnchanged && !item.IsSkipWorktree);
        tsmiUnstageFile.IsVisible = _unstage is not null;
        tsmiUnstageFile.IsEnabled = _unstage is not null && hasItems;
        tsmiCherryPickChanges.IsVisible = _cherryPickChanges is not null;
        tsmiCherryPickChanges.IsEnabled = hasSingleItem && (_getSupportLinePatching?.Invoke() ?? false);
        sepGit.IsVisible = tsmiStageFile.IsVisible || tsmiUnstageFile.IsVisible || tsmiCherryPickChanges.IsVisible;

        tsmiOpenWorkingDirectoryFile.IsVisible = workingFileExists;
        tsmiCopyPaths.IsEnabled = hasPath;
        tsmiShowInFolder.IsEnabled = absolutePath is not null
                                     && (File.Exists(absolutePath) || Directory.Exists(absolutePath));
        sepBrowse.IsVisible = hasPath;
        tsmiShowInFileTree.IsVisible = !_isFileTreeMode && _openInFileTreeTab_AsBlame is not null;
        tsmiShowInFileTree.IsEnabled = hasSingleItem;
        tsmiFilterFileInGrid.IsVisible = _filterFileInGrid is not null;
        tsmiFilterFileInGrid.IsEnabled = hasPath;
        tsmiFileHistory.IsEnabled = hasSingleItem && selectedItems[0].IsTracked;
        tsmiBlame.IsEnabled = hasSingleItem && selectedItems[0].IsTracked;

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

        UpdateStatusOfTreeContextMenuItems();
    }

    private void OpenWorkingDirectoryFile_Click(object? sender, EventArgs e)
    {
        if (GetSelectedAbsolutePath() is string path && File.Exists(path))
        {
            OsShellUtil.Open(path);
        }
    }

    private void SetTreeExpansion(bool expanded, bool rootOnly)
    {
        if (_showDiffGroups)
        {
            foreach (DiffTreeNode node in tvDiffFiles.Items.Cast<DiffTreeNode>())
            {
                SetDiffExpansion(node);
            }

            return;
        }

        foreach (FileTreeNode node in tvFiles.Items.Cast<FileTreeNode>())
        {
            SetExpansion(node);
        }

        return;

        void SetExpansion(FileTreeNode node)
        {
            if (node.IsFolder)
            {
                node.IsExpanded = expanded;
            }

            if (!rootOnly)
            {
                foreach (FileTreeNode child in node.Children)
                {
                    SetExpansion(child);
                }
            }
        }

        void SetDiffExpansion(DiffTreeNode node)
        {
            if (node.Children.Count > 0)
            {
                node.IsExpanded = expanded;
            }

            if (!rootOnly)
            {
                foreach (DiffTreeNode child in node.Children)
                {
                    SetDiffExpansion(child);
                }
            }
        }
    }

    private void ShowInFolder_Click(object? sender, EventArgs e)
    {
        if (GetSelectedAbsolutePath() is not string path)
        {
            return;
        }

        string? directory = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
        if (directory is not null && Directory.Exists(directory))
        {
            OsShellUtil.Open(directory);
        }
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
        FileStatusItem[] selected = [.. SelectedItems];
        bool hasItems = selected.Length > 0;
        bool singleItem = selected.Length == 1;
        bool singleFolder = SelectedFolder is not null;
        bool anyTracked = selected.Any(item => item.Item.IsTracked);
        bool anyWorkTree = selected.Any(item => item.Item.Staged == StagedStatus.WorkTree);
        bool anyIndex = selected.Any(item => item.Item.Staged == StagedStatus.Index);
        bool anySubmodule = selected.Any(item => item.Item.IsSubmodule);
        bool allFilesExist = hasItems && selected.All(item => File.Exists(_fullPathResolver.Resolve(item.Item.Name)));
        bool allDirectoriesExist = hasItems && selected.All(item => Directory.Exists(_fullPathResolver.Resolve(item.Item.Name)));
        bool anyArtificial = selected.Any(item => item.SecondRevision.IsArtificial);
        bool displayOnly = selected.Any(item => item.Item.IsRangeDiff || item.Item.IsStatusOnly)
                           || selected.FirstIds().Contains(ObjectId.CombinedDiffId);

        bool showSubmodule = anySubmodule && allDirectoriesExist && selected.SecondIds().All(id => id == ObjectId.WorkTreeId);
        tsmiUpdateSubmodule.IsVisible = showSubmodule;
        tsmiResetSubmoduleChanges.IsVisible = showSubmodule;
        tsmiStashSubmoduleChanges.IsVisible = showSubmodule;
        tsmiCommitSubmoduleChanges.IsVisible = showSubmodule;
        sepSubmodule.IsVisible = showSubmodule;

        tsmiStageFile.IsVisible = anyWorkTree;
        tsmiStageFile.IsEnabled = anyWorkTree;
        tsmiUnstageFile.IsVisible = anyIndex;
        tsmiUnstageFile.IsEnabled = anyIndex;
        tsmiResetFileTo.IsVisible = hasItems && anyTracked && !displayOnly && !Module.IsBareRepository();
        tsmiResetFileTo.IsEnabled = tsmiResetFileTo.IsVisible;
        tsmiResetFileToSelected.IsVisible = selected.SecondIds().Any(id => !id.IsZeroOrArtificial);
        tsmiResetFileToSelected.IsEnabled = tsmiResetFileToSelected.IsVisible;
        tsmiResetFileToSelected.Header = _selectedRevision.Text + DescribeRevisions([.. selected.SecondRevs()]);
        tsmiResetFileToParent.IsVisible = selected.FirstIds().Any(id => !id.IsZeroOrArtificial);
        tsmiResetFileToParent.IsEnabled = tsmiResetFileToParent.IsVisible;
        tsmiResetFileToParent.Header = _firstRevision.Text + DescribeRevisions([.. selected.FirstRevs()]);
        tsmiCherryPickChanges.IsVisible = _cherryPickChanges is not null;
        tsmiCherryPickChanges.IsEnabled = singleItem && (_getSupportLinePatching?.Invoke() ?? false);

        tsmiOpenWithDifftool.IsVisible = hasItems && !displayOnly;
        tsmiOpenWithDifftool.IsEnabled = tsmiOpenWithDifftool.IsVisible;
        tsmiOpenWorkingDirectoryFile.IsVisible = singleItem && allFilesExist;
        tsmiOpenWorkingDirectoryFileWith.IsVisible = singleItem && allFilesExist;
        tsmiEditWorkingDirectoryFile.IsVisible = singleItem && allFilesExist;
        tsmiOpenRevisionFile.IsVisible = singleItem && !anySubmodule && !anyArtificial && !displayOnly;
        tsmiOpenRevisionFileWith.IsVisible = tsmiOpenRevisionFile.IsVisible;
        tsmiSaveAs.IsVisible = hasItems && !anySubmodule && !anyArtificial && !displayOnly;
        tsmiMove.IsVisible = (singleItem && anyTracked && !anySubmodule) || singleFolder;
        tsmiDeleteFile.IsVisible = anyArtificial && (allFilesExist || allDirectoriesExist);
        tsmiDeleteFile.IsEnabled = tsmiDeleteFile.IsVisible;
        tsmiOpenInVisualStudio.IsVisible = false;
        sepFile.IsVisible = tsmiOpenWithDifftool.IsVisible
                            || tsmiOpenWorkingDirectoryFile.IsVisible
                            || tsmiOpenRevisionFile.IsVisible
                            || tsmiSaveAs.IsVisible
                            || tsmiMove.IsVisible
                            || tsmiDeleteFile.IsVisible;

        tsmiCopyPaths.IsEnabled = (hasItems || singleFolder) && !selected.Any(item => item.Item.IsStatusOnly);
        tsmiShowInFolder.IsVisible = hasItems || singleFolder;
        tsmiShowInFolder.IsEnabled = GetSelectedAbsolutePath() is string absolute
                                     && (File.Exists(absolute) || Directory.Exists(absolute));
        tsmiShowInFileTree.IsVisible = !_isFileTreeMode && _openInFileTreeTab_AsBlame is not null && (singleItem || singleFolder);
        tsmiFilterFileInGrid.IsVisible = _filterFileInGrid is not null;
        tsmiFilterFileInGrid.IsEnabled = singleItem || singleFolder;
        tsmiFileHistory.IsEnabled = (singleItem || singleFolder) && anyTracked;
        tsmiBlame.IsEnabled = singleItem && anyTracked && !anySubmodule;
        tsmiFindFile.IsVisible = false;
        tsmiOpenFindInCommitFilesGitGrepDialog.IsVisible = CanUseFindInCommitFilesGitGrep;
        tsmiShowFindInCommitFilesGitGrep.IsVisible = CanUseFindInCommitFilesGitGrep;
        tsmiShowFindInCommitFilesGitGrep.IsChecked = FindInCommitFilesGitGrepVisible;

        // The native ignore dialogs are owned by their later dialog tranche. Keeping these
        // entries hidden avoids exposing the current GitUICommands NotPorted boundary.
        tsmiAddFileToGitIgnore.IsVisible = false;
        tsmiAddFileToGitInfoExclude.IsVisible = false;
        tsmiSkipWorktree.IsVisible = anyWorkTree && anyTracked && !anySubmodule;
        tsmiAssumeUnchanged.IsVisible = tsmiSkipWorktree.IsVisible;
        tsmiStopTracking.IsVisible = singleItem && anyTracked;
        tsmiSkipWorktree.IsChecked = selected.Any(item => item.Item.IsSkipWorktree);
        tsmiAssumeUnchanged.IsChecked = selected.Any(item => item.Item.IsAssumeUnchanged);
        ToolTip.SetTip(tsmiSkipWorktree, _skipWorktreeToolTip.Text);
        ToolTip.SetTip(tsmiAssumeUnchanged, _assumeUnchangedToolTip.Text);
        sepIgnore.IsVisible = tsmiSkipWorktree.IsVisible || tsmiAssumeUnchanged.IsVisible || tsmiStopTracking.IsVisible;
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
        switch ((RevisionDiffControl.Command)cmd)
        {
            case RevisionDiffControl.Command.DeleteSelectedFiles: DeleteFile_Click(this, EventArgs.Empty); break;
            case RevisionDiffControl.Command.ShowHistory: StartFileHistoryDialog(showBlame: false); break;
            case RevisionDiffControl.Command.Blame: Blame_Click(this, EventArgs.Empty); break;
            case RevisionDiffControl.Command.OpenWithDifftool: OpenFilesWithDiffTool(RevisionDiffKind.DiffAB); break;
            case RevisionDiffControl.Command.EditFile: EditWorkingDirectoryFile_Click(this, EventArgs.Empty); break;
            case RevisionDiffControl.Command.OpenAsTempFile: SaveSelectedItemToTempFile(OsShellUtil.Open); break;
            case RevisionDiffControl.Command.OpenAsTempFileWith: SaveSelectedItemToTempFile(OsShellUtil.OpenAs); break;
            case RevisionDiffControl.Command.OpenWithDifftoolFirstToLocal: OpenFilesWithDiffTool(RevisionDiffKind.DiffALocal); break;
            case RevisionDiffControl.Command.OpenWithDifftoolSelectedToLocal: OpenFilesWithDiffTool(RevisionDiffKind.DiffBLocal); break;
            case RevisionDiffControl.Command.ResetSelectedFiles: ResetSelectedItemsWithConfirmation(resetToParent: true); break;
            case RevisionDiffControl.Command.StageSelectedFile: StageFile_Click(this, EventArgs.Empty); break;
            case RevisionDiffControl.Command.UnStageSelectedFile: UnstageFile_Click(this, EventArgs.Empty); break;
            case RevisionDiffControl.Command.ShowFileTree: _openInFileTreeTab_AsBlame?.Invoke(false); break;
            case RevisionDiffControl.Command.FilterFileInGrid: _filterFileInGrid?.Invoke(); break;
            case RevisionDiffControl.Command.SelectFirstGroupChanges:
                SelectedItems = FirstGroupItems;
                break;
            case RevisionDiffControl.Command.OpenWorkingDirectoryFileWith: OpenWorkingDirectoryFileWith_Click(this, EventArgs.Empty); break;
            case RevisionDiffControl.Command.OpenWorkingDirectoryFile: OpenWorkingDirectoryFile_Click(this, EventArgs.Empty); break;
            case RevisionDiffControl.Command.RenameMove: Move_Click(this, EventArgs.Empty); break;
            case RevisionDiffControl.Command.FindFile:
                return false;
            case RevisionDiffControl.Command.FindInCommitFilesUsingGitGrep_DiffTab:
                if (_isFileTreeMode)
                {
                    return false;
                }

                ShowFindInCommitFileGitGrepDialog(_getSelectedText?.Invoke() ?? string.Empty);
                break;
            case RevisionDiffControl.Command.GoToFirstParent:
            case RevisionDiffControl.Command.GoToLastParent:
            case RevisionDiffControl.Command.OpenInVisualStudio:
            case RevisionDiffControl.Command.AddFileToGitIgnore:
                return false;
            case RevisionDiffControl.Command.FindInCommitFilesUsingGitGrep_FileTreeTab:
                if (!_isFileTreeMode)
                {
                    return false;
                }

                ShowFindInCommitFileGitGrepDialog(_getSelectedText?.Invoke() ?? string.Empty);
                break;
            default:
                return false;
        }

        return true;
    }

    private void OpenFindInCommitFilesGitGrepDialog_Click(object? sender, EventArgs e)
    {
        ShowFindInCommitFileGitGrepDialog(_getSelectedText?.Invoke() ?? string.Empty);
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

        bool firstUsesSecondRevision = _rememberFileContextMenuController.ShouldEnableFirstItemDiff(diffFiles[0], isSecondRevision: true);
        string? first = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash, diffFiles[0], firstUsesSecondRevision);
        bool secondUsesSecondRevision = _rememberFileContextMenuController.ShouldEnableSecondItemDiff(diffFiles[1], isSecondRevision: true);
        string? second = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash, diffFiles[1], secondUsesSecondRevision);
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

        ContextMenuDiffToolInfo info = GetContextMenuDiffToolInfo(selected);
        tsmiDiffFirstToSelected.IsEnabled = _itemContextMenuController.ShouldShowMenuFirstToSelected(info);
        tsmiDiffFirstToLocal.IsEnabled = _itemContextMenuController.ShouldShowMenuFirstToLocal(info);
        tsmiDiffSelectedToLocal.IsEnabled = _itemContextMenuController.ShouldShowMenuSelectedToLocal(info);
        bool hideToLocal = _itemContextMenuController.ShouldHideToLocal(info);
        tsmiDiffFirstToLocal.IsVisible = !hideToLocal;
        tsmiDiffSelectedToLocal.IsVisible = !hideToLocal;

        tsmiDiffTwoSelected.IsVisible = selected.Count == 2;
        tsmiDiffTwoSelected.IsEnabled = selected.Count == 2
                                         && _rememberFileContextMenuController.ShouldEnableFirstItemDiff(selected[0])
                                         && _rememberFileContextMenuController.ShouldEnableSecondItemDiff(selected[1]);
        tsmiDiffWithRemembered.IsVisible = selected.Count == 1 && _rememberFileContextMenuController.RememberedDiffFileItem is not null;
        tsmiDiffWithRemembered.IsEnabled = tsmiDiffWithRemembered.IsVisible
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

    private string? DescribeRevisions(List<GitRevision> revisions)
        => revisions.Count switch
        {
            1 => revisions[0].ObjectId.ToShortString(),
            > 1 => _multipleDescription.Text,
            _ => null
        };

    private static ContextMenuDiffToolInfo GetContextMenuDiffToolInfo(IReadOnlyList<FileStatusItem> selected)
    {
        List<GitRevision> revisions = [.. selected.SecondRevs()];
        GitRevision? selectedRevision = revisions.Count == 1 ? revisions[0] : null;
        ObjectId[] parentIds = [.. selected.FirstIds()];
        return new ContextMenuDiffToolInfo(
            selectedRevision,
            parentIds,
            allAreNew: selected.All(item => item.Item.IsNew),
            allAreDeleted: selected.All(item => item.Item.IsDeleted),
            firstIsParent: parentIds.Length > 0,
            localExists: selected.All(item => !item.Item.IsDeleted));
    }

    private void OpenWorkingDirectoryFileWith_Click(object? sender, EventArgs e)
    {
        if (GetSelectedAbsolutePath() is string path && File.Exists(path))
        {
            OsShellUtil.OpenAs(path);
        }
    }

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
        => ResetSelectedItemsWithConfirmation(resetToParent: !ReferenceEquals(sender, tsmiResetFileToSelected));

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
        if (selected.Length == 0 || topLevel?.StorageProvider is null)
        {
            return;
        }

        if (selected.Length == 1)
        {
            FileStatusItem item = selected[0];
            IStorageFile? target = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                SuggestedFileName = Path.GetFileName(item.Item.Name),
            });
            if (target is not null)
            {
                await Module.SaveBlobAsAsync(target.Path.LocalPath, $"{item.SecondRevision.Guid}:\"{item.Item.Name}\"");
            }

            return;
        }

        IReadOnlyList<IStorageFolder> folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
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
