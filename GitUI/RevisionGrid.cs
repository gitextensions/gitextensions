using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class RevisionGrid : GitExtensionsControl
    {
        private readonly TranslationString _authorCaption = new TranslationString("Author");
        private readonly TranslationString _authorDateCaption = new TranslationString("AuthorDate");
        private readonly TranslationString _commitDateCaption = new TranslationString("CommitDate");
        private readonly IndexWatcher _indexWatcher = new IndexWatcher();
        private readonly GitRevision _initialSelectedRevision;
        private readonly TranslationString _messageCaption = new TranslationString("Message");
        private readonly FormRevisionFilter _revisionFilter = new FormRevisionFilter();

        private readonly SynchronizationContext _syncContext;
        public string LogParam = "HEAD --all --boundary";
        private bool _contextMenuEnabled = true;

        private bool _initialLoad = true;
        private string _lastQuickSearchString = string.Empty;
        private Label _quickSearchLabel;
        private string _quickSearchString;
        private RevisionGraph _revisionGraphCommand;


        public RevisionGrid() : this(null)
        {
        }

        public RevisionGrid(GitRevision initialSelectedRevision)
        {
            _initialSelectedRevision = initialSelectedRevision;
            _syncContext = SynchronizationContext.Current;

            base.InitLayout();
            InitializeComponent();
            Translate();

            NormalFont = Revisions.Font;
            HeadFont = new Font(NormalFont, FontStyle.Underline);
            RefsFont = new Font(NormalFont, FontStyle.Bold);

            Revisions.CellPainting += RevisionsCellPainting;
            Revisions.KeyDown += RevisionsKeyDown;

            showRevisionGraphToolStripMenuItem.Checked = Settings.ShowRevisionGraph;
            showAuthorDateToolStripMenuItem.Checked = Settings.ShowAuthorDate;
            orderRevisionsByDateToolStripMenuItem.Checked = Settings.OrderRevisionByDate;
            showRelativeDateToolStripMenuItem.Checked = Settings.RelativeDate;

            BranchFilter = String.Empty;
            SetShowBranches();
            Filter = "";
            _quickSearchString = "";
            quickSearchTimer.Tick += QuickSearchTimerTick;

            Revisions.Loading += RevisionsLoading;
        }

        public Font HeadFont { get; private set; }
        public int LastScrollPos { get; private set; }
        public IComparable[] LastSelectedRows { get; private set; }
        public Font RefsFont { get; private set; }
        public string Filter { get; set; }
        public string BranchFilter { get; set; }
        public Font NormalFont { get; set; }
        public string CurrentCheckout { get; set; }
        public int LastRow { get; set; }

        public event EventHandler ChangedCurrentBranch;

        public virtual void OnChangedCurrentBranch()
        {
            if (ChangedCurrentBranch != null)
                ChangedCurrentBranch(this, null);
        }

        private void RevisionsLoading(bool isLoading)
        {
            Loading.Visible = isLoading;
        }

        private void ShowQuickSearchString()
        {
            if (_quickSearchLabel == null)
            {
                _quickSearchLabel
                    = new Label
                          {
                              Location = new Point(10, 10),
                              BorderStyle = BorderStyle.FixedSingle,
                              ForeColor = SystemColors.InfoText,
                              BackColor = SystemColors.Info
                          };
                Controls.Add(_quickSearchLabel);
            }

            _quickSearchLabel.Visible = true;
            _quickSearchLabel.BringToFront();
            _quickSearchLabel.Text = _quickSearchString;
            _quickSearchLabel.AutoSize = true;
        }

        private void HideQuickSearchString()
        {
            if (_quickSearchLabel != null)
                _quickSearchLabel.Visible = false;
        }

        private void QuickSearchTimerTick(object sender, EventArgs e)
        {
            quickSearchTimer.Stop();
            _quickSearchString = "";
            HideQuickSearchString();
        }

        private void RevisionsKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt && e.KeyCode == Keys.Up)
            {
                var nextIndex = 0;
                if (Revisions.SelectedRows.Count > 0)
                    nextIndex = Revisions.SelectedRows[0].Index - 1;

                FindNextMatch(nextIndex, _lastQuickSearchString, true);
                e.Handled = true;
                return;
            }
            if (e.Alt && e.KeyCode == Keys.Down)
            {
                var nextIndex = 0;
                if (Revisions.SelectedRows.Count > 0)
                    nextIndex = Revisions.SelectedRows[0].Index + 1;

                FindNextMatch(nextIndex, _lastQuickSearchString, false);
                e.Handled = true;
                return;
            }
            var key = (char) e.KeyValue;
            if (!e.Alt && !e.Control && char.IsLetterOrDigit(key) || char.IsNumber(key) || char.IsSeparator(key))
            {
                quickSearchTimer.Stop();
                quickSearchTimer.Interval = 700;
                quickSearchTimer.Start();

                _quickSearchString = string.Concat(_quickSearchString, (char) e.KeyValue).ToLower();

                var oldIndex = 0;
                if (Revisions.SelectedRows.Count > 0)
                    oldIndex = Revisions.SelectedRows[0].Index;

                FindNextMatch(oldIndex, _quickSearchString, false);
                _lastQuickSearchString = _quickSearchString;

                e.Handled = true;
                ShowQuickSearchString();
            }
            else
            {
                _quickSearchString = "";
                HideQuickSearchString();
                e.Handled = false;
                return;
            }
        }

        private void FindNextMatch(int startIndex, string searchString, bool reverse)
        {
            if (Revisions.RowCount == 0)
                return;

            var searchResult =
                reverse
                    ? SearchInReverseOrder(startIndex, searchString)
                    : SearchForward(startIndex, searchString);

            if (searchResult.IsNone)
                return;

            Revisions.ClearSelection();
            Revisions.Rows[searchResult.Some].Selected = true;

            Revisions.CurrentCell = Revisions.Rows[searchResult.Some].Cells[1];
        }

        private Option<int> SearchForward(int startIndex, string searchString)
        {
            // Check for out of bounds roll over if required
            int index;
            if (startIndex < 0 || startIndex >= Revisions.RowCount)
                startIndex = 0;

            for (index = startIndex; index < Revisions.RowCount; ++index)
            {
                if (((GitRevision) Revisions.GetRowData(index)).MatchesSearchString(searchString))
                    return Option<int>.From(index);
            }

            // We didn't find it so start searching from the top
            for (index = 0; index < startIndex; ++index)
            {
                if (((GitRevision) Revisions.GetRowData(index)).MatchesSearchString(searchString))
                    return Option<int>.From(index);
            }

            return Option<int>.None;
        }

        private Option<int> SearchInReverseOrder(int startIndex, string searchString)
        {
            // Check for out of bounds roll over if required
            int index;
            if (startIndex < 0 || startIndex >= Revisions.RowCount)
                startIndex = Revisions.RowCount - 1;

            for (index = startIndex; index >= 0; --index)
            {
                if (((GitRevision) Revisions.GetRowData(index)).MatchesSearchString(searchString))
                    return Option<int>.From(index);
            }

            // We didn't find it so start searching from the bottom
            for (index = Revisions.RowCount - 1; index > startIndex; --index)
            {
                if (((GitRevision) Revisions.GetRowData(index)).MatchesSearchString(searchString))
                    return Option<int>.From(index);
            }


            return Option<int>.None;
        }

        public void DisableContextMenu()
        {
            Revisions.ContextMenuStrip = null;
            _contextMenuEnabled = false;
        }

        public string FormatQuickFilter(string filter)
        {
            return
                string.IsNullOrEmpty(filter)
                    ? ""
                    : string.Format(" --regexp-ignore-case --grep=\"{0}\" --committer=\"{0}\" --author=\"{0}\" ",
                                    filter);
        }

        ~RevisionGrid()
        {
            if (_revisionGraphCommand != null)
                _revisionGraphCommand.Kill();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            RefreshRevisions();
        }

        public event EventHandler SelectionChanged;

        public void SetSelectedIndex(int index)
        {
            Revisions.ClearSelection();

            Revisions.Rows[index].Selected = true;

            Revisions.Select();
        }

        public void SetSelectedRevision(GitRevision revision)
        {
            Revisions.ClearSelection();

            if (revision != null)
            {
                foreach (DataGridViewRow row in Revisions.Rows)
                {
                    if (((GitRevision) row.DataBoundItem).Guid == revision.Guid)
                        row.Selected = true;
                }
            }
            Revisions.Select();
        }

        private void RevisionsSelectionChanged(object sender, EventArgs e)
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
            var retval = new List<GitRevision>();

            foreach (DataGridViewRow row in Revisions.SelectedRows)
            {
                if (Revisions.RowCount > row.Index)
                    retval.Add(GetRevision(row.Index));
            }
            return retval;
        }

        private GitRevision GetRevision(int aRow)
        {
            return Revisions.GetRowData(aRow) as GitRevision;
        }

        public void RefreshRevisions()
        {
            if (_indexWatcher.IndexChanged)
                ForceRefreshRevisions();
        }

        public void ForceRefreshRevisions()
        {
            try
            {
                _initialLoad = true;

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


                if (_revisionGraphCommand != null)
                {
                    _revisionGraphCommand.Kill();
                }

                var newCurrentCheckout = GitCommands.GitCommands.GetCurrentCheckout();

                // If the current checkout changed, don't get the currently selected rows, select the
                // new current checkout instead.
                if (newCurrentCheckout == CurrentCheckout)
                {
                    LastSelectedRows = Revisions.SelectedIds;
                }
                Revisions.ClearSelection();

                CurrentCheckout = newCurrentCheckout;
                Error.Visible = false;
                NoCommits.Visible = false;
                NoGit.Visible = false;
                Revisions.Visible = true;
                Loading.Visible = true;

                Revisions.Clear();

                if (!Settings.ValidWorkingDir())
                {
                    Revisions.Visible = false;

                    NoCommits.Visible = false;
                    NoGit.Visible = true;
                    Loading.Visible = false;
                    return;
                }

                Revisions.Enabled = false;
                Loading.Visible = true;
                _indexWatcher.Reset();
                _revisionGraphCommand = new RevisionGraph {BranchFilter = BranchFilter, LogParam = LogParam + Filter};
                _revisionGraphCommand.Updated += GitGetCommitsCommandUpdated;
                _revisionGraphCommand.Exited += GitGetCommitsCommandExited;
                _revisionGraphCommand.Execute();

                LoadRevisions();
            }
            catch (Exception exception)
            {
                Error.Visible = true;
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GitGetCommitsCommandUpdated(object sender, EventArgs e)
        {
            var updatedEvent = (RevisionGraph.RevisionGraphUpdatedEvent) e;
            UpdateGraph(updatedEvent.Revision);
        }

        private bool FilterIsApplied()
        {
            return Filter != "" || BranchFilter != "";
        }

        private void GitGetCommitsCommandExited(object sender, EventArgs e)
        {
            Revisions.SetExpectedRowCount(_revisionGraphCommand.Revisions.Count);
            UpdateGraph(null);

            if (_revisionGraphCommand.Revisions.Count == 0 && !FilterIsApplied())
            {
                // This has to happen on the UI thread
                _syncContext.Send(o =>
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
                _syncContext.Send(o =>
                                      {
                                          Loading.Visible = false;
                                          SelectInitialRevision();
                                      }, this);
            }
        }

        private void SelectInitialRevision()
        {
            if (Revisions.SelectedRows.Count != 0 || _initialSelectedRevision == null) return;
            for (var i = 0; i < _revisionGraphCommand.Revisions.Count; i++)
            {
                if (_revisionGraphCommand.Revisions[i].Guid == _initialSelectedRevision.Guid)
                    SetSelectedIndex(i);
            }
        }

        private string GetDateHeaderText()
        {
            return Settings.ShowAuthorDate ? _authorDateCaption.Text : _commitDateCaption.Text;
        }

        private void LoadRevisions()
        {
            if (_revisionGraphCommand == null)
            {
                return;
            }

            Revisions.SuspendLayout();

            Revisions.Columns[1].HeaderText = _messageCaption.Text;
            Revisions.Columns[2].HeaderText = _authorCaption.Text;
            Revisions.Columns[3].HeaderText = GetDateHeaderText();

            Revisions.SelectionChanged -= RevisionsSelectionChanged;

            if (LastSelectedRows != null)
            {
                Revisions.SelectedIds = LastSelectedRows;
                LastSelectedRows = null;
            }
            else
            {
                Revisions.SelectedIds = new IComparable[] {CurrentCheckout};
            }

            if (LastScrollPos > 0 && Revisions.RowCount > LastScrollPos)
            {
                Revisions.FirstDisplayedScrollingRowIndex = LastScrollPos;
                LastScrollPos = -1;
            }

            Revisions.Enabled = true;
            Revisions.Focus();
            Revisions.SelectionChanged += RevisionsSelectionChanged;

            Revisions.ResumeLayout();

            if (!_initialLoad)
                return;

            _initialLoad = false;
            SelecctionTimer.Enabled = false;
            SelecctionTimer.Stop();
            SelecctionTimer.Enabled = true;
            SelecctionTimer.Start();
        }

        private void RevisionsCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // The graph column is handled by the DvcsGraph
            if (e.ColumnIndex == 0)
            {
                return;
            }

            var column = e.ColumnIndex;
            if (e.RowIndex < 0 || (e.State & DataGridViewElementStates.Visible) == 0)
                return;

            if (Revisions.RowCount <= e.RowIndex)
                return;

            var revision = GetRevision(e.RowIndex);
            if (revision == null)
                return;

            e.Handled = true;

            if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
                e.Graphics.FillRectangle(
                    new LinearGradientBrush(e.CellBounds,
                                            Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor,
                                            Color.LightBlue, 90, false), e.CellBounds);
            else
                e.Graphics.FillRectangle(new SolidBrush(Color.White), e.CellBounds);

            Brush foreBrush = new SolidBrush(e.CellStyle.ForeColor);
            var rowFont = revision.Guid == CurrentCheckout ? HeadFont : NormalFont;

            switch (column)
            {
                case 1:
                    {
                        float offset = 0;
                        var heads = revision.Heads;
                        if (heads.Count > 0)
                        {
                            heads.Sort(new Comparison<GitHead>(
                                           (left, right) =>
                                           {
                                               if (left.IsTag != right.IsTag)
                                                   return right.IsTag.CompareTo(left.IsTag);
                                               if (left.IsRemote != right.IsRemote)
                                                   return left.IsRemote.CompareTo(right.IsRemote);
                                               return left.Name.CompareTo(right.Name);
                                           }));

                            foreach (var head in heads)
                            {
                                if ((head.IsRemote && !ShowRemoteBranches.Checked))
                                    continue;

                                var brush =
                                    new SolidBrush(head.IsTag
                                                       ? Settings.TagColor
                                                       : head.IsHead
                                                             ? Settings.BranchColor
                                                             : head.IsRemote
                                                                   ? Settings.RemoteBranchColor
                                                                   : Settings.OtherTagColor);

                                var headName = "[" + head.Name + "] ";
                                e.Graphics.DrawString(headName, RefsFont, brush,
                                                      new PointF(e.CellBounds.Left + offset, e.CellBounds.Top + 4));

                                offset += e.Graphics.MeasureString(headName, RefsFont).Width;
                            }
                        }
                        var text = revision.Message;
                        e.Graphics.DrawString(text, rowFont, foreBrush,
                                              new PointF(e.CellBounds.Left + offset, e.CellBounds.Top + 4));
                    }
                    break;
                case 2:
                    {
                        var text = revision.Author;
                        e.Graphics.DrawString(text, rowFont, foreBrush,
                                              new PointF(e.CellBounds.Left, e.CellBounds.Top + 4));
                    }
                    break;
                case 3:
                    {
                        var time = Settings.ShowAuthorDate ? revision.AuthorDate : revision.CommitDate;
                        var text = TimeToString(time);
                        e.Graphics.DrawString(text, rowFont, foreBrush,
                                              new PointF(e.CellBounds.Left, e.CellBounds.Top + 4));
                    }
                    break;
            }
        }

        private void RevisionsDoubleClick(object sender, EventArgs e)
        {
            var r = GetRevisions();
            if (r.Count > 0)
            {
                var form = new FormDiffSmall();
                form.SetRevision(r[0]);
                form.ShowDialog();
            }
            else
                GitUICommands.Instance.StartCompareRevisionsDialog();
        }

        private void SelecctionTimerTick(object sender, EventArgs e)
        {
            SelecctionTimer.Enabled = false;
            SelecctionTimer.Stop();
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        private void CreateTagToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            var frm = new FormTagSmall {Revision = GetRevision(LastRow)};
            frm.ShowDialog();
            RefreshRevisions();
        }

        private void ResetCurrentBranchToHereToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            var frm = new FormResetCurrentBranch(GetRevision(LastRow));
            frm.ShowDialog();
            RefreshRevisions();
        }

        private void CreateNewBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            var frm = new FormBranchSmall {Revision = GetRevision(LastRow)};
            frm.ShowDialog();
            RefreshRevisions();
            OnChangedCurrentBranch();
        }

        private void RevisionsMouseClick(object sender, MouseEventArgs e)
        {
            var pt = Revisions.PointToClient(Cursor.Position);
            var hti = Revisions.HitTest(pt.X, pt.Y);
            LastRow = hti.RowIndex;
        }

        private void RevisionsCellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            var pt = Revisions.PointToClient(Cursor.Position);
            var hti = Revisions.HitTest(pt.X, pt.Y);
            LastRow = hti.RowIndex;
            Revisions.ClearSelection();
            if (LastRow >= 0 && Revisions.Rows.Count > LastRow)
                Revisions.Rows[LastRow].Selected = true;
        }

        private void CommitClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartCommitDialog();
            RefreshRevisions();
        }

        private static void GitIgnoreClick(object sender, EventArgs e)
        {
            GitUICommands.Instance.StartEditGitIgnoreDialog();
        }

        private void ShowRemoteBranchesClick(object sender, EventArgs e)
        {
            ShowRemoteBranches.Checked = !ShowRemoteBranches.Checked;
            Revisions.Invalidate();
        }

        private void ShowCurrentBranchOnlyToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (showCurrentBranchOnlyToolStripMenuItem.Checked)
                return;

            Settings.BranchFilterEnabled = true;
            Settings.ShowCurrentBranchOnly = true;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        private void ShowAllBranchesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (showAllBranchesToolStripMenuItem.Checked)
                return;

            Settings.BranchFilterEnabled = false;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        private void ShowFilteredBranchesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (showFilteredBranchesToolStripMenuItem.Checked)
                return;

            Settings.BranchFilterEnabled = true;
            Settings.ShowCurrentBranchOnly = false;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        private void SetShowBranches()
        {
            showAllBranchesToolStripMenuItem.Checked = !Settings.BranchFilterEnabled;
            showCurrentBranchOnlyToolStripMenuItem.Checked =
                Settings.BranchFilterEnabled && Settings.ShowCurrentBranchOnly;
            showFilteredBranchesToolStripMenuItem.Checked =
                Settings.BranchFilterEnabled && !Settings.ShowCurrentBranchOnly;

            BranchFilter = _revisionFilter.GetBranchFilter();

            if (!Settings.BranchFilterEnabled)
                LogParam = "HEAD --all --boundary";
            else if (Settings.ShowCurrentBranchOnly)
                LogParam = "HEAD";
            else
                LogParam = BranchFilter.Length > 0
                               ? String.Empty
                               : "HEAD --all --boundary";
        }

        private void RevertCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            var frm = new FormRevertCommitSmall(GetRevision(LastRow));
            frm.ShowDialog();
            RefreshRevisions();
        }

        private void ShowRevisionGraphToolStripMenuItemClick(object sender, EventArgs e)
        {
            Settings.ShowRevisionGraph = !showRevisionGraphToolStripMenuItem.Checked;
            showRevisionGraphToolStripMenuItem.Checked = Settings.ShowRevisionGraph;

            Revisions.ShowHideRevisionGraph(Settings.ShowRevisionGraph);
        }

        private void FilterToolStripMenuItemClick(object sender, EventArgs e)
        {
            _revisionFilter.ShowDialog();
            Filter = _revisionFilter.GetFilter();
            BranchFilter = _revisionFilter.GetBranchFilter();
            SetShowBranches();
            ForceRefreshRevisions();
        }

        private void RevisionsKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F && _contextMenuEnabled)
                FilterToolStripMenuItemClick(null, null);
        }

        private void CreateTagOpening(object sender, CancelEventArgs e)
        {
            if (Revisions.RowCount < LastRow || LastRow < 0 || Revisions.RowCount == 0)
                return;

            var revision = GetRevision(LastRow);

            var tagDropDown = new ToolStripDropDown();
            var branchDropDown = new ToolStripDropDown();
            var checkoutBranchDropDown = new ToolStripDropDown();
            var mergeBranchDropDown = new ToolStripDropDown();
            var rebaseDropDown = new ToolStripDropDown();

            foreach (var head in revision.Heads)
            {
                if (head.IsTag)
                {
                    ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Click += ToolStripItemClick;
                    tagDropDown.Items.Add(toolStripItem);
                }
                else if (head.IsHead || head.IsRemote)
                {
                    ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Click += ToolStripItemClickMergeBranch;
                    mergeBranchDropDown.Items.Add(toolStripItem);

                    toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Click += ToolStripItemClickRebaseBranch;
                    rebaseDropDown.Items.Add(toolStripItem);

                    if (head.IsHead && !head.IsRemote)
                    {
                        toolStripItem = new ToolStripMenuItem(head.Name);
                        toolStripItem.Click += ToolStripItemClickBranch;
                        branchDropDown.Items.Add(toolStripItem);

                        toolStripItem = new ToolStripMenuItem(head.Name);
                        toolStripItem.Click += ToolStripItemClickCheckoutBranch;
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

        private void ToolStripItemClick(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            new FormProcess(GitCommands.GitCommands.DeleteTagCmd(toolStripItem.Text)).ShowDialog();
            ForceRefreshRevisions();
        }

        private void ToolStripItemClickBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            GitUICommands.Instance.StartDeleteBranchDialog(toolStripItem.Text);

            ForceRefreshRevisions();
        }

        private void ToolStripItemClickCheckoutBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            new FormProcess("checkout \"" + toolStripItem.Text + "\"").ShowDialog();

            ForceRefreshRevisions();
            OnChangedCurrentBranch();
        }

        private void ToolStripItemClickMergeBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            GitUICommands.Instance.StartMergeBranchDialog(toolStripItem.Text);

            ForceRefreshRevisions();
        }

        private void ToolStripItemClickRebaseBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            GitUICommands.Instance.StartRebaseDialog(toolStripItem.Text);

            ForceRefreshRevisions();
        }

        private void CheckoutRevisionToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            if (MessageBox.Show("Are you sure to checkout the selected revision", "Checkout revision",
                                MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            new FormProcess(string.Format("checkout \"{0}\"", GetRevision(LastRow).Guid)).ShowDialog();
            ForceRefreshRevisions();
            OnChangedCurrentBranch();
        }

        private void ShowAuthorDateToolStripMenuItemClick(object sender, EventArgs e)
        {
            Settings.ShowAuthorDate = !showAuthorDateToolStripMenuItem.Checked;
            showAuthorDateToolStripMenuItem.Checked = Settings.ShowAuthorDate;
            ForceRefreshRevisions();
        }

        private void OrderRevisionsByDateToolStripMenuItemClick(object sender, EventArgs e)
        {
            Settings.OrderRevisionByDate = !orderRevisionsByDateToolStripMenuItem.Checked;
            orderRevisionsByDateToolStripMenuItem.Checked = Settings.OrderRevisionByDate;
            ForceRefreshRevisions();
        }

        private void CheckoutBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (GitUICommands.Instance.StartCheckoutBranchDialog())
                RefreshRevisions();
        }

        private void CherryPickCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            var frm = new FormCherryPickCommitSmall(GetRevision(LastRow));
            frm.ShowDialog();
            RefreshRevisions();
        }

        private void ShowRelativeDateToolStripMenuItemClick(object sender, EventArgs e)
        {
            Settings.RelativeDate = !showRelativeDateToolStripMenuItem.Checked;
            showRelativeDateToolStripMenuItem.Checked = Settings.RelativeDate;
            ForceRefreshRevisions();
        }

        private static string TimeToString(DateTime time)
        {
            if (time == DateTime.MinValue || time == DateTime.MaxValue)
                return "";

            if (!Settings.RelativeDate)
                return string.Format("{0} {1}", time.ToShortDateString(), time.ToLongTimeString());

            var span = DateTime.Now - time;

            if (span.Minutes < 0)
                return string.Format("{0} seconds ago", span.Seconds);

            if (span.TotalHours < 1)
                return string.Format("{0} minutes ago", span.Minutes + Math.Round(span.Seconds/60.0, 0));

            if (span.TotalHours < 2)
                return "1 hour ago";

            if (span.TotalHours < 24)
                return string.Format("{0} hours ago", (int) span.TotalHours + Math.Round(span.Minutes/60.0, 0));

            if (span.TotalDays < 30)
                return string.Format("{0} days ago", (int) span.TotalDays + Math.Round(span.Hours/24.0, 0));

            if (span.TotalDays < 45)
                return "1 month ago";

            if (span.TotalDays < 365)
                return string.Format("{0} months ago", (int) Math.Round(span.TotalDays/30, 0));

            return string.Format("{0:#.#} years ago", Math.Round(span.TotalDays/365));
        }

        private void UpdateGraph(GitRevision rev)
        {
            if (rev == null)
            {
                // Prune the graph and make sure the row count matches reality
                Revisions.Prune();
                Revisions.SetExpectedRowCount(-1);
                return;
            }

            var dataType = DvcsGraph.DataType.Normal;
            if (rev.Guid == CurrentCheckout)
                dataType = DvcsGraph.DataType.Active;
            else if (rev.Heads.Count > 0)
                dataType = DvcsGraph.DataType.Special;

            Revisions.Add(rev.Guid, rev.ParentGuids, dataType, rev);
        }
    }
}