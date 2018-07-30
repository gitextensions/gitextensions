using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    public interface IFileStatusListContextMenuController
    {
        bool ShouldShowMenuFirstToSelected(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuFirstToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuSelectedToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuFirstParentToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuSelectedParentToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldDisplayMenuFirstParentToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldDisplayMenuSelectedParentToLocal(ContextMenuDiffToolInfo selectionInfo);
    }

    public sealed class ContextMenuDiffToolInfo
    {
        public ContextMenuDiffToolInfo(
            GitRevision selectedRevision = null,
            IReadOnlyList<ObjectId> selectedItemParentRevs = null,
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

        [CanBeNull]
        public GitRevision SelectedRevision { get; }
        [CanBeNull]
        public IEnumerable<ObjectId> SelectedItemParentRevs { get; }
        public bool AllAreNew { get; }
        public bool AllAreDeleted { get; }
        public bool FirstIsParent { get; }
        public bool FirstParentsValid { get; }
        public bool LocalExists { get; }
    }

    public class FileStatusListContextMenuController : IFileStatusListContextMenuController
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
                && (selectionInfo.SelectedItemParentRevs == null || !selectionInfo.SelectedItemParentRevs.Contains(ObjectId.WorkTreeId));
        }

        public bool ShouldShowMenuSelectedToLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null && selectionInfo.LocalExists

                // Selected (B) exists
                && !selectionInfo.AllAreDeleted

                // Selected (B) is not local
                && selectionInfo.SelectedRevision.Guid != GitRevision.WorkTreeGuid;
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
            // Not visible if same revision as ShouldShowMenuFirstToLocal()
            return !selectionInfo.FirstIsParent;
        }
    }
}