using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeleteUnusedBranches.Properties;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
namespace DeleteUnusedBranches
{
    public sealed partial class DeleteUnusedBranchesForm : GitExtensionsFormBase
    {
        private readonly TranslationString _deleteCaption = new TranslationString("Delete");
        private readonly TranslationString _selectBranchesToDelete = new TranslationString("Select branches to delete using checkboxes in '{0}' column.");
        private readonly TranslationString _areYouSureToDelete = new TranslationString("Are you sure to delete {0} selected branches?");
        private readonly TranslationString _dangerousAction = new TranslationString("DANGEROUS ACTION!\nBranches will be deleted on the remote '{0}'. This can not be undone.\nAre you sure you want to continue?");
        private readonly TranslationString _deletingBranches = new TranslationString("Deleting branches...");
        private readonly TranslationString _deletingUnmergedBranches = new TranslationString("Deleting unmerged branches will result in dangling commits. Use with caution!");
        private readonly TranslationString _chooseBranchesToDelete = new TranslationString("Choose branches to delete. Only branches that are fully merged in '{0}' will be deleted.");
        private readonly TranslationString _pressToSearch = new TranslationString("Press '{0}' to search for branches to delete.");
        private readonly TranslationString _cancel = new TranslationString("Cancel");
        private readonly TranslationString _searchBranches = new TranslationString("Search branches");
        private readonly TranslationString _loading = new TranslationString("Loading...");
        private readonly TranslationString _branchesSelected = new TranslationString("{0}/{1} branches selected.");
        private readonly DeleteUnusedBranchesFormSettings _settings;

        private readonly SortableBranchesList _branches = new SortableBranchesList();
        private readonly IGitModule _gitCommands;
        private readonly IGitUICommands _gitUiCommands;
        private readonly IGitPlugin _gitPlugin;
        private CancellationTokenSource _refreshCancellation;

        public DeleteUnusedBranchesForm(DeleteUnusedBranchesFormSettings settings, IGitModule gitCommands, IGitUICommands gitUiCommands, IGitPlugin gitPlugin)
        {
            _settings = settings;
            _gitCommands = gitCommands;
            _gitUiCommands = gitUiCommands;
            _gitPlugin = gitPlugin;

            InitializeComponent();

            _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn.Width = DpiUtil.Scale(50);
            dateDataGridViewTextBoxColumn.Width = DpiUtil.Scale(175);
            Author.Width = DpiUtil.Scale(91);

            imgLoading.Image = Resources.loadingpanel;

            _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn.DataPropertyName = nameof(Branch.Delete);
            nameDataGridViewTextBoxColumn.DataPropertyName = nameof(Branch.Name);
            dateDataGridViewTextBoxColumn.DataPropertyName = nameof(Branch.Date);
            Author.DataPropertyName = nameof(Branch.Author);
            Message.DataPropertyName = nameof(Branch.Message);

            InitializeComplete();

            ThreadHelper.JoinableTaskFactory.RunAsync(() => RefreshObsoleteBranchesAsync());
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            mergedIntoBranch.Text = _settings.MergedInBranch;
            olderThanDays.Value = _settings.DaysOlderThan;
            IncludeRemoteBranches.Checked = _settings.DeleteRemoteBranchesFromFlag;
            _NO_TRANSLATE_Remote.Text = _settings.RemoteName;
            useRegexFilter.Checked = _settings.UseRegexToFilterBranchesFlag;
            regexFilter.Text = _settings.RegexFilter;
            useRegexCaseInsensitive.Checked = _settings.RegexCaseInsensitiveFlag;
            regexDoesNotMatch.Checked = _settings.RegexInvertedFlag;
            includeUnmergedBranches.Checked = _settings.IncludeUnmergedBranchesFlag;

            checkBoxHeaderCell.CheckBoxClicked += CheckBoxHeader_OnCheckBoxClicked;
            _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn.HeaderText = string.Empty;

            BranchesGrid.DataSource = _branches;
        }

        private static IEnumerable<Branch> GetObsoleteBranches(RefreshContext context, string curBranch)
        {
            foreach (string branchName in GetObsoleteBranchNames(context, curBranch))
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var args = new GitArgumentBuilder("log")
                {
                    "--pretty=%ci\n%an\n%s",
                    $"{branchName}^1..{branchName}"
                };

                var commitLog = context.Commands.GitExecutable.GetOutput(args).Split('\n');
                DateTime.TryParse(commitLog[0], out var commitDate);
                var authorName = commitLog.Length > 1 ? commitLog[1] : string.Empty;
                var message = commitLog.Length > 2 ? commitLog[2] : string.Empty;

                yield return new Branch(branchName, commitDate, authorName, message, commitDate < DateTime.Now - context.ObsolescenceDuration);
            }
        }

