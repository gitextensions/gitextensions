using System.Runtime.InteropServices;
using System.Text;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.Kernel32, ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern int GetLongPathNameW(string lpszShortPath, StringBuilder lpszLongPath, int cchBuffer);
    }
}
