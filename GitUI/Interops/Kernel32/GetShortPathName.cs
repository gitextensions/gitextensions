using System.Runtime.InteropServices;
using System.Text;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.Kernel32, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, int cchBuffer);
    }
}
