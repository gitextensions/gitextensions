using System;
using System.Collections.Generic;
using System.Text;
using GitUI;
using System.Collections;
using System.Windows.Forms;

namespace GitExtensions
{
    public class GitPluginSettingsContainer : IGitPluginSettingsContainer
    {
        private string pluginName;
        public GitPluginSettingsContainer(string pluginName)
        {
            this.pluginName = pluginName;
        }

        Dictionary<string, string> settings = new Dictionary<string, string>();

        public void AddSetting(string name, string defaultValue)
        {
            string value = Application.UserAppDataRegistry.GetValue(name) as string;

            if (value == null)
            {
                settings.Add(name, defaultValue);
                Application.UserAppDataRegistry.SetValue(pluginName + name, defaultValue);
            }
            else
            {
                settings.Add(name, value);
            }
        }

        public void SetSetting(string name, string value)
        {
            if (!settings.ContainsKey(name))
                throw new ArgumentOutOfRangeException("Cannot find setting. Dit you add the setting in the Register() function of the plugin?");

            settings[name] = value;

            Application.UserAppDataRegistry.SetValue(pluginName + name, value);
        }

        public string GetSetting(string name) 
        {
            if (!settings.ContainsKey(name))
                throw new ArgumentOutOfRangeException("Cannot find setting. Dit you add the setting in the Register() function of the plugin?");

            string value = Application.UserAppDataRegistry.GetValue(pluginName + name) as string;
            
            if (value == null)
                return settings[name];

            return (string)value;
        }
    }
}
