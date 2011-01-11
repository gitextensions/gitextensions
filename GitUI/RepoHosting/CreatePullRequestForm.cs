using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces;

namespace GitUI.RepoHosting
{
    public partial class CreatePullRequestForm : Form
    {
        private IGitHostingPlugin _repoHost;
        private IHostedRemote _currentHostedRemote;
        private string _chooseBranch;
        private string _chooseRemote;

        public CreatePullRequestForm(IGitHostingPlugin repoHost, string chooseRemote, string chooseBranch)
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
            _yourBranchCB.Text = "Loading...";
            LoadRemotes();
            LoadBranches();
        }

        private void LoadRemotes()
        {
            var hostedRemotes = _repoHost.GetPullRequestTargetsForCurrentWorkingDirRepo().Where(r => !r.IsProbablyOwnedByMe);

            _pullReqTargetsCB.Items.Clear();
            foreach (var pra in hostedRemotes)
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
        }

        private void LoadBranches()
        {
            var myRemote = _repoHost.GetPullRequestTargetsForCurrentWorkingDirRepo().Where(r => r.IsProbablyOwnedByMe).FirstOrDefault();
            if (myRemote == null)
                return;
            _yourBranchCB.Items.Clear();

            AsyncHelpers.DoAsync(
                () => GitCommands.GitCommandHelpers.GetRemoteHeads(myRemote.Name, false, true),
                (branches) =>
                {
                    foreach (var branch in branches)
                        _yourBranchCB.Items.Add(branch.Name);
                    _createBtn.Enabled = true;
                    _yourBranchCB.Text = _chooseBranch ?? "";
                },
                (ex) => { MessageBox.Show(this, "Failed to load branches. " + ex.Message, "Error"); _createBtn.Enabled = true; } );
        }

        private void _yourBranchCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            _remoteBranchTB.Text = _yourBranchCB.Text;
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
                _currentHostedRemote.CreatePullRequest(_yourBranchCB.Text, _remoteBranchTB.Text, title, body);
                MessageBox.Show(this, "Done.", "Pull request");
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Failed to create pull request.\r\n" + ex.Message, "Error");
            }
        }

        private void _pullReqTargetsCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentHostedRemote = _pullReqTargetsCB.SelectedItem as IHostedRemote;
        }
    }
}
