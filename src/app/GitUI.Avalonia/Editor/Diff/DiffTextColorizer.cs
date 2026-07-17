using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using MediaColor = Avalonia.Media.Color;

namespace GitUI.Editor.Diff;

internal sealed class DiffTextColorizer : DocumentColorizingTransformer
{
    private readonly Control _owner;
    private DiffHighlightService? _highlightService;

    public DiffTextColorizer(Control owner)
    {
        _owner = owner;
    }

    public void SetHighlightService(DiffHighlightService? highlightService)
    {
        _highlightService = highlightService;
    }

    protected override void ColorizeLine(DocumentLine line)
    {
        DiffHighlightService? service = _highlightService;
        if (service is null || service.UseBackgroundColoring)
        {
            return;
        }

        if (service.LinesInfo.DiffLines.TryGetValue(line.LineNumber, out DiffLineInfo? info))
        {
            IBrush? lineBrush = info.LineType switch
            {
                DiffLineType.Minus => GetBrush(
                    info.IsMovedLine ? "GitExtensionsDiffMovedRemovedForegroundBrush" : "GitExtensionsDiffRemovedForegroundBrush",
                    Colors.IndianRed),
                DiffLineType.Plus => GetBrush(
                    info.IsMovedLine ? "GitExtensionsDiffMovedAddedForegroundBrush" : "GitExtensionsDiffAddedForegroundBrush",
                    Colors.SeaGreen),
                _ => null,
            };
            if (lineBrush is not null)
            {
                ChangeLinePart(line.Offset, line.EndOffset, element => element.TextRunProperties.SetForegroundBrush(lineBrush));
            }
        }

        foreach (DiffTextMarker marker in service.TextMarkers.Where(marker => marker.Offset < line.EndOffset && marker.EndOffset > line.Offset))
        {
            int start = Math.Max(marker.Offset, line.Offset);
            int end = Math.Min(marker.EndOffset, line.EndOffset);
            IBrush markerBrush = marker.Kind switch
            {
                DiffMarkerKind.Removed => GetBrush("GitExtensionsDiffRemovedForegroundBrush", Colors.IndianRed),
                DiffMarkerKind.Added => GetBrush("GitExtensionsDiffAddedForegroundBrush", Colors.SeaGreen),
                DiffMarkerKind.MovedRemoved => GetBrush("GitExtensionsDiffMovedRemovedForegroundBrush", Colors.Magenta),
                DiffMarkerKind.MovedAdded => GetBrush("GitExtensionsDiffMovedAddedForegroundBrush", Colors.Blue),
                _ => Brushes.Black,
            };
            ChangeLinePart(start, end, element => element.TextRunProperties.SetForegroundBrush(markerBrush));
        }

        foreach (DiffInlineMarker marker in service.InlineMarkers.Where(marker => marker.Offset < line.EndOffset && marker.Offset + marker.Length > line.Offset))
        {
            int start = Math.Max(marker.Offset, line.Offset);
            int end = Math.Min(marker.Offset + marker.Length, line.EndOffset);
            IBrush dimBrush = marker.IsRemoved
                ? GetBrush("GitExtensionsDiffRemovedDimForegroundBrush", Colors.Gray)
                : GetBrush("GitExtensionsDiffAddedDimForegroundBrush", Colors.Gray);
            ChangeLinePart(start, end, element => element.TextRunProperties.SetForegroundBrush(dimBrush));
        }
    }

    private IBrush GetBrush(string key, MediaColor fallback) => DiffBrushes.Get(_owner, key, fallback);
}
