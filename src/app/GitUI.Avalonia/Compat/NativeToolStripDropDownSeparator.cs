using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace GitUI.Compat;

/// <summary>
///  Keeps a dropdown separator's source allocation and paints its native renderer's line.
/// </summary>
public sealed class NativeToolStripDropDownSeparator : Separator
{
    private const int SystemHorizontalCap = 2;
    private const int SystemShadowRow = 2;
    private const int SystemHighlightRow = 3;
    private const int ProfessionalPaddingInset = 2;

    /// <summary>
    ///  Identifies the source choice between System and Professional rendering.
    /// </summary>
    public static readonly StyledProperty<bool> UseSystemVisualStyleProperty =
        AvaloniaProperty.Register<NativeToolStripDropDownSeparator, bool>(nameof(UseSystemVisualStyle));

    /// <summary>
    ///  Identifies ProfessionalColorTable.SeparatorDark's resolved native color.
    /// </summary>
    public static readonly StyledProperty<IBrush?> DarkBrushProperty =
        AvaloniaProperty.Register<NativeToolStripDropDownSeparator, IBrush?>(nameof(DarkBrush));

    private static readonly IBrush SystemShadow = Brush.Parse("#73000000");
    private static readonly IBrush SystemHighlight = Brush.Parse("#4DFFFFFF");

    static NativeToolStripDropDownSeparator()
    {
        AffectsRender<NativeToolStripDropDownSeparator>(UseSystemVisualStyleProperty, DarkBrushProperty, FlowDirectionProperty);
    }

    /// <summary>
    ///  Initializes renderer-only chrome on the retained source separator identity.
    /// </summary>
    public NativeToolStripDropDownSeparator()
    {
        IsHitTestVisible = false;
        Focusable = false;
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
    }

    /// <summary>
    ///  Gets or sets the source ThemeFix choice between System and Professional rendering.
    /// </summary>
    public bool UseSystemVisualStyle
    {
        get => GetValue(UseSystemVisualStyleProperty);
        set => SetValue(UseSystemVisualStyleProperty, value);
    }

    /// <summary>
    ///  Gets or sets ProfessionalColorTable.SeparatorDark's resolved native color.
    /// </summary>
    public IBrush? DarkBrush
    {
        get => GetValue(DarkBrushProperty);
        set => SetValue(DarkBrushProperty, value);
    }

    protected override bool BypassFlowDirectionPolicies => true;

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        int width = (int)Bounds.Width;
        int height = (int)Bounds.Height;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        using DrawingContext.PushedState clip = context.PushClip(new Rect(Bounds.Size));
        if (UseSystemVisualStyle)
        {
            // TOOLBAR part 6 uses a 5x6 premultiplied image with two-pixel horizontal
            // caps. Its only ink is black alpha115 at (2,2), white alpha77 at (2,3).
            // Stretch the center over this item's actual backdrop, not its raw BackColor.
            if (width > SystemHorizontalCap * 2 && height > SystemHighlightRow)
            {
                context.FillRectangle(SystemShadow, new Rect(SystemHorizontalCap, SystemShadowRow, width - (SystemHorizontalCap * 2), 1));
                context.FillRectangle(SystemHighlight, new Rect(SystemHorizontalCap, SystemHighlightRow, width - (SystemHorizontalCap * 2), 1));
            }

            return;
        }

        NativeToolStripDropDownLayout.Group? group = NativeToolStripDropDownLayout.GetGroup(this);
        if (group is null || DarkBrush is null)
        {
            return;
        }

        // ToolStripProfessionalRenderer offsets dropdown separators by the parent's
        // actual image/text padding. Unlike toolbar separators, it paints no light pen.
        bool rightToLeft = group.RightToLeft;
        int first = rightToLeft ? ProfessionalPaddingInset : (int)group.Padding.Left - ProfessionalPaddingInset;
        int last = rightToLeft ? group.PopupWidth - (int)group.Padding.Right - 1 : group.PopupWidth - 1;
        if (first != last)
        {
            context.FillRectangle(DarkBrush, new Rect(Math.Min(first, last), height / 2, Math.Abs(last - first) + 1, 1));
        }
    }
}
