using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GitCommands;
using GitUI.UserControls;
using GitUIPluginInterfaces;

namespace GitUI
{
    internal static class RevisionGridRefRenderer
    {
        private static readonly float[] dashPattern = { 4, 4 };
        private static readonly PointF[] _arrowPoints = new PointF[4];

        public static void DrawRef(DataGridViewCellPaintingEventArgs e, bool isRowSelected, Font font, ref float offset, string name, Color headColor, RevisionGrid.ArrowType arrowType, bool dashedLine = false, bool fill = false)
        {
            var textColor = fill ? headColor : Lerp(headColor, Color.White, 0.5f);

            var headBounds = AdjustCellBounds(e.CellBounds, offset);
            var textSize = e.Graphics.MeasureString(name, font);

            offset += textSize.Width;
            offset += 9;

            var extraOffset = DrawRefBackground(
                isRowSelected,
                e.Graphics,
                headColor,
                headBounds.X,
                headBounds.Y,
                RoundToEven(textSize.Width),
                RoundToEven(textSize.Height),
                radius: 3, arrowType, dashedLine, fill);

            offset += extraOffset;
            headBounds.Offset((int)(extraOffset + 1), 0);

            RevisionGridUtils.DrawColumnTextTruncated(e.Graphics, name, font, textColor, headBounds);
        }

        private static float RoundToEven(float value)
        {
            var result = (int)value;
            result <<= 1;
            result >>= 1;
            return result < value
                ? result + 2
                : result;
        }

        private static float DrawRefBackground(bool isSelected, Graphics graphics, Color color, float x, float y, float width, float height, float radius, RevisionGrid.ArrowType arrowType, bool dashedLine, bool fill)
        {
            float additionalOffset = arrowType != RevisionGrid.ArrowType.None ? height - 6 : 0;
            width += additionalOffset;
            var oldMode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            try
            {
                // shade
                if (fill)
                {
                    using (var shadePath = CreateRoundRectPath(x + 1, y + 1, width, height, radius))
                    {
                        var shadeBrush = isSelected ? Brushes.Black : Brushes.Gray;
                        graphics.FillPath(shadeBrush, shadePath);
                    }
                }

                using (var forePath = CreateRoundRectPath(x, y, width, height, radius))
                {
                    Color fillColor = Lerp(color, Color.White, 0.92F);

                    if (fill)
                    {
                        using (var fillBrush = new LinearGradientBrush(new RectangleF(x, y, width, height), fillColor, Lerp(fillColor, Color.White, 0.9F), 90))
                        {
                            graphics.FillPath(fillBrush, forePath);
                        }
                    }
                    else if (isSelected)
                    {
                        graphics.FillPath(Brushes.White, forePath);
                    }

                    // frame
                    using (var pen = new Pen(Lerp(color, Color.White, 0.83F)))
                    {
                        if (dashedLine)
                        {
                            pen.DashPattern = dashPattern;
                        }

                        graphics.DrawPath(pen, forePath);
                    }

                    // arrow if the head is the current branch
                    if (arrowType != RevisionGrid.ArrowType.None)
                    {
                        DrawArrow(graphics, x, y, height, color, arrowType == RevisionGrid.ArrowType.Filled);
                    }
                }
            }
            finally
            {
                graphics.SmoothingMode = oldMode;
            }

            return additionalOffset;
        }

        public static Rectangle AdjustCellBounds(Rectangle cellBounds, float offset)
        {
            return new Rectangle((int)(cellBounds.Left + offset), cellBounds.Top + 4,
                cellBounds.Width - (int)offset, cellBounds.Height);
        }

        public static Color GetHeadColor(IGitRef gitRef)
        {
            if (gitRef.IsTag)
            {
                return AppSettings.TagColor;
            }

            if (gitRef.IsHead)
            {
                return AppSettings.BranchColor;
            }

            if (gitRef.IsRemote)
            {
                return AppSettings.RemoteBranchColor;
            }

            return AppSettings.OtherTagColor;
        }

        private static void DrawArrow(Graphics graphics, float x, float y, float rowHeight, Color color, bool filled)
        {
            ThreadHelper.AssertOnUIThread();

            const float horShift = 4;
            const float verShift = 3;

            float height = rowHeight - (verShift * 2);
            float width = height / 2;

            _arrowPoints[0] = new PointF(x + horShift, y + verShift);
            _arrowPoints[1] = new PointF(x + horShift + width, y + verShift + (height / 2));
            _arrowPoints[2] = new PointF(x + horShift, y + verShift + height);
            _arrowPoints[3] = new PointF(x + horShift, y + verShift);

            if (filled)
            {
                using (var brush = new SolidBrush(color))
                {
                    graphics.FillPolygon(brush, _arrowPoints);
                }
            }
            else
            {
                using (var pen = new Pen(color))
                {
                    graphics.DrawPolygon(pen, _arrowPoints);
                }
            }
        }

        private static GraphicsPath CreateRoundRectPath(float x, float y, float width, float height, float radius)
        {
            var path = new GraphicsPath();
            path.AddLine(x + radius, y, x + width - (radius * 2), y);
            path.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90);
            path.AddLine(x + width, y + radius, x + width, y + height - (radius * 2));
            path.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90);
            path.AddLine(x + width - (radius * 2), y + height, x + radius, y + height);
            path.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90);
            path.AddLine(x, y + height - (radius * 2), x, y + radius);
            path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
            path.CloseFigure();
            return path;
        }

        private static Color Lerp(Color colour, Color to, float amount)
        {
            // start colours as lerp-able floats
            float sr = colour.R, sg = colour.G, sb = colour.B;

            // end colours as lerp-able floats
            float er = to.R, eg = to.G, eb = to.B;

            // lerp the colours to get the difference
            byte r = (byte)Lerp(sr, er),
                g = (byte)Lerp(sg, eg),
                b = (byte)Lerp(sb, eb);

            // return the new colour
            return Color.FromArgb(r, g, b);

            float Lerp(float start, float end)
            {
                var difference = end - start;
                var adjusted = difference * amount;
                return start + adjusted;
            }
        }
    }
}