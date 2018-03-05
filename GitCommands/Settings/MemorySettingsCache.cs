using System.Collections.Generic;

namespace GitCommands.Settings
{
    public class MemorySettingsCache : SettingsCache
    {
        private IDictionary<string, string> stringSettings = new Dictionary<string, string>();

        protected override void LoadImpl()
        {
            // cached in memory, do nothing
        }

        protected override bool NeedRefresh()
        {
            return false;
        }

        protected override void SaveImpl()
        {
            // cached in memory, do nothing
        }

        protected override void SetValueImpl(string key, string value)
        {
            stringSettings[key] = value;
        }

        protected override string GetValueImpl(string key)
        {
            if (stringSettings.TryGetValue(key, out var value))
                return value;

            return null;
        }

        protected override void ClearImpl()
        {
            stringSettings.Clear();
        }
    }
}
