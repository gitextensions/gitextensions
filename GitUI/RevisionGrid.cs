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
using ResourceManager.Translation;

namespace GitUI
{
    public partial class RevisionGrid : GitExtensionsControl
    {
        TranslationString authorDate = new TranslationString("AuthorDate");
        TranslationString commitDate = new TranslationString("CommitDate");
        TranslationString messageCaption = new TranslationString("Message");
        TranslationString authorCaption = new TranslationString("Author");

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
            InitializeComponent(); Translate();

            NormalFont = Revisions.Font;
            HeadFont = new Font(NormalFont, FontStyle.Underline);
            RefsFont = new Font(NormalFont, FontStyle.Bold);

            Revisions.CellPainting += new DataGridViewCellPaintingEventHandler(Revisions_CellPainting);
            Revisions.KeyDown += new KeyEventHandler(Revisions_KeyDown);
            
            showRevisionGraphToolStripMenuItem.Checked = Settings.ShowRevisionGraph;
            showAuthorDateToolStripMenuItem.Checked = Settings.ShowAuthorDate;
            orderRevisionsByDateToolStripMenuItem.Checked = Settings.OrderRevisionByDate;
            showRelativeDateToolStripMenuItem.Checked = Settings.RelativeDate;

            BranchFilter = String.Empty;
            SetShowBranches();
            filter = "";
            quickSearchString = "";
            quickSearchTimer.Tick += new EventHandler(quickSearchTimer_Tick);

            Revisions.Loading += new DvcsGraph.LoadingHandler(Revisions_Loading);
        }

