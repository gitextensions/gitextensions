using GitCommands;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;

namespace GitUI;

public class GitPluginSettingsContainer : SettingsSource, IGitPluginSettingsContainer
{
    private readonly string _pluginOldCompatibilityPrefix;
    private readonly string _pluginPrefix;
    private SettingsSource? _settingsSource;

    public GitPluginSettingsContainer(Guid pluginId, string pluginName)
    {
        _pluginPrefix = pluginId == Guid.Empty ? pluginName : $"{pluginId}.";
        _pluginOldCompatibilityPrefix = pluginName;
    }

    public SettingsSource GetSettingsSource() => this;

    public void SetSettingsSource(SettingsSource? settingsSource)
    {
        _settingsSource = settingsSource;
    }

    private SettingsSource ExternalSettings => _settingsSource ?? AppSettings.SettingsContainer;

    public override SettingLevel SettingLevel
    {
        get => ExternalSettings.SettingLevel;
        set => throw new InvalidOperationException(nameof(SettingLevel));
    }

    public override string? GetValue(string name)
        => ExternalSettings.GetValue($"{_pluginPrefix}{name}")
            ?? ExternalSettings.GetValue($"{_pluginOldCompatibilityPrefix}{name}");

    public override void SetValue(string name, string? value)
        => ExternalSettings.SetValue($"{_pluginPrefix}{name}", value);
}
