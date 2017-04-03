using System;
using System.Collections.Generic;

namespace GitUIPluginInterfaces
{
    public interface IConfigFileSettings : ISettingsValueGetter
    {
        /// <summary>
        /// Retrieves configuration sections the .git/config file.
        /// </summary>
        IList<IConfigSection> GetConfigSections();

        /// <summary>
        /// Save pending changes.
        /// </summary>
        void Save();
    }
}