using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;

using System.Text;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using System.Drawing.Drawing2D;
using System.IO;

namespace GitUI
{
    public partial class RevisionGrid : UserControl
    {
        public event EventHandler ChangedCurrentBranch;

        public virtual void OnChangedCurrentBranch()
        {
            if (ChangedCurrentBranch != null)
                ChangedCurrentBranch(this, null);
        }

        private readonly SynchronizationContext syncContext;
        IndexWatcher indexWatcher = new IndexWatcher();

        public RevisionGrid()
        {
            syncContext = SynchronizationContext.Current;

            base.InitLayout();
            InitializeComponent();

            NormalFont = Revisions.Font;
            HeadFont = new Font(NormalFont, FontStyle.Bold);

            Revisions.CellPainting += new DataGridViewCellPaintingEventHandler(Revisions_CellPainting);
            Revisions.KeyDown += new KeyEventHandler(Revisions_KeyDown);

            showRevisionGraphToolStripMenuItem.Checked = Settings.ShowRevisionGraph;
            showAuthorDateToolStripMenuItem.Checked = Settings.ShowAuthorDate;
            orderRevisionsByDateToolStripMenuItem.Checked = Settings.OrderRevisionByDate;
            showRelativeDateToolStripMenuItem.Checked = Settings.RelativeDate;

            SetShowBranches();
            filter = "";
            quickSearchString = "";
            quickSearchTimer.Tick += new EventHandler(quickSearchTimer_Tick);
        }

        Label quickSearchLabel;
        private void ShowQuickSearchString()
        {
            if (quickSearchLabel == null)
            {
                quickSearchLabel = new Label();
                quickSearchLabel.Location = new Point(10, 10);
                quickSearchLabel.BorderStyle = BorderStyle.FixedSingle;
                quickSearchLabel.ForeColor = SystemColors.InfoText;
                quickSearchLabel.BackColor = SystemColors.Info;
                //quickSearchLabel.Size = new Size(200, 50);
                this.Controls.Add(quickSearchLabel);
            }

            quickSearchLabel.Visible = true;
            quickSearchLabel.BringToFront();
            quickSearchLabel.Text = quickSearchString;
            quickSearchLabel.AutoSize = true;
        }

        private void HideQuickSearchString()
        {
            if (quickSearchLabel != null)
                quickSearchLabel.Visible = false;
        }

        void quickSearchTimer_Tick(object sender, EventArgs e)
        {
            quickSearchTimer.Stop();
            quickSearchString = "";
            HideQuickSearchString();
        }

        private string quickSearchString;
        private string lastQuickSearchString = string.Empty;
        
