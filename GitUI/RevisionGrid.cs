using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitUI.Tag;
using ResourceManager.Translation;
using System.Text.RegularExpressions;

namespace GitUI
{
    public partial class RevisionGrid : GitExtensionsControl
    {
        private readonly TranslationString _authorCaption = new TranslationString("Author");
        private readonly TranslationString _authorDateCaption = new TranslationString("Author date");
        private readonly TranslationString _commitDateCaption = new TranslationString("Commit date");
        private readonly IndexWatcher _indexWatcher = new IndexWatcher();
        private readonly TranslationString _messageCaption = new TranslationString("Message");
        private readonly TranslationString _currentWorkingDirChanges = new TranslationString("Current uncommitted changes");
        private readonly TranslationString _currentIndex = new TranslationString("Commit index");
        private readonly TranslationString _secondsAgo = new TranslationString("{0} seconds ago");
        private readonly TranslationString _minutesAgo = new TranslationString("{0} minutes ago");
        private readonly TranslationString _hourAgo = new TranslationString("{0} hour ago");
        private readonly TranslationString _hoursAgo = new TranslationString("{0} hours ago");
        private readonly TranslationString _dayAgo = new TranslationString("{0} day ago");
        private readonly TranslationString _daysAgo = new TranslationString("{0} days ago");
        private readonly TranslationString _monthAgo = new TranslationString("{0} month ago");
        private readonly TranslationString _monthsAgo = new TranslationString("{0} months ago");
        private readonly TranslationString _yearsAgo = new TranslationString("{0} years ago");

        private readonly FormRevisionFilter _revisionFilter = new FormRevisionFilter();

        private readonly SynchronizationContext _syncContext;
        public string LogParam = "HEAD --all --boundary";
        private bool _contextMenuEnabled = true;

        private bool _initialLoad = true;
        private GitRevision _initialSelectedRevision;
        private string _lastQuickSearchString = string.Empty;
        private Label _quickSearchLabel;
        private string _quickSearchString;
        private RevisionGraph _revisionGraphCommand;

        public RevisionGrid()
        {
            _syncContext = SynchronizationContext.Current;

            base.InitLayout();
            InitializeComponent();
            Translate();

            Message.DefaultCellStyle.Font = SystemFonts.DefaultFont;
            Date.DefaultCellStyle.Font = SystemFonts.DefaultFont;

            NormalFont = SystemFonts.DefaultFont;
            RefsFont = new Font(NormalFont, FontStyle.Bold);
            HeadFont = new Font(NormalFont, FontStyle.Bold);
            Loading.Paint += new PaintEventHandler(Loading_Paint);

            Revisions.CellPainting += RevisionsCellPainting;
            Revisions.KeyDown += RevisionsKeyDown;

            showRevisionGraphToolStripMenuItem.Checked = Settings.ShowRevisionGraph;
            showAuthorDateToolStripMenuItem.Checked = Settings.ShowAuthorDate;
            orderRevisionsByDateToolStripMenuItem.Checked = Settings.OrderRevisionByDate;
            showRelativeDateToolStripMenuItem.Checked = Settings.RelativeDate;
            drawNonrelativesGrayToolStripMenuItem.Checked = Settings.RevisionGraphDrawNonRelativesGray;

            BranchFilter = String.Empty;
            SetShowBranches();
            Filter = "";
            InMemFilterIgnoreCase = false;
            InMemAuthorFilter = "";
            InMemCommitterFilter = "";
            InMemMessageFilter = "";
            AllowGraphWithFilter = false;
            _quickSearchString = "";
            quickSearchTimer.Tick += QuickSearchTimerTick;

            Revisions.Loading += RevisionsLoading;
        }

        void Loading_Paint(object sender, PaintEventArgs e)
        {
            // If our loading state has changed since the last paint, update it.
            if (Loading.Visible != _isLoading)
            {
                Loading.Visible = _isLoading;
            }
        }

        public Font HeadFont { get; private set; }
        public int LastScrollPos { get; private set; }
        public IComparable[] LastSelectedRows { get; private set; }
        public Font RefsFont { get; private set; }
        public string Filter { get; set; }
        public bool InMemFilterIgnoreCase { get; set; }
        public string InMemAuthorFilter { get; set; }
        public string InMemCommitterFilter { get; set; }
        public string InMemMessageFilter { get; set; }

