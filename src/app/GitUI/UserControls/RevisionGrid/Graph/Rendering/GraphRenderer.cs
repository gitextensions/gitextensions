using System.Drawing.Drawing2D;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using Microsoft;

namespace GitUI.UserControls.RevisionGrid.Graph.Rendering
{
    internal static class GraphRenderer
    {
        internal const int MaxLanes = RevisionGraph.MaxLanes;

        public static readonly int LaneLineWidth = DpiUtil.Scale(2);
        public static readonly int LaneWidth = DpiUtil.Scale(16);
        public static readonly int NodeDimension = DpiUtil.Scale(10);

        private const int _noLane = -10;

        public static void DrawItem(RevisionGraphConfig config, Graphics g, int index, int rowHeight,
            Func<int, IRevisionGraphRow?> getSegmentsForRow,
            RevisionGraphDrawStyle revisionGraphDrawStyle,
            ObjectId headId)
        {
            g.Clear(Color.Transparent);

            DrawItem();

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
                p.Center.Y = g.RenderingOrigin.Y + (rowHeight / 2);
                p.Start.Y = p.Center.Y - rowHeight;
                p.End.Y = p.Center.Y + rowHeight;

                LaneInfo? currentRowRevisionLaneInfo = null;

                foreach (RevisionGraphSegment revisionGraphSegment in currentRow.Segments.Reverse().OrderBy(s => s.Child.IsRelative))
                {
                    bool skipSecondarySharedSegments = revisionGraphDrawStyle is not (RevisionGraphDrawStyle.DrawNonRelativesGray or RevisionGraphDrawStyle.HighlightSelected);
                    SegmentLanesInfo lanes = GetLanesInfo(revisionGraphSegment, previousRow, currentRow, nextRow, skipSecondarySharedSegments, config.MergeGraphLanesHavingCommonParent,
                        setLaneInfo: li => currentRowRevisionLaneInfo = li);
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
                    SegmentRenderer segmentRenderer = new(new Context(config, g, lanePen, LaneWidth, rowHeight));

                    if (config.RenderGraphWithDiagonals)
                    {
                        Lazy<DiagonalSegmentInfo> previousSegmentInfo = new(() =>
                        {
                            Validates.NotNull(previousRow);
                            SegmentLanesInfo previousLanes = GetLanesInfo(revisionGraphSegment, getSegmentsForRow(index - 2), previousRow, currentRow, skipSecondarySharedSegments, config.MergeGraphLanesHavingCommonParent);
                            return GetDiagonalSegmentInfo(previousLanes, config.MergeGraphLanesHavingCommonParent);
                        });
                        Lazy<DiagonalSegmentInfo> nextSegmentInfo = new(() =>
                        {
                            Validates.NotNull(nextRow);
                            SegmentLanesInfo nextLanes = GetLanesInfo(revisionGraphSegment, currentRow, nextRow, getSegmentsForRow(index + 2), skipSecondarySharedSegments, config.MergeGraphLanesHavingCommonParent);
                            return GetDiagonalSegmentInfo(nextLanes, config.MergeGraphLanesHavingCommonParent);
                        });
                        DiagonalSegmentInfo currentSegmentInfo = GetDiagonalSegmentInfo(lanes, config.MergeGraphLanesHavingCommonParent);

                        DrawSegmentWithDiagonals(segmentRenderer, p, previousSegmentInfo, currentSegmentInfo, nextSegmentInfo);
                    }
                    else
                    {
                        DrawSegmentCurvy(segmentRenderer, p, lanes);
                    }
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

        private static SegmentLanesInfo GetLanesInfo(in RevisionGraphSegment revisionGraphSegment,
            in IRevisionGraphRow? previousRow,
            in IRevisionGraphRow currentRow,
            in IRevisionGraphRow? nextRow,
            in bool skipSecondarySharedSegments,
            in bool mergeGraphLanesHavingCommonParent,
            in Action<LaneInfo?>? setLaneInfo = null)
        {
            Lane currentLane = currentRow.GetLaneForSegment(revisionGraphSegment);

            int startLane = _noLane;
            int centerLane = _noLane;
            int endLane = _noLane;

            // Avoid drawing the same curve twice (caused aliasing artifacts, particularly when in different colors)
            if (skipSecondarySharedSegments && currentLane.Sharing == LaneSharing.Entire)
            {
                return new SegmentLanesInfo(startLane, centerLane, endLane, primaryEndLane: endLane, isTheRevisionLane: false, drawFromStart: false, drawToEnd: false);
            }

            centerLane = currentLane.Index;
            bool isTheRevisionLane = true;
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
                    isTheRevisionLane = false;
                }
            }

            int primaryEndLane = endLane;
            switch (currentLane.Sharing)
            {
                case LaneSharing.DifferentStart:
                    if (mergeGraphLanesHavingCommonParent)
                    {
                        if (skipSecondarySharedSegments)
                        {
                            endLane = _noLane;
                        }
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

            return new SegmentLanesInfo(startLane, centerLane, endLane, primaryEndLane, isTheRevisionLane,
                drawFromStart: startLane >= 0 && centerLane >= 0 && (startLane <= MaxLanes || centerLane <= MaxLanes),
                drawToEnd: endLane >= 0 && centerLane >= 0 && (endLane <= MaxLanes || centerLane <= MaxLanes));
        }

        private static DiagonalSegmentInfo GetDiagonalSegmentInfo(in SegmentLanesInfo currentLanes, bool mergeGraphLanesHavingCommonParent)
        {
            bool drawFromStart = currentLanes.DrawFromStart;
            bool drawToEnd = currentLanes.DrawToEnd;
            bool isTheRevisionLane = currentLanes.IsTheRevisionLane;

            int startShift = currentLanes.CenterLane - currentLanes.StartLane;
            int endShift = currentLanes.EndLane - currentLanes.CenterLane;
            bool startIsDiagonal = Math.Abs(startShift) == 1;
            bool endIsDiagonal = Math.Abs(endShift) == 1;
            bool isBowOfDiagonals = startIsDiagonal && endIsDiagonal && -Math.Sign(startShift) == Math.Sign(endShift);
            int bowOffset = LaneWidth / 6;
            int junctionBowOffset = mergeGraphLanesHavingCommonParent ? LaneLineWidth : bowOffset;
            int horizontalOffset = isBowOfDiagonals ? -Math.Sign(startShift) * junctionBowOffset : 0;

            // Go perpendicularly through the center in order to avoid crossing independend nodes
            bool drawCenterToStartPerpendicularly = drawFromStart && (startShift == 0 || (!startIsDiagonal && !isTheRevisionLane));
            bool drawCenterToEndPerpendicularly = drawToEnd && (endShift == 0 || (!endIsDiagonal && !isTheRevisionLane));
            bool drawCenterPerpendicularly = isBowOfDiagonals;
            bool drawCenter = drawCenterPerpendicularly
                || !drawFromStart
                || !drawToEnd
                || (!drawCenterToStartPerpendicularly && !drawCenterToEndPerpendicularly);

            // handle non-straight junctions
            if (currentLanes.EndLane < 0 && currentLanes.PrimaryEndLane >= 0 && startShift != 0)
            {
                endShift = currentLanes.PrimaryEndLane - currentLanes.CenterLane;
                bool sameDirection = Math.Sign(endShift) == Math.Sign(startShift);
                if (startIsDiagonal)
                {
                    int endDelta = Math.Abs(endShift);
                    if (!sameDirection || endDelta > 1)
                    {
                        drawCenterToEndPerpendicularly = true;
                        drawCenter = false;
                        horizontalOffset = -Math.Sign(startShift) * (endDelta != 1 || sameDirection ? LaneLineWidth / 3 : bowOffset);
                    }
                }
                else if (Math.Abs(endShift) == 1)
                {
                    // multi-lane crossing continued by a diagonal
                    drawCenterToStartPerpendicularly = false;
                    if (!sameDirection)
                    {
                        // bow
                        horizontalOffset = -Math.Sign(startShift) * LaneLineWidth * 2 / 3;
                    }
                }
                else
                {
                    // multi-lane crossing continued by a straight or a multi-lane crossing
                    drawCenterToStartPerpendicularly = false;
                }
            }

            return new DiagonalSegmentInfo(drawFromStart, drawToEnd,
                drawCenterToStartPerpendicularly, drawCenter, drawCenterPerpendicularly, drawCenterToEndPerpendicularly,
                isTheRevisionLane, horizontalOffset);
        }

        private static void DrawSegmentCurvy(SegmentRenderer segmentRenderer, in SegmentPointsInfo p, in SegmentLanesInfo lanes)
        {
            if (lanes.DrawFromStart)
            {
                segmentRenderer.DrawTo(p.Start);
            }

            segmentRenderer.DrawTo(p.Center);

            if (lanes.DrawToEnd)
            {
                segmentRenderer.DrawTo(p.End);
            }
        }

        private static void DrawSegmentWithDiagonals(SegmentRenderer segmentRenderer,
            in SegmentPointsInfo p,
            in Lazy<DiagonalSegmentInfo> previousSegmentInfo,
            in DiagonalSegmentInfo current,
            in Lazy<DiagonalSegmentInfo> nextSegmentInfo)
        {
            int halfPerpendicularHeight = segmentRenderer.RowHeight / 6;

            if (current.DrawFromStart)
            {
                DiagonalSegmentInfo previous = previousSegmentInfo.Value;
                DebugHelpers.Assert(previous.DrawToEnd || AppSettings.MergeGraphLanesHavingCommonParent.Value, nameof(previous.DrawToEnd));
                int startX = p.Start.X + previous.HorizontalOffset;
                if (previous.DrawCenterToEndPerpendicularly)
                {
                    segmentRenderer.DrawTo(startX, p.Start.Y + halfPerpendicularHeight);
                }
                else if (previous.DrawCenter)
                {
                    segmentRenderer.DrawTo(startX, p.Start.Y, previous.DrawCenterPerpendicularly);
                }
                else
                {
                    segmentRenderer.DrawTo(startX, p.Start.Y - halfPerpendicularHeight);
                }
            }

            int centerX = p.Center.X + current.HorizontalOffset;

            if (current.DrawCenterToStartPerpendicularly)
            {
                segmentRenderer.DrawTo(centerX, p.Center.Y - halfPerpendicularHeight);
            }

            if (current.DrawCenter)
            {
                segmentRenderer.DrawTo(centerX, p.Center.Y, current.DrawCenterPerpendicularly);
            }

            if (current.DrawCenterToEndPerpendicularly)
            {
                segmentRenderer.DrawTo(centerX, p.Center.Y + halfPerpendicularHeight);
            }

            if (current.DrawToEnd)
            {
                DiagonalSegmentInfo next = nextSegmentInfo.Value;
                DebugHelpers.Assert(next.DrawFromStart || AppSettings.MergeGraphLanesHavingCommonParent.Value, nameof(next.DrawFromStart));
                int endX = p.End.X + next.HorizontalOffset;
                if (next.DrawCenterToStartPerpendicularly)
                {
                    segmentRenderer.DrawTo(endX, p.End.Y - halfPerpendicularHeight);
                }
                else if (next.DrawCenter)
                {
                    segmentRenderer.DrawTo(endX, p.End.Y, next.DrawCenterPerpendicularly);
                }
                else
                {
                    segmentRenderer.DrawTo(endX, p.End.Y + halfPerpendicularHeight);
                }
            }
        }

        private static Brush GetBrushForLaneInfo(LaneInfo? laneInfo, bool isRelative, RevisionGraphDrawStyle revisionGraphDrawStyle)
        {
            // laneInfo can be null for revisions without parents and children, especially when filtering, draw them gray, too
            if (laneInfo is null
                || (!isRelative && (revisionGraphDrawStyle is RevisionGraphDrawStyle.DrawNonRelativesGray or RevisionGraphDrawStyle.HighlightSelected)))
            {
                return RevisionGraphLaneColor.NonRelativeBrush;
            }

            return RevisionGraphLaneColor.GetBrushForLane(laneInfo.Color);
        }

        private static int GetLaneForRow(IRevisionGraphRow? row, RevisionGraphSegment revisionGraphRevision)
        {
            if (row is not null)
            {
                int lane = row.GetLaneForSegment(revisionGraphRevision).Index;
                if (lane >= 0)
                {
                    return lane;
                }
            }

            return _noLane;
        }
    }
}
