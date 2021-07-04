using System.Drawing;

namespace Windows.Win32.Foundation
{
    partial struct RECT
    {
        public static implicit operator Rectangle(RECT r)
            => Rectangle.FromLTRB(r.left, r.top, r.right, r.bottom);

        public static implicit operator RECT(Rectangle r)
            => new()
            {
                left = r.Left,
                top = r.Top,
                right = r.Right,
                bottom = r.Bottom,
            };

        public Size Size
            => new(right - left, bottom - top);
    }
}
