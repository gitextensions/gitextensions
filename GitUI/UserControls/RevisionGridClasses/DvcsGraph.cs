using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;

namespace GitUI.RevisionGridClasses
{
    public sealed partial class DvcsGraph : DataGridView
    {
        #region Delegates

        public class LoadingEventArgs : EventArgs
        {
            public LoadingEventArgs(bool isLoading)
            {
                IsLoading = isLoading;
            }

            public bool IsLoading { get; }
        }

        #endregion

        #region DataType enum

        [Flags]
        public enum DataType
        {
            Normal = 0,
            Active = 1,
            Special = 2,
            Filtered = 4,
        }

        #endregion

        #region FilterType enum

        public enum FilterType
        {
            None,
            Highlight,
            Hide,
        }

        #endregion

        private int _nodeDimension = DpiUtil.Scale(10);
        private int _laneWidth = DpiUtil.Scale(13);
        private int _laneLineWidth = DpiUtil.Scale(2);
        private const int MaxLanes = 40;

        private Pen _whiteBorderPen;
        private Pen _blackBorderPen;

        private readonly AutoResetEvent _backgroundEvent = new AutoResetEvent(false);
        private readonly Graph _graphData;
        private readonly Dictionary<Junction, int> _junctionColors = new Dictionary<Junction, int>();
        private readonly Color _nonRelativeColor = Color.LightGray;

        private readonly Color[] _possibleColors =
            {
                Color.Red,
                Color.MistyRose,
                Color.Magenta,
                Color.Violet,
                Color.Blue,
                Color.Azure,
                Color.Cyan,
                Color.SpringGreen,
                Color.Green,
                Color.Chartreuse,
                Color.Gold,
                Color.Orange
            };

        private int _backgroundScrollTo;
        private readonly Thread _backgroundThread;
        private volatile bool _shouldRun = LicenseManager.UsageMode != LicenseUsageMode.Designtime;
        private int _cacheCount; // Number of elements in the cache.
        private int _cacheCountMax; // Number of elements allowed in the cache. Is based on control height.
        private int _cacheHead = -1; // The 'slot' that is the head of the circular bitmap
        private int _cacheHeadRow; // The node row that is in the head slot
        private FilterType _filterMode = FilterType.None;
        private Bitmap _graphBitmap;
        private int _graphDataCount;
        private Graphics _graphWorkArea;
        private int _rowHeight; // Height of elements in the cache. Is equal to the control's row height.
        private int _visibleBottom;
        private int _visibleTop;

        public void SetDimensions(int nodeDimension, int laneWidth, int laneLineWidth, int rowHeight)
        {
            RowTemplate.Height = rowHeight;
            _nodeDimension = DpiUtil.Scale(nodeDimension);
            _laneWidth = DpiUtil.Scale(laneWidth);
            _laneLineWidth = DpiUtil.Scale(laneLineWidth);

            dataGrid_Resize(null, null);
        }

        public DvcsGraph()
        {
            _graphData = new Graph();

            _backgroundThread = new Thread(BackgroundThreadEntry)
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal,
                Name = "DvcsGraph.backgroundThread"
            };
            _backgroundThread.Start();

            InitializeComponent();

            ColumnHeadersDefaultCellStyle.Font = SystemFonts.DefaultFont;
            Font = SystemFonts.DefaultFont;
            DefaultCellStyle.Font = SystemFonts.DefaultFont;
            AlternatingRowsDefaultCellStyle.Font = SystemFonts.DefaultFont;
            RowsDefaultCellStyle.Font = SystemFonts.DefaultFont;
            RowHeadersDefaultCellStyle.Font = SystemFonts.DefaultFont;
            RowTemplate.DefaultCellStyle.Font = SystemFonts.DefaultFont;

            _whiteBorderPen = new Pen(Brushes.White, _laneLineWidth);
            _blackBorderPen = new Pen(Brushes.Black, _laneLineWidth + 1);

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            ColumnWidthChanged += dataGrid_ColumnWidthChanged;
            Scroll += dataGrid_Scroll;
            _graphData.Updated += graphData_Updated;

            VirtualMode = true;
            Clear();
        }

        protected override void OnCreateControl()
        {
            DataGridViewColumn dataGridColumnGraph;
            if (ColumnCount <= 0 || GraphColumn.HeaderText != "")
            {
                dataGridColumnGraph = new DataGridViewTextBoxColumn();
            }
            else
            {
                dataGridColumnGraph = GraphColumn;
            }

            dataGridColumnGraph.HeaderText = "";
            dataGridColumnGraph.Frozen = true;
            dataGridColumnGraph.Name = "dataGridColumnGraph";
            dataGridColumnGraph.ReadOnly = true;
            dataGridColumnGraph.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridColumnGraph.Width = 70;
            dataGridColumnGraph.DefaultCellStyle.Font = SystemFonts.DefaultFont;
            if (ColumnCount == 0 || GraphColumn.HeaderText != "")
            {
                Columns.Insert(0, dataGridColumnGraph);
            }
        }

