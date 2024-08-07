using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [LibraryImport(Libraries.WinInet)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool InternetGetConnectedState(out int description, int reservedValue);
    }
}
