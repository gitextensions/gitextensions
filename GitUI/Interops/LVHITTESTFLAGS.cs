namespace System
{
    internal static partial class NativeMethods
    {
        /// <summary>
        /// see http://msdn.microsoft.com/en-us/library/bb774754%28v=VS.85%29.aspx
        /// </summary>
        [Flags]
        public enum LVHITTESTFLAGS : uint
        {
            LVHT_NOWHERE = 0x00000001,
            LVHT_ONITEMICON = 0x00000002,
            LVHT_ONITEMLABEL = 0x00000004,
            LVHT_ONITEMSTATEICON = 0x00000008,
            LVHT_ONITEM = LVHT_ONITEMICON | LVHT_ONITEMLABEL | LVHT_ONITEMSTATEICON,
            LVHT_ABOVE = 0x00000008,
            LVHT_BELOW = 0x00000010,
            LVHT_TORIGHT = 0x00000020,
            LVHT_TOLEFT = 0x00000040,

            // Vista/Win7+ only
            LVHT_EX_GROUP_HEADER = 0x10000000,
            LVHT_EX_GROUP_FOOTER = 0x20000000,
            LVHT_EX_GROUP_COLLAPSE = 0x40000000,
            LVHT_EX_GROUP_BACKGROUND = 0x80000000,
            LVHT_EX_GROUP_STATEICON = 0x01000000,
            LVHT_EX_GROUP_SUBSETLINK = 0x02000000,
        }
    }
}
