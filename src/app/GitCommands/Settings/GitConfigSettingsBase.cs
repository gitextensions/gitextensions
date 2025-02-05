#nullable enable

using System.Diagnostics;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitUI;

namespace GitCommands.Settings;

[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public abstract class GitConfigSettingsBase(IExecutable gitExecutable, GitSettingLevel gitSettingLevel) : ISettingsValueGetter
{
    protected readonly GitSettingLevel _gitSettingLevel = gitSettingLevel;
    protected readonly IExecutable _gitExecutable = gitExecutable;
    protected readonly Dictionary<string, string> _uniqueValueSettings = [];
    protected bool Valid { get; private set; }

    public abstract string? GetValue(string name);

    public void Invalidate()
    {
        Valid = false;
        ThreadHelper.FileAndForget(async () =>
        {
            await Task.Delay(millisecondsDelay: 250);
            Update();
        });
    }

    public void Reload()
    {
        Invalidate();
        Update();
    }

    protected static string NormalizeSettingName(string name)
    {
        if (name.Any(char.IsAsciiLetterUpper))
        {
            int firstSeparator = name.IndexOf('.');
            int lastSeparator = name.LastIndexOf('.');
            if (lastSeparator < 0 || firstSeparator == lastSeparator)
            {
                Debug.WriteLine(@$"(No Exception) Setting name should be lowercase: ""{name}"".");
                return name.ToLowerInvariant();
            }

            ReadOnlySpan<char> section = name.AsSpan(0, firstSeparator);
            ReadOnlySpan<char> variable = name.AsSpan(lastSeparator + 1);

            if (!section.ContainsAnyInRange('A', 'Z') && !variable.ContainsAnyInRange('A', 'Z'))
            {
                return name;
            }

            Debug.WriteLine(@$"(No Exception) Setting section and variable name should be lowercase: ""{name}"".");

            ReadOnlySpan<char> caseSignificant = name.AsSpan(firstSeparator, length: lastSeparator + 1 - firstSeparator);
            return $"{section.ToString().ToLowerInvariant()}{caseSignificant}{variable.ToString().ToLowerInvariant()}";
        }

        return name;
    }

    protected abstract void Update();

    protected void Update(Action clear, Action<string, string> storeSetting)
    {
        if (Valid)
        {
            return;
        }

        lock (_uniqueValueSettings)
        {
            if (Valid)
            {
                return;
            }

            string settings = _gitExecutable.GetGitSettings(_gitSettingLevel);

            clear();
            Parse(settings, storeSetting);

            Valid = true;
        }
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
                Trace.WriteLine(@$"Invalid git config ""{setting}"".");
            }
        }
    }

    protected string DebuggerDisplay => $"{{ {_gitSettingLevel}, {(Valid ? "" : "in")}valid, {_uniqueValueSettings.Count} }}";
}