        void Revisions_Loading(bool isLoading)
        {
            Loading.Visible = isLoading;
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
                    if (gitHead.Name.StartsWith(searchString, StringComparison.CurrentCultureIgnoreCase))
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

                Revisions.CurrentCell = Revisions.Rows[index].Cells[1];
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

        public string BranchFilter
        {
            get;
            set;
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
        public Font HeadFont;
        public Font RefsFont;

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

        /// <summary>
        /// Select the top revision.
        /// </summary>
        public void SelectTopRevision()
        {
            if(Revisions.Rows.Count == 0)
                return;

            Revisions.ClearSelection();
            Revisions.Rows[0].Selected = true;

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

        public void RefreshRevisions()
        {
            if (indexWatcher.IndexChanged)
            {
                ForceRefreshRevisions();
            }
        }

        public int LastScrollPos = 0;
        public IComparable[] LastSelectedRows = null;

        public void ForceRefreshRevisions()
        {
            try
            {
                initialLoad = true;

                LastScrollPos = Revisions.FirstDisplayedScrollingRowIndex;

                //Hide graph column when there it is disabled OR when a filter is active
                if (!Settings.ShowRevisionGraph || !string.IsNullOrEmpty(Filter))
                {
                    Revisions.ShowHideRevisionGraph(false);
                }
                else
                {
                    Revisions.ShowHideRevisionGraph(true);
                }

                
                if (revisionGraphCommand != null)
                {
                    revisionGraphCommand.Kill();
                }

                string newCurrentCheckout = GitCommands.GitCommands.GetCurrentCheckout();

                // If the current checkout changed, don't get the currently selected rows, select the
                // new current checkout instead.
                if (newCurrentCheckout == currentCheckout)
                {
                    LastSelectedRows = Revisions.SelectedIds;
                }
                Revisions.ClearSelection();

                currentCheckout = newCurrentCheckout;
                Error.Visible = false;
                NoCommits.Visible = false;
                NoGit.Visible = false;
                Revisions.Visible = true;
                Loading.Visible = true;

                Revisions.Clear();

                if (!GitCommands.Settings.ValidWorkingDir())
                {
                    Revisions.Visible = false;

                    NoCommits.Visible = false;
                    NoGit.Visible = true;
                    Loading.Visible = false;
                    return;
                }

                Revisions.Enabled = false;
                Loading.Visible = true;
                indexWatcher.Reset();
                revisionGraphCommand = new RevisionGraph();
                revisionGraphCommand.BranchFilter = BranchFilter;
                revisionGraphCommand.LogParam = LogParam + Filter;
                revisionGraphCommand.Updated += new EventHandler(gitGetCommitsCommand_Updated);
                revisionGraphCommand.Exited += new EventHandler(gitGetCommitsCommand_Exited);
                revisionGraphCommand.Execute();

                LoadRevisions();
            }
            catch (Exception exception)
            {
                Error.Visible = true;
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void gitGetCommitsCommand_Updated(object sender, EventArgs e)
        {
            RevisionGraph.RevisionGraphUpdatedEvent updatedEvent = (RevisionGraph.RevisionGraphUpdatedEvent)e;
            update(updatedEvent.Revision);
        }
        
        void gitGetCommitsCommand_Exited(object sender, EventArgs e)
        {
            Revisions.SetExpectedRowCount(revisionGraphCommand.Revisions.Count);
            update(null);

            if (revisionGraphCommand.Revisions.Count == 0)
            {
                // This has to happen on the UI thread
                syncContext.Send(o =>
                                     {
                                         NoGit.Visible = false;
                                         NoCommits.Visible = true;
                                         Revisions.Visible = false;
                                         Loading.Visible = false;
                                     }, this);
            }
            else
            {
                // This has to happen on the UI thread
                syncContext.Send(o =>
                                     {
                                         Loading.Visible = false;
                                         if (Revisions.SelectedRows.Count == 0)
                                             SelectTopRevision();
                                     }, this);
            }
        }

        void update(GitRevision rev)
        {
            if( rev == null )
            {
                // Prune the graph and make sure the row count matches reality
                Revisions.Prune();
                Revisions.SetExpectedRowCount(-1);
                return;
            }
        
            if (rev.AuthorDate == null)
            {
                // This should never happen.
                return;
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
            Revisions.Add(rev.Guid, rev.ParentGuids, dataType, rev);
        }

        public string currentCheckout { get; set; }
        private bool initialLoad = true;

        private string GetDateHeaderText()
        {
            return Settings.ShowAuthorDate ? authorDate.Text : commitDate.Text;
        }

        private void LoadRevisions()
        {
            if (revisionGraphCommand == null)
            {
                return;
            }

            Revisions.SuspendLayout();

            Revisions.Columns[1].HeaderText = messageCaption.Text;
            Revisions.Columns[2].HeaderText = authorCaption.Text;
            Revisions.Columns[3].HeaderText = GetDateHeaderText();

            Revisions.SelectionChanged -= new EventHandler(Revisions_SelectionChanged);

            if (LastSelectedRows != null)
            {
                Revisions.SelectedIds = LastSelectedRows;
                LastSelectedRows = null;
            }
            else
            {
                Revisions.SelectedIds = new IComparable[] { currentCheckout };
            }

            if (LastScrollPos > 0 && Revisions.RowCount > LastScrollPos)
            {
                Revisions.FirstDisplayedScrollingRowIndex = LastScrollPos;
                LastScrollPos = -1;
            }

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

                    if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
                        e.Graphics.FillRectangle(new LinearGradientBrush(e.CellBounds, Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor, Color.LightBlue, 90, false), e.CellBounds);
                    else
                        e.Graphics.FillRectangle(new SolidBrush(Color.White), e.CellBounds);

                    Brush foreBrush = new SolidBrush(e.CellStyle.ForeColor);

                    Font rowFont;
                    if (revision.Guid == currentCheckout)
                    {
                        rowFont = HeadFont;
                    }
                    else
                    {
                        rowFont = NormalFont;
                    }

                    if (column == 1)
                    {
                        float offset = 0;
                        foreach (GitHead h in revision.Heads)
                        {
                            if ((h.IsRemote && !ShowRemoteBranches.Checked) == false)
                            {
                                SolidBrush brush = new SolidBrush(h.IsTag == true ? Settings.TagColor : h.IsHead ? Settings.BranchColor : h.IsRemote ? Settings.RemoteBranchColor : Settings.OtherTagColor);

                                e.Graphics.DrawString("[" + h.Name + "] ", RefsFont, brush, new PointF(e.CellBounds.Left + offset, e.CellBounds.Top + 4));

                                offset += e.Graphics.MeasureString("[" + h.Name + "] ", RefsFont).Width;
                            }
                        }
                        string text = revision.Message;
                        e.Graphics.DrawString(text, rowFont, foreBrush, new PointF(e.CellBounds.Left + offset, e.CellBounds.Top + 4));
                    }
                    else if (column == 2)
                    {
                        string text = revision.Author;
                        e.Graphics.DrawString(text, rowFont, foreBrush, new PointF(e.CellBounds.Left, e.CellBounds.Top + 4));
                    }
                    else if (column == 3)
                    {
                        DateTime time = Settings.ShowAuthorDate ? revision.AuthorDate : revision.CommitDate;
                        string text = TimeToString(time);
                        e.Graphics.DrawString(text, rowFont, foreBrush, new PointF(e.CellBounds.Left, e.CellBounds.Top + 4));
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

        private void showCurrentBranchOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showCurrentBranchOnlyToolStripMenuItem.Checked)
            {
                return;
            }

            Settings.BranchFilterEnabled = true;
            Settings.ShowCurrentBranchOnly = true;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        private void showAllBranchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showAllBranchesToolStripMenuItem.Checked)
            {
                return;
            }
            
            Settings.BranchFilterEnabled = false;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        private void showFilteredBranchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showFilteredBranchesToolStripMenuItem.Checked)
            {
                return;
            }

            Settings.BranchFilterEnabled = true;
            Settings.ShowCurrentBranchOnly = false;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        private void SetShowBranches()
        {

            showAllBranchesToolStripMenuItem.Checked = !Settings.BranchFilterEnabled;
            showCurrentBranchOnlyToolStripMenuItem.Checked = Settings.BranchFilterEnabled
                                                             && Settings.ShowCurrentBranchOnly;
            showFilteredBranchesToolStripMenuItem.Checked = Settings.BranchFilterEnabled
                                                            && !Settings.ShowCurrentBranchOnly;

            BranchFilter = RevisionFilter.GetBranchFilter();

            if (!Settings.BranchFilterEnabled)
                LogParam = "HEAD --all --boundary";
            else if (Settings.ShowCurrentBranchOnly)
                LogParam = "HEAD";
            else
                LogParam = BranchFilter.Length > 0 
                    ? String.Empty 
                    : "HEAD --all --boundary";
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

            Revisions.ShowHideRevisionGraph(Settings.ShowRevisionGraph);
            //Not needed
            //this.ForceRefreshRevisions();
        }

        private readonly FormRevisionFilter RevisionFilter = new FormRevisionFilter();
        private void filterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RevisionFilter.ShowDialog();
            filter = RevisionFilter.GetFilter();
            BranchFilter = RevisionFilter.GetBranchFilter();
            SetShowBranches();
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

            new FormProcess(GitCommands.GitCommands.DeleteTagCmd(toolStripItem.Text)).ShowDialog();
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

            new FormProcess("checkout \"" + toolStripItem.Text + "\"").ShowDialog();

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
                    new FormProcess("checkout \"" + GetRevision(LastRow).Guid + "\"").ShowDialog();
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
            if (time == DateTime.MinValue || time == DateTime.MaxValue)
            {
                return "";
            }

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
                    return string.Format("{0} months ago", (int)Math.Round(span.TotalDays / 30, 0));
                }

                return string.Format("{0:#.#} years ago", Math.Round(span.TotalDays / 365), 1);
            }
            else
            {
                return time.ToShortDateString() + " " + time.ToLongTimeString();
            }
        }
    }
}
