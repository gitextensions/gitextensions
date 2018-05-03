using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitUIPluginInterfaces.RepositoryHosts;
using ResourceManager;

namespace GitUI.CommandsDialogs.RepoHosting
{
    public partial class ForkAndCloneForm : GitExtensionsForm
    {
        #region Translation

        private readonly TranslationString _strError = new TranslationString("Error");
        private readonly TranslationString _strLoading = new TranslationString(" : LOADING : ");
        private readonly TranslationString _strYes = new TranslationString("Yes");
        private readonly TranslationString _strNo = new TranslationString("No");
        private readonly TranslationString _strFailedToGetRepos = new TranslationString("Failed to get repositories. This most likely means you didn't configure {0}, please do so via the menu \"Plugins/{0}\".");
        private readonly TranslationString _strWillCloneWithPushAccess = new TranslationString("Will clone {0} into {1}.\r\nYou will have push access. {2}");
        private readonly TranslationString _strWillCloneInfo = new TranslationString("Will clone {0} into {1}.\r\nYou can not push unless you are a collaborator. {2}");
        private readonly TranslationString _strWillBeAddedAsARemote = new TranslationString("\"{0}\" will be added as a remote.");
        private readonly TranslationString _strCouldNotAddRemote = new TranslationString("Could not add remote");
        private readonly TranslationString _strNoHomepageDefined = new TranslationString("No homepage defined");
        private readonly TranslationString _strFailedToFork = new TranslationString("Failed to fork:");
        private readonly TranslationString _strSearchFailed = new TranslationString("Search failed!");
        private readonly TranslationString _strUserNotFound = new TranslationString("User not found!");
        private readonly TranslationString _strCouldNotFetchReposOfUser = new TranslationString("Could not fetch repositories of user!");
        private readonly TranslationString _strSearching = new TranslationString(" : SEARCHING : ");
        private readonly TranslationString _strSelectOneItem = new TranslationString("You must select exactly one item");
        private readonly TranslationString _strCloneFolderCanNotBeEmpty = new TranslationString("Clone folder can not be empty");
        #endregion

        private readonly IRepositoryHostPlugin _gitHoster;
        private readonly EventHandler<GitModuleEventArgs> _gitModuleChanged;

        public ForkAndCloneForm(IRepositoryHostPlugin gitHoster, EventHandler<GitModuleEventArgs> gitModuleChanged)
        {
            _gitModuleChanged = gitModuleChanged;
            _gitHoster = gitHoster;
            InitializeComponent();
            Translate();
            this.AdjustForDpiScaling();
        }

