using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using System.IO;

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
            GitCommands.GitCommands.RunRealCmd(GitCommands.Settings.GitDir + "git.cmd", "mergetool");

            if (MessageBox.Show("Resolved all conflicts? Commit?", "Conflicts solved", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //Output.Text += "\n";
                FormCommit form = new FormCommit();
                form.ShowDialog();
            }
        }

        private void PullSource_TextChanged(object sender, EventArgs e)
        {
            Branches.DataSource = null;
        }

        private void Branches_DropDown(object sender, EventArgs e)
        {
            if ((PullFromUrl.Checked && string.IsNullOrEmpty(PullSource.Text)) &&
                (PullFromRemote.Checked && string.IsNullOrEmpty(Remotes.Text)))
            {
                Branches.DataSource = null;
                return;
            }

            string realWorkingDir = GitCommands.Settings.WorkingDir;

            try
            {
                if (PullFromUrl.Checked)
                {
                    GitCommands.Settings.WorkingDir = PullSource.Text;
                }
                else
                {
                    GitCommands.Settings.WorkingDir = GitCommands.GitCommands.GetSetting("remote." + Remotes.Text + ".url");
                }
                Branches.DisplayMember = "Name";
                List<GitCommands.GitHead> heads = GitCommands.GitCommands.GetHeads(false);

                GitCommands.GitHead allHead = new GitCommands.GitHead();
                allHead.Name = "*";
                heads.Insert(0, allHead);
                GitCommands.GitHead noHead = new GitCommands.GitHead();
                noHead.Name = "";
                heads.Insert(0, noHead);
                Branches.DataSource = heads;
            }
            finally
            {
                GitCommands.Settings.WorkingDir = realWorkingDir;
            }
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
                if (GitCommands.GitCommands.Plink())
                {
                    if (!File.Exists(GitCommands.Settings.Pageant))
                        MessageBox.Show("Cannot load SSH key. PuTTY is not configured properly.", "PuTTY");
                    else
                        GitCommands.GitCommands.StartPageantForRemote(Remotes.Text);
                }
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

    }
}
