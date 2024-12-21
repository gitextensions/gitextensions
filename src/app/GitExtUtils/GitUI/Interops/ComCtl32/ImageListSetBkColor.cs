using System.Runtime.InteropServices;

namespace System;

internal static partial class NativeMethods
{
    internal const int ComCtl32CLRNone = unchecked((int)0xFFFFFFFF);

    [LibraryImport("comctl32.dll", EntryPoint = "ImageList_SetBkColor")]
    internal static partial int ImageListSetBkColor(IntPtr himl, int clrBk);
}
