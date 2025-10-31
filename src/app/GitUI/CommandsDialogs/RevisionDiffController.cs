using System.Diagnostics;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls;

namespace GitUI.CommandsDialogs;

internal interface IRevisionDiffController
{
    void SaveFiles(List<FileStatusItem> files, Func<string, string?> userSelection);
    bool ShouldShowMenuBlame(ContextMenuSelectionInfo selectionInfo);
    bool ShouldShowMenuCherryPick(ContextMenuSelectionInfo selectionInfo);
    bool ShouldShowMenuEditWorkingDirectoryFile(ContextMenuSelectionInfo selectionInfo);
    bool ShouldShowMenuOpenRevision(ContextMenuSelectionInfo selectionInfo);
    bool ShouldShowMenuDeleteFile(ContextMenuSelectionInfo selectionInfo);
    bool ShouldShowMenuMove(ContextMenuSelectionInfo selectionInfo);
    bool ShouldShowResetFileMenus(ContextMenuSelectionInfo selectionInfo);
    bool ShouldShowMenuFileHistory(ContextMenuSelectionInfo selectionInfo);
    bool ShouldShowMenuSaveAs(ContextMenuSelectionInfo selectionInfo);
    bool ShouldShowMenuCopyFileName(ContextMenuSelectionInfo selectionInfo);
    bool ShouldShowMenuShowInFolder(ContextMenuSelectionInfo selectionInfo);
    bool ShouldShowMenuShowInFileTree(ContextMenuSelectionInfo selectionInfo);
    bool ShouldShowMenuStage(ContextMenuSelectionInfo selectionInfo);
    bool ShouldShowMenuUnstage(ContextMenuSelectionInfo selectionInfo);
    bool ShouldShowSubmoduleMenus(ContextMenuSelectionInfo selectionInfo);
    bool ShouldShowDifftoolMenus(ContextMenuSelectionInfo selectionInfo);
}

internal sealed class RevisionDiffController(Func<IGitModule> getModule, IFullPathResolver fullPathResolver) : IRevisionDiffController
{
    private readonly Func<IGitModule> _getModule = getModule;
    private readonly IFullPathResolver _fullPathResolver = fullPathResolver;

    public void SaveFiles(List<FileStatusItem> files, Func<string, string?> userSelection)
    {
        ArgumentNullException.ThrowIfNull(files);

        if (files.Count == 0)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(userSelection);

        if (files.Count > 1)
        {
            SaveMultipleFiles(files);
            return;
        }

        SaveSingleFile(files[0]);
        return;

        void SaveMultipleFiles(List<FileStatusItem> selectedFiles)
        {
            // Derive the folder from the first selected file.
            string firstItemFullName = _fullPathResolver.Resolve(selectedFiles[0].Item.Name);
            string baseSourceDirectory = Path.GetDirectoryName(firstItemFullName).EnsureTrailingPathSeparator();

            string selectedPath = userSelection(baseSourceDirectory);
            if (selectedPath is null)
            {
                // User has cancelled the selection
                return;
            }

            Uri baseSourceDirectoryUri = new(baseSourceDirectory);

            foreach (FileStatusItem item in selectedFiles)
            {
                string selectedItemFullName = _fullPathResolver.Resolve(item.Item.Name);
                string selectedItemSourceDirectory = Path.GetDirectoryName(selectedItemFullName).EnsureTrailingPathSeparator();

                string targetDirectory;
                if (selectedItemSourceDirectory == baseSourceDirectory)
                {
                    targetDirectory = selectedPath;
                }
                else
                {
                    Uri selectedItemUri = new(selectedItemSourceDirectory);
                    targetDirectory = Path.Combine(selectedPath, baseSourceDirectoryUri.MakeRelativeUri(selectedItemUri).OriginalString);
                }

                // TODO: check target file exists.
                // TODO: allow cancel the whole sequence

                string targetFileName = Path.Combine(targetDirectory, Path.GetFileName(selectedItemFullName)).ToNativePath();
                Debug.WriteLine($"Saving {selectedItemFullName} --> {targetFileName}");

                GetModule().SaveBlobAs(targetFileName, $"{item.SecondRevision.Guid}:\"{item.Item.Name}\"");
            }
        }

        void SaveSingleFile(FileStatusItem item)
        {
            string fullName = _fullPathResolver.Resolve(item.Item.Name);
            string selectedFileName = userSelection(fullName);
            if (selectedFileName is null)
            {
                // User has cancelled the selection
                return;
            }

            GetModule().SaveBlobAs(selectedFileName, $"{item.SecondRevision.Guid}:\"{item.Item.Name}\"");
        }
    }

    // The enabling of menu items is related to how the actions have been implemented

