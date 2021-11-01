using System;
using System.Linq;
using GitUIPluginInterfaces;

namespace GitCommands.Settings
{
    /// <summary>
    /// Implements the logic for working with the composition of caches.
    /// When disposing a class, the caches will be disposed as well.
    /// </summary>
    /// <typeparam name="TCache">Cache type</typeparam>
    public class CompositeSettingsContainer<TCache> : ISettingsSource, IDisposable
        where TCache : SettingsCache
    {
        private readonly ICredentialsManager _credentialsManager = new CredentialsManager();

        protected TCache[] SettingsCaches;

        public CompositeSettingsContainer(params TCache[] settingsCaches)
        {
            if (!settingsCaches.Any())
            {
                throw new ArgumentException($"{nameof(settingsCaches)} must contain at least one value.");
            }

            if (settingsCaches.Contains(null))
            {
                throw new ArgumentException($"{nameof(settingsCaches)} should not contain null value.");
            }

            SettingsCaches = settingsCaches;
        }

        public void LockedAction(Action action)
        {
            foreach (TCache settingsCache in SettingsCaches.Reverse())
            {
                action = () => settingsCache.LockedAction(action);
            }

            action();
        }

        public void Save()
        {
            _credentialsManager.Save();

            foreach (TCache settingsCache in SettingsCaches)
            {
                settingsCache.Save();
            }
        }

        public override string? GetValue(string name)
        {
            TryGetValue(name, out string? value);

            return value;
        }

        /// <summary>
        /// sets given value at the possible lowest priority level
        /// </summary>
        public override void SetValue(string name, string? value)
        {
            foreach (TCache settingsCache in SettingsCaches)
            {
                if (settingsCache.HasValue(name))
                {
                    settingsCache.SetValue(name, value);

                    return;
                }
            }

            SettingsCaches.Last()
                .SetValue(name, value);
        }

        public virtual bool TryGetValue(string name, out string? value)
        {
            value = default;

            foreach (TCache settingsCache in SettingsCaches)
            {
                if (settingsCache.TryGetValue(name, out value))
                {
                    return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (TCache settingsCache in SettingsCaches)
                {
                    settingsCache.Dispose();
                }
            }
        }
    }
}
