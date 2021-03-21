using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using GitExtUtils;

// ReSharper disable once CheckNamespace

namespace System
{
    public static class StringExtensions
    {
        // NOTE ordinal string comparison is the default as most string comparison in GE is against static ASCII output from git.exe

        /// <summary>
        /// Returns <paramref name="str"/> without <paramref name="prefix"/>.
        /// If <paramref name="prefix"/> is not found, <paramref name="str"/> is returned unchanged.
        /// </summary>
        [Pure]
        public static string RemovePrefix(this string str, string prefix, StringComparison comparison = StringComparison.Ordinal)
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
        public static string RemoveSuffix(this string str, string suffix, StringComparison comparison = StringComparison.Ordinal)
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
        public static string SubstringUntil(this string str, char c)
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
        public static string SubstringUntilLast(this string str, char c)
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
        public static string SubstringAfter(this string str, char c)
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
        public static string SubstringAfter(this string str, string s, StringComparison comparison = StringComparison.Ordinal)
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
        public static string SubstringAfterLast(this string str, char c)
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
        public static string SubstringAfterLast(this string str, string s, StringComparison comparison = StringComparison.Ordinal)
        {
            var index = str.LastIndexOf(s, comparison);

            return index != -1
                ? str.Substring(index + s.Length)
                : str;
        }

        [Pure]
        public static string CommonPrefix(this string? s, string? other)
        {
            if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(other))
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
        public static string? Combine(this string? left, string sep, string? right)
        {
            if (string.IsNullOrEmpty(left))
            {
                return right;
            }
            else if (string.IsNullOrEmpty(right))
            {
                return left;
            }
            else
            {
                return left + sep + right;
            }
        }

        /// <summary>
        /// Quotes this string with the specified <paramref name="q"/>.
        /// </summary>
        [Pure]
        public static string Quote(this string? s, string q = "\"")
        {
            if (s is null)
            {
                return "";
            }

            return $"{q}{s.Replace(q, "\\" + q)}{q}";
        }

        /// <summary>
        /// Quotes this string if it is not null and not empty.
        /// </summary>
        [Pure]
        [return: NotNullIfNotNull("s")]
        public static string? QuoteNE(this string? s)
        {
            return string.IsNullOrEmpty(s) ? s : s.Quote();
        }

        /// <summary>
        /// Adds parentheses if string is not null and not empty.
        /// </summary>
        [Pure]
        [return: NotNullIfNotNull("s")]
        public static string? AddParenthesesNE(this string? s)
        {
            return string.IsNullOrEmpty(s) ? s : "(" + s + ")";
        }

        /// <summary>
        /// Determines whether the beginning of this instance matches any of the specified strings.
        /// </summary>
        /// <param name="starts">array of strings to compare.</param>
        /// <returns>true if any starts element matches the beginning of this string; otherwise, false.</returns>
        [Pure]
        public static bool StartsWithAny(this string? value, IEnumerable<string> starts)
        {
            return value is not null && starts.Any(s => value.StartsWith(s));
        }

        [Pure]
        [return: NotNullIfNotNull("value")]
        public static string? RemoveLines(this string? value, Func<string, bool> shouldRemoveLine)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (value[value.Length - 1] == '\n')
            {
                value = value.Substring(0, value.Length - 1);
            }

            var sb = new StringBuilder(capacity: value.Length);

            foreach (var line in value.LazySplit('\n'))
            {
                if (!shouldRemoveLine(line))
                {
                    sb.Append(line).Append('\n');
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Shortens <paramref name="str"/> if necessary, so that the resulting string has fewer than <paramref name="maxLength"/> characters.
        /// If shortened, ellipsis are appended to the truncated string.
        /// </summary>
        public static string ShortenTo(this string? str, int maxLength)
        {
            if (string.IsNullOrEmpty(str))
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
        /// <param name="stringComparison">The Comparison type.</param>
        /// <returns>
        /// true if the <paramref name="other"/> parameter occurs within <paramref name="str"/>,
        /// or if <paramref name="other"/> is the empty string (""); otherwise, false.
        /// </returns>
        public static bool Contains(this string str, string other, StringComparison stringComparison)
        {
            return str.IndexOf(other, stringComparison) != -1;
        }
    }
}
