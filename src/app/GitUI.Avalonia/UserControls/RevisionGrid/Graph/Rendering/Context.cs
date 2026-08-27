using Avalonia.Media;

namespace GitUI.UserControls.RevisionGrid.Graph.Rendering;

// Avalonia DrawingContext replaces GDI Graphics; geometry remains in System.Drawing types.
internal readonly ref struct Context
{
    public readonly RevisionGraphConfig Config;
    public readonly DrawingContext DrawingContext;
    public readonly Pen Pen;
    public readonly Size CellSize;

    public Context(RevisionGraphConfig config, DrawingContext drawingContext, Pen pen, int laneWidth, int rowHeight)
    {
        Config = config;
        DrawingContext = drawingContext;
        Pen = pen;
        CellSize = new Size(laneWidth, rowHeight);
    }
}
