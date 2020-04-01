namespace System
{
    internal static partial class NativeMethods
    {
        /// <summary>
        /// see http://msdn.microsoft.com/en-us/library/bb774754%28v=VS.85%29.aspx
        /// </summary>
        public struct LVHITTESTINFO
        {
            public POINT pt;
            public LVHITTESTFLAGS flags;
            public int iItem;
            public int iSubItem;

            // Vista/Win7+
            public int iGroup;
        }
    }
}
