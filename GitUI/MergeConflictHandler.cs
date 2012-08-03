using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public static class MergeConflictHandler
    {
        public static bool HandleMergeConflicts(IWin32Window owner, bool offerCommit)
        {
            if (Settings.Module.InTheMiddleOfConflictedMerge())
            {
                if (MessageBoxes.UnresolvedMergeConflicts(owner))
                {
                    SolveMergeConflicts(owner, offerCommit);
                }
                return true;
            }
            return false;
        }

        public static bool HandleMergeConflicts(IWin32Window owner)
        {
            return HandleMergeConflicts(owner, true);
        }

        private static void SolveMergeConflicts(IWin32Window owner, bool offerCommit)
        {
            if (Settings.Module.InTheMiddleOfConflictedMerge())
            {
                GitUICommands.Instance.StartResolveConflictsDialog(owner, offerCommit);
            }

            if (Settings.Module.InTheMiddleOfPatch())
            {
                if (MessageBoxes.MiddleOfPatchApply(owner))
                {
                    GitUICommands.Instance.StartApplyPatchDialog(owner);
                }
            }
            else if (Settings.Module.InTheMiddleOfRebase())
            {
                if (MessageBoxes.MiddleOfRebase(owner))
                {
                    GitUICommands.Instance.StartRebaseDialog(owner, null);
                }
            }

        }
    }
}
