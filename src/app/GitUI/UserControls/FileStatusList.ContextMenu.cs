#nullable enable

using System.Text;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.HelperDialogs;
using GitUI.ScriptsEngine;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI;

partial class FileStatusList
{
    /* Mnemonics
        A Save as
        B Blame
        C Find in commit files, Commit submodule
        D Open with difftool
        E Edit, Reset submodule, Select all (on folder)
        F Find file
        G Filter in grid
        H History
        I Show in folder
        J
        K Skip worktree
        L Delete, Show "Find in commit files"
        M Assume unchanged
        N Open temp
        O Open (on file), Open (submodule), Collapse all (folder)
        P Copy paths
        Q
        R Reset files
        S Stage, Sort and group by
        T Show in file tree, Stash submodule
        U Unstage, Update submodule
        V Open in VS
        W Open temp with
        X Add file to .git/info/exclude, Expand all (on folder)
        Y Cherry pick
        Z
        . Add file to .gitignore

        Without mnemonic but with hotkey
        - Open with

        Not important
        - Stop tracking this file
        - Reset chunk of file
        - Interactive add
    */

    private Action? _blame;
    private Action? _cherryPickChanges;
    private Action? _filterFileInGrid;
    private Action<bool>? _openInFileTreeTab_AsBlame;
    private Action? _refreshParent;
    private Action? _stage;
    private Action? _unstage;

    private Func<GitRevision?>? _getCurrentRevision;
    private Func<int>? _getLineNumber;
    private Func<string>? _getSelectedText;
    private Func<bool>? _getSupportLinePatching;

    private readonly CancellationTokenSequence _customDiffToolsSequence = new();
    private readonly IFindFilePredicateProvider _findFilePredicateProvider = new FindFilePredicateProvider();
    private readonly IFileStatusListContextMenuController _itemContextMenuController = new FileStatusListContextMenuController();
    private readonly CancellationTokenSequence _interactiveAddResetChunkSequence = new();
    private readonly RememberFileContextMenuController _rememberFileContextMenuController = RememberFileContextMenuController.Default;

    private readonly TranslationString _deleteSelectedFilesCaption = new("Delete");
    private readonly TranslationString _deleteSelectedFiles = new("Are you sure you want to delete the selected file(s)?");
    private readonly TranslationString _deleteFailed = new("Delete file failed");
    private readonly TranslationString _firstRevision = new("First: A ");
    private readonly TranslationString _multipleDescription = new("<multiple>");
    private readonly TranslationString _resetSelectedChangesText = new("Are you sure you want to reset all selected files to {0}?");
    private readonly TranslationString _saveFileFilterAllFiles = new("All files");
    private readonly TranslationString _saveFileFilterCurrentFormat = new("Current format");
    private readonly TranslationString _selectedRevision = new("Second: B ");
    private readonly TranslationString _stopTrackingFail = new("Fail to stop tracking the file '{0}'.");

    private readonly TranslationString _skipWorktreeToolTip = new("Hide already tracked files that will change but that you don\'t want to commit."
        + Environment.NewLine + "Suitable for some config files modified locally.");
    private readonly TranslationString _assumeUnchangedToolTip = new("Tell git to not check the status of this file for performance benefits."
        + Environment.NewLine + "Use this feature when a file is big and never change."
        + Environment.NewLine + "Git will never check if the file has changed that will improve status check performance.");

    private void AddFileToGitIgnore_Click(object sender, EventArgs e)
    {
        AddFileToIgnoreFile(localExclude: false);
    }

    private void AddFileToGitInfoExclude_Click(object sender, EventArgs e)
    {
        AddFileToIgnoreFile(localExclude: true);
    }

    private void AddFileToIgnoreFile(bool localExclude)
    {
        string[] fileNames = [.. SelectedItems.Select(item => "/" + item.Item.Name)];
        if (fileNames.Length > 0 && UICommands.StartAddToGitIgnoreDialog(this, localExclude, fileNames))
        {
            RequestRefresh();
        }
    }

    private void AssumeUnchanged_Click(object sender, EventArgs e)
    {
        Module.AssumeUnchangedFiles([.. SelectedItems.Items()], tsmiAssumeUnchanged.Checked, out _);
        RequestRefresh();
    }

    public void BindContextMenu(Action refreshParent, bool canAutoRefresh, Action? stage, Action? unstage)
    {
        _refreshParent = refreshParent;
        _stage = stage;
        _unstage = unstage;

        btnRefresh.Click += (s, e) => refreshParent();
        btnRefresh.Visible = canAutoRefresh;
        tsmiRefreshOnFormFocus.Visible = canAutoRefresh;
        sepToolbar.Visible = canAutoRefresh;

        sepShow.Visible = canAutoRefresh;
        tsmiShowIgnoredFiles.Visible = canAutoRefresh;
        tsmiShowSkipWorktreeFiles.Visible = canAutoRefresh;
        tsmiShowAssumeUnchangedFiles.Visible = canAutoRefresh;
        tsmiShowUntrackedFiles.Visible = canAutoRefresh;
        tsmiShowUntrackedFiles.Checked = canAutoRefresh && Module.EffectiveConfigFile.GetValue("status.showuntrackedfiles") != "no";

        tsmiStageFile.Font = new Font(tsmiStageFile.Font, FontStyle.Bold);
        tsmiUnstageFile.Font = new Font(tsmiUnstageFile.Font, FontStyle.Bold);
    }

