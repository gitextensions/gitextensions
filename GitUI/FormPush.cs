using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using ResourceManager.Translation;


namespace GitUI
{
    public partial class FormPush : GitExtensionsForm
    {
        TranslationString selectDestinationDirectory = new TranslationString("Please select a destination directory");
        TranslationString selectRemote = new TranslationString("Please select a remote repository");
        TranslationString selectTag = new TranslationString("You need to select a tag to push or select \"Push all tags\".");
        TranslationString cannotLoadPutty = new TranslationString("Cannot load SSH key. PuTTY is not configured properly.");
        TranslationString pushCaption = new TranslationString("Push");
        TranslationString branchNewForRemote = new TranslationString("The branch you are about to push seems to be a new branch for the remote." + Environment.NewLine + "Are you sure you want to push this branch?");
        TranslationString pushToCaption = new TranslationString("Push to {0}");
        
        private readonly string currentBranch;
        public FormPush()
        {
            currentBranch = GitCommands.GitCommands.GetSelectedBranch(); 
            InitializeComponent(); Translate();
        }

        private void BrowseSource_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = PushDestination.Text;
            if (dialog.ShowDialog() == DialogResult.OK)
                PushDestination.Text = dialog.SelectedPath;

        }

        private void Push_Click(object sender, EventArgs e)
        {
            if (PullFromUrl.Checked && string.IsNullOrEmpty(PushDestination.Text))
            {
                MessageBox.Show(selectDestinationDirectory.Text);
                return;
            }
            if (PullFromRemote.Checked && string.IsNullOrEmpty(Remotes.Text))
            {
                MessageBox.Show(selectRemote.Text);
                return;
            }
            if (TabControlTagBranch.SelectedTab == TagTab && string.IsNullOrEmpty(Tag.Text) && !PushAllTags.Checked)
            {
                MessageBox.Show(selectTag.Text);
                return;
            }

            //Extra check if the branch is already known to the remote, give a warning when not.
            //This is not possible when the remote is an URL, but this is ok since most users push to
            //known remotes anyway.
            if (TabControlTagBranch.SelectedTab == BranchTab && PullFromRemote.Checked)
            {
                //The current branch is not known by the remote (as far as we now since we are disconnected....)
                //if (!GitCommands.GitCommands.GetHeads(true, true).Exists(h => h.Remote.Equals(Remotes.Text) && h.Name.Equals(RemoteBranch.Text)))
                if (!GitCommands.GitCommands.GetBranches(true, Remotes.Text).Contains(RemoteBranch.Text) )
                    //Ask if this is what the user wants
                    if (MessageBox.Show(branchNewForRemote.Text, pushCaption.Text, MessageBoxButtons.YesNo) == DialogResult.No)
                        return;
            }

            GitCommands.Repositories.RepositoryHistory.AddMostRecentRepository(PushDestination.Text);

            FormProcess form;

            string remote = "";
            string destination;
            if (PullFromUrl.Checked)
            {
                destination = PushDestination.Text;
            }
            else
            {
                if (GitCommands.GitCommands.Plink())
                {
                    if (!File.Exists(GitCommands.Settings.Pageant))
                        MessageBox.Show(cannotLoadPutty.Text, "PuTTY");
                    else
                        GitCommands.GitCommands.StartPageantForRemote(Remotes.Text);
                }

                destination = Remotes.Text;
                remote = Remotes.Text.Trim();
            }

            string pushCmd;
            if (TabControlTagBranch.SelectedTab == BranchTab)
                pushCmd = GitCommands.GitCommands.PushCmd(destination, Branch.Text, RemoteBranch.Text, PushAllBranches.Checked, ForcePushBranches.Checked);
            else
                pushCmd = GitCommands.GitCommands.PushTagCmd(destination, Tag.Text, PushAllTags.Checked, ForcePushBranches.Checked);
            form = new FormProcess(pushCmd);
            form.Remote = remote;
            form.Text = string.Format(pushToCaption.Text, destination);
            form.ShowDialog();

            if (!GitCommands.GitCommands.InTheMiddleOfConflictedMerge() && !GitCommands.GitCommands.InTheMiddleOfRebase() && !form.ErrorOccured())
                Close();
        }

        private void PushDestination_DropDown(object sender, EventArgs e)
        {
            PushDestination.DataSource = GitCommands.Repositories.RepositoryHistory.Repositories;
            PushDestination.DisplayMember = "Path";
        }

        private void Branch_DropDown(object sender, EventArgs e)
        {
            string curBranch = Branch.Text;

            Branch.DisplayMember = "Name";
            Branch.Items.Clear();
            Branch.Items.Add("HEAD");

            if (string.IsNullOrEmpty(curBranch))
            {
                curBranch = currentBranch;
                if (curBranch.IndexOfAny("() ".ToCharArray()) != -1)
                {
                    curBranch = "HEAD";
                }
            }

            foreach (GitCommands.GitHead h in GitCommands.GitCommands.GetHeads(false, true))
            {
                Branch.Items.Add(h.Name);
            }
            Branch.Text = curBranch;
        }