        private static IEnumerable<string> GetObsoleteBranchNames(RefreshContext context, string curBranch)
        {
            RegexOptions options;
            if (context.RegexIgnoreCase)
            {
                options = RegexOptions.Compiled | RegexOptions.IgnoreCase;
            }
            else
            {
                options = RegexOptions.Compiled;
            }

            var regex = string.IsNullOrEmpty(context.RegexFilter) ? null : new Regex(context.RegexFilter, options);
            bool regexMustMatch = !context.RegexDoesNotMatch;

            var args = new GitArgumentBuilder("branch")
            {
                 { context.IncludeRemotes, "-r" },
                 { !context.IncludeUnmerged, "--merged" },
                 context.ReferenceBranch
            };

            string[] branches = context.Commands.GitExecutable.GetOutput(args)
                .Split('\n')
                .Where(branchName => !string.IsNullOrEmpty(branchName))
                .Select(branchName => branchName.Trim('*', ' ', '\n', '\r'))
                .Where(branchName => branchName != "HEAD" && branchName != curBranch &&
                                   branchName != context.ReferenceBranch)
                .ToArray();

            return branches.Where(branchName => (!context.IncludeRemotes || branchName.StartsWith(context.RemoteRepositoryName + "/")) &&
                                   (regex == null || regex.IsMatch(branchName) == regexMustMatch)).ToArray();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            var selectedBranches = _branches.Where(branch => branch.Delete).ToList();
            if (selectedBranches.Count == 0)
            {
                MessageBox.Show(string.Format(_selectBranchesToDelete.Text, _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn.HeaderText), _deleteCaption.Text);
                return;
            }

            if (MessageBox.Show(this, string.Format(_areYouSureToDelete.Text, selectedBranches.Count), _deleteCaption.Text, MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            var remoteName = _NO_TRANSLATE_Remote.Text;
            var remoteBranchPrefix = remoteName + "/";
            var remoteBranchesSource = IncludeRemoteBranches.Checked
                ? selectedBranches.Where(branch => branch.Name.StartsWith(remoteBranchPrefix))
                : Enumerable.Empty<Branch>();
            var remoteBranches = remoteBranchesSource.ToList();

            if (remoteBranches.Count > 0)
            {
                var message = string.Format(_dangerousAction.Text, remoteName);
                if (MessageBox.Show(this, message, _deleteCaption.Text, MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }

            var localBranches = selectedBranches.Except(remoteBranches).ToList();
            tableLayoutPanel2.Enabled = tableLayoutPanel3.Enabled = false;
            imgLoading.Visible = true;
            lblStatus.Text = _deletingBranches.Text;

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default.SwitchTo(alwaysYield: true);
                foreach (var remoteBranch in remoteBranches)
                {
                    // Delete branches one by one, because it is possible one fails
                    var remoteBranchNameOffset = remoteBranchPrefix.Length;
                    var args = new GitArgumentBuilder("push")
                    {
                        remoteName,
                        $":{remoteBranch.Name.Substring(remoteBranchNameOffset)}"
                    };
                    _gitCommands.GitExecutable.GetOutput(args);
                }

                foreach (var localBranch in localBranches)
                {
                    var args = new GitArgumentBuilder("branch")
                    {
                        "-d",
                        localBranch.Name
                    };
                    _gitCommands.GitExecutable.GetOutput(args);

                    // Delete branches one by one, because it is possible one fails
                    _gitCommands.GitExecutable.GetOutput(args);
                }

                await this.SwitchToMainThreadAsync();

                tableLayoutPanel2.Enabled = tableLayoutPanel3.Enabled = true;
                await RefreshObsoleteBranchesAsync().ConfigureAwait(false);
            });
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            Hide();
            Close();
            _gitUiCommands.StartSettingsDialog(_gitPlugin);
        }

        private void includeUnmergedBranches_CheckedChanged(object sender, EventArgs e)
        {
            ClearResults(sender, e);

            if (includeUnmergedBranches.Checked)
            {
                MessageBox.Show(this, _deletingUnmergedBranches.Text, _deleteCaption.Text, MessageBoxButtons.OK);
            }
        }

        private void ClearResults(object sender, EventArgs e)
        {
            instructionLabel.Text = string.Format(_chooseBranchesToDelete.Text, mergedIntoBranch.Text);
            lblStatus.Text = string.Format(_pressToSearch.Text, RefreshBtn.Text);
            _branches.Clear();
            _branches.ResetBindings();
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(() => RefreshObsoleteBranchesAsync());
        }

        private void CheckBoxHeader_OnCheckBoxClicked(object sender, CheckBoxHeaderCellEventArgs e)
        {
            BranchesGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);

            for (int i = 0; i < BranchesGrid.Rows.Count; i++)
            {
                DataGridViewRow row = BranchesGrid.Rows[i];
                DataGridViewCheckBoxCell cell =
                    (DataGridViewCheckBoxCell)row.Cells[nameof(_NO_TRANSLATE_deleteDataGridViewCheckBoxColumn)];
                cell.Value = e.Checked;
            }

            BranchesGrid.EndEdit();
        }

        private void BranchesGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // track only “Deleted” column, ignoring the checkbox header
            if (e.ColumnIndex != 0 || e.RowIndex == -1)
            {
                return;
            }

            BranchesGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            checkBoxHeaderCell.Checked = _branches.All(b => b.Delete);
            lblStatus.Text = GetDefaultStatusText();
        }

