#nullable enable

using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Settings;

namespace GitCommands.Settings;

/// <summary>
///  Adapter for using an <see cref="IConfigValueStore"/>-compatible instance as <see cref="SettingsSource"/>.
/// </summary>
/// <typeparam name="T">The actual config-value-store type.</typeparam>
/// <param name="configValueStore">The config-value-store instance.</param>
public sealed class SettingsSource<T>(T configValueStore) : SettingsSource where T : IConfigValueStore
{
    public T ConfigValueStore { get; } = configValueStore;

    public override string? GetValue(string name) => ConfigValueStore.GetValue(name);
    public override void SetValue(string name, string? value) => ConfigValueStore.SetValue(name, value);
}
