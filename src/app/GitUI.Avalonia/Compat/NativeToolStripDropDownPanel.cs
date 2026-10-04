using Avalonia;
using Avalonia.Controls;
using Size = Avalonia.Size;

namespace GitUI.Compat;

/// <summary>
///  Arranges an opted-in dropdown with ToolStrip's top-down layout and RTL control-host rules.
/// </summary>
public sealed class NativeToolStripDropDownPanel : Panel
{
    public NativeToolStripDropDownPanel()
    {
    }

    protected override bool BypassFlowDirectionPolicies => true;

    protected override Size MeasureOverride(Size availableSize)
    {
        NativeToolStripDropDownLayout.Group? group = Children.Select(NativeToolStripDropDownLayout.GetGroup).FirstOrDefault(value => value is not null);
        double height = 4;
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
        NativeToolStripDropDownLayout.Group? group = Children.Select(NativeToolStripDropDownLayout.GetGroup).FirstOrDefault(value => value is not null);
        double y = 2;
        foreach (Control child in Children)
        {
            if (!child.IsVisible)
            {
                child.Arrange(default);
                continue;
            }

            double height = GetHeight(child, group);
            Rect bounds = new(0, y, group?.ItemWidth ?? finalSize.Width, height);
            if (child is Separator)
            {
                bounds = new Rect(2, y, Math.Max(0, finalSize.Width - 4), height);
            }
            else if (group?.FilterHost == child && group.Filter is TextBox filter)
            {
                double x = group.Padding.Left;
                if (group.RightToLeft)
                {
                    // FlowLayout's RTL container proxy mirrors the vertical display
                    // rectangle, including its leading padding and the hosted margin.
                    double displayRight = finalSize.Width - group.Padding.Right;
                    double outerX = displayRight - (group.Padding.Left + filter.Margin.Left + filter.Width);
                    x = outerX - filter.Margin.Left;
                }

                bounds = new Rect(x, y, filter.Width + filter.Margin.Left + filter.Margin.Right, height);
            }

            child.Arrange(bounds);
            y += height;
        }

        return finalSize;
    }

    private static double GetHeight(Control child, NativeToolStripDropDownLayout.Group? group)
        => child is Separator ? 6
            : child == group?.FilterHost && group.Filter is TextBox filter
                ? filter.Height + filter.Margin.Top + filter.Margin.Bottom
                : group?.ItemHeight ?? child.DesiredSize.Height;
}
