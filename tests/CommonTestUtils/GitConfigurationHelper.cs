using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;

namespace CommonTestUtils;

public sealed class GitConfigurationHelper
{
    private static Dictionary<string, string> _cachedConfiguration = new();

    static GitConfigurationHelper()
    {
        try
        {
            Executable gitExecutable = new Executable(() => AppSettings.GitCommand, ".");

            string packedSettings = gitExecutable.GetGitSettings(GitSettingLevel.Effective);

            foreach (string line in packedSettings.LazySplit('\0'))
            {
                int separator = line.IndexOf('\n');

                if (separator > 0)
                {
                    string key = line.Substring(0, separator);
                    string value = line.Substring(separator + 1);

                    _cachedConfiguration[key] = value;
                }
            }
        }
        catch
        {
        }
    }

    public static string GetSetting(string key, string? defaultValue)
    {
        if (_cachedConfiguration.TryGetValue(key, out string assignedValue) && !string.IsNullOrWhiteSpace(assignedValue))
        {
            return assignedValue;
        }

        return defaultValue ?? "";
    }
}
