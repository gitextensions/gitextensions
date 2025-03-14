#nullable enable

namespace GitExtensions.Extensibility.Settings;

public static class PathSetting
{
    /// <summary>
    ///  Converts a path value with POSIX directory separators for storing as git config setting.
    ///  UNC paths need to be stored with backward slashes.
    /// </summary>
    public static string? ConvertPathToGitSetting(this string? path)
    {
        if (path?.StartsWith(@"\\") is false)
        {
            path = path.Replace(Path.DirectorySeparatorChar, '/');
        }

        return path;
    }
}
