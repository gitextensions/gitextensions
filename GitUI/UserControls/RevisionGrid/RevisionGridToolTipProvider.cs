using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GitUI.UserControls.RevisionGrid.Columns;

namespace GitUI
{
    internal sealed class RevisionGridToolTipProvider
    {
        private readonly ToolTip _toolTip = new ToolTip();
        private readonly Dictionary<Point, bool> _showCellToolTip = new Dictionary<Point, bool>();
        private readonly RevisionGridControl _grid;

        public RevisionGridToolTipProvider(RevisionGridControl grid)
        {
            _grid = grid;
        }

        public void OnCellMouseEnter()
        {
            _toolTip.Active = false;
            _toolTip.AutoPopDelay = 32767;
        }

        public void OnCellMouseMove(DataGridViewCellMouseEventArgs e)
        {
            var revision = _grid.Graph.GetRowData(e.RowIndex);

            if (revision == null)
            {
                return;
            }

            var oldText = _toolTip.GetToolTip(_grid.Graph);
            var newText = GetToolTipText();

            if (newText != oldText)
            {
                _toolTip.SetToolTip(_grid.Graph, newText);
            }

            if (!_toolTip.Active)
            {
                _toolTip.Active = true;
            }

            return;

            string GetToolTipText()
            {
                if (_grid.Graph.Columns[e.ColumnIndex].Tag is ColumnProvider provider &&
                    provider.TryGetToolTip(e, revision, out var toolTip) &&
                    !string.IsNullOrWhiteSpace(toolTip))
                {
                    return toolTip;
                }

                if (_showCellToolTip.TryGetValue(new Point(e.ColumnIndex, e.RowIndex), out var showToolTip)
                    && showToolTip)
                {
                    return _grid.Graph.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue?.ToString() ?? "";
                }

                // no tooltip unless always active or truncated
                return "";
            }
        }

        public void Clear()
        {
            _showCellToolTip.Clear();
        }

        public void SetTruncation(DataGridViewCellPaintingEventArgs e, bool truncated)
        {
            _showCellToolTip[new Point(e.ColumnIndex, e.RowIndex)] = truncated;
        }
    }
}