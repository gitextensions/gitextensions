#nullable enable

using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Settings;

namespace GitCommands.Settings;

public sealed class SettingsSource<T>(T configValueStore) : SettingsSource where T : IConfigValueStore
{
    public T ConfigValueStore { get; } = configValueStore;

    public override string? GetValue(string name) => ConfigValueStore.GetValue(name);
    public override void SetValue(string name, string? value) => ConfigValueStore.SetValue(name, value);
}
