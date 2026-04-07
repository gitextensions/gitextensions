using System.Drawing.Drawing2D;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid;

namespace GitUI;

internal static class RevisionGridRefRenderer
{
    private static readonly float[] _dashPattern = [4, 4];
    private static readonly PointF[] _arrowPoints = new PointF[4];

    /// <summary>
    ///  Draws a ref label and returns the painted rectangle in the same coordinate space as <paramref name="bounds"/>.
    /// </summary>
    /// <returns>The bounding rectangle of the drawn ref label in DataGridView client coordinates, or <see cref="Rectangle.Empty"/> if nothing was drawn.</returns>
    public static Rectangle DrawRef(bool isRowSelected, Font font, ref int offset, string name, Color headColor, RefLabelIcon icon, in Rectangle bounds, Graphics graphics, bool dashedLine = false, bool fill = false, bool highlight = false)
    {
        // Copy ref/in parameters to locals so they can be captured by local functions.
        Rectangle boundsLocal = bounds;
        int offsetLocal = offset;

        int paddingLeftRight = !string.IsNullOrEmpty(name) ? DpiUtil.Scale(6) : DpiUtil.Scale(3);
        int paddingTopBottom = DpiUtil.Scale(2);
        int marginRight = DpiUtil.Scale(5);

        // Solid-fill capsules: choose contrasting text colour for readability.
        // Use 0.40 as the luminance threshold — this ensures green hues (which are perceptually
        // bright despite having luminance ~0.45) get dark text rather than white.
        float luminanceForText = ((0.299f * headColor.R) + (0.587f * headColor.G) + (0.114f * headColor.B)) / 255f;
        Color textColor = luminanceForText > 0.40f ? Color.FromArgb(30, 30, 30) : Color.White;

        Size textSize = !string.IsNullOrEmpty(name)
            ? TextRenderer.MeasureText(graphics, name, font, Size.Empty, TextFormatFlags.NoPadding)
            : new(width: 0, height: TextRenderer.MeasureText(graphics, " ", font, Size.Empty, TextFormatFlags.NoPadding).Height);

        int backgroundHeight = textSize.Height + (paddingTopBottom * 2) - 1;
        int outerMarginTopBottom = (boundsLocal.Height - backgroundHeight) / 2;

        // Tags use a custom tag-shaped background rather than a capsule with an icon inside.
        if (icon == RefLabelIcon.Tag)
        {
            Rectangle tagResult = DrawTagRef();
            offset = offsetLocal;
            return tagResult;
        }

        bool hasArrow = icon is RefLabelIcon.ArrowFilled or RefLabelIcon.ArrowNotFilled;
        bool hasIcon = icon is RefLabelIcon.Head or RefLabelIcon.Branch or RefLabelIcon.Remote or RefLabelIcon.Stash;

        // | left padding | icon (optional) | gap (if icon and text) | text | arrow (optional) | right padding |
        //                |__|    icon   |__|
        //                  \______________\___ padding

        int arrowWidth = hasArrow ? boundsLocal.Height / 2 : 0;
        int iconSize = hasIcon ? textSize.Height - (paddingTopBottom * 2) : 0;
        int iconGap = 0;

        // When there is no text and only an icon (e.g. a condensed remote cloud label), force the
        // rect to be square so the fully-rounded capsule path renders as a true circle.
        int naturalWidth = textSize.Width + arrowWidth + iconSize + iconGap + paddingLeftRight + paddingLeftRight - 1;
        int idealWidth = string.IsNullOrEmpty(name) && hasIcon && !hasArrow ? backgroundHeight : naturalWidth;

        Rectangle rect = new(
            boundsLocal.X + offsetLocal,
            boundsLocal.Y + outerMarginTopBottom,
            width: Math.Min(boundsLocal.Width - offsetLocal, idealWidth),
            backgroundHeight);

        if (rect.Width <= 0 || rect.Height <= 0)
        {
            // it may happen, as observe in #5396
            return Rectangle.Empty;
        }

        DrawRefBackground(isRowSelected, headColor, rect, icon, dashedLine, fill, highlight);

        if (hasIcon)
        {
            // When there is no text, centre the icon in the (circular) rect.
            // When text follows, left-align the icon just after the left padding.
            float iconX = string.IsNullOrEmpty(name)
                ? rect.X + ((rect.Width - iconSize) / 2f)
                : rect.X + (paddingLeftRight / 2f) + 1;
            float iconY = rect.Y + ((rect.Height - iconSize) / 2f);

            DrawRefIcon(iconX, iconY);
        }

        Rectangle textBounds = new(
            rect.X + arrowWidth + paddingLeftRight + iconSize + iconGap,
            rect.Y + paddingTopBottom - 1,
            Math.Min(boundsLocal.Width - offsetLocal - paddingLeftRight - paddingLeftRight, textSize.Width),
            textSize.Height);

        TextRenderer.DrawText(graphics, name, font, textBounds, textColor, TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

        offset += rect.Width + marginRight;
        return rect;

        void FillFrameAndHighlight(GraphicsPath path, Rectangle bounds, Color color, bool dashedLine, bool fill, bool isRowSelected, bool highlight)
        {
            using SolidBrush brush = new(color);
            graphics.FillPath(brush, path);

            bool isDark = SystemColors.Window.GetBrightness() < 0.5f;

            if (dashedLine)
            {
                // Dashed border indicates a superproject ref — always visible so the dashes read.
                Color dashColor = isDark
                    ? ColorHelper.Lerp(color, Color.White, 0.5f)
                    : ColorHelper.Lerp(color, Color.Black, 0.5f);
                using Pen dashPen = new(dashColor);
                dashPen.DashPattern = _dashPattern;
                graphics.DrawPath(dashPen, path);
            }

            if (highlight)
            {
                // On hover: border becomes lighter (dark theme) or darker (light theme).
                Color borderColor = isDark
                    ? ColorHelper.Lerp(color, Color.White, 0.6f)
                    : ColorHelper.Lerp(color, Color.Black, 0.4f);
                using Pen highlightPen = new(borderColor, 2f);
                graphics.DrawPath(highlightPen, path);
            }
        }

        Rectangle DrawTagRef()
        {
            // The tag shape has a pointed notch on the left and a flat right edge,
            // with slightly rounded corners (chamfered notch corners, small arcs on the right).
            // Use tighter padding than the standard capsule: less height and less right padding.
            int tagPaddingTopBottom = Math.Max(1, paddingTopBottom - 1);
            int tagPaddingRight = DpiUtil.Scale(2);
            int tagHeight = textSize.Height + (tagPaddingTopBottom * 2) - 1;
            int tagOuterMargin = (boundsLocal.Height - tagHeight) / 2;
            int notchWidth = tagHeight / 2;

            Rectangle tagRect = new(
                boundsLocal.X + offsetLocal,
                boundsLocal.Y + tagOuterMargin,
                width: Math.Min(boundsLocal.Width - offsetLocal, textSize.Width + notchWidth + paddingLeftRight + tagPaddingRight - 1),
                tagHeight);

            if (tagRect.Width == 0 || tagRect.Height == 0)
            {
                return Rectangle.Empty;
            }

            float l = tagRect.X;
            float t = tagRect.Y;
            float r = tagRect.Right;
            float b = tagRect.Bottom;
            float midY = t + (tagRect.Height / 2f);
            float notchX = l + notchWidth;

            // Small corner radius for the right corners; chamfer size for the 45° notch corners.
            float cr = Math.Max(2f, backgroundHeight * 0.12f);
            float chamfer = cr * 0.707f;

            SmoothingMode oldMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            try
            {
                using GraphicsPath path = new();
                path.AddLine(l, midY, notchX - chamfer, t + chamfer);              // tip → near top-notch corner
                path.AddLine(notchX - chamfer, t + chamfer, notchX + chamfer, t);  // chamfer the top-notch corner
                path.AddLine(notchX + chamfer, t, r - cr, t);                      // top edge
                path.AddArc(r - (2 * cr), t, 2 * cr, 2 * cr, 270, 90);            // top-right rounded corner
                path.AddArc(r - (2 * cr), b - (2 * cr), 2 * cr, 2 * cr, 0, 90);   // bottom-right rounded corner
                path.AddLine(r - cr, b, notchX + chamfer, b);                      // bottom edge
                path.AddLine(notchX + chamfer, b, notchX - chamfer, b - chamfer);  // chamfer the bottom-notch corner
                path.CloseFigure();                                                 // back to tip

                FillFrameAndHighlight(path, tagRect, headColor, dashedLine, fill, isRowSelected, highlight);

                // Cut-out dot in the notch: filled with the row background so it appears as a hole.
                // Positioned towards the text side of the notch so the tip has breathing room.
                float dotDiameter = Math.Max(3f, tagRect.Height * 0.26f);
                float dotX = l + (notchWidth * 0.72f) - (dotDiameter / 2f);
                Color dotFill = isRowSelected ? SystemColors.Highlight : SystemColors.Window;
                using SolidBrush dotBrush = new(dotFill);
                graphics.FillEllipse(dotBrush, dotX, midY - (dotDiameter / 2f), dotDiameter, dotDiameter);
            }
            finally
            {
                graphics.SmoothingMode = oldMode;
            }

            Rectangle textBounds = new(
                tagRect.X + notchWidth + DpiUtil.Scale(4),
                tagRect.Y + tagPaddingTopBottom - 1,
                Math.Min(boundsLocal.Width - offsetLocal - notchWidth - tagPaddingRight, textSize.Width),
                textSize.Height);

            TextRenderer.DrawText(graphics, name, font, textBounds, textColor, TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            offsetLocal += tagRect.Width + marginRight;
            return tagRect;
        }

        void DrawRefBackground(bool isRowSelected, Color color, Rectangle bounds, RefLabelIcon icon, bool dashedLine, bool fill, bool highlight)
        {
            SmoothingMode oldMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            try
            {
                using GraphicsPath path = CreateRoundRectPath();

                FillFrameAndHighlight(path, bounds, color, dashedLine, fill, isRowSelected, highlight);

                // arrow for superproject ref indicators
                if (icon is RefLabelIcon.ArrowFilled or RefLabelIcon.ArrowNotFilled)
                {
                    DrawArrow(graphics, bounds.X, bounds.Y, bounds.Height, color, filled: icon == RefLabelIcon.ArrowFilled);
                }
            }
            finally
            {
                graphics.SmoothingMode = oldMode;
            }

            return;

            GraphicsPath CreateRoundRectPath()
            {
                int left = bounds.X;
                int top = bounds.Y;
                int right = left + bounds.Width;
                int bottom = top + bounds.Height;
                int diameter = Math.Min(bounds.Width, bounds.Height);

                GraphicsPath path = new();
                path.AddArc(left, top, diameter, diameter, startAngle: 180, sweepAngle: 90);
                path.AddArc(right - diameter, top, diameter, diameter, 270, 90);
                path.AddArc(right - diameter, bottom - diameter, diameter, diameter, 0, 90);
                path.AddArc(left, bottom - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();

                return path;
            }
        }

        void DrawRefIcon(float x, float y)
        {
            SmoothingMode oldMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.HighQuality;

            try
            {
                switch (icon)
                {
                    case RefLabelIcon.Head:
                        DrawHeadIcon();
                        break;
                    case RefLabelIcon.Branch:
                        DrawBranchIcon();
                        break;
                    case RefLabelIcon.Remote:
                        DrawRemoteIcon();
                        break;
                    case RefLabelIcon.Stash:
                        DrawStashIcon();
                        break;
                }
            }
            finally
            {
                graphics.SmoothingMode = oldMode;
            }

            return;

            void DrawHeadIcon()
            {
                float penWidth = Math.Max(1f, iconSize * 0.08f);
                using Pen pen = new(textColor, penWidth);
                using SolidBrush brush = new(textColor);

                float cx = x + (iconSize / 2f);
                float cy = y + (iconSize / 2f);
                float outerDiameter1 = iconSize * 1f;
                float outerDiameter2 = iconSize * 0.5f;
                float dotDiameter = Math.Max(2f, iconSize * 0.15f);

                graphics.DrawEllipse(pen, cx - (outerDiameter1 / 2f), cy - (outerDiameter1 / 2f), outerDiameter1, outerDiameter1);
                graphics.DrawEllipse(pen, cx - (outerDiameter2 / 2f), cy - (outerDiameter2 / 2f), outerDiameter2, outerDiameter2);
                graphics.FillEllipse(brush, cx - (dotDiameter / 2f), cy - (dotDiameter / 2f), dotDiameter, dotDiameter);
            }

            void DrawBranchIcon()
            {
                float penWidth = Math.Max(1f, iconSize * 0.08f);
                using Pen pen = new(textColor, penWidth);

                float dotRadius = iconSize * 0.1f;
                float dotDiameter = dotRadius * 2;

                float mainX = MathF.Round(x + (iconSize * 0.35f));
                float topY = y + (iconSize * 0.15f);
                float bottomY = y + (iconSize * 0.85f);
                float branchStartY = y + (iconSize * 0.7f);
                float branchEndX = x + (iconSize * 0.75f);
                float branchEndY = y + (iconSize * 0.4f);
                float branchMidY = branchStartY + ((branchEndY - branchStartY) / 2f);

                graphics.DrawLine(pen, mainX, topY + dotRadius, mainX, bottomY - dotRadius);

                graphics.DrawBezier(
                    pen,
                    mainX, branchStartY,
                    mainX, branchMidY,
                    branchEndX, branchMidY,
                    branchEndX, branchEndY + dotRadius);

                graphics.DrawEllipse(pen, mainX - dotRadius, topY - dotRadius, dotDiameter, dotDiameter);
                graphics.DrawEllipse(pen, mainX - dotRadius, bottomY - dotRadius, dotDiameter, dotDiameter);
                graphics.DrawEllipse(pen, branchEndX - dotRadius, branchEndY - dotRadius, dotDiameter, dotDiameter);
            }

            void DrawRemoteIcon()
            {
                DrawCloudIcon(graphics, textColor, x, y, iconSize);
            }

            void DrawStashIcon()
            {
                float penWidth = Math.Max(1f, iconSize * 0.08f);
                using Pen pen = new(textColor, penWidth);

                float padding = iconSize * 0.15f;
                float left = x + padding;
                float right = x + iconSize - padding;
                float top = y + padding;
                float bottom = y + iconSize - padding;
                float width = right - left;
                float height = bottom - top;

                // Drawer front face
                graphics.DrawRectangle(pen, left, top + (height * 0.3f), width, height * 0.7f);

                // Lid/top edge
                graphics.DrawLine(pen, left, top + (height * 0.3f), left + (width * 0.5f), top);
                graphics.DrawLine(pen, left + (width * 0.5f), top, right, top + (height * 0.3f));
            }
        }
    }

    /// <summary>
    /// Computes the ideal rendered size of a ref label without drawing it.
    /// Returns the capsule's ideal width and height given the same inputs as <see cref="DrawRef"/>.
    /// Does not account for clipping to available cell width.
    /// </summary>
    public static (int idealWidth, int backgroundHeight) MeasureRef(Font font, string name, RefLabelIcon icon, Graphics graphics)
    {
        int paddingLeftRight = !string.IsNullOrEmpty(name) ? DpiUtil.Scale(6) : DpiUtil.Scale(3);
        int paddingTopBottom = DpiUtil.Scale(2);

        int textHeight = TextRenderer.MeasureText(graphics, " ", font, Size.Empty, TextFormatFlags.NoPadding).Height;
        int textWidth = !string.IsNullOrEmpty(name)
            ? TextRenderer.MeasureText(graphics, name, font, Size.Empty, TextFormatFlags.NoPadding).Width
            : 0;

        int backgroundHeight = textHeight + (paddingTopBottom * 2) - 1;
        bool hasIcon = icon is RefLabelIcon.Head or RefLabelIcon.Branch or RefLabelIcon.Remote or RefLabelIcon.Stash;
        int iconSize = hasIcon ? textHeight - (paddingTopBottom * 2) : 0;

        int naturalWidth = textWidth + iconSize + (paddingLeftRight * 2) - 1;
        int idealWidth = string.IsNullOrEmpty(name) && hasIcon ? backgroundHeight : naturalWidth;

        return (idealWidth, backgroundHeight);
    }

    /// <summary>
    /// Draws a remote ref label as a D-shape (neck rectangle + right semicircle) nestled
    /// against the right edge of a preceding branch capsule.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The D-shape's left neck aligns with the branch's straight top/bottom edges, so the
    /// combined outline of branch + remote looks like a single capsule. The branch is drawn
    /// on top afterwards — its right rounded cap provides the curved visual boundary between
    /// the two colours.
    /// </para>
    /// <para>
    /// Geometry: given <paramref name="precedingRight"/> as the absolute x of the previous
    /// shape's right edge and <c>R = backgroundHeight / 2</c>:
    /// <list type="bullet">
    ///   <item>Neck left edge: precedingRight − R</item>
    ///   <item>Semicircle centre: precedingRight + R</item>
    ///   <item>Shape right edge: precedingRight + 2R (= precedingRight + backgroundHeight)</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="precedingRight">Absolute x-coordinate of the right edge of the preceding capsule.</param>
    /// <param name="capsuleTop">Absolute y-coordinate of the capsule top edge.</param>
    /// <param name="backgroundHeight">Capsule height (must equal the branch capsule's height).</param>
    /// <returns>Full D-shape bounding rectangle (left half will be covered by the branch drawn on top).</returns>
    public static Rectangle DrawNestledRemoteRef(
        bool isRowSelected,
        Color headColor,
        int precedingRight,
        int capsuleTop,
        int backgroundHeight,
        Graphics graphics,
        bool highlight)
    {
        int diameter = backgroundHeight;
        int radius = diameter / 2;

        // D-shape bounding box.
        int shapeLeft = precedingRight - radius;
        int shapeTop = capsuleTop;
        int shapeBottom = capsuleTop + backgroundHeight;
        float midY = shapeTop + radius;

        // Total width = neck(radius) + semicircle(diameter) = 3*radius.
        Rectangle shapeBounds = new(shapeLeft, shapeTop, 3 * radius, backgroundHeight);

        if (shapeBounds.Width <= 0 || shapeBounds.Height <= 0)
        {
            return Rectangle.Empty;
        }

        SmoothingMode oldMode = graphics.SmoothingMode;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        try
        {
            // Fill path: full closed D-shape (neck rect + right semicircle).
            // Top edge ends at the semicircle centre; AddArc begins there.
            using GraphicsPath fillPath = new();
            fillPath.AddLine(shapeLeft, shapeTop, shapeLeft + (2 * radius), shapeTop);
            fillPath.AddArc(precedingRight, shapeTop, diameter, diameter, -90, 180);
            fillPath.AddLine(shapeLeft + (2 * radius), shapeBottom, shapeLeft, shapeBottom);
            fillPath.CloseFigure();

            using SolidBrush fillBrush = new(headColor);
            graphics.FillPath(fillBrush, fillPath);

            // Stroke path: top edge + right arc + bottom edge only (no left vertical edge —
            // it is hidden under the branch capsule drawn on top).
            using GraphicsPath strokePath = new();
            strokePath.AddLine(shapeLeft, shapeTop, shapeLeft + (2 * radius), shapeTop);
            strokePath.AddArc(precedingRight, shapeTop, diameter, diameter, -90, 180);
            strokePath.AddLine(shapeLeft + (2 * radius), shapeBottom, shapeLeft, shapeBottom);

            if (highlight)
            {
                bool isDark = SystemColors.Window.GetBrightness() < 0.5f;
                Color borderColor = isDark
                    ? ColorHelper.Lerp(headColor, Color.White, 0.6f)
                    : ColorHelper.Lerp(headColor, Color.Black, 0.4f);
                using Pen highlightPen = new(borderColor, 2f);
                graphics.DrawPath(highlightPen, strokePath);
            }
        }
        finally
        {
            graphics.SmoothingMode = oldMode;
        }

        // Cloud icon centred in the semicircle, using a contrasting colour for readability.
        float luminance = ((0.299f * headColor.R) + (0.587f * headColor.G) + (0.114f * headColor.B)) / 255f;
        Color iconColor = luminance > 0.40f ? Color.FromArgb(30, 30, 30) : Color.White;

        int paddingTopBottom = DpiUtil.Scale(2);
        int iconSize = Math.Max(4, backgroundHeight - (paddingTopBottom * 4) + 1);
        float iconX = (precedingRight + radius) - (iconSize / 2f);
        float iconY = midY - (iconSize / 2f);

        SmoothingMode iconOldMode = graphics.SmoothingMode;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        try
        {
            DrawCloudIcon(graphics, iconColor, iconX, iconY, iconSize);
        }
        finally
        {
            graphics.SmoothingMode = iconOldMode;
        }

        return shapeBounds;
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

        if (gitRef.IsStash)
        {
            return AppColor.Stash.GetThemeColor();
        }

        return AppColor.OtherTag.GetThemeColor();
    }

    private static void DrawCloudIcon(Graphics graphics, Color color, float x, float y, float iconSize)
    {
        float penWidth = Math.Max(1f, iconSize * 0.08f);
        using Pen pen = new(color, penWidth) { LineJoin = LineJoin.Round };
        using GraphicsPath path = new();

        float s = iconSize;

        // Bezier curves traced from a cloud SVG (72×72 viewBox), normalized to 0–1.
        // Three bumps: left (small), middle (medium), right (large).

        // Left bump, descending left side
        path.AddBezier(
            x + (0.221f * s), y + (0.420f * s),
            x + (0.139f * s), y + (0.439f * s),
            x + (0.083f * s), y + (0.510f * s),
            x + (0.083f * s), y + (0.596f * s));

        // Bottom-left rounded corner
        path.AddBezier(
            x + (0.083f * s), y + (0.596f * s),
            x + (0.083f * s), y + (0.687f * s),
            x + (0.146f * s), y + (0.760f * s),
            x + (0.224f * s), y + (0.760f * s));

        // Flat bottom
        path.AddLine(
            x + (0.224f * s), y + (0.760f * s),
            x + (0.762f * s), y + (0.760f * s));

        // Bottom-right rounded corner
        path.AddBezier(
            x + (0.762f * s), y + (0.760f * s),
            x + (0.847f * s), y + (0.760f * s),
            x + (0.917f * s), y + (0.682f * s),
            x + (0.917f * s), y + (0.586f * s));

        // Right bump, ascending right side
        path.AddBezier(
            x + (0.917f * s), y + (0.586f * s),
            x + (0.917f * s), y + (0.494f * s),
            x + (0.853f * s), y + (0.419f * s),
            x + (0.773f * s), y + (0.413f * s));

        // Right bump top arc
        path.AddBezier(
            x + (0.773f * s), y + (0.413f * s),
            x + (0.743f * s), y + (0.309f * s),
            x + (0.660f * s), y + (0.240f * s),
            x + (0.561f * s), y + (0.240f * s));

        // Right bump descending to middle valley
        path.AddBezier(
            x + (0.561f * s), y + (0.240f * s),
            x + (0.498f * s), y + (0.240f * s),
            x + (0.441f * s), y + (0.269f * s),
            x + (0.404f * s), y + (0.315f * s));

        // Middle valley across to left bump top
        path.AddLine(
            x + (0.404f * s), y + (0.315f * s),
            x + (0.343f * s), y + (0.310f * s));

        // Left bump top arc, back to start
        path.AddBezier(
            x + (0.343f * s), y + (0.310f * s),
            x + (0.280f * s), y + (0.310f * s),
            x + (0.228f * s), y + (0.358f * s),
            x + (0.221f * s), y + (0.420f * s));

        graphics.DrawPath(pen, path);
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
}
