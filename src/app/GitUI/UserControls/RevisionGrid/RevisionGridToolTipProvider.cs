using System.Diagnostics;
using GitExtensions.Extensibility.Git;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Columns;

namespace GitUI;

internal sealed class RevisionGridToolTipProvider
{
    /// <summary>
    ///  Lane info and commit bodies are long, so they must stay readable much longer than the default 5 seconds.
    /// </summary>
    private const int _autoPopDelayMilliseconds = short.MaxValue;

    private readonly ToolTip _toolTip = new() { AutoPopDelay = _autoPopDelayMilliseconds };
    private readonly Dictionary<Point, bool> _isTruncatedByCellPos = [];
    private readonly RevisionDataGridView _gridView;
    private int _previousRowIndex = -1;
    private int _previousColumnIndex = -1;
    private IGitRef? _previousHighlight = null;

    public RevisionGridToolTipProvider(RevisionDataGridView gridView)
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
        bool wasActive = _toolTip.Active;
        _toolTip.Active = false;
        return wasActive;
    }

    public void OnCellMouseMove(DataGridViewCellMouseEventArgs e, RefLabelHitInfo? hitInfo)
    {
        if (hitInfo?.GitRef is { } gitRef)
        {
            if (gitRef.Equals(_previousHighlight))
            {
                return;
            }

            _previousHighlight = gitRef;
            _previousRowIndex = -1;
            UpdateToolTip(gitRef);
            return;
        }

        _previousHighlight = null;

        if (!ShowRevisionGridTooltips)
        {
            _toolTip.SetToolTip(_gridView, null);
            return;
        }

        // Always generated tooltip text of first column (graph) because it **really** depends of the pixel hovered
        if (e.ColumnIndex != 0 && _previousRowIndex == e.RowIndex && _previousColumnIndex == e.ColumnIndex)
        {
            return;
        }

        _previousRowIndex = e.RowIndex;
        _previousColumnIndex = e.ColumnIndex;
        UpdateToolTip();

        return;

        void UpdateToolTip(IGitRef? highlightRef = null)
        {
            GitUIPluginInterfaces.GitRevision? revision = _gridView.GetRevision(e.RowIndex);

            if (revision is null)
            {
                return;
            }

            string newText = GetToolTipText(revision, highlightRef);
            if (_toolTip.GetToolTip(_gridView) != newText)
            {
                // The whole grid is registered as a single tooltip "tool", so Windows neither refreshes
                // nor re-shows the tooltip while the pointer stays inside the grid: a tooltip which is
                // currently displayed would keep the outdated text until AutoPopDelay elapses.
                // Deactivating closes it, the activation below shows the new text again.
                _toolTip.Active = false;
                _toolTip.SetToolTip(_gridView, newText);
            }

            if (!_toolTip.Active)
            {
                _toolTip.Active = true;
            }
        }

        string GetToolTipText(GitUIPluginInterfaces.GitRevision revision, IGitRef? highlightRef)
        {
            try
            {
                if (_gridView.Columns[e.ColumnIndex].Tag is ColumnProvider provider
                    && provider.TryGetToolTip(e, revision, highlightRef, out string? toolTip)
                    && !string.IsNullOrWhiteSpace(toolTip))
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

        // A row index no longer identifies the same revision, so the memorized cell must not
        // suppress the update of the tooltip for the cell the pointer is resting on.
        _previousRowIndex = -1;
        _previousColumnIndex = -1;
        _previousHighlight = null;
    }

    public void SetTruncation(int columnIndex, int rowIndex, bool truncated)
    {
        _isTruncatedByCellPos[new Point(columnIndex, rowIndex)] = truncated;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly RevisionGridToolTipProvider _provider;

        public TestAccessor(RevisionGridToolTipProvider provider)
        {
            _provider = provider;
        }

        public ToolTip ToolTip => _provider._toolTip;

        public int PreviousRowIndex => _provider._previousRowIndex;

        public string? GetToolTipText() => _provider._toolTip.GetToolTip(_provider._gridView);
    }
}
