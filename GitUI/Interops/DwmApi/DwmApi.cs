using System;
using System.Runtime.InteropServices;

namespace GitUI.Interops.DwmApi
{
    /// <summary>
    /// Allow to set dark title bar on Win10. From: https://stackoverflow.com/questions/57124243/winforms-dark-title-bar-on-windows-10/62811758#62811758.
    /// </summary>
    internal static class DwmApi
    {
        [DllImport("dwmapi.dll", ExactSpelling = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, uint attr, ref int attrValue, int attrSize);

        // Non-documented Windows constants
        private const uint DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const uint DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private static readonly bool _isSupported = IsWindows10BuildOrGreater(17763);
        private static readonly uint _dwmAttribute = IsWindows10BuildOrGreater(18985)
            ? DWMWA_USE_IMMERSIVE_DARK_MODE
            : DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;

        internal static bool UseImmersiveDarkMode(IntPtr hwnd, bool enabled)
        {
            if (!_isSupported || hwnd == IntPtr.Zero)
            {
                return false;
            }

            int useImmersiveDarkMode = enabled ? 1 : 0;

            try
            {
                return DwmSetWindowAttribute(hwnd, _dwmAttribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool IsWindows10BuildOrGreater(int build) => Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
    }
}
