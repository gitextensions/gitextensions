using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Size = Avalonia.Size;

namespace GitUI.Compat;

/// <summary>
///  Allocates both menu captions and shortcuts to the source renderer's shared text rectangle.
/// </summary>
public sealed class NativeToolStripMenuItemPanel : Panel
{
    public NativeToolStripMenuItemPanel()
    {
    }

    protected override bool BypassFlowDirectionPolicies => true;

    protected override Size MeasureOverride(Size availableSize)
    {
        MenuItem? item = this.GetVisualAncestors().OfType<MenuItem>().FirstOrDefault();
        NativeToolStripDropDownLayout.Group? group = item is null ? null : NativeToolStripDropDownLayout.GetGroup(item);
        foreach (Control child in Children)
        {
            child.Measure(Size.Infinity);
        }

        return group is null ? default : new Size(group.ItemWidth, group.ItemHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        MenuItem? item = this.GetVisualAncestors().OfType<MenuItem>().FirstOrDefault();
        NativeToolStripDropDownLayout.Group? group = item is null ? null : NativeToolStripDropDownLayout.GetGroup(item);
        foreach (Control child in Children)
        {
            Rect bounds = new(finalSize);
            if (group is not null && item != group.FilterHost)
            {
                bounds = child.Name switch
                {
                    "PART_IconPresenter" => group.ImageRectangle,
                    "PART_ChevronPath" => group.ArrowRectangle,
                    _ => group.TextRectangle,
                };
                if (child is ContentPresenter header)
                {
                    header.SetCurrentValue(ContentPresenter.PaddingProperty, group.GetTextPadding(item!));
                    header.SetCurrentValue(ContentPresenter.HorizontalContentAlignmentProperty,
                        group.RightToLeft ? HorizontalAlignment.Right : HorizontalAlignment.Left);
                }

                if (child is TextBlock shortcut && item is not null)
                {
                    shortcut.SetCurrentValue(TextBlock.PaddingProperty, group.GetShortcutPadding(item));
                    shortcut.SetCurrentValue(TextBlock.TextProperty, NativeToolStripDropDownLayout.Group.GetShortcut(item));
                    shortcut.SetCurrentValue(TextBlock.TextAlignmentProperty,
                        group.RightToLeft ? Avalonia.Media.TextAlignment.Left : Avalonia.Media.TextAlignment.Right);
                }

                if (child is NativeToolStripMenuArrow arrow && item is not null)
                {
                    arrow.SetCurrentValue(IsVisibleProperty, item.Items.Count > 0);
                }
            }

            child.Arrange(bounds);
        }

        return finalSize;
    }
}
