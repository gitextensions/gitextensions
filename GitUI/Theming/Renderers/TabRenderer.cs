using System;
using System.Drawing;
using System.Windows.Forms;

namespace GitUI.Theming
{
    internal class TabRenderer : ThemeRenderer
    {
        protected override string Clsid { get; } = "Tab";

        public override int RenderBackground(IntPtr hdc, int partid, int stateid, Rectangle prect, NativeMethods.RECTCLS pcliprect)
        {
            using var ctx = CreateRenderContext(hdc, pcliprect);
            var border = prect.Inclusive();
            switch ((Parts)partid)
            {
                case Parts.TABP_TOPTABITEM:
                case Parts.TABP_TOPTABITEMRIGHTEDGE:
                    RenderTab((States)stateid, prect, ctx, AnchorStyles.Top | AnchorStyles.Right);
                    return Handled;

                case Parts.TABP_TOPTABITEMLEFTEDGE:
                case Parts.TABP_TOPTABITEMBOTHTEDGE:
                    RenderTab((States)stateid, prect, ctx, AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
                    return Handled;

                case Parts.TABP_PANE:
                    RenderPane(ctx, border);
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

        private static void RenderPane(Context ctx, Rectangle border)
        {
            ctx.Graphics.DrawRectangle(SystemPens.ActiveBorder, border);
        }

        private static void RenderTab(States stateId, Rectangle prect, Context ctx, AnchorStyles borders)
        {
            Brush backBrush;
            switch (stateId)
            {
                case States.TIS_NORMAL:
                case States.TIS_FOCUSED:
                    backBrush = SystemBrushes.Control;
                    break;

                case States.TIS_SELECTED:
                    backBrush = SystemBrushes.Window;
                    break;

                case States.TIS_HOT:
                    backBrush = SystemBrushes.ControlDark;
                    break;

                // case States.TIS_DISABLED:
                default:
                    backBrush = SystemBrushes.ControlLight;
                    break;
            }

            Rectangle border = prect.Inclusive();
            Pen borderPen = SystemPens.ActiveBorder;
            ctx.Graphics.FillRectangle(backBrush, prect);
            if ((borders & AnchorStyles.Top) != AnchorStyles.None)
            {
                ctx.Graphics.DrawLine(borderPen, border.Left, border.Top, border.Right, border.Top);
            }

            if ((borders & AnchorStyles.Left) != AnchorStyles.None)
            {
                ctx.Graphics.DrawLine(borderPen, border.Left, border.Top, border.Left, border.Bottom);
            }

            if ((borders & AnchorStyles.Right) != AnchorStyles.None)
            {
                ctx.Graphics.DrawLine(borderPen, border.Right, border.Top, border.Right, border.Bottom);
            }

            if ((borders & AnchorStyles.Bottom) != AnchorStyles.None)
            {
                ctx.Graphics.DrawLine(borderPen, border.Left, border.Bottom, border.Right, border.Bottom);
            }
        }

        public override bool ForceUseRenderTextEx { get; } = true;

        public override int RenderTextEx(IntPtr htheme, IntPtr hdc, int partid, int stateid, string psztext, int cchtext, NativeMethods.DT dwtextflags, IntPtr prect, ref NativeMethods.DTTOPTS poptions)
        {
            NativeMethods.GetThemeColor(htheme, partid, stateid, poptions.iColorPropId,
                out var crefText);

            // do not render, just modify text color
            poptions.iColorPropId = 0;
            poptions.crText = ColorTranslator.ToWin32(SystemColors.ControlText);
            poptions.dwFlags |= NativeMethods.DTT.TextColor;

            // proceed to default implementation with modified poptions parameter
            return Unhandled;
        }

        public override int RenderBackgroundEx(IntPtr htheme, IntPtr hdc, int partid, int stateid, NativeMethods.RECTCLS prect, ref NativeMethods.DTBGOPTS poptions)
        {
            return Unhandled;
        }

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
