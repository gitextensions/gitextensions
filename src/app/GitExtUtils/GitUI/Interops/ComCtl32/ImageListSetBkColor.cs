using System.Runtime.InteropServices;

namespace System;

internal static partial class NativeMethods
{
    internal const int ComCtl32CLRNone = unchecked((int)0xFFFFFFFF);

    [DllImport("comctl32.dll", EntryPoint = "ImageList_SetBkColor")]
    internal static extern int ImageListSetBkColor(IntPtr himl, int clrBk);
}
