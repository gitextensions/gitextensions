using System;
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

        public void OnCellMouseMove(DataGridViewCellMouseEventArgs e)
        {
            var revision = _gridView.GetRevision(e.RowIndex);

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
                try
                {
                    if (_gridView.Columns[e.ColumnIndex].Tag is ColumnProvider provider &&
                        provider.TryGetToolTip(e, revision, out var toolTip) &&
                        !string.IsNullOrWhiteSpace(toolTip))
                    {
                        int lineCount = 0;
                        for (int pos = 0; pos < toolTip.Length; ++pos)
                        {
                            if (toolTip[pos] == '\n' && ++lineCount == 30)
                            {
                                return toolTip.Substring(0, pos + 1) + "...";
                            }
                        }

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