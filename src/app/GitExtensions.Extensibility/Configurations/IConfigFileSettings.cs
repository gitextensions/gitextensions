#nullable enable

namespace GitExtensions.Extensibility.Configurations;

public interface IConfigFileSettings : IPersistentConfigValueStore
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
    ///  Retrieves all configured values of a git setting that accepts multiple values for the same key.
    /// </summary>
    /// <param name="setting">The git setting key.</param>
    /// <returns>The collection of all the <see cref="string"/> values.</returns>
    IReadOnlyList<string> GetValues(string setting);

    /// <summary>
    /// Removes the specific configuration section from the .git/config file.
    /// </summary>
    /// <param name="configSectionName">The name of the configuration section.</param>
    /// <param name="performSave">If <see langword="true"/> the configuration changes will be saved immediately.</param>
    void RemoveConfigSection(string configSectionName, bool performSave = false);
}
