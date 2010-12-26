using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using GitCommands;
using System.IO;

namespace GitUI.RepoHosting
{
    public partial class ForkAndCloneForm : Form
    {
        IGitHostingPlugin _gitHoster;
        public ForkAndCloneForm(IGitHostingPlugin gitHoster)
        {
            _gitHoster = gitHoster;
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            UpdateMyRepos();
            _searchTB.Focus();
        }

        private void UpdateMyRepos()
        {
            _myReposLV.Items.Clear();
            var repos = _gitHoster.GetMyRepos();
            foreach (var repo in repos)
            {
                var lvi = new ListViewItem();
                lvi.Tag = repo;
                lvi.Text = repo.Name;
                lvi.SubItems.Add(repo.IsAFork ? "Yes" : "No");
                lvi.SubItems.Add(repo.Forks.ToString());
                lvi.SubItems.Add(repo.IsPrivate ? "Yes" : "No");
                _myReposLV.Items.Add(lvi);
            }
        }

        private void _searchBtn_Click(object sender, EventArgs e)
        {
            AcceptButton = null;
            _searchResultsLV.Items.Clear();

            var search = _searchTB.Text;
            if (search == null || search.Trim().Length == 0)
                return;

            var repos = _gitHoster.SearchForRepo(search);

            foreach (var repo in repos)
            {
                var lvi = new ListViewItem();
                lvi.Tag = repo;
                lvi.Text = repo.Name;
                lvi.SubItems.Add(repo.Owner);
                lvi.SubItems.Add(repo.Forks.ToString());
                _searchResultsLV.Items.Add(lvi);
            }
        }

        private void _forkBtn_Click(object sender, EventArgs e)
        {
            if (_searchResultsLV.SelectedItems.Count != 1)
            {
                MessageBox.Show(this, "You must select exactly one item", "Error");
                return;
            }

            var hostedRepo = _searchResultsLV.SelectedItems[0].Tag as IHostedGitRepo;
            try
            {
                hostedRepo.Fork();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Exception: " + ex.Message, "Error");
            }

            UpdateMyRepos();
        }

        private void _searchTB_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = _searchBtn;
        }

        private void _searchTB_Leave(object sender, EventArgs e)
        {
            this.AcceptButton = null;
        }

        private void _searchResultsLV_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_searchResultsLV.SelectedItems.Count != 1)
                return;

            var hostedRepo = (IHostedGitRepo)_searchResultsLV.SelectedItems[0].Tag;
            _searchResultItemDescription.Text = hostedRepo.Description;
        }

        private void _browseForCloneToDirbtn_Click(object sender, EventArgs e)
        {
            var initialDir = _cloneToTB.Text.Length > 0 ? _cloneToTB.Text : "C:\\";

            var browseDialog = new FolderBrowserDialog { SelectedPath = initialDir };

            if (browseDialog.ShowDialog(this) == DialogResult.OK)
                _cloneToTB.Text = browseDialog.SelectedPath;
        }

        private void _cloneForeignBtn_Click(object sender, EventArgs e)
        {

        }

        private void _cloneBtn_Click(object sender, EventArgs e)
        {
            if (_myReposLV.SelectedItems.Count != 1)
                return;

            var hostedRepo = (IHostedGitRepo)_myReposLV.SelectedItems[0].Tag;
            Clone(hostedRepo);
        }

        private void _openGitupPageBtn_Click(object sender, EventArgs e)
        {

        }

        private void Clone(IHostedGitRepo repo)
        {
            string targetDir = _cloneToTB.Text.Trim(); 
            if (targetDir.Length == 0)
            {
                MessageBox.Show(this, "Clone folder can not be empty", "Error");
                return;
            }

            if (Directory.Exists(targetDir))
                targetDir = Path.Combine(targetDir, repo.Name);
            string repoSrc = repo.IsMine ? repo.CloneReadWriteUrl : repo.CloneReadOnlyUrl;

            var fromProcess = new FormProcess(Settings.GitCommand, GitCommandHelpers.CloneCmd(repoSrc, targetDir, false, null));
            fromProcess.ShowDialog();

            if (fromProcess.ErrorOccurred())
                return;

            if (repo.ParentOwner != null)
            {
                var error = GitCommandHelpers.AddRemote(repo.ParentOwner, repo.ParentReadOnlyUrl);
                if (string.IsNullOrEmpty(error))
                    MessageBox.Show(this, error, "Could not add remote");
            }
        }
    }
}
