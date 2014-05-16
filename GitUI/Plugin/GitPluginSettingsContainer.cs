using System;
using System.Collections.Generic;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI
{
    public class GitPluginSettingsContainer : ISettingsSource
    {
        private readonly string pluginName;
        public GitPluginSettingsContainer(string pluginName)
        {
            this.pluginName = pluginName;
        }

        public T GetValue<T>(string name, T defaultValue, Func<string, T> decode)
        {
            var value = AppSettings.GetString(pluginName + name, null);

            if (value == null)
                return defaultValue;

            return decode(value);
        }

        public void SetValue<T>(string name, T value, Func<T, string> encode)
        {
            AppSettings.SetString(pluginName + name, encode(value));
        }
    }
}
