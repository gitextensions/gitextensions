using System;
using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace GitUI.Theming
{
    internal class TooltipRenderer : ThemeRenderer
    {
        protected override string Clsid { get; } = "Tooltip";

        public override int RenderBackground(IntPtr hdc, int partid, int stateid, Rectangle prect,
            NativeMethods.RECTCLS pcliprect)
        {
            using (var ctx = CreateRenderContext(hdc, pcliprect))
            {
                switch ((Parts)partid)
                {
                    case Parts.TTP_STANDARD:
                    case Parts.TTP_STANDARDTITLE:
                    case Parts.TTP_BALLOON:
                    case Parts.TTP_BALLOONTITLE:
                        ctx.Graphics.FillRectangle(SystemBrushes.Info, prect);
                        ctx.Graphics.DrawRectangle(SystemPens.ControlDark, prect.Inclusive());
                        return Handled;

                    // case Parts.TTP_BALLOONSTEM:
                    // case Parts.TTP_CLOSE:
                    // case Parts.TTP_WRENCH:
                    default:
                        return Unhandled;
                }
            }
        }

        public override int GetThemeColor(int ipartid, int istateid, int ipropid, out int pcolor)
        {
            pcolor = 0;
            switch ((ColorProperty)ipropid)
            {
                case ColorProperty.TextColor:
                    pcolor = ColorTranslator.ToWin32(SystemColors.InfoText);
                    break;

                default:
                    return Unhandled;
            }

            return Handled;
        }

        private enum Parts
        {
            TTP_STANDARD = 1,
            TTP_STANDARDTITLE = 2,
            TTP_BALLOON = 3,
            TTP_BALLOONTITLE = 4,
            TTP_CLOSE = 5,
            TTP_BALLOONSTEM = 6,
            TTP_WRENCH = 7,
        }

        private class State
        {
            public enum Close
            {
                TTCS_NORMAL = 1,
                TTCS_HOT = 2,
                TTCS_PRESSED = 3,
            }

            public enum Standard
            {
                TTSS_NORMAL = 1,
                TTSS_LINK = 2,
            }

            public enum Baloon
            {
                TTBS_NORMAL = 1,
                TTBS_LINK = 2,
            }

            public enum BaloonStem
            {
                TTBSS_POINTINGUPLEFTWALL = 1,
                TTBSS_POINTINGUPCENTERED = 2,
                TTBSS_POINTINGUPRIGHTWALL = 3,
                TTBSS_POINTINGDOWNRIGHTWALL = 4,
                TTBSS_POINTINGDOWNCENTERED = 5,
                TTBSS_POINTINGDOWNLEFTWALL = 6,
            }

            public enum Wrench
            {
                TTWS_NORMAL = 1,
                TTWS_HOT = 2,
                TTWS_PRESSED = 3,
            }
        }
    }
}
