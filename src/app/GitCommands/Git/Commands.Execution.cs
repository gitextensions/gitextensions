using System.Diagnostics;
using System.Security;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;

namespace GitCommands.Git;

public static partial class Commands
{
    /// <summary>
    ///  Get all config settings from git according to the scope.
    /// </summary>
    /// <param name="gitExecutable">The <see cref="IGitExecutor.GitExecutable"/> for the affected repo.</param>
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
    ///  Gets the name of the currently checked out branch.
    /// </summary>
    /// <param name="gitExecutor">The IGitExecutor for the repo of interest.</param>
    /// <param name="emptyIfDetached">Defines the value returned if HEAD is detached. <see langword="true"/> to return <see cref="string.Empty"/>; <see langword="false"/> to return "(no branch)".</param>
    /// <returns>
    ///  The name of the branch (for example: "main"); the value requested by <paramref name="emptyIfDetached"/>, if HEAD is detached; <see cref="string.Empty"/> if it fails to retrieve the branch name for any reason (for example, if the repository is not reachable).
    /// </returns>
    public static string GetSelectedBranch(IGitExecutor gitExecutor, bool emptyIfDetached = false)
    {
        if (!gitExecutor.IsReftableRepo && !string.IsNullOrEmpty(gitExecutor.WorkingDir))
        {
            string head = GetSelectedBranchFast(gitExecutor.GetGitDirectory(), emptyIfDetached);

            if (head == ".invalid")
            {
                gitExecutor.IsReftableRepo = true;
            }
            else if (head.Length > 0)
            {
                gitExecutor.IsReftableRepo = false;
                return head;
            }
        }

        GitArgumentBuilder args = new("symbolic-ref")
        {
            "--quiet",
            "HEAD"
        };

        try
        {
            ExecutionResult result = gitExecutor.GitExecutable.Execute(args, throwOnErrorExit: false);

            if (result.ExitedSuccessfully)
            {
                return result.StandardOutput[GitRefName.RefsHeadsPrefix.Length..].TrimEnd();
            }

            return emptyIfDetached ? string.Empty : DetachedHeadParser.DetachedBranch;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Failed to read repo '{gitExecutor.WorkingDir}': {ex.Message}");
        }

        return DetachedHeadParser.UnknownBranchName;

        // Attempt to read the branch name from the HEAD file instead of calling a git command.
        // Dirty but fast. This sometimes fails. In reftable repos, it always returns ".invalid".
        static string GetSelectedBranchFast(string gitDirectory, bool emptyIfDetached)
        {
            string headFileContents;
            try
            {
                // eg. "/path/to/repo/.git/HEAD"
                string headFileName = Path.Join(gitDirectory, "HEAD");

                if (!File.Exists(headFileName))
                {
                    return string.Empty;
                }

                headFileContents = File.ReadAllText(headFileName, GitExecutor.SystemEncoding);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
            {
                // ignore inaccessible file
                return string.Empty;
            }

            // eg. "ref: refs/heads/master"
            //     "9601551c564b48208bccd50b705264e9bd68140d"

            if (!headFileContents.StartsWith("ref: "))
            {
                return emptyIfDetached ? string.Empty : DetachedHeadParser.DetachedBranch;
            }

            const string prefix = "ref: refs/heads/";

            if (!headFileContents.StartsWith(prefix))
            {
                return string.Empty;
            }

            return headFileContents[prefix.Length..].TrimEnd();
        }
    }

    /// <summary>
    ///  Removes a git config (sub)section.
    /// </summary>
    /// <param name="gitExecutable">The <see cref="IGitExecutor.GitExecutable"/> for the affected repo.</param>
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
    /// <param name="gitExecutable">The <see cref="IGitExecutor.GitExecutable"/> for the affected repo.</param>
    /// <param name="settingLevel">The scope for the config (must not be <see cref="GitSettingLevel.Effective"/>).</param>
    /// <param name="setting">The name of the setting (may contain dots, e.g. "core.autocrlf").</param>
    /// <param name="value">The value of the setting or <see langword="null"/> for removing the setting.</param>
    public static void SetGitSetting(this IExecutable gitExecutable, GitSettingLevel settingLevel, string setting, string? value, bool append)
    {
        bool isSet = !string.IsNullOrEmpty(value);
        bool newSyntax = gitExecutable.SupportNewGitConfigSyntax();
        GitArgumentBuilder args = new("config")
            {
                { newSyntax, isSet ? "set" : "unset" },
                { !newSyntax && !isSet, "--unset" },
                { isSet && append, newSyntax ? "--append" : "--add" },
                settingLevel switch
                {
                    GitSettingLevel.Local => "--local",
                    GitSettingLevel.Global => "--global",
                    GitSettingLevel.SystemWide => "--system",
                    GitSettingLevel.Effective or _ => throw new ArgumentOutOfRangeException(nameof(settingLevel))
                },
                "--",
                setting.Quote(),
                { isSet, QuoteSettingValue(value) }
            };
        ExecutionResult result = gitExecutable.Execute(args, throwOnErrorExit: false);
        const int exitCodeOnUnsetNotExistingSetting = 5;
        if (isSet || result.ExitCode != exitCodeOnUnsetNotExistingSetting)
        {
            result.ThrowIfErrorExit();
        }

        return;

        string QuoteSettingValue(string? value)
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
