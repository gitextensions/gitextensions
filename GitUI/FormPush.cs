﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using ResourceManager.Translation;
using GitUI.RepoHosting;
using GitUI.Script;

namespace GitUI
{
    public partial class FormPush : GitExtensionsForm
    {
        private const string PuttyText = "PuTTY";
        private const string HeadText = "HEAD";

        private readonly TranslationString _branchNewForRemote =
            new TranslationString("The branch you are about to push seems to be a new branch for the remote." +
                                  Environment.NewLine + "Are you sure you want to push this branch?");

        private readonly TranslationString _cannotLoadPutty =
            new TranslationString("Cannot load SSH key. PuTTY is not configured properly.");

        private readonly string _currentBranch;

        private readonly TranslationString _pushCaption = new TranslationString("Push");

        private readonly TranslationString _pushToCaption = new TranslationString("Push to {0}");

        private readonly TranslationString _selectDestinationDirectory =
            new TranslationString("Please select a destination directory");

        private readonly TranslationString _selectRemote = new TranslationString("Please select a remote repository");

        private readonly TranslationString _selectTag =
            new TranslationString("You need to select a tag to push or select \"Push all tags\".");

		private readonly TranslationString _yes = new TranslationString("Yes");
		private readonly TranslationString _no = new TranslationString("No");

        public FormPush()
        {
            InitializeComponent();
            Translate();

            //can't be set in OnLoad, because after PushAndShowDialogWhenFailed()
            //they are reset to false
            PushAllTags.Checked = Settings.PushAllTags;
            AutoPullOnRejected.Checked = Settings.AutoPullOnRejected;

            _currentBranch = GitCommandHelpers.GetSelectedBranch();

            Remotes.DataSource = GitCommandHelpers.GetRemotes();

            UpdateBranchDropDown();
            UpdateRemoteBranchDropDown();

            Push.Focus();

            Remotes.Text = GitCommandHelpers.GetSetting(string.Format("branch.{0}.remote", _currentBranch));
            RemotesUpdated(null, null);
        }

        public void PushAndShowDialogWhenFailed()
        {
            if (!PushChanges())
                ShowDialog();
        }


        private void BrowseSourceClick(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog { SelectedPath = PushDestination.Text };
            if (dialog.ShowDialog() == DialogResult.OK)
                PushDestination.Text = dialog.SelectedPath;
        }

        private void PushClick(object sender, EventArgs e)
        {
            if (PushChanges())
                Close();
        }

