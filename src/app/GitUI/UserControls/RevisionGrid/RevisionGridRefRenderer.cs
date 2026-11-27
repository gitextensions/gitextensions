using System.Drawing.Drawing2D;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;

namespace GitUI.UserControls.RevisionGrid;

internal static class RevisionGridRefRenderer
{
    private static readonly float[] _dashPattern = [4, 4];

    // Cache for GetTextAscent, which is only invalidated by a font change.
    private static Font? _ascentFont;
    private static int _ascentTextHeight;
    private static int _ascent;

    private static int PaddingTopBottom => DpiUtil.Scale(2);

    // Uniform horizontal padding applied on both sides of any ref label icon.
    private static int IconPaddingLeftRight => DpiUtil.Scale(3);

    // Pixel radius for the rounded corners of ref label capsules.
    private static int RefLabelCornerRadius => DpiUtil.Scale(5);

    // Pixel width of the highlight frame drawn around a hovered ref label,
    // and the left-side offset used when drawing the nestled remote label.
    private static float RefLabelHighlightWidth => DpiUtil.ScaleX;

    private static int PointWidth(int height) => height / 2;

    private static int PaddingLeftRight(string name) => string.IsNullOrEmpty(name) ? DpiUtil.Scale(1) : DpiUtil.Scale(4);

    /// <summary>
    ///  Creates a closed path for a capsule whose left edge is a concave '>' notch
    ///  that exactly fits the convex point tip of a preceding capsule.
    /// </summary>
    private static GraphicsPath CreateNotchLeftRoundRectPath(Rectangle rect, int radius, int pointWidth)
    {
        int left = rect.X;
        int top = rect.Y;
        int right = rect.Right;
        int bottom = rect.Bottom;
        int midY = top + (rect.Height / 2);

        // The notch corners are at the leftmost pixels; the notch tip is indented by pointWidth.
        GraphicsPath path = new();
        path.AddLine(left, top, left + pointWidth, midY);                      // top notch corner → indented tip
        path.AddLine(left + pointWidth, midY, left, bottom);                   // indented tip → bottom notch corner
        path.AddArc(right - radius, bottom - radius, radius, radius, 90, -90); // bottom-right arc
        path.AddArc(right - radius, top, radius, radius, 0, -90);              // top-right arc
        path.CloseFigure();

        return path;
    }

    /// <summary>
    ///  Creates a closed path for a capsule whose right edge is a concave '&lt;' notch
    ///  that exactly fits the convex point tip of a following capsule.
    /// </summary>
    private static GraphicsPath CreateNotchRightRoundRectPath(Rectangle rect, int radius, int pointWidth)
    {
        int left = rect.X;
        int top = rect.Y;
        int right = rect.Right;
        int bottom = rect.Bottom;
        int midY = top + (rect.Height / 2);

        // The notch corners are at the rightmost pixels; the notch tip is indented by pointWidth.
        GraphicsPath path = new();
        path.AddArc(left, top, radius, radius, startAngle: 180, sweepAngle: 90); // top-left arc
        path.AddLine(right, top, right - pointWidth, midY);                      // top notch corner → indented tip
        path.AddLine(right - pointWidth, midY, right, bottom);                   // indented tip → bottom notch corner
        path.AddArc(left, bottom - radius, radius, radius, 90, 90);              // bottom-left arc
        path.CloseFigure();

        return path;
    }

    /// <summary>
    ///  Creates a closed path for a capsule whose left edge is a convex '&lt;' point
    ///  that protrudes leftward, so it visually connects to a nestled preceding label.
    /// </summary>
    private static GraphicsPath CreatePointLeftRoundRectPath(Rectangle rect, int radius, int pointWidth)
    {
        int left = rect.X;
        int top = rect.Y;
        int right = rect.Right;
        int bottom = rect.Bottom;
        int midY = top + (rect.Height / 2);

        // The point tip is at the leftmost pixel; the top/bottom corners step back by pointWidth.
        GraphicsPath path = new();
        path.AddLine(left, midY, left + pointWidth, top);                    // tip → top-left corner
        path.AddArc(right - radius, top, radius, radius, 270, 90);           // top-right arc
        path.AddArc(right - radius, bottom - radius, radius, radius, 0, 90); // bottom-right arc
        path.AddLine(left + pointWidth, bottom, left, midY);                 // bottom-left corner → tip
        path.CloseFigure();

        return path;
    }

