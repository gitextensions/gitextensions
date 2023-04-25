using System.Diagnostics;
using System.Drawing.Drawing2D;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using Microsoft;

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
                    SegmentDrawer segmentDrawer = new(g, lanePen, LaneWidth, rowHeight);

                    if (AppSettings.DrawGraphWithDiagonals.Value)
                    {
                        Lazy<SegmentLanes> previousLanes = new(() =>
                        {
                            Validates.NotNull(previousRow);
                            return GetLanes(revisionGraphSegment, getSegmentsForRow(index - 2), previousRow, currentRow);
                        });
                        Lazy<SegmentLanes> nextLanes = new(() =>
                        {
                            Validates.NotNull(nextRow);
                            return GetLanes(revisionGraphSegment, currentRow, nextRow, getSegmentsForRow(index + 2));
                        });
                        Lazy<SegmentLanes> farLanesDontMatter = null;

                        Lazy<SegmentLaneFlags> previousLaneFlags = new(() => GetDiagonalLaneFlags(previousLanes: farLanesDontMatter, currentLanes: previousLanes.Value, nextLanes: new(() => lanes)));
                        Lazy<SegmentLaneFlags> nextLaneFlags = new(() => GetDiagonalLaneFlags(previousLanes: new(() => lanes), currentLanes: nextLanes.Value, nextLanes: farLanesDontMatter));
                        SegmentLaneFlags currentLaneFlags = GetDiagonalLaneFlags(previousLanes, lanes, nextLanes);

                        DrawSegmentWithDiagonals(segmentDrawer, p, previousLaneFlags, currentLaneFlags, nextLaneFlags);
                    }
                    else
                    {
                        DrawSegmentCurvy(segmentDrawer, p, lanes);
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

        private static SegmentLanes GetLanes(RevisionGraphSegment revisionGraphSegment,
            IRevisionGraphRow? previousRow,
            IRevisionGraphRow currentRow,
            IRevisionGraphRow? nextRow,
            Action<LaneInfo?>? setLaneInfo = null)
        {
            SegmentLanes lanes = new() { StartLane = _noLane, EndLane = _noLane, IsTheRevisionLane = true };

            Lane currentLane = currentRow.GetLaneForSegment(revisionGraphSegment);
            BoolViewSetting drawSetting = currentLane.Sharing switch
            {
                LaneSharing.ExclusiveOrPrimary => AppSettings.DrawGraphLanesPrimary,
                LaneSharing.DifferentStart => AppSettings.DrawGraphLanesDifferentStart,
                LaneSharing.DifferentEnd => AppSettings.DrawGraphLanesDifferentEnd,
                LaneSharing.Entire => AppSettings.DrawGraphLanesEntire,
                _ => throw new InvalidOperationException(currentLane.Sharing.ToString())
            };

            // Avoid drawing the same curve twice (caused aliasing artefacts, particularly in different colors)
            if (!drawSetting.Value)
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
                    lanes.IsTheRevisionLane = false;
                }
            }

            lanes.PrimaryEndLane = lanes.EndLane;
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

        private static SegmentLaneFlags GetDiagonalLaneFlags(Lazy<SegmentLanes>? previousLanes,
            SegmentLanes currentLanes,
            Lazy<SegmentLanes>? nextLanes)
        {
            SegmentLaneFlags flags = new()
            {
                DrawFromStart = currentLanes.DrawFromStart,
                DrawToEnd = currentLanes.DrawToEnd,
                IsTheRevisionLane = currentLanes.IsTheRevisionLane
            };

            int startShift = currentLanes.CenterLane - currentLanes.StartLane;
            int endShift = currentLanes.EndLane - currentLanes.CenterLane;
            bool startIsDiagonal = Math.Abs(startShift) == 1;
            bool endIsDiagonal = Math.Abs(endShift) == 1;
            bool isBowOfDiagonals = startIsDiagonal && endIsDiagonal && -Math.Sign(startShift) == Math.Sign(endShift);
            int bowOffset = LaneWidth / 5;
            int junctionBowOffset = AppSettings.MergeGraphLanesHavingCommonParent.Value ? LaneLineWidth : bowOffset;
            flags.HorizontalOffset = isBowOfDiagonals ? -Math.Sign(startShift) * junctionBowOffset : 0;

            // Go perpendicularly through the center in order to avoid crossing independend nodes
            bool straightOneLaneDiagonals = AppSettings.StraightOneLaneDiagonals.Value;
            flags.DrawCenterToStartPerpendicularly = flags.DrawFromStart && (straightOneLaneDiagonals
                ? (startShift == 0 || (!startIsDiagonal && !flags.IsTheRevisionLane))
                : !startIsDiagonal);
            flags.DrawCenterToEndPerpendicularly = flags.DrawToEnd && (straightOneLaneDiagonals
                ? (endShift == 0 || (!endIsDiagonal && !flags.IsTheRevisionLane))
                : !endIsDiagonal);
            flags.DrawCenterPerpendicularly
                = isBowOfDiagonals
                //// lane shifted by one at end, not starting a diagonal over multiple lanes
                || (!straightOneLaneDiagonals
                    && (currentLanes.StartLane < 0 || startShift == 0)
                    && endIsDiagonal
                    && (nextLanes?.Value.EndLane is not >= 0 || endShift != nextLanes!.Value.EndLane - currentLanes.EndLane))
                //// lane shifted by one at start, not starting a diagonal over multiple lanes
                || (!straightOneLaneDiagonals
                    && (currentLanes.EndLane < 0 || endShift == 0)
                    && startIsDiagonal
                    && (previousLanes?.Value.StartLane is not >= 0 || startShift != currentLanes.StartLane - previousLanes!.Value.StartLane));
            flags.DrawCenter = flags.DrawCenterPerpendicularly
                || !flags.DrawFromStart
                || !flags.DrawToEnd
                || (!flags.DrawCenterToStartPerpendicularly && !flags.DrawCenterToEndPerpendicularly);

            // handle non-straight junctions
            if (currentLanes.EndLane < 0 && currentLanes.PrimaryEndLane >= 0 && startShift != 0)
            {
                endShift = currentLanes.PrimaryEndLane - currentLanes.CenterLane;
                bool sameDirection = Math.Sign(endShift) == Math.Sign(startShift);
                if (startIsDiagonal)
                {
                    if (straightOneLaneDiagonals && (!sameDirection || Math.Abs(endShift) > 1))
                    {
                        flags.DrawCenterToEndPerpendicularly = true;
                        flags.DrawCenter = false;
                        flags.HorizontalOffset = -Math.Sign(startShift) * (endShift == 0 || sameDirection ? LaneLineWidth / 3 : bowOffset);
                    }
                }
                else if (Math.Abs(endShift) == 1)
                {
                    // multi-lane crossing continued by a diagonal
                    flags.DrawCenterToStartPerpendicularly = false;
                    if (!sameDirection)
                    {
                        // bow
                        flags.HorizontalOffset = -Math.Sign(startShift) * LaneLineWidth * 2 / 3;
                    }
                }
                else if (straightOneLaneDiagonals)
                {
                    // multi-lane crossing continued by a straight or a multi-lane crossing
                    flags.DrawCenterToStartPerpendicularly = false;
                }
            }

            return flags;
        }

        private static void DrawSegmentCurvy(SegmentDrawer segmentDrawer, SegmentPoints p, SegmentLanes lanes)
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

        private static void DrawSegmentWithDiagonals(SegmentDrawer segmentDrawer,
            SegmentPoints p,
            Lazy<SegmentLaneFlags> previousLaneFlags,
            SegmentLaneFlags currentLaneFlags,
            Lazy<SegmentLaneFlags> nextLaneFlags)
        {
            int halfPerpendicularHeight = segmentDrawer.RowHeight / 5;
            int diagonalLaneEndOffset = segmentDrawer.RowHeight / 20;

            if (currentLaneFlags.DrawFromStart)
            {
                SegmentLaneFlags previous = previousLaneFlags.Value;
                ////Debug.Assert(previous.DrawToEnd || AppSettings.MergeGraphLanesHavingCommonParent.Value, nameof(previous.DrawToEnd));
                int startX = p.Start.X + previous.HorizontalOffset;
                if (previous.DrawCenterToEndPerpendicularly)
                {
                    segmentDrawer.DrawTo(startX, p.Start.Y + halfPerpendicularHeight);
                }
                else if (previous.DrawCenter)
                {
                    // shift diagonal lane end
                    if (previous.IsTheRevisionLane && !previous.DrawCenterPerpendicularly && !previous.DrawFromStart)
                    {
                        segmentDrawer.DrawTo(startX, p.Start.Y + diagonalLaneEndOffset, toPerpendicularly: false);
                    }
                    else
                    {
                        segmentDrawer.DrawTo(startX, p.Start.Y, previous.DrawCenterPerpendicularly);
                    }
                }
                else
                {
                    segmentDrawer.DrawTo(startX, p.Start.Y - halfPerpendicularHeight);
                }
            }

            int centerX = p.Center.X + currentLaneFlags.HorizontalOffset;

            if (currentLaneFlags.DrawCenterToStartPerpendicularly)
            {
                segmentDrawer.DrawTo(centerX, p.Center.Y - halfPerpendicularHeight);
            }

            if (currentLaneFlags.DrawCenter)
            {
                // shift diagonal lane ends
                if (currentLaneFlags.IsTheRevisionLane && !currentLaneFlags.DrawCenterPerpendicularly && !currentLaneFlags.DrawToEnd)
                {
                    segmentDrawer.DrawTo(centerX, p.Center.Y - diagonalLaneEndOffset, toPerpendicularly: false);
                }
                else if (currentLaneFlags.IsTheRevisionLane && !currentLaneFlags.DrawCenterPerpendicularly && !currentLaneFlags.DrawFromStart)
                {
                    segmentDrawer.DrawTo(centerX, p.Center.Y + diagonalLaneEndOffset, toPerpendicularly: false);
                }
                else
                {
                    segmentDrawer.DrawTo(centerX, p.Center.Y, currentLaneFlags.DrawCenterPerpendicularly);
                }
            }

            if (currentLaneFlags.DrawCenterToEndPerpendicularly)
            {
                segmentDrawer.DrawTo(centerX, p.Center.Y + halfPerpendicularHeight);
            }

            if (currentLaneFlags.DrawToEnd)
            {
                SegmentLaneFlags next = nextLaneFlags.Value;
                ////Debug.Assert(next.DrawFromStart || AppSettings.MergeGraphLanesHavingCommonParent.Value, nameof(next.DrawFromStart));
                int endX = p.End.X + next.HorizontalOffset;
                if (next.DrawCenterToStartPerpendicularly)
                {
                    segmentDrawer.DrawTo(endX, p.End.Y - halfPerpendicularHeight);
                }
                else if (next.DrawCenter)
                {
                    // shift diagonal lane end
                    if (next.IsTheRevisionLane && !next.DrawCenterPerpendicularly && !next.DrawToEnd)
                    {
                        segmentDrawer.DrawTo(endX, p.End.Y - diagonalLaneEndOffset, toPerpendicularly: false);
                    }
                    else
                    {
                        segmentDrawer.DrawTo(endX, p.End.Y, next.DrawCenterPerpendicularly);
                    }
                }
                else
                {
                    segmentDrawer.DrawTo(endX, p.End.Y + halfPerpendicularHeight);
                }
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
