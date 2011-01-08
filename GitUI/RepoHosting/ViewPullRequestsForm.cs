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

            SetPullRequestsData(fetcher.Fetch());
        }

        private void SetPullRequestsData(List<IPullRequestInformation> infos)
        {
            _pullRequestsInfo = infos;
            _pullRequestsList.Items.Clear();

            if (_pullRequestsInfo == null)
                return;

            LoadListView();
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

            _pullRequestsList_SelectedIndexChanged(null, null);
        }

        private void _pullRequestsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentPullRequestInfo = null;
            if (_pullRequestsList.SelectedItems.Count != 1)
            {
                _pullRequestBody.Text = "";
                _diffViewer.ViewText("", "");
                return;
            }

            _currentPullRequestInfo = _pullRequestsList.SelectedItems[0].Tag as IPullRequestInformation;
            if (_currentPullRequestInfo == null)
                return;
            _pullRequestBody.Text = _currentPullRequestInfo.Body;
            _diffViewer.ViewPatch(_currentPullRequestInfo.DiffData);
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
        }
    }
}
