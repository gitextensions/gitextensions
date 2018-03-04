using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitCommands;

namespace GitUI.CommandsDialogs
{
    public interface IRevisionDiffController
    {
        bool ShouldShowMenuBlame(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuCherryPick(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuEditFile(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowResetFileMenus(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuFileHistory(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuSaveAs(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuCopyFileName(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuShowInFileTree(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuStage(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuUnstage(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowSubmoduleMenus(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowDifftoolMenus(ContextMenuSelectionInfo selectionInfo);

        bool ShouldShowMenuAB(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuALocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuBLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuAParentLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuBParentLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldDisplayMenuAParentLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldDisplayMenuBParentLocal(ContextMenuDiffToolInfo selectionInfo);

        bool LocalExists(IEnumerable<GitItemStatusWithParent> selectedItemsWithParent, IFullPathResolver fullPathResolver);
        bool AisParent(IEnumerable<string> selectedParentRevs, IEnumerable<string> selectedItemParentRevs);
    }

    public sealed class ContextMenuSelectionInfo
    {
        // Defaults are set to simplify test cases, the defaults enables most
        public ContextMenuSelectionInfo(
            GitRevision selectedRevision = null,
            bool firstIsParent = false,
            bool isAnyCombinedDiff = false,
            bool isSingleGitItemSelected = true,
            bool isAnyItemSelected = true,
            bool isBareRepository = false,
            bool singleFileExists = true,
            bool isAnyTracked = true,
            bool isAnySubmodule = false)
        {
            SelectedRevision = selectedRevision;
            FirstIsParent = firstIsParent;
            IsAnyCombinedDiff = isAnyCombinedDiff;
            IsSingleGitItemSelected = isSingleGitItemSelected;
            IsAnyItemSelected = isAnyItemSelected;
            IsBareRepository = isBareRepository;
            SingleFileExists = singleFileExists;
            IsAnyTracked = isAnyTracked;
            IsAnySubmodule = isAnySubmodule;
        }

        public GitRevision SelectedRevision { get; }
        public bool FirstIsParent { get; }
        public bool IsAnyCombinedDiff { get; }
        public bool IsSingleGitItemSelected { get; }
        public bool IsAnyItemSelected { get; }
        public bool IsBareRepository { get; }
        public bool SingleFileExists { get; }
        public bool IsAnyTracked { get; }
        public bool IsAnySubmodule { get; }
    }

    public sealed class ContextMenuDiffToolInfo
    {
        public ContextMenuDiffToolInfo(
            GitRevision selectedRevision = null,
            IEnumerable<string> selectedItemParentRevs = null,
            bool allAreNew = false,
            bool allAreDeleted = false,
            bool firstIsParent = false,
            bool firstParentsValid = true,
            bool localExists = true)
        {
            SelectedRevision = selectedRevision;
            SelectedItemParentRevs = selectedItemParentRevs;
            AllAreNew = allAreNew;
            AllAreDeleted = allAreDeleted;
            FirstIsParent = firstIsParent;
            FirstParentsValid = firstParentsValid;
            LocalExists = localExists;
        }

        public GitRevision SelectedRevision { get; }
        public IEnumerable<string> SelectedItemParentRevs { get; }
        public bool AllAreNew { get; }
        public bool AllAreDeleted { get; }
        public bool FirstIsParent { get; }
        public bool FirstParentsValid { get; }
        public bool LocalExists { get; }
    }

    public sealed class RevisionDiffController : IRevisionDiffController
    {
        // The enabling of menu items is related to how the actions have been implemented

        #region Menu dropdowns
        public bool ShouldShowDifftoolMenus(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnyItemSelected && !selectionInfo.IsAnyCombinedDiff && selectionInfo.IsAnyTracked;
        }

        public bool ShouldShowResetFileMenus(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnyItemSelected && !selectionInfo.IsBareRepository && selectionInfo.IsAnyTracked;
        }
        #endregion

        #region Main menu items
        public bool ShouldShowMenuSaveAs(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !selectionInfo.IsAnySubmodule
                && !selectionInfo.SelectedRevision.IsArtificial;
        }

        public bool ShouldShowMenuCherryPick(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !selectionInfo.IsAnySubmodule
                && !selectionInfo.IsAnyCombinedDiff && !selectionInfo.IsBareRepository
                && !selectionInfo.SelectedRevision.IsArtificial;
        }

        // Stage/unstage must limit the selected items, IsStaged is not reflecting Staged status
        public bool ShouldShowMenuStage(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.FirstIsParent && selectionInfo.SelectedRevision.Guid == GitRevision.UnstagedGuid;
        }

        public bool ShouldShowMenuUnstage(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.FirstIsParent && selectionInfo.SelectedRevision.Guid == GitRevision.IndexGuid;
        }

        public bool ShouldShowSubmoduleMenus(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnySubmodule && selectionInfo.SelectedRevision.Guid == GitRevision.UnstagedGuid;
        }

        public bool ShouldShowMenuEditFile(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !selectionInfo.IsAnySubmodule && selectionInfo.SingleFileExists;
        }

        public bool ShouldShowMenuCopyFileName(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnyItemSelected;
        }

        public bool ShouldShowMenuShowInFileTree(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !selectionInfo.SelectedRevision.IsArtificial;
        }

        public bool ShouldShowMenuFileHistory(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && selectionInfo.IsAnyTracked;
        }

        public bool ShouldShowMenuBlame(ContextMenuSelectionInfo selectionInfo)
        {
            return ShouldShowMenuFileHistory(selectionInfo) && !selectionInfo.IsAnySubmodule;
        }
        #endregion

        #region difftool submenu
        public bool ShouldShowMenuAB(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null;
        }

        public bool ShouldShowMenuALocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null && selectionInfo.LocalExists

                // A exists (Can only determine that A does not exist if A is parent and B is new)
                && (!selectionInfo.FirstIsParent || !selectionInfo.AllAreNew)

                // A is not local
                && (selectionInfo.SelectedItemParentRevs == null || !selectionInfo.SelectedItemParentRevs.Contains(GitRevision.UnstagedGuid));
        }

        public bool ShouldShowMenuBLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null && selectionInfo.LocalExists

                // B exists
                && !selectionInfo.AllAreDeleted

                // B is not local
                && selectionInfo.SelectedRevision.Guid != GitRevision.UnstagedGuid;
        }

        public bool ShouldShowMenuAParentLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null && selectionInfo.LocalExists
                && ShouldDisplayMenuAParentLocal(selectionInfo);
        }

        public bool ShouldShowMenuBParentLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null && selectionInfo.LocalExists
                && ShouldDisplayMenuBParentLocal(selectionInfo)

                // B parent exists
                && !selectionInfo.AllAreNew;
        }

        public bool ShouldDisplayMenuAParentLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            // A parents may not be known, then hide this option
            return selectionInfo.FirstParentsValid;
        }

        public bool ShouldDisplayMenuBParentLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            // Do not be visable if same as ShouldShowMenuALocal()
            return !selectionInfo.FirstIsParent;
        }
        #endregion

        #region helpers
        public bool LocalExists(IEnumerable<GitItemStatusWithParent> selectedItemsWithParent, IFullPathResolver fullPathResolver)
        {
            bool localExists = selectedItemsWithParent.Any(item => !item.Item.IsTracked);
            if (!localExists)
            {
                // enable *<->Local items only when (any) local file exists
                foreach (var item in selectedItemsWithParent)
                {
                    string filePath = fullPathResolver.Resolve(item.Item.Name);
                    if (File.Exists(filePath))
                    {
                        localExists = true;
                        break;
                    }
                }
            }

            return localExists;
        }

        public bool AisParent(IEnumerable<string> selectedParentRevs, IEnumerable<string> selectedItemParentRevs)
        {
            if (selectedParentRevs == null)
            {
                return false;
            }

            foreach (var item in selectedItemParentRevs)
            {
                if (!selectedParentRevs.Contains(item))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}