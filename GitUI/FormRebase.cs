using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GitUI
{
    public partial class FormRebase : Form
    {
        public FormRebase()
        {
            InitializeComponent();
        }

        private void FormRebase_Load(object sender, EventArgs e)
        {
            string selectedHead = GitCommands.GitCommands.GetSelectedBranch();
            Currentbranch.Text = "Current branch: " + selectedHead;

            Branches.DisplayMember = "Name";
            Branches.DataSource = GitCommands.GitCommands.GetHeads(false, true);

            EnableButtons();
        }

        private void EnableButtons()
        {
            if (Directory.Exists(GitCommands.Settings.WorkingDir + ".git\\rebase-apply\\"))
            {
                Branches.Enabled = false;
                Ok.Enabled = false;
                AddFiles.Enabled = true;
                Resolved.Enabled = true;
                Mergetool.Enabled = true;
                Skip.Enabled = true;
                Abort.Enabled = true;
            }
            else
            {
                Branches.Enabled = true;
                Ok.Enabled = true;
                AddFiles.Enabled = false;
                Resolved.Enabled = false;
                Mergetool.Enabled = false;
                Skip.Enabled = false;
                Abort.Enabled = false;
            }

            if (GitCommands.GitCommands.InTheMiddleOfConflictedMerge())
            {
                if (MessageBox.Show("There are mergeconflicts and a rebase is progress, solve conflicts?", "Solve conflics", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Mergetool_Click(null, null);
                }
            }
            else
                if (Directory.Exists(GitCommands.Settings.WorkingDir + ".git\\rebase-apply\\"))
                {
                    if (MessageBox.Show("There are no mergeconflicts and a rebase is progress, continue rebase?\n\nIf you get this dialog a few times, choose no and read output.", "Continue rebase", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Resolved_Click(null, null);
                    }
                }
        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.cmd", "mergetool");

            if (!GitCommands.GitCommands.InTheMiddleOfConflictedMerge())
            {
                if (MessageBox.Show("You have resolved all conflicts! Continue rebase?", "Conflicts solved", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Output.Text += "\n";
                    Output.Text += GitCommands.GitCommands.ContinueRebase();
                    EnableButtons();
                }
            }
            else
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("Not all mergeconflicts are solved, please solve the following files manually:\n");

                foreach(GitCommands.GitItem file in GitCommands.GitCommands.GetConflictedFiles())
                {
                    msg.Append(file.FileName);
                    msg.Append("\n");
                }

                MessageBox.Show(msg.ToString(), "Unsolved conflicts", MessageBoxButtons.OK);
                new FormResolveConflicts().ShowDialog();
                EnableButtons();
            }
        }

        private void AddFiles_Click(object sender, EventArgs e)
        {
            FormAddFiles form = new FormAddFiles();
            form.ShowDialog();
        }

        private void Resolved_Click(object sender, EventArgs e)
        {
            Output.Text += "\n";
            Output.Text += GitCommands.GitCommands.ContinueRebase();
            EnableButtons();
        }

        private void Skip_Click(object sender, EventArgs e)
        {
            Output.Text += "\n";
            Output.Text += GitCommands.GitCommands.SkipRebase();
            EnableButtons();
        }

        private void Abort_Click(object sender, EventArgs e)
        {
            Output.Text += "\n";
            Output.Text += GitCommands.GitCommands.AbortRebase();
            EnableButtons();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Branches.Text))
            {
                MessageBox.Show("Please select a branch");
                return;
            }

            string result = GitCommands.GitCommands.Rebase(Branches.Text);
            Output.Text = result;
            if (result.Contains("Rebase failed"))
            {
            }
            EnableButtons();


        }
    }
}
