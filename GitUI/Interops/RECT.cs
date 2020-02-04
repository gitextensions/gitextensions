using System.Drawing;
using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public class RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public static implicit operator Rectangle(RECT rect) =>
                Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
    }
}