        private void Pull_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartPullDialog();
        }

        private void RemoteBranch_DropDown(object sender, EventArgs e)
        {
            RemoteBranch.DisplayMember = "Name";
            RemoteBranch.Items.Clear();

            if (!string.IsNullOrEmpty(Branch.Text))
            {
                RemoteBranch.Items.Add(Branch.Text);
            }

            List<string> heads = GitCommands.GitCommands.GetBranches(true, Remotes.Text);
            foreach (string h in heads)
            {
                if (!RemoteBranch.Items.Contains(h))
                {
                    RemoteBranch.Items.Add(h);
                }
            }

        }

        private void Branch_SelectedValueChanged(object sender, EventArgs e)
        {
            if (Branch.Text != "HEAD")
            {
                RemoteBranch.Text = Branch.Text;
            }
        }

        private void FormPush_Load(object sender, EventArgs e)
        {
            Remotes.Text = GitCommands.GitCommands.GetSetting("branch." + currentBranch + ".remote");
            Remotes_Updated(null, null);

            this.Text = string.Concat(pushCaption.Text, " (", GitCommands.Settings.WorkingDir, ")");
        }

        private void AddRemote_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartRemotesDialog();
        }

        private void Remotes_DropDown(object sender, EventArgs e)
        {
            Remotes.DataSource = GitCommands.GitCommands.GetRemotes();
        }

        private void PullFromRemote_CheckedChanged(object sender, EventArgs e)
        {
            if (PullFromRemote.Checked)
            {
                PushDestination.Enabled = false;
                BrowseSource.Enabled = false;
                Remotes.Enabled = true;
                AddRemote.Enabled = true;
            }
        }

        private void PullFromUrl_CheckedChanged(object sender, EventArgs e)
        {
            if (PullFromUrl.Checked)
            {
                PushDestination.Enabled = true;
                BrowseSource.Enabled = true;
                Remotes.Enabled = false;
                AddRemote.Enabled = false;
            }
        }

        private void Remotes_Updated(object sender, EventArgs e)
        {
            EnableLoadSSHButton();

            string pushSettingName = "remote." + Remotes.Text + ".push";
            string pushSettingValue = GitCommands.GitCommands.GetSetting(pushSettingName);
            if (!string.IsNullOrEmpty(pushSettingValue))
            {
                string[] values = pushSettingValue.Split(':');
                if (values.Length > 0)
                {
                    Branch.Text = values[0];
                }
                if (values.Length > 1)
                {
                    RemoteBranch.Text = values[1];
                }
                else
                {
                    RemoteBranch.Text = "";
                }
            }
            else
            {
                // Doing this makes it pretty easy to accidentally create a branch on the remote.
                // But leaving it blank will do the 'default' thing, meaning all branches are pushed.
                // Solution: when pushing a branch that doesn't exist on the remote, ask what to do
                Branch.Text = currentBranch;
                RemoteBranch.Text = Branch.Text;
            }
        }

        private void EnableLoadSSHButton()
        {
            if (!string.IsNullOrEmpty(GitCommands.GitCommands.GetPuttyKeyFileForRemote(Remotes.Text)))
            {
                LoadSSHKey.Visible = true;
            }
            else
            {
                LoadSSHKey.Visible = false;
            }
        }

        private void LoadSSHKey_Click(object sender, EventArgs e)
        {
            if (!File.Exists(GitCommands.Settings.Pageant))
                MessageBox.Show(cannotLoadPutty.Text, "PuTTY");
            else
                GitCommands.GitCommands.StartPageantForRemote(Remotes.Text);
        }

        private void Remotes_Validated(object sender, EventArgs e)
        {
            EnableLoadSSHButton();
        }

        private void Tag_DropDown(object sender, EventArgs e)
        {
            Tag.DisplayMember = "Name";
            Tag.DataSource = GitCommands.GitCommands.GetHeads(true, false);
        }

        void ForcePushBranches_CheckedChanged(object sender, System.EventArgs e)
        {
            this.ForcePushTags.Checked = this.ForcePushBranches.Checked;
        }

        void ForcePushTags_CheckedChanged(object sender, System.EventArgs e)
        {
            this.ForcePushBranches.Checked = this.ForcePushTags.Checked;
        }

        private void PushAllBranches_CheckedChanged(object sender, EventArgs e)
        {
            Branch.Enabled = !PushAllBranches.Checked;
            RemoteBranch.Enabled = !PushAllBranches.Checked;
        }

    }
}
