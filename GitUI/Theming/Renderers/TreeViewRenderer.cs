using System;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using GitExtUtils.GitUI;

namespace GitUI.Theming
{
    internal class TreeViewRenderer : ThemeRenderer
    {
        protected override string Clsid { get; } = "Treeview";

        public override int RenderBackground(IntPtr hdc, int partid, int stateid, Rectangle prect,
            NativeMethods.RECTCLS pcliprect)
        {
            using (var ctx = CreateRenderContext(hdc, pcliprect))
            {
                switch ((Parts)partid)
                {
                    case Parts.TVP_GLYPH:
                        return RenderGlyph(ctx, (State.Glyph)stateid, prect);

                    case Parts.TVP_HOTGLYPH:
                        return RenderHotTrackedGlyph(ctx, (State.HotGlyph)stateid, prect);

                    case Parts.TVP_TREEITEM:
                        return RenderItemBackground(ctx, (State.Item)stateid, prect);

                    default:
                        return Unhandled;
                }
            }
        }

        public override int RenderTextEx(IntPtr htheme, IntPtr hdc, int partid, int stateid,
            string psztext, int cchtext,
            NativeMethods.DT dwtextflags, IntPtr prect,
            ref NativeMethods.DTTOPTS poptions)
        {
            switch ((Parts)partid)
            {
                case Parts.TVP_TREEITEM:
                {
                    Color foreColor;
                    switch ((State.Item)stateid)
                    {
                        case State.Item.TREIS_DISABLED:
                            foreColor = SystemColors.GrayText;
                            break;

                        case State.Item.TREIS_SELECTED:
                        case State.Item.TREIS_HOTSELECTED:
                        case State.Item.TREIS_SELECTEDNOTFOCUS:
                            foreColor = SystemColors.WindowText;
                            break;

                        case State.Item.TREIS_NORMAL:
                        case State.Item.TREIS_HOT:
                            foreColor = SystemColors.WindowText;
                            break;
                        default:
                            return Unhandled;
                    }

                    // do not render, just modify text color
                    poptions.dwFlags |= NativeMethods.DTT.TextColor;
                    poptions.iColorPropId = 0;
                    poptions.crText = ColorTranslator.ToWin32(foreColor);
                    break;
                }
            }

            return Unhandled;
        }

        public override int GetThemeColor(int ipartid, int istateid, int ipropid, out int pcolor)
        {
            switch ((Parts)ipartid)
            {
                case Parts.None:
                    if (istateid == 0)
                    {
                        switch ((ColorProperty)ipropid)
                        {
                            case ColorProperty.FillColor:
                                pcolor = ColorTranslator.ToWin32(SystemColors.Window);
                                return Handled;
                        }
                    }

                    break;

                case Parts.TVP_BRANCH:
                    if (istateid == 0)
                    {
                        switch ((ThemeProperty)ipropid)
                        {
                            case ThemeProperty.TMT_COLOR:
                                pcolor = ColorTranslator.ToWin32(SystemColors.ControlDark);
                                return Handled;
                        }
                    }

                    break;
            }

            pcolor = 0;
            return Unhandled;
        }

        private static int RenderGlyph(Context ctx, State.Glyph stateid, Rectangle prect)
        {
            switch (stateid)
            {
                case State.Glyph.GLPS_CLOSED:
                    RenderClosed(ctx, prect, SystemColors.ControlDarkDark);
                    return Handled;

                case State.Glyph.GLPS_OPENED:
                    RenderOpened(ctx, prect, SystemColors.ControlDarkDark);
                    return Handled;

                default:
                    return Unhandled;
            }
        }

        private static int RenderHotTrackedGlyph(Context ctx, State.HotGlyph stateid,
            Rectangle prect)
        {
            switch (stateid)
            {
                case State.HotGlyph.HGLPS_CLOSED:
                    RenderClosed(ctx, prect, SystemColors.HotTrack);
                    return Handled;

                case State.HotGlyph.HGLPS_OPENED:
                    RenderOpened(ctx, prect, SystemColors.HotTrack);
                    return Handled;

                default:
                    return Unhandled;
            }
        }

        private static int RenderItemBackground(Context ctx, State.Item stateid, Rectangle prect)
        {
            switch (stateid)
            {
                case State.Item.TREIS_SELECTEDNOTFOCUS:
                    using (var brush = new SolidBrush(Color.FromArgb(64, SystemColors.HotTrack)))
                    {
                        ctx.Graphics.FillRectangle(brush, prect);
                        return Handled;
                    }

                default:
                    return Unhandled;
            }
        }

        private static void RenderClosed(Context ctx, Rectangle prect, Color foreColor)
        {
            int w = prect.Width / 4;
            int h = w * 2;

            int y1 = prect.Top + ((prect.Height - h) / 2);
            int y2 = y1 + (h / 2);
            int y3 = y1 + h;

            int x1 = prect.Left + ((prect.Width - w) / 2);
            int x2 = x1 + w;

            var arrowPoints = new[]
            {
                new Point(x1, y1),
                new Point(x2, y2),
                new Point(x1, y3)
            };

            using (ctx.HighQuality())
            using (var forePen = new Pen(foreColor, DpiUtil.Scale(2)))
            {
                ctx.Graphics.DrawLines(forePen, arrowPoints);
            }
        }

        private static void RenderOpened(Context ctx, Rectangle prect, Color foreColor)
        {
            int h = prect.Height / 4;
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

            using (ctx.HighQuality())
            using (var forePen = new Pen(foreColor, DpiUtil.Scale(2)))
            {
                ctx.Graphics.DrawLines(forePen, arrowPoints);
            }
        }

        private enum Parts
        {
            None = 0,
            TVP_TREEITEM = 1,
            TVP_GLYPH = 2,
            TVP_BRANCH = 3,
            TVP_HOTGLYPH = 4,
        }

        private class State
        {
            public enum Item
            {
                TREIS_NORMAL = 1,
                TREIS_HOT = 2,
                TREIS_SELECTED = 3,
                TREIS_DISABLED = 4,
                TREIS_SELECTEDNOTFOCUS = 5,
                TREIS_HOTSELECTED = 6,
            }

            public enum Glyph
            {
                GLPS_CLOSED = 1,
                GLPS_OPENED = 2,
            }

            public enum HotGlyph
            {
                HGLPS_CLOSED = 1,
                HGLPS_OPENED = 2,
            }
        }
    }
}
