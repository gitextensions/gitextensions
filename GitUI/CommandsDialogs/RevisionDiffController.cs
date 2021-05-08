using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs
{
    public interface IRevisionDiffController
    {
        bool ShouldShowMenuBlame(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuCherryPick(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuEditWorkingDirectoryFile(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuOpenRevision(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuDeleteFile(ContextMenuSelectionInfo selectionInfo);
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

    public sealed class ContextMenuSelectionInfo
    {
        // Defaults are set to simplify test cases, the defaults enables most
        public ContextMenuSelectionInfo(
            GitRevision? selectedRevision,
            bool isDisplayOnlyDiff,
            bool isStatusOnly,
            int selectedGitItemCount,
            bool isAnyItemIndex,
            bool isAnyItemWorkTree,
            bool isBareRepository,
            bool allFilesExist,
            bool allDirectoriesExist,
            bool allFilesOrUntrackedDirectoriesExist,
            bool isAnyTracked,
            bool supportPatches,
            bool isDeleted,
            bool isAnySubmodule)
        {
            SelectedRevision = selectedRevision;
            IsDisplayOnlyDiff = isDisplayOnlyDiff;
            IsStatusOnly = isStatusOnly;
            SelectedGitItemCount = selectedGitItemCount;
            IsAnyItemIndex = isAnyItemIndex;
            IsAnyItemWorkTree = isAnyItemWorkTree;
            IsBareRepository = isBareRepository;
            AllFilesExist = allFilesExist;
            AllDirectoriesExist = allDirectoriesExist;
            AllFilesOrUntrackedDirectoriesExist = allFilesOrUntrackedDirectoriesExist;
            IsAnyTracked = isAnyTracked;
            SupportPatches = supportPatches;
            IsDeleted = isDeleted;
            IsAnySubmodule = isAnySubmodule;
        }

        public GitRevision? SelectedRevision { get; }
        public bool IsDisplayOnlyDiff { get; }
        public bool IsStatusOnly { get; }
        public int SelectedGitItemCount { get; }
        public bool IsAnyItemIndex { get; }
        public bool IsAnyItemWorkTree { get; }
        public bool IsBareRepository { get; }
        public bool AllFilesExist { get; }
        public bool AllDirectoriesExist { get; }
        public bool AllFilesOrUntrackedDirectoriesExist { get; }
        public bool IsAnyTracked { get; }
        public bool SupportPatches { get; }
        public bool IsDeleted { get; }
        public bool IsAnySubmodule { get; }
    }

    public sealed class RevisionDiffController : IRevisionDiffController
    {
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
                && !selectionInfo.IsDisplayOnlyDiff;
        }

        #region Main menu items
        public bool ShouldShowMenuSaveAs(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.SelectedGitItemCount == 1 && !selectionInfo.IsAnySubmodule
                && !(selectionInfo.SelectedRevision?.IsArtificial ?? false)
                && !selectionInfo.IsDisplayOnlyDiff;
        }

        public bool ShouldShowMenuCherryPick(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.SelectedGitItemCount == 1
                && !selectionInfo.IsAnySubmodule
                && !selectionInfo.IsDisplayOnlyDiff
                && !selectionInfo.IsBareRepository
                && selectionInfo.AllFilesExist
                && selectionInfo.SupportPatches
                && !(selectionInfo.IsAnyItemWorkTree || selectionInfo.IsAnyItemIndex)
                && !(selectionInfo.SelectedRevision?.IsArtificial ?? false);
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
            return selectionInfo.SelectedGitItemCount == 1
                && !(selectionInfo.SelectedRevision?.IsArtificial ?? false)
                && !selectionInfo.IsDeleted
                && !selectionInfo.IsStatusOnly;
        }

        public bool ShouldShowMenuFileHistory(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.SelectedGitItemCount == 1 && selectionInfo.IsAnyTracked;
        }

        public bool ShouldShowMenuBlame(ContextMenuSelectionInfo selectionInfo)
        {
            return ShouldShowMenuFileHistory(selectionInfo) && !selectionInfo.IsAnySubmodule;
        }
        #endregion
    }
}
