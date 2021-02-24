using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs
{
    public interface IFileStatusListContextMenuController
    {
        bool ShouldShowMenuFirstToSelected(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuFirstToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuSelectedToLocal(ContextMenuDiffToolInfo selectionInfo);
    }

    public sealed class ContextMenuDiffToolInfo
    {
        public ContextMenuDiffToolInfo(
            GitRevision? selectedRevision = null,
            IReadOnlyList<ObjectId>? selectedItemParentRevs = null,
            bool allAreNew = false,
            bool allAreDeleted = false,
            bool firstIsParent = false,
            bool localExists = true)
        {
            SelectedRevision = selectedRevision;
            SelectedItemParentRevs = selectedItemParentRevs;
            AllAreNew = allAreNew;
            AllAreDeleted = allAreDeleted;
            FirstIsParent = firstIsParent;
            LocalExists = localExists;
        }

        public GitRevision? SelectedRevision { get; }
        public IEnumerable<ObjectId>? SelectedItemParentRevs { get; }
        public bool AllAreNew { get; }
        public bool AllAreDeleted { get; }
        public bool FirstIsParent { get; }
        public bool LocalExists { get; }
    }

    public class FileStatusListContextMenuController : IFileStatusListContextMenuController
    {
        public bool ShouldShowMenuFirstToSelected(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision is not null;
        }

        public bool ShouldShowMenuFirstToLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision is not null && selectionInfo.LocalExists

                // First (A) exists (Can only determine that A does not exist if A is parent and B is new)
                && (!selectionInfo.FirstIsParent || !selectionInfo.AllAreNew)

                // First (A) is not local
                && (selectionInfo.SelectedItemParentRevs is null || !selectionInfo.SelectedItemParentRevs.Contains(ObjectId.WorkTreeId));
        }

        public bool ShouldShowMenuSelectedToLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision is not null && selectionInfo.LocalExists

                // Selected (B) exists
                && !selectionInfo.AllAreDeleted

                // Selected (B) is not local
                && selectionInfo.SelectedRevision.ObjectId != ObjectId.WorkTreeId;
        }
    }
}
