using System.Buffers;

namespace GitExtensions.ExtensibilityTests;

[TestFixture]
public sealed class StringExtensionsTests
{
    private static readonly SearchValues<char> _separators = SearchValues.Create(',', ';');

    [TestCase("a,b;c", 0, ExpectedResult = 1)]
    [TestCase("a,b;c", 1, ExpectedResult = 1)]
    [TestCase("a,b;c", 2, ExpectedResult = 3)]
    [TestCase("a,b;c", 3, ExpectedResult = 3)]
    [TestCase("a,b;c", 4, ExpectedResult = -1)]
    [TestCase("a,b;c", 5, ExpectedResult = -1)]
    [TestCase("abc", 0, ExpectedResult = -1)]
    [TestCase(",", 0, ExpectedResult = 0)]
    [TestCase(",", 1, ExpectedResult = -1)]
    [TestCase("", 0, ExpectedResult = -1)]
    public int IndexOfAny_returns_expected(string input, int startIndex)
        => input.IndexOfAny(_separators, startIndex);

    [TestCase("abc", 0, ExpectedResult = 3)] // no line ending → string length
    [TestCase("abc", 2, ExpectedResult = 3)] // startIndex at last char
    [TestCase("abc", 3, ExpectedResult = 3)] // startIndex past end
    [TestCase("", 0, ExpectedResult = 0)] // empty string → length (0)
    [TestCase("a\nb", 0, ExpectedResult = 1)] // LF found from start
    [TestCase("a\nb", 1, ExpectedResult = 1)] // startIndex at LF
    [TestCase("a\nb", 2, ExpectedResult = 3)] // startIndex past LF → no further ending
    [TestCase("a\r\nb", 0, ExpectedResult = 1)] // CR found before LF
    [TestCase("a\r\nb", 2, ExpectedResult = 2)] // startIndex at LF
    [TestCase("a\r\nb", 3, ExpectedResult = 4)] // startIndex past CRLF → no ending
    [TestCase("a\nb\nc", 2, ExpectedResult = 3)] // second LF
    public int GetLineEnd_returns_expected(string input, int startIndex)
        => input.GetLineEnd(startIndex);
}
