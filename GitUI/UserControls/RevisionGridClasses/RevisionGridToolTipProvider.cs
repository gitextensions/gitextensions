using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GitUI
{
    internal sealed class RevisionGridToolTipProvider
    {
        private readonly ToolTip _toolTip = new ToolTip();
        private readonly Dictionary<Point, bool> _showCellToolTip = new Dictionary<Point, bool>();
        private readonly RevisionGrid _grid;

        public RevisionGridToolTipProvider(RevisionGrid grid)
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

            string oldTooltip = _toolTip.GetToolTip(_grid.Graph);

            string newToolTip;
            if (e.ColumnIndex == _grid.GraphDataGridViewColumn.Index)
            {
                newToolTip = _grid.Graph.GetLaneInfo(e.RowIndex, e.X, _grid.Module);
            }
            else if (e.ColumnIndex == _grid.IsMessageMultilineDataGridViewColumn.Index)
            {
                newToolTip = revision.HasMultiLineMessage ? revision.Body : string.Empty;
            }
            else if (e.ColumnIndex == _grid.MessageDataGridViewColumn.Index && revision.HasMultiLineMessage)
            {
                newToolTip = revision.Body;
            }
            else if (_showCellToolTip.TryGetValue(new Point(e.ColumnIndex, e.RowIndex), out var showToolTip) && showToolTip)
            {
                newToolTip = e.ColumnIndex == _grid.IdDataGridViewColumn.Index
                    ? revision.Guid
                    : GetCellText(e.RowIndex, e.ColumnIndex);
            }
            else
            {
                // no tooltip unless always active or truncated
                newToolTip = string.Empty;
            }

            if (newToolTip != oldTooltip)
            {
                _toolTip.SetToolTip(_grid.Graph, newToolTip);
            }

            if (!_toolTip.Active)
            {
                _toolTip.Active = true;
            }

            return;

            string GetCellText(int row, int col)
            {
                return _grid.Graph.Rows[row].Cells[col].FormattedValue?.ToString() ?? "";
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