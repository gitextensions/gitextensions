namespace GitCommands.Settings
{
    public class MemorySettings : SettingsContainer<MemorySettings, MemorySettingsCache>
    {
        public MemorySettings()
            : base(null, new MemorySettingsCache())
        {
        }
    }
}
