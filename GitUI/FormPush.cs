using System;
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

        public FormPush()
        {
            _currentBranch = GitCommands.GitCommands.GetSelectedBranch();
            InitializeComponent();
            Translate();
        }

        private void BrowseSourceClick(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog {SelectedPath = PushDestination.Text};
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
                if (!GitCommands.GitCommands.GetBranches(true, Remotes.Text).Contains(RemoteBranch.Text))
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
                if (GitCommands.GitCommands.Plink())
                {
                    if (!File.Exists(Settings.Pageant))
                        MessageBox.Show(_cannotLoadPutty.Text, PuttyText);
                    else
                        GitCommands.GitCommands.StartPageantForRemote(Remotes.Text);
                }

                destination = Remotes.Text;
                remote = Remotes.Text.Trim();
            }

            string pushCmd;
            if (TabControlTagBranch.SelectedTab == BranchTab)
                pushCmd = GitCommands.GitCommands.PushCmd(destination, Branch.Text, RemoteBranch.Text,
                                                          PushAllBranches.Checked, ForcePushBranches.Checked);
            else
                pushCmd = GitCommands.GitCommands.PushTagCmd(destination, TagComboBox.Text, PushAllTags.Checked,
                                                             ForcePushBranches.Checked);
            var form = new FormProcess(pushCmd)
                       {
                           Remote = remote,
                           Text = string.Format(_pushToCaption.Text, destination)
                       };

            form.ShowDialog();

            if (!GitCommands.GitCommands.InTheMiddleOfConflictedMerge() &&
                !GitCommands.GitCommands.InTheMiddleOfRebase() && !form.ErrorOccured())
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

            foreach (var head in GitCommands.GitCommands.GetHeads(false, true))
                Branch.Items.Add(head.Name);

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

            foreach (var head in GitCommands.GitCommands.GetBranches(true, Remotes.Text))
                if (!RemoteBranch.Items.Contains(head))
                    RemoteBranch.Items.Add(head);
        }

        private void BranchSelectedValueChanged(object sender, EventArgs e)
        {
            if (Branch.Text != HeadText)
                RemoteBranch.Text = Branch.Text;
        }

        private void FormPushLoad(object sender, EventArgs e)
        {
            Remotes.Text = GitCommands.GitCommands.GetSetting(string.Format("branch.{0}.remote", _currentBranch));
            RemotesUpdated(null, null);

            Text = string.Concat(_pushCaption.Text, " (", Settings.WorkingDir, ")");
        }

        private static void AddRemoteClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartRemotesDialog();
        }

        private void RemotesDropDown(object sender, EventArgs e)
        {
            Remotes.DataSource = GitCommands.GitCommands.GetRemotes();
        }

        private void PullFromRemoteCheckedChanged(object sender, EventArgs e)
        {
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

            var pushSettingName = "remote." + Remotes.Text + ".push";
            var pushSettingValue = GitCommands.GitCommands.GetSetting(pushSettingName);
            if (!string.IsNullOrEmpty(pushSettingValue))
            {
                var values = pushSettingValue.Split(':');
                RemoteBranch.Text = "";
                if (values.Length > 0)
                    Branch.Text = values[0];
                if (values.Length > 1)
                    RemoteBranch.Text = values[1];
            }
            else
            {
                // Doing this makes it pretty easy to accidentally create a branch on the remote.
                // But leaving it blank will do the 'default' thing, meaning all branches are pushed.
                // Solution: when pushing a branch that doesn't exist on the remote, ask what to do
                Branch.Text = _currentBranch;
                RemoteBranch.Text = Branch.Text;
            }
        }

        private void EnableLoadSshButton()
        {
            LoadSSHKey.Visible = !string.IsNullOrEmpty(GitCommands.GitCommands.GetPuttyKeyFileForRemote(Remotes.Text));
        }

        private void LoadSshKeyClick(object sender, EventArgs e)
        {
            if (!File.Exists(Settings.Pageant))
                MessageBox.Show(_cannotLoadPutty.Text, PuttyText);
            else
                GitCommands.GitCommands.StartPageantForRemote(Remotes.Text);
        }

        private void RemotesValidated(object sender, EventArgs e)
        {
            EnableLoadSshButton();
        }

        private void TagDropDown(object sender, EventArgs e)
        {
            TagComboBox.DisplayMember = "Name";
            var tags = GitCommands.GitCommands.GetHeads(true, false);
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
    }
}