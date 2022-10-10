using GitCommands;
using GitUIPluginInterfaces.RepositoryHosts;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs.RepoHosting
{
    public partial class CreatePullRequestForm : GitModuleForm
    {
        #region Translation
        private readonly TranslationString _strLoading = new("Loading...");
        private readonly TranslationString _strYouMustSpecifyATitle = new("You must specify a title.");
        private readonly TranslationString _strPullRequest = new("Pull request");
        private readonly TranslationString _strFailedToCreatePullRequest = new("Failed to create pull request.");
        private readonly TranslationString _strPleaseCloneGitHubRep = new("Please clone GitHub repository before pull request.");
        private readonly TranslationString _strDone = new("Done");
        private readonly TranslationString _strRemoteFailToLoadBranches = new("Fail to load target branches");
        #endregion

        private readonly IRepositoryHostPlugin _repoHost;
        private IHostedRemote? _currentHostedRemote;
        private readonly string? _chooseRemote;
        private IReadOnlyList<IHostedRemote>? _hostedRemotes;
        private string? _currentBranch;
        private string? _prevTitle;
        private readonly AsyncLoader _remoteLoader = new();
        private bool _ignoreFirstRemoteLoading = true;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private CreatePullRequestForm()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        public CreatePullRequestForm(GitUICommands commands, IRepositoryHostPlugin repoHost, string? chooseRemote, string? chooseBranch)
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
            _hostedRemotes = _repoHost.GetHostedRemotesForModule();
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

            if (_chooseRemote is not null)
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

            _ignoreFirstRemoteLoading = false;

            _pullReqTargetsCB_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void _pullReqTargetsCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreFirstRemoteLoading)
            {
                return;
            }

            _currentHostedRemote = (IHostedRemote)_pullReqTargetsCB.SelectedItem;

            _remoteBranchesCB.Items.Clear();
            _remoteBranchesCB.Text = _strLoading.Text;

            PopulateBranchesComboAndEnableCreateButton(_currentHostedRemote, _remoteBranchesCB);
        }

        private IHostedRemote? MyRemote => _hostedRemotes.FirstOrDefault(r => r.IsOwnedByMe);

        private void LoadMyBranches()
        {
            _yourBranchesCB.Items.Clear();

            var myRemote = MyRemote;

            if (myRemote is null)
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

                        try
                        {
                            IHostedRepository hostedRepository = remote.GetHostedRepository();
                            var branches = hostedRepository.GetBranches();

                            await this.SwitchToMainThreadAsync();

                            comboBox.Items.Clear();

                            var selectItem = 0;
                            var defaultBranch = hostedRepository.GetDefaultBranch();
                            for (var i = 0; i < branches.Count; i++)
                            {
                                if (branches[i].Name == defaultBranch)
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
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.ShowDialog(new TaskDialogPage
                                {
                                    Icon = TaskDialogIcon.Error,
                                    Caption = _strRemoteFailToLoadBranches.Text,
                                    Text = string.Format(TranslatedStrings.RemoteInError, ex.Message, remote.DisplayData),
                                    Buttons = { TaskDialogButton.OK },
                                    SizeToContent = true
                                });
                        }
                    })
                .FileAndForget();
        }

        private void _yourBranchCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_prevTitle == _titleTB.Text && !string.IsNullOrWhiteSpace(_yourBranchesCB.Text) && MyRemote is not null)
            {
                var lastMsg = Module.GetPreviousCommitMessages(1, MyRemote.Name.Combine("/", _yourBranchesCB.Text)!).FirstOrDefault();
                _titleTB.Text = lastMsg?.SubstringUntil('\n');
                _prevTitle = _titleTB.Text;
            }
        }

        private void _createBtn_Click(object sender, EventArgs e)
        {
            if (_currentHostedRemote is null)
            {
                return;
            }

            var title = _titleTB.Text.Trim();
            var body = _bodyTB.Text.Trim();
            if (title.Length == 0)
            {
                MessageBox.Show(this, _strYouMustSpecifyATitle.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var hostedRepo = _currentHostedRemote.GetHostedRepository();

                hostedRepo.CreatePullRequest(_yourBranchesCB.Text, _remoteBranchesCB.Text, title, body);
                MessageBox.Show(this, _strDone.Text, _strPullRequest.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _strFailedToCreatePullRequest.Text + Environment.NewLine +
                    ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
