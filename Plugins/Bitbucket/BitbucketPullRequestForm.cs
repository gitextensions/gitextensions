using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace Bitbucket
{
    public partial class BitbucketPullRequestForm : GitExtensionsFormBase
    {
        private readonly TranslationString _yourRepositoryIsNotInBitbucket = new TranslationString("Your repository is not hosted in Bitbucket.");
        private readonly TranslationString _commited = new TranslationString("{0} committed\n{1}");
        private readonly TranslationString _success = new TranslationString("Success");
        private readonly TranslationString _error = new TranslationString("Error");
        private readonly TranslationString _linkLabelToolTip = new TranslationString("Right-click to copy link");

        private readonly Settings _settings;
        private readonly BindingList<BitbucketUser> _reviewers = new BindingList<BitbucketUser>();

        public BitbucketPullRequestForm(BitbucketPlugin plugin, ISettingsSource settings, GitUIEventArgs gitUiCommands)
        {
            InitializeComponent();
            Translate();

            // NOTE ddlBranchSource and ddlBranchTarget both have string items so do not need a display member
            ddlRepositorySource.DisplayMember = nameof(Repository.DisplayName);
            ddlRepositoryTarget.DisplayMember = nameof(Repository.DisplayName);

            _settings = Settings.Parse(gitUiCommands.GitModule, settings, plugin);
            if (_settings == null)
            {
                MessageBox.Show(_yourRepositoryIsNotInBitbucket.Text);
                Close();
                return;
            }

            Load += BitbucketViewPullRequestFormLoad;
            Load += BitbucketPullRequestFormLoad;

            lblLinkCreatePull.Text = string.Format("{0}/projects/{1}/repos/{2}/pull-requests?create",
                                      _settings.BitbucketUrl, _settings.ProjectKey, _settings.RepoSlug);
            toolTipLink.SetToolTip(lblLinkCreatePull, _linkLabelToolTip.Text);

            lblLinkViewPull.Text = string.Format("{0}/projects/{1}/repos/{2}/pull-requests",
                _settings.BitbucketUrl, _settings.ProjectKey, _settings.RepoSlug);
            toolTipLink.SetToolTip(lblLinkViewPull, _linkLabelToolTip.Text);

            this.AdjustForDpiScaling();
        }

        private void BitbucketPullRequestFormLoad(object sender, EventArgs e)
        {
            if (_settings == null)
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
        }

        private void BitbucketViewPullRequestFormLoad(object sender, EventArgs e)
        {
            if (_settings == null)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;
                var pullReqs = await GetPullRequestsAsync();

                await this.SwitchToMainThreadAsync();
                lbxPullRequests.DataSource = pullReqs;
                lbxPullRequests.DisplayMember = nameof(PullRequest.DisplayName);
            }).FileAndForget();
        }

        private async Task<List<Repository>> GetRepositoriesAsync()
        {
            var list = new List<Repository>();
            var getDefaultRepo = new GetRepoRequest(_settings.ProjectKey, _settings.RepoSlug, _settings);
            var defaultRepo = await getDefaultRepo.SendAsync().ConfigureAwait(false);
            if (defaultRepo.Success)
            {
                list.Add(defaultRepo.Result);
            }

            return list;
        }

        private async Task<List<PullRequest>> GetPullRequestsAsync()
        {
            var list = new List<PullRequest>();
            var getPullReqs = new GetPullRequest(_settings.ProjectKey, _settings.RepoSlug, _settings);
            var result = await getPullReqs.SendAsync().ConfigureAwait(false);
            if (result.Success)
            {
                list.AddRange(result.Result);
            }

            return list;
        }

        private void BtnCreateClick(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                if (ddlBranchSource.SelectedValue == null ||
                    ddlBranchTarget.SelectedValue == null ||
                    ddlRepositorySource.SelectedValue == null ||
                    ddlRepositoryTarget.SelectedValue == null)
                {
                    return;
                }

                var info = new PullRequestInfo
                {
                    Title = txtTitle.Text,
                    Description = txtDescription.Text,
                    SourceBranch = ddlBranchSource.SelectedValue.ToString(),
                    TargetBranch = ddlBranchTarget.SelectedValue.ToString(),
                    SourceRepo = (Repository)ddlRepositorySource.SelectedValue,
                    TargetRepo = (Repository)ddlRepositoryTarget.SelectedValue,
                    Reviewers = _reviewers
                };
                var pullRequest = new CreatePullRequestRequest(_settings, info);
                var response = await pullRequest.SendAsync();
                await this.SwitchToMainThreadAsync();
                if (response.Success)
                {
                    MessageBox.Show(_success.Text);
                    BitbucketViewPullRequestFormLoad(null, null);
                }
                else
                {
                    MessageBox.Show(string.Join(Environment.NewLine, response.Messages),
                        _error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        private readonly Dictionary<Repository, IEnumerable<string>> _branches = new Dictionary<Repository, IEnumerable<string>>();
        private async Task<IEnumerable<string>> GetBitbucketBranchesAsync(Repository selectedRepo)
        {
            lock (_branches)
            {
                if (_branches.ContainsKey(selectedRepo))
                {
                    return _branches[selectedRepo];
                }
            }

            var list = new List<string>();
            var getBranches = new GetBranchesRequest(selectedRepo, _settings);
            var result = await getBranches.SendAsync().ConfigureAwait(false);
            if (result.Success)
            {
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
            List<string> branchNames = (await GetBitbucketBranchesAsync((Repository)selectedValue)).ToList();
            if (AppSettings.BranchOrderingCriteria == BranchOrdering.Alphabetically)
            {
                branchNames.Sort();
            }

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

        private async Task<Commit> GetCommitInfoAsync(Repository repo, string branch)
        {
            if (repo == null || string.IsNullOrWhiteSpace(branch))
            {
                return null;
            }

            var getCommit = new GetHeadCommitRequest(repo, branch, _settings);
            var result = await getCommit.SendAsync().ConfigureAwait(false);
            return result.Success ? result.Result : null;
        }

        private void UpdateCommitInfo(Label label, Commit commit)
        {
            if (commit == null)
            {
                label.Text = string.Empty;
            }
            else
            {
                label.Text = string.Format(_commited.Text,
                    commit.AuthorName, commit.Message);
            }
        }

        private async Task UpdatePullRequestDescriptionAsync()
        {
            await this.SwitchToMainThreadAsync();

            if (ddlRepositorySource.SelectedValue == null
                || ddlRepositoryTarget.SelectedValue == null
                || ddlBranchSource.Tag == null
                || ddlBranchTarget.Tag == null)
            {
                return;
            }

            var getCommitsInBetween = new GetInBetweenCommitsRequest(
                (Repository)ddlRepositorySource.SelectedValue,
                (Repository)ddlRepositoryTarget.SelectedValue,
                (Commit)ddlBranchSource.Tag,
                (Commit)ddlBranchTarget.Tag,
                _settings);

            var result = await getCommitsInBetween.SendAsync();
            if (result.Success)
            {
                await this.SwitchToMainThreadAsync();

                var sb = new StringBuilder();
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

            lblLinkViewPull.Text = string.Format("{0}/projects/{1}/repos/{2}/pull-requests/{3}/overview",
                _settings.BitbucketUrl, _settings.ProjectKey, _settings.RepoSlug, curItem.Id);
        }

        private void BtnMergeClick(object sender, EventArgs e)
        {
            if (lbxPullRequests.SelectedItem is PullRequest curItem)
            {
                var mergeInfo = new MergeRequestInfo
                {
                    Id = curItem.Id,
                    Version = curItem.Version,
                    ProjectKey = curItem.DestProjectKey,
                    TargetRepo = curItem.DestRepo,
                };

                // Merge
                var mergeRequest = new MergePullRequest(_settings, mergeInfo);
                var response = ThreadHelper.JoinableTaskFactory.Run(() => mergeRequest.SendAsync());
                if (response.Success)
                {
                    MessageBox.Show(_success.Text);
                    BitbucketViewPullRequestFormLoad(null, null);
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
                var mergeInfo = new MergeRequestInfo
                {
                    Id = curItem.Id,
                    Version = curItem.Version,
                    ProjectKey = curItem.DestProjectKey,
                    TargetRepo = curItem.DestRepo,
                };

                // Approve
                var approveRequest = new ApprovePullRequest(_settings, mergeInfo);
                var response = ThreadHelper.JoinableTaskFactory.Run(() => approveRequest.SendAsync());
                if (response.Success)
                {
                    MessageBox.Show(_success.Text);
                    BitbucketViewPullRequestFormLoad(null, null);
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
                    Clipboard.SetText(link);
                }
                else
                {
                    System.Diagnostics.Process.Start(link);
                }
            }
            catch
            {
            }
        }
    }
}
