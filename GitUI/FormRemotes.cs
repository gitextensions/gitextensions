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
    public partial class FormRemotes : GitExtensionsForm
    {
        public FormRemotes()
        {
            InitializeComponent();
        }

        private bool IsDirty = false;

        private void FormRemotes_Load(object sender, EventArgs e)
        {
            Initialize();

        }

        void RemoteBranches_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

            MessageBox.Show(string.Format("Invalid ´{1}´ found for branch ´{0}´." + Environment.NewLine + "Value has been reset to empty value.", RemoteBranches.Rows[e.RowIndex].Cells[0].Value, RemoteBranches.Columns[e.ColumnIndex].HeaderText));

            RemoteBranches.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
        }

        private void Initialize()
        {
            Remotes.DataSource = GitCommands.GitCommands.GetRemotes();

            List<GitCommands.GitHead> heads = GitCommands.GitCommands.GetHeads(false, true);
            RemoteBranches.DataSource = heads;
/*            foreach (object item in GitCommands.GitCommands.GetRemotes())
            {
                RemoteCombo.Items.Add(item);
            }
            mergeWithDataGridViewTextBoxColumn.Items.Add("");
            /*foreach (GitCommands.GitHead head in heads)
            {
                mergeWithDataGridViewTextBoxColumn.Items.Add(head.Name);
            }*/
            RemoteBranches.DataError += new DataGridViewDataErrorEventHandler(RemoteBranches_DataError);

            if (GitCommands.GitCommands.Plink())
                PuTTYSSH.Visible = true;
            else
                PuTTYSSH.Visible = false;
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
            Url.DataSource = GitCommands.RepositoryHistory.MostRecentRepositories;
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
                if (string.IsNullOrEmpty(RemoteName.Text) && string.IsNullOrEmpty(Url.Text))
                {
                    return;
                }

                output = GitCommands.GitCommands.AddRemote(RemoteName.Text, Url.Text);

                if (MessageBox.Show("You have added a new remote repository." + Environment.NewLine + "Do you want to automaticly configure the default push and pull behaviour for this remote?", "New remote", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    string remoteUrl = Url.Text;

                    if (!string.IsNullOrEmpty(remoteUrl))
                    {
                        new FormProcess("remote update");

                        foreach (GitCommands.GitHead remoteHead in GitCommands.GitCommands.GetHeads(true, true))
                        {
                            foreach (GitCommands.GitHead localHead in GitCommands.GitCommands.GetHeads(true, true))
                            {
                                if (remoteHead.IsRemote &&
                                    !localHead.IsRemote &&
                                    string.IsNullOrEmpty(localHead.Remote) &&
                                    string.IsNullOrEmpty(localHead.Remote) &&
                                    !remoteHead.IsTag &&
                                    !localHead.IsTag &&
                                    remoteHead.Name.ToLower().Contains(localHead.Name.ToLower()) &&
                                    remoteHead.Name.ToLower().Contains(remote.ToLower()))
                                {
                                    localHead.Remote = RemoteName.Text;
                                    localHead.MergeWith = remoteHead.Name;
                                }
                            }
                        }
                    }

                    else
                    {
                        MessageBox.Show("You need to configure a valid url for this remote", "Url needed");
                    }

                }
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
            new FormProcess("remote update");
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

        private void RemoteName_TextChanged(object sender, EventArgs e)
        {
        }

        private void Url_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void PuttySshKey_TextChanged(object sender, EventArgs e)
        {
        }

        private void LoadSSHKey_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PuttySshKey.Text))
                MessageBox.Show("No SSH key file entered");
            else
                GitCommands.GitCommands.StartPageantWithKey(PuttySshKey.Text);
        }

        private void TestConnection_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunRealCmdDetatched("cmd.exe", "/k \"\"" + GitCommands.Settings.Plink + "\" -T \"" + Url.Text + "\"\"");
        }

        private void Prune_Click(object sender, EventArgs e)
        {
            new FormProcess("remote prune " + remote);
        }

        private void RemoteBranches_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void RemoteBranches_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void RemoteBranches_SelectionChanged(object sender, EventArgs e)
        {
            if (RemoteBranches.SelectedRows.Count != 1)
                return;

            GitCommands.GitHead head = RemoteBranches.SelectedRows[0].DataBoundItem as GitCommands.GitHead;

            if (head == null)
                return;

            LocalBranchNameEdit.Text = head.Name;
            LocalBranchNameEdit.ReadOnly = true;
            RemoteRepositoryCombo.Items.Clear();
            RemoteRepositoryCombo.Items.Add("");
            foreach (string remote in GitCommands.GitCommands.GetRemotes())
                RemoteRepositoryCombo.Items.Add(remote);

            RemoteRepositoryCombo.Text = head.Remote;

            DefaultMergeWithCombo.Text = head.MergeWith;

        }

        private void RemoteRepositoryCombo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void DefaultMergeWithCombo_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void DefaultMergeWithCombo_DropDown(object sender, EventArgs e)
        {
            if (RemoteBranches.SelectedRows.Count != 1)
                return;

            GitCommands.GitHead head = RemoteBranches.SelectedRows[0].DataBoundItem as GitCommands.GitHead;

            if (head == null)
                return;

            DefaultMergeWithCombo.Items.Clear();
            DefaultMergeWithCombo.Items.Add("");

            string currentSelectedRemote = RemoteRepositoryCombo.Text.Trim();

            if (!string.IsNullOrEmpty(head.Remote) && !string.IsNullOrEmpty(currentSelectedRemote))
            {
                string remoteUrl = GitCommands.GitCommands.GetSetting("remote." + currentSelectedRemote + ".url");

                if (!string.IsNullOrEmpty(remoteUrl))
                {
                    foreach (GitCommands.GitHead remoteHead in GitCommands.GitCommands.GetHeads(true, true))
                    {
                        if (remoteHead.IsRemote && remoteHead.Name.ToLower().Contains(currentSelectedRemote.ToLower()))
                        {
                            if (string.IsNullOrEmpty(remoteHead.MergeWith))
                            {
                                DefaultMergeWithCombo.Items.Add(remoteHead.Name);
                            }
                        }
                    }
                }
            }

        }

        private void RemoteRepositoryCombo_Validated(object sender, EventArgs e)
        {
            if (RemoteBranches.SelectedRows.Count != 1)
                return;

            GitCommands.GitHead head = RemoteBranches.SelectedRows[0].DataBoundItem as GitCommands.GitHead;
            if (head == null)
                return;

            head.Remote = RemoteRepositoryCombo.Text;
        }

        private void DefaultMergeWithCombo_Validated(object sender, EventArgs e)
        {
            if (RemoteBranches.SelectedRows.Count != 1)
                return;

            GitCommands.GitHead head = RemoteBranches.SelectedRows[0].DataBoundItem as GitCommands.GitHead;
            if (head == null)
                return;

            head.MergeWith = DefaultMergeWithCombo.Text;
        }

        private void LocalBranchNameEdit_TextChanged(object sender, EventArgs e)
        {

        }

        private void SaveDefaultPushPull_Click(object sender, EventArgs e)
        {
            Initialize();
        }
    }
}

