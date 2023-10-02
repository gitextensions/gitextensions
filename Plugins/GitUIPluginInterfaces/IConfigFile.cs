namespace GitUIPluginInterfaces;

/// <summary>
///  Provides the ability to access the config file.
/// </summary>
public interface IConfigFile : ISubmodulesConfigFile
{
    string FileName { get; }

    void AddConfigSection(IConfigSection configSection);

    IConfigSection? FindConfigSection(string name);

    IConfigSection FindOrCreateConfigSection(string name);

    string GetAsString();

    IEnumerable<IConfigSection> GetConfigSections(string sectionName);

    string GetValue(string setting, string defaultValue);

    IReadOnlyList<string> GetValues(string setting);

    void LoadFromString(string str);

    void Save(string fileName);

    void SetValue(string setting, string value);
}
