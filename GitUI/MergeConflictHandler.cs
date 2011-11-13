using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public static class MergeConflictHandler
    {
        public static bool HandleMergeConflicts(IWin32Window owner)
        {
            if (Settings.Module.InTheMiddleOfConflictedMerge())
            {
                if (MessageBox.Show(owner, "There are unresolved mergeconflicts, solve conflicts now?", "Merge conflicts", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SolveMergeConflicts();
                }
                return true;
            }
            return false;
        }

        public static bool HandleMergeConflicts()
        {
            return HandleMergeConflicts(null);
        }

        public static void SolveMergeConflicts(IWin32Window owner)
        {
            if (Settings.Module.InTheMiddleOfConflictedMerge())
            {
                GitUICommands.Instance.StartResolveConflictsDialog(owner);
            }

            if (Settings.Module.InTheMiddleOfPatch())
            {
                if (MessageBox.Show(owner, "You are in the middle of a patch apply, continue patch apply?", "Patch apply", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitUICommands.Instance.StartApplyPatchDialog(owner);
                }
            }
            else
                if (Settings.Module.InTheMiddleOfRebase())
                {
                    if (MessageBox.Show(owner, "You are in the middle of a rebase , continue rebase?", "Rebase", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        GitUICommands.Instance.StartRebaseDialog(owner, null);
                    }
                }

        }

        public static void SolveMergeConflicts()
        {
            SolveMergeConflicts(null);
        }

    }
}
