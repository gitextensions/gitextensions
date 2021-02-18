using System.Drawing.Drawing2D;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.GraphDrawer
{
    internal static class GraphDrawer
    {
        internal const int MaxLanes = RevisionGraph.MaxLanes;

        internal static readonly int LaneLineWidth = DpiUtil.Scale(2);
        internal static readonly int LaneWidth = DpiUtil.Scale(16);
        internal static readonly int NodeDimension = DpiUtil.Scale(10);

        private const int _noLane = -10;

        internal static void DrawItem(Graphics g, int index, int width, int rowHeight,
            Func<int, IRevisionGraphRow?> getSegmentsForRow,
            RevisionGraphDrawStyleEnum revisionGraphDrawStyle,
            ObjectId headId)
        {
            SmoothingMode oldSmoothingMode = g.SmoothingMode;
            Region oldClip = g.Clip;

            int top = g.RenderingOrigin.Y;
            Rectangle laneRect = new(0, top, width, rowHeight);
            Region newClip = new(laneRect);
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

                SegmentPoints p = new();
                p.Center.Y = top + (rowHeight / 2);
                p.Start.Y = p.Center.Y - rowHeight;
                p.End.Y = p.Center.Y + rowHeight;

                LaneInfo? currentRowRevisionLaneInfo = null;

                foreach (RevisionGraphSegment revisionGraphSegment in currentRow.Segments.Reverse().OrderBy(s => s.Child.IsRelative))
                {
                    SegmentLanes lanes = GetLanes(revisionGraphSegment, previousRow, currentRow, nextRow, li => currentRowRevisionLaneInfo = li);
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
                    SegmentDrawer segmentDrawer = new(g, lanePen);

                    DrawSegment(segmentDrawer, p, lanes);
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

        private static SegmentLanes GetLanes(RevisionGraphSegment revisionGraphSegment,
            IRevisionGraphRow? previousRow,
            IRevisionGraphRow currentRow,
            IRevisionGraphRow? nextRow,
            Action<LaneInfo?>? setLaneInfo)
        {
            SegmentLanes lanes = new() { StartLane = _noLane, EndLane = _noLane };

            Lane currentLane = currentRow.GetLaneForSegment(revisionGraphSegment);

            // Avoid drawing the same curve twice (caused aliasing artefacts, particularly in different colors)
            if (currentLane.Sharing == LaneSharing.Entire)
            {
                lanes.DrawFromStart = false;
                lanes.DrawToEnd = false;
                return lanes;
            }

            if (revisionGraphSegment.Parent == currentRow.Revision)
            {
                // This lane ends here
                lanes.StartLane = GetLaneForRow(previousRow, revisionGraphSegment);
                lanes.CenterLane = currentLane.Index;
                setLaneInfo?.Invoke(revisionGraphSegment.LaneInfo);
            }
            else
            {
                if (revisionGraphSegment.Child == currentRow.Revision)
                {
                    // This lane starts here
                    lanes.CenterLane = currentLane.Index;
                    lanes.EndLane = GetLaneForRow(nextRow, revisionGraphSegment);
                    setLaneInfo?.Invoke(revisionGraphSegment.LaneInfo);
                }
                else
                {
                    // This lane crosses
                    lanes.StartLane = GetLaneForRow(previousRow, revisionGraphSegment);
                    lanes.CenterLane = currentLane.Index;
                    lanes.EndLane = GetLaneForRow(nextRow, revisionGraphSegment);
                }
            }

            switch (currentLane.Sharing)
            {
                case LaneSharing.DifferentStart:
                    if (AppSettings.MergeGraphLanesHavingCommonParent.Value)
                    {
                        lanes.EndLane = _noLane;
                    }
                    else if (lanes.EndLane != _noLane)
                    {
                        throw new Exception($"{currentRow.Revision.Objectid.ToShortString()}: lane {lanes.CenterLane} has DifferentStart but has EndLane {lanes.EndLane} (StartLane {lanes.StartLane})");
                    }

                    break;

                case LaneSharing.DifferentEnd:
                    if (lanes.StartLane != _noLane)
                    {
                        throw new Exception($"{currentRow.Revision.Objectid.ToShortString()}: lane {lanes.CenterLane} has DifferentEnd but has StartLane {lanes.StartLane} (EndLane {lanes.EndLane})");
                    }

                    break;
            }

            lanes.DrawFromStart = lanes.StartLane >= 0 && lanes.CenterLane >= 0 && (lanes.StartLane <= MaxLanes || lanes.CenterLane <= MaxLanes);
            lanes.DrawToEnd = lanes.EndLane >= 0 && lanes.CenterLane >= 0 && (lanes.EndLane <= MaxLanes || lanes.CenterLane <= MaxLanes);

            return lanes;
        }

        private static void DrawSegment(SegmentDrawer segmentDrawer, SegmentPoints p, SegmentLanes lanes)
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
