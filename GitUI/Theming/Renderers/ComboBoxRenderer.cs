using System;
using System.Drawing;
using GitExtUtils.GitUI;

namespace GitUI.Theming
{
    internal class ComboBoxRenderer : ThemeRenderer
    {
        protected override string Clsid { get; } = "Combobox";

        public override int RenderBackground(IntPtr hdc, int partid, int stateid, Rectangle prect,
            NativeMethods.RECTCLS pcliprect)
        {
            using (var ctx = CreateRenderContext(hdc, pcliprect))
            {
                switch ((Parts)partid)
                {
                    case Parts.CP_BACKGROUND:
                    case Parts.CP_CUEBANNER:
                        return RenderBackground(ctx, prect);

                    case Parts.CP_BORDER:
                        return RenderBorder(ctx, stateid, prect);

                    case Parts.CP_DROPDOWNBUTTON:
                    case Parts.CP_DROPDOWNBUTTONRIGHT:
                    case Parts.CP_DROPDOWNBUTTONLEFT:
                        return RenderDropDownButton(ctx, (State.DropDown)stateid, prect);

                    case Parts.CP_READONLY:
                        return RenderReadonlyDropDown(ctx, stateid, prect);

                    // case Parts.CP_TRANSPARENTBACKGROUND:
                    default:
                        return Unhandled;
                }
            }
        }

        public override bool ForceUseRenderTextEx { get; } = true;

        public override int RenderTextEx(IntPtr htheme, IntPtr hdc, int partid, int stateid,
            string psztext, int cchtext, NativeMethods.DT dwtextflags,
            IntPtr prect, ref NativeMethods.DTTOPTS poptions)
        {
            Color textColor;
            switch ((Parts)partid)
            {
                case Parts.CP_READONLY:
                    switch ((State.Readonly)stateid)
                    {
                        case State.Readonly.CBRO_NORMAL:
                        case State.Readonly.CBRO_HOT:
                        case State.Readonly.CBRO_PRESSED:
                            textColor = SystemColors.ControlText;
                            break;

                        case State.Readonly.CBRO_DISABLED:
                            textColor = SystemColors.ControlDark;
                            break;

                        default:
                            return Unhandled;
                    }

                    // do not render, just modify text color
                    poptions.dwFlags |= NativeMethods.DTT.TextColor;
                    poptions.iColorPropId = 0;
                    poptions.crText = ColorTranslator.ToWin32(textColor);
                    return Unhandled;

                default:
                    return Unhandled;
            }
        }

        private static int RenderBackground(Context ctx, Rectangle prect)
        {
            ctx.Graphics.FillRectangle(SystemBrushes.Window, prect);
            return Handled;
        }

        private static int RenderBorder(Context ctx, int stateid, Rectangle prect)
        {
            Pen borderPen;
            Brush backBrush;
            switch ((State.Border)stateid)
            {
                case State.Border.CBB_NORMAL:
                    borderPen = SystemPens.ControlDark;
                    backBrush = SystemBrushes.Window;
                    break;

                case State.Border.CBB_HOT:
                    borderPen = SystemPens.HotTrack;
                    backBrush = SystemBrushes.Window;
                    break;

                case State.Border.CBB_FOCUSED:
                    borderPen = SystemPens.Highlight;
                    backBrush = SystemBrushes.Window;
                    break;

                case State.Border.CBB_DISABLED:
                    borderPen = SystemPens.ControlLight;
                    backBrush = SystemBrushes.Control;
                    break;

                default:
                    return Unhandled;
            }

            ctx.Graphics.FillRectangle(backBrush, prect);
            ctx.Graphics.DrawRectangle(borderPen, prect.Inclusive());
            return Handled;
        }

        private static int RenderDropDownButton(Context ctx, State.DropDown stateid, Rectangle prect)
        {
            var border = prect.Inclusive();
            switch (stateid)
            {
                case State.DropDown.CBXS_HOT:
                    ctx.Graphics.FillRectangle(SystemBrushes.Control, prect);
                    ctx.Graphics.DrawRectangle(SystemPens.HotTrack, border);
                    break;

                case State.DropDown.CBXS_PRESSED:
                    ctx.Graphics.FillRectangle(SystemBrushes.Control, prect);
                    ctx.Graphics.DrawRectangle(SystemPens.Highlight, border);
                    break;

                case State.DropDown.CBXS_DISABLED:
                case State.DropDown.CBXS_NORMAL:
                    break;

                default:
                    return Unhandled;
            }

            RenderDownArrow(ctx, stateid, prect);
            return Handled;
        }

