using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    public static class MergeConflictHandler
    {
        public static bool HandleMergeConflicts(GitUICommands aCommands, IWin32Window owner, bool offerCommit = true, bool offerUpdateSubmodules = true)
        {
            if (aCommands.Module.InTheMiddleOfConflictedMerge())
            {
                if (MessageBoxes.UnresolvedMergeConflicts(owner))
                {
                    SolveMergeConflicts(aCommands, owner, offerCommit);
                }
                return true;
            }

            if (offerUpdateSubmodules)
                aCommands.UpdateSubmodules(owner);
            return false;
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
