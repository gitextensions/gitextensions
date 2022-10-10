using System.Diagnostics.Contracts;
using System.Drawing.Drawing2D;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;

namespace GitUI
{
    internal static class RevisionGridRefRenderer
    {
        private static readonly float[] _dashPattern = { 4, 4 };
        private static readonly PointF[] _arrowPoints = new PointF[4];

        public static void DrawRef(bool isRowSelected, Font font, ref int offset, string name, Color headColor, RefArrowType arrowType, in Rectangle bounds, Graphics graphics, bool dashedLine = false, bool fill = false)
        {
            var paddingLeftRight = !string.IsNullOrEmpty(name) ? DpiUtil.Scale(4) : DpiUtil.Scale(1);
            var paddingTopBottom = DpiUtil.Scale(2);
            var marginRight = DpiUtil.Scale(7);

            var textColor = fill ? headColor : ColorHelper.Lerp(headColor, Color.Black, 0.25F);

            Size textSize = !string.IsNullOrEmpty(name)
                ? TextRenderer.MeasureText(graphics, name, font, Size.Empty, TextFormatFlags.NoPadding)
                : new(0, TextRenderer.MeasureText(graphics, " ", font, Size.Empty, TextFormatFlags.NoPadding).Height);

            var arrowWidth = arrowType == RefArrowType.None ? 0 : bounds.Height / 2;

            var backgroundHeight = textSize.Height + paddingTopBottom + paddingTopBottom - 1;
            var outerMarginTopBottom = (bounds.Height - backgroundHeight) / 2;

            Rectangle rect = new(
                bounds.X + offset,
                bounds.Y + outerMarginTopBottom,
                Math.Min(bounds.Width - offset, textSize.Width + arrowWidth + paddingLeftRight + paddingLeftRight - 1),
                backgroundHeight);
            if (rect.Width == 0 || rect.Height == 0)
            {
                // it may happen, as observe in #5396
                return;
            }

            DrawRefBackground(
                isRowSelected,
                graphics,
                headColor,
                rect,
                radius: 3, arrowType, dashedLine, fill);

            Rectangle textBounds = new(
                rect.X + arrowWidth + paddingLeftRight,
                rect.Y + paddingTopBottom - 1,
                Math.Min(bounds.Width - offset - paddingLeftRight - paddingLeftRight, textSize.Width),
                textSize.Height);

            TextRenderer.DrawText(graphics, name, font, textBounds, textColor, TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            offset += rect.Width + marginRight;
        }

        private static void DrawRefBackground(bool isRowSelected, Graphics graphics, Color color, Rectangle bounds, int radius, RefArrowType arrowType, bool dashedLine, bool fill)
        {
            var oldMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            try
            {
                using var path = CreateRoundRectPath(bounds, radius);
                if (fill)
                {
                    var color1 = ColorHelper.Lerp(color, SystemColors.Window, 0.92F);
                    var color2 = ColorHelper.Lerp(color1, SystemColors.Window, 0.9f);
                    using var brush = new LinearGradientBrush(bounds, color1, color2, angle: 90);
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

        [Pure]
        public static Rectangle ReduceLeft(this Rectangle rect, int offset)
        {
            return new Rectangle(
                rect.Left + offset,
                rect.Top,
                rect.Width - offset,
                rect.Height);
        }

        [Pure]
        public static Rectangle ReduceRight(this Rectangle rect, int offset)
        {
            return new Rectangle(
                rect.Left,
                rect.Top,
                rect.Width - offset,
                rect.Height);
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

            const float horShift = 4;
            const float verShift = 3;

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

        internal static GraphicsPath CreateRoundRectPath(Rectangle rect, int radius)
        {
            var left = rect.X;
            var top = rect.Y;
            var right = left + rect.Width;
            var bottom = top + rect.Height;

            GraphicsPath path = new();
            path.AddArc(left, top, radius, radius, startAngle: 180, sweepAngle: 90);
            path.AddArc(right - radius, top, radius, radius, 270, 90);
            path.AddArc(right - radius, bottom - radius, radius, radius, 0, 90);
            path.AddArc(left, bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
