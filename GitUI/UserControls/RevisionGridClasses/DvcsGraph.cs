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
using GitCommands.Git.Extensions;
using GitExtUtils.GitUI;

namespace GitUI.RevisionGridClasses
{
    public sealed partial class DvcsGraph : DataGridView
    {
        #region EventArgs

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
        public enum DataTypes
        {
            Normal = 0,
            Active = 1,
            Special = 2
        }

        #endregion

        private readonly int _nodeDimension = DpiUtil.Scale(10);
        private readonly int _laneWidth = DpiUtil.Scale(13);
        private readonly int _laneSidePadding = DpiUtil.Scale(4);
        private readonly int _laneLineWidth = DpiUtil.Scale(2);
        private const int MaxLanes = 40;

        private Pen _whiteBorderPen;
        private Pen _blackBorderPen;

        private readonly AutoResetEvent _backgroundEvent = new AutoResetEvent(false);
        private readonly Dictionary<Junction, int> _colorByJunction = new Dictionary<Junction, int>();
        private readonly Color _nonRelativeColor = Color.LightGray;

        private readonly Graph _graphData = new Graph();

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
        private Bitmap _graphBitmap;
        private int _graphDataCount;
        private Graphics _graphWorkArea;
        private int _rowHeight; // Height of elements in the cache. Is equal to the control's row height.
        private int _visibleBottom;
        private int _visibleTop;

        public void SetRowHeight(int rowHeight)
        {
            RowTemplate.Height = rowHeight;

            dataGrid_Resize(null, null);
        }

