using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI.SettingsDialog
{
    /// <summary>
    /// to jump to a specific page
    /// 
    /// TODO: extend with attributes to jump to specific control on settingspage
    /// </summary>
    public class SettingsPageReference
    {
        private readonly Section _section;
        private readonly Type _settingsPageType;

        /// <summary>
        /// Jump to first page of GitExt settings or Plugin settings
        /// </summary>
        /// <param name="section"></param>
        public SettingsPageReference(Section section)
        {
            _section = section;
        }

        public SettingsPageReference(Type settingsPageType)
        {
            _settingsPageType = settingsPageType;
        }

        public Type SettingsPageType { get { return _settingsPageType; } }

        public enum Section
        {
            GitExtensionsSettings,
            PluginsSettings
        }
    }
}
