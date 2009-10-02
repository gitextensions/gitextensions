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
            InitializeComponent();
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

            GitCommands.RepositoryHistory.AddMostRecentRepository(PushDestination.Text);

            FormProcess form;

            if (PullFromUrl.Checked)
            {
                if (TabControlTagBranch.SelectedTab == BranchTab)
                    form = new FormProcess(GitCommands.GitCommands.PushCmd(PushDestination.Text, Branch.Text, PushAllBranches.Checked));
                else
                    form = new FormProcess(GitCommands.GitCommands.PushTagCmd(PushDestination.Text, Tag.Text, PushAllTags.Checked));
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
                    form = new FormProcess(GitCommands.Settings.GitDir + "git.cmd", GitCommands.GitCommands.PushCmd(Remotes.Text, Branch.Text, PushAllBranches.Checked), Remotes.Text.Trim());
                else
                    form = new FormProcess(GitCommands.Settings.GitDir + "git.cmd", GitCommands.GitCommands.PushTagCmd(Remotes.Text, Tag.Text, PushAllTags.Checked), Remotes.Text.Trim());
            }

            if (!GitCommands.GitCommands.InTheMiddleOfConflictedMerge() && !GitCommands.GitCommands.InTheMiddleOfRebase() && !form.ErrorOccured())
                Close();
        }

        private void PushDestination_DropDown(object sender, EventArgs e)
        {
            PushDestination.DataSource = GitCommands.RepositoryHistory.MostRecentRepositories;
        }

        private void Branch_DropDown(object sender, EventArgs e)
        {
            Branch.DisplayMember = "Name";
            Branch.DataSource = GitCommands.GitCommands.GetHeads(false, true);
        }

        private void Pull_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartPullDialog();
        }

        private void FormPush_Load(object sender, EventArgs e)
        {
            string branch = GitCommands.GitCommands.GetSelectedBranch();
            Remotes.Text = GitCommands.GitCommands.GetSetting("branch." + branch + ".remote");

            Branch.Text = branch;

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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Tag_DropDown(object sender, EventArgs e)
        {
            Tag.DisplayMember = "Name";
            Tag.DataSource = GitCommands.GitCommands.GetHeads(true, false);
        }
    }
}
