using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace GitUI
{
    public partial class FormPush : Form
    {
        public FormPush()
        {
            InitializeComponent();
        }

        private void BrowseSource_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
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

            RepositoryHistory.AddMostRecentRepository(PushDestination.Text);

            FormProcess form;

            if (PullFromUrl.Checked)
                form = new FormProcess(GitCommands.GitCommands.PushCmd(PushDestination.Text, Branch.Text, PushAllBranches.Checked));
            else
            {
                GitCommands.GitCommands.StartPageantForRemote(Remotes.Text);
                form = new FormProcess(GitCommands.GitCommands.PushCmd(Remotes.Text, Branch.Text, PushAllBranches.Checked));
            }
        }

        private void PushDestination_DropDown(object sender, EventArgs e)
        {
            PushDestination.DataSource = RepositoryHistory.MostRecentRepositories;
        }

        private void Branch_DropDown(object sender, EventArgs e)
        {
            Branch.DisplayMember = "Name";
            Branch.DataSource = GitCommands.GitCommands.GetHeads(false);
        }

        private void Pull_Click(object sender, EventArgs e)
        {
            new FormPull().ShowDialog();
        }

        private void FormPush_Load(object sender, EventArgs e)
        {
            string branch = GitCommands.GitCommands.GetSelectedBranch();
            Remotes.Text = GitCommands.GitCommands.GetSetting("branch." + branch + ".remote");
            EnableLoadSSHButton();

            this.Text = "Push (" + GitCommands.Settings.WorkingDir + ")";
        }

        private void AddRemote_Click(object sender, EventArgs e)
        {
            new FormRemotes().ShowDialog();
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
                LoadSSHKey.Enabled = true;
            }
            else
            {
                LoadSSHKey.Enabled = false;
            }
        }

        private void LoadSSHKey_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands.StartPageantForRemote(Remotes.Text);
        }

        private void Remotes_Validated(object sender, EventArgs e)
        {
            EnableLoadSSHButton();
        }
    }
}
