namespace GitUI.UserControls.RevisionGrid.Graph.Rendering;

internal readonly struct DiagonalSegmentInfo
{
    public readonly bool DrawFromStart;
    public readonly bool DrawToEnd;
    public readonly bool DrawCenterToStartPerpendicularly;
    public readonly bool DrawCenter;
    public readonly bool DrawCenterPerpendicularly;
    public readonly bool DrawCenterToEndPerpendicularly;
    public readonly bool IsTheRevisionLane;
    public readonly int HorizontalOffset;

    public DiagonalSegmentInfo(bool drawFromStart,
        bool drawToEnd,
        bool drawCenterToStartPerpendicularly,
        bool drawCenter,
        bool drawCenterPerpendicularly,
        bool drawCenterToEndPerpendicularly,
        bool isTheRevisionLane,
        int horizontalOffset)
    {
        DrawFromStart = drawFromStart;
        DrawToEnd = drawToEnd;
        DrawCenterToStartPerpendicularly = drawCenterToStartPerpendicularly;
        DrawCenter = drawCenter;
        DrawCenterPerpendicularly = drawCenterPerpendicularly;
        DrawCenterToEndPerpendicularly = drawCenterToEndPerpendicularly;
        IsTheRevisionLane = isTheRevisionLane;
        HorizontalOffset = horizontalOffset;
    }
}
