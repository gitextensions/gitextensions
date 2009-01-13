using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormPull : Form
    {
        public FormPull()
        {
            InitializeComponent();
        }

        private void BrowseSource_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                PullSource.Text = dialog.SelectedPath;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.cmd", "mergetool");

            if (MessageBox.Show("Resolved all conflicts? Commit?", "Conflicts solved", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //Output.Text += "\n";
                FormCommit form = new FormCommit();
                form.ShowDialog();
            }
        }

        private void PullSource_TextChanged(object sender, EventArgs e)
        {
            Branches.DataSource = null;
        }

        private void Branches_DropDown(object sender, EventArgs e)
        {
            if ((PullFromUrl.Checked && string.IsNullOrEmpty(PullSource.Text)) &&
                (PullFromRemote.Checked && string.IsNullOrEmpty(Remotes.Text)))
            {
                Branches.DataSource = null;
                return;
            }

            string realWorkingDir = GitCommands.Settings.WorkingDir;

            try
            {
                if (PullFromUrl.Checked)
                {
                    GitCommands.Settings.WorkingDir = PullSource.Text;
                }
                else
                {
                    GitCommands.Settings.WorkingDir = GitCommands.GitCommands.GetSetting("remote." + Remotes.Text + ".url");
                }
                Branches.DisplayMember = "Name";
                List<GitCommands.GitHead> heads = GitCommands.GitCommands.GetHeads(false);

                GitCommands.GitHead allHead = new GitCommands.GitHead();
                allHead.Name = "*";
                heads.Insert(0, allHead);
                GitCommands.GitHead noHead = new GitCommands.GitHead();
                noHead.Name = "";
                heads.Insert(0, noHead);
                Branches.DataSource = heads;
            }
            finally
            {
                GitCommands.Settings.WorkingDir = realWorkingDir;
            }
        }

        private void Pull_Click(object sender, EventArgs e)
        {
            if (PullFromUrl.Checked && string.IsNullOrEmpty(PullSource.Text))
            {
                MessageBox.Show("Please select a source directory");
                return;
            }
            if (PullFromRemote.Checked && string.IsNullOrEmpty(Remotes.Text))
            {
                MessageBox.Show("Please select a remote repository");
                return;
            }
            if (!Fetch.Checked && Branches.Text == "*")
            {
                MessageBox.Show("You can only fetch all remote branches (*) whithout merge or rebase.\nIf you want to fetch all remote branches, choose fetch.\nIf you want to fetch and merge a branch, choose a specific branch.");
                return;                
            }

            RepositoryHistory.AddMostRecentRepository(PullSource.Text);

            string source;

            if (PullFromUrl.Checked)
                source = PullSource.Text;
            else
                source = Remotes.Text;


            if (Fetch.Checked)
                /*Output.Text = */
                GitCommands.GitCommands.Fetch(source, Branches.Text);
            else if (Merge.Checked)
                /*Output.Text = */
                GitCommands.GitCommands.Pull(source, Branches.Text, false);
            else if (Rebase.Checked)
                /*Output.Text = */
                GitCommands.GitCommands.Pull(source, Branches.Text, true);

            if (GitCommands.GitCommands.InTheMiddleOfConflictedMerge())
            {
                if (MessageBox.Show("There where mergeconflicts, run mergetool now?", "Merge conflicts", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.cmd", "mergetool");
            }

        }

        private void FormPull_Load(object sender, EventArgs e)
        {
            string branch = GitCommands.GitCommands.GetSelectedBranch();
            Remotes.Text = GitCommands.GitCommands.GetSetting("branch." + branch + ".remote");

            this.Text = "Pull (" + GitCommands.Settings.WorkingDir + ")";
        }

        private void PullSource_DrawItem(object sender, DrawItemEventArgs e)
        {

        }

        private void PullSource_DropDown(object sender, EventArgs e)
        {
            PullSource.DataSource = RepositoryHistory.MostRecentRepositories;
        }

        private void Stash_Click(object sender, EventArgs e)
        {
            new FormStash().ShowDialog();
        }

        private void Remotes_DropDown(object sender, EventArgs e)
        {
            Remotes.DataSource = GitCommands.GitCommands.GetRemotes();
        }

        private void PullFromRemote_CheckedChanged(object sender, EventArgs e)
        {
            if (PullFromRemote.Checked)
            {
                PullSource.Enabled = false;
                BrowseSource.Enabled = false;
                Remotes.Enabled = true;
                AddRemote.Enabled = true;
            }
        }

        private void PullFromUrl_CheckedChanged(object sender, EventArgs e)
        {
            if (PullFromUrl.Checked)
            {
                PullSource.Enabled = true;
                BrowseSource.Enabled = true;
                Remotes.Enabled = false;
                AddRemote.Enabled = false;
            }
        }

        private void AddRemote_Click(object sender, EventArgs e)
        {
            new FormRemotes().ShowDialog();
        }

    }
}
