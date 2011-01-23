using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitUIPluginInterfaces.RepositoryHosts;
using GitCommands;
using System.IO;
using GitCommands.Repository;
using System.Diagnostics;
using ResourceManager.Translation;

namespace GitUI.RepoHosting
{
    public partial class ForkAndCloneForm : GitExtensionsForm
    {
        #region Translation

        private readonly TranslationString _strError = new TranslationString("Error");
        private readonly TranslationString _strLoading = new TranslationString(" : LOADING : ");
        private readonly TranslationString _strYes = new TranslationString("Yes");
        private readonly TranslationString _strNo = new TranslationString("No");
        private readonly TranslationString _strFailedToGetRepos = new TranslationString("Failed to get repos. Username/ApiToken incorrect?\r\n");
        private readonly TranslationString _strWillCloneWithPushAccess = new TranslationString("Will clone {0} into {1}.\r\nYou will have push access. {2}");
        private readonly TranslationString _strWillCloneInfo = new TranslationString("Will clone {0} into {1}.\r\nYou can not push unless you are a collaborator. {2}");
        private readonly TranslationString _strWillBeAddedAsARemote = new TranslationString("\"{0}\" will be added as a remote.");
        private readonly TranslationString _strCouldNotAddRemote = new TranslationString("Could not add remote");
        private readonly TranslationString _strNoHomepageDefined = new TranslationString("No homepage defined");
        private readonly TranslationString _strFailedToFork = new TranslationString("Failed to fork:\r\n");
        private readonly TranslationString _strSearchFailed = new TranslationString("Search failed!\r\n");
        private readonly TranslationString _strUserNotFound = new TranslationString("User not found!");
        private readonly TranslationString _strCouldNotFetchReposOfUser = new TranslationString("Could not fetch repositories of user!\r\n");
        private readonly TranslationString _strSearching = new TranslationString(" : SEARCHING : ");
        private readonly TranslationString _strSelectOneItem = new TranslationString("You must select exactly one item");
        private readonly TranslationString _strCloneFolderCanNotBeEmpty = new TranslationString("Clone folder can not be empty");
        #endregion

        readonly IRepositoryHostPlugin _gitHoster;

        public ForkAndCloneForm(IRepositoryHostPlugin gitHoster)
        {
            _gitHoster = gitHoster;
            InitializeComponent();
            Translate();
        }

        private void ForkAndCloneForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void Init()
        {
            var hist = Repositories.RepositoryHistory;
            var lastRepo = hist.Repositories.FirstOrDefault();
            if (lastRepo != null && !string.IsNullOrEmpty(lastRepo.Path))
            {
                string p = lastRepo.Path.Trim('/', '\\');

                _destinationTB.Text = Path.GetDirectoryName(p);
            }

            Text = _gitHoster.Description + ": " + Text;

            UpdateCloneInfo();
            UpdateMyRepos();
        }