        private bool PushChanges()
        {
            if (PullFromUrl.Checked && string.IsNullOrEmpty(PushDestination.Text))
            {
                MessageBox.Show(_selectDestinationDirectory.Text);
                return false;
            }
            if (PullFromRemote.Checked && string.IsNullOrEmpty(Remotes.Text))
            {
                MessageBox.Show(_selectRemote.Text);
                return false;
            }
            if (TabControlTagBranch.SelectedTab == TagTab && string.IsNullOrEmpty(TagComboBox.Text) &&
                !PushAllTags.Checked)
            {
                MessageBox.Show(_selectTag.Text);
                return false;
            }

            bool newBranch = false;

            //Extra check if the branch is already known to the remote, give a warning when not.
            //This is not possible when the remote is an URL, but this is ok since most users push to
            //known remotes anyway.
            if (TabControlTagBranch.SelectedTab == BranchTab && PullFromRemote.Checked)
            {
                //The current branch is not known by the remote (as far as we now since we are disconnected....)
                if (!GitCommandHelpers.GetHeads(true, true).Exists(x => x.Remote == Remotes.Text && x.LocalName == RemoteBranch.Text))
                    //Ask if this is what the user wants
                    if (MessageBox.Show(_branchNewForRemote.Text, _pushCaption.Text, MessageBoxButtons.YesNo) ==
                        DialogResult.No)
                    {
                        return false;
                    }
                    else
                    {
                        newBranch = true;
                    }
            }

            Repositories.RepositoryHistory.AddMostRecentRepository(PushDestination.Text);
            Settings.PushAllTags = PushAllTags.Checked;
            Settings.AutoPullOnRejected = AutoPullOnRejected.Checked;

            var remote = "";
            string destination;
            if (PullFromUrl.Checked)
            {
                destination = PushDestination.Text;
            }
            else
            {
                if (GitCommandHelpers.Plink())
                {
                    if (!File.Exists(Settings.Pageant))
                        MessageBox.Show(_cannotLoadPutty.Text, PuttyText);
                    else
                        GitCommandHelpers.StartPageantForRemote(Remotes.Text);
                }

                destination = Remotes.Text;
                remote = Remotes.Text.Trim();
            }

            string pushCmd;
            if (TabControlTagBranch.SelectedTab == BranchTab)
            {
                bool track = newBranch;
                string[] remotes = Remotes.DataSource as string[];
                if (remotes != null)
                    foreach (string remoteBranch in remotes)
                        if (!string.IsNullOrEmpty(remoteBranch) && Branch.Text.StartsWith(remoteBranch))
                            track = false;


                pushCmd = GitCommandHelpers.PushCmd(destination, Branch.Text, RemoteBranch.Text,
                                                          PushAllBranches.Checked, ForcePushBranches.Checked, track);
            }
            else if (TabControlTagBranch.SelectedTab == TagTab)
                pushCmd = GitCommandHelpers.PushTagCmd(destination, TagComboBox.Text, PushAllTags.Checked,
                                                             ForcePushBranches.Checked);
            else
            {
                var pushActions = new List<GitPushAction>();
                foreach (DataRow row in _branchTable.Rows)
                {
                    var push = (bool)row["Push"];
                    var force = (bool)row["Force"];
                    var delete = (bool)row["Delete"];

                    if (push || force)
                        pushActions.Add(new GitPushAction(row["Local"].ToString(), row["Remote"].ToString(), force));
                    else if (delete)
                        pushActions.Add(new GitPushAction(row["Remote"].ToString()));
                }
                pushCmd = GitCommandHelpers.PushMultipleCmd(destination, pushActions);
            }

            ScriptManager.RunEventScripts(ScriptEvent.BeforePush);

            var form = new FormRemoteProcess(pushCmd)
                       {
                           Remote = remote,
                           Text = string.Format(_pushToCaption.Text, destination),
                           HandleOnExitCallback = new HandleOnExit(HandlePushOnExit)
                       };

            form.ShowDialog();

            if (!GitCommandHelpers.InTheMiddleOfConflictedMerge() &&
                !GitCommandHelpers.InTheMiddleOfRebase() && !form.ErrorOccurred())
            {
                ScriptManager.RunEventScripts(ScriptEvent.AfterPush);
                if (_createPullRequestCB.Checked)
                    GitUICommands.Instance.StartCreatePullRequest();
                return true;
            }

            return false;
        }

