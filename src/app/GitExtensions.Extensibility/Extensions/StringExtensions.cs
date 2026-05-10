using System.Buffers;
using GitExtensions.Extensibility;

// ReSharper disable once CheckNamespace
namespace System;

public static class StringExtensions
{
    /// <summary>
    ///  Returns the index of the first line-ending character at or after <paramref name="startIndex"/>,
    ///  or <see cref="string.Length"/> if the remainder of the string contains no line ending.
    /// </summary>
    /// <param name="startIndex">The zero-based index at which the search starts.</param>
    /// <param name="lineEndings">The set of characters that count as line endings.</param>
    /// <returns>The zero-based index of the first line-ending character, or <see cref="string.Length"/> if none was found.</returns>
    public static int GetLineEnd(this string s, int startIndex = 0, SearchValues<char>? lineEndings = null)
    {
        int relativeIndex = s.AsSpan(startIndex).IndexOfAny(lineEndings ?? Delimiters.LineFeedAndCarriageReturnSearchValues);
        return relativeIndex < 0 ? s.Length : startIndex + relativeIndex;
    }

    /// <summary>
    ///  Reports the zero-based index of the first occurrence of any character in <paramref name="values"/>
    ///  found in this string, starting at <paramref name="startIndex"/>.
    /// </summary>
    /// <param name="values">The set of characters to search for.</param>
    /// <param name="startIndex">The zero-based index at which the search starts.</param>
    /// <returns>The zero-based index of the first occurrence of any character in <paramref name="values"/>, or -1 if no character was found.</returns>
    public static int IndexOfAny(this string s, SearchValues<char> values, int startIndex)
    {
        int relativeIndex = s.AsSpan(startIndex).IndexOfAny(values);
        return relativeIndex < 0 ? -1 : startIndex + relativeIndex;
    }
}
