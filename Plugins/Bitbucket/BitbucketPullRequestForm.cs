using System.ComponentModel;
using System.Text;
using GitCommands;
using GitCommands.Git;
using GitExtUtils;
using GitUI;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitExtensions.Plugins.Bitbucket
{
    public partial class BitbucketPullRequestForm : GitExtensionsFormBase
    {
        private readonly TranslationString _committed = new("{0} committed\n{1}");
        private readonly TranslationString _success = new("Success");
        private readonly TranslationString _error = new("Error");
        private readonly TranslationString _linkLabelToolTip = new("Right-click to copy link");
        private readonly string _NO_TRANSLATE_RepoUrl = "{0}/projects/{1}/repos/{2}/";
        private readonly string _NO_TRANSLATE_LinkCreatePull = "compare/commits?sourceBranch={0}";
        private readonly string _NO_TRANSLATE_LinkCreatePullNoBranch = "pull-requests?create";
        private readonly string _NO_TRANSLATE_LinkViewPull = "pull-requests";

        private readonly Settings? _settings;
        private readonly BindingList<BitbucketUser> _reviewers = new();

        public BitbucketPullRequestForm(Settings? settings, IGitModule? module)
        {
            InitializeComponent();

            // NOTE ddlBranchSource and ddlBranchTarget both have string items so do not need a display member
            ddlRepositorySource.DisplayMember = nameof(Repository.DisplayName);
            ddlRepositoryTarget.DisplayMember = nameof(Repository.DisplayName);

            _settings = settings ?? new Settings();

            Load += delegate
            {
                ReloadPullRequests();
                ReloadRepositories();
            };

            if (module is not null)
            {
                var repoUrl = _NO_TRANSLATE_RepoUrl = string.Format(_NO_TRANSLATE_RepoUrl,
                                          _settings.BitbucketUrl, _settings.ProjectKey, _settings.RepoSlug);
                var branch = GitCommands.GitRefName.GetFullBranchName(module.GetSelectedBranch());

                _NO_TRANSLATE_lblLinkCreatePull.Text = repoUrl +
                    ((string.IsNullOrEmpty(branch) || branch.Equals(DetachedHeadParser.DetachedBranch)) ?
                    _NO_TRANSLATE_LinkCreatePullNoBranch :
                    string.Format(_NO_TRANSLATE_LinkCreatePull, branch));
                toolTipLink.SetToolTip(_NO_TRANSLATE_lblLinkCreatePull, _linkLabelToolTip.Text);

                _NO_TRANSLATE_lblLinkViewPull.Text = repoUrl + _NO_TRANSLATE_LinkViewPull;
                toolTipLink.SetToolTip(_NO_TRANSLATE_lblLinkViewPull, _linkLabelToolTip.Text);
            }

            InitializeComplete();
        }

        private void ReloadRepositories()
        {
            if (_settings is null)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;
                var repositories = await GetRepositoriesAsync();

                await this.SwitchToMainThreadAsync();
                ddlRepositorySource.DataSource = repositories.ToList();
                ddlRepositoryTarget.DataSource = repositories.ToList();
                ddlRepositorySource.Enabled = true;
                ddlRepositoryTarget.Enabled = true;
            }).FileAndForget();

            async Task<List<Repository>> GetRepositoriesAsync()
            {
                Validates.NotNull(_settings.ProjectKey);
                Validates.NotNull(_settings.RepoSlug);

                List<Repository> list = new();
                GetRepoRequest getDefaultRepo = new(_settings.ProjectKey, _settings.RepoSlug, _settings);
                var defaultRepo = await getDefaultRepo.SendAsync().ConfigureAwait(false);
                if (defaultRepo.Success)
                {
                    Validates.NotNull(defaultRepo.Result);
                    list.Add(defaultRepo.Result);
                }

                return list;
            }
        }

        private void ReloadPullRequests()
        {
            if (_settings is null)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;
                var pullRequests = await GetPullRequestsAsync();

                await this.SwitchToMainThreadAsync();
                lbxPullRequests.DataSource = pullRequests;
                lbxPullRequests.DisplayMember = nameof(PullRequest.DisplayName);
            }).FileAndForget();

            async Task<List<PullRequest>> GetPullRequestsAsync()
            {
                Validates.NotNull(_settings.ProjectKey);
                Validates.NotNull(_settings.RepoSlug);

                List<PullRequest> list = new();
                GetPullRequest getPullRequests = new(_settings.ProjectKey, _settings.RepoSlug, _settings);
                var result = await getPullRequests.SendAsync().ConfigureAwait(false);
                if (result.Success)
                {
                    list.AddRange(result.Result);
                }

                return list;
            }
        }

        private void BtnCreateClick(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                if (ddlBranchSource.SelectedValue is null ||
                    ddlBranchTarget.SelectedValue is null ||
                    ddlRepositorySource.SelectedValue is null ||
                    ddlRepositoryTarget.SelectedValue is null)
                {
                    return;
                }

                PullRequestInfo info = new()
                {
                    Title = txtTitle.Text,
                    Description = txtDescription.Text,
                    SourceBranch = ddlBranchSource.SelectedValue.ToString(),
                    TargetBranch = ddlBranchTarget.SelectedValue.ToString(),
                    SourceRepo = (Repository)ddlRepositorySource.SelectedValue,
                    TargetRepo = (Repository)ddlRepositoryTarget.SelectedValue,
                    Reviewers = _reviewers
                };
                Validates.NotNull(_settings);
                CreatePullRequestRequest pullRequest = new(_settings, info);
                var response = await pullRequest.SendAsync();
                await this.SwitchToMainThreadAsync();
                if (response.Success)
                {
                    MessageBox.Show(_success.Text, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ReloadPullRequests();
                }
                else
                {
                    MessageBox.Show(string.Join(Environment.NewLine, response.Messages),
                        _error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        private readonly Dictionary<Repository, IEnumerable<string>> _branches = new();
        private async Task<IEnumerable<string>> GetBitbucketBranchesAsync(Repository selectedRepo)
        {
            lock (_branches)
            {
                if (_branches.ContainsKey(selectedRepo))
                {
                    return _branches[selectedRepo];
                }
            }

            Validates.NotNull(_settings);

            List<string> list = new();
            GetBranchesRequest getBranches = new(selectedRepo, _settings);
            var result = await getBranches.SendAsync().ConfigureAwait(false);
            if (result.Success)
            {
                Validates.NotNull(result.Result);
                foreach (var value in result.Result["values"])
                {
                    list.Add(value["displayId"].ToString());
                }
            }

            lock (_branches)
            {
                _branches.Add(selectedRepo, list);
            }

            return list;
        }

        private void DdlRepositorySourceSelectedValueChanged(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(() => RefreshDDLBranchAsync(ddlBranchSource, ((ComboBox)sender).SelectedValue));
        }

        private void DdlRepositoryTargetSelectedValueChanged(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(() => RefreshDDLBranchAsync(ddlBranchTarget, ((ComboBox)sender).SelectedValue));
        }

        private async Task RefreshDDLBranchAsync(ComboBox branchComboBox, object selectedValue)
        {
            var branchNames = (await GetBitbucketBranchesAsync((Repository)selectedValue)).ToList();
            branchNames.Sort();
            branchNames.Insert(0, "");
            await this.SwitchToMainThreadAsync();
            branchComboBox.DataSource = branchNames;
        }

        private void DdlBranchSourceSelectedValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlBranchSource.SelectedValue.ToString()))
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var commit = await GetCommitInfoAsync((Repository)ddlRepositorySource.SelectedValue,
                                                    ddlBranchSource.SelectedValue.ToString());
                await this.SwitchToMainThreadAsync();

                ddlBranchSource.Tag = commit;
                UpdateCommitInfo(lblCommitInfoSource, commit);
                txtTitle.Text = ddlBranchSource.SelectedValue.ToString().Replace("-", " ");
                await UpdatePullRequestDescriptionAsync();
            });
        }

        private void DdlBranchTargetSelectedValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlBranchTarget.SelectedValue.ToString()))
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var commit = await GetCommitInfoAsync((Repository)ddlRepositoryTarget.SelectedValue,
                                                    ddlBranchTarget.SelectedValue.ToString());
                await this.SwitchToMainThreadAsync();

                ddlBranchTarget.Tag = commit;
                UpdateCommitInfo(lblCommitInfoTarget, commit);
                await UpdatePullRequestDescriptionAsync();
            });
        }

        private async Task<Commit?> GetCommitInfoAsync(Repository? repo, string branch)
        {
            if (repo is null || string.IsNullOrWhiteSpace(branch))
            {
                return null;
            }

            Validates.NotNull(_settings);
            GetHeadCommitRequest getCommit = new(repo, branch, _settings);
            var result = await getCommit.SendAsync().ConfigureAwait(false);
            return result.Success ? result.Result : null;
        }

        private void UpdateCommitInfo(Label label, Commit? commit)
        {
            if (commit is null)
            {
                label.Text = string.Empty;
            }
            else
            {
                label.Text = string.Format(_committed.Text, commit.AuthorName, commit.Message);
            }
        }

        private async Task UpdatePullRequestDescriptionAsync()
        {
            await this.SwitchToMainThreadAsync();

            if (ddlRepositorySource.SelectedValue is null
                || ddlRepositoryTarget.SelectedValue is null
                || ddlBranchSource.Tag is null
                || ddlBranchTarget.Tag is null)
            {
                return;
            }

            Validates.NotNull(_settings);

            GetInBetweenCommitsRequest getCommitsInBetween = new(
                (Repository)ddlRepositorySource.SelectedValue,
                (Repository)ddlRepositoryTarget.SelectedValue,
                (Commit)ddlBranchSource.Tag,
                (Commit)ddlBranchTarget.Tag,
                _settings);

            var result = await getCommitsInBetween.SendAsync();
            if (result.Success)
            {
                Validates.NotNull(result.Result);

                await this.SwitchToMainThreadAsync();

                StringBuilder sb = new();
                sb.AppendLine();
                foreach (var commit in result.Result)
                {
                    if (!commit.IsMerge)
                    {
                        sb.Append("* ").AppendLine(commit.Message);
                    }
                }

                txtDescription.Text = sb.ToString();
            }
        }

        private void PullRequestChanged(object sender, EventArgs e)
        {
            var curItem = (PullRequest)lbxPullRequests.SelectedItem;

            txtPRTitle.Text = curItem.Title;
            txtPRDescription.Text = curItem.Description;
            lblPRAuthor.Text = curItem.Author;
            lblPRState.Text = curItem.State;
            txtPRReviewers.Text = curItem.Reviewers;
            lblPRSourceRepo.Text = curItem.SrcDisplayName;
            lblPRSourceBranch.Text = curItem.SrcBranch;
            lblPRDestRepo.Text = curItem.DestDisplayName;
            lblPRDestBranch.Text = curItem.DestBranch;

            Validates.NotNull(_settings);

            _NO_TRANSLATE_lblLinkViewPull.Text = string.Format("{0}/projects/{1}/repos/{2}/pull-requests/{3}/overview",
                _settings.BitbucketUrl, _settings.ProjectKey, _settings.RepoSlug, curItem.Id);
        }

        private void BtnMergeClick(object sender, EventArgs e)
        {
            if (lbxPullRequests.SelectedItem is PullRequest curItem)
            {
                MergeRequestInfo mergeInfo = new()
                {
                    Id = curItem.Id,
                    Version = curItem.Version,
                    ProjectKey = curItem.DestProjectKey,
                    TargetRepo = curItem.DestRepo,
                };

                Validates.NotNull(_settings);

                // Merge
                MergePullRequest mergeRequest = new(_settings, mergeInfo);
                var response = ThreadHelper.JoinableTaskFactory.Run(() => mergeRequest.SendAsync());
                if (response.Success)
                {
                    MessageBox.Show(_success.Text, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ReloadPullRequests();
                }
                else
                {
                    MessageBox.Show(
                        string.Join(Environment.NewLine, response.Messages),
                        _error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnApproveClick(object sender, EventArgs e)
        {
            if (lbxPullRequests.SelectedItem is PullRequest curItem)
            {
                MergeRequestInfo mergeInfo = new()
                {
                    Id = curItem.Id,
                    Version = curItem.Version,
                    ProjectKey = curItem.DestProjectKey,
                    TargetRepo = curItem.DestRepo,
                };

                Validates.NotNull(_settings);

                // Approve
                ApprovePullRequest approveRequest = new(_settings, mergeInfo);
                var response = ThreadHelper.JoinableTaskFactory.Run(() => approveRequest.SendAsync());
                if (response.Success)
                {
                    MessageBox.Show(_success.Text, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ReloadPullRequests();
                }
                else
                {
                    MessageBox.Show(
                        string.Join(Environment.NewLine, response.Messages),
                        _error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void textLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                var link = ((LinkLabel)sender).Text;
                if (e.Button == MouseButtons.Right)
                {
                    // Just copy the text
                    ClipboardUtil.TrySetText(link);
                }
                else
                {
                    OsShellUtil.OpenUrlInDefaultBrowser(link);
                }
            }
            catch
            {
            }
        }
    }
}
