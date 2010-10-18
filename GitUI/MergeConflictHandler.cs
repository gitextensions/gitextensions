using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public class MergeConflictHandler
    {
        public static bool HandleMergeConflicts()
        {
            if (GitCommandHelpers.InTheMiddleOfConflictedMerge())
            {
                if (MessageBox.Show("There are unresolved mergeconflicts, solve conflicts now?", "Merge conflicts", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SolveMergeConflicts();
                    return true;

                }
            }
            return false;
        }

        public static void SolveMergeConflicts()
        {
            if (GitCommandHelpers.InTheMiddleOfConflictedMerge())
            {
                GitUICommands.Instance.StartResolveConflictsDialog();
            }

            if (GitCommandHelpers.InTheMiddleOfPatch())
            {
                if (MessageBox.Show("You are in the middle of a patch apply, continue patch apply?", "Patch apply", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitUICommands.Instance.StartApplyPatchDialog();
                }
            }
            else
                if (GitCommandHelpers.InTheMiddleOfRebase())
                {
                    if (MessageBox.Show("You are in the middle of a rebase , continue rebase?", "Rebase", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        GitUICommands.Instance.StartRebaseDialog(null);
                    }
                }

        }

    }
}
