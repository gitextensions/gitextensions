using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class GraphColumnProvider : ColumnProvider
    {
        private const int MaxLanes = 40;

        private static readonly IReadOnlyList<Color> _graphColors = new[]
        {
            Color.FromArgb(240, 36, 117),
            Color.FromArgb(52, 152, 219),
            Color.FromArgb(46, 204, 113),
            Color.FromArgb(142, 68, 173),
            Color.FromArgb(231, 76, 60),
            Color.FromArgb(40, 40, 40),
            Color.FromArgb(26, 188, 156),
            Color.FromArgb(241, 196, 15)
        };

        private static readonly int _nodeDimension = DpiUtil.Scale(10);
        private static readonly int _laneWidth = DpiUtil.Scale(16);
        private static readonly int _laneLineWidth = DpiUtil.Scale(2);

        private readonly Color _nonRelativeColor = Color.LightGray;
        private readonly HashSet<int> _adjacentColors = new HashSet<int>();
        private readonly List<Color> _junctionColors = new List<Color>(capacity: 2);
        private readonly Random _random = new Random();

        private readonly RevisionGridControl _grid;
        private readonly GraphModel _graphModel;

        private RevisionGraphDrawStyleEnum _revisionGraphDrawStyleCache;
        private RevisionGraphDrawStyleEnum _revisionGraphDrawStyle;
        private int _cacheCount; // Number of elements in the cache.
        private int _cacheCountMax; // Number of elements allowed in the cache. Is based on control height.
        private int _cacheHead = -1; // The 'slot' that is the head of the circular bitmap
        private int _cacheHeadRow; // The node row that is in the head slot
        [CanBeNull] private Bitmap _graphBitmap;
        [CanBeNull] private Graphics _graphBitmapGraphics;

        public GraphColumnProvider(RevisionGridControl grid, GraphModel graphModel)
            : base("Graph")
        {
            _grid = grid;
            _graphModel = graphModel;

            // TODO is it worth creating a lighter-weight column type?

            Column = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Resizable = DataGridViewTriState.False
            };
        }

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
            set { _revisionGraphDrawStyle = value; }
        }

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in (Brush backBrush, Color foreColor, Font normalFont, Font boldFont) style)
        {
            if (AppSettings.ShowRevisionGridGraphColumn &&
                e.State.HasFlag(DataGridViewElementStates.Visible) &&
                e.RowIndex >= 0 &&
                _graphModel.Count != 0 &&
                _graphModel.Count > e.RowIndex &&
                PaintGraphCell(e.RowIndex, e.CellBounds, e.Graphics))
            {
                e.Handled = true;
            }

            return;

            bool PaintGraphCell(int rowIndex, Rectangle cellBounds, Graphics graphics)
            {
                // Draws the required row into _graphBitmap, or retrieves an equivalent one from the cache.

                int height = _cacheCountMax * rowHeight;
                int width = Column.Width;

                if (width <= 0 || height <= 0)
                {
                    return false;
                }

                EnsureCacheIsLargeEnough();

                // Compute how much the head needs to move to show the requested item.
                int neededHeadAdjustment = rowIndex - _cacheHead;
                if (neededHeadAdjustment > 0)
                {
                    neededHeadAdjustment -= _cacheCountMax - 1;
                    if (neededHeadAdjustment < 0)
                    {
                        neededHeadAdjustment = 0;
                    }
                }

                var newRows = _cacheCount < _cacheCountMax
                    ? (rowIndex - _cacheCount) + 1
                    : 0;

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
                    CreateRectangle();
                    return true;
                }

                if (!DrawVisibleGraph())
                {
                    return false;
                }

                CreateRectangle();
                return true;

                void CreateRectangle()
                {
                    var cellRect = new Rectangle(
                        0,
                        ((_cacheHeadRow + rowIndex - _cacheHead) % _cacheCountMax) * rowHeight,
                        width,
                        rowHeight);

                    graphics.DrawImage(
                        _graphBitmap,
                        cellBounds,
                        cellRect,
                        GraphicsUnit.Pixel);
                }

                bool DrawVisibleGraph()
                {
                    for (var index = start; index < end; index++)
                    {
                        var row = _graphModel.GetLaneRow(index);

                        if (row == null)
                        {
                            // This shouldn't be happening...If it does, clear the cache so we
                            // eventually pick it up.
                            Debug.WriteLine("Draw lane {0} NO DATA", index);
                            ClearDrawCache();
                            return false;
                        }

                        // Get the x,y value of the current item's upper left in the cache
                        var curCacheRow = (_cacheHeadRow + index - _cacheHead) % _cacheCountMax;
                        var x = ColumnLeftMargin;
                        var y = curCacheRow * rowHeight;

                        var laneRect = new Rectangle(0, y, width, rowHeight);
                        var oldClip = _graphBitmapGraphics.Clip;

                        if (index == start || curCacheRow == 0)
                        {
                            // Draw previous row first. Clip top to row. We also need to clear the area
                            // before we draw since nothing else would clear the top 1/2 of the item to draw.
                            _graphBitmapGraphics.RenderingOrigin = new Point(x, y - rowHeight);
                            _graphBitmapGraphics.Clip = new Region(laneRect);
                            _graphBitmapGraphics.Clear(Color.Transparent);
                            DrawItem(_graphBitmapGraphics, _graphModel.GetLaneRow(index - 1));
                            _graphBitmapGraphics.Clip = oldClip;
                        }

                        if (index == end - 1)
                        {
                            // Use a custom clip for the last row
                            _graphBitmapGraphics.Clip = new Region(laneRect);
                        }

                        _graphBitmapGraphics.RenderingOrigin = new Point(x, y);

                        var success = DrawItem(_graphBitmapGraphics, row);

                        _graphBitmapGraphics.Clip = oldClip;

                        if (!success)
                        {
                            ClearDrawCache();
                            return false;
                        }
                    }

                    return true;
                }

                void EnsureCacheIsLargeEnough()
                {
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

                        if (_graphBitmapGraphics != null)
                        {
                            _graphBitmapGraphics.Dispose();
                            _graphBitmapGraphics = null;
                        }

                        _graphBitmap = new Bitmap(
                            Math.Max(width, _laneWidth * 3),
                            height,
                            PixelFormat.Format32bppPArgb);
                        _graphBitmapGraphics = Graphics.FromImage(_graphBitmap);
                        _graphBitmapGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                        _cacheHead = 0;
                        _cacheCount = 0;
                    }
                }

                bool DrawItem(Graphics g, ILaneRow row)
                {
                    if (row == null || row.NodeLane == -1)
                    {
                        return false;
                    }

                    // Clip to the area we're drawing in, but draw 1 pixel past so
                    // that the top/bottom of the line segment's anti-aliasing isn't
                    // visible in the final rendering.
                    int top = g.RenderingOrigin.Y + (rowHeight / 2);
                    var laneRect = new Rectangle(0, top, width, rowHeight);
                    Region oldClip = g.Clip;
                    var newClip = new Region(laneRect);
                    newClip.Intersect(oldClip);
                    g.Clip = newClip;
                    g.Clear(Color.Transparent);

                    // Getting RevisionGraphDrawStyle results in call to AppSettings. This is not very cheap, cache.
                    _revisionGraphDrawStyleCache = RevisionGraphDrawStyle;

                    var oldSmoothingMode = g.SmoothingMode;

                    for (int lane = 0; lane < row.Count; lane++)
                    {
                        int mid = g.RenderingOrigin.X + (int)((lane + 0.5) * _laneWidth);

                        for (int item = 0; item < row.LaneInfoCount(lane); item++)
                        {
                            LaneInfo laneInfo = row[lane, item];

                            UpdateJunctionColors(laneInfo.Junctions);

                            // Create the brush for drawing the line
                            Brush lineBrush = null;
                            try
                            {
                                if (_junctionColors.Count == 1 || !AppSettings.StripedBranchChange)
                                {
                                    if (_junctionColors[0] != _nonRelativeColor)
                                    {
                                        lineBrush = new SolidBrush(GetAdjustedLineColor(_junctionColors[0]));
                                    }
                                    else if (_junctionColors.Count > 1 && _junctionColors[1] != _nonRelativeColor)
                                    {
                                        lineBrush = new SolidBrush(GetAdjustedLineColor(_junctionColors[1]));
                                    }
                                    else
                                    {
                                        lineBrush = new SolidBrush(GetAdjustedLineColor(_nonRelativeColor));
                                    }
                                }
                                else
                                {
                                    Color lastRealColor = _junctionColors.LastOrDefault(c => c != _nonRelativeColor);

                                    if (lastRealColor.IsEmpty)
                                    {
                                        lineBrush = new SolidBrush(GetAdjustedLineColor(_nonRelativeColor));
                                    }
                                    else
                                    {
                                        lineBrush = new HatchBrush(HatchStyle.DarkDownwardDiagonal, GetAdjustedLineColor(_junctionColors[0]), lastRealColor);
                                    }
                                }

                                Color GetAdjustedLineColor(Color c) => ColorHelper.MakeColorDarker(c, amount: 0.1);

                                // Precalculate line endpoints
                                bool sameLane = laneInfo.ConnectLane == lane;
                                int x0 = mid;
                                int y0 = top - 1;
                                int x1 = sameLane ? x0 : mid + ((laneInfo.ConnectLane - lane) * _laneWidth);
                                int y1 = top + rowHeight;

                                var p0 = new Point(x0, y0);
                                var p1 = new Point(x1, y1);

                                using (var linePen = new Pen(lineBrush, _laneLineWidth))
                                {
                                    if (sameLane)
                                    {
                                        g.SmoothingMode = SmoothingMode.None;
                                        g.DrawLine(linePen, p0, p1);
                                    }
                                    else
                                    {
                                        // Anti-aliasing seems to introduce an offset of two thirds
                                        // of a pixel to the right - compensate it.
                                        g.SmoothingMode = SmoothingMode.AntiAlias;
                                        float offset = -0.667F;

                                        // Left shifting int is fast equivalent of dividing by two,
                                        // thus computing the average of y0 and y1.
                                        var yMid = (y0 + y1) >> 1;
                                        var c0 = new PointF(offset + x0, yMid);
                                        var c1 = new PointF(offset + x1, yMid);
                                        var e0 = new PointF(offset + p0.X, p0.Y);
                                        var e1 = new PointF(offset + p1.X, p1.Y);
                                        g.DrawBezier(linePen, e0, c0, c1, e1);
                                    }
                                }
                            }
                            finally
                            {
                                lineBrush?.Dispose();
                            }
                        }
                    }

                    // Reset graphics options
                    g.Clip = oldClip;

                    // Draw node
                    var nodeRect = new Rectangle(
                        g.RenderingOrigin.X + ((_laneWidth - _nodeDimension) / 2) + (row.NodeLane * _laneWidth),
                        g.RenderingOrigin.Y + ((rowHeight - _nodeDimension) / 2),
                        _nodeDimension,
                        _nodeDimension);

                    Color? nodeColor = null;
                    Brush nodeBrush;

                    UpdateJunctionColors(row.Node.Ancestors);

                    bool highlight = (_revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.DrawNonRelativesGray && row.Node.Ancestors.Any(j => j.IsRelative)) ||
                                     (_revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.HighlightSelected && row.Node.Ancestors.Any(j => j.IsHighlighted)) ||
                                     (_revisionGraphDrawStyleCache == RevisionGraphDrawStyleEnum.Normal);

                    if (_junctionColors.Count == 1)
                    {
                        nodeColor = highlight ? _junctionColors[0] : _nonRelativeColor;
                        nodeBrush = new SolidBrush(nodeColor.Value);
                    }
                    else
                    {
                        nodeBrush = new LinearGradientBrush(
                            nodeRect, _junctionColors[0], _junctionColors[1],
                            LinearGradientMode.Horizontal);
                    }

                    var square = row.Node.HasRef;
                    var hasOutline = row.Node.IsCheckedOut;

                    if (square)
                    {
                        g.SmoothingMode = SmoothingMode.None;
                        g.FillRectangle(nodeBrush, nodeRect);
                    }
                    else //// Circle
                    {
                        nodeRect.Width = nodeRect.Height = _nodeDimension - 1;

                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.FillEllipse(nodeBrush, nodeRect);
                    }

                    if (hasOutline)
                    {
                        nodeRect.Inflate(1, 1);

                        var outlineColor = nodeColor == null
                            ? Color.Black
                            : ColorHelper.MakeColorDarker(nodeColor.Value, 0.3);

                        using (var pen = new Pen(outlineColor, 2))
                        {
                            if (square)
                            {
                                g.SmoothingMode = SmoothingMode.None;
                                g.DrawRectangle(pen, nodeRect);
                            }
                            else //// Circle
                            {
                                g.SmoothingMode = SmoothingMode.AntiAlias;
                                g.DrawEllipse(pen, nodeRect);
                            }
                        }
                    }

                    if (row.Node.Revision == null)
                    {
                        nodeRect.Inflate(1, 1);

                        using (var pen = new Pen(Color.Red, 2))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.FillEllipse(Brushes.White, nodeRect);
                            g.DrawEllipse(pen, nodeRect);
                        }
                    }

                    nodeBrush.Dispose();

                    g.SmoothingMode = oldSmoothingMode;

                    return true;

                    void UpdateJunctionColors(IReadOnlyList<Junction> junctions)
                    {
                        _junctionColors.Clear();

                        // Select one or two colours to use when rendering this junction
                        if (junctions.Count == 0)
                        {
                            _junctionColors.Add(Color.Black);
                        }
                        else
                        {
                            for (var i = 0; i < 2 && i < junctions.Count; i++)
                            {
                                _junctionColors.Add(GetJunctionColor(junctions[i]));
                            }
                        }

                        Color GetJunctionColor(Junction junction)
                        {
                            // Non relatives or non-highlighted in grey
                            switch (_revisionGraphDrawStyleCache)
                            {
                                case RevisionGraphDrawStyleEnum.DrawNonRelativesGray when !junction.IsRelative:
                                case RevisionGraphDrawStyleEnum.HighlightSelected when !junction.IsHighlighted:
                                    return _nonRelativeColor;
                            }

                            if (!AppSettings.MulticolorBranches)
                            {
                                return AppSettings.GraphColor;
                            }

                            // See if this junction's colour has already been calculated
                            if (junction.ColorIndex != -1)
                            {
                                return _graphColors[junction.ColorIndex];
                            }

                            var colorIndex = FindDistinctColour();

                            junction.ColorIndex = colorIndex;

                            return _graphColors[colorIndex];

                            int FindDistinctColour()
                            {
                                // NOTE we reuse _adjacentColors to avoid allocating lists during UI painting.
                                // This is safe as we are always on the UI thread here.
                                _adjacentColors.Clear();
                                AddAdjacentColors(junction.Youngest.Ancestors);
                                AddAdjacentColors(junction.Youngest.Descendants);
                                AddAdjacentColors(junction.Oldest.Ancestors);
                                AddAdjacentColors(junction.Oldest.Descendants);

                                if (_adjacentColors.Count == 0)
                                {
                                    // This is an end-point. Use the first colour.
                                    return 0;
                                }

                                // This is a parent branch, calculate new color based on parent branch
                                for (var i = 0; i < _graphColors.Count; i++)
                                {
                                    if (!_adjacentColors.Contains(i))
                                    {
                                        return i;
                                    }
                                }

                                // All colours are adjacent (highly uncommon!) so just pick one at random
                                return _random.Next(_graphColors.Count);

                                void AddAdjacentColors(IReadOnlyList<Junction> peers)
                                {
                                    // ReSharper disable once ForCanBeConvertedToForeach
                                    for (var i = 0; i < peers.Count; i++)
                                    {
                                        var peer = peers[i];
                                        var peerColorIndex = peer.ColorIndex;
                                        if (peerColorIndex != -1)
                                        {
                                            _adjacentColors.Add(peerColorIndex);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Clear()
        {
            lock (_graphModel)
            {
                _graphModel.Clear();
                ////_graphDataCount = 0;
                _cacheHead = -1;
                _cacheHeadRow = 0;
                ClearDrawCache();
            }
        }

        public override void Refresh(int rowHeight, in VisibleRowRange range)
        {
            // Hide graph column when there it is disabled OR when a filter is active
            // allowing for special case when history of a single file is being displayed
            Column.Visible
                = AppSettings.ShowRevisionGridGraphColumn &&
                  !_grid.ShouldHideGraph(inclBranchFilter: false);

            ClearDrawCache();
            UpdateGraphColumnWidth(range);
        }

        public override void OnColumnWidthChanged(DataGridViewColumnEventArgs e)
        {
            ClearDrawCache();
        }

        public void HighlightBranch(ObjectId id)
        {
            _graphModel.HighlightBranch(id);
        }

        public override void OnVisibleRowsChanged(in VisibleRowRange range)
        {
            // Keep an extra page in the cache
            _cacheCountMax = (range.Count * 2) + 1;
            UpdateGraphColumnWidth(range);
        }

        // TODO when rendering, if we notice a row has too many lanes, trigger updating the column's width

        private void UpdateGraphColumnWidth(in VisibleRowRange range)
        {
            if (!Column.Visible)
            {
                return;
            }

            int width = 1;
            lock (_graphModel)
            {
                foreach (var index in range)
                {
                    var laneRow = _graphModel.GetLaneRow(index);
                    if (laneRow != null)
                    {
                        width = Math.Max(laneRow.Count, width);
                    }
                }
            }

            // When 'git log --first-parent' filtration is enabled and when only current
            // branch needed to be rendered (and this filter actually works),
            // it is much more readable to limit max lanes to 1.
            int maxLanes =
                (AppSettings.ShowFirstParent &&
                 AppSettings.ShowCurrentBranchOnly &&
                 AppSettings.BranchFilterEnabled)
                    ? 1
                    : MaxLanes;

            var laneCount = Math.Min(Math.Max(1, width), maxLanes);
            var columnWidth = (_laneWidth * laneCount) + ColumnLeftMargin;
            if (Column.Width != columnWidth && columnWidth > Column.MinimumWidth)
            {
                Column.Width = columnWidth;
            }
        }

        private void ClearDrawCache()
        {
            _cacheHead = 0;
            _cacheCount = 0;
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, out string toolTip)
        {
            if (!revision.IsArtificial)
            {
                toolTip = GetLaneInfo(e.X - ColumnLeftMargin, e.RowIndex);
                return true;
            }

            toolTip = default;
            return false;

            string GetLaneInfo(int x, int rowIndex)
            {
                int lane = x / _laneWidth;
                var laneInfoText = new StringBuilder();
                lock (_graphModel)
                {
                    ILaneRow laneRow = _graphModel.GetLaneRow(rowIndex);
                    if (laneRow != null)
                    {
                        Node node = null;
                        if (lane == laneRow.NodeLane)
                        {
                            node = laneRow.Node;
                            if (!node.Revision.IsArtificial)
                            {
                                laneInfoText.AppendLine(node.Revision.Guid);
                            }
                        }
                        else if (lane >= 0 && lane < laneRow.Count)
                        {
                            for (int laneInfoIndex = 0, laneInfoCount = laneRow.LaneInfoCount(lane); laneInfoIndex < laneInfoCount; ++laneInfoIndex)
                            {
                                // search for next node below this row
                                LaneInfo laneInfo = laneRow[lane, laneInfoIndex];
                                Junction firstJunction = laneInfo.Junctions.First();
                                for (int nodeIndex = 0, nodeCount = firstJunction.NodeCount; nodeIndex < nodeCount; ++nodeIndex)
                                {
                                    Node laneNode = firstJunction[nodeIndex];
                                    if (laneNode.Index > rowIndex)
                                    {
                                        node = laneNode;
                                        break; // from for (nodes)
                                    }
                                }
                            }
                        }

                        if (node != null)
                        {
                            if (laneInfoText.Length > 0)
                            {
                                laneInfoText.AppendLine();
                            }

                            laneInfoText.Append(node.Revision.Body ?? node.Revision.Subject);
                        }
                    }
                }

                return laneInfoText.ToString();
            }
        }
    }
}