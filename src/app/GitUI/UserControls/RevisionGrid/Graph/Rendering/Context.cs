namespace GitUI.UserControls.RevisionGrid.Graph.Rendering;

internal readonly ref struct Context
{
    public readonly RevisionGraphConfig Config;
    public readonly Graphics Graphics;
    public readonly Pen Pen;
    public readonly Size CellSize;

    public Context(RevisionGraphConfig config, Graphics graphics, Pen pen, int laneWidth, int rowHeight)
    {
        Config = config;
        Graphics = graphics;
        Pen = pen;
        CellSize = new Size(laneWidth, rowHeight);
    }
}
