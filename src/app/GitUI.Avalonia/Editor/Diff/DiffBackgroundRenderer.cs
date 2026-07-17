using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using MediaColor = Avalonia.Media.Color;

namespace GitUI.Editor.Diff;

internal sealed class DiffBackgroundRenderer : IBackgroundRenderer
{
    private readonly Control _owner;
    private DiffHighlightService? _highlightService;

    public DiffBackgroundRenderer(Control owner)
    {
        _owner = owner;
    }

    public KnownLayer Layer => KnownLayer.Background;

    public void SetHighlightService(DiffHighlightService? highlightService)
    {
        _highlightService = highlightService;
    }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        DiffHighlightService? service = _highlightService;
        if (service is null || !textView.VisualLinesValid)
        {
            return;
        }

        foreach (VisualLine visualLine in textView.VisualLines)
        {
            int lineNumber = visualLine.FirstDocumentLine.LineNumber;
            if (!service.LinesInfo.DiffLines.TryGetValue(lineNumber, out DiffLineInfo? info))
            {
                continue;
            }

            IBrush? brush = GetLineBrush(info, service.UseBackgroundColoring);
            if (brush is not null)
            {
                drawingContext.FillRectangle(
                    brush,
                    new Avalonia.Rect(
                        0,
                        visualLine.VisualTop - textView.ScrollOffset.Y,
                        textView.Bounds.Width,
                        visualLine.Height));
            }
        }

        if (service.UseBackgroundColoring)
        {
            foreach (DiffTextMarker marker in service.TextMarkers)
            {
                DrawSegment(textView, drawingContext, marker.Offset, marker.Length, GetMarkerBrush(marker.Kind));
            }

            foreach (DiffInlineMarker marker in service.InlineMarkers)
            {
                IBrush dimBrush = marker.IsRemoved
                    ? GetBrush("GitExtensionsDiffRemovedDimBrush", MediaColor.Parse("#FFF1F1"))
                    : GetBrush("GitExtensionsDiffAddedDimBrush", MediaColor.Parse("#F1FFF1"));
                DrawSegment(textView, drawingContext, marker.Offset, marker.Length, dimBrush);
            }
        }
    }

    private IBrush? GetLineBrush(DiffLineInfo info, bool useBackgroundColoring)
    {
        if (info.LineType == DiffLineType.Header)
        {
            return GetBrush("GitExtensionsDiffSectionBrush", MediaColor.Parse("#E6E6E6"));
        }

        if (!useBackgroundColoring)
        {
            return null;
        }

        return info.LineType switch
        {
            DiffLineType.Minus => info.IsMovedLine
                ? GetBrush("GitExtensionsDiffMovedRemovedBrush", MediaColor.Parse("#F6CEFF"))
                : GetBrush("GitExtensionsDiffRemovedBrush", MediaColor.Parse("#FFC8C8")),
            DiffLineType.Plus => info.IsMovedLine
                ? GetBrush("GitExtensionsDiffMovedAddedBrush", MediaColor.Parse("#C8D0F4"))
                : GetBrush("GitExtensionsDiffAddedBrush", MediaColor.Parse("#C8FFC8")),
            _ => null,
        };
    }

    private IBrush GetMarkerBrush(DiffMarkerKind kind)
        => kind switch
        {
            DiffMarkerKind.Removed => GetBrush("GitExtensionsDiffRemovedBrush", MediaColor.Parse("#FFC8C8")),
            DiffMarkerKind.Added => GetBrush("GitExtensionsDiffAddedBrush", MediaColor.Parse("#C8FFC8")),
            DiffMarkerKind.MovedRemoved => GetBrush("GitExtensionsDiffMovedRemovedBrush", MediaColor.Parse("#F6CEFF")),
            DiffMarkerKind.MovedAdded => GetBrush("GitExtensionsDiffMovedAddedBrush", MediaColor.Parse("#C8D0F4")),
            _ => Brushes.Transparent,
        };

    private static void DrawSegment(TextView textView, DrawingContext context, int offset, int length, IBrush brush)
    {
        if (length <= 0 || textView.Document is null || offset >= textView.Document.TextLength)
        {
            return;
        }

        SimpleSegment segment = new(offset, Math.Min(length, textView.Document.TextLength - offset));
        foreach (Avalonia.Rect rectangle in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment))
        {
            context.FillRectangle(brush, rectangle);
        }
    }

    private IBrush GetBrush(string key, MediaColor fallback) => DiffBrushes.Get(_owner, key, fallback);
}
