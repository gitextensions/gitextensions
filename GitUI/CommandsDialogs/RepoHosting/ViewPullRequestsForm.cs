using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.RepositoryHosts;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs.RepoHosting
{
    public partial class ViewPullRequestsForm : GitModuleForm
    {
        #region Translation
        private readonly TranslationString _strFailedToFetchPullData = new("Failed to fetch pull data!");
        private readonly TranslationString _strFailedToLoadDiscussionItem = new("Failed to post discussion item!");
        private readonly TranslationString _strFailedToClosePullRequest = new("Failed to close pull request!");
        private readonly TranslationString _strFailedToLoadDiffData = new("Failed to load diff data!");
        private readonly TranslationString _strCouldNotLoadDiscussion = new("Could not load discussion!");
        private readonly TranslationString _strLoading = new(" : LOADING : ");
        private readonly TranslationString _strUnableUnderstandPatch = new("Error: Unable to understand patch");
        private readonly TranslationString _strRemoteAlreadyExist = new("ERROR: Remote with name {0} already exists but it points to a different repository!\r\nDetails: Is {1} expected {2}");
        private readonly TranslationString _strCouldNotAddRemote = new("Could not add remote with name {0} and URL {1}");
        private readonly TranslationString _strRemoteIgnore = new("Remote ignored");
        #endregion

        private GitProtocol _cloneGitProtocol;
        private IPullRequestInformation? _currentPullRequestInfo;
        private Dictionary<string, string>? _diffCache;
        private readonly IRepositoryHostPlugin _gitHoster;
        private IReadOnlyList<IHostedRemote>? _hostedRemotes;
        private bool _isFirstLoad;
        private IReadOnlyList<IPullRequestInformation>? _pullRequestsInfo;
        private readonly AsyncLoader _loader = new();

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private ViewPullRequestsForm()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        public ViewPullRequestsForm(GitUICommands commands, IRepositoryHostPlugin gitHoster)
            : base(commands)
        {
            _gitHoster = gitHoster;
            InitializeComponent();
            _selectHostedRepoCB.DisplayMember = nameof(IHostedRemote.DisplayData);
            _diffViewer.ExtraDiffArgumentsChanged += _fileStatusList_SelectedIndexChanged;
            _loader.LoadingError += (sender, ex) =>
            {
                MessageBox.Show(this, ex.Exception.ToString(), TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.UnMask();
            };
            _diffViewer.TopScrollReached += FileViewer_TopScrollReached;
            _diffViewer.BottomScrollReached += FileViewer_BottomScrollReached;
            InitializeComplete();
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
                    var hostedRemotes = _gitHoster.GetHostedRemotesForModule().ToArray();

                    // load all hosted repositories.
                    foreach (var hostedRemote in hostedRemotes)
                    {
                        try
                        {
                            hostedRemote.GetHostedRepository(); // We do this now because we want to do it in the async part.
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.ShowDialog(new TaskDialogPage
                            {
                                Icon = TaskDialogIcon.Error,
                                Caption = _strRemoteIgnore.Text,
                                Text = string.Format(TranslatedStrings.RemoteInError, ex.Message, hostedRemote.DisplayData),
                                Buttons = { TaskDialogButton.OK },
                                SizeToContent = true
                            });
                        }
                    }

                    return hostedRemotes;
                },
                hostedRemotes =>
                {
                    _hostedRemotes = hostedRemotes;
                    _selectHostedRepoCB.Items.Clear();
                    _selectHostedRepoCB.Items.AddRange(hostedRemotes);

                    SelectHostedRepositoryForCurrentRemote();
                    this.UnMask();
                });
        }

        private void _selectedOwner_SelectedIndexChanged(object sender, EventArgs e)
        {
            var hostedRemote = _selectHostedRepoCB.SelectedItem as IHostedRemote;

            _pullRequestsList.Items.Clear();
            IHostedRepository? hostedRepo;
            try
            {
                hostedRepo = hostedRemote?.GetHostedRepository();
            }
            catch (Exception)
            {
                // if fails to load this remote, select the next one
                SelectNextHostedRepositoryIfFirstLoad();
                return;
            }

            if (hostedRepo is null)
            {
                SelectNextHostedRepositoryIfFirstLoad();
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
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        MessageBox.Show(this, _strFailedToFetchPullData.Text + Environment.NewLine + ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                })
                .FileAndForget();

            void SelectNextHostedRepositoryIfFirstLoad()
            {
                if (_isFirstLoad)
                {
                    SelectNextHostedRepository();
                }
            }
        }

        private void FileViewer_TopScrollReached(object sender, EventArgs e)
        {
            _fileStatusList.SelectPreviousVisibleItem();
            _diffViewer.ScrollToBottom();
        }

        private void FileViewer_BottomScrollReached(object sender, EventArgs e)
        {
            _fileStatusList.SelectNextVisibleItem();
            _diffViewer.ScrollToTop();
        }

        private void SetPullRequestsData(IReadOnlyList<IPullRequestInformation>? infos)
        {
            if (_isFirstLoad)
            {
                if (infos is not null && infos.Count == 0 && _hostedRemotes is not null && _hostedRemotes.Count > 0)
                {
                    SelectNextHostedRepository();
                    return;
                }
                else
                {
                    _isFirstLoad = false;
                }
            }

            _pullRequestsInfo = infos;
            _pullRequestsList.Items.Clear();

            if (_pullRequestsInfo is null)
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
                FirstOrDefault(remote => string.Equals(remote.Name, currentRemote, StringComparison.OrdinalIgnoreCase));

            if (hostedRemote is null)
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
                return;
            }

            _selectHostedRepoCB.SelectedIndex = i;
            _selectedOwner_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void ResetAllAndShowLoadingPullRequests()
        {
            _discussionWB.DocumentText = "";
            _diffViewer.Clear();
            _fileStatusList.ClearDiffs();

            _pullRequestsList.Items.Clear();
            _pullRequestsList.Items.Add(new ListViewItem("") { SubItems = { _strLoading.Text } });
        }

        private void LoadListView()
        {
            Validates.NotNull(_pullRequestsInfo);
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
                _diffViewer.Clear();
                return;
            }

            _currentPullRequestInfo = _pullRequestsList.SelectedItems[0].Tag as IPullRequestInformation;
            if (prevPri is not null && prevPri.Equals(_currentPullRequestInfo))
            {
                return;
            }

            if (_currentPullRequestInfo is null)
            {
                return;
            }

            _currentPullRequestInfo.HeadRepo.CloneProtocol = _cloneGitProtocol;

            _discussionWB.DocumentText = DiscussionHtmlCreator.CreateFor(_currentPullRequestInfo);
            _diffViewer.Clear();
            _fileStatusList.ClearDiffs();

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
                        Validates.NotNull(_currentPullRequestInfo);
                        var discussion = _currentPullRequestInfo.GetDiscussion();

                        await this.SwitchToMainThreadAsync();

                        LoadDiscussion(discussion);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        MessageBox.Show(this, _strCouldNotLoadDiscussion.Text + Environment.NewLine + ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LoadDiscussion(null);
                    }
                })
                .FileAndForget();
        }

        private void LoadDiscussion(IPullRequestDiscussion? discussion)
        {
            Validates.NotNull(_currentPullRequestInfo);
            var t = DiscussionHtmlCreator.CreateFor(_currentPullRequestInfo, discussion?.Entries);
            _discussionWB.DocumentText = t;
        }

        private void _discussionWB_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (_discussionWB.Document?.Window is not null && _discussionWB.Document.Body is not null)
            {
                _discussionWB.Document.Window.ScrollTo(0, _discussionWB.Document.Body.ScrollRectangle.Height);
            }
        }

        private void LoadDiffPatch()
        {
            Validates.NotNull(_currentPullRequestInfo);
            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    try
                    {
                        var content = await _currentPullRequestInfo.GetDiffDataAsync();

                        await this.SwitchToMainThreadAsync();

                        SplitAndLoadDiff(content, _currentPullRequestInfo.BaseSha, _currentPullRequestInfo.HeadSha);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        MessageBox.Show(this, _strFailedToLoadDiffData.Text + Environment.NewLine + ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                })
                .FileAndForget();
        }

        private void SplitAndLoadDiff(string diffData, string baseSha, string secondSha)
        {
            _diffCache = new Dictionary<string, string>();

            var fileParts = Regex.Split(diffData, @"(?:\n|^)diff --git ").Where(el => el is not null && el.Trim().Length > 10).ToList();
            List<GitItemStatus> giss = new();

            // baseSha is the sha of the merge to ("master") sha, the commit to be firstId
            GitRevision? firstRev = ObjectId.TryParse(baseSha, out ObjectId? firstId) ? new GitRevision(firstId) : null;
            GitRevision? secondRev = ObjectId.TryParse(secondSha, out ObjectId? secondId) ? new GitRevision(secondId) : null;
            if (secondRev is null)
            {
                MessageBox.Show(this, _strUnableUnderstandPatch.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (var part in fileParts)
            {
                var match = Regex.Match(part, @"^a/([^\n]+) b/([^\n]+)\s*(.*)$", RegexOptions.Singleline);
                if (!match.Success)
                {
                    MessageBox.Show(this, _strUnableUnderstandPatch.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                GitItemStatus gis = new(name: match.Groups[2].Value.Trim())
                {
                    IsChanged = true,
                    IsNew = false,
                    IsDeleted = false,
                    IsTracked = true,
                    Staged = StagedStatus.None
                };

                giss.Add(gis);
                _diffCache.Add(gis.Name, match.Groups[3].Value);
            }

            // Note: Commits in PR may not exist in the local repo
            _fileStatusList.SetDiffs(firstRev, secondRev, items: giss);
        }

        private void _fetchBtn_Click(object sender, EventArgs e)
        {
            if (_currentPullRequestInfo is null)
            {
                return;
            }

            var cmd = string.Format("fetch --no-tags --progress {0} {1}:{2}",
                _currentPullRequestInfo.HeadRepo.CloneReadOnlyUrl, _currentPullRequestInfo.HeadRef, _currentPullRequestInfo.FetchBranch);
            var success = FormProcess.ShowDialog(this, process: AppSettings.GitCommand, arguments: cmd, Module.WorkingDir, input: null, useDialogSettings: true);
            if (!success)
            {
                return;
            }

            UICommands.RepoChangedNotifier.Notify();

            Close();
        }

        private void _addAsRemoteAndFetch_Click(object sender, EventArgs e)
        {
            if (_currentPullRequestInfo is null)
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
                if (existingRepo is not null)
                {
                    var hostedRepository = existingRepo.GetHostedRepository();
                    hostedRepository.CloneProtocol = _cloneGitProtocol;
                    if (hostedRepository.CloneReadOnlyUrl != remoteUrl)
                    {
                        MessageBox.Show(this, string.Format(_strRemoteAlreadyExist.Text, remoteName, hostedRepository.CloneReadOnlyUrl, remoteUrl),
                            TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                var success = FormProcess.ShowDialog(this, process: AppSettings.GitCommand, arguments: cmd, Module.WorkingDir, input: null, useDialogSettings: true);
                if (!success)
                {
                    return;
                }

                UICommands.RepoChangedNotifier.Notify();

                cmd = string.Format("checkout {0}/{1}", remoteName, remoteRef);
                success = FormProcess.ShowDialog(this, process: AppSettings.GitCommand, arguments: cmd, Module.WorkingDir, input: null, useDialogSettings: true);
                if (success)
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
            var gis = _fileStatusList.SelectedItem?.Item;
            if (gis is null)
            {
                return;
            }

            Validates.NotNull(_diffCache);
            var data = _diffCache[gis.Name];

            if (gis.IsSubmodule)
            {
                _diffViewer.ViewText(gis.Name, text: data);
            }
            else
            {
                _diffViewer.ViewFixedPatch(gis.Name, text: data);
            }
        }

        private void _closePullRequestBtn_Click(object sender, EventArgs e)
        {
            if (_currentPullRequestInfo is null)
            {
                return;
            }

            try
            {
                _currentPullRequestInfo.Close();
                _selectedOwner_SelectedIndexChanged(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _strFailedToClosePullRequest.Text + Environment.NewLine + ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
