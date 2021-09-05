using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Git.Commands;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Avatars;
using GitUI.BuildServerIntegration;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.HelperDialogs;
using GitUI.Hotkey;
using GitUI.Properties;
using GitUI.Script;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using TaskDialog = System.Windows.Forms.TaskDialog;
using TaskDialogButton = System.Windows.Forms.TaskDialogButton;

namespace GitUI
{
    public enum RevisionGraphDrawStyleEnum
    {
        Normal,
        DrawNonRelativesGray,
        HighlightSelected
    }

    public enum SortDirection
    {
        /// <summary>
        /// Sort from smallest to largest. For example, A to Z.
        /// </summary>
        Ascending,

        /// <summary>
        /// Sort from largest to smallest. For example, Z to A.
        /// </summary>
        Descending
    }

    [DefaultEvent("DoubleClick")]
    public sealed partial class RevisionGridControl : GitModuleControl, IScriptHostControl, ICheckRefs, IRunScript, IRevisionGridFilter
    {
        public event EventHandler<DoubleClickRevisionEventArgs>? DoubleClickRevision;
        public event EventHandler<FilterChangedEventArgs>? FilterChanged;
        public event EventHandler? RevisionGraphLoaded;
        public event EventHandler? SelectionChanged;

        /// <summary>
        ///  Occurs whenever a user toggles between the artificial and the HEAD commits
        ///  via the navigation menu item or the shortcut command.
        /// </summary>
        public event EventHandler? ToggledBetweenArtificialAndHeadCommits;

        public static readonly string HotkeySettingsName = "RevisionGrid";

        private readonly TranslationString _droppingFilesBlocked = new("For you own protection dropping more than 10 patch files at once is blocked!");
        private readonly TranslationString _cannotHighlightSelectedBranch = new("Cannot highlight selected branch when revision graph is loading.");
        private readonly TranslationString _noRevisionFoundError = new("No revision found.");
        private readonly TranslationString _baseForCompareNotSelectedError = new("Base commit for compare is not selected.");
        private readonly TranslationString _strLoading = new("Loading");
        private readonly TranslationString _rebaseConfirmTitle = new("Rebase Confirmation");
        private readonly TranslationString _rebaseBranch = new("Rebase branch.");
        private readonly TranslationString _rebaseBranchInteractive = new("Rebase branch interactively.");
        private readonly TranslationString _areYouSureRebase = new("Are you sure you want to rebase? This action will rewrite commit history.");
        private readonly TranslationString _dontShowAgain = new("Don't show me this message again.");
        private readonly TranslationString _noMergeBaseCommit = new("There is no common ancestor for the selected commits.");
        private readonly TranslationString _invalidDiffContainsFilter = new("Filter text '{0}' not valid for \"Diff contains\" filter.");

        private readonly FilterInfo _filterInfo = new();
        private readonly NavigationHistory _navigationHistory = new();
        private readonly Control _loadingControlAsync;
        private readonly Control _loadingControlSync;
        private readonly RevisionGridToolTipProvider _toolTipProvider;
        private readonly QuickSearchProvider _quickSearchProvider;
        private readonly ParentChildNavigationHistory _parentChildNavigationHistory;
        private readonly AuthorRevisionHighlighting _authorHighlighting;
        private readonly Lazy<IndexWatcher> _indexWatcher;
        private readonly BuildServerWatcher _buildServerWatcher;
        private readonly Timer _selectionTimer;
        private readonly RevisionGraphColumnProvider _revisionGraphColumnProvider;
        private readonly DataGridViewColumn _maximizedColumn;
        private DataGridViewColumn? _lastVisibleResizableColumn;
        private readonly ArtificialCommitChangeCount _workTreeChangeCount = new();
        private readonly ArtificialCommitChangeCount _indexChangeCount = new();
        private readonly CancellationTokenSequence _customDiffToolsSequence = new();

        /// <summary>
        /// The set of ref names that are ambiguous.
        /// Any refs present in this collection should be displayed using their full name.
        /// </summary>
        private IReadOnlyCollection<string>? _ambiguousRefs;

        private bool _initialLoad = true;
        private bool _isReadingRevisions = true;
        private int _updatingFilters;

        private RevisionReader? _revisionReader;
        private IDisposable? _revisionSubscription;
        private GitRevision? _baseCommitToCompare;
        private string? _rebaseOnTopOf;
        private bool _isRefreshingRevisions;
        private IReadOnlyList<ObjectId>? _selectedObjectIds;
        private SuperProjectInfo? _superprojectCurrentCheckout;
        private int _latestSelectedRowIndex;

        // NOTE internal properties aren't serialised by the WinForms designer

        internal ObjectId? CurrentCheckout { get; private set; }
        internal FilterInfo CurrentFilter => _filterInfo;
        internal bool ShowUncommittedChangesIfPossible { get; set; } = true;
        internal bool ShowBuildServerInfo { get; set; }
        internal bool DoubleClickDoesNotOpenCommitInfo { get; set; }

        /// <summary>
        /// The last selected commit in the grid (with related CommitInfo in Browse).
        /// </summary>
        internal ObjectId? SelectedId { private get; set; }

        /// <summary>
        /// The first selected, the first commit in a diff.
        /// </summary>
        internal ObjectId? FirstId { private get; set; }

        internal RevisionGridMenuCommands MenuCommands { get; }

        /// <summary>
        /// The (first) seen name for commits, for FileHistory with path filters.
        /// See BuildFilter() for limitations of commits included.
        /// The property is explicitly initialized by FileHistory.
        /// </summary>
        internal Dictionary<ObjectId, string>? FilePathByObjectId { get; set; } = null;