        private void UpdateMyRepos()
        {
            _myReposLV.Items.Clear();
            _myReposLV.Items.Add(new ListViewItem { Text = _strLoading.Text });

            AsyncHelpers.DoAsync(
                () => _gitHoster.GetMyRepos(),

                repos =>
                {
                    _myReposLV.Items.Clear();
                    foreach (var repo in repos)
                    {
                        var lvi = new ListViewItem { Tag = repo, Text = repo.Name };
                        lvi.SubItems.Add(repo.IsAFork ? _strYes.Text : _strNo.Text);
                        lvi.SubItems.Add(repo.Forks.ToString());
                        lvi.SubItems.Add(repo.IsPrivate ? _strYes.Text : _strNo.Text);
                        _myReposLV.Items.Add(lvi);
                    }
                },

                ex =>
                {
                    MessageBox.Show(this, _strFailedToGetRepos.Text + ex.Message, _strError.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                });
        }

        #region GUI Handlers
        private void _searchBtn_Click(object sender, EventArgs e)
        {
            var search = _searchTB.Text;
            if (search == null || search.Trim().Length == 0)
                return;

            PrepareSearch(sender, e);

            AsyncHelpers.DoAsync(
                () => _gitHoster.SearchForRepository(search),
                HandleSearchResult,
                ex => { MessageBox.Show(this, _strSearchFailed.Text + ex.Message, _strError.Text); _searchBtn.Enabled = true; });
        }
        private void _getFromUserBtn_Click(object sender, EventArgs e)
        {
            var search = _searchTB.Text;
            if (search == null || search.Trim().Length == 0)
                return;
            PrepareSearch(sender, e);

            AsyncHelpers.DoAsync(
                () => _gitHoster.GetRepositoriesOfUser(search.Trim()),
                HandleSearchResult,
                ex =>
                {
                    if (ex.Message.Contains("404"))
                        MessageBox.Show(this, _strUserNotFound.Text, _strError.Text);
                    else
                        MessageBox.Show(this, _strCouldNotFetchReposOfUser.Text + ex.Message, _strError.Text);
                    _searchBtn.Enabled = true;
                });
        }


        private void PrepareSearch(object sender, EventArgs e)
        {
            _searchResultsLV.Items.Clear();
            _searchResultsLV_SelectedIndexChanged(sender, e);
            _searchBtn.Enabled = false;
            _searchResultsLV.Items.Add(new ListViewItem { Text = _strSearching.Text });
        }

        private void HandleSearchResult(IList<IHostedRepository> repos)
        {
            _searchResultsLV.Items.Clear();
            foreach (var repo in repos)
            {
                var lvi = new ListViewItem { Tag = repo, Text = repo.Name };
                lvi.SubItems.Add(repo.Owner);
                lvi.SubItems.Add(repo.Forks.ToString());
                lvi.SubItems.Add(repo.IsAFork ? _strYes.Text : _strNo.Text);
                _searchResultsLV.Items.Add(lvi);
            }
            _searchBtn.Enabled = true;
        }


        private void _forkBtn_Click(object sender, EventArgs e)
        {
            if (_searchResultsLV.SelectedItems.Count != 1)
            {
                MessageBox.Show(this, _strSelectOneItem.Text, _strError.Text);
                return;
            }

            var hostedRepo = _searchResultsLV.SelectedItems[0].Tag as IHostedRepository;
            try
            {
                if (hostedRepo != null) hostedRepo.Fork();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _strFailedToFork.Text + ex.Message, _strError.Text);
            }

            _tabControl.SelectedTab = _myReposPage;
            UpdateMyRepos();
        }

        private void _searchTB_Enter(object sender, EventArgs e)
        {
            AcceptButton = _searchBtn;
        }

        private void _searchTB_Leave(object sender, EventArgs e)
        {
            AcceptButton = null;
        }

        private void _searchResultsLV_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCloneInfo();
            if (_searchResultsLV.SelectedItems.Count != 1)
            {
                _forkBtn.Enabled = false;
                return;
            }

            _forkBtn.Enabled = true;
            var hostedRepo = (IHostedRepository)_searchResultsLV.SelectedItems[0].Tag;
            _searchResultItemDescription.Text = hostedRepo.Description;
        }

        private void _browseForCloneToDirbtn_Click(object sender, EventArgs e)
        {
            var initialDir = _destinationTB.Text.Length > 0 ? _destinationTB.Text : "C:\\";

            var browseDialog = new FolderBrowserDialog { SelectedPath = initialDir };

            if (browseDialog.ShowDialog(this) == DialogResult.OK)
            {
                _destinationTB.Text = browseDialog.SelectedPath;
                _destinationTB_TextChanged(sender, e);
            }
        }

        private void _cloneBtn_Click(object sender, EventArgs e)
        {
            Clone(CurrentySelectedGitRepo);
        }

