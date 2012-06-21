using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitUIPluginInterfaces.RepositoryHosts;
using GitCommands;
using System.Text.RegularExpressions;
using ResourceManager.Translation;

namespace GitUI.RepoHosting
{
    public partial class ViewPullRequestsForm : GitExtensionsForm
    {
        #region Translation
        private readonly TranslationString _strFailedToFetchPullData = new TranslationString("Failed to fetch pull data!\r\n");
        private readonly TranslationString _strFailedToLoadDiscussionItem = new TranslationString("Failed to post discussion item!\r\n");
        private readonly TranslationString _strFailedToClosePullRequest = new TranslationString("Failed to close pull request!\r\n");
        private readonly TranslationString _strFailedToLoadDiffData = new TranslationString("Failed to load diff data!\r\n");
        private readonly TranslationString _strCouldNotLoadDiscussion = new TranslationString("Could not load discussion!\r\n");
        private readonly TranslationString _strError = new TranslationString("Error");
        private readonly TranslationString _strLoading = new TranslationString(" : LOADING : ");
        private readonly TranslationString _strUnableUnderstandPatch = new TranslationString("Error: Unable to understand patch");
        private readonly TranslationString _strRemoteAlreadyExist = new TranslationString("ERROR: Remote with name {0} already exists but it does not point to the same repository!\r\nDetails: Is {1} expected {2}");
        private readonly TranslationString _strCouldNotAddRemote = new TranslationString("Could not add remote with name {0} and URL {1}");
        #endregion

        private readonly IRepositoryHostPlugin _gitHoster;
        private bool _isFirstLoad;

        // for translation only
        internal ViewPullRequestsForm()
        {
            InitializeComponent();
            Translate();
        }

        public ViewPullRequestsForm(IRepositoryHostPlugin gitHoster)
            : this()
        {
            _gitHoster = gitHoster;
        }

        List<IHostedRemote> _hostedRemotes;
        List<IPullRequestInformation> _pullRequestsInfo;
        IPullRequestInformation _currentPullRequestInfo;

        private void ViewPullRequestsForm_Load(object sender, EventArgs e)
        {
            _fileStatusList.SelectedIndexChanged += _fileStatusList_SelectedIndexChanged;
            _discussionWB.DocumentCompleted += _discussionWB_DocumentCompleted;

            Init();
        }

        private void Init()
        {
            _isFirstLoad = true;

            AsyncHelpers.DoAsync(
                () =>
                {
                    var t = _gitHoster.GetHostedRemotesForCurrentWorkingDirRepo().ToList();
                    foreach (var el in t)
                        el.GetHostedRepository(); // We do this now because we want to do it in the async part.
                    return t;
                },
                hostedRemotes =>
                {
                    _hostedRemotes = hostedRemotes;
                    _selectHostedRepoCB.Items.Clear();
                    foreach (var hostedRepo in _hostedRemotes)
                        _selectHostedRepoCB.Items.Add(hostedRepo.GetHostedRepository());

                    SelectNextHostedRepository();
                },
                ex => MessageBox.Show(this, ex.Message, _strError.Text, MessageBoxButtons.OK, MessageBoxIcon.Error));
        }

        private void _selectedOwner_SelectedIndexChanged(object sender, EventArgs e)
        {
            var hostedRepo = _selectHostedRepoCB.SelectedItem as IHostedRepository;
            if (hostedRepo == null)
                return;
            _selectHostedRepoCB.Enabled = false;
            ResetAllAndShowLoadingPullRequests();

            AsyncHelpers.DoAsync <List<IPullRequestInformation>>(
               hostedRepo.GetPullRequests,
               res => { SetPullRequestsData(res); _selectHostedRepoCB.Enabled = true; },
               ex => MessageBox.Show(this, _strFailedToFetchPullData.Text + ex.Message, _strError.Text)
            );
        }

        private void SetPullRequestsData(List<IPullRequestInformation> infos)
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
                return;

            LoadListView();
        }

