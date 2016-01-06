using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeleteUnusedBranches.Properties;
using GitUIPluginInterfaces;
using System.Text.RegularExpressions;
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
        private readonly TranslationString _chooseBrancesToDelete = new TranslationString("Choose branches to delete. Only branches that are fully merged in '{0}' will be deleted.");
        private readonly TranslationString _pressToSearch = new TranslationString("Press '{0}' to search for branches to delete.");
        private readonly TranslationString _cancel = new TranslationString("Cancel");
        private readonly TranslationString _searchBranches = new TranslationString("Search branches");
        private readonly TranslationString _loading = new TranslationString("Loading...");
        private readonly TranslationString _branchesSelected = new TranslationString("{0}/{1} branches selected.");

        private readonly SortableBranchesList _branches = new SortableBranchesList();
        private int _days;
        private string _referenceBranch;
        private readonly IGitModule _gitCommands;
        private readonly IGitUICommands _gitUiCommands;
        private readonly IGitPlugin _gitPlugin;
        private CancellationTokenSource _refreshCancellation;

        public DeleteUnusedBranchesForm()
        {
            InitializeComponent();
            Translate();
        }

        public DeleteUnusedBranchesForm(int days, string referenceBranch, IGitModule gitCommands, IGitUICommands gitUiCommands, IGitPlugin gitPlugin)
        {
            InitializeComponent();
            Translate();

            this._referenceBranch = referenceBranch;
            this._days = days;
            this._gitCommands = gitCommands;
            _gitUiCommands = gitUiCommands;
            _gitPlugin = gitPlugin;
            imgLoading.Image = IsMonoRuntime() ? Resources.loadingpanel_static : Resources.loadingpanel;
            RefreshObsoleteBranches();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            mergedIntoBranch.Text = _referenceBranch;
            olderThanDays.Value = _days;

            BranchesGrid.DataSource = _branches;
            ClearResults();
        }

        private static IEnumerable<Branch> GetObsoleteBranches(RefreshContext context, string curBranch)
        {
            foreach (string branchName in GetObsoleteBranchNames(context, curBranch))
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var commitLog = context.Commands.RunGitCmd(string.Concat("log --pretty=%ci\n%an\n%s ", branchName, "^1..", branchName)).Split('\n');
                DateTime commitDate;
                DateTime.TryParse(commitLog[0], out commitDate);
                var authorName = commitLog.Length > 1 ? commitLog[1] : string.Empty;
                var message = commitLog.Length > 2 ? commitLog[2] : string.Empty;

                yield return new Branch(branchName, commitDate, authorName, message, commitDate < DateTime.Now - context.ObsolescenceDuration);
            }
        }

        private static IEnumerable<string> GetObsoleteBranchNames(RefreshContext context, string curBranch)
        {
            var regex = string.IsNullOrEmpty(context.RegexFilter) ? null : new Regex(context.RegexFilter, RegexOptions.Compiled);

            return context.Commands.RunGitCmd("branch" + (context.IncludeRemotes ? " -r" : "") + (context.IncludeUnmerged ? "" : " --merged " + context.ReferenceBranch))
                .Split('\n')
                .Where(branchName => !string.IsNullOrEmpty(branchName))
                .Select(branchName => branchName.Trim('*', ' ', '\n', '\r'))
                .Where(branchName => branchName != "HEAD" && branchName != curBranch &&
                                     branchName != context.ReferenceBranch &&
                                     (!context.IncludeRemotes || branchName.StartsWith(context.RemoteRepositoryName + "/")) &&
                                     (regex == null || regex.IsMatch(branchName)));
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            var selectedBranches = _branches.Where(branch => branch.Delete).ToList();
            if (selectedBranches.Count == 0)
            {
                MessageBox.Show(string.Format(_selectBranchesToDelete.Text, deleteDataGridViewCheckBoxColumn.HeaderText), _deleteCaption.Text);
                return;
            }

            if (MessageBox.Show(this, string.Format(_areYouSureToDelete.Text, selectedBranches.Count), _deleteCaption.Text, MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

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
                    return;
            }

            var localBranches = selectedBranches.Except(remoteBranches).ToList();
            tableLayoutPanel2.Enabled = tableLayoutPanel3.Enabled = false;
            imgLoading.Visible = true;
            lblStatus.Text = _deletingBranches.Text;

            Task.Factory.StartNew(() =>
            {
                if (remoteBranches.Count > 0)
                {
                    // TODO: use GitCommandHelpers.PushMultipleCmd after moving this window to GE (see FormPush as example)
                    var remoteBranchNameOffset = remoteBranchPrefix.Length;
                    var remoteBranchNames = string.Join(" ", remoteBranches.Select(branch => ":" + branch.Name.Substring(remoteBranchNameOffset)));
                    _gitCommands.RunGitCmd(string.Format("push {0} {1}", remoteName, remoteBranchNames));
                }

                if (localBranches.Count > 0)
                {
                    var localBranchNames = string.Join(" ", localBranches.Select(branch => branch.Name));
                    _gitCommands.RunGitCmd("branch -d " + localBranchNames);
                }
            })
            .ContinueWith(_ =>
            {
                if (IsDisposed)
                    return;

                tableLayoutPanel2.Enabled = tableLayoutPanel3.Enabled = true;
                RefreshObsoleteBranches();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            Hide();
            Close();
            _gitUiCommands.StartSettingsDialog(_gitPlugin);
        }

        private void IncludeRemoteBranches_CheckedChanged(object sender, EventArgs e)
        {
            ClearResults();
        }

        private void useRegexFilter_CheckedChanged(object sender, EventArgs e)
        {
            ClearResults();
        }

        private void remote_TextChanged(object sender, EventArgs e)
        {
            ClearResults();
        }

        private void regexFilter_TextChanged(object sender, EventArgs e)
        {
            ClearResults();
        }

        private void mergedIntoBranch_TextChanged(object sender, EventArgs e)
        {
            _referenceBranch = mergedIntoBranch.Text;
            ClearResults();
        }

        private void includeUnmergedBranches_CheckedChanged(object sender, EventArgs e)
        {
            ClearResults();

            if (includeUnmergedBranches.Checked)
                MessageBox.Show(this, _deletingUnmergedBranches.Text, _deleteCaption.Text, MessageBoxButtons.OK);
        }

        private void olderThanDays_ValueChanged(object sender, EventArgs e)
        {
            _days = (int)olderThanDays.Value;
            ClearResults();
        }

        private void ClearResults()
        {
            instructionLabel.Text = string.Format(_chooseBrancesToDelete.Text, _referenceBranch);
            lblStatus.Text = string.Format(_pressToSearch.Text, RefreshBtn.Text);
            _branches.Clear();
            _branches.ResetBindings();
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            RefreshObsoleteBranches();
        }

        private void BranchesGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // track only “Deleted” column
            if (e.ColumnIndex != 0)
                return;

            BranchesGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            lblStatus.Text = GetDefaultStatusText();
        }

        private void RefreshObsoleteBranches()
        {
            if (IsRefreshing)
            {
                _refreshCancellation.Cancel();
                IsRefreshing = false;
                return;
            }

            IsRefreshing = true;
            var curBranch = _gitUiCommands.GitModule.GetSelectedBranch();
            var context = new RefreshContext(_gitCommands, IncludeRemoteBranches.Checked, includeUnmergedBranches.Checked, _referenceBranch, _NO_TRANSLATE_Remote.Text,
                useRegexFilter.Checked ? regexFilter.Text : null, TimeSpan.FromDays(_days), _refreshCancellation.Token);
            Task.Factory.StartNew(() => GetObsoleteBranches(context, curBranch).ToList(), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default)
                .ContinueWith(task =>
                {
                    if (IsDisposed || context.CancellationToken.IsCancellationRequested)
                        return;

                    if (task.IsCompleted)
                    {
                        _branches.Clear();
                        _branches.AddRange(task.Result);
                        _branches.ResetBindings();
                    }

                    IsRefreshing = false;
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private bool IsRefreshing
        {
            get
            {
                return _refreshCancellation != null;
            }
            set
            {
                if (value == IsRefreshing)
                    return;

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

        private static bool IsMonoRuntime()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        private struct RefreshContext
        {
            private readonly IGitModule _commands;
            private readonly bool _includeRemotes;
            private readonly bool _includeUnmerged;
            private readonly string _referenceBranch;
            private readonly string _remoteRepositoryName;
            private readonly string _regexFilter;
            private readonly TimeSpan _obsolescenceDuration;
            private readonly CancellationToken _cancellationToken;

            public RefreshContext(IGitModule commands, bool includeRemotes, bool includeUnmerged, string referenceBranch,
                string remoteRepositoryName, string regexFilter, TimeSpan obsolescenceDuration, CancellationToken cancellationToken)
            {
                this._commands = commands;
                this._includeRemotes = includeRemotes;
                this._includeUnmerged = includeUnmerged;
                this._referenceBranch = referenceBranch;
                this._remoteRepositoryName = remoteRepositoryName;
                this._regexFilter = regexFilter;
                this._obsolescenceDuration = obsolescenceDuration;
                this._cancellationToken = cancellationToken;
            }

            public IGitModule Commands
            {
                get { return _commands; }
            }

            public bool IncludeRemotes
            {
                get { return _includeRemotes; }
            }

            public bool IncludeUnmerged
            {
                get { return _includeUnmerged; }
            }

            public string ReferenceBranch
            {
                get { return _referenceBranch; }
            }

            public string RemoteRepositoryName
            {
                get { return _remoteRepositoryName; }
            }

            public string RegexFilter
            {
                get { return _regexFilter; }
            }

            public TimeSpan ObsolescenceDuration
            {
                get { return _obsolescenceDuration; }
            }

            public CancellationToken CancellationToken
            {
                get { return _cancellationToken; }
            }
        }
    }
}
