#nullable enable

using GitExtensions.Extensibility.Settings;

namespace GitExtensions.Extensibility.Configurations;

public interface IConfigValueStore : ISettingsValueGetter
{
    /// <summary>
    ///  Stores a path value with POSIX directory separators.
    ///  UNC paths need to be stored with backward slashes.
    /// </summary>
    void SetPathValue(string setting, string? path)
    {
        if (path?.StartsWith(@"\\") is false)
        {
            path = path.Replace(Path.PathSeparator, '/');
        }

        SetValue(setting, path);
    }

    void SetValue(string setting, string? value);
}
