namespace System
{
    internal static partial class NativeMethods
    {
        [Flags]
        public enum LVGF : uint
        {
            NONE = 0x00000000,
            HEADER = 0x00000001,
            FOOTER = 0x00000002,
            STATE = 0x00000004,
            ALIGN = 0x00000008,
            GROUPID = 0x00000010,
            SUBTITLE = 0x00000100,
            TASK = 0x00000200,
            DESCRIPTIONTOP = 0x00000400,
            DESCRIPTIONBOTTOM = 0x00000800,
            TITLEIMAGE = 0x00001000,
            EXTENDEDIMAGE = 0x00002000,
            ITEMS = 0x00004000,
            SUBSET = 0x00008000,
            SUBSETITEMS = 0x00010000,
        }
    }
}
