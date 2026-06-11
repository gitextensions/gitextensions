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

    private static int ChevronWidth(int height) => height / 2;

    private static int PaddingLeftRight(string name) => string.IsNullOrEmpty(name) ? DpiUtil.Scale(1) : DpiUtil.Scale(4);

    /// <summary>
    ///  Creates a closed path for a remote capsule whose left edge is a concave '>' notch
    ///  that exactly fits the convex chevron tip of a preceding branch capsule.
    /// </summary>
    private static GraphicsPath CreateChevronLeftRoundRectPath(in Rectangle rect, int radius, int chevronWidth)
    {
        int left = rect.X;
        int top = rect.Y;
        int right = rect.Right;
        int bottom = rect.Bottom;
        int midY = top + (rect.Height / 2);

        // The notch corners are at the leftmost pixels; the notch tip is indented by chevronWidth.
        GraphicsPath path = new();
        path.AddLine(left, top, left + chevronWidth, midY);                      // top notch corner → indented tip
        path.AddLine(left + chevronWidth, midY, left, bottom);                   // indented tip → bottom notch corner
        path.AddArc(right - radius, bottom - radius, radius, radius, 90, -90);   // bottom-right arc
        path.AddArc(right - radius, top, radius, radius, 0, -90);                // top-right arc
        path.CloseFigure();

        return path;
    }

    /// <summary>
    ///  Creates a closed path for a branch capsule whose right edge is a '&gt;' chevron point
    ///  instead of a rounded cap, so it visually connects to a nestled remote label.
    /// </summary>
    private static GraphicsPath CreateChevronRightRoundRectPath(in Rectangle rect, int radius, int chevronWidth)
    {
        int left = rect.X;
        int top = rect.Y;
        int right = rect.Right;
        int bottom = rect.Bottom;
        int midY = top + (rect.Height / 2);

        // The chevron tip is at the rightmost pixel; the top/bottom corners step back by chevronWidth.
        GraphicsPath path = new();
        path.AddArc(left, top, radius, radius, startAngle: 180, sweepAngle: 90); // top-left arc
        path.AddLine(right - chevronWidth, top, right, midY);                    // top-right corner → tip
        path.AddLine(right, midY, right - chevronWidth, bottom);                 // tip → bottom-right corner
        path.AddArc(left, bottom - radius, radius, radius, 90, 90);              // bottom-left arc
        path.CloseFigure();

        return path;
    }

    internal static GraphicsPath CreateRoundRectPath(in Rectangle rect, int radius)
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
    ///  Draws a remote ref label nestled immediately after a preceding branch capsule.
    /// </summary>
    /// <remarks>
    ///  The label is drawn without an arrow and positioned flush against the right edge of the
    ///  preceding capsule, so the two labels appear as a single visual group.
    /// </remarks>
    /// <param name="precedingRight">Absolute x-coordinate of the right edge of the preceding capsule.</param>
    /// <param name="capsuleTop">Absolute y-coordinate of the top edge shared with the preceding capsule.</param>
    /// <param name="backgroundHeight">Height of the capsule, matching the preceding branch capsule.</param>
    /// <returns>
    ///  The bounding rectangle of the drawn label (or <see cref="Rectangle.Empty"/> if nothing was drawn),
    ///  and a deferred action that paints the highlight frame — or <see langword="null"/> when <paramref name="highlight"/> is <see langword="false"/>.
    ///  The caller is responsible for invoking the action at the appropriate time (typically after all adjacent labels are drawn).
    /// </returns>
    public static (Rectangle Rect, Action? DrawHighlight) DrawNestledRemoteRef(
        bool isRowSelected,
        Font font,
        string name,
        Color headColor,
        int precedingRight,
        int capsuleTop,
        int backgroundHeight,
        Graphics graphics,
        bool fill,
        bool highlight)
    {
        int paddingLeftRight = PaddingLeftRight(name);

        Size textSize = !string.IsNullOrEmpty(name)
            ? TextRenderer.MeasureText(graphics, name, font, Size.Empty, TextFormatFlags.NoPadding)
            : new(0, TextRenderer.MeasureText(graphics, " ", font, Size.Empty, TextFormatFlags.NoPadding).Height);

        // rect.X is at the notch corners (= branch ankle corners);
        // the notch tip is chevronWidth to the right, so visible content starts at rect.X + chevronWidth.
        int chevronWidth = ChevronWidth(backgroundHeight);
        Rectangle rect = new(
            precedingRight - (chevronWidth / 2),
            capsuleTop,
            chevronWidth + textSize.Width + (paddingLeftRight * 2) - 1,
            backgroundHeight);

        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return (Rectangle.Empty, DrawHighlight: null);
        }

        GraphicsPath remotePath = CreateChevronLeftRoundRectPath(rect, RefLabelCornerRadius, chevronWidth);
        DrawRefBackground(isRowSelected, graphics, headColor, rect, remotePath, RefArrowType.None, dashedLine: false, fill, highlight: false);

        // Text is right-aligned within the visible portion (right of precedingRight), keeping paddingLeftRight from the right edge.
        Rectangle textBounds = new(
            rect.Right - paddingLeftRight - textSize.Width,
            rect.Y + PaddingTopBottom - 1,
            textSize.Width,
            textSize.Height);

        Color textColor = fill ? headColor : ColorHelper.Lerp(headColor, Color.Black, 0.25F);
        TextRenderer.DrawText(graphics, name, font, textBounds, textColor, TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

        if (!highlight)
        {
            remotePath.Dispose();
            return (rect, DrawHighlight: null);
        }

        return (rect, DrawHighlight: () =>
            {
                using (remotePath)
                {
                    SmoothingMode oldMode = graphics.SmoothingMode;
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    try
                    {
                        using Pen highlightPen = new(headColor, RefLabelHighlightWidth);
                        graphics.DrawPath(highlightPen, remotePath);
                    }
                    finally
                    {
                        graphics.SmoothingMode = oldMode;
                    }
                }
            });
    }

    /// <summary>
    ///  Draws a ref label and returns the painted rectangle in the same coordinate space as <paramref name="bounds"/>.
    /// </summary>
    /// <returns>The bounding rectangle of the drawn ref label in DataGridView client coordinates, or <see cref="Rectangle.Empty"/> if nothing was drawn.</returns>
    public static Rectangle DrawRef(bool isRowSelected, Font font, ref int offset, string name, in Color headColor, RefArrowType arrowType, in Rectangle bounds, Graphics graphics, bool dashedLine = false, bool fill = false, bool highlight = false, bool nestledRight = false)
    {
        int paddingLeftRight = PaddingLeftRight(name);
        int paddingTopBottom = PaddingTopBottom;
        int marginRight = DpiUtil.Scale(5);

        Color textColor = fill ? headColor : ColorHelper.Lerp(headColor, Color.Black, 0.25F);

        Size textSize = !string.IsNullOrEmpty(name)
            ? TextRenderer.MeasureText(graphics, name, font, Size.Empty, TextFormatFlags.NoPadding)
            : new(0, TextRenderer.MeasureText(graphics, " ", font, Size.Empty, TextFormatFlags.NoPadding).Height);

        int arrowWidth = arrowType == RefArrowType.None ? 0 : bounds.Height / 2;

        int backgroundHeight = textSize.Height + paddingTopBottom + paddingTopBottom - 1;
        int outerMarginTopBottom = (bounds.Height - backgroundHeight) / 2;

        Rectangle rect = new(
            bounds.X + offset,
            bounds.Y + outerMarginTopBottom,
            Math.Min(bounds.Width - offset, textSize.Width + arrowWidth + paddingLeftRight + paddingLeftRight - 1),
            backgroundHeight);
        if (rect.Width == 0 || rect.Height == 0)
        {
            // it may happen, as observe in #5396
            return Rectangle.Empty;
        }

        int scaledRadius = RefLabelCornerRadius;
        if (nestledRight)
        {
            int chevronWidth = ChevronWidth(backgroundHeight);
            rect = rect with { Width = rect.Width + (chevronWidth / 2) };
            using GraphicsPath branchPath = CreateChevronRightRoundRectPath(rect, scaledRadius, chevronWidth);
            DrawRefBackground(isRowSelected, graphics, headColor, rect, branchPath, arrowType, dashedLine, fill, highlight);
        }
        else
        {
            using GraphicsPath roundPath = CreateRoundRectPath(rect, scaledRadius);
            DrawRefBackground(isRowSelected, graphics, headColor, rect, roundPath, arrowType, dashedLine, fill, highlight);
        }

        Rectangle textBounds = new(
            rect.X + arrowWidth + paddingLeftRight,
            rect.Y + paddingTopBottom - 1,
            Math.Min(bounds.Width - offset - paddingLeftRight - paddingLeftRight, textSize.Width),
            textSize.Height);

        TextRenderer.DrawText(graphics, name, font, textBounds, textColor, TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

        offset += rect.Width + marginRight;
        return rect;
    }

    private static void DrawRefBackground(bool isRowSelected, Graphics graphics, in Color color, in Rectangle bounds, GraphicsPath path, RefArrowType arrowType, bool dashedLine, bool fill, bool highlight)
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
            if (arrowType != RefArrowType.None)
            {
                DrawArrow(graphics, bounds.X, bounds.Y, bounds.Height, color, filled: arrowType == RefArrowType.Filled);
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

    private static void DrawArrow(Graphics graphics, float x, float y, float rowHeight, in Color color, bool filled)
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

    /// <summary>
    ///  Computes the ideal rendered size of a ref label without drawing it.
    /// </summary>
    /// <remarks>
    ///  Returns the capsule's ideal width and height given the same inputs as <see cref="DrawRef"/>.
    ///  Does not account for clipping to available cell width.
    /// </remarks>
    public static (int idealWidth, int backgroundHeight) MeasureRef(Font font, string name, RefArrowType arrowType, int rowHeight, Graphics graphics)
    {
        Size textSize = !string.IsNullOrEmpty(name)
            ? TextRenderer.MeasureText(graphics, name, font, Size.Empty, TextFormatFlags.NoPadding)
            : new(0, TextRenderer.MeasureText(graphics, " ", font, Size.Empty, TextFormatFlags.NoPadding).Height);

        int backgroundHeight = textSize.Height + (PaddingTopBottom * 2) - 1;
        int arrowWidth = arrowType == RefArrowType.None ? 0 : rowHeight / 2;
        int idealWidth = textSize.Width + arrowWidth + (PaddingLeftRight(name) * 2) - 1;

        return (idealWidth, backgroundHeight);
    }
}
