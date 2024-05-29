using System.Runtime.InteropServices;

namespace System
{
    internal static partial class NativeMethods
    {
        [LibraryImport(Libraries.User32, StringMarshalling = StringMarshalling.Utf16)]
        public static partial uint RegisterWindowMessageW(string name);
    }
}
