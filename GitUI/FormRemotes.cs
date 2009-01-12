using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

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
    }
}