        public string BranchFilter { get; set; }
        public Font NormalFont { get; set; }
        public string CurrentCheckout { get; set; }
        public int LastRow { get; set; }
        public bool AllowGraphWithFilter { get; set; }

        public void SetInitialRevision(GitRevision initialSelectedRevision)
        {
            _initialSelectedRevision = initialSelectedRevision;
        }

        public event EventHandler ActionOnRepositoryPerformed;

        public virtual void OnActionOnRepositoryPerformed()
        {
            if (ActionOnRepositoryPerformed != null)
                ActionOnRepositoryPerformed(this, null);
        }

        private bool _isLoading = false;
        private void RevisionsLoading(bool isLoading)
        {
            // Since this can happen on a background thread, we'll just set a
            // flag and deal with it next time we paint (a bit of a hack, but
            // it works)
            _isLoading = isLoading;
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
                quickSearchTimer.Stop();
                quickSearchTimer.Interval = Settings.RevisionGridQuickSearchTimeout;
                quickSearchTimer.Start();

                var nextIndex = 0;
                if (Revisions.SelectedRows.Count > 0)
                    nextIndex = Revisions.SelectedRows[0].Index - 1;
                _quickSearchString = _lastQuickSearchString;
                FindNextMatch(nextIndex, _quickSearchString, true);
                ShowQuickSearchString();
                e.Handled = true;
                return;
            }
            if (e.Alt && e.KeyCode == Keys.Down)
            {
                quickSearchTimer.Stop();
                quickSearchTimer.Interval = Settings.RevisionGridQuickSearchTimeout;
                quickSearchTimer.Start();

                var nextIndex = 0;
                if (Revisions.SelectedRows.Count > 0)
                    nextIndex = Revisions.SelectedRows[0].Index + 1;
                _quickSearchString = _lastQuickSearchString;
                FindNextMatch(nextIndex, _quickSearchString, false);
                ShowQuickSearchString();
                e.Handled = true;
                return;
            }


            int key = e.KeyValue;
            if (!e.Alt && !e.Control && key == 8 && _quickSearchString.Length > 1) //backspace
            {
                quickSearchTimer.Stop();
                quickSearchTimer.Interval = Settings.RevisionGridQuickSearchTimeout;
                quickSearchTimer.Start();

                _quickSearchString = _quickSearchString.Substring(0, _quickSearchString.Length - 1);

                var oldIndex = 0;
                if (Revisions.SelectedRows.Count > 0)
                    oldIndex = Revisions.SelectedRows[0].Index;

                FindNextMatch(oldIndex, _quickSearchString, false);
                _lastQuickSearchString = _quickSearchString;

                e.Handled = true;
                ShowQuickSearchString();
            }
            else
                if (!e.Alt && !e.Control && (char.IsLetterOrDigit((char)key) || char.IsNumber((char)key) || char.IsSeparator((char)key) || key == 191))
                {
                    quickSearchTimer.Stop();
                    quickSearchTimer.Interval = Settings.RevisionGridQuickSearchTimeout;
                    quickSearchTimer.Start();

                    //The code below is ment to fix the wierd keyvalues when pressing keys e.g. ".".
                    switch (key)
                    {
                        case 51:
                            if (e.Shift)
                                _quickSearchString = string.Concat(_quickSearchString, "#").ToLower();
                            else
                                _quickSearchString = string.Concat(_quickSearchString, "3").ToLower();
                            break;
                        case 188:
                            _quickSearchString = string.Concat(_quickSearchString, ",").ToLower();
                            break;
                        case 189:
                            if (e.Shift)
                                _quickSearchString = string.Concat(_quickSearchString, "_").ToLower();
                            else
                                _quickSearchString = string.Concat(_quickSearchString, "-").ToLower();
                            break;
                        case 190:
                            _quickSearchString = string.Concat(_quickSearchString, ".").ToLower();
                            break;
                        case 191:
                            _quickSearchString = string.Concat(_quickSearchString, "/").ToLower();
                            break;
                        default:
                            _quickSearchString = string.Concat(_quickSearchString, (char)e.KeyValue).ToLower();
                            break;
                    }

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

            if (!searchResult.HasValue)
                return;

            Revisions.ClearSelection();
            Revisions.Rows[searchResult.Value].Selected = true;

            Revisions.CurrentCell = Revisions.Rows[searchResult.Value].Cells[1];

        }


