#nullable enable

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GitCommands;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid
{
    public sealed partial class RevisionDataGridView : DataGridView
    {
        private const int BackgroundThreadUpdatePeriod = 25;
        private static readonly AccessibleDataGridViewTextBoxCell _accessibleDataGridViewTextBoxCell = new();

        private readonly SolidBrush _alternatingRowBackgroundBrush;
        private readonly SolidBrush _authoredHighlightBrush;

        private readonly BackgroundUpdater _backgroundUpdater;
        private readonly Stopwatch _lastRepaint = Stopwatch.StartNew();
        private readonly Stopwatch _lastScroll = Stopwatch.StartNew();
        private readonly Stopwatch _consecutiveScroll = Stopwatch.StartNew();
        private readonly List<ColumnProvider> _columnProviders = new();

        internal RevisionGraph _revisionGraph = new();

        // Set while loading the revisions and data grid, see also ToBeSelectedObjectIds.
        private Lazy<IList<int>> _toBeSelectedGraphIndexesCache;
        private int _loadedToBeSelectedRevisionsCount = 0;

        private int _backgroundScrollTo;
        private int _rowHeight; // Height of elements in the cache. Is equal to the control's row height.

        private VisibleRowRange _visibleRowRange;

        private Font _normalFont;
        private Font _boldFont;
        private Font _monospaceFont;

        /// <summary>
        /// Indicates whether 'interesting' rows in the data grid is currently being loaded.
        ///
        /// The property is set to true when all revisions have been read (including
        /// artificial commits that are inserted before HEAD) and the data grid rows for
        /// selected revisions have been loaded.
        ///
        /// Revisions are handled directly when git-log delivers them.
        /// All revisions are always saved.
        ///
        /// GridRows are loaded when needed, starting when revisions are loaded.
        /// If the top revision is selected then only the first rows need to be loaded.
        /// If one of the bottom rows are selected, all rows must be loaded.
        /// (https://github.com/gitextensions/gitextensions/wiki/Revision-Graph#revisiongraphrow describes this some.)
        /// So all grid rows are not forced to be loaded.
        /// The code now forces loading more rows to be able to select them.
        ///
        /// This property is intended to be used by tests.
        /// </summary>
        public bool IsDataLoadComplete { get; private set; } = true;

        public bool UpdatingVisibleRows { get; private set; }

        // _toBeSelectedGraphIndexesCache is init in Clear()
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public RevisionDataGridView()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitFonts();

            _backgroundUpdater = new BackgroundUpdater(UpdateVisibleRowRangeInternalAsync, BackgroundThreadUpdatePeriod);

            InitializeComponent();
            DoubleBuffered = true;

            _alternatingRowBackgroundBrush = new SolidBrush(KnownColor.Window.MakeBackgroundDarkerBy(0.025)); // 0.018
            _authoredHighlightBrush = new SolidBrush(AppColor.AuthoredHighlight.GetThemeColor());

            UpdateRowHeight();

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            ColumnWidthChanged += (s, e) =>
            {
                if (e.Column.Tag is ColumnProvider provider)
                {
                    provider.OnColumnWidthChanged(e);
                }
            };

            Scroll += (_, _) => OnScroll();
            Resize += (_, _) => UpdateVisibleRowRange();
            GotFocus += (_, _) => InvalidateSelectedRows();
            LostFocus += (_, _) => InvalidateSelectedRows();
            RowPrePaint += (_, _) => _lastRepaint.Restart();

            CellPainting += OnCellPainting;
            CellFormatting += (_, e) =>
            {
                if (Columns[e.ColumnIndex].Tag is ColumnProvider provider)
                {
                    var revision = GetRevision(e.RowIndex);
                    if (revision is not null)
                    {
                        provider.OnCellFormatting(e, revision);
                    }
                }
            };

            _revisionGraph.Updated += () =>
            {
                // We have to post this since the thread owns a lock on GraphData that we'll
                // need in order to re-draw the graph.
                this.InvokeAsync(() =>
                    {
                        Debug.Assert(_rowHeight != 0, "_rowHeight != 0");

                        // Refresh column providers
                        foreach (var columnProvider in _columnProviders)
                        {
                            columnProvider.Refresh(_rowHeight, _visibleRowRange);
                        }

                        Invalidate();
                    })
                    .FileAndForget();
            };

            VirtualMode = true;
            Clear();

            return;

            void InitializeComponent()
            {
                ((ISupportInitialize)this).BeginInit();
                SuspendLayout();
                AllowUserToAddRows = false;
                AllowUserToDeleteRows = false;
                BackgroundColor = SystemColors.Window;
                CellBorderStyle = DataGridViewCellBorderStyle.None;
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    BackColor = SystemColors.Window,
                    ForeColor = SystemColors.ControlText,
                    SelectionBackColor = SystemColors.Highlight,
                    SelectionForeColor = SystemColors.HighlightText,
                    WrapMode = DataGridViewTriState.False
                };
                Dock = DockStyle.Fill;
                GridColor = SystemColors.Window;
                ReadOnly = true;
                RowHeadersVisible = false;
                SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                StandardTab = true;
                ((ISupportInitialize)this).EndInit();
                ResumeLayout(false);
            }

            void InvalidateSelectedRows()
            {
                for (int index = 0; index < SelectedRows.Count; ++index)
                {
                    InvalidateRow(SelectedRows[index].Index);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        internal AuthorRevisionHighlighting? AuthorHighlighting { get; set; }

        // Contains the object Id's that will be selected as soon as all of them have been loaded.
        // The object Id's are in the order in which they were originally selected.
        public IReadOnlyList<ObjectId> ToBeSelectedObjectIds { get; set; } = Array.Empty<ObjectId>();
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlyList<ObjectId>? SelectedObjectIds
        {
            get
            {
                if (SelectedRows.Count == 0)
                {
                    return null;
                }

                var data = new ObjectId[SelectedRows.Count];

                for (var i = 0; i < SelectedRows.Count; i++)
                {
                    var row = _revisionGraph.GetNodeForRow(SelectedRows[i].Index);

                    if (row is not null && row.GitRevision is not null)
                    {
                        // NOTE returned collection has reverse order of SelectedRows
                        data[SelectedRows.Count - 1 - i] = row.GitRevision.ObjectId;
                    }
                }

                return data;
            }
        }

        internal void AddColumn(ColumnProvider columnProvider)
        {
            _columnProviders.Add(columnProvider);

            columnProvider.Column.Tag = columnProvider;
            columnProvider.Column.CellTemplate = _accessibleDataGridViewTextBoxCell;

            Columns.Add(columnProvider.Column);
        }

        private Color GetForeground(DataGridViewElementStates state, int rowIndex)
        {
            bool isNonRelativeGray = AppSettings.RevisionGraphDrawNonRelativesTextGray && !RowIsRelative(rowIndex);
            bool isSelectedAndFocused = state.HasFlag(DataGridViewElementStates.Selected) && Focused;
            return (isNonRelativeGray, isSelectedAndFocused) switch
            {
                (isNonRelativeGray: false, isSelectedAndFocused: false) => SystemColors.ControlText,
                (isNonRelativeGray: false, isSelectedAndFocused: true) => SystemColors.HighlightText,
                (isNonRelativeGray: true, isSelectedAndFocused: false) => SystemColors.GrayText,

                // (isGray: true, isSelected: true)
                _ => getHighlightedGrayTextColor()
            };
        }

        private Color GetCommitBodyForeground(DataGridViewElementStates state, int rowIndex)
        {
            bool isNonRelativeGray = AppSettings.RevisionGraphDrawNonRelativesTextGray && !RowIsRelative(rowIndex);
            bool isSelected = state.HasFlag(DataGridViewElementStates.Selected);

            return (isNonRelativeGray, isSelected) switch
            {
                (isNonRelativeGray: false, isSelected: false) => SystemColors.GrayText,
                (isNonRelativeGray: false, isSelected: true) => getHighlightedGrayTextColor(),
                (isNonRelativeGray: true, isSelected: false) => getGrayTextColor(degreeOfGrayness: 1.4f),

                // (isGray: true, isSelected: true)
                _ => getHighlightedGrayTextColor(degreeOfGrayness: 1.4f)
            };
        }

        private Brush GetBackground(DataGridViewElementStates state, int rowIndex, GitRevision? revision)
        {
            if (state.HasFlag(DataGridViewElementStates.Selected))
            {
                return Focused ? SystemBrushes.Highlight : OtherColors.InactiveSelectionHighlightBrush;
            }

            if (AppSettings.HighlightAuthoredRevisions && revision is not null && !revision.IsArtificial && AuthorHighlighting?.IsHighlighted(revision) != false)
            {
                return _authoredHighlightBrush;
            }

            if (rowIndex % 2 == 0 && AppSettings.RevisionGraphDrawAlternateBackColor)
            {
                return _alternatingRowBackgroundBrush;
            }

            return SystemBrushes.Window;
        }

        private void OnCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            _lastRepaint.Restart();

            Debug.Assert(_rowHeight != 0, "_rowHeight != 0");

            var revision = GetRevision(e.RowIndex);

            if (e.RowIndex < 0 ||
                e.RowIndex >= RowCount ||
                !e.State.HasFlag(DataGridViewElementStates.Visible) ||
                revision is null)
            {
                return;
            }

            Brush backBrush = GetBackground(e.State, e.RowIndex, revision);
            e.Graphics.FillRectangle(backBrush, e.CellBounds);

            if (Columns[e.ColumnIndex].Tag is ColumnProvider provider)
            {
                Color foreColor = GetForeground(e.State, e.RowIndex);
                Color commitBodyForeColor = GetCommitBodyForeground(e.State, e.RowIndex);
                CellStyle cellStyle = new(backBrush, foreColor, commitBodyForeColor, _normalFont, _boldFont, _monospaceFont);

                provider.OnCellPainting(e, revision, _rowHeight, cellStyle);
            }

            e.Handled = true;
        }

        /// <summary>
        /// Add a single revision from the git log to the graph, including segments to parents.
        /// Update visible rows if needed.
        /// </summary>
        /// <param name="revision">The revision to add.</param>
        /// <param name="insertWithMatch">Insert the (artificial) revision with the first match in headParents or first if no match found (or headParents is null).</param>
        /// <param name="insertRange">Number of scores "reserved" in the list when inserting.</param>
        /// <param name="parents">Parent ids for the revision to find (and insert before).</param>
        public void Add(GitRevision revision, bool insertWithMatch = false, int insertRange = 0, IEnumerable<ObjectId>? parents = null)
        {
            // Where to insert the revision, null is last
            int? insertScore = null;
            if (insertWithMatch)
            {
                if (_loadedToBeSelectedRevisionsCount == 0
                    && ToBeSelectedObjectIds.Count == 0
                    && SelectedRows?.Count is > 0)
                {
                    // (GraphIndex) selection in grid was 'premature'
                    ToBeSelectedObjectIds = SelectedObjectIds ?? Array.Empty<ObjectId>();
                    _loadedToBeSelectedRevisionsCount = ToBeSelectedObjectIds.Count;
                    ResetGraphIndices();
                }

                // Insert first by default (if HEAD not found)
                // Actual value is ignored if insertRange is 0
                // (Used when child (like WorkTree) is already inserted when adding parent (like Index))
                insertScore = -1;
                if (insertRange > 0 && parents is not null)
                {
                    foreach (var parentId in parents)
                    {
                        if (_revisionGraph.TryGetNode(parentId, out RevisionGraphRevision parentRev))
                        {
                            insertScore = parentRev.Score;
                            break;
                        }
                    }
                }
            }

            _revisionGraph.Add(revision, insertScore, insertRange);
            if (ToBeSelectedObjectIds.Contains(revision.ObjectId))
            {
                ++_loadedToBeSelectedRevisionsCount;
            }

            UpdateVisibleRowRange();
        }

        public void Clear()
        {
            _backgroundScrollTo = 0;

            // Set rowcount to 0 first, to ensure it is not possible to select or redraw, since we are about to delete the data
            SetRowCount(0);
            _revisionGraph.Clear();
            ClearToBeSelected();

            // The graphdata is stored in one of the columnproviders, clear this last
            foreach (var columnProvider in _columnProviders)
            {
                columnProvider.Clear();
            }

            // Redraw
            UpdateVisibleRowRange();
            Invalidate(invalidateChildren: true);
        }

        public void ClearToBeSelected()
        {
            _loadedToBeSelectedRevisionsCount = 0;
            ToBeSelectedObjectIds = Array.Empty<ObjectId>();
            ResetGraphIndices();
        }

        public void EnsureRowVisible(int row)
        {
            int countVisible = DisplayedRowCount(includePartialRow: false);
            int firstVisible = FirstDisplayedScrollingRowIndex;
            if (row >= 0 && (row < firstVisible || firstVisible + countVisible <= row))
            {
                FirstDisplayedScrollingRowIndex = row;
            }
        }

        /// <summary>
        /// Returns if any of the to-be-selected was found in the loaded revisions but are not yet selected.
        /// </summary>
        public bool PendingToBeSelected => _loadedToBeSelectedRevisionsCount > 0;

        /// <summary>
        /// Set the first objectid in the parent list that is found in loaded revisions
        /// as the to-be-selected objectid in the grid.
        /// </summary>
        /// <param name="parents">List with parents to the objectid initially intended to be selected.</param>
        public void SetToBeSelectedFromParents(IEnumerable<ObjectId>? parents)
        {
            if (parents is null)
            {
                return;
            }

            foreach (ObjectId parentId in parents)
            {
                if (_revisionGraph.TryGetRowIndex(parentId, out int _))
                {
                    ToBeSelectedObjectIds = new ObjectId[] { parentId };
                    _loadedToBeSelectedRevisionsCount = ToBeSelectedObjectIds.Count;
                    break;
                }
            }
        }

        public void LoadingCompleted()
        {
            if (_loadedToBeSelectedRevisionsCount < ToBeSelectedObjectIds.Count)
            {
                // All expected revisions not found, settle with partial (empty) match
                _loadedToBeSelectedRevisionsCount = ToBeSelectedObjectIds.Count;
            }

            if (_loadedToBeSelectedRevisionsCount > 0 && _revisionGraph.Count > 0)
            {
                // Rows have not been selected yet
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    await this.SwitchToMainThreadAsync();

                    if (_toBeSelectedGraphIndexesCache.Value.Count == 0)
                    {
                        // Nothing to select or interrupted
                        MarkAsDataLoadingComplete();
                        return;
                    }

                    int scrollTo = _toBeSelectedGraphIndexesCache.Value.Max();
                    int firstGraphIndex = _toBeSelectedGraphIndexesCache.Value[0];
                    if (RowCount - 1 < scrollTo)
                    {
                        // Wait for the periodic background thread to load all rows in the grid
                        while (RowCount - 1 < scrollTo && firstGraphIndex >= Rows.Count)
                        {
                            // Force loading of rows
                            int maxScroll = Math.Min(RowCount - 1, scrollTo);
                            EnsureRowVisible(maxScroll);

                            // Wait for background thread to update grid rows
                            UpdateVisibleRowRange();
                            await Task.Delay(BackgroundThreadUpdatePeriod);
                            if (_loadedToBeSelectedRevisionsCount == 0)
                            {
                                // Selection done or aborted
                                break;
                            }
                        }
                    }
                    else
                    {
                        // Rows already selected once, reselect and refresh
                        SelectRowsIfReady(RowCount);
                    }

                    // Scroll to first selected only if selection is not changed
                    if (firstGraphIndex >= 0 && firstGraphIndex < Rows.Count && Rows[firstGraphIndex].Selected)
                    {
                        EnsureRowVisible(firstGraphIndex);
                    }

                    MarkAsDataLoadingComplete();
                })
                .FileAndForget();
            }
            else
            {
                MarkAsDataLoadingComplete();
            }

            foreach (ColumnProvider columnProvider in _columnProviders)
            {
                columnProvider.LoadingCompleted();
            }

            return;
        }

        public void MarkAsDataLoadingComplete()
        {
            Debug.Assert(!IsDataLoadComplete, "The grid is already marked as 'data load complete'.");
            IsDataLoadComplete = true;
        }

        public void MarkAsDataLoading()
        {
            Debug.Assert(IsDataLoadComplete, "The grid is already marked as 'data load in process'.");
            IsDataLoadComplete = false;
        }

        /// <summary>
        /// Checks whether the given hash is present in the graph.
        /// </summary>
        /// <param name="objectId">The hash to find.</param>
        /// <returns><see langword="true"/>, if the given hash if found; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="objectId"/> is <see langword="null"/>.</exception>
        public bool Contains(ObjectId objectId) => _revisionGraph.Contains(objectId);

        public bool RowIsRelative(int rowIndex)
        {
            return _revisionGraph.IsRowRelative(rowIndex);
        }

        public GitRevision? GetRevision(int rowIndex)
        {
            return _revisionGraph.GetNodeForRow(rowIndex)?.GitRevision;
        }

        private void SetRowCount(int count)
        {
            if (InvokeRequired)
            {
                // DO NOT INVOKE! The RowCount is fixed at other strategic points in time.
                // -Doing this in synch can lock up the application
                // -Doing this async causes the scroll bar to flicker and eats performance
                // -At first I was concerned that returning might lead to some cases where
                //  we have more items in the list than we're showing, but I'm pretty sure
                //  when we're done processing we'll update with the final count, so the
                //  problem will only be temporary, and not able to distinguish it from
                //  just git giving us data slowly.
                ////Invoke(new MethodInvoker(delegate { setRowCount(count); }));
                return;
            }

            UpdatingVisibleRows = true;

            try
            {
                if (CurrentCell is null)
                {
                    RowCount = count;
                    CurrentCell = null;
                }
                else
                {
                    RowCount = count;
                }
            }
            finally
            {
                UpdatingVisibleRows = false;
            }
        }

        /// <summary>
        /// Reset the calculated indices.
        /// </summary>
        private void ResetGraphIndices()
        {
            _toBeSelectedGraphIndexesCache = new(() => CalculateGraphIndices());
        }

        /// <summary>
        /// Get the revision graph row indexes for the ToBeSelectedObjectIds.
        /// (In filtering situations, all may no longer be in the grid).
        /// </summary>
        private IList<int> CalculateGraphIndices()
        {
            Debug.Assert(_loadedToBeSelectedRevisionsCount == ToBeSelectedObjectIds.Count,
                $"{nameof(CalculateGraphIndices)}() was called before all expected revisions were loaded.");

            List<int> toBeSelectedGraphIndexes = new();
            foreach (ObjectId objectId in ToBeSelectedObjectIds)
            {
                if (_revisionGraph.TryGetRowIndex(objectId, out int rowIndexToBeSelected))
                {
                    toBeSelectedGraphIndexes.Add(rowIndexToBeSelected);
                }
            }

            return toBeSelectedGraphIndexes;
        }

        private void SelectRowsIfReady(int rowCount)
        {
            // Wait till we have all the row indexes to be selected
            if (_loadedToBeSelectedRevisionsCount == 0
                || _loadedToBeSelectedRevisionsCount < ToBeSelectedObjectIds.Count)
            {
                return;
            }

            // All grid rows must be loaded before they are shown
            if (_toBeSelectedGraphIndexesCache.Value.Any(i => i > rowCount - 1))
            {
                return;
            }

            // If updating selection, clear is required first
            ClearSelection();
            bool first = true;
            foreach (int index in _toBeSelectedGraphIndexesCache.Value)
            {
                Rows[index].Selected = true;

                if (first)
                {
                    first = false;
                    CurrentCell = Rows[index].Cells[Math.Min(1, Rows[index].Cells.Count - 1)];
                }
            }

            // The to-be-selected are handled. Prevent from selecting them again.
            ClearToBeSelected();
        }

        private void SetRowCountAndSelectRowsIfReady(int rowCount)
        {
            SetRowCount(rowCount);
            SelectRowsIfReady(rowCount);
        }

        private void OnScroll()
        {
            UpdateVisibleRowRange();

            // When scrolling many rows within a short time, the message pump is
            // flooded with WM_CTLCOLORSCROLLBAR messages and the DataGridView
            // is not repainted. This happens for example when the mouse wheel
            // is spinning fast (with free-spinning mouse wheels) or while dragging
            // the scroll bar fast. In such cases, force a repaint to make the GUI
            // feel more responsive.
            if (_lastScroll.ElapsedMilliseconds > 100)
            {
                _consecutiveScroll.Restart();
            }

            if (_consecutiveScroll.ElapsedMilliseconds > 50
                && _lastRepaint.ElapsedMilliseconds > 50)
            {
                Update();
                _lastRepaint.Restart();
            }

            _lastScroll.Restart();
        }

        private void UpdateVisibleRowRange()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                // TODO: Switch to IsDesignMode? See Github discussion in #8809
                // Don't run background operations in the designer.
                return;
            }

            _backgroundUpdater.ScheduleExcecution();
        }

        private async Task UpdateVisibleRowRangeInternalAsync()
        {
            var fromIndex = Math.Max(0, FirstDisplayedScrollingRowIndex);
            var visibleRowCount = _rowHeight > 0 ? (Height / _rowHeight) + 2 /*Add 2 for rounding*/ : 0;

            visibleRowCount = Math.Min(_revisionGraph.Count - fromIndex, visibleRowCount);

            if (_visibleRowRange.FromIndex != fromIndex || _visibleRowRange.Count != visibleRowCount)
            {
                _visibleRowRange = new VisibleRowRange(fromIndex, visibleRowCount);

                if (visibleRowCount > 0)
                {
                    int newBackgroundScrollTo = fromIndex + visibleRowCount;

                    // We always want to set _backgroundScrollTo. Because we want the backgroundthread to stop working when we scroll up
                    if (_backgroundScrollTo != newBackgroundScrollTo)
                    {
                        _backgroundScrollTo = newBackgroundScrollTo;

                        if (AppSettings.ShowRevisionGridGraphColumn)
                        {
                            int scrollTo;
                            int curCount;

                            do
                            {
                                scrollTo = newBackgroundScrollTo;
                                curCount = _revisionGraph.GetCachedCount();
                                await UpdateGraphAsync(fromIndex: curCount, toIndex: scrollTo);
                            }
                            while (curCount < scrollTo);
                        }
                        else
                        {
                            await UpdateGraphAsync(fromIndex: _revisionGraph.Count, toIndex: _revisionGraph.Count);
                        }
                    }

                    await this.InvokeAsync(NotifyProvidersVisibleRowRangeChanged);
                }
            }

            return;

            async Task UpdateGraphAsync(int fromIndex, int toIndex)
            {
                // Cache the next item
                _revisionGraph.CacheTo(currentRowIndex: toIndex, lastToCacheRowIndex: Math.Min(fromIndex + 1500, toIndex));

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                int rowCount = _revisionGraph.Count;
                if (RowCount < rowCount)
                {
                    SetRowCountAndSelectRowsIfReady(rowCount);
                }
            }
        }

        private void NotifyProvidersVisibleRowRangeChanged()
        {
            foreach (var provider in _columnProviders)
            {
                provider.OnVisibleRowsChanged(_visibleRowRange);
            }
        }

        public override void Refresh()
        {
            InitFonts();

            UpdateRowHeight();
            UpdateVisibleRowRange();

            // Refresh column providers
            foreach (var columnProvider in _columnProviders)
            {
                columnProvider.Refresh(_rowHeight, _visibleRowRange);
            }

            base.Refresh();
        }

        private void UpdateRowHeight()
        {
            // TODO allow custom grid row spacing
            using var g = Graphics.FromHwnd(Handle);
            _rowHeight = (int)g.MeasureString("By", _normalFont).Height + DpiUtil.Scale(9);
            //// + AppSettings.GridRowSpacing
            RowTemplate.Height = _rowHeight;
        }

        public bool IsRevisionRelative(ObjectId objectId)
        {
            return _revisionGraph.IsRevisionRelative(objectId);
        }

        public GitRevision? GetRevision(ObjectId objectId)
        {
            return _revisionGraph.TryGetNode(objectId, out var node) ? node.GitRevision : null;
        }

        public int? TryGetRevisionIndex(ObjectId? objectId)
        {
            return objectId is not null && _revisionGraph.TryGetRowIndex(objectId, out var index) ? index : null;
        }

        public IReadOnlyList<ObjectId> GetRevisionChildren(ObjectId objectId)
        {
            // We do not need a lock here since we load the data from the first commit and walk through all
            // parents. Children are always loaded, since we start at the newest commit.
            // With lock, loading the commit info slows down terribly.
            if (_revisionGraph.TryGetNode(objectId, out var node))
            {
                var children = node.Children.Select(d => d.GitRevision!.ObjectId).ToList();
                children.Reverse();
                return children;
            }

            return Array.Empty<ObjectId>();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Home:
                    if (RowCount != 0)
                    {
                        ClearSelection();
                        Rows[0].Selected = true;
                        CurrentCell = Rows[0].Cells[1];
                    }

                    break;
                case Keys.End:
                    if (RowCount != 0)
                    {
                        ClearSelection();
                        Rows[RowCount - 1].Selected = true;
                        CurrentCell = Rows[RowCount - 1].Cells[1];
                    }

                    break;
                case Keys.Control | Keys.C:
                    var selectedRevisions = SelectedObjectIds;
                    if (selectedRevisions is not null && selectedRevisions.Count != 0)
                    {
                        ClipboardUtil.TrySetText(string.Join(Environment.NewLine, selectedRevisions));
                    }

                    break;
                default:
                    base.OnKeyDown(e);
                    break;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            var hit = HitTest(e.X, e.Y);

            if (hit.Type == DataGridViewHitTestType.None)
            {
                // Work around the fact that clicking in the space to the right of the last column does not
                // actually select the row. Instead, we test if the click would hit if done to the far left
                // of the row, and if so, pretend that's what happened.
                const int fakeX = 5;

                hit = HitTest(fakeX, e.Y);

                if (hit.Type == DataGridViewHitTestType.Cell && hit.RowIndex != -1)
                {
                    base.OnMouseDown(new MouseEventArgs(e.Button, e.Clicks, fakeX, e.Y, e.Delta));
                    return;
                }
            }

            base.OnMouseDown(e);

            // If clicking while loading, cancel load-select
            ClearToBeSelected();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                int currentIndex = HorizontalScrollingOffset;
                int scrollLines = DpiUtil.Scale(32);

                HorizontalScrollingOffset = e.Delta switch
                {
                    > 0 => Math.Max(0, currentIndex - scrollLines),
                    < 0 => currentIndex + scrollLines,
                    _ => HorizontalScrollingOffset
                };
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }

        [MemberNotNull(nameof(_normalFont))]
        [MemberNotNull(nameof(_boldFont))]
        [MemberNotNull(nameof(_monospaceFont))]
        private void InitFonts()
        {
            _normalFont = AppSettings.Font;
            _boldFont = new Font(_normalFont, FontStyle.Bold);
            _monospaceFont = AppSettings.MonospaceFont;
        }

        private static Color getHighlightedGrayTextColor(float degreeOfGrayness = 1f) =>
            ColorHelper.GetHighlightGrayTextColor(
                backgroundColorName: KnownColor.Control,
                textColorName: KnownColor.ControlText,
                highlightColorName: KnownColor.Highlight,
                degreeOfGrayness);

        private static Color getGrayTextColor(float degreeOfGrayness = 1f) =>
            ColorHelper.GetGrayTextColor(textColorName: KnownColor.ControlText, degreeOfGrayness);
    }
}