        public DvcsGraph()
        {
            _backgroundThread = new Thread(BackgroundThreadEntry)
            {
                IsBackground = true,
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

        internal DataGridViewColumn GraphColumn => Columns[0];
        internal DataGridViewColumn MessageColumn => Columns[1];
        internal DataGridViewColumn AuthorColumn => Columns[2];
        internal DataGridViewColumn DateColumn => Columns[3];
        internal DataGridViewColumn IdColumn => Columns[4];

        public void ShowAuthor(bool show)
        {
            AuthorColumn.Visible = show;
            DateColumn.Visible = show;
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

        public void Add(GitRevision revision, DataTypes types)
        {
            lock (_graphData)
            {
                _graphData.Add(revision, types);
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
                _colorByJunction.Clear();
                _graphData.Clear();
                _graphDataCount = 0;
                RebuildGraph();
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

        private static bool AppendBranches(string prefix, ref System.Text.StringBuilder text, IEnumerable<GitUIPluginInterfaces.IGitRef> refs, ref HashSet<string> shown)
        {
            bool any = false;
            foreach (var gitref in refs)
            {
                if (!AppSettings.ShowRemoteBranches && !gitref.Remote.IsNullOrEmpty())
                {
                    continue;
                }

                text.Append(any ? ", " : prefix);
                any = true;
                text.Append(gitref.Name);
                shown.Add(gitref.Name);
            }

            return any;
        }

        public string GetLaneInfo(int row, int x, GitModule currentModule)
        {
            int lane = (x - _laneSidePadding) / _laneWidth;
            var laneInfoText = new System.Text.StringBuilder();
            lock (_graphData)
            {
                Graph.ILaneRow laneRow = _graphData[row];
                if (laneRow != null)
                {
                    DvcsGraph.Node node = null;
                    if (lane == laneRow.NodeLane)
                    {
                        node = laneRow.Node;
                        if (!node.Id.IsArtificial())
                        {
                            laneInfoText.AppendLine(node.Id);
                        }
                    }
                    else if (lane >= 0 && lane < laneRow.Count)
                    {
                        for (int laneInfoIndex = 0, laneInfoCount = laneRow.LaneInfoCount(lane); laneInfoIndex < laneInfoCount; ++laneInfoIndex)
                        {
                            // search for next node below this row
                            Graph.LaneInfo laneInfo = laneRow[lane, laneInfoIndex];
                            DvcsGraph.Junction firstJunction = laneInfo.Junctions.First();
                            for (int nodeIndex = 0, nodeCount = firstJunction.NodesCount; nodeIndex < nodeCount; ++nodeIndex)
                            {
                                DvcsGraph.Node laneNode = firstJunction[nodeIndex];
                                if (laneNode.Index > row)
                                {
                                    node = laneNode;
                                    break; // from for (nodes)
                                }
                            }
                        }
                    }

                    if (node != null)
                    {
                        var shownBranches = new HashSet<string>();
                        if (AppendBranches("at ", ref laneInfoText, node.Data.Refs, ref shownBranches))
                        {
                            laneInfoText.AppendLine();
                        }

                        const string headSuffix = "/HEAD";
                        const string remotesPrefix = "remotes/";
                        var branches
                            = currentModule.GetAllBranchesWhichContainGivenCommit(node.Id, getLocal: true, getRemote: AppSettings.ShowRemoteBranches)
                              .Where(branch => !branch.EndsWith(headSuffix)).ToList();
                        for (int branchIndex = 0, branchCount = branches.Count; branchIndex < branchCount; ++branchIndex)
                        {
                            if (branches[branchIndex].StartsWith(remotesPrefix))
                            {
                                branches[branchIndex] = branches[branchIndex].Remove(0, remotesPrefix.Length);
                            }
                        }

                        branches = branches.Except(shownBranches).ToList();
                        if (branches.Any())
                        {
                            const int MaximumDisplayedRefs = 20;
                            if (branches.Count > MaximumDisplayedRefs)
                            {
                                branches[MaximumDisplayedRefs - 2] = "…";
                                branches[MaximumDisplayedRefs - 1] = branches[branches.Count - 1];
                                branches.RemoveRange(MaximumDisplayedRefs, branches.Count - MaximumDisplayedRefs);
                            }

                            laneInfoText.Append("in ").Append(string.Join(", ", branches)).AppendLine();
                        }

                        if (laneInfoText.Length > 0)
                        {
                            laneInfoText.AppendLine();
                        }

                        laneInfoText.Append(node.Data.Subject);
                    }
                }
            }

            return laneInfoText.ToString();
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

        private void graphData_Updated()
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
                    GraphColumn.Width = (_laneWidth * laneCount) + (_laneSidePadding * 2);
                }
            }
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
                    return RevisionGraphDrawStyleEnum.HighlightSelected;
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

        // This is the order to grab the colors in.
        private static readonly int[] preferedColors = { 4, 8, 6, 10, 2, 5, 7, 3, 9, 1, 11 };

        private readonly List<int> _adjacentColors = new List<int>(capacity: 3);
        private readonly Random _random = new Random();

        private Color GetJunctionColor(Junction junction)
        {
            ThreadHelper.AssertOnUIThread();

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

            // See if this junciton's colour has already been calculated
            if (_colorByJunction.TryGetValue(junction, out var colorIndex))
            {
                return _possibleColors[colorIndex];
            }

            // NOTE we reuse _adjacentColors to avoid allocating lists during UI painting.
            // This is safe as we are always on the UI thread here.
            _adjacentColors.Clear();
            _adjacentColors.AddRange(
                from peer in GetPeers().SelectMany()
                where _colorByJunction.TryGetValue(peer, out colorIndex)
                select colorIndex);

            if (_adjacentColors.Count == 0)
            {
                // This is an end-point. We need to 'pick' a new color
                colorIndex = 0;
            }
            else
            {
                // This is a parent branch, calculate new color based on parent branch
                int start = _adjacentColors[0];
                int i;
                for (i = 0; i < preferedColors.Length; i++)
                {
                    colorIndex = (start + preferedColors[i]) % _possibleColors.Length;
                    if (!_adjacentColors.Contains(colorIndex))
                    {
                        break;
                    }
                }

                if (i == preferedColors.Length)
                {
                    colorIndex = _random.Next(preferedColors.Length);
                }
            }

            _colorByJunction[junction] = colorIndex;
            return _possibleColors[colorIndex];

            // Get adjacent (peer) junctions
            IEnumerable<IEnumerable<Junction>> GetPeers()
            {
                yield return junction.Youngest.Ancestors;
                yield return junction.Youngest.Descendants;
                yield return junction.Oldest.Ancestors;
                yield return junction.Oldest.Descendants;
            }
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

                if (_graphWorkArea != null)
                {
                    _graphWorkArea.Dispose();
                    _graphWorkArea = null;
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
                return CreateRectangle();
            }

            if (RevisionGraphVisible)
            {
                if (!DrawVisibleGraph(start, end))
                {
                    return Rectangle.Empty;
                }
            }

            return CreateRectangle();

            Rectangle CreateRectangle()
            {
                return new Rectangle(
                    0,
                    ((_cacheHeadRow + neededRow - _cacheHead) % _cacheCountMax) * RowTemplate.Height,
                    width,
                    _rowHeight);
            }
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
                int x = _laneSidePadding;
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

        // end drawGraph

        private RevisionGraphDrawStyleEnum _revisionGraphDrawStyleCache;
        private readonly List<Color> _junctionColors = new List<Color>(4);

        private bool DrawItem(Graphics wa, Graph.ILaneRow row)
        {
            ThreadHelper.AssertOnUIThread();

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

                    UpdateJunctionColors(laneInfo.Junctions);

                    // Create the brush for drawing the line
                    Brush lineBrush = null;
                    Pen linePen = null;
                    try
                    {
                        bool drawBorder = highLight && AppSettings.BranchBorders; // hide border for "non-relatives"

                        if (_junctionColors.Count == 1 || !AppSettings.StripedBranchChange)
                        {
                            if (_junctionColors[0] != _nonRelativeColor)
                            {
                                lineBrush = new SolidBrush(_junctionColors[0]);
                            }
                            else if (_junctionColors.Count > 1 && _junctionColors[1] != _nonRelativeColor)
                            {
                                lineBrush = new SolidBrush(_junctionColors[1]);
                            }
                            else
                            {
                                drawBorder = false;
                                lineBrush = new SolidBrush(_nonRelativeColor);
                            }
                        }
                        else
                        {
                            Color lastRealColor = _junctionColors.LastOrDefault(c => c != _nonRelativeColor);

                            if (lastRealColor.IsEmpty)
                            {
                                lineBrush = new SolidBrush(_nonRelativeColor);
                                drawBorder = false;
                            }
                            else
                            {
                                lineBrush = new HatchBrush(HatchStyle.DarkDownwardDiagonal, _junctionColors[0], lastRealColor);
                            }
                        }

                        // Precalculate line endpoints
                        bool sameLane = laneInfo.ConnectLane == lane;
                        int x0 = mid;
                        int y0 = top - 1;
                        int x1 = sameLane ? x0 : mid + ((laneInfo.ConnectLane - lane) * _laneWidth);
                        int y1 = top + _rowHeight;

                        Point p0 = new Point(x0, y0);
                        Point p1 = new Point(x1, y1);

                        // Precalculate curve control points when needed
                        Point c0, c1;
                        if (sameLane)
                        {
                            // We are drawing between two points in the same
                            // lane, so there will be no curve
                            c0 = c1 = default;
                        }
                        else
                        {
                            // Left shifting int is fast equivalent of dividing by two,
                            // thus computing the average of y0 and y1.
                            var yMid = (y0 + y1) >> 1;

                            c0 = new Point(x0, yMid);
                            c1 = new Point(x1, yMid);
                        }

                        for (int i = drawBorder ? 0 : 2; i < 3; i++)
                        {
                            Pen pen;
                            switch (i)
                            {
                                case 0:
                                    pen = _whiteBorderPen;
                                    break;
                                case 1:
                                    pen = _blackBorderPen;
                                    break;
                                default:
                                    if (linePen == null)
                                    {
                                        linePen = new Pen(lineBrush, _laneLineWidth);
                                    }

                                    pen = linePen;
                                    break;
                            }

                            if (sameLane)
                            {
                                wa.DrawLine(pen, p0, p1);
                            }
                            else
                            {
                                wa.DrawBezier(pen, p0, c0, c1, p1);
                            }
                        }
                    }
                    finally
                    {
                        linePen?.Dispose();
                        lineBrush?.Dispose();
                    }
                }
            }

            // Reset the clip region
            wa.Clip = oldClip;

            // Draw node
            var nodeRect = new Rectangle(
                wa.RenderingOrigin.X + ((_laneWidth - _nodeDimension) / 2) + (row.NodeLane * _laneWidth),
                wa.RenderingOrigin.Y + ((_rowHeight - _nodeDimension) / 2),
                _nodeDimension,
                _nodeDimension);

            Brush nodeBrush;

            UpdateJunctionColors(row.Node.Ancestors);

            bool highlight = (_revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.DrawNonRelativesGray && row.Node.Ancestors.Any(j => j.IsRelative)) ||
                             (_revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.HighlightSelected && row.Node.Ancestors.Any(j => j.HighLight)) ||
                             (_revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.Normal);

            bool drawNodeBorder = AppSettings.BranchBorders && highlight;

            if (_junctionColors.Count == 1)
            {
                nodeBrush = new SolidBrush(highlight ? _junctionColors[0] : _nonRelativeColor);
                if (_junctionColors[0] == _nonRelativeColor)
                {
                    drawNodeBorder = false;
                }
            }
            else
            {
                nodeBrush = new LinearGradientBrush(
                    nodeRect, _junctionColors[0], _junctionColors[1],
                    LinearGradientMode.Horizontal);
                if (_junctionColors.All(c => c == _nonRelativeColor))
                {
                    drawNodeBorder = false;
                }
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
                if (drawNodeBorder)
                {
                    wa.DrawRectangle(Pens.Black, nodeRect);
                }
            }
            else
            {
                wa.FillEllipse(nodeBrush, nodeRect);
                if (drawNodeBorder)
                {
                    wa.DrawEllipse(Pens.Black, nodeRect);
                }
            }

            nodeBrush.Dispose();

            return true;

            void UpdateJunctionColors(IEnumerable<Junction> junction)
            {
                _junctionColors.Clear();

                // Color of non-relative branches.
                _junctionColors.AddRange(junction.Select(GetJunctionColor));

                if (_junctionColors.Count == 0)
                {
                    _junctionColors.Add(Color.Black);
                }
            }
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

            // We do not need a lock here since we load the data from the first commit and walk through all
            // parents. Children are always loaded, since we start at the newest commit.
            // With lock, loading the commit info slows down terribly.
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

        #region Nested type: Node

        private sealed class Node
        {
            public List<Junction> Ancestors { get; } = new List<Junction>(capacity: 2);
            public List<Junction> Descendants { get; } = new List<Junction>(capacity: 2);
            public string Id { get; }

            public GitRevision Data { get; set; }
            public DataTypes DataTypes { get; set; }
            public int InLane { get; set; } = int.MaxValue;
            public int Index { get; set; } = int.MaxValue;

            public Node(string id)
            {
                Id = id;
            }

            public bool IsActive => DataTypes.HasFlag(DataTypes.Active);
            public bool IsSpecial => DataTypes.HasFlag(DataTypes.Special);

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