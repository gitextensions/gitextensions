#nullable enable

using System.Diagnostics;
using System.Text;
using GitCommands.Config;
using GitExtensions.Extensibility.Settings;

namespace GitCommands.Settings;

/// <summary>
///  Provides read access to git config settings for encodings.
/// </summary>
public sealed class GitEncodingSettingsGetter(ISettingsValueGetter settingsValueGetter)
{
    public ISettingsValueGetter SettingsValueGetter { get; } = settingsValueGetter;

    public Encoding? FilesEncoding => GetEncoding(SettingKeyString.FilesEncoding);

    public Encoding? CommitEncoding => GetEncoding("i18n.commitencoding");

    public Encoding? LogOutputEncoding => GetEncoding("i18n.logoutputencoding");

    private Encoding? GetEncoding(string settingName)
    {
        string? encodingName = SettingsValueGetter.GetValue(settingName);

        if (string.IsNullOrEmpty(encodingName))
        {
            return null;
        }

        // The keys in AppSettings.AvailableEncodings are lower-case, and the actual
        // configuration is case-insensitive, and can be configured uppercase on the
        // command line. If configured as UTF-8, then if we don't use the predefined
        // encoding, it will use the default, which adds BOM.
        // Convert it to lowercase, to ensure matching.
        encodingName = encodingName.ToLowerInvariant();

        if (AppSettings.AvailableEncodings.TryGetValue(encodingName, out Encoding? result))
        {
            return result;
        }

        try
        {
            return Encoding.GetEncoding(encodingName);
        }
        catch (ArgumentException)
        {
            Trace.WriteLine($"Unsupported encoding \"{encodingName}\"\nPlease check the setting \"{settingName}\" in the git config file.");
            return null;
        }
    }
}
