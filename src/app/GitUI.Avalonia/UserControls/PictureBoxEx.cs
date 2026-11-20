using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using GitUI.Compat;
using GitUI.Theming;
using Color = Avalonia.Media.Color;
using KnownColor = System.Drawing.KnownColor;
using Size = Avalonia.Size;

namespace GitUI.UserControls;

/// <summary>
///  A PictureBox which handles exceptions from Render.
/// </summary>
internal sealed class PictureBoxEx : Control
{
    // Avalonia seals Image.Render. Keep the native image's sizing and drawing inside this control.
    private readonly Image _image = new();

    // The native preview loader instantiates this element from another assembly.
    public PictureBoxEx()
    {
    }

    public event EventHandler<Exception>? PaintFailed;

    public IImage? Source
    {
        get => _image.Source;
        set
        {
            _image.Source = value;
            InvalidateMeasure();
            InvalidateVisual();
        }
    }

    public Stretch Stretch
    {
        get => _image.Stretch;
        set
        {
            _image.Stretch = value;
            InvalidateMeasure();
            InvalidateVisual();
        }
    }

    public StretchDirection StretchDirection
    {
        get => _image.StretchDirection;
        set
        {
            _image.StretchDirection = value;
            InvalidateMeasure();
            InvalidateVisual();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _image.Measure(availableSize);
        return _image.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _image.Arrange(new Rect(finalSize));
        return finalSize;
    }

    public override void Render(DrawingContext context)
    {
        try
        {
            _image.Render(context);
        }
        catch (Exception ex)
        {
            Color window = AvaloniaThemeResources.ToMediaColor(
                AvaloniaThemeResources.ResolveSystemColor(ThemeModule.Settings, KnownColor.Window));
            context.FillRectangle(new SolidColorBrush(window), new Rect(Bounds.Size));
            PaintFailed?.Invoke(this, ex);
        }
    }
}
