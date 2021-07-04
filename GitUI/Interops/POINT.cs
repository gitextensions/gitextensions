namespace Windows.Win32.Foundation
{
    partial struct POINT
    {
        public static implicit operator System.Drawing.Point(POINT p) => new(p.x, p.y);
        public static implicit operator POINT(System.Drawing.Point p) => new() { x = p.X, y = p.Y };
    }
}
