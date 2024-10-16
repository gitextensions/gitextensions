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
    public const char LineFeed = '\n';
    public const char VerticalFeed = '\v';
    public static readonly char[] LineAndVerticalFeed = [LineFeed, VerticalFeed];
    public const char Space = ' ';
    public const char Tab = '\t';
    public const char Null = '\0';
    public static readonly char[] TabAndSpace = [Tab, Space];
    public static readonly char[] TabAndLineFeedAndCarriageReturn = [Tab, LineFeed, CarriageReturn];
    public static readonly char[] NullAndLineFeed = [Null, LineFeed];
    public static readonly char[] LineFeedAndCarriageReturn = [LineFeed, CarriageReturn];
    public static readonly char[] LineFeedCarriageReturnAndNull = [LineFeed, CarriageReturn, Null];
    public static readonly string[] NewLines = [$"{CarriageReturn}{LineFeed}", $"{LineFeed}"];
    public static readonly char[] GitOutput = ['*', Space, LineFeed, CarriageReturn];
    public const char ForwardSlash = '/';
    public const char Colon = ':';
    public const char Comma = ',';
    public const char Period = '.';
    public const char Hash = '#';
    public static readonly char[] PathSeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];
}
