using System.Buffers;

namespace GitExtensions.Extensibility;

/// <summary>
/// Singleton instances of commonly used character arrays.
/// </summary>
/// <remarks>
/// Using these instances avoids allocating an array for each invocation of methods
/// such as <c>string.Split</c>.
/// </remarks>
public static class Delimiters
{
    public const char CarriageReturn = '\r';
    public const char Colon = ':';
    public const char Comma = ',';
    public const char ForwardSlash = '/';
    public const char Hash = '#';
    public const char LineFeed = '\n';
    public const char Null = '\0';
    public const char Period = '.';
    public const char Space = ' ';
    public const char Tab = '\t';
    public const char VerticalFeed = '\v';

    public static readonly char[] GitOutput = ['*', Space, LineFeed, CarriageReturn];
    public static readonly SearchValues<char> LineAndVerticalFeed = SearchValues.Create(LineFeed, VerticalFeed);
    public static readonly char[] LineFeedAndCarriageReturn = [LineFeed, CarriageReturn];
    public static readonly SearchValues<char> LineFeedAndCarriageReturnSearchValues = SearchValues.Create(LineFeedAndCarriageReturn);
    public static readonly SearchValues<char> LineFeedAndCarriageReturnAndNull = SearchValues.Create(LineFeed, CarriageReturn, Null);
    public static readonly string[] NewLines = [$"{CarriageReturn}{LineFeed}", $"{LineFeed}"];
    public static readonly char[] NullAndLineFeed = [Null, LineFeed];
    public static readonly char[] PathSeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];
    public static readonly SearchValues<char> PathSeparatorsSearchValues = SearchValues.Create(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    public static readonly char[] TabAndLineFeedAndCarriageReturn = [Tab, LineFeed, CarriageReturn];
    public static readonly char[] TabAndSpace = [Tab, Space];
    public static readonly SearchValues<char> WildcardBranchSearchValues = SearchValues.Create('?', '*', '[');
    public static readonly SearchValues<char> InvalidPathCharsSearchValues = SearchValues.Create(Path.GetInvalidPathChars());
    public static readonly SearchValues<char> WhiteSpaceChars = SearchValues.Create(Space, CarriageReturn, LineFeed, Tab);
}
