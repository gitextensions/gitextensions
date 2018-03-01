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
                        Application.AddMessageFilter(instance);
                    else
                        Application.RemoveMessageFilter(instance);
                }
            }
        }

        private IntPtr _previousHWnd = IntPtr.Zero;
        private bool _GEControl;

        public bool PreFilterMessage(ref Message m)
        {
            const int WM_MOUSEWHEEL = 0x20a;
            const int WM_MOUSEHWHEEL = 0x20e;
            if (m.Msg == WM_MOUSEWHEEL || m.Msg == WM_MOUSEHWHEEL)
            {
                // WM_MOUSEWHEEL, find the control at screen position m.LParam
                Point pos = new Point(m.LParam.ToInt32());
                IntPtr hWnd = NativeMethods.WindowFromPoint(pos);
                if (hWnd != IntPtr.Zero && hWnd != m.HWnd && Control.FromHandle(hWnd) != null)
                {
                    if (_previousHWnd != hWnd)
                    {
                        Control control = Control.FromHandle(hWnd);
                        while (control != null && !(control is GitExtensionsControl))
                            control = control.Parent;
                        _previousHWnd = hWnd;
                        _GEControl = control != null;
                    }
                    if (_GEControl)
                    {
                        NativeMethods.SendMessage(hWnd, m.Msg, m.WParam, m.LParam);
                        return true;
                    }
                }
            }
            return false;
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
            public struct POINT
            {
                public int X;
                public int Y;

                public POINT(int x, int y)
                {
                    X = x;
                    Y = y;
                }

                public static implicit operator System.Drawing.Point(POINT p)
                {
                    return new System.Drawing.Point(p.X, p.Y);
                }

                public static implicit operator POINT(System.Drawing.Point p)
                {
                    return new POINT(p.X, p.Y);
                }
            }
        }
    }
}
