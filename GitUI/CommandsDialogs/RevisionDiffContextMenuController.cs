using System.Linq;
using GitCommands;

namespace GitUI.CommandsDialogs
{
    public interface IRevisionDiffContextMenuController
    {
        bool ShouldShowMenuFirstToSelected(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuFirstToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuSelectedToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuFirstParentToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuSelectedParentToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldDisplayMenuFirstParentToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldDisplayMenuSelectedParentToLocal(ContextMenuDiffToolInfo selectionInfo);
    }

    public class RevisionDiffContextMenuController : IRevisionDiffContextMenuController
    {
        public bool ShouldShowMenuFirstToSelected(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null;
        }

        public bool ShouldShowMenuFirstToLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null && selectionInfo.LocalExists

                // First (A) exists (Can only determine that A does not exist if A is parent and B is new)
                && (!selectionInfo.FirstIsParent || !selectionInfo.AllAreNew)

                // First (A) is not local
                && (selectionInfo.SelectedItemParentRevs == null || !selectionInfo.SelectedItemParentRevs.Contains(GitRevision.UnstagedGuid));
        }

        public bool ShouldShowMenuSelectedToLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null && selectionInfo.LocalExists

                // Selected (B) exists
                && !selectionInfo.AllAreDeleted

                // Selected (B) is not local
                && selectionInfo.SelectedRevision.Guid != GitRevision.UnstagedGuid;
        }

        public bool ShouldShowMenuFirstParentToLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null && selectionInfo.LocalExists
                && ShouldDisplayMenuFirstParentToLocal(selectionInfo);
        }

        public bool ShouldShowMenuSelectedParentToLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null && selectionInfo.LocalExists
                && ShouldDisplayMenuSelectedParentToLocal(selectionInfo)

                // Selected (B) parent exists
                && !selectionInfo.AllAreNew;
        }

        public bool ShouldDisplayMenuFirstParentToLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            // First (A) parents may not be known, then hide this option
            return selectionInfo.FirstParentsValid;
        }

        public bool ShouldDisplayMenuSelectedParentToLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            // Do not be visable if same as ShouldShowMenuALocal()
            return !selectionInfo.FirstIsParent;
        }
    }
}