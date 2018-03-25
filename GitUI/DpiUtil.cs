using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GitUI
{
    /// <summary>
    /// Utility class related to DPI settings, primarily used for scaling dimensions on high-DPI displays.
    /// </summary>
    public static class DpiUtil
    {
        public static int DpiX { get; }
        public static int DpiY { get; }

        public static double ScaleX { get; }
        public static double ScaleY { get; }

        static DpiUtil()
        {
            var hdc = GetDC(IntPtr.Zero);

            try
            {
                const int LOGPIXELSX = 88;
                const int LOGPIXELSY = 90;

                DpiX = GetDeviceCaps(hdc, LOGPIXELSX);
                DpiY = GetDeviceCaps(hdc, LOGPIXELSY);

                ScaleX = DpiX / 96.0;
                ScaleY = DpiY / 96.0;
            }
            catch
            {
                DpiX = 96;
                DpiY = 96;

                ScaleX = 1.0;
                ScaleY = 1.0;
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, hdc);
            }
        }

        /// <summary>
        /// Gets whether the current pixel density is not 96 DPI.
        /// </summary>
        public static bool IsNonStandard => DpiX != 96 || DpiY != 96;

        /// <summary>
        /// Returns a scaled copy of <paramref name="size"/> which takes equivalent
        /// screen space at the current DPI as the original would at 96 DPI.
        /// </summary>
        public static Size Scale(Size size)
        {
            Scale(ref size);
            return size;
        }

        /// <summary>
        /// Modifies <paramref name="size"/> in place so that it takes equivalent screen
        /// space at the current DPI as the original value would at 96 DPI.
        /// </summary>
        public static void Scale(ref Size size)
        {
            size.Width = (int)(size.Width * ScaleX);
            size.Height = (int)(size.Height * ScaleY);
        }

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int index);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr deviceContextHandle);
    }
}