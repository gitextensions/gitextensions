using System.Drawing;
using Windows.Win32.Foundation;

namespace GitUI.Theming
{
    internal static class RectangleExtension
    {
        public static Rectangle Inclusive(this Rectangle rect) =>
            Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right - 1, rect.Bottom - 1);

        public static RECT Inclusive(this RECT rect) =>
            new() { left = rect.left, top = rect.top, right = rect.right - 1, bottom = rect.bottom - 1 };
    }
}
