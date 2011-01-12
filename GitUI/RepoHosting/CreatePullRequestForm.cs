using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces.RepositoryHosts;

namespace GitUI.RepoHosting
{
    public partial class CreatePullRequestForm : Form
    {
        private IRepositoryHostPlugin _repoHost;
        private IHostedRemote _currentHostedRemote;
        private string _chooseBranch;
        private string _chooseRemote;
        private List<IHostedRemote> _hostedRemotes;

        public CreatePullRequestForm(IRepositoryHostPlugin repoHost, string chooseRemote, string chooseBranch)
        {
            _repoHost = repoHost;
            _chooseBranch = chooseBranch;
            _chooseRemote = chooseRemote;
            InitializeComponent();
        }

        private void CreatePullRequestForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void Init()
        {
            _createBtn.Enabled = false;
            _yourBranchesCB.Text = "Loading...";
            _hostedRemotes = _repoHost.GetHostedRemotesForCurrentWorkingDirRepo();
            LoadRemotes();
            LoadMyBranches();
        }

        private void LoadRemotes()
        {
            var foreignHostedRemotes = _hostedRemotes.Where(r => !r.IsOwnedByMe);

            _pullReqTargetsCB.Items.Clear();
            foreach (var pra in foreignHostedRemotes)
                _pullReqTargetsCB.Items.Add(pra);

            if (_chooseRemote != null)
            {
                for (int i = 0; i < _pullReqTargetsCB.Items.Count; i++)
                {
                    IHostedRemote ihr = _pullReqTargetsCB.Items[i] as IHostedRemote;
                    if (ihr.Name == _chooseRemote)
                    {
                        _pullReqTargetsCB.SelectedIndex = i;
                        break;
                    }
                }
            }
            else if (_pullReqTargetsCB.Items.Count > 0)
                _pullReqTargetsCB.SelectedIndex = 0;

            _pullReqTargetsCB_SelectedIndexChanged(null, null);
        }

        private void _pullReqTargetsCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentHostedRemote = _pullReqTargetsCB.SelectedItem as IHostedRemote;

            _remoteBranchesCB.Items.Clear();
            _remoteBranchesCB.Text = "Loading...";

            AsyncHelpers.DoAsync(
                () => _currentHostedRemote.GetHostedRepository().Branches,
                (branches) =>
                {
                    foreach (var branch in branches)
                        _remoteBranchesCB.Items.Add(branch.Name);
                    _createBtn.Enabled = true;
                    if (branches.Count > 0)
                        _remoteBranchesCB.SelectedIndex = 0;
                },
                (ex) => { throw ex; });
        }

        private void LoadMyBranches()
        {
            var myRemote = _hostedRemotes.Where(r => r.IsOwnedByMe).FirstOrDefault();
            if (myRemote == null)
                throw new InvalidOperationException("Could not locate a remote that belongs to your user!");

            _yourBranchesCB.Items.Clear();

            AsyncHelpers.DoAsync(
                () => myRemote.GetHostedRepository().Branches,
                (branches) =>
                {
                    foreach (var branch in branches)
                        _yourBranchesCB.Items.Add(branch.Name);
                    _createBtn.Enabled = true;
                    if (branches.Count > 0)
                        _yourBranchesCB.SelectedIndex = 0;
                },
                (ex) => { throw ex; });
        }

        private void _yourBranchCB_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void _createBtn_Click(object sender, EventArgs e)
        {
            if (_currentHostedRemote == null)
                return;

            var title = _titleTB.Text.Trim();
            var body = _bodyTB.Text.Trim();
            if (title.Length == 0 || body.Length == 0)
            {
                MessageBox.Show(this, "You must specify a title and a body.", "Error");
                return;
            }

            try
            {
                var hostedRepo = _currentHostedRemote.GetHostedRepository();
                if (hostedRepo == null)
                    throw new InvalidOperationException("Failed to get hosted repo interface");

                hostedRepo.CreatePullRequest(_yourBranchesCB.Text, _remoteBranchesCB.Text, title, body);
                MessageBox.Show(this, "Done.", "Pull request");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to create pull request.\r\n" + ex.Message, "Error");
            }
        }
    }
}
