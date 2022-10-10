using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Commands;
using GitCommands.UserRepositoryHistory;
using GitExtUtils.GitUI;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces.RepositoryHosts;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs.RepoHosting
{
    public partial class ForkAndCloneForm : GitExtensionsForm
    {
        #region Translation

        private readonly TranslationString _strLoading = new(" : LOADING : ");
        private readonly TranslationString _strFailedToGetRepos = new("Failed to get repositories. This most likely means you didn't configure {0}, please do so via the menu \"Plugins/{0}\".");
        private readonly TranslationString _strWillCloneWithPushAccess = new("Will clone {0} into {1}.\r\nYou will have push access. {2}");
        private readonly TranslationString _strWillCloneInfo = new("Will clone {0} into {1}.\r\nYou can not push unless you are a collaborator. {2}");
        private readonly TranslationString _strWillBeAddedAsARemote = new("\"{0}\" will be added as a remote.");
        private readonly TranslationString _strCouldNotAddRemote = new("Could not add remote");
        private readonly TranslationString _strNoHomepageDefined = new("No homepage defined");
        private readonly TranslationString _strFailedToFork = new("Failed to fork:");
        private readonly TranslationString _strSearchFailed = new("Search failed!");
        private readonly TranslationString _strUserNotFound = new("User not found!");
        private readonly TranslationString _strCouldNotFetchReposOfUser = new("Could not fetch repositories of user!");
        private readonly TranslationString _strSearching = new(" : SEARCHING : ");
        private readonly TranslationString _strSelectOneItem = new("You must select exactly one item");
        private readonly TranslationString _strCloneFolderCanNotBeEmpty = new("Clone folder can not be empty");

        #endregion

        private const string UpstreamRemoteName = "upstream";
        private readonly IRepositoryHostPlugin _gitHoster;
        private readonly EventHandler<GitModuleEventArgs>? _gitModuleChanged;

        public ForkAndCloneForm(IRepositoryHostPlugin gitHoster, EventHandler<GitModuleEventArgs>? gitModuleChanged)
        {
            _gitModuleChanged = gitModuleChanged;
            _gitHoster = gitHoster;
            InitializeComponent();
            InitializeComplete();

            foreach (ColumnHeader column in myReposLV.Columns)
            {
                column.Width = DpiUtil.Scale(column.Width);
            }

            foreach (ColumnHeader column in searchResultsLV.Columns)
            {
                column.Width = DpiUtil.Scale(column.Width);
            }
        }

        private void ForkAndCloneForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void Init()
        {
            if (!string.IsNullOrEmpty(AppSettings.DefaultCloneDestinationPath))
            {
                destinationTB.Text = AppSettings.DefaultCloneDestinationPath;
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
                        destinationTB.Text = Path.GetDirectoryName(p);
                    }
                });
            }

            Text = $"{_gitHoster.Name}: {Text}";

            UpdateCloneInfo();
            UpdateMyRepos();
        }

        private void UpdateMyRepos()
        {
            myReposLV.Items.Clear();
            myReposLV.Items.Add(new ListViewItem { Text = _strLoading.Text });

            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    try
                    {
                        await TaskScheduler.Default;

                        var repos = _gitHoster.GetMyRepos();

                        await this.SwitchToMainThreadAsync();

                        myReposLV.Items.Clear();

                        foreach (var repo in repos.OrderBy(r => r.Name))
                        {
                            myReposLV.Items.Add(new ListViewItem
                            {
                                Tag = repo,
                                Text = repo.Name,
                                SubItems =
                                {
                                    repo.IsAFork ? TranslatedStrings.Yes : TranslatedStrings.No,
                                    repo.Forks.ToString(),
                                    repo.IsPrivate ? TranslatedStrings.Yes : TranslatedStrings.No
                                }
                            });
                        }

                        ResizeColumnToFitContent(myReposLV.Columns[0]);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        await this.SwitchToMainThreadAsync();

                        myReposLV.Items.Clear();
                        helpTextLbl.Text = string.Format(_strFailedToGetRepos.Text, _gitHoster.Name) +
                                            "\r\n\r\nException: " + ex.Message + "\r\n\r\n" + helpTextLbl.Text;
                    }
                })
                .FileAndForget();
        }

        private const int ResizeOnContent = -1;
        private const int ResizeOnHeader = -2;

        private void ResizeColumnToFitContent(ColumnHeader column)
        {
            var resizeStrategy = column.ListView.Items.Count == 0 ? ResizeOnHeader : ResizeOnContent;
            column.Width = resizeStrategy;
        }

        #region GUI Handlers

        private void _searchBtn_Click(object sender, EventArgs e)
        {
            var search = searchTB.Text;
            if (search is null || search.Trim().Length == 0)
            {
                return;
            }

            PrepareSearch(sender, e);

            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    try
                    {
                        await TaskScheduler.Default;

                        var repositories = _gitHoster.SearchForRepository(search);

                        await this.SwitchToMainThreadAsync();

                        HandleSearchResult(repositories);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        await this.SwitchToMainThreadAsync();

                        MessageBox.Show(this, _strSearchFailed.Text + Environment.NewLine + ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        searchBtn.Enabled = true;
                    }
                })
                .FileAndForget();
        }

        private void _getFromUserBtn_Click(object sender, EventArgs e)
        {
            var search = searchTB.Text;
            if (search is null || search.Trim().Length == 0)
            {
                return;
            }

            PrepareSearch(sender, e);

            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    try
                    {
                        await TaskScheduler.Default;

                        var repositories = _gitHoster.GetRepositoriesOfUser(search.Trim());

                        await this.SwitchToMainThreadAsync();

                        HandleSearchResult(repositories);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        await this.SwitchToMainThreadAsync();

                        if (ex.Message.Contains("404"))
                        {
                            MessageBox.Show(this, _strUserNotFound.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show(this, _strCouldNotFetchReposOfUser.Text + Environment.NewLine +
                                                  ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        searchBtn.Enabled = true;
                    }
                })
                .FileAndForget();
        }

        private void PrepareSearch(object sender, EventArgs e)
        {
            searchResultsLV.Items.Clear();
            _searchResultsLV_SelectedIndexChanged(sender, e);
            searchBtn.Enabled = false;
            searchResultsLV.Items.Add(new ListViewItem { Text = _strSearching.Text });
        }

        private void HandleSearchResult(IReadOnlyList<IHostedRepository> repos)
        {
            searchResultsLV.Items.Clear();

            foreach (var repo in repos.OrderBy(r => r.Name))
            {
                searchResultsLV.Items.Add(new ListViewItem
                {
                    Tag = repo,
                    Text = repo.Name,
                    SubItems =
                    {
                        repo.Owner,
                        repo.IsAFork ? TranslatedStrings.Yes : TranslatedStrings.No,
                        repo.Forks.ToString()
                    }
                });
            }

            ResizeColumnToFitContent(searchResultsLV.Columns[0]);
            ResizeColumnToFitContent(searchResultsLV.Columns[1]);

            searchBtn.Enabled = true;
        }

        private void _forkBtn_Click(object sender, EventArgs e)
        {
            if (searchResultsLV.SelectedItems.Count != 1)
            {
                MessageBox.Show(this, _strSelectOneItem.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var hostedRepo = searchResultsLV.SelectedItems[0].Tag as IHostedRepository;
            try
            {
                hostedRepo?.Fork();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _strFailedToFork.Text + Environment.NewLine + ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            tabControl.SelectedTab = myReposPage;
            UpdateMyRepos();
        }

        private void _searchTB_Enter(object sender, EventArgs e)
        {
            AcceptButton = searchBtn;
        }

        private void _searchTB_Leave(object sender, EventArgs e)
        {
            AcceptButton = null;
        }

        private void _searchResultsLV_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCloneInfo();
            if (searchResultsLV.SelectedItems.Count != 1)
            {
                forkBtn.Enabled = false;
                return;
            }

            forkBtn.Enabled = true;
            var hostedRepo = (IHostedRepository)searchResultsLV.SelectedItems[0].Tag;
            searchResultItemDescription.Text = hostedRepo.Description;
        }

        private void _browseForCloneToDirbtn_Click(object sender, EventArgs e)
        {
            var initialDir = destinationTB.Text.Length > 0 ? destinationTB.Text : "C:\\";

            var userSelectedPath = OsShellUtil.PickFolder(this, initialDir);

            if (userSelectedPath is not null)
            {
                destinationTB.Text = userSelectedPath;
                _destinationTB_TextChanged(sender, e);
            }
        }

        private void _cloneBtn_Click(object sender, EventArgs e)
        {
            var repo = CurrentySelectedGitRepo;

            if (repo is not null)
            {
                Clone(repo);
            }
        }

        private void _openGitupPageBtn_Click(object sender, EventArgs e)
        {
            if (CurrentySelectedGitRepo is null)
            {
                return;
            }

            string hp = CurrentySelectedGitRepo.Homepage;
            if (string.IsNullOrEmpty(hp) || (!hp.StartsWith("http://") && !hp.StartsWith("https://")))
            {
                MessageBox.Show(this, _strNoHomepageDefined.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                OsShellUtil.OpenUrlInDefaultBrowser(CurrentySelectedGitRepo.Homepage);
            }
        }

        private void _closeBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void _tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCloneInfo();
            if (tabControl.SelectedTab == searchReposPage)
            {
                searchTB.Focus();
            }
        }

        private void _myReposLV_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateCloneInfo();
        }

        private void _createDirTB_TextChanged(object sender, EventArgs e)
        {
            UpdateCloneInfo(false, false);
        }

        private void _destinationTB_TextChanged(object sender, EventArgs e)
        {
            UpdateCloneInfo(false, false);
        }

        private void _addRemoteAsTB_TextChanged(object sender, EventArgs e)
        {
            UpdateCloneInfo(false, false);
        }
        #endregion

        private void Clone(IHostedRepository repo)
        {
            string? targetDir = GetTargetDir();
            if (targetDir is null)
            {
                return;
            }

            GitUICommands uiCommands = new(new GitModule(null));

            string repoSrc = PathUtil.IsLocalFile(repo.CloneReadWriteUrl)
                ? uiCommands.Module.GetGitExecPath(repo.CloneReadWriteUrl)
                : repo.CloneReadWriteUrl;

            var cmd = GitCommandHelpers.CloneCmd(repoSrc, uiCommands.Module.GetGitExecPath(targetDir), depth: GetDepth());

            FormRemoteProcess formRemoteProcess = new(uiCommands, cmd)
            {
                Remote = repoSrc
            };

            formRemoteProcess.ShowDialog();

            if (formRemoteProcess.ErrorOccurred())
            {
                return;
            }

            GitModule module = new(targetDir);

            if (addUpstreamRemoteAsCB.Text.Trim().Length > 0 && !string.IsNullOrEmpty(repo.ParentReadOnlyUrl))
            {
                var error = module.AddRemote(addUpstreamRemoteAsCB.Text.Trim(), repo.ParentReadOnlyUrl);
                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show(this, error, _strCouldNotAddRemote.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            _gitModuleChanged?.Invoke(this, new GitModuleEventArgs(module));

            Close();
        }

        private IHostedRepository? CurrentySelectedGitRepo
        {
            get
            {
                if (tabControl.SelectedTab == searchReposPage)
                {
                    if (searchResultsLV.SelectedItems.Count != 1)
                    {
                        return null;
                    }

                    return (IHostedRepository)searchResultsLV.SelectedItems[0].Tag;
                }

                if (myReposLV.SelectedItems.Count != 1)
                {
                    return null;
                }

                return (IHostedRepository)myReposLV.SelectedItems[0].Tag;
            }
        }

        private void UpdateCloneInfo(bool updateCreateDirTB = true, bool updateProtocols = true)
        {
            var repo = CurrentySelectedGitRepo;

            if (repo is not null)
            {
                bool multipleProtocols = repo.SupportedCloneProtocols.Any();

                if (multipleProtocols && updateProtocols)
                {
                    var currentSelection = (GitProtocol)(ProtocolDropdownList.SelectedItem ?? repo.SupportedCloneProtocols.First());
                    ProtocolDropdownList.DataSource = repo.SupportedCloneProtocols;
                    if (repo.SupportedCloneProtocols.Contains(currentSelection))
                    {
                        repo.CloneProtocol = currentSelection;
                    }

                    ProtocolDropdownList.SelectedItem = repo.CloneProtocol;
                }

                SetProtocolSelectionVisibility(multipleProtocols);

                if (updateCreateDirTB)
                {
                    createDirTB.Text = repo.Name;
                    addUpstreamRemoteAsCB.Text = "";
                    addUpstreamRemoteAsCB.Items.Clear();
                    if (repo.ParentOwner is not null)
                    {
                        var upstreamRemoteName = repo.ParentOwner ?? "";
                        addUpstreamRemoteAsCB.Items.Add(upstreamRemoteName);
                        addUpstreamRemoteAsCB.Items.Add(UpstreamRemoteName);
                        if (addUpstreamRemoteAsCB.Text != UpstreamRemoteName)
                        {
                            addUpstreamRemoteAsCB.Text = upstreamRemoteName;
                        }
                    }

                    addUpstreamRemoteAsCB.Enabled = repo.ParentOwner is not null;
                }

                cloneBtn.Enabled = true;
                SetCloneInfoText(repo);
            }
            else
            {
                SetProtocolSelectionVisibility(false);
                cloneBtn.Enabled = false;
                cloneInfoText.Text = "";
                createDirTB.Text = "";
            }
        }

        private void SetCloneInfoText(IHostedRepository repo)
        {
            var moreInfo = !string.IsNullOrEmpty(addUpstreamRemoteAsCB.Text) ? string.Format(_strWillBeAddedAsARemote.Text, addUpstreamRemoteAsCB.Text.Trim()) : "";

            if (tabControl.SelectedTab == searchReposPage)
            {
                cloneInfoText.Text = string.Format(_strWillCloneInfo.Text, repo.CloneReadWriteUrl, GetTargetDir(), moreInfo);
            }
            else if (tabControl.SelectedTab == myReposPage)
            {
                cloneInfoText.Text = string.Format(_strWillCloneWithPushAccess.Text, repo.CloneReadWriteUrl, GetTargetDir(), moreInfo);
            }
        }

        private void SetProtocolSelectionVisibility(bool multipleProtocols)
        {
            ProtocolLabel.Visible = multipleProtocols;
            ProtocolDropdownList.Visible = multipleProtocols;
        }

        private string? GetTargetDir()
        {
            string targetDir = destinationTB.Text.Trim();
            if (targetDir.Length == 0)
            {
                MessageBox.Show(this, _strCloneFolderCanNotBeEmpty.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            targetDir = Path.Combine(targetDir, createDirTB.Text);
            return targetDir;
        }

        private int? GetDepth()
        {
            if (depthUpDown.Value != 0)
            {
                return (int)depthUpDown.Value;
            }

            return null;
        }

        private void _destinationTB_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (destinationTB.Text.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                e.Cancel = true;
            }
        }

        private void _createDirTB_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (createDirTB.Text.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                e.Cancel = true;
            }
        }

        private void ProtocolSelectionChanged(object sender, EventArgs e)
        {
            Validates.NotNull(CurrentySelectedGitRepo);

            CurrentySelectedGitRepo.CloneProtocol = (GitProtocol)ProtocolDropdownList.SelectedItem;
            SetCloneInfoText(CurrentySelectedGitRepo);
        }
    }
}