    /// <summary>
    ///  Creates a closed path for a capsule whose right edge is a convex '&gt;' point
    ///  instead of a rounded cap, so it visually connects to a nestled following label.
    /// </summary>
    private static GraphicsPath CreatePointRightRoundRectPath(Rectangle rect, int radius, int pointWidth)
    {
        int left = rect.X;
        int top = rect.Y;
        int right = rect.Right;
        int bottom = rect.Bottom;
        int midY = top + (rect.Height / 2);

        // The point tip is at the rightmost pixel; the top/bottom corners step back by pointWidth.
        GraphicsPath path = new();
        path.AddArc(left, top, radius, radius, startAngle: 180, sweepAngle: 90); // top-left arc
        path.AddLine(right - pointWidth, top, right, midY);                      // top-right corner → tip
        path.AddLine(right, midY, right - pointWidth, bottom);                   // tip → bottom-right corner
        path.AddArc(left, bottom - radius, radius, radius, 90, 90);              // bottom-left arc
        path.CloseFigure();

        return path;
    }

    private static GraphicsPath CreateRoundRectPath(Rectangle rect, int radius)
    {
        int left = rect.X;
        int top = rect.Y;
        int right = left + rect.Width;
        int bottom = top + rect.Height;

        GraphicsPath path = new();
        path.AddArc(left, top, radius, radius, startAngle: 180, sweepAngle: 90);
        path.AddArc(right - radius, top, radius, radius, 270, 90);
        path.AddArc(right - radius, bottom - radius, radius, radius, 0, 90);
        path.AddArc(left, bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();

        return path;
    }

    /// <summary>
    ///  Draws a ref label and returns the painted rectangle in the same coordinate space as <paramref name="bounds"/>.
    /// </summary>
    /// <returns>The bounding rectangle of the drawn ref label in DataGridView client coordinates, or <see cref="Rectangle.Empty"/> if nothing was drawn.</returns>
    public static Rectangle DrawRef(bool isRowSelected, Font font, ref int offset, string name, Color headColor, RefLabelIcon icon, in Rectangle bounds, Graphics graphics, bool dashedLine = false, bool fill = false, bool highlight = false, RefLabelShape shape = RefLabelShape.Rect)
    {
        (Rectangle rect, Action? drawHighlight) = DrawRefEx(isRowSelected, font, ref offset, name, headColor, icon, bounds, graphics, dashedLine, fill, highlight, shape);
        drawHighlight?.Invoke();
        return rect;
    }

    /// <summary>
    ///  Draws a ref label with the specified edge shape and returns the bounding rectangle and an optional deferred highlight action.
    /// </summary>
    /// <returns>
    ///  The bounding rectangle of the drawn label (or <see cref="Rectangle.Empty"/> if nothing was drawn),
    ///  and a deferred action that paints the highlight frame — or <see langword="null"/> when <paramref name="highlight"/> is <see langword="false"/>.
    ///  The caller is responsible for invoking the action at the appropriate time (typically after all adjacent labels are drawn).
    /// </returns>
    public static (Rectangle Rect, Action? DrawHighlight) DrawRefEx(
        bool isRowSelected,
        Font font,
        ref int offset,
        string name,
        Color headColor,
        RefLabelIcon icon,
        in Rectangle bounds,
        Graphics graphics,
        bool dashedLine = false,
        bool fill = false,
        bool highlight = false,
        RefLabelShape shape = RefLabelShape.Rect)
    {
        int paddingLeftRight = PaddingLeftRight(name);
        int marginRight = DpiUtil.Scale(5);

        Color textColor = fill ? headColor : ColorHelper.Lerp(headColor, Color.Black, 0.25F);

        (Size textSize, int backgroundHeight, int textOffsetY, RefLabelIconRenderer? iconRenderer, int iconAreaWidth) = MeasureContent(graphics, font, name, icon);
        int outerMarginTopBottom = (bounds.Height - backgroundHeight) / 2;
        int capsuleTop = bounds.Y + outerMarginTopBottom;

        int scaledRadius = RefLabelCornerRadius;
        int pointWidth = PointWidth(backgroundHeight);

        // For notch/point the rect must include the full/half notch/point area
        // so text can be positioned within the visible portion, matching the symmetric padding of other shapes.
        int extraWidth = shape switch
        {
            RefLabelShape.NotchLeft or RefLabelShape.NotchRight => pointWidth,
            RefLabelShape.PointLeft or RefLabelShape.PointRight => pointWidth / 2,
            _ => 0
        };

        // When an icon is present, its own right-side padding (part of iconAreaWidth) already provides most of the gap
        // before the text, so only a small extra nudge (instead of a second full paddingLeftRight) is added.
        int leadingPadding = iconRenderer is null ? paddingLeftRight : DpiUtil.Scale(1);

        Rectangle rect = new(
            bounds.X + offset,
            capsuleTop,
            Math.Min(bounds.Width - offset, textSize.Width + iconAreaWidth + leadingPadding + paddingLeftRight + extraWidth - 1),
            backgroundHeight);

        if (rect.Width <= 0 || rect.Height <= 0)
        {
            // It may happen, as observed in #5396
            return (Rectangle.Empty, DrawHighlight: null);
        }

        GraphicsPath refPath = shape switch
        {
            RefLabelShape.NotchLeft => CreateNotchLeftRoundRectPath(rect, scaledRadius, pointWidth),
            RefLabelShape.NotchRight => CreateNotchRightRoundRectPath(rect, scaledRadius, pointWidth),
            RefLabelShape.PointLeft => CreatePointLeftRoundRectPath(rect, scaledRadius, pointWidth),
            RefLabelShape.PointRight => CreatePointRightRoundRectPath(rect, scaledRadius, pointWidth),
            RefLabelShape.Rect => CreateRoundRectPath(rect, scaledRadius),
            _ => throw new ArgumentOutOfRangeException(nameof(shape), $"Unsupported shape: {shape}")
        };

        // For NotchLeft and PointLeft the point/notch occupies the left portion of the rect,
        // so the icon and text must be shifted right by pointWidth.
        int iconXOffset = shape is RefLabelShape.NotchLeft or RefLabelShape.PointLeft ? pointWidth : iconRenderer is null ? 0 : DpiUtil.Scale(1);
        int iconX = rect.X + iconXOffset + IconPaddingLeftRight;
        DrawRefBackground(isRowSelected, graphics, headColor, rect, refPath, iconRenderer, dashedLine, fill, highlight: false, iconX);

        // For PointLeft, offset by half pointWidth so text starts inside the point.
        int textX = rect.X + iconXOffset + iconAreaWidth + leadingPadding - (shape is RefLabelShape.PointLeft ? pointWidth / 2 : 0);
        int textWidth = Math.Min(bounds.Width - offset - paddingLeftRight - paddingLeftRight, textSize.Width);
        Rectangle textBounds = new(
            textX,
            capsuleTop + textOffsetY,
            Math.Clamp(textWidth, 0, Math.Max(0, bounds.Right - textX)),
            textSize.Height);

        TextRenderer.DrawText(graphics, name, font, textBounds, textColor, TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

        offset += rect.Width + marginRight;

        if (!highlight)
        {
            refPath.Dispose();
            return (rect, DrawHighlight: null);
        }

        return (rect, DrawHighlight: () =>
            {
                using (refPath)
                {
                    SmoothingMode oldMode = graphics.SmoothingMode;
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    try
                    {
                        using Pen highlightPen = new(headColor, RefLabelHighlightWidth);
                        if (dashedLine)
                        {
                            highlightPen.DashPattern = _dashPattern;
                        }

                        graphics.DrawPath(highlightPen, refPath);
                    }
                    finally
                    {
                        graphics.SmoothingMode = oldMode;
                    }
                }
            });
    }

    private static void DrawRefBackground(bool isRowSelected, Graphics graphics, Color color, Rectangle bounds, GraphicsPath path, RefLabelIconRenderer? iconRenderer, bool dashedLine, bool fill, bool highlight, int iconX)
    {
        SmoothingMode oldMode = graphics.SmoothingMode;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        try
        {
            if (fill)
            {
                Color color1 = ColorHelper.Lerp(color, SystemColors.Window, 0.92F);
                Color color2 = ColorHelper.Lerp(color1, SystemColors.Window, 0.9f);
                using LinearGradientBrush brush = new(bounds, color1, color2, angle: 90);
                graphics.FillPath(brush, path);
            }
            else if (isRowSelected)
            {
                graphics.FillPath(SystemBrushes.Window, path);
            }

            // frame
            using Pen pen = new(ColorHelper.Lerp(color, SystemColors.Window, fill ? 0.83F : 0.5F));
            if (dashedLine)
            {
                pen.DashPattern = _dashPattern;
            }

            graphics.DrawPath(pen, path);

            if (highlight)
            {
                using Pen highlightPen = new(color, RefLabelHighlightWidth);
                graphics.DrawPath(highlightPen, path);
            }

            iconRenderer?.Draw(graphics, iconX, bounds.Y, color);
        }
        finally
        {
            graphics.SmoothingMode = oldMode;
        }
    }

    public static Color GetHeadColor(IGitRef gitRef)
    {
        if (gitRef.IsTag)
        {
            return AppColor.Tag.GetThemeColor();
        }

        if (gitRef.IsHead)
        {
            return AppColor.Branch.GetThemeColor();
        }

        if (gitRef.IsRemote)
        {
            return AppColor.RemoteBranch.GetThemeColor();
        }

        return AppColor.OtherTag.GetThemeColor();
    }

    /// <summary>
    ///  Computes the width of the text and icon of a ref label including their padding, excluding the shape-specific extra width.
    /// </summary>
    public static int GetContentWidth(Graphics graphics, Font font, string name, RefLabelIcon icon)
    {
        (Size textSize, _, _, _, int iconAreaWidth) = MeasureContent(graphics, font, name, icon);
        return textSize.Width + iconAreaWidth + (2 * PaddingLeftRight(name));
    }

    private static (Size TextSize, int BackgroundHeight, int TextOffsetY, RefLabelIconRenderer? IconRenderer, int IconAreaWidth) MeasureContent(
        Graphics graphics,
        Font font,
        string name,
        RefLabelIcon icon)
    {
        int paddingTopBottom = PaddingTopBottom;

        Size textSize = !string.IsNullOrEmpty(name)
            ? TextRenderer.MeasureText(graphics, name, font, Size.Empty, TextFormatFlags.NoPadding)
            : new(0, TextRenderer.MeasureText(graphics, " ", font, Size.Empty, TextFormatFlags.NoPadding).Height);

        int backgroundHeight = textSize.Height + paddingTopBottom + paddingTopBottom - 1;
        int textOffsetY = paddingTopBottom - 1;

        // The metrics are relative to the capsule and therefore constant, so the icon renderers are cached and shared.
        RefLabelIconRenderer? iconRenderer = RefLabelIconRenderer.Get(
            icon,
            new RefLabelIconMetrics(backgroundHeight, textSize.Height, textOffsetY + GetTextAscent(font, textSize.Height)));
        int iconAreaWidth = iconRenderer is null ? 0 : iconRenderer.Width + (2 * IconPaddingLeftRight);

        return (textSize, backgroundHeight, textOffsetY, iconRenderer, iconAreaWidth);
    }

    /// <summary>
    ///  Computes the distance from the top of the text to its baseline.
    /// </summary>
    /// <remarks>
    ///  The measured text height corresponds to the line spacing of the font, so the ascent can be scaled by the same ratio.
    ///  The result is cached because it only changes when the font changes.
    /// </remarks>
    private static int GetTextAscent(Font font, int textHeight)
    {
        if (ReferenceEquals(font, _ascentFont) && textHeight == _ascentTextHeight)
        {
            return _ascent;
        }

        FontFamily family = font.FontFamily;
        int lineSpacing = family.GetLineSpacing(font.Style);
        int cellAscent = family.GetCellAscent(font.Style);
        _ascent = lineSpacing > 0 ? (int)Math.Round((double)textHeight * cellAscent / lineSpacing) : textHeight;
        _ascentFont = font;
        _ascentTextHeight = textHeight;

        return _ascent;
    }

    /// <summary>
    ///  Computes the point width for the given font and graphics context, which is needed to calculate the ideal capsule size for Notch and Point shapes.
    /// </summary>
    /// <remarks>
    ///  Computes the capsule's ideal height given the same inputs as <see cref="DrawRef"/> and <see cref="DrawRefEx"/>.
    ///  Does not account for clipping to available cell width.
    /// </remarks>
    public static int GetPointWidth(Font font, Graphics graphics)
    {
        int textHeight = TextRenderer.MeasureText(graphics, " ", font, Size.Empty, TextFormatFlags.NoPadding).Height;
        int backgroundHeight = textHeight + (PaddingTopBottom * 2) - 1;
        return PointWidth(backgroundHeight);
    }
}
