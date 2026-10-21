using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using GitUIPluginInterfaces;
using MediaColor = Avalonia.Media.Color;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class MultilineIndicator : Control
{
    private const int DotCount = 3;

    private const int paddingX = 4;
    private const int paddingTop = 5;
    private const int paddingBottom = 4;
    private const int marginX = 4;
    private const int dotSize = 2;
    private const int dotSpacing = 2;

    private readonly int _indicatorReservedWidth;
    private readonly int _indicatorRectHeight;
    private readonly int _indicatorRectWidth;
    private bool _isMultiline;
    private bool _hasAvailableWidth = true;

    public MultilineIndicator()
    {
        // Avalonia scales these device-independent dimensions through its native render transform.
        _indicatorRectWidth = paddingX + paddingX + (DotCount * (dotSize + dotSpacing)) - dotSpacing;
        _indicatorReservedWidth = _indicatorRectWidth + marginX + marginX;
        _indicatorRectHeight = dotSize + paddingTop + paddingBottom;
        Width = _indicatorReservedWidth;
        Height = _indicatorRectHeight;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        IsHitTestVisible = false;
        IsVisible = false;
        Classes.Add("revision-multiline-indicator");
    }

    public void Update(GitRevision revision)
    {
        if (_isMultiline == revision.HasMultiLineMessage)
        {
            return;
        }

        _isMultiline = revision.HasMultiLineMessage;
        UpdateVisibility();
        InvalidateVisual();
    }

    public void UpdateAvailableWidth(double availableWidth)
    {
        bool hasAvailableWidth = availableWidth >= 2 * _indicatorReservedWidth;
        if (_hasAvailableWidth == hasAvailableWidth)
        {
            return;
        }

        _hasAvailableWidth = hasAvailableWidth;
        UpdateVisibility();
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        if (!IsVisible)
        {
            return;
        }

        MediaColor foreground = this.TryFindResource("GitExtensionsWindowTextBrush", ActualThemeVariant, out object? value)
            && value is SolidColorBrush brush
                ? brush.Color
                : Colors.Black;
        SolidColorBrush indicatorForeBrush = new(foreground, 128d / 255d);
        SolidColorBrush indicatorBackBrush = new(foreground, 32d / 255d);
        Rect indicatorRect = new(
            marginX,
            (Bounds.Height - _indicatorRectHeight) / 2,
            _indicatorRectWidth,
            _indicatorRectHeight);

        context.FillRectangle(indicatorBackBrush, indicatorRect);

        double x = indicatorRect.X + paddingX;
        double y = indicatorRect.Y + paddingTop;

        for (int i = 0; i < DotCount; i++)
        {
            context.FillRectangle(indicatorForeBrush, new Rect(x, y, dotSize, dotSize));
            x += dotSize + dotSpacing;
        }
    }

    private void UpdateVisibility()
        => IsVisible = _isMultiline && _hasAvailableWidth;
}
