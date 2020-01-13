using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces.RepositoryHosts;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs.RepoHosting
{
    public partial class ViewPullRequestsForm : GitModuleForm
    {
        #region Translation
        private readonly TranslationString _strFailedToFetchPullData = new TranslationString("Failed to fetch pull data!");
        private readonly TranslationString _strFailedToLoadDiscussionItem = new TranslationString("Failed to post discussion item!");
        private readonly TranslationString _strFailedToClosePullRequest = new TranslationString("Failed to close pull request!");
        private readonly TranslationString _strFailedToLoadDiffData = new TranslationString("Failed to load diff data!");
        private readonly TranslationString _strCouldNotLoadDiscussion = new TranslationString("Could not load discussion!");
        private readonly TranslationString _strError = new TranslationString("Error");
        private readonly TranslationString _strLoading = new TranslationString(" : LOADING : ");
        private readonly TranslationString _strUnableUnderstandPatch = new TranslationString("Error: Unable to understand patch");
        private readonly TranslationString _strRemoteAlreadyExist = new TranslationString("ERROR: Remote with name {0} already exists but it points to a different repository!\r\nDetails: Is {1} expected {2}");
        private readonly TranslationString _strCouldNotAddRemote = new TranslationString("Could not add remote with name {0} and URL {1}");
        #endregion

        private GitProtocol _cloneGitProtocol;
        private IPullRequestInformation _currentPullRequestInfo;
        private Dictionary<string, string> _diffCache;
        private readonly IRepositoryHostPlugin _gitHoster;
        private IReadOnlyList<IHostedRemote> _hostedRemotes;
        private bool _isFirstLoad;
        private IReadOnlyList<IPullRequestInformation> _pullRequestsInfo;
        private readonly AsyncLoader _loader = new AsyncLoader();

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private ViewPullRequestsForm()
        {
            InitializeComponent();
        }

        private ViewPullRequestsForm(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            _selectHostedRepoCB.DisplayMember = nameof(IHostedRemote.DisplayData);
            _loader.LoadingError += (sender, ex) =>
                {
                    MessageBox.Show(this, ex.Exception.ToString(), _strError.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.UnMask();
                };
            InitializeComplete();
        }

        public ViewPullRequestsForm(GitUICommands commands, IRepositoryHostPlugin gitHoster)
            : this(commands)
        {
            _gitHoster = gitHoster;
        }

        private void ViewPullRequestsForm_Load(object sender, EventArgs e)
        {
            _fileStatusList.SelectedIndexChanged += _fileStatusList_SelectedIndexChanged;
            _discussionWB.DocumentCompleted += _discussionWB_DocumentCompleted;

            _isFirstLoad = true;

            this.Mask();
            _loader.LoadAsync(
                () =>
                {
                    var t = _gitHoster.GetHostedRemotesForModule().ToList();
                    foreach (var el in t)
                    {
                        el.GetHostedRepository(); // We do this now because we want to do it in the async part.
                    }

                    return t;
                },
                hostedRemotes =>
                {
                    _hostedRemotes = hostedRemotes;
                    _selectHostedRepoCB.Items.Clear();
                    foreach (var hostedRepo in _hostedRemotes)
                    {
                        _selectHostedRepoCB.Items.Add(hostedRepo);
                    }

                    SelectHostedRepositoryForCurrentRemote();
                    this.UnMask();
                });
        }

        private void _selectedOwner_SelectedIndexChanged(object sender, EventArgs e)
        {
            var hostedRemote = _selectHostedRepoCB.SelectedItem as IHostedRemote;

            var hostedRepo = hostedRemote?.GetHostedRepository();
            if (hostedRepo == null)
            {
                return;
            }

            _selectHostedRepoCB.Enabled = false;
            ResetAllAndShowLoadingPullRequests();

            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    try
                    {
                        await TaskScheduler.Default;

                        var pullRequests = hostedRepo.GetPullRequests();

                        await this.SwitchToMainThreadAsync();

                        SetPullRequestsData(pullRequests);
                        _selectHostedRepoCB.Enabled = true;
                    }
                    catch (Exception ex) when (!(ex is OperationCanceledException))
                    {
                        MessageBox.Show(this, _strFailedToFetchPullData.Text + Environment.NewLine + ex.Message, _strError.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                })
                .FileAndForget();
        }

        private void SetPullRequestsData(IReadOnlyList<IPullRequestInformation> infos)
        {
            if (_isFirstLoad)
            {
                _isFirstLoad = false;
                if (infos != null && infos.Count == 0 && _hostedRemotes.Count > 0)
                {
                    SelectNextHostedRepository();
                    return;
                }
            }

            _pullRequestsInfo = infos;
            _pullRequestsList.Items.Clear();

            if (_pullRequestsInfo == null)
            {
                return;
            }

            LoadListView();
        }

        private void SelectHostedRepositoryForCurrentRemote()
        {
            var currentRemote = Module.GetCurrentRemote();

            // Local branches have no current remote, return value is empty string.
            // In this case we fallback to the first remote in the list.
            // Currently, local git repo with no remote will show error message and can not open this dialog.
            // So there will always be at least 1 remote when this dialog is open
            _cloneGitProtocol = ThreadHelper.JoinableTaskFactory.Run(async () => (await Module.GetRemotesAsync())
                .First(r => string.IsNullOrEmpty(currentRemote) || r.Name == currentRemote).FetchUrl.IsUrlUsingHttp() ? GitProtocol.Https : GitProtocol.Ssh);
            var hostedRemote = _selectHostedRepoCB.Items.
                Cast<IHostedRemote>().
                FirstOrDefault(remote => remote.Name.Equals(currentRemote, StringComparison.OrdinalIgnoreCase));

            if (hostedRemote == null)
            {
                if (_selectHostedRepoCB.Items.Count > 0)
                {
                    _selectHostedRepoCB.SelectedIndex = 0;
                }
            }
            else
            {
                _selectHostedRepoCB.SelectedItem = hostedRemote;
            }
        }

        private void SelectNextHostedRepository()
        {
            if (_selectHostedRepoCB.Items.Count == 0)
            {
                return;
            }

            int i = _selectHostedRepoCB.SelectedIndex + 1;
            if (i >= _selectHostedRepoCB.Items.Count)
            {
                i = 0;
            }

            _selectHostedRepoCB.SelectedIndex = i;
            _selectedOwner_SelectedIndexChanged(null, null);
        }

        private void ResetAllAndShowLoadingPullRequests()
        {
            _discussionWB.DocumentText = "";
            _diffViewer.ViewPatch(null);
            _fileStatusList.SetDiffs();

            _pullRequestsList.Items.Clear();
            _pullRequestsList.Items.Add(new ListViewItem("") { SubItems = { _strLoading.Text } });
        }

        private void LoadListView()
        {
            foreach (var info in _pullRequestsInfo)
            {
                _pullRequestsList.Items.Add(new ListViewItem
                {
                    Text = info.Id,
                    Tag = info,
                    SubItems =
                    {
                        info.Title,
                        info.Owner,
                        info.Created.ToString(),
                        info.FetchBranch,
                    }
                });
            }

            ResizeColumnsToFitContent();

            if (_pullRequestsList.Items.Count > 0)
            {
                _pullRequestsList.Items[0].Selected = true;
            }
        }

        private void ResizeColumnsToFitContent()
        {
            var resizeStrategy = _pullRequestsList.Items.Count == 0 ? -2 : -1;

            foreach (ColumnHeader column in _pullRequestsList.Columns)
            {
                column.Width = resizeStrategy;
            }
        }

        private void _pullRequestsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var prevPri = _currentPullRequestInfo;

            if (_pullRequestsList.SelectedItems.Count != 1)
            {
                _currentPullRequestInfo = null;
                _discussionWB.DocumentText = "";
                ThreadHelper.JoinableTaskFactory.RunAsync(
                    () => _diffViewer.ViewTextAsync("", ""));
                return;
            }

            _currentPullRequestInfo = _pullRequestsList.SelectedItems[0].Tag as IPullRequestInformation;
            if (prevPri != null && prevPri.Equals(_currentPullRequestInfo))
            {
                return;
            }

            if (_currentPullRequestInfo == null)
            {
                return;
            }

            _currentPullRequestInfo.HeadRepo.CloneProtocol = _cloneGitProtocol;

            _discussionWB.DocumentText = DiscussionHtmlCreator.CreateFor(_currentPullRequestInfo);
            _diffViewer.ViewPatch(null);
            _fileStatusList.SetDiffs();

            LoadDiffPatch();
            LoadDiscussion();
        }

        private void LoadDiscussion()
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    try
                    {
                        await TaskScheduler.Default;

                        // TODO make this operation async (requires change to Git.hub submodule)
                        var discussion = _currentPullRequestInfo.GetDiscussion();

                        await this.SwitchToMainThreadAsync();

                        LoadDiscussion(discussion);
                    }
                    catch (Exception ex) when (!(ex is OperationCanceledException))
                    {
                        MessageBox.Show(this, _strCouldNotLoadDiscussion.Text + Environment.NewLine + ex.Message, _strError.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LoadDiscussion(null);
                    }
                })
                .FileAndForget();
        }

        private void LoadDiscussion([CanBeNull] IPullRequestDiscussion discussion)
        {
            var t = DiscussionHtmlCreator.CreateFor(_currentPullRequestInfo, discussion?.Entries);
            _discussionWB.DocumentText = t;
        }

        private void _discussionWB_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (_discussionWB.Document != null)
            {
                if (_discussionWB.Document.Window != null && _discussionWB.Document.Body != null)
                {
                    _discussionWB.Document.Window.ScrollTo(0, _discussionWB.Document.Body.ScrollRectangle.Height);
                }
            }
        }

        private void LoadDiffPatch()
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    try
                    {
                        var content = await _currentPullRequestInfo.GetDiffDataAsync();

                        await this.SwitchToMainThreadAsync();

                        SplitAndLoadDiff(content);
                    }
                    catch (Exception ex) when (!(ex is OperationCanceledException))
                    {
                        MessageBox.Show(this, _strFailedToLoadDiffData.Text + Environment.NewLine + ex.Message, _strError.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                })
                .FileAndForget();
        }

        private void SplitAndLoadDiff(string diffData)
        {
            _diffCache = new Dictionary<string, string>();

            var fileParts = Regex.Split(diffData, @"(?:\n|^)diff --git ").Where(el => el != null && el.Trim().Length > 10).ToList();
            var giss = new List<GitItemStatus>();

            foreach (var part in fileParts)
            {
                var match = Regex.Match(part, @"^a/([^\n]+) b/([^\n]+)\s*(.*)$", RegexOptions.Singleline);
                if (!match.Success)
                {
                    MessageBox.Show(this, _strUnableUnderstandPatch.Text, _strError.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var gis = new GitItemStatus
                {
                    IsChanged = true,
                    IsNew = false,
                    IsDeleted = false,
                    IsTracked = true,
                    Name = match.Groups[2].Value.Trim(),
                    Staged = StagedStatus.None
                };

                giss.Add(gis);
                _diffCache.Add(gis.Name, match.Groups[3].Value);
            }

            _fileStatusList.SetDiffs(items: giss);
        }

        private void _fetchBtn_Click(object sender, EventArgs e)
        {
            if (_currentPullRequestInfo == null)
            {
                return;
            }

            var cmd = string.Format("fetch --no-tags --progress {0} {1}:{2}",
                _currentPullRequestInfo.HeadRepo.CloneReadOnlyUrl, _currentPullRequestInfo.HeadRef, _currentPullRequestInfo.FetchBranch);
            var errorOccurred = !FormProcess.ShowDialog(this, AppSettings.GitCommand, cmd);

            if (errorOccurred)
            {
                return;
            }

            UICommands.RepoChangedNotifier.Notify();

            Close();
        }

        private void _addAsRemoteAndFetch_Click(object sender, EventArgs e)
        {
            if (_currentPullRequestInfo == null)
            {
                return;
            }

            UICommands.RepoChangedNotifier.Lock();
            try
            {
                var remoteName = _currentPullRequestInfo.Owner;
                var remoteUrl = _currentPullRequestInfo.HeadRepo.CloneReadOnlyUrl;
                var remoteRef = _currentPullRequestInfo.HeadRef;

                var existingRepo = _hostedRemotes.FirstOrDefault(el => el.Name == remoteName);
                if (existingRepo != null)
                {
                    var hostedRepository = existingRepo.GetHostedRepository();
                    hostedRepository.CloneProtocol = _cloneGitProtocol;
                    if (hostedRepository.CloneReadOnlyUrl != remoteUrl)
                    {
                        MessageBox.Show(this, string.Format(_strRemoteAlreadyExist.Text,
                                            remoteName, hostedRepository.CloneReadOnlyUrl, remoteUrl),
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                else
                {
                    var error = Module.AddRemote(remoteName, remoteUrl);
                    if (!string.IsNullOrEmpty(error))
                    {
                        MessageBox.Show(this, error, string.Format(_strCouldNotAddRemote.Text, remoteName, remoteUrl), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    UICommands.RepoChangedNotifier.Notify();
                }

                var cmd = string.Format("fetch --no-tags --progress {0} {1}:{0}/{1}", remoteName, remoteRef);
                var errorOccurred = !FormProcess.ShowDialog(this, AppSettings.GitCommand, cmd);

                if (errorOccurred)
                {
                    return;
                }

                UICommands.RepoChangedNotifier.Notify();

                cmd = string.Format("checkout {0}/{1}", remoteName, remoteRef);
                if (FormProcess.ShowDialog(this, AppSettings.GitCommand, cmd))
                {
                    UICommands.RepoChangedNotifier.Notify();
                }
            }
            finally
            {
                UICommands.RepoChangedNotifier.UnLock(false);
            }

            Close();
        }

        private void _fileStatusList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var gis = _fileStatusList.SelectedItem;
            if (gis == null)
            {
                return;
            }

            var data = _diffCache[gis.Name];
            _diffViewer.ViewPatch(text: data, openWithDifftool: null /* not implemented */);
        }

        private void _closePullRequestBtn_Click(object sender, EventArgs e)
        {
            if (_currentPullRequestInfo == null)
            {
                return;
            }

            try
            {
                _currentPullRequestInfo.Close();
                _selectedOwner_SelectedIndexChanged(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _strFailedToClosePullRequest.Text + Environment.NewLine + ex.Message, _strError.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _loader.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
