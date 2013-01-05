using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUI.Plugin;
using GitUI.SettingsDialog.Plugins;

namespace GitUI.SettingsDialog
{
    /// <summary>
    /// For GE and Plugin settings pages
    /// </summary>
    public class SettingsPageRegistry
    {
        private readonly IList<ISettingsPage> _settingsPageCollection = new List<ISettingsPage>();
        private readonly IList<ISettingsPage> _pluginSettingsPageCollection = new List<ISettingsPage>();

        public void RegisterSettingsPage(ISettingsPage settingsPage)
        {
            _settingsPageCollection.Add(settingsPage);
        }

        public IEnumerable<ISettingsPage> GetSettingsPages()
        {
            return _settingsPageCollection;
        }

        /// <summary>
        /// Register or reregister list of plugin settings pages
        /// </summary>
        public void RegisterPluginSettingsPages()
        {
            foreach (var gitPlugin in LoadedPlugins.Plugins)
            {
                var settingsPage = PluginSettingsPage.CreateSettingsPageFromPlugin(gitPlugin);
                _pluginSettingsPageCollection.Add(settingsPage);
            }
        }

        public IEnumerable<ISettingsPage> GetPluginSettingsPages()
        {
            return _pluginSettingsPageCollection;
        }

        public IEnumerable<ISettingsPage> GetAllSettingsPages()
        {
            return GetSettingsPages().Concat(GetPluginSettingsPages());
        }

    }
}
