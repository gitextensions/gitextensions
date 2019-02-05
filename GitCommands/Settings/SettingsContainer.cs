﻿using System;
using System.Linq;
using System.Net;
using GitExtUtils;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Settings
{
    public class SettingsContainer<TLowerPriority, TCache> : ISettingsSource where TLowerPriority : SettingsContainer<TLowerPriority, TCache> where TCache : SettingsCache
    {
        [CanBeNull]
        public TLowerPriority LowerPriority { get; }
        [NotNull]
        public TCache SettingsCache { get; }

        public SettingsContainer([CanBeNull] TLowerPriority lowerPriority, [NotNull] TCache settingsCache)
        {
            LowerPriority = lowerPriority;
            SettingsCache = settingsCache;
        }

        public void LockedAction(Action action)
        {
            SettingsCache.LockedAction(() =>
                {
                    if (LowerPriority != null)
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
            foreach (var networkCredentials in Credentials.ToList())
            {
                CredentialsManager.UpdateCredentials(networkCredentials.Key, networkCredentials.Value.UserName,
                    networkCredentials.Value.Password);
            }

            Credentials.Clear();

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
        public override void SetValue<T>(string name, T value, Func<T, string> encode)
        {
            if (LowerPriority == null || SettingsCache.HasValue(name))
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

            if (LowerPriority != null && LowerPriority.TryGetValue(name, defaultValue, decode, out value))
            {
                return true;
            }

            return false;
        }

        public override NetworkCredential GetCredentials(string name, IGitModule gitModule, NetworkCredential defaultValue)
        {
            NetworkCredential result = base.GetCredentials(name, gitModule, null);
            if (result == null && LowerPriority != null)
            {
                return LowerPriority.GetCredentials(name, gitModule, defaultValue);
            }

            return result ?? defaultValue;
        }
    }
}
