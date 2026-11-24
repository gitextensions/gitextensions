using Avalonia;
using Avalonia.Controls;
using Size = Avalonia.Size;

namespace GitUI.Compat;

/// <summary>
///  Preserves ToolStripDropDownMenu's default frame allocation for retained filter menus.
/// </summary>
/// <remarks>
///  StackPanel supplies the framework's vertical keyboard navigation while the overrides
///  retain the source dropdown's layout instead of its default stacking geometry.
///  Its ordinary MenuItem children retain framework RTL presentation; canceling that
///  policy on their panel reflects rows within the extra popup border pixel.
/// </remarks>
public sealed class NativeFilterToolStripDropDownPanel : StackPanel
{
    private const double SeparatorHeight = 6;
    private const double SeparatorMargin = 2;
    internal static readonly AttachedProperty<NativeToolStripDropDownLayout.Group?> GroupProperty =
        AvaloniaProperty.RegisterAttached<NativeFilterToolStripDropDownPanel, Control, NativeToolStripDropDownLayout.Group?>("Group", inherits: true);

    public NativeFilterToolStripDropDownPanel()
    {
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        NativeToolStripDropDownLayout.Group? group = GetGroup();

        // ToolStrip.GetPreferredSizeCore adds DefaultPadding after measuring flow rows;
        // the renderer's border is painted over that canvas, not another layout inset.
        double height = group?.Padding.Top + group?.Padding.Bottom ?? 0;
        double width = group?.PopupWidth ?? 0;
        foreach (Control child in Children)
        {
            child.Measure(Size.Infinity);
            if (child.IsVisible)
            {
                height += GetHeight(child, group);
                width = Math.Max(width, child.DesiredSize.Width);
            }
        }

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        NativeToolStripDropDownLayout.Group? group = GetGroup();
        double y = group?.Padding.Top ?? 0;
        foreach (Control child in Children)
        {
            if (!child.IsVisible)
            {
                child.Arrange(default);
                continue;
            }

            double height = GetHeight(child, group);
            Rect bounds = child is Separator
                ? new Rect(SeparatorMargin, y, Math.Max(0, finalSize.Width - (SeparatorMargin * 2)), height)
                : new Rect(0, y, group?.ItemWidth ?? finalSize.Width, height);
            child.Arrange(bounds);
            y += height;
        }

        return finalSize;
    }

    private NativeToolStripDropDownLayout.Group? GetGroup()
        => Children.Select(child => child.GetValue(GroupProperty)).FirstOrDefault(group => group is not null);

    private static double GetHeight(Control child, NativeToolStripDropDownLayout.Group? group)
        => child is Separator ? SeparatorHeight : group?.ItemHeight ?? child.DesiredSize.Height;
}
