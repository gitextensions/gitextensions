using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;

namespace GitUI;

internal sealed class RevisionGridToolTipProvider
{
    private Control? _toolTip;
    private readonly Dictionary<(int ColumnIndex, int RowIndex), bool> _isTruncatedByCellPos = [];
    private readonly RevisionGridControl _gridView;
    private int _previousRowIndex = -1;
    private int _previousColumnIndex = -1;
    private object? _previousHighlight = null;
    private readonly Dictionary<Control, CellState> _cellStates = [];

    public RevisionGridToolTipProvider(RevisionGridControl gridView)
    {
        _gridView = gridView;
    }

    public bool ShowRevisionGridTooltips { get; set; }

    /// <summary>
    /// Hides the tooltip.
    /// </summary>
    /// <returns>Returns <cref>true</cref> if the tooltip was active.</returns>
    public bool Hide()
    {
        bool wasActive = _toolTip is not null && ToolTip.GetIsOpen(_toolTip);
        if (_toolTip is not null)
        {
            ToolTip.SetIsOpen(_toolTip, false);
            _toolTip = null;
        }

        return wasActive;
    }

    // Avalonia's virtualized grid reports the realized cell and revision instead of DataGridView coordinates.
    public void OnCellMouseMove(Control cell, PointerEventArgs e)
    {
        if (!_cellStates.TryGetValue(cell, out CellState? state) || state is null)
        {
            return;
        }

        object? highlight = (e.Source as Control) is { } source && !ReferenceEquals(source, cell)
            ? ToolTip.GetTip(source)
            : null;
        if (highlight is not null)
        {
            if (ReferenceEquals(highlight, _previousHighlight))
            {
                return;
            }

            _previousHighlight = highlight;
            _previousRowIndex = -1;
            UpdateToolTip(highlight);
            return;
        }

        _previousHighlight = null;
        SetTruncation(state.ColumnIndex, state.RowIndex, IsTruncated(cell));

        if (!ShowRevisionGridTooltips)
        {
            ToolTip.SetTip(cell, null);
            return;
        }

        // Always generated tooltip text of first column (graph) because it **really** depends of the pixel hovered
        if (state.ColumnIndex != 0 && _previousRowIndex == state.RowIndex && _previousColumnIndex == state.ColumnIndex)
        {
            return;
        }

        _previousRowIndex = state.RowIndex;
        _previousColumnIndex = state.ColumnIndex;
        UpdateToolTip();

        return;

        void UpdateToolTip(object? highlightToolTip = null)
        {
            string newText = GetToolTipText(highlightToolTip);
            object? tip = string.IsNullOrEmpty(newText) ? null : newText;
            if (!Equals(ToolTip.GetTip(cell), tip))
            {
                ToolTip.SetTip(cell, tip);
            }

            _toolTip = tip is null ? null : cell;
        }

        string GetToolTipText(object? highlightToolTip)
        {
            try
            {
                if (highlightToolTip?.ToString() is { Length: > 0 } highlightText)
                {
                    return highlightText;
                }

                ColumnProvider provider = _gridView.ColumnProviders[state.ColumnIndex];
                if (provider.TryGetToolTip(state.Revision, out string? toolTip)
                    && !string.IsNullOrWhiteSpace(toolTip))
                {
                    return toolTip;
                }

                if (_isTruncatedByCellPos.TryGetValue((state.ColumnIndex, state.RowIndex), out bool showToolTip)
                    && showToolTip)
                {
                    return GetCellText(cell);
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
        _toolTip = null;
    }

    public void SetTruncation(int columnIndex, int rowIndex, bool truncated)
    {
        _isTruncatedByCellPos[(columnIndex, rowIndex)] = truncated;
    }

    public void UpdateCell(Control cell, int columnIndex, int rowIndex, GitRevision revision)
    {
        if (!_cellStates.ContainsKey(cell))
        {
            cell.PointerMoved += OnCellPointerMoved;
        }

        _cellStates[cell] = new CellState(columnIndex, rowIndex, revision);
        TextBlock? textBlock = cell as TextBlock ?? cell.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault();
        SetTruncation(columnIndex, rowIndex, IsTruncated(textBlock));

        void OnCellPointerMoved(object? sender, PointerEventArgs e)
        {
            OnCellMouseMove(cell, e);
        }
    }

    private static string GetCellText(Control cell)
    {
        TextBlock? textBlock = cell as TextBlock ?? cell.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault();
        return textBlock?.Text ?? "";
    }

    private static bool IsTruncated(Control cell)
    {
        TextBlock? textBlock = cell as TextBlock ?? cell.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault();
        return IsTruncated(textBlock);
    }

    private static bool IsTruncated(TextBlock? textBlock)
        => textBlock?.TextLayout.TextLines.Any(line => line.HasCollapsed || line.HasOverflowed) == true;

    private sealed record CellState(int ColumnIndex, int RowIndex, GitRevision Revision);
}
