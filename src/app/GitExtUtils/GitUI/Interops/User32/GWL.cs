namespace System;

internal static partial class NativeMethods
{
    /// <summary>
    ///  Window long values for <see cref="GetWindowLongPtrW(nint, GWL)"/>.
    /// </summary>
    internal enum GWL : int
    {
        WNDPROC = -4,
        HWNDPARENT = -8,
        STYLE = -16,
        EXSTYLE = -20,
        ID = -12,
    }
}
