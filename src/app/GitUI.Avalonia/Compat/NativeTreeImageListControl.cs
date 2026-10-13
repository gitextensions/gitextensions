using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;
using Color = Avalonia.Media.Color;

namespace GitUI.Compat;

/// <summary>
/// Supplies the repository icon's live native header backdrop without changing Image layout.
/// </summary>
internal sealed class NativeTreeImageListControl : Image
{
    private readonly List<Control> _paintAncestors = [];
    private readonly List<SolidColorBrush> _paintBrushes = [];

    public NativeTreeImageListControl()
    {
    }

    internal NativeTreeImageListControl(IImage original)
    {
        Source = NativeTreeImageListImage.Create(original, ResolveBackdrop);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OpacityProperty)
        {
            InvalidateVisual();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        foreach (Control ancestor in this.GetVisualAncestors().OfType<Control>())
        {
            _paintAncestors.Add(ancestor);
            ancestor.PropertyChanged += OnAncestorPaintChanged;
        }

        ObserveBrushes();
        InvalidateVisual();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        foreach (Control ancestor in _paintAncestors)
        {
            ancestor.PropertyChanged -= OnAncestorPaintChanged;
        }

        _paintAncestors.Clear();
        ReleaseBrushes();
        base.OnDetachedFromVisualTree(e);
    }

    private void OnAncestorPaintChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == OpacityProperty || e.Property == Border.BackgroundProperty || e.Property == Panel.BackgroundProperty
            || e.Property == TemplatedControl.BackgroundProperty || e.Property == ContentPresenter.BackgroundProperty)
        {
            ObserveBrushes();
            InvalidateVisual();
        }
    }

    private void OnBrushPaintChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        => InvalidateVisual();

    private void ObserveBrushes()
    {
        ReleaseBrushes();
        foreach (SolidColorBrush brush in _paintAncestors.Select(GetBackground).OfType<SolidColorBrush>().Distinct())
        {
            _paintBrushes.Add(brush);
            brush.PropertyChanged += OnBrushPaintChanged;
        }
    }

    private void ReleaseBrushes()
    {
        foreach (SolidColorBrush brush in _paintBrushes)
        {
            brush.PropertyChanged -= OnBrushPaintChanged;
        }

        _paintBrushes.Clear();
    }

    private Color? ResolveBackdrop()
    {
        if (Opacity != 1 || _paintAncestors.Any(ancestor => ancestor.Opacity != 1))
        {
            return null;
        }

        foreach (Control ancestor in _paintAncestors)
        {
            IBrush? background = GetBackground(ancestor);
            if (background is null || background.Opacity == 0)
            {
                continue;
            }

            if (background is ISolidColorBrush solid)
            {
                if (solid.Color.A == 0)
                {
                    continue;
                }

                if (solid.Color.A == byte.MaxValue && solid.Opacity == 1)
                {
                    return solid.Color;
                }
            }

            // A gradient, partially transparent brush, or unknown ancestor renderer
            // has no proved uniform backdrop. Retain native premultiplied Image rendering.
            return null;
        }

        return null;
    }

    private static IBrush? GetBackground(Control control)
        => control switch
        {
            Border border => border.Background,
            Panel panel => panel.Background,
            ContentPresenter presenter => presenter.Background,
            TemplatedControl templated => templated.Background,
            _ => null,
        };
}