        /// <summary>
        /// 0
        /// </summary>
        internal DataGridViewColumn GraphColumn => Columns[0];

        /// <summary>
        /// 1
        /// </summary>
        internal DataGridViewColumn MessageColumn => Columns[1];

        /// <summary>
        /// 2
        /// </summary>
        internal DataGridViewColumn AuthorColumn => Columns[2];

        /// <summary>
        /// 3
        /// </summary>
        internal DataGridViewColumn DateColumn => Columns[3];

        internal DataGridViewColumn IdColumn => Columns[4];

        public void ShowAuthor(bool show)
        {
            AuthorColumn.Visible = show;
            DateColumn.Visible = show;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Justification = "It looks like such lock was made intentionally but it is better to rewrite this")]
        [DefaultValue(FilterType.None)]
        [Category("Behavior")]
        public FilterType FilterMode
        {
            get { return _filterMode; }
            set
            {
                // TODO: We only need to rebuild the graph if switching to or from hide
                if (_filterMode == value)
                {
                    return;
                }

                this.InvokeSync(() =>
                    {
                        lock (_backgroundEvent)
                        {
                            // Make sure the background thread isn't running
                            lock (_backgroundThread)
                            {
                                _backgroundScrollTo = 0;
                                _graphDataCount = 0;
                            }

                            lock (_graphData)
                            {
                                _filterMode = value;
                                _graphData.IsFilter = (_filterMode & FilterType.Hide) == FilterType.Hide;
                                RebuildGraph();
                            }
                        }
                    });
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public object[] SelectedData
        {
            get
            {
                if (SelectedRows.Count == 0)
                {
                    return null;
                }

                var data = new object[SelectedRows.Count];
                for (int i = 0; i < SelectedRows.Count; i++)
                {
                    data[i] = _graphData[i].Node.Data;
                }

                return data;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string[] SelectedIds
        {
            get
            {
                if (SelectedRows.Count == 0)
                {
                    return null;
                }

                var data = new string[SelectedRows.Count];
                for (int i = 0; i < SelectedRows.Count; i++)
                {
                    if (_graphData[SelectedRows[i].Index] != null)
                    {
                        data[SelectedRows.Count - 1 - i] = _graphData[SelectedRows[i].Index].Node.Id;
                    }
                }

                return data;
            }
            set
            {
                string[] currentSelection = SelectedIds;
                if (value != null && currentSelection != null && value.SequenceEqual(currentSelection))
                {
                    return;
                }

                lock (_backgroundEvent)
                {
                    lock (_graphData)
                    {
                        ClearSelection();
                        CurrentCell = null;
                        if (value == null)
                        {
                            return;
                        }

                        foreach (string rowItem in value)
                        {
                            int? row = TryGetRevisionIndex(rowItem);
                            if (row.HasValue && row.Value >= 0 && Rows.Count > row.Value)
                            {
                                Rows[row.Value].Selected = true;
                                if (CurrentCell == null)
                                {
                                    // Set the current cell to the first item. We use cell
                                    // 1 because cell 0 could be hidden if they've chosen to
                                    // not see the graph
                                    CurrentCell = Rows[row.Value].Cells[1];
                                }
                            }
                        }
                    }
                }
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
                if (_whiteBorderPen != null)
                {
                    _whiteBorderPen.Dispose();
                    _whiteBorderPen = null;
                }

                if (_blackBorderPen != null)
                {
                    _blackBorderPen.Dispose();
                    _blackBorderPen = null;
                }

                if (_graphBitmap != null)
                {
                    _graphBitmap.Dispose();
                    _graphBitmap = null;
                }

                if (_backgroundEvent != null)
                {
                    _backgroundEvent.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        [Description("Loading Handler. NOTE: This will often happen on a background thread so UI operations may not be safe!")]
        [Category("Behavior")]
        public event EventHandler<LoadingEventArgs> Loading;

        public void ShowRevisionGraph()
        {
            GraphColumn.Visible = true;
            ////updateData();
            _backgroundEvent.Set();
        }

        public void HideRevisionGraph()
        {
            GraphColumn.Visible = false;
            ////updateData();
            _backgroundEvent.Set();
        }

        [DefaultValue(true)]
        [Browsable(false)]
        public bool RevisionGraphVisible => GraphColumn.Visible;

        public void Add(string id, string[] parentIds, DataType type, GitRevision data)
        {
            lock (_graphData)
            {
                _graphData.Add(id, parentIds, type, data);
            }

            UpdateData();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Justification = "It looks like such lock was made intentionally but it is better to rewrite this")]
        public void Clear()
        {
            lock (_backgroundThread)
            {
                _backgroundScrollTo = 0;
            }

            lock (_graphData)
            {
                SetRowCount(0);
                _junctionColors.Clear();
                _graphData.Clear();
                _graphDataCount = 0;
                RebuildGraph();
            }

            _filterMode = FilterType.None;
        }

        public void FilterClear()
        {
            lock (_graphData)
            {
                foreach (Node n in _graphData.Nodes.Values)
                {
                    n.IsFiltered = false;
                }

                _graphData.IsFilter = false;
            }
        }

        public void Filter(string id)
        {
            lock (_graphData)
            {
                _graphData.Filter(id);
            }
        }

        public bool RowIsRelative(int rowIndex)
        {
            lock (_graphData)
            {
                Graph.ILaneRow row = _graphData[rowIndex];
                if (row == null)
                {
                    return false;
                }

                if (row.Node.Ancestors.Count > 0)
                {
                    return row.Node.Ancestors[0].IsRelative;
                }

                return true;
            }
        }

        public GitRevision GetRowData(int rowIndex)
        {
            lock (_graphData)
            {
                return _graphData[rowIndex]?.Node.Data;
            }
        }

        public string GetRowId(int rowIndex)
        {
            lock (_graphData)
            {
                return _graphData[rowIndex]?.Node.Id;
            }
        }

        public int FindRow(string id)
        {
            lock (_graphData)
            {
                int i;
                for (i = 0; i < _graphData.CachedCount; i++)
                {
                    if (_graphData[i] != null && _graphData[i].Node.Id.CompareTo(id) == 0)
                    {
                        break;
                    }
                }

                return i == _graphData.Count ? -1 : i;
            }
        }

        public void Prune()
        {
            int count;
            lock (_graphData)
            {
                _graphData.Prune();
                count = _graphData.Count;
                SetRowCount(count);
            }
        }

        private void RebuildGraph()
        {
            // Redraw
            _cacheHead = -1;
            _cacheHeadRow = 0;
            ClearDrawCache();
            UpdateData();
            Invalidate(true);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Justification = "It looks like such lock was made intentionally but it is better to rewrite this")]
        private void SetRowCount(int count)
        {
            if (InvokeRequired)
            {
                // DO NOT INVOKE! The RowCount is fixed at other strategic points in time.
                // -Doing this in synch can lock up the application
                // -Doing this asynch causes the scrollbar to flicker and eats performance
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

        private void graphData_Updated(object graph)
        {
            // We have to post this since the thread owns a lock on GraphData that we'll
            // need in order to re-draw the graph.
            this.InvokeAsync(() =>
                {
                    ClearDrawCache();
                    Invalidate();
                })
                .FileAndForget();
        }

        public void dataGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (Rows[e.RowIndex].Height != RowTemplate.Height)
            {
                Rows[e.RowIndex].Height = RowTemplate.Height;
                dataGrid_Scroll(null, null);
            }

            if ((e.State & DataGridViewElementStates.Visible) == 0 || e.ColumnIndex != 0)
            {
                return;
            }

            Rectangle srcRect = DrawGraph(e.RowIndex);
            if (!srcRect.IsEmpty)
            {
                e.Graphics.DrawImage(
                        _graphBitmap,
                        e.CellBounds,
                        srcRect,
                        GraphicsUnit.Pixel);
            }

            e.Handled = true;
        }

        private void dataGrid_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            ClearDrawCache();
        }

        private void dataGrid_Scroll(object sender, ScrollEventArgs e)
        {
            UpdateData();
            UpdateColumnWidth();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Justification = "It looks like such lock was made intentionally but it is better to rewrite this")]
        private void BackgroundThreadEntry()
        {
            while (_shouldRun)
            {
                if (_backgroundEvent.WaitOne(500))
                {
                    lock (_backgroundEvent)
                    {
                        if (!_shouldRun)
                        {
                            return;
                        }

                        int scrollTo;
                        lock (_backgroundThread)
                        {
                            scrollTo = _backgroundScrollTo;
                        }

                        int curCount;
                        lock (_graphData)
                        {
                            curCount = _graphDataCount;
                            _graphDataCount = _graphData.CachedCount;
                        }

                        if (RevisionGraphVisible)
                        {
                            UpdateGraph(curCount, scrollTo);
                        }
                        else
                        {
                            // do nothing... do not cache, the graph is invisible
                            Thread.Sleep(10);
                        }
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Justification = "It looks like such lock was made intentionally but it is better to rewrite this")]
        private void UpdateGraph(int curCount, int scrollTo)
        {
            lock (_graphData)
            {
                while (curCount < scrollTo)
                {
                    // Cache the next item
                    if (!_graphData.CacheTo(curCount))
                    {
                        Debug.WriteLine("Cached item FAILED {0}", curCount.ToString());
                        lock (_backgroundThread)
                        {
                            _backgroundScrollTo = curCount;
                        }

                        break;
                    }

                    // Update the row (if needed)
                    if (curCount == Math.Min(scrollTo, _visibleBottom) - 1)
                    {
                        this.InvokeAsync(o => UpdateRow(o), curCount).FileAndForget();
                    }

                    int count = 0;
                    if (FirstDisplayedCell != null)
                    {
                        count = FirstDisplayedCell.RowIndex + DisplayedRowCount(true);
                    }

                    if (curCount == count)
                    {
                        this.InvokeAsync(UpdateColumnWidth).FileAndForget();
                    }

                    curCount = _graphData.CachedCount;
                    _graphDataCount = curCount;
                }
            }
        }

        private void UpdateData()
        {
            _visibleTop = FirstDisplayedCell?.RowIndex ?? 0;
            _visibleBottom = _rowHeight > 0 ? _visibleTop + (Height / _rowHeight) : _visibleTop;

            // Add 5 for safe merge (1 for rounding and 1 for whitespace)....
            if (_visibleBottom + 2 > _graphData.Count)
            {
                // Currently we are doing some important work; we are recieving
                // rows that the user is viewing
                if (Loading != null && _graphData.Count > RowCount) //// && graphData.Count != RowCount)
                {
                    Loading(this, new LoadingEventArgs(true));
                }
            }
            else
            {
                // All rows that the user is viewing are loaded. We now can hide the loading
                // animation that is shown. (the event Loading(bool) triggers this!)
                Loading?.Invoke(this, new LoadingEventArgs(false));
            }

            if (_visibleBottom >= _graphData.Count)
            {
                _visibleBottom = _graphData.Count;
            }

            int targetBottom = _visibleBottom + 250;
            targetBottom = Math.Min(targetBottom, _graphData.Count);
            if (_backgroundScrollTo < targetBottom)
            {
                _backgroundScrollTo = targetBottom;
                _backgroundEvent.Set();
            }
        }

        private void UpdateRow(int row)
        {
            if (RowCount < _graphData.Count)
            {
                lock (_graphData)
                {
                    SetRowCount(_graphData.Count);
                }
            }

            // We only need to invalidate if the row is visible
            if (_visibleBottom >= row &&
                _visibleTop <= row &&
                row < RowCount)
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

        public bool UpdatingVisibleRows { get; private set; }

        private void UpdateColumnWidth()
        {
            // Auto scale width on scroll
            if (GraphColumn.Visible)
            {
                int laneCount = 2;
                if (_graphData != null)
                {
                    int width = 1;
                    int start = VerticalScrollBar.Value / _rowHeight;
                    int stop = start + DisplayedRowCount(true);
                    lock (_graphData)
                    {
                        for (int i = start; i < stop && _graphData[i] != null; i++)
                        {
                            width = Math.Max(_graphData[i].Count, width);
                        }
                    }

                    // When 'git log --first-parent' filtration is enabled and when only current
                    // branch needed to be rendered (and this filter actually works),
                    // it is much more readable to limit max lanes to 1.
                    int maxLanes =
                        (AppSettings.ShowFirstParent &&
                        AppSettings.ShowCurrentBranchOnly &&
                        AppSettings.BranchFilterEnabled) ? 1 : MaxLanes;
                    laneCount = Math.Min(Math.Max(laneCount, width), maxLanes);
                }

                if (GraphColumn.Width != _laneWidth * laneCount && _laneWidth * laneCount > GraphColumn.MinimumWidth)
                {
                    GraphColumn.Width = _laneWidth * laneCount;
                }
            }
        }

        // Color of non-relative branches.

        private List<Color> GetJunctionColors(IEnumerable<Junction> junction)
        {
            var colors = junction.Select(GetJunctionColor).ToList();

            if (colors.Count == 0)
            {
                colors.Add(Color.Black);
            }

            return colors;
        }

        private RevisionGraphDrawStyleEnum _revisionGraphDrawStyle;
        [DefaultValue(RevisionGraphDrawStyleEnum.DrawNonRelativesGray)]
        [Browsable(false)]
        public RevisionGraphDrawStyleEnum RevisionGraphDrawStyle
        {
            get
            {
                if (_revisionGraphDrawStyle == RevisionGraphDrawStyleEnum.HighlightSelected)
                {
                    return _revisionGraphDrawStyle;
                }

                if (AppSettings.RevisionGraphDrawNonRelativesGray)
                {
                    return RevisionGraphDrawStyleEnum.DrawNonRelativesGray;
                }

                return RevisionGraphDrawStyleEnum.Normal;
            }
            set
            {
                _revisionGraphDrawStyle = value;
            }
        }

        // http://en.wikipedia.org/wiki/File:RBG_color_wheel.svg

        private Color GetJunctionColor(Junction junction)
        {
            // Draw non-relative branches gray
            if (!junction.IsRelative && _revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.DrawNonRelativesGray)
            {
                return _nonRelativeColor;
            }

            // Draw non-highlighted branches gray
            if (!junction.HighLight && _revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.HighlightSelected)
            {
                return _nonRelativeColor;
            }

            if (!AppSettings.MulticolorBranches)
            {
                return AppSettings.GraphColor;
            }

            // This is the order to grab the colors in.
            int[] preferedColors = { 4, 8, 6, 10, 2, 5, 7, 3, 9, 1, 11 };

            int colorIndex;
            if (_junctionColors.TryGetValue(junction, out colorIndex))
            {
                return _possibleColors[colorIndex];
            }

            // Get adjacent junctions
            var adjacentJunctions = new List<Junction>();
            var adjacentColors = new List<int>();
            adjacentJunctions.AddRange(junction.Youngest.Ancestors);
            adjacentJunctions.AddRange(junction.Youngest.Descendants);
            adjacentJunctions.AddRange(junction.Oldest.Ancestors);
            adjacentJunctions.AddRange(junction.Oldest.Descendants);
            foreach (Junction peer in adjacentJunctions)
            {
                if (_junctionColors.TryGetValue(peer, out colorIndex))
                {
                    adjacentColors.Add(colorIndex);
                }
                else
                {
                    colorIndex = -1;
                }
            }

            if (adjacentColors.Count == 0)
            {
                // This is an end-point. We need to 'pick' a new color
                colorIndex = 0;
            }
            else
            {
                // This is a parent branch, calculate new color based on parent branch
                int start = adjacentColors[0];
                int i;
                for (i = 0; i < preferedColors.Length; i++)
                {
                    colorIndex = (start + preferedColors[i]) % _possibleColors.Length;
                    if (!adjacentColors.Contains(colorIndex))
                    {
                        break;
                    }
                }

                if (i == preferedColors.Length)
                {
                    var r = new Random();
                    colorIndex = r.Next(preferedColors.Length);
                }
            }

            _junctionColors[junction] = colorIndex;
            return _possibleColors[colorIndex];
        }

        public override void Refresh()
        {
            ClearDrawCache();
            base.Refresh();
        }

        private void ClearDrawCache()
        {
            _cacheHead = 0;
            _cacheCount = 0;
        }

        private Rectangle DrawGraph(int neededRow)
        {
            if (neededRow < 0 || _graphData.Count == 0 || _graphData.Count <= neededRow)
            {
                return Rectangle.Empty;
            }

            #region Make sure the graph cache bitmap is setup

            int height = _cacheCountMax * _rowHeight;
            int width = GraphColumn.Width;
            if (_graphBitmap == null ||

                // Resize the bitmap when the with or height is changed. The height won't change very often.
                // The with changes more often, when branches become visible/invisible.
                // Try to be 'smart' and not resize the bitmap for each little change. Enlarge when needed
                // but never shrink the bitmap since the huge performance hit is worse than the little extra memory.
                _graphBitmap.Width < width || _graphBitmap.Height != height)
            {
                if (_graphBitmap != null)
                {
                    _graphBitmap.Dispose();
                    _graphBitmap = null;
                }

                if (width > 0 && height > 0)
                {
                    _graphBitmap = new Bitmap(Math.Max(width, _laneWidth * 3), height, PixelFormat.Format32bppPArgb);
                    _graphWorkArea = Graphics.FromImage(_graphBitmap);
                    _graphWorkArea.SmoothingMode = SmoothingMode.AntiAlias;
                    _cacheHead = 0;
                    _cacheCount = 0;
                }
                else
                {
                    return Rectangle.Empty;
                }
            }

            #endregion

            // Compute how much the head needs to move to show the requested item.
            int neededHeadAdjustment = neededRow - _cacheHead;
            if (neededHeadAdjustment > 0)
            {
                neededHeadAdjustment -= _cacheCountMax - 1;
                if (neededHeadAdjustment < 0)
                {
                    neededHeadAdjustment = 0;
                }
            }

            int newRows = 0;
            if (_cacheCount < _cacheCountMax)
            {
                newRows = (neededRow - _cacheCount) + 1;
            }

            // Adjust the head of the cache
            _cacheHead = _cacheHead + neededHeadAdjustment;
            _cacheHeadRow = (_cacheHeadRow + neededHeadAdjustment) % _cacheCountMax;
            if (_cacheHeadRow < 0)
            {
                _cacheHeadRow = _cacheCountMax + _cacheHeadRow;
            }

            int start;
            int end;
            if (newRows > 0)
            {
                start = _cacheHead + _cacheCount;
                _cacheCount = Math.Min(_cacheCount + newRows, _cacheCountMax);
                end = _cacheHead + _cacheCount;
            }
            else if (neededHeadAdjustment > 0)
            {
                end = _cacheHead + _cacheCount;
                start = Math.Max(_cacheHead, end - neededHeadAdjustment);
            }
            else if (neededHeadAdjustment < 0)
            {
                start = _cacheHead;
                end = start + Math.Min(_cacheCountMax, -neededHeadAdjustment);
            }
            else
            {
                // Item already in the cache
                return CreateRectangle(neededRow, width);
            }

            if (RevisionGraphVisible)
            {
                if (!DrawVisibleGraph(start, end))
                {
                    return Rectangle.Empty;
                }
            }

            return CreateRectangle(neededRow, width);
        }

        private bool DrawVisibleGraph(int start, int end)
        {
            for (int rowIndex = start; rowIndex < end; rowIndex++)
            {
                Graph.ILaneRow row = _graphData[rowIndex];
                if (row == null)
                {
                    // This shouldn't be happening...If it does, clear the cache so we
                    // eventually pick it up.
                    Debug.WriteLine("Draw lane {0} NO DATA", rowIndex.ToString());
                    ClearDrawCache();
                    return false;
                }

                Region oldClip = _graphWorkArea.Clip;

                // Get the x,y value of the current item's upper left in the cache
                int curCacheRow = (_cacheHeadRow + rowIndex - _cacheHead) % _cacheCountMax;
                const int x = 0;
                int y = curCacheRow * _rowHeight;

                var laneRect = new Rectangle(0, y, Width, _rowHeight);
                if (rowIndex == start || curCacheRow == 0)
                {
                    // Draw previous row first. Clip top to row. We also need to clear the area
                    // before we draw since nothing else would clear the top 1/2 of the item to draw.
                    _graphWorkArea.RenderingOrigin = new Point(x, y - _rowHeight);
                    var newClip = new Region(laneRect);
                    _graphWorkArea.Clip = newClip;
                    _graphWorkArea.Clear(Color.Transparent);
                    DrawItem(_graphWorkArea, _graphData[rowIndex - 1]);
                    _graphWorkArea.Clip = oldClip;
                }

                bool isLast = rowIndex == end - 1;
                if (isLast)
                {
                    var newClip = new Region(laneRect);
                    _graphWorkArea.Clip = newClip;
                }

                _graphWorkArea.RenderingOrigin = new Point(x, y);
                bool success = DrawItem(_graphWorkArea, row);

                _graphWorkArea.Clip = oldClip;

                if (!success)
                {
                    ClearDrawCache();
                    return false;
                }
            }

            return true;
        }

        private Rectangle CreateRectangle(int neededRow, int width)
        {
            return new Rectangle(
                0,
                ((_cacheHeadRow + neededRow - _cacheHead) % _cacheCountMax) * RowTemplate.Height,
                width,
                _rowHeight);
        }

        // end drawGraph

        private RevisionGraphDrawStyleEnum _revisionGraphDrawStyleCache;
        private bool DrawItem(Graphics wa, Graph.ILaneRow row)
        {
            if (row == null || row.NodeLane == -1)
            {
                return false;
            }

            // Clip to the area we're drawing in, but draw 1 pixel past so
            // that the top/bottom of the line segment's anti-aliasing isn't
            // visible in the final rendering.
            int top = wa.RenderingOrigin.Y + (_rowHeight / 2);
            var laneRect = new Rectangle(0, top, Width, _rowHeight);
            Region oldClip = wa.Clip;
            var newClip = new Region(laneRect);
            newClip.Intersect(oldClip);
            wa.Clip = newClip;
            wa.Clear(Color.Transparent);

            // Getting RevisionGraphDrawStyle results in call to AppSettings. This is not very cheap, cache.
            _revisionGraphDrawStyleCache = RevisionGraphDrawStyle;

            ////for (int r = 0; r < 2; r++)
            for (int lane = 0; lane < row.Count; lane++)
            {
                int mid = wa.RenderingOrigin.X + (int)((lane + 0.5) * _laneWidth);

                for (int item = 0; item < row.LaneInfoCount(lane); item++)
                {
                    Graph.LaneInfo laneInfo = row[lane, item];

                    bool highLight = (_revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.DrawNonRelativesGray && laneInfo.Junctions.Any(j => j.IsRelative)) ||
                                     (_revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.HighlightSelected && laneInfo.Junctions.Any(j => j.HighLight)) ||
                                     (_revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.Normal);

                    List<Color> curColors = GetJunctionColors(laneInfo.Junctions);

                    // Create the brush for drawing the line
                    Brush brushLineColor = null;
                    Pen brushLineColorPen = null;
                    try
                    {
                        bool drawBorder = highLight && AppSettings.BranchBorders; // hide border for "non-relatives"

                        if (curColors.Count == 1 || !AppSettings.StripedBranchChange)
                        {
                            if (curColors[0] != _nonRelativeColor)
                            {
                                brushLineColor = new SolidBrush(curColors[0]);
                            }
                            else if (curColors.Count > 1 && curColors[1] != _nonRelativeColor)
                            {
                                brushLineColor = new SolidBrush(curColors[1]);
                            }
                            else
                            {
                                drawBorder = false;
                                brushLineColor = new SolidBrush(_nonRelativeColor);
                            }
                        }
                        else
                        {
                            Color lastRealColor = curColors.LastOrDefault(c => c != _nonRelativeColor);

                            if (lastRealColor.IsEmpty)
                            {
                                brushLineColor = new SolidBrush(_nonRelativeColor);
                                drawBorder = false;
                            }
                            else
                            {
                                brushLineColor = new HatchBrush(HatchStyle.DarkDownwardDiagonal, curColors[0], lastRealColor);
                            }
                        }

                        // Precalculate line endpoints
                        bool singleLane = laneInfo.ConnectLane == lane;
                        int x0 = mid;
                        int y0 = top - 1;
                        int x1 = singleLane ? x0 : mid + ((laneInfo.ConnectLane - lane) * _laneWidth);
                        int y1 = top + _rowHeight;

                        Point p0 = new Point(x0, y0);
                        Point p1 = new Point(x1, y1);

                        // Precalculate curve control points when needed
                        Point c0, c1;
                        if (singleLane)
                        {
                            c0 = c1 = Point.Empty;
                        }
                        else
                        {
                            // Controls the curvature of cross-lane lines (0 = straight line, 1 = 90 degree turns)
                            const float severity = 0.5f;
                            c0 = new Point(x0, (int)((y0 * (1.0f - severity)) + (y1 * severity)));
                            c1 = new Point(x1, (int)((y1 * (1.0f - severity)) + (y0 * severity)));
                        }

                        for (int i = drawBorder ? 0 : 2; i < 3; i++)
                        {
                            Pen penLine;
                            switch (i)
                            {
                                case 0:
                                    penLine = _whiteBorderPen;
                                    break;
                                case 1:
                                    penLine = _blackBorderPen;
                                    break;
                                default:
                                    if (brushLineColorPen == null)
                                    {
                                        brushLineColorPen = new Pen(brushLineColor, _laneLineWidth);
                                    }

                                    penLine = brushLineColorPen;
                                    break;
                            }

                            if (singleLane)
                            {
                                wa.DrawLine(penLine, p0, p1);
                            }
                            else
                            {
                                wa.DrawBezier(penLine, p0, c0, c1, p1);
                            }
                        }
                    }
                    finally
                    {
                        brushLineColorPen?.Dispose();
                        ((IDisposable)brushLineColor)?.Dispose();
                    }
                }
            }

            // Reset the clip region
            wa.Clip = oldClip;
            {
                // Draw node
                var nodeRect = new Rectangle(
                    wa.RenderingOrigin.X + ((_laneWidth - _nodeDimension) / 2) + (row.NodeLane * _laneWidth),
                    wa.RenderingOrigin.Y + ((_rowHeight - _nodeDimension) / 2),
                    _nodeDimension,
                    _nodeDimension);

                Brush nodeBrush;

                List<Color> nodeColors = GetJunctionColors(row.Node.Ancestors);

                bool highlight = (_revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.DrawNonRelativesGray && row.Node.Ancestors.Any(j => j.IsRelative)) ||
                                 (_revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.HighlightSelected && row.Node.Ancestors.Any(j => j.HighLight)) ||
                                 (_revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.Normal);

                bool drawBorder = AppSettings.BranchBorders && highlight;

                if (nodeColors.Count == 1)
                {
                    nodeBrush = new SolidBrush(highlight ? nodeColors[0] : _nonRelativeColor);
                    if (nodeColors[0] == _nonRelativeColor)
                    {
                        drawBorder = false;
                    }
                }
                else
                {
                    nodeBrush = new LinearGradientBrush(nodeRect, nodeColors[0], nodeColors[1],
                                                        LinearGradientMode.Horizontal);
                    if (nodeColors.All(c => c == _nonRelativeColor))
                    {
                        drawBorder = false;
                    }
                }

                if (_filterMode == FilterType.Highlight && row.Node.IsFiltered)
                {
                    Rectangle highlightRect = nodeRect;
                    highlightRect.Inflate(2, 3);
                    wa.FillRectangle(Brushes.Yellow, highlightRect);
                    wa.DrawRectangle(Pens.Black, highlightRect);
                }

                if (row.Node.Data == null)
                {
                    wa.FillEllipse(Brushes.White, nodeRect);
                    using (var pen = new Pen(Color.Red, 2))
                    {
                        wa.DrawEllipse(pen, nodeRect);
                    }
                }
                else if (row.Node.IsActive)
                {
                    wa.FillRectangle(nodeBrush, nodeRect);
                    nodeRect.Inflate(1, 1);
                    using (var pen = new Pen(Color.Black, 3))
                    {
                        wa.DrawRectangle(pen, nodeRect);
                    }
                }
                else if (row.Node.IsSpecial)
                {
                    wa.FillRectangle(nodeBrush, nodeRect);
                    if (drawBorder)
                    {
                        wa.DrawRectangle(Pens.Black, nodeRect);
                    }
                }
                else
                {
                    wa.FillEllipse(nodeBrush, nodeRect);
                    if (drawBorder)
                    {
                        wa.DrawEllipse(Pens.Black, nodeRect);
                    }
                }
            }

            return true;
        }

        public void HighlightBranch(string id)
        {
            _graphData.HighlightBranch(id);
            Update();
        }

        public bool IsRevisionRelative(string guid)
        {
            return _graphData.IsRevisionRelative(guid);
        }

        public GitRevision GetRevision(string guid)
        {
            return _graphData.Nodes.TryGetValue(guid, out var node) ? node.Data : null;
        }

        public int? TryGetRevisionIndex(string guid)
        {
            if (Rows.Count == 0)
            {
                return null;
            }

            return guid != null && _graphData.Nodes.TryGetValue(guid, out var node) ? (int?)node.Index : null;
        }

        public IReadOnlyList<string> GetRevisionChildren(string guid)
        {
            var childrenIds = new List<string>();

            // We do not need a lock here since we load the data from the first commit and walkt through all
            // parents. Children are always loaded, since we start at the newest commit.
            // With lock, loading the commit info slows down terrible.
            if (_graphData.Nodes.TryGetValue(guid, out var node))
            {
                foreach (var descendant in node.Descendants)
                {
                    childrenIds.Add(descendant.ChildOf(node).Id);
                }
            }

            return childrenIds;
        }

        private void dataGrid_Resize(object sender, EventArgs e)
        {
            _rowHeight = RowTemplate.Height;

            // Keep an extra page in the cache
            _cacheCountMax = (Height * 2 / _rowHeight) + 1;
            ClearDrawCache();
            dataGrid_Scroll(null, null);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Home)
            {
                if (RowCount != 0)
                {
                    ClearSelection();
                    Rows[0].Selected = true;
                    CurrentCell = Rows[0].Cells[1];
                }

                return;
            }
            else if (e.KeyData == Keys.End)
            {
                if (RowCount != 0)
                {
                    ClearSelection();
                    Rows[RowCount - 1].Selected = true;
                    CurrentCell = Rows[RowCount - 1].Cells[1];
                }

                return;
            }

            base.OnKeyDown(e);
        }

        #region Nested type: Node

        private sealed class Node
        {
            public readonly List<Junction> Ancestors = new List<Junction>();
            public readonly List<Junction> Descendants = new List<Junction>();
            public readonly string Id;
            public GitRevision Data;
            public DataType DataType;
            public int InLane = int.MaxValue;
            public int Index = int.MaxValue;

            public Node(string id)
            {
                Id = id;
            }

            public bool IsActive => (DataType & DataType.Active) == DataType.Active;

            public bool IsFiltered
            {
                get { return (DataType & DataType.Filtered) == DataType.Filtered; }
                set
                {
                    if (value)
                    {
                        DataType |= DataType.Filtered;
                    }
                    else
                    {
                        DataType &= ~DataType.Filtered;
                    }
                }
            }

            public bool IsSpecial => (DataType & DataType.Special) == DataType.Special;

            public override string ToString()
            {
                if (Data == null)
                {
                    string name = Id;
                    if (name.Length > 8)
                    {
                        name = name.Substring(0, 4) + ".." + name.Substring(name.Length - 4, 4);
                    }

                    return string.Format("{0} ({1})", name, Index);
                }

                return Data.ToString();
            }
        }

        #endregion
    }

    // end of class DvcsGraph
}