        private int? SearchForward(int startIndex, string searchString)
        {
            // Check for out of bounds roll over if required
            int index;
            if (startIndex < 0 || startIndex >= Revisions.RowCount)
                startIndex = 0;

            for (index = startIndex; index < Revisions.RowCount; ++index)
            {
                if (((GitRevision)Revisions.GetRowData(index)).MatchesSearchString(searchString))
                    return index;
            }

            // We didn't find it so start searching from the top
            for (index = 0; index < startIndex; ++index)
            {
                if (((GitRevision)Revisions.GetRowData(index)).MatchesSearchString(searchString))
                    return index;
            }

            return null;
        }

        private int? SearchInReverseOrder(int startIndex, string searchString)
        {
            // Check for out of bounds roll over if required
            int index;
            if (startIndex < 0 || startIndex >= Revisions.RowCount)
                startIndex = Revisions.RowCount - 1;

            for (index = startIndex; index >= 0; --index)
            {
                if (((GitRevision)Revisions.GetRowData(index)).MatchesSearchString(searchString))
                    return index;
            }

            // We didn't find it so start searching from the bottom
            for (index = Revisions.RowCount - 1; index > startIndex; --index)
            {
                if (((GitRevision)Revisions.GetRowData(index)).MatchesSearchString(searchString))
                    return index;
            }


            return null;
        }

        public void DisableContextMenu()
        {
            Revisions.ContextMenuStrip = null;
            _contextMenuEnabled = false;
        }

        public void FormatQuickFilter(string filter,
                                      bool[] parameters,
                                      out string revListArgs,
                                      out string inMemMessageFilter,
                                      out string inMemCommitterFilter,
                                      out string inMemAuthorFilter)
        {
            revListArgs = string.Empty;
            inMemMessageFilter = string.Empty;
            inMemCommitterFilter = string.Empty;
            inMemAuthorFilter = string.Empty;
            if (!string.IsNullOrEmpty(filter))
            {
                var cmdLineSafe = GitCommandHelpers.VersionInUse.IsRegExStringCmdPassable(filter);
                revListArgs = " --regexp-ignore-case ";
                if (parameters[0])
                    if (cmdLineSafe)
                        revListArgs += "--grep=\"" + filter + "\" ";
                    else
                        inMemMessageFilter = filter;
                if (parameters[1])
                    if (cmdLineSafe)
                        revListArgs += "--committer=\"" + filter + "\" ";
                    else
                        inMemCommitterFilter = filter;
                if (parameters[2])
                    if (cmdLineSafe)
                        revListArgs += "--author=\"" + filter + "\" ";
                    else
                        inMemAuthorFilter = filter;
                if (parameters[3])
                    if (cmdLineSafe)
                        revListArgs += "\"-S" + filter + "\" ";
                    else
                        throw new InvalidOperationException("Filter text not valid for \"Diff contains\" filter.");
            }
        }

        public bool SetAndApplyBranchFilter(string filter)
        {
            if (filter.Equals(_revisionFilter.GetBranchFilter())) return false;
            if (filter.Equals(""))
            {
                Settings.BranchFilterEnabled = false;
                Settings.ShowCurrentBranchOnly = true;
            }
            else
            {
                Settings.BranchFilterEnabled = true;
                Settings.ShowCurrentBranchOnly = false;
                _revisionFilter.SetBranchFilter(filter);
            }
            SetShowBranches();
            return true;
        }

        public override void Refresh()
        {
            base.Refresh();

            Revisions.Refresh();
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
            Revisions.CurrentCell = Revisions.Rows[index].Cells[1];

            Revisions.Select();
        }

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

        public GitRevision GetRevision(int aRow)
        {
            return Revisions.GetRowData(aRow) as GitRevision;
        }

