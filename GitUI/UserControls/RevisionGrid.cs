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
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitUI.BuildServerIntegration;
using GitUI.CommandsDialogs;
using GitUI.HelperDialogs;
using GitUI.Hotkey;
using GitUI.RevisionGridClasses;
using GitUI.Script;
using Gravatar;
using ResourceManager.Translation;
using GitUI.UserControls.RevisionGridClasses;
using GitUI.CommandsDialogs.BrowseDialog;

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

    public enum RevisionGraphDrawStyleEnum
    {
        Normal,
        DrawNonRelativesGray,
        HighlightSelected
    }

    [DefaultEvent("DoubleClick")]
    public sealed partial class RevisionGrid : GitModuleControl
    {
        private readonly TranslationString _droppingFilesBlocked = new TranslationString("For you own protection dropping more than 10 patch files at once is blocked!");
        private readonly TranslationString _cannotHighlightSelectedBranch = new TranslationString("Cannot highlight selected branch when revision graph is loading.");
        private readonly TranslationString _noRevisionFoundError = new TranslationString("No revision found.");

        private const int NodeDimension = 8;
        private const int LaneWidth = 13;
        private const int LaneLineWidth = 2;
        private Brush _selectedItemBrush;
        private Brush _filledItemBrush; // disposable brush

        private readonly FormRevisionFilter _revisionFilter = new FormRevisionFilter();

        private RefsFiltringOptions _refsOptions = RefsFiltringOptions.All | RefsFiltringOptions.Boundary;

        private bool _initialLoad = true;
        private string _initialSelectedRevision;
        private string _lastQuickSearchString = string.Empty;
        private Label _quickSearchLabel;
        private string _quickSearchString;
        private RevisionGraph _revisionGraphCommand;

        public BuildServerWatcher BuildServerWatcher { get; private set; }

        private RevisionGridLayout _layout;
        private int _rowHeigth;
        public event GitModuleChangedEventHandler GitModuleChanged;
        public event EventHandler<DoubleClickRevisionEventArgs> DoubleClickRevision;

        private readonly RevisionGridMenuCommands _revisionGridMenuCommands;

        bool showCurrentBranchOnlyToolStripMenuItemChecked; // refactoring
        bool showAllBranchesToolStripMenuItemChecked; // refactoring
        bool showFilteredBranchesToolStripMenuItemChecked; // refactoring

        public RevisionGrid()
        {
            InitLayout();
            InitializeComponent();
            this.Loading.Image = global::GitUI.Properties.Resources.loadingpanel;

            Translate();

            _revisionGridMenuCommands = new RevisionGridMenuCommands(this);
            _revisionGridMenuCommands.CreateOrUpdateMenuCommands();

            // fill View context menu from MenuCommands
            var viewMenuCommands = _revisionGridMenuCommands.GetViewMenuCommands();
            FillMenuFromMenuCommands(viewMenuCommands, viewToolStripMenuItem);

            // fill Navigate context menu from MenuCommands
            var navigateMenuCommands = _revisionGridMenuCommands.GetNavigateMenuCommands();
            FillMenuFromMenuCommands(navigateMenuCommands, navigateToolStripMenuItem);

            NormalFont = AppSettings.Font;
            Loading.Paint += Loading_Paint;

            Revisions.CellPainting += RevisionsCellPainting;
            Revisions.CellFormatting += RevisionsCellFormatting;
            Revisions.KeyPress += RevisionsKeyPress;

            showMergeCommitsToolStripMenuItem.Checked = AppSettings.ShowMergeCommits;
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

            IsMessageMultilineDataGridViewColumn.Width = 25;
            IsMessageMultilineDataGridViewColumn.DisplayIndex = 2;

            this.HotkeysEnabled = true;
            try
            {
                SetRevisionsLayout((RevisionGridLayout)AppSettings.RevisionGraphLayout);
            }
            catch
            {
                SetRevisionsLayout(RevisionGridLayout.SmallWithGraph);
            }
        }

        private void FillMenuFromMenuCommands(IEnumerable<MenuCommand> menuCommands, ToolStripMenuItem targetMenuItem)
        {
            foreach (var menuCommand in menuCommands)
            {
                var toolStripItem = (ToolStripItem)MenuCommand.CreateToolStripItem(menuCommand);
                var toolStripMenuItem = toolStripItem as ToolStripMenuItem;
                if (toolStripMenuItem != null)
                {
                    menuCommand.RegisterMenuItem(toolStripMenuItem);
                }
                targetMenuItem.DropDownItems.Add(toolStripItem);
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
                MessageDataGridViewColumn.DefaultCellStyle.Font = _normalFont;
                DateDataGridViewColumn.DefaultCellStyle.Font = _normalFont;
                IsMessageMultilineDataGridViewColumn.DefaultCellStyle.Font = _normalFont;

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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FiltredFileName { get; set; }
        [Browsable(false)]
        private string FiltredCurrentCheckout { get; set; }
        [Browsable(false)]
        public Task<string[]> SuperprojectCurrentCheckout { get; private set; }
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

        [Description("Show uncommited changes in revision grid if enabled in settings.")]
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool ShowUncommitedChangesIfPossible
        {
            get; set;
        }

        [Description("Show build server information in revision grid if enabled in settings.")]
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool ShowBuildServerInfo
        {
            get;
            set;
        }

        [Description("Do not open the commit info dialog on double click. This is used if the double click event is handled elseswhere.")]
        [Category("Behavior")]
        [DefaultValue(false)]
        public bool DoubleClickDoesNotOpenCommitInfo
        {
            get;
            set;
        }

        private IndexWatcher _indexWatcher;
        [Browsable(false)]
        public IndexWatcher IndexWatcher
        {
            get
            {
                if (_indexWatcher == null)
                    _indexWatcher = new IndexWatcher(UICommandsSource);

                return _indexWatcher;
            }
        }


        public void SetInitialRevision(GitRevision initialSelectedRevision)
        {
            _initialSelectedRevision = initialSelectedRevision != null ? initialSelectedRevision.Guid : null;
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
            quickSearchTimer.Interval = AppSettings.RevisionGridQuickSearchTimeout;
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

            int? searchResult;
            if (reverse)
                searchResult = SearchInReverseOrder(startIndex, searchString);
            else
                searchResult = SearchForward(startIndex, searchString);

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
                AppSettings.BranchFilterEnabled = false;
                AppSettings.ShowCurrentBranchOnly = true;
            }
            else
            {
                AppSettings.BranchFilterEnabled = true;
                AppSettings.ShowCurrentBranchOnly = false;
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

            BuildServerWatcher = new BuildServerWatcher(this, Revisions);
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

        public bool SetSelectedRevision(string revision)
        {
            if (revision != null)
            {
                for (var i = 0; i < Revisions.RowCount; i++)
                {
                    if (GetRevision(i).Guid == revision)
                    {
                        SetSelectedIndex(i);
                        return true;
                    }
                }
            }

            Revisions.ClearSelection();
            Revisions.Select();
            return false;
        }

        public GitRevision GetRevision(string guid)
        {
            return Revisions.GetRevision(guid);
        }

        public bool SetSelectedRevision(GitRevision revision)
        {
            return SetSelectedRevision(revision != null ? revision.Guid : null);
        }

        public void HighlightBranch(string aId)
        {
            RevisionGraphDrawStyle = RevisionGraphDrawStyleEnum.HighlightSelected;
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

        public RevisionGraphDrawStyleEnum RevisionGraphDrawStyle
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
            var revision = Module.GetRevision(CurrentCheckout, true);
            var refs = Module.GetRefs(true, true);
            foreach (var gitRef in refs)
            {
                if (gitRef.Guid.Equals(revision.Guid))
                {
                    revision.Refs.Add(gitRef);
                }
            }
            return revision;
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
            _revisionGridMenuCommands.CreateOrUpdateMenuCommands();
        }

        public void ReloadTranslation()
        {
            Translate();
        }

        public void ForceRefreshRevisions()
        {
            try
            {
                RevisionGraphDrawStyle = RevisionGraphDrawStyleEnum.DrawNonRelativesGray;
                IsMessageMultilineDataGridViewColumn.Visible = AppSettings.ShowIndicatorForMultilineMessage;

                ApplyFilterFromRevisionFilterDialog();

                _initialLoad = true;

                BuildServerWatcher.CancelBuildStatusFetchOperation();

                DisposeRevisionGraphCommand();

                var newCurrentCheckout = Module.GetCurrentCheckout();
                Task<string[]> newSuperprojectCurrentCheckout =
                    Task.Factory.StartNew(() => GetSuperprojectCheckout());
                newSuperprojectCurrentCheckout.ContinueWith((task) => Refresh(),
                    TaskScheduler.FromCurrentSynchronizationContext());

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

                if (!Module.IsValidGitWorkingDir())
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

                if (!AppSettings.ShowGitNotes && (_refsOptions & (RefsFiltringOptions.All | RefsFiltringOptions.Boundary)) == (RefsFiltringOptions.All | RefsFiltringOptions.Boundary))
                    _refsOptions |= RefsFiltringOptions.ShowGitNotes;

                if (AppSettings.ShowGitNotes)
                    _refsOptions &= ~RefsFiltringOptions.ShowGitNotes;

                if (!AppSettings.ShowMergeCommits)
                    _refsOptions |= RefsFiltringOptions.NoMerges;

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

                _revisionGraphCommand = new RevisionGraph(Module) { BranchFilter = BranchFilter, RefsOptions = _refsOptions, Filter = _revisionFilter.GetFilter() + Filter + FixedFilter };
                _revisionGraphCommand.Updated += GitGetCommitsCommandUpdated;
                _revisionGraphCommand.Exited += GitGetCommitsCommandExited;
                _revisionGraphCommand.Error += _revisionGraphCommand_Error;
                _revisionGraphCommand.InMemFilter = revGraphIMF;
                _revisionGraphCommand.Execute();
                LoadRevisions();
                SetRevisionsLayout();
            }
            catch (Exception)
            {
                Error.Visible = true;
                Error.BringToFront();
                throw;
            }
        }

        private string[] GetSuperprojectCheckout()
        {
            if (Module.SuperprojectModule == null)
                return new string[] {};
            var currentCheckout = Module.GetSuperprojectCurrentCheckout();
            if (currentCheckout.Key == 'U')
            {
                // return local and remote hashes
                return Module.SuperprojectModule.GetConflictedSubmoduleHashes(Module.SubmodulePath).Skip(1).ToArray();
            }
            return new[] { currentCheckout.Value };
        }

        private static readonly Regex PotentialShaPattern = new Regex(@"^[a-f0-9]{5,}", RegexOptions.Compiled);
        public static bool MessageFilterCouldBeSHA(string filter)
        {
            bool result = PotentialShaPattern.IsMatch(filter);

            return result;
        }

        private void _revisionGraphCommand_Error(object sender, AsyncErrorEventArgs e)
        {
            // This has to happen on the UI thread
            this.InvokeSync(o =>
                                  {
                                      Error.Visible = true;
                                      //Error.BringToFront();
                                      NoGit.Visible = false;
                                      NoCommits.Visible = false;
                                      Revisions.Visible = false;
                                      Loading.Visible = false;
                                  }, this);

            DisposeRevisionGraphCommand();
            this.InvokeAsync(() =>
                {
                    throw e.Exception;
                }
            );
            e.Handled = true;
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
                this.InvokeSync(o =>
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
                this.InvokeSync(o =>
                                      {
                                          UpdateGraph(null);
                                          Loading.Visible = false;
                                          SelectInitialRevision();
                                          _isLoading = false;
                                          if (ShowBuildServerInfo)
                                            BuildServerWatcher.LaunchBuildServerInfoFetchOperation();
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

            if (LastSelectedRows == null)
            {
                if (!string.IsNullOrEmpty(_initialSelectedRevision))
                {
                    string revision;
                    int index = SearchRevision(_initialSelectedRevision, out revision);
                    if (index >= 0)
                        SetSelectedIndex(index);
                }
                else
                {
                    SetSelectedRevision(FiltredCurrentCheckout);
                }
            }
            LastSelectedRows = null;

            if (!Revisions.IsRevisionRelative(FiltredCurrentCheckout))
            {
                HighlightBranch(FiltredCurrentCheckout);
            }
        }

        internal int TrySearchRevision(string initRevision, out string graphRevision)
        {
            var rows = Revisions
                .Rows
                .Cast<DataGridViewRow>();
            var revisions = rows
                .Select(row => new { row.Index, GetRevision(row.Index).Guid });

            var idx = revisions.FirstOrDefault(rev => rev.Guid == initRevision);
            if (idx != null)
            {
                graphRevision = idx.Guid;
                return idx.Index;
            }

            graphRevision = null;
            return -1;
        }

        private int SearchRevision(string initRevision, out string graphRevision)
        {
            int index = TrySearchRevision(initRevision, out graphRevision);
            if (index >= 0)
                return index;

            var rows = Revisions
                .Rows
                .Cast<DataGridViewRow>();
            var dict = rows
                .ToDictionary(row => GetRevision(row.Index).Guid, row => row.Index);
            var revListParams = "rev-list ";
            if (AppSettings.OrderRevisionByDate)
                revListParams += "--date-order ";
            else
                revListParams += "--topo-order ";
            if (AppSettings.MaxRevisionGraphCommits > 0)
                revListParams += string.Format("--max-count=\"{0}\" ", (int)AppSettings.MaxRevisionGraphCommits);

            var allrevisions = Module.ReadGitOutputLines(revListParams + initRevision);
            graphRevision = allrevisions.FirstOrDefault(rev => dict.TryGetValue(rev, out index));
            if (graphRevision != null)
                return index;
            return -1;
        }

        private static string GetDateHeaderText()
        {
            return AppSettings.ShowAuthorDate ? Strings.GetAuthorDateText() : Strings.GetCommitDateText();
        }

        private void LoadRevisions()
        {
            if (_revisionGraphCommand == null)
            {
                return;
            }

            Revisions.SuspendLayout();

            Revisions.MessageColumn.HeaderText = Strings.GetMessageText();
            Revisions.AuthorColumn.HeaderText = Strings.GetAuthorText();
            Revisions.DateColumn.HeaderText = GetDateHeaderText();

            Revisions.SelectionChanged -= RevisionsSelectionChanged;

            if (LastSelectedRows != null)
                Revisions.SelectedIds = LastSelectedRows;

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

            var columnIndex = e.ColumnIndex;

            int graphColIndex = GraphDataGridViewColumn.Index;
            int messageColIndex = MessageDataGridViewColumn.Index;
            int authorColIndex = AuthorDataGridViewColumn.Index;
            int dateColIndex = DateDataGridViewColumn.Index;
            int isMsgMultilineColIndex = IsMessageMultilineDataGridViewColumn.Index;

            // The graph column is handled by the DvcsGraph
            if (e.ColumnIndex == graphColIndex)
            {
                return;
            }

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
                e.Graphics.FillRectangle(_selectedItemBrush, e.CellBounds);
            else
                e.Graphics.FillRectangle(Brushes.White, e.CellBounds);

            Color foreColor;

            if (!AppSettings.RevisionGraphDrawNonRelativesTextGray || Revisions.RowIsRelative(e.RowIndex))
            {
                foreColor = isRowSelected && IsFilledBranchesLayout()
                    ? SystemColors.HighlightText
                    : e.CellStyle.ForeColor;
            }
            else
            {
                foreColor = isRowSelected ? SystemColors.HighlightText : Color.Gray;
            }

            using (Brush foreBrush = new SolidBrush(foreColor))
            {
                var rowFont = NormalFont;
                if (revision.Guid == CurrentCheckout /*&& !showRevisionCards*/)
                    rowFont = HeadFont;
                else if (SuperprojectCurrentCheckout.IsCompleted && SuperprojectCurrentCheckout.Result.Contains(revision.Guid))
                    rowFont = SuperprojectFont;

                if (columnIndex == messageColIndex)
                {
                    int baseOffset = 0;
                    if (IsCardLayout())
                    {
                        baseOffset = 5;

                        Rectangle cellRectangle = new Rectangle(e.CellBounds.Left + baseOffset, e.CellBounds.Top + 1, e.CellBounds.Width - (baseOffset * 2), e.CellBounds.Height - 4);

                        if (!AppSettings.RevisionGraphDrawNonRelativesGray || Revisions.RowIsRelative(e.RowIndex))
                        {
                            e.Graphics.FillRectangle(
                                new LinearGradientBrush(cellRectangle,
                                                        Color.FromArgb(255, 220, 220, 231),
                                                        Color.FromArgb(255, 240, 240, 250), 90, false), cellRectangle);
                            using (var pen = new Pen(Color.FromArgb(255, 200, 200, 200), 1))
                            {
                                e.Graphics.DrawRectangle(pen, cellRectangle);
                            }
                        }
                        else
                        {
                            e.Graphics.FillRectangle(
                                new LinearGradientBrush(cellRectangle,
                                                        Color.FromArgb(255, 240, 240, 240),
                                                        Color.FromArgb(255, 250, 250, 250), 90, false), cellRectangle);
                        }

                        if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
                        {
                            using (var penSelectionBackColor = new Pen(Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor, 1))
                                e.Graphics.DrawRectangle(penSelectionBackColor, cellRectangle);
                        }
                    }

                    float offset = baseOffset;
                    var gitRefs = revision.Refs;

                    if (gitRefs.Any())
                    {
                        gitRefs.Sort((left, right) =>
                                        {
                                            if (left.IsTag != right.IsTag)
                                                return right.IsTag.CompareTo(left.IsTag);
                                            if (left.IsRemote != right.IsRemote)
                                                return left.IsRemote.CompareTo(right.IsRemote);
                                            return left.Name.CompareTo(right.Name);
                                        });

                        foreach (var gitRef in gitRefs.Where(head => (!head.IsRemote || _revisionGridMenuCommands.ShowRemoteBranches)))
                        {
                            Font refsFont;

                            if (gitRef.IsTag)
                            {
                                if (!AppSettings.ShowTags)
                                {
                                    continue;
                                }
                            }

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

                            Color headColor = GetHeadColor(gitRef);
                            using (Brush textBrush = new SolidBrush(headColor))
                            {
                            string headName;

                            if (IsCardLayout())
                            {
                                headName = gitRef.Name;
                                offset += e.Graphics.MeasureString(headName, refsFont).Width + 6;
                                PointF location = new PointF(e.CellBounds.Right - offset, e.CellBounds.Top + 4);
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
                                                ? gitRef.Name
                                                : string.Concat("[", gitRef.Name, "] ");

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
                                                                            gitRef.Selected,
                                                                            gitRef.SelectedHeadMergeSource);

                                    offset += extraOffset;
                                    headBounds.Offset((int)(extraOffset + 1), 0);
                                }

                                DrawColumnText(e.Graphics, headName, refsFont, headColor, headBounds);
                            }
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
                        int gravatarSize = _rowHeigth - textHeight - 12;
                        int gravatarTop = e.CellBounds.Top + textHeight + 6;
                        int gravatarLeft = e.CellBounds.Left + baseOffset + 2;


                        Image gravatar = Gravatar.GravatarService.GetImageFromCache(revision.AuthorEmail + gravatarSize.ToString() + ".png", revision.AuthorEmail, AppSettings.AuthorImageCacheDays, gravatarSize, AppSettings.GravatarCachePath, FallBackService.MonsterId);

                        if (gravatar == null && !string.IsNullOrEmpty(revision.AuthorEmail))
                        {
                            ThreadPool.QueueUserWorkItem(o =>
                                    Gravatar.GravatarService.LoadCachedImage(revision.AuthorEmail + gravatarSize.ToString() + ".png", revision.AuthorEmail, null, AppSettings.AuthorImageCacheDays, gravatarSize, AppSettings.GravatarCachePath, RefreshGravatar, FallBackService.MonsterId));
                        }

                        if (gravatar != null)
                            e.Graphics.DrawImage(gravatar, gravatarLeft + 1, gravatarTop + 1, gravatarSize, gravatarSize);

                        e.Graphics.DrawRectangle(Pens.Black, gravatarLeft, gravatarTop, gravatarSize + 1, gravatarSize + 1);

                        string authorText;
                        string timeText;

                        if (_rowHeigth >= 60)
                        {
                            authorText = revision.Author;
                            timeText = TimeToString(AppSettings.ShowAuthorDate ? revision.AuthorDate : revision.CommitDate);
                        }
                        else
                        {
                            timeText = string.Concat(revision.Author, " (", TimeToString(AppSettings.ShowAuthorDate ? revision.AuthorDate : revision.CommitDate), ")");
                            authorText = string.Empty;
                        }



                        e.Graphics.DrawString(authorText, rowFont, foreBrush,
                                                new PointF(gravatarLeft + gravatarSize + 5, gravatarTop + 6));
                        e.Graphics.DrawString(timeText, rowFont, foreBrush,
                                                new PointF(gravatarLeft + gravatarSize + 5, e.CellBounds.Bottom - textHeight - 4));
                    }
                }
                else if (columnIndex == authorColIndex)
                {
                    var text = (string)e.FormattedValue;
                    e.Graphics.DrawString(text, rowFont, foreBrush,
                                            new PointF(e.CellBounds.Left, e.CellBounds.Top + 4));
                }
                else if (columnIndex == dateColIndex)
                {
                    var time = AppSettings.ShowAuthorDate ? revision.AuthorDate : revision.CommitDate;
                    var text = TimeToString(time);
                    e.Graphics.DrawString(text, rowFont, foreBrush,
                                            new PointF(e.CellBounds.Left, e.CellBounds.Top + 4));
                }
                else if (columnIndex == BuildServerWatcher.BuildStatusImageColumnIndex)
                {
                    BuildInfoDrawingLogic.BuildStatusImageColumnCellPainting(e, revision, foreBrush, rowFont);
                }
                else if (columnIndex == BuildServerWatcher.BuildStatusMessageColumnIndex)
                {
                    BuildInfoDrawingLogic.BuildStatusMessageCellPainting(e, revision, foreBrush, rowFont);
                }
                else if (AppSettings.ShowIndicatorForMultilineMessage && columnIndex == isMsgMultilineColIndex)
                {
                    var text = (string)e.FormattedValue;
                    e.Graphics.DrawString(text, rowFont, foreBrush,
                                            new PointF(e.CellBounds.Left, e.CellBounds.Top + 4));
                }
            }
        }

        private void RevisionsCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var columnIndex = e.ColumnIndex;
            if (e.RowIndex < 0)
                return;

            if (Revisions.RowCount <= e.RowIndex)
                return;

            var revision = GetRevision(e.RowIndex);
            if (revision == null)
                return;

            e.FormattingApplied = true;

            int graphColIndex = GraphDataGridViewColumn.Index;
            int messageColIndex = MessageDataGridViewColumn.Index;
            int authorColIndex = AuthorDataGridViewColumn.Index;
            int dateColIndex = DateDataGridViewColumn.Index;
            int isMsgMultilineColIndex = IsMessageMultilineDataGridViewColumn.Index;

            if (columnIndex == graphColIndex)
            {
                e.Value = revision.Guid;
            }
            else if (columnIndex == messageColIndex)
            {
                e.Value = revision.Message;
            }
            else if (columnIndex == authorColIndex)
            {
                e.Value = revision.Author ?? "";
            }
            else if (columnIndex == dateColIndex)
            {
                var time = AppSettings.ShowAuthorDate ? revision.AuthorDate : revision.CommitDate;
                if (time == DateTime.MinValue || time == DateTime.MaxValue)
                    e.Value = "";
                else
                    e.Value = string.Format("{0} {1}", time.ToShortDateString(), time.ToLongTimeString());
            }
            else if (columnIndex == BuildServerWatcher.BuildStatusImageColumnIndex)
            {
                BuildInfoDrawingLogic.BuildStatusImageColumnCellFormatting(e, Revisions, revision);
            }
            else if (columnIndex == BuildServerWatcher.BuildStatusMessageColumnIndex)
            {
                BuildInfoDrawingLogic.BuildStatusMessageCellFormatting(e, revision);
            }
            else if (AppSettings.ShowIndicatorForMultilineMessage && columnIndex == isMsgMultilineColIndex)
            {
                if (revision.Body == null && !revision.IsArtificial())
                {
                    ThreadPool.QueueUserWorkItem(o => LoadIsMultilineMessageInfo(revision, columnIndex, e.RowIndex, Revisions.RowCount));
                }

                if (revision.Body != null)
                {
                    e.Value = revision.Body.TrimEnd().Contains("\n") ? "[...]" : "";
                }
                else
                {
                    e.Value = "";
                }
            }
            else
            {
                e.FormattingApplied = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="revision"></param>
        /// <param name="totalRowCount">check if grid has changed while thread is queued</param>
        /// <param name="colIndex"></param>
        /// <param name="rowIndex"></param>
        private void LoadIsMultilineMessageInfo(GitRevision revision, int colIndex, int rowIndex, int totalRowCount)
        {
            // code taken from CommitInfo.cs
            CommitData commitData = CommitData.CreateFromRevision(revision);
            string error = "";
            if (revision.Body == null)
            {
                CommitData.UpdateCommitMessage(commitData, Module, revision.Guid, ref error);
                revision.Body = commitData.Body;
            }

            // now that Body is filled (not null anymore) the revision grid can be refreshed to display the new information
            this.InvokeAsync(() =>
            {
                if (Revisions == null || Revisions.RowCount == 0 || Revisions.RowCount <= rowIndex || Revisions.RowCount != totalRowCount)
                {
                    return;
                }
                Revisions.InvalidateCell(colIndex, rowIndex);
            });
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

        private static Color GetHeadColor(GitRef gitRef)
        {
            if (gitRef.IsTag)
                return AppSettings.TagColor;
            if (gitRef.IsHead)
                return AppSettings.BranchColor;
            if (gitRef.IsRemote)
                return AppSettings.RemoteBranchColor;
            return AppSettings.OtherTagColor;
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
                    var shadeBrush = isSelected ? Brushes.Black : Brushes.Gray;
                    graphics.FillPath(shadeBrush, shadePath);
                }

                using (var forePath = CreateRoundRectPath(x, y, width, height, radius))
                {
                    Color fillColor = Lerp(color, Color.White, 0.92F);

                    using (var fillBrush = new LinearGradientBrush(new RectangleF(x, y, width, height), fillColor, Lerp(fillColor, Color.White, 0.9F), 90))
                    {
                        // fore rectangle
                        graphics.FillPath(fillBrush, forePath);
                        // frame
                        using (var pen = new Pen(Lerp(color, Color.White, 0.83F)))
                            graphics.DrawPath(pen, forePath);

                        // arrow if the head is the current branch 
                        if (isCurrentBranch)
                            DrawArrow(graphics, x, y, height, color, true);
                        else if (isCurentBranchMergeSource)
                            DrawArrow(graphics, x, y, height, color, false);
                    }
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
            {
                using (var solidBrush = new SolidBrush(color))
                    graphics.FillPolygon(solidBrush, points);
            }
            else
            {
                using (var pen = new Pen(color))
                    graphics.DrawPolygon(pen, points);
            }
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
            this.InvokeAsync(Revisions.Refresh);
        }

        private void RevisionsDoubleClick(object sender, EventArgs e)
        {
            if (DoubleClickRevision != null)
            {
                var selectedRevisions = GetSelectedRevisions();
                DoubleClickRevision(this, new DoubleClickRevisionEventArgs(selectedRevisions.FirstOrDefault()));
            }

            if (!DoubleClickDoesNotOpenCommitInfo)
            {
                ViewSelectedRevisions();
            }
        }

        public void ViewSelectedRevisions()
        {
            var selectedRevisions = GetSelectedRevisions();
            if (selectedRevisions.Any())
            {
                var form = new FormCommitDiff(UICommands, selectedRevisions[0].Guid);

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

            using (var frm = new FormCreateTag(UICommands, GetRevision(LastRow)))
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
        }

        private void CreateNewBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            UICommands.DoActionOnRepo(() =>
                {
                    var frm = new FormCreateBranch(UICommands, GetRevision(LastRow));

                    return frm.ShowDialog(this) == DialogResult.OK;
                });
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
        }

        private void GitIgnoreClick(object sender, EventArgs e)
        {
            UICommands.StartEditGitIgnoreDialog(this);
        }

        internal void InvalidateRevisions()
        {
            Revisions.Invalidate();
        }

        // internal because used by RevisonGridMenuCommands
        internal void ShowCurrentBranchOnlyToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (showCurrentBranchOnlyToolStripMenuItemChecked)
                return;

            AppSettings.BranchFilterEnabled = true;
            AppSettings.ShowCurrentBranchOnly = true;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        // internal because used by RevisonGridMenuCommands
        internal void ShowAllBranchesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (showAllBranchesToolStripMenuItemChecked)
                return;

            AppSettings.BranchFilterEnabled = false;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        // internal because used by RevisonGridMenuCommands
        internal void ShowFilteredBranchesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (showFilteredBranchesToolStripMenuItemChecked)
                return;

            AppSettings.BranchFilterEnabled = true;
            AppSettings.ShowCurrentBranchOnly = false;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        internal bool ShowCurrentBranchOnlyToolStripMenuItemChecked { get { return showCurrentBranchOnlyToolStripMenuItemChecked; } }
        internal bool ShowAllBranchesToolStripMenuItemChecked { get { return showAllBranchesToolStripMenuItemChecked; } }
        internal bool ShowFilteredBranchesToolStripMenuItemChecked { get { return showFilteredBranchesToolStripMenuItemChecked; } }

        private void SetShowBranches()
        {
            showAllBranchesToolStripMenuItemChecked = !AppSettings.BranchFilterEnabled;
            showCurrentBranchOnlyToolStripMenuItemChecked =
                AppSettings.BranchFilterEnabled && AppSettings.ShowCurrentBranchOnly;
            showFilteredBranchesToolStripMenuItemChecked =
                AppSettings.BranchFilterEnabled && !AppSettings.ShowCurrentBranchOnly;

            BranchFilter = _revisionFilter.GetBranchFilter();

            if (!AppSettings.BranchFilterEnabled)
                _refsOptions = RefsFiltringOptions.All | RefsFiltringOptions.Boundary;
            else if (AppSettings.ShowCurrentBranchOnly)
                _refsOptions = 0;
            else
                _refsOptions = BranchFilter.Length > 0
                               ? 0
                               : RefsFiltringOptions.All | RefsFiltringOptions.Boundary;

            _revisionGridMenuCommands.TriggerPropertyChanged(); // apply checkboxes changes also to FormBrowse main menu
        }

        private void RevertCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            UICommands.StartRevertCommitDialog(this, GetRevision(LastRow));
        }

        internal void FilterToolStripMenuItemClick(object sender, EventArgs e)
        {
            _revisionFilter.ShowDialog(this);
            ForceRefreshRevisions();
        }

        private void ApplyFilterFromRevisionFilterDialog()
        {
            BranchFilter = _revisionFilter.GetBranchFilter();
            SetShowBranches();
        }

        private void ContextMenuOpening(object sender, CancelEventArgs e)
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

            var gitRefListsForRevision = new GitRefListsForRevision(revision);

            foreach (var head in gitRefListsForRevision.AllTags)
            {
                ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                ToolStripItem tagName = new ToolStripMenuItem(head.Name);
                toolStripItem.Click += ToolStripItemClickDeleteTag;
                deleteTagDropDown.Items.Add(toolStripItem);
            }

            //For now there is no action that could be done on currentBranch
            bool bareRepository = Module.IsBareRepository();
            string currentBranch = Module.GetSelectedBranch();
            var allBranches = gitRefListsForRevision.AllBranches;
            var localBranches = gitRefListsForRevision.LocalBranches;
            var branchesWithNoIdenticalRemotes = gitRefListsForRevision.BranchesWithNoIdenticalRemotes;

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

            // clipboard branch and tag menu handling
            {
                branchNameCopyToolStripMenuItem.Tag = "caption";
                tagNameCopyToolStripMenuItem.Tag = "caption";
                MenuUtil.SetAsCaptionMenuItem(branchNameCopyToolStripMenuItem, mainContextMenu);
                MenuUtil.SetAsCaptionMenuItem(tagNameCopyToolStripMenuItem, mainContextMenu);

                var branchNames = gitRefListsForRevision.GetAllBranchNames();
                CopyToClipboardMenuHelper.SetCopyToClipboardMenuItems(
                    copyToClipboardToolStripMenuItem, branchNameCopyToolStripMenuItem, branchNames, "branchNameItem");

                var tagNames = gitRefListsForRevision.GetAllTagNames();
                CopyToClipboardMenuHelper.SetCopyToClipboardMenuItems(
                    copyToClipboardToolStripMenuItem, tagNameCopyToolStripMenuItem, tagNames, "tagNameItem");

                toolStripSeparator6.Visible = branchNames.Any() || tagNames.Any();
            }

            foreach (var head in allBranches)
            {
                ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);

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
            deleteTagToolStripMenuItem.Enabled = deleteTagDropDown.Items.Count > 0;

            deleteBranchToolStripMenuItem.DropDown = deleteBranchDropDown;
            deleteBranchToolStripMenuItem.Enabled = deleteBranchDropDown.Items.Count > 0;

            checkoutBranchToolStripMenuItem.DropDown = checkoutBranchDropDown;
            checkoutBranchToolStripMenuItem.Enabled = !bareRepository && checkoutBranchDropDown.Items.Count > 0;

            mergeBranchToolStripMenuItem.DropDown = mergeBranchDropDown;
            mergeBranchToolStripMenuItem.Enabled = !bareRepository && mergeBranchDropDown.Items.Count > 0;

            rebaseOnToolStripMenuItem.DropDown = rebaseDropDown;
            rebaseOnToolStripMenuItem.Enabled = !bareRepository && rebaseDropDown.Items.Count > 0;

            renameBranchToolStripMenuItem.DropDown = renameDropDown;
            renameBranchToolStripMenuItem.Enabled = renameDropDown.Items.Count > 0;

            checkoutRevisionToolStripMenuItem.Enabled = !bareRepository;
            revertCommitToolStripMenuItem.Enabled = !bareRepository;
            cherryPickCommitToolStripMenuItem.Enabled = !bareRepository;
            manipulateCommitToolStripMenuItem.Enabled = !bareRepository;

            toolStripSeparator6.Enabled = branchNameCopyToolStripMenuItem.Enabled || tagNameCopyToolStripMenuItem.Enabled;

            RefreshOwnScripts();
        }

        private void ToolStripItemClickDeleteTag(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            UICommands.StartDeleteTagDialog(this, toolStripItem.Text);
        }

        private void ToolStripItemClickDeleteBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            UICommands.StartDeleteBranchDialog(this, toolStripItem.Text);
        }

        private void ToolStripItemClickCheckoutBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            string branch = toolStripItem.Text;
            UICommands.StartCheckoutBranch(this, branch, false);
        }

        private void ToolStripItemClickCheckoutRemoteBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            UICommands.StartCheckoutRemoteBranch(this, toolStripItem.Text);
        }

        private void ToolStripItemClickMergeBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            UICommands.StartMergeBranchDialog(this, toolStripItem.Text);
        }

        private void ToolStripItemClickRebaseBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            UICommands.StartRebaseDialog(this, toolStripItem.Text);
        }

        private void ToolStripItemClickRenameBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            UICommands.StartRenameDialog(this, toolStripItem.Text);
        }

        private void CheckoutRevisionToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            string revision = GetRevision(LastRow).Guid;
            UICommands.StartCheckoutRevisionDialog(this, revision);
        }

        private void ArchiveRevisionToolStripMenuItemClick(object sender, EventArgs e)
        {
            var selectedRevisions = GetSelectedRevisions();
            if (selectedRevisions.Count > 2)
            {
                MessageBox.Show(this, "Select only one or two revisions. Abort.", "Archive revision");
                return;
            }

            GitRevision mainRevision = selectedRevisions.First();
            GitRevision diffRevision = null;
            if (selectedRevisions.Count == 2)
                diffRevision = selectedRevisions.Last();

            UICommands.StartArchiveDialog(this, mainRevision, diffRevision);
        }

        internal void ShowAuthorDateToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowAuthorDate = !AppSettings.ShowAuthorDate;
            ForceRefreshRevisions();
        }

        internal void OrderRevisionsByDateToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.OrderRevisionByDate = !AppSettings.OrderRevisionByDate;
            ForceRefreshRevisions();
        }

        private void CherryPickCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            var revisions = GetSelectedRevisions(SortDirection.Descending);
            UICommands.StartCherryPickDialog(this, revisions);
        }

        private void FixupCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            UICommands.StartFixupCommitDialog(this, GetRevision(LastRow));
        }

        private void SquashCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRow || LastRow < 0)
                return;

            UICommands.StartSquashCommitDialog(this, GetRevision(LastRow));
        }

        internal void ShowRelativeDateToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.RelativeDate = !AppSettings.RelativeDate;
            ForceRefreshRevisions();
        }

        private string TimeToString(DateTime time)
        {
            if (time == DateTime.MinValue || time == DateTime.MaxValue)
                return "";

            if (!AppSettings.RelativeDate)
                return string.Format("{0} {1}", time.ToShortDateString(), time.ToLongTimeString());

            return GitCommandHelpers.GetRelativeDateString(DateTime.Now, time, false);
        }

        private bool ShowUncommitedChanged()
        {
            return ShowUncommitedChangesIfPossible && AppSettings.RevisionGraphShowWorkingDirChanges;
        }

        private void UpdateGraph(GitRevision rev)
        {
            if (rev == null)
            {
                // Prune the graph and make sure the row count matches reality
                Revisions.Prune();

                if (Revisions.RowCount == 0 && ShowUncommitedChanged())
                    CheckUncommitedChanged();
                return;
            }

            var dataType = DvcsGraph.DataType.Normal;
            if (rev.Guid == FiltredCurrentCheckout)
                dataType = DvcsGraph.DataType.Active;
            else if (rev.Refs.Any())
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
            if (Module.GetUnstagedFiles().Any())
                unstagedChanges = true;
            if (Module.GetStagedFiles().Any())
                stagedChanges = true;

            // FiltredCurrentCheckout doesn't works here because only calculated after loading all revisions in SelectInitialRevision()
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

        internal void DrawNonrelativesGrayToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.RevisionGraphDrawNonRelativesGray = !AppSettings.RevisionGraphDrawNonRelativesGray;
            _revisionGridMenuCommands.TriggerPropertyChanged();
            Revisions.Refresh();
        }

        private void MessageToolStripMenuItemClick(object sender, EventArgs e)
        {
            Clipboard.SetText(GetRevision(LastRow).Message);
        }

        private void AuthorToolStripMenuItemClick(object sender, EventArgs e)
        {
            Clipboard.SetText(GetRevision(LastRow).Author);
        }

        private void DateToolStripMenuItemClick(object sender, EventArgs e)
        {
            Clipboard.SetText(GetRevision(LastRow).CommitDate.ToString());
        }

        private void HashToolStripMenuItemClick(object sender, EventArgs e)
        {
            Clipboard.SetText(GetRevision(LastRow).Guid);
        }

        private void MarkRevisionAsBadToolStripMenuItemClick(object sender, EventArgs e)
        {
            ContinueBisect(GitBisectOption.Bad);
        }

        private void MarkRevisionAsGoodToolStripMenuItemClick(object sender, EventArgs e)
        {
            ContinueBisect(GitBisectOption.Good);
        }

        private void BisectSkipRevisionToolStripMenuItemClick(object sender, EventArgs e)
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

        private void StopBisectToolStripMenuItemClick(object sender, EventArgs e)
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
            if (scripts == null)
                return;
            int lastIndex = mainContextMenu.Items.Count;
            foreach (ScriptInfo scriptInfo in scripts)
            {
                if (scriptInfo.Enabled)
                {
                    ToolStripItem item = new ToolStripMenuItem(scriptInfo.Name);
                    item.Name = item.Text + "_ownScript";
                    item.Click += RunScript;
                    if (scriptInfo.AddToRevisionGridContextMenu)
                        mainContextMenu.Items.Add(item);
                    else
                        runScriptToolStripMenuItem.DropDown.Items.Add(item);
                }
            }

            if (lastIndex != mainContextMenu.Items.Count)
                mainContextMenu.Items.Insert(lastIndex, new ToolStripSeparator());
            bool showScriptsMenu = runScriptToolStripMenuItem.DropDown.Items.Count > 0;
            runScriptToolStripMenuItem.Visible = showScriptsMenu;
        }

        private void RemoveOwnScripts()
        {
            runScriptToolStripMenuItem.DropDown.Items.Clear();
            List<ToolStripItem> list = new List<ToolStripItem>();
            foreach (ToolStripItem item in mainContextMenu.Items)
                list.Add(item);
            foreach (ToolStripItem item in list)
                if (item.Name.Contains("_ownScript"))
                    mainContextMenu.Items.RemoveByKey(item.Name);
            if (mainContextMenu.Items[mainContextMenu.Items.Count - 1] is ToolStripSeparator)
                mainContextMenu.Items.RemoveAt(mainContextMenu.Items.Count - 1);
        }

        private bool _settingsLoaded;

        private void RunScript(object sender, EventArgs e)
        {
            if (_settingsLoaded == false)
            {
                new FormSettings(UICommands).LoadSettings();
                _settingsLoaded = true;
            }
            if (ScriptRunner.RunScript(this, Module, sender.ToString(), this))
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

        internal void ShowGitNotesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.ShowGitNotes = !AppSettings.ShowGitNotes;
            ForceRefreshRevisions();
        }

        internal void ShowMergeCommitsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.ShowMergeCommits = !showMergeCommitsToolStripMenuItem.Checked;
            showMergeCommitsToolStripMenuItem.Checked = AppSettings.ShowMergeCommits;

            // hide revision graph when hiding merge commits, reasons:
            // 1, revison graph is no longer relevant, as we are not sohwing all commits
            // 2, performance hit when both revision graph and no merge commits are enabled
            if (IsGraphLayout() && !AppSettings.ShowMergeCommits)
            {
                ToggleRevisionGraph();
                SetRevisionsLayout();
            }
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

        internal void ShowRevisionGraphToolStripMenuItemClick(object sender, EventArgs e)
        {

            ToggleRevisionGraph();
            SetRevisionsLayout();
            _revisionGridMenuCommands.TriggerPropertyChanged();
            // must show MergeCommits when showing revision graph
            if (!AppSettings.ShowMergeCommits && IsGraphLayout())
            {
                AppSettings.ShowMergeCommits = true;
                showMergeCommitsToolStripMenuItem.Checked = true;
                ForceRefreshRevisions();
            }
            else
                Refresh();
        }

        private void ToggleRevisionGraph()
        {
            if (AppSettings.RevisionGraphLayout == (int) RevisionGridLayout.Small)
                AppSettings.RevisionGraphLayout = (int) RevisionGridLayout.SmallWithGraph;
            else if (AppSettings.RevisionGraphLayout == (int) RevisionGridLayout.Card)
                AppSettings.RevisionGraphLayout = (int) RevisionGridLayout.CardWithGraph;
            else if (AppSettings.RevisionGraphLayout == (int) RevisionGridLayout.LargeCard)
                AppSettings.RevisionGraphLayout = (int) RevisionGridLayout.LargeCardWithGraph;
            else if (AppSettings.RevisionGraphLayout == (int) RevisionGridLayout.SmallWithGraph)
                AppSettings.RevisionGraphLayout = (int) RevisionGridLayout.Small;
            else if (AppSettings.RevisionGraphLayout == (int) RevisionGridLayout.CardWithGraph)
                AppSettings.RevisionGraphLayout = (int) RevisionGridLayout.Card;
            else if (AppSettings.RevisionGraphLayout == (int) RevisionGridLayout.LargeCardWithGraph)
                AppSettings.RevisionGraphLayout = (int) RevisionGridLayout.LargeCard;
            else if (AppSettings.RevisionGraphLayout == (int) RevisionGridLayout.FilledBranchesSmall)
                AppSettings.RevisionGraphLayout = (int) RevisionGridLayout.FilledBranchesSmallWithGraph;
            else if (AppSettings.RevisionGraphLayout == (int) RevisionGridLayout.FilledBranchesSmallWithGraph)
                AppSettings.RevisionGraphLayout = (int) RevisionGridLayout.FilledBranchesSmall;
        }

        internal void ShowTagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AppSettings.ShowTags = !AppSettings.ShowTags;
            _revisionGridMenuCommands.TriggerPropertyChanged();
            Refresh();
        }
        
        public void ToggleRevisionCardLayout()
        {
            var layouts = new List<RevisionGridLayout>((RevisionGridLayout[])Enum.GetValues(typeof(RevisionGridLayout)));
            layouts.Sort();
            var maxLayout = (int)layouts[layouts.Count - 1];

            int nextLayout = AppSettings.RevisionGraphLayout + 1;

            if (nextLayout > maxLayout)
                nextLayout = 1;

            SetRevisionsLayout((RevisionGridLayout)nextLayout);
        }

        public void SetRevisionsLayout(RevisionGridLayout revisionGridLayout)
        {
            AppSettings.RevisionGraphLayout = (int)revisionGridLayout;
            SetRevisionsLayout();
        }

        private void SetRevisionsLayout()
        {
            _layout = Enum.IsDefined(typeof(RevisionGridLayout), AppSettings.RevisionGraphLayout)
                         ? (RevisionGridLayout)AppSettings.RevisionGraphLayout
                         : RevisionGridLayout.SmallWithGraph;

            IsCardLayout();

            NormalFont = AppSettings.Font;// new Font(Settings.Font.Name, Settings.Font.Size + 2); // SystemFonts.DefaultFont.FontFamily, SystemFonts.DefaultFont.Size + 2);

            if (IsCardLayout())
            {
                if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.Card
                    || AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.CardWithGraph)
                {
                    _rowHeigth = 45;
                }
                else
                {
                    _rowHeigth = 70;
                }

                if (_filledItemBrush == null)
                {
                    _filledItemBrush = new LinearGradientBrush(new Rectangle(0, 0, _rowHeigth, _rowHeigth),
                        Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor,
                        Color.LightBlue, 90, false);
                }
                _selectedItemBrush = _filledItemBrush;

                Revisions.ShowAuthor(!IsCardLayout());
                Revisions.SetDimensions(NodeDimension, LaneWidth, LaneLineWidth, _rowHeigth, _selectedItemBrush);

            }
            else
            {
                if (IsFilledBranchesLayout())
                {
                    using (var graphics = Graphics.FromHwnd(Handle))
                    {
                        _rowHeigth = (int)graphics.MeasureString("By", NormalFont).Height + 9;
                    }

                    _selectedItemBrush = SystemBrushes.Highlight;
                }
                else
                {
                    _rowHeigth = 25;

                    if (_filledItemBrush == null)
                    {
                        _filledItemBrush = new LinearGradientBrush(new Rectangle(0, 0, _rowHeigth, _rowHeigth),
                            Revisions.RowTemplate.DefaultCellStyle.SelectionBackColor,
                            Color.LightBlue, 90, false);
                    }
                    _selectedItemBrush = _filledItemBrush;
                }

                Revisions.ShowAuthor(!IsCardLayout());
                Revisions.SetDimensions(NodeDimension, LaneWidth, LaneLineWidth, _rowHeigth, _selectedItemBrush);
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
            return _layout == RevisionGridLayout.FilledBranchesSmall || _layout == RevisionGridLayout.FilledBranchesSmallWithGraph;
        }

        private bool IsCardLayout()
        {
            return _layout == RevisionGridLayout.Card
                   || _layout == RevisionGridLayout.CardWithGraph
                   || _layout == RevisionGridLayout.LargeCard
                   || _layout == RevisionGridLayout.LargeCardWithGraph;
        }

        internal bool IsGraphLayout()
        {
            return _layout == RevisionGridLayout.SmallWithGraph
                   || _layout == RevisionGridLayout.CardWithGraph
                   || _layout == RevisionGridLayout.LargeCardWithGraph
                   || _layout == RevisionGridLayout.FilledBranchesSmallWithGraph;
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
            PrevQuickSearch,
            SelectCurrentRevision,
            GoToCommit
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
                case Commands.ToggleDrawNonRelativesGray: DrawNonrelativesGrayToolStripMenuItemClick(null, null); break;
                case Commands.ToggleShowGitNotes: ShowGitNotesToolStripMenuItem_Click(null, null); break;
                case Commands.ToggleRevisionCardLayout: ToggleRevisionCardLayout(); break;
                case Commands.ShowAllBranches: ShowAllBranchesToolStripMenuItemClick(null, null); break;
                case Commands.SelectCurrentRevision: SetSelectedRevision(new GitRevision(Module, CurrentCheckout)); break;
                case Commands.GoToCommit: _revisionGridMenuCommands.GotoCommitExcecute(); break;
                case Commands.ShowCurrentBranchOnly: ShowCurrentBranchOnlyToolStripMenuItemClick(null, null); break;
                case Commands.GoToParent: goToParentToolStripMenuItem_Click(null, null); break;
                case Commands.GoToChild: goToChildToolStripMenuItem_Click(null, null); break;
                case Commands.ToggleHighlightSelectedBranch: ToggleHighlightSelectedBranch(); break;
                case Commands.NextQuickSearch: NextQuickSearch(true); break;
                case Commands.PrevQuickSearch: NextQuickSearch(false); break;
                default:
                {
                    bool result = base.ExecuteCommand(cmd);
                    RefreshRevisions();
                    return result;
                }
            }

            return true;
        }

        #endregion

        internal bool ExecuteCommand(Commands cmd)
        {
            return ExecuteCommand((int)cmd);
        }

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
                MessageBox.Show(_cannotHighlightSelectedBranch.Text);
                return;
            }

            HighlightSelectedBranch();
        }

        public void HighlightSelectedBranch()
        {
            var revisions = GetSelectedRevisions();
            if (revisions.Count > 0)
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
                SetSelectedRevision(r.ParentGuids[0]);
        }

        private void goToChildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = GetRevision(LastRow);
            var children = GetRevisionChildren(r.Guid);
            if (children.Any())
                SetSelectedRevision(children[0]);
        }

        private void copyToClipboardToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            var revision = GetRevision(LastRow);
            CopyToClipboardMenuHelper.AddOrUpdateTextPostfix(hashCopyToolStripMenuItem, CopyToClipboardMenuHelper.StrLimitWithElipses(revision.Guid, 15));
            CopyToClipboardMenuHelper.AddOrUpdateTextPostfix(messageCopyToolStripMenuItem, CopyToClipboardMenuHelper.StrLimitWithElipses(revision.Message, 30));
            CopyToClipboardMenuHelper.AddOrUpdateTextPostfix(authorCopyToolStripMenuItem, revision.Author);
            CopyToClipboardMenuHelper.AddOrUpdateTextPostfix(dateCopyToolStripMenuItem, revision.CommitDate.ToString());
        }

        public void GoToRef(string refName, bool showNoRevisionMsg)
        {
            string revisionGuid = Module.RevParse(refName);
            if (!string.IsNullOrEmpty(revisionGuid))
            {
                if (_isLoading)
                {
                    _initialSelectedRevision = revisionGuid;
                    Revisions.SelectedIds = null;
                    LastSelectedRows = null;
                }
                else
                {
                    SetSelectedRevision(new GitRevision(Module, revisionGuid));
                }
            }
            else if(showNoRevisionMsg)
            {
                MessageBox.Show((ParentForm as IWin32Window) ?? this, _noRevisionFoundError.Text);
            }
        }

        /// <summary>
        /// duplicated from GitExtensionsForm
        /// </summary>
        /// <param name="commandCode"></param>
        /// <returns></returns>
        private Keys GetShortcutKeys(int commandCode)
        {
            var hotkey = GetHotkeyCommand(commandCode);
            return hotkey == null ? Keys.None : hotkey.KeyData;
        }

        internal Keys GetShortcutKeys(Commands cmd)
        {
            return GetShortcutKeys((int)cmd);
        }

        /// <summary>
        /// duplicated from GitExtensionsForm
        /// </summary>
        /// <param name="commandCode"></param>
        /// <returns></returns>
        private Hotkey.HotkeyCommand GetHotkeyCommand(int commandCode)
        {
            if (Hotkeys == null)
                return null;

            return Hotkeys.FirstOrDefault(h => h.CommandCode == commandCode);
        }

        internal RevisionGridMenuCommands MenuCommands { get { return _revisionGridMenuCommands; } }
    }
}