        public RevisionGridControl()
        {
            InitializeComponent();
            openPullRequestPageStripMenuItem.AdaptImageLightness();
            renameBranchToolStripMenuItem.AdaptImageLightness();
            InitializeComplete();

            _loadingControlAsync = new Label
            {
                Padding = DpiUtil.Scale(new Padding(7, 5, 5, 5)),
                BorderStyle = BorderStyle.None,
                ForeColor = SystemColors.InfoText,
                BackColor = SystemColors.Info,
                Font = AppSettings.Font,
                Visible = false,
                UseMnemonic = false,
                AutoSize = true,
                Text = _strLoading.Text
            };
            Controls.Add(_loadingControlAsync);

            _loadingControlSync = new WaitSpinner
            {
                BackColor = SystemColors.Window,
                Visible = false,
                Size = DpiUtil.Scale(new Size(50, 50))
            };
            Controls.Add(_loadingControlSync);

            // Delay raising the SelectionChanged event for a barely noticeable period to throttle
            // rapid changes, for example by holding the down arrow key in the revision grid.
            // 75ms is longer than the default keyboard repeat rate of 15 keypresses per second.
            _selectionTimer = new Timer(components) { Interval = 75 };
            _selectionTimer.Tick += (_, e) =>
            {
                _selectionTimer.Enabled = false;
                _selectionTimer.Stop();
                SelectionChanged?.Invoke(this, e);
            };

            _toolTipProvider = new RevisionGridToolTipProvider(_gridView);

            _quickSearchProvider = new QuickSearchProvider(_gridView, () => Module.WorkingDir);

            // Parent-child navigation can expect that SetSelectedRevision is always successful since it always uses first-parents
            _parentChildNavigationHistory = new ParentChildNavigationHistory(objectId => SetSelectedRevision(objectId));
            _authorHighlighting = new AuthorRevisionHighlighting();
            _indexWatcher = new Lazy<IndexWatcher>(() => new IndexWatcher(UICommandsSource));

            copyToClipboardToolStripMenuItem.SetRevisionFunc(() => GetSelectedRevisions());

            MenuCommands = new RevisionGridMenuCommands(this);
            MenuCommands.CreateOrUpdateMenuCommands();

            // fill View context menu from MenuCommands
            FillMenuFromMenuCommands(MenuCommands.ViewMenuCommands, viewToolStripMenuItem);

            // fill Navigate context menu from MenuCommands
            FillMenuFromMenuCommands(MenuCommands.NavigateMenuCommands, navigateToolStripMenuItem);

            // Apply checkboxes changes also to FormBrowse main menu
            MenuCommands.TriggerMenuChanged();

            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            HotkeysEnabled = true;

            _gridView.ShowCellToolTips = false;
            _gridView.AuthorHighlighting = _authorHighlighting;

            _gridView.KeyPress += (_, e) => _quickSearchProvider.OnKeyPress(e);
            _gridView.KeyUp += OnGridViewKeyUp;
            _gridView.KeyDown += OnGridViewKeyDown;
            _gridView.MouseDown += OnGridViewMouseDown;
            _gridView.CellMouseDown += OnGridViewCellMouseDown;
            _gridView.MouseDoubleClick += OnGridViewDoubleClick;
            _gridView.MouseClick += OnGridViewMouseClick;
            _gridView.MouseEnter += (_, e) => _toolTipProvider.OnCellMouseEnter();
            _gridView.CellMouseMove += (_, e) => _toolTipProvider.OnCellMouseMove(e);

            // Allow to drop patch file on revision grid
            _gridView.AllowDrop = true;
            _gridView.DragEnter += OnGridViewDragEnter;
            _gridView.DragDrop += OnGridViewDragDrop;

            _buildServerWatcher = new BuildServerWatcher(this, _gridView, () => Module);

            GitRevisionSummaryBuilder gitRevisionSummaryBuilder = new();
            _revisionGraphColumnProvider = new RevisionGraphColumnProvider(this, _gridView._revisionGraph, gitRevisionSummaryBuilder);
            _gridView.AddColumn(_revisionGraphColumnProvider);
            _gridView.AddColumn(new MessageColumnProvider(this, gitRevisionSummaryBuilder));
            _gridView.AddColumn(new AvatarColumnProvider(_gridView, AvatarService.DefaultProvider, AvatarService.CacheCleaner));
            _gridView.AddColumn(new AuthorNameColumnProvider(this, _authorHighlighting));
            _gridView.AddColumn(new DateColumnProvider(this));
            _gridView.AddColumn(new CommitIdColumnProvider(this));
            _gridView.AddColumn(_buildServerWatcher.ColumnProvider);
            _maximizedColumn = _gridView.Columns.Cast<DataGridViewColumn>()
                .FirstOrDefault(column => column.Resizable == DataGridViewTriState.True && column.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //// MenuCommands not disposable
                //// _superprojectCurrentCheckout not disposable
                //// _selectedObjectIds not disposable
                //// _baseCommitToCompare not disposable
                _revisionSubscription?.Dispose();
                _revisionReader?.Dispose();
                //// _artificialStatus not disposable
                //// _ambiguousRefs not disposable
                //// _refFilterOptions not disposable
                //// _lastVisibleResizableColumn not owned
                //// _maximizedColumn not owned
                //// _revisionGraphColumnProvider not disposable
                //// _selectionTimer handled by this.components
                _buildServerWatcher?.Dispose();
                _customDiffToolsSequence.Dispose();

                if (_indexWatcher.IsValueCreated)
                {
                    _indexWatcher.Value.Dispose();
                }

                //// _authorHighlighting not disposable
                //// _parentChildNavigationHistory not disposable
                //// _quickSearchProvider not disposable
                //// _toolTipProvider  not disposable
                //// _loadingControlSync handled by this.Controls
                //// _loadingControlAsync handled by this.Controls
                //// _navigationHistory not disposable

                components?.Dispose();

                if (!Controls.Contains(_gridView))
                {
                    // Dispose _gridView explicitly since it won't be disposed automatically
                    _gridView.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private void SetPage(Control content)
        {
            ShowLoading(false);
            for (int i = Controls.Count - 1; i >= 0; i--)
            {
                Control oldControl = Controls[i];
                if (!oldControl.Equals(content))
                {
                    Controls.RemoveAt(i);
                }
            }

            if (Controls.Count == 0)
            {
                Controls.Add(content);
            }

            RevisionGraphLoaded?.Invoke(content == _gridView ? _gridView : null, EventArgs.Empty);
        }

        internal int DrawColumnText(DataGridViewCellPaintingEventArgs e, string text, Font font, Color color, Rectangle bounds, bool useEllipsis = true)
        {
            var flags =
                TextFormatFlags.NoPrefix |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding |
                TextFormatFlags.SingleLine;

            if (useEllipsis)
            {
                flags |= TextFormatFlags.EndEllipsis;
            }

            var size = TextRenderer.MeasureText(e.Graphics, text, font, bounds.Size, flags);
            TextRenderer.DrawText(e.Graphics, text, font, bounds, color, flags);

            _toolTipProvider.SetTruncation(e.ColumnIndex, e.RowIndex, truncated: size.Width > bounds.Width);

            return size.Width;
        }

        internal IndexWatcher IndexWatcher => _indexWatcher.Value;

        internal GitRevision? LatestSelectedRevision => IsValidRevisionIndex(_latestSelectedRowIndex) ? GetRevision(_latestSelectedRowIndex) : null;

        internal bool MultiSelect
        {
            get => _gridView.MultiSelect;
            set => _gridView.MultiSelect = value;
        }

        private static void FillMenuFromMenuCommands(IEnumerable<MenuCommand> menuCommands, ToolStripDropDownItem targetItem)
        {
            foreach (var menuCommand in menuCommands)
            {
                var item = MenuCommand.CreateToolStripItem(menuCommand);

                if (item is ToolStripMenuItem menuItem)
                {
                    menuCommand.RegisterMenuItem(menuItem);
                }

                targetItem.DropDownItems.Add(item);
            }
        }

        // returns " --find-renames=..." according to app settings
        private static ArgumentString FindRenamesOpt()
        {
            return AppSettings.FollowRenamesInFileHistoryExactOnly
                ? " --find-renames=\"100%\""
                : " --find-renames";
        }

        // returns " --find-renames=... --find-copies=..." according to app settings
        private static ArgumentString FindRenamesAndCopiesOpts()
        {
            var findCopies = AppSettings.FollowRenamesInFileHistoryExactOnly
                ? " --find-copies=\"100%\""
                : " --find-copies";
            return FindRenamesOpt() + findCopies;
        }

        public void DisableFilters()
        {
            _filterInfo.DisableFilters();
        }

        public void SetAndApplyPathFilter(string filter)
        {
            _filterInfo.ByPathFilter = !string.IsNullOrWhiteSpace(filter);
            _filterInfo.PathFilter = filter;

            ForceRefreshRevisions();
        }

        private void InitiateRefAction(IReadOnlyList<IGitRef>? refs, Action<IGitRef> action, FormQuickGitRefSelector.Action actionLabel)
        {
            if (refs is null || refs.Count < 1)
            {
                return;
            }

            if (refs.Count == 1)
            {
                action(refs[0]);
                return;
            }

            using FormQuickGitRefSelector dlg = new();
            dlg.Init(actionLabel, refs);
            dlg.Location = GetQuickItemSelectorLocation();
            if (dlg.ShowDialog(ParentForm) != DialogResult.OK || dlg.SelectedRef is null)
            {
                return;
            }

            action(dlg.SelectedRef);
        }

        public Point GetQuickItemSelectorLocation()
        {
            var rect = _gridView.GetCellDisplayRectangle(0, _latestSelectedRowIndex, true);
            return PointToScreen(new Point(rect.Right, rect.Bottom));
        }

        #region Navigation

        private void ResetNavigationHistory()
        {
            var selectedRevisions = GetSelectedRevisions();
            if (selectedRevisions.Count == 1)
            {
                _navigationHistory.Push(selectedRevisions[0].ObjectId);
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
                SetSelectedRevision(_navigationHistory.NavigateBackward());
            }
        }

        public void NavigateForward()
        {
            if (_navigationHistory.CanNavigateForward)
            {
                SetSelectedRevision(_navigationHistory.NavigateForward());
            }
        }

        #endregion

        public void DisableContextMenu()
        {
            _gridView.ContextMenuStrip = null;
        }

        /// <summary>
        ///  Prevents revisions refreshes and stops <see cref="ForceRefreshRevisions"/> from executing
        ///  until <see cref="ResumeRefreshRevisions"/> is called.
        /// </summary>
        internal void SuspendRefreshRevisions() => _updatingFilters++;

        /// <summary>
        ///  Resume revisions refreshes and invokes <see cref="ForceRefreshRevisions"/>.
        /// </summary>
        internal void ResumeRefreshRevisions()
        {
            --_updatingFilters;
            Debug.Assert(_updatingFilters >= 0, $"{nameof(ResumeRefreshRevisions)} was called without matching {nameof(SuspendRefreshRevisions)}!");

            if (_updatingFilters == 0)
            {
                ForceRefreshRevisions();
            }
        }

        public void SetAndApplyBranchFilter(string filter)
        {
            // TODO: clean up and move all internals to FilterInfo

            // ShowCurrentBranchOnly depends on BranchFilterEnabled, and to show the current branch
            // both flags must be set simultaneously (check SetShowBranches implementation).
            // And since we set a filter - we can't be showing the current branch.
            _filterInfo.ShowCurrentBranchOnly = false;

            string newFilter = filter?.Trim() ?? string.Empty;
            _filterInfo.ByBranchFilter = !string.IsNullOrWhiteSpace(newFilter);
            _filterInfo.BranchFilter = newFilter;

            ForceRefreshRevisions();
        }

        /// <summary>
        ///  Applies a revision filter.
        /// </summary>
        /// <param name="filter">The filter to apply.</param>
        /// <exception cref="InvalidOperationException">Invalid 'diff contains' filter.</exception>
        public void SetAndApplyRevisionFilter(RevisionFilter filter)
        {
            bool changed = _filterInfo.Apply(filter);
            if (!changed)
            {
                return;
            }

            ForceRefreshRevisions();
        }

        public override void Refresh()
        {
            if (IsDisposed)
            {
                return;
            }

            if (_lastVisibleResizableColumn is not null)
            {
                // restore its resizable state
                _lastVisibleResizableColumn.Resizable = DataGridViewTriState.True;
            }

            _gridView.Refresh(); // columns could change their Resizable state, e.g. the BuildStatusColumnProvider

            base.Refresh();

            _toolTipProvider.Clear();

            // suppress the manual resizing of the last visible column because it will be resized when the maximized column is resized
            //// LINQ because the following did not work reliable:
            //// _lastVisibleResizableColumn = _gridView.Columns.GetLastColumn(DataGridViewElementStates.Visible | DataGridViewElementStates.Resizable, DataGridViewElementStates.None);
            _lastVisibleResizableColumn = _gridView.Columns.Cast<DataGridViewColumn>()
                .OrderBy(column => column.Index).Last(column => column.Visible && column.Resizable == DataGridViewTriState.True);
            if (_lastVisibleResizableColumn is not null)
            {
                _lastVisibleResizableColumn.Resizable = DataGridViewTriState.False;
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ShowLoading();
        }

        public new void Load()
        {
            if (!DesignMode)
            {
                ReloadHotkeys();
            }

            ForceRefreshRevisions();

            LoadCustomDifftools();
        }

        public void LoadCustomDifftools()
        {
            List<CustomDiffMergeTool> menus = new()
            {
                new(openCommitsWithDiffToolMenuItem, diffSelectedCommitsMenuItem_Click)
            };

            new CustomDiffMergeToolProvider().LoadCustomDiffMergeTools(Module, menus, components, isDiff: true, cancellationToken: _customDiffToolsSequence.Next());
        }

        public void CancelLoadCustomDifftools()
        {
            _customDiffToolsSequence.CancelCurrent();
        }

        private void SetSelectedIndex(int index, bool toggleSelection = false)
        {
            try
            {
                _gridView.Select();

                bool shallSelect;
                bool wasSelected = _gridView.Rows[index].Selected;
                if (toggleSelection)
                {
                    // Toggle the selection, but do not deselect if it is the last one.
                    shallSelect = !wasSelected || _gridView.SelectedRows.Count == 1;
                }
                else
                {
                    // Single select this line.
                    shallSelect = true;
                    if (!wasSelected || _gridView.SelectedRows.Count > 1)
                    {
                        _gridView.ClearSelection();
                        wasSelected = false;
                    }
                }

                if (wasSelected && shallSelect)
                {
                    EnsureRowVisible(_gridView, index);
                    return;
                }

                _gridView.Rows[index].Selected = shallSelect;

                // Set the first selected row as current.
                // Assigning _gridView.CurrentCell results in a single selection of that row.
                // So do not set row as current but make it visible at least.
                var selectedRows = _gridView.SelectedRows;
                var firstSelectedRow = selectedRows[0];
                if (selectedRows.Count == 1)
                {
                    _gridView.CurrentCell = firstSelectedRow.Cells[1];
                }

                EnsureRowVisible(_gridView, firstSelectedRow.Index);
            }
            catch (ArgumentException)
            {
                // Ignore if selection failed. Datagridview is not threadsafe
            }

            return;

            static void EnsureRowVisible(DataGridView gridView, int row)
            {
                int countVisible = gridView.DisplayedRowCount(includePartialRow: false);
                int firstVisible = gridView.FirstDisplayedScrollingRowIndex;
                if (row < firstVisible || firstVisible + countVisible <= row)
                {
                    gridView.FirstDisplayedScrollingRowIndex = row;
                }
            }
        }

        /// <summary>
        /// Gets the index of the revision identified by <paramref name="objectId"/>.
        /// </summary>
        /// <param name="objectId">Id of the revision to find.</param>
        /// <returns>Index of the found revision, or <c>-1</c> if not found.</returns>
        private int FindRevisionIndex(ObjectId? objectId)
        {
            return _gridView.TryGetRevisionIndex(objectId) ?? -1;
        }

        /// <summary>
        /// Selects row containing revision matching <paramref name="objectId"/>.
        /// If the revision is not found, the grid's selection is cleared.
        /// Returns whether the required revision was found and selected.
        /// </summary>
        /// <param name="objectId">Id of the revision to select.</param>
        /// <returns><c>true</c> if the required revision was found and selected, otherwise <c>false</c>.</returns>
        public bool SetSelectedRevision(ObjectId? objectId, bool toggleSelection = false)
        {
            var index = FindRevisionIndex(objectId);

            if (index >= 0 && index < _gridView.RowCount)
            {
                Validates.NotNull(objectId);
                SetSelectedIndex(index, toggleSelection);
                _navigationHistory.Push(objectId);
                return true;
            }

            _gridView.ClearSelection();
            _gridView.Select();
            return false;
        }

        public GitRevision? GetRevision(ObjectId objectId)
        {
            return _gridView.GetRevision(objectId);
        }

        private void HighlightBranch(ObjectId id)
        {
            _revisionGraphColumnProvider.RevisionGraphDrawStyle = RevisionGraphDrawStyleEnum.HighlightSelected;
            _revisionGraphColumnProvider.HighlightBranch(id);
            _gridView.Update();
        }

        public string DescribeRevision(GitRevision revision, int maxLength = 0)
        {
            var description = revision.IsArtificial
                ? string.Empty
                : revision.ObjectId.ToShortString() + ": ";

            GitRefListsForRevision gitRefListsForRevision = new(revision);

            var descriptiveRef = gitRefListsForRevision.AllBranches
                .Concat(gitRefListsForRevision.AllTags)
                .FirstOrDefault();

            description += descriptiveRef is not null
                ? GetRefUnambiguousName(descriptiveRef)
                : revision.Subject;

            if (maxLength > 0)
            {
                description = description.ShortenTo(maxLength);
            }

            return description;
        }

        /// <summary>
        /// Get the selected revisions in the grid.
        /// Note that the parents may be rewritten if a filter is applied.
        /// </summary>
        /// <param name="direction">Sort direction if set.</param>
        /// <returns>The selected revisions.</returns>
        public IReadOnlyList<GitRevision> GetSelectedRevisions(SortDirection? direction = null)
        {
            var rows = _gridView
                .SelectedRows
                .Cast<DataGridViewRow>()
                .Where(row => _gridView.RowCount > row.Index);

            if (direction.HasValue)
            {
                var d = direction.Value == SortDirection.Ascending ? 1 : -1;
                rows = rows.OrderBy(row => row.Index, (r1, r2) => d * (r1 - r2));
            }

            return rows
                .Select(row => GetRevision(row.Index))
                .WhereNotNull()
                .ToList();
        }

        private (ObjectId? firstId, GitRevision? selectedRev) getFirstAndSelected()
        {
            var revisions = GetSelectedRevisions();
            var selectedRev = revisions?.FirstOrDefault();
            var firstId = revisions is not null && revisions.Count > 1 ? revisions.LastOrDefault().ObjectId : selectedRev?.FirstParentId;

            return (firstId, selectedRev);
        }

        public IReadOnlyList<ObjectId> GetRevisionChildren(ObjectId objectId)
        {
            return _gridView.GetRevisionChildren(objectId);
        }

        private bool IsValidRevisionIndex(int index)
        {
            return index >= 0 && index < _gridView.RowCount;
        }

        private GitRevision? GetRevision(int row)
        {
            return _gridView.GetRevision(row);
        }

        /// <summary>
        /// Get the actual GitRevision from grid or use GitModule if parents may be rewritten or the commit is not in the grid.
        /// </summary>
        /// <returns>The GitRevision or null if not found</returns>
        public GitRevision? GetActualRevision(ObjectId objectId)
        {
            GitRevision revision = GetRevision(objectId);
            if (revision is not null)
            {
                return GetActualRevision(revision);
            }

            // Revision is not in grid, try get from Git
            return Module.GetRevision(objectId, shortFormat: true, loadRefs: true);
        }

        /// <summary>
        /// Get the GitRevision with the actual parents as they may be rewritten in filtered grids.
        /// </summary>
        /// <param name="revision">The revision, likely from the grid.</param>
        /// <returns>The input GitRevision if no changes and a clone with actual parents if parents are rewritte.</returns>
        public GitRevision GetActualRevision(GitRevision revision)
        {
            // Index commits must have HEAD as parent already
            if (ParentsAreRewritten && !revision.IsArtificial)
            {
                // Grid is filtered and revision may have incorrect parents
                revision = revision.Clone();
                revision.ParentIds = Module.GetParents(revision.ObjectId).ToList();
            }

            return revision;
        }

        public void RefreshRevisions()
        {
            if (IndexWatcher.IndexChanged)
            {
                ForceRefreshRevisions();
            }
        }

        public void ReloadHotkeys()
        {
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            MenuCommands.CreateOrUpdateMenuCommands();
        }

        public void ReloadTranslation()
        {
            Translator.Translate(this, AppSettings.CurrentTranslation);
        }

        private void ShowLoading(bool sync = true)
        {
            _loadingControlSync.Visible = sync;
            _loadingControlSync.Left = (ClientSize.Width - _loadingControlSync.Width) / 2;
            _loadingControlSync.Top = (ClientSize.Height - _loadingControlSync.Height) / 2;

            _loadingControlAsync.Visible = !sync;
            _loadingControlAsync.Left = ClientSize.Width - _loadingControlSync.Width;
            _loadingControlSync.BringToFront();
            _loadingControlAsync.BringToFront();
        }

        public void ForceRefreshRevisions()
        {
            ThreadHelper.AssertOnUIThread();

            if (_isRefreshingRevisions || _updatingFilters != 0)
            {
                return;
            }

            _isRefreshingRevisions = true;
            ShowLoading();

            bool firstRevisionReceived = false;
            bool headIsHandled = false;

            try
            {
                _revisionGraphColumnProvider.RevisionGraphDrawStyle = RevisionGraphDrawStyleEnum.DrawNonRelativesGray;

                // Apply checkboxes changes also to FormBrowse main menu
                MenuCommands.TriggerMenuChanged();

                FilterChanged?.Invoke(this, new FilterChangedEventArgs(_filterInfo));

                _initialLoad = true;

                _buildServerWatcher.CancelBuildStatusFetchOperation();

                DisposeRevisionReader();

                var newCurrentCheckout = Module.GetCurrentCheckout();
                GitModule capturedModule = Module;

                // If the current checkout changed, don't get the currently selected rows, select the
                // new current checkout instead.
                if (newCurrentCheckout == CurrentCheckout)
                {
                    _selectedObjectIds = _gridView.SelectedObjectIds;
                }
                else
                {
                    // This is a new checkout, so ensure the variable is cleared out.
                    _selectedObjectIds = null;
                }

                CurrentCheckout = newCurrentCheckout;
                base.Refresh();

                IndexWatcher.Reset();

                SelectInitialRevision(newCurrentCheckout);

                _isReadingRevisions = true;
                Subject<GitRevision> revisions = new();
                _revisionSubscription?.Dispose();
                _revisionSubscription = revisions
                    .ObserveOn(ThreadPoolScheduler.Instance)
                    .Subscribe(OnRevisionRead, OnRevisionReaderError, OnRevisionReadCompleted);

                _revisionReader ??= new RevisionReader();

                // Find all ambiguous refs (including stash, notes etc)
                var refs = Module.GetRefs(RefsFilter.NoFilter);
                _ambiguousRefs = GitRef.GetAmbiguousRefNames(refs);

                _gridView.SuspendLayout();
                _gridView.SelectionChanged -= OnGridViewSelectionChanged;
                _gridView.ClearSelection();
                _gridView.Clear();
                _gridView.Enabled = true;
                _gridView.Focus();
                _gridView.SelectionChanged += OnGridViewSelectionChanged;
                _gridView.ResumeLayout();

                string pathFilter = BuildPathFilter(_filterInfo.PathFilter);
                _revisionReader.Execute(
                    Module,
                    refs,
                    revisions,
                    _filterInfo.CommitsLimit,
                    _filterInfo.RefFilterOptions,
                    _filterInfo.IsShowFilteredBranchesChecked ? _filterInfo.BranchFilter : string.Empty,
                    _filterInfo.GetRevisionFilter(),
                    pathFilter);

                if (_initialLoad)
                {
                    _selectionTimer.Enabled = false;
                    _selectionTimer.Stop();
                    _selectionTimer.Enabled = true;
                    _selectionTimer.Start();

                    _initialLoad = false;
                }

                _superprojectCurrentCheckout = null;
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    var scc = await GetSuperprojectCheckoutAsync(capturedModule, noLocks: true);
                    await this.SwitchToMainThreadAsync();

                    if (_superprojectCurrentCheckout != scc)
                    {
                        _superprojectCurrentCheckout = scc;
                        Refresh();
                    }
                });
                ResetNavigationHistory();
            }
            catch
            {
                SetPage(new ErrorControl());
                throw;
            }

            return;

            string BuildPathFilter(string? path)
            {
                FilePathByObjectId?.Clear();

                if (string.IsNullOrWhiteSpace(path))
                {
                    return "";
                }

                // Manual arguments must be quoted if needed (internal paths are quoted)
                // except for simple arguments without any quotes or spaces
                path = path.Trim();
                bool multpleArgs = false;
                if (!path.Any(c => c == '"') && !path.Any(c => c == '\''))
                {
                    if (!path.Any(c => c == ' '))
                    {
                        path = path.Quote();
                    }
                    else
                    {
                        multpleArgs = true;
                    }
                }
                else if (path.Count(c => c == '"') + path.Count(c => c == '\'') > 2)
                {
                    // Basic detection of multiple quoted strings (let the Git command fail for more advanced usage)
                    multpleArgs = true;
                }

                if (!AppSettings.FollowRenamesInFileHistory

                    // The command line can be very long for folders, just ignore.
                    || path.EndsWith("/")
                    || path.EndsWith("/\"")

                    // --follow only accepts exactly one argument, error for all other
                    || multpleArgs)
                {
                    return path;
                }

                // git log --follow is not working as expected (see  https://stackoverflow.com/questions/46487476/git-log-follow-graph-skips-commits)
                //
                // But we can take a more complicated path to get reasonable results:
                //  1. use git log --follow to get all previous filenames of the file we are interested in
                //  2. use git log "list of files names" to get the history graph

                const string startOfObjectId = "????";
                GitArgumentBuilder args = new("log")
                {
                    // --name-only will list each filename on a separate line, ending with an empty line
                    // Find start of a new commit with a sequence impossible in a filename
                    $"--format=\"{startOfObjectId}%H\"",
                    "--name-only",
                    "--follow",
                    FindRenamesAndCopiesOpts(),
                    "--",
                    path
                };

                HashSet<string?> setOfFileNames = new();
                ExecutionResult result = Module.GitExecutable.Execute(args, outputEncoding: GitModule.LosslessEncoding, throwOnErrorExit: false);
                var lines = result.StandardOutput.LazySplit('\n');

                // TODO Check the exit code and warn the user that rename detection could not be done.

                ObjectId currentObjectId = null;
                foreach (var line in lines.Select(GitModule.ReEncodeFileNameFromLossless))
                {
                    if (string.IsNullOrEmpty(line))
                    {
                        // empty line after sha
                        continue;
                    }

                    if (line.StartsWith(startOfObjectId))
                    {
                        if (line.Length < ObjectId.Sha1CharCount + startOfObjectId.Length
                            || !ObjectId.TryParse(line, offset: startOfObjectId.Length, out currentObjectId))
                        {
                            // Parse error, ignore
                            currentObjectId = null;
                        }

                        continue;
                    }

                    if (currentObjectId == null)
                    {
                        // Parsing has failed, ignore
                        continue;
                    }

                    // Add only the first file to the dictionary
                    FilePathByObjectId?.TryAdd(currentObjectId, line);
                    setOfFileNames.Add(line);
                }

                // Add path in case of no matches so result is never empty
                // This also occurs if Git detects more than one path argument
                return setOfFileNames.Count == 0
                    ? path
                    : string.Join("", setOfFileNames.Select(s => @$" ""{s}"""));
            }

            void OnRevisionRead(GitRevision revision)
            {
                if (!firstRevisionReceived)
                {
                    firstRevisionReceived = true;
                    ParentsAreRewritten = _revisionReader.ParentsAreRewritten;

                    this.InvokeAsync(() => { ShowLoading(false); }).FileAndForget();
                }

                bool isCurrentCheckout = revision.ObjectId.Equals(CurrentCheckout);

                if (isCurrentCheckout)
                {
                    headIsHandled = true;
                    AddArtificialRevisions(insertAsFirst: false);
                }

                var flags = RevisionNodeFlags.None;

                if (isCurrentCheckout)
                {
                    flags = RevisionNodeFlags.CheckedOut;
                }

                if (revision.Refs.Count != 0)
                {
                    flags |= RevisionNodeFlags.HasRef;
                }

                if (_filterInfo.RefFilterOptions.HasFlag(RefFilterOptions.FirstParent))
                {
                    flags |= RevisionNodeFlags.OnlyFirstParent;
                }

                _gridView.Add(revision, flags);

                return;
            }

            void AddArtificialRevisions(bool insertAsFirst)
            {
                if (!ShowUncommittedChangesIfPossible
                    || !AppSettings.RevisionGraphShowArtificialCommits
                    || Module.IsBareRepository())
                {
                    return;
                }

                var userName = Module.GetEffectiveSetting(SettingKeyString.UserName);
                var userEmail = Module.GetEffectiveSetting(SettingKeyString.UserEmail);

                // Add working directory as an artificial commit
                GitRevision workTreeRev = new(ObjectId.WorkTreeId)
                {
                    Author = userName,
                    AuthorUnixTime = 0,
                    AuthorEmail = userEmail,
                    Committer = userName,
                    CommitUnixTime = 0,
                    CommitterEmail = userEmail,
                    Subject = ResourceManager.TranslatedStrings.Workspace,
                    ParentIds = new[] { ObjectId.IndexId },
                    HasNotes = true
                };
                _gridView.Add(workTreeRev, insertAsFirst: insertAsFirst);

                // Add index as an artificial commit
                GitRevision indexRev = new(ObjectId.IndexId)
                {
                    Author = userName,
                    AuthorUnixTime = 0,
                    AuthorEmail = userEmail,
                    Committer = userName,
                    CommitUnixTime = 0,
                    CommitterEmail = userEmail,
                    Subject = ResourceManager.TranslatedStrings.Index,
                    ParentIds = new[] { CurrentCheckout },
                    HasNotes = true
                };

                _gridView.Add(indexRev, insertAsFirst: insertAsFirst);
            }

            void OnRevisionReaderError(Exception exception)
            {
                // This has to happen on the UI thread
                this.InvokeAsync(() => SetPage(new ErrorControl()))
                    .FileAndForget();

                DisposeRevisionReader();

                // Rethrow the exception on the UI thread
                this.InvokeAsync(() => throw new AggregateException(exception))
                    .FileAndForget();
            }

            void OnRevisionReadCompleted()
            {
                if (firstRevisionReceived && !headIsHandled)
                {
                    // If parents are rewritten HEAD may not be included
                    // Insert the artificial commits first as unrelated commits so they always appear
                    // (finding the most relevant commit is tricky)
                    AddArtificialRevisions(insertAsFirst: true);
                }

                _isReadingRevisions = false;

                if (!firstRevisionReceived && !FilterIsApplied(inclBranchFilter: true))
                {
                    // This has to happen on the UI thread
                    this.InvokeAsync(
                            () =>
                            {
                                SetPage(new EmptyRepoControl(Module.IsBareRepository()));
                                _isRefreshingRevisions = false;
                            })
                        .FileAndForget();
                }
                else
                {
                    // This has to happen on the UI thread
                    ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                    {
                        await this.SwitchToMainThreadAsync();

                        _gridView.LoadingCompleted();
                        SetPage(_gridView);
                        _isRefreshingRevisions = false;
                        CheckAndRepairInitialRevision();
                        HighlightRevisionsByAuthor(GetSelectedRevisions());

                        if (ShowBuildServerInfo)
                        {
                            await _buildServerWatcher.LaunchBuildServerInfoFetchOperationAsync();
                        }
                    }).FileAndForget();
                }

                DisposeRevisionReader();
            }

            void DisposeRevisionReader()
            {
                if (_revisionReader is not null)
                {
                    _revisionReader.Dispose();
                    _revisionReader = null;
                }
            }
        }

        /// <summary>
        /// The parents for commits are replaced with the parent in the graph (as all commits may not be included)
        /// See https://git-scm.com/docs/git-log#Documentation/git-log.txt---parents
        /// </summary>
        private bool ParentsAreRewritten { get; set; } = false;

        private static async Task<SuperProjectInfo?> GetSuperprojectCheckoutAsync(GitModule gitModule, bool noLocks = false)
        {
            if (gitModule.SuperprojectModule is null)
            {
                return null;
            }

            SuperProjectInfo spi = new();
            var (code, commit) = await gitModule.GetSuperprojectCurrentCheckoutAsync().ConfigureAwait(false);
            if (code == 'U')
            {
                // return local and remote hashes
                var array = await gitModule.SuperprojectModule.GetConflictAsync(gitModule.SubmodulePath)
                    .ConfigureAwaitRunInline();
                spi.ConflictBase = array.Base.ObjectId;
                spi.ConflictLocal = array.Local.ObjectId;
                spi.ConflictRemote = array.Remote.ObjectId;
            }
            else
            {
                spi.CurrentBranch = commit;
            }

            var refs = await gitModule.SuperprojectModule.GetSubmoduleItemsForEachRefAsync(gitModule.SubmodulePath, noLocks: noLocks);

            if (refs is not null)
            {
                spi.Refs = refs
                    .Where(a => a.Value is not null && a.Value.ObjectId is not null)
                    .GroupBy(a => a.Value!.ObjectId!)
                    .ToDictionary(gr => gr.Key, gr => gr.Select(a => a.Key).AsReadOnlyList());
            }

            return spi;
        }

        internal bool FilterIsApplied(bool inclBranchFilter)
        {
            return _filterInfo.HasFilter || (inclBranchFilter && _filterInfo.IsShowFilteredBranchesChecked && !string.IsNullOrEmpty(_filterInfo.BranchFilter));
        }

        /// <summary>
        /// Select initial revision(s) in the grid.
        /// The SelectedId is the last selected commit in the grid (with related CommitInfo in Browse).
        /// The FirstId is first selected, the first commit in a diff.
        /// </summary>
        private void SelectInitialRevision(ObjectId? currentCheckout)
        {
            var toBeSelectedObjectIds = _selectedObjectIds;

            if (toBeSelectedObjectIds is null || toBeSelectedObjectIds.Count == 0)
            {
                if (SelectedId is not null)
                {
                    if (FirstId is not null)
                    {
                        toBeSelectedObjectIds = new ObjectId[] { FirstId, SelectedId };
                        FirstId = null;
                    }
                    else
                    {
                        toBeSelectedObjectIds = new ObjectId[] { SelectedId };
                    }

                    SelectedId = null;
                }
                else
                {
                    toBeSelectedObjectIds = currentCheckout is null ? Array.Empty<ObjectId>() : new ObjectId[] { currentCheckout };
                }
            }

            _gridView.ToBeSelectedObjectIds = toBeSelectedObjectIds;
            _selectedObjectIds = null;
        }

        private void CheckAndRepairInitialRevision()
        {
            // Check if there is any commit that couldn't be selected.
            if (!_gridView.ToBeSelectedObjectIds.Any())
            {
                return;
            }

            // Search for the commitid that was not selected in the grid. If not found, select the first parent.
            int index = SearchRevision(_gridView.ToBeSelectedObjectIds.First());
            if (index >= 0)
            {
                SetSelectedIndex(index);
            }

            return;

            int SearchRevision(ObjectId objectId)
            {
                // Attempt to look up an item by its ID
                if (_gridView.TryGetRevisionIndex(objectId) is int exactIndex)
                {
                    return exactIndex;
                }

                if (objectId is not null && !objectId.IsArtificial)
                {
                    // Not found, so search for its parents
                    foreach (var parentId in TryGetParents(objectId))
                    {
                        if (_gridView.TryGetRevisionIndex(parentId) is int parentIndex)
                        {
                            return parentIndex;
                        }
                    }
                }

                // Not found...
                return -1;
            }
        }

        private IEnumerable<ObjectId> TryGetParents(ObjectId objectId)
        {
            GitArgumentBuilder args = new("rev-list")
            {
                { _filterInfo.HasCommitsLimit, $"--max-count={_filterInfo.CommitsLimit}" },
                objectId
            };

            ExecutionResult result = Module.GitExecutable.Execute(args, throwOnErrorExit: false);
            foreach (var line in result.StandardOutput.LazySplit('\n'))
            {
                if (ObjectId.TryParse(line, out var parentId))
                {
                    yield return parentId;
                }
            }
        }

        #region Graph event handlers

        private void OnGridViewSelectionChanged(object sender, EventArgs e)
        {
            _parentChildNavigationHistory.RevisionsSelectionChanged();

            if (_gridView.SelectedRows.Count > 0)
            {
                _latestSelectedRowIndex = _gridView.SelectedRows[0].Index;

                // if there was selected a new revision while data is being loaded
                // then don't change the new selection when restoring selected revisions after data is loaded
                if (_isRefreshingRevisions && !_gridView.UpdatingVisibleRows)
                {
                    _selectedObjectIds = _gridView.SelectedObjectIds;
                }
            }

            _selectionTimer.Enabled = false;
            _selectionTimer.Stop();
            _selectionTimer.Enabled = true;
            _selectionTimer.Start();

            var (first, selected) = getFirstAndSelected();

            compareToWorkingDirectoryMenuItem.Enabled = selected is not null && selected.ObjectId != ObjectId.WorkTreeId;
            compareWithCurrentBranchToolStripMenuItem.Enabled = !string.IsNullOrWhiteSpace(Module.GetSelectedBranch(setDefaultIfEmpty: false));
            compareSelectedCommitsMenuItem.Enabled = first is not null && selected is not null;
            openCommitsWithDiffToolMenuItem.Enabled = first is not null && selected is not null;

            var selectedRevisions = GetSelectedRevisions();
            HighlightRevisionsByAuthor(selectedRevisions);

            if (selectedRevisions.Count == 1 && selected is not null)
            {
                _navigationHistory.Push(selected.ObjectId);
            }
        }

        private void HighlightRevisionsByAuthor(in IReadOnlyList<GitRevision> selectedRevisions)
        {
            if (Parent is not null &&
                !_gridView.UpdatingVisibleRows &&
                _authorHighlighting.ProcessRevisionSelectionChange(Module, selectedRevisions))
            {
                Refresh();
            }
        }

        private void OnGridViewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.BrowserBack:
                case Keys.Left when e.Modifiers.HasFlag(Keys.Alt):
                    {
                        NavigateBackward();
                        break;
                    }

                case Keys.BrowserForward:
                case Keys.Right when e.Modifiers.HasFlag(Keys.Alt):
                    {
                        NavigateForward();
                        break;
                    }
            }
        }