        public GitRevision GetCurrentRevision()
        {
            string formatString =
                /* Tree           */ "%T%n" +
                /* Author Name    */ "%aN%n" +
                /* Author Date    */ "%ai%n" +
                /* Committer Name */ "%cN%n" +
                /* Committer Date */ "%ci%n" +
                /* Commit Message */ "%s";
            string cmd = "log -n 1 --pretty=format:" + formatString + " " + CurrentCheckout;
            var RevInfo = GitCommandHelpers.RunCmd(Settings.GitCommand, cmd);
            string[] Infos = RevInfo.Split('\n');
            GitRevision Revision = new GitRevision
            {
                Guid = CurrentCheckout,
                TreeGuid = Infos[0],
                Author = Infos[1],
                Committer = Infos[3],
                Message = Infos[5]
            };
            DateTime Date;
            DateTime.TryParse(Infos[2], out Date);
            Revision.AuthorDate = Date;
            DateTime.TryParse(Infos[4], out Date);
            Revision.CommitDate = Date;
            List<GitHead> heads = GitCommandHelpers.GetHeads(true, true);
            foreach (GitHead head in heads)
            {
                if (head.Guid.Equals(Revision.Guid))
                    Revision.Heads.Add(head);
            }
            return Revision;
        }

        public void RefreshRevisions()
        {
            if (_indexWatcher.IndexChanged)
                ForceRefreshRevisions();
        }

        private class RevisionGridInMemFilter : RevisionGraphInMemFilter
        {
            private bool _IgnoreCase;
            private string _AuthorFilter;
            private Regex _AuthorFilterRegex;
            private string _CommitterFilter;
            private Regex _CommitterFilterRegex;
            private string _MessageFilter;
            private Regex _MessageFilterRegex;

            public RevisionGridInMemFilter(string authorFilter, string committerFilter, string messageFilter, bool ignoreCase)
            {
                _IgnoreCase = ignoreCase;
                SetUpVars(authorFilter, ref _AuthorFilter, ref _AuthorFilterRegex);
                SetUpVars(committerFilter, ref _CommitterFilter, ref _CommitterFilterRegex);
                SetUpVars(messageFilter, ref _MessageFilter, ref _MessageFilterRegex);
            }

            private void SetUpVars(string filterValue,
                                   ref string filterStr,
                                   ref Regex filterRegEx)
            {
                RegexOptions opts = RegexOptions.None;
                if (_IgnoreCase) opts = opts | RegexOptions.IgnoreCase;
                filterStr = filterValue != null ? filterValue.Trim() : string.Empty;
                try
                {
                    filterRegEx = new Regex(filterStr, opts);
                }
                catch (ArgumentException)
                {
                    filterRegEx = null;
                }
            }

            private bool CheckCondition(string filter, Regex regex, string value)
            {
                return string.IsNullOrEmpty(filter) ||
                       ((regex != null) && regex.Match(value).Success);
            }

            public override bool PassThru(GitRevision rev)
            {
                return CheckCondition(_AuthorFilter, _AuthorFilterRegex, rev.Author) &&
                       CheckCondition(_CommitterFilter, _CommitterFilterRegex, rev.Committer) &&
                       CheckCondition(_MessageFilter, _MessageFilterRegex, rev.Message);
            }
        }

