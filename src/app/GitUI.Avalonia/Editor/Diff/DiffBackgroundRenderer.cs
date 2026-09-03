using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using GitUI.Theming;
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
                IBrush brush = marker.BackColor is System.Drawing.Color backColor
                    ? new SolidColorBrush(AvaloniaThemeResources.ToMediaColor(backColor))
                    : GetMarkerBrush(marker.Kind);
                DrawSegment(textView, drawingContext, marker.Offset, marker.Length, brush);
            }

            foreach (DiffInlineMarker marker in service.InlineMarkers)
            {
                IBrush dimBrush = marker.IsRemoved
                    ? GetBrush("GitExtensionsDiffRemovedDimBrush", GetDimmedAppColor(AppColor.AnsiTerminalRedBackNormal, count: 2))
                    : GetBrush("GitExtensionsDiffAddedDimBrush", GetDimmedAppColor(AppColor.AnsiTerminalGreenBackNormal, count: 2));
                DrawSegment(textView, drawingContext, marker.Offset, marker.Length, dimBrush);
            }
        }
    }

    private IBrush? GetLineBrush(DiffLineInfo info, bool useBackgroundColoring)
    {
        if (info.LineType == DiffLineType.Header)
        {
            return GetBrush("GitExtensionsDiffSectionBrush", GetAppColor(AppColor.DiffSection));
        }

        if (!useBackgroundColoring)
        {
            return null;
        }

        return info.LineType switch
        {
            DiffLineType.Minus => info.IsMovedLine
                ? GetBrush("GitExtensionsDiffMovedRemovedBrush", GetAppColor(AppColor.AnsiTerminalMagentaBackNormal))
                : GetBrush("GitExtensionsDiffRemovedBrush", GetAppColor(AppColor.AnsiTerminalRedBackNormal)),
            DiffLineType.Plus => info.IsMovedLine
                ? GetBrush("GitExtensionsDiffMovedAddedBrush", GetAppColor(AppColor.AnsiTerminalBlueBackNormal))
                : GetBrush("GitExtensionsDiffAddedBrush", GetAppColor(AppColor.AnsiTerminalGreenBackNormal)),
            _ => null,
        };
    }

    private IBrush GetMarkerBrush(DiffMarkerKind kind)
        => kind switch
        {
            DiffMarkerKind.Removed => GetBrush("GitExtensionsDiffRemovedBrush", GetAppColor(AppColor.AnsiTerminalRedBackNormal)),
            DiffMarkerKind.Added => GetBrush("GitExtensionsDiffAddedBrush", GetAppColor(AppColor.AnsiTerminalGreenBackNormal)),
            DiffMarkerKind.MovedRemoved => GetBrush("GitExtensionsDiffMovedRemovedBrush", GetAppColor(AppColor.AnsiTerminalMagentaBackNormal)),
            DiffMarkerKind.MovedAdded => GetBrush("GitExtensionsDiffMovedAddedBrush", GetAppColor(AppColor.AnsiTerminalBlueBackNormal)),
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

    private static MediaColor GetAppColor(AppColor name)
        => AvaloniaThemeResources.ToMediaColor(AvaloniaThemeResources.ResolveAppColor(ThemeModule.Settings, name));

    private static MediaColor GetDimmedAppColor(AppColor name, int count)
    {
        System.Drawing.Color color = AvaloniaThemeResources.ResolveAppColor(ThemeModule.Settings, name);
        for (int index = 0; index < count; index++)
        {
            color = color.DimColor();
        }

        return AvaloniaThemeResources.ToMediaColor(color);
    }
}
