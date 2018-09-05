using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces.RepositoryHosts;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs.RepoHosting
{
    public partial class CreatePullRequestForm : GitModuleForm
    {
        #region Translation
        private readonly TranslationString _strLoading = new TranslationString("Loading...");
        private readonly TranslationString _strYouMustSpecifyATitle = new TranslationString("You must specify a title.");
        private readonly TranslationString _strPullRequest = new TranslationString("Pull request");
        private readonly TranslationString _strFailedToCreatePullRequest = new TranslationString("Failed to create pull request.");
        private readonly TranslationString _strPleaseCloneGitHubRep = new TranslationString("Please clone GitHub repository before pull request.");
        private readonly TranslationString _strDone = new TranslationString("Done");
        private readonly TranslationString _strError = new TranslationString("Error");
        #endregion

        private readonly IRepositoryHostPlugin _repoHost;
        private IHostedRemote _currentHostedRemote;
        private readonly string _chooseRemote;
        private IReadOnlyList<IHostedRemote> _hostedRemotes;
        private string _currentBranch;
        private string _prevTitle;
        private readonly AsyncLoader _remoteLoader = new AsyncLoader();

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private CreatePullRequestForm()
        {
            InitializeComponent();
        }

        public CreatePullRequestForm(GitUICommands commands, IRepositoryHostPlugin repoHost, string chooseRemote, string chooseBranch)
            : base(commands)
        {
            _repoHost = repoHost;
            _chooseRemote = chooseRemote;
            _currentBranch = chooseBranch;
            InitializeComponent();
            InitializeComplete();
            _prevTitle = _titleTB.Text;
            _pullReqTargetsCB.DisplayMember = nameof(IHostedRemote.DisplayData);
        }

        private void CreatePullRequestForm_Load(object sender, EventArgs e)
        {
            _createBtn.Enabled = false;
            _yourBranchesCB.Text = _strLoading.Text;
            _hostedRemotes = _repoHost.GetHostedRemotesForModule(Module);
            this.Mask();
            _remoteLoader.LoadAsync(
                () => _hostedRemotes.Where(r => !r.IsOwnedByMe).ToArray(),
                foreignHostedRemotes =>
                {
                    if (foreignHostedRemotes.Length == 0)
                    {
                        MessageBox.Show(this, _strFailedToCreatePullRequest.Text + Environment.NewLine +
                                              _strPleaseCloneGitHubRep.Text, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Close();
                        return;
                    }

                    this.UnMask();

                    _currentBranch = Module.IsValidGitWorkingDir() ? Module.GetSelectedBranch() : "";
                    LoadRemotes(foreignHostedRemotes);
                    LoadMyBranches();
                });
        }

        private void LoadRemotes(IHostedRemote[] foreignHostedRemotes)
        {
            _pullReqTargetsCB.Items.Clear();
            _pullReqTargetsCB.Items.AddRange(foreignHostedRemotes);

            if (_chooseRemote != null)
            {
                for (int i = 0; i < _pullReqTargetsCB.Items.Count; i++)
                {
                    if (_pullReqTargetsCB.Items[i] is IHostedRemote ihr && ihr.Name == _chooseRemote)
                    {
                        _pullReqTargetsCB.SelectedIndex = i;
                        break;
                    }
                }
            }
            else if (_pullReqTargetsCB.Items.Count > 0)
            {
                _pullReqTargetsCB.SelectedIndex = 0;
            }

            _pullReqTargetsCB_SelectedIndexChanged(null, null);
        }

        private void _pullReqTargetsCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentHostedRemote = _pullReqTargetsCB.SelectedItem as IHostedRemote;

            _remoteBranchesCB.Items.Clear();
            _remoteBranchesCB.Text = _strLoading.Text;

            PopulateBranchesComboAndEnableCreateButton(_currentHostedRemote, _remoteBranchesCB);
        }

        [CanBeNull]
        private IHostedRemote MyRemote => _hostedRemotes.FirstOrDefault(r => r.IsOwnedByMe);

        private void LoadMyBranches()
        {
            _yourBranchesCB.Items.Clear();

            var myRemote = MyRemote;

            if (myRemote == null)
            {
                return;
            }

            PopulateBranchesComboAndEnableCreateButton(myRemote, _yourBranchesCB);
        }

        private void PopulateBranchesComboAndEnableCreateButton(IHostedRemote remote, ComboBox comboBox)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        await TaskScheduler.Default;

                        var branches = remote.GetHostedRepository().GetBranches();

                        await this.SwitchToMainThreadAsync();

                        comboBox.Items.Clear();

                        var selectItem = 0;
                        for (var i = 0; i < branches.Count; i++)
                        {
                            if (branches[i].Name == _currentBranch)
                            {
                                selectItem = i;
                            }

                            comboBox.Items.Add(branches[i].Name);
                        }

                        if (branches.Count > 0)
                        {
                            comboBox.SelectedIndex = selectItem;
                        }

                        _createBtn.Enabled = true;
                    })
                .FileAndForget();
        }

        private void _yourBranchCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_prevTitle == _titleTB.Text && !_yourBranchesCB.Text.IsNullOrWhiteSpace() && MyRemote != null)
            {
                var lastMsg = Module.GetPreviousCommitMessages(1, MyRemote.Name.Combine("/", _yourBranchesCB.Text)).FirstOrDefault();
                _titleTB.Text = lastMsg?.SubstringUntil('\n');
                _prevTitle = _titleTB.Text;
            }
        }

        private void _createBtn_Click(object sender, EventArgs e)
        {
            if (_currentHostedRemote == null)
            {
                return;
            }

            var title = _titleTB.Text.Trim();
            var body = _bodyTB.Text.Trim();
            if (title.Length == 0)
            {
                MessageBox.Show(this, _strYouMustSpecifyATitle.Text, _strError.Text);
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
                MessageBox.Show(this, _strFailedToCreatePullRequest.Text + Environment.NewLine +
                    ex.Message, _strError.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                _remoteLoader.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
