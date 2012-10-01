using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
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

        private readonly TranslationString _labelUrlAsFetch =
            new TranslationString("Fetch Url");
        private readonly TranslationString _labelUrlAsFetchPush =
            new TranslationString("Url");

        public FormRemotes()
            : base(true)
        {
            InitializeComponent();
            Translate();
        }

        private void FormRemotesLoad(object sender, EventArgs e)
        {
            Initialize();

            if (!string.IsNullOrEmpty(PreselectRemoteOnLoad))
            {
                Remotes.Text = PreselectRemoteOnLoad;
            }
        }

        private void RemoteBranchesDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show(this,
                string.Format(_remoteBranchDataError.Text, RemoteBranches.Rows[e.RowIndex].Cells[0].Value,
                    RemoteBranches.Columns[e.ColumnIndex].HeaderText));

            RemoteBranches.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
        }

        /// <summary>
        /// If this is not null before showing the dialog the given
        /// remote name will be preselected in the listbox
        /// </summary>
        public string PreselectRemoteOnLoad { get; set; }

        private void Initialize()
        {
            FillUrlDropDown();
            FillPushUrlDropDown();

            Remotes.DataSource = GitModule.Current.GetRemotes();

            var heads = GitModule.Current.GetHeads(false, true);
            RemoteBranches.DataSource = heads;

            RemoteBranches.DataError += RemoteBranchesDataError;

            PuTTYSSH.Visible = GitCommandHelpers.Plink();
        }

        private void FillUrlDropDown()
        {
            Url.DataSource = Repositories.RemoteRepositoryHistory.Repositories;
            Url.DisplayMember = "Path";
        }

        private void FillPushUrlDropDown()
        {
            comboBoxPushUrl.DataSource = Repositories.RemoteRepositoryHistory.Repositories;
            comboBoxPushUrl.DisplayMember = "Path";
        }

        private void BrowseClick(object sender, EventArgs e)
        {
            ShowFolderBrowserDialogWithPreselectedPath(() => Url.Text, path => Url.Text = path);
        }

        private void buttonBrowsePushUrl_Click(object sender, EventArgs e)
        {
            ShowFolderBrowserDialogWithPreselectedPath(() => comboBoxPushUrl.Text, path => comboBoxPushUrl.Text = path);
        }

        /// <summary>
        /// Opens a a FolderBrowserDialog with the path in "getter" preselected and 
        /// if the DialogResult.OK is returned uses "setter" to set the path
        /// 
        /// TODO: extract this method and use it at more places
        /// </summary>
        /// <param name="getter"></param>
        /// <param name="setter"></param>
        private void ShowFolderBrowserDialogWithPreselectedPath(Func<string> getter, Action<string> setter)
        {
            string directoryInfoPath = null;
            try
            {
                directoryInfoPath = new DirectoryInfo(getter()).FullName;
            }
            catch (Exception)
            {
                // since the DirectoryInfo stuff is for convenience we swallow exceptions
            }

            using (var dialog = new FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.Desktop,
                // if we do not use the DirectoryInfo then a path with slashes instead of backslashes won't work
                SelectedPath = directoryInfoPath ?? getter()
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    setter(dialog.SelectedPath);
                }
            }
        }

        private void SaveClick(object sender, EventArgs e)
        {
            var output = "";

            if ((string.IsNullOrEmpty(comboBoxPushUrl.Text) && checkBoxSepPushUrl.Checked) ||
                (comboBoxPushUrl.Text == Url.Text))
            {
                checkBoxSepPushUrl.Checked = false;
            }

            if (string.IsNullOrEmpty(_remote))
            {
                if (string.IsNullOrEmpty(RemoteName.Text) && string.IsNullOrEmpty(Url.Text))
                {
                    return;
                }

                output = GitModule.Current.AddRemote(RemoteName.Text, Url.Text);

                if (checkBoxSepPushUrl.Checked)
                {
                    GitModule.Current.SetPathSetting(string.Format(SettingKeyString.RemotePushUrl, RemoteName.Text), comboBoxPushUrl.Text);
                }

                if (MessageBox.Show(this, _questionAutoPullBehaviour.Text, _questionAutoPullBehaviourCaption.Text,
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var remoteUrl = Url.Text;

                    if (!string.IsNullOrEmpty(remoteUrl))
                    {
                        FormRemoteProcess.ShowDialog(this, "remote update");
                        ConfigureRemotes();
                    }
                    else
                    {
                        MessageBox.Show(this, _warningValidRemote.Text, _warningValidRemoteCaption.Text);
                    }
                }
            }
            else
            {
                if (RemoteName.Text != _remote)
                {
                    output = GitModule.Current.RenameRemote(_remote, RemoteName.Text);
                }

                GitModule.Current.SetPathSetting(string.Format(SettingKeyString.RemoteUrl, RemoteName.Text), Url.Text);
                GitModule.Current.SetPathSetting(string.Format("remote.{0}.puttykeyfile", RemoteName.Text), PuttySshKey.Text);
                if (checkBoxSepPushUrl.Checked)
                {
                    GitModule.Current.SetPathSetting(string.Format(SettingKeyString.RemotePushUrl, RemoteName.Text), comboBoxPushUrl.Text);
                }
                else
                {
                    GitModule.Current.UnsetSetting(string.Format(SettingKeyString.RemotePushUrl, RemoteName.Text));
                }
            }

            if (!string.IsNullOrEmpty(output))
            {
                MessageBox.Show(this, output, _hintDelete.Text);
            }

            Initialize();
        }

        private void ConfigureRemotes()
        {
            ConfigFile localConfig = GitModule.Current.GetLocalConfig();

            foreach (var remoteHead in GitModule.Current.GetHeads(true, true))
            {
                foreach (var localHead in GitModule.Current.GetHeads(true, true))
                {
                    if (!remoteHead.IsRemote ||
                        localHead.IsRemote ||
                        !string.IsNullOrEmpty(localHead.GetTrackingRemote(localConfig)) ||
                        !string.IsNullOrEmpty(localHead.GetTrackingRemote(localConfig)) ||
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
            var output = GitModule.Current.AddRemote("<new>", "");
            if (!string.IsNullOrEmpty(output))
            {
                MessageBox.Show(this, output, _hintDelete.Text);
            }
            Initialize();
        }

        private void DeleteClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_remote))
            {
                return;
            }

            if (MessageBox.Show(this, _questionDeleteRemote.Text, _questionDeleteRemoteCaption.Text, MessageBoxButtons.YesNo) ==
                DialogResult.Yes)
            {
                var output = GitModule.Current.RemoveRemote(_remote);
                if (!string.IsNullOrEmpty(output))
                {
                    MessageBox.Show(this, output, _hintDelete.Text);
                }
            }

            Initialize();
        }

        private void SshBrowseClick(object sender, EventArgs e)
        {
            using (var dialog =
                new OpenFileDialog
                    {
                        Filter = _sshKeyOpenFilter.Text + "|*.ppk",
                        InitialDirectory = ".",
                        Title = _sshKeyOpenCaption.Text
                    })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    PuttySshKey.Text = dialog.FileName;
            }
        }

        private void LoadSshKeyClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PuttySshKey.Text))
                MessageBox.Show(this, _warningNoKeyEntered.Text);
            else
                GitModule.Current.StartPageantWithKey(PuttySshKey.Text);
        }

        private void TestConnectionClick(object sender, EventArgs e)
        {
            System.Uri uri;
            string sshURL = "";
            if (System.Uri.TryCreate(Url.Text, UriKind.RelativeOrAbsolute, out uri) &&
                uri.IsAbsoluteUri && uri.Scheme == "ssh")
            {
                if (!string.IsNullOrEmpty(uri.UserInfo))
                    sshURL = uri.UserInfo + "@";
                sshURL += uri.Authority;
                if (uri.IsDefaultPort)
                    sshURL += ":" + uri.LocalPath.Substring(1);
                else
                    sshURL += uri.LocalPath;
            }
            else
                sshURL = Url.Text;
            GitModule.Current.RunRealCmdDetached(
                "cmd.exe",
                string.Format("/k \"\"{0}\" -T \"{1}\"\"", Settings.Plink, sshURL));
        }

        private void PruneClick(object sender, EventArgs e)
        {
            FormRemoteProcess.ShowDialog(this, "remote prune " + _remote);
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
            foreach (var remote in GitModule.Current.GetRemotes())
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

            var remoteUrl = GitModule.Current.GetPathSetting(string.Format(SettingKeyString.RemoteUrl, currentSelectedRemote));

            if (string.IsNullOrEmpty(remoteUrl))
                return;

            foreach (var remoteHead in GitModule.Current.GetHeads(true, true))
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

            Url.Text = GitModule.Current.GetPathSetting(string.Format(SettingKeyString.RemoteUrl, _remote));

            comboBoxPushUrl.Text = GitModule.Current.GetPathSetting(string.Format(SettingKeyString.RemotePushUrl, _remote));
            if (string.IsNullOrEmpty(comboBoxPushUrl.Text))
                checkBoxSepPushUrl.Checked = false;
            else
                checkBoxSepPushUrl.Checked = true;

            PuttySshKey.Text =
                GitModule.Current.GetPathSetting(string.Format("remote.{0}.puttykeyfile", RemoteName.Text));
        }

        private void UpdateBranchClick(object sender, EventArgs e)
        {
            FormRemoteProcess.ShowDialog(this, "remote update");
        }

        private void checkBoxSepPushUrl_CheckedChanged(object sender, EventArgs e)
        {
            ShowSeperatePushUrl(checkBoxSepPushUrl.Checked);
        }

        private void ShowSeperatePushUrl(bool visible)
        {
            labelPushUrl.Visible = visible;
            comboBoxPushUrl.Visible = visible;
            buttonBrowsePushUrl.Visible = visible;
            if (!visible)
                label2.Text = _labelUrlAsFetchPush.Text;
            else
                label2.Text = _labelUrlAsFetch.Text;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}