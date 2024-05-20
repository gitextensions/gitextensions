namespace System
{
    internal static partial class NativeMethods
    {
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
