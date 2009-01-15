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
                Ok.Enabled = true;
                AddFiles.Enabled = true;
                Resolved.Enabled = true;
                Mergetool.Enabled = true;
                Skip.Enabled = true;
                Abort.Enabled = true;
            }
            else
            {
                Branches.Enabled = true;
                Ok.Enabled = false;
                AddFiles.Enabled = false;
                Resolved.Enabled = false;
                Mergetool.Enabled = false;
                Skip.Enabled = false;
                Abort.Enabled = false;
            }
        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.cmd", "mergetool");

            if (MessageBox.Show("Resolved all conflicts? Continue rebase?", "Conflicts solved", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Output.Text += "\n";
                Output.Text += GitCommands.GitCommands.Resolved();
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
    }
}
