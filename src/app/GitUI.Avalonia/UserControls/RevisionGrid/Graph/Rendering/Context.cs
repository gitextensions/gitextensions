using Avalonia.Media;

namespace GitUI.UserControls.RevisionGrid.Graph.Rendering;

// Twin of GitUI/UserControls/RevisionGrid/Graph/Rendering/Context.cs: the GDI Graphics is
// replaced by Avalonia's DrawingContext; the geometry stays System.Drawing like upstream.
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
