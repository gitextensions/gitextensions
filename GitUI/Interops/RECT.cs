using System.Drawing;
using System.Runtime.InteropServices;

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

namespace System
{
    internal static partial class NativeMethods
    {
        /// <summary>
        /// Theming interop requires RECT to be class.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class RECTCLS
        {
#pragma warning disable 649
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
#pragma warning restore 649

            public static implicit operator Rectangle(RECTCLS r)
                => Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
        }
    }
}
