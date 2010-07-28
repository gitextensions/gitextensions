using System;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormRemotes : GitExtensionsForm
    {
        private string _remote = "";

        public FormRemotes()
        {
            InitializeComponent();
            Translate();
        }

        private void FormRemotesLoad(object sender, EventArgs e)
        {
            Initialize();
        }

        private void RemoteBranchesDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show(
                string.Format(
                    "Invalid ´{1}´ found for branch ´{0}´." + Environment.NewLine +
                    "Value has been reset to empty value.", RemoteBranches.Rows[e.RowIndex].Cells[0].Value,
                    RemoteBranches.Columns[e.ColumnIndex].HeaderText));

            RemoteBranches.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
        }

        private void Initialize()
        {
            Remotes.DataSource = GitCommands.GitCommands.GetRemotes();

            var heads = GitCommands.GitCommands.GetHeads(false, true);
            RemoteBranches.DataSource = heads;

            RemoteBranches.DataError += RemoteBranchesDataError;

            PuTTYSSH.Visible = GitCommands.GitCommands.Plink();
        }

        private void UrlDropDown(object sender, EventArgs e)
        {
            Url.DataSource = Repositories.RepositoryHistory.Repositories;
            Url.DisplayMember = "Path";
        }

        private void BrowseClick(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                Url.Text = dialog.SelectedPath;
        }

        private void SaveClick(object sender, EventArgs e)
        {
            var output = "";

            if (string.IsNullOrEmpty(_remote))
            {
                if (string.IsNullOrEmpty(RemoteName.Text) && string.IsNullOrEmpty(Url.Text))
                    return;

                output = GitCommands.GitCommands.AddRemote(RemoteName.Text, Url.Text);

                if (MessageBox.Show(
                        "You have added a new remote repository." + Environment.NewLine +
                        "Do you want to automaticly configure the default push and pull behaviour for this remote?",
                        "New remote", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var remoteUrl = Url.Text;

                    if (!string.IsNullOrEmpty(remoteUrl))
                    {
                        new FormProcess("remote update").ShowDialog();
                        ConfigureRemotes();
                    }
                    else
                        MessageBox.Show("You need to configure a valid url for this remote", "Url needed");
                }
            }
            else
            {
                if (RemoteName.Text != _remote)
                {
                    output = GitCommands.GitCommands.RenameRemote(_remote, RemoteName.Text);
                }

                GitCommands.GitCommands.SetSetting(string.Format("remote.{0}.url", RemoteName.Text), Url.Text);
                GitCommands.GitCommands.SetSetting(string.Format("remote.{0}.puttykeyfile", RemoteName.Text), PuttySshKey.Text);
            }

            if (!string.IsNullOrEmpty(output))
                MessageBox.Show(output, "Delete");

            Initialize();
        }

        private void ConfigureRemotes()
        {
            foreach (var remoteHead in GitCommands.GitCommands.GetHeads(true, true))
            {
                foreach (var localHead in GitCommands.GitCommands.GetHeads(true, true))
                {
                    if (!remoteHead.IsRemote || 
                        localHead.IsRemote ||
                        !string.IsNullOrEmpty(localHead.Remote) || 
                        !string.IsNullOrEmpty(localHead.Remote) ||
                        remoteHead.IsTag || 
                        localHead.IsTag ||
                        !remoteHead.Name.ToLower().Contains(localHead.Name.ToLower()) ||
                        !remoteHead.Name.ToLower().Contains(_remote.ToLower())) 
                        continue;
                    localHead.Remote = RemoteName.Text;
                    localHead.MergeWith = remoteHead.Name;
                }
            }
        }

        private void NewClick(object sender, EventArgs e)
        {
            var output = GitCommands.GitCommands.AddRemote("<new>", "");
            if (!string.IsNullOrEmpty(output))
                MessageBox.Show(output, "Delete");
            Initialize();
        }

        private void DeleteClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_remote))
                return;

            if (MessageBox.Show("Are you sure you want to delete this remote?", "Delete", MessageBoxButtons.YesNo) ==
                DialogResult.Yes)
            {
                var output = GitCommands.GitCommands.RemoveRemote(_remote);
                if (!string.IsNullOrEmpty(output))
                    MessageBox.Show(output, "Delete");
            }

            Initialize();
        }

        private void SshBrowseClick(object sender, EventArgs e)
        {
            var dialog =
                new OpenFileDialog
                    {
                        Filter = "Private key (*.ppk)|*.ppk",
                        InitialDirectory = ".",
                        Title = "Select ssh key file"
                    };
            if (dialog.ShowDialog() == DialogResult.OK)
                PuttySshKey.Text = dialog.FileName;
        }

        private void LoadSshKeyClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PuttySshKey.Text))
                MessageBox.Show("No SSH key file entered");
            else
                GitCommands.GitCommands.StartPageantWithKey(PuttySshKey.Text);
        }

        private void TestConnectionClick(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunRealCmdDetatched(
                "cmd.exe",
                string.Format("/k \"\"{0}\" -T \"{1}\"\"", Settings.Plink, Url.Text));
        }

        private void PruneClick(object sender, EventArgs e)
        {
            new FormProcess("remote prune " + _remote).ShowDialog();
        }

        private void RemoteBranchesSelectionChanged(object sender, EventArgs e)
        {
            if (RemoteBranches.SelectedRows.Count != 1)
                return;

            var head = RemoteBranches.SelectedRows[0].DataBoundItem as GitHead;

            if (head == null)
                return;

            LocalBranchNameEdit.Text = head.Name;
            LocalBranchNameEdit.ReadOnly = true;
            RemoteRepositoryCombo.Items.Clear();
            RemoteRepositoryCombo.Items.Add("");
            foreach (var remote in GitCommands.GitCommands.GetRemotes())
                RemoteRepositoryCombo.Items.Add(remote);

            RemoteRepositoryCombo.Text = head.Remote;

            DefaultMergeWithCombo.Text = head.MergeWith;
        }


        private void DefaultMergeWithComboDropDown(object sender, EventArgs e)
        {
            if (RemoteBranches.SelectedRows.Count != 1)
                return;

            var head = RemoteBranches.SelectedRows[0].DataBoundItem as GitHead;

            if (head == null)
                return;

            DefaultMergeWithCombo.Items.Clear();
            DefaultMergeWithCombo.Items.Add("");

            var currentSelectedRemote = RemoteRepositoryCombo.Text.Trim();

            if (string.IsNullOrEmpty(head.Remote) || string.IsNullOrEmpty(currentSelectedRemote))
                return;

            var remoteUrl = GitCommands.GitCommands.GetSetting("remote." + currentSelectedRemote + ".url");

            if (string.IsNullOrEmpty(remoteUrl))
                return;

            foreach (var remoteHead in GitCommands.GitCommands.GetHeads(true, true))
            {
                if (remoteHead.IsRemote &&
                    remoteHead.Name.ToLower().Contains(currentSelectedRemote.ToLower()) &&
                    string.IsNullOrEmpty(remoteHead.MergeWith))
                    DefaultMergeWithCombo.Items.Add(remoteHead.Name);
            }
        }

        private void RemoteRepositoryComboValidated(object sender, EventArgs e)
        {
            if (RemoteBranches.SelectedRows.Count != 1)
                return;

            var head = RemoteBranches.SelectedRows[0].DataBoundItem as GitHead;
            if (head == null)
                return;

            head.Remote = RemoteRepositoryCombo.Text;
        }

        private void DefaultMergeWithComboValidated(object sender, EventArgs e)
        {
            if (RemoteBranches.SelectedRows.Count != 1)
                return;

            var head = RemoteBranches.SelectedRows[0].DataBoundItem as GitHead;
            if (head == null)
                return;

            head.MergeWith = DefaultMergeWithCombo.Text;
        }

        private void SaveDefaultPushPullClick(object sender, EventArgs e)
        {
            Initialize();
        }

        private void RemotesSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(Remotes.SelectedItem is string))
                return;

            _remote = (string) Remotes.SelectedItem;
            RemoteName.Text = _remote;
            Url.Text = GitCommands.GitCommands.GetSetting(string.Format("remote.{0}.url", _remote));
            PuttySshKey.Text =
                GitCommands.GitCommands.GetSetting(string.Format("remote.{0}.puttykeyfile", RemoteName.Text));
        }

        private static void UpdateBranchClick(object sender, EventArgs e)
        {
            new FormProcess("remote update").ShowDialog();
        }
    }
}