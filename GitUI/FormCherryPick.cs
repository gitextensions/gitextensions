using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormCherryPick : Form
    {
        public FormCherryPick()
        {
            InitializeComponent();
        }

        private void CherryPick_Click(object sender, EventArgs e)
        {
            if (RevisionGrid.GetRevisions().Count != 1)
            {
                MessageBox.Show("Select 1 revision to pick.", "Cherry pick");
                return;
            }



            MessageBox.Show("Command executed \n" + GitCommands.GitCommands.CherryPick(RevisionGrid.GetRevisions()[0].Guid, AutoCommit.Checked), "Cherry pick");

            if (GitCommands.GitCommands.InTheMiddleOfConflictedMerge())
            {
                if (MessageBox.Show("There are unresolved mergeconflicts, run mergetool now?", "Merge conflicts", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.cmd", "mergetool");
                    if (MessageBox.Show("When all mergeconflicts are resolved, you can commit.\nDo you want to commit now?", "Commit", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        new FormCommit().ShowDialog();
                    }
                }
            }

            RevisionGrid.RefreshRevisions();
        }
    }
}
