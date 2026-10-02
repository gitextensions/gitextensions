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
            desired = new Size(desired.Width + Math.Ceiling(size.Width), Math.Max(desired.Height, Math.Ceiling(size.Height)));
            hasImage |= child is Image;
        }

        return new Size(desired.Width + (hasImage ? ImageTextSpacing : 0), desired.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double x = 0;
        foreach (Control child in Children)
        {
            if (!child.IsVisible || child is Image { Source: null })
            {
                continue;
            }

            Size size = child is TextBlock text
                ? WinFormsTextMeasurer.MeasureSize(text, GetCaption(text))
                : child.DesiredSize;
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
