using System.Drawing;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class GraphColumnProvider : ColumnProvider
    {
        private readonly RevisionGridControl _grid;
        private readonly RevisionDataGridView _gridView;

        public GraphColumnProvider(RevisionGridControl grid, RevisionDataGridView gridView)
            : base("Graph")
        {
            _grid = grid;
            _gridView = gridView;

            // TODO lightweight column template (not text box)

            Column = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Resizable = DataGridViewTriState.False
            };
        }

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, in (Brush backBrush, Color foreColor, Font normalFont, Font boldFont) style)
        {
            _gridView.OnGraphCellPainting(e, revision);
        }

        public override void Refresh()
        {
            // Hide graph column when there it is disabled OR when a filter is active
            // allowing for special case when history of a single file is being displayed
            Column.Visible
                = AppSettings.ShowRevisionGridGraphColumn &&
                  !_grid.ShouldHideGraph(inclBranchFilter: false);
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, out string toolTip)
        {
            if (!revision.IsArtificial)
            {
                toolTip = _gridView.GetLaneInfo(e.X - ColumnLeftMargin, e.RowIndex);
                return true;
            }

            toolTip = default;
            return false;
        }
    }
}