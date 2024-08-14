using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;
using GitCommands.Utils;
using GitExtensions.Extensibility;

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
                ? str[prefix.Length..]
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
                ? str[..^suffix.Length]
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
            int index = str.IndexOf(c);

            return index != -1
                ? str[..index]
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
            int index = str.LastIndexOf(c);

            return index != -1
                ? str[..index]
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
            int index = str.IndexOf(c);

            return index != -1
                ? str[(index + 1)..]
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
            int index = str.IndexOf(s, comparison);

            return index != -1
                ? str[(index + s.Length)..]
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
            int index = str.LastIndexOf(c);

            return index != -1
                ? str[(index + 1)..]
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
            int index = str.LastIndexOf(s, comparison);

            return index != -1
                ? str[(index + s.Length)..]
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
                    return s[..prefixLength];
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
        ///  Escapes this string for use as a command line argument.
        /// </summary>
        [Pure]
        public static string EscapeForCommandLine(this string s, bool? forWindows = null)
        {
            return (forWindows ?? EnvUtils.RunningOnWindows()) ? EscapeForWindowsCommandLine(s) : EscapeForPosixCommandLine(s);

            static string EscapeForWindowsCommandLine(string s) => s.Replace("\"", "\"\"");
            static string EscapeForPosixCommandLine(string s) => s.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("'", @"\'");
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

            if (value[^1] == '\n')
            {
                value = value[..^1];
            }

            StringBuilder sb = new(capacity: value.Length);

            foreach (string line in value.LazySplit('\n'))
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
                return str[..maxLength];
            }

            return str[..(maxLength - 3)] + "...";
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

        /// <summary>
        ///  Returns a <see langword="null"/> for an empty string. Primarily used to store settings.
        /// </summary>
        [Pure]
        public static string? NullIfEmpty(this string? text)
            => string.IsNullOrEmpty(text) ? null : text;
    }
}
