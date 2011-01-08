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
using System.Text.RegularExpressions;

namespace GitUI.RepoHosting
{
    public partial class ViewPullRequestsForm : Form
    {
        private GitUIPluginInterfaces.IGitHostingPlugin _gitHoster;

        public ViewPullRequestsForm()
        {
            InitializeComponent();
        }

        public ViewPullRequestsForm(GitUIPluginInterfaces.IGitHostingPlugin gitHoster) : this()
        {
            _gitHoster = gitHoster;
        }

        List<IPullRequestsFetcher> _fetchers;
        List<IPullRequestInformation> _pullRequestsInfo;
        IPullRequestInformation _currentPullRequestInfo;

        private void ViewPullRequestsForm_Load(object sender, EventArgs e)
        {
            _fileStatusList.SelectedIndexChanged += _fileStatusList_SelectedIndexChanged;

            Init();
        }

        private void Init()
        {
            _fetchers = _gitHoster.GetPullRequestTargetsForCurrentWorkingDirRepo();

            _selectedOwner.Items.Clear();
            foreach (var fetcher in _fetchers)
                _selectedOwner.Items.Add(fetcher);

            if (_selectedOwner.Items.Count > 0)
            {
                _selectedOwner.SelectedIndex = 0;
                _selectedOwner_SelectedIndexChanged(null, null);
            }
        }

        private void _selectedOwner_SelectedIndexChanged(object sender, EventArgs e)
        {
            var fetcher = _selectedOwner.SelectedItem as IPullRequestsFetcher;
            if (fetcher == null)
                return;
            _selectedOwner.Enabled = false;
            ShowLoadingPullRequests();

            AsyncHelpers.DoAsync(
                () => fetcher.Fetch(),
                (res) => { SetPullRequestsData(res); _selectedOwner.Enabled = true; },
                (ex) => MessageBox.Show(this, "Failed to fetch pull data! " + ex.Message, "Error")
            );
        }

        private void SetPullRequestsData(List<IPullRequestInformation> infos)
        {
            _pullRequestsInfo = infos;
            _pullRequestsList.Items.Clear();

            if (_pullRequestsInfo == null)
                return;

            LoadListView();
        }

        private void ShowLoadingPullRequests()
        {
            _pullRequestsList.Items.Clear();
            var lvi = new ListViewItem("");
            lvi.SubItems.Add(" : LOADING : ");
            _pullRequestsList.Items.Add(lvi);
        }

        private void LoadListView()
        {
            foreach (var info in _pullRequestsInfo)
            {
                var lvi = new ListViewItem()
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
            var prevPRI = _currentPullRequestInfo;
            
            if (_pullRequestsList.SelectedItems.Count != 1)
            {
                _currentPullRequestInfo = null;
                _pullRequestBody.Text = "";
                _diffViewer.ViewText("", "");
                return;
            }

            _currentPullRequestInfo = _pullRequestsList.SelectedItems[0].Tag as IPullRequestInformation;
            if (prevPRI == _currentPullRequestInfo)
                return;

            if (_currentPullRequestInfo == null)
                return;
            _pullRequestBody.Text = _currentPullRequestInfo.Body;
            _diffViewer.ViewPatch("");
            _fileStatusList.GitItemStatuses = new List<GitItemStatus>();

            LoadDiffPatch();
        }

        private void LoadDiffPatch()
        {
            AsyncHelpers.DoAsync(
                () => _currentPullRequestInfo.DiffData,
                (data) => SplitAndLoadDiff(data),
                (ex) => MessageBox.Show(this, "Failed to load diff stuff! " + ex.Message, "Error"));
        }


        Dictionary<string, string> _diffCache;
        private void SplitAndLoadDiff(string diffData)
        {
            _diffCache = new Dictionary<string, string>();

            var fileParts = Regex.Split(diffData, @"(?:\n|^)diff --git ").Where(el => el != null && el.Trim().Length > 10).ToList();
            List<GitItemStatus> giss = new List<GitItemStatus>();

            foreach (var part in fileParts)
            {
                var match = Regex.Match(part, @"^a/([^\n]+) b/([^\n]+)\s*(.*)$", RegexOptions.Singleline);
                if (!match.Success)
                {
                    MessageBox.Show(this, "Error: Unable to understand patch", "Error");
                    return;
                }

                var gis = new GitItemStatus()
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
            formProcess.ShowDialog();

            if (formProcess.ErrorOccurred())
                return;
            Close();
        }

        void _fileStatusList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var gis = _fileStatusList.SelectedItem as GitItemStatus;
            if (gis == null)
                return;

            var data = _diffCache[gis.Name];
            _diffViewer.ViewPatch(data);
        }
    }
}
