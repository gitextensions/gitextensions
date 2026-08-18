using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using GitUI.Theming;
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
                    GetAppColor(info.IsMovedLine ? AppColor.AnsiTerminalMagentaForeNormal : AppColor.AnsiTerminalRedForeNormal)),
                DiffLineType.Plus => GetBrush(
                    info.IsMovedLine ? "GitExtensionsDiffMovedAddedForegroundBrush" : "GitExtensionsDiffAddedForegroundBrush",
                    GetAppColor(info.IsMovedLine ? AppColor.AnsiTerminalBlueForeNormal : AppColor.AnsiTerminalGreenForeNormal)),
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
            IBrush markerBrush = marker.ForeColor is System.Drawing.Color foreColor
                ? new SolidColorBrush(AvaloniaThemeResources.ToMediaColor(foreColor))
                : marker.Kind switch
            {
                DiffMarkerKind.Removed => GetBrush("GitExtensionsDiffRemovedForegroundBrush", GetAppColor(AppColor.AnsiTerminalRedForeNormal)),
                DiffMarkerKind.Added => GetBrush("GitExtensionsDiffAddedForegroundBrush", GetAppColor(AppColor.AnsiTerminalGreenForeNormal)),
                DiffMarkerKind.MovedRemoved => GetBrush("GitExtensionsDiffMovedRemovedForegroundBrush", GetAppColor(AppColor.AnsiTerminalMagentaForeNormal)),
                DiffMarkerKind.MovedAdded => GetBrush("GitExtensionsDiffMovedAddedForegroundBrush", GetAppColor(AppColor.AnsiTerminalBlueForeNormal)),
                _ => new SolidColorBrush(GetSystemColor(System.Drawing.KnownColor.WindowText)),
            };
            ChangeLinePart(start, end, element => element.TextRunProperties.SetForegroundBrush(markerBrush));
        }

        foreach (DiffInlineMarker marker in service.InlineMarkers.Where(marker => marker.Offset < line.EndOffset && marker.Offset + marker.Length > line.Offset))
        {
            int start = Math.Max(marker.Offset, line.Offset);
            int end = Math.Min(marker.Offset + marker.Length, line.EndOffset);
            IBrush dimBrush = marker.IsRemoved
                ? GetBrush("GitExtensionsDiffRemovedDimForegroundBrush", GetDimmedAppColor(AppColor.AnsiTerminalRedForeNormal))
                : GetBrush("GitExtensionsDiffAddedDimForegroundBrush", GetDimmedAppColor(AppColor.AnsiTerminalGreenForeNormal));
            ChangeLinePart(start, end, element => element.TextRunProperties.SetForegroundBrush(dimBrush));
        }
    }

    private IBrush GetBrush(string key, MediaColor fallback) => DiffBrushes.Get(_owner, key, fallback);

    private static MediaColor GetAppColor(AppColor name)
        => AvaloniaThemeResources.ToMediaColor(AvaloniaThemeResources.ResolveAppColor(ThemeModule.Settings, name));

    private static MediaColor GetDimmedAppColor(AppColor name)
        => AvaloniaThemeResources.ToMediaColor(
            AvaloniaThemeResources.ResolveAppColor(ThemeModule.Settings, name).DimColor());

    private static MediaColor GetSystemColor(System.Drawing.KnownColor name)
        => AvaloniaThemeResources.ToMediaColor(AvaloniaThemeResources.ResolveSystemColor(ThemeModule.Settings, name));
}
