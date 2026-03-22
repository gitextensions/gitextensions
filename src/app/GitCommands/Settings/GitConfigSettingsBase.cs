#nullable enable

using System.Diagnostics;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUI;

namespace GitCommands.Settings;

/// <summary>
///  Provides read-only access to git config settings of different scopes (by running "git config list").
/// </summary>
/// <param name="gitExecutable">The <see cref="IGitExecutor.GitExecutable"/> for the repo of interest.</param>
/// <param name="gitSettingLevel">The scope (<see cref="GitSettingLevel"/>) of the git config settings.</param>
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public abstract class GitConfigSettingsBase(IExecutable gitExecutable, GitSettingLevel gitSettingLevel) : ISettingsValueGetter
{
    protected GitSettingLevel GitSettingLevel { get; } = gitSettingLevel;
    protected IExecutable GitExecutable { get; } = gitExecutable;
    protected Dictionary<string, string> UniqueValueSettings { get; } = [];

    private readonly Lock _isValidLock = new();
    private bool _isValid;

    protected bool IsValid => _isValid;

    public abstract string? GetValue(string name);

    public void Invalidate()
    {
        lock (_isValidLock)
        {
            _isValid = false;
        }

        ThreadHelper.FileAndForget(async () =>
        {
            await Task.Delay(millisecondsDelay: 250);
            if (Path.Exists(GitExecutable.WorkingDir))
            {
                try
                {
                    Update();
                }
                catch (ExternalOperationException ex)
                {
                    Trace.WriteLine($"{nameof(GitConfigSettingsBase)}: Background update failed:\n{ex}");
                }
            }
        });
    }

    /// <summary>
    ///  Turns the section and value names to lower case in order to match the output of "git config list".
    ///  The case-sensitive subsection is not modified.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <returns>The normalized setting name.</returns>
    protected static string NormalizeSettingName(string name)
    {
        if (name.Any(char.IsAsciiLetterUpper))
        {
            int firstDotIndex = name.IndexOf('.');
            int lastDotIndex = name.LastIndexOf('.');
            if (lastDotIndex < 0 || firstDotIndex == lastDotIndex)
            {
                Debug.WriteLine(@$"(No Exception) Setting name should be lowercase: ""{name}"".");
                return name.ToLowerInvariant();
            }

            ReadOnlySpan<char> section = name.AsSpan(0, firstDotIndex);
            ReadOnlySpan<char> variable = name.AsSpan(lastDotIndex + 1);

            if (!section.ContainsAnyInRange('A', 'Z') && !variable.ContainsAnyInRange('A', 'Z'))
            {
                return name;
            }

            Debug.WriteLine(@$"(No Exception) Setting section and variable name should be lowercase: ""{name}"".");

            ReadOnlySpan<char> caseSignificant = name.AsSpan(firstDotIndex, length: lastDotIndex + 1 - firstDotIndex);
            return $"{section.ToString().ToLowerInvariant()}{caseSignificant}{variable.ToString().ToLowerInvariant()}";
        }

        return name;
    }

    private static void Parse(string settings, Action<string, string> storeSetting)
    {
        foreach (string setting in settings.TrimEnd('\0').LazySplit('\0'))
        {
            int linefeedIndex = setting.IndexOf('\n');
            if (linefeedIndex > 0)
            {
                storeSetting(setting[..linefeedIndex], setting[(linefeedIndex + 1)..]);
            }
            else
            {
                storeSetting(setting, "true");
            }
        }
    }

    /// <summary>
    ///  Reloads the settings if invalid.
    /// </summary>
    protected abstract void Update();

    /// <summary>
    ///  Reloads the settings if invalid (generic implementation).
    /// </summary>
    /// <param name="clear">Shall clear (previously loaded) git settings, but keep modified settings.</param>
    /// <param name="storeSetting">Shall store a parsed git setting.</param>
    protected void Update(Action clear, Action<string, string> storeSetting)
    {
        if (_isValid)
        {
            return;
        }

        lock (_isValidLock)
        {
            if (_isValid)
            {
                return;
            }

            string settings = GitExecutable.GetGitSettings(GitSettingLevel);

            clear();
            Parse(settings, storeSetting);

            _isValid = true;
        }
    }

    protected string DebuggerDisplay => $"{{ {GitSettingLevel}, {(_isValid ? "" : "in")}valid, {UniqueValueSettings.Count} }}";
}
