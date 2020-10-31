using GitExtensions.Core.Settings;

namespace GitCommands.Settings
{
    public sealed class MemorySettings : SettingsContainer<MemorySettings, MemorySettingsCache>
    {
        public MemorySettings()
            : base(null, new MemorySettingsCache())
        {
            SettingLevel = SettingLevel.Effective;
        }
    }
}
