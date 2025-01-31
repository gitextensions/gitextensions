#nullable enable

using GitExtensions.Extensibility.Settings;

namespace GitExtensions.Extensibility.Configurations;

public interface IConfigValueStore : ISettingsValueGetter
{
    void SetValue(string setting, string? value);
}
