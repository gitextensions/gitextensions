using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace System
{
    public static class StringExtensions
    {

        public static string SkipStr(this string str, string toSkip)
        {
            if (str == null)
                return null;

            int idx;
            idx = str.IndexOf(toSkip);
            if (idx != -1)
                return str.Substring(idx + toSkip.Length);
            else
                return null;
        }

        public static String TakeUntilStr(this string str, String untilStr)
        {
            if (str == null)
                return null;

            int idx;
            idx = str.IndexOf(untilStr);
            if (idx != -1)
                return str.Substring(0, idx);
            else
                return str;
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }


        public static string Combine(this string left, string sep, string right)
        {
            if (left.IsNullOrEmpty())
                return right;
            else if (right.IsNullOrEmpty())
                return left;
            else
                return left + sep + right;
        }

        public static string Quote(this string s)
        {
            return s.Quote("\"");
        }

        public static string Quote(this string s, string quotationMark)
        {
            if (s == null)
                return string.Empty;

            return quotationMark + s + quotationMark;
        }

        /// <summary>
        /// Quotes string if it is not null and not empty
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string QuoteNE(this string s)
        {
            return s.IsNullOrEmpty() ? s : s.Quote("\"");
        }

        /// <summary>
        /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <remarks>
        /// This method is copied from .Net Framework 4.0 and should be deleted after leaving 3.5.
        /// </remarks>
        /// <returns>
        /// true if the value parameter is null or <see cref="string.Empty"/>, or if value consists exclusively of white-space characters.
        /// </returns>
        [Pure]
        public static bool IsNullOrWhiteSpace([CanBeNull] this string value)
        {
            return value == null || value.All(Char.IsWhiteSpace);
        }

        /// <summary>
        /// Determines whether the beginning of this instance matches any of the specified strings.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="starts">array of strings to compare</param>
        /// <returns>true if any starts element matches the beginning of this string; otherwise, false.</returns>
        public static bool StartsWithAny([CanBeNull] this string value, string[] starts)
        {
            return value != null && starts.Any(s => value.StartsWith(s));
        }

        public static string RemoveLines(this string value, Func<string, bool> shouldRemoveLine)
        {
            if (value.IsNullOrEmpty())
                return value;

            if (value[value.Length - 1] == '\n')
                value = value.Substring(0, value.Length - 1);

            StringBuilder sb = new StringBuilder();
            string[] lines = value.Split('\n');
            
            foreach (string line in lines)
                if (!shouldRemoveLine(line))
                    sb.Append(line + '\n');

            return sb.ToString();
        }

    }

    public static class BoolExtensions
    {

        public static string AsForce(this bool force)
        {
            return force ? " -f " : string.Empty;
        }

    }
}
