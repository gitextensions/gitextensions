using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ResourceManager;

namespace GitUI
{
    public sealed class MouseWheelRedirector : IMessageFilter
    {
        private static readonly MouseWheelRedirector instance = new();

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
            if (control is null)
            {
                return false;
            }

            if (hwnd == m.HWnd && !IsNonScrollableRichTextBox(control))
            {
                return false;
            }

            while (control is not (null or GitExtensionsControl))
            {
                bool nonScrollableRtbx = IsNonScrollableRichTextBox(control);

                control = control.Parent;
                if (nonScrollableRtbx)
                {
                    hwnd = control.Handle;
                }
            }

            if (control is null)
            {
                return false;
            }

            NativeMethods.SendMessage(hwnd, m.Msg, m.WParam, m.LParam);
            return true;

            static bool IsNonScrollableRichTextBox(Control c) => c is RichTextBox { ScrollBars: RichTextBoxScrollBars.None };
        }

        private static class NativeMethods
        {
            // P/Invoke declarations
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

                public static implicit operator Point(POINT p) => new(p.X, p.Y);
                public static implicit operator POINT(Point p) => new(p.X, p.Y);
            }
        }
    }
}
