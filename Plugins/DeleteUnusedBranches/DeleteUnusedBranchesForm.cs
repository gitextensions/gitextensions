using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeleteUnusedBranches.Properties;
using GitExtensions.Core.Commands;
using GitExtensions.Core.Module;
using GitExtensions.Core.Utils.UI;
using GitExtensions.Extensibility;
using Microsoft.VisualStudio.Threading;

namespace DeleteUnusedBranches
{
    public sealed partial class DeleteUnusedBranchesForm : Form
    {
        private readonly DeleteUnusedBranchesFormSettings _settings;

        private readonly SortableBranchesList _branches = new SortableBranchesList();
        private readonly IGitModule _gitModule;
        private readonly IGitUICommands _gitUiCommands;
        private readonly IGitPlugin _gitPlugin;
        private readonly GitBranchOutputCommandParser _commandOutputParser;
        private CancellationTokenSource _refreshCancellation;

        public DeleteUnusedBranchesForm(DeleteUnusedBranchesFormSettings settings, IGitModule gitModule, IGitUICommands gitUiCommands, IGitPlugin gitPlugin)
        {
            _settings = settings;
            _gitModule = gitModule;
            _gitUiCommands = gitUiCommands;
            _gitPlugin = gitPlugin;
            _commandOutputParser = new GitBranchOutputCommandParser();

            InitializeComponent();

            _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn.Width = DpiUtil.Scale(50);
            dateDataGridViewTextBoxColumn.Width = DpiUtil.Scale(175);
            Author.Width = DpiUtil.Scale(91);

            imgLoading.Image = Images.loadingpanel;

            _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn.DataPropertyName = nameof(Branch.Delete);
            nameDataGridViewTextBoxColumn.DataPropertyName = nameof(Branch.Name);
            dateDataGridViewTextBoxColumn.DataPropertyName = nameof(Branch.Date);
            Author.DataPropertyName = nameof(Branch.Author);
            Message.DataPropertyName = nameof(Branch.Message);

            Text = Strings.FormText;
            Author.HeaderText = Strings.Author;
            Cancel.Text = Strings.Cancel;
            Delete.Text = Strings.Delete;
            IncludeRemoteBranches.Text = Strings.IncludeRemoteBranches;
            Message.HeaderText = Strings.Message;
            RefreshBtn.Text = Strings.RefreshBtn;
            buttonSettings.Text = Strings.ButtonSettingsText;
            dateDataGridViewTextBoxColumn.HeaderText = Strings.DateDataGridViewTextBoxColumn;
            includeUnmergedBranches.Text = Strings.IncludeUnmergedBranchesText;
            label1.Text = Strings.Label1Text;
            label2.Text = Strings.Label2Text;
            nameDataGridViewTextBoxColumn.HeaderText = Strings.NameDataGridViewTextBoxColumn;
            regexDoesNotMatch.Text = Strings.RegexDoesNotMatch;
            regexFilter.Text = Strings.RegexFilter;
            useRegexCaseInsensitive.Text = Strings.UseRegexCaseInsensitive;
            useRegexFilter.Text = Strings.UseRegexFilter;

            if (gitUiCommands == null)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(RefreshObsoleteBranchesAsync);
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

        private IEnumerable<Branch> GetObsoleteBranches(RefreshContext context, string curBranch)
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

        private IEnumerable<string> GetObsoleteBranchNames(RefreshContext context, string curBranch)
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
                 "--list",
                 { context.IncludeRemotes, "-r" },
                 { !context.IncludeUnmerged, "--merged" },
                 context.ReferenceBranch
            };

            var result = context.Commands.GitExecutable.Execute(args, _gitModule.Encoding);

            if (!result.ExitedSuccessfully)
            {
                MessageBox.Show(this, result.AllOutput, $"git {args}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return Array.Empty<string>();
            }

            return _commandOutputParser.GetBranchNames(result.AllOutput)
                                        .Where(branchName => branchName != curBranch && branchName != context.ReferenceBranch)
                                        .Where(branchName => (!context.IncludeRemotes || branchName.StartsWith(context.RemoteRepositoryName + "/"))
                                                            && (regex == null || regex.IsMatch(branchName) == regexMustMatch));
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            var selectedBranches = _branches.Where(branch => branch.Delete).ToList();
            if (selectedBranches.Count == 0)
            {
                MessageBox.Show(string.Format(Strings.SelectBranchesToDeleteText, _NO_TRANSLATE_deleteDataGridViewCheckBoxColumn.HeaderText), Strings.DeleteCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show(this, string.Format(Strings.AreYouSureToDelete, selectedBranches.Count), Strings.DeleteCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
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
                var message = string.Format(Strings.DangerousActionText, remoteName);
                if (MessageBox.Show(this, message, Strings.DeleteCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return;
                }
            }

            var localBranches = selectedBranches.Except(remoteBranches).ToList();
            tableLayoutPanel2.Enabled = tableLayoutPanel3.Enabled = false;
            imgLoading.Visible = true;
            lblStatus.Text = Strings.DeletingBranchesText;

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
                    _gitModule.GitExecutable.GetOutput(args);
                }

                foreach (var localBranch in localBranches)
                {
                    var args = new GitArgumentBuilder("branch")
                    {
                        "-d",
                        localBranch.Name
                    };
                    _gitModule.GitExecutable.GetOutput(args);

                    // Delete branches one by one, because it is possible one fails
                    _gitModule.GitExecutable.GetOutput(args);
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
            _gitUiCommands.StartSettingsDialog(_gitPlugin.GetType());
        }

        private void includeUnmergedBranches_CheckedChanged(object sender, EventArgs e)
        {
            ClearResults(sender, e);

            if (includeUnmergedBranches.Checked)
            {
                MessageBox.Show(this, Strings.DeletingUnmergedBranchesText, Strings.DeleteCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ClearResults(object sender, EventArgs e)
        {
            instructionLabel.Text = string.Format(Strings.ChooseBranchesToDeleteText, mergedIntoBranch.Text);
            lblStatus.Text = string.Format(Strings.PressToSearchText, RefreshBtn.Text);
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
                _gitModule,
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
                RefreshBtn.Text = value ? Strings.CancelText : Strings.SearchBranchesText;
                imgLoading.Visible = value;
                lblStatus.Text = value ? Strings.LoadingText : GetDefaultStatusText();
            }
        }

        private string GetDefaultStatusText()
        {
            return string.Format(Strings.BranchesSelected, _branches.Count(b => b.Delete), _branches.Count);
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
