using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [DllImport(Libraries.WinInet)]
        public static extern bool InternetGetConnectedState(out int description, int reservedValue);
    }
}
