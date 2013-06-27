using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using System.Linq;

namespace Stash
{
    public partial class StashPullRequestForm : Form
    {
        private Settings _settings;
        private readonly GitUIBaseEventArgs _gitUiCommands;
        private readonly IGitPluginSettingsContainer _settingsContainer;
        private readonly BindingList<StashUser> _reviewers = new BindingList<StashUser>();
        private readonly List<string> _stashUsers = new List<string>();

        public StashPullRequestForm(GitUIBaseEventArgs gitUiCommands,
            IGitPluginSettingsContainer settings)
        {
            InitializeComponent();

            _gitUiCommands = gitUiCommands;
            _settingsContainer = settings;
        }

        private void StashPullRequestFormLoad(object sender, EventArgs e)
        {
            _settings = Settings.Parse(_gitUiCommands.GitModule, _settingsContainer);
            if (_settings == null)
            {
                MessageBox.Show("Your repository is not hosted in Stash.");
                Close();
                return;
            }

            _stashUsers.AddRange(GetStashUsers().Select(a => a.Slug));

            var relatedRepos = GetRelatedRepos();

            ddlRepositorySource.DataSource = relatedRepos.ToList();
            ddlRepositoryTarget.DataSource = relatedRepos.ToList();

            ReviewersDataGrid.DataSource = _reviewers;
        }

        private List<Repository> GetRelatedRepos()
        {
            var list = new List<Repository>();
            var getRelatedRepos = new GetRelatedRepoRequest(_settings);
            var result = getRelatedRepos.Send();
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
                               SourceBranch = txtSourceBranch.Text,
                               TargetBranch = txtTargetBranch.Text,
                               SourceRepo = (Repository) ddlRepositorySource.SelectedValue,
                               TargetRepo = (Repository) ddlRepositoryTarget.SelectedValue,
                               Reviewers = _reviewers
                           };

            var pullRequest = new CreatePullRequestRequest(_settings, info);
            var response = pullRequest.Send();
            if (response.Success)
                MessageBox.Show("Success");
            else
                MessageBox.Show(string.Join(Environment.NewLine, response.Messages));
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

        private IEnumerable<string> GetStashBranches(Repository selectedRepo)
        {
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
            RefreshAutoCompleteBranch(txtSourceBranch, ((ComboBox) sender).SelectedValue);
        }

        private void RefreshAutoCompleteBranch(TextBox textBox, object selectedValue)
        {
            var branches = GetStashBranches((Repository) selectedValue);
            textBox.AutoCompleteCustomSource.Clear();
            textBox.AutoCompleteCustomSource.AddRange(branches.ToArray());
        }

        private void DdlRepositoryTargetSelectedValueChanged(object sender, EventArgs e)
        {
            RefreshAutoCompleteBranch(txtTargetBranch, ((ComboBox)sender).SelectedValue);
        }
    }
}
