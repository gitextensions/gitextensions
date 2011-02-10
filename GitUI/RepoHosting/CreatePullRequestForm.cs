using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitUIPluginInterfaces.RepositoryHosts;
using ResourceManager.Translation;

namespace GitUI.RepoHosting
{
    public partial class CreatePullRequestForm : GitExtensionsForm
    {
        #region Translation
        private readonly TranslationString _strLoading = new TranslationString("Loading...");
        private readonly TranslationString _strCouldNotLocateARemoteThatBelongsToYourUser = new TranslationString("Could not locate a remote that belongs to your user!");
        private readonly TranslationString _strYouMustSpecifyATitleAndABody = new TranslationString("You must specify a title and a body.");
        private readonly TranslationString _strPullRequest = new TranslationString("Pull request");
        private readonly TranslationString _strFailedToCreatePullRequest = new TranslationString("Failed to create pull request.\r\n");
        private readonly TranslationString _strDone = new TranslationString("Done");
        private readonly TranslationString _strError = new TranslationString("Error");
        #endregion

        private readonly IRepositoryHostPlugin _repoHost;
        private IHostedRemote _currentHostedRemote;
        private string _chooseBranch;
        private readonly string _chooseRemote;
        private List<IHostedRemote> _hostedRemotes;

        public CreatePullRequestForm(IRepositoryHostPlugin repoHost, string chooseRemote, string chooseBranch)
        {
            _repoHost = repoHost;
            _chooseBranch = chooseBranch;
            _chooseRemote = chooseRemote;
            InitializeComponent();
            Translate();
        }

        private void CreatePullRequestForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void Init()
        {
            _createBtn.Enabled = false;
            _yourBranchesCB.Text = _strLoading.Text;
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
                    var ihr = _pullReqTargetsCB.Items[i] as IHostedRemote;
                    if (ihr != null && ihr.Name == _chooseRemote)
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
            _remoteBranchesCB.Text = _strLoading.Text;

            AsyncHelpers.DoAsync(
                () => _currentHostedRemote.GetHostedRepository().Branches,
                branches =>
                {
                    foreach (var branch in branches)
                        _remoteBranchesCB.Items.Add(branch.Name);
                    _createBtn.Enabled = true;
                    if (branches.Count > 0)
                        _remoteBranchesCB.SelectedIndex = 0;
                },
                ex => { throw ex; });
        }

        private void LoadMyBranches()
        {
            var myRemote = _hostedRemotes.Where(r => r.IsOwnedByMe).FirstOrDefault();
            if (myRemote == null)
                throw new InvalidOperationException(_strCouldNotLocateARemoteThatBelongsToYourUser.Text);

            _yourBranchesCB.Items.Clear();

            AsyncHelpers.DoAsync(
                () => myRemote.GetHostedRepository().Branches,
                branches =>
                {
                    foreach (var branch in branches)
                        _yourBranchesCB.Items.Add(branch.Name);
                    _createBtn.Enabled = true;
                    if (branches.Count > 0)
                        _yourBranchesCB.SelectedIndex = 0;
                },
                ex => { throw ex; });
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
                MessageBox.Show(this, _strYouMustSpecifyATitleAndABody.Text , _strError.Text);
                return;
            }

            try
            {
                var hostedRepo = _currentHostedRemote.GetHostedRepository();

                hostedRepo.CreatePullRequest(_yourBranchesCB.Text, _remoteBranchesCB.Text, title, body);
                MessageBox.Show(this, _strDone.Text, _strPullRequest.Text);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _strFailedToCreatePullRequest.Text + ex.Message, _strError.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
