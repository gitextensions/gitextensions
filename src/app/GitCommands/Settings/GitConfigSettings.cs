#nullable enable

using System.Diagnostics;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;
using GitUI;

namespace GitCommands.Settings;

[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public sealed class GitConfigSettings(IGitModule module, GitSettingLevel gitSettingLevel) : IPersistentConfigValueStore
{
    private readonly GitSettingLevel _gitSettingLevel = gitSettingLevel;
    private readonly Dictionary<string, string?> _modifiedSettings = [];
    private readonly IGitModule _module = module;
    private readonly HashSet<string> _multiValueSettings = [];
    private readonly Dictionary<string, string> _uniqueValueSettings = [];
    private bool _valid;

    public string? GetValue(string name)
    {
        name = NormalizeSettingName(name);
        if (_modifiedSettings.TryGetValue(name, out string? value))
        {
            return value;
        }

        Update();
        return _uniqueValueSettings.TryGetValue(name, out value) ? value : null;
    }

    public void Invalidate()
    {
        _valid = false;
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

    public void Save()
    {
        if (_gitSettingLevel == GitSettingLevel.Effective)
        {
            throw new InvalidOperationException("Effective Git settings are read-only.");
        }

        foreach ((string name, string? value) in _modifiedSettings)
        {
            _module.SetGitSetting(_gitSettingLevel, name, value);

            if (value is null)
            {
                _uniqueValueSettings.Remove(name);
            }
            else
            {
                _uniqueValueSettings[name] = value;
            }
        }

        _modifiedSettings.Clear();
    }

    public void SetValue(string name, string? value)
    {
        if (_gitSettingLevel == GitSettingLevel.Effective)
        {
            throw new InvalidOperationException("Effective Git settings are read-only.");
        }

        if (_multiValueSettings.Contains(name))
        {
            throw new InvalidOperationException(@"Changing multi-value git settings is not supported. Tried to set ""{name}"" = ""{value}"".");
        }

        name = NormalizeSettingName(name);

        if (value?.Length is 0)
        {
            value = null;
        }

        if (value == GetValue(name))
        {
            return;
        }

        if (_uniqueValueSettings.TryGetValue(name, out string? storedValue) && value == storedValue)
        {
            _modifiedSettings.Remove(name);
        }
        else
        {
            _modifiedSettings[name] = value;
        }
    }

    private static string NormalizeSettingName(string name)
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

    private void StoreScopedSetting(string name, string value)
    {
        if (_multiValueSettings.Contains(name))
        {
            // ignore additional value
            return;
        }

        if (!_uniqueValueSettings.TryAdd(name, value))
        {
            _multiValueSettings.Add(name);
            _uniqueValueSettings.Remove(name);
        }
    }

    private void StoreEffectiveSetting(string name, string value)
    {
        // Entries from all scopes are listed starting with the most generic scope, use the last one, i.e. the most specific
        _uniqueValueSettings[name] = value;
    }

    private void Update()
    {
        if (_valid)
        {
            return;
        }

        lock (_uniqueValueSettings)
        {
            if (_valid)
            {
                return;
            }

            string settings = _module.GetGitSettings(_gitSettingLevel);

            _uniqueValueSettings.Clear();
            _multiValueSettings.Clear();
            Parse(settings, _gitSettingLevel == GitSettingLevel.Effective ? StoreEffectiveSetting : StoreScopedSetting);

            _valid = true;
        }
    }

    private string DebuggerDisplay => $"{{ {_gitSettingLevel}, {(_valid ? "" : "in")}valid, {_uniqueValueSettings.Count} + {_modifiedSettings.Count} - {_multiValueSettings.Count} }}";
}
