using System.Drawing;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class CommitIdColumnProvider : ColumnProvider
    {
        private readonly RevisionGridControl _grid;

        public CommitIdColumnProvider(RevisionGridControl grid)
            : base("Commit ID")
        {
            _grid = grid;

            Column = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "Commit ID",
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                FillWeight = 20,
                Resizable = DataGridViewTriState.False
            };
        }

        public override void UpdateVisibility() => Column.Visible = AppSettings.ShowIds;

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, (Brush backBrush, Color backColor, Color foreColor, Font normalFont, Font boldFont) style)
        {
            if (!revision.IsArtificial)
            {
                // TODO don't truncate with ellipsis
                _grid.DrawColumnText(e, revision.Guid, style.normalFont, style.foreColor);
            }
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, out string toolTip)
        {
            toolTip = revision.Guid;
            return true;
        }
    }
}