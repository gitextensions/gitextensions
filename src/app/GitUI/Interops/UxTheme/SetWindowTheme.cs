using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [LibraryImport(Libraries.UxTheme)]
        private static unsafe partial int SetWindowTheme(IntPtr hWnd, char* pszSubAppName, char* pszSubIdList);

        public static unsafe int SetWindowTheme(IntPtr hWnd, string subAppName, string? subIdList)
        {
            fixed (char* pszSubAppName = subAppName)
            {
                fixed (char* pszSubIdList = subIdList)
                {
                    return SetWindowTheme(hWnd, pszSubAppName, pszSubIdList);
                }
            }
        }
    }
}
