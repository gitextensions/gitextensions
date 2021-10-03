using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
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
        private sealed class SortableRefLineList : BindingList<RefLine>
        {
            public void AddRange(IEnumerable<RefLine> refLines)
            {
                RefLines.AddRange(refLines);

                // NOTE: adding items via wrapper's AddRange doesn't generate ListChanged event, so DataGridView doesn't update itself
                // There are two solutions:
                //  0. Add items one by one using direct this.Add method (without IList<T> wrapper).
                //     Too many ListChanged events will be generated (one per item), too many updates for gridview. Bad performance.
                //  1. Batch add items through Items wrapper's AddRange method.
                //     One reset event will be generated, one batch update for gridview. Ugly but fast code.
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }

            protected override bool SupportsSortingCore => true;

            protected override void ApplySortCore(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
            {
                RefLines.Sort(RefLineComparer.Create(propertyDescriptor, direction == ListSortDirection.Descending));
            }

            private List<RefLine> RefLines => (List<RefLine>)Items;

            private static class RefLineComparer
            {
                private static readonly Dictionary<string, Comparison<RefLine>> PropertyComparers = new Dictionary<string, Comparison<RefLine>>();

                static RefLineComparer()
                {
                    AddSortableProperty(refLine => refLine.Sha, (x, y) => x.Sha.CompareTo(y.Sha));
                    AddSortableProperty(refLine => refLine.Ref, (x, y) => string.Compare(x.Ref, y.Ref, StringComparison.Ordinal));
                    AddSortableProperty(refLine => refLine.Action, (x, y) => string.Compare(x.Action, y.Action, StringComparison.CurrentCulture));
                }

                /// <summary>
                /// Creates a comparer to sort lostObjects by specified property.
                /// </summary>
                /// <param name="propertyDescriptor">Property to sort by.</param>
                /// <param name="isReversedComparing">Use reversed sorting order.</param>
                public static Comparison<RefLine> Create(PropertyDescriptor propertyDescriptor, bool isReversedComparing)
                {
                    if (PropertyComparers.TryGetValue(propertyDescriptor.Name, out var comparer))
                    {
                        return isReversedComparing ? (x, y) => comparer(y, x) : comparer;
                    }

                    throw new NotSupportedException(string.Format("Custom sort by {0} property is not supported.", propertyDescriptor.Name));
                }

                /// <summary>
                /// Adds custom property comparer.
                /// </summary>
                /// <typeparam name="T">Property type.</typeparam>
                /// <param name="expr">Property to sort by.</param>
                /// <param name="propertyComparer">Property values comparer.</param>
                private static void AddSortableProperty<T>(Expression<Func<RefLine, T>> expr, Comparison<RefLine> propertyComparer)
                {
                    PropertyComparers[((MemberExpression)expr.Body).Member.Name] = propertyComparer;
                }
            }
        }

        private readonly TranslationString _continueResetCurrentBranchEvenWithChangesText = new("You have changes in your working directory that could be lost.\n\nDo you want to continue?");
        private readonly TranslationString _continueResetCurrentBranchCaptionText = new("Changes not committed...");

        private readonly Regex _regexReflog = new("^([^ ]+) ([^:]+): (.+)$", RegexOptions.Compiled);

        private string? _currentBranch;
        private bool _isBranchCheckedOut;
        private bool _isDirtyDir;
        private int _lastHitRowIndex;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormReflog()
        {
            InitializeComponent();
        }

        public FormReflog(GitUICommands uiCommands)
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

            List<string> branches = new() { "HEAD" };
            branches.AddRange(UICommands.Module.GetRefs(RefsFilter.Heads).Select(r => r.Name).OrderBy(n => n));
            branches.AddRange(UICommands.Module.GetRemoteBranches().Select(r => r.Name).OrderBy(n => n));
            Branches.DataSource = branches;
        }

        private void Branches_SelectedIndexChanged(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(DisplayRefLog);

            async Task DisplayRefLog()
            {
                var item = (string)Branches.SelectedItem;
                await TaskScheduler.Default;
                GitArgumentBuilder arguments = new("reflog")
                {
                    "--no-abbrev",
                    item
                };
                var output = UICommands.GitModule.GitExecutable.GetOutput(arguments);
                var refLines = ConvertReflogOutput().ToList();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _lastHitRowIndex = 0;
                var refLinesList = new SortableRefLineList();
                refLinesList.AddRange(refLines);
                gridReflog.DataSource = refLinesList;

                IEnumerable<RefLine> ConvertReflogOutput()
                    => from line in output.LazySplit('\n')
                        where line.Length != 0
                        select _regexReflog.Match(line)
                        into match
                        where match.Success
                        select new RefLine(ObjectId.Parse(match.Groups[1].Value), match.Groups[2].Value, match.Groups[3].Value);
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
            var row = GetSelectedRow();
            var refLine = (RefLine)row.DataBoundItem;
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

            var gitRevision = UICommands.Module.GetRevision(GetShaOfRefLine());
            var resetType = _isDirtyDir ? FormResetCurrentBranch.ResetType.Soft : FormResetCurrentBranch.ResetType.Hard;
            UICommands.DoActionOnRepo(() =>
            {
                using var form = FormResetCurrentBranch.Create(UICommands, gitRevision, resetType);
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
            var hit = gridReflog.HitTest(e.X, e.Y);

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
