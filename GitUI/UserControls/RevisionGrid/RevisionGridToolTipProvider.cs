using System.Diagnostics;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;

namespace GitUI
{
    internal sealed class RevisionGridToolTipProvider
    {
        private readonly ToolTip _toolTip = new();
        private readonly Dictionary<Point, bool> _isTruncatedByCellPos = [];
        private readonly RevisionDataGridView _gridView;
        private int _previousRowIndex = -1;
        private int _previousColumnIndex = -1;

        public RevisionGridToolTipProvider(RevisionDataGridView gridView)
        {
            _gridView = gridView;
        }

        /// <summary>
        /// Hides the tooltip.
        /// </summary>
        /// <returns>Returns <cref>true</cref> if the tooltip was active.</returns>
        public bool Hide()
        {
            bool wasActive = _toolTip.Active;
            _toolTip.Active = false;
            _toolTip.AutoPopDelay = 32767;
            return wasActive;
        }

        public void OnCellMouseMove(DataGridViewCellMouseEventArgs e)
        {
            GitUIPluginInterfaces.GitRevision revision = _gridView.GetRevision(e.RowIndex);

            if (revision is null)
            {
                return;
            }

            // Always generated tooltip text of first column (graph) because it **really** depends of the pixel hovered
            if (e.ColumnIndex != 0 && _previousRowIndex == e.RowIndex && _previousColumnIndex == e.ColumnIndex)
            {
                return;
            }

            _previousRowIndex = e.RowIndex;
            _previousColumnIndex = e.ColumnIndex;

            string newText = GetToolTipText();
            if (_toolTip.GetToolTip(_gridView) != newText)
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
                        provider.TryGetToolTip(e, revision, out string? toolTip) &&
                        !string.IsNullOrWhiteSpace(toolTip))
                    {
                        return toolTip;
                    }

                    if (_isTruncatedByCellPos.TryGetValue(new Point(e.ColumnIndex, e.RowIndex), out bool showToolTip)
                        && showToolTip)
                    {
                        return _gridView.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue?.ToString() ?? "";
                    }
                }
                catch (Exception ex)
                {
                    // Ignore exception when fetching tooltip. It's not worth crashing for.
                    Trace.WriteLine(ex);
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
