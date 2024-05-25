using System.Text.RegularExpressions;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormReflog : GitModuleForm
    {
        private sealed class SortableRefLineList : SortableBindingList<RefLine>
        {
            static SortableRefLineList()
            {
                AddSortableProperty(refLine => refLine.Sha, (x, y) => x.Sha.CompareTo(y.Sha));
                AddSortableProperty(refLine => refLine.Ref, (x, y) => string.Compare(x.Ref, y.Ref, StringComparison.Ordinal));
                AddSortableProperty(refLine => refLine.Action, (x, y) => string.Compare(x.Action, y.Action, StringComparison.CurrentCulture));
            }
        }

        private readonly TranslationString _continueResetCurrentBranchEvenWithChangesText = new("You have changes in your working directory that could be lost.\n\nDo you want to continue?");
        private readonly TranslationString _continueResetCurrentBranchCaptionText = new("Changes not committed...");

        [GeneratedRegex(@"^(?<sha>[^ ]+) (?<ref>[^:]+): (?<action>.+)$", RegexOptions.ExplicitCapture)]
        private static partial Regex ReflogRegex();

        private string? _currentBranch;
        private bool _isBranchCheckedOut;
        private bool _isDirtyDir;
        private int _lastHitRowIndex;

        public FormReflog(IGitUICommands uiCommands)
            : base(uiCommands)
        {
            InitializeComponent();
            InitializeComplete();

            gridReflog.RowTemplate.Height = DpiUtil.Scale(24);
            gridReflog.ColumnHeadersHeight = DpiUtil.Scale(30);

            Sha.DataPropertyName = nameof(RefLine.Sha);
            Ref.DataPropertyName = nameof(RefLine.Ref);
            Action.DataPropertyName = nameof(RefLine.Action);
        }

        private void FormReflog_Load(object sender, EventArgs e)
        {
            _isDirtyDir = UICommands.Module.IsDirtyDir();
            _currentBranch = UICommands.Module.GetSelectedBranch();
            _isBranchCheckedOut = _currentBranch != DetachedHeadParser.DetachedBranch;
            linkCurrentBranch.Text = "current branch (" + _currentBranch + ")";
            linkCurrentBranch.Visible = _isBranchCheckedOut;
            _lastHitRowIndex = 0;
            lblDirtyWorkingDirectory.Visible = _isDirtyDir;
            resetCurrentBranchOnThisCommitToolStripMenuItem.Enabled = _isBranchCheckedOut;

            List<string> branches =
            [
                "HEAD",
                .. UICommands.Module.GetRefs(RefsFilter.Heads).Select(r => r.Name).OrderBy(n => n),
                .. UICommands.Module.GetRemoteBranches().Select(r => r.Name).OrderBy(n => n),
            ];
            Branches.DataSource = branches;
        }

        private void Branches_SelectedIndexChanged(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(DisplayRefLog);

            async Task DisplayRefLog()
            {
                string item = (string)Branches.SelectedItem;
                await TaskScheduler.Default;
                GitArgumentBuilder arguments = new("reflog")
                {
                    "--no-abbrev",
                    item
                };
                string output = UICommands.Module.GitExecutable.GetOutput(arguments);
                List<RefLine> refLines = ConvertReflogOutput().ToList();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _lastHitRowIndex = 0;
                SortableRefLineList refLinesList = new();
                refLinesList.AddRange(refLines);
                gridReflog.DataSource = refLinesList;

                IEnumerable<RefLine> ConvertReflogOutput()
                    => from line in output.LazySplit('\n')
                        where line.Length != 0
                        select ReflogRegex().Match(line)
                        into match
                        where match.Success
                        select new RefLine(ObjectId.Parse(match.Groups["sha"].Value), match.Groups["ref"].Value, match.Groups["action"].Value);
            }
        }

        private void createABranchOnThisCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gridReflog.SelectedCells.Count == 0 && gridReflog.SelectedRows.Count == 0)
            {
                return;
            }

            UICommands.DoActionOnRepo(() =>
            {
                using FormCreateBranch form = new(UICommands, GetShaOfRefLine());
                form.CheckoutAfterCreation = false;
                form.UserAbleToChangeRevision = false;
                form.CouldBeOrphan = false;
                return form.ShowDialog(this) == DialogResult.OK;
            });
        }

        private ObjectId GetShaOfRefLine()
        {
            DataGridViewRow row = GetSelectedRow();
            RefLine refLine = (RefLine)row.DataBoundItem;
            return refLine.Sha;

            DataGridViewRow GetSelectedRow()
            {
                if (gridReflog.SelectedRows.Count > 0)
                {
                    return gridReflog.SelectedRows[0];
                }

                if (gridReflog.SelectedCells.Count > 0)
                {
                    return gridReflog.Rows[gridReflog.SelectedCells[0].RowIndex];
                }

                return gridReflog.CurrentRow;
            }
        }

        private void resetCurrentBranchOnThisCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_isDirtyDir)
            {
                if (MessageBox.Show(this, _continueResetCurrentBranchEvenWithChangesText.Text,
                        _continueResetCurrentBranchCaptionText.Text,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    return;
                }
            }

            GitRevision gitRevision = UICommands.Module.GetRevision(GetShaOfRefLine());
            FormResetCurrentBranch.ResetType resetType = _isDirtyDir ? FormResetCurrentBranch.ResetType.Soft : FormResetCurrentBranch.ResetType.Hard;
            UICommands.DoActionOnRepo(() =>
            {
                using FormResetCurrentBranch form = FormResetCurrentBranch.Create(UICommands, gitRevision, resetType);
                return form.ShowDialog(this) == DialogResult.OK;
            });
        }

        private void copySha1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClipboardUtil.TrySetText(GetShaOfRefLine().ToString());
        }

        private void linkCurrentBranch_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Branches.SelectedItem = _currentBranch;
        }

        private void linkHead_Click(object sender, EventArgs e)
        {
            Branches.SelectedIndex = 0;
        }

        private void gridReflog_MouseMove(object sender, MouseEventArgs e)
        {
            DataGridView.HitTestInfo hit = gridReflog.HitTest(e.X, e.Y);

            if (hit.Type == DataGridViewHitTestType.Cell && _lastHitRowIndex != hit.RowIndex)
            {
                if (_lastHitRowIndex < gridReflog.Rows.Count)
                {
                    gridReflog.Rows[_lastHitRowIndex].Selected = false;
                }

                _lastHitRowIndex = hit.RowIndex;
                gridReflog.Rows[_lastHitRowIndex].Selected = true;
            }
        }

        private void gridReflog_MouseClick(object sender, MouseEventArgs e)
        {
            contextMenuStripReflog.Show((Control)sender, e.Location);
        }
    }

    internal class RefLine
    {
        public ObjectId Sha { get; set; }
        public string Ref { get; set; }
        public string Action { get; set; }

        public RefLine(ObjectId sha, string @ref, string action)
        {
            Sha = sha;
            Ref = @ref;
            Action = action;
        }
    }
}
