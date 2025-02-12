#nullable enable

using System.Text;
using GitCommands.Config;
using GitExtensions.Extensibility.Configurations;

namespace GitCommands.Settings;

public sealed class GitEncodingSettingsSetter(IConfigValueStore configValueStore)
{
    public IConfigValueStore ConfigValueStore { get; } = configValueStore;

    public Encoding? FilesEncoding
    {
        set => ConfigValueStore.SetValue(SettingKeyString.FilesEncoding, value?.WebName);
    }
}
