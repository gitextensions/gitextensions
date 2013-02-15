using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUI.Hotkey;
using GitUI.Script;
using GitUI.Tag;
using Gravatar;
using ResourceManager.Translation;

namespace GitUI
{
    public enum RevisionGridLayout
    {
        FilledBranchesSmall = 1,
        FilledBranchesSmallWithGraph = 2,
        Small = 3,
        SmallWithGraph = 4,
        Card = 5,
        CardWithGraph = 6,
        LargeCard = 7,
        LargeCardWithGraph = 8
    }

    [DefaultEvent("DoubleClick")]
    public sealed partial class RevisionGrid : GitModuleControl
    {
        private readonly TranslationString _areYouSureYouWantCheckout = new TranslationString("Are you sure to checkout the selected revision?");
        private readonly TranslationString _areYouSureYouWantCheckoutCaption = new TranslationString("Checkout revision");
        private readonly TranslationString _droppingFilesBlocked = new TranslationString("For you own protection dropping more than 10 patch files at once is blocked!");

        private const int NODE_DIMENSION = 8;
        private const int LANE_WIDTH = 13;
        private const int LANE_LINE_WIDTH = 2;
        private Brush selectedItemBrush;

        private readonly FormRevisionFilter _revisionFilter = new FormRevisionFilter();

        private readonly SynchronizationContext _syncContext;
        public string LogParam = "--all --boundary";

        private bool _initialLoad = true;
        private string _initialSelectedRevision;
        private string _lastQuickSearchString = string.Empty;
        private Label _quickSearchLabel;
        private string _quickSearchString;
        private RevisionGraph _revisionGraphCommand;

        private RevisionGridLayout layout;
        private int rowHeigth;
        public event GitModuleChangedEventHandler GitModuleChanged;

        public RevisionGrid()
        {
            _syncContext = SynchronizationContext.Current;

            InitLayout();
            InitializeComponent();
            this.Loading.Image = global::GitUI.Properties.Resources.loadingpanel;

            Translate();

            NormalFont = Settings.Font;
            Loading.Paint += Loading_Paint;

            Revisions.CellPainting += RevisionsCellPainting;
            Revisions.CellFormatting += RevisionsCellFormatting;
            Revisions.KeyPress += RevisionsKeyPress;

            showAuthorDateToolStripMenuItem.Checked = Settings.ShowAuthorDate;
            orderRevisionsByDateToolStripMenuItem.Checked = Settings.OrderRevisionByDate;
            showRelativeDateToolStripMenuItem.Checked = Settings.RelativeDate;
            drawNonrelativesGrayToolStripMenuItem.Checked = Settings.RevisionGraphDrawNonRelativesGray;
            showGitNotesToolStripMenuItem.Checked = Settings.ShowGitNotes;

            BranchFilter = String.Empty;
            SetShowBranches();
            Filter = "";
            FixedFilter = "";
            InMemFilterIgnoreCase = false;
            InMemAuthorFilter = "";
            InMemCommitterFilter = "";
            InMemMessageFilter = "";
            AllowGraphWithFilter = false;
            _quickSearchString = "";
            quickSearchTimer.Tick += QuickSearchTimerTick;

            Revisions.Loading += RevisionsLoading;

            //Allow to drop patch file on revisiongrid
            Revisions.DragEnter += Revisions_DragEnter;
            Revisions.DragDrop += Revisions_DragDrop;
            Revisions.AllowDrop = true;
            Revisions.ColumnHeadersVisible = false;

            this.HotkeysEnabled = true;
            try
            {
                SetRevisionsLayout((RevisionGridLayout)Settings.RevisionGraphLayout);
            }
            catch
            {
                SetRevisionsLayout(RevisionGridLayout.SmallWithGraph);
            }
        }

        void Loading_Paint(object sender, PaintEventArgs e)
        {
            // If our loading state has changed since the last paint, update it.
            if (Loading != null)
            {
                if (Loading.Visible != _isLoading)
                {
                    Loading.Visible = _isLoading;
                }
            }
        }

        [Browsable(false)]
        public Font HeadFont { get; private set; }
        [Browsable(false)]
        public Font SuperprojectFont { get; private set; }
        [Browsable(false)]
        public int LastScrollPos { get; private set; }
        [Browsable(false)]
        public string[] LastSelectedRows { get; private set; }
        [Browsable(false)]
        public Font RefsFont { get; private set; }
        private Font _normalFont;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Font NormalFont
        {
            get { return _normalFont; }
            set
            {
                _normalFont = value;
                Message.DefaultCellStyle.Font = _normalFont;
                Date.DefaultCellStyle.Font = _normalFont;

                RefsFont = IsFilledBranchesLayout() ? _normalFont : new Font(_normalFont, FontStyle.Bold);
                HeadFont = new Font(_normalFont, FontStyle.Bold);
                SuperprojectFont = new Font(_normalFont, FontStyle.Underline);
            }
        }

        [Category("Filter")]
        [DefaultValue("")]
        public string Filter { get; set; }
        [Category("Filter")]
        [DefaultValue("")]
        public string FixedFilter { get; set; }
        [Category("Filter")]
        [DefaultValue(false)]
        public bool InMemFilterIgnoreCase { get; set; }
        [Category("Filter")]
        [DefaultValue("")]
        public string InMemAuthorFilter { get; set; }
        [Category("Filter")]
        [DefaultValue("")]
        public string InMemCommitterFilter { get; set; }
        [Category("Filter")]
        [DefaultValue("")]
        public string InMemMessageFilter { get; set; }
        [Category("Filter")]
        [DefaultValue("")]
        public string BranchFilter { get; set; }
        [Category("Filter")]
        [DefaultValue(false)]
        public bool AllowGraphWithFilter { get; set; }

        [Browsable(false)]
        public string CurrentCheckout { get; private set; }
        [Browsable(false)]
        public string FiltredFileName { get; set; }
        [Browsable(false)]
        private string FiltredCurrentCheckout { get; set; }
        [Browsable(false)]
        public string SuperprojectCurrentCheckout { get; private set; }
        [Browsable(false)]
        public int LastRow { get; private set; }

        [Description("Indicates whether the user is allowed to select more than one commit at a time.")]
        [Category("Behavior")]
        [DefaultValue(true)]
        public bool MultiSelect
        {
            get { return Revisions.MultiSelect; }
            set { Revisions.MultiSelect = value; }
        }

        private IndexWatcher _IndexWatcher;
        [Browsable(false)]
        public IndexWatcher IndexWatcher
        {
            get
            {
                if (_IndexWatcher == null)
                    _IndexWatcher = new IndexWatcher(UICommandsSource);

                return _IndexWatcher;
            }
        }


        public void SetInitialRevision(GitRevision initialSelectedRevision)
        {
            _initialSelectedRevision = initialSelectedRevision != null ? initialSelectedRevision.Guid : null;
        }

        public event EventHandler ActionOnRepositoryPerformed;

        private void OnActionOnRepositoryPerformed()
        {
            if (ActionOnRepositoryPerformed != null)
                ActionOnRepositoryPerformed(this, null);
        }

        private bool _isLoading;
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

        private void RestartQuickSearchTimer()
        {
            quickSearchTimer.Stop();
            quickSearchTimer.Interval = Settings.RevisionGridQuickSearchTimeout;
            quickSearchTimer.Start();
        }

        private void RevisionsKeyPress(object sender, KeyPressEventArgs e)
        {
            var curIndex = -1;
            if (Revisions.SelectedRows.Count > 0)
                curIndex = Revisions.SelectedRows[0].Index;

            curIndex = curIndex >= 0 ? curIndex : 0;
            if (e.KeyChar == 8 && _quickSearchString.Length > 1) //backspace
            {
                RestartQuickSearchTimer();

                _quickSearchString = _quickSearchString.Substring(0, _quickSearchString.Length - 1);

                FindNextMatch(curIndex, _quickSearchString, false);
                _lastQuickSearchString = _quickSearchString;

                e.Handled = true;
                ShowQuickSearchString();
            }
            else if (!char.IsControl(e.KeyChar))
            {
                RestartQuickSearchTimer();

                //The code below is meant to fix the weird keyvalues when pressing keys e.g. ".".
                _quickSearchString = string.Concat(_quickSearchString, char.ToLower(e.KeyChar));

                FindNextMatch(curIndex, _quickSearchString, false);
                _lastQuickSearchString = _quickSearchString;

                e.Handled = true;
                ShowQuickSearchString();
            }
            else
            {
                _quickSearchString = "";
                HideQuickSearchString();
                e.Handled = false;
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
                if (GetRevision(index).MatchesSearchString(searchString))
                    return index;
            }

            // We didn't find it so start searching from the top
            for (index = 0; index < startIndex; ++index)
            {
                if (GetRevision(index).MatchesSearchString(searchString))
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
                if (GetRevision(index).MatchesSearchString(searchString))
                    return index;
            }

            // We didn't find it so start searching from the bottom
            for (index = Revisions.RowCount - 1; index > startIndex; --index)
            {
                if (GetRevision(index).MatchesSearchString(searchString))
                    return index;
            }


            return null;
        }

