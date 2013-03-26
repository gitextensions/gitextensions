﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces.RepositoryHosts;
using ResourceManager.Translation;

namespace GitUI.CommandsDialogs.RepoHosting
{
    public partial class CreatePullRequestForm : GitModuleForm
    {
        #region Translation
        private readonly TranslationString _strLoading = new TranslationString("Loading...");
        private readonly TranslationString _strCouldNotLocateARemoteThatBelongsToYourUser = new TranslationString("Could not locate a remote that belongs to your user!");
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
        private List<IHostedRemote> _hostedRemotes;
        private string _currentBranch;
        private string _prevTitle;
        private readonly AsyncLoader _remoteLoader = new AsyncLoader();

        public CreatePullRequestForm(GitUICommands aCommands, IRepositoryHostPlugin repoHost, string chooseRemote, string chooseBranch)
            : base(aCommands)
        {
            _repoHost = repoHost;
            _chooseRemote = chooseRemote;
            _currentBranch = chooseBranch;
            InitializeComponent();
            Translate();
            _prevTitle = _titleTB.Text;
        }

        private void CreatePullRequestForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void Init()
        {
            _createBtn.Enabled = false;
            _yourBranchesCB.Text = _strLoading.Text;
            _hostedRemotes = _repoHost.GetHostedRemotesForModule(Module);
            this.Mask();
            _remoteLoader.Load(
                () => _hostedRemotes.Where(r => !r.IsOwnedByMe).ToArray(),
                (IHostedRemote[] foreignHostedRemotes) =>
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

            AsyncLoader.DoAsync(
                () => _currentHostedRemote.GetHostedRepository().Branches,
                branches =>
                {
                    branches.Sort((a, b) => String.Compare(a.Name, b.Name, true));
                    int selectItem = 0;
                    _remoteBranchesCB.Items.Clear();
                    for (int i = 0; i < branches.Count; i++)
                    {
                        if (branches[i].Name == _currentBranch)
                            selectItem = i;
                        _remoteBranchesCB.Items.Add(branches[i].Name);
                    }
                    _createBtn.Enabled = true;
                    if (branches.Count > 0)
                        _remoteBranchesCB.SelectedIndex = selectItem;
                },
                ex => { ex.Handled = false; });
        }

        private IHostedRemote MyRemote
        {
            get
            {
                return _hostedRemotes.FirstOrDefault(r => r.IsOwnedByMe);
            }
        }

        private void LoadMyBranches()
        {
            _yourBranchesCB.Items.Clear();

            if (MyRemote == null)
                return;

            AsyncLoader.DoAsync(
                () => MyRemote.GetHostedRepository().Branches,
                branches =>
                {
                    branches.Sort((a, b) => String.Compare(a.Name, b.Name, true));
                    int selectItem = 0;
                    for (int i = 0; i < branches.Count; i++)
                    {
                        if (branches[i].Name == _currentBranch)
                            selectItem = i;
                        _yourBranchesCB.Items.Add(branches[i].Name);
                    }
                    _createBtn.Enabled = true;
                    if (branches.Count > 0)
                        _yourBranchesCB.SelectedIndex = selectItem;
                },
                ex => { ex.Handled = false; });
        }

        private void _yourBranchCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_prevTitle.Equals(_titleTB.Text) && !_yourBranchesCB.Text.IsNullOrWhiteSpace() && MyRemote != null)
            {
                var lastMsg = Module.GetPreviousCommitMessages(MyRemote.Name.Combine("/", _yourBranchesCB.Text), 1).FirstOrDefault();
                _titleTB.Text = lastMsg.TakeUntilStr("\n");
                _prevTitle = _titleTB.Text;
            }
        }

        private void _createBtn_Click(object sender, EventArgs e)
        {
            if (_currentHostedRemote == null)
                return;

            var title = _titleTB.Text.Trim();
            var body = _bodyTB.Text.Trim();
            if (title.Length == 0)
            {
                MessageBox.Show(this, _strYouMustSpecifyATitle.Text , _strError.Text);
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
    }
}
