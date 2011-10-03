﻿using System;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormRemotes : GitExtensionsForm
    {
        private string _remote = "";

        private readonly TranslationString _remoteBranchDataError =
            new TranslationString("Invalid ´{1}´ found for branch ´{0}´." + Environment.NewLine +
                                  "Value has been reset to empty value.");

        private readonly TranslationString _questionAutoPullBehaviour =
            new TranslationString("You have added a new remote repository." + Environment.NewLine +
                                  "Do you want to automatically configure the default push and pull behavior for this remote?");

        private readonly TranslationString _questionAutoPullBehaviourCaption =
            new TranslationString("New remote");

        private readonly TranslationString _warningValidRemote = 
            new TranslationString("You need to configure a valid url for this remote");
        
        private readonly TranslationString _warningValidRemoteCaption =
            new TranslationString("Url needed");

        private readonly TranslationString _hintDelete =
            new TranslationString("Delete");

        private readonly TranslationString _questionDeleteRemote = 
            new TranslationString("Are you sure you want to delete this remote?");
        
        private readonly TranslationString _questionDeleteRemoteCaption = 
            new TranslationString("Delete");

        private readonly TranslationString _sshKeyOpenFilter =
            new TranslationString("Private key (*.ppk)");

        private readonly TranslationString _sshKeyOpenCaption =
            new TranslationString("Select ssh key file");

        private readonly TranslationString _warningNoKeyEntered =
            new TranslationString("No SSH key file entered");


        public FormRemotes()
        {
            InitializeComponent();
            Translate();
        }

        private void FormRemotesLoad(object sender, EventArgs e)
        {
            RestorePosition("remotes");
            Initialize();
        }

        private void RemoteBranchesDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show(
                string.Format(_remoteBranchDataError.Text, RemoteBranches.Rows[e.RowIndex].Cells[0].Value,
                    RemoteBranches.Columns[e.ColumnIndex].HeaderText));

            RemoteBranches.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
        }

        private void Initialize()
        {
            Remotes.DataSource = GitCommandHelpers.GetRemotes();

            var heads = GitCommandHelpers.GetHeads(false, true);
            RemoteBranches.DataSource = heads;

            RemoteBranches.DataError += RemoteBranchesDataError;

            PuTTYSSH.Visible = GitCommandHelpers.Plink();
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

                output = GitCommandHelpers.AddRemote(RemoteName.Text, Url.Text);

                if (MessageBox.Show(_questionAutoPullBehaviour.Text, _questionAutoPullBehaviourCaption.Text,
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var remoteUrl = Url.Text;

                    if (!string.IsNullOrEmpty(remoteUrl))
                    {
                        new FormRemoteProcess("remote update").ShowDialog();
                        ConfigureRemotes();
                    }
                    else
                        MessageBox.Show(_warningValidRemote.Text,_warningValidRemoteCaption.Text);
                }
            }
            else
            {
                if (RemoteName.Text != _remote)
                {
                    output = GitCommandHelpers.RenameRemote(_remote, RemoteName.Text);
                }

                GitCommandHelpers.SetSetting(string.Format("remote.{0}.url", RemoteName.Text), Url.Text);
                GitCommandHelpers.SetSetting(string.Format("remote.{0}.puttykeyfile", RemoteName.Text), PuttySshKey.Text);
            }

            if (!string.IsNullOrEmpty(output))
                MessageBox.Show(output, _hintDelete.Text);

            Initialize();
        }

        private void ConfigureRemotes()
        {
            foreach (var remoteHead in GitCommandHelpers.GetHeads(true, true))
            {
                foreach (var localHead in GitCommandHelpers.GetHeads(true, true))
                {
                    if (!remoteHead.IsRemote ||
                        localHead.IsRemote ||
                        !string.IsNullOrEmpty(localHead.TrackingRemote) ||
                        !string.IsNullOrEmpty(localHead.TrackingRemote) ||
                        remoteHead.IsTag ||
                        localHead.IsTag ||
                        !remoteHead.Name.ToLower().Contains(localHead.Name.ToLower()) ||
                        !remoteHead.Name.ToLower().Contains(_remote.ToLower()))
                        continue;
                    localHead.TrackingRemote = RemoteName.Text;
                    localHead.MergeWith = localHead.Name;
                }
            }
        }

        private void NewClick(object sender, EventArgs e)
        {
            var output = GitCommandHelpers.AddRemote("<new>", "");
            if (!string.IsNullOrEmpty(output))
                MessageBox.Show(output, _hintDelete.Text);
            Initialize();
        }

        private void DeleteClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_remote))
                return;

            if (MessageBox.Show(_questionDeleteRemote.Text, _questionDeleteRemoteCaption.Text, MessageBoxButtons.YesNo) ==
                DialogResult.Yes)
            {
                var output = GitCommandHelpers.RemoveRemote(_remote);
                if (!string.IsNullOrEmpty(output))
                    MessageBox.Show(output, _hintDelete.Text);
            }

            Initialize();
        }

        private void SshBrowseClick(object sender, EventArgs e)
        {
            var dialog =
                new OpenFileDialog
                    {
                        Filter = _sshKeyOpenFilter.Text + "|*.ppk",
                        InitialDirectory = ".",
                        Title = _sshKeyOpenCaption.Text
                    };
            if (dialog.ShowDialog() == DialogResult.OK)
                PuttySshKey.Text = dialog.FileName;
        }

        private void LoadSshKeyClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PuttySshKey.Text))
                MessageBox.Show(_warningNoKeyEntered.Text);
            else
                GitCommandHelpers.StartPageantWithKey(PuttySshKey.Text);
        }

        private void TestConnectionClick(object sender, EventArgs e)
        {
            GitCommandHelpers.RunRealCmdDetached(
                "cmd.exe",
                string.Format("/k \"\"{0}\" -T \"{1}\"\"", Settings.Plink, Url.Text));
        }

        private void PruneClick(object sender, EventArgs e)
        {
            new FormRemoteProcess("remote prune " + _remote).ShowDialog();
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
            foreach (var remote in GitCommandHelpers.GetRemotes())
                RemoteRepositoryCombo.Items.Add(remote);

            RemoteRepositoryCombo.Text = head.TrackingRemote;

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

            if (string.IsNullOrEmpty(head.TrackingRemote) || string.IsNullOrEmpty(currentSelectedRemote))
                return;

            var remoteUrl = GitCommandHelpers.GetSetting("remote." + currentSelectedRemote + ".url");

            if (string.IsNullOrEmpty(remoteUrl))
                return;

            foreach (var remoteHead in GitCommandHelpers.GetHeads(true, true))
            {
                if (remoteHead.IsRemote &&
                    remoteHead.Name.ToLower().Contains(currentSelectedRemote.ToLower()) /*&&
                    string.IsNullOrEmpty(remoteHead.MergeWith)*/)
                    DefaultMergeWithCombo.Items.Add(remoteHead.LocalName);
            }
        }

        private void RemoteRepositoryComboValidated(object sender, EventArgs e)
        {
            if (RemoteBranches.SelectedRows.Count != 1)
                return;

            var head = RemoteBranches.SelectedRows[0].DataBoundItem as GitHead;
            if (head == null)
                return;

            head.TrackingRemote = RemoteRepositoryCombo.Text;
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

            _remote = (string)Remotes.SelectedItem;
            RemoteName.Text = _remote;
            Url.Text = GitCommandHelpers.GetSetting(string.Format("remote.{0}.url", _remote));
            PuttySshKey.Text =
                GitCommandHelpers.GetSetting(string.Format("remote.{0}.puttykeyfile", RemoteName.Text));
        }

        private void UpdateBranchClick(object sender, EventArgs e)
        {
            new FormRemoteProcess("remote update").ShowDialog();
        }

        private void FormRemotes_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("remotes");
        }

    }
}