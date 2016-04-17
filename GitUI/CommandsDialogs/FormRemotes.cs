using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Repository;
using GitUI.Objects;

namespace GitUI.CommandsDialogs
{
    public partial class FormRemotes : GitModuleForm
    {
        private GitRemoteController _gitRemoteController;
        private GitRemote _selectedRemote;


        public FormRemotes(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();

            Application.Idle += application_Idle;
        }


        /// <summary>
        /// If this is not null before showing the dialog the given
        /// remote name will be preselected in the listbox
        /// </summary>
        public string PreselectRemoteOnLoad { get; set; }


        private void Initialize(string preselectRemote = null)
        {
            // refresh registered git remotes
            _gitRemoteController.Refresh();
            
            InitialiseTabRemotes(preselectRemote);
            InitialiseTabBehaviors();
        }

        private void InitialiseTabRemotes(string preselectRemote = null)
        {
            // because the binding the same BindingList to multiple controls,
            // and changes in one of the bound control automatically get reflected 
            // in the other control, which causes rather frustrating UX.
            // to address that, re-create binding lists for each individual control
            var repos = Repositories.RemoteRepositoryHistory.Repositories;

            try
            {
                // to stop the flicker binding the lists and 
                // when the selected remote is getting reset and then selected again
                Url.BeginUpdate();
                comboBoxPushUrl.BeginUpdate();
                Remotes.BeginUpdate();

                Url.DataSource = new BindingList<Repository>(repos);
                Url.DisplayMember = "Path";
                Url.SelectedItem = null;

                comboBoxPushUrl.DataSource = new BindingList<Repository>(repos);
                comboBoxPushUrl.DisplayMember = "Path";
                comboBoxPushUrl.SelectedItem = null;

                // we need to unwire and rewire the events to avoid excessive flickering
                Remotes.SelectedIndexChanged -= Remotes_SelectedIndexChanged;
                Remotes.DataSource = _gitRemoteController.Remotes;
                Remotes.DisplayMember = "Name";
                Remotes.SelectedIndexChanged += Remotes_SelectedIndexChanged;

                Remotes.SelectedItem = null;
                if (_gitRemoteController.Remotes.Any())
                {
                    if (!string.IsNullOrEmpty(preselectRemote))
                    {
                        Remotes.Text = preselectRemote;
                    }
                    // default fallback - if the preselection didn't work select the first available one
                    if (Remotes.SelectedItem == null)
                    {
                        Remotes.SelectedItem = _gitRemoteController.Remotes.First();
                    }
                }
                else
                {
                    RemoteName.Focus();
                }
            }
            finally
            {
                Remotes.EndUpdate();
                Url.EndUpdate();
                comboBoxPushUrl.EndUpdate();
            }
        }

        private void InitialiseTabBehaviors()
        {
            var heads = Module.GetRefs(false, true);

            RemoteRepositoryCombo.Sorted = false;
            RemoteRepositoryCombo.DataSource = new[] { new GitRemote() }.Union( _gitRemoteController.Remotes).ToList();
            RemoteRepositoryCombo.DisplayMember = "Name";

            RemoteBranches.SelectionChanged -= RemoteBranchesSelectionChanged;
            RemoteBranches.DataSource = heads;
            RemoteBranches.ClearSelection();
            RemoteBranches.SelectionChanged += RemoteBranchesSelectionChanged;

            if (RemoteBranches.Rows.Count > 0)
            {
                RemoteBranches.Rows[0].Selected = true;
            }
        }


        private void application_Idle(object sender, EventArgs e)
        {
            // we need this event only once, so unwire
            Application.Idle -= application_Idle;

            pnlMgtPuttySsh.Visible = GitCommandHelpers.Plink();

            if (Module == null)
            {
                return;
            }
            _gitRemoteController = new GitRemoteController(Module);
            // load the data for the very first time
            Initialize(PreselectRemoteOnLoad);
        }

        private void btnRemoteMoveUpDown_Click(object sender, EventArgs e)
        {
            if (_selectedRemote == null || !(sender == btnRemoteMoveUp || sender == btnRemoteMoveDown))
            {
                return;
            }

            // we need to unwire and rewire the events to avoid excessive flickering
            Remotes.SelectedIndexChanged -= Remotes_SelectedIndexChanged;
            // the underlying collection will be changing, suppress all unnecesary updates
            Remotes.BeginUpdate();

            var selection = _selectedRemote;

            _gitRemoteController.MoveRemote(_selectedRemote, (sender == btnRemoteMoveUp) ? ArrowDirection.Up : ArrowDirection.Down);

            // reflect the order change and select the original selection
            Remotes.SelectedItem = selection;
            Remotes.EndUpdate();
            Remotes.SelectedIndexChanged += Remotes_SelectedIndexChanged;

            btnRemoteMoveUp.Enabled = Remotes.SelectedIndex > 0;
            btnRemoteMoveDown.Enabled = Remotes.SelectedIndex < Remotes.Items.Count - 1;
        }