        public void DisableContextMenu()
        {
            Revisions.ContextMenuStrip = null;
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
                // hash filtering only possible in memory
                var cmdLineSafe = GitCommandHelpers.VersionInUse.IsRegExStringCmdPassable(filter);
                revListArgs = " --regexp-ignore-case ";
                if (parameters[0])
                    if (cmdLineSafe && !MessageFilterCouldBeSHA(filter))
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
            if (filter.Equals(_revisionFilter.GetBranchFilter()))
                return false;
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

        public void SetLimit(int limit)
        {
            _revisionFilter.SetLimit(limit);
        }

        public override void Refresh()
        {
            SetRevisionsLayout();

            base.Refresh();

            Revisions.Refresh();
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            _isLoading = true;
            Error.Visible = false;
            NoCommits.Visible = false;
            NoGit.Visible = false;
            Revisions.Visible = false;
            Loading.Visible = true;
            Loading.BringToFront();
        }

        public new void Load()
        {
            if (!DesignMode)
                ReloadHotkeys();
            ForceRefreshRevisions();
        }

        public event EventHandler SelectionChanged;

        public void SetSelectedIndex(int index)
        {
            if (Revisions.Rows[index].Selected)
                return;

            Revisions.ClearSelection();

            Revisions.Rows[index].Selected = true;
            Revisions.CurrentCell = Revisions.Rows[index].Cells[1];

            Revisions.Select();
        }

        public void SetSelectedRevision(GitRevision revision)
        {
            if (revision != null)
            {
                for (var i = 0; i < Revisions.RowCount; i++)
                {
                    if (GetRevision(i).Guid == revision.Guid)
                    {
                        SetSelectedIndex(i);
                        return;
                    }
                }
            }

            Revisions.ClearSelection();
            Revisions.Select();
        }

        public void HighlightBranch(string aId)
        {
            Revisions.HighlightBranch(aId);
        }

        private void RevisionsSelectionChanged(object sender, EventArgs e)
        {
            if (Revisions.SelectedRows.Count > 0)
                LastRow = Revisions.SelectedRows[0].Index;

            SelectionTimer.Enabled = false;
            SelectionTimer.Stop();
            SelectionTimer.Enabled = true;
            SelectionTimer.Start();
        }

        public DvcsGraph.RevisionGraphDrawStyleEnum RevisionGraphDrawStyle
        {
            get
            {
                return Revisions.RevisionGraphDrawStyle;
            }
            set
            {
                Revisions.RevisionGraphDrawStyle = value;
            }
        }

        public List<GitRevision> GetSelectedRevisions()
        {
            return GetSelectedRevisions(null);
        }

        public List<GitRevision> GetSelectedRevisions(SortDirection? direction)
        {
            var rows = Revisions
                .SelectedRows
                .Cast<DataGridViewRow>()
                .Where(row => Revisions.RowCount > row.Index);


            if (direction.HasValue)
            {
                int d = direction.Value == SortDirection.Ascending ? 1 : -1;
                rows = rows.OrderBy((row) => row.Index, (r1, r2) => d * (r1 - r2));
            }

            return rows
                .Select(row => GetRevision(row.Index))
                .ToList();
        }

        public List<string> GetRevisionChildren(string revision)
        {
            var rows = Revisions
                .Rows
                .Cast<DataGridViewRow>()
                .Where(row => Revisions.RowCount > row.Index);

            return rows
                .Select(row => GetRevision(row.Index))
                .Where(row => row.ParentGuids.Contains(revision))
                .Select(row => row.Guid)
                .ToList();
        }

        public GitRevision GetRevision(int aRow)
        {
            return Revisions.GetRowData(aRow);
        }

        public GitRevision GetCurrentRevision()
        {
            const string formatString =
                /* Tree           */ "%T%n" +
                /* Author Name    */ "%aN%n" +
                /* Author Date    */ "%ai%n" +
                /* Committer Name */ "%cN%n" +
                /* Committer Date */ "%ci%n" +
                /* Commit Message */ "%s";
            string cmd = "log -n 1 --pretty=format:" + formatString + " " + CurrentCheckout;
            var RevInfo = Module.RunGitCmd(cmd);
            string[] Infos = RevInfo.Split('\n');
            var Revision = new GitRevision(Module, CurrentCheckout)
            {
                TreeGuid = Infos[0],
                Author = Infos[1],
                Committer = Infos[3],
                Message = Infos[5]
            };
            DateTime date;
            DateTime.TryParse(Infos[2], out date);
            Revision.AuthorDate = date;
            DateTime.TryParse(Infos[4], out date);
            Revision.CommitDate = date;
            var heads = Module.GetHeads(true, true);
            foreach (var head in heads)
            {
                if (head.Guid.Equals(Revision.Guid))
                {
                    Revision.Heads.Add(head);
                }
            }
            return Revision;
        }

        public void RefreshRevisions()
        {
            if (IndexWatcher.IndexChanged)
                ForceRefreshRevisions();
        }

        private class RevisionGraphInMemFilterOr : RevisionGraphInMemFilter
        {
            private RevisionGraphInMemFilter fFilter1;
            private RevisionGraphInMemFilter fFilter2;
            public RevisionGraphInMemFilterOr(RevisionGraphInMemFilter aFilter1,
                                              RevisionGraphInMemFilter aFilter2)
            {
                fFilter1 = aFilter1;
                fFilter2 = aFilter2;
            }

            public override bool PassThru(GitRevision rev)
            {
                return fFilter1.PassThru(rev) || fFilter2.PassThru(rev);
            }
        }

        private class RevisionGridInMemFilter : RevisionGraphInMemFilter
        {
            private readonly string _AuthorFilter;
            private readonly Regex _AuthorFilterRegex;
            private readonly string _CommitterFilter;
            private readonly Regex _CommitterFilterRegex;
            private readonly string _MessageFilter;
            private readonly Regex _MessageFilterRegex;
            private readonly string _ShaFilter;
            private readonly Regex _ShaFilterRegex;

            public RevisionGridInMemFilter(string authorFilter, string committerFilter, string messageFilter, bool ignoreCase)
            {
                SetUpVars(authorFilter, ref _AuthorFilter, ref _AuthorFilterRegex, ignoreCase);
                SetUpVars(committerFilter, ref _CommitterFilter, ref _CommitterFilterRegex, ignoreCase);
                SetUpVars(messageFilter, ref _MessageFilter, ref _MessageFilterRegex, ignoreCase);
                if (!string.IsNullOrEmpty(_MessageFilter) && MessageFilterCouldBeSHA(_MessageFilter))
                {
                    SetUpVars(messageFilter, ref _ShaFilter, ref _ShaFilterRegex, false);
                }
            }

            private static void SetUpVars(string filterValue,
                                   ref string filterStr,
                                   ref Regex filterRegEx,
                                   bool ignoreCase)
            {
                RegexOptions opts = RegexOptions.None;
                if (ignoreCase) opts = opts | RegexOptions.IgnoreCase;
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

            private static bool CheckCondition(string filter, Regex regex, string value)
            {
                return string.IsNullOrEmpty(filter) ||
                       ((regex != null) && regex.Match(value).Success);
            }

            public override bool PassThru(GitRevision rev)
            {
                return CheckCondition(_AuthorFilter, _AuthorFilterRegex, rev.Author) &&
                       CheckCondition(_CommitterFilter, _CommitterFilterRegex, rev.Committer) &&
                       (CheckCondition(_MessageFilter, _MessageFilterRegex, rev.Message) ||
                        CheckCondition(_ShaFilter, _ShaFilterRegex, rev.Guid));
            }

            public static RevisionGridInMemFilter CreateIfNeeded(string authorFilter,
                                                                 string committerFilter,
                                                                 string messageFilter,
                                                                 bool ignoreCase)
            {
                if (!(string.IsNullOrEmpty(authorFilter) &&
                      string.IsNullOrEmpty(committerFilter) &&
                      string.IsNullOrEmpty(messageFilter) &&
                      !MessageFilterCouldBeSHA(messageFilter)))
                    return new RevisionGridInMemFilter(authorFilter,
                                                       committerFilter,
                                                       messageFilter,
                                                       ignoreCase);
                else
                    return null;
            }
        }

        public void ReloadHotkeys()
        {
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
        }

        public void ReloadTranslation()
        {
            Translate();
        }

        public void ForceRefreshRevisions()
        {
            try
            {
                RevisionGraphDrawStyle = DvcsGraph.RevisionGraphDrawStyleEnum.DrawNonRelativesGray;

                ApplyFilterFromRevisionFilterDialog();

                _initialLoad = true;

                LastScrollPos = Revisions.FirstDisplayedScrollingRowIndex;

                DisposeRevisionGraphCommand();

                var newCurrentCheckout = Module.GetCurrentCheckout();
                var newSuperprojectCurrentCheckout = Module.GetSuperprojectCurrentCheckout();

                // If the current checkout changed, don't get the currently selected rows, select the
                // new current checkout instead.
                if (newCurrentCheckout == CurrentCheckout)
                {
                    LastSelectedRows = Revisions.SelectedIds;
                }
                else
                {
                    // This is a new checkout, so ensure the variable is cleared out.
                    LastSelectedRows = null;
                }

                Revisions.ClearSelection();
                CurrentCheckout = newCurrentCheckout;
                FiltredCurrentCheckout = CurrentCheckout;
                SuperprojectCurrentCheckout = newSuperprojectCurrentCheckout;
                Revisions.Clear();
                Error.Visible = false;

                if (!Module.ValidWorkingDir())
                {
                    Revisions.Visible = false;
                    NoCommits.Visible = true;
                    Loading.Visible = false;
                    NoGit.Visible = true;
                    string dir = Module.WorkingDir;
                    if (String.IsNullOrEmpty(dir) || !Directory.Exists(dir) ||
                        Directory.GetDirectories(dir).Length == 0 &&
                        Directory.GetFiles(dir).Length == 0)
                        CloneRepository.Show();
                    else
                        CloneRepository.Hide();
                    NoGit.BringToFront();
                    return;
                }

                NoCommits.Visible = false;
                NoGit.Visible = false;
                Revisions.Visible = true;
                Revisions.BringToFront();
                Revisions.Enabled = false;
                Loading.Visible = true;
                Loading.BringToFront();
                _isLoading = true;
                base.Refresh();

                IndexWatcher.Reset();

                if (!Settings.ShowGitNotes && !LogParam.Contains(" --not --glob=notes --not"))
                    LogParam = LogParam + " --not --glob=notes --not";

                if (Settings.ShowGitNotes && LogParam.Contains(" --not --glob=notes --not"))
                    LogParam = LogParam.Replace("  --not --glob=notes --not", string.Empty);

                RevisionGridInMemFilter revisionFilterIMF = RevisionGridInMemFilter.CreateIfNeeded(_revisionFilter.GetInMemAuthorFilter(),
                                                                                                   _revisionFilter.GetInMemCommitterFilter(),
                                                                                                   _revisionFilter.GetInMemMessageFilter(),
                                                                                                   _revisionFilter.GetIgnoreCase());
                RevisionGridInMemFilter filterBarIMF = RevisionGridInMemFilter.CreateIfNeeded(InMemAuthorFilter,
                                                                                              InMemCommitterFilter,
                                                                                              InMemMessageFilter,
                                                                                              InMemFilterIgnoreCase);
                RevisionGraphInMemFilter revGraphIMF;
                if (revisionFilterIMF != null && filterBarIMF != null)
                    revGraphIMF = new RevisionGraphInMemFilterOr(revisionFilterIMF, filterBarIMF);
                else if (revisionFilterIMF != null)
                    revGraphIMF = revisionFilterIMF;
                else
                    revGraphIMF = filterBarIMF;

                _revisionGraphCommand = new RevisionGraph(Module) { BranchFilter = BranchFilter, LogParam = LogParam + _revisionFilter.GetFilter() + Filter + FixedFilter };
                _revisionGraphCommand.Updated += GitGetCommitsCommandUpdated;
                _revisionGraphCommand.Exited += GitGetCommitsCommandExited;
                _revisionGraphCommand.Error += _revisionGraphCommand_Error;
                _revisionGraphCommand.InMemFilter = revGraphIMF;
                //_revisionGraphCommand.BeginUpdate += ((s, e) => Revisions.Invoke((Action) (() => Revisions.Clear())));
                _revisionGraphCommand.Execute();
                LoadRevisions();
                SetRevisionsLayout();
            }
            catch (Exception exception)
            {
                Error.Visible = true;
                Error.BringToFront();
                MessageBox.Show(this, exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static readonly Regex potentialShaPattern = new Regex(@"^[a-f0-9]{5,}", RegexOptions.Compiled);
        public static bool MessageFilterCouldBeSHA(string filter)
        {
            bool result = potentialShaPattern.IsMatch(filter);

            return result;
        }

        private void _revisionGraphCommand_Error(object sender, EventArgs e)
        {
            // This has to happen on the UI thread
            _syncContext.Send(o =>
                                  {
                                      Error.Visible = true;
                                      //Error.BringToFront();
                                      NoGit.Visible = false;
                                      NoCommits.Visible = false;
                                      Revisions.Visible = false;
                                      Loading.Visible = false;
                                  }, this);

            DisposeRevisionGraphCommand();
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
                     !_revisionFilter.FilterEnabled() &&
                     string.IsNullOrEmpty(InMemAuthorFilter) &&
                     string.IsNullOrEmpty(InMemCommitterFilter) &&
                     string.IsNullOrEmpty(InMemMessageFilter));
        }

        private bool ShouldHideGraph(bool inclBranchFilter)
        {
            return (inclBranchFilter && !string.IsNullOrEmpty(BranchFilter)) ||
                   !(!_revisionFilter.ShouldHideGraph() &&
                     string.IsNullOrEmpty(InMemAuthorFilter) &&
                     string.IsNullOrEmpty(InMemCommitterFilter) &&
                     string.IsNullOrEmpty(InMemMessageFilter));
        }

        private void DisposeRevisionGraphCommand()
        {
            if (_revisionGraphCommand != null)
            {
                //Dispose command, it is not needed anymore
                _revisionGraphCommand.Updated -= GitGetCommitsCommandUpdated;
                _revisionGraphCommand.Exited -= GitGetCommitsCommandExited;
                _revisionGraphCommand.Error -= _revisionGraphCommand_Error;

                _revisionGraphCommand.Dispose();
                _revisionGraphCommand = null;
            }
        }

        private void GitGetCommitsCommandExited(object sender, EventArgs e)
        {
            _isLoading = false;

            if (_revisionGraphCommand.RevisionCount == 0 &&
                !FilterIsApplied(true))
            {
                // This has to happen on the UI thread
                _syncContext.Send(o =>
                                      {
                                          NoGit.Visible = false;
                                          NoCommits.Visible = true;
                                          //NoCommits.BringToFront();
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
                                          _isLoading = false;
                                      }, this);
            }

            DisposeRevisionGraphCommand();
        }

        private void SelectInitialRevision()
        {
            string filtredCurrentCheckout;
            if (SearchRevision(CurrentCheckout, out filtredCurrentCheckout) >= 0)
                FiltredCurrentCheckout = filtredCurrentCheckout;
            else
                FiltredCurrentCheckout = CurrentCheckout;

            if (!string.IsNullOrEmpty(_initialSelectedRevision) && Revisions.SelectedRows.Count == 0)
            {
                string revision;
                int index = SearchRevision(_initialSelectedRevision, out revision);
                if (index >= 0)
                    SetSelectedIndex(index);
            }
        }

        private int SearchRevision(string initRevision, out string graphRevision)
        {
            var rows = Revisions
                .Rows
                .Cast<DataGridViewRow>();
            var revisions = rows
                .Select(row => new { Index = row.Index, Guid = GetRevision(row.Index).Guid });

            var idx = revisions.FirstOrDefault(rev => rev.Guid == initRevision);
            if (idx != null)
            {
                graphRevision = idx.Guid;
                return idx.Index;
            }

            var dict = rows
                .ToDictionary(row => GetRevision(row.Index).Guid, row => row.Index);
            var revListParams = "rev-list ";
            if (Settings.OrderRevisionByDate)
                revListParams += "--date-order ";
            else
                revListParams += "--topo-order ";
            if (Settings.MaxRevisionGraphCommits > 0)
                revListParams += string.Format("--max-count=\"{0}\" ", (int)Settings.MaxRevisionGraphCommits);

            var allrevisions = Module.RunGitCmdAsync(revListParams + initRevision);
            foreach (var rev in allrevisions)
            {
                int index;
                if (dict.TryGetValue(rev, out index))
                {
                    graphRevision = rev;
                    return index;
                }
            }
            graphRevision = null;
            return -1;
        }
        private static string GetDateHeaderText()
        {
            return Settings.ShowAuthorDate ? Strings.GetAuthorDateText() : Strings.GetCommitDateText();
        }

        private void LoadRevisions()
        {
            if (_revisionGraphCommand == null)
            {
                return;
            }

            Revisions.SuspendLayout();

            Revisions.Columns[1].HeaderText = Strings.GetMessageText();
            Revisions.Columns[2].HeaderText = Strings.GetAuthorText();
            Revisions.Columns[3].HeaderText = GetDateHeaderText();

            Revisions.SelectionChanged -= RevisionsSelectionChanged;

            if (LastSelectedRows != null)
            {
                Revisions.SelectedIds = LastSelectedRows;
                LastSelectedRows = null;
            }
            else if (_initialSelectedRevision == null)
            {
                Revisions.SelectedIds = new[] { CurrentCheckout };
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
            SelectionTimer.Enabled = false;
            SelectionTimer.Stop();
            SelectionTimer.Enabled = true;
            SelectionTimer.Start();
        }

        private void RevisionsCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // If our loading state has changed since the last paint, update it.
            if (Loading != null)
            {
                if (Loading.Visible != _isLoading)
                {
                    Loading.Visible = _isLoading;
                }
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

            bool isRowSelected = ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected);

            if (isRowSelected /*&& !showRevisionCards*/)
                e.Graphics.FillRectangle(selectedItemBrush, e.CellBounds);
            else
                e.Graphics.FillRectangle(new SolidBrush(Color.White), e.CellBounds);

            Color foreColor;

            if (!Settings.RevisionGraphDrawNonRelativesTextGray || Revisions.RowIsRelative(e.RowIndex))
            {
                foreColor = isRowSelected && IsFilledBranchesLayout()
                    ? SystemColors.HighlightText
                    : e.CellStyle.ForeColor;
            }
            else
            {
                foreColor = isRowSelected ? SystemColors.HighlightText : Color.Gray;
            }

            Brush foreBrush = new SolidBrush(foreColor);
            var rowFont = NormalFont;
            if (revision.Guid == CurrentCheckout /*&& !showRevisionCards*/)
                rowFont = HeadFont;
            else if (revision.Guid == SuperprojectCurrentCheckout)
                rowFont = SuperprojectFont;

            switch (column)
            {
                case 1: //Description!!
                    {
                        int baseOffset = 0;
                        if (IsCardLayout())
                        {
                            baseOffset = 5;

                            Rectangle cellRectangle = new Rectangle(e.CellBounds.Left + baseOffset, e.CellBounds.Top + 1, e.CellBounds.Width - (baseOffset * 2), e.CellBounds.Height - 4);

                            if (!Settings.RevisionGraphDrawNonRelativesGray || Revisions.RowIsRelative(e.RowIndex))
                            {
                                e.Graphics.FillRectangle(
                                    new LinearGradientBrush(cellRectangle,
                                                            Color.FromArgb(255, 220, 220, 231),
                                                            Color.FromArgb(255, 240, 240, 250), 90, false), cellRectangle);
                                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(255, 200, 200, 200), 1), cellRectangle);
                            }
                            else
                            {
                                e.Graphics.FillRectangle(
                                    new LinearGradientBrush(cellRectangle,
                                                            Color.FromArgb(255, 240, 240, 240),
                                                            Color.FromArgb(255, 250, 250, 250), 90, false), cellRectangle);
                            }

                            if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
                                e.Graphics.DrawRectangle(new Pen(Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor, 1), cellRectangle);
                        }

                        float offset = baseOffset;
                        var heads = revision.Heads;

                        if (heads.Count > 0)
                        {
                            heads.Sort((left, right) =>
                                           {
                                               if (left.IsTag != right.IsTag)
                                                   return right.IsTag.CompareTo(left.IsTag);
                                               if (left.IsRemote != right.IsRemote)
                                                   return left.IsRemote.CompareTo(right.IsRemote);
                                               return left.Name.CompareTo(right.Name);
                                           });

                            foreach (var head in heads)
                            {
                                if ((head.IsRemote && !ShowRemoteBranches.Checked))
                                    continue;

                                Font refsFont;

                                if (IsFilledBranchesLayout())
                                {
                                    //refsFont = head.Selected ? rowFont : new Font(rowFont, FontStyle.Regular);
                                    refsFont = rowFont;

                                    //refsFont = head.Selected
                                    //    ? new Font(rowFont, rowFont.Style | FontStyle.Italic)
                                    //    : rowFont;
                                }
                                else
                                {
                                    refsFont = RefsFont;
                                }

                                Color headColor = GetHeadColor(head);
                                Brush textBrush = new SolidBrush(headColor);

                                string headName;
                                PointF location;

                                if (IsCardLayout())
                                {
                                    headName = head.Name;
                                    offset += e.Graphics.MeasureString(headName, refsFont).Width + 6;
                                    location = new PointF(e.CellBounds.Right - offset, e.CellBounds.Top + 4);
                                    var size = new SizeF(e.Graphics.MeasureString(headName, refsFont).Width,
                                                         e.Graphics.MeasureString(headName, RefsFont).Height);
                                    e.Graphics.FillRectangle(new SolidBrush(SystemColors.Info), location.X - 1,
                                                             location.Y - 1, size.Width + 3, size.Height + 2);
                                    e.Graphics.DrawRectangle(new Pen(SystemColors.InfoText), location.X - 1,
                                                             location.Y - 1, size.Width + 3, size.Height + 2);
                                    e.Graphics.DrawString(headName, refsFont, textBrush, location);
                                }
                                else
                                {
                                    headName = IsFilledBranchesLayout()
                                                   ? head.Name
                                                   : string.Concat("[", head.Name, "] ");

                                    var headBounds = AdjustCellBounds(e.CellBounds, offset);
                                    SizeF textSize = e.Graphics.MeasureString(headName, refsFont);

                                    offset += textSize.Width;

                                    if (IsFilledBranchesLayout())
                                    {
                                        offset += 9;

                                        float extraOffset = DrawHeadBackground(isRowSelected, e.Graphics,
                                                                               headColor, headBounds.X,
                                                                               headBounds.Y,
                                                                               RoundToEven(textSize.Width + 3),
                                                                               RoundToEven(textSize.Height), 3,
                                                                               head.Selected,
                                                                               head.SelectedHeadMergeSource);

                                        offset += extraOffset;
                                        headBounds.Offset((int)(extraOffset + 1), 0);
                                    }

                                    DrawColumnText(e.Graphics, headName, refsFont, headColor, headBounds);
                                }
                            }
                        }

                        if (IsCardLayout())
                            offset = baseOffset;

                        var text = (string)e.FormattedValue;
                        var bounds = AdjustCellBounds(e.CellBounds, offset);
                        DrawColumnText(e.Graphics, text, rowFont, foreColor, bounds);

                        if (IsCardLayout())
                        {
                            int textHeight = (int)e.Graphics.MeasureString(text, rowFont).Height;
                            int gravatarSize = rowHeigth - textHeight - 12;
                            int gravatarTop = e.CellBounds.Top + textHeight + 6;
                            int gravatarLeft = e.CellBounds.Left + baseOffset + 2;


                            Image gravatar = Gravatar.GravatarService.GetImageFromCache(revision.AuthorEmail + gravatarSize.ToString() + ".png", revision.AuthorEmail, Settings.AuthorImageCacheDays, gravatarSize, Settings.GravatarCachePath, FallBackService.MonsterId);

                            if (gravatar == null && !string.IsNullOrEmpty(revision.AuthorEmail))
                            {
                                ThreadPool.QueueUserWorkItem(o =>
                                        Gravatar.GravatarService.LoadCachedImage(revision.AuthorEmail + gravatarSize.ToString() + ".png", revision.AuthorEmail, null, Settings.AuthorImageCacheDays, gravatarSize, Settings.GravatarCachePath, RefreshGravatar, FallBackService.MonsterId));
                            }

                            if (gravatar != null)
                                e.Graphics.DrawImage(gravatar, gravatarLeft + 1, gravatarTop + 1, gravatarSize, gravatarSize);

                            e.Graphics.DrawRectangle(Pens.Black, gravatarLeft, gravatarTop, gravatarSize + 1, gravatarSize + 1);

                            string authorText;
                            string timeText;

                            if (rowHeigth >= 60)
                            {
                                authorText = revision.Author;
                                timeText = TimeToString(Settings.ShowAuthorDate ? revision.AuthorDate : revision.CommitDate);
                            }
                            else
                            {
                                timeText = string.Concat(revision.Author, " (", TimeToString(Settings.ShowAuthorDate ? revision.AuthorDate : revision.CommitDate), ")");
                                authorText = string.Empty;
                            }



                            e.Graphics.DrawString(authorText, rowFont, foreBrush,
                                                  new PointF(gravatarLeft + gravatarSize + 5, gravatarTop + 6));
                            e.Graphics.DrawString(timeText, rowFont, foreBrush,
                                                  new PointF(gravatarLeft + gravatarSize + 5, e.CellBounds.Bottom - textHeight - 4));
                        }
                    }
                    break;
                case 2:
                    {
                        var text = (string)e.FormattedValue;
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

        private void RevisionsCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var column = e.ColumnIndex;
            if (e.RowIndex < 0)
                return;

            if (Revisions.RowCount <= e.RowIndex)
                return;

            var revision = GetRevision(e.RowIndex);
            if (revision == null)
                return;

            e.FormattingApplied = true;

            switch (column)
            {
                case 0:
                    e.Value = revision.Guid;
                    break;
                case 1:
                    e.Value = revision.Message;
                    break;
                case 2:
                    e.Value = revision.Author ?? "";
                    break;
                case 3:
                    {
                        var time = Settings.ShowAuthorDate ? revision.AuthorDate : revision.CommitDate;
                        if (time == DateTime.MinValue || time == DateTime.MaxValue)
                            e.Value = "";
                        else
                            e.Value = string.Format("{0} {1}", time.ToShortDateString(), time.ToLongTimeString());
                    }
                    break;
                default:
                    e.FormattingApplied = false;
                    break;
            }
        }

        private void DrawColumnText(IDeviceContext dc, string text, Font font, Color color, Rectangle bounds)
        {
            TextRenderer.DrawText(dc, text, font, bounds, color, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private static Rectangle AdjustCellBounds(Rectangle cellBounds, float offset)
        {
            return new Rectangle((int)(cellBounds.Left + offset), cellBounds.Top + 4,
                                 cellBounds.Width - (int)offset, cellBounds.Height);
        }

        private static Color GetHeadColor(GitHead head)
        {
            return head.IsTag
                       ? Settings.TagColor
                       : head.IsHead
                             ? Settings.BranchColor
                             : head.IsRemote
                                   ? Settings.RemoteBranchColor
                                   : Settings.OtherTagColor;
        }

        private float RoundToEven(float value)
        {
            int result = ((int)value / 2) * 2;
            return result < value ? result + 2 : result;
        }

        private float DrawHeadBackground(bool isSelected, Graphics graphics, Color color,
            float x, float y, float width, float height, float radius, bool isCurrentBranch,
            bool isCurentBranchMergeSource)
        {
            float additionalOffset = isCurrentBranch || isCurentBranchMergeSource ? GetArrowSize(height) : 0;
            width += additionalOffset;
            var oldMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            try
            {
                // shade
                using (var shadePath = CreateRoundRectPath(x + 1, y + 1, width, height, radius))
                {
                    Color shadeColor = isSelected ? Color.Black : Color.Gray;
                    graphics.FillPath(new SolidBrush(shadeColor), shadePath);
                }

                using (var forePath = CreateRoundRectPath(x, y, width, height, radius))
                {
                    Color fillColor = Lerp(color, Color.White, 0.92F);

                    var fillBrush = new LinearGradientBrush(new RectangleF(x, y, width, height), fillColor,
                                                            Lerp(fillColor, Color.White, 0.9F), 90);

                    // fore rectangle
                    graphics.FillPath(fillBrush, forePath);
                    // frame
                    graphics.DrawPath(new Pen(Lerp(color, Color.White, 0.83F)), forePath);

                    // arrow if the head is the current branch 
                    if (isCurrentBranch)
                        DrawArrow(graphics, x, y, height, color, true);
                    else if (isCurentBranchMergeSource)
                        DrawArrow(graphics, x, y, height, color, false);
                }
            }
            finally
            {
                graphics.SmoothingMode = oldMode;
            }

            return additionalOffset;
        }

        private float GetArrowSize(float rowHeight)
        {
            return rowHeight - 6;
        }

        private void DrawArrow(Graphics graphics, float x, float y, float rowHeight, Color color, bool filled)
        {
            const float horShift = 4;
            const float verShift = 3;
            float height = rowHeight - verShift * 2;
            float width = height / 2;

            var points = new[]
                                 {
                                     new PointF(x + horShift, y + verShift),
                                     new PointF(x + horShift + width, y + verShift + height/2),
                                     new PointF(x + horShift, y + verShift + height),
                                     new PointF(x + horShift, y + verShift)
                                 };

            if (filled)
                graphics.FillPolygon(new SolidBrush(color), points);
            else
                graphics.DrawPolygon(new Pen(color), points);
        }

        private static GraphicsPath CreateRoundRectPath(float x, float y, float width, float height, float radius)
        {
            var path = new GraphicsPath();
            path.AddLine(x + radius, y, x + width - (radius * 2), y);
            path.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90);
            path.AddLine(x + width, y + radius, x + width, y + height - (radius * 2));
            path.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90);
            path.AddLine(x + width - (radius * 2), y + height, x + radius, y + height);
            path.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90);
            path.AddLine(x, y + height - (radius * 2), x, y + radius);
            path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
            path.CloseFigure();
            return path;
        }

        private static float Lerp(float start, float end, float amount)
        {
            float difference = end - start;
            float adjusted = difference * amount;
            return start + adjusted;
        }

        private static Color Lerp(Color colour, Color to, float amount)
        {
            // start colours as lerp-able floats
            float sr = colour.R, sg = colour.G, sb = colour.B;

            // end colours as lerp-able floats
            float er = to.R, eg = to.G, eb = to.B;

            // lerp the colours to get the difference
            byte r = (byte)Lerp(sr, er, amount),
                 g = (byte)Lerp(sg, eg, amount),
                 b = (byte)Lerp(sb, eb, amount);

            // return the new colour
            return Color.FromArgb(r, g, b);
        }

        private void RefreshGravatar(Image image)
        {
            _syncContext.Post(state => Revisions.Refresh(), null);
        }


        private void RevisionsDoubleClick(object sender, EventArgs e)
        {
            ViewSelectedRevisions();
        }

        public void ViewSelectedRevisions()
        {
            var selectedRevisions = GetSelectedRevisions();
            if (selectedRevisions.Count > 0)
            {
                var form = new FormDiffSmall(UICommands, selectedRevisions[0]);
                form.ShowDialog(this);
            }
            else
                UICommands.StartCompareRevisionsDialog(this);
        }

        private void SelectionTimerTick(object sender, EventArgs e)
        {
            SelectionTimer.Enabled = false;
            SelectionTimer.Stop();
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        private void CreateTagToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            using (var frm = new FormTagSmall(UICommands, GetRevision(LastRow)))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshRevisions();
                }
            }
        }

        private void ResetCurrentBranchToHereToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            var frm = new FormResetCurrentBranch(UICommands, GetRevision(LastRow));
            frm.ShowDialog(this);
            RefreshRevisions();
            OnActionOnRepositoryPerformed();
        }

