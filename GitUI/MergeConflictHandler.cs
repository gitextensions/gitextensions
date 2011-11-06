using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public static class MergeConflictHandler
    {
        public static bool HandleMergeConflicts()
        {
            if (Settings.Module.InTheMiddleOfConflictedMerge())
            {
                if (MessageBox.Show("There are unresolved mergeconflicts, solve conflicts now?", "Merge conflicts", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SolveMergeConflicts();
                }
                return true;
            }
            return false;
        }

        public static void SolveMergeConflicts()
        {
            if (Settings.Module.InTheMiddleOfConflictedMerge())
            {
                GitUICommands.Instance.StartResolveConflictsDialog();
            }

            if (Settings.Module.InTheMiddleOfPatch())
            {
                if (MessageBox.Show("You are in the middle of a patch apply, continue patch apply?", "Patch apply", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitUICommands.Instance.StartApplyPatchDialog();
                }
            }
            else
                if (Settings.Module.InTheMiddleOfRebase())
                {
                    if (MessageBox.Show("You are in the middle of a rebase , continue rebase?", "Rebase", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        GitUICommands.Instance.StartRebaseDialog(null);
                    }
                }

        }

    }
}
