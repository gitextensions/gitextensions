using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace GitExtUtils.GitUI
{
    /// <summary>
    /// Utility class related to DPI settings, primarily used for scaling dimensions on high-DPI displays.
    /// </summary>
    public static class DpiUtil
    {
        public static int DpiX { get; }
        public static int DpiY { get; }

        public static float ScaleX { get; }
        public static float ScaleY { get; }

        static DpiUtil()
        {
            using (var hdc = GetDC(IntPtr.Zero))
            {
                try
                {
                    const int LOGPIXELSX = 88;
                    const int LOGPIXELSY = 90;

                    DpiX = GetDeviceCaps(hdc, LOGPIXELSX);
                    DpiY = GetDeviceCaps(hdc, LOGPIXELSY);

                    ScaleX = DpiX / 96.0f;
                    ScaleY = DpiY / 96.0f;
                }
                catch
                {
                    DpiX = 96;
                    DpiY = 96;

                    ScaleX = 1.0f;
                    ScaleY = 1.0f;
                }
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

        /// <summary>
        /// Returns a scaled copy of measurement <paramref name="i"/> which has
        /// equivalent length on screen at the current DPI at the original would
        /// at 96 DPI.
        /// </summary>
        public static int Scale(int i)
        {
            return (int)Math.Round(i * ScaleX);
        }

        /// <summary>
        /// Returns a scaled copy of measurement <paramref name="i"/> which has
        /// equivalent length on screen at the current DPI at the original would
        /// at 96 DPI.
        /// </summary>
        public static float Scale(float i)
        {
            return (float)Math.Round(i * ScaleX);
        }

        public static Point Scale(Point point)
        {
            return new Point(
                (int)(point.X * ScaleX),
                (int)(point.Y * ScaleY));
        }

        /// <summary>
        /// Returns a scaled copy of <paramref name="padding"/> which takes equivalent
        /// screen space at the current DPI as the original would at 96 DPI.
        /// </summary>
        public static Padding Scale(Padding padding)
        {
            return new Padding((int)(padding.Left * ScaleX),
                               (int)(padding.Top * ScaleX),
                               (int)(padding.Right * ScaleX),
                               (int)(padding.Bottom * ScaleX));
        }

        [NotNull]
        public static Image Scale([NotNull] Image image)
        {
            const string dpiScaled = "__DPI_SCALED__";

            if (!IsNonStandard || image.Tag as string == dpiScaled)
            {
                return image;
            }

            var size = Scale(new Size(image.Width, image.Height));
            var bitmap = new Bitmap(size.Width, size.Height);

            using (var g = Graphics.FromImage(bitmap))
            {
                // NearestNeighbor is better for 200% and above
                // http://blogs.msdn.com/b/visualstudio/archive/2014/03/19/improving-high-dpi-support-for-visual-studio-2013.aspx

                g.InterpolationMode = ScaleX >= 2
                    ? InterpolationMode.NearestNeighbor
                    : InterpolationMode.HighQualityBicubic;

                g.DrawImage(image, new Rectangle(Point.Empty, size));
            }

            bitmap.Tag = dpiScaled;

            return bitmap;
        }

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(DeviceContextSafeHandle hdc, int index);

        [DllImport("user32.dll")]
        private static extern DeviceContextSafeHandle GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr deviceContextHandle);

        [UsedImplicitly]
        private sealed class DeviceContextSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            /// <summary>
            /// Called by P/Invoke.
            /// </summary>
            public DeviceContextSafeHandle()
                : base(ownsHandle: true)
            {
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            protected override bool ReleaseHandle()
            {
                ReleaseDC(IntPtr.Zero, handle);
                return true;
            }
        }
    }
}