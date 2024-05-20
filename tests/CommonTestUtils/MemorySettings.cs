#nullable enable

using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;

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
