using System;
using GitUIPluginInterfaces;

namespace GitCommands.Settings
{
    public class SettingsContainer<TLowerPriority, TCache> : ISettingsSource where TLowerPriority : SettingsContainer<TLowerPriority, TCache> where TCache : SettingsCache
    {
        private readonly ICredentialsManager _credentialsManager = new CredentialsManager();

        public TLowerPriority? LowerPriority { get; }
        public TCache SettingsCache { get; }

        public SettingsContainer(TLowerPriority? lowerPriority, TCache settingsCache)
        {
            LowerPriority = lowerPriority;
            SettingsCache = settingsCache;
        }

        public void LockedAction(Action action)
        {
            SettingsCache.LockedAction(() =>
                {
                    if (LowerPriority is not null)
                    {
                        LowerPriority.LockedAction(action);
                    }
                    else
                    {
                        action();
                    }
                });
        }

        public void Save()
        {
            _credentialsManager.Save();
            SettingsCache.Save();
            LowerPriority?.Save();
        }

        public override T GetValue<T>(string name, T defaultValue, Func<string, T> decode)
        {
            TryGetValue(name, defaultValue, decode, out var value);
            return value;
        }

        /// <summary>
        /// sets given value at the possible lowest priority level
        /// </summary>
        public override void SetValue<T>(string name, T value, Func<T, string?> encode)
        {
            if (LowerPriority is null || SettingsCache.HasValue(name))
            {
                SettingsCache.SetValue(name, value, encode);
            }
            else
            {
                LowerPriority.SetValue(name, value, encode);
            }
        }

        public virtual bool TryGetValue<T>(string name, T defaultValue, Func<string, T> decode, out T value)
        {
            if (SettingsCache.TryGetValue(name, defaultValue, decode, out value))
            {
                return true;
            }

            if (LowerPriority is not null && LowerPriority.TryGetValue(name, defaultValue, decode, out value))
            {
                return true;
            }

            return false;
        }
    }
}
