using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming
{
    internal class ListViewRenderer : ThemeRenderer
    {
        protected override string Clsid { get; } = "Listview";

        public override int RenderTextEx(IntPtr htheme,
            IntPtr hdc,
            int partid, int stateid,
            string psztext, int cchtext,
            NativeMethods.DT dwtextflags,
            IntPtr prect, ref NativeMethods.DTTOPTS poptions)
        {
            switch ((Parts)partid)
            {
                case Parts.LVP_GROUPHEADER:
                {
                    NativeMethods.GetThemeColor(htheme, partid, stateid, poptions.iColorPropId,
                        out var crefText);
                    var color = Color.FromArgb(crefText.R, crefText.G, crefText.B);
                    var adaptedColor = color.AdaptTextColor();

                    // do not render, just modify text color
                    poptions.iColorPropId = 0;
                    poptions.crText = ColorTranslator.ToWin32(adaptedColor);

                    // proceed to default implementation with modified poptions parameter
                    return Unhandled;
                }
            }

            return Unhandled;
        }

        public override int RenderBackground(IntPtr hdc, int partid, int stateid, Rectangle prect,
            NativeMethods.RECTCLS pcliprect)
        {
            using (var ctx = CreateRenderContext(hdc, pcliprect))
            {
                switch ((Parts)partid)
                {
                    case Parts.LVP_GROUPHEADERLINE:
                        return RenderGroupHeaderLine(ctx, prect);

                    case Parts.LVP_EXPANDBUTTON:
                        return RenderExpandButton(ctx, (State.ExpandButton)stateid, prect);

                    case Parts.LVP_COLLAPSEBUTTON:
                        return RenderCollapseButton(ctx, (State.CollapseButton)stateid, prect);

                    case Parts.LVP_LISTITEM:
                        return RenderItemBackground(ctx, (State.ListItem)stateid, prect);

                    default:
                        return Unhandled;
                }
            }
        }

        public override int RenderBackgroundEx(
            IntPtr htheme, IntPtr hdc,
            int partid, int stateid,
            NativeMethods.RECTCLS prect, ref NativeMethods.DTBGOPTS poptions)
        {
            switch ((Parts)partid)
            {
                case Parts.LVP_LISTDETAIL:
                    CheckBoxState state;
                    switch ((State.ListItem)stateid)
                    {
                        case State.ListItem.LISS_NORMAL:
                            state = CheckBoxState.UncheckedNormal;
                            break;

                        case State.ListItem.LISS_SELECTEDNOTFOCUS:
                            state = CheckBoxState.CheckedNormal;
                            break;

                        default:
                            return Unhandled;
                    }

                    using (var ctx = CreateRenderContext(hdc, clip: null))
                    {
                        CheckBoxRenderer.DrawCheckBox(
                            ctx.Graphics,
                            new Point(prect.Left, prect.Top),
                            state);
                    }

                    return Handled;

                default:
                    return Unhandled;
            }
        }

        private static int RenderItemBackground(Context ctx, State.ListItem stateid, Rectangle prect)
        {
            switch (stateid)
            {
                case State.ListItem.LISS_SELECTEDNOTFOCUS:
                    using (var brush = new SolidBrush(Color.FromArgb(64, SystemColors.HotTrack)))
                    {
                        ctx.Graphics.FillRectangle(brush, prect);
                        return Handled;
                    }

                default:
                    return Unhandled;
            }
        }

        private static int RenderGroupHeaderLine(Context ctx, Rectangle prect)
        {
            int y = prect.Top + (prect.Height / 2);
            ctx.Graphics.DrawLine(SystemPens.Highlight, prect.Left, y, prect.Right - 1, y);
            return Handled;
        }

        private static int RenderCollapseButton(Context ctx, State.CollapseButton stateid, Rectangle prect)
        {
            Brush backBrush;
            Color foreColor;
            switch (stateid)
            {
                case State.CollapseButton.LVCB_HOVER:
                    backBrush = SystemBrushes.HotTrack;
                    foreColor = SystemColors.ControlLightLight;
                    break;

                case State.CollapseButton.LVCB_PUSHED:
                    backBrush = SystemBrushes.Highlight;
                    foreColor = SystemColors.ControlLightLight;
                    break;

                // case State.CollapseButton.LVCB_NORMAL:
                default:
                    backBrush = null;
                    foreColor = SystemColors.HotTrack;
                    break;
            }

            RenderArrow(ctx, prect, backBrush, foreColor, false);
            return Handled;
        }

        private static int RenderExpandButton(Context ctx, State.ExpandButton stateid, Rectangle prect)
        {
            Brush backBrush;
            Color foreColor;
            switch (stateid)
            {
                case State.ExpandButton.LVEB_HOVER:
                    backBrush = SystemBrushes.HotTrack;
                    foreColor = SystemColors.ControlLightLight;
                    break;

                case State.ExpandButton.LVEB_PUSHED:
                    backBrush = SystemBrushes.Highlight;
                    foreColor = SystemColors.ControlLightLight;
                    break;

                // case State.ExpandButton.LVEB_NORMAL:
                default:
                    backBrush = null;
                    foreColor = SystemColors.HotTrack;
                    break;
            }

            RenderArrow(ctx, prect, backBrush, foreColor, true);
            return Handled;
        }

        private static void RenderArrow(Context ctx, Rectangle prect, Brush backBrush,
            Color foreColor, bool down)
        {
            int h = prect.Height / 4;
            int w = h * 2;

            int x1 = prect.Left + ((prect.Width - w) / 2);
            int x2 = x1 + (w / 2);
            int x3 = x1 + w;

            int y1 = prect.Top + ((prect.Height - h) / 2);
            int y2 = y1 + h;

            var arrowPoints = down
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

            using (ctx.HighQuality())
            {
                if (backBrush != null)
                {
                    ctx.Graphics.FillEllipse(backBrush, prect.Inclusive());
                }

                using (var forePen = new Pen(foreColor, DpiUtil.Scale(2)))
                {
                    ctx.Graphics.DrawLines(forePen, arrowPoints);
                }
            }
        }

        private enum Parts
        {
            LVP_LISTITEM = 1,
            LVP_LISTGROUP = 2,
            LVP_LISTDETAIL = 3,
            LVP_LISTSORTEDDETAIL = 4,
            LVP_EMPTYTEXT = 5,
            LVP_GROUPHEADER = 6,
            LVP_GROUPHEADERLINE = 7,
            LVP_EXPANDBUTTON = 8,
            LVP_COLLAPSEBUTTON = 9,
            LVP_COLUMNDETAIL = 10
        }

        private static class State
        {
            public enum CollapseButton
            {
                LVCB_NORMAL = 1,
                LVCB_HOVER = 2,
                LVCB_PUSHED = 3,
            }

            public enum ExpandButton
            {
                LVEB_NORMAL = 1,
                LVEB_HOVER = 2,
                LVEB_PUSHED = 3,
            }

            public enum GroupHeader
            {
                LVGH_OPEN = 1,
                LVGH_OPENHOT = 2,
                LVGH_OPENSELECTED = 3,
                LVGH_OPENSELECTEDHOT = 4,
                LVGH_OPENSELECTEDNOTFOCUSED = 5,
                LVGH_OPENSELECTEDNOTFOCUSEDHOT = 6,
                LVGH_OPENMIXEDSELECTION = 7,
                LVGH_OPENMIXEDSELECTIONHOT = 8,
                LVGH_CLOSE = 9,
                LVGH_CLOSEHOT = 10,
                LVGH_CLOSESELECTED = 11,
                LVGH_CLOSESELECTEDHOT = 12,
                LVGH_CLOSESELECTEDNOTFOCUSED = 13,
                LVGH_CLOSESELECTEDNOTFOCUSEDHOT = 14,
                LVGH_CLOSEMIXEDSELECTION = 15,
                LVGH_CLOSEMIXEDSELECTIONHOT = 16,
            }

            public enum GroupHeaderLine
            {
                LVGHL_OPEN = 1,
                LVGHL_OPENHOT = 2,
                LVGHL_OPENSELECTED = 3,
                LVGHL_OPENSELECTEDHOT = 4,
                LVGHL_OPENSELECTEDNOTFOCUSED = 5,
                LVGHL_OPENSELECTEDNOTFOCUSEDHOT = 6,
                LVGHL_OPENMIXEDSELECTION = 7,
                LVGHL_OPENMIXEDSELECTIONHOT = 8,
                LVGHL_CLOSE = 9,
                LVGHL_CLOSEHOT = 10,
                LVGHL_CLOSESELECTED = 11,
                LVGHL_CLOSESELECTEDHOT = 12,
                LVGHL_CLOSESELECTEDNOTFOCUSED = 13,
                LVGHL_CLOSESELECTEDNOTFOCUSEDHOT = 14,
                LVGHL_CLOSEMIXEDSELECTION = 15,
                LVGHL_CLOSEMIXEDSELECTIONHOT = 16,
            }

            public enum ListItem
            {
                LISS_NORMAL = 1,
                LISS_HOT = 2,
                LISS_SELECTED = 3,
                LISS_DISABLED = 4,
                LISS_SELECTEDNOTFOCUS = 5,
                LISS_HOTSELECTED = 6,
            }
        }
    }
}
