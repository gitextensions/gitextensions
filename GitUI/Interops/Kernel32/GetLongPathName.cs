using System.Runtime.InteropServices;
using System.Text;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetLongPathName(string lpszShortPath, StringBuilder lpszLongPath, int cchBuffer);
    }
}
