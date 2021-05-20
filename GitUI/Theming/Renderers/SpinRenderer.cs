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
            using var ctx = CreateRenderContext(hdc, pcliprect);
            return (Parts)partId switch
            {
                Parts.SPNP_UP => RenderUpButton(ctx, stateId, prect),
                Parts.SPNP_DOWN => RenderDownButton(ctx, stateId, prect),
                _ => Unhandled
            };
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
            return stateId switch
            {
                State.Up.UPS_HOT => SystemBrushes.ControlDark,
                State.Up.UPS_PRESSED => SystemBrushes.ControlDarkDark,
                _ => SystemBrushes.Control
            };
        }

        private static Brush GetBackBrush(State.Down stateId)
        {
            return stateId switch
            {
                State.Down.DNS_HOT => SystemBrushes.ControlDark,
                State.Down.DNS_PRESSED => SystemBrushes.ControlDarkDark,
                _ => SystemBrushes.Control
            };
        }

        private static Color GetForeColor(State.Up stateId)
        {
            return stateId switch
            {
                State.Up.UPS_PRESSED => SystemColors.Control,
                State.Up.UPS_DISABLED => SystemColors.ControlDark,
                _ => SystemColors.ControlDarkDark
            };
        }

        private static Color GetForeColor(State.Down stateId)
        {
            return stateId switch
            {
                State.Down.DNS_PRESSED => SystemColors.Control,
                State.Down.DNS_DISABLED => SystemColors.ControlDark,
                _ => SystemColors.ControlDarkDark
            };
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
            using (ctx.HighQuality())
            {
                using Pen pen = new(foreColor, DpiUtil.Scale(2));
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
