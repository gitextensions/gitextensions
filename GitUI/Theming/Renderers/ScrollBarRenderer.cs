using System;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using GitExtUtils.GitUI;

namespace GitUI.Theming
{
    internal class ScrollBarRenderer : ThemeRenderer
    {
        protected override string Clsid { get; } = "Scrollbar";

        public override int RenderBackground(IntPtr hdc, int partId, int stateId, Rectangle prect,
            NativeMethods.RECTCLS pcliprect)
        {
            using (var ctx = CreateRenderContext(hdc, pcliprect))
            {
                DrawBackground(ctx, partId, stateId, prect);
                if ((Parts)partId == Parts.SBP_ARROWBTN)
                {
                    DrawArrow(ctx, (States.ArrowButton)stateId, prect);
                }
            }

            return Handled;
        }

        private static void DrawBackground(Context ctx, int partId, int stateId, Rectangle prect)
        {
            var backBrush = GetBackBrush(stateId, (Parts)partId);
            ctx.Graphics.FillRectangle(backBrush, prect);
        }

        private static void DrawArrow(Context ctx, States.ArrowButton stateId, Rectangle prect)
        {
            var foreColor = GetArrowButtonForeColor(stateId);
            var arrowPts = GetArrowPolygon(prect, stateId);
            using (var pen = new Pen(foreColor, DpiUtil.Scale(2)))
            using (ctx.HighQuality())
            {
                ctx.Graphics.DrawLines(pen, arrowPts);
            }
        }

        private static Brush GetBackBrush(int stateId, Parts partId)
        {
            switch (partId)
            {
                case Parts.SBP_ARROWBTN:
                    return GetArrowButtonBackBrush((States.ArrowButton)stateId);

                case Parts.SBP_THUMBBTNHORZ:
                case Parts.SBP_THUMBBTNVERT:
                case Parts.SBP_GRIPPERHORZ:
                case Parts.SBP_GRIPPERVERT:
                    switch ((States.TrackThumb)stateId)
                    {
                        case States.TrackThumb.SCRBS_NORMAL:
                            return SystemBrushes.ScrollBar;

                        case States.TrackThumb.SCRBS_HOVER:
                            return SystemBrushes.ControlLight;

                        case States.TrackThumb.SCRBS_HOT:
                            return SystemBrushes.ControlDark;

                        case States.TrackThumb.SCRBS_PRESSED:
                            return SystemBrushes.ControlDarkDark;

                        // case States.TrackThumb.SCRBS_DISABLED:
                        default:
                            return SystemBrushes.Control;
                    }

                // case Parts.SBP_LOWERTRACKHORZ:
                // case Parts.SBP_LOWERTRACKVERT:
                // case Parts.SBP_UPPERTRACKHORZ:
                // case Parts.SBP_UPPERTRACKVERT:
                // case Parts.SBP_SIZEBOX:
                default:
                    switch ((States.TrackThumb)stateId)
                    {
                        // case States.TrackThumb.SCRBS_NORMAL:
                        // case States.TrackThumb.SCRBS_HOVER:
                        // case States.TrackThumb.SCRBS_HOT:
                        // case States.TrackThumb.SCRBS_PRESSED:
                        // case States.TrackThumb.SCRBS_DISABLED:
                        default:
                            return SystemBrushes.Control;
                    }
            }
        }