        public void ForceRefreshRevisions()
        {
            try
            {
                _initialLoad = true;

                LastScrollPos = Revisions.FirstDisplayedScrollingRowIndex;

                //Hide graph column when there it is disabled OR when a filter is active
                //allowing for special case when history of a single file is being displayed
                if (!Settings.ShowRevisionGraph || (FilterIsApplied(false) && !AllowGraphWithFilter))
                {
                    Revisions.ShowHideRevisionGraph(false);
                }
                else
                {
                    Revisions.ShowHideRevisionGraph(true);
                }


                if (_revisionGraphCommand != null)
                {
                    _revisionGraphCommand.Dispose();
                    _revisionGraphCommand = null;
                }

                var newCurrentCheckout = GitCommandHelpers.GetCurrentCheckout();

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
                _revisionGraphCommand = new RevisionGraph { BranchFilter = BranchFilter, LogParam = LogParam + Filter };
                _revisionGraphCommand.Updated += GitGetCommitsCommandUpdated;
                _revisionGraphCommand.Exited += GitGetCommitsCommandExited;
                _revisionGraphCommand.Error += _revisionGraphCommand_Error;
                if (!(string.IsNullOrEmpty(InMemAuthorFilter) &&
                      string.IsNullOrEmpty(InMemCommitterFilter) &&
                      string.IsNullOrEmpty(InMemMessageFilter)))
                    _revisionGraphCommand.InMemFilter = new RevisionGridInMemFilter(InMemAuthorFilter,
                                                                                    InMemCommitterFilter,
                                                                                    InMemMessageFilter,
                                                                                    InMemFilterIgnoreCase);

                _revisionGraphCommand.Execute();

                LoadRevisions();
            }
            catch (Exception exception)
            {
                Error.Visible = true;
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void _revisionGraphCommand_Error(object sender, EventArgs e)
        {
            // This has to happen on the UI thread
            _syncContext.Send(o =>
                                  {
                                      Error.Visible = true;
                                      NoGit.Visible = false;
                                      NoCommits.Visible = false;
                                      Revisions.Visible = false;
                                      Loading.Visible = false;
                                  }, this);
        }

        private void GitGetCommitsCommandUpdated(object sender, EventArgs e)
        {
            var updatedEvent = (RevisionGraph.RevisionGraphUpdatedEventArgs)e;
            UpdateGraph(updatedEvent.Revision);
        }

        private bool FilterIsApplied(bool inclBranchFilter)
        {
            return (inclBranchFilter && !string.IsNullOrEmpty(BranchFilter)) ||
                   !(string.IsNullOrEmpty(Filter) &&
                     string.IsNullOrEmpty(InMemAuthorFilter) &&
                     string.IsNullOrEmpty(InMemCommitterFilter) &&
                     string.IsNullOrEmpty(InMemMessageFilter));
        }

        private void GitGetCommitsCommandExited(object sender, EventArgs e)
        {
            _isLoading = false;

            if (_revisionGraphCommand.Revisions.Count == 0 &&
                !FilterIsApplied(true))
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
                                          UpdateGraph(null);
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
            else if (_initialSelectedRevision == null)
            {
                Revisions.SelectedIds = new IComparable[] { CurrentCheckout };
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
            // If our loading state has changed since the last paint, update it.
            if (Loading.Visible != _isLoading)
            {
                Loading.Visible = _isLoading;
            }

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

            var frm = new FormTagSmall { Revision = GetRevision(LastRow) };
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
            OnActionOnRepositoryPerformed();
        }

        private void CreateNewBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            var frm = new FormBranchSmall { Revision = GetRevision(LastRow) };
            frm.ShowDialog();
            RefreshRevisions();
            OnActionOnRepositoryPerformed();
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

        private void GitIgnoreClick(object sender, EventArgs e)
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
            InMemAuthorFilter = _revisionFilter.GetInMemAuthorFilter();
            InMemCommitterFilter = _revisionFilter.GetInMemCommitterFilter();
            InMemMessageFilter = _revisionFilter.GetInMemMessageFilter();
            InMemFilterIgnoreCase = _revisionFilter.GetIgnoreCase();
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

            var inTheMiddleOfBisect = GitCommandHelpers.InTheMiddleOfBisect();
            markRevisionAsBadToolStripMenuItem.Visible = inTheMiddleOfBisect;
            markRevisionAsGoodToolStripMenuItem.Visible = inTheMiddleOfBisect;
            stopBisectToolStripMenuItem.Visible = inTheMiddleOfBisect;
            bisectSeparator.Visible = inTheMiddleOfBisect;

            var revision = GetRevision(LastRow);

            var tagDropDown = new ToolStripDropDown();
            var branchDropDown = new ToolStripDropDown();
            var checkoutBranchDropDown = new ToolStripDropDown();
            var mergeBranchDropDown = new ToolStripDropDown();
            var rebaseDropDown = new ToolStripDropDown();

            var tagNameCopy = new ToolStripDropDown();
            var branchNameCopy = new ToolStripDropDown();

            foreach (var head in revision.Heads)
            {
                if (head.IsTag)
                {
                    ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                    ToolStripItem tagName = new ToolStripMenuItem(head.Name);
                    toolStripItem.Click += ToolStripItemClick;
                    tagDropDown.Items.Add(toolStripItem);
                    tagName.Click += copyToClipBoard;
                    tagNameCopy.Items.Add(tagName);
                }
                else if (head.IsHead || head.IsRemote)
                {
                    ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Click += ToolStripItemClickMergeBranch;
                    mergeBranchDropDown.Items.Add(toolStripItem);

                    toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Click += ToolStripItemClickRebaseBranch;
                    rebaseDropDown.Items.Add(toolStripItem);

                    ToolStripItem branchName = new ToolStripMenuItem(head.Name);
                    branchName.Click += copyToClipBoard;
                    branchNameCopy.Items.Add(branchName);

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

            branchNameToolStripMenuItem.DropDown = branchNameCopy;
            branchNameToolStripMenuItem.Visible = branchNameCopy.Items.Count > 0;

            tagToolStripMenuItem.DropDown = tagNameCopy;
            tagToolStripMenuItem.Visible = tagNameCopy.Items.Count > 0;

            toolStripSeparator6.Visible = tagNameCopy.Items.Count > 0 || branchNameCopy.Items.Count > 0;

            RefreshOwnScripts();
        }

        private void ToolStripItemClick(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            new FormProcess(GitCommandHelpers.DeleteTagCmd(toolStripItem.Text)).ShowDialog();
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
            OnActionOnRepositoryPerformed();
        }

        private void ToolStripItemClickMergeBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            GitUICommands.Instance.StartMergeBranchDialog(toolStripItem.Text);

            ForceRefreshRevisions();
            OnActionOnRepositoryPerformed();
        }

        private void ToolStripItemClickRebaseBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            GitUICommands.Instance.StartRebaseDialog(toolStripItem.Text);

            ForceRefreshRevisions();
            OnActionOnRepositoryPerformed();
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
            OnActionOnRepositoryPerformed();
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
            {
                RefreshRevisions();
                OnActionOnRepositoryPerformed();
            }
        }

        private void CherryPickCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            var frm = new FormCherryPickCommitSmall(GetRevision(LastRow));
            frm.ShowDialog();
            RefreshRevisions();
            OnActionOnRepositoryPerformed();
        }

        private void ShowRelativeDateToolStripMenuItemClick(object sender, EventArgs e)
        {
            Settings.RelativeDate = !showRelativeDateToolStripMenuItem.Checked;
            showRelativeDateToolStripMenuItem.Checked = Settings.RelativeDate;
            ForceRefreshRevisions();
        }

        private string TimeToString(DateTime time)
        {
            if (time == DateTime.MinValue || time == DateTime.MaxValue)
                return "";

            if (!Settings.RelativeDate)
                return string.Format("{0} {1}", time.ToShortDateString(), time.ToLongTimeString());

            var span = DateTime.Now - time;

            if (span.Minutes < 0)
                return string.Format(_secondsAgo.Text, span.Seconds);

            if (span.TotalHours < 1)
                return string.Format(_minutesAgo.Text, span.Minutes + Math.Round(span.Seconds / 60.0, 0));

            if (span.TotalHours < 2)
                return string.Format(_hourAgo.Text, span.Minutes + Math.Round(span.Seconds / 60.0, 0));

            if (span.TotalHours < 24)
                return string.Format(_hoursAgo.Text, (int)span.TotalHours + Math.Round(span.Minutes / 60.0, 0));

            if (span.TotalDays + Math.Round(span.Hours / 24.0, 0) < 2)
                return string.Format(_dayAgo.Text, (int)span.TotalDays + Math.Round(span.Hours / 24.0, 0));

            if (span.TotalDays < 30)
                return string.Format(_daysAgo.Text, (int)span.TotalDays + Math.Round(span.Hours / 24.0, 0));

            if (span.TotalDays < 45)
                return string.Format(_monthAgo.Text, "1");

            if (span.TotalDays < 365)
                return string.Format(_monthsAgo.Text, (int)Math.Round(span.TotalDays / 30, 0));

            return string.Format(_yearsAgo.Text, string.Format("{0:#.#} ", Math.Round(span.TotalDays / 365)));
        }

        private void UpdateGraph(GitRevision rev)
        {
            if (rev == null)
            {
                // Prune the graph and make sure the row count matches reality
                Revisions.Prune();

                if (Revisions.RowCount == 0 && Settings.RevisionGraphShowWorkingDirChanges)
                {
                    bool uncommittedChanges = false;
                    bool stagedChanges = false;
                    //Only check for tracked files. This usually makes more sense and it performs a lot
                    //better then checking for untrackd files.
                    if (GitCommandHelpers.GetTrackedChangedFiles().Count > 0)
                        uncommittedChanges = true;
                    if (GitCommandHelpers.GetStagedFiles().Count > 0)
                        stagedChanges = true;

                    if (uncommittedChanges)
                    {
                        //Add working dir as virtual commit
                        GitRevision workingDir = new GitRevision();
                        workingDir.Guid = GitRevision.UncommittedWorkingDirGuid;
                        workingDir.Message = _currentWorkingDirChanges.Text;
                        workingDir.ParentGuids = stagedChanges ? new string[] { GitRevision.IndexGuid } : new string[] { CurrentCheckout };
                        Revisions.Add(workingDir.Guid, workingDir.ParentGuids, DvcsGraph.DataType.Normal, workingDir);
                    }

                    if (stagedChanges)
                    {
                        //Add index as virtual commit
                        GitRevision index = new GitRevision();
                        index.Guid = GitRevision.IndexGuid;
                        index.Message = _currentIndex.Text;
                        index.ParentGuids = new string[] { CurrentCheckout };
                        Revisions.Add(index.Guid, index.ParentGuids, DvcsGraph.DataType.Normal, index);
                    }
                }
                return;
            }

            var dataType = DvcsGraph.DataType.Normal;
            if (rev.Guid == CurrentCheckout)
                dataType = DvcsGraph.DataType.Active;
            else if (rev.Heads.Count > 0)
                dataType = DvcsGraph.DataType.Special;

            Revisions.Add(rev.Guid, rev.ParentGuids, dataType, rev);
        }

        private void drawNonrelativesGrayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.RevisionGraphDrawNonRelativesGray = !Settings.RevisionGraphDrawNonRelativesGray;
            drawNonrelativesGrayToolStripMenuItem.Checked = Settings.RevisionGraphDrawNonRelativesGray;
            Revisions.Refresh();
        }

