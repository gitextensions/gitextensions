using System;
using System.Drawing;
using GitExtUtils.GitUI;

namespace GitUI.Theming
{
    internal class SpinRenderer : ThemeRenderer
    {
        protected override string Clsid { get; } = "Spin";

        public override int RenderBackground(IntPtr hdc, int partId, int stateId, Rectangle prect,
            NativeMethods.RECTCLS pcliprect)
        {
            using (var ctx = CreateRenderContext(hdc, pcliprect))
            {
                switch ((Parts)partId)
                {
                    case Parts.SPNP_UP:
                        return RenderUpButton(ctx, stateId, prect);

                    case Parts.SPNP_DOWN:
                        return RenderDownButton(ctx, stateId, prect);

                    default:
                        return Unhandled;
                }
            }
        }

        private static int RenderDownButton(Context ctx, int stateId, Rectangle prect)
        {
            var backBrush = GetBackBrush((State.Down)stateId);
            var foreColor = GetForeColor((State.Down)stateId);
            var arrowPolygon = GetArrowPolygon(prect, down: true);
            RenderButton(ctx, prect, backBrush, foreColor, arrowPolygon);
            return Handled;
        }

        private static int RenderUpButton(Context ctx, int stateId, Rectangle prect)
        {
            var backBrush = GetBackBrush((State.Up)stateId);
            var foreColor = GetForeColor((State.Up)stateId);
            var arrowPolygon = GetArrowPolygon(prect, down: false);
            RenderButton(ctx, prect, backBrush, foreColor, arrowPolygon);
            return Handled;
        }

        private static Brush GetBackBrush(State.Up stateId)
        {
            switch (stateId)
            {
                case State.Up.UPS_HOT:
                    return SystemBrushes.ControlDark;

                case State.Up.UPS_PRESSED:
                    return SystemBrushes.ControlDarkDark;

                // case States.Up.UPS_NORMAL:
                // case States.Up.UPS_DISABLED:
                default:
                    return SystemBrushes.Control;
            }
        }

        private static Brush GetBackBrush(State.Down stateId)
        {
            switch (stateId)
            {
                case State.Down.DNS_HOT:
                    return SystemBrushes.ControlDark;

                case State.Down.DNS_PRESSED:
                    return SystemBrushes.ControlDarkDark;

                // case States.Down.DNS_NORMAL:
                // case States.Down.DNS_DISABLED:
                default:
                    return SystemBrushes.Control;
            }
        }

        private static Color GetForeColor(State.Up stateId)
        {
            switch (stateId)
            {
                case State.Up.UPS_PRESSED:
                    return SystemColors.Control;

                case State.Up.UPS_DISABLED:
                    return SystemColors.ControlDark;

                // case States.Up.UPS_NORMAL:
                // case States.Up.UPS_HOT:
                default:
                    return SystemColors.ControlDarkDark;
            }
        }

        private static Color GetForeColor(State.Down stateId)
        {
            switch (stateId)
            {
                case State.Down.DNS_PRESSED:
                    return SystemColors.Control;

                case State.Down.DNS_DISABLED:
                    return SystemColors.ControlDark;

                // case States.Down.DNS_NORMAL:
                // case States.Down.DNS_HOT:
                default:
                    return SystemColors.ControlDarkDark;
            }
        }

        private static Point[] GetArrowPolygon(Rectangle prect, bool down)
        {
            int arrowHeight = prect.Height / 3;
            int arrowWidth = 2 * arrowHeight;
            int arrowLeft = prect.Left + ((prect.Width - arrowWidth) / 2);
            int arrowTop = prect.Top + ((prect.Height - arrowHeight) / 2);
            int x1 = arrowLeft;
            int x2 = arrowLeft + (arrowWidth / 2);
            int x3 = arrowLeft + arrowWidth;
            int y1 = arrowTop;
            int y2 = arrowTop + arrowHeight;
            return down
                ? new[]
                {
                    new Point(x1, y1),
                    new Point(x2, y2),
                    new Point(x3, y1)
                }
                : new[]
                {
                    new Point(x1, y2),
                    new Point(x2, y1),
                    new Point(x3, y2)
                };
        }

        private static void RenderButton(Context ctx, Rectangle prect, Brush backBrush, Color foreColor,
            Point[] arrowPolygon)
        {
            ctx.Graphics.FillRectangle(backBrush, prect);
            using (var pen = new Pen(foreColor, DpiUtil.Scale(2)))
            using (ctx.HighQuality())
            {
                ctx.Graphics.DrawLines(pen, arrowPolygon);
            }
        }

        private enum Parts
        {
            SPNP_UP = 1,
            SPNP_DOWN = 2,
            SPNP_UPHORZ = 3,
            SPNP_DOWNHORZ = 4,
        }

        private static class State
        {
            public enum Up
            {
                UPS_NORMAL = 1,
                UPS_HOT = 2,
                UPS_PRESSED = 3,
                UPS_DISABLED = 4,
            }

            public enum Down
            {
                DNS_NORMAL = 1,
                DNS_HOT = 2,
                DNS_PRESSED = 3,
                DNS_DISABLED = 4,
            }

            public enum UpHorizontal
            {
                UPHZS_NORMAL = 1,
                UPHZS_HOT = 2,
                UPHZS_PRESSED = 3,
                UPHZS_DISABLED = 4,
            }

            public enum DownHorizontal
            {
                DNHZS_NORMAL = 1,
                DNHZS_HOT = 2,
                DNHZS_PRESSED = 3,
                DNHZS_DISABLED = 4,
            }
        }
    }
}