        private static Brush GetArrowButtonBackBrush(States.ArrowButton stateId)
        {
            switch (stateId)
            {
                case States.ArrowButton.ABS_UPPRESSED:
                case States.ArrowButton.ABS_DOWNPRESSED:
                case States.ArrowButton.ABS_LEFTPRESSED:
                case States.ArrowButton.ABS_RIGHTPRESSED:
                    return SystemBrushes.ControlDarkDark;

                case States.ArrowButton.ABS_UPHOT:
                case States.ArrowButton.ABS_DOWNHOT:
                case States.ArrowButton.ABS_LEFTHOT:
                case States.ArrowButton.ABS_RIGHTHOT:
                    return SystemBrushes.ControlDark;

                case States.ArrowButton.ABS_UPHOVER:
                case States.ArrowButton.ABS_DOWNHOVER:
                case States.ArrowButton.ABS_LEFTHOVER:
                case States.ArrowButton.ABS_RIGHTHOVER:
                    return SystemBrushes.ControlLight;

                // case States.ArrowButton.ABS_UPDISABLED:
                // case States.ArrowButton.ABS_DOWNDISABLED:
                // case States.ArrowButton.ABS_LEFTDISABLED:
                // case States.ArrowButton.ABS_RIGHTDISABLED:
                // case States.ArrowButton.ABS_UPNORMAL:
                // case States.ArrowButton.ABS_DOWNNORMAL:
                // case States.ArrowButton.ABS_LEFTNORMAL:
                // case States.ArrowButton.ABS_RIGHTNORMAL:
                default:
                    return SystemBrushes.Control;
            }
        }

        private static Color GetArrowButtonForeColor(States.ArrowButton stateId)
        {
            switch (stateId)
            {
                case States.ArrowButton.ABS_UPPRESSED:
                case States.ArrowButton.ABS_DOWNPRESSED:
                case States.ArrowButton.ABS_LEFTPRESSED:
                case States.ArrowButton.ABS_RIGHTPRESSED:
                    return SystemColors.Control;

                case States.ArrowButton.ABS_UPDISABLED:
                case States.ArrowButton.ABS_DOWNDISABLED:
                case States.ArrowButton.ABS_LEFTDISABLED:
                case States.ArrowButton.ABS_RIGHTDISABLED:
                    return SystemColors.ControlDark;

                // case States.ArrowButton.ABS_UPHOT:
                // case States.ArrowButton.ABS_DOWNHOT:
                // case States.ArrowButton.ABS_LEFTHOT:
                // case States.ArrowButton.ABS_RIGHTHOT:
                // case States.ArrowButton.ABS_UPNORMAL:
                // case States.ArrowButton.ABS_DOWNNORMAL:
                // case States.ArrowButton.ABS_LEFTNORMAL:
                // case States.ArrowButton.ABS_RIGHTNORMAL:
                // case States.ArrowButton.ABS_UPHOVER:
                // case States.ArrowButton.ABS_DOWNHOVER:
                // case States.ArrowButton.ABS_LEFTHOVER:
                // case States.ArrowButton.ABS_RIGHTHOVER:
                default:
                    return SystemColors.ControlDarkDark;
            }
        }