    public void BindContextMenu(Action cherryPickChanges, Func<bool> getSupportLinePatching)
    {
        _cherryPickChanges = cherryPickChanges;
        _getSupportLinePatching = getSupportLinePatching;

        tsmiCherryPickChanges.Visible = true;
    }

    public void BindContextMenu(
        Action? blame,
        Action cherryPickChanges,
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
        _filterFileInGrid = filterFileInGrid;
        _openInFileTreeTab_AsBlame = openInFileTreeTab_AsBlame;
        _refreshParent = refreshParent;
        _getCurrentRevision = getCurrentRevision;
        _getLineNumber = getLineNumber;
        _getSelectedText = getSelectedText;
        _getSupportLinePatching = getSupportLinePatching;

        tsmiCherryPickChanges.Visible = true;
        tsmiShowInFileTree.Visible = true;
        tsmiFilterFileInGrid.Visible = true;

        if (CanUseFindInCommitFilesGitGrep && !_isFileTreeMode)
        {
            tsmiOpenFindInCommitFilesGitGrepDialog.Visible = true;
            tsmiShowFindInCommitFilesGitGrep.Visible = true;
        }
    }

    private void Blame_Click(object sender, EventArgs e)
    {
        if (_blame is not null)
        {
            _blame?.Invoke();
        }
        else
        {
            StartFileHistoryDialog(showBlame: true);
        }
    }

    public void CancelLoadCustomDifftools()
    {
        _customDiffToolsSequence.CancelCurrent();
    }

    /// <summary>
    ///  Return whether it is possible to reset to the first commit.
    /// </summary>
    /// <param name="parentId">The parent commit id.</param>
    /// <param name="selectedItems">The selected file status items.</param>
    /// <returns><see langword="true"/> if it is possible to reset to first id.</returns>
    private static bool CanResetToFirst(ObjectId? parentId, IEnumerable<FileStatusItem> selectedItems)
    {
        return CanResetToSecond(parentId) || (parentId == ObjectId.IndexId && selectedItems.SecondIds().All(i => i == ObjectId.WorkTreeId));
    }

    /// <summary>
    ///  Return whether it is possible to reset to the second (selected) commit.
    /// </summary>
    /// <param name="resetId">The selected commit id.</param>
    /// <returns><see langword="true"/> if it is possible to reset to first id.</returns>
    private static bool CanResetToSecond(ObjectId? resetId) => resetId?.IsArtificial is false;

    private void CherryPickChanges_Click(object sender, EventArgs e)
    {
        _cherryPickChanges?.Invoke();
    }

    private void CommitSubmoduleChanges_Click(object sender, EventArgs e)
    {
        string[] submodules = [.. SelectedItems.Where(it => it.Item.IsSubmodule).Select(it => it.Item.Name).Distinct()];
        foreach (string name in submodules)
        {
            IGitUICommands submodulCommands = UICommands.WithWorkingDirectory(_fullPathResolver.Resolve(name.EnsureTrailingPathSeparator()));
            submodulCommands.StartCommitDialog(this);
        }

        RequestRefresh();
    }

