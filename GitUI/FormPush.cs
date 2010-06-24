using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace GitUI
{
    public partial class FormPush : GitExtensionsForm
    {
        public FormPush()
        {
            InitializeComponent(); Translate();
        }

        private void BrowseSource_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = PushDestination.Text;
            if (dialog.ShowDialog() == DialogResult.OK)
                PushDestination.Text = dialog.SelectedPath;
            
        }

        private void Push_Click(object sender, EventArgs e)
        {
            if (PullFromUrl.Checked && string.IsNullOrEmpty(PushDestination.Text))
            {
                MessageBox.Show("Please select a destination directory");
                return;
            }
            if (PullFromRemote.Checked && string.IsNullOrEmpty(Remotes.Text))
            {
                MessageBox.Show("Please select a remote repository");
                return;
            }
            if (TabControlTagBranch.SelectedTab == TagTab && string.IsNullOrEmpty(Tag.Text) && !PushAllTags.Checked)
            {
                MessageBox.Show("You need to select a tag to push or select \"Push all tags\".");
                return;
            }

            GitCommands.Repositories.RepositoryHistory.AddMostRecentRepository(PushDestination.Text);

            FormProcess form;

            if (PullFromUrl.Checked)
            {
                if (TabControlTagBranch.SelectedTab == BranchTab)
                    form = new FormProcess(GitCommands.GitCommands.PushCmd(PushDestination.Text, Branch.Text, RemoteBranch.Text, PushAllBranches.Checked, ForcePushBranches.Checked));
                else
                    form = new FormProcess(GitCommands.GitCommands.PushTagCmd(PushDestination.Text, Tag.Text, PushAllTags.Checked, ForcePushBranches.Checked));
            }
            else
            {
                if (GitCommands.GitCommands.Plink())
                {
                    if (!File.Exists(GitCommands.Settings.Pageant))
                        MessageBox.Show("Cannot load SSH key. PuTTY is not configured properly.", "PuTTY");
                    else
                        GitCommands.GitCommands.StartPageantForRemote(Remotes.Text);
                }

                if (TabControlTagBranch.SelectedTab == BranchTab)
                    form = new FormProcess(GitCommands.Settings.GitCommand, GitCommands.GitCommands.PushCmd(Remotes.Text, Branch.Text, RemoteBranch.Text, PushAllBranches.Checked, ForcePushBranches.Checked), Remotes.Text.Trim());
                else
                    form = new FormProcess(GitCommands.Settings.GitCommand, GitCommands.GitCommands.PushTagCmd(Remotes.Text, Tag.Text, PushAllTags.Checked, ForcePushBranches.Checked), Remotes.Text.Trim());
            }

            if (!GitCommands.GitCommands.InTheMiddleOfConflictedMerge() && !GitCommands.GitCommands.InTheMiddleOfRebase() && !form.ErrorOccured())
                Close();
        }

        private void PushDestination_DropDown(object sender, EventArgs e)
        {
            PushDestination.DataSource = GitCommands.Repositories.RepositoryHistory.Repositories;
            PushDestination.DisplayMember = "Path";
        }

        private void Branch_DropDown(object sender, EventArgs e)
        {
            string curBranch = Branch.Text;

            Branch.DisplayMember = "Name";
            Branch.DataSource = GitCommands.GitCommands.GetHeads(false, true);

            if (string.IsNullOrEmpty(curBranch))
            {
                curBranch = GitCommands.GitCommands.GetSelectedBranch();
            }
            Branch.Text = curBranch;
        }

        private void Pull_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartPullDialog();
        }

        private void RemoteBranch_DropDown(object sender, EventArgs e)
        {
            RemoteBranch.DisplayMember = "Name";
            RemoteBranch.Items.Clear();

            if (!string.IsNullOrEmpty(Branch.Text))
            {
                RemoteBranch.Items.Add(Branch.Text);
            }

            List<string> heads = GitCommands.GitCommands.GetBranches(true, Remotes.Text);
            foreach (string h in heads)
            {
                if (!RemoteBranch.Items.Contains(h))
                {
                    RemoteBranch.Items.Add(h);
                }
            }

        }

        private void Branch_SelectedValueChanged(object sender, EventArgs e)
        {
            RemoteBranch.Text = Branch.Text;
        }

        private void FormPush_Load(object sender, EventArgs e)
        {
            string branch = GitCommands.GitCommands.GetSelectedBranch();
            Remotes.Text = GitCommands.GitCommands.GetSetting("branch." + branch + ".remote");

            // Doing this makes it pretty easy to accidentally create a branch on the remote.
            // leaving it blank will do the 'default' thing, and can be configured by setting 
            // the push option of the remote.
            //Branch.Text = branch;
            //RemoteBranch.Text = Branch.Text;

            EnableLoadSSHButton();

            this.Text = "Push (" + GitCommands.Settings.WorkingDir + ")";
        }

        private void AddRemote_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartRemotesDialog();
        }

        private void Remotes_DropDown(object sender, EventArgs e)
        {
            Remotes.DataSource = GitCommands.GitCommands.GetRemotes();
        }

        private void PullFromRemote_CheckedChanged(object sender, EventArgs e)
        {
            if (PullFromRemote.Checked)
            {
                PushDestination.Enabled = false;
                BrowseSource.Enabled = false;
                Remotes.Enabled = true;
                AddRemote.Enabled = true;
            }
        }

        private void PullFromUrl_CheckedChanged(object sender, EventArgs e)
        {
            if (PullFromUrl.Checked)
            {
                PushDestination.Enabled = true;
                BrowseSource.Enabled = true;
                Remotes.Enabled = false;
                AddRemote.Enabled = false;
            }
        }

        private void Remotes_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableLoadSSHButton();
        }

        private void EnableLoadSSHButton()
        {
            if (!string.IsNullOrEmpty(GitCommands.GitCommands.GetPuttyKeyFileForRemote(Remotes.Text)))
            {
                LoadSSHKey.Visible = true;
            }
            else
            {
                LoadSSHKey.Visible = false;
            }
        }

        private void LoadSSHKey_Click(object sender, EventArgs e)
        {
            if (!File.Exists(GitCommands.Settings.Pageant))
                MessageBox.Show("Cannot load SSH key. PuTTY is not configured properly.", "PuTTY");
            else
                GitCommands.GitCommands.StartPageantForRemote(Remotes.Text);
        }

        private void Remotes_Validated(object sender, EventArgs e)
        {
            EnableLoadSSHButton();
        }

        private void Tag_DropDown(object sender, EventArgs e)
        {
            Tag.DisplayMember = "Name";
            Tag.DataSource = GitCommands.GitCommands.GetHeads(true, false);
        }

        void ForcePushBranches_CheckedChanged(object sender, System.EventArgs e) {
            this.ForcePushTags.Checked = this.ForcePushBranches.Checked;
        }

        void ForcePushTags_CheckedChanged(object sender, System.EventArgs e) {
            this.ForcePushBranches.Checked = this.ForcePushTags.Checked;
        }

        private void PushAllBranches_CheckedChanged(object sender, EventArgs e)
        {
            Branch.Enabled = !PushAllBranches.Checked;
            RemoteBranch.Enabled = !PushAllBranches.Checked;
        }

    }
}