        private void _openGitupPageBtn_Click(object sender, EventArgs e)
        {
            if (CurrentySelectedGitRepo == null)
                return;
            string hp = CurrentySelectedGitRepo.Homepage;
            if (string.IsNullOrEmpty(hp) || (!hp.StartsWith("http://") && !hp.StartsWith("https://")))
                MessageBox.Show(this, _strNoHomepageDefined.Text, _strError.Text);
            else
                Process.Start(CurrentySelectedGitRepo.Homepage);
        }

        private void _closeBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void _tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCloneInfo();
            if (_tabControl.SelectedTab == _searchReposPage)
                _searchTB.Focus();
        }

        private void _myReposLV_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCloneInfo();
        }

        private void _createDirTB_TextChanged(object sender, EventArgs e)
        {
            UpdateCloneInfo(false);
        }

        private void _destinationTB_TextChanged(object sender, EventArgs e)
        {
            UpdateCloneInfo(false);
        }

        private void _addRemoteAsTB_TextChanged(object sender, EventArgs e)
        {
            UpdateCloneInfo(false);
        }
        #endregion

        private void Clone(IHostedRepository repo)
        {
            string targetDir = GetTargetDir();
            if (targetDir == null)
                return;

            string repoSrc = repo.CloneReadWriteUrl;

            string cmd = GitCommandHelpers.CloneCmd(repoSrc, targetDir, false, null);
            var formProcess = new FormProcess(Settings.GitCommand, cmd);
            formProcess.ShowDialog();

            if (formProcess.ErrorOccurred())
                return;

            Repositories.RepositoryHistory.AddMostRecentRepository(targetDir);
            Settings.WorkingDir = targetDir;

            if (_addRemoteAsTB.Text.Trim().Length > 0)
            {
                var error = GitCommandHelpers.AddRemote(_addRemoteAsTB.Text.Trim(), repo.ParentReadOnlyUrl);
                if (!string.IsNullOrEmpty(error))
                    MessageBox.Show(this, error, _strCouldNotAddRemote.Text);
            }

            Close();
        }

        private IHostedRepository CurrentySelectedGitRepo
        {
            get
            {
                if (_tabControl.SelectedTab == _searchReposPage)
                {
                    if (_searchResultsLV.SelectedItems.Count != 1)
                        return null;

                    return (IHostedRepository)_searchResultsLV.SelectedItems[0].Tag;
                }
                if (_myReposLV.SelectedItems.Count != 1)
                    return null;

                return (IHostedRepository)_myReposLV.SelectedItems[0].Tag;
            }
        }

        private void UpdateCloneInfo()
        {
            UpdateCloneInfo(true);
        }

        private void UpdateCloneInfo(bool updateCreateDirTB)
        {
            var repo = CurrentySelectedGitRepo;

            if (repo != null)
            {
                if (updateCreateDirTB)
                {
                    _createDirTB.Text = repo.Name;
                    _addRemoteAsTB.Text = repo.ParentOwner ?? "";
                }

                _cloneBtn.Enabled = true;
                var moreInfo = !string.IsNullOrEmpty(_addRemoteAsTB.Text) ? string.Format(_strWillBeAddedAsARemote.Text, _addRemoteAsTB.Text.Trim()) : "";

                if (_tabControl.SelectedTab == _searchReposPage)
                    _cloneInfoText.Text = string.Format(_strWillCloneInfo.Text, repo.CloneReadWriteUrl, GetTargetDir(), moreInfo);
                else if (_tabControl.SelectedTab == _myReposPage)
                    _cloneInfoText.Text = string.Format(_strWillCloneWithPushAccess.Text, repo.CloneReadWriteUrl, GetTargetDir(), moreInfo);
            }
            else
            {
                _cloneBtn.Enabled = false;
                _cloneInfoText.Text = "";
                _createDirTB.Text = "";
            }
        }

        private string GetTargetDir()
        {
            string targetDir = _destinationTB.Text.Trim();
            if (targetDir.Length == 0)
            {
                MessageBox.Show(this, _strCloneFolderCanNotBeEmpty.Text, _strError.Text);
                return null;
            }

            targetDir = Path.Combine(targetDir, _createDirTB.Text);
            return targetDir;
        }
    }
}