        private bool HandlePushOnExit(ref bool isError, FormProcess form) 
        {
            if (isError) 
            {

                if (Settings.AutoPullOnRejected && 
                    form.OutputString.ToString().Contains("To prevent you from losing history, non-fast-forward updates were rejected"))
                {
                    if (Settings.PullMerge == "fetch")
                        form.OutputString.Append("\nCan not perform auto pull, when merge option is set to fetch.");
                    else 
                    {
                        bool pullCompleted;
                        GitUICommands.Instance.StartPullDialog(true, out pullCompleted);
                        if (pullCompleted)
                        {
                            form.Retry();
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void FillPushDestinationDropDown()
        {
            PushDestination.DataSource = Repositories.RepositoryHistory.Repositories;
            PushDestination.DisplayMember = "Path";
        }

        private void UpdateBranchDropDown()
        {
            var curBranch = Branch.Text;

            Branch.DisplayMember = "Name";
            Branch.Items.Clear();
            Branch.Items.Add(HeadText);

            if (string.IsNullOrEmpty(curBranch))
            {
                curBranch = _currentBranch;
                if (curBranch.IndexOfAny("() ".ToCharArray()) != -1)
                    curBranch = HeadText;
            }

            foreach (var head in GitCommandHelpers.GetHeads(false, true))
                Branch.Items.Add(head);

            Branch.Text = curBranch;
        }

        private void PullClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartPullDialog();
        }

        private void UpdateRemoteBranchDropDown()
        {
            RemoteBranch.DisplayMember = "Name";
            RemoteBranch.Items.Clear();

            if (!string.IsNullOrEmpty(Branch.Text))
                RemoteBranch.Items.Add(Branch.Text);

            foreach (var head in GitCommandHelpers.GetHeads(false, true))
                if (!RemoteBranch.Items.Contains(head))
                    RemoteBranch.Items.Add(head);
        }

        private void BranchSelectedValueChanged(object sender, EventArgs e)
        {
            if (Branch.Text != HeadText)
            {
                if (PullFromRemote.Checked)
                {
                    var branch = Branch.SelectedItem as GitHead;
                    if (branch != null && branch.TrackingRemote.Equals(Remotes.Text.Trim()))
                    {
                        RemoteBranch.Text = branch.MergeWith;
                        if (!string.IsNullOrEmpty(RemoteBranch.Text))
                            return;
                    }
                }

                RemoteBranch.Text = Branch.Text;
            }
        }

        private void FormPushLoad(object sender, EventArgs e)
        {
            RestorePosition("push");

            Remotes.Select();

            Text = string.Concat(_pushCaption.Text, " (", Settings.WorkingDir, ")");

            var gitHoster = RepoHosts.TryGetGitHosterForCurrentWorkingDir();
            _createPullRequestCB.Enabled = gitHoster != null;
        }

        private void AddRemoteClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartRemotesDialog();
        }

        private void PullFromRemoteCheckedChanged(object sender, EventArgs e)
        {
            BranchSelectedValueChanged(null, null);
            if (!PullFromRemote.Checked)
                return;

            PushDestination.Enabled = false;
            BrowseSource.Enabled = false;
            Remotes.Enabled = true;
            AddRemote.Enabled = true;
        }

        private void PullFromUrlCheckedChanged(object sender, EventArgs e)
        {
            if (!PullFromUrl.Checked)
                return;

            PushDestination.Enabled = true;
            BrowseSource.Enabled = true;
            Remotes.Enabled = false;
            AddRemote.Enabled = false;

            FillPushDestinationDropDown();
        }

        private void RemotesUpdated(object sender, EventArgs e)
        {
			if (TabControlTagBranch.SelectedTab == MultipleBranchTab)
				UpdateMultiBranchView();

			EnableLoadSshButton();

            var pushSettingValue = GitCommandHelpers.GetSetting("remote." + Remotes.Text + ".push");

            if (PullFromRemote.Checked && !string.IsNullOrEmpty(pushSettingValue))
            {
                var values = pushSettingValue.Split(':');
                RemoteBranch.Text = "";
                if (values.Length > 0)
                {
                    var currentBranch = new GitHead(null, values[0], Remotes.Text);
                    Branch.Items.Add(currentBranch);
                    Branch.SelectedItem = currentBranch;
                }
                if (values.Length > 1)
                    RemoteBranch.Text = values[1];

                return;
            }

            if (string.IsNullOrEmpty(Branch.Text))
            {
                // Doing this makes it pretty easy to accidentally create a branch on the remote.
                // But leaving it blank will do the 'default' thing, meaning all branches are pushed.
                // Solution: when pushing a branch that doesn't exist on the remote, ask what to do
                var currentBranch = new GitHead(null, _currentBranch, Remotes.Text);
                Branch.Items.Add(currentBranch);
                Branch.SelectedItem = currentBranch;
                return;
            }

            BranchSelectedValueChanged(null, null);
        }

        private void EnableLoadSshButton()
        {
            LoadSSHKey.Visible = !string.IsNullOrEmpty(GitCommandHelpers.GetPuttyKeyFileForRemote(Remotes.Text));
        }

        private void LoadSshKeyClick(object sender, EventArgs e)
        {
            if (!File.Exists(Settings.Pageant))
                MessageBox.Show(_cannotLoadPutty.Text, PuttyText);
            else
                GitCommandHelpers.StartPageantForRemote(Remotes.Text);
        }

        private void RemotesValidated(object sender, EventArgs e)
        {
            EnableLoadSshButton();
        }

        private void FillTagDropDown()
        {
            TagComboBox.DisplayMember = "Name";
            var tags = GitCommandHelpers.GetHeads(true, false);
            TagComboBox.DataSource = tags;
        }

        private void ForcePushBranchesCheckedChanged(object sender, EventArgs e)
        {
            ForcePushTags.Checked = ForcePushBranches.Checked;
        }

        private void ForcePushTagsCheckedChanged(object sender, EventArgs e)
        {
            ForcePushBranches.Checked = ForcePushTags.Checked;
        }

        private void PushAllBranchesCheckedChanged(object sender, EventArgs e)
        {
            Branch.Enabled = !PushAllBranches.Checked;
            RemoteBranch.Enabled = !PushAllBranches.Checked;
        }

		#region Multi-Branch Methods

    	private DataTable _branchTable;

		private void UpdateMultiBranchView()
		{
			_branchTable = new DataTable();
			_branchTable.Columns.Add("Local", typeof (string));
			_branchTable.Columns.Add("Remote", typeof (string));
			_branchTable.Columns.Add("New", typeof (string));
			_branchTable.Columns.Add("Push", typeof(bool));
			_branchTable.Columns.Add("Force", typeof(bool));
			_branchTable.Columns.Add("Delete", typeof(bool));
			_branchTable.ColumnChanged += BranchTable_ColumnChanged;
			var bs = new BindingSource {DataSource = _branchTable};
		    BranchGrid.DataSource = bs;

			string remote = Remotes.Text.Trim();
			if (remote == "")
				return;

			List<GitHead> localHeads = GitCommandHelpers.GetHeads(false, true);
			List<GitHead> remoteHeads = GitCommandHelpers.GetRemoteHeads(remote, false, true);

			// Add all the local branches.
			foreach (var head in localHeads)
			{
				DataRow row = _branchTable.NewRow();
				row["Force"] = false;
				row["Delete"] = false;
				row["Local"] = head.Name;

				string remoteName;
				if (head.Remote == remote)
					remoteName = head.MergeWith ?? head.Name;
				else
					remoteName = head.Name;

				row["Remote"] = remoteName;
				bool newAtRemote = remoteHeads.Any(h => h.Name == remoteName);
				row["New"] =  newAtRemote ? _no.Text : _yes.Text;
				row["Push"] = newAtRemote;

				_branchTable.Rows.Add(row);
			}

			// Offer to delete all the left over remote branches.
			foreach (var remoteHead in remoteHeads)
			{
			    GitHead head = remoteHead;
			    if (!localHeads.Any(h => h.Name == head.Name))
				{
					DataRow row = _branchTable.NewRow();
					row["Local"] = null;
					row["Remote"] = remoteHead.Name;
					row["New"] = _no.Text;
					row["Push"] = false;
					row["Force"] = false;
					row["Delete"] = false;
					_branchTable.Rows.Add(row);
				}
			}
		}

        static void BranchTable_ColumnChanged(object sender, DataColumnChangeEventArgs e)
		{
			if (e.Column.ColumnName == "Push" && (bool)e.ProposedValue)
			{
				e.Row["Force"] = false;
				e.Row["Delete"] = false;
			}
			if (e.Column.ColumnName == "Force" && (bool)e.ProposedValue)
			{
				e.Row["Push"] = false;
				e.Row["Delete"] = false;
			}
			if (e.Column.ColumnName == "Delete" && (bool)e.ProposedValue)
			{
				e.Row["Push"] = false;
				e.Row["Force"] = false;
			}
		}

		private void TabControlTagBranch_Selected(object sender, TabControlEventArgs e)
		{
            if (TabControlTagBranch.SelectedTab == MultipleBranchTab)
                UpdateMultiBranchView();
            else if (TabControlTagBranch.SelectedTab == TagTab)
                FillTagDropDown();
            else
            {
                UpdateBranchDropDown();
                UpdateRemoteBranchDropDown();
            }
		}

		private void BranchGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			// Push grid checkbox changes immediately into the underlying data table.
			if (BranchGrid.CurrentCell is DataGridViewCheckBoxCell)
			{
				BranchGrid.EndEdit();
				((BindingSource)BranchGrid.DataSource).EndEdit();
			}
		}

		#endregion

        private void FormPush_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("push");
        }
    }
}