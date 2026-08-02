using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using GitCommands;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUI.UserControls.RevisionGrid.Graph.Rendering;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class RevisionGraphColumnProvider : ColumnProvider
{
    private static readonly Cursor HandCursor = new(StandardCursorType.Hand);

    private readonly RevisionGridControl _grid;
    private readonly LaneInfoProvider _laneInfoProvider;
    private readonly RevisionGraph _revisionGraph;

    public RevisionGraphColumnProvider(
        RevisionGraph revisionGraph,
        RevisionGridControl grid,
        IGitRevisionSummaryBuilder gitRevisionSummaryBuilder)
        : base("Graph", new GridLength(CalculateGraphColumnWidth(visibleLaneCount: 0)), GraphRenderer.LaneWidth, resizable: false)
    {
        _revisionGraph = revisionGraph;
        _grid = grid;
        _laneInfoProvider = new LaneInfoProvider(new LaneNodeLocator(revisionGraph), gitRevisionSummaryBuilder);
    }

    public RevisionGraphDrawStyle RevisionGraphDrawStyle { get; set; } = RevisionGraphDrawStyle.DrawNonRelativesGray;

    public override void ApplySettings()
    {
        Column.IsVisible = AppSettings.ShowRevisionGridGraphColumn;
        RevisionGraphDrawStyle = AppSettings.RevisionGraphDrawNonRelativesGray
            ? RevisionGraphDrawStyle.DrawNonRelativesGray
            : RevisionGraphDrawStyle.Normal;
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
        GraphCellControl graph = (GraphCellControl)control;
        graph.Revision = revision;
        ToolTip.SetTip(graph, null);
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

    internal string? GetLaneToolTip(GitRevision revision, double x)
    {
        if (!AppSettings.ShowRevisionGridTooltips.Value
            || x < 0
            || !_revisionGraph.TryGetRowIndex(revision.ObjectId, out int rowIndex))
        {
            return null;
        }

        int lane = (int)(x / GraphRenderer.LaneWidth);
        string toolTip = _laneInfoProvider.GetLaneInfo(rowIndex, lane);
        return string.IsNullOrEmpty(toolTip) ? null : toolTip;
    }

    private sealed class GraphCellControl : Control
    {
        private readonly RevisionGraphColumnProvider _provider;
        private GitRevision? _revision;

        public GraphCellControl(RevisionGraphColumnProvider provider)
        {
            _provider = provider;
            PointerMoved += OnPointerMoved;
            PointerExited += (_, _) =>
            {
                ToolTip.SetTip(this, null);
                Cursor = null;
            };
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

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            string? toolTip = _revision is null
                ? null
                : _provider.GetLaneToolTip(_revision, e.GetPosition(this).X);
            ToolTip.SetTip(this, toolTip);
            Cursor = toolTip is null
                ? null
                : HandCursor;
        }
    }
}
