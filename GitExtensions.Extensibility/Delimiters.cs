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
    public static readonly char[] LineFeed = ['\n'];
    public static readonly char[] LineAndVerticalFeed = ['\n', '\v'];
    public static readonly char[] Space = [' '];
    public static readonly char[] Tab = ['\t'];
    public static readonly char[] Null = ['\0'];
    public static readonly char[] TabAndSpace = ['\t', ' '];
    public static readonly char[] TabAndLineFeedAndCarriageReturn = ['\t', '\n', '\r'];
    public static readonly char[] NullAndLineFeed = ['\0', '\n'];
    public static readonly char[] LineFeedAndCarriageReturn = ['\n', '\r'];
    public static readonly char[] LineFeedCarriageReturnAndNull = ['\n', '\r', '\0'];
    public static readonly string[] NewLines = ["\r\n", "\n"];
    public static readonly char[] GitOutput = ['*', ' ', '\n', '\r'];
    public static readonly char[] ForwardSlash = ['/'];
    public static readonly char[] Colon = [':'];
    public static readonly char[] Comma = [','];
    public static readonly char[] Period = ['.'];
    public static readonly char[] Hash = ['#'];
    public static readonly char[] PathSeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];
}
