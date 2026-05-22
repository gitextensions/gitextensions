using static System.NativeMethods;

namespace GitUI.UserControls;

public class NativeTreeView : TreeView
{
    private const int MK_SHIFT = 0x0004;

    public NativeTreeView()
    {
        DoubleBuffered = true;
    }

    protected override void CreateHandle()
    {
        base.CreateHandle();
        if (!Application.IsDarkModeEnabled)
        {
            // explorer style selection painting in left panel
            // Not needed in dark mode, this is the same for "DarkMode_Explorer"
            NativeMethods.SetWindowTheme(Handle, "explorer", null);
        }
    }

    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case WM_MOUSEWHEEL when (m.WParam.ToInt64() & MK_SHIFT) != 0:
            {
                int delta = (short)(m.WParam.ToInt64() >> 16);
                if (delta != 0)
                {
                    // Shift+wheel-up scrolls left, Shift+wheel-down scrolls right
                    ScrollHorizontally(-delta);
                }

                return;
            }

            case WM_MOUSEHWHEEL:
            {
                int delta = (short)(m.WParam.ToInt64() >> 16);
                if (delta != 0)
                {
                    // Positive delta = tilt right = scroll right
                    ScrollHorizontally(delta);
                }

                return;
            }
        }

        base.WndProc(ref m);

        return;

        void ScrollHorizontally(int delta)
        {
            SBH direction;
            int count;

            int scrollLines = SystemInformation.MouseWheelScrollLines;
            if (scrollLines == -1)
            {
                direction = delta > 0 ? SBH.PAGERIGHT : SBH.PAGELEFT;
                count = 1;
            }
            else
            {
                direction = delta > 0 ? SBH.LINERIGHT : SBH.LINELEFT;
                count = Math.Max(1, Math.Abs(delta) * scrollLines * 3 / SystemInformation.MouseWheelScrollDelta);
            }

            for (int i = 0; i < count; i++)
            {
                Message hscrollMsg = Message.Create(Handle, WM_HSCROLL, (IntPtr)direction, IntPtr.Zero);
                DefWndProc(ref hscrollMsg);
            }
        }
    }
}
