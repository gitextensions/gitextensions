using System.Collections.Generic;

namespace GitUIPluginInterfaces
{
    public interface IConfigFileSettings : ISettingsValueGetter
    {
        /// <summary>
        /// Adds the specific configuration section to the .git/config file.
        /// </summary>
        /// <param name="configSection">The configuration section.</param>
        void AddConfigSection(IConfigSection configSection);

        /// <summary>
        /// Retrieves configuration sections the .git/config file.
        /// </summary>
        IReadOnlyList<IConfigSection> GetConfigSections();

        /// <summary>
        /// Removes the specific configuration section from the .git/config file.
        /// </summary>
        /// <param name="configSectionName">The name of the configuration section.</param>
        /// <param name="performSave">If <see langword="true"/> the configuration changes will be saved immediately.</param>
        void RemoveConfigSection(string configSectionName, bool performSave = false);

        /// <summary>
        /// Save pending changes.
        /// </summary>
        void Save();
    }
}