using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;

namespace GitCommands.Git;

public static partial class Commands
{
    /// <summary>
    ///  Get all config settings from git according to the scope.
    /// </summary>
    /// <param name="gitExecutable">The <see cref="IGitModule.GitExecutable"/> for the affected repo.</param>
    /// <param name="settingLevel">The scope for the config.</param>
    /// <returns>The names and values of the settings delimited by '\0'.</returns>
    public static string GetGitSettings(this IExecutable gitExecutable, GitSettingLevel settingLevel)
    {
        GitArgumentBuilder args = new("config")
            {
                gitExecutable.SupportNewGitConfigSyntax() ? "list" : "--list",
                settingLevel switch
                {
                    GitSettingLevel.Effective => "",
                    GitSettingLevel.Local => "--local",
                    GitSettingLevel.Global => "--global",
                    GitSettingLevel.SystemWide => "--system",
                    _ => throw new ArgumentOutOfRangeException(nameof(settingLevel))
                },
                "--includes",
                "--null"
            };
        ExecutionResult result = gitExecutable.Execute(args, throwOnErrorExit: false);

        if (result.ExitedSuccessfully)
        {
            return result.StandardOutput;
        }

        if (result.StandardError.StartsWith("fatal: unable to read config file") && result.StandardError.TrimEnd().EndsWith(": No such file or directory"))
        {
            return "";
        }

        result.ThrowIfErrorExit("Error getting config values");
        return "unreachable code";
    }

    /// <summary>
    ///  Removes a git config (sub)section.
    /// </summary>
    /// <param name="gitExecutable">The <see cref="IGitModule.GitExecutable"/> for the affected repo.</param>
    /// <param name="settingLevel">The scope for the config (must not be <see cref="GitSettingLevel.Effective"/>).</param>
    /// <param name="section">The name of the section.</param>
    /// <param name="subsection">The optional name of the subsection.</param>
    public static void RemoveConfigSection(this IExecutable gitExecutable, GitSettingLevel settingLevel, string section, string? subsection = null)
    {
        GitArgumentBuilder args = new("config")
            {
                gitExecutable.SupportNewGitConfigSyntax() ? "remove-section" : "--remove-section",
                settingLevel switch
                {
                    GitSettingLevel.Local => "--local",
                    GitSettingLevel.Global => "--global",
                    GitSettingLevel.SystemWide => "--system",
                    GitSettingLevel.Effective or _ => throw new ArgumentOutOfRangeException(nameof(settingLevel))
                },
                "--",
                subsection is null ? section : @$"""{section}.{subsection}"""
            };
        gitExecutable.Execute(args);
    }

    /// <summary>
    ///  Sets or unsets a git config setting.
    /// </summary>
    /// <param name="gitExecutable">The <see cref="IGitModule.GitExecutable"/> for the affected repo.</param>
    /// <param name="settingLevel">The scope for the config (must not be <see cref="GitSettingLevel.Effective"/>).</param>
    /// <param name="setting">The name of the setting (may contain dots, e.g. "core.autocrlf").</param>
    /// <param name="value">The value of the setting or <see langword="null"/> for removing the setting.</param>
    public static void SetGitSetting(this IExecutable gitExecutable, GitSettingLevel settingLevel, string setting, string? value)
    {
        bool isSet = !string.IsNullOrEmpty(value);
        bool newSyntax = gitExecutable.SupportNewGitConfigSyntax();
        GitArgumentBuilder args = new("config")
            {
                { newSyntax, isSet ? "set" : "unset" },
                { !newSyntax && !isSet, "--unset" },
                settingLevel switch
                {
                    GitSettingLevel.Local => "--local",
                    GitSettingLevel.Global => "--global",
                    GitSettingLevel.SystemWide => "--system",
                    GitSettingLevel.Effective or _ => throw new ArgumentOutOfRangeException(nameof(settingLevel))
                },
                setting,
                { isSet, QuoteSettingValue(value) }
            };
        ExecutionResult result = gitExecutable.Execute(args, throwOnErrorExit: false);
        const int exitCodeOnUnsetNotExistingSetting = 5;
        if (isSet || result.ExitCode != exitCodeOnUnsetNotExistingSetting)
        {
            result.ThrowIfErrorExit();
        }

        return;

        string QuoteSettingValue(string value)
        {
            value = value.Quote();

            // Quote diff / merge placeholders so that they are not evaluated by the WSL shell
            return PathUtil.IsWslPath(gitExecutable.WorkingDir)
                ? value.Replace("$", @"\$")
                : value;
        }
    }

    private static bool SupportNewGitConfigSyntax(this IExecutable gitExecutable) => GitVersion.CurrentVersion(gitExecutable).SupportNewGitConfigSyntax;
}