        private void SelectNextHostedRepository()
        {
            if (_selectHostedRepoCB.Items.Count == 0)
                return;

            int i = _selectHostedRepoCB.SelectedIndex + 1;
            if (i >= _selectHostedRepoCB.Items.Count)
                i = 0;
            _selectHostedRepoCB.SelectedIndex = i;
            _selectedOwner_SelectedIndexChanged(null, null);
        }

        private void ResetAllAndShowLoadingPullRequests()
        {
            _discussionWB.DocumentText = "";
            _diffViewer.ViewPatch("");
            _fileStatusList.GitItemStatuses = new List<GitItemStatus>();

            _pullRequestsList.Items.Clear();
            var lvi = new ListViewItem("");
            lvi.SubItems.Add(_strLoading.Text);
            _pullRequestsList.Items.Add(lvi);
        }

        private void LoadListView()
        {
            foreach (var info in _pullRequestsInfo)
            {
                var lvi = new ListViewItem
                              {
                                  Text = info.Id,
                                  Tag = info
                              };
                lvi.SubItems.Add(info.Title);
                lvi.SubItems.Add(info.Owner);
                lvi.SubItems.Add(info.Created.ToString());
                _pullRequestsList.Items.Add(lvi);
            }
            if (_pullRequestsList.Items.Count > 0)
                _pullRequestsList.Items[0].Selected = true;
        }

        private void _pullRequestsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var prevPri = _currentPullRequestInfo;

            if (_pullRequestsList.SelectedItems.Count != 1)
            {
                _currentPullRequestInfo = null;
                _discussionWB.DocumentText = "";
                _diffViewer.ViewText("", "");
                return;
            }

            _currentPullRequestInfo = _pullRequestsList.SelectedItems[0].Tag as IPullRequestInformation;
            if (prevPri != null && prevPri.Equals(_currentPullRequestInfo))
                return;

            if (_currentPullRequestInfo == null)
                return;
            _discussionWB.DocumentText = DiscussionHtmlCreator.CreateFor(_currentPullRequestInfo);
            _diffViewer.ViewPatch("");
            _fileStatusList.GitItemStatuses = new List<GitItemStatus>();

