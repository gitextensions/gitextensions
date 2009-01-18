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
    public partial class FormRemotes : Form
    {
        public FormRemotes()
        {
            InitializeComponent();
        }

        private void FormRemotes_Load(object sender, EventArgs e)
        {
            Initialize();
            List<GitCommands.GitHead> heads = GitCommands.GitCommands.GetHeads(false, true);
            RemoteBranches.DataSource = heads;
            foreach (object item in GitCommands.GitCommands.GetRemotes())
            {
                RemoteCombo.Items.Add(item);
            }
            mergeWithDataGridViewTextBoxColumn.Items.Add("");
            foreach (GitCommands.GitHead head in heads)
            {
                mergeWithDataGridViewTextBoxColumn.Items.Add(head.Name);
            }
            RemoteBranches.DataError += new DataGridViewDataErrorEventHandler(RemoteBranches_DataError);
        }

        void RemoteBranches_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

            MessageBox.Show(string.Format("Invalid ´{1}´ found for branch ´{0}´.\nValue has been reset to empty value.", RemoteBranches.Rows[e.RowIndex].Cells[0].Value, RemoteBranches.Columns[e.ColumnIndex].HeaderText));

            RemoteBranches.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
        }

        private void Initialize()
        {
            Remotes.DataSource = GitCommands.GitCommands.GetRemotes();
        }
        
        string remote = "";

        private void Remotes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Remotes.SelectedItem is string)
            {
                remote = (string)Remotes.SelectedItem;
                RemoteName.Text = remote;
                Url.Text = GitCommands.GitCommands.GetSetting("remote." + remote + ".url");
                PuttySshKey.Text = GitCommands.GitCommands.GetSetting("remote." + RemoteName.Text + ".puttykeyfile");
            }
        }

        private void Url_DropDown(object sender, EventArgs e)
        {
            Url.DataSource = RepositoryHistory.MostRecentRepositories;
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                Url.Text = dialog.SelectedPath;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            string output = "";

            if (string.IsNullOrEmpty(remote))
            {
                output = GitCommands.GitCommands.AddRemote(RemoteName.Text, Url.Text);
            }
            else
            {

                if (RemoteName.Text != remote)
                {
                    output = GitCommands.GitCommands.RenameRemote(remote, RemoteName.Text);
                }

                GitCommands.GitCommands.SetSetting("remote." + RemoteName.Text + ".url", Url.Text);
                GitCommands.GitCommands.SetSetting("remote." + RemoteName.Text + ".puttykeyfile", PuttySshKey.Text);
            }
            if (!string.IsNullOrEmpty(output))
                MessageBox.Show(output, "Delete");

            Initialize();
        }

        private void New_Click(object sender, EventArgs e)
        {
            string output = GitCommands.GitCommands.AddRemote("<new>", "");
            if (!string.IsNullOrEmpty(output))
                MessageBox.Show(output, "Delete");
            Initialize();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(remote))
                return;

            if (MessageBox.Show("Are you sure you want to delete this remote?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string output = GitCommands.GitCommands.RemoveRemote(remote);
                if (!string.IsNullOrEmpty(output))
                    MessageBox.Show(output, "Delete");
            }

            Initialize();
        }

        private void UpdateBranch_Click(object sender, EventArgs e)
        {
            string output = GitCommands.GitCommands.UpdateRemotes();
            MessageBox.Show(output, "Update remote info");
        }

        private void SshBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Private key (*.ppk)|*.ppk";
            dialog.InitialDirectory = ".";
            dialog.Title = "Select ssh key file";
            if (dialog.ShowDialog() == DialogResult.OK)
                PuttySshKey.Text = dialog.FileName;
        }
    }
}
