namespace System
{
    internal static partial class NativeMethods
    {
        public enum DT : int
        {
            DT_LEFT = 0x0,
            DT_TOP = 0x0,
            DT_CENTER = 0x1,
            DT_RIGHT = 0x2,
            DT_VCENTER = 0x4,
            DT_BOTTOM = 0x8,
            DT_WORDBREAK = 0x10,
            DT_SINGLELINE = 0x20,
            DT_EXPANDTABS = 0x40,
            DT_TABSTOP = 0x80,
            DT_NOCLIP = 0x100,
            DT_EXTERNALLEADING = 0x200,
            DT_CALCRECT = 0x400,
            DT_NOPREFIX = 0x800,
            DT_INTERNAL = 0x1000,
            DT_EDITCONTROL = 0x2000,
            DT_PATH_ELLIPSIS = 0x4000,
            DT_END_ELLIPSIS = 0x8000,
            DT_MODIFYSTRING = 0x10000,
            DT_RTLREADING = 0x20000,
            DT_WORD_ELLIPSIS = 0x40000,
            DT_NOFULLWIDTHCHARBREAK = 0x80000,
            DT_HIDEPREFIX = 0x100000,
            DT_PREFIXONLY = 0x200000,
        }
    }
}
