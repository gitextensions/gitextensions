using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using System.IO;
using GitCommands;

namespace GitUI
{
    public partial class FormPull : GitExtensionsForm
    {
        public FormPull()
        {
            InitializeComponent();
        }

        private void BrowseSource_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = PullSource.Text;
            if (dialog.ShowDialog() == DialogResult.OK)
                PullSource.Text = dialog.SelectedPath;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Mergetool_Click(object sender, EventArgs e)
        {
            GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitCommand, "mergetool");

            if (MessageBox.Show("Resolved all conflicts? Commit?", "Conflicts solved", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //Output.Text += "\n";
                FormCommit form = new FormCommit();
                form.ShowDialog();
            }
        }

        private void PullSource_TextChanged(object sender, EventArgs e)
        {
        }

        private List<GitCommands.GitHead> Heads = null;

        private void Branches_DropDown(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            if ((PullFromUrl.Checked && string.IsNullOrEmpty(PullSource.Text)) &&
                (PullFromRemote.Checked && string.IsNullOrEmpty(Remotes.Text)))
            {
                Branches.DataSource = null;
                return;
            }

            //string realWorkingDir = GitCommands.Settings.WorkingDir;

            try
            {
                LoadPuttyKey();

                if (Heads == null)
                {
                    if (PullFromUrl.Checked)
                    {
                        //Heads = GitCommands.GitCommands.GetRemoteHeads(PullSource.Text, false, true);
                        Heads = GitCommands.GitCommands.GetHeads(false, true);
                    }
                    else
                    {
                        //The line below is the most reliable way to get a list containing
                        //all remote branches but it is also the slowest.
                        //Heads = GitCommands.GitCommands.GetRemoteHeads(Remotes.Text, false, true);

                        //The code below is a quick way to get a lost containg all remote branches.
                        //It only returns the heads that are allready known to the repository. This
                        //doesn't return heads that are new on the server. This can be updated using
                        //update branch info in the manage remotes dialog.
                        Heads = new List<GitHead>();
                        foreach (GitHead head in GitCommands.GitCommands.GetHeads(true, true))
                        {
                            if (head.IsRemote && head.Name.StartsWith(Remotes.Text, StringComparison.CurrentCultureIgnoreCase))
                            {
                                GitCommands.GitHead remoteHead = new GitCommands.GitHead();
                                remoteHead.Name = head.Name.Substring(head.Name.LastIndexOf("/") + 1);
                                Heads.Insert(0, remoteHead);
                            }

                        }
                    }
                }
                Branches.DisplayMember = "Name";

                GitCommands.GitHead allHead = new GitCommands.GitHead();
                allHead.Name = "*";
                Heads.Insert(0, allHead);
                GitCommands.GitHead noHead = new GitCommands.GitHead();
                noHead.Name = "";
                Heads.Insert(0, noHead);
                Branches.DataSource = Heads;
            }
            finally
            {
                //GitCommands.Settings.WorkingDir = realWorkingDir;
            }

            Cursor.Current = Cursors.Default;
        }

