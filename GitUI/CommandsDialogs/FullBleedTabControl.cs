using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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
                rc.Right += 1;
                rc.Top -= 1;
                rc.Bottom += 2;
                Marshal.StructureToPtr(rc, m.LParam, true);
            }

            base.WndProc(ref m);
        }

        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}