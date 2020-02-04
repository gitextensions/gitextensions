using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.UxTheme, ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static unsafe extern int SetWindowTheme(IntPtr hWnd, char* pszSubAppName, char* pszSubIdList);

        public static unsafe int SetWindowTheme(IntPtr hWnd, string subAppName, string subIdList)
        {
            fixed (char* pszSubAppName = subAppName)
            fixed (char* pszSubIdList = subIdList)
            {
                return SetWindowTheme(hWnd, pszSubAppName, pszSubIdList);
            }
        }
    }
}
