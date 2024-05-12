using GitExtensions.Extensibility.Settings;

namespace GitExtensions.Extensibility.Configurations;

public interface IConfigValueStore : ISettingsValueGetter
{
    void SetPathValue(string setting, string? value);
    void SetValue(string setting, string? value);
}
