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
using GitExtUtils;
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
        HasRef = 2,
        OnlyFirstParent = 4
    }

    public sealed class RevisionDataGridView : DataGridView
    {
        private static readonly SolidBrush _alternatingRowBackgroundBrush = new SolidBrush(Color.FromArgb(250, 250, 250));

        internal RevisionGraph _revisionGraph = new RevisionGraph();

        private readonly List<ColumnProvider> _columnProviders = new List<ColumnProvider>();
        private readonly AutoResetEvent _backgroundEvent = new AutoResetEvent(false);
        private Thread _backgroundThread;
        private volatile bool _shouldRun = LicenseManager.UsageMode != LicenseUsageMode.Designtime;
        private int _backgroundScrollTo;

        private int _rowHeight; // Height of elements in the cache. Is equal to the control's row height.
        private VisibleRowRange _visibleRowRange;

        private Font _normalFont;
        private Font _boldFont;
        private Font _monospaceFont;

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
            _monospaceFont = AppSettings.MonospaceFont;

            InitializeComponent();
            DoubleBuffered = true;

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Justification = "It looks like such lock was made intentionally but it is better to rewrite this")]
        protected override void Dispose(bool disposing)
        {
            _shouldRun = false;

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

        internal AuthorRevisionHighlighting AuthorHighlighting { get; set; }

        // Contains the object Id's that will be selected as soon as they are loaded.
        public HashSet<ObjectId> ToBeSelectedObjectIds { get; set; } = new HashSet<ObjectId>();

        // The ToBeSelectedObjectId's should be converted to indexes. This queue is then used to select the correct index. This is used cross-threads.
        private ConcurrentQueue<int> ToBeSelectedRowIndexes { get; set; } = new ConcurrentQueue<int>();

        public bool HasSelection()
        {
            return ToBeSelectedObjectIds.Any() || ToBeSelectedRowIndexes.Any() || SelectedRows.Count > 0;
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
                    var row = _revisionGraph.GetNodeForRow(SelectedRows[i].Index);

                    if (row != null && row.GitRevision != null)
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
            if (state.HasFlag(DataGridViewElementStates.Selected))
            {
                return SystemColors.HighlightText;
            }

            return AppSettings.RevisionGraphDrawNonRelativesTextGray && !RowIsRelative(rowIndex)
                ? Color.Gray
                : Color.Black;
        }

        private Brush GetBackground(DataGridViewElementStates state, int rowIndex, GitRevision revision)
        {
            if (state.HasFlag(DataGridViewElementStates.Selected))
            {
                return SystemBrushes.Highlight;
            }

            if (AppSettings.HighlightAuthoredRevisions && revision != null && !revision.IsArtificial && AuthorHighlighting.IsHighlighted(revision))
            {
                return new SolidBrush(AppSettings.AuthoredRevisionsHighlightColor);
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
                revision == null)
            {
                return;
            }

            if (Columns[e.ColumnIndex].Tag is ColumnProvider provider)
            {
                var backBrush = GetBackground(e.State, e.RowIndex, revision);
                var foreColor = GetForeground(e.State, e.RowIndex);

                e.Graphics.FillRectangle(backBrush, e.CellBounds);
                provider.OnCellPainting(e, revision, _rowHeight, new CellStyle(backBrush, foreColor, _normalFont, _boldFont, _monospaceFont));

                e.Handled = true;
            }
        }

        public void Add(GitRevision revision, RevisionNodeFlags types = RevisionNodeFlags.None)
        {
            _revisionGraph.Add(revision, types);

            if (ToBeSelectedObjectIds.Remove(revision.ObjectId))
            {
                ToBeSelectedRowIndexes.Enqueue(_revisionGraph.Count - 1);
            }

            UpdateVisibleRowRange();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Justification = "It looks like such lock was made intentionally but it is better to rewrite this")]
        public void Clear()
        {
            _backgroundScrollTo = 0;

            // Force the background thread to be killed, we need to be sure no background processes are running. Not the best practice, but safe.
            _backgroundThread.Abort();

            // Set rowcount to 0 first, to ensure it is not possible to select or redraw, since we are about te delete the data
            SetRowCount(0);
            _revisionGraph.Clear();
            ToBeSelectedRowIndexes = new ConcurrentQueue<int>();

            // The graphdata is stored in one of the columnproviders, clear this last
            foreach (var columnProvider in _columnProviders)
            {
                columnProvider.Clear();
            }

            // Redraw
            UpdateVisibleRowRange();
            Invalidate(invalidateChildren: true);

            _backgroundThread = new Thread(BackgroundThreadEntry)
            {
                IsBackground = true,
                Name = "RevisionDataGridView.backgroundThread"
            };
            _backgroundThread.Start();
        }

        public bool RowIsRelative(int rowIndex)
        {
            return _revisionGraph.IsRowRelative(rowIndex);
        }

        [CanBeNull]
        public GitRevision GetRevision(int rowIndex)
        {
            return _revisionGraph.GetNodeForRow(rowIndex)?.GitRevision;
        }

        public void Prune()
        {
            _revisionGraph.Clear();

            SetRowCount(_revisionGraph.Count);
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

                if (ToBeSelectedRowIndexes.TryPeek(out int toBeSelectedRowIndex) &&
                    count > toBeSelectedRowIndex)
                {
                    try
                    {
                        ToBeSelectedRowIndexes.TryDequeue(out _);
                        Rows[toBeSelectedRowIndex].Selected = true;
                        if (CurrentCell == null)
                        {
                            CurrentCell = Rows[toBeSelectedRowIndex].Cells[1];
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // Not worth crashing for. Ignore exception.
                    }
                }
            }
            finally
            {
                UpdatingVisibleRows = false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Justification = "It looks like such lock was made intentionally but it is better to rewrite this")]
        private void BackgroundThreadEntry()
        {
            bool keepRunning = false;
            while (_shouldRun)
            {
                if (!_shouldRun)
                {
                    return;
                }

                if (keepRunning || _backgroundEvent.WaitOne(200))
                {
                    keepRunning = false;

                    if (!_shouldRun)
                    {
                        return;
                    }

                    if (AppSettings.ShowRevisionGridGraphColumn)
                    {
                        int scrollTo = _backgroundScrollTo;

                        int curCount = _revisionGraph.GetCachedCount();

                        UpdateGraph(curCount, scrollTo);
                        keepRunning = curCount < scrollTo;
                    }
                    else
                    {
                        UpdateGraph(_revisionGraph.Count, _revisionGraph.Count);
                    }

                    if (!keepRunning)
                    {
                        this.InvokeAsync(NotifyProvidersVisibleRowRangeChanged).FileAndForget();
                    }
                }
                else
                {
                    if (RowCount < _revisionGraph.Count)
                    {
                        this.InvokeAsync(() => { SetRowCount(_revisionGraph.Count); }).FileAndForget();
                    }
                }
            }

            void UpdateGraph(int fromIndex, in int toIndex)
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
                        SetRowCount(_revisionGraph.Count);
                    }
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
                        _backgroundEvent.Set();
                    }

                    this.InvokeAsync(NotifyProvidersVisibleRowRangeChanged).FileAndForget();
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
            using (var g = Graphics.FromHwnd(Handle))
            {
                _rowHeight = (int)g.MeasureString("By", _normalFont).Height + DpiUtil.Scale(9);
                //// + AppSettings.GridRowSpacing
                RowTemplate.Height = _rowHeight;
            }
        }

        public bool IsRevisionRelative(ObjectId objectId)
        {
            return _revisionGraph.IsRevisionRelative(objectId);
        }

        [CanBeNull]
        public GitRevision GetRevision(ObjectId objectId)
        {
            return _revisionGraph.TryGetNode(objectId, out var node) ? node.GitRevision : null;
        }

        public int? TryGetRevisionIndex([CanBeNull] ObjectId objectId)
        {
            return objectId != null && _revisionGraph.TryGetRowIndex(objectId, out var index) ? (int?)index : null;
        }

        public IReadOnlyList<ObjectId> GetRevisionChildren(ObjectId objectId)
        {
            // We do not need a lock here since we load the data from the first commit and walk through all
            // parents. Children are always loaded, since we start at the newest commit.
            // With lock, loading the commit info slows down terribly.
            if (_revisionGraph.TryGetNode(objectId, out var node))
            {
                var children = node.Children.Select(d => d.GitRevision.ObjectId).ToList();
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
                    if (selectedRevisions != null && selectedRevisions.Count != 0)
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