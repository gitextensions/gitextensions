using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using System.Linq;
using System.Threading;
using ResourceManager;

namespace Stash
{
    public partial class StashPullRequestForm : GitExtensionsFormBase
    {
        private readonly TranslationString _yourRepositoryIsNotInStash = new TranslationString("Your repository is not hosted in Stash.");
        private readonly TranslationString _commited = new TranslationString("{0} committed\n{1}");
        private readonly TranslationString _success = new TranslationString("Success");
        private readonly TranslationString _error = new TranslationString("Error");

        private Settings _settings;
        private readonly GitUIBaseEventArgs _gitUiCommands;
        private readonly ISettingsSource _settingsContainer;
        private readonly BindingList<StashUser> _reviewers = new BindingList<StashUser>();
        private readonly List<string> _stashUsers = new List<string>();


        public StashPullRequestForm(GitUIBaseEventArgs gitUiCommands,
            ISettingsSource settings)
        {
            InitializeComponent();
            Translate();

            _gitUiCommands = gitUiCommands;
            _settingsContainer = settings;
        }

        private void StashPullRequestFormLoad(object sender, EventArgs e)
        {
            _settings = Settings.Parse(_gitUiCommands.GitModule, _settingsContainer);
            if (_settings == null)
            {
                MessageBox.Show(_yourRepositoryIsNotInStash.Text);
                Close();
                return;
            }
            //_stashUsers.AddRange(GetStashUsers().Select(a => a.Slug));
            ThreadPool.QueueUserWorkItem(state =>
            {
                var repositories = GetRepositories();
                try
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        ddlRepositorySource.DataSource = repositories.ToList();
                        ddlRepositoryTarget.DataSource = repositories.ToList();
                        ddlRepositorySource.Enabled = true;
                        ddlRepositoryTarget.Enabled = true;
                    });
                }
                catch (System.InvalidOperationException)
                {
                    return;
                }
            });
        }
        private void StashViewPullRequestFormLoad(object sender, EventArgs e)
        {
            if (_settings == null)
                return;
            ThreadPool.QueueUserWorkItem(state =>
            {
                var pullReqs = GetPullRequests();
                try
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        lbxPullRequests.DataSource = pullReqs;
                        lbxPullRequests.DisplayMember = "DisplayName";
                    });
                }
                catch(System.InvalidOperationException){
                    return;
                }

            });
        }

        private List<Repository> GetRepositories()
        {
            var list = new List<Repository>();
            var getDefaultRepo = new GetRepoRequest(_settings.ProjectKey, _settings.RepoSlug, _settings);
            var defaultRepo = getDefaultRepo.Send();
            if (defaultRepo.Success)
                list.Add(defaultRepo.Result);
            //var getRelatedRepos = new GetRelatedRepoRequest(_settings);
            //var result = getRelatedRepos.Send();
            //if (result.Success)
            //{
            //    list.AddRange(result.Result);
            //}
            return list;
        }

        private List<PullRequest> GetPullRequests()
        {
            var list = new List<PullRequest>();
            var getPullReqs = new GetPullRequest(_settings.ProjectKey, _settings.RepoSlug, _settings);
            var result = getPullReqs.Send();
            if (result.Success)
            {
                list.AddRange(result.Result);
            }
            return list;
        }

        private void BtnCreateClick(object sender, EventArgs e)
        {
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
            var response = pullRequest.Send();
            if (response.Success)
            {
                MessageBox.Show(_success.Text);
                StashViewPullRequestFormLoad(null, null);
            }
            else
                MessageBox.Show(string.Join(Environment.NewLine, response.Messages),
                    _error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private IEnumerable<StashUser> GetStashUsers()
        {
            var list = new List<StashUser>();
            var getUser = new GetUserRequest(_settings);
            var result = getUser.Send();
            if (result.Success)
            {
                foreach (var value in result.Result["values"])
                {
                    list.Add(new StashUser { Slug = value["slug"].ToString() });
                }
            }
            return list;
        }
        Dictionary<Repository, IEnumerable<string>> Branches = new Dictionary<Repository,IEnumerable<string>>();
        private IEnumerable<string> GetStashBranches(Repository selectedRepo)
        {
            if (Branches.ContainsKey(selectedRepo))
            {
                return Branches[selectedRepo];
            }
            var list = new List<string>();
            var getBranches = new GetBranchesRequest(selectedRepo, _settings);
            var result = getBranches.Send();
            if (result.Success)
            {
                foreach (var value in result.Result["values"])
                {
                    list.Add(value["displayId"].ToString());
                }
            }
            Branches.Add(selectedRepo, list);
            return list;
        }

        private void ReviewersDataGridEditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            var cellEdit = e.Control as DataGridViewTextBoxEditingControl;
            if (cellEdit != null)
            {
                cellEdit.AutoCompleteCustomSource = new AutoCompleteStringCollection();
                cellEdit.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cellEdit.AutoCompleteSource = AutoCompleteSource.CustomSource;
                cellEdit.AutoCompleteCustomSource.AddRange(_stashUsers.ToArray());
            }
        }

        private void DdlRepositorySourceSelectedValueChanged(object sender, EventArgs e)
        {
            RefreshDDLBranch(ddlBranchSource, ((ComboBox)sender).SelectedValue);
        }

        private void DdlRepositoryTargetSelectedValueChanged(object sender, EventArgs e)
        {
            RefreshDDLBranch(ddlBranchTarget, ((ComboBox)sender).SelectedValue);
        }

        private void RefreshDDLBranch(ComboBox comboBox, object selectedValue)
        {
            List<string> lsNames = (GetStashBranches((Repository)selectedValue)).ToList();
            lsNames.Sort();
            lsNames.Insert(0, "");
            comboBox.DataSource = lsNames;
        }

        private void DdlBranchSourceSelectedValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlBranchSource.SelectedValue.ToString())) return;
            var commit = GetCommitInfo((Repository)ddlRepositorySource.SelectedValue,
                                                ddlBranchSource.SelectedValue.ToString());

            ddlBranchSource.Tag = commit;
            UpdateCommitInfo(lblCommitInfoSource, commit);
            txtTitle.Text = ddlBranchSource.SelectedValue.ToString().Replace("-"," ");
            UpdatePullRequestDescription();
        }

        private void DdlBranchTargetSelectedValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlBranchTarget.SelectedValue.ToString())) return;
            var commit = GetCommitInfo((Repository)ddlRepositoryTarget.SelectedValue,
                                                ddlBranchTarget.SelectedValue.ToString());

            ddlBranchTarget.Tag = commit;
            UpdateCommitInfo(lblCommitInfoTarget, commit);
            UpdatePullRequestDescription();
        }

        private Commit GetCommitInfo(Repository repo, string branch)
        {
            if (repo == null || string.IsNullOrWhiteSpace(branch))
                return null;
            var getCommit = new GetHeadCommitRequest(repo, branch, _settings);
            var result = getCommit.Send();
            return result.Success ? result.Result : null;
        }

        private void UpdateCommitInfo(Label label, Commit commit)
        {
            if (commit == null)
                label.Text = string.Empty;
            else
                label.Text = string.Format(_commited.Text,
                    commit.AuthorName, commit.Message);
        }

        private void UpdatePullRequestDescription()
        {
            if (ddlRepositorySource.SelectedValue == null
                || ddlRepositoryTarget.SelectedValue == null
                || ddlBranchSource.Tag == null
                || ddlBranchTarget.Tag == null)
                return;

            var getCommitsInBetween = new GetInBetweenCommitsRequest(
                (Repository)ddlRepositorySource.SelectedValue,
                (Repository)ddlRepositoryTarget.SelectedValue,
                (Commit)ddlBranchSource.Tag,
                (Commit)ddlBranchTarget.Tag,
                _settings);

            var result = getCommitsInBetween.Send();
            if (result.Success)
            {
                var sb = new StringBuilder();
                sb.AppendLine();
                foreach (var commit in result.Result)
                {
                    if (!commit.IsMerge)
                        sb.Append("* ").AppendLine(commit.Message);
                }
                txtDescription.Text = sb.ToString();
            }
        }
        private void PullRequestChanged(object sender, EventArgs e)
        {
            var curItem = lbxPullRequests.SelectedItem as PullRequest;

            txtPRTitle.Text = curItem.Title;
            txtPRDescription.Text = curItem.Description;
            lblPRAuthor.Text = curItem.Author;
            lblPRState.Text = curItem.State;
            txtPRReviewers.Text = curItem.Reviewers;
            lblPRSourceRepo.Text = curItem.SrcDisplayName;
            lblPRSourceBranch.Text = curItem.SrcBranch;
            lblPRDestRepo.Text = curItem.DestDisplayName;
            lblPRDestBranch.Text = curItem.DestBranch;
        }

        private void BtnMergeClick(object sender, EventArgs e)
        {
            var curItem = lbxPullRequests.SelectedItem as PullRequest;
            var mergeInfo = new MergeRequestInfo
            {
                Id = curItem.Id,
                Version = curItem.Version,
                ProjectKey = curItem.DestProjectKey,
                TargetRepo = curItem.DestRepo,
            };

            //Merge
            var mergeRequest = new MergePullRequest(_settings, mergeInfo);
            var response = mergeRequest.Send();
            if (response.Success)
            {
                MessageBox.Show(_success.Text);
                StashViewPullRequestFormLoad(null, null);
            }
            else
                MessageBox.Show(string.Join(Environment.NewLine, response.Messages),
                    _error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void BtnApproveClick(object sender, EventArgs e)
        {
            var curItem = lbxPullRequests.SelectedItem as PullRequest;
            var mergeInfo = new MergeRequestInfo
            {
                Id = curItem.Id,
                Version = curItem.Version,
                ProjectKey = curItem.DestProjectKey,
                TargetRepo = curItem.DestRepo,
            };

            //Approve
            var approveRequest = new ApprovePullRequest(_settings, mergeInfo);
            var response = approveRequest.Send();
            if (response.Success)
            {
                MessageBox.Show(_success.Text);
                    StashViewPullRequestFormLoad(null, null);
            }
            else
                MessageBox.Show(string.Join(Environment.NewLine, response.Messages),
                    _error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
