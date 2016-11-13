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
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.HelperDialogs;
using GitUI.Hotkey;
using GitUI.RevisionGridClasses;
using GitUI.Script;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGridClasses;
using GitUIPluginInterfaces;
using Gravatar;
using ResourceManager;

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
        private const int MaxSuperprojectRefs = 4;
        private Brush _selectedItemBrush;
        private SolidBrush _authoredRevisionsBrush;
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
        public event EventHandler<GitModuleEventArgs> GitModuleChanged;
        public event EventHandler<DoubleClickRevisionEventArgs> DoubleClickRevision;
        public event EventHandler<EventArgs> ShowFirstParentsToggled;

        private readonly RevisionGridMenuCommands _revisionGridMenuCommands;

        bool _showCurrentBranchOnlyToolStripMenuItemChecked; // refactoring
        bool _showAllBranchesToolStripMenuItemChecked; // refactoring
        bool _showFilteredBranchesToolStripMenuItemChecked; // refactoring

        private readonly ParentChildNavigationHistory _parentChildNavigationHistory;
        private readonly NavigationHistory _navigationHistory = new NavigationHistory();
        private AuthorEmailBasedRevisionHighlighting _revisionHighlighting;

        private GitRevision _baseCommitToCompare = null;
        /// <summary>
        /// Refs loaded while the latest processing of git log
        /// </summary>
        private IEnumerable<IGitRef> LatestRefs = Enumerable.Empty<IGitRef>();

        public RevisionGrid()
        {
            InitLayout();
            InitializeComponent();

            // Parent-child navigation can expect that SetSelectedRevision is always successfull since it always uses first-parents
            _parentChildNavigationHistory = new ParentChildNavigationHistory(revision => SetSelectedRevision(revision));
            _revisionHighlighting = new AuthorEmailBasedRevisionHighlighting();

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
            Revisions.KeyDown += RevisionsKeyDown;
            Revisions.MouseDown += RevisionsMouseDown;

            showMergeCommitsToolStripMenuItem.Checked = AppSettings.ShowMergeCommits;
            BranchFilter = String.Empty;
            SetShowBranches();
            QuickRevisionFilter = "";
            FixedRevisionFilter = "";
            FixedPathFilter = "";
            InMemFilterIgnoreCase = true;
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
            Revisions.IdColumn.Visible = AppSettings.ShowIds;

            IsMessageMultilineDataGridViewColumn.Width = 25;
            IsMessageMultilineDataGridViewColumn.DisplayIndex = 2;
            IsMessageMultilineDataGridViewColumn.Resizable = DataGridViewTriState.False;

            this.HotkeysEnabled = true;
            try
            {
                SetRevisionsLayout((RevisionGridLayout)AppSettings.RevisionGraphLayout);
            }
            catch
            {
                SetRevisionsLayout(RevisionGridLayout.SmallWithGraph);
            }
            compareToBaseToolStripMenuItem.Enabled = false;
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
                IdDataGridViewColumn.DefaultCellStyle.Font = new Font("Consolas", _normalFont.SizeInPoints);
                IsMessageMultilineDataGridViewColumn.DefaultCellStyle.Font = _normalFont;

                RefsFont = IsFilledBranchesLayout() ? _normalFont : new Font(_normalFont, FontStyle.Bold);
                HeadFont = new Font(_normalFont, FontStyle.Bold);
                SuperprojectFont = new Font(_normalFont, FontStyle.Underline);
            }
        }

        [Category("Filter")]
        [DefaultValue("")]
        public string QuickRevisionFilter { get; set; }
        [Category("Filter")]
        [DefaultValue("")]
        public string FixedRevisionFilter { get; set; }
        [Category("Filter")]
        [DefaultValue("")]
        public string FixedPathFilter { get; set; }
        [Category("Filter")]
        [DefaultValue(true)]
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
        public Task<SuperProjectInfo> SuperprojectCurrentCheckout { get; private set; }
        [Browsable(false)]
        public int LastRowIndex { get; private set; }

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
            get;
            set;
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
        private void RevisionsLoading(object sender, DvcsGraph.LoadingEventArgs e)
        {
            // Since this can happen on a background thread, we'll just set a
            // flag and deal with it next time we paint (a bit of a hack, but
            // it works)
            _isLoading = e.IsLoading;
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

        private void RevisionsKeyDown(object sender, KeyEventArgs e)
        {
            // BrowserBack/BrowserForward keys and additional handling for Alt+Right/Left sent by some keyboards
            if ((e.KeyCode == Keys.BrowserBack) || ((e.KeyCode == Keys.Left) && (e.Modifiers.HasFlag(Keys.Alt))))
            {
                NavigateBackward();
            }
            else if ((e.KeyCode == Keys.BrowserForward) || ((e.KeyCode == Keys.Right) && (e.Modifiers.HasFlag(Keys.Alt))))
            {
                NavigateForward();
            }
        }

        private void RevisionsMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.XButton1)
            {
                NavigateBackward();
            }
            else if (e.Button == MouseButtons.XButton2)
            {
                NavigateForward();
            }
        }

        public void ResetNavigationHistory()
        {
            var selectedRevisions = GetSelectedRevisions();
            if (selectedRevisions.Count == 1)
            {
                _navigationHistory.Push(selectedRevisions[0].Guid);
            }
            else
            {
                _navigationHistory.Clear();
            }
        }

        public void NavigateBackward()
        {
            if (_navigationHistory.CanNavigateBackward)
            {
                InternalSetSelectedRevision(_navigationHistory.NavigateBackward());
            }
        }

        public void NavigateForward()
        {
            if (_navigationHistory.CanNavigateForward)
            {
                InternalSetSelectedRevision(_navigationHistory.NavigateForward());
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
                if (parameters[1] && !filter.IsNullOrWhiteSpace())
                    if (cmdLineSafe)
                        revListArgs += "--committer=\"" + filter + "\" ";
                    else
                        inMemCommitterFilter = filter;
                if (parameters[2] && !filter.IsNullOrWhiteSpace())
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
            if (IsDisposed)
                return;

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

        // Selects row containing revision given its revisionId
        // Returns whether the required revision was found and selected
        private bool InternalSetSelectedRevision(string revision)
        {
            int index = FindRevisionIndex(revision);
            if( index >= 0 )
            {
                SetSelectedIndex(index);
                return true;
            }
            else
            {
                Revisions.ClearSelection();
                Revisions.Select();
                return false;
            }
        }

        /// <summary>
        /// Find specified revision in known to the grid revisions
        /// </summary>
        /// <param name="revision">Revision to lookup</param>
        /// <returns>Index of the found revision or -1 if nothing was found</returns>
        private int FindRevisionIndex(string revision)
        {
            int? revIdx = Revisions.TryGetRevisionIndex(revision);

            return revIdx.HasValue ? revIdx.Value : -1;
        }

        public bool SetSelectedRevision(string revision)
        {
            var found = InternalSetSelectedRevision(revision);
            if (found)
            {
                _navigationHistory.Push(revision);
            }
            return found;
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
            _parentChildNavigationHistory.RevisionsSelectionChanged();

            if (Revisions.SelectedRows.Count > 0)
                LastRowIndex = Revisions.SelectedRows[0].Index;

            SelectionTimer.Enabled = false;
            SelectionTimer.Stop();
            SelectionTimer.Enabled = true;
            SelectionTimer.Start();

            var selectedRevisions = GetSelectedRevisions();
            var firstSelectedRevision = selectedRevisions.FirstOrDefault();
            if (selectedRevisions.Count == 1 && firstSelectedRevision != null)
                _navigationHistory.Push(firstSelectedRevision.Guid);

            if (this.Parent != null && !Revisions.UpdatingVisibleRows &&
                _revisionHighlighting.ProcessRevisionSelectionChange(Module, selectedRevisions) ==
                AuthorEmailBasedRevisionHighlighting.SelectionChangeAction.RefreshUserInterface)
            {
                Refresh();
            }
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
            return Revisions.GetRevisionChildren(revision);
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
                       ((regex != null) && (value != null) && regex.Match(value).Success);
            }

            public override bool PassThru(GitRevision rev)
            {
                return CheckCondition(_AuthorFilter, _AuthorFilterRegex, rev.Author) &&
                       CheckCondition(_CommitterFilter, _CommitterFilterRegex, rev.Committer) &&
                       (CheckCondition(_MessageFilter, _MessageFilterRegex, rev.Body) ||
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

        public bool ShowRemoteRef(IGitRef r)
        {
            if (r.IsTag)
                return AppSettings.ShowSuperprojectTags;

            if (r.IsHead)
                return AppSettings.ShowSuperprojectBranches;

            if (r.IsRemote)
                return AppSettings.ShowSuperprojectRemoteBranches;

            return false;
        }

        [Browsable(false)]
        public Task<bool> UnstagedChanges { get; private set; }

        [Browsable(false)]
        public Task<bool> StagedChanges { get; private set; }

        private string _filtredCurrentCheckout;

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
                Task<SuperProjectInfo> newSuperPrjectInfo =
                    Task.Factory.StartNew(() => GetSuperprojectCheckout(ShowRemoteRef));
                newSuperPrjectInfo.ContinueWith((task) => Refresh(),
                    TaskScheduler.FromCurrentSynchronizationContext());
                //Only check for tracked files. This usually makes more sense and it performs a lot
                //better then checking for untracked files.
                // TODO: Check FiltredFileName
                Task<bool> unstagedChanges =
                    Task.Factory.StartNew(() => Module.GetUnstagedFiles().Any());
                Task<bool> stagedChanges =
                    Task.Factory.StartNew(() => Module.GetStagedFiles().Any());

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
                _filtredCurrentCheckout = null;
                _currentCheckoutParents = null;
                SuperprojectCurrentCheckout = newSuperPrjectInfo;
                UnstagedChanges = unstagedChanges;
                StagedChanges = stagedChanges;
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

                if (AppSettings.ShowFirstParent)
                    _refsOptions |= RefsFiltringOptions.FirstParent;

                if (AppSettings.ShowSimplifyByDecoration)
                    _refsOptions |= RefsFiltringOptions.SimplifyByDecoration;

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

                _revisionGraphCommand = new RevisionGraph(Module) {
                    BranchFilter = BranchFilter,
                    RefsOptions = _refsOptions,
                    RevisionFilter = _revisionFilter.GetRevisionFilter() + QuickRevisionFilter + FixedRevisionFilter,
                    PathFilter = _revisionFilter.GetPathFilter() + FixedPathFilter,
                };
                _revisionGraphCommand.Updated += GitGetCommitsCommandUpdated;
                _revisionGraphCommand.Exited += GitGetCommitsCommandExited;
                _revisionGraphCommand.Error += _revisionGraphCommand_Error;
                _revisionGraphCommand.InMemFilter = revGraphIMF;
                _revisionGraphCommand.Execute();
                LoadRevisions();
                SetRevisionsLayout();
                ResetNavigationHistory();
            }
            catch (Exception)
            {
                Error.Visible = true;
                Error.BringToFront();
                throw;
            }
        }

        public class SuperProjectInfo
        {
            public string CurrentBranch;
            public string Conflict_Base;
            public string Conflict_Remote;
            public string Conflict_Local;
            public Dictionary<string, List<IGitRef>> Refs;
        }

        private SuperProjectInfo GetSuperprojectCheckout(Func<IGitRef, bool> showRemoteRef)
        {
            if (Module.SuperprojectModule == null)
                return null;

            SuperProjectInfo spi = new SuperProjectInfo();
            var currentCheckout = Module.GetSuperprojectCurrentCheckout();
            if (currentCheckout.Key == 'U')
            {
                // return local and remote hashes
                var array = Module.SuperprojectModule.GetConflict(Module.SubmodulePath);
                spi.Conflict_Base = array.Base.Hash;
                spi.Conflict_Local = array.Local.Hash;
                spi.Conflict_Remote = array.Remote.Hash;
            }
            else
            {
                spi.CurrentBranch = currentCheckout.Value;
            }

            var refs = Module.SuperprojectModule.GetSubmoduleItemsForEachRef(Module.SubmodulePath, showRemoteRef);

            if (refs != null)
            {
                spi.Refs = refs.Where(a => a.Value != null).GroupBy(a => a.Value.Guid).ToDictionary(gr => gr.Key, gr => gr.Select(a => a.Key).ToList());
            }

            return spi;
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
            this.InvokeAsync(o =>
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

        internal bool FilterIsApplied(bool inclBranchFilter)
        {
            return (inclBranchFilter && !string.IsNullOrEmpty(BranchFilter)) ||
                   !(string.IsNullOrEmpty(QuickRevisionFilter) &&
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
                LatestRefs = _revisionGraphCommand.LatestRefs();
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
                this.InvokeAsync(o =>
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
                this.InvokeAsync(o =>
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
            string filtredCurrentCheckout = _filtredCurrentCheckout;
            string[] lastSelectedRows = LastSelectedRows ?? new string[0];

            //filter out all unavailable commits from LastSelectedRows.
            lastSelectedRows = lastSelectedRows.Where(revision => FindRevisionIndex(revision) >= 0).ToArray();

            if (lastSelectedRows.Any())
            {
                Revisions.SelectedIds = lastSelectedRows;
                LastSelectedRows = null;
            }
            else
            {
                if (!string.IsNullOrEmpty(_initialSelectedRevision))
                {
                    int index = SearchRevision(_initialSelectedRevision);
                    if (index >= 0)
                        SetSelectedIndex(index);
                }
                else
                {
                    SetSelectedRevision(filtredCurrentCheckout);
                }
            }

            if (string.IsNullOrEmpty(filtredCurrentCheckout))
                return;

            if (!Revisions.IsRevisionRelative(filtredCurrentCheckout))
            {
                HighlightBranch(filtredCurrentCheckout);
            }
        }

        private string[] GetAllParents(string initRevision)
        {
            var revListParams = "rev-list ";
            if (AppSettings.OrderRevisionByDate)
                revListParams += "--date-order ";
            else
                revListParams += "--topo-order ";
            if (AppSettings.MaxRevisionGraphCommits > 0)
                revListParams += string.Format("--max-count=\"{0}\" ", (int)AppSettings.MaxRevisionGraphCommits);

            return Module.ReadGitOutputLines(revListParams + initRevision).ToArray();
        }

        private int SearchRevision(string initRevision)
        {
            var exactIndex = Revisions.TryGetRevisionIndex(initRevision);
            if (exactIndex.HasValue)
                return exactIndex.Value;

            foreach (var parentHash in GetAllParents(initRevision))
            {
                var parentIndex = Revisions.TryGetRevisionIndex(parentHash);
                if (parentIndex.HasValue)
                    return parentIndex.Value;
            }

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

        public struct DrawRefArgs
        {
            public Graphics Graphics;
            public Rectangle CellBounds;
            public bool IsRowSelected;
            public Font RefsFont;
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
            int idColIndex = IdDataGridViewColumn.Index;
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

            var spi = SuperprojectCurrentCheckout.IsCompleted ? SuperprojectCurrentCheckout.Result : null;
            var superprojectRefs = new List<IGitRef>();
            if (spi != null && spi.Refs != null && spi.Refs.ContainsKey(revision.Guid))
            {
                superprojectRefs.AddRange(spi.Refs[revision.Guid].Where(ShowRemoteRef));
            }

            e.Handled = true;

            var drawRefArgs = new DrawRefArgs();

            drawRefArgs.Graphics = e.Graphics;
            drawRefArgs.CellBounds = e.CellBounds;

            drawRefArgs.IsRowSelected = ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected);

            if (drawRefArgs.IsRowSelected /*&& !showRevisionCards*/)
            {
                e.Graphics.FillRectangle(_selectedItemBrush, e.CellBounds);
            }
            else if (ShouldHighlightRevisionByAuthor(revision))
            {
                e.Graphics.FillRectangle(_authoredRevisionsBrush, e.CellBounds);
            }
            else
            {
                e.Graphics.FillRectangle(Brushes.White, e.CellBounds);
            }

            Color foreColor;

            if (!AppSettings.RevisionGraphDrawNonRelativesTextGray || Revisions.RowIsRelative(e.RowIndex))
            {
                foreColor = drawRefArgs.IsRowSelected && IsFilledBranchesLayout()
                    ? SystemColors.HighlightText
                    : e.CellStyle.ForeColor;
            }
            else
            {
                foreColor = drawRefArgs.IsRowSelected ? SystemColors.HighlightText : Color.Gray;
            }

            using (Brush foreBrush = new SolidBrush(foreColor))
            {
                var rowFont = NormalFont;
                if (revision.Guid == CurrentCheckout /*&& !showRevisionCards*/)
                    rowFont = HeadFont;
                else if (spi != null && spi.CurrentBranch == revision.Guid)
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

                    drawRefArgs.RefsFont = IsFilledBranchesLayout() ? rowFont : RefsFont;

                    if (spi != null)
                    {
                        if (spi.Conflict_Base == revision.Guid)
                            offset = DrawRef(drawRefArgs, offset, "Base", Color.OrangeRed, ArrowType.NotFilled);

                        if (spi.Conflict_Local == revision.Guid)
                            offset = DrawRef(drawRefArgs, offset, "Local", Color.OrangeRed, ArrowType.NotFilled);

                        if (spi.Conflict_Remote == revision.Guid)
                            offset = DrawRef(drawRefArgs, offset, "Remote", Color.OrangeRed, ArrowType.NotFilled);
                    }

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

                        foreach (var gitRef in gitRefs.Where(head => (!head.IsRemote || AppSettings.ShowRemoteBranches)))
                        {
                            if (gitRef.IsTag)
                            {
                                if (!AppSettings.ShowTags)
                                {
                                    continue;
                                }
                            }

                            Color headColor = GetHeadColor(gitRef);

                            ArrowType arrowType = gitRef.Selected ? ArrowType.Filled :
                                                  gitRef.SelectedHeadMergeSource ? ArrowType.NotFilled : ArrowType.None;

                            var superprojectRef = superprojectRefs.FirstOrDefault(spGitRef => gitRef.CompleteName == spGitRef.CompleteName);
                            if (superprojectRef != null)
                                superprojectRefs.Remove(superprojectRef);

                            string name = gitRef.Name;
                            if ( gitRef.IsTag
                                 && gitRef.IsDereference // see note on using IsDereference in CommitInfo class.
                                 && AppSettings.ShowAnnotatedTagsMessages
                                 && AppSettings.ShowIndicatorForMultilineMessage )
                            {
                                name = name + "  " + MultilineMessageIndicator;
                            }

                            offset = DrawRef(drawRefArgs, offset, name, headColor, arrowType, superprojectRef != null, true);
                        }
                    }

                    for (int i = 0; i < Math.Min(MaxSuperprojectRefs, superprojectRefs.Count); i++)
                    {
                        var gitRef = superprojectRefs[i];
                        Color headColor = GetHeadColor(gitRef);
                        var gitRefName = i < (MaxSuperprojectRefs - 1) ? gitRef.Name : "…";

                        ArrowType arrowType = gitRef.Selected ? ArrowType.Filled :
                                              gitRef.SelectedHeadMergeSource ? ArrowType.NotFilled : ArrowType.None;

                        offset = DrawRef(drawRefArgs, offset, gitRefName, headColor, arrowType, true, false);
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
                else if (columnIndex == idColIndex)
                {
                    if (!revision.IsArtificial())
                    {
                        var text = revision.Guid;
                        e.Graphics.DrawString(text, new Font("Consolas", rowFont.SizeInPoints), foreBrush,
                                              new PointF(e.CellBounds.Left, e.CellBounds.Top + 4));
                    }
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

        private bool ShouldHighlightRevisionByAuthor(GitRevision revision)
        {
            return AppSettings.HighlightAuthoredRevisions &&
                   AuthorEmailEqualityComparer.Instance.Equals(revision.AuthorEmail,
                                                               _revisionHighlighting.AuthorEmailToHighlight);
        }

        private float DrawRef(DrawRefArgs drawRefArgs, float offset, string name, Color headColor, ArrowType arrowType, bool dashedLine = false, bool fill = false)
        {
            var textColor = fill ? headColor : Lerp(headColor, Color.White, 0.5f);

            if (IsCardLayout())
            {
                using (Brush textBrush = new SolidBrush(textColor))
                {
                    string headName = name;
                    offset += drawRefArgs.Graphics.MeasureString(headName, drawRefArgs.RefsFont).Width + 6;
                    var location = new PointF(drawRefArgs.CellBounds.Right - offset, drawRefArgs.CellBounds.Top + 4);
                    var size = new SizeF(drawRefArgs.Graphics.MeasureString(headName, drawRefArgs.RefsFont).Width,
                                     drawRefArgs.Graphics.MeasureString(headName, drawRefArgs.RefsFont).Height);
                    if (fill)
                        drawRefArgs.Graphics.FillRectangle(SystemBrushes.Info, location.X - 1,
                                             location.Y - 1, size.Width + 3, size.Height + 2);

                    drawRefArgs.Graphics.DrawRectangle(SystemPens.InfoText, location.X - 1,
                                         location.Y - 1, size.Width + 3, size.Height + 2);
                    drawRefArgs.Graphics.DrawString(headName, drawRefArgs.RefsFont, textBrush, location);
                }
            }
            else
            {
                string headName = IsFilledBranchesLayout()
                               ? name
                               : string.Concat("[", name, "] ");

                var headBounds = AdjustCellBounds(drawRefArgs.CellBounds, offset);
                SizeF textSize = drawRefArgs.Graphics.MeasureString(headName, drawRefArgs.RefsFont);

                offset += textSize.Width;

                if (IsFilledBranchesLayout())
                {
                    offset += 9;

                    float extraOffset = DrawHeadBackground(drawRefArgs.IsRowSelected, drawRefArgs.Graphics,
                                                           headColor, headBounds.X,
                                                           headBounds.Y,
                                                           RoundToEven(textSize.Width + 3),
                                                           RoundToEven(textSize.Height), 3,
                                                           arrowType, dashedLine, fill);

                    offset += extraOffset;
                    headBounds.Offset((int)(extraOffset + 1), 0);
                }

                DrawColumnText(drawRefArgs.Graphics, headName, drawRefArgs.RefsFont, textColor, headBounds);
            }

            return offset;
        }

        private static readonly string MultilineMessageIndicator = "[...]";

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
                e.Value = revision.Subject;
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
                    var moduleRef = Module;
                    ThreadPool.QueueUserWorkItem(o => LoadIsMultilineMessageInfo(revision, columnIndex, e.RowIndex, Revisions.RowCount, moduleRef));
                }

                if (revision.Body != null)
                {
                    e.Value = revision.Body.TrimEnd().Contains("\n") ? MultilineMessageIndicator : "";
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
        private void LoadIsMultilineMessageInfo(GitRevision revision, int colIndex, int rowIndex, int totalRowCount, GitModule aModule)
        {
            // code taken from CommitInfo.cs
            CommitData commitData = CommitData.CreateFromRevision(revision);
            string error = "";
            if (revision.Body == null)
            {
                CommitData.UpdateCommitMessage(commitData, aModule, revision.Guid, ref error);
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

        private void DrawColumnText(Graphics gc, string text, Font font, Color color, Rectangle bounds)
        {
            TextRenderer.DrawText(gc, text, font, bounds, color, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        private static Rectangle AdjustCellBounds(Rectangle cellBounds, float offset)
        {
            return new Rectangle((int)(cellBounds.Left + offset), cellBounds.Top + 4,
                                 cellBounds.Width - (int)offset, cellBounds.Height);
        }

        private static Color GetHeadColor(IGitRef gitRef)
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

        enum ArrowType
        {
            None,
            Filled,
            NotFilled
        }

        static readonly float[] dashPattern = new float[] { 4, 4 };

        private float DrawHeadBackground(bool isSelected, Graphics graphics, Color color,
            float x, float y, float width, float height, float radius, ArrowType arrowType, bool dashedLine, bool fill)
        {
            float additionalOffset = arrowType != ArrowType.None ? GetArrowSize(height) : 0;
            width += additionalOffset;
            var oldMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            try
            {
                // shade
                if (fill)
                    using (var shadePath = CreateRoundRectPath(x + 1, y + 1, width, height, radius))
                    {
                        var shadeBrush = isSelected ? Brushes.Black : Brushes.Gray;
                        graphics.FillPath(shadeBrush, shadePath);
                    }

                using (var forePath = CreateRoundRectPath(x, y, width, height, radius))
                {
                    Color fillColor = Lerp(color, Color.White, 0.92F);

                    if (fill)
                        using (var fillBrush = new LinearGradientBrush(new RectangleF(x, y, width, height), fillColor, Lerp(fillColor, Color.White, 0.9F), 90))
                            graphics.FillPath(fillBrush, forePath);
                    else if (isSelected)
                        graphics.FillPath(Brushes.White, forePath);


                    // frame
                    using (var pen = new Pen(Lerp(color, Color.White, 0.83F)))
                    {
                        if (dashedLine)
                            pen.DashPattern = dashPattern;

                        graphics.DrawPath(pen, forePath);
                    }

                    // arrow if the head is the current branch
                    if (arrowType != ArrowType.None)
                        DrawArrow(graphics, x, y, height, color, arrowType == ArrowType.Filled);
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

        private void RevisionsDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
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
            if (selectedRevisions.Any(rev => !GitRevision.IsArtificial(rev.Guid)))
            {
                using (var form = new FormCommitDiff(UICommands, selectedRevisions[0].Guid))
                {
                    form.ShowDialog(this);
                }
            }
            else if (!selectedRevisions.Any())
            {
                UICommands.StartCompareRevisionsDialog(this);
            }
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
            if (Revisions.RowCount <= LastRowIndex || LastRowIndex < 0)
                return;

            using (var frm = new FormCreateTag(UICommands, GetRevision(LastRowIndex)))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    RefreshRevisions();
                }
            }
        }

        private void ResetCurrentBranchToHereToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRowIndex || LastRowIndex < 0)
                return;

            var frm = new FormResetCurrentBranch(UICommands, GetRevision(LastRowIndex));
            frm.ShowDialog(this);
        }

        private void CreateNewBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRowIndex || LastRowIndex < 0)
                return;

            UICommands.DoActionOnRepo(() =>
                {
                    var frm = new FormCreateBranch(UICommands, GetRevision(LastRowIndex));

                    return frm.ShowDialog(this) == DialogResult.OK;
                });
        }

        private void RevisionsMouseClick(object sender, MouseEventArgs e)
        {
            var pt = Revisions.PointToClient(Cursor.Position);
            var hti = Revisions.HitTest(pt.X, pt.Y);
            LastRowIndex = hti.RowIndex;
        }

        private void RevisionsCellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            var pt = Revisions.PointToClient(Cursor.Position);
            var hti = Revisions.HitTest(pt.X, pt.Y);

            if (LastRowIndex == hti.RowIndex)
                return;

            LastRowIndex = hti.RowIndex;
            Revisions.ClearSelection();

            if (LastRowIndex >= 0 && Revisions.Rows.Count > LastRowIndex)
                Revisions.Rows[LastRowIndex].Selected = true;
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
        internal void ShowCurrentBranchOnly_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_showCurrentBranchOnlyToolStripMenuItemChecked)
                return;

            AppSettings.BranchFilterEnabled = true;
            AppSettings.ShowCurrentBranchOnly = true;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        // internal because used by RevisonGridMenuCommands
        internal void ShowAllBranches_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_showAllBranchesToolStripMenuItemChecked)
                return;

            AppSettings.BranchFilterEnabled = false;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        // internal because used by RevisonGridMenuCommands
        internal void ShowFilteredBranches_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_showFilteredBranchesToolStripMenuItemChecked)
                return;

            AppSettings.BranchFilterEnabled = true;
            AppSettings.ShowCurrentBranchOnly = false;

            SetShowBranches();
            ForceRefreshRevisions();
        }

        internal bool ShowCurrentBranchOnly_ToolStripMenuItemChecked { get { return _showCurrentBranchOnlyToolStripMenuItemChecked; } }
        internal bool ShowAllBranches_ToolStripMenuItemChecked { get { return _showAllBranchesToolStripMenuItemChecked; } }
        internal bool ShowFilteredBranches_ToolStripMenuItemChecked { get { return _showFilteredBranchesToolStripMenuItemChecked; } }

        private void SetShowBranches()
        {
            _showAllBranchesToolStripMenuItemChecked = !AppSettings.BranchFilterEnabled;
            _showCurrentBranchOnlyToolStripMenuItemChecked =
                AppSettings.BranchFilterEnabled && AppSettings.ShowCurrentBranchOnly;
            _showFilteredBranchesToolStripMenuItemChecked =
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

            _revisionGridMenuCommands.TriggerMenuChanged(); // apply checkboxes changes also to FormBrowse main menu
        }

        private void RevertCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRowIndex || LastRowIndex < 0)
                return;

            UICommands.StartRevertCommitDialog(this, GetRevision(LastRowIndex));
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
            if (Revisions.RowCount < LastRowIndex || LastRowIndex < 0 || Revisions.RowCount == 0)
                return;

            var inTheMiddleOfBisect = Module.InTheMiddleOfBisect();
            markRevisionAsBadToolStripMenuItem.Visible = inTheMiddleOfBisect;
            markRevisionAsGoodToolStripMenuItem.Visible = inTheMiddleOfBisect;
            bisectSkipRevisionToolStripMenuItem.Visible = inTheMiddleOfBisect;
            stopBisectToolStripMenuItem.Visible = inTheMiddleOfBisect;
            bisectSeparator.Visible = inTheMiddleOfBisect;
            compareWithCurrentBranchToolStripMenuItem.Visible = Module.GetSelectedBranch().IsNotNullOrWhitespace();

            var revision = GetRevision(LastRowIndex);

            var deleteTagDropDown = new ContextMenuStrip();
            var deleteBranchDropDown = new ContextMenuStrip();
            var checkoutBranchDropDown = new ContextMenuStrip();
            var mergeBranchDropDown = new ContextMenuStrip();
            var rebaseDropDown = new ContextMenuStrip();
            var renameDropDown = new ContextMenuStrip();

            var gitRefListsForRevision = new GitRefListsForRevision(revision);
            ISet<string> ambiguosuRefs = GitRef.GetAmbiguousRefNames(LatestRefs);
            Func<IGitRef, string> getRefUnambiguousName = (IGitRef gitRef) =>
            {
                if (ambiguosuRefs.Contains(gitRef.Name))
                {
                    return gitRef.CompleteName;
                }

                return gitRef.Name;
            };

            foreach (var head in gitRefListsForRevision.AllTags)
            {
                ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                toolStripItem.Tag = head.Name;
                toolStripItem.Click += ToolStripItemClickDeleteTag;
                deleteTagDropDown.Items.Add(toolStripItem);

                toolStripItem = new ToolStripMenuItem(head.Name);
                toolStripItem.Tag = getRefUnambiguousName(head);
                toolStripItem.Click += ToolStripItemClickMergeBranch;
                mergeBranchDropDown.Items.Add(toolStripItem);
            }

            //For now there is no action that could be done on currentBranch
            bool bareRepository = Module.IsBareRepository();
            string currentBranch = Module.GetSelectedBranch();
            var allBranches = gitRefListsForRevision.AllBranches;
            var branchesWithNoIdenticalRemotes = gitRefListsForRevision.BranchesWithNoIdenticalRemotes;

            bool currentBranchPointsToRevision = false;
            foreach (var head in branchesWithNoIdenticalRemotes)
            {
                if (head.Name.Equals(currentBranch))
                    currentBranchPointsToRevision = true;
                else
                {
                    ToolStripItem toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Tag = getRefUnambiguousName(head);
                    toolStripItem.Click += ToolStripItemClickMergeBranch;
                    mergeBranchDropDown.Items.Add(toolStripItem);

                    toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Tag = getRefUnambiguousName(head);
                    toolStripItem.Click += ToolStripItemClickRebaseBranch;
                    rebaseDropDown.Items.Add(toolStripItem);
                }
            }

            //if there is no branch to rebase on, then allow user to rebase on selected commit
            if (rebaseDropDown.Items.Count == 0 && !currentBranchPointsToRevision)
            {
                ToolStripItem toolStripItem = new ToolStripMenuItem(revision.Guid);
                toolStripItem.Tag = revision.Guid;
                toolStripItem.Click += ToolStripItemClickRebaseBranch;
                rebaseDropDown.Items.Add(toolStripItem);
            }

            //if there is no branch to merge, then let user to merge selected commit into current branch
            if (mergeBranchDropDown.Items.Count == 0 && !currentBranchPointsToRevision)
            {
                ToolStripItem toolStripItem = new ToolStripMenuItem(revision.Guid);
                toolStripItem.Tag = revision.Guid;
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
                        toolStripItem.Tag = head.Name;
                        toolStripItem.Click += ToolStripItemClickDeleteBranch;
                        deleteBranchDropDown.Items.Add(toolStripItem); //Add to delete branch
                    }

                    toolStripItem = new ToolStripMenuItem(head.Name);
                    toolStripItem.Tag = head.Name;
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

            UICommands.StartDeleteTagDialog(this, toolStripItem.Tag as string);
        }

        private void ToolStripItemClickDeleteBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            UICommands.StartDeleteBranchDialog(this, toolStripItem.Tag as string);
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

            UICommands.StartMergeBranchDialog(this, toolStripItem.Tag as string);
        }

        private void ToolStripItemClickRebaseBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            UICommands.StartRebaseDialog(this, toolStripItem.Tag as string);
        }

        private void ToolStripItemClickRenameBranch(object sender, EventArgs e)
        {
            var toolStripItem = sender as ToolStripItem;

            if (toolStripItem == null)
                return;

            UICommands.StartRenameDialog(this, toolStripItem.Tag as string);
        }

        private void CheckoutRevisionToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRowIndex || LastRowIndex < 0)
                return;

            string revision = GetRevision(LastRowIndex).Guid;
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

        internal void ShowAuthorDate_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowAuthorDate = !AppSettings.ShowAuthorDate;
            ForceRefreshRevisions();
        }

        internal void OrderRevisionsByDate_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.OrderRevisionByDate = !AppSettings.OrderRevisionByDate;
            ForceRefreshRevisions();
        }

        internal void ShowRemoteBranches_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowRemoteBranches = !AppSettings.ShowRemoteBranches;
            InvalidateRevisions();
        }

        internal void ShowSuperprojectTags_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowSuperprojectTags = !AppSettings.ShowSuperprojectTags;
            ForceRefreshRevisions();
        }

        internal void ShowSuperprojectBranches_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowSuperprojectBranches = !AppSettings.ShowSuperprojectBranches;
            ForceRefreshRevisions();
        }

        internal void ShowSuperprojectRemoteBranches_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowSuperprojectRemoteBranches = !AppSettings.ShowSuperprojectRemoteBranches;
            ForceRefreshRevisions();
        }

        private void CherryPickCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            var revisions = GetSelectedRevisions(SortDirection.Descending);
            UICommands.StartCherryPickDialog(this, revisions);
        }

        private void FixupCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRowIndex || LastRowIndex < 0)
                return;

            UICommands.StartFixupCommitDialog(this, GetRevision(LastRowIndex));
        }

        private void SquashCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (Revisions.RowCount <= LastRowIndex || LastRowIndex < 0)
                return;

            UICommands.StartSquashCommitDialog(this, GetRevision(LastRowIndex));
        }

        internal void ShowRelativeDate_ToolStripMenuItemClick(object sender, EventArgs e)
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

            return LocalizationHelpers.GetRelativeDateString(DateTime.Now, time, false);
        }

        private bool ShowUncommitedChanges()
        {
            return ShowUncommitedChangesIfPossible && AppSettings.RevisionGraphShowWorkingDirChanges;
        }

        private string[] _currentCheckoutParents;

        private void UpdateGraph(GitRevision rev)
        {
            if (rev == null)
            {
                // Prune the graph and make sure the row count matches reality
                Revisions.Prune();
                return;
            }

            if (_filtredCurrentCheckout == null)
            {
                if (rev.Guid == CurrentCheckout)
                {
                    _filtredCurrentCheckout = CurrentCheckout;
                }
                else
                {
                    if (_currentCheckoutParents == null)
                    {
                        _currentCheckoutParents = GetAllParents(CurrentCheckout);
                    }
                    _filtredCurrentCheckout = _currentCheckoutParents.FirstOrDefault(parent => parent == rev.Guid);
                }
            }
            string filtredCurrentCheckout = _filtredCurrentCheckout;

            if (filtredCurrentCheckout == rev.Guid && ShowUncommitedChanges())
            {
                CheckUncommitedChanged( filtredCurrentCheckout );
            }

            var dataType = DvcsGraph.DataType.Normal;
            if (rev.Guid == filtredCurrentCheckout)
                dataType = DvcsGraph.DataType.Active;
            else if (rev.Refs.Any())
                dataType = DvcsGraph.DataType.Special;

            Revisions.Add(rev.Guid, rev.ParentGuids, dataType, rev);
        }

        private void CheckUncommitedChanged(string filtredCurrentCheckout)
        {
            if (UnstagedChanges.Result)
            {
                //Add working directory as virtual commit
                var workingDir = new GitRevision(Module, GitRevision.UnstagedGuid)
                {
                    Subject = Strings.GetCurrentUnstagedChanges(),
                    ParentGuids =
                        StagedChanges.Result
                            ? new[] { GitRevision.IndexGuid }
                            : new[] { filtredCurrentCheckout }
                };
                Revisions.Add(workingDir.Guid, workingDir.ParentGuids, DvcsGraph.DataType.Normal, workingDir);
            }

            if (StagedChanges.Result)
            {
                //Add index as virtual commit
                var index = new GitRevision(Module, GitRevision.IndexGuid)
                {
                    Subject = Strings.GetCurrentIndex(),
                    ParentGuids = new[] { filtredCurrentCheckout }
                };
                Revisions.Add(index.Guid, index.ParentGuids, DvcsGraph.DataType.Normal, index);
            }
        }

        internal void DrawNonrelativesGray_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.RevisionGraphDrawNonRelativesGray = !AppSettings.RevisionGraphDrawNonRelativesGray;
            _revisionGridMenuCommands.TriggerMenuChanged();
            Revisions.Refresh();
        }

        private void MessageToolStripMenuItemClick(object sender, EventArgs e)
        {
            Clipboard.SetText(GetRevision(LastRowIndex).Subject);
        }

        private void AuthorToolStripMenuItemClick(object sender, EventArgs e)
        {
            Clipboard.SetText(GetRevision(LastRowIndex).Author);
        }

        private void DateToolStripMenuItemClick(object sender, EventArgs e)
        {
            Clipboard.SetText(GetRevision(LastRowIndex).CommitDate.ToString());
        }

        private void HashToolStripMenuItemClick(object sender, EventArgs e)
        {
            Clipboard.SetText(GetRevision(LastRowIndex).Guid);
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
            if (Revisions.RowCount <= LastRowIndex || LastRowIndex < 0)
                return;

            FormProcess.ShowDialog(this, Module, GitCommandHelpers.ContinueBisectCmd(bisectOption, GetRevision(LastRowIndex).Guid), false);
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

        internal void ShowGitNotes_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowGitNotes = !AppSettings.ShowGitNotes;
            ForceRefreshRevisions();
        }

        internal void ShowMergeCommits_ToolStripMenuItemClick(object sender, EventArgs e)
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

        internal void ShowFirstParent_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowFirstParent = !AppSettings.ShowFirstParent;

            var handler = ShowFirstParentsToggled;
            if (handler != null)
                handler(this, e);

            ForceRefreshRevisions();
        }

        public void OnModuleChanged(object sender, GitModuleEventArgs e)
        {
            var handler = GitModuleChanged;
            if (handler != null)
                handler(this, e);
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

        internal void ShowRevisionGraph_ToolStripMenuItemClick(object sender, EventArgs e)
        {

            ToggleRevisionGraph();
            SetRevisionsLayout();
            _revisionGridMenuCommands.TriggerMenuChanged();
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
            if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.Small)
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.SmallWithGraph;
            else if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.Card)
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.CardWithGraph;
            else if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.LargeCard)
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.LargeCardWithGraph;
            else if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.SmallWithGraph)
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.Small;
            else if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.CardWithGraph)
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.Card;
            else if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.LargeCardWithGraph)
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.LargeCard;
            else if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.FilledBranchesSmall)
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.FilledBranchesSmallWithGraph;
            else if (AppSettings.RevisionGraphLayout == (int)RevisionGridLayout.FilledBranchesSmallWithGraph)
                AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.FilledBranchesSmall;
        }

        internal void ShowTags_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowTags = !AppSettings.ShowTags;
            _revisionGridMenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal void ShowIds_ToolStripMenuItemClick(object sender, EventArgs e)
        {
            AppSettings.ShowIds = !AppSettings.ShowIds;
            Revisions.IdColumn.Visible = AppSettings.ShowIds;
            _revisionGridMenuCommands.TriggerMenuChanged();
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

            SetAuthoredRevisionsBrush();

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

        private void SetAuthoredRevisionsBrush()
        {
            if (_authoredRevisionsBrush != null && _authoredRevisionsBrush.Color != AppSettings.AuthoredRevisionsColor)
            {
                _authoredRevisionsBrush.Dispose();
                _authoredRevisionsBrush = null;
            }

            if (_authoredRevisionsBrush == null)
            {
                _authoredRevisionsBrush = new SolidBrush(AppSettings.AuthoredRevisionsColor);
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
            ToggleShowMergeCommits,
            ShowAllBranches,
            ShowCurrentBranchOnly,
            ShowFilteredBranches,
            ShowRemoteBranches,
            ShowFirstParent,
            GoToParent,
            GoToChild,
            ToggleHighlightSelectedBranch,
            NextQuickSearch,
            PrevQuickSearch,
            SelectCurrentRevision,
            GoToCommit,
            NavigateBackward,
            NavigateForward,
            SelectAsBaseToCompare,
            CompareToBase
        }

        protected override bool ExecuteCommand(int cmd)
        {
            Commands command = (Commands)cmd;

            switch (command)
            {
                case Commands.ToggleRevisionGraph: ShowRevisionGraph_ToolStripMenuItemClick(null, null); break;
                case Commands.RevisionFilter: FilterToolStripMenuItemClick(null, null); break;
                case Commands.ToggleAuthorDateCommitDate: ShowAuthorDate_ToolStripMenuItemClick(null, null); break;
                case Commands.ToggleOrderRevisionsByDate: OrderRevisionsByDate_ToolStripMenuItemClick(null, null); break;
                case Commands.ToggleShowRelativeDate: ShowRelativeDate_ToolStripMenuItemClick(null, null); break;
                case Commands.ToggleDrawNonRelativesGray: DrawNonrelativesGray_ToolStripMenuItemClick(null, null); break;
                case Commands.ToggleShowGitNotes: ShowGitNotes_ToolStripMenuItemClick(null, null); break;
                case Commands.ToggleRevisionCardLayout: ToggleRevisionCardLayout(); break;
                case Commands.ToggleShowMergeCommits: ShowMergeCommits_ToolStripMenuItemClick(null, null);  break;
                case Commands.ShowAllBranches: ShowAllBranches_ToolStripMenuItemClick(null, null); break;
                case Commands.ShowCurrentBranchOnly: ShowCurrentBranchOnly_ToolStripMenuItemClick(null, null); break;
                case Commands.ShowFilteredBranches: ShowFilteredBranches_ToolStripMenuItemClick(null, null); break;
                case Commands.ShowRemoteBranches: ShowRemoteBranches_ToolStripMenuItemClick(null, null); break;
                case Commands.ShowFirstParent: ShowFirstParent_ToolStripMenuItemClick(null, null); break;
                case Commands.SelectCurrentRevision: SetSelectedRevision(new GitRevision(Module, CurrentCheckout)); break;
                case Commands.GoToCommit: _revisionGridMenuCommands.GotoCommitExcecute(); break;
                case Commands.GoToParent: goToParentToolStripMenuItem_Click(null, null); break;
                case Commands.GoToChild: goToChildToolStripMenuItem_Click(null, null); break;
                case Commands.ToggleHighlightSelectedBranch: ToggleHighlightSelectedBranch(); break;
                case Commands.NextQuickSearch: NextQuickSearch(true); break;
                case Commands.PrevQuickSearch: NextQuickSearch(false); break;
                case Commands.NavigateBackward: NavigateBackward(); break;
                case Commands.NavigateForward: NavigateForward(); break;
                case Commands.SelectAsBaseToCompare: selectAsBaseToolStripMenuItem_Click(null, null); break;
                case Commands.CompareToBase: compareToBaseToolStripMenuItem_Click(null, null); break;
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
            var r = GetRevision(LastRowIndex);
            if (r != null)
            {
                if (_parentChildNavigationHistory.HasPreviousParent)
                    _parentChildNavigationHistory.NavigateToPreviousParent(r.Guid);
                else if (r.HasParent())
                    _parentChildNavigationHistory.NavigateToParent(r.Guid, r.ParentGuids[0]);
            }
        }

        private void goToChildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = GetRevision(LastRowIndex);
            if (r != null)
            {
                var children = GetRevisionChildren(r.Guid);

                if (_parentChildNavigationHistory.HasPreviousChild)
                    _parentChildNavigationHistory.NavigateToPreviousChild(r.Guid);
                else if (children.Any())
                    _parentChildNavigationHistory.NavigateToChild(r.Guid, children[0]);
            }
        }

        private void copyToClipboardToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            var r = GetRevision(LastRowIndex);
            if (r != null)
            {
                CopyToClipboardMenuHelper.AddOrUpdateTextPostfix(hashCopyToolStripMenuItem, CopyToClipboardMenuHelper.StrLimitWithElipses(r.Guid, 15));
                CopyToClipboardMenuHelper.AddOrUpdateTextPostfix(messageCopyToolStripMenuItem, CopyToClipboardMenuHelper.StrLimitWithElipses(r.Subject, 30));
                CopyToClipboardMenuHelper.AddOrUpdateTextPostfix(authorCopyToolStripMenuItem, r.Author);
                CopyToClipboardMenuHelper.AddOrUpdateTextPostfix(dateCopyToolStripMenuItem, r.CommitDate.ToString());
            }
        }

        public void GoToRef(string refName, bool showNoRevisionMsg)
        {
            string revisionGuid = Module.RevParse(refName);
            if (!string.IsNullOrEmpty(revisionGuid))
            {
                SetSelectedRevision(new GitRevision(Module, revisionGuid));
            }
            else if (showNoRevisionMsg)
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
        private HotkeyCommand GetHotkeyCommand(int commandCode)
        {
            if (Hotkeys == null)
                return null;

            return Hotkeys.FirstOrDefault(h => h.CommandCode == commandCode);
        }

        internal RevisionGridMenuCommands MenuCommands { get { return _revisionGridMenuCommands; } }

        private void CompareToBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var headCommit = this.GetSelectedRevisions().First();
            using (var form = new FormCompareToBranch(UICommands, headCommit.Guid))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    var baseCommit = Module.RevParse(form.BranchName);
                    using (var diffForm = new FormDiff(UICommands, this, baseCommit, headCommit.Guid,
                        form.BranchName, headCommit.Subject))
                    {
                        diffForm.ShowDialog(this);
                    }
                }
            }
        }

        private void CompareWithCurrentBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var baseCommit = this.GetSelectedRevisions().First();
            var headBranch = Module.GetSelectedBranch();
            var headBranchName = Module.RevParse(headBranch);
            using (var diffForm = new FormDiff(UICommands, this, baseCommit.Guid, headBranchName,
                baseCommit.Subject, headBranch))
            {
                diffForm.ShowDialog(this);
            }
        }

        private void selectAsBaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _baseCommitToCompare = GetSelectedRevisions().First();
            compareToBaseToolStripMenuItem.Enabled = true;
        }

        private void compareToBaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var headCommit = GetSelectedRevisions().First();
            using (var diffForm = new FormDiff(UICommands, this, _baseCommitToCompare.Guid, headCommit.Guid,
                _baseCommitToCompare.Subject, headCommit.Subject))
            {
                diffForm.ShowDialog(this);
            }
        }

        private void getHelpOnHowToUseTheseFeaturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenManual();
        }

        private void OpenManual()
        {
            string url = UserManual.UserManual.UrlFor("modify_history", "using-autosquash-rebase-feature");
            OsShellUtil.OpenUrlInDefaultBrowser(url);
        }
    }
}