            LoadDiffPatch();
            LoadDiscussion();
        }

        private void LoadDiscussion()
        {
            AsyncHelpers.DoAsync(
                () => _currentPullRequestInfo.Discussion,
                LoadDiscussion,
                ex => 
                { 
                    MessageBox.Show(this, _strCouldNotLoadDiscussion.Text + ex.Message, _strError.Text);
                    LoadDiscussion(null);
                });
        }

        private void LoadDiscussion(IPullRequestDiscussion discussion)
        {
            var t = DiscussionHtmlCreator.CreateFor(_currentPullRequestInfo, discussion != null ? discussion.Entries : null);
            _discussionWB.DocumentText = t;
        }

        void _discussionWB_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (_discussionWB.Document != null)
            {
                if (_discussionWB.Document.Window != null && _discussionWB.Document.Body != null)
                    _discussionWB.Document.Window.ScrollTo(0, _discussionWB.Document.Body.ScrollRectangle.Height);
            }
        }

        private void LoadDiffPatch()
        {
            AsyncHelpers.DoAsync(
                () => _currentPullRequestInfo.DiffData,
                SplitAndLoadDiff,
                ex => MessageBox.Show(this, _strFailedToLoadDiffData.Text + ex.Message, _strError.Text));
        }

        Dictionary<string, string> _diffCache;
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
                    MessageBox.Show(this, _strUnableUnderstandPatch.Text, _strError.Text);
                    return;
                }

                var gis = new GitItemStatus
                              {
                                  IsChanged = true,
                                  IsNew = false,
                                  IsDeleted = false,
                                  IsTracked = true,
                                  Name = match.Groups[2].Value.Trim()
                              };

                giss.Add(gis);
                _diffCache.Add(gis.Name, match.Groups[3].Value);
            }

            _fileStatusList.GitItemStatuses = giss;
        }

        private void _fetchBtn_Click(object sender, EventArgs e)
        {
            if (_currentPullRequestInfo == null)
                return;

            var localBranchName = string.Format("pr/n{0}_{1}", _currentPullRequestInfo.Id, _currentPullRequestInfo.Owner);

            var cmd = string.Format("fetch --no-tags --progress {0} {1}:{2}", _currentPullRequestInfo.HeadRepo.CloneReadOnlyUrl, _currentPullRequestInfo.HeadRef, localBranchName);
            var formProcess = new FormProcess(Settings.GitCommand, cmd);
            formProcess.ShowDialog(this);

            if (formProcess.ErrorOccurred())
                return;
            Close();
        }

        private void _addAsRemoteAndFetch_Click(object sender, EventArgs e)
        {
            if (_currentPullRequestInfo == null)
                return;

            var remoteName = _currentPullRequestInfo.Owner;
            var remoteUrl = _currentPullRequestInfo.HeadRepo.CloneReadOnlyUrl;
            var remoteRef = _currentPullRequestInfo.HeadRef;

            var existingRepo = _hostedRemotes.FirstOrDefault(el => el.Name == remoteName);
            if (existingRepo != null)
            {
                if (existingRepo.GetHostedRepository().CloneReadOnlyUrl != remoteUrl)
                {
                    MessageBox.Show(this, string.Format(_strRemoteAlreadyExist.Text,
                                        remoteName, existingRepo.GetHostedRepository().CloneReadOnlyUrl, remoteUrl));
                    return;
                }
            }
            else
            {
                var error = Settings.Module.AddRemote(remoteName, remoteUrl);
                if (!string.IsNullOrEmpty(error))
                {
                    MessageBox.Show(this, error, string.Format(_strCouldNotAddRemote.Text, remoteName, remoteUrl));
                    return;
                }
            }

            var cmd = string.Format("fetch --no-tags --progress {0} {1}:{0}/{1}", remoteName, remoteRef);
            var formProcess = new FormProcess(Settings.GitCommand, cmd);
            formProcess.ShowDialog(this);

            if (formProcess.ErrorOccurred())
                return;

            cmd = string.Format("checkout {0}/{1}", remoteName, remoteRef);
            formProcess = new FormProcess(Settings.GitCommand, cmd);
            formProcess.ShowDialog(this);

            Close();
        }

        void _fileStatusList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var gis = _fileStatusList.SelectedItem;
            if (gis == null)
                return;

            var data = _diffCache[gis.Name];
            _diffViewer.ViewPatch(data);
        }

        private void _closePullRequestBtn_Click(object sender, EventArgs e)
        {
            if (_currentPullRequestInfo == null)
                return;

            try
            {
                _currentPullRequestInfo.Close();
                _selectedOwner_SelectedIndexChanged(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _strFailedToClosePullRequest.Text + ex.Message, _strError.Text);
            }
        }

        private void _postComment_Click(object sender, EventArgs e)
        {
            string text = _postCommentText.Text;
            if (_currentPullRequestInfo == null || text == null || text.Trim().Length == 0)
                return;

            try
            {
                _currentPullRequestInfo.Discussion.Post(text);
                _postCommentText.Text = "";
                _currentPullRequestInfo.Discussion.ForceReload();
                LoadDiscussion(_currentPullRequestInfo.Discussion);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _strFailedToLoadDiscussionItem.Text + ex.Message, _strError.Text);
            }
        }

        private void _postCommentText_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && e.Control)
            {
                _postComment_Click(sender, e);
                e.Handled = true;
            }
        }

        private void _refreshCommentsBtn_Click(object sender, EventArgs e)
        {
            if (_currentPullRequestInfo == null)
                return;

            try
            {
                _currentPullRequestInfo.Discussion.ForceReload();
                LoadDiscussion(_currentPullRequestInfo.Discussion);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _strFailedToLoadDiscussionItem.Text + ex.Message, _strError.Text);
            }
        }
    }
}
