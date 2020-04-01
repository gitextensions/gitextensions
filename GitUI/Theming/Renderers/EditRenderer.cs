using System;
using System.Drawing;

namespace GitUI.Theming
{
    internal class EditRenderer : ThemeRenderer
    {
        protected override string Clsid { get; } = "Edit";

        public override int RenderBackground(IntPtr hdc, int partid, int stateid, Rectangle prect,
            NativeMethods.RECTCLS pcliprect)
        {
            using (var ctx = CreateRenderContext(hdc, pcliprect))
            {
                switch ((Parts)partid)
                {
                    case Parts.EP_EDITTEXT:
                        return RenderEditText(ctx, stateid, prect);

                    case Parts.EP_EDITBORDER_NOSCROLL:
                        return RenderEditBorderNoScroll(ctx, stateid, prect);
                }
            }

            return Unhandled;
        }

        public override int GetThemeColor(int ipartid, int istateid, int ipropid, out int pcolor)
        {
            switch ((Parts)ipartid)
            {
                case Parts.EP_EDITTEXT:
                {
                    switch ((State.Text)istateid)
                    {
                        case State.Text.ETS_CUEBANNER:
                            pcolor = ColorTranslator.ToWin32(SystemColors.ControlDark);
                            return Handled;
                    }
                }

                break;
            }

            pcolor = 0;
            return Unhandled;
        }

        private int RenderEditText(Context ctx, int stateid, Rectangle prect)
        {
            Brush backBrush;
            switch ((State.Text)stateid)
            {
                case State.Text.ETS_NORMAL:
                case State.Text.ETS_HOT:
                case State.Text.ETS_FOCUSED:
                case State.Text.ETS_SELECTED:
                case State.Text.ETS_READONLY:
                    // fix numeric updown border
                    backBrush = SystemBrushes.Window;
                    break;

                case State.Text.ETS_DISABLED:
                    backBrush = SystemBrushes.Control;
                    break;

                default:
                    return Unhandled;
            }

            Pen borderPen;
            switch ((State.Text)stateid)
            {
                case State.Text.ETS_NORMAL:
                case State.Text.ETS_READONLY:
                case State.Text.ETS_SELECTED:
                    borderPen = SystemPens.ControlDark;
                    break;

                case State.Text.ETS_HOT:
                    borderPen = SystemPens.ControlDarkDark;
                    break;

                case State.Text.ETS_FOCUSED:
                    borderPen = SystemPens.HotTrack;
                    break;

                case State.Text.ETS_DISABLED:
                    borderPen = SystemPens.ControlLight;
                    break;

                default:
                    return Unhandled;
            }

            ctx.Graphics.FillRectangle(backBrush, prect);
            ctx.Graphics.DrawRectangle(borderPen, prect.Inclusive());

            return Handled;
        }

        private static int RenderEditBorderNoScroll(Context ctx, int stateid, Rectangle prect)
        {
            Pen borderPen;
            Brush backBrush;
            switch ((State.BorderNoScroll)stateid)
            {
                case State.BorderNoScroll.EPSN_NORMAL:
                    backBrush = SystemBrushes.Window;
                    borderPen = SystemPens.ControlDark;
                    break;
                case State.BorderNoScroll.EPSN_HOT:
                    backBrush = SystemBrushes.Window;
                    borderPen = SystemPens.ControlDarkDark;
                    break;
                case State.BorderNoScroll.EPSN_FOCUSED:
                    backBrush = SystemBrushes.Window;
                    borderPen = SystemPens.HotTrack;
                    break;
                case State.BorderNoScroll.EPSN_DISABLED:
                    backBrush = SystemBrushes.Control;
                    borderPen = SystemPens.ControlLight;
                    break;
                default:
                    return Unhandled;
            }

            ctx.Graphics.FillRectangle(backBrush, prect);
            ctx.Graphics.DrawRectangle(borderPen, prect.Inclusive());

            return Handled;
        }

        private enum Parts
        {
            EP_EDITTEXT = 1,
            EP_CARET = 2,
            EP_BACKGROUND = 3,
            EP_PASSWORD = 4,
            EP_BACKGROUNDWITHBORDER = 5,
            EP_EDITBORDER_NOSCROLL = 6,
            EP_EDITBORDER_HSCROLL = 7,
            EP_EDITBORDER_VSCROLL = 8,
            EP_EDITBORDER_HVSCROLL = 9,
        }

        private class State
        {
            public enum Text
            {
                ETS_NORMAL = 1,
                ETS_HOT = 2,
                ETS_SELECTED = 3,
                ETS_DISABLED = 4,
                ETS_FOCUSED = 5,
                ETS_READONLY = 6,
                ETS_ASSIST = 7,
                ETS_CUEBANNER = 8,
            }

            public enum Background
            {
                EBS_NORMAL = 1,
                EBS_HOT = 2,
                EBS_DISABLED = 3,
                EBS_FOCUSED = 4,
                EBS_READONLY = 5,
                EBS_ASSIST = 6,
            }

            public enum BackgroundWithBorder
            {
                EBWBS_NORMAL = 1,
                EBWBS_HOT = 2,
                EBWBS_DISABLED = 3,
                EBWBS_FOCUSED = 4,
            }

            public enum BorderNoScroll
            {
                EPSN_NORMAL = 1,
                EPSN_HOT = 2,
                EPSN_FOCUSED = 3,
                EPSN_DISABLED = 4,
            }

            public enum BorderHScroll
            {
                EPSH_NORMAL = 1,
                EPSH_HOT = 2,
                EPSH_FOCUSED = 3,
                EPSH_DISABLED = 4,
            }

            public enum BorderVScroll
            {
                EPSV_NORMAL = 1,
                EPSV_HOT = 2,
                EPSV_FOCUSED = 3,
                EPSV_DISABLED = 4,
            }

            public enum BorderHVScroll
            {
                EPSHV_NORMAL = 1,
                EPSHV_HOT = 2,
                EPSHV_FOCUSED = 3,
                EPSHV_DISABLED = 4,
            }
        }
    }
}
