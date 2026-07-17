using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using GitCommands;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUI.UserControls.RevisionGrid.Graph.Rendering;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class RevisionGraphColumnProvider : ColumnProvider
{
    private readonly RevisionGridControl _grid;
    private readonly RevisionGraph _revisionGraph;

    public RevisionGraphColumnProvider(RevisionGraph revisionGraph, RevisionGridControl grid)
        : base("Graph", new GridLength(CalculateGraphColumnWidth(visibleLaneCount: 0)), GraphRenderer.LaneWidth, resizable: false)
    {
        _revisionGraph = revisionGraph;
        _grid = grid;
    }

    public RevisionGraphDrawStyle RevisionGraphDrawStyle { get; set; } = RevisionGraphDrawStyle.DrawNonRelativesGray;

    public override void ApplySettings()
    {
        Column.IsVisible = AppSettings.ShowRevisionGridGraphColumn;
    }

    public override Control CreateCell()
    {
        GraphCellControl graph = new(this)
        {
            ClipToBounds = true,
            Margin = new Thickness(ColumnLeftMargin, 0, 0, 0),
        };
        graph.Classes.Add("revision-graph-cell");
        return graph;
    }

    public override void UpdateCell(Control control, GitRevision revision)
    {
        ((GraphCellControl)control).Revision = revision;
    }

    internal static int CalculateGraphColumnWidth(int visibleLaneCount)
        => 6
            + Math.Max(
                GraphRenderer.LaneWidth * Math.Min(visibleLaneCount, GraphRenderer.MaxLanes),
                GraphRenderer.LaneWidth);

    internal int GetLaneCount(GitRevision revision)
    {
        try
        {
            return _revisionGraph.TryGetRowIndex(revision.ObjectId, out int rowIndex)
                ? _revisionGraph.GetSegmentsForRow(rowIndex)?.GetLaneCount() ?? 0
                : 0;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    internal bool DrawGraph(DrawingContext context, GitRevision revision)
        => _grid.DrawGraphCell(context, revision, RevisionGraphDrawStyle);

    private sealed class GraphCellControl : Control
    {
        private readonly RevisionGraphColumnProvider _provider;
        private GitRevision? _revision;

        public GraphCellControl(RevisionGraphColumnProvider provider)
        {
            _provider = provider;
        }

        public GitRevision? Revision
        {
            get => _revision;
            set
            {
                _revision = value;
                InvalidateVisual();
            }
        }

        public override void Render(DrawingContext context)
        {
            if (_revision is not null)
            {
                _provider.DrawGraph(context, _revision);
            }
        }
    }
}
