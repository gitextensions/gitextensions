#nullable enable

using System.Diagnostics;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;

namespace GitCommands.Settings;

[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public sealed class GitConfigSettings : GitConfigSettingsBase, IPersistentConfigValueStore
{
    private readonly Dictionary<string, string?> _modifiedSettings = [];
    private readonly HashSet<string> _multiValueSettings = [];

    public GitConfigSettings(IExecutable gitExecutable, GitSettingLevel gitSettingLevel)
        : base(gitExecutable, gitSettingLevel)
    {
        if (gitSettingLevel == GitSettingLevel.Effective)
        {
            throw new ArgumentOutOfRangeException(nameof(gitSettingLevel), $"Use read-only {nameof(EffectiveGitConfigSettings)} instance instead.");
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
        return _uniqueValueSettings.TryGetValue(name, out value) ? value : null;
    }

    public void Save()
    {
        foreach ((string name, string? value) in _modifiedSettings)
        {
            _gitExecutable.SetGitSetting(_gitSettingLevel, name, value);

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

    protected override void Update() => Update(Clear, StoreSetting);

    private void Clear()
    {
        _multiValueSettings.Clear();
        _uniqueValueSettings.Clear();
    }

    private void StoreSetting(string name, string value)
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

    private new string DebuggerDisplay => $"{{ {_gitSettingLevel}, {(Valid ? "" : "in")}valid, {_uniqueValueSettings.Count} + {_modifiedSettings.Count} - {_multiValueSettings.Count} }}";
}
