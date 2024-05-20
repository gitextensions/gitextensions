using static System.Interop;

namespace System
{
    internal partial class Interop
    {
        /// <summary>
        ///  Blittable version of Windows BOOL type. It is convenient in situations where
        ///  manual marshalling is required, or to avoid overhead of regular bool marshalling.
        /// </summary>
        /// <remarks>
        ///  Some Windows APIs return arbitrary integer values although the return type is defined
        ///  as BOOL. It is best to never compare BOOL to TRUE. Always use bResult != BOOL.FALSE
        ///  or bResult == BOOL.FALSE .
        /// </remarks>
        public enum BOOL : int
        {
            FALSE = 0,
            TRUE = 1,
        }
    }

    internal static class BoolExtensions
    {
        public static bool IsTrue(this BOOL b) => b != BOOL.FALSE;
        public static bool IsFalse(this BOOL b) => b == BOOL.FALSE;
        public static BOOL ToBOOL(this bool b) => b ? BOOL.TRUE : BOOL.FALSE;
    }
}
