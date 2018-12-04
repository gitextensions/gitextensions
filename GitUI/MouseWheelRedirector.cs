using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ResourceManager;

namespace GitUI
{
    public sealed class MouseWheelRedirector : IMessageFilter
    {
        private static readonly MouseWheelRedirector instance = new MouseWheelRedirector();

        private MouseWheelRedirector()
        {
        }

        private bool _active;
        public static bool Active
        {
            get { return instance._active; }
            set
            {
                if (instance._active != value)
                {
                    instance._active = value;
                    if (instance._active)
                    {
                        Application.AddMessageFilter(instance);
                    }
                    else
                    {
                        Application.RemoveMessageFilter(instance);
                    }
                }
            }
        }

        public bool PreFilterMessage(ref Message m)
        {
            const int WM_MOUSEWHEEL = 0x20a;
            const int WM_MOUSEHWHEEL = 0x20e;
            if (m.Msg != WM_MOUSEWHEEL && m.Msg != WM_MOUSEHWHEEL)
            {
                return false;
            }

            // WM_MOUSEWHEEL, find the control at screen position m.LParam
            IntPtr hwnd = NativeMethods.WindowFromPoint(m.LParam.ToPoint());
            if (hwnd == IntPtr.Zero)
            {
                return false;
            }

            Control control = Control.FromHandle(hwnd);
            if (control == null)
            {
                return false;
            }

            if (hwnd == m.HWnd && !isNonScrollableRichTextBox(control))
            {
                return false;
            }

            while (control != null && !(control is GitExtensionsControl))
            {
                bool nonScrollableRtbx = isNonScrollableRichTextBox(control);

                control = control.Parent;
                if (nonScrollableRtbx)
                {
                    hwnd = control.Handle;
                }
            }

            if (control == null)
            {
                return false;
            }

            NativeMethods.SendMessage(hwnd, m.Msg, m.WParam, m.LParam);
            return true;

            bool isNonScrollableRichTextBox(Control c) =>
                c is RichTextBox rtb && rtb.ScrollBars == RichTextBoxScrollBars.None;
        }

        private static class NativeMethods
        {
            // P/Invoke declarations
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "0", Justification = "https://social.msdn.microsoft.com/Forums/en-US/180fcf90-ff90-45b2-839f-438eb17f2f07/is-this-a-bug-in-vs-code-analysis?forum=vstscode")]
            [DllImport("user32.dll")]
            public static extern IntPtr WindowFromPoint(POINT pt);

            [DllImport("user32.dll")]
            public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

            [StructLayout(LayoutKind.Sequential)]
            public readonly struct POINT
            {
                public readonly int X;
                public readonly int Y;

                public POINT(int x, int y)
                {
                    X = x;
                    Y = y;
                }

                public static implicit operator Point(POINT p) => new Point(p.X, p.Y);
                public static implicit operator POINT(Point p) => new POINT(p.X, p.Y);
            }
        }
    }
}
