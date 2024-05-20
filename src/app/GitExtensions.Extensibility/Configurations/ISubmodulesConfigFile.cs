namespace GitExtensions.Extensibility.Configurations;

/// <summary>
///  Provides the ability to access the .gitmodules config file.
/// </summary>
public interface ISubmodulesConfigFile
{
    IReadOnlyList<IConfigSection> ConfigSections { get; }

    string GetPathValue(string setting);

    void RemoveConfigSection(string configSectionName);

    void Save();
}
