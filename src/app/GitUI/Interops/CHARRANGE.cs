namespace System
{
    internal static partial class NativeMethods
    {
        public struct CHARRANGE
        {
            public int cpMin;         // First character of range (0 for start of doc)
            public int cpMax;         // Last character of range (-1 for end of doc)
        }
    }
}