        private void ForkAndCloneForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void Init()
        {
            if (!string.IsNullOrEmpty(AppSettings.DefaultCloneDestinationPath))
            {
                _destinationTB.Text = AppSettings.DefaultCloneDestinationPath;
            }
            else
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    var repositoryHistory = await RepositoryHistoryManager.Locals.LoadRecentHistoryAsync();

                    await this.SwitchToMainThreadAsync();
                    var lastRepo = repositoryHistory.FirstOrDefault();
                    if (!string.IsNullOrEmpty(lastRepo?.Path))
                    {
                        string p = lastRepo.Path.Trim('/', '\\');
                        _destinationTB.Text = Path.GetDirectoryName(p);
                    }
                });
            }

            Text = _gitHoster.Description + ": " + Text;

            UpdateCloneInfo();
            UpdateMyRepos();
        }

        private void UpdateMyRepos()
        {
            _myReposLV.Items.Clear();
            _myReposLV.Items.Add(new ListViewItem { Text = _strLoading.Text });

            AsyncLoader.DoAsync(
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
                    _myReposLV.Items.Clear();
                    _helpTextLbl.Text = string.Format(_strFailedToGetRepos.Text, _gitHoster.Description) +
                        "\r\n\r\nException: " + ex.Exception.Message + "\r\n\r\n" + _helpTextLbl.Text;
                });
        }

        #region GUI Handlers
        private void _searchBtn_Click(object sender, EventArgs e)
        {
            var search = _searchTB.Text;
            if (search == null || search.Trim().Length == 0)
            {
                return;
            }

            PrepareSearch(sender, e);

            AsyncLoader.DoAsync(
                () => _gitHoster.SearchForRepository(search),
                HandleSearchResult,
                ex =>
                {
                    MessageBox.Show(this, _strSearchFailed.Text + Environment.NewLine + ex.Exception.Message,
                        _strError.Text);
                    _searchBtn.Enabled = true;
                });
        }

        private void _getFromUserBtn_Click(object sender, EventArgs e)
        {
            var search = _searchTB.Text;
            if (search == null || search.Trim().Length == 0)
            {
                return;
            }

            PrepareSearch(sender, e);

            AsyncLoader.DoAsync(
                () => _gitHoster.GetRepositoriesOfUser(search.Trim()),
                HandleSearchResult,
                ex =>
                {
                    if (ex.Exception.Message.Contains("404"))
                    {
                        MessageBox.Show(this, _strUserNotFound.Text, _strError.Text);
                    }
                    else
                    {
                        MessageBox.Show(this, _strCouldNotFetchReposOfUser.Text + Environment.NewLine +
                            ex.Exception.Message, _strError.Text);
                    }

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

        private void HandleSearchResult(IReadOnlyList<IHostedRepository> repos)
        {
            _searchResultsLV.Items.Clear();

            foreach (var repo in repos)
            {
                _searchResultsLV.Items.Add(new ListViewItem
                {
                    Tag = repo,
                    Text = repo.Name,
                    SubItems =
                    {
                        repo.Owner,
                        repo.Forks.ToString(),
                        repo.IsAFork ? _strYes.Text : _strNo.Text
                    }
                });
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
                hostedRepo?.Fork();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _strFailedToFork.Text + Environment.NewLine + ex.Message, _strError.Text);
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

            var userSelectedPath = OsShellUtil.PickFolder(this, initialDir);

            if (userSelectedPath != null)
            {
                _destinationTB.Text = userSelectedPath;
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
            {
                return;
            }

            string hp = CurrentySelectedGitRepo.Homepage;
            if (string.IsNullOrEmpty(hp) || (!hp.StartsWith("http://") && !hp.StartsWith("https://")))
            {
                MessageBox.Show(this, _strNoHomepageDefined.Text, _strError.Text);
            }
            else
            {
                Process.Start(CurrentySelectedGitRepo.Homepage);
            }
        }

        private void _closeBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void _tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCloneInfo();
            if (_tabControl.SelectedTab == _searchReposPage)
            {
                _searchTB.Focus();
            }
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
            {
                return;
            }

            string repoSrc = repo.CloneReadWriteUrl;

            string cmd = GitCommandHelpers.CloneCmd(repoSrc, targetDir);

            var formRemoteProcess = new FormRemoteProcess(new GitModule(null), AppSettings.GitCommand, cmd)
            {
                Remote = repoSrc
            };

            formRemoteProcess.ShowDialog();

            if (formRemoteProcess.ErrorOccurred())
            {
                return;
            }

            GitModule module = new GitModule(targetDir);

            if (_addRemoteAsTB.Text.Trim().Length > 0 && !string.IsNullOrEmpty(repo.ParentReadOnlyUrl))
            {
                var error = module.AddRemote(_addRemoteAsTB.Text.Trim(), repo.ParentReadOnlyUrl);
                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show(this, error, _strCouldNotAddRemote.Text);
                }
            }

            _gitModuleChanged?.Invoke(this, new GitModuleEventArgs(module));

            Close();
        }

        private IHostedRepository CurrentySelectedGitRepo
        {
            get
            {
                if (_tabControl.SelectedTab == _searchReposPage)
                {
                    if (_searchResultsLV.SelectedItems.Count != 1)
                    {
                        return null;
                    }

                    return (IHostedRepository)_searchResultsLV.SelectedItems[0].Tag;
                }

                if (_myReposLV.SelectedItems.Count != 1)
                {
                    return null;
                }

                return (IHostedRepository)_myReposLV.SelectedItems[0].Tag;
            }
        }

        private void UpdateCloneInfo(bool updateCreateDirTB = true)
        {
            var repo = CurrentySelectedGitRepo;

            if (repo != null)
            {
                if (updateCreateDirTB)
                {
                    _createDirTB.Text = repo.Name;
                    _addRemoteAsTB.Text = repo.ParentOwner ?? "";
                    _addRemoteAsTB.Enabled = repo.ParentOwner != null;
                }

                _cloneBtn.Enabled = true;
                var moreInfo = !string.IsNullOrEmpty(_addRemoteAsTB.Text) ? string.Format(_strWillBeAddedAsARemote.Text, _addRemoteAsTB.Text.Trim()) : "";

                if (_tabControl.SelectedTab == _searchReposPage)
                {
                    _cloneInfoText.Text = string.Format(_strWillCloneInfo.Text, repo.CloneReadWriteUrl, GetTargetDir(), moreInfo);
                }
                else if (_tabControl.SelectedTab == _myReposPage)
                {
                    _cloneInfoText.Text = string.Format(_strWillCloneWithPushAccess.Text, repo.CloneReadWriteUrl, GetTargetDir(), moreInfo);
                }
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

        private void _destinationTB_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_destinationTB.Text.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                e.Cancel = true;
            }
        }

        private void _createDirTB_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_createDirTB.Text.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                e.Cancel = true;
            }
        }
    }
}
