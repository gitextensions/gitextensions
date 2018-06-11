using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;

namespace GitUI
{
    internal sealed class RevisionGridToolTipProvider
    {
        private readonly ToolTip _toolTip = new ToolTip();
        private readonly Dictionary<Point, bool> _isTruncatedByCellPos = new Dictionary<Point, bool>();
        private readonly RevisionDataGridView _gridView;

        public RevisionGridToolTipProvider(RevisionDataGridView gridView)
        {
            _gridView = gridView;
        }

        public void OnCellMouseEnter()
        {
            _toolTip.Active = false;
            _toolTip.AutoPopDelay = 32767;
        }

        public void OnCellMouseMove(int columnIndex, int rowIndex)
        {
            var revision = _gridView.GetRevision(rowIndex);

            if (revision == null)
            {
                return;
            }

            var oldText = _toolTip.GetToolTip(_gridView);
            var newText = GetToolTipText();

            if (newText != oldText)
            {
                _toolTip.SetToolTip(_gridView, newText);
            }

            if (!_toolTip.Active)
            {
                _toolTip.Active = true;
            }

            return;

            string GetToolTipText()
            {
                if (_gridView.Columns[columnIndex].Tag is ColumnProvider provider &&
                    provider.TryGetToolTip(revision, out var toolTip) &&
                    !string.IsNullOrWhiteSpace(toolTip))
                {
                    return toolTip;
                }

                if (_isTruncatedByCellPos.TryGetValue(new Point(columnIndex, rowIndex), out var showToolTip)
                    && showToolTip)
                {
                    return _gridView.Rows[rowIndex].Cells[columnIndex].FormattedValue?.ToString() ?? "";
                }

                // no tooltip unless always active or truncated
                return "";
            }
        }

        public void Clear()
        {
            _isTruncatedByCellPos.Clear();
        }

        public void SetTruncation(int columnIndex, int rowIndex, bool truncated)
        {
            _isTruncatedByCellPos[new Point(columnIndex, rowIndex)] = truncated;
        }
    }
}