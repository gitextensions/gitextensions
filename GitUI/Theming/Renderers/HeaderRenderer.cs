using System;
using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace GitUI.Theming
{
    internal class HeaderRenderer : ThemeRenderer
    {
        protected override string Clsid { get; } = "Header";

        public override int RenderBackground(IntPtr hdc, int partId, int stateId, Rectangle prect,
            NativeMethods.RECTCLS pcliprect)
        {
            using (var ctx = CreateRenderContext(hdc, pcliprect))
            {
                switch ((Parts)partId)
                {
                    case Parts.None:
                    {
                        ctx.Graphics.FillRectangle(SystemBrushes.Control, prect);
                        return Handled;
                    }

                    case Parts.HP_HEADERITEM:
                    {
                        var backBrush = GetBackBrush((State.Item)stateId);
                        ctx.Graphics.FillRectangle(backBrush, prect);
                        ctx.Graphics.DrawLine(SystemPens.ControlDark,
                            new Point(prect.Right - 1, prect.Top),
                            new Point(prect.Right - 1, prect.Bottom - 1));
                        return Handled;
                    }

                    case Parts.HP_HEADERSORTARROW:
                    {
                        var arrowPoints = GetArrowPolygon((State.SortArrow)stateId, prect);
                        ctx.Graphics.FillRectangle(SystemBrushes.Control, prect);
                        using (ctx.HighQuality())
                        {
                            ctx.Graphics.FillPolygon(SystemBrushes.ControlDarkDark, arrowPoints);
                        }

                        return Handled;
                    }

                    // case Parts.HP_HEADERITEMLEFT:
                    // case Parts.HP_HEADERITEMRIGHT:
                    // case Parts.HP_HEADERDROPDOWN:
                    // case Parts.HP_HEADERDROPDOWNFILTER:
                    // case Parts.HP_HEADEROVERFLOW:
                    default:
                    {
                        return Unhandled;
                    }
                }
            }
        }

        public override int GetThemeColor(int ipartid, int istateid, int ipropid, out int pcolor)
        {
            switch ((Parts)ipartid)
            {
                case Parts.HP_HEADERITEM:
                    switch ((ColorProperty)ipropid)
                    {
                        case ColorProperty.TextColor:
                            pcolor = ColorTranslator.ToWin32(SystemColors.ControlText);
                            return Handled;
                    }

                    break;
            }

            pcolor = 0;
            return Unhandled;
        }

        private static Point[] GetArrowPolygon(State.SortArrow stateId, Rectangle prect)
        {
            switch (stateId)
            {
                case State.SortArrow.HSAS_SORTEDUP:
                    return GetUpArrowPolygon(prect);

                // case State.SortArrow.HSAS_SORTEDDOWN:
                default:
                    return GetDownArrowPolygon(prect);
            }
        }

        private static Brush GetBackBrush(State.Item stateId)
        {
            switch (stateId)
            {
                case State.Item.HIS_NORMAL:
                case State.Item.HIS_SORTEDNORMAL:
                case State.Item.HIS_ICONNORMAL:
                case State.Item.HIS_ICONSORTEDNORMAL:
                    return SystemBrushes.Control;

                case State.Item.HIS_HOT:
                case State.Item.HIS_SORTEDHOT:
                case State.Item.HIS_ICONHOT:
                case State.Item.HIS_ICONSORTEDHOT:
                    return SystemBrushes.ControlLight;

                // case State.HeaderItem.HIS_PRESSED:
                // case State.HeaderItem.HIS_SORTEDPRESSED:
                // case State.HeaderItem.HIS_ICONPRESSED:
                // case State.HeaderItem.HIS_ICONSORTEDPRESSED:
                default:
                    return SystemBrushes.ControlDark;
            }
        }

        private static Point[] GetUpArrowPolygon(Rectangle prect)
        {
            int arrowHeight = prect.Height / 4;
            int arrowWidth = arrowHeight * 2;
            int arrowLeft = prect.Left + ((prect.Width - arrowWidth) / 2);
            int arrowTop = prect.Top + ((prect.Height - arrowHeight) / 2);
            int x1 = arrowLeft;
            int x2 = arrowLeft + (arrowWidth / 2);
            int x3 = arrowLeft + arrowWidth;
            int y1 = arrowTop;
            int y2 = arrowTop + arrowHeight;
            return new[]
            {
                new Point(x1, y2),
                new Point(x2, y1),
                new Point(x3, y2)
            };
        }

        private static Point[] GetDownArrowPolygon(Rectangle prect)
        {
            int arrowHeight = prect.Height / 4;
            int arrowWidth = arrowHeight * 2;
            int arrowLeft = prect.Left + ((prect.Width - arrowWidth) / 2);
            int arrowTop = prect.Top + ((prect.Height - arrowHeight) / 2);
            int x1 = arrowLeft;
            int x2 = arrowLeft + (arrowWidth / 2);
            int x3 = arrowLeft + arrowWidth;
            int y1 = arrowTop;
            int y2 = arrowTop + arrowHeight;
            return new[]
            {
                new Point(x1, y1),
                new Point(x2, y2),
                new Point(x3, y1)
            };
        }

        private enum Parts
        {
            None = 0,
            HP_HEADERITEM = 1,
            HP_HEADERITEMLEFT = 2,
            HP_HEADERITEMRIGHT = 3,
            HP_HEADERSORTARROW = 4,
            HP_HEADERDROPDOWN = 5,
            HP_HEADERDROPDOWNFILTER = 6,
            HP_HEADEROVERFLOW = 7
        }

        private static class State
        {
            public enum Item
            {
                HIS_NORMAL = 1,
                HIS_HOT = 2,
                HIS_PRESSED = 3,
                HIS_SORTEDNORMAL = 4,
                HIS_SORTEDHOT = 5,
                HIS_SORTEDPRESSED = 6,
                HIS_ICONNORMAL = 7,
                HIS_ICONHOT = 8,
                HIS_ICONPRESSED = 9,
                HIS_ICONSORTEDNORMAL = 10,
                HIS_ICONSORTEDHOT = 11,
                HIS_ICONSORTEDPRESSED = 12,
            }

            public enum ItemLeft
            {
                HILS_NORMAL = 1,
                HILS_HOT = 2,
                HILS_PRESSED = 3
            }

            public enum ItemRight
            {
                HIRS_NORMAL = 1,
                HIRS_HOT = 2,
                HIRS_PRESSED = 3
            }

            public enum ItemOverflow
            {
                HOFS_NORMAL = 1,
                HOFS_HOT = 2,
            }

            public enum DropDown
            {
                HDDS_NORMAL = 1,
                HDDS_SOFTHOT = 2,
                HDDS_HOT = 3
            }

            public enum DropDownFilter
            {
                HDDFS_NORMAL = 1,
                HDDFS_SOFTHOT = 2,
                HDDFS_HOT = 3
            }

            public enum SortArrow
            {
                HSAS_SORTEDUP = 1,
                HSAS_SORTEDDOWN = 2,
            }
        }
    }
}
