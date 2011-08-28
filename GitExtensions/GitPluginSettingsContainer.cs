using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using GitCommands;

namespace GitExtensions
{
    public class GitPluginSettingsContainer : IGitPluginSettingsContainer
    {
        private readonly string pluginName;
        public GitPluginSettingsContainer(string pluginName)
        {
            this.pluginName = pluginName;
        }

        readonly Dictionary<string, string> settings = new Dictionary<string, string>();

        public void AddSetting(string name, string defaultValue)
        {
            var value = Settings.GetValue<string>(pluginName + name, null);

            if (value == null)
            {
                settings.Add(name, defaultValue);
                Settings.SetValue(pluginName + name, defaultValue);
            }
            else
            {
                settings.Add(name, value);
            }
        }

        public void SetSetting(string name, string value)
        {
            if (!settings.ContainsKey(name))
                throw new ArgumentOutOfRangeException("name", "Cannot find setting. Dit you add the setting in the Register() function of the plugin?");

            settings[name] = value;

            Settings.SetValue(pluginName + name, value);
        }

        public string GetSetting(string name) 
        {
            if (!settings.ContainsKey(name))
                throw new ArgumentOutOfRangeException("name", "Cannot find setting. Dit you add the setting in the Register() function of the plugin?");

            var value = Settings.GetValue<string>(pluginName + name, null);
            
            if (value == null)
                return settings[name];

            return value;
        }

        public IList<string> GetAvailableSettings()
        {
            IList<string> keys = new List<string>(settings.Keys.Count);
            foreach (string key in settings.Keys)
            {
                keys.Add(key);
            }

            return keys;
        }
    }
}
