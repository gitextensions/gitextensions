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
                if (MessageBox.Show(owner, "There are unresolved mergeconflicts, solve conflicts now?", "Merge conflicts", MessageBoxButtons.YesNo) == DialogResult.Yes)
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
                if (MessageBox.Show(owner, "You are in the middle of a patch apply, continue patch apply?", "Patch apply", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitUICommands.Instance.StartApplyPatchDialog(owner);
                }
            }
            else if (Settings.Module.InTheMiddleOfRebase())
            {
                if (MessageBox.Show(owner, "You are in the middle of a rebase , continue rebase?", "Rebase", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitUICommands.Instance.StartRebaseDialog(owner, null);
                }
            }

        }
    }
}
