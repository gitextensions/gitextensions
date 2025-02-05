#nullable enable

using System.Diagnostics;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;

namespace GitCommands.Settings;

[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public sealed class EffectiveGitConfigSettings(IExecutable gitExecutable) : GitConfigSettingsBase(gitExecutable, GitSettingLevel.Effective), IConfigValueStore
{
    public override string? GetValue(string name)
    {
        name = NormalizeSettingName(name);
        Update();
        return _uniqueValueSettings.TryGetValue(name, out string? value) ? value : null;
    }

    public void SetValue(string setting, string? value)
    {
        throw new InvalidOperationException("Effective Git settings are read-only.");
    }

    protected override void Update() => Update(Clear, StoreSetting);

    private void Clear()
    {
        _uniqueValueSettings.Clear();
    }

    private void StoreSetting(string name, string value)
    {
        // Entries from all scopes are listed starting with the most generic scope, use the last one, i.e. the most specific
        _uniqueValueSettings[name] = value;
    }
}
