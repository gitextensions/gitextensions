using System.IO;

namespace System
{
    internal static class Delimiters
    {
        public static readonly char[] Newline = { '\n' };
        public static readonly char[] Space = { ' ' };
        public static readonly char[] Tab = { '\t' };
        public static readonly char[] Null = { '\0' };
        public static readonly char[] TabAndSpace = { '\t', ' ' };
        public static readonly char[] TabAndNewlineAndCarriageReturn = { '\t', '\n', '\r' };
        public static readonly char[] NullAndNewline = { '\0', '\n' };
        public static readonly char[] NewlineAndCarriageReturn = { '\n', '\r' };
        public static readonly char[] ForwardSlash = { '/' };
        public static readonly char[] Colon = { ':' };
        public static readonly char[] Comma = { ',' };
        public static readonly char[] Period = { '.' };
        public static readonly char[] Hash = { '#' };
        public static readonly char[] PathSeparators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
    }
}