    private void DeleteFile_Click(object sender, EventArgs e)
    {
        try
        {
            FileStatusItem[] selected = [.. SelectedItems];
            if (selected.Length == 0 || !selected[0].SecondRevision.IsArtificial ||
                MessageBox.Show(this, _deleteSelectedFiles.Text, _deleteSelectedFilesCaption.Text, MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) !=
                DialogResult.Yes)
            {
                return;
            }

            StoreNextItemToSelect();

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
        }

        return;

        void DeleteFromFilesystem(IEnumerable<FileStatusItem> items)
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
                    File.Delete(path!);
                }
            }
        }
    }

    /// <summary>
    /// Provide a description for the first selected or parent to the "primary" selected last.
    /// </summary>
    /// <returns>A description of the selected parent.</returns>
    private string? DescribeRevisions(List<GitRevision> parents)
    {
        return parents.Count switch
        {
            1 => GetDescriptionForRevision(parents[0]?.ObjectId),
            > 1 => _multipleDescription.Text,
            _ => null
        };
    }

    private void DiffFirstToLocal_Click(object? sender, EventArgs e)
    {
        OpenFilesWithDiffTool(RevisionDiffKind.DiffALocal, sender);
    }

    private void DiffFirstToSelected_Click(object? sender, EventArgs e)
    {
        OpenFilesWithDiffTool(RevisionDiffKind.DiffAB, sender);
    }

    private void DiffSelectedToLocal_Click(object? sender, EventArgs e)
    {
        OpenFilesWithDiffTool(RevisionDiffKind.DiffBLocal, sender);
    }

    private void DiffTwoSelected_Click(object? sender, EventArgs e)
    {
        ToolStripMenuItem? item = sender as ToolStripMenuItem;
        if (item?.DropDownItems is not null)
        {
            // "main menu" clicked, cancel dropdown manually, invoke default difftool
            item.HideDropDown();
        }

        string? toolName = item?.Tag as string;
        List<FileStatusItem> diffFiles = SelectedItems.ToList();
        if (diffFiles.Count != 2)
        {
            return;
        }

        // The order is always the order in the list, not clicked order, but the (last) selected is known
        int firstIndex = FocusedItem == diffFiles[0] ? 1 : 0;
        int secondIndex = 1 - firstIndex;

        // Fallback to first revision if second revision cannot be used
        bool isFirstItemSecondRev = _rememberFileContextMenuController.ShouldEnableFirstItemDiff(diffFiles[firstIndex], isSecondRevision: true);
        string? first = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash, diffFiles[firstIndex], isSecondRevision: isFirstItemSecondRev);
        bool isSecondItemSecondRev = _rememberFileContextMenuController.ShouldEnableSecondItemDiff(diffFiles[secondIndex], isSecondRevision: true);
        string? second = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash, diffFiles[secondIndex], isSecondRevision: isSecondItemSecondRev);

        Module.OpenFilesWithDifftool(first, second, customTool: toolName);
    }

    private void DiffWithRemembered_Click(object? sender, EventArgs e)
    {
        ToolStripMenuItem? item = sender as ToolStripMenuItem;
        if (item?.DropDownItems is not null)
        {
            // "main menu" clicked, cancel dropdown manually, invoke default difftool
            item.HideDropDown();
        }

        string? toolName = item?.Tag as string;

        // For first item, the second revision is explicitly remembered
        string? first = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash,
            _rememberFileContextMenuController.RememberedDiffFileItem, isSecondRevision: true);

        // Fallback to first revision if second cannot be used
        bool isSecond = _rememberFileContextMenuController.ShouldEnableSecondItemDiff(SelectedItem, isSecondRevision: true);
        string? second = _rememberFileContextMenuController.GetGitCommit(Module.GetFileBlobHash, SelectedItem, isSecondRevision: isSecond);

        Module.OpenFilesWithDifftool(first, second, customTool: toolName);
    }

    private void EditWorkingDirectoryFile_Click(object sender, EventArgs e)
    {
        if (SelectedItem is null)
        {
            return;
        }

        string? fileName = _fullPathResolver.Resolve(SelectedItem.Item.Name);
        UICommands.StartFileEditorDialog(fileName, lineNumber: GetLineNumber());
        RequestRefresh();
    }

    internal bool ExecuteCommand(RevisionDiffControl.Command cmd) => ExecuteCommand((int)cmd);

    protected override bool ExecuteCommand(int cmd)
    {
        if ((FilterFilesByNameRegexFocused || FindInCommitFilesGitGrepFocused) && IsTextEditKey(GetShortcutKeys(cmd)))
        {
            return false;
        }

        UpdateStatusOfMenuItems();

        switch ((RevisionDiffControl.Command)cmd)
        {
            case RevisionDiffControl.Command.DeleteSelectedFiles: tsmiDeleteFile.PerformClick(); break;
            case RevisionDiffControl.Command.ShowHistory: tsmiFileHistory.PerformClick(); break;
            case RevisionDiffControl.Command.Blame: tsmiBlame.PerformClick(); break;
            case RevisionDiffControl.Command.OpenWithDifftool: OpenFilesWithDiffTool(RevisionDiffKind.DiffAB); break;
            case RevisionDiffControl.Command.OpenWithDifftoolFirstToLocal: OpenFilesWithDiffTool(RevisionDiffKind.DiffALocal); break;
            case RevisionDiffControl.Command.OpenWithDifftoolSelectedToLocal: OpenFilesWithDiffTool(RevisionDiffKind.DiffBLocal); break;
            case RevisionDiffControl.Command.OpenWorkingDirectoryFile: tsmiOpenWorkingDirectoryFile.PerformClick(); break;
            case RevisionDiffControl.Command.OpenWorkingDirectoryFileWith: tsmiOpenWorkingDirectoryFileWith.PerformClick(); break;
            case RevisionDiffControl.Command.EditFile: tsmiEditWorkingDirectoryFile.PerformClick(); break;
            case RevisionDiffControl.Command.OpenAsTempFile: tsmiOpenRevisionFile.PerformClick(); break;
            case RevisionDiffControl.Command.OpenAsTempFileWith: tsmiOpenRevisionFileWith.PerformClick(); break;
            case RevisionDiffControl.Command.ResetSelectedFiles: return ResetSelectedFilesWithConfirmation();
            case RevisionDiffControl.Command.StageSelectedFile: tsmiStageFile.PerformClick(); break;
            case RevisionDiffControl.Command.UnStageSelectedFile: tsmiUnstageFile.PerformClick(); break;
            case RevisionDiffControl.Command.ShowFileTree: tsmiShowInFileTree.PerformClick(); break;
            case RevisionDiffControl.Command.FilterFileInGrid: tsmiFilterFileInGrid.PerformClick(); break;
            case RevisionDiffControl.Command.SelectFirstGroupChanges: return SelectFirstGroupChangesIfFocused();
            case RevisionDiffControl.Command.FindFile: tsmiFindFile.PerformClick(); break;
            case RevisionDiffControl.Command.FindInCommitFilesUsingGitGrep:
                if (_isFileTreeMode)
                {
                    return base.ExecuteCommand(cmd);
                }

                tsmiOpenFindInCommitFilesGitGrepDialog.PerformClick();
                break;
            case RevisionDiffControl.Command.OpenInVisualStudio: tsmiOpenInVisualStudio.PerformClick(); break;
            case RevisionDiffControl.Command.AddFileToGitIgnore: return AddFileToGitIgnore();
            default: return base.ExecuteCommand(cmd);
        }

        return true;

        bool AddFileToGitIgnore()
        {
            if (!Focused)
            {
                return false;
            }

            tsmiAddFileToGitIgnore.PerformClick();
            return true;
        }

        bool ResetSelectedFilesWithConfirmation()
        {
            if (!Focused)
            {
                return false;
            }

            InitResetFileToToolStripMenuItem();
            if (!tsmiResetFileToParent.Enabled)
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
            if (!Focused)
            {
                return false;
            }

            SelectedItems = FirstGroupItems;
            return true;
        }
    }

    private void FileHistory_Click(object sender, EventArgs e)
    {
        StartFileHistoryDialog(showBlame: false);
    }

    private void FilterFileInGrid_Click(object sender, EventArgs e)
    {
        _filterFileInGrid?.Invoke();
    }

    private void FindFile_Click(object sender, EventArgs e)
    {
        IReadOnlyList<GitItemStatus> candidates = GitItemStatuses;

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
            SelectedGitItem = selectedItem;
        }
    }

    private ContextMenuDiffToolInfo GetContextMenuDiffToolInfo()
    {
        // Some items are not supported if more than one revision is selected
        List<GitRevision> revisions = SelectedItems.SecondRevs().ToList();
        GitRevision? selectedRev = revisions.Count == 1 ? revisions[0] : null;

        List<ObjectId> parentIds = SelectedItems.FirstIds().ToList();
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

    private int GetLineNumber()
        => _getLineNumber is not null
            ? _getLineNumber()
            : FindScriptOptionsProvider() is IScriptOptionsProvider scriptOptionsProvider
                ? int.Parse(scriptOptionsProvider.GetValues(ScriptOptionsProvider._lineNumber).FirstOrDefault("0"))
                : 0;

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

    public void InitResetFileToToolStripMenuItem()
    {
        // Multiple parent/child can be selected, only the the first is shown.
        // The only artificial commit that can be reset to is Index<-WorkTree
        ObjectId? selectedId = SelectedItems.SecondIds().FirstOrDefault();
        ObjectId? parentId = SelectedItems.FirstIds().FirstOrDefault();

        bool canResetToSecond = CanResetToSecond(selectedId);
        tsmiResetFileToSelected.Enabled = canResetToSecond;
        tsmiResetFileToSelected.Visible = canResetToSecond;
        if (canResetToSecond)
        {
            tsmiResetFileToSelected.Text =
                _selectedRevision + GetDescriptionForRevision(selectedId);
        }

        bool canResetToFirst = CanResetToFirst(parentId, SelectedItems);
        tsmiResetFileToParent.Enabled = canResetToFirst;
        tsmiResetFileToParent.Visible = canResetToFirst;
        if (canResetToFirst)
        {
            tsmiResetFileToParent.Text =
                _firstRevision + GetDescriptionForRevision(parentId);
        }

        bool canReset = canResetToSecond || canResetToFirst;
        tsmiResetFileTo.Enabled = canReset;
    }

    private void InteractiveAdd_Click(object sender, EventArgs e)
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

    public void LoadCustomDifftools()
    {
        List<CustomDiffMergeTool> menus =
        [
            new(tsmiDiffFirstToSelected, DiffFirstToSelected_Click),
            new(tsmiDiffSelectedToLocal, DiffSelectedToLocal_Click),
            new(tsmiDiffFirstToLocal, DiffFirstToLocal_Click),
            new(tsmiDiffWithRemembered, DiffWithRemembered_Click),
            new(tsmiDiffTwoSelected, DiffTwoSelected_Click)
        ];

        new CustomDiffMergeToolProvider().LoadCustomDiffMergeTools(Module, menus, components, isDiff: true, cancellationToken: _customDiffToolsSequence.Next());
    }

    private void OpenFilesWithDiffTool(RevisionDiffKind diffKind, object? sender)
    {
        ToolStripMenuItem? item = sender as ToolStripMenuItem;
        if (item?.DropDownItems != null)
        {
            // "main menu" clicked, cancel dropdown manually, invoke default mergetool
            item.HideDropDown();
            item.Owner?.Hide();
        }

        string? toolName = item?.Tag as string;
        OpenFilesWithDiffTool(diffKind, toolName);
    }

    private void OpenFilesWithDiffTool(RevisionDiffKind diffKind, string? toolName = null)
    {
        using (WaitCursorScope.Enter())
        {
            foreach (FileStatusItem item in SelectedItems)
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

    private void OpenFindInCommitFilesGitGrepDialog_Click(object sender, EventArgs e)
    {
        ShowFindInCommitFileGitGrepDialog(_getSelectedText?.Invoke() ?? "");
    }

    private void OpenInVisualStudio_Click(object sender, EventArgs e)
    {
        if (VisualStudioIntegration.IsVisualStudioInstalled && SelectedItemAbsolutePath is string itemName)
        {
            VisualStudioIntegration.OpenFile(itemName, GetLineNumber());
        }
    }

    private void OpenRevisionFile_Click(object sender, EventArgs e)
    {
        SaveSelectedItemToTempFile(fileName => OsShellUtil.Open(fileName));
    }

    private void OpenRevisionFileWith_Click(object sender, EventArgs e)
    {
        SaveSelectedItemToTempFile(OsShellUtil.OpenAs);
    }

    private void OpenWithDifftool_DropDownOpening(object sender, EventArgs e)
    {
        ContextMenuDiffToolInfo selectionInfo = GetContextMenuDiffToolInfo();
        List<GitRevision> revisions = SelectedItems.SecondRevs().ToList();

        if (revisions.Any())
        {
            tsmiSecondDiffCaption.Text = _selectedRevision + (DescribeRevisions(revisions) ?? string.Empty);
            tsmiSecondDiffCaption.Visible = true;
            MenuUtil.SetAsCaptionMenuItem(tsmiSecondDiffCaption, ItemContextMenu);

            tsmiFirstDiffCaption.Text = _firstRevision + (DescribeRevisions(SelectedItems.FirstRevs().ToList()) ?? string.Empty);
            tsmiFirstDiffCaption.Visible = true;
            MenuUtil.SetAsCaptionMenuItem(tsmiFirstDiffCaption, ItemContextMenu);
        }
        else
        {
            tsmiFirstDiffCaption.Visible = false;
            tsmiSecondDiffCaption.Visible = false;
        }

        tsmiDiffFirstToSelected.Enabled = _itemContextMenuController.ShouldShowMenuFirstToSelected(selectionInfo);
        tsmiDiffFirstToLocal.Enabled = _itemContextMenuController.ShouldShowMenuFirstToLocal(selectionInfo);
        tsmiDiffSelectedToLocal.Enabled = _itemContextMenuController.ShouldShowMenuSelectedToLocal(selectionInfo);
        tsmiDiffFirstToLocal.Visible
            = tsmiDiffSelectedToLocal.Visible
            = !_itemContextMenuController.ShouldHideToLocal(selectionInfo);

        List<FileStatusItem> diffFiles = SelectedItems.ToList();
        sepDifftoolRemember.Visible = diffFiles.Count == 1 || diffFiles.Count == 2;

        // The order is always the order in the list, not clicked order, but the (last) selected is known
        int firstIndex = diffFiles.Count == 2 && FocusedItem == diffFiles[0] ? 1 : 0;
        int secondIndex = 1 - firstIndex;

        tsmiDiffTwoSelected.Visible = diffFiles.Count == 2;
        tsmiDiffTwoSelected.Enabled =
            diffFiles.Count == 2
            && _rememberFileContextMenuController.ShouldEnableFirstItemDiff(diffFiles[firstIndex])
            && _rememberFileContextMenuController.ShouldEnableSecondItemDiff(diffFiles[secondIndex]);

        tsmiDiffWithRemembered.Visible = diffFiles.Count == 1 && _rememberFileContextMenuController.RememberedDiffFileItem is not null;
        tsmiDiffWithRemembered.Enabled =
            diffFiles.Count == 1
            && diffFiles[0] != _rememberFileContextMenuController.RememberedDiffFileItem
            && _rememberFileContextMenuController.ShouldEnableSecondItemDiff(diffFiles[0]);
        tsmiDiffWithRemembered.Text =
            _rememberFileContextMenuController.RememberedDiffFileItem is not null
                ? string.Format(TranslatedStrings.DiffSelectedWithRememberedFile, _rememberFileContextMenuController.RememberedDiffFileItem.Item.Name)
                : string.Empty;

        tsmiRememberSecondRevDiff.Visible = diffFiles.Count == 1;
        tsmiRememberSecondRevDiff.Enabled = diffFiles.Count == 1
                                            && _rememberFileContextMenuController.ShouldEnableFirstItemDiff(diffFiles[0], isSecondRevision: true);

        tsmiRememberFirstRevDiff.Visible = diffFiles.Count == 1;
        tsmiRememberFirstRevDiff.Enabled = diffFiles.Count == 1
                                           && _rememberFileContextMenuController.ShouldEnableFirstItemDiff(diffFiles[0], isSecondRevision: false);
    }

    private void OpenWorkingDirectoryFile_Click(object sender, EventArgs e)
    {
        if (SelectedGitItem is GitItemStatus gitItemStatus
            && _fullPathResolver.Resolve(gitItemStatus.Name) is string fileName)
        {
            OsShellUtil.Open(fileName.ToNativePath());
        }
    }

    private void OpenWorkingDirectoryFileWith_Click(object sender, EventArgs e)
    {
        if (SelectedGitItem is GitItemStatus gitItemStatus
            && _fullPathResolver.Resolve(gitItemStatus.Name) is string fileName)
        {
            OsShellUtil.OpenAs(fileName.ToNativePath());
        }
    }

    public void RepositoryChanged()
    {
        _rememberFileContextMenuController.RememberedDiffFileItem = null;
    }

    public void ReloadHotkeys()
    {
        HotkeysEnabled = true;
        LoadHotkeys(RevisionDiffControl.HotkeySettingsName);
        tsmiDeleteFile.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.DeleteSelectedFiles);
        tsmiFileHistory.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.ShowHistory);
        tsmiBlame.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.Blame);
        tsmiDiffFirstToSelected.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.OpenWithDifftool);
        tsmiDiffFirstToLocal.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.OpenWithDifftoolFirstToLocal);
        tsmiDiffSelectedToLocal.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.OpenWithDifftoolSelectedToLocal);
        tsmiOpenWorkingDirectoryFile.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.OpenWorkingDirectoryFile);
        tsmiOpenWorkingDirectoryFileWith.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.OpenWorkingDirectoryFileWith);
        tsmiEditWorkingDirectoryFile.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.EditFile);
        tsmiOpenRevisionFile.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.OpenAsTempFile);
        tsmiOpenRevisionFileWith.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.OpenAsTempFileWith);
        tsmiResetFileToParent.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.ResetSelectedFiles);
        tsmiStageFile.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.StageSelectedFile);
        tsmiUnstageFile.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.UnStageSelectedFile);
        tsmiShowInFileTree.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.ShowFileTree);
        tsmiFilterFileInGrid.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.FilterFileInGrid);
        tsmiFindFile.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.FindFile);
        tsmiOpenFindInCommitFilesGitGrepDialog.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.FindInCommitFilesUsingGitGrep);
        tsmiOpenInVisualStudio.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.OpenInVisualStudio);
        tsmiAddFileToGitIgnore.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(RevisionDiffControl.Command.AddFileToGitIgnore);
    }

    private void RememberFirstRevDiff_Click(object sender, EventArgs e)
    {
        if (SelectedItem?.FirstRevision is null)
        {
            return;
        }

        FileStatusItem item = new(
            firstRev: SelectedItem.SecondRevision,
            secondRev: SelectedItem.FirstRevision,
            item: SelectedItem.Item);
        if (!string.IsNullOrWhiteSpace(SelectedItem.Item.OldName))
        {
            string name = SelectedItem.Item.OldName;
            SelectedItem.Item.OldName = SelectedItem.Item.Name;
            SelectedItem.Item.Name = name;
        }

        _rememberFileContextMenuController.RememberedDiffFileItem = item;
    }

    private void RememberSecondRevDiff_Click(object sender, EventArgs e)
    {
        _rememberFileContextMenuController.RememberedDiffFileItem = SelectedItem;
    }

    private void RequestRefresh()
    {
        if (_refreshParent is not null)
        {
            _refreshParent.Invoke();
        }
        else
        {
            btnRefresh.PerformClick();
        }
    }

    private void ResetChunkOfFile_Click(object sender, EventArgs e)
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

    private void ResetFile_Click(object? sender, EventArgs e)
    {
        if (sender == tsmiResetFileTo)
        {
            sender = tsmiResetFileToParent;
            if (!tsmiResetFileToParent.Enabled)
            {
                return;
            }
        }

        ResetSelectedItemsWithConfirmation(resetToParent: sender == tsmiResetFileToParent);
    }

    public void ResetSelectedItemsWithConfirmation(bool resetToParent)
    {
        FileStatusItem[] items = [.. SelectedItems];

        // The "new" state could change when resetting, allow user to tick the checkbox.
        // If there are only changed files, it is safe to disable the checkboc (also for restting to selected).
        bool hasNewFiles = !items.All(item => item.Item.IsChanged);
        bool hasExistingFiles = items.Any(item => !(item.Item.IsUncommittedAdded || IsRenamedIndexItem(item)));

        string revDescription = resetToParent
            ? $"{_firstRevision}{DescribeRevisions(items.FirstRevs().ToList())}"
            : $"{_selectedRevision}{DescribeRevisions(items.SecondRevs().ToList())}";
        string confirmationMessage = string.Format(_resetSelectedChangesText.Text, revDescription);

        FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(ParentForm, hasExistingFiles, hasNewFiles, confirmationMessage);
        if (resetType == FormResetChanges.ActionEnum.Cancel)
        {
            return;
        }

        bool resetAndDelete = resetType == FormResetChanges.ActionEnum.ResetAndDelete;
        ResetSelectedItemsTo(resetToParent, resetAndDelete);

        return;

        static bool IsRenamedIndexItem(FileStatusItem item) => item.Item.IsRenamed && item.Item.Staged == StagedStatus.Index;

        void ResetSelectedItemsTo(bool resetToParent, bool resetAndDelete)
        {
            FileStatusItem[] selectedItems = [.. SelectedItems];

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
    }

    private void ResetSubmoduleChanges_Click(object sender, EventArgs e)
    {
        string[] submodules = [.. SelectedItems.Where(it => it.Item.IsSubmodule).Select(it => it.Item.Name).Distinct()];

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

    private void SaveAs_Click(object sender, EventArgs e)
    {
        List<FileStatusItem> files = SelectedItems.ToList();

        Func<string, string?>? userSelection = null;
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

        if (userSelection is not null)
        {
            _revisionDiffController.SaveFiles(files, userSelection);
        }
    }

    private void SaveSelectedItemToTempFile(Action<string> onSaved)
    {
        FileStatusItem? item = SelectedItem;
        if (item?.Item.Name is null)
        {
            return;
        }

        ThreadHelper.FileAndForget(async () =>
        {
            ObjectId? blob = Module.GetFileBlobHash(item.Item.Name, item.SecondRevision.ObjectId);

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

    private void ShowFindInCommitFilesGitGrep_Click(object sender, EventArgs e)
    {
        AppSettings.ShowFindInCommitFilesGitGrep.Value = tsmiShowFindInCommitFilesGitGrep.Checked;
        SetFindInCommitFilesGitGrepVisibility(AppSettings.ShowFindInCommitFilesGitGrep.Value);
    }

    private void ShowInFileTree_Click(object sender, EventArgs e)
    {
        if (!_isFileTreeMode)
        {
            _openInFileTreeTab_AsBlame?.Invoke(false);
        }
    }

    private void ShowInFolder_Click(object sender, EventArgs e)
    {
        FormBrowse.OpenContainingFolder(this, Module);
    }

    private void SkipWorktree_Click(object sender, EventArgs e)
    {
        Module.SkipWorktreeFiles([.. SelectedItems.Items()], tsmiSkipWorktree.Checked, out _);
        RequestRefresh();
    }

    private void StageFile_Click(object sender, EventArgs e)
    {
        if (_stage is not null)
        {
            _stage();
            return;
        }

        GitItemStatus[] files = [.. SelectedItems.Where(item => item.Item.Staged == StagedStatus.WorkTree).Select(i => i.Item)];
        Module.StageFiles(files, out _);
        RequestRefresh();
    }

    private void StartFileHistoryDialog(bool showBlame)
    {
        (string? fileName, GitRevision? revision) = SelectedFolder is RelativePath relativePath
            ? (relativePath.Length == 0 ? null : relativePath.Value, _getCurrentRevision?.Invoke())
            : SelectedItem is FileStatusItem item && item.Item.IsTracked
                ? (item.Item.Name, item.SecondRevision)
                : (null, null);
        if (fileName is null)
        {
            return;
        }

        UICommands.StartFileHistoryDialog(this, fileName, revision, showBlame: showBlame);
    }

    private void StashSubmoduleChanges_Click(object sender, EventArgs e)
    {
        string[] submodules = [.. SelectedItems.Where(it => it.Item.IsSubmodule).Select(it => it.Item.Name).Distinct()];
        foreach (string name in submodules)
        {
            IGitUICommands uiCmds = UICommands.WithGitModule(Module.GetSubmodule(name));
            uiCmds.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash);
        }

        RequestRefresh();
    }

    private void StopTracking_Click(object sender, EventArgs e)
    {
        if (SelectedGitItem?.Name is not string filename)
        {
            return;
        }

        if (Module.StopTrackingFile(filename))
        {
            RequestRefresh();
        }
        else
        {
            MessageBox.Show(string.Format(_stopTrackingFail.Text, filename), TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UnstageFile_Click(object sender, EventArgs e)
    {
        if (_unstage is not null)
        {
            _unstage();
            return;
        }

        GitItemStatus[] files = [.. SelectedItems.Where(item => item.Item.Staged == StagedStatus.Index).Select(i => i.Item)];
        Module.BatchUnstageFiles(files);
        RequestRefresh();
    }

    public void UpdateStatusOfMenuItems()
    {
        ContextMenuSelectionInfo selectionInfo = GetSelectionInfo([.. SelectedItems], SelectedFolder, isBareRepository: Module.IsBareRepository(), supportLinePatching: _getSupportLinePatching?.Invoke() ?? false, _fullPathResolver);

        // Many options have no meaning for artificial commits or submodules
        // Hide the obviously no action options when single selected, handle them in actions if multi select

        // open submodule is added in FileStatusList
        tsmiFileHistory.Font = ((SelectedItem?.Item.IsSubmodule ?? false) && AppSettings.OpenSubmoduleDiffInSeparateWindow) || _openInFileTreeTab_AsBlame is null
            ? new Font(tsmiFileHistory.Font, FontStyle.Regular)
            : new Font(tsmiFileHistory.Font, FontStyle.Bold);

        tsmiUpdateSubmodule.Visible
            = tsmiResetSubmoduleChanges.Visible
            = tsmiStashSubmoduleChanges.Visible
            = tsmiCommitSubmoduleChanges.Visible
            = sepSubmodule.Visible
            = _revisionDiffController.ShouldShowSubmoduleMenus(selectionInfo);

        tsmiStageFile.Enabled
            = tsmiStageFile.Visible
            = _revisionDiffController.ShouldShowMenuStage(selectionInfo);
        tsmiUnstageFile.Enabled
            = tsmiUnstageFile.Visible
            = _revisionDiffController.ShouldShowMenuUnstage(selectionInfo);
        InitResetFileToToolStripMenuItem();
        if (!_revisionDiffController.ShouldShowResetFileMenus(selectionInfo))
        {
            tsmiResetFileTo.Enabled = false;
        }

        tsmiCherryPickChanges.Visible = _revisionDiffController.ShouldShowMenuCherryPick(selectionInfo);

        sepFile.Visible = _revisionDiffController.ShouldShowDifftoolMenus(selectionInfo)
            || _revisionDiffController.ShouldShowMenuDeleteFile(selectionInfo)
            || _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo)
            || _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);

        tsmiOpenWithDifftool.Enabled = _revisionDiffController.ShouldShowDifftoolMenus(selectionInfo);
        tsmiOpenWorkingDirectoryFileWith.Visible = _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo);
        tsmiOpenRevisionFile.Visible = _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);
        tsmiOpenRevisionFile.Enabled = _revisionDiffController.ShouldShowMenuShowInFileTree(selectionInfo);
        tsmiOpenRevisionFileWith.Visible = _revisionDiffController.ShouldShowMenuOpenRevision(selectionInfo);
        tsmiOpenRevisionFileWith.Enabled = _revisionDiffController.ShouldShowMenuShowInFileTree(selectionInfo);
        tsmiSaveAs.Visible = _revisionDiffController.ShouldShowMenuSaveAs(selectionInfo);
        tsmiShowInFolder.Visible = _revisionDiffController.ShouldShowMenuShowInFolder(selectionInfo);
        tsmiEditWorkingDirectoryFile.Visible = _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo);
        tsmiOpenInVisualStudio.Visible = _revisionDiffController.ShouldShowMenuEditWorkingDirectoryFile(selectionInfo) && VisualStudioIntegration.IsVisualStudioInstalled;
        tsmiDeleteFile.Text = ResourceManager.TranslatedStrings.GetDeleteFile(selectionInfo.SelectedGitItemCount);
        tsmiDeleteFile.Enabled = _revisionDiffController.ShouldShowMenuDeleteFile(selectionInfo);
        tsmiDeleteFile.Visible = tsmiDeleteFile.Enabled;

        tsmiCopyPaths.Enabled = _revisionDiffController.ShouldShowMenuCopyFileName(selectionInfo);
        tsmiShowInFolder.Enabled = false;

        foreach (FileStatusItem item in SelectedItems)
        {
            string? filePath = _fullPathResolver.Resolve(item.Item.Name);
            if (filePath is not null && FormBrowseUtil.FileOrParentDirectoryExists(filePath))
            {
                tsmiShowInFolder.Enabled = true;
                break;
            }
        }

        // Visibility of FileTree is not known, assume (CommitInfoTabControl.Contains(TreeTabPage);)
        tsmiShowInFileTree.Visible = _openInFileTreeTab_AsBlame is not null && _revisionDiffController.ShouldShowMenuShowInFileTree(selectionInfo);
        tsmiFilterFileInGrid.Enabled = _filterFileInGrid is not null && _revisionDiffController.ShouldShowMenuFileHistory(selectionInfo);
        tsmiFileHistory.Enabled = _revisionDiffController.ShouldShowMenuFileHistory(selectionInfo);
        tsmiBlame.Enabled = AppSettings.UseDiffViewerForBlame.Value || _blame is null
                ? _revisionDiffController.ShouldShowMenuBlame(selectionInfo)
                : _revisionDiffController.ShouldShowMenuShowInFileTree(selectionInfo);
        if (!tsmiBlame.Enabled)
        {
            tsmiBlame.Checked = false;
        }

        sepScripts.Visible = ItemContextMenu.AddUserScripts(tsmiRunScript, ExecuteCommand, script => script.OnEvent == ScriptEvent.ShowInFileList, UICommands);

        tsmiShowFindInCommitFilesGitGrep.Checked = FindInCommitFilesGitGrepVisible;

        bool isSubmodule = selectionInfo.SelectedGitItemCount == 1 && selectionInfo.IsAnySubmodule;
        bool isSingleFile = selectionInfo.SelectedGitItemCount == 1 && !isSubmodule;

        bool canResetAddInteractively = selectionInfo.IsAnyItemWorkTree && isSingleFile;
        tsmiResetChunkOfFile.Visible = canResetAddInteractively;
        tsmiInteractiveAdd.Visible = canResetAddInteractively;

        bool canOpenFile = selectionInfo.SelectedGitItemCount == 1 && selectionInfo.AllFilesExist;
        tsmiOpenWorkingDirectoryFile.Visible = canOpenFile;
        tsmiOpenWorkingDirectoryFileWith.Visible = canOpenFile;

        bool canIgnoreFiles = selectionInfo.IsAnyItemWorkTree && !isSubmodule;
        bool canStopTracking = isSingleFile && selectionInfo.IsAnyTracked;

        sepIgnore.Visible = canIgnoreFiles || canStopTracking;

        tsmiAddFileToGitIgnore.Visible = canIgnoreFiles;
        tsmiAddFileToGitInfoExclude.Visible = canIgnoreFiles;
        tsmiSkipWorktree.Visible = canIgnoreFiles && selectionInfo.IsAnyTracked;
        tsmiAssumeUnchanged.Visible = canIgnoreFiles && selectionInfo.IsAnyTracked;

        tsmiStopTracking.Visible = canStopTracking;

        tsmiSkipWorktree.ToolTipText = _skipWorktreeToolTip.Text;
        tsmiAssumeUnchanged.ToolTipText = _assumeUnchangedToolTip.Text;
    }

    private void UpdateSubmodule_Click(object sender, EventArgs e)
    {
        string[] submodules = [.. SelectedItems.Where(it => it.Item.IsSubmodule).Select(it => it.Item.Name).Distinct()];
        FormProcess.ShowDialog(FindForm() as FormBrowse, UICommands, arguments: Commands.SubmoduleUpdate(submodules), Module.WorkingDir, input: null, useDialogSettings: true);
        RequestRefresh();
    }
}
