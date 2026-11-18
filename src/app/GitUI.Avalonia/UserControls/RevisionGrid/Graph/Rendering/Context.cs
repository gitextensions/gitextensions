using Avalonia.Media;

namespace GitUI.UserControls.RevisionGrid.Graph.Rendering;

// Avalonia DrawingContext replaces GDI Graphics; geometry remains in System.Drawing types.
internal readonly ref struct Context
{
    public readonly RevisionGraphConfig Config;
    public readonly DrawingContext Graphics;
    public readonly Pen Pen;
    public readonly Size CellSize;

    public Context(RevisionGraphConfig config, DrawingContext graphics, Pen pen, int laneWidth, int rowHeight)
    {
        Config = config;
        Graphics = graphics;
        Pen = pen;
        CellSize = new Size(laneWidth, rowHeight);
    }
}
