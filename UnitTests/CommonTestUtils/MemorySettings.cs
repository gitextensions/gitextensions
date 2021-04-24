#nullable enable

using GitCommands.Settings;
using GitUIPluginInterfaces;

namespace GitUITests
{
    public class MemorySettings : SettingsContainer<MemorySettings, MemorySettingsCache>
    {
        public MemorySettings()
            : base(null, new MemorySettingsCache())
        {
            SettingLevel = SettingLevel.Effective;
        }
    }
}
