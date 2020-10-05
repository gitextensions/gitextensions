using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitUI.UserControls.RevisionGrid
{
    [Flags]
    public enum RevisionNodeFlags
    {
        None = 0,
        CheckedOut = 1,
        HasRef = 2,
        OnlyFirstParent = 4
    }

    public sealed class RevisionDataGridView : DataGridView
    {
        private readonly SolidBrush _alternatingRowBackgroundBrush;

        internal RevisionGraph _revisionGraph = new();

        private readonly List<ColumnProvider> _columnProviders = new List<ColumnProvider>();
        private readonly CancellationTokenSequence _backgroundCancellationSequence;
        private readonly AsyncQueue<(Func<CancellationToken, Task> backgroundOperation, CancellationToken cancellationToken)> _backgroundQueue =
            new AsyncQueue<(Func<CancellationToken, Task> backgroundOperation, CancellationToken cancellationToken)>();
        private CancellationToken _backgroundCancellationToken;
        private JoinableTask? _backgroundProcessingTask;
        private int _backgroundScrollTo;

        private int _rowHeight; // Height of elements in the cache. Is equal to the control's row height.
        private VisibleRowRange _visibleRowRange;

        private readonly Font _normalFont;
        private readonly Font _boldFont;
        private readonly Font _monospaceFont;

        public bool UpdatingVisibleRows { get; private set; }

        public RevisionDataGridView()
        {
            _backgroundCancellationSequence = new CancellationTokenSequence();
            _backgroundCancellationToken = _backgroundCancellationSequence.Next();
            StartBackgroundProcessingTask(_backgroundCancellationToken);

            _normalFont = AppSettings.Font;
            _boldFont = new Font(AppSettings.Font, FontStyle.Bold);
            _monospaceFont = AppSettings.MonospaceFont;

            InitializeComponent();
            DoubleBuffered = true;

            _alternatingRowBackgroundBrush =
                new SolidBrush(KnownColor.Window.MakeBackgroundDarkerBy(0.025)); // 0.018

            UpdateRowHeight();

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            ColumnWidthChanged += (s, e) =>
            {
                if (e.Column.Tag is ColumnProvider provider)
                {
                    provider.OnColumnWidthChanged(e);
                }
            };
            Scroll += delegate { UpdateVisibleRowRange(); };
            Resize += delegate { UpdateVisibleRowRange(); };
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
            RowPrePaint += OnRowPrePaint;

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

            void OnRowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
            {
                if (e.PaintParts.HasFlag(DataGridViewPaintParts.Background) &&
                    e.RowBounds.Width > 0 &&
                    e.RowBounds.Height > 0)
                {
                    // Draw row background
                    var backBrush = GetBackground(e.State, e.RowIndex, null);
                    e.Graphics.FillRectangle(backBrush, e.RowBounds);
                }
            }

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
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Make sure to mark the background queue as complete before disposing the cancellation token sequence.
                _backgroundQueue.Complete();
                _backgroundCancellationSequence.Dispose();
                _backgroundProcessingTask?.Join();
            }

            base.Dispose(disposing);
        }

        internal AuthorRevisionHighlighting? AuthorHighlighting { get; set; }

        // Contains the object Id's that will be selected as soon as all of them have been loaded.
        // The object Id's are in the order in which they were originally selected.
        public IReadOnlyList<ObjectId> ToBeSelectedObjectIds { get; set; } = Array.Empty<ObjectId>();

        private int _loadedToBeSelectedRevisionsCount = 0;

        public bool HasSelection()
        {
            return ToBeSelectedObjectIds.Any() || SelectedRows.Count > 0;
        }

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

            Columns.Add(columnProvider.Column);
        }

        private Color GetForeground(DataGridViewElementStates state, int rowIndex)
        {
            bool isNonRelativeGray = AppSettings.RevisionGraphDrawNonRelativesTextGray && !RowIsRelative(rowIndex);
            bool isSelected = state.HasFlag(DataGridViewElementStates.Selected);
            return (isNonRelativeGray, isSelected) switch
            {
                (isNonRelativeGray: false, isSelected: false) => SystemColors.ControlText,
                (isNonRelativeGray: false, isSelected: true) => SystemColors.HighlightText,
                (isNonRelativeGray: true, isSelected: false) => SystemColors.GrayText,

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
                return SystemBrushes.Highlight;
            }

            if (AppSettings.HighlightAuthoredRevisions && revision is not null && !revision.IsArtificial && AuthorHighlighting?.IsHighlighted(revision) != false)
            {
                return new SolidBrush(AppColor.AuthoredHighlight.GetThemeColor());
            }

            if (rowIndex % 2 == 0 && AppSettings.RevisionGraphDrawAlternateBackColor)
            {
                return _alternatingRowBackgroundBrush;
            }

            return SystemBrushes.Window;
        }

        private void OnCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            Debug.Assert(_rowHeight != 0, "_rowHeight != 0");

            var revision = GetRevision(e.RowIndex);

            if (e.RowIndex < 0 ||
                e.RowIndex >= RowCount ||
                !e.State.HasFlag(DataGridViewElementStates.Visible) ||
                revision is null)
            {
                return;
            }

            if (Columns[e.ColumnIndex].Tag is ColumnProvider provider)
            {
                var backBrush = GetBackground(e.State, e.RowIndex, revision);
                var foreColor = GetForeground(e.State, e.RowIndex);
                var commitBodyForeColor = GetCommitBodyForeground(e.State, e.RowIndex);

                e.Graphics.FillRectangle(backBrush, e.CellBounds);
                var cellStyle = new CellStyle(backBrush, foreColor, commitBodyForeColor, _normalFont, _boldFont, _monospaceFont);
                provider.OnCellPainting(e, revision, _rowHeight, cellStyle);

                e.Handled = true;
            }
        }

        public void Add(GitRevision revision, RevisionNodeFlags types = RevisionNodeFlags.None)
        {
            _revisionGraph.Add(revision, types);

            if (ToBeSelectedObjectIds.Contains(revision.ObjectId))
            {
                ++_loadedToBeSelectedRevisionsCount;
            }

            UpdateVisibleRowRange();
        }

        public void Clear()
        {
            _backgroundScrollTo = 0;

            // Cancel all outstanding background operations, and provide a new cancellation token for future work.
            var cancellationToken = _backgroundCancellationToken = _backgroundCancellationSequence.Next();
            _backgroundProcessingTask?.Join();

            // Set rowcount to 0 first, to ensure it is not possible to select or redraw, since we are about te delete the data
            SetRowCount(0);
            _revisionGraph.Clear();
            _loadedToBeSelectedRevisionsCount = 0;

            // The graphdata is stored in one of the columnproviders, clear this last
            foreach (var columnProvider in _columnProviders)
            {
                columnProvider.Clear();
            }

            // Redraw
            UpdateVisibleRowRange();
            Invalidate(invalidateChildren: true);

            StartBackgroundProcessingTask(cancellationToken);
        }

        /// <summary>
        /// Checks whether the given hash is present in the graph.
        /// </summary>
        /// <param name="objectId">The hash to find.</param>
        /// <returns><see langword="true"/>, if the given hash if found; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="objectId"/> is <see langword="null"/>.</exception>
        public bool Contains(ObjectId objectId) => _revisionGraph.Contains(objectId);

        private void StartBackgroundProcessingTask(CancellationToken cancellationToken)
        {
            // Start the background processing via JoinableTaskContext.Factory to avoid tracking the long-running
            // operation in JoinPendingOperationsAsync.
            _backgroundProcessingTask = ThreadHelper.JoinableTaskContext.Factory.RunAsync(() => RunBackgroundAsync(cancellationToken));
        }

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

        private void SelectRowsIfReady(int rowCount)
        {
            // Wait till we have all the row indexes to be selected.
            if (_loadedToBeSelectedRevisionsCount == 0 || _loadedToBeSelectedRevisionsCount < ToBeSelectedObjectIds.Count)
            {
                return;
            }

            foreach (var objectId in ToBeSelectedObjectIds)
            {
                try
                {
                    if (!_revisionGraph.TryGetRowIndex(objectId, out int rowIndexToBeSelected) || rowIndexToBeSelected >= rowCount)
                    {
                        return;
                    }

                    Rows[rowIndexToBeSelected].Selected = true;

                    CurrentCell ??= Rows[rowIndexToBeSelected].Cells[1];
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Not worth crashing for. Ignore exception.
                }
            }

            // The rows to be selected have just been selected. Prevent from selecting them again.
            _loadedToBeSelectedRevisionsCount = 0;
            ToBeSelectedObjectIds = Array.Empty<ObjectId>();
        }

        private void SetRowCountAndSelectRowsIfReady(int rowCount)
        {
            SetRowCount(rowCount);
            SelectRowsIfReady(rowCount);
        }

        private async Task RunBackgroundAsync(CancellationToken cancellationToken)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                // Don't run background operations in the designer.
                return;
            }

            await TaskScheduler.Default;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    // The background thread is requesting shutdown. Return immediately unless the work queue is marked
                    // as completed (meaning the background thread will not be restarted) and still contains work items.
                    if (!_backgroundQueue.IsCompleted || _backgroundQueue.IsEmpty)
                    {
                        return;
                    }
                }

                try
                {
                    CancellationToken timeoutToken = default;
                    Func<CancellationToken, Task> backgroundOperation;
                    CancellationToken backgroundOperationCancellation;
                    try
                    {
                        using var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
                        using var linkedCancellation = timeoutTokenSource.Token.CombineWith(cancellationToken);
                        timeoutToken = timeoutTokenSource.Token;
                        (backgroundOperation, backgroundOperationCancellation) = await _backgroundQueue.DequeueAsync(linkedCancellation.Token);
                    }
                    catch (OperationCanceledException) when (timeoutToken.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                    {
                        // No work was received from the queue within the timeout.
                        if (RowCount < _revisionGraph.Count)
                        {
                            this.InvokeAsync(() => { SetRowCountAndSelectRowsIfReady(_revisionGraph.Count); }).FileAndForget();
                        }

                        continue;
                    }

                    if (backgroundOperationCancellation.IsCancellationRequested)
                    {
                        continue;
                    }

                    try
                    {
                        await backgroundOperation(backgroundOperationCancellation);
                    }
                    catch (OperationCanceledException) when (backgroundOperationCancellation.IsCancellationRequested)
                    {
                        // Normal cancellation of background work
                    }
                }
                catch (OperationCanceledException) when (_backgroundQueue.IsCompleted && _backgroundQueue.IsEmpty)
                {
                    // Normal completion of background work
                    return;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Normal cancellation of background queue during clear
                    return;
                }
            }
        }

        private void UpdateVisibleRowRange()
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
                        _backgroundQueue.Enqueue((BackgroundUpdateAsync, _backgroundCancellationToken));
                    }

                    this.InvokeAsync(NotifyProvidersVisibleRowRangeChanged).FileAndForget();
                }
            }
        }

        private Task BackgroundUpdateAsync(CancellationToken cancellationToken)
        {
            if (AppSettings.ShowRevisionGridGraphColumn)
            {
                int scrollTo;
                int curCount;
                do
                {
                    scrollTo = _backgroundScrollTo;
                    curCount = _revisionGraph.GetCachedCount();
                    UpdateGraph(curCount, scrollTo);
                }
                while (curCount < scrollTo);
            }
            else
            {
                UpdateGraph(_revisionGraph.Count, _revisionGraph.Count);
            }

            this.InvokeAsync(NotifyProvidersVisibleRowRangeChanged).FileAndForget();
            return Task.CompletedTask;

            void UpdateGraph(int fromIndex, int toIndex)
            {
                // Cache the next item
                _revisionGraph.CacheTo(toIndex, Math.Min(fromIndex + 1500, toIndex));

                var rowIndex = _revisionGraph.GetCachedCount();

                this.InvokeAsync(UpdateRowCount, toIndex).FileAndForget();
                return;

                void UpdateRowCount(int row)
                {
                    if (RowCount < _revisionGraph.Count)
                    {
                        SetRowCountAndSelectRowsIfReady(_revisionGraph.Count);
                    }
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
            // TODO allow custom grid font
            ////NormalFont = AppSettings.RevisionGridFont;
            ////NormalFont = new Font(Settings.Font.Name, Settings.Font.Size + 2); // SystemFonts.DefaultFont.FontFamily, SystemFonts.DefaultFont.Size + 2);

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
