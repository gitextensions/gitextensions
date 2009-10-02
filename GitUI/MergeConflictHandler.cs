using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GitUI
{
    public class MergeConflictHandler
    {
        public static bool HandleMergeConflicts()
        {
            if (GitCommands.GitCommands.InTheMiddleOfConflictedMerge())
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
            if (GitCommands.GitCommands.InTheMiddleOfConflictedMerge())
            {
                GitUICommands.Instance.StartResolveConflictsDialog();
            }

            if (GitCommands.GitCommands.InTheMiddleOfPatch())
            {
                if (MessageBox.Show("You are in the middle of a patch apply, continue patch apply?", "Patch apply", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitUICommands.Instance.StartApplyPatchDialog();
                }
            }
            else
            if (GitCommands.GitCommands.InTheMiddleOfRebase())
            {
                if (MessageBox.Show("You are in the middle of a rebase , continue rebase?", "Rebase", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitUICommands.Instance.StartRebaseDialog();
                }
            }
      
        }

    }
}
