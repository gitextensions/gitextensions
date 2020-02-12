using System.Drawing;
using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public RECT(Rectangle r)
            {
                left = r.Left;
                top = r.Top;
                right = r.Right;
                bottom = r.Bottom;
            }

            public static implicit operator Rectangle(RECT r)
                => Rectangle.FromLTRB(r.left, r.top, r.right, r.bottom);

            public static implicit operator RECT(Rectangle r)
                => new RECT(r);

            public Size Size
                => new Size(right - left, bottom - top);
        }

        /// <summary>
        /// Theming interop requires RECT to be class
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
