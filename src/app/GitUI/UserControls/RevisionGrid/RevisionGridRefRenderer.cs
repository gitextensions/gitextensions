using System.Drawing.Drawing2D;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;

namespace GitUI.UserControls.RevisionGrid;

internal static class RevisionGridRefRenderer
{
    private static readonly float[] _dashPattern = [4, 4];
    private static readonly PointF[] _arrowPoints = new PointF[4];

    private static int PaddingTopBottom => DpiUtil.Scale(2);

    // Pixel radius for the rounded corners of ref label capsules.
    private static int RefLabelCornerRadius => DpiUtil.Scale(5);

    // Pixel width of the highlight frame drawn around a hovered ref label,
    // and the left-side offset used when drawing the nestled remote label.
    private static int RefLabelHighlightWidth => DpiUtil.Scale(1);

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
        icon = GetEffectiveIcon(icon);
        int paddingLeftRight = PaddingLeftRight(name);
        int paddingTopBottom = PaddingTopBottom;
        int marginRight = DpiUtil.Scale(5);

        Color textColor = fill ? headColor : ColorHelper.Lerp(headColor, Color.Black, 0.25F);

        Size textSize = !string.IsNullOrEmpty(name)
            ? TextRenderer.MeasureText(graphics, name, font, Size.Empty, TextFormatFlags.NoPadding)
            : new(0, TextRenderer.MeasureText(graphics, " ", font, Size.Empty, TextFormatFlags.NoPadding).Height);

        int iconWidth = icon == RefLabelIcon.None ? 0 : bounds.Height / 2;

        int backgroundHeight = textSize.Height + paddingTopBottom + paddingTopBottom - 1;
        int outerMarginTopBottom = (bounds.Height - backgroundHeight) / 2;

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

        Rectangle rect = new(
            bounds.X + offset,
            bounds.Y + outerMarginTopBottom,
            Math.Min(bounds.Width - offset, textSize.Width + iconWidth + paddingLeftRight + paddingLeftRight + extraWidth - 1),
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
        int iconXOffset = shape is RefLabelShape.NotchLeft or RefLabelShape.PointLeft ? pointWidth : 0;
        DrawRefBackground(isRowSelected, graphics, headColor, rect, refPath, icon, dashedLine, fill, highlight: false, iconXOffset);

        // For PointLeft, offset by half pointWidth so text starts inside the point.
        int textX = rect.X + iconXOffset + iconWidth + paddingLeftRight - (shape is RefLabelShape.PointLeft ? pointWidth / 2 : 0);
        int textWidth = Math.Min(bounds.Width - offset - paddingLeftRight - paddingLeftRight, textSize.Width);
        Rectangle textBounds = new(
            textX,
            rect.Y + paddingTopBottom - 1,
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

    private static void DrawRefBackground(bool isRowSelected, Graphics graphics, Color color, Rectangle bounds, GraphicsPath path, RefLabelIcon icon, bool dashedLine, bool fill, bool highlight, int iconXOffset)
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

            // arrow if the head is the current branch
            if (icon != RefLabelIcon.None)
            {
                DrawArrow(graphics, bounds.X + iconXOffset, bounds.Y, bounds.Height, color, filled: icon == RefLabelIcon.Head);
            }
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

    private static void DrawArrow(Graphics graphics, float x, float y, float rowHeight, Color color, bool filled)
    {
        ThreadHelper.AssertOnUIThread();

        float horShift = DpiUtil.Scale(4f);
        float verShift = DpiUtil.Scale(3f);

        float height = rowHeight - (verShift * 2);
        float width = height / 2;

        x += horShift;
        y += verShift;

        _arrowPoints[0] = new PointF(x, y);
        _arrowPoints[1] = new PointF(x + width, y + (height / 2));
        _arrowPoints[2] = new PointF(x, y + height);
        _arrowPoints[3] = new PointF(x, y);

        if (filled)
        {
            using SolidBrush brush = new(color);
            graphics.FillPolygon(brush, _arrowPoints);
        }
        else
        {
            using Pen pen = new(color);
            graphics.DrawPolygon(pen, _arrowPoints);
        }
    }

    private static RefLabelIcon GetEffectiveIcon(RefLabelIcon icon)
    {
        return icon is RefLabelIcon.Head or RefLabelIcon.HeadMergeSource ? icon : RefLabelIcon.None;
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
