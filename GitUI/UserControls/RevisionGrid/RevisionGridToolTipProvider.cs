﻿using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;

namespace GitUI
{
    internal sealed class RevisionGridToolTipProvider
    {
        private readonly ToolTip _toolTip = new();
        private readonly Dictionary<Point, bool> _isTruncatedByCellPos = new();
        private readonly RevisionDataGridView _gridView;
        private int _previousRowIndex = -1;
        private int _previousColumnIndex = -1;

        public RevisionGridToolTipProvider(RevisionDataGridView gridView)
        {
            _gridView = gridView;
        }

        public void OnCellMouseEnter()
        {
            _toolTip.Active = false;
            _toolTip.AutoPopDelay = 32767;
        }

        public void OnCellMouseMove(DataGridViewCellMouseEventArgs e)
        {
            var revision = _gridView.GetRevision(e.RowIndex);

            if (revision is null)
            {
                return;
            }

            var oldText = _toolTip.GetToolTip(_gridView);

            // Always generated tooltip text of first column (graph) because it **really** depends of the pixel hovered
            if (e.ColumnIndex != 0 && _previousRowIndex == e.RowIndex && _previousColumnIndex == e.ColumnIndex)
            {
                return;
            }

            _previousRowIndex = e.RowIndex;
            _previousColumnIndex = e.ColumnIndex;

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
                try
                {
                    if (_gridView.Columns[e.ColumnIndex].Tag is ColumnProvider provider &&
                        provider.TryGetToolTip(e, revision, out var toolTip) &&
                        !string.IsNullOrWhiteSpace(toolTip))
                    {
                        return toolTip;
                    }

                    if (_isTruncatedByCellPos.TryGetValue(new Point(e.ColumnIndex, e.RowIndex), out var showToolTip)
                        && showToolTip)
                    {
                        return _gridView.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue?.ToString() ?? "";
                    }
                }
                catch (Exception)
                {
                    // Ignore exception when fetching tooltip. It's not worth crashing for.
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