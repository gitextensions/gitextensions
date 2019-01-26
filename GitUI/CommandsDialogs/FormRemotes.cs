using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Remotes;
using GitCommands.UserRepositoryHistory;
using GitExtUtils.GitUI;
using GitUI.Properties;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormRemotes : GitModuleForm
    {
        private IGitRemoteManager _remoteManager;
        private GitRemote _selectedRemote;
        private readonly ListViewGroup _lvgEnabled;
        private readonly ListViewGroup _lvgDisabled;

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

        private readonly TranslationString _btnDeleteTooltip =
            new TranslationString("Delete the selected remote");

        private readonly TranslationString _btnNewTooltip =
            new TranslationString("Add a new remote");

        private readonly TranslationString _btnToggleStateTooltip_Activate =
            new TranslationString("Activate the selected remote");

        private readonly TranslationString _btnToggleStateTooltip_Deactivate =
            new TranslationString(@"Deactivate the selected remote.
Inactive remote is completely invisible to git.");

        private readonly TranslationString _lvgEnabledHeader =
            new TranslationString("Active");

        private readonly TranslationString _lvgDisabledHeader =
            new TranslationString("Inactive");

        private readonly TranslationString _enabledRemoteAlreadyExists =
            new TranslationString("An active remote named \"{0}\" already exists.");

        private readonly TranslationString _disabledRemoteAlreadyExists =
            new TranslationString("An inactive remote named \"{0}\" already exists.");
        #endregion

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormRemotes()
        {
            InitializeComponent();
        }

        public FormRemotes(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();

            // remove text from 'new' and 'delete' buttons because now they are represented by icons
            New.Text = string.Empty;
            Delete.Text = string.Empty;
            toolTip1.SetToolTip(New, _btnNewTooltip.Text);
            toolTip1.SetToolTip(Delete, _btnDeleteTooltip.Text);

            _lvgEnabled = new ListViewGroup(_lvgEnabledHeader.Text, HorizontalAlignment.Left);
            _lvgDisabled = new ListViewGroup(_lvgDisabledHeader.Text, HorizontalAlignment.Left);
            Remotes.Groups.AddRange(new[] { _lvgEnabled, _lvgDisabled });

            Application.Idle += application_Idle;

            BranchName.DataPropertyName = nameof(IGitRef.LocalName);
            RemoteCombo.DataPropertyName = nameof(IGitRef.TrackingRemote);
            MergeWith.DataPropertyName = nameof(IGitRef.MergeWith);

            Remotes.Columns[0].Width = DpiUtil.Scale(120);
        }

        /// <summary>
        /// If this is not null before showing the dialog the given
        /// remote name will be preselected in the listbox
        /// </summary>
        public string PreselectRemoteOnLoad { get; set; }

        /// <summary>
        /// Gets the list of remotes configured in .git/config file.
        /// </summary>
        private List<GitRemote> UserGitRemotes { get; set; }

        private void BindRemotes(string preselectRemote)
        {
            // we need to unwire and rewire the events to avoid excessive flickering
            Remotes.SelectedIndexChanged -= Remotes_SelectedIndexChanged;
            Remotes.Items.Clear();
            Remotes.Items.AddRange(UserGitRemotes.Select(remote =>
            {
                var group = remote.Disabled ? _lvgDisabled : _lvgEnabled;
                var color = remote.Disabled ? SystemColors.GrayText : SystemColors.WindowText;
                return new ListViewItem(group) { Text = remote.Name, Tag = remote, ForeColor = color };
            }).ToArray());
            Remotes.SelectedIndexChanged += Remotes_SelectedIndexChanged;

            Remotes.SelectedIndices.Clear();
            if (UserGitRemotes.Any())
            {
                if (!string.IsNullOrEmpty(preselectRemote))
                {
                    var lvi = Remotes.Items.Cast<ListViewItem>().FirstOrDefault(x => x.Text == preselectRemote);
                    if (lvi != null)
                    {
                        lvi.Selected = true;
                        flpnlRemoteManagement.Enabled = !((GitRemote)lvi.Tag).Disabled;
                    }
                }

                // default fallback - if the preselection didn't work select the first available one
                if (Remotes.SelectedIndices.Count < 1)
                {
                    var group = _lvgEnabled.Items.Count > 0 ? _lvgEnabled : _lvgDisabled;
                    group.Items[0].Selected = true;
                }

                Remotes.FocusedItem = Remotes.SelectedItems[0];
                Remotes.Select();
            }
            else
            {
                RemoteName.Focus();
            }
        }

        private void BindBtnToggleState(bool disabled)
        {
            if (disabled)
            {
                btnToggleState.Image = DpiUtil.Scale(Images.EyeOpened);
                toolTip1.SetToolTip(btnToggleState, (_btnToggleStateTooltip_Activate.Text ?? "").Trim());
            }
            else
            {
                btnToggleState.Image = DpiUtil.Scale(Images.EyeClosed);
                toolTip1.SetToolTip(btnToggleState, (_btnToggleStateTooltip_Deactivate.Text ?? "").Trim());
            }
        }

        [CanBeNull]
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
            UserGitRemotes = _remoteManager.LoadRemotes(true).ToList();

            InitialiseTabRemotes(preselectRemote);
            InitialiseTabBehaviors();
        }

        private void InitialiseTabRemotes(string preselectRemote = null)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var repositoryHistory = await RepositoryHistoryManager.Remotes.LoadRecentHistoryAsync();

                await this.SwitchToMainThreadAsync();
                try
                {
                    // because the binding the same BindingList to multiple controls,
                    // and changes in one of the bound control automatically get reflected
                    // in the other control, which causes rather frustrating UX.
                    // to address that, re-create binding lists for each individual control

                    // to stop the flicker binding the lists and
                    // when the selected remote is getting reset and then selected again
                    Url.BeginUpdate();
                    comboBoxPushUrl.BeginUpdate();
                    Remotes.BeginUpdate();

                    Url.DataSource = repositoryHistory.ToList();
                    Url.DisplayMember = nameof(Repository.Path);
                    Url.SelectedItem = null;

                    comboBoxPushUrl.DataSource = repositoryHistory.ToList();
                    comboBoxPushUrl.DisplayMember = nameof(Repository.Path);
                    comboBoxPushUrl.SelectedItem = null;

                    BindRemotes(preselectRemote);
                }
                finally
                {
                    Remotes.EndUpdate();
                    if (Remotes.SelectedIndices.Count > 0)
                    {
                        Remotes.EnsureVisible(Remotes.SelectedIndices[0]);
                    }

                    Url.EndUpdate();
                    comboBoxPushUrl.EndUpdate();
                }
            });
        }

        private void InitialiseTabBehaviors()
        {
            var heads = Module.GetRefs(false, true);

            RemoteRepositoryCombo.Sorted = false;
            RemoteRepositoryCombo.DataSource = new[] { new GitRemote() }.Union(UserGitRemotes).ToList();
            RemoteRepositoryCombo.DisplayMember = nameof(GitRemote.Name);

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

        private static void RemoteDelete(IList<Repository> remotes, string oldRemoteUrl)
        {
            if (string.IsNullOrWhiteSpace(oldRemoteUrl))
            {
                return;
            }

            var oldRemote = remotes.FirstOrDefault(r => r.Path == oldRemoteUrl);
            if (oldRemote != null)
            {
                remotes.Remove(oldRemote);
            }
        }

        private static void RemoteUpdate(IList<Repository> remotes, string oldRemoteUrl, string newRemoteUrl)
        {
            if (string.IsNullOrWhiteSpace(newRemoteUrl))
            {
                return;
            }

            // if remote url was renamed - delete the old value
            if (!string.Equals(oldRemoteUrl, newRemoteUrl, StringComparison.OrdinalIgnoreCase))
            {
                RemoteDelete(remotes, oldRemoteUrl);
            }

            if (remotes.All(r => r.Path != newRemoteUrl))
            {
                remotes.Add(new Repository(newRemoteUrl));
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
            // the longest label is likely to be label3 (Private key file), so use it as a guide
            var widestLabelMinSize = new Size(label3.Width, 0);
            label1.MinimumSize = label1.MaximumSize = widestLabelMinSize;        // Name
            label2.MinimumSize = label2.MaximumSize = widestLabelMinSize;        // Url
            labelPushUrl.MinimumSize = labelPushUrl.MaximumSize = widestLabelMinSize;  // Push URL

            _remoteManager = new GitRemoteManager(() => Module);

            // load the data for the very first time
            Initialize(PreselectRemoteOnLoad);
        }

        private void btnToggleState_Click(object sender, EventArgs e)
        {
            if (_selectedRemote == null)
            {
                btnToggleState.Visible = false;
                return;
            }

            _selectedRemote.Disabled = !_selectedRemote.Disabled;
            _remoteManager.ToggleRemoteState(_selectedRemote.Name, _selectedRemote.Disabled);

            Initialize(_selectedRemote.Name);
        }

        private void SaveClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RemoteName.Text))
            {
                return;
            }

            var remote = RemoteName.Text.Trim();
            var remoteUrl = Url.Text.Trim();
            var remotePushUrl = comboBoxPushUrl.Text.Trim();
            try
            {
                // disable the control while saving
                tabControl1.Enabled = false;

                if ((string.IsNullOrEmpty(remotePushUrl) && checkBoxSepPushUrl.Checked) ||
                    (!string.IsNullOrEmpty(remotePushUrl) && remotePushUrl.Equals(remoteUrl, StringComparison.OrdinalIgnoreCase)))
                {
                    checkBoxSepPushUrl.Checked = false;
                }

                if (_remoteManager.EnabledRemoteExists(remote))
                {
                    MessageBox.Show(this, string.Format(_enabledRemoteAlreadyExists.Text, remote), _gitMessage.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                if (_remoteManager.DisabledRemoteExists(remote))
                {
                    MessageBox.Show(this, string.Format(_disabledRemoteAlreadyExists.Text, remote), _gitMessage.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                // update all other remote properties
                var result = _remoteManager.SaveRemote(_selectedRemote,
                                                       remote,
                                                       remoteUrl,
                                                       checkBoxSepPushUrl.Checked ? remotePushUrl : null,
                                                       PuttySshKey.Text);

                if (!string.IsNullOrEmpty(result.UserMessage))
                {
                    MessageBox.Show(this, result.UserMessage, _gitMessage.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                else
                {
                    ThreadHelper.JoinableTaskFactory.Run(async () =>
                    {
                        var repositoryHistory = await RepositoryHistoryManager.Remotes.LoadRecentHistoryAsync();

                        await this.SwitchToMainThreadAsync();
                        RemoteUpdate(repositoryHistory, _selectedRemote?.Url, remoteUrl);
                        if (checkBoxSepPushUrl.Checked)
                        {
                            RemoteUpdate(repositoryHistory, _selectedRemote?.PushUrl, remotePushUrl);
                        }

                        await RepositoryHistoryManager.Remotes.SaveRecentHistoryAsync(repositoryHistory);
                    });
                }

                // if the user has just created a fresh new remote
                // there may be a need to configure it
                if (result.ShouldUpdateRemote &&
                    !string.IsNullOrEmpty(remoteUrl) &&
                    MessageBox.Show(this,
                        _questionAutoPullBehaviour.Text,
                        _questionAutoPullBehaviourCaption.Text,
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    FormRemoteProcess.ShowDialog(this, "remote update");
                    _remoteManager.ConfigureRemotes(remote);
                    UICommands.RepoChangedNotifier.Notify();
                }
            }
            finally
            {
                // re-enable the control and re-initialize
                tabControl1.Enabled = true;
                Initialize(remote);
            }
        }

        private void NewClick(object sender, EventArgs e)
        {
            Remotes.SelectedIndices.Clear();
            RemoteName.Focus();
        }

        private void DeleteClick(object sender, EventArgs e)
        {
            if (_selectedRemote == null)
            {
                return;
            }

            if (MessageBox.Show(this,
                                _questionDeleteRemote.Text,
                                _questionDeleteRemoteCaption.Text,
                                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var output = _remoteManager.RemoveRemote(_selectedRemote);
                if (!string.IsNullOrEmpty(output))
                {
                    MessageBox.Show(this, output, _gitMessage.Text);
                }

                // Deleting a remote from the history list may be undesirable as
                // it would hinder user's ability to *quickly* clone the remote repository
                // The flipside is that the history list may grow long without a UI to manage it

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
            var url = Url.Text;

            ThreadHelper.JoinableTaskFactory
                .RunAsync(() => new Plink().ConnectAsync(url))
                .FileAndForget();
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
            RemoteRepositoryCombo.SelectedItem = UserGitRemotes.FirstOrDefault(x => x.Name.Equals(head.TrackingRemote, StringComparison.OrdinalIgnoreCase));
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

            var remoteUrl = Module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentSelectedRemote));
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
            if (Remotes.SelectedIndices.Count > 0 && _selectedRemote == Remotes.SelectedItems[0].Tag)
            {
                return;
            }

            New.Enabled = Delete.Enabled = btnToggleState.Enabled = false;
            RemoteName.Text = string.Empty;
            Url.Text = string.Empty;
            comboBoxPushUrl.Text = string.Empty;
            checkBoxSepPushUrl.Checked = false;
            PuttySshKey.Text = string.Empty;
            gbMgtPanel.Text = _gbMgtPanelHeaderNew.Text;

            if (Remotes.SelectedIndices.Count < 1)
            {
                // we are here because we're adding a new remote - so no remotes selected
                // we just need to enable the panel so the user can enter the information
                _selectedRemote = null;
                flpnlRemoteManagement.Enabled = true;
                return;
            }

            // reset all controls and disable all buttons until we have a selection
            _selectedRemote = Remotes.SelectedItems[0].Tag as GitRemote;
            if (_selectedRemote == null)
            {
                return;
            }

            New.Enabled = Delete.Enabled = btnToggleState.Enabled = true;
            RemoteName.Text = _selectedRemote.Name;
            Url.Text = _selectedRemote.Url;
            comboBoxPushUrl.Text = _selectedRemote.PushUrl;
            checkBoxSepPushUrl.Checked = !string.IsNullOrEmpty(_selectedRemote.PushUrl);
            PuttySshKey.Text = _selectedRemote.PuttySshKey;
            gbMgtPanel.Text = _gbMgtPanelHeaderEdit.Text;
            BindBtnToggleState(_selectedRemote.Disabled);
            btnToggleState.Visible = true;
            flpnlRemoteManagement.Enabled = !_selectedRemote.Disabled;
        }

        private void UpdateBranchClick(object sender, EventArgs e)
        {
            FormRemoteProcess.ShowDialog(this, "remote update");
        }

        private void checkBoxSepPushUrl_CheckedChanged(object sender, EventArgs e)
        {
            ShowSeparatePushUrl(checkBoxSepPushUrl.Checked);
        }

        private void Remotes_MouseUp(object sender, MouseEventArgs e)
        {
            flpnlRemoteManagement.Enabled = !_selectedRemote?.Disabled ?? true;
        }

        private void ShowSeparatePushUrl(bool visible)
        {
            labelPushUrl.Visible = visible;
            comboBoxPushUrl.Visible = visible;
            folderBrowserButtonPushUrl.Visible = visible;

            label2.Text = visible
                ? _labelUrlAsFetch.Text
                : _labelUrlAsFetchPush.Text;
        }
    }
}