        private void messageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(GetRevision(LastRow).Message);
        }

        private void authorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(GetRevision(LastRow).Author);
        }

        private void dateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(GetRevision(LastRow).CommitDate.ToString());
        }

        private void copyToClipBoard(object sender, EventArgs e)
        {
            Clipboard.SetText(sender.ToString());
        }

        private void markRevisionAsBadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            Settings.CloseProcessDialog = false;
            new FormProcess(GitCommandHelpers.MarkRevisionBisectCmd(false, GetRevision(LastRow).Guid)).ShowDialog();
            RefreshRevisions();
        }

        private void markRevisionAsGoodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            Settings.CloseProcessDialog = false;
            new FormProcess(GitCommandHelpers.MarkRevisionBisectCmd(true, GetRevision(LastRow).Guid)).ShowDialog();
            RefreshRevisions();
        }

        private void stopBisectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormProcess(GitCommandHelpers.StopBisectCmd()).ShowDialog();
            RefreshRevisions();
        }

        private void RefreshOwnScripts()
        {
            RemoveOwnScripts();
            AddOwnScripts();
        }

        private void AddOwnScripts()
        {
            string[][] scripts = Settings.GetScripts();
            foreach (string[] parameters in scripts)
            {
                ToolStripItem item = new ToolStripMenuItem(parameters[0]);
                item.Name = item.Text + "_ownScript";
                item.Click += runScript;
                if (parameters[3].Equals("yes"))
                    CreateTag.Items.Add(item);
                else
                    runScriptToolStripMenuItem.DropDown.Items.Add(item);
            }
            toolStripSeparator7.Visible = scripts.Length > 1;
            runScriptToolStripMenuItem.Visible = runScriptToolStripMenuItem.DropDown.Items.Count > 0;
        }

        private void RemoveOwnScripts()
        {
            runScriptToolStripMenuItem.DropDown.Items.Clear();
            List<ToolStripItem> list = new List<ToolStripItem>();
            foreach (ToolStripItem item in CreateTag.Items)
                list.Add(item);
            foreach (ToolStripItem item in list)
                if (item.Name.Contains("_ownScript"))
                    CreateTag.Items.RemoveByKey(item.Name);
        }

        private bool settingsLoaded = false;

        private void runScript(object sender, EventArgs e)
        {
            if (settingsLoaded == false)
            {
                new FormSettings().LoadSettings();
                settingsLoaded = true;
            }
            new RunScript(sender.ToString(), this);
            RefreshRevisions();
        }
    }
}