using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace GitUI.Compat;

/// <summary>
///  Paints the Professional renderer's connected area without changing Border layout.
/// </summary>
public sealed class NativeToolStripMenuPopupConnection : Control
{
    /// <summary>
    ///  Identifies the actual menu title width supplied by the popup border.
    /// </summary>
    public static readonly StyledProperty<double> ConnectedTitleWidthProperty =
        NativeToolStripMenuPopupBorder.ConnectedTitleWidthProperty.AddOwner<NativeToolStripMenuPopupConnection>();

    /// <summary>
    ///  Identifies the popup surface used to erase the connected outline.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        Border.BackgroundProperty.AddOwner<NativeToolStripMenuPopupConnection>();

    /// <summary>
    ///  Identifies the popup's source outline thickness.
    /// </summary>
    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        Border.BorderThicknessProperty.AddOwner<NativeToolStripMenuPopupConnection>();

    static NativeToolStripMenuPopupConnection()
    {
        AffectsRender<NativeToolStripMenuPopupConnection>(ConnectedTitleWidthProperty, BackgroundProperty, BorderThicknessProperty, FlowDirectionProperty);
    }

    /// <summary>
    ///  Gets or sets the actual source title width.
    /// </summary>
    public double ConnectedTitleWidth
    {
        get => GetValue(ConnectedTitleWidthProperty);
        set => SetValue(ConnectedTitleWidthProperty, value);
    }

    /// <summary>
    ///  Gets or sets the popup surface brush.
    /// </summary>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    ///  Gets or sets the source outline thickness.
    /// </summary>
    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        // Border.Render is sealed in Avalonia. Paint its source ConnectedArea as
        // a non-interactive sibling overlay, preserving the Border's child/layout.
        double width = Math.Min(ConnectedTitleWidth, Bounds.Width) - BorderThickness.Left - BorderThickness.Right;
        if (Background is not null && width > 0)
        {
            double x = FlowDirection == FlowDirection.RightToLeft
                ? Bounds.Width - BorderThickness.Right - width
                : BorderThickness.Left;
            context.FillRectangle(Background, new Rect(x, 0, width, BorderThickness.Top));
        }
    }

    protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize) => default;
}