        private static Point[] GetArrowPolygon(Rectangle prect, States.ArrowButton stateId)
        {
            switch (stateId)
            {
                case States.ArrowButton.ABS_UPNORMAL:
                case States.ArrowButton.ABS_UPHOT:
                case States.ArrowButton.ABS_UPPRESSED:
                case States.ArrowButton.ABS_UPDISABLED:
                case States.ArrowButton.ABS_UPHOVER:
                    return GetUpArrowPolygon(prect);

                case States.ArrowButton.ABS_DOWNNORMAL:
                case States.ArrowButton.ABS_DOWNHOT:
                case States.ArrowButton.ABS_DOWNPRESSED:
                case States.ArrowButton.ABS_DOWNDISABLED:
                case States.ArrowButton.ABS_DOWNHOVER:
                    return GetDownArrowPolygon(prect);

                case States.ArrowButton.ABS_RIGHTNORMAL:
                case States.ArrowButton.ABS_RIGHTHOT:
                case States.ArrowButton.ABS_RIGHTPRESSED:
                case States.ArrowButton.ABS_RIGHTDISABLED:
                case States.ArrowButton.ABS_RIGHTHOVER:
                    return GetRightArrowPolygon(prect);

                // case States.ArrowButton.ABS_LEFTNORMAL:
                // case States.ArrowButton.ABS_LEFTHOT:
                // case States.ArrowButton.ABS_LEFTPRESSED:
                // case States.ArrowButton.ABS_LEFTDISABLED:
                // case States.ArrowButton.ABS_LEFTHOVER:
                default:
                    return GetLeftArrowPolygon(prect);
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

        private static Point[] GetRightArrowPolygon(Rectangle prect)
        {
            int arrowWidth = prect.Width / 4;
            int arrowHeight = arrowWidth * 2;
            int arrowLeft = prect.Left + ((prect.Width - arrowWidth) / 2);
            int arrowTop = prect.Top + ((prect.Height - arrowHeight) / 2);
            int x1 = arrowLeft;
            int x2 = arrowLeft + arrowWidth;
            int y1 = arrowTop;
            int y2 = arrowTop + (arrowHeight / 2);
            int y3 = arrowTop + arrowHeight;
            return new[]
            {
                new Point(x1, y1),
                new Point(x2, y2),
                new Point(x1, y3)
            };
        }

        private static Point[] GetLeftArrowPolygon(Rectangle prect)
        {
            int arrowWidth = prect.Width / 4;
            int arrowHeight = arrowWidth * 2;
            int arrowLeft = prect.Left + ((prect.Width - arrowWidth) / 2);
            int arrowTop = prect.Top + ((prect.Height - arrowHeight) / 2);
            int x1 = arrowLeft;
            int x2 = arrowLeft + arrowWidth;
            int y1 = arrowTop;
            int y2 = arrowTop + (arrowHeight / 2);
            int y3 = arrowTop + arrowHeight;

            return new[]
            {
                new Point(x2, y1),
                new Point(x1, y2),
                new Point(x2, y3)
            };
        }

        public override int GetThemeColor(int ipartid, int istateid, int ipropid, out int pcolor)
        {
            if ((Parts)ipartid == Parts.SBP_UNDOCUMENTED && (ColorProperty)ipropid == ColorProperty.FillColor)
            {
                pcolor = ColorTranslator.ToWin32(SystemColors.Control);
                return Handled;
            }

            pcolor = 0;
            return Unhandled;
        }

        private enum Parts
        {
            SBP_ARROWBTN = 1,
            SBP_THUMBBTNHORZ = 2,
            SBP_THUMBBTNVERT = 3,
            SBP_LOWERTRACKHORZ = 4,
            SBP_UPPERTRACKHORZ = 5,
            SBP_LOWERTRACKVERT = 6,
            SBP_UPPERTRACKVERT = 7,
            SBP_GRIPPERHORZ = 8,
            SBP_GRIPPERVERT = 9,
            SBP_SIZEBOX = 10,

            // square gap between horizontal and vertical scroll
            SBP_UNDOCUMENTED = 11
        }

        private static class States
        {
            public enum ArrowButton
            {
                ABS_UPNORMAL = 1,
                ABS_UPHOT = 2,
                ABS_UPPRESSED = 3,
                ABS_UPDISABLED = 4,
                ABS_DOWNNORMAL = 5,
                ABS_DOWNHOT = 6,
                ABS_DOWNPRESSED = 7,
                ABS_DOWNDISABLED = 8,
                ABS_LEFTNORMAL = 9,
                ABS_LEFTHOT = 10,
                ABS_LEFTPRESSED = 11,
                ABS_LEFTDISABLED = 12,
                ABS_RIGHTNORMAL = 13,
                ABS_RIGHTHOT = 14,
                ABS_RIGHTPRESSED = 15,
                ABS_RIGHTDISABLED = 16,
                ABS_UPHOVER = 17,
                ABS_DOWNHOVER = 18,
                ABS_LEFTHOVER = 19,
                ABS_RIGHTHOVER = 20,
            }

            public enum SizeBox
            {
                SZB_HALFBOTTOMLEFTALIGN = 6,
                SZB_HALFBOTTOMRIGHTALIGN = 5,
                SZB_HALFTOPLEFTALIGN = 8,
                SZB_HALFTOPRIGHTALIGN = 7,
                SZB_LEFTALIGN = 2,
                SZB_RIGHTALIGN = 1,
                SZB_TOPLEFTALIGN = 4,
                SZB_TOPRIGHTALIGN = 3,
            }

            public enum TrackThumb
            {
                SCRBS_NORMAL = 1,
                SCRBS_HOT = 2,
                SCRBS_PRESSED = 3,
                SCRBS_DISABLED = 4,
                SCRBS_HOVER = 5,
            }
        }
    }
}
