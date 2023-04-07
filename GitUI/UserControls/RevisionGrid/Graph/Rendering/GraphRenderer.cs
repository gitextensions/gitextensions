using System.Drawing.Drawing2D;
using GitCommands;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Graph.Rendering
{
    internal static class GraphRenderer
    {
        internal const int MaxLanes = RevisionGraph.MaxLanes;

        public static readonly int LaneLineWidth = DpiUtil.Scale(2);
        public static readonly int LaneWidth = DpiUtil.Scale(16);
        public static readonly int NodeDimension = DpiUtil.Scale(10);

        private const int _noLane = -10;

        public static void DrawItem(Graphics g, int index, int width, int rowHeight,
            Func<int, IRevisionGraphRow?> getSegmentsForRow,
            RevisionGraphDrawStyleEnum revisionGraphDrawStyle,
            ObjectId headId)
        {
            SmoothingMode oldSmoothingMode = g.SmoothingMode;
            Region oldClip = g.Clip;

            int top = g.RenderingOrigin.Y;
            Rectangle laneRect = new(0, top, width, rowHeight);
            using Region newClip = new(laneRect);
            newClip.Intersect(oldClip);
            g.Clip = newClip;
            g.Clear(Color.Transparent);

            DrawItem();

            // Restore graphics options
            g.Clip = oldClip;
            g.SmoothingMode = oldSmoothingMode;

            return;

            void DrawItem()
            {
                IRevisionGraphRow? currentRow = getSegmentsForRow(index);
                if (currentRow is null)
                {
                    return;
                }

                IRevisionGraphRow? previousRow = getSegmentsForRow(index - 1);
                IRevisionGraphRow? nextRow = getSegmentsForRow(index + 1);

                SegmentPointsInfo p = new();
                p.Center.Y = top + (rowHeight / 2);
                p.Start.Y = p.Center.Y - rowHeight;
                p.End.Y = p.Center.Y + rowHeight;

                LaneInfo? currentRowRevisionLaneInfo = null;

                foreach (RevisionGraphSegment revisionGraphSegment in currentRow.Segments.Reverse().OrderBy(s => s.Child.IsRelative))
                {
                    SegmentLanesInfo lanes = GetLanesInfo(revisionGraphSegment, previousRow, currentRow, nextRow, li => currentRowRevisionLaneInfo = li);
                    if (!lanes.DrawFromStart && !lanes.DrawToEnd)
                    {
                        continue;
                    }

                    int originX = g.RenderingOrigin.X;
                    p.Start.X = originX + (int)((lanes.StartLane + 0.5) * LaneWidth);
                    p.Center.X = originX + (int)((lanes.CenterLane + 0.5) * LaneWidth);
                    p.End.X = originX + (int)((lanes.EndLane + 0.5) * LaneWidth);

                    Brush laneBrush = GetBrushForLaneInfo(revisionGraphSegment.LaneInfo, revisionGraphSegment.Child.IsRelative, revisionGraphDrawStyle);
                    using Pen lanePen = new(laneBrush, LaneLineWidth);
                    SegmentRenderer segmentRenderer = new(g, lanePen);

                    DrawSegment(segmentRenderer, p, lanes);
                }

                if (currentRow.GetCurrentRevisionLane() < MaxLanes)
                {
                    int centerX = g.RenderingOrigin.X + (int)((currentRow.GetCurrentRevisionLane() + 0.5) * LaneWidth);
                    Rectangle nodeRect = new(centerX - (NodeDimension / 2), p.Center.Y - (NodeDimension / 2), NodeDimension, NodeDimension);

                    bool square = currentRow.Revision.GitRevision.Refs.Count > 0;
                    bool hasOutline = currentRow.Revision.GitRevision.ObjectId == headId;

                    Brush brush = GetBrushForLaneInfo(currentRowRevisionLaneInfo, currentRow.Revision.IsRelative, revisionGraphDrawStyle);
                    if (square)
                    {
                        g.SmoothingMode = SmoothingMode.None;
                        g.FillRectangle(brush, nodeRect);
                    }
                    else //// Circle
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.FillEllipse(brush, nodeRect);
                    }

                    if (hasOutline)
                    {
                        nodeRect.Inflate(1, 1);

                        Color outlineColor = SystemColors.WindowText;

                        using Pen pen = new(outlineColor, 2);
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
            }
        }

        public static SegmentLanesInfo GetLanesInfo(RevisionGraphSegment revisionGraphSegment,
            IRevisionGraphRow? previousRow,
            IRevisionGraphRow currentRow,
            IRevisionGraphRow? nextRow,
            Action<LaneInfo?>? setLaneInfo)
        {
            Lane currentLane = currentRow.GetLaneForSegment(revisionGraphSegment);

            int startLane = _noLane;
            int centerLane = _noLane;
            int endLane = _noLane;

            // Avoid drawing the same curve twice (caused aliasing artifacts, particularly when in different colors)
            if (currentLane.Sharing == LaneSharing.Entire)
            {
                return new SegmentLanesInfo(startLane, centerLane, endLane, drawFromStart: false, drawToEnd: false);
            }

            centerLane = currentLane.Index;
            if (revisionGraphSegment.Parent == currentRow.Revision)
            {
                // This lane ends here
                startLane = GetLaneForRow(previousRow, revisionGraphSegment);
                setLaneInfo?.Invoke(revisionGraphSegment.LaneInfo);
            }
            else
            {
                if (revisionGraphSegment.Child == currentRow.Revision)
                {
                    // This lane starts here
                    endLane = GetLaneForRow(nextRow, revisionGraphSegment);
                    setLaneInfo?.Invoke(revisionGraphSegment.LaneInfo);
                }
                else
                {
                    // This lane crosses
                    startLane = GetLaneForRow(previousRow, revisionGraphSegment);
                    endLane = GetLaneForRow(nextRow, revisionGraphSegment);
                }
            }

            switch (currentLane.Sharing)
            {
                case LaneSharing.DifferentStart:
                    if (AppSettings.ShowRevisionGridGraphColumn)
                    {
                        endLane = _noLane;
                    }
                    else if (endLane != _noLane)
                    {
                        throw new Exception($"{currentRow.Revision.Objectid.ToShortString()}: lane {centerLane} has DifferentStart but has EndLane {endLane} (StartLane {startLane})");
                    }

                    break;

                case LaneSharing.DifferentEnd:
                    if (startLane != _noLane)
                    {
                        throw new Exception($"{currentRow.Revision.Objectid.ToShortString()}: lane {centerLane} has DifferentEnd but has StartLane {startLane} (EndLane {endLane})");
                    }

                    break;
            }

            return new SegmentLanesInfo(startLane, centerLane, endLane,
                drawFromStart: startLane >= 0 && centerLane >= 0 && (startLane <= MaxLanes || centerLane <= MaxLanes),
                drawToEnd: endLane >= 0 && centerLane >= 0 && (endLane <= MaxLanes || centerLane <= MaxLanes));
        }

        private static void DrawSegment(SegmentRenderer segmentDrawer, SegmentPointsInfo p, SegmentLanesInfo lanes)
        {
            if (lanes.DrawFromStart)
            {
                segmentDrawer.DrawTo(p.Start);
            }

            segmentDrawer.DrawTo(p.Center);

            if (lanes.DrawToEnd)
            {
                segmentDrawer.DrawTo(p.End);
            }
        }

        private static Brush GetBrushForLaneInfo(LaneInfo? laneInfo, bool isRelative, RevisionGraphDrawStyleEnum revisionGraphDrawStyle)
        {
            // laneInfo can be null for revisions without parents and children, especially when filtering, draw them gray, too
            if (laneInfo is null
                || (!isRelative && (revisionGraphDrawStyle is RevisionGraphDrawStyleEnum.DrawNonRelativesGray or RevisionGraphDrawStyleEnum.HighlightSelected)))
            {
                return RevisionGraphLaneColor.NonRelativeBrush;
            }

            return RevisionGraphLaneColor.GetBrushForLane(laneInfo.Color);
        }

        private static int GetLaneForRow(IRevisionGraphRow? row, RevisionGraphSegment revisionGraphRevision)
        {
            if (row is not null)
            {
                return row.GetLaneForSegment(revisionGraphRevision).Index;
            }

            return -1;
        }
    }
}
