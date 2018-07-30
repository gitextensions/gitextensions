using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.UserControls.RevisionGrid
{
    [Flags]
    public enum RevisionNodeFlags
    {
        None = 0,
        CheckedOut = 1,
        HasRef = 2
    }

    public sealed class RevisionDataGridView : DataGridView
    {
        private static readonly SolidBrush _alternatingRowBackgroundBrush = new SolidBrush(Color.FromArgb(250, 250, 250));

        private readonly ConcurrentDictionary<int, bool> _isRelativeByIndex = new ConcurrentDictionary<int, bool>();
        private readonly ConcurrentDictionary<int, GitRevision> _revisionByRowIndex = new ConcurrentDictionary<int, GitRevision>();

        internal readonly GraphModel _graphModel = new GraphModel();

        private readonly List<ColumnProvider> _columnProviders = new List<ColumnProvider>();
        private readonly AutoResetEvent _backgroundEvent = new AutoResetEvent(false);
        private readonly Thread _backgroundThread;
        private volatile bool _shouldRun = LicenseManager.UsageMode != LicenseUsageMode.Designtime;
        private int _backgroundScrollTo;

        private int _graphDataCount;
        private int _rowHeight; // Height of elements in the cache. Is equal to the control's row height.
        private VisibleRowRange _visibleRowRange;

        private Font _normalFont;
        private Font _boldFont;

        public bool UpdatingVisibleRows { get; private set; }

        public RevisionDataGridView()
        {
            _backgroundThread = new Thread(BackgroundThreadEntry)
            {
                IsBackground = true,
                Name = "RevisionDataGridView.backgroundThread"
            };
            _backgroundThread.Start();

            NormalFont = AppSettings.Font;

            InitializeComponent();

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
                    if (revision != null)
                    {
                        provider.OnCellFormatting(e, revision);
                    }
                }
            };
            RowPrePaint += OnRowPrePaint;

            _graphModel.Updated += () =>
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
                    var backBrush = GetBackground(e.State, e.RowIndex);
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Justification = "It looks like such lock was made intentionally but it is better to rewrite this")]
        protected override void Dispose(bool disposing)
        {
            lock (_backgroundEvent)
            {
                _shouldRun = false;
            }

            if (disposing)
            {
                _backgroundEvent?.Dispose();
            }

            base.Dispose(disposing);
        }

        internal Font NormalFont
        {
            get => _normalFont;
            set
            {
                _normalFont = value;
                _boldFont = new Font(value, FontStyle.Bold);
            }
        }

        [CanBeNull]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IReadOnlyList<ObjectId> SelectedObjectIds
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
                    var row = _graphModel.GetLaneRow(SelectedRows[i].Index);

                    if (row != null)
                    {
                        // NOTE returned collection has reverse order of SelectedRows
                        data[SelectedRows.Count - 1 - i] = row.Node.ObjectId;
                    }
                }

                return data;
            }
            set
            {
                if (value != null &&
                    SelectedRows.Count == value.Count &&
                    SelectedObjectIds?.SequenceEqual(value) == true)
                {
                    return;
                }

                lock (_backgroundEvent)
                lock (_graphModel)
                {
                    if (value == null)
                    {
                        // Setting CurrentCell to null internally calls ClearSelection
                        CurrentCell = null;
                        return;
                    }

                    DataGridViewCell currentCell = null;

                    foreach (var guid in value)
                    {
                        if (TryGetRevisionIndex(guid) is int index &&
                            index >= 0 &&
                            index < Rows.Count)
                        {
                            Rows[index].Selected = true;

                            if (currentCell == null)
                            {
                                // Set the current cell to the first item. We use cell
                                // 1 because cell 0 could be hidden if they've chosen to
                                // not see the graph
                                currentCell = Rows[index].Cells[1];
                            }
                        }
                    }

                    // Only clear selection if we have a current cell
                    if (currentCell != null)
                    {
                        ClearSelection();
                    }

                    CurrentCell = currentCell;
                }
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
            if (state.HasFlag(DataGridViewElementStates.Selected))
            {
                return SystemColors.HighlightText;
            }

            return AppSettings.RevisionGraphDrawNonRelativesTextGray && !RowIsRelative(rowIndex)
                ? Color.Gray
                : Color.Black;
        }

        private static Brush GetBackground(DataGridViewElementStates state, int rowIndex)
        {
            if (state.HasFlag(DataGridViewElementStates.Selected))
            {
                return SystemBrushes.Highlight;
            }

            if (AppSettings.RevisionGraphDrawAlternateBackColor && rowIndex % 2 == 0)
            {
                return _alternatingRowBackgroundBrush;
            }

            return Brushes.White;
        }

        private void OnCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            Debug.Assert(_rowHeight != 0, "_rowHeight != 0");

            var revision = GetRevision(e.RowIndex);

            if (e.RowIndex < 0 ||
                e.RowIndex >= RowCount ||
                !e.State.HasFlag(DataGridViewElementStates.Visible) ||
                revision == null)
            {
                return;
            }

            if (Columns[e.ColumnIndex].Tag is ColumnProvider provider)
            {
                var backBrush = GetBackground(e.State, e.RowIndex);
                var foreColor = GetForeground(e.State, e.RowIndex);

                provider.OnCellPainting(e, revision, _rowHeight, (backBrush, foreColor, _normalFont, _boldFont));

                e.Handled = true;
            }
        }

        public void Add(GitRevision revision, RevisionNodeFlags types = RevisionNodeFlags.None)
        {
            lock (_graphModel)
            {
                _graphModel.Add(revision, types);
            }

            UpdateVisibleRowRange();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Justification = "It looks like such lock was made intentionally but it is better to rewrite this")]
        public void Clear()
        {
            lock (_backgroundThread)
            {
                _backgroundScrollTo = 0;
            }

            foreach (var columnProvider in _columnProviders)
            {
                columnProvider.Clear();
            }

            lock (_graphModel)
            {
                SetRowCount(0);
                _graphDataCount = 0;
                _revisionByRowIndex.Clear();
                _isRelativeByIndex.Clear();

                ////// Redraw
                UpdateVisibleRowRange();
                Invalidate(invalidateChildren: true);
            }
        }

        public bool RowIsRelative(int rowIndex)
        {
            return _isRelativeByIndex.GetOrAdd(rowIndex, IsRelative);

            bool IsRelative(int index)
            {
                lock (_graphModel)
                {
                    var laneRow = _graphModel.GetLaneRow(index);

                    if (laneRow == null)
                    {
                        return false;
                    }

                    if (laneRow.Node.Ancestors.Count > 0)
                    {
                        return laneRow.Node.Ancestors[0].IsRelative;
                    }

                    return true;
                }
            }
        }

        [CanBeNull]
        public GitRevision GetRevision(int rowIndex)
        {
            return _revisionByRowIndex.GetOrAdd(rowIndex, Create);

            GitRevision Create(int row)
            {
                lock (_graphModel)
                {
                    return _graphModel.GetLaneRow(row)?.Node.Revision;
                }
            }
        }

        public void Prune()
        {
            lock (_graphModel)
            {
                _graphModel.Prune();
                _revisionByRowIndex.Clear();
                _isRelativeByIndex.Clear();

                SetRowCount(_graphModel.Count);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Justification = "It looks like such lock was made intentionally but it is better to rewrite this")]
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

            lock (_backgroundThread)
            {
                UpdatingVisibleRows = true;

                try
                {
                    if (CurrentCell == null)
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
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Justification = "It looks like such lock was made intentionally but it is better to rewrite this")]
        private void BackgroundThreadEntry()
        {
            while (_shouldRun)
            {
                if (!_shouldRun)
                {
                    return;
                }

                if (_backgroundEvent.WaitOne(500))
                {
                    if (!_shouldRun)
                    {
                        return;
                    }

                    if (AppSettings.ShowRevisionGridGraphColumn)
                    {
                        lock (_backgroundEvent)
                        {
                            int scrollTo;
                            lock (_backgroundThread)
                            {
                                scrollTo = _backgroundScrollTo;
                            }

                            int curCount;
                            lock (_graphModel)
                            {
                                curCount = _graphDataCount;
                                _graphDataCount = _graphModel.CachedCount;
                            }

                            UpdateGraph(curCount, scrollTo);
                        }
                    }
                    else
                    {
                        // Graph is not visible, so sleep a little while to prevent wasting time do nothing... do not cache, the graph is invisible
                        Thread.Sleep(10);
                    }
                }
            }

            void UpdateGraph(int fromIndex, in int toIndex)
            {
                var rowIndex = fromIndex;

                while (rowIndex < toIndex)
                {
                    lock (_graphModel)
                    {
                        // Cache the next item
                        if (!_graphModel.CacheTo(rowIndex))
                        {
                            Debug.WriteLine("Cached item FAILED {0}", rowIndex);
                            lock (_backgroundThread)
                            {
                                _backgroundScrollTo = rowIndex;
                            }

                            break;
                        }

                        // Update the row (if needed)
                        if (rowIndex == Math.Min(toIndex, _visibleRowRange.ToIndex) - 1)
                        {
                            this.InvokeAsync(UpdateRow, rowIndex).FileAndForget();
                        }

                        rowIndex = _graphModel.CachedCount;
                        _graphDataCount = rowIndex;
                    }
                }

                return;

                void UpdateRow(int row)
                {
                    if (RowCount < _graphModel.Count)
                    {
                        lock (_graphModel)
                        {
                            SetRowCount(_graphModel.Count);
                        }
                    }

                    // We only need to invalidate if the row is visible
                    if (_visibleRowRange.Contains(row) && row < RowCount)
                    {
                        try
                        {
                            InvalidateRow(row);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            // Ignore. It is possible that RowCount gets changed before
                            // this is processed and the row is larger than RowCount.
                        }
                    }
                }
            }
        }

        private void UpdateVisibleRowRange()
        {
            var oldRange = _visibleRowRange;
            var fromIndex = FirstDisplayedCell?.RowIndex ?? 0;
            var toIndex = _rowHeight > 0 ? fromIndex + (Height / _rowHeight) : fromIndex;

            if (toIndex >= _graphModel.Count)
            {
                toIndex = _graphModel.Count;
            }

            _visibleRowRange = new VisibleRowRange(fromIndex, toIndex);

            if (oldRange == _visibleRowRange)
            {
                return;
            }

            this.InvokeAsync(NotifyProvidersVisibleRowRangeChanged).FileAndForget();

            var targetBottom = Math.Min(
                toIndex + 250,
                _graphModel.Count);

            if (_backgroundScrollTo < targetBottom)
            {
                _backgroundScrollTo = targetBottom;
                _backgroundEvent.Set();
            }

            return;

            void NotifyProvidersVisibleRowRangeChanged()
            {
                foreach (var provider in _columnProviders)
                {
                    provider.OnVisibleRowsChanged(_visibleRowRange);
                }
            }
        }

        public override void Refresh()
        {
            _revisionByRowIndex.Clear();
            _isRelativeByIndex.Clear();

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
            using (var g = Graphics.FromHwnd(Handle))
            {
                _rowHeight = (int)g.MeasureString("By", _normalFont).Height + DpiUtil.Scale(9);
                //// + AppSettings.GridRowSpacing
                RowTemplate.Height = _rowHeight;
            }
        }

        public bool IsRevisionRelative(ObjectId objectId)
        {
            return _graphModel.IsRevisionRelative(objectId);
        }

        [CanBeNull]
        public GitRevision GetRevision(ObjectId objectId)
        {
            return _graphModel.TryGetNode(objectId, out var node) ? node.Revision : null;
        }

        public int? TryGetRevisionIndex([CanBeNull] ObjectId objectId)
        {
            if (Rows.Count == 0)
            {
                return null;
            }

            return objectId != null && _graphModel.TryGetNode(objectId, out var node) ? (int?)node.Index : null;
        }

        public IReadOnlyList<ObjectId> GetRevisionChildren(ObjectId objectId)
        {
            // We do not need a lock here since we load the data from the first commit and walk through all
            // parents. Children are always loaded, since we start at the newest commit.
            // With lock, loading the commit info slows down terribly.
            if (_graphModel.TryGetNode(objectId, out var node))
            {
                return node.Descendants.Select(d => d.GetChildOf(node).ObjectId).ToList();
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

                if (e.Delta > 0)
                {
                    HorizontalScrollingOffset = Math.Max(0, currentIndex - scrollLines);
                }
                else if (e.Delta < 0)
                {
                    HorizontalScrollingOffset = currentIndex + scrollLines;
                }
            }
            else
            {
                base.OnMouseWheel(e);
            }
        }
    }
}