        private void Pull_Click(object sender, EventArgs e)
        {
            if (PullFromUrl.Checked && string.IsNullOrEmpty(PullSource.Text))
            {
                MessageBox.Show("Please select a source directory");
                return;
            }
            if (PullFromRemote.Checked && string.IsNullOrEmpty(Remotes.Text))
            {
                MessageBox.Show("Please select a remote repository");
                return;
            }
            if (!Fetch.Checked && Branches.Text == "*")
            {
                MessageBox.Show("You can only fetch all remote branches (*) whithout merge or rebase." + Environment.NewLine + "If you want to fetch all remote branches, choose fetch." + Environment.NewLine + "If you want to fetch and merge a branch, choose a specific branch.");
                return;                
            }

            if (Merge.Checked)
                GitCommands.Settings.PullMerge = "merge";
            if (Rebase.Checked)
                GitCommands.Settings.PullMerge = "rebase";
            if (Fetch.Checked)
                GitCommands.Settings.PullMerge = "fetch";

            GitCommands.Settings.AutoStash = AutoStash.Checked;

            GitCommands.RepositoryHistory.AddMostRecentRepository(PullSource.Text);

            string source;

            if (PullFromUrl.Checked)
                source = PullSource.Text;
            else
            {
                LoadPuttyKey();
                source = Remotes.Text;
            }

            bool stashed = false;
            if (AutoStash.Checked && GitCommands.GitCommands.GitStatus(false).Count > 0)
            {
                new FormProcess("stash save");
                stashed = true;
            }

            FormProcess process = null;
            if (Fetch.Checked)
                /*Output.Text = */
                process = new FormProcess(GitCommands.GitCommands.FetchCmd(source, Branches.Text));
            else if (Merge.Checked)
                /*Output.Text = */
                process = new FormProcess(GitCommands.GitCommands.PullCmd(source, Branches.Text, false));
            else if (Rebase.Checked)
                /*Output.Text = */
                process = new FormProcess(GitCommands.GitCommands.PullCmd(source, Branches.Text, true));

            if (!GitCommands.GitCommands.InTheMiddleOfConflictedMerge() && !GitCommands.GitCommands.InTheMiddleOfRebase() && (process != null && !process.ErrorOccured()))
                Close();

            //Rebase failed -> special 'rebase' merge conflict
            if (Rebase.Checked && GitCommands.GitCommands.InTheMiddleOfRebase())
            {
                GitUICommands.Instance.StartRebaseDialog();
                if (!GitCommands.GitCommands.InTheMiddleOfConflictedMerge() && !GitCommands.GitCommands.InTheMiddleOfRebase())
                    Close();
            }
            else
            {
                MergeConflictHandler.HandleMergeConflicts();
                if (!GitCommands.GitCommands.InTheMiddleOfConflictedMerge() && !GitCommands.GitCommands.InTheMiddleOfRebase())
                    Close();
            }

            if (AutoStash.Checked && stashed && !GitCommands.GitCommands.InTheMiddleOfConflictedMerge() && !GitCommands.GitCommands.InTheMiddleOfRebase())
            {
                if (MessageBox.Show("Apply stashed items to working dir again?", "Auto stash", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    new FormProcess("stash pop");

                    MergeConflictHandler.HandleMergeConflicts();
                }
            }


        }

        private void LoadPuttyKey()
        {
            if (GitCommands.GitCommands.Plink())
            {
                if (!File.Exists(GitCommands.Settings.Pageant))
                    MessageBox.Show("Cannot load SSH key. PuTTY is not configured properly.", "PuTTY");
                else
                    GitCommands.GitCommands.StartPageantForRemote(Remotes.Text);
            }
        }

        private void FormPull_Load(object sender, EventArgs e)
        {
            Pull.Select();

            string branch = GitCommands.GitCommands.GetSelectedBranch();
            Remotes.Text = GitCommands.GitCommands.GetSetting("branch." + branch + ".remote");

            this.Text = "Pull (" + GitCommands.Settings.WorkingDir + ")";
            EnableLoadSSHButton();


            Merge.Checked = GitCommands.Settings.PullMerge == "merge";
            Rebase.Checked = GitCommands.Settings.PullMerge == "rebase";
            Fetch.Checked = GitCommands.Settings.PullMerge == "fetch";

            AutoStash.Checked = GitCommands.Settings.AutoStash;
        }

        private void PullSource_DrawItem(object sender, DrawItemEventArgs e)
        {

        }

        private void PullSource_DropDown(object sender, EventArgs e)
        {
            PullSource.DataSource = GitCommands.RepositoryHistory.MostRecentRepositories;
        }

        private void Stash_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartStashDialog();
        }

        private void Remotes_DropDown(object sender, EventArgs e)
        {
            Remotes.DataSource = GitCommands.GitCommands.GetRemotes();
        }

        private void PullFromRemote_CheckedChanged(object sender, EventArgs e)
        {
            if (PullFromRemote.Checked)
            {
                ResetRemoteHeads();
                PullSource.Enabled = false;
                BrowseSource.Enabled = false;
                Remotes.Enabled = true;
                AddRemote.Enabled = true;
            }
        }

        private void PullFromUrl_CheckedChanged(object sender, EventArgs e)
        {
            if (PullFromUrl.Checked)
            {
                ResetRemoteHeads();
                PullSource.Enabled = true;
                BrowseSource.Enabled = true;
                Remotes.Enabled = false;
                AddRemote.Enabled = false;
            }
        }

        private void AddRemote_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartRemotesDialog();
        }

        private void AutoStash_CheckedChanged(object sender, EventArgs e)
        {

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
                MessageBox.Show("Cannot load SSH key. PuTTY is not configured properly.", "PuTTY");
            else
                GitCommands.GitCommands.StartPageantForRemote(Remotes.Text);
        }

        private void Remotes_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableLoadSSHButton();
        }

        private void Remotes_Validated(object sender, EventArgs e)
        {
            EnableLoadSSHButton();
        }

        private void Merge_CheckedChanged(object sender, EventArgs e)
        {
            PullImage.BackgroundImage = GitUI.Properties.Resources.merge;
        }

        private void Rebase_CheckedChanged(object sender, EventArgs e)
        {
            PullImage.BackgroundImage = GitUI.Properties.Resources.Rebase;
        }

        private void Fetch_CheckedChanged(object sender, EventArgs e)
        {
            PullImage.BackgroundImage = GitUI.Properties.Resources.fetch;
        }

        private void PullSource_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void PullSource_Validating(object sender, CancelEventArgs e)
        {
            ResetRemoteHeads();
        }

        private void Remotes_Validating(object sender, CancelEventArgs e)
        {
            ResetRemoteHeads();
        }

        private void ResetRemoteHeads()
        {
            Branches.DataSource = null;
            Heads = null;
        }
    }
}
