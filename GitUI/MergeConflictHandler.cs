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
            GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.cmd", "mergetool");


            if (GitCommands.GitCommands.InTheMiddleOfConflictedMerge())
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("Not all mergeconflicts are solved, please solve the following files manually:\n");

                foreach (GitCommands.GitItem file in GitCommands.GitCommands.GetConflictedFiles())
                {
                    msg.Append(file.FileName);
                    msg.Append("\n");
                }

                MessageBox.Show(msg.ToString(), "Unsolved conflicts", MessageBoxButtons.OK);
                new FormResolveConflicts().ShowDialog();
            }

            if (GitCommands.GitCommands.InTheMiddleOfConflictedMerge())
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("Not all mergeconflicts are solved, please solve the following files manually:\n");

                foreach (GitCommands.GitItem file in GitCommands.GitCommands.GetConflictedFiles())
                {
                    msg.Append(file.FileName);
                    msg.Append("\n");
                }

                MessageBox.Show(msg.ToString(), "Unsolved conflicts", MessageBoxButtons.OK);
                new FormResolveConflicts().ShowDialog();
            }

            if (GitCommands.GitCommands.InTheMiddleOfRebase())
            {
                if (MessageBox.Show("You are in the middle of a rebase (or patch apply), continue rebase?", "Rebase", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    new FormRebase().ShowDialog();
                }
            }
            else
                if (MessageBox.Show("When all mergeconflicts are resolved, you can commit.\nDo you want to commit now?", "Commit", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    FormCommit frm = new FormCommit();
                    frm.ShowDialog();
                }
        }

    }
}