        private void SaveClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(RemoteName.Text))
            {
                return;
            }

            try
            {
                // disable the control while saving
                tabControl1.Enabled = false;

                if ((string.IsNullOrEmpty(comboBoxPushUrl.Text) && checkBoxSepPushUrl.Checked) ||
                    (comboBoxPushUrl.Text.Equals(Url.Text, StringComparison.OrdinalIgnoreCase)))
                {
                    checkBoxSepPushUrl.Checked = false;
                }

                // update all other remote properties
                var result = _gitRemoteController.SaveRemote(_selectedRemote,
                                                             RemoteName.Text,
                                                             Url.Text,
                                                             checkBoxSepPushUrl.Checked ? comboBoxPushUrl.Text : null,
                                                             PuttySshKey.Text);

                if (!string.IsNullOrEmpty(result.UserMessage))
                {
                    MessageBox.Show(this, result.UserMessage, _gitMessage.Text);
                }

                // if the user has just created a fresh new remote 
                // there may be a need to configure it
                if (result.ShouldUpdateRemote && !string.IsNullOrEmpty(Url.Text) &&
                    DialogResult.Yes == MessageBox.Show(this,
                                                        _questionAutoPullBehaviour.Text,
                                                        _questionAutoPullBehaviourCaption.Text,
                                                        MessageBoxButtons.YesNo))
                {
                    FormRemoteProcess.ShowDialog(this, "remote update");
                    _gitRemoteController.ConfigureRemotes(RemoteName.Text);
                }
            }
            finally
            {
                // re-enable the control and re-initialize
                tabControl1.Enabled = true;
                Initialize(RemoteName.Text);
            }
        }

        private void NewClick(object sender, EventArgs e)
        {
            Remotes.SelectedItem = null;
            RemoteName.Focus();
        }

        private void DeleteClick(object sender, EventArgs e)
        {
            if (_selectedRemote == null)
            {
                return;
            }

            if (DialogResult.Yes == MessageBox.Show(this, 
                                                    _questionDeleteRemote.Text, 
                                                    _questionDeleteRemoteCaption.Text, 
                                                    MessageBoxButtons.YesNo))
            {
                var output = _gitRemoteController.RemoveRemote(_selectedRemote);
                if (!string.IsNullOrEmpty(output))
                {
                    MessageBox.Show(this, output, _gitMessage.Text);
                }

                Initialize();
            }
        }

        private void SshBrowseClick(object sender, EventArgs e)
        {
            using (var dialog =
                new OpenFileDialog
                    {
                        Filter = _sshKeyOpenFilter.Text + @"|*.ppk",
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
                GitModule.StartPageantWithKey(PuttySshKey.Text);
        }

        private void TestConnectionClick(object sender, EventArgs e)
        {
            string url = GitCommandHelpers.GetPlinkCompatibleUrl(Url.Text);

            Module.RunExternalCmdDetachedShowConsole(
                "cmd.exe",
                string.Format("/k \"\"{0}\" -T {1}\"", AppSettings.Plink, url));
        }

        private void PruneClick(object sender, EventArgs e)
        {
            if (_selectedRemote == null)
            {
                return;
            }
            FormRemoteProcess.ShowDialog(this, "remote prune " + _selectedRemote.Name);
        }

        private void RemoteBranchesSelectionChanged(object sender, EventArgs e)
        {
            if (RemoteBranches.SelectedRows.Count != 1)
            {
                return;
            }

            var head = RemoteBranches.SelectedRows[0].DataBoundItem as GitRef;
            if (head == null)
            {
                return;
            }

            LocalBranchNameEdit.Text = head.Name;
            LocalBranchNameEdit.ReadOnly = true;

            RemoteRepositoryCombo.SelectedItem = _gitRemoteController.Remotes.FirstOrDefault(x => x.Name.Equals(head.TrackingRemote, StringComparison.OrdinalIgnoreCase));
            if (RemoteRepositoryCombo.SelectedItem == null)
            {
                RemoteRepositoryCombo.SelectedIndex = 0;
            }

            DefaultMergeWithCombo.Text = head.MergeWith;
        }

        private void DefaultMergeWithComboDropDown(object sender, EventArgs e)
        {
            if (RemoteBranches.SelectedRows.Count != 1)
                return;

            var head = RemoteBranches.SelectedRows[0].DataBoundItem as GitRef;

            if (head == null)
                return;

            DefaultMergeWithCombo.Items.Clear();
            DefaultMergeWithCombo.Items.Add("");

            var currentSelectedRemote = RemoteRepositoryCombo.Text.Trim();

            if (string.IsNullOrEmpty(head.TrackingRemote) || string.IsNullOrEmpty(currentSelectedRemote))
                return;

            var remoteUrl = Module.GetPathSetting(string.Format(SettingKeyString.RemoteUrl, currentSelectedRemote));

            if (string.IsNullOrEmpty(remoteUrl))
                return;

            foreach (var remoteHead in Module.GetRefs(true, true))
            {
                if (remoteHead.IsRemote &&
                    remoteHead.Name.ToLower().Contains(currentSelectedRemote.ToLower()) /*&&
                    string.IsNullOrEmpty(remoteHead.MergeWith)*/)
                    DefaultMergeWithCombo.Items.Add(remoteHead.LocalName);
            }
        }

        private void RemoteBranchesDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show(this,
                string.Format(_remoteBranchDataError.Text, RemoteBranches.Rows[e.RowIndex].Cells[0].Value,
                    RemoteBranches.Columns[e.ColumnIndex].HeaderText));

            RemoteBranches.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
        }

        private void RemoteRepositoryComboValidated(object sender, EventArgs e)
        {
            if (RemoteBranches.SelectedRows.Count != 1)
                return;

            var head = RemoteBranches.SelectedRows[0].DataBoundItem as GitRef;
            if (head == null)
                return;

            head.TrackingRemote = RemoteRepositoryCombo.Text;
        }

        private void DefaultMergeWithComboValidated(object sender, EventArgs e)
        {
            if (RemoteBranches.SelectedRows.Count != 1)
                return;

            var head = RemoteBranches.SelectedRows[0].DataBoundItem as GitRef;
            if (head == null)
                return;

            head.MergeWith = DefaultMergeWithCombo.Text;
        }

        private void SaveDefaultPushPullClick(object sender, EventArgs e)
        {
            Initialize();
        }

        private void RemoteName_TextChanged(object sender, EventArgs e)
        {
            Save.Enabled = RemoteName.Text.Trim().Length > 0;
        }

        private void Remotes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_selectedRemote == Remotes.SelectedItem)
            {
                return;
            }

            // reset all controls and disable all buttons until we have a selection
            New.Enabled =
                Delete.Enabled =
                btnRemoteMoveUp.Enabled =
                btnRemoteMoveDown.Enabled = false;
            RemoteName.Text = string.Empty;
            Url.Text = string.Empty;
            comboBoxPushUrl.Text = string.Empty;
            checkBoxSepPushUrl.Checked = false;
            PuttySshKey.Text = string.Empty;
            gbMgtPanel.Text = _gbMgtPanelHeaderNew.Text;

            _selectedRemote = Remotes.SelectedItem as GitRemote;
            if (_selectedRemote == null)
            {
                return;
            }

            New.Enabled =
                Delete.Enabled = true;
            btnRemoteMoveUp.Enabled = Remotes.SelectedIndex > 0;
            btnRemoteMoveDown.Enabled = Remotes.SelectedIndex < Remotes.Items.Count - 1;

            RemoteName.Text = _selectedRemote.Name;
            Url.Text = _selectedRemote.Url;
            comboBoxPushUrl.Text = _selectedRemote.PushUrl;
            checkBoxSepPushUrl.Checked = !string.IsNullOrEmpty(_selectedRemote.PushUrl);
            PuttySshKey.Text = _selectedRemote.PuttySshKey;
            gbMgtPanel.Text = _gbMgtPanelHeaderEdit.Text;
        }

        private void UpdateBranchClick(object sender, EventArgs e)
        {
            FormRemoteProcess.ShowDialog(this, "remote update");
        }

        private void checkBoxSepPushUrl_CheckedChanged(object sender, EventArgs e)
        {
            ShowSeparatePushUrl(checkBoxSepPushUrl.Checked);
        }

        private void ShowSeparatePushUrl(bool visible)
        {
            labelPushUrl.Visible = visible;
            comboBoxPushUrl.Visible = visible;
            folderBrowserButtonPushUrl.Visible = visible;

            if (!visible)
                label2.Text = _labelUrlAsFetchPush.Text;
            else
                label2.Text = _labelUrlAsFetch.Text;
        }
    }
}