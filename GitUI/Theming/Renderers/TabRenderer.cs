using System;
using System.Drawing;
using System.Windows.Forms;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming
{
    internal class TabRenderer : ThemeRenderer
    {
        protected override string Clsid { get; } = "Tab";

        public override int RenderTextEx(IntPtr htheme, IntPtr hdc, int partid, int stateid, string psztext, int cchtext, NativeMethods.DT dwtextflags, IntPtr prect, ref NativeMethods.DTTOPTS poptions)
        {
            // do not render, just modify text color
            var textColor = GetTextColor((States)stateid);
            poptions.crText = ColorTranslator.ToWin32(textColor);
            poptions.dwFlags |= NativeMethods.DTT.TextColor;

            // proceed to default implementation with modified poptions parameter
            return Unhandled;
        }

        public override int RenderBackground(IntPtr hdc, int partid, int stateid, Rectangle prect, NativeMethods.RECTCLS pcliprect)
        {
            using var ctx = CreateRenderContext(hdc, pcliprect);
            switch ((Parts)partid)
            {
                case Parts.TABP_TOPTABITEM:
                case Parts.TABP_TOPTABITEMRIGHTEDGE:
                case Parts.TABP_TOPTABITEMLEFTEDGE:
                case Parts.TABP_TOPTABITEMBOTHTEDGE:
                    RenderTab((Parts)partid, (States)stateid, prect, ctx);
                    return Handled;

                case Parts.TABP_PANE:
                    RenderPane(ctx, prect);
                    return Handled;

                // case Parts.TABP_TABITEM:
                // case Parts.TABP_TABITEMRIGHTEDGE:
                // case Parts.TABP_TABITEMLEFTEDGE:
                // case Parts.TABP_TABITEMBOTHTEDGE:
                // case Parts.TABP_BODY:
                // case Parts.TABP_AEROWIZARDBODY:
                default:
                    return Unhandled;
            }
        }

        private static void RenderPane(Context ctx, Rectangle prect)
        {
            var borderPen = GetBorderPen();
            ctx.Graphics.DrawRectangle(borderPen, prect.Inclusive());
        }

        private static void RenderTab(Parts partId, States stateId, Rectangle prect, Context ctx)
        {
            Brush backBrush = GetTabBackgroundBrush(stateId);
            ctx.Graphics.FillRectangle(backBrush, prect);

            var borderPen = GetBorderPen();
            var borderLines = GetTabBorders(partId, stateId);
            Rectangle border = prect.Inclusive();
            if ((borderLines & AnchorStyles.Top) != AnchorStyles.None)
            {
                ctx.Graphics.DrawLine(borderPen, border.Left, border.Top, border.Right, border.Top);
            }

            if ((borderLines & AnchorStyles.Left) != AnchorStyles.None)
            {
                ctx.Graphics.DrawLine(borderPen, border.Left, border.Top, border.Left, border.Bottom);
            }

            if ((borderLines & AnchorStyles.Right) != AnchorStyles.None)
            {
                ctx.Graphics.DrawLine(borderPen, border.Right, border.Top, border.Right, border.Bottom);
            }

            if ((borderLines & AnchorStyles.Bottom) != AnchorStyles.None)
            {
                ctx.Graphics.DrawLine(borderPen, border.Left, border.Bottom, border.Right, border.Bottom);
            }
        }

        private static Pen GetBorderPen() =>
            new Pen(Color.LightGray.AdaptBackColor());

        private static Brush GetTabBackgroundBrush(States stateId) =>
            stateId switch
            {
                States.TIS_SELECTED => SystemBrushes.Window,
                States.TIS_HOT => SystemBrushes.ControlDark,
                States.TIS_DISABLED => SystemBrushes.ControlLight,

                // States.TIS_NORMAL
                // States.TIS_FOCUSED
                _ => SystemBrushes.Control
            };

        private static Color GetTextColor(States stateId) =>
            stateId switch
            {
                States.TIS_SELECTED => SystemColors.WindowText,

                // States.TIS_NORMAL
                // States.TIS_FOCUSED
                // States.TIS_HOT
                _ => SystemColors.ControlText
            };

        private static AnchorStyles GetTabBorders(Parts partId, States stateId)
        {
            AnchorStyles borders = AnchorStyles.Top | AnchorStyles.Right;
            if (stateId == States.TIS_SELECTED)
            {
                borders |= AnchorStyles.Left;
            }
            else if (partId == Parts.TABP_TOPTABITEMLEFTEDGE)
            {
                borders |= AnchorStyles.Left;
            }
            else if (partId == Parts.TABP_TOPTABITEMBOTHTEDGE)
            {
                borders |= AnchorStyles.Left;
            }

            return borders;
        }

        public override bool ForceUseRenderTextEx { get; } = true;

        private enum Parts
        {
            TABP_TABITEM = 1,
            TABP_TABITEMLEFTEDGE = 2,
            TABP_TABITEMRIGHTEDGE = 3,
            TABP_TABITEMBOTHTEDGE = 4,
            TABP_TOPTABITEM = 5,
            TABP_TOPTABITEMLEFTEDGE = 6,
            TABP_TOPTABITEMRIGHTEDGE = 7,
            TABP_TOPTABITEMBOTHTEDGE = 8,
            TABP_PANE = 9,
            TABP_BODY = 10,
            TABP_AEROWIZARDBODY = 11,
        }

        private enum States
        {
            TIS_NORMAL = 1,
            TIS_HOT = 2,
            TIS_SELECTED = 3,
            TIS_DISABLED = 4,
            TIS_FOCUSED = 5,
        }
    }
}