    public bool ShouldShowDifftoolMenus(ContextMenuSelectionInfo selectionInfo)
    {
        return selectionInfo.SelectedGitItemCount > 0 && !selectionInfo.IsDisplayOnlyDiff;
    }

    public bool ShouldShowResetFileMenus(ContextMenuSelectionInfo selectionInfo)
    {
        return selectionInfo.SelectedGitItemCount > 0
            && !selectionInfo.IsBareRepository
            && selectionInfo.IsAnyTracked
            && !selectionInfo.IsDisplayOnlyDiff
            && !(selectionInfo.IsAnySubmodule && selectionInfo.SelectedGitItemCount == 1 && selectionInfo.IsAnyItemWorkTree);
    }

    #region Main menu items
    public bool ShouldShowMenuSaveAs(ContextMenuSelectionInfo selectionInfo)
    {
        return selectionInfo.SelectedGitItemCount > 0 && !selectionInfo.IsAnySubmodule
            && !(selectionInfo.SelectedRevision?.IsArtificial ?? false)
            && !selectionInfo.IsDisplayOnlyDiff;
    }

    public bool ShouldShowMenuCherryPick(ContextMenuSelectionInfo selectionInfo)
    {
        return selectionInfo.SupportPatches && !selectionInfo.IsAnyItemWorkTree;
    }

    // Stage/unstage must limit the selected items, IsStaged is not reflecting Staged status
    public bool ShouldShowMenuStage(ContextMenuSelectionInfo selectionInfo)
    {
        return selectionInfo.IsAnyItemWorkTree;
    }

    public bool ShouldShowMenuUnstage(ContextMenuSelectionInfo selectionInfo)
    {
        return selectionInfo.IsAnyItemIndex;
    }

    public bool ShouldShowSubmoduleMenus(ContextMenuSelectionInfo selectionInfo)
    {
        return selectionInfo.IsAnySubmodule && selectionInfo.SelectedRevision?.ObjectId == ObjectId.WorkTreeId && selectionInfo.AllDirectoriesExist;
    }

    public bool ShouldShowMenuEditWorkingDirectoryFile(ContextMenuSelectionInfo selectionInfo)
    {
        return selectionInfo.SelectedGitItemCount == 1 && selectionInfo.AllFilesExist;
    }

    public bool ShouldShowMenuDeleteFile(ContextMenuSelectionInfo selectionInfo)
    {
        return selectionInfo.AllFilesOrUntrackedDirectoriesExist && (selectionInfo.SelectedRevision?.IsArtificial ?? false);
    }

    public bool ShouldShowMenuMove(ContextMenuSelectionInfo selectionInfo)
    {
        return (selectionInfo.SelectedGitItemCount == 1 && selectionInfo.IsAnyTracked && !selectionInfo.IsAnySubmodule)
            || selectionInfo.SelectedFolder is not null;
    }

    public bool ShouldShowMenuOpenRevision(ContextMenuSelectionInfo selectionInfo)
    {
        return selectionInfo.SelectedGitItemCount == 1
            && !selectionInfo.IsAnySubmodule
            && !selectionInfo.IsDisplayOnlyDiff
            && !(selectionInfo.SelectedRevision?.IsArtificial ?? false);
    }

    public bool ShouldShowMenuCopyFileName(ContextMenuSelectionInfo selectionInfo)
    {
        return selectionInfo.SelectedGitItemCount > 0 && !selectionInfo.IsStatusOnly;
    }

    public bool ShouldShowMenuShowInFolder(ContextMenuSelectionInfo selectionInfo)
    {
        return selectionInfo.SelectedGitItemCount > 0 && !selectionInfo.IsStatusOnly;
    }

    public bool ShouldShowMenuShowInFileTree(ContextMenuSelectionInfo selectionInfo)
    {
        return (selectionInfo.SelectedGitItemCount == 1 && selectionInfo.IsAnyTracked && !selectionInfo.IsDeleted)
            || selectionInfo.SelectedFolder is not null;
    }

    public bool ShouldShowMenuFileHistory(ContextMenuSelectionInfo selectionInfo)
    {
        return (selectionInfo.SelectedGitItemCount == 1 || !string.IsNullOrEmpty(selectionInfo.SelectedFolder?.Value)) && selectionInfo.IsAnyTracked;
    }

    public bool ShouldShowMenuBlame(ContextMenuSelectionInfo selectionInfo)
    {
        return ShouldShowMenuFileHistory(selectionInfo) && !selectionInfo.IsAnySubmodule && selectionInfo.SelectedFolder is null;
    }
    #endregion

    private IGitModule GetModule()
    {
        return _getModule() ?? throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
    }
}
