using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Repository;
using GitUI.Objects;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormRemotes : GitModuleForm
    {
        private IGitRemoteController _gitRemoteController;
        private GitRemote _selectedRemote;

        #region Translation
        private readonly TranslationString _remoteBranchDataError =
            new TranslationString("Invalid ´{1}´ found for branch ´{0}´." + Environment.NewLine +
                                  "Value has been reset to empty value.");

        private readonly TranslationString _questionAutoPullBehaviour =
            new TranslationString("You have added a new remote repository." + Environment.NewLine +
                                  "Do you want to automatically configure the default push and pull behavior for this remote?");

        private readonly TranslationString _questionAutoPullBehaviourCaption =
            new TranslationString("New remote");

        private readonly TranslationString _gitMessage =
          new TranslationString("Message");

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

        private readonly TranslationString _gbMgtPanelHeaderNew =
            new TranslationString("Create New Remote");

        private readonly TranslationString _gbMgtPanelHeaderEdit =
            new TranslationString("Edit Remote Details");
        #endregion


        public FormRemotes(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();

            // remove text from 'new' and 'delete' buttons because now they are represented by icons
            New.Text = string.Empty;
            Delete.Text = string.Empty;

            Application.Idle += application_Idle;
        }

        /// <summary>
        /// If this is not null before showing the dialog the given
        /// remote name will be preselected in the listbox
        /// </summary>
        public string PreselectRemoteOnLoad { get; set; }


        private IGitRef GetHeadForSelectedRemoteBranch()
        {
            if (RemoteBranches.SelectedRows.Count != 1)
            {
                return null;
            }
            var head = RemoteBranches.SelectedRows[0].DataBoundItem as IGitRef;
            return head;
        }

        private void Initialize(string preselectRemote = null)
        {
            // refresh registered git remotes
            _gitRemoteController.LoadRemotes();

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
            RemoteRepositoryCombo.DataSource = new[] { new GitRemote() }.Union(_gitRemoteController.Remotes).ToList();
            RemoteRepositoryCombo.DisplayMember = "Name";

            RemoteBranches.AutoGenerateColumns = false;
            RemoteBranches.SelectionChanged -= RemoteBranchesSelectionChanged;
            RemoteBranches.DataError += RemoteBranchesDataError;
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
            // if Putty SSH isn't enabled, reduce the minimum height of the form
            MinimumSize = new Size(MinimumSize.Width, pnlMgtPuttySsh.Visible ? MinimumSize.Height : MinimumSize.Height - pnlMgtPuttySsh.Height);

            // adjust width of the labels if required
            // this may be necessary if the translated labels require more space than English versions
            // the longest label is likely to be lebel3 (Private key file), so use it as a guide
            var widestLabelMinSize = new Size(label3.Width, 0);
            label1.MinimumSize = widestLabelMinSize;        // Name
            label2.MinimumSize = widestLabelMinSize;        // Url
            labelPushUrl.MinimumSize = widestLabelMinSize;  // Push URL

            if (Module == null)
            {
                return;
            }
            _gitRemoteController = new GitRemoteController(Module);
            // load the data for the very first time
            Initialize(PreselectRemoteOnLoad);
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
            using (var dialog = new OpenFileDialog
                {
                    Filter = _sshKeyOpenFilter.Text + @"|*.ppk",
                    InitialDirectory = ".",
                    Title = _sshKeyOpenCaption.Text
                })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    PuttySshKey.Text = dialog.FileName;
                }
            }
        }

        private void LoadSshKeyClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PuttySshKey.Text))
            {
                MessageBox.Show(this, _warningNoKeyEntered.Text);
            }
            else
            {
                GitModule.StartPageantWithKey(PuttySshKey.Text);
            }
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

        private void RemoteBranchesDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show(this,
                            string.Format(_remoteBranchDataError.Text, RemoteBranches.Rows[e.RowIndex].Cells[0].Value, RemoteBranches.Columns[e.ColumnIndex].HeaderText));
            RemoteBranches.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";
        }

        private void RemoteBranchesSelectionChanged(object sender, EventArgs e)
        {
            var head = GetHeadForSelectedRemoteBranch();
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
            var head = GetHeadForSelectedRemoteBranch();
            if (head == null)
            {
                return;
            }

            DefaultMergeWithCombo.Items.Clear();
            DefaultMergeWithCombo.Items.Add("");

            var currentSelectedRemote = RemoteRepositoryCombo.Text.Trim();

            if (string.IsNullOrEmpty(head.TrackingRemote) || string.IsNullOrEmpty(currentSelectedRemote))
            {
                return;
            }

            var remoteUrl = Module.GetPathSetting(string.Format(SettingKeyString.RemoteUrl, currentSelectedRemote));
            if (string.IsNullOrEmpty(remoteUrl))
            {
                return;
            }
            foreach (var remoteHead in Module.GetRefs(true, true))
            {
                if (remoteHead.IsRemote && remoteHead.Name.ToLower().Contains(currentSelectedRemote.ToLower()))
                {
                    DefaultMergeWithCombo.Items.Add(remoteHead.LocalName);
                }
            }
        }

        private void RemoteRepositoryComboValidated(object sender, EventArgs e)
        {
            var head = GetHeadForSelectedRemoteBranch();
            if (head == null)
            {
                return;
            }
            head.TrackingRemote = RemoteRepositoryCombo.Text;
        }

        private void DefaultMergeWithComboValidated(object sender, EventArgs e)
        {
            var head = GetHeadForSelectedRemoteBranch();
            if (head == null)
            {
                return;
            }
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
            New.Enabled = Delete.Enabled = false;
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

            New.Enabled = Delete.Enabled = true;
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