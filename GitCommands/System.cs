using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace System
{
    public static class StringExtensions
    {
        /// <summary>'\n'</summary>
        static readonly char[] NewLineSeparator = new char[] { '\n' };

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

        public static string CommonPrefix(this string s, string other)
        {
            if (s.IsNullOrEmpty() || other.IsNullOrEmpty())
                return string.Empty;

            int prefixLength = 0;

            foreach (char c in other)
            {
                if (s.Length <= prefixLength || s[prefixLength] != c)
                    return s.Substring(0, prefixLength);

                prefixLength++;
            }

            return s;
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

        /// <summary>
        /// Quotes string if it is not null
        /// </summary>
        public static string Quote(this string s)
        {
            return s.Quote("\"");
        }

        /// <summary>
        /// Quotes this string with the specified <paramref name="quotationMark"/>
        /// </summary>
        public static string Quote(this string s, string quotationMark)
        {
            if (s == null)
                return string.Empty;

            return quotationMark + s + quotationMark;
        }

        /// <summary>
        /// Quotes this string if it is not null and not empty
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string QuoteNE(this string s)
        {
            return s.IsNullOrEmpty() ? s : s.Quote("\"");
        }

        /// <summary>
        /// Adds parentheses if string is not null and not empty
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string AddParenthesesNE(this string s)
        {
            return s.IsNullOrEmpty() ? s : "(" + s + ")";
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
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>Indicates whether the specified string is neither null, nor empty, nor has only whitespace.</summary>
        public static bool IsNotNullOrWhitespace([CanBeNull] this string value)
        {
            return !value.IsNullOrWhiteSpace();
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

        /// <summary>Split a string, removing empty entries, then trim whitespace.</summary>
        public static IEnumerable<string> SplitThenTrim(this string value, params string[] separator)
        {
            return value
                .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim());
        }

        /// <summary>Split a string, removing empty entries, then trim whitespace.</summary>
        public static IEnumerable<string> SplitThenTrim(this string value, params char[] separator)
        {
            return value
                .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim());
        }

        /// <summary>Split a string, delimited by line-breaks, excluding empty entries.</summary>
        public static string[] SplitLines(this string value)
        {
            return value.Split(NewLineSeparator, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>Split a string, delimited by line-breaks, excluding empty entries; then trim whitespace.</summary>
        public static IEnumerable<string> SplitLinesThenTrim(this string value)
        {
            return value.SplitThenTrim(NewLineSeparator);
        }

        /// <summary>Gets the text after the last separator.
        /// If NO separator OR ends with separator, returns the original value.</summary>
        public static string SubstringAfterLastSafe(this string value, string separator)
        {// ex: "origin/master" -> "master"
            if (value.EndsWith(separator) || !value.Contains(separator))
            {// "origin/master/" OR "master" -> return original
                return value;
            }
            return value.Substring(1 + value.LastIndexOf(separator, StringComparison.InvariantCultureIgnoreCase));
        }
        public static string SubstringAfterFirst(this string value, string separator)
        {
            return value.Substring(1 + value.IndexOf(separator, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Shortens this string, that it will be no longer than the specified <paramref name="maxLength"/>.
        /// If this string is longer than the specified <paramref name="maxLength"/>, it'll be truncated to the length of <paramref name="maxLength"/>-3
        /// and the "..." will be appended to the end of the resulting string.
        /// </summary>
        public static string ShortenTo(this string str, int maxLength)
        {
            if (str.IsNullOrEmpty())
                return string.Empty;

            if (str.Length <= maxLength)
                return str;
            else
                return str.Substring(0, maxLength - 3) + "...";
        }

    }

    public static class BoolExtensions
    {

        /// <summary>
        /// Translates this bool value to the git command line force flag
        /// </summary>
        public static string AsForce(this bool force)
        {
            return force ? " -f " : string.Empty;
        }

    }
}