        private static int RenderReadonlyDropDown(Context ctx, int stateid, Rectangle prect)
        {
            Pen borderPen;
            Brush backBrush;
            switch ((State.Readonly)stateid)
            {
                case State.Readonly.CBRO_DISABLED:
                    backBrush = SystemBrushes.ControlLight;
                    borderPen = SystemPens.ControlDark;
                    break;

                case State.Readonly.CBRO_HOT:
                    backBrush = SystemBrushes.ControlLight;
                    borderPen = SystemPens.HotTrack;
                    break;

                case State.Readonly.CBRO_PRESSED:
                    backBrush = SystemBrushes.ControlDark;
                    borderPen = SystemPens.HotTrack;
                    break;

                case State.Readonly.CBRO_NORMAL:
                    backBrush = SystemBrushes.Control;
                    borderPen = SystemPens.ControlDark;
                    break;

                default:
                    return Unhandled;
            }

            ctx.Graphics.FillRectangle(backBrush, prect);
            ctx.Graphics.DrawRectangle(borderPen, prect.Inclusive());

            return Handled;
        }

        private static void RenderDownArrow(Context ctx, State.DropDown stateid, Rectangle prect)
        {
            int h = prect.Width / 4;
            int w = h * 2;

            int x1 = prect.Left + ((prect.Width - w) / 2);
            int x2 = x1 + (w / 2);
            int x3 = x1 + w;

            int y1 = prect.Top + ((prect.Height - h) / 2);
            int y2 = y1 + h;

            var arrowPoints = new[]
            {
                new Point(x1, y1),
                new Point(x2, y2),
                new Point(x3, y1)
            };

            Color arrowColor;
            switch (stateid)
            {
                case State.DropDown.CBXS_DISABLED:
                    arrowColor = SystemColors.ControlDark;
                    break;

                // case State.DropDown.CBXS_NORMAL:
                // case State.DropDown.CBXS_HOT:
                // case State.DropDown.CBXS_PRESSED:
                default:
                    arrowColor = SystemColors.ControlDarkDark;
                    break;
            }

            using (ctx.HighQuality())
            using (var pen = new Pen(arrowColor, DpiUtil.Scale(2)))
            {
                ctx.Graphics.DrawLines(pen, arrowPoints);
            }
        }

        private enum Parts
        {
            CP_DROPDOWNBUTTON = 1,
            CP_BACKGROUND = 2,
            CP_TRANSPARENTBACKGROUND = 3,
            CP_BORDER = 4,
            CP_READONLY = 5,
            CP_DROPDOWNBUTTONRIGHT = 6,
            CP_DROPDOWNBUTTONLEFT = 7,
            CP_CUEBANNER = 8,
        }

        private class State
        {
            public enum DropDown
            {
                CBXS_NORMAL = 1,
                CBXS_HOT = 2,
                CBXS_PRESSED = 3,
                CBXS_DISABLED = 4,
            }

            public enum DropDownRight
            {
                CBXSR_NORMAL = 1,
                CBXSR_HOT = 2,
                CBXSR_PRESSED = 3,
                CBXSR_DISABLED = 4,
            }

            public enum DropDownLeft
            {
                CBXSL_NORMAL = 1,
                CBXSL_HOT = 2,
                CBXSL_PRESSED = 3,
                CBXSL_DISABLED = 4,
            }

            public enum TransparentBack
            {
                CBTBS_NORMAL = 1,
                CBTBS_HOT = 2,
                CBTBS_DISABLED = 3,
                CBTBS_FOCUSED = 4,
            }

            public enum Border
            {
                CBB_NORMAL = 1,
                CBB_HOT = 2,
                CBB_FOCUSED = 3,
                CBB_DISABLED = 4,
            }

            public enum Readonly
            {
                CBRO_NORMAL = 1,
                CBRO_HOT = 2,
                CBRO_PRESSED = 3,
                CBRO_DISABLED = 4,
            }

            public enum CueBanner
            {
                CBCB_NORMAL = 1,
                CBCB_HOT = 2,
                CBCB_PRESSED = 3,
                CBCB_DISABLED = 4,
            }
        }
    }
}
