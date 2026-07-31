using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace GitUI.Compat;

/// <summary>Draws the one-pixel dotted keyboard-focus rectangle used by WinForms controls.</summary>
internal sealed class WinFormsFocusAdorner : Control
{
    private static readonly DashStyle DottedLine = new([1, 1], 0);

    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<WinFormsFocusAdorner, IBrush?>(nameof(Stroke));

    public WinFormsFocusAdorner()
    {
        Focusable = false;
        IsHitTestVisible = false;
    }

    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        IBrush? stroke = Stroke;
        if (stroke is null
            && ResourceNodeExtensions.TryFindResource(this, "GitExtensionsWindowTextBrush", out object? resource))
        {
            stroke = resource as IBrush;
        }

        if (stroke is null || Bounds.Width <= 7 || Bounds.Height <= 7)
        {
            return;
        }

        Rect rectangle = new(3.5, 3.5, Bounds.Width - 7, Bounds.Height - 7);
        Pen pen = new(stroke, 1, DottedLine, PenLineCap.Flat, PenLineJoin.Miter, 10);
        context.DrawRectangle(brush: null, pen, rectangle);
    }
}

/// <summary>Creates a focus adorner through the template contract used by Avalonia controls.</summary>
internal sealed class WinFormsFocusAdornerTemplate : ITemplate<Control>
{
    public Control Build() => new WinFormsFocusAdorner();

    object Avalonia.Styling.ITemplate.Build() => Build();
}
