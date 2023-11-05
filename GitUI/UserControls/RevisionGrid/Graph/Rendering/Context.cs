namespace GitUI.UserControls.RevisionGrid.Graph.Rendering;

internal readonly ref struct Context
{
    public readonly RevisionGraphConfig Config;
    public readonly Graphics Graphics;
    public readonly Pen Pen;

    public Context(RevisionGraphConfig config, Graphics graphics, Pen pen)
    {
        Config = config;
        Graphics = graphics;
        Pen = pen;
    }
}
