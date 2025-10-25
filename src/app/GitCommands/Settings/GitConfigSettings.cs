#nullable enable

using System.Diagnostics;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;

namespace GitCommands.Settings;

/// <summary>
///  Provides read/write access to git config settings of a specific scope (by running "git config").
/// </summary>
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public sealed class GitConfigSettings : GitConfigSettingsBase, IGitConfigSettingsGetter, IPersistentConfigValueStore
{
    private readonly Dictionary<string, string?> _modifiedSettings = [];
    private readonly Dictionary<string, List<string>> _multiValueSettings = [];

    /// <summary>
    ///  The constructor.
    /// </summary>
    /// <param name="gitExecutable">The <see cref="IGitModule.GitExecutable"/> for the repo of interest.</param>
    /// <param name="gitSettingLevel">The scope (excluding <see cref="GitSettingLevel.Effective"/>) of the git config settings.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public GitConfigSettings(IExecutable gitExecutable, GitSettingLevel gitSettingLevel)
        : base(gitExecutable, gitSettingLevel)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(gitSettingLevel == GitSettingLevel.Effective, true, $"Use read-only {nameof(EffectiveGitConfigSettings)} instance instead.");
    }

    private void Clear()
    {
        _multiValueSettings.Clear();
        UniqueValueSettings.Clear();
    }

    public IEnumerable<(string Setting, string Value)> GetAllValues()
    {
        Update();

        foreach (KeyValuePair<string, string?> modified in _modifiedSettings)
        {
            if (modified.Value is not null)
            {
                yield return (modified.Key, modified.Value);
            }
        }

        foreach (KeyValuePair<string, string> unique in UniqueValueSettings)
        {
            if (!_modifiedSettings.ContainsKey(unique.Key))
            {
                yield return (unique.Key, unique.Value);
            }
        }

        foreach (KeyValuePair<string, List<string>> multi in _multiValueSettings)
        {
            foreach (string value in multi.Value)
            {
                yield return (multi.Key, value);
            }
        }
    }

    public override string? GetValue(string name)
    {
        name = NormalizeSettingName(name);
        if (_modifiedSettings.TryGetValue(name, out string? value))
        {
            return value;
        }

        Update();
        return UniqueValueSettings.TryGetValue(name, out value) ? value : null;
    }

    public IReadOnlyList<string> GetValues(string name)
    {
        name = NormalizeSettingName(name);
        Update();
        return _multiValueSettings.TryGetValue(name, out List<string>? values)
            ? values
            : GetValue(name) is string value
                ? [value]
                : [];
    }

    public void Save()
    {
        foreach ((string name, string? value) in _modifiedSettings)
        {
            GitExecutable.SetGitSetting(GitSettingLevel, name, value, append: false);

            if (value is null)
            {
                UniqueValueSettings.Remove(name);
            }
            else
            {
                UniqueValueSettings[name] = value;
            }
        }

        _modifiedSettings.Clear();
    }

    public void SetValue(string name, string? value)
    {
        if (_multiValueSettings.ContainsKey(name))
        {
            throw new InvalidOperationException(@$"Changing multi-value git settings is not supported. Tried to set ""{name}"" = ""{value}"".");
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

        if (UniqueValueSettings.TryGetValue(name, out string? storedValue) && value == storedValue)
        {
            _modifiedSettings.Remove(name);
        }
        else
        {
            _modifiedSettings[name] = value;
        }
    }

    private void StoreSetting(string name, string value)
    {
        if (_multiValueSettings.TryGetValue(name, out List<string>? values))
        {
            values.Add(value);
            return;
        }

        if (!UniqueValueSettings.TryAdd(name, value))
        {
            UniqueValueSettings.Remove(name, out string? firstValue);
            _multiValueSettings.Add(name, [firstValue!, value]);
        }
    }

    protected override void Update() => Update(Clear, StoreSetting);

    private new string DebuggerDisplay => $"{{ {GitSettingLevel}, {(IsValid ? "" : "in")}valid, {UniqueValueSettings.Count} + {_modifiedSettings.Count}! + {_multiValueSettings.Count}* }}";
}
