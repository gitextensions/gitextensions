using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace System
{
    public static class StringExtensions
    {
        private static readonly char[] _newLine = { '\n' };
        private static readonly char[] _space = { ' ' };

        // NOTE ordinal string comparison is the default as most string comparison in GE is against static ASCII output from git.exe

        /// <summary>
        /// Returns <paramref name="str"/> without <paramref name="prefix"/>.
        /// If <paramref name="prefix"/> is not found, <paramref name="str"/> is returned unchanged.
        /// </summary>
        [Pure]
        [NotNull]
        public static string RemovePrefix([NotNull] this string str, [NotNull] string prefix, StringComparison comparison = StringComparison.Ordinal)
        {
            return str.StartsWith(prefix, comparison)
                ? str.Substring(prefix.Length)
                : str;
        }

        /// <summary>
        /// Returns <paramref name="str"/> without <paramref name="suffix"/>.
        /// If <paramref name="suffix"/> is not found, <paramref name="str"/> is returned unchanged.
        /// </summary>
        [Pure]
        [NotNull]
        public static string RemoveSuffix([NotNull] this string str, [NotNull] string suffix, StringComparison comparison = StringComparison.Ordinal)
        {
            return str.EndsWith(suffix, comparison)
                ? str.Substring(0, str.Length - suffix.Length)
                : str;
        }

        /// <summary>
        /// Returns the substring of <paramref name="str"/> up until (and excluding) the first
        /// instance of character <paramref name="c"/>.
        /// If <paramref name="c"/> is not found, <paramref name="str"/> is returned unchanged.
        /// </summary>
        [Pure]
        [NotNull]
        public static string SubstringUntil([NotNull] this string str, char c)
        {
            var index = str.IndexOf(c);

            return index != -1
                ? str.Substring(0, index)
                : str;
        }

        /// <summary>
        /// Returns the substring of <paramref name="str"/> up until (and excluding) the last
        /// instance of character <paramref name="c"/>.
        /// If <paramref name="c"/> is not found, <paramref name="str"/> is returned unchanged.
        /// </summary>
        [Pure]
        [NotNull]
        public static string SubstringUntilLast([NotNull] this string str, char c)
        {
            var index = str.LastIndexOf(c);

            return index != -1
                ? str.Substring(0, index)
                : str;
        }

        /// <summary>
        /// Returns the substring of <paramref name="str"/> after (and excluding) the first
        /// instance of character <paramref name="c"/>.
        /// If <paramref name="c"/> is not found, <paramref name="str"/> is returned unchanged.
        /// </summary>
        [Pure]
        [NotNull]
        public static string SubstringAfter([NotNull] this string str, char c)
        {
            var index = str.IndexOf(c);

            return index != -1
                ? str.Substring(index + 1)
                : str;
        }

        /// <summary>
        /// Returns the substring of <paramref name="str"/> up until (and excluding) the first
        /// instance of string <paramref name="s"/>.
        /// If <paramref name="s"/> is not found, <paramref name="str"/> is returned unchanged.
        /// </summary>
        [Pure]
        [NotNull]
        public static string SubstringAfter([NotNull] this string str, string s, StringComparison comparison = StringComparison.Ordinal)
        {
            var index = str.IndexOf(s, comparison);

            return index != -1
                ? str.Substring(index + s.Length)
                : str;
        }

        /// <summary>
        /// Returns the substring of <paramref name="str"/> after (and excluding) the last
        /// instance of character <paramref name="c"/>.
        /// If <paramref name="c"/> is not found, <paramref name="str"/> is returned unchanged.
        /// </summary>
        [Pure]
        [NotNull]
        public static string SubstringAfterLast([NotNull] this string str, char c)
        {
            var index = str.LastIndexOf(c);

            return index != -1
                ? str.Substring(index + 1)
                : str;
        }

        /// <summary>
        /// Returns the substring of <paramref name="str"/> up until (and excluding) the last
        /// instance of string <paramref name="s"/>.
        /// If <paramref name="s"/> is not found, <paramref name="str"/> is returned unchanged.
        /// </summary>
        [Pure]
        [NotNull]
        public static string SubstringAfterLast([NotNull] this string str, string s, StringComparison comparison = StringComparison.Ordinal)
        {
            var index = str.LastIndexOf(s, comparison);

            return index != -1
                ? str.Substring(index + s.Length)
                : str;
        }

        [Pure]
        [NotNull]
        public static string CommonPrefix([CanBeNull] this string s, [CanBeNull] string other)
        {
            if (s.IsNullOrEmpty() || other.IsNullOrEmpty())
            {
                return string.Empty;
            }

            int prefixLength = 0;

            foreach (char c in other)
            {
                if (s.Length <= prefixLength || s[prefixLength] != c)
                {
                    return s.Substring(0, prefixLength);
                }

                prefixLength++;
            }

            return s;
        }

        [Pure]
        [ContractAnnotation("s:null=>true")]
        public static bool IsNullOrEmpty([CanBeNull] this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        [Pure]
        [CanBeNull]
        public static string Combine([CanBeNull] this string left, [NotNull] string sep, [CanBeNull] string right)
        {
            if (left.IsNullOrEmpty())
            {
                return right;
            }
            else if (right.IsNullOrEmpty())
            {
                return left;
            }
            else
            {
                return left + sep + right;
            }
        }

        /// <summary>
        /// Quotes this string with the specified <paramref name="quotationMark"/>
        /// </summary>
        [Pure]
        [NotNull]
        public static string Quote([CanBeNull] this string s, [NotNull] string quotationMark = "\"")
        {
            if (s == null)
            {
                return "";
            }

            return quotationMark + s + quotationMark;
        }

        /// <summary>
        /// Quotes this string if it is not null and not empty
        /// </summary>
        [Pure]
        [ContractAnnotation("s:null=>null")]
        public static string QuoteNE([CanBeNull] this string s)
        {
            return s.IsNullOrEmpty() ? s : s.Quote();
        }

        /// <summary>
        /// Adds parentheses if string is not null and not empty
        /// </summary>
        [Pure]
        [ContractAnnotation("s:null=>null")]
        public static string AddParenthesesNE([CanBeNull] this string s)
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
        [ContractAnnotation("value:null=>true")]
        public static bool IsNullOrWhiteSpace([CanBeNull] this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>Indicates whether the specified string is neither null, nor empty, nor has only whitespace.</summary>
        [Pure]
        [ContractAnnotation("value:null=>false")]
        public static bool IsNotNullOrWhitespace([CanBeNull] this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Determines whether the beginning of this instance matches any of the specified strings.
        /// </summary>
        /// <param name="starts">array of strings to compare</param>
        /// <returns>true if any starts element matches the beginning of this string; otherwise, false.</returns>
        [Pure]
        [ContractAnnotation("value:null=>false")]
        public static bool StartsWithAny([CanBeNull] this string value, [NotNull, ItemNotNull] IEnumerable<string> starts)
        {
            return value != null && starts.Any(s => value.StartsWith(s));
        }

        [Pure]
        [ContractAnnotation("value:null=>null")]
        public static string RemoveLines([CanBeNull] this string value, [NotNull] Func<string, bool> shouldRemoveLine)
        {
            if (value.IsNullOrEmpty())
            {
                return value;
            }

            if (value[value.Length - 1] == '\n')
            {
                value = value.Substring(0, value.Length - 1);
            }

            var sb = new StringBuilder(capacity: value.Length);

            foreach (var line in value.Split('\n'))
            {
                if (!shouldRemoveLine(line))
                {
                    sb.Append(line).Append('\n');
                }
            }

            return sb.ToString();
        }

        /// <summary>Split a string, delimited by line-breaks, excluding empty entries.</summary>
        [Pure]
        [NotNull]
        public static string[] SplitLines([NotNull] this string value) => value.Split(_newLine, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>Split a string, delimited by the space character, excluding empty entries.</summary>
        [Pure]
        [NotNull]
        public static string[] SplitBySpace([NotNull] this string value) => value.Split(_space, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Shortens <paramref name="str"/> if necessary, so that the resulting string has fewer than <paramref name="maxLength"/> characters.
        /// If shortened, ellipsis are appended to the truncated string.
        /// </summary>
        [NotNull]
        public static string ShortenTo([CanBeNull] this string str, int maxLength)
        {
            if (str.IsNullOrEmpty())
            {
                return string.Empty;
            }

            if (str.Length <= maxLength)
            {
                return str;
            }

            if (maxLength < 3)
            {
                return str.Substring(0, maxLength);
            }

            return str.Substring(0, maxLength - 3) + "...";
        }

        /// <summary>
        /// Returns a value indicating whether the <paramref name="other"/> occurs within <paramref name="str"/>.
        /// </summary>
        /// <param name="other">The string to seek. </param>
        /// <param name="stringComparison">The Comparison type</param>
        /// <returns>
        /// true if the <paramref name="other"/> parameter occurs within <paramref name="str"/>,
        /// or if <paramref name="other"/> is the empty string (""); otherwise, false.
        /// </returns>
        public static bool Contains([NotNull] this string str, [NotNull] string other, StringComparison stringComparison)
        {
            return str.IndexOf(other, stringComparison) != -1;
        }
    }
}
