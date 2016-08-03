﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces;

namespace GitCommands.Settings
{
    public class SettingsContainer<L, C> : ISettingsSource where L : SettingsContainer<L, C> where C : SettingsCache
    {
        public L LowerPriority { get; private set; }
        public C SettingsCache { get; private set; }

        public SettingsContainer(L aLowerPriority, C aSettingsCache)
        {
            LowerPriority = aLowerPriority;
            SettingsCache = aSettingsCache;
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
            SettingsCache.Save();

            if (LowerPriority != null)
            {
                LowerPriority.Save();
            }
        }

        public override T GetValue<T>(string name, T defaultValue, Func<string, T> decode)
        {
            T value;

            TryGetValue(name, defaultValue, decode, out value);

            return value;
        }

        /// <summary>
        /// sets given value at the possible lowest priority level
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="encode"></param>
        public override void SetValue<T>(string name, T value, Func<T, string> encode)
        {
            if (LowerPriority == null || SettingsCache.HasValue(name))
                SettingsCache.SetValue(name, value, encode);
            else
                LowerPriority.SetValue(name, value, encode);
        }

        public virtual bool TryGetValue<T>(string name, T defaultValue, Func<string, T> decode, out T value)
        {
            if (SettingsCache.TryGetValue<T>(name, defaultValue, decode, out value))
                return true;

            if (LowerPriority != null && LowerPriority.TryGetValue(name, defaultValue, decode, out value))
                return true;

            return false;
        }

    }
}