        void Revisions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.Up)
            {
                int nextIndex = 0;
                if (Revisions.SelectedRows.Count > 0)
                    nextIndex = Revisions.SelectedRows[0].Index - 1;

                FindNextMatch(nextIndex, lastQuickSearchString, true);
                e.Handled = true;
                return;
            }
            if (e.Alt && e.KeyCode == Keys.Down)
            {
                int nextIndex = 0;
                if (Revisions.SelectedRows.Count > 0)
                    nextIndex = Revisions.SelectedRows[0].Index + 1;
                
                FindNextMatch(nextIndex, lastQuickSearchString, false);  
                e.Handled = true;
                return;  
            }
            char key = (char)e.KeyValue;
            if (char.IsLetterOrDigit(key) || char.IsNumber(key) || char.IsSeparator(key))
            {
                quickSearchTimer.Stop();
                quickSearchTimer.Interval = 700;
                quickSearchTimer.Start();

                quickSearchString = string.Concat(quickSearchString, (char)e.KeyValue).ToLower();

                int oldIndex = 0;
                if (Revisions.SelectedRows.Count > 0)
                    oldIndex = Revisions.SelectedRows[0].Index;

                FindNextMatch(oldIndex, quickSearchString, false);
                lastQuickSearchString = quickSearchString;

                e.Handled = true;
                ShowQuickSearchString();
            }
            else
            {
                quickSearchString = "";
                HideQuickSearchString();
                return;
            }
        }

        private void FindNextMatch(int startIndex, string searchString, bool reverse)
        {
            if (Revisions.RowCount == 0)
            {
                return;
            }

            Predicate<object> match = delegate(object m)
            {
                GitRevision r = (GitRevision) m;
                foreach (GitHead gitHead in r.Heads)
                {
                    if (gitHead.Name.StartsWith(searchString))
                    {
                        return true;
                    }
                }

                // Make sure it only matches the start of a word
                string modifiedSearchString = " " + searchString;

                if ((" " + r.Author.ToLower()).Contains(modifiedSearchString))
                {
                    return true;
                }

                if ((" " + r.Message.ToLower()).Contains(modifiedSearchString))
                {
                    return true;
                }
                return false;
            };

            bool isFound = false;
            int index;
            if (reverse)
            {
                // Check for out of bounds roll over if required
                if (startIndex < 0 || startIndex >= Revisions.RowCount)
                    startIndex = Revisions.RowCount - 1;

                for (index = startIndex; index >= 0; --index)
                {
                    if (match(Revisions.GetRowData(index)))
                    {
                        isFound = true;
                        break;
                    }
                }

                if (index == -1)
                {
                    // We didn't find it so start searching from the bottom
                    //index = Revisions.FindLastIndex(bottomIndex, bottomIndex - startIndex, match);
                    for (index = Revisions.RowCount - 1; index > startIndex; --index)
                    {
                        if (match(Revisions.GetRowData(index)))
                        {
                            isFound = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                // Check for out of bounds roll over if required
                if (startIndex < 0 || startIndex >= Revisions.RowCount)
                    startIndex = 0;

                for (index = startIndex; index < Revisions.RowCount; ++index)
                {
                    if (match(Revisions.GetRowData(index)))
                    {
                        isFound = true;
                        break;
                    }
                }

                if (!isFound)
                {
                    // We didn't find it so start searching from the top
                    for (index = 0; index < startIndex; ++index)
                    {
                        if (match(Revisions.GetRowData(index)))
                        {
                            isFound = true;
                            break;
                        }
                    }
                }
            }

            if (isFound)
            {
                Revisions.ClearSelection();
                Revisions.Rows[index].Selected = true;

                Revisions.CurrentCell = Revisions.Rows[index].Cells[0];
            }
        }

        public string LogParam = "HEAD --all --boundary";

        private bool ContextMenuEnabled = true;
        public void DisableContextMenu()
        {
            Revisions.ContextMenuStrip = null;
            ContextMenuEnabled = false;
        }

        private string filter;
        public string Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
            }
        }

        public string FormatQuickFilter(string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return "";
            else
                return " --regexp-ignore-case --grep=\"" + filter + "\" --committer=\"" + filter + "\" --author=\"" + filter + "\" ";
        }

        ~RevisionGrid()
        {
            if (revisionGraphCommand != null)
                revisionGraphCommand.Kill();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            RefreshRevisions();
        }

        public Font NormalFont { get; set; }
        public Font HeadFont { get; set; }

        public event EventHandler SelectionChanged;

        public void SetSelectedRevision(GitRevision revision)
        {
            Revisions.ClearSelection();

            if (revision != null)
            {
                foreach (DataGridViewRow row in Revisions.Rows)
                {
                    if (((GitRevision)row.DataBoundItem).Guid == revision.Guid)
                        row.Selected = true;
                }
            }
            Revisions.Select();
        }

        void Revisions_SelectionChanged(object sender, EventArgs e)
        {
            if (Revisions.SelectedRows.Count > 0)
                LastRow = Revisions.SelectedRows[0].Index;

            SelecctionTimer.Enabled = false;
            SelecctionTimer.Stop();
            SelecctionTimer.Enabled = true;
            SelecctionTimer.Start();
        }

        public List<GitRevision> GetRevisions()
        {
            List<GitRevision> retval = new List<GitRevision>();

            foreach (DataGridViewRow row in Revisions.SelectedRows)
            {
                if (Revisions.RowCount > row.Index)
                    retval.Add(GetRevision(row.Index));
            }
            return retval;
        }

        GitRevision GetRevision(int aRow)
        {
            return Revisions.GetRowData(aRow) as GitRevision;
        }

        GitCommands.RevisionGraph revisionGraphCommand = null;
        private bool ScrollBarSet;

        public void RefreshRevisions()
        {
            if (indexWatcher.IndexChanged)
            {
                ForceRefreshRevisions();
            }
        }

        public int LastScrollPos = 0;
        public List<int> LastSelectedRows = new List<int>();

        public void ForceRefreshRevisions()
        {
            try
            {
                initialLoad = true;
                
                LastScrollPos = Revisions.FirstDisplayedScrollingRowIndex;
                LastSelectedRows.Clear();

                foreach (DataGridViewRow row in Revisions.SelectedRows)
                {
                    LastSelectedRows.Add(row.Index);
                }

                if (!Settings.ShowRevisionGraph)
                    Revisions.Columns[0].Width = 0;

                Error.Visible = false;
                NoCommits.Visible = false;
                NoGit.Visible = false;
                Revisions.Visible = true;

                if (!GitCommands.Settings.ValidWorkingDir())
                {
                    Revisions.RowCount = 0;
                    Revisions.ScrollBars = ScrollBars.None;
                    Revisions.Visible = false;

                    NoCommits.Visible = true;
                    NoGit.Visible = true;
                    Loading.Visible = false;
                    return;
                }

                if (revisionGraphCommand != null)
                {
                    revisionGraphCommand.Kill();
                }

                LastRevision = 0;
                ScrollBarSet = false;
                Revisions.ClearSelection();
                Revisions.VirtualMode = true;
                //Revisions.ScrollBars = ScrollBars.None;
                Revisions.RowCount = 0;
                Revisions.RowCount = Math.Max(Revisions.DisplayedRowCount(true), GitCommands.Settings.MaxCommits);

                currentCheckout = GitCommands.GitCommands.GetCurrentCheckout();
                ScrollBarSet = false;
                InternalRefresh();
            }
            catch (Exception exception)
            {
                Error.Visible = true;
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InternalRefresh()
        {
            Error.Visible = false;
            NoCommits.Visible = false;
            NoGit.Visible = false;
            Revisions.Visible = true;

            if (!GitCommands.Settings.ValidWorkingDir())
            {
                Revisions.RowCount = 0;
                Revisions.ScrollBars = ScrollBars.None;
                Revisions.Visible = false;

                NoCommits.Visible = true;
                NoGit.Visible = true;
                Loading.Visible = false;
                return;
            }

            int numberOfVisibleRows = Revisions.DisplayedRowCount(true) + 1;
            int firstVisibleRow = Revisions.FirstDisplayedScrollingRowIndex;

            if (numberOfVisibleRows < 1)
                numberOfVisibleRows = 20;

            if (LastRevision >= Math.Min(Revisions.RowCount, firstVisibleRow + numberOfVisibleRows))
            {
                return;
            }

            LastRevision = Math.Min(Revisions.RowCount, Math.Max(LastScrollPos + numberOfVisibleRows, Math.Max(firstVisibleRow + numberOfVisibleRows, Math.Max(GitCommands.Settings.MaxCommits, LastRevision * 2))));

            Revisions.Enabled = false;
            Loading.Visible = true;
            indexWatcher.Reset();
            revisionGraphCommand = new RevisionGraph();

            revisionGraphCommand.BackgroundThread = true;
            revisionGraphCommand.LogParam = LogParam + Filter;
            revisionGraphCommand.Updated += new EventHandler(gitGetCommitsCommand_Exited);
            revisionGraphCommand.Exited += new EventHandler(gitGetCommitsCommand_Exited);
            revisionGraphCommand.LimitRevisions = LastRevision;
            revisionGraphCommand.Execute();
        }

        void gitGetCommitsCommand_Exited(object sender, EventArgs e)
        {
            List<GitRevision> revisionList = revisionGraphCommand.Revisions;
            if (revisionList == null)
            {
                return;
            }

            DvcsGraph.GraphData graph = new DvcsGraph.GraphData();

            #region Set the sort order for the graph
            if (!Settings.OrderRevisionByDate)
            {
                graph.Sorter = delegate(object a, object b)
                {
                    GitRevision left = (GitRevision)a;
                    GitRevision right = (GitRevision)b;
                    return right.Order.CompareTo(left.Order);
                };
            }
            else if (Settings.ShowAuthorDate)
            {
                graph.Sorter = delegate(object a, object b)
                {
                    GitRevision left = (GitRevision)a;
                    GitRevision right = (GitRevision)b;
                    return left.AuthorDate.CompareTo(right.AuthorDate);
                };
            }
            else
            {
                graph.Sorter = delegate(object a, object b)
                {
                    GitRevision left = (GitRevision)a;
                    GitRevision right = (GitRevision)b;
                    return left.CommitDate.CompareTo(right.CommitDate);
                };
            }
            #endregion

            foreach (GitRevision rev in revisionList)
            {
                if (rev == null || rev.AuthorDate == null)
                {
                    // This should never happen.
                    continue;
                }
                DvcsGraph.DataType dataType;
                if (rev.Guid == currentCheckout)
                {
                    dataType = DvcsGraph.DataType.Active;
                }
                else if (rev.Heads.Count > 0)
                {
                    dataType = DvcsGraph.DataType.Special;
                }
                else
                {
                    dataType = DvcsGraph.DataType.Normal;
                }
                graph.Add(rev.Guid, rev.ParentGuids.ToArray(), dataType, rev);
            }
            Revisions.SetData(graph);
            

            syncContext.Post(_ => LoadRevisions(), null);
        }

        public string currentCheckout { get; set; }
        private int LastRevision = 0;
        private bool initialLoad = true;

        private string GetDateHeaderText()
        {
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(RevisionGrid));
            return Settings.ShowAuthorDate ? resources.GetString("AuthorDate") : resources.GetString("CommitDate");
        }

        private void LoadRevisions()
        {
            if (revisionGraphCommand == null)
            {
                return;
            }

            if (Revisions.RowCount == 0 && string.IsNullOrEmpty(Filter))
            {
                Loading.Visible = false;
                NoCommits.Visible = true;
                Revisions.Visible = false;
                return;
            }

            Revisions.SuspendLayout();
            Revisions.Columns[3].HeaderText = GetDateHeaderText();

            if (!ScrollBarSet)
            {
                ScrollBarSet = true;
                Revisions.ScrollBars = ScrollBars.None;
                Revisions.RowCount = Revisions.RowCount;
                Revisions.ScrollBars = ScrollBars.Vertical;
            }

            Revisions.SelectionChanged -= new EventHandler(Revisions_SelectionChanged);

            if (LastScrollPos > 0 && Revisions.RowCount > LastScrollPos)
            {
                Revisions.FirstDisplayedScrollingRowIndex = LastScrollPos;
                LastScrollPos = -1;
            }

            if (LastSelectedRows.Count > 0)
            {
                Revisions.ClearSelection();

                if (Revisions.Rows.Count > LastSelectedRows[0])
                    Revisions.CurrentCell = Revisions.Rows[LastSelectedRows[0]].Cells[0];

                foreach (int row in LastSelectedRows)
                {
                    if (Revisions.Rows.Count > row)
                    {
                        Revisions.Rows[row].Selected = true;
                    }
                }
                LastSelectedRows.Clear();
            }

            Loading.Visible = false;
            Revisions.Enabled = true;
            Revisions.Focus();
            Revisions.SelectionChanged += new EventHandler(Revisions_SelectionChanged);

            Revisions.ResumeLayout();

            if (initialLoad)
            {
                initialLoad = false;
                SelecctionTimer.Enabled = false;
                SelecctionTimer.Stop();
                SelecctionTimer.Enabled = true;
                SelecctionTimer.Start();
            }
        }

        void Revisions_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // The graph column is handled by the DvcsGraph
            if (e.ColumnIndex == 0)
            {
                return;
            }

            int column = e.ColumnIndex;
            if (e.RowIndex >= 0 && (e.State & DataGridViewElementStates.Visible) != 0)
            {
                if (Revisions.RowCount > e.RowIndex)
                {
                    GitRevision revision = GetRevision(e.RowIndex);
                    if (revision == null)
                    {
                        return;
                    }

                    e.Handled = true;

                    e.PaintBackground(e.CellBounds, true);

                    if (column == 1)
                    {
                        float offset = 0;
                        foreach (GitHead h in revision.Heads)
                        {
                            if ((h.IsRemote && !ShowRemoteBranches.Checked) == false)
                            {
                                SolidBrush brush = new SolidBrush(h.IsTag == true ? Settings.TagColor : h.IsHead ? Settings.BranchColor : h.IsRemote ? Settings.RemoteBranchColor : Settings.OtherTagColor);

                                e.Graphics.DrawString("[" + h.Name + "] ", HeadFont, brush, new PointF(e.CellBounds.Left + offset, e.CellBounds.Top + 4));

                                offset += e.Graphics.MeasureString("[" + h.Name + "] ", HeadFont).Width;
                            }
                        }
                        string text = revision.Message;
                        e.Graphics.DrawString(text, NormalFont, Brushes.Black, new PointF(e.CellBounds.Left + offset, e.CellBounds.Top + 4));
                    }
                    else if (column == 2)
                    {
                        string text = revision.Author;
                        e.Graphics.DrawString(text, NormalFont, Brushes.Black, new PointF(e.CellBounds.Left, e.CellBounds.Top + 4));
                    }
                    else if (column == 3)
                    {
                        DateTime time = Settings.ShowAuthorDate ? revision.AuthorDate : revision.CommitDate;
                        string text = TimeToString(time);
                        e.Graphics.DrawString(text, NormalFont, Brushes.Black, new PointF(e.CellBounds.Left, e.CellBounds.Top + 4));
                    }
                }
            }
        }

        private void Revisions_DoubleClick(object sender, EventArgs e)
        {
            List<GitRevision> r = GetRevisions();
            if (r.Count > 0)
            {
                FormDiffSmall form = new FormDiffSmall();
                form.SetRevision(r[0]);
                form.ShowDialog();
            }
            else
                GitUICommands.Instance.StartCompareRevisionsDialog();
        }

        private void SelecctionTimer_Tick(object sender, EventArgs e)
        {
            SelecctionTimer.Enabled = false;
            SelecctionTimer.Stop();
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        private void createTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Revisions.RowCount > LastRow && LastRow >= 0)
            {
                FormTagSmall frm = new FormTagSmall();
                frm.Revision = GetRevision(LastRow);
                frm.ShowDialog();
                RefreshRevisions();
            }
        }

        private void resetCurrentBranchToHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Revisions.RowCount > LastRow && LastRow >= 0)
            {
                FormResetCurrentBranch frm = new FormResetCurrentBranch((GitRevision)GetRevision(LastRow));
                frm.ShowDialog();
                RefreshRevisions();
            }
        }

        private void createNewBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Revisions.RowCount > LastRow && LastRow >= 0)
            {
                FormBranchSmall frm = new FormBranchSmall();
                frm.Revision = (GitRevision)GetRevision(LastRow);
                frm.ShowDialog();
                RefreshRevisions();
                OnChangedCurrentBranch();
            }

        }

        public int LastRow { get; set; }

        private void Revisions_MouseClick(object sender, MouseEventArgs e)
        {
            System.Drawing.Point pt = Revisions.PointToClient(Cursor.Position);
            DataGridView.HitTestInfo hti = Revisions.HitTest(pt.X, pt.Y);
            LastRow = hti.RowIndex;
        }

        private void Revisions_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                System.Drawing.Point pt = Revisions.PointToClient(Cursor.Position);
                DataGridView.HitTestInfo hti = Revisions.HitTest(pt.X, pt.Y);
                LastRow = hti.RowIndex;
                Revisions.ClearSelection();
                if (LastRow >= 0 && Revisions.Rows.Count > LastRow)
                    Revisions.Rows[LastRow].Selected = true;
            }
        }

        private void Commit_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartCommitDialog();
            RefreshRevisions();
        }

        private void GitIgnore_Click(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartEditGitIgnoreDialog();
        }

        private void ShowRemoteBranches_Click(object sender, EventArgs e)
        {
            ShowRemoteBranches.Checked = !ShowRemoteBranches.Checked;
            Revisions.Invalidate();
        }

        private void showAllBranchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.ShowAllBranches = !showAllBranchesToolStripMenuItem.Checked;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        private void SetShowBranches()
        {
            showAllBranchesToolStripMenuItem.Checked = Settings.ShowAllBranches;
            if (Settings.ShowAllBranches)
                LogParam = "HEAD --all --boundary";
            else
                LogParam = "HEAD";

        }

        private void revertCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Revisions.RowCount > LastRow && LastRow >= 0)
            {
                FormRevertCommitSmall frm = new FormRevertCommitSmall((GitRevision)GetRevision(LastRow));
                frm.ShowDialog();
                RefreshRevisions();
            }
        }

        private void showRevisionGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.ShowRevisionGraph = !showRevisionGraphToolStripMenuItem.Checked;
            showRevisionGraphToolStripMenuItem.Checked = Settings.ShowRevisionGraph;
            this.ForceRefreshRevisions();
        }

        private FormRevisionFilter RevisionFilter = null;
        private void filterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RevisionFilter == null)
                RevisionFilter = new FormRevisionFilter();

            RevisionFilter.ShowDialog();
            filter = RevisionFilter.GetFilter();
            ForceRefreshRevisions();
        }

        private void Revisions_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F && ContextMenuEnabled)
                filterToolStripMenuItem_Click(null, null);
        }

        private void CreateTag_Opening(object sender, CancelEventArgs e)
        {
            if (Revisions.RowCount < LastRow || LastRow < 0 || Revisions.RowCount == 0)
                return;

            GitRevision revision = GetRevision(LastRow) as GitRevision;

            ToolStripDropDown tagDropDown = new ToolStripDropDown();
            ToolStripDropDown branchDropDown = new ToolStripDropDown();
            ToolStripDropDown checkoutBranchDropDown = new ToolStripDropDown();
            ToolStripDropDown mergeBranchDropDown = new ToolStripDropDown();
            ToolStripDropDown rebaseDropDown = new ToolStripDropDown();

            foreach (GitHead head in revision.Heads)
            {
                if (head.IsTag)
                {
                    ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Click += new EventHandler(toolStripItem_Click);
                    tagDropDown.Items.Add(toolStripItem);
                }
                else
                    if (head.IsHead || head.IsRemote)
                    {
                        ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                        toolStripItem.Click += new EventHandler(toolStripItem_ClickMergeBranch);
                        mergeBranchDropDown.Items.Add(toolStripItem);

                        toolStripItem = new ToolStripMenuItem(head.Name);
                        toolStripItem.Click += new EventHandler(toolStripItem_ClickRebaseBranch);
                        rebaseDropDown.Items.Add(toolStripItem);

                        if (head.IsHead && !head.IsRemote)
                        {
                            toolStripItem = new ToolStripMenuItem(head.Name);
                            toolStripItem.Click += new EventHandler(toolStripItem_ClickBranch);
                            branchDropDown.Items.Add(toolStripItem);

                            toolStripItem = new ToolStripMenuItem(head.Name);
                            toolStripItem.Click += new EventHandler(toolStripItem_ClickCheckoutBranch);
                            checkoutBranchDropDown.Items.Add(toolStripItem);
                        }

                    }
            }

            deleteTagToolStripMenuItem.DropDown = tagDropDown;
            deleteTagToolStripMenuItem.Visible = tagDropDown.Items.Count > 0;

            deleteBranchToolStripMenuItem.DropDown = branchDropDown;
            deleteBranchToolStripMenuItem.Visible = branchDropDown.Items.Count > 0;

            checkoutBranchToolStripMenuItem.DropDown = checkoutBranchDropDown;
            checkoutBranchToolStripMenuItem.Visible = checkoutBranchDropDown.Items.Count > 0;

            mergeBranchToolStripMenuItem.DropDown = mergeBranchDropDown;
            mergeBranchToolStripMenuItem.Visible = mergeBranchDropDown.Items.Count > 0;

            rebaseOnToolStripMenuItem.DropDown = rebaseDropDown;
            rebaseOnToolStripMenuItem.Visible = rebaseDropDown.Items.Count > 0;

        }

        void toolStripItem_Click(object sender, EventArgs e)
        {
            ToolStripItem toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            new FormProcess(GitCommands.GitCommands.DeleteTagCmd(toolStripItem.Text));
            ForceRefreshRevisions();
        }

        void toolStripItem_ClickBranch(object sender, EventArgs e)
        {
            ToolStripItem toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            GitUICommands.Instance.StartDeleteBranchDialog(toolStripItem.Text);

            ForceRefreshRevisions();
        }

        void toolStripItem_ClickCheckoutBranch(object sender, EventArgs e)
        {
            ToolStripItem toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            new FormProcess("checkout \"" + toolStripItem.Text + "\"");

            ForceRefreshRevisions();
            OnChangedCurrentBranch();
        }

        void toolStripItem_ClickMergeBranch(object sender, EventArgs e)
        {
            ToolStripItem toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            GitUICommands.Instance.StartMergeBranchDialog(toolStripItem.Text);

            ForceRefreshRevisions();
        }

        void toolStripItem_ClickRebaseBranch(object sender, EventArgs e)
        {
            ToolStripItem toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            GitUICommands.Instance.StartRebaseDialog(toolStripItem.Text);

            ForceRefreshRevisions();
        }

        private void checkoutRevisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Revisions.RowCount > LastRow && LastRow >= 0)
            {
                if (MessageBox.Show("Are you sure to checkout the selected revision", "Checkout revision", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    new FormProcess("checkout \"" + GetRevision(LastRow).Guid + "\"");
                    ForceRefreshRevisions();
                    OnChangedCurrentBranch();
                }
            }
        }

        private void showAuthorDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.ShowAuthorDate = !showAuthorDateToolStripMenuItem.Checked;
            showAuthorDateToolStripMenuItem.Checked = Settings.ShowAuthorDate;
            this.ForceRefreshRevisions();
        }

        private void orderRevisionsByDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.OrderRevisionByDate = !orderRevisionsByDateToolStripMenuItem.Checked;
            orderRevisionsByDateToolStripMenuItem.Checked = Settings.OrderRevisionByDate;
            this.ForceRefreshRevisions();
        }

        private void checkoutBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCheckoutBranchDialog())
                this.RefreshRevisions();
        }

        private void cherryPickCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Revisions.RowCount > LastRow && LastRow >= 0)
            {
                FormCherryPickCommitSmall frm = new FormCherryPickCommitSmall((GitRevision)GetRevision(LastRow));
                frm.ShowDialog();
                RefreshRevisions();
            }

        }

        private void showRelativeDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.RelativeDate = !showRelativeDateToolStripMenuItem.Checked;
            showRelativeDateToolStripMenuItem.Checked = Settings.RelativeDate;
            this.ForceRefreshRevisions();
        }

        private string TimeToString(DateTime time)
        {
            if (Settings.RelativeDate)
            {
                TimeSpan span = DateTime.Now - time;

                if (span.Minutes < 0)
                {
                    return string.Format("{0} seconds ago", (int)span.Seconds);
                }
                if (span.TotalHours < 1)
                {
                    return string.Format("{0} minutes ago", (int)span.Minutes + Math.Round(span.Seconds / 60.0, 0));
                }
                if (span.TotalHours < 2)
                {
                    return "1 hour ago";
                }
                if (span.TotalHours < 24)
                {
                    return string.Format("{0} hours ago", (int)span.TotalHours + Math.Round(span.Minutes / 60.0, 0));
                }
                //if (span.TotalHours < 36)
                //{
                //    return "yesterday";
                //}
                if (span.TotalDays < 30)
                {
                    return string.Format("{0} days ago", (int)span.TotalDays + Math.Round(span.Hours / 24.0, 0));
                }
                if (span.TotalDays < 45)
                {
                    return "1 month ago";
                }
                if (span.TotalDays < 365)
                {
                    return string.Format("{0} months ago", (int)(span.TotalDays / 30));
                }

                return string.Format("{0:#.#} years ago", span.TotalDays / 365);
            }
            else
            {
                return time.ToShortDateString() + " " + time.ToLongTimeString();
            }
        }

    }
}