        private void CreateNewBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            var frm = new FormBranchSmall(UICommands) { Revision = GetRevision(LastRow) };

            if (frm.ShowDialog(this) == DialogResult.OK)
            {
                RefreshRevisions();
                OnActionOnRepositoryPerformed();
            }
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

            if (LastRow == hti.RowIndex)
                return;

            LastRow = hti.RowIndex;
            Revisions.ClearSelection();

            if (LastRow >= 0 && Revisions.Rows.Count > LastRow)
                Revisions.Rows[LastRow].Selected = true;
        }

        private void CommitClick(object sender, EventArgs e)
        {
            UICommands.StartCommitDialog(this);
            OnActionOnRepositoryPerformed();
            RefreshRevisions();
        }

        private void GitIgnoreClick(object sender, EventArgs e)
        {
            UICommands.StartEditGitIgnoreDialog(this);
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
                LogParam = "--all --boundary";
            else if (Settings.ShowCurrentBranchOnly)
                LogParam = "";
            else
                LogParam = BranchFilter.Length > 0
                               ? String.Empty
                               : "--all --boundary";
        }

        private void RevertCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            var frm = new FormRevertCommitSmall(UICommands, GetRevision(LastRow));
            if (frm.ShowDialog(this) == DialogResult.OK)
            {
                RefreshRevisions();
            }
        }

        private void FilterToolStripMenuItemClick(object sender, EventArgs e)
        {
            _revisionFilter.ShowDialog(this);
            ForceRefreshRevisions();
        }

        private void ApplyFilterFromRevisionFilterDialog()
        {
            BranchFilter = _revisionFilter.GetBranchFilter();
            SetShowBranches();
        }

        private void CreateTagOpening(object sender, CancelEventArgs e)
        {
            if (Revisions.RowCount < LastRow || LastRow < 0 || Revisions.RowCount == 0)
                return;

            var inTheMiddleOfBisect = Module.InTheMiddleOfBisect();
            markRevisionAsBadToolStripMenuItem.Visible = inTheMiddleOfBisect;
            markRevisionAsGoodToolStripMenuItem.Visible = inTheMiddleOfBisect;
            bisectSkipRevisionToolStripMenuItem.Visible = inTheMiddleOfBisect;
            stopBisectToolStripMenuItem.Visible = inTheMiddleOfBisect;
            bisectSeparator.Visible = inTheMiddleOfBisect;

            var revision = GetRevision(LastRow);

            var deleteTagDropDown = new ContextMenuStrip();
            var deleteBranchDropDown = new ContextMenuStrip();
            var checkoutBranchDropDown = new ContextMenuStrip();
            var mergeBranchDropDown = new ContextMenuStrip();
            var rebaseDropDown = new ContextMenuStrip();
            var renameDropDown = new ContextMenuStrip();

            var tagNameCopy = new ContextMenuStrip();
            var branchNameCopy = new ContextMenuStrip();

            foreach (var head in revision.Heads.Where(h => h.IsTag))
            {
                ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                ToolStripItem tagName = new ToolStripMenuItem(head.Name);
                toolStripItem.Click += ToolStripItemClickDeleteTag;
                deleteTagDropDown.Items.Add(toolStripItem);
                tagName.Click += copyToClipBoard;
                tagNameCopy.Items.Add(tagName);
            }

            //For now there is no action that could be done on currentBranch
            string currentBranch = Module.GetSelectedBranch();
            var allBranches = revision.Heads.Where(h => !h.IsTag && (h.IsHead || h.IsRemote));
            var localBranches = allBranches.Where(b => !b.IsRemote);

            var branchesWithNoIdenticalRemotes = allBranches.Where(
                b => !b.IsRemote || !localBranches.Any(lb => lb.TrackingRemote == b.Remote && lb.MergeWith == b.LocalName));

            bool currentBranchPointsToRevision = false;
            foreach (var head in branchesWithNoIdenticalRemotes)
            {
                if (head.Name.Equals(currentBranch))
                    currentBranchPointsToRevision = true;
                else
                {
                    ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Click += ToolStripItemClickMergeBranch;
                    mergeBranchDropDown.Items.Add(toolStripItem);

                    toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Click += ToolStripItemClickRebaseBranch;
                    rebaseDropDown.Items.Add(toolStripItem);
                }
            }

            //if there is no branch to rebase on, then allow user to rebase on selected commit 
            if (rebaseDropDown.Items.Count == 0 && !currentBranchPointsToRevision)
            {
                ToolStripItem toolStripItem = new ToolStripMenuItem(revision.Guid);
                toolStripItem.Click += ToolStripItemClickRebaseBranch;
                rebaseDropDown.Items.Add(toolStripItem);
            }

            //if there is no branch to merge, then let user to merge selected commit into current branch 
            if (mergeBranchDropDown.Items.Count == 0 && !currentBranchPointsToRevision)
            {
                ToolStripItem toolStripItem = new ToolStripMenuItem(revision.Guid);
                toolStripItem.Click += ToolStripItemClickMergeBranch;
                mergeBranchDropDown.Items.Add(toolStripItem);
            }


            foreach (var head in allBranches)
            {
                ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                ToolStripItem branchName = new ToolStripMenuItem(head.Name);
                branchName.Click += copyToClipBoard;
                branchNameCopy.Items.Add(branchName);

                //skip remote branches - they can not be deleted this way
                if (!head.IsRemote)
                {
                    if (!head.Name.Equals(currentBranch))
                    {
                        toolStripItem = new ToolStripMenuItem(head.Name);
                        toolStripItem.Click += ToolStripItemClickDeleteBranch;
                        deleteBranchDropDown.Items.Add(toolStripItem); //Add to delete branch
                    }

                    toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Click += ToolStripItemClickRenameBranch;
                    renameDropDown.Items.Add(toolStripItem); //Add to rename branch
                }

                if (!head.Name.Equals(currentBranch))
                {

                    toolStripItem = new ToolStripMenuItem(head.Name);
                    if (head.IsRemote)
                        toolStripItem.Click += ToolStripItemClickCheckoutRemoteBranch;
                    else
                        toolStripItem.Click += ToolStripItemClickCheckoutBranch;
                    checkoutBranchDropDown.Items.Add(toolStripItem);
                }
            }

            deleteTagToolStripMenuItem.DropDown = deleteTagDropDown;
            deleteTagToolStripMenuItem.Visible = deleteTagDropDown.Items.Count > 0;

            deleteBranchToolStripMenuItem.DropDown = deleteBranchDropDown;
            deleteBranchToolStripMenuItem.Visible = deleteBranchDropDown.Items.Count > 0;

            checkoutBranchToolStripMenuItem.DropDown = checkoutBranchDropDown;
            checkoutBranchToolStripMenuItem.Visible = checkoutBranchDropDown.Items.Count > 0;

            mergeBranchToolStripMenuItem.DropDown = mergeBranchDropDown;
            mergeBranchToolStripMenuItem.Visible = mergeBranchDropDown.Items.Count > 0;

            rebaseOnToolStripMenuItem.DropDown = rebaseDropDown;
            rebaseOnToolStripMenuItem.Visible = rebaseDropDown.Items.Count > 0;

            renameBranchToolStripMenuItem.DropDown = renameDropDown;
            renameBranchToolStripMenuItem.Visible = renameDropDown.Items.Count > 0;

            branchNameToolStripMenuItem.DropDown = branchNameCopy;
            branchNameToolStripMenuItem.Visible = branchNameCopy.Items.Count > 0;

            tagToolStripMenuItem.DropDown = tagNameCopy;
            tagToolStripMenuItem.Visible = tagNameCopy.Items.Count > 0;

            toolStripSeparator6.Visible = tagNameCopy.Items.Count > 0 || branchNameCopy.Items.Count > 0;

            RefreshOwnScripts();
        }

        private void ToolStripItemClickDeleteTag(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            if (UICommands.StartDeleteTagDialog(this, toolStripItem.Text))
            {
                ForceRefreshRevisions();
            }
        }

        private void ToolStripItemClickDeleteBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            UICommands.StartDeleteBranchDialog(this, toolStripItem.Text);

            ForceRefreshRevisions();
        }

        private void ToolStripItemClickCheckoutBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            string branch = toolStripItem.Text;
            UICommands.StartCheckoutBranchDialog(this, branch, false);

            ForceRefreshRevisions();
            OnActionOnRepositoryPerformed();
        }

        private void ToolStripItemClickCheckoutRemoteBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            UICommands.StartCheckoutRemoteBranchDialog(this, toolStripItem.Text);

            ForceRefreshRevisions();
            OnActionOnRepositoryPerformed();
        }

        private void ToolStripItemClickMergeBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            UICommands.StartMergeBranchDialog(this, toolStripItem.Text);

            ForceRefreshRevisions();
            OnActionOnRepositoryPerformed();
        }

        private void ToolStripItemClickRebaseBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            UICommands.StartRebaseDialog(this, toolStripItem.Text);

            ForceRefreshRevisions();
            OnActionOnRepositoryPerformed();
        }

        private void ToolStripItemClickRenameBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            var renameExecuted = UICommands.StartRenameDialog(this, toolStripItem.Text);
            if (!renameExecuted)
                return;

            ForceRefreshRevisions();
            OnActionOnRepositoryPerformed();
        }

        private void CheckoutRevisionToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            if (MessageBox.Show(this, _areYouSureYouWantCheckout.Text, _areYouSureYouWantCheckoutCaption.Text,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            UICommands.StartCheckoutBranchDialog(this, GetRevision(LastRow).Guid, false);
            ForceRefreshRevisions();
            OnActionOnRepositoryPerformed();
        }

        private void ArchiveRevisionToolStripMenuItemClick(object sender, EventArgs e)
        {
            var selectedRevisions = GetSelectedRevisions();
            if (selectedRevisions.Count != 1)
            {
                MessageBox.Show(this, "Select exactly one revision. Abort.", "Archive revision");
                return;
            }

            UICommands.StartArchiveDialog(this, selectedRevisions.First());
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

        private void CherryPickCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            var revisions = GetSelectedRevisions(SortDirection.Descending);

            FormCherryPick prevForm = null;

            try
            {
                foreach (var r in revisions)
                {
                    var frm = new FormCherryPick(UICommands, r);
                    if (prevForm != null)
                    {
                        frm.CopyOptions(prevForm);
                        prevForm.Dispose();
                    }
                    prevForm = frm;
                    if (frm.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                }
            }
            finally
            {
                if (prevForm != null)
                {
                    prevForm.Dispose();
                }
            }

            ForceRefreshRevisions();
            OnActionOnRepositoryPerformed();
        }

        private void FixupCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            PrepareCorrectionCommit(CommitKind.Fixup);
        }

        private void SquashCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            PrepareCorrectionCommit(CommitKind.Squash);
        }

        private void PrepareCorrectionCommit(CommitKind commitKind)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            using (var frm = new FormCommit(UICommands, commitKind, GetRevision(LastRow))) frm.ShowDialog(this);

            ForceRefreshRevisions();
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

            return GitCommandHelpers.GetRelativeDateString(DateTime.Now, time, false);
        }

        private void UpdateGraph(GitRevision rev)
        {
            if (rev == null)
            {
                // Prune the graph and make sure the row count matches reality
                Revisions.Prune();

                if (Revisions.RowCount == 0 && Settings.RevisionGraphShowWorkingDirChanges)
                    CheckUncommitedChanged();
                return;
            }

            var dataType = DvcsGraph.DataType.Normal;
            if (rev.Guid == CurrentCheckout)
                dataType = DvcsGraph.DataType.Active;
            else if (rev.Heads.Count > 0)
                dataType = DvcsGraph.DataType.Special;

            Revisions.Add(rev.Guid, rev.ParentGuids, dataType, rev);
        }

        private void CheckUncommitedChanged()
        {
            bool unstagedChanges = false;
            bool stagedChanges = false;
            //Only check for tracked files. This usually makes more sense and it performs a lot
            //better then checking for untracked files.
            // TODO: Check FiltredFileName
            if (Module.GetUnstagedFiles().Count > 0)
                unstagedChanges = true;
            if (Module.GetStagedFiles().Count > 0)
                stagedChanges = true;

            // FiltredCurrentCheckout doesn't works because only calculated after loading all revisions in SelectInitialRevision()
            if (unstagedChanges)
            {
                //Add working dir as virtual commit
                var workingDir = new GitRevision(Module, GitRevision.UnstagedGuid)
                                     {
                                         Message = Strings.GetCurrentUnstagedChanges(),
                                         ParentGuids =
                                             stagedChanges
                                                 ? new[] { GitRevision.IndexGuid }
                                                 : new[] { FiltredCurrentCheckout }
                                     };
                Revisions.Add(workingDir.Guid, workingDir.ParentGuids, DvcsGraph.DataType.Normal, workingDir);
            }

            if (stagedChanges)
            {
                //Add index as virtual commit
                var index = new GitRevision(Module, GitRevision.IndexGuid)
                                {
                                    Message = Strings.GetCurrentIndex(),
                                    ParentGuids = new[] { FiltredCurrentCheckout }
                                };
                Revisions.Add(index.Guid, index.ParentGuids, DvcsGraph.DataType.Normal, index);
            }
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

        private void hashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(GetRevision(LastRow).Guid);
        }

        private static void copyToClipBoard(object sender, EventArgs e)
        {
            Clipboard.SetText(sender.ToString());
        }

        private void markRevisionAsBadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ContinueBisect(GitBisectOption.Bad);
        }

        private void markRevisionAsGoodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ContinueBisect(GitBisectOption.Good);
        }

        private void bisectSkipRevisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ContinueBisect(GitBisectOption.Skip);
        }

        private void ContinueBisect(GitBisectOption bisectOption)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            FormProcess.ShowDialog(this, Module, GitCommandHelpers.ContinueBisectCmd(bisectOption, GetRevision(LastRow).Guid), false);
            RefreshRevisions();
        }

        private void stopBisectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormProcess.ShowDialog(this, Module, GitCommandHelpers.StopBisectCmd());
            RefreshRevisions();
        }

        private void RefreshOwnScripts()
        {
            RemoveOwnScripts();
            AddOwnScripts();
        }

        private void AddOwnScripts()
        {
            IList<ScriptInfo> scripts = ScriptManager.GetScripts();
            int addedScripts = 0;
            if (scripts != null)
            {
                foreach (ScriptInfo scriptInfo in scripts)
                {
                    if (scriptInfo.Enabled)
                    {
                        addedScripts++;
                        ToolStripItem item = new ToolStripMenuItem(scriptInfo.Name);
                        item.Name = item.Text + "_ownScript";
                        item.Click += runScript;
                        if (scriptInfo.AddToRevisionGridContextMenu)
                            CreateTag.Items.Add(item);
                        else
                            runScriptToolStripMenuItem.DropDown.Items.Add(item);
                    }
                }
                toolStripSeparator7.Visible = addedScripts > 1;
                runScriptToolStripMenuItem.Visible = runScriptToolStripMenuItem.DropDown.Items.Count > 0;
            }
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

        private bool settingsLoaded;

        private void runScript(object sender, EventArgs e)
        {
            if (settingsLoaded == false)
            {
                new FormSettings(UICommands).LoadSettings();
                settingsLoaded = true;
            }
            ScriptRunner.RunScript(Module, sender.ToString(), this);
            RefreshRevisions();
        }

        #region Drag/drop patch files on revision grid

        void Revisions_DragDrop(object sender, DragEventArgs e)
        {
            var fileNameArray = e.Data.GetData(DataFormats.FileDrop) as Array;
            if (fileNameArray != null)
            {
                if (fileNameArray.Length > 10)
                {
                    //Some users need to be protected against themselves!
                    MessageBox.Show(this, _droppingFilesBlocked.Text);
                    return;
                }

                foreach (object fileNameObject in fileNameArray)
                {
                    var fileName = fileNameObject as string;

                    if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".patch", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //Start apply patch dialog for each dropped patch file...
                        UICommands.StartApplyPatchDialog(this, fileName);
                    }
                }
            }
        }

        static void Revisions_DragEnter(object sender, DragEventArgs e)
        {
            var fileNameArray = e.Data.GetData(DataFormats.FileDrop) as Array;
            if (fileNameArray != null)
            {
                foreach (object fileNameObject in fileNameArray)
                {
                    var fileName = fileNameObject as string;

                    if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".patch", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //Allow drop (copy, not move) patch files
                        e.Effect = DragDropEffects.Copy;
                    }
                    else
                    {
                        //When a non-patch file is dragged, do not allow it
                        e.Effect = DragDropEffects.None;
                        return;
                    }
                }
            }
        }
        #endregion

        private void ShowGitNotesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.ShowGitNotes = !showGitNotesToolStripMenuItem.Checked;
            showGitNotesToolStripMenuItem.Checked = Settings.ShowGitNotes;

            ForceRefreshRevisions();
        }

        public void OnModuleChanged(GitModule aModule)
        {
            if (GitModuleChanged != null)
                GitModuleChanged(aModule);
        }

        private void InitRepository_Click(object sender, EventArgs e)
        {
            UICommands.StartInitializeDialog(this, Module.WorkingDir, OnModuleChanged);
        }

        private void CloneRepository_Click(object sender, EventArgs e)
        {
            if (UICommands.StartCloneDialog(this, null, OnModuleChanged))
                ForceRefreshRevisions();
        }

        private void ShowRevisionGraphToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Settings.RevisionGraphLayout == (int)RevisionGridLayout.Small) Settings.RevisionGraphLayout = (int)RevisionGridLayout.SmallWithGraph;
            else if (Settings.RevisionGraphLayout == (int)RevisionGridLayout.Card) Settings.RevisionGraphLayout = (int)RevisionGridLayout.CardWithGraph;
            else if (Settings.RevisionGraphLayout == (int)RevisionGridLayout.LargeCard) Settings.RevisionGraphLayout = (int)RevisionGridLayout.LargeCardWithGraph;
            else if (Settings.RevisionGraphLayout == (int)RevisionGridLayout.SmallWithGraph) Settings.RevisionGraphLayout = (int)RevisionGridLayout.Small;
            else if (Settings.RevisionGraphLayout == (int)RevisionGridLayout.CardWithGraph) Settings.RevisionGraphLayout = (int)RevisionGridLayout.Card;
            else if (Settings.RevisionGraphLayout == (int)RevisionGridLayout.LargeCardWithGraph) Settings.RevisionGraphLayout = (int)RevisionGridLayout.LargeCard;
            else if (Settings.RevisionGraphLayout == (int)RevisionGridLayout.FilledBranchesSmall) Settings.RevisionGraphLayout = (int)RevisionGridLayout.FilledBranchesSmallWithGraph;
            else if (Settings.RevisionGraphLayout == (int)RevisionGridLayout.FilledBranchesSmallWithGraph) Settings.RevisionGraphLayout = (int)RevisionGridLayout.FilledBranchesSmall;
            SetRevisionsLayout();
            Refresh();
        }

        public void ToggleRevisionCardLayout()
        {
            var layouts = new List<RevisionGridLayout>((RevisionGridLayout[])Enum.GetValues(typeof(RevisionGridLayout)));
            layouts.Sort();
            var maxLayout = (int)layouts[layouts.Count - 1];

            int nextLayout = Settings.RevisionGraphLayout + 1;

            if (nextLayout > maxLayout)
                nextLayout = 1;

            SetRevisionsLayout((RevisionGridLayout)nextLayout);
        }

        public void SetRevisionsLayout(RevisionGridLayout revisionGridLayout)
        {
            Settings.RevisionGraphLayout = (int)revisionGridLayout;
            SetRevisionsLayout();
        }

        private void SetRevisionsLayout()
        {
            layout = Enum.IsDefined(typeof(RevisionGridLayout), Settings.RevisionGraphLayout)
                         ? (RevisionGridLayout)Settings.RevisionGraphLayout
                         : RevisionGridLayout.SmallWithGraph;

            showRevisionGraphToolStripMenuItem.Checked = IsGraphLayout();
            IsCardLayout();

            NormalFont = Settings.Font;// new Font(Settings.Font.Name, Settings.Font.Size + 2); // SystemFonts.DefaultFont.FontFamily, SystemFonts.DefaultFont.Size + 2);

            if (IsCardLayout())
            {
                if (Settings.RevisionGraphLayout == (int)RevisionGridLayout.Card
                    || Settings.RevisionGraphLayout == (int)RevisionGridLayout.CardWithGraph)
                {
                    rowHeigth = 45;
                }
                else
                {
                    rowHeigth = 70;
                }

                selectedItemBrush = new LinearGradientBrush(new Rectangle(0, 0, rowHeigth, rowHeigth),
                Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor,
                Color.LightBlue, 90, false);

                Revisions.ShowAuthor(!IsCardLayout());
                Revisions.SetDimensions(NODE_DIMENSION, LANE_WIDTH, LANE_LINE_WIDTH, rowHeigth, selectedItemBrush);

            }
            else
            {
                if (IsFilledBranchesLayout())
                {
                    using (var graphics = Graphics.FromHwnd(Handle))
                    {
                        rowHeigth = (int)graphics.MeasureString("By", NormalFont).Height + 9;
                    }

                    selectedItemBrush = SystemBrushes.Highlight;
                }
                else
                {
                    rowHeigth = 25;

                    selectedItemBrush = new LinearGradientBrush(new Rectangle(0, 0, rowHeigth, rowHeigth),
                                                                Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor,
                                                                Color.LightBlue, 90, false);
                }

                Revisions.ShowAuthor(!IsCardLayout());
                Revisions.SetDimensions(NODE_DIMENSION, LANE_WIDTH, LANE_LINE_WIDTH, rowHeigth, selectedItemBrush);
            }

            //Hide graph column when there it is disabled OR when a filter is active
            //allowing for special case when history of a single file is being displayed
            if (!IsGraphLayout() || (ShouldHideGraph(false) && !AllowGraphWithFilter))
            {
                Revisions.HideRevisionGraph();
            }
            else
            {
                Revisions.ShowRevisionGraph();
            }
        }

        private bool IsFilledBranchesLayout()
        {
            return layout == RevisionGridLayout.FilledBranchesSmall || layout == RevisionGridLayout.FilledBranchesSmallWithGraph;
        }

        private bool IsCardLayout()
        {
            return layout == RevisionGridLayout.Card
                   || layout == RevisionGridLayout.CardWithGraph
                   || layout == RevisionGridLayout.LargeCard
                   || layout == RevisionGridLayout.LargeCardWithGraph;
        }

        private bool IsGraphLayout()
        {
            return layout == RevisionGridLayout.SmallWithGraph
                   || layout == RevisionGridLayout.CardWithGraph
                   || layout == RevisionGridLayout.LargeCardWithGraph
                   || layout == RevisionGridLayout.FilledBranchesSmallWithGraph;
        }

        #region Hotkey commands

        public const string HotkeySettingsName = "RevisionGrid";

        internal enum Commands
        {
            ToggleRevisionGraph,
            RevisionFilter,
            ToggleAuthorDateCommitDate,
            ToggleOrderRevisionsByDate,
            ToggleShowRelativeDate,
            ToggleDrawNonRelativesGray,
            ToggleShowGitNotes,
            ToggleRevisionCardLayout,
            ShowAllBranches,
            ShowCurrentBranchOnly,
            GoToParent,
            GoToChild,
            ToggleHighlightSelectedBranch,
            NextQuickSearch,
            PrevQuickSearch
        }

        protected override bool ExecuteCommand(int cmd)
        {
            Commands command = (Commands)cmd;

            switch (command)
            {
                case Commands.ToggleRevisionGraph: ShowRevisionGraphToolStripMenuItemClick(null, null); break;
                case Commands.RevisionFilter: FilterToolStripMenuItemClick(null, null); break;
                case Commands.ToggleAuthorDateCommitDate: ShowAuthorDateToolStripMenuItemClick(null, null); break;
                case Commands.ToggleOrderRevisionsByDate: OrderRevisionsByDateToolStripMenuItemClick(null, null); break;
                case Commands.ToggleShowRelativeDate: ShowRelativeDateToolStripMenuItemClick(null, null); break;
                case Commands.ToggleDrawNonRelativesGray: drawNonrelativesGrayToolStripMenuItem_Click(null, null); break;
                case Commands.ToggleShowGitNotes: ShowGitNotesToolStripMenuItem_Click(null, null); break;
                case Commands.ToggleRevisionCardLayout: ToggleRevisionCardLayout(); break;
                case Commands.ShowAllBranches: ShowAllBranchesToolStripMenuItemClick(null, null); break;
                case Commands.ShowCurrentBranchOnly: ShowCurrentBranchOnlyToolStripMenuItemClick(null, null); break;
                case Commands.GoToParent: goToParentToolStripMenuItem_Click(null, null); break;
                case Commands.GoToChild: goToChildToolStripMenuItem_Click(null, null); break;
                case Commands.ToggleHighlightSelectedBranch: ToggleHighlightSelectedBranch(); break;
                case Commands.NextQuickSearch: NextQuickSearch(true); break;
                case Commands.PrevQuickSearch: NextQuickSearch(false); break;
                default: return base.ExecuteCommand(cmd);
            }

            return true;
        }

        #endregion

        private void NextQuickSearch(bool down)
        {
            var curIndex = -1;
            if (Revisions.SelectedRows.Count > 0)
                curIndex = Revisions.SelectedRows[0].Index;

            RestartQuickSearchTimer();

            bool reverse = !down;
            var nextIndex = 0;
            if (curIndex >= 0)
                nextIndex = reverse ? curIndex - 1 : curIndex + 1;
            _quickSearchString = _lastQuickSearchString;
            FindNextMatch(nextIndex, _quickSearchString, reverse);
            ShowQuickSearchString();
        }

        private void ToggleHighlightSelectedBranch()
        {
            if (_revisionGraphCommand != null)
            {
                MessageBox.Show("Cannot highlight selected branch when revision graph is loading.");
                return;
            }
            Revisions.RevisionGraphDrawStyle = DvcsGraph.RevisionGraphDrawStyleEnum.HighlightSelected;
            HighlightSelectedBranch();
        }

        public void HighlightSelectedBranch()
        {
            var revisions = GetSelectedRevisions();
            if (RevisionGraphDrawStyle == DvcsGraph.RevisionGraphDrawStyleEnum.HighlightSelected &&
                revisions.Count > 0)
            {
                HighlightBranch(revisions[0].Guid);
                Refresh();
            }
        }

        private void deleteBranchTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
            {
                if (item.DropDown != null && item.DropDown.Items.Count == 1)
                    item.DropDown.Items[0].PerformClick();
            }
        }

        private void goToParentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = GetRevision(LastRow);
            if (r.HasParent())
                SetSelectedRevision(new GitRevision(Module, r.ParentGuids[0]));
        }

        private void goToChildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = GetRevision(LastRow);
            var children = GetRevisionChildren(r.Guid);
            if (children.Count > 0)
                SetSelectedRevision(new GitRevision(Module, children[0]));
        }
    }
}
