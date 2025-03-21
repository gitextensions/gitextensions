#nullable enable

using System.Diagnostics;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;

namespace GitCommands.Settings;

/// <summary>
///  Provides read-only access to the effective git config settings (by running "git config list").
///  <br>Only the last value of multi-value settings is provided - in contrast to <see cref="GitConfigSettings"/>.</br>
///  <br>"Implements" <see cref="IConfigValueStore"/> so it can be used with <see cref="SettingsSource{T}"/>.</br>
/// </summary>
/// <param name="gitExecutable">The <see cref="IGitModule.GitExecutable"/> for the repo of interest.</param>
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public sealed class EffectiveGitConfigSettings(IExecutable gitExecutable) : GitConfigSettingsBase(gitExecutable, GitSettingLevel.Effective), IConfigValueStore
{
    private void Clear()
    {
        _uniqueValueSettings.Clear();
    }

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

    private void StoreSetting(string name, string value)
    {
        // "git config list" yields entries from all scopes starting with the most generic scope, use the last one, i.e. the most specific
        _uniqueValueSettings[name] = value;
    }
}
