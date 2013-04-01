using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    public static class MergeConflictHandler
    {
        public static bool HandleMergeConflicts(GitUICommands aCommands, IWin32Window owner, bool offerCommit)
        {
            if (aCommands.Module.InTheMiddleOfConflictedMerge())
            {
                if (MessageBoxes.UnresolvedMergeConflicts(owner))
                {
                    SolveMergeConflicts(aCommands, owner, offerCommit);
                }
                return true;
            }
            return false;
        }

        public static bool HandleMergeConflicts(GitUICommands aCommands, IWin32Window owner)
        {
            return HandleMergeConflicts(aCommands, owner, true);
        }

        private static void SolveMergeConflicts(GitUICommands aCommands, IWin32Window owner, bool offerCommit)
        {
            if (aCommands.Module.InTheMiddleOfConflictedMerge())
            {
                aCommands.StartResolveConflictsDialog(owner, offerCommit);
            }

            if (aCommands.Module.InTheMiddleOfPatch())
            {
                if (MessageBoxes.MiddleOfPatchApply(owner))
                {
                    aCommands.StartApplyPatchDialog(owner);
                }
            }
            else if (aCommands.Module.InTheMiddleOfRebase())
            {
                if (MessageBoxes.MiddleOfRebase(owner))
                {
                    aCommands.StartRebaseDialog(owner, null);
                }
            }

        }
    }
}
