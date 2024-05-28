#nullable enable

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
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
        private const int MouseWheelDeltaTimeout = 1500; // Mouse wheel idle time in milliseconds after which unconsumed wheel delta will be dropped.
        private const int RowCountUpdateCoolDown = 300;

        private static readonly AccessibleDataGridViewTextBoxCell _accessibleDataGridViewTextBoxCell = new();

        private readonly SolidBrush _alternatingRowBackgroundBrush;
        private readonly SolidBrush _authoredHighlightBrush;

        private readonly BackgroundUpdater _rowCountUpdater;
        private readonly BackgroundUpdater _visibleRowRangeUpdater;
        private readonly List<ColumnProvider> _columnProviders = [];
        private readonly TaskManager _taskManager = ThreadHelper.CreateTaskManager();
        private readonly CancellationTokenSequence _updateVisibleRowRangeSequence = new();

        internal RevisionGraph _revisionGraph = new();

        // Set while loading the revisions and data grid, see also ToBeSelectedObjectIds.
        private Lazy<IList<int>> _toBeSelectedGraphIndexesCache;
        private int _loadedToBeSelectedRevisionsCount = 0;

        private int _backgroundScrollTo;
        private long _lastMouseWheelTickCount; // Timestamp of the last vertical scroll via mouse wheel.
        private int _mouseWheelDeltaRemainder; // Corresponds to unconsumed scroll distance while scrolling via mouse wheel, see OnMouseWheel().
        private int _rowHeight; // Height of elements in the cache. Is equal to the control's row height.

        private VisibleRowRange _visibleRowRange;

        private Font _normalFont;
        private Font _boldFont;
        private Font _monospaceFont;

        private bool _revisionGraphDrawNonRelativesTextGray;
        private bool _highlightAuthoredRevisions;
        private bool _revisionGraphDrawAlternateBackColor;
        private Color _highlightedGrayTextColor;
        private Color _grayTextColor;
        private Color _highlightedGrayTextColorCustom;

        /// <summary>
        /// Force refresh the gridview, set when revision graph is changed while loading revisions.
        /// </summary>
        private bool _forceRefresh = false;

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

            _rowCountUpdater = new BackgroundUpdater(_taskManager, UpdateRowCountAsync, RowCountUpdateCoolDown);
            _visibleRowRangeUpdater = new BackgroundUpdater(_taskManager, UpdateVisibleRowRangeInternalAsync, BackgroundThreadUpdatePeriod);

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

            Scroll += (_, _) => UpdateVisibleRowRange();
            Resize += (_, _) => UpdateVisibleRowRange();
            GotFocus += (_, _) => InvalidateSelectedRows();
            LostFocus += (_, _) => InvalidateSelectedRows();

            CellPainting += OnCellPainting;
            CellFormatting += (_, e) =>
            {
                if (Columns[e.ColumnIndex].Tag is ColumnProvider provider)
                {
                    GitRevision? revision = GetRevision(e.RowIndex);
                    if (revision is not null)
                    {
                        provider.OnCellFormatting(e, revision);
                    }
                }
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

        internal void CancelBackgroundTasks()
        {
            _columnProviders.Clear();
            _backgroundScrollTo = -1;
            _updateVisibleRowRangeSequence.CancelCurrent();
            _taskManager.JoinPendingOperations();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CancelBackgroundTasks();
            }

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

                ObjectId[] data = new ObjectId[SelectedRows.Count];

                for (int i = 0; i < SelectedRows.Count; i++)
                {
                    RevisionGraphRevision? row = _revisionGraph.GetNodeForRow(SelectedRows[i].Index);

                    if (row?.GitRevision is not null)
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

        private Color GetForeground(bool isSelected, bool isFocused, bool isNonRelativeGray)
        {
            return (isNonRelativeGray, isSelectedAndFocused: isSelected && isFocused) switch
            {
                (isNonRelativeGray: false, isSelectedAndFocused: false) => SystemColors.ControlText,
                (isNonRelativeGray: false, isSelectedAndFocused: true) => SystemColors.HighlightText,
                (isNonRelativeGray: true, isSelectedAndFocused: false) => SystemColors.GrayText,

                // (isGray: true, isSelected: true)
                _ => _highlightedGrayTextColor
            };
        }

        private Color GetCommitBodyForeground(bool isSelected, bool isNonRelativeGray)
        {
            return (isNonRelativeGray, isSelected) switch
            {
                (isNonRelativeGray: false, isSelected: false) => SystemColors.GrayText,
                (isNonRelativeGray: false, isSelected: true) => _highlightedGrayTextColor,
                (isNonRelativeGray: true, isSelected: false) => _grayTextColor,

                // (isGray: true, isSelected: true)
                _ => _highlightedGrayTextColorCustom
            };
        }

        private Brush GetBackground(bool isSelected, bool isFocused, int rowIndex, GitRevision? revision)
        {
            if (isSelected)
            {
                return isFocused ? SystemBrushes.Highlight : OtherColors.InactiveSelectionHighlightBrush;
            }

            if (_highlightAuthoredRevisions && revision?.IsArtificial is false && AuthorHighlighting?.IsHighlighted(revision) is true)
            {
                return _authoredHighlightBrush;
            }

            if (_revisionGraphDrawAlternateBackColor && rowIndex % 2 == 0)
            {
                return _alternatingRowBackgroundBrush;
            }

            return SystemBrushes.Window;
        }

        private CellStyle? _cellStyle = null;
        private GitRevision? _revision = null;
        private bool _isSelected = false;
        private bool _isFocused = false;

        private void OnCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            DebugHelpers.Assert(_rowHeight != 0, "_rowHeight != 0");

            if (e.RowIndex < 0 ||
                e.RowIndex >= RowCount ||
                !e.State.HasFlag(DataGridViewElementStates.Visible))
            {
                return;
            }

            GitRevision? revision = GetRevision(e.RowIndex);
            bool isSelected = e.State.HasFlag(DataGridViewElementStates.Selected);
            if (revision != _revision || _isSelected != isSelected || _isFocused != Focused)
            {
                _revision = revision;
                _isSelected = isSelected;
                _isFocused = Focused;
                if (_revision is null)
                {
                    _cellStyle = null;

                    return;
                }

                Brush backBrush = GetBackground(_isSelected, _isFocused, e.RowIndex, _revision);
                bool isNonRelativeGray = _revisionGraphDrawNonRelativesTextGray && !RowIsRelative(e.RowIndex);
                Color foreColor = GetForeground(_isSelected, _isFocused, isNonRelativeGray);
                Color commitBodyForeColor = GetCommitBodyForeground(_isSelected, isNonRelativeGray);
                _cellStyle = new(backBrush, foreColor, commitBodyForeColor, _normalFont, _boldFont, _monospaceFont);
            }

            if (_cellStyle is null)
            {
                e.Handled = false;
                return;
            }

            e.Graphics!.FillRectangle(_cellStyle.Value.BackBrush, e.CellBounds);

            e.Handled = true;

            if (Columns[e.ColumnIndex].Tag is ColumnProvider provider)
            {
                provider.OnCellPainting(e, _revision!, _rowHeight, _cellStyle.Value);
            }
        }

        /// <summary>
        /// Add revisions from the git log to the graph, including segments to parents.
        /// Update visible rows if needed.
        /// </summary>
        /// <param name="revisions">The revisions to add.</param>
        public void AddRange(IEnumerable<GitRevision> revisions)
        {
            foreach (GitRevision revision in revisions)
            {
                _forceRefresh |= _revisionGraph.Add(revision);
                if (ToBeSelectedObjectIds.Contains(revision.ObjectId))
                {
                    ++_loadedToBeSelectedRevisionsCount;
                }
            }

            TriggerRowCountUpdate();
        }

        /// <summary>
        /// Insert workTree and index revisions to the graph, offset existing revisions.
        /// </summary>
        /// <param name="workTreeRev">The workTree revision to add.</param>
        /// <param name="indexRev">The index revision to add.</param>
        /// <param name="parents">Parent ids for the revision to find (and insert before).</param>
        public void Insert(GitRevision workTreeRev, GitRevision indexRev, IReadOnlyList<ObjectId> parents)
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

            // Insert at matching parent.
            _forceRefresh |= _revisionGraph.Insert(workTreeRev, indexRev, parents);

            if (ToBeSelectedObjectIds.Contains(workTreeRev.ObjectId))
            {
                ++_loadedToBeSelectedRevisionsCount;
            }

            if (ToBeSelectedObjectIds.Contains(indexRev.ObjectId))
            {
                ++_loadedToBeSelectedRevisionsCount;
            }

            TriggerRowCountUpdate();
        }

        public void Clear()
        {
            ThreadHelper.AssertOnUIThread();

            _updateVisibleRowRangeSequence.CancelCurrent();
            _backgroundScrollTo = -1;
            _forceRefresh = false;
            _visibleRowRange = new VisibleRowRange(fromIndex: 0, count: 0);

            // Set rowcount to 0 first, to ensure it is not possible to select or redraw, since we are about to delete the data
            SetRowCount(0);
            _revisionGraph.Clear();
            ClearToBeSelected();

            // The graphdata is stored in one of the columnproviders, clear this last
            foreach (ColumnProvider columnProvider in _columnProviders)
            {
                columnProvider.Clear();
            }

            // Reload settings that will be used during drawing
            _revisionGraphDrawNonRelativesTextGray = AppSettings.RevisionGraphDrawNonRelativesTextGray;
            _highlightedGrayTextColor = getHighlightedGrayTextColor();
            _grayTextColor = getGrayTextColor(degreeOfGrayness: 1.4f);
            _highlightedGrayTextColorCustom = getHighlightedGrayTextColor(degreeOfGrayness: 1.4f);
            _highlightAuthoredRevisions = AppSettings.HighlightAuthoredRevisions;
            _revisionGraphDrawAlternateBackColor = AppSettings.RevisionGraphDrawAlternateBackColor;

            // Redraw
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

            _revisionGraph.LoadingCompleted();
            if (_revisionGraph.Count == 0)
            {
                MarkAsDataLoadingComplete();
                return;
            }

            // Rows have not been selected yet
            this.InvokeAndForget(async () =>
            {
                SetRowCountAndSelectRowsIfReady();

                if (_toBeSelectedGraphIndexesCache.Value.Count == 0)
                {
                    // Nothing to select or interrupted
                    MarkAsDataLoadingComplete();
                    return;
                }

                int rowCount;

                // Wait for the periodic background thread to load the first selected grid row, stop if aborted
                int firstGraphIndex = _toBeSelectedGraphIndexesCache.Value[0];
                do
                {
                    rowCount = RowCount;
                    if (firstGraphIndex < rowCount)
                    {
                        break;
                    }

                    // Scroll to currently last loaded row
                    SetRowCount(rowCount);
                    EnsureRowVisible(rowCount - 1);

                    // Wait for background thread to load grid rows
                    await Task.Delay(BackgroundThreadUpdatePeriod);
                }
                while (_loadedToBeSelectedRevisionsCount > 0);

                // Scroll to first selected only if selection is not changed
                if (firstGraphIndex >= 0 && firstGraphIndex < rowCount && Rows[firstGraphIndex].Selected)
                {
                    EnsureRowVisible(firstGraphIndex);
                }

                MarkAsDataLoadingComplete();
            });
        }

        public void MarkAsDataLoadingComplete()
        {
            DebugHelpers.Assert(!IsDataLoadComplete, "The grid is already marked as 'data load complete'.");
            IsDataLoadComplete = true;
        }

        public void MarkAsDataLoading()
        {
            DebugHelpers.Assert(IsDataLoadComplete, "The grid is already marked as 'data load in process'.");
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
                UpdateVisibleRowRange();
            }
        }

        /// <summary>
        /// Reset the calculated indices.
        /// </summary>
        private void ResetGraphIndices()
        {
            _toBeSelectedGraphIndexesCache = new Lazy<IList<int>>(CalculateGraphIndices);
            return;

            // Get the revision graph row indexes for the ToBeSelectedObjectIds.
            // (In filtering situations, all previous commits may no longer be in the grid).
            IList<int> CalculateGraphIndices()
            {
                List<int> toBeSelectedGraphIndexes = [];
                foreach (ObjectId objectId in ToBeSelectedObjectIds)
                {
                    if (_revisionGraph.TryGetRowIndex(objectId, out int rowIndexToBeSelected))
                    {
                        toBeSelectedGraphIndexes.Add(rowIndexToBeSelected);
                    }
                }

                return toBeSelectedGraphIndexes;
            }
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

        private void SetRowCountAndSelectRowsIfReady()
        {
            int rowCount = _revisionGraph.Count;
            if (RowCount < rowCount)
            {
                SetRowCount(rowCount);
                SelectRowsIfReady(rowCount);
            }
        }

        private void TriggerRowCountUpdate()
        {
            _rowCountUpdater.ScheduleExecution();
        }

        private async Task UpdateRowCountAsync()
        {
            await _taskManager.JoinableTaskFactory.SwitchToMainThreadAsync();
            SetRowCountAndSelectRowsIfReady();
        }

        private void UpdateVisibleRowRange()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                // TODO: Switch to IsDesignMode? See Github discussion in #8809
                // Don't run background operations in the designer.
                return;
            }

            if (_forceRefresh)
            {
                // Always set _backgroundScrollTo in order to stop the background thread
                _backgroundScrollTo = -1;

                // The graph cache must be cleared at once
                foreach (ColumnProvider columnProvider in _columnProviders)
                {
                    columnProvider.Clear();
                }
            }

            _visibleRowRangeUpdater.ScheduleExecution();
        }

        private async Task UpdateVisibleRowRangeInternalAsync()
        {
            if (!AppSettings.ShowRevisionGridGraphColumn
                || _columnProviders.Count == 0
                || _columnProviders[0] is not RevisionGraphColumnProvider revisionGraphColumnProvider)
            {
                return;
            }

            CancellationToken cancellationToken = _updateVisibleRowRangeSequence.Next();

            int fromIndex = Math.Max(0, FirstDisplayedScrollingRowIndex);
            int visibleRowCount = DisplayedRowCount(includePartialRow: true);
            visibleRowCount = Math.Min(_revisionGraph.Count - fromIndex, visibleRowCount);

            if (!_forceRefresh && _visibleRowRange.FromIndex == fromIndex && _visibleRowRange.Count == visibleRowCount)
            {
                return;
            }

            _forceRefresh = false;
            VisibleRowRange visibleRowRange = new(fromIndex, visibleRowCount);
            _visibleRowRange = visibleRowRange;

            if (visibleRowCount == 0)
            {
                return;
            }

            try
            {
                // Preload the next page, too, in order to avoid delayed display of the graph when scrolling down
                int newBackgroundScrollTo = fromIndex + (2 * visibleRowCount) - 1;

                // We always want to set _backgroundScrollTo. Because we want the backgroundthread to stop working when we scroll up
                _backgroundScrollTo = newBackgroundScrollTo;

                int curCount;
                do
                {
                    curCount = _revisionGraph.GetCachedCount();
                    _revisionGraph.CacheTo(currentRowIndex: curCount, lastToCacheRowIndex: newBackgroundScrollTo, cancellationToken);

                    // Take changes to _backgroundScrollTo and IsDataLoadComplete by another thread into account
                    if (IsDataLoadComplete)
                    {
                        _backgroundScrollTo = Math.Min(_backgroundScrollTo, _revisionGraph.Count - 1);
                    }
                }
                while (curCount <= _backgroundScrollTo);

                cancellationToken.ThrowIfCancellationRequested();

                await revisionGraphColumnProvider.RenderGraphToCacheAsync(visibleRowRange, newBackgroundScrollTo, _rowHeight, cancellationToken);
            }
            catch (Exception exception)
            {
                _visibleRowRange = new VisibleRowRange(fromIndex: 0, count: 0);
                Trace.WriteLineIf(exception is not OperationCanceledException, exception);
            }
        }

        public void ApplySettings()
        {
            InitFonts();
            UpdateRowHeight();

            foreach (ColumnProvider columnProvider in _columnProviders)
            {
                columnProvider.ApplySettings();
            }

            Refresh();
        }

        public override void Refresh()
        {
            _forceRefresh = true;
            UpdateVisibleRowRange();

            base.Refresh();
        }

        private void UpdateRowHeight()
        {
            // TODO allow custom grid row spacing
            using Graphics g = Graphics.FromHwnd(Handle);
            _rowHeight = (int)g.MeasureString("By", _normalFont).Height + DpiUtil.Scale(9);
            //// + AppSettings.GridRowSpacing
            RowTemplate.Height = _rowHeight;
        }

        public GitRevision? GetRevision(ObjectId objectId)
        {
            return _revisionGraph.TryGetNode(objectId, out RevisionGraphRevision? node) ? node.GitRevision : null;
        }

        public int? TryGetRevisionIndex(ObjectId? objectId)
        {
            return objectId is not null && _revisionGraph.TryGetRowIndex(objectId, out int index) ? index : null;
        }

        public IReadOnlyList<ObjectId> GetRevisionChildren(ObjectId objectId)
        {
            // We do not need a lock here since we load the data from the first commit and walk through all
            // parents. Children are always loaded, since we start at the newest commit.
            // With lock, loading the commit info slows down terribly.
            if (_revisionGraph.TryGetNode(objectId, out RevisionGraphRevision? node))
            {
                List<ObjectId> children = node.Children.Select(d => d.GitRevision!.ObjectId).ToList();
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
                    IReadOnlyList<ObjectId>? selectedRevisions = SelectedObjectIds;
                    if (selectedRevisions?.Count is > 0)
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
            HitTestInfo hit = HitTest(e.X, e.Y);

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
            int maxRowIndex = RowCount - 1;
            if (maxRowIndex < 0)
            {
                return;
            }

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
                // Reset unconsumed wheel delta when the mouse wheel is idle, because there are at least
                // two situations in which unconsumed wheel delta causes an issue:
                // - When switching back to a notched mouse wheel. Whenever the scroll direction is changed,
                //   unconsumed delta will reduce the absolute value of the total delta so that the threshold
                //   for scrolling one row is never reached on the first wheel rotation.
                // - When using a precision scrolling device, the unconsumed delta will offset the first scroll,
                //   which makes the user experience a subtle "lag" or "leap" at beginning of a scroll.
                long currentTickCount = Environment.TickCount64;
                if (currentTickCount - _lastMouseWheelTickCount > MouseWheelDeltaTimeout)
                {
                    _mouseWheelDeltaRemainder = 0;
                }

                _lastMouseWheelTickCount = currentTickCount;

                int visibleCompleteRowsCount = _rowHeight > 0 ? Height / _rowHeight : 0;

                // The wheel might be configured to scroll more than one row at once.
                // Respect this by scaling MouseEventArgs.Delta accordingly.
                int scrollLines = SystemInformation.MouseWheelScrollLines;

                // Value of -1 indicates the "One screen at a time" mouse option.
                if (scrollLines == -1)
                {
                    scrollLines = visibleCompleteRowsCount;
                }

                scrollLines = Math.Max(1, scrollLines);

                // Calculate the total wheel delta, which corresponds to the intended scrolling distance, from
                // MouseEventArgs.Delta, which is usually a multiple of SystemInformation.MouseWheelScrollDelta
                // for notched mouse wheels, but can be an arbitrary number in the case of precision scrolling
                // devices like free-spinning mouse wheels or touchpads.
                // Consume the total wheel delta in multiples of SystemInformation.MouseWheelScrollDelta, which
                // is the wheel delta threshold for scrolling one row, and save the remainder for consumption
                // during the next MouseWheel event.
                int totalWheelDelta = (scrollLines * e.Delta) + _mouseWheelDeltaRemainder;
                int wheelDeltaPerRow = SystemInformation.MouseWheelScrollDelta;
                _mouseWheelDeltaRemainder = totalWheelDelta % wheelDeltaPerRow;
                int rowDelta = -(totalWheelDelta - _mouseWheelDeltaRemainder) / wheelDeltaPerRow;
                if (rowDelta != 0)
                {
                    int toRowIndex = Math.Clamp(FirstDisplayedScrollingRowIndex + rowDelta, 0, maxRowIndex);

                    // Drop unconsumed wheel delta when reaching the upper or lower bound of the grid
                    // to prevent the grid being stuck there for a moment.
                    if (toRowIndex == 0 || toRowIndex + visibleCompleteRowsCount > maxRowIndex)
                    {
                        _mouseWheelDeltaRemainder = 0;
                    }

                    try
                    {
                        // This will raise the Scroll event.
                        FirstDisplayedScrollingRowIndex = toRowIndex;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // Tried to scroll to nonexistent row.
                    }
                }
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
