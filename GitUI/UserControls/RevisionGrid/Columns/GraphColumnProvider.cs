using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class GraphColumnProvider : ColumnProvider
    {
        private readonly RevisionGridControl _grid;

        public GraphColumnProvider(RevisionGridControl grid)
            : base("Graph")
        {
            _grid = grid;

            // TODO lightweight column template (not text box)

            Column = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Frozen = true,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Width = DpiUtil.Scale(70),
                DefaultCellStyle = { Font = SystemFonts.DefaultFont }
            };
        }

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, (Brush backBrush, Color backColor, Color foreColor, Font normalFont, Font boldFont) style)
        {
            _grid.Graph.dataGrid_CellPainting(revision, e);
        }

        // Hide graph column when there it is disabled OR when a filter is active
        // allowing for special case when history of a single file is being displayed
        public override void UpdateVisibility() => Column.Visible = AppSettings.ShowRevisionGridGraphColumn && !_grid.ShouldHideGraph(inclBranchFilter: false);

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, out string toolTip)
        {
            if (!revision.IsArtificial)
            {
                toolTip = revision.Guid;
                return true;
            }

            toolTip = default;
            return false;
        }
    }
}