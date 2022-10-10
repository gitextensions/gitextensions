using System.Runtime.InteropServices;

namespace GitUI.CommandsDialogs
{
    /// <summary>
    /// Subclass of <see cref="TabControl"/> whose tab items have no gaps with the border.
    /// </summary>
    public sealed class FullBleedTabControl : TabControl
    {
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x1300 + 40)
            {
                var rc = (RECT)m.GetLParam(typeof(RECT));
                rc.Left -= 3;
                rc.Right += 3;
                rc.Top -= 1;
                rc.Bottom += 3;
                Marshal.StructureToPtr(rc, m.LParam, true);
            }

            base.WndProc(ref m);
        }

        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