        private async Task RefreshObsoleteBranchesAsync()
        {
            if (IsRefreshing)
            {
                _refreshCancellation.Cancel();
                IsRefreshing = false;
                return;
            }

            IsRefreshing = true;
            var curBranch = _gitUiCommands.GitModule.GetSelectedBranch();
            var context = new RefreshContext(
                _gitCommands,
                IncludeRemoteBranches.Checked,
                includeUnmergedBranches.Checked,
                mergedIntoBranch.Text,
                _NO_TRANSLATE_Remote.Text,
                useRegexFilter.Checked ? regexFilter.Text : null,
                useRegexCaseInsensitive.Checked,
                regexDoesNotMatch.Checked,
                TimeSpan.FromDays((int)olderThanDays.Value),
                _refreshCancellation.Token);

            await TaskScheduler.Default.SwitchTo(alwaysYield: true);

            IEnumerable<Branch> branches;
            try
            {
                branches = GetObsoleteBranches(context, curBranch);
            }
            catch
            {
                await this.SwitchToMainThreadAsync();
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                throw;
            }

            await this.SwitchToMainThreadAsync();
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            _branches.Clear();
            _branches.AddRange(branches);
            checkBoxHeaderCell.Checked = _branches.All(b => b.Delete);
            _branches.ResetBindings();

            IsRefreshing = false;
        }

        private bool IsRefreshing
        {
            get => _refreshCancellation != null;
            set
            {
                if (value == IsRefreshing)
                {
                    return;
                }

                _refreshCancellation = value ? new CancellationTokenSource() : null;
                RefreshBtn.Text = value ? _cancel.Text : _searchBranches.Text;
                imgLoading.Visible = value;
                lblStatus.Text = value ? _loading.Text : GetDefaultStatusText();
            }
        }

        private string GetDefaultStatusText()
        {
            return string.Format(_branchesSelected.Text, _branches.Count(b => b.Delete), _branches.Count);
        }

        private readonly struct RefreshContext
        {
            public RefreshContext(IGitModule commands, bool includeRemotes, bool includeUnmerged, string referenceBranch,
                string remoteRepositoryName, string regexFilter, bool regexIgnoreCase, bool regexDoesNotMatch,
                TimeSpan obsolescenceDuration, CancellationToken cancellationToken)
            {
                Commands = commands;
                IncludeRemotes = includeRemotes;
                IncludeUnmerged = includeUnmerged;
                ReferenceBranch = referenceBranch;
                RemoteRepositoryName = remoteRepositoryName;
                RegexFilter = regexFilter;
                RegexIgnoreCase = regexIgnoreCase;
                RegexDoesNotMatch = regexDoesNotMatch;
                ObsolescenceDuration = obsolescenceDuration;
                CancellationToken = cancellationToken;
            }

            public IGitModule Commands { get; }
            public bool IncludeRemotes { get; }
            public bool IncludeUnmerged { get; }
            public string ReferenceBranch { get; }
            public string RemoteRepositoryName { get; }
            public string RegexFilter { get; }
            public bool RegexIgnoreCase { get; }
            public bool RegexDoesNotMatch { get; }
            public TimeSpan ObsolescenceDuration { get; }
            public CancellationToken CancellationToken { get; }
        }
    }
}
