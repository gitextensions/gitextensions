using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GitExtUtils.GitUI;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Color = System.Drawing.Color;

namespace GitUI
{
    internal static class RevisionGridRefRenderer
    {
        private static readonly float[] _dashPattern = { 4, 4 };
        private static readonly PointF[] _arrowPoints = new PointF[4];

        public static void DrawRef(bool isRowSelected, Font font, ref int offset, string name, Color headColor, RefArrowType arrowType, in Rectangle bounds, Graphics graphics, bool dashedLine = false)
        {
            var paddingLeftRight = DpiUtil.Scale(4);
            var paddingTopBottom = DpiUtil.Scale(2);
            var marginRight = DpiUtil.Scale(7);

            var textColor = ColorHelper.Lerp(headColor, Color.Black, 0.25F);

            var textSize = TextRenderer.MeasureText(graphics, name, font, Size.Empty, TextFormatFlags.NoPadding);

            var arrowWidth = arrowType == RefArrowType.None ? 0 : bounds.Height / 2;

            var backgroundHeight = textSize.Height + paddingTopBottom + paddingTopBottom - 1;
            var outerMarginTopBottom = (bounds.Height - backgroundHeight) / 2;

            var rect = new Rectangle(
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
                radius: 3, arrowType, dashedLine);

            var textBounds = new Rectangle(
                rect.X + arrowWidth + paddingLeftRight,
                rect.Y + paddingTopBottom - 1,
                Math.Min(bounds.Width - offset - paddingLeftRight - paddingLeftRight, textSize.Width),
                textSize.Height);

            TextRenderer.DrawText(graphics, name, font, textBounds, textColor, TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            offset += rect.Width + marginRight;
        }

        private static void DrawRefBackground(bool isRowSelected, Graphics graphics, Color color, Rectangle bounds, int radius, RefArrowType arrowType, bool dashedLine)
        {
            var oldMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            try
            {
                using var path = CreateRoundRectPath(bounds, radius);

                if (isRowSelected)
                {
                    graphics.FillPath(SystemBrushes.Window, path);
                }

                // frame
                using var pen = new Pen(ColorHelper.Lerp(color, SystemColors.Window, 0.5F));
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
            ////if (gitRef.IsTag)
            ////{
            ////    return AppColor.Tag.GetThemeColor();
            ////}

            ////if (gitRef.IsHead)
            ////{
            ////    return AppColor.Branch.GetThemeColor();
            ////}

            ////if (gitRef.IsRemote)
            ////{
            ////    return AppColor.RemoteBranch.GetThemeColor();
            ////}

            ////return AppColor.OtherTag.GetThemeColor();

            return SystemColors.ControlText;
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
                using var brush = new SolidBrush(color);
                graphics.FillPolygon(brush, _arrowPoints);
            }
            else
            {
                using var pen = new Pen(color);
                graphics.DrawPolygon(pen, _arrowPoints);
            }
        }

        internal static GraphicsPath CreateRoundRectPath(Rectangle rect, int radius)
        {
            var left = rect.X;
            var top = rect.Y;
            var right = left + rect.Width;
            var bottom = top + rect.Height;

            var path = new GraphicsPath();
            path.AddArc(left, top, radius, radius, startAngle: 180, sweepAngle: 90);
            path.AddArc(right - radius, top, radius, radius, 270, 90);
            path.AddArc(right - radius, bottom - radius, radius, radius, 0, 90);
            path.AddArc(left, bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
