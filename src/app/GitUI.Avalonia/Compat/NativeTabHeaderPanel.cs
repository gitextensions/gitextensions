using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Size = Avalonia.Size;

namespace GitUI.Compat;

/// <summary>
///  Allocates a native tab's image and caption independently of the selected outline.
/// </summary>
public sealed class NativeTabHeaderPanel : Panel
{
    // ControlDpiExtensions supplies eight horizontal pixels around the image and label;
    // the native common control uses the same spacing between those two elements.
    private const int ImageTextSpacing = 8;

    private readonly HashSet<Control> _measuredChildren = [];

    public NativeTabHeaderPanel()
    {
        Children.CollectionChanged += OnChildrenChanged;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        Size desired = default;
        bool hasImage = false;
        double captionMinimum = 0;
        foreach (Control child in Children)
        {
            child.Measure(Size.Infinity);
            if (!child.IsVisible || child is Image { Source: null })
            {
                continue;
            }

            Size size = child is TextBlock text
                ? WinFormsTextMeasurer.MeasureSize(text, GetCaption(text))
                : child.DesiredSize;
            if (child is TextBlock emptyCaption && size.Height == 0)
            {
                size = new Size(size.Width, WinFormsTextMeasurer.MeasureSize(emptyCaption, "0").Height);
            }

            desired = new Size(desired.Width + Math.Ceiling(size.Width), Math.Max(desired.Height, Math.Ceiling(size.Height)));
            hasImage |= child is Image;
            if (child is TextBlock caption)
            {
                captionMinimum = WinFormsTextMeasurer.GetTabCaptionMinimumWidth(caption);
            }
        }

        double width = desired.Width + (hasImage ? ImageTextSpacing : 0);
        return new Size(Math.Max(width, captionMinimum), desired.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double contentWidth = 0;
        foreach (Control child in Children)
        {
            if (child.IsVisible && child is not Image { Source: null })
            {
                contentWidth += child is TextBlock text
                    ? Math.Ceiling(WinFormsTextMeasurer.MeasureSize(text, GetCaption(text)).Width)
                    : child.DesiredSize.Width + (child is Image ? ImageTextSpacing : 0);
            }
        }

        // Short image-free captions are centred inside the native minimum allocation.
        double x = Math.Floor((finalSize.Width - contentWidth) / 2);
        foreach (Control child in Children)
        {
            if (!child.IsVisible || child is Image { Source: null })
            {
                continue;
            }

            Size size = child is TextBlock text
                ? WinFormsTextMeasurer.MeasureSize(text, GetCaption(text))
                : child.DesiredSize;
            if (child is TextBlock emptyCaption && size.Height == 0)
            {
                size = new Size(size.Width, WinFormsTextMeasurer.MeasureSize(emptyCaption, "0").Height);
            }

            double width = Math.Ceiling(size.Width);
            double height = Math.Ceiling(size.Height);
            double y = Math.Floor((finalSize.Height - height) / 2);
            child.Arrange(new Rect(x, y, width, height));
            x += width + (child is Image ? ImageTextSpacing : 0);
        }

        return finalSize;
    }

    private static string GetCaption(TextBlock text)
        => text is AccessText
            ? AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(text.Text ?? string.Empty)
            : text.Text ?? string.Empty;

    private void OnChildrenChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        foreach (Control child in _measuredChildren.Where(child => !Children.Contains(child)).ToArray())
        {
            child.PropertyChanged -= OnChildPropertyChanged;
            _measuredChildren.Remove(child);
        }

        foreach (Control child in Children)
        {
            if (_measuredChildren.Add(child))
            {
                child.PropertyChanged += OnChildPropertyChanged;
            }
        }

        InvalidateMeasure();
    }

    private void OnChildPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        // A fixed-size Image can retain its desired size after Source becomes null.
        // Our allocation still changes: native tabs omit both the image and its gap.
        if (e.Property == Image.SourceProperty || e.Property == IsVisibleProperty
            || e.Property == TextBlock.TextProperty || e.Property == TextBlock.FontFamilyProperty
            || e.Property == TextBlock.FontSizeProperty || e.Property == TextBlock.FontWeightProperty
            || e.Property == TextBlock.FontStyleProperty)
        {
            InvalidateMeasure();
        }
    }
}
