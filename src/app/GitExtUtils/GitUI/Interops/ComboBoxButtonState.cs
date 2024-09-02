namespace System;

internal static partial class NativeMethods
{
    internal enum ComboBoxButtonState : uint
    {
        STATE_SYSTEM_NONE = 0,
        STATE_SYSTEM_INVISIBLE = 0x00008000,
        STATE_SYSTEM_PRESSED = 0x00000008
    }
}
