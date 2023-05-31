using System.Windows.Forms.VisualStyles;

namespace GitUI.Theming
{
    internal class TooltipRenderer : ThemeRenderer
    {
        protected override string Clsid { get; } = "Tooltip";

        public override int RenderBackground(IntPtr hdc, int partid, int stateid, Rectangle prect,
            NativeMethods.RECTCLS pcliprect)
        {
            using var ctx = CreateRenderContext(hdc, pcliprect);
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
    }
}