        private void OnGridViewKeyUp(object sender, KeyEventArgs e)
        {
            GitRevision? selectedRevision = LatestSelectedRevision;

            if (selectedRevision is null)
            {
                return;
            }

            // https://github.com/gitextensions/gitextensions/issues/5636
            if (e.Modifiers != Keys.None)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.F2:
                    {
                        InitiateRefAction(
                            new GitRefListsForRevision(selectedRevision).GetRenameableLocalBranches(),
                            gitRef => UICommands.StartRenameDialog(ParentForm, gitRef.Name),
                            FormQuickGitRefSelector.Action.Rename);
                        break;
                    }

                case Keys.Delete:
                    {
                        InitiateRefAction(
                            new GitRefListsForRevision(selectedRevision).GetDeletableRefs(Module.GetSelectedBranch()),
                            gitRef =>
                            {
                                if (gitRef.IsTag)
                                {
                                    UICommands.StartDeleteTagDialog(ParentForm, gitRef.Name);
                                }
                                else if (gitRef.IsRemote)
                                {
                                    UICommands.StartDeleteRemoteBranchDialog(ParentForm, gitRef.Name);
                                }
                                else
                                {
                                    UICommands.StartDeleteBranchDialog(ParentForm, gitRef.Name);
                                }
                            },
                            FormQuickGitRefSelector.Action.Delete);
                        break;
                    }
            }
        }

        private void OnGridViewMouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.XButton1: NavigateBackward(); break;
                case MouseButtons.XButton2: NavigateForward(); break;
                case MouseButtons.Left when _maximizedColumn is not null && _lastVisibleResizableColumn is not null:
                    // make resizing of the maximized column work and restore the settings afterwards
                    _gridView.MouseCaptureChanged += OnGridViewMouseCaptureChanged;
                    _maximizedColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // None must be set before Fill
                    _lastVisibleResizableColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    break;

                    void OnGridViewMouseCaptureChanged(object ignoredSender, EventArgs ignoredArgs)
                    {
                        _gridView.MouseCaptureChanged -= OnGridViewMouseCaptureChanged;
                        _lastVisibleResizableColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // None must be set before Fill
                        _maximizedColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
            }
        }

        internal bool TryGetSuperProjectInfo([NotNullWhen(returnValue: true)] out SuperProjectInfo? spi)
        {
            spi = _superprojectCurrentCheckout;
            return spi is not null;
        }

        private void OnGridViewDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            DoubleClickRevision?.Invoke(this, new DoubleClickRevisionEventArgs(GetSelectedRevisions().FirstOrDefault()));

            if (!DoubleClickDoesNotOpenCommitInfo)
            {
                ViewSelectedRevisions();
            }
        }

        private void OnGridViewMouseClick(object sender, MouseEventArgs e)
        {
            var hti = _gridView.HitTest(e.X, e.Y);
            _latestSelectedRowIndex = hti.RowIndex;
        }

        private void OnGridViewCellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.Button != MouseButtons.Right)
                {
                    return;
                }

                if (_latestSelectedRowIndex == e.RowIndex
                    && _latestSelectedRowIndex < _gridView.Rows.Count
                    && _gridView.Rows[_latestSelectedRowIndex].Selected)
                {
                    return;
                }

                _latestSelectedRowIndex = e.RowIndex;
                _gridView.ClearSelection();

                if (IsValidRevisionIndex(_latestSelectedRowIndex))
                {
                    _gridView.Rows[_latestSelectedRowIndex].Selected = true;
                }
            }
            catch (Exception)
            {
                // Checks for bounds seems not enough. See https://github.com/gitextensions/gitextensions/issues/8475
            }
        }

        #endregion

        public void ViewSelectedRevisions()
        {
            var selectedRevisions = GetSelectedRevisions();
            if (selectedRevisions.Count > 0 && !selectedRevisions[0].IsArtificial)
            {
                Form ProvideForm()
                {
                    return new FormCommitDiff(UICommands, selectedRevisions[0].ObjectId);
                }

                UICommands.ShowModelessForm(ParentForm, false, null, null, ProvideForm);
            }
            else if (!selectedRevisions.Any())
            {
                UICommands.StartCompareRevisionsDialog(ParentForm);
            }
        }

        private void CreateTagToolStripMenuItemClick(object sender, EventArgs e)
        {
            var revision = LatestSelectedRevision;

            UICommands.DoActionOnRepo(() =>
            {
                using FormCreateTag form = new(UICommands, revision?.ObjectId);
                return form.ShowDialog(ParentForm) == DialogResult.OK;
            });
        }

        private void ResetCurrentBranchToHereToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision is null)
            {
                return;
            }

            UICommands.DoActionOnRepo(() =>
            {
                using var form = FormResetCurrentBranch.Create(UICommands, LatestSelectedRevision);
                return form.ShowDialog(ParentForm) == DialogResult.OK;
            });
        }

        private void ResetAnotherBranchToHereToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision is null)
            {
                return;
            }

            UICommands.DoActionOnRepo(() =>
            {
                using var form = FormResetAnotherBranch.Create(UICommands, LatestSelectedRevision);
                return form.ShowDialog(ParentForm) == DialogResult.OK;
            });
        }

        private void CreateNewBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            var revision = LatestSelectedRevision;

            UICommands.DoActionOnRepo(() =>
            {
                using FormCreateBranch form = new(UICommands, revision?.ObjectId);
                return form.ShowDialog(ParentForm) == DialogResult.OK;
            });
        }

        public void ShowCurrentBranchOnly()
        {
            if (_filterInfo.IsShowCurrentBranchOnlyChecked)
            {
                return;
            }

            _filterInfo.ByBranchFilter = true;
            _filterInfo.ShowCurrentBranchOnly = true;
            _filterInfo.ShowReflogReferences = false;

            ForceRefreshRevisions();
        }

        public void ShowAllBranches()
        {
            if (_filterInfo.IsShowAllBranchesChecked)
            {
                return;
            }

            _filterInfo.ByBranchFilter = false;
            _filterInfo.ShowCurrentBranchOnly = false;

            ForceRefreshRevisions();
        }

        public void ShowFilteredBranches()
        {
            if (_filterInfo.IsShowFilteredBranchesChecked)
            {
                return;
            }

            _filterInfo.ByBranchFilter = true;
            _filterInfo.ShowCurrentBranchOnly = false;
            _filterInfo.ShowReflogReferences = false;

            ForceRefreshRevisions();
        }

        public void ShowRevisionFilterDialog()
        {
            using FormRevisionFilter form = new(UICommands, _filterInfo);
            if (form.ShowDialog(ParentForm) == DialogResult.OK)
            {
                ForceRefreshRevisions();
            }
        }

        private void ContextMenuOpening(object sender, CancelEventArgs e)
        {
            if (LatestSelectedRevision is null)
            {
                return;
            }

            var inTheMiddleOfBisect = Module.InTheMiddleOfBisect();
            SetEnabled(markRevisionAsBadToolStripMenuItem, inTheMiddleOfBisect);
            SetEnabled(markRevisionAsGoodToolStripMenuItem, inTheMiddleOfBisect);
            SetEnabled(bisectSkipRevisionToolStripMenuItem, inTheMiddleOfBisect);
            SetEnabled(stopBisectToolStripMenuItem, inTheMiddleOfBisect);
            SetEnabled(bisectSeparator, inTheMiddleOfBisect);

            ContextMenuStrip deleteTagDropDown = new();
            ContextMenuStrip deleteBranchDropDown = new();
            ContextMenuStrip checkoutBranchDropDown = new();
            ContextMenuStrip mergeBranchDropDown = new();
            ContextMenuStrip renameDropDown = new();

            var revision = LatestSelectedRevision;
            GitRefListsForRevision gitRefListsForRevision = new(revision);
            _rebaseOnTopOf = null;

            foreach (var head in gitRefListsForRevision.AllTags)
            {
                AddBranchMenuItem(deleteTagDropDown, head, delegate { UICommands.StartDeleteTagDialog(ParentForm, head.Name); });

                var refUnambiguousName = GetRefUnambiguousName(head);
                var mergeItem = AddBranchMenuItem(mergeBranchDropDown, head,
                    delegate { UICommands.StartMergeBranchDialog(ParentForm, refUnambiguousName); });
                mergeItem.Tag = refUnambiguousName;
            }

            // For now there is no action that could be done on currentBranch
            string currentBranchRef = GitRefName.RefsHeadsPrefix + Module.GetSelectedBranch();
            var branchesWithNoIdenticalRemotes = gitRefListsForRevision.BranchesWithNoIdenticalRemotes;

            bool currentBranchPointsToRevision = false;
            foreach (var head in branchesWithNoIdenticalRemotes)
            {
                if (head.CompleteName == currentBranchRef)
                {
                    currentBranchPointsToRevision = !revision.IsArtificial;
                }
                else
                {
                    var toolStripItem = AddBranchMenuItem(mergeBranchDropDown, head,
                        delegate { UICommands.StartMergeBranchDialog(ParentForm, GetRefUnambiguousName(head)); });

                    _rebaseOnTopOf ??= (string)toolStripItem.Tag;
                }
            }

            // if there is no branch to rebase on, then allow user to rebase on selected commit
            if (_rebaseOnTopOf is null && !currentBranchPointsToRevision)
            {
                _rebaseOnTopOf = revision.Guid;
            }

            // if there is no branch to merge, then let user to merge selected commit into current branch
            if (mergeBranchDropDown.Items.Count == 0 && !currentBranchPointsToRevision)
            {
                ToolStripMenuItem toolStripItem = new(revision.Guid);
                toolStripItem.Click += delegate { UICommands.StartMergeBranchDialog(ParentForm, revision.Guid); };
                mergeBranchDropDown.Items.Add(toolStripItem);
                _rebaseOnTopOf ??= toolStripItem.Tag as string;
            }

            var allBranches = gitRefListsForRevision.AllBranches;
            bool isHeadOfCurrentBranch = false;
            bool firstRemoteBranchForCheckout = false;
            foreach (var head in allBranches)
            {
                // skip remote branches - they can not be deleted this way
                if (!head.IsRemote)
                {
                    if (head.CompleteName == currentBranchRef)
                    {
                        isHeadOfCurrentBranch = true;
                    }
                    else
                    {
                        AddBranchMenuItem(deleteBranchDropDown, head, delegate { UICommands.StartDeleteBranchDialog(ParentForm, head.Name); });
                    }

                    AddBranchMenuItem(renameDropDown, head, delegate { UICommands.StartRenameDialog(ParentForm, head.Name); });
                }

                if (head.CompleteName != currentBranchRef)
                {
                    if (!head.IsRemote)
                    {
                        firstRemoteBranchForCheckout = true;
                    }
                    else if (firstRemoteBranchForCheckout)
                    {
                        checkoutBranchDropDown.Items.Add(new ToolStripSeparator());
                        firstRemoteBranchForCheckout = false;
                    }

                    AddBranchMenuItem(checkoutBranchDropDown, head, delegate
                    {
                        if (head.IsRemote)
                        {
                            UICommands.StartCheckoutRemoteBranch(ParentForm, head.Name);
                        }
                        else
                        {
                            UICommands.StartCheckoutBranch(ParentForm, head.Name);
                        }
                    });
                }
            }

            bool firstRemoteBranchForDelete = true;
            foreach (var head in allBranches)
            {
                if (head.IsRemote)
                {
                    if (firstRemoteBranchForDelete)
                    {
                        firstRemoteBranchForDelete = false;
                        if (deleteBranchDropDown.Items.Count > 0)
                        {
                            deleteBranchDropDown.Items.Add(new ToolStripSeparator());
                        }
                    }

                    AddBranchMenuItem(deleteBranchDropDown, head, delegate { UICommands.StartDeleteRemoteBranchDialog(ParentForm, head.Name); });
                }
            }

            bool bareRepositoryOrArtificial = Module.IsBareRepository() || revision.IsArtificial;
            deleteTagToolStripMenuItem.DropDown = deleteTagDropDown;
            SetEnabled(deleteTagToolStripMenuItem, deleteTagDropDown.Items.Count > 0);

            deleteBranchToolStripMenuItem.DropDown = deleteBranchDropDown;
            SetEnabled(deleteBranchToolStripMenuItem, deleteBranchDropDown.Items.Count > 0 && !Module.IsBareRepository());
            if (isHeadOfCurrentBranch)
            {
                deleteBranchToolStripMenuItem.Visible = true;
            }

            checkoutBranchToolStripMenuItem.DropDown = checkoutBranchDropDown;
            SetEnabled(checkoutBranchToolStripMenuItem, !bareRepositoryOrArtificial && HasEnabledItem(checkoutBranchDropDown) && !Module.IsBareRepository());

            mergeBranchToolStripMenuItem.DropDown = mergeBranchDropDown;
            SetEnabled(mergeBranchToolStripMenuItem, !bareRepositoryOrArtificial && HasEnabledItem(mergeBranchDropDown) && !Module.IsBareRepository());

            SetEnabled(rebaseOnToolStripMenuItem, !bareRepositoryOrArtificial && !Module.IsBareRepository());

            renameBranchToolStripMenuItem.DropDown = renameDropDown;
            SetEnabled(renameBranchToolStripMenuItem, renameDropDown.Items.Count > 0);

            SetEnabled(checkoutRevisionToolStripMenuItem, !bareRepositoryOrArtificial);
            SetEnabled(revertCommitToolStripMenuItem, !bareRepositoryOrArtificial);
            SetEnabled(cherryPickCommitToolStripMenuItem, !bareRepositoryOrArtificial);
            SetEnabled(manipulateCommitToolStripMenuItem, !bareRepositoryOrArtificial);

            SetEnabled(copyToClipboardToolStripMenuItem, !revision.IsArtificial);
            SetEnabled(createNewBranchToolStripMenuItem, !bareRepositoryOrArtificial);
            SetEnabled(resetCurrentBranchToHereToolStripMenuItem, !bareRepositoryOrArtificial);
            SetEnabled(resetAnotherBranchToHereToolStripMenuItem, !bareRepositoryOrArtificial);
            SetEnabled(archiveRevisionToolStripMenuItem, !revision.IsArtificial);
            SetEnabled(createTagToolStripMenuItem, !revision.IsArtificial);

            SetEnabled(openBuildReportToolStripMenuItem, !string.IsNullOrWhiteSpace(revision.BuildStatus?.Url));

            SetEnabled(openPullRequestPageStripMenuItem, !string.IsNullOrWhiteSpace(revision.BuildStatus?.PullRequestUrl));

            mainContextMenu.AddUserScripts(runScriptToolStripMenuItem, ((IRunScript)this).Execute);

            UpdateSeparators();

            return;

            void SetEnabled(ToolStripItem item, bool isEnabled)
            {
                // NOTE we have to set 'enabled' in order to filter separators because
                // setting 'visible' to true sets some internal flag, yet the property still returns
                // false, presumably because the menu item is not actually yet visible on screen.
                item.Visible = isEnabled;
                item.Enabled = isEnabled;
            }

            bool HasEnabledItem(ToolStrip item)
            {
                return item.Items.Count != 0 && item.Items.Cast<ToolStripItem>().Any(i => i.Enabled);
            }

            void UpdateSeparators()
            {
                var seenItem = false;
                foreach (var item in mainContextMenu.Items.Cast<ToolStripItem>())
                {
                    if (item is ToolStripSeparator separator)
                    {
                        separator.Visible = seenItem;
                        seenItem = false;
                    }
                    else if (item.Enabled)
                    {
                        seenItem = true;
                    }
                }
            }

            ToolStripMenuItem AddBranchMenuItem(ContextMenuStrip menu, IGitRef gitRef, EventHandler action)
            {
                ToolStripMenuItem menuItem = new(gitRef.Name)
                {
                    Image = gitRef.IsRemote ? Images.BranchRemote : Images.BranchLocal
                };
                menuItem.Click += action;
                menu.Items.Add(menuItem);
                return menuItem;
            }
        }

        private string GetRefUnambiguousName(IGitRef gitRef)
        {
            return _ambiguousRefs.Contains(gitRef.Name)
                ? gitRef.CompleteName
                : gitRef.Name;
        }

        private void ToolStripItemClickRebaseBranch(object sender, EventArgs e)
        {
            if (_rebaseOnTopOf is null)
            {
                return;
            }

            if (AppSettings.DontConfirmRebase)
            {
                UICommands.StartRebase(ParentForm, _rebaseOnTopOf);
                return;
            }

            TaskDialogPage page = new()
            {
                Text = _areYouSureRebase.Text,
                Caption = _rebaseConfirmTitle.Text,
                Heading = _rebaseBranch.Text,
                Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                Icon = TaskDialogIcon.Information,
                Verification = new TaskDialogVerificationCheckBox
                {
                    Text = _dontShowAgain.Text
                },
                SizeToContent = true
            };

            TaskDialogButton result = TaskDialog.ShowDialog(Handle, page);

            if (page.Verification.Checked)
            {
                AppSettings.DontConfirmRebase = true;
            }

            if (result == TaskDialogButton.Yes)
            {
                UICommands.StartRebase(ParentForm, _rebaseOnTopOf);
            }
        }

        private void OnRebaseInteractivelyClicked(object sender, EventArgs e)
        {
            if (_rebaseOnTopOf is null)
            {
                return;
            }

            if (AppSettings.DontConfirmRebase)
            {
                UICommands.StartInteractiveRebase(ParentForm, _rebaseOnTopOf);
                return;
            }

            TaskDialogPage page = new()
            {
                Text = _areYouSureRebase.Text,
                Caption = _rebaseConfirmTitle.Text,
                Heading = _rebaseBranchInteractive.Text,
                Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                Icon = TaskDialogIcon.Information,
                Verification = new TaskDialogVerificationCheckBox
                {
                    Text = _dontShowAgain.Text
                },
                SizeToContent = true
            };

            TaskDialogButton result = TaskDialog.ShowDialog(Handle, page);

            if (page.Verification.Checked)
            {
                AppSettings.DontConfirmRebase = true;
            }

            if (result == TaskDialogButton.Yes)
            {
                UICommands.StartInteractiveRebase(ParentForm, _rebaseOnTopOf);
            }
        }

        private void OnRebaseWithAdvOptionsClicked(object sender, EventArgs e)
        {
            if (_rebaseOnTopOf is not null)
            {
                UICommands.StartRebaseDialogWithAdvOptions(ParentForm, _rebaseOnTopOf);
            }
        }

        private void CheckoutRevisionToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision is not null)
            {
                UICommands.StartCheckoutRevisionDialog(ParentForm, LatestSelectedRevision.Guid);
            }
        }

        private void ArchiveRevisionToolStripMenuItemClick(object sender, EventArgs e)
        {
            var selectedRevisions = GetSelectedRevisions();
            if (selectedRevisions.Count > 2)
            {
                MessageBox.Show(this, "Select only one or two revisions. Abort.", "Archive revision", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            GitRevision mainRevision = selectedRevisions.FirstOrDefault();
            GitRevision? diffRevision = null;
            if (selectedRevisions.Count == 2)
            {
                diffRevision = selectedRevisions.Last();
            }

            UICommands.StartArchiveDialog(ParentForm, mainRevision, diffRevision);
        }

        internal void ToggleShowAuthorDate()
        {
            AppSettings.ShowAuthorDate = !AppSettings.ShowAuthorDate;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal void ToggleShowRemoteBranches()
        {
            AppSettings.ShowRemoteBranches = !AppSettings.ShowRemoteBranches;
            MenuCommands.TriggerMenuChanged();
            _gridView.Invalidate();
        }

        internal void ToggleShowArtificialCommits()
        {
            AppSettings.RevisionGraphShowArtificialCommits = !AppSettings.RevisionGraphShowArtificialCommits;
            ForceRefreshRevisions();
        }

        internal void ToggleAuthorDateSort()
        {
            AppSettings.SortByAuthorDate = !AppSettings.SortByAuthorDate;
            ForceRefreshRevisions();
        }

        public void ToggleShowReflogReferences()
        {
            _filterInfo.ShowReflogReferences = !_filterInfo.ShowReflogReferences;
            ForceRefreshRevisions();
        }

        internal void ToggleShowLatestStash()
        {
            AppSettings.ShowLatestStash = !AppSettings.ShowLatestStash;
            ForceRefreshRevisions();
        }

        internal void ToggleShowSuperprojectTags()
        {
            AppSettings.ShowSuperprojectTags = !AppSettings.ShowSuperprojectTags;
            ForceRefreshRevisions();
        }

        internal void ShowSuperprojectBranches_ToolStripMenuItemClick()
        {
            AppSettings.ShowSuperprojectBranches = !AppSettings.ShowSuperprojectBranches;
            ForceRefreshRevisions();
        }

        internal void ShowSuperprojectRemoteBranches_ToolStripMenuItemClick()
        {
            AppSettings.ShowSuperprojectRemoteBranches = !AppSettings.ShowSuperprojectRemoteBranches;
            ForceRefreshRevisions();
        }

        private void RevertCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            var revisions = GetSelectedRevisions(SortDirection.Ascending);
            foreach (var rev in revisions)
            {
                UICommands.StartRevertCommitDialog(ParentForm, rev);
            }
        }

        private void CherryPickCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            var revisions = GetSelectedRevisions(SortDirection.Descending);
            UICommands.StartCherryPickDialog(ParentForm, revisions);
        }

        private void FixupCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision is null)
            {
                return;
            }

            UICommands.StartFixupCommitDialog(ParentForm, LatestSelectedRevision);
        }

        private void SquashCommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (LatestSelectedRevision is null)
            {
                return;
            }

            UICommands.StartSquashCommitDialog(ParentForm, LatestSelectedRevision);
        }

        internal void ToggleShowRelativeDate(EventArgs e)
        {
            AppSettings.RelativeDate = !AppSettings.RelativeDate;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        #region Artificial commit change counters

        public ArtificialCommitChangeCount? GetChangeCount(ObjectId objectId)
        {
            return objectId == ObjectId.WorkTreeId
                ? _workTreeChangeCount
                : objectId == ObjectId.IndexId
                    ? _indexChangeCount
                    : null;
        }

        public void UpdateArtificialCommitCount(IReadOnlyList<GitItemStatus>? status)
        {
            status ??= new List<GitItemStatus>();

            UpdateChangeCount(ObjectId.WorkTreeId, status);
            UpdateChangeCount(ObjectId.IndexId, status);

            _gridView.Invalidate();
            return;

            void UpdateChangeCount(ObjectId objectId, IReadOnlyList<GitItemStatus> status)
            {
                Debug.Assert(objectId == ObjectId.WorkTreeId || objectId == ObjectId.IndexId,
                    $"Unexpected Git object id {objectId}");
                var staged = objectId == ObjectId.WorkTreeId ? StagedStatus.WorkTree : StagedStatus.Index;
                var items = status.Where(item => item.Staged == staged).ToList();

                var changeCount = GetChangeCount(objectId);
                Validates.NotNull(changeCount);
                changeCount.Update(items);
            }
        }

        #endregion

        internal void ToggleDrawNonRelativesGray()
        {
            AppSettings.RevisionGraphDrawNonRelativesGray = !AppSettings.RevisionGraphDrawNonRelativesGray;
            MenuCommands.TriggerMenuChanged();
            _gridView.Refresh();
        }

        #region Bisect

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
            if (LatestSelectedRevision is null)
            {
                return;
            }

            string? command = GitCommandHelpers.ContinueBisectCmd(bisectOption, LatestSelectedRevision.ObjectId);
            FormProcess.ShowDialog(ParentForm, arguments: command, Module.WorkingDir, input: null, useDialogSettings: false);
            RefreshRevisions();
        }

        private void StopBisectToolStripMenuItemClick(object sender, EventArgs e)
        {
            FormProcess.ShowDialog(ParentForm, arguments: GitCommandHelpers.StopBisectCmd(), Module.WorkingDir, input: null, useDialogSettings: true);
            RefreshRevisions();
        }

        #endregion

        internal void ToggleShowGitNotes()
        {
            AppSettings.ShowGitNotes = !AppSettings.ShowGitNotes;
            ForceRefreshRevisions();
        }

        internal void ToggleShowMergeCommits()
        {
            AppSettings.ShowMergeCommits = !AppSettings.ShowMergeCommits;

            // hide revision graph when hiding merge commits, reasons:
            // 1, revision graph is no longer relevant, as we are not showing all commits
            // 2, performance hit when both revision graph and no merge commits are enabled
            if (AppSettings.ShowRevisionGridGraphColumn && !AppSettings.ShowMergeCommits)
            {
                AppSettings.ShowRevisionGridGraphColumn = !AppSettings.ShowRevisionGridGraphColumn;
            }

            ForceRefreshRevisions();
        }

        internal void ToggleShowCommitBodyInRevisionGrid()
        {
            AppSettings.ShowCommitBodyInRevisionGrid = !AppSettings.ShowCommitBodyInRevisionGrid;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        public void ToggleShowFirstParent()
        {
            _filterInfo.ShowFirstParent = !_filterInfo.ShowFirstParent;
            ForceRefreshRevisions();
        }

        internal void ToggleBetweenArtificialAndHeadCommits()
        {
            GoToRef(GetIdToSelect()?.ToString(), showNoRevisionMsg: false);
            ToggledBetweenArtificialAndHeadCommits?.Invoke(this, EventArgs.Empty);
            return;

            ObjectId? GetIdToSelect()
            {
                // Try the up to 3 next possibilities in the circle: WorkTree -> Index -> Head -> WorkTree.
                // WorkTree and Index are skipped if and only if we do retrieve the ChangeCount info and HasChanges returns false.
                ObjectId? idToSelect = LatestSelectedRevision?.ObjectId;
                for (int i = 0; i < 3; ++i)
                {
                    idToSelect = GetNextIdToSelect(idToSelect);
                    if (idToSelect is not null
                        && (!idToSelect.IsArtificial || !AppSettings.ShowGitStatusForArtificialCommits || GetChangeCount(idToSelect)?.HasChanges == true))
                    {
                        if (idToSelect == CurrentCheckout && AppSettings.ShowGitStatusForArtificialCommits && _gridView.GetRevision(idToSelect) is null)
                        {
                            // HEAD is not in revision grid (filtered)
                            return ObjectId.WorkTreeId;
                        }

                        return idToSelect;
                    }
                }

                return CurrentCheckout;

                ObjectId? GetNextIdToSelect(ObjectId? id)
                    => id == ObjectId.WorkTreeId ? ObjectId.IndexId
                     : id == ObjectId.IndexId ? CurrentCheckout
                     : ObjectId.WorkTreeId;
            }
        }

        #region Column visibilities

        internal void ToggleRevisionGraphColumn()
        {
            AppSettings.ShowRevisionGridGraphColumn = !AppSettings.ShowRevisionGridGraphColumn;

            // must show MergeCommits when showing revision graph
            if (!AppSettings.ShowMergeCommits && AppSettings.ShowRevisionGridGraphColumn)
            {
                AppSettings.ShowMergeCommits = true;
                ForceRefreshRevisions();
            }
            else
            {
                Refresh();
            }

            MenuCommands.TriggerMenuChanged();
        }

        internal void ToggleAuthorAvatarColumn()
        {
            AppSettings.ShowAuthorAvatarColumn = !AppSettings.ShowAuthorAvatarColumn;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal void ToggleAuthorNameColumn()
        {
            AppSettings.ShowAuthorNameColumn = !AppSettings.ShowAuthorNameColumn;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal void ToggleDateColumn()
        {
            AppSettings.ShowDateColumn = !AppSettings.ShowDateColumn;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal void ToggleObjectIdColumn()
        {
            AppSettings.ShowObjectIdColumn = !AppSettings.ShowObjectIdColumn;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal void ToggleBuildStatusIconColumn()
        {
            ////Module.EffectiveSettings.BuildServer.ShowBuildIconInGrid.Value = !Module.EffectiveSettings.BuildServer.ShowBuildIconInGrid.Value;
            AppSettings.ShowBuildStatusIconColumn = !AppSettings.ShowBuildStatusIconColumn;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal void ToggleBuildStatusTextColumn()
        {
            ////Module.EffectiveSettings.BuildServer.ShowBuildSummaryInGrid.Value = !Module.EffectiveSettings.BuildServer.ShowBuildSummaryInGrid.Value;
            AppSettings.ShowBuildStatusTextColumn = !AppSettings.ShowBuildStatusTextColumn;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        #endregion

        internal void ToggleShowTags()
        {
            AppSettings.ShowTags = !AppSettings.ShowTags;
            MenuCommands.TriggerMenuChanged();
            Refresh();
        }

        internal CommandStatus ExecuteCommand(Command cmd)
        {
            return ExecuteCommand((int)cmd);
        }

        private void ToggleHighlightSelectedBranch()
        {
            if (_revisionReader is not null)
            {
                MessageBox.Show(_cannotHighlightSelectedBranch.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var revision = GetSelectedRevisions().FirstOrDefault();

            if (revision is not null)
            {
                HighlightBranch(revision.ObjectId);
                Refresh();
            }
        }

        private void renameBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;

            if (item.DropDown is not null && item.DropDown.Items.Count == 1)
            {
                item.DropDown.Items[0].PerformClick();
            }
        }

        private void deleteBranchTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;

            if (item.DropDown is not null && item.DropDown.Items.Count == 1)
            {
                item.DropDown.Items[0].PerformClick();
            }
        }

        private void goToParentToolStripMenuItem_Click()
        {
            var r = LatestSelectedRevision;
            if (r is not null)
            {
                if (_parentChildNavigationHistory.HasPreviousParent)
                {
                    _parentChildNavigationHistory.NavigateToPreviousParent(r.ObjectId);
                }
                else if (r.HasParent)
                {
                    _parentChildNavigationHistory.NavigateToParent(r.ObjectId, r.FirstParentId);
                }
            }
        }

        private void goToMergeBaseCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Artificial commits are replaced with HEAD
            // If only one revision is selected, compare to HEAD
            // => Fill with HEAD to if less than two normal revisions (it is OK to compare HEAD HEAD)
            var revisions = GetSelectedRevisions().Select(i => i.ObjectId).Where(i => !i.IsArtificial).ToList();
            bool hasArtificial = GetSelectedRevisions().Any(i => i.IsArtificial);
            if (revisions.Count == 0 && !hasArtificial)
            {
                return;
            }

            GitArgumentBuilder args = new("merge-base")
            {
                { revisions.Count > 2 || (revisions.Count == 2 && hasArtificial), "--octopus" },
                { revisions.Count < 1, "HEAD" },
                { revisions.Count < 2, "HEAD" },
                revisions
            };

            var mergeBaseCommitId = UICommands.GitModule.GitExecutable.GetOutput(args).TrimEnd('\n');
            if (string.IsNullOrWhiteSpace(mergeBaseCommitId))
            {
                MessageBox.Show(_noMergeBaseCommit.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SetSelectedRevision(ObjectId.Parse(mergeBaseCommitId));
        }

        private void goToChildToolStripMenuItem_Click()
        {
            var r = LatestSelectedRevision;
            if (r is not null)
            {
                var children = GetRevisionChildren(r.ObjectId);

                if (_parentChildNavigationHistory.HasPreviousChild)
                {
                    _parentChildNavigationHistory.NavigateToPreviousChild(r.ObjectId);
                }
                else if (children.Any())
                {
                    _parentChildNavigationHistory.NavigateToChild(r.ObjectId, children[0]);
                }
            }
        }

        public void GoToRef(string? refName, bool showNoRevisionMsg, bool toggleSelection = false)
        {
            if (string.IsNullOrEmpty(refName))
            {
                return;
            }

            if (DetachedHeadParser.TryParse(refName, out var sha1))
            {
                refName = sha1;
            }

            var revisionGuid = Module.RevParse(refName);
            if (revisionGuid is not null)
            {
                if (_isReadingRevisions || !SetSelectedRevision(revisionGuid, toggleSelection))
                {
                    SelectedId = revisionGuid;
                    _selectedObjectIds = null;
                }
            }
            else if (showNoRevisionMsg)
            {
                MessageBox.Show(this, _noRevisionFoundError.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        internal void SetShortcutKeys()
        {
            SetShortcutString(fixupCommitToolStripMenuItem, Command.CreateFixupCommit);
            SetShortcutString(selectAsBaseToolStripMenuItem, Command.SelectAsBaseToCompare);
            SetShortcutString(openCommitsWithDiffToolMenuItem, Command.OpenCommitsWithDifftool);
            SetShortcutString(compareToBaseToolStripMenuItem, Command.CompareToBase);
            SetShortcutString(compareToWorkingDirectoryMenuItem, Command.CompareToWorkingDirectory);
            SetShortcutString(compareSelectedCommitsMenuItem, Command.CompareSelectedCommits);
        }

        private void SetShortcutString(ToolStripMenuItem item, Command command)
        {
            item.ShortcutKeyDisplayString = GetShortcutKeys(command)
                .ToShortcutKeyDisplayString();
        }

        internal Keys GetShortcutKeys(Command cmd)
        {
            return GetShortcutKeys((int)cmd);
        }

        private void CompareToBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var headCommit = GetSelectedRevisions().FirstOrDefault();
            if (headCommit is null)
            {
                return;
            }

            using FormCompareToBranch form = new(UICommands, headCommit.ObjectId);
            if (form.ShowDialog(ParentForm) == DialogResult.OK)
            {
                Validates.NotNull(form.BranchName);
                var baseCommit = Module.RevParse(form.BranchName);
                Validates.NotNull(baseCommit);
                UICommands.ShowFormDiff(baseCommit, headCommit.ObjectId, form.BranchName, headCommit.Subject);
            }
        }

        private void CompareWithCurrentBranchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var headBranch = Module.GetSelectedBranch(setDefaultIfEmpty: false);
            if (string.IsNullOrWhiteSpace(headBranch))
            {
                MessageBox.Show(this, "No branch is currently selected", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var baseCommit = GetSelectedRevisions().FirstOrDefault();
            if (baseCommit is null)
            {
                return;
            }

            var headBranchName = Module.RevParse(headBranch);

            if (headBranchName is not null)
            {
                UICommands.ShowFormDiff(baseCommit.ObjectId, headBranchName, baseCommit.Subject, headBranch);
            }
        }

        private void selectAsBaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _baseCommitToCompare = GetSelectedRevisions().FirstOrDefault();
            compareToBaseToolStripMenuItem.Enabled = _baseCommitToCompare is not null;
        }

        private void compareToBaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_baseCommitToCompare is null)
            {
                MessageBox.Show(this, _baseForCompareNotSelectedError.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var headCommit = GetSelectedRevisions().FirstOrDefault();
            if (headCommit is null)
            {
                return;
            }

            UICommands.ShowFormDiff(_baseCommitToCompare.ObjectId, headCommit.ObjectId, _baseCommitToCompare.Subject, headCommit.Subject);
        }

        private void compareToWorkingDirectoryMenuItem_Click(object sender, EventArgs e)
        {
            var baseCommit = GetSelectedRevisions().FirstOrDefault();
            if (baseCommit is null)
            {
                return;
            }

            if (baseCommit.ObjectId == ObjectId.WorkTreeId)
            {
                MessageBox.Show(this, "Cannot diff working directory to itself", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            UICommands.ShowFormDiff(baseCommit.ObjectId, ObjectId.WorkTreeId, baseCommit.Subject, "Working directory");
        }

        private void compareSelectedCommitsMenuItem_Click(object sender, EventArgs e)
        {
            var (first, selected) = getFirstAndSelected();

            if (selected is not null && first is not null)
            {
                var firstRev = GetRevision(first);
                if (firstRev is not null)
                {
                    UICommands.ShowFormDiff(first, selected.ObjectId, firstRev.Subject, selected.Subject);
                }
            }
            else
            {
                MessageBox.Show(this, "You must have two commits selected to compare", TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void diffSelectedCommitsMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item?.DropDownItems != null)
            {
                // "main menu" clicked, cancel dropdown manually, invoke default difftool
                item.HideDropDown();
            }

            var toolName = item?.Tag as string;
            DiffSelectedCommitsWithDifftool(toolName);
        }

        public void DiffSelectedCommitsWithDifftool(string? customTool = null)
        {
            var (first, selected) = getFirstAndSelected();
            if (selected is not null)
            {
                Module.OpenWithDifftoolDirDiff(first?.ToString(), selected.ObjectId.ToString(), customTool: customTool);
            }
        }

        private void getHelpOnHowToUseTheseFeaturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OsShellUtil.OpenUrlInDefaultBrowser(
                UserManual.UserManual.UrlFor("modify_history", "using-autosquash-rebase-feature"));
        }

        private void openBuildReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var revision = GetSelectedRevisions().FirstOrDefault();

            if (revision is not null && !string.IsNullOrWhiteSpace(revision.BuildStatus?.Url))
            {
                OsShellUtil.OpenUrlInDefaultBrowser(revision.BuildStatus.Url);
            }
        }

        private void openPullRequestPageStripMenuItem_Click(object sender, EventArgs e)
        {
            var revision = GetSelectedRevisions().FirstOrDefault();

            if (revision is not null && !string.IsNullOrWhiteSpace(revision.BuildStatus?.PullRequestUrl))
            {
                OsShellUtil.OpenUrlInDefaultBrowser(revision.BuildStatus.PullRequestUrl);
            }
        }

        private void editCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LaunchRebase("e");
        }

        private void rewordCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LaunchRebase("r");
        }

        private void LaunchRebase(string command)
        {
            if (LatestSelectedRevision is null)
            {
                return;
            }

            string rebaseCmd = GitCommandHelpers.RebaseCmd(
                LatestSelectedRevision.FirstParentId?.ToString(), interactive: true, preserveMerges: false,
                autosquash: false, autoStash: true, ignoreDate: false, committerDateIsAuthorDate: false);

            using (FormProcess formProcess = new(UICommands, arguments: rebaseCmd, Module.WorkingDir, input: null, useDialogSettings: true))
            {
                formProcess.ProcessEnvVariables.Add("GIT_SEQUENCE_EDITOR", string.Format("sed -i -re '0,/pick/s//{0}/'", command));
                formProcess.ShowDialog(ParentForm);
            }

            RefreshRevisions();
        }

        #region Drag/drop patch files on revision grid

        private void OnGridViewDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is Array fileNameArray)
            {
                if (fileNameArray.Length > 10)
                {
                    // Some users need to be protected against themselves!
                    MessageBox.Show(this, _droppingFilesBlocked.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                foreach (object fileNameObject in fileNameArray)
                {
                    var fileName = fileNameObject as string;

                    if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".patch", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Start apply patch dialog for each dropped patch file...
                        UICommands.StartApplyPatchDialog(ParentForm, fileName);
                    }
                }
            }
        }

        private static void OnGridViewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is Array fileNameArray)
            {
                foreach (object fileNameObject in fileNameArray)
                {
                    var fileName = fileNameObject as string;

                    if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".patch", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Allow drop (copy, not move) patch files
                        e.Effect = DragDropEffects.Copy;
                    }
                    else
                    {
                        // When a non-patch file is dragged, do not allow it
                        e.Effect = DragDropEffects.None;
                        return;
                    }
                }
            }
        }

        #endregion

        #region Hotkey commands

        protected override CommandStatus ExecuteCommand(int cmd)
        {
            switch ((Command)cmd)
            {
                case Command.ToggleRevisionGraph: ToggleRevisionGraphColumn(); break;
                case Command.RevisionFilter: ShowRevisionFilterDialog(); break;
                case Command.ToggleAuthorDateCommitDate: ToggleShowAuthorDate(); break;
                case Command.ToggleShowRelativeDate: ToggleShowRelativeDate(EventArgs.Empty); break;
                case Command.ToggleDrawNonRelativesGray: ToggleDrawNonRelativesGray(); break;
                case Command.ToggleShowGitNotes: ToggleShowGitNotes(); break;
                case Command.ToggleShowMergeCommits: ToggleShowMergeCommits(); break;
                case Command.ToggleShowTags: ToggleShowTags(); break;
                case Command.ShowAllBranches: ShowAllBranches(); break;
                case Command.ShowCurrentBranchOnly: ShowCurrentBranchOnly(); break;
                case Command.ShowFilteredBranches: ShowFilteredBranches(); break;
                case Command.ShowReflogReferences: ToggleShowReflogReferences(); break;
                case Command.ShowRemoteBranches: ToggleShowRemoteBranches(); break;
                case Command.ShowFirstParent: ToggleShowFirstParent(); break;
                case Command.ToggleBetweenArtificialAndHeadCommits: ToggleBetweenArtificialAndHeadCommits(); break;
                case Command.SelectCurrentRevision: SetSelectedRevision(CurrentCheckout); break;
                case Command.GoToCommit: MenuCommands.GotoCommitExecute(); break;
                case Command.GoToParent: goToParentToolStripMenuItem_Click(); break;
                case Command.GoToMergeBaseCommit: goToMergeBaseCommitToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case Command.GoToChild: goToChildToolStripMenuItem_Click(); break;
                case Command.ToggleHighlightSelectedBranch: ToggleHighlightSelectedBranch(); break;
                case Command.NextQuickSearch: _quickSearchProvider.NextResult(down: true); break;
                case Command.PrevQuickSearch: _quickSearchProvider.NextResult(down: false); break;
                case Command.NavigateBackward: NavigateBackward(); break;
                case Command.NavigateForward: NavigateForward(); break;
                case Command.SelectAsBaseToCompare: selectAsBaseToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case Command.CompareToBase: compareToBaseToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case Command.CreateFixupCommit: FixupCommitToolStripMenuItemClick(this, EventArgs.Empty); break;
                case Command.OpenCommitsWithDifftool: DiffSelectedCommitsWithDifftool(); break;
                case Command.CompareToWorkingDirectory: compareToWorkingDirectoryMenuItem_Click(this, EventArgs.Empty); break;
                case Command.CompareToCurrentBranch: CompareWithCurrentBranchToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case Command.CompareToBranch: CompareToBranchToolStripMenuItem_Click(this, EventArgs.Empty); break;
                case Command.CompareSelectedCommits: compareSelectedCommitsMenuItem_Click(this, EventArgs.Empty); break;
                default:
                    {
                        var result = base.ExecuteCommand(cmd);
                        if (result.NeedsGridRefresh)
                        {
                            RefreshRevisions();
                        }

                        return result;
                    }
            }

            return true;
        }
        #endregion

        #region IScriptHostControl

        GitRevision IScriptHostControl.GetCurrentRevision()
            => GetActualRevision(CurrentCheckout);

        GitRevision? IScriptHostControl.GetLatestSelectedRevision()
            => LatestSelectedRevision;

        IReadOnlyList<GitRevision> IScriptHostControl.GetSelectedRevisions()
            => GetSelectedRevisions();

        Point IScriptHostControl.GetQuickItemSelectorLocation()
            => GetQuickItemSelectorLocation();

        #endregion

        bool ICheckRefs.Contains(ObjectId objectId) => _gridView.Contains(objectId);

        void IRunScript.Execute(string name)
        {
            if (ScriptRunner.RunScript(this, Module, name, UICommands, this).NeedsGridRefresh)
            {
                RefreshRevisions();
            }
        }

        internal TestAccessor GetTestAccessor()
            => new(this);

        internal readonly struct TestAccessor
        {
            private readonly RevisionGridControl _revisionGridControl;

            public TestAccessor(RevisionGridControl revisionGridControl)
            {
                _revisionGridControl = revisionGridControl;
            }

            public int VisibleRevisionCount => _revisionGridControl._gridView.RowCount;

            public bool IsUiStable =>
                !_revisionGridControl._isRefreshingRevisions &&
                !_revisionGridControl._gridView.IsBackgroundUpdaterActive;
        }
    }
}
