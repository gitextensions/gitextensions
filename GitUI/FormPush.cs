using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using ResourceManager.Translation;

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

        public Boolean PushOnShow { get; set; }

        public FormPush()
        {
            _currentBranch = GitCommandHelpers.GetSelectedBranch();
            InitializeComponent();
            Translate();
        }

        private void BrowseSourceClick(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog { SelectedPath = PushDestination.Text };
            if (dialog.ShowDialog() == DialogResult.OK)
                PushDestination.Text = dialog.SelectedPath;
        }

        private void PushClick(object sender, EventArgs e)
        {
            if (PullFromUrl.Checked && string.IsNullOrEmpty(PushDestination.Text))
            {
                MessageBox.Show(_selectDestinationDirectory.Text);
                return;
            }
            if (PullFromRemote.Checked && string.IsNullOrEmpty(Remotes.Text))
            {
                MessageBox.Show(_selectRemote.Text);
                return;
            }
            if (TabControlTagBranch.SelectedTab == TagTab && string.IsNullOrEmpty(TagComboBox.Text) &&
                !PushAllTags.Checked)
            {
                MessageBox.Show(_selectTag.Text);
                return;
            }

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
                        return;
            }

            Repositories.RepositoryHistory.AddMostRecentRepository(PushDestination.Text);

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
                pushCmd = GitCommands.GitCommandHelpers.PushCmd(destination, Branch.Text, RemoteBranch.Text,
                                                          PushAllBranches.Checked, ForcePushBranches.Checked);
            else
                pushCmd = GitCommands.GitCommandHelpers.PushTagCmd(destination, TagComboBox.Text, PushAllTags.Checked,
                                                             ForcePushBranches.Checked);
            var form = new FormProcess(pushCmd)
                       {
                           Remote = remote,
                           Text = string.Format(_pushToCaption.Text, destination)
                       };

            form.ShowDialog();

            if (!GitCommandHelpers.InTheMiddleOfConflictedMerge() &&
                !GitCommandHelpers.InTheMiddleOfRebase() && !form.ErrorOccurred())
                Close();
        }

        private void PushDestinationDropDown(object sender, EventArgs e)
        {
            PushDestination.DataSource = Repositories.RepositoryHistory.Repositories;
            PushDestination.DisplayMember = "Path";
        }

        private void BranchDropDown(object sender, EventArgs e)
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

        private static void PullClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartPullDialog();
        }

        private void RemoteBranchDropDown(object sender, EventArgs e)
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
                    GitHead branch = Branch.SelectedItem as GitHead;
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
            Remotes.Select();
            Remotes.Text = GitCommandHelpers.GetSetting(string.Format("branch.{0}.remote", _currentBranch));
            RemotesUpdated(null, null);

            Text = string.Concat(_pushCaption.Text, " (", Settings.WorkingDir, ")");
        }

        private static void AddRemoteClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartRemotesDialog();
        }

        private void RemotesDropDown(object sender, EventArgs e)
        {
            Remotes.DataSource = GitCommandHelpers.GetRemotes();
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
        }

        private void RemotesUpdated(object sender, EventArgs e)
        {
            EnableLoadSshButton();

            var pushSettingValue = GitCommandHelpers.GetSetting("remote." + Remotes.Text + ".push");

            if (PullFromRemote.Checked && !string.IsNullOrEmpty(pushSettingValue))
            {
                var values = pushSettingValue.Split(':');
                RemoteBranch.Text = "";
                if (values.Length > 0)
                {
                    GitHead currentBranch = new GitHead(null, values[0], Remotes.Text);
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
                GitHead currentBranch = new GitHead(null, _currentBranch, Remotes.Text);
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

        private void TagDropDown(object sender, EventArgs e)
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

        private void FormPush_Shown(object sender, EventArgs e)
        {
            if (PushOnShow)
                Push.PerformClick();
        }
    }
}