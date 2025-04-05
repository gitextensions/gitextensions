#nullable enable

using System.Xml;
using System.Xml.Serialization;
using GitCommands.Settings;
using GitUI.NBugReports;

namespace GitUI.ScriptsEngine;

/// <summary>
/// Interface for user scripts storage.
/// </summary>
public interface IUserScriptsStorage
{
    /// <summary>
    ///  Loads external link definitions from the settings.
    /// </summary>
    /// <param name="settings">The settings to load from.</param>
    /// <returns>A list of script information.</returns>
    IReadOnlyList<ScriptInfo> Load(DistributedSettings settings);

    /// <summary>
    ///  Saves the provided script definitions to the settings.
    /// </summary>
    /// <param name="settings">The settings to save to.</param>
    /// <param name="scripts">The scripts to save.</param>
    void Save(DistributedSettings settings, IReadOnlyList<ScriptInfo> scripts);
}

/// <summary>
/// Implementation of <see cref="IUserScriptsStorage"/> that uses XML serialization.
/// </summary>
internal sealed class UserScriptsStorage : IUserScriptsStorage
{
    private static readonly XmlSerializer _serializer = new(typeof(List<ScriptInfo>));
    private const string SettingName = "ownScripts";

    /// <summary>
    ///  Loads external link definitions from the settings.
    /// </summary>
    /// <param name="settings">The settings to load from.</param>
    /// <returns>A list of script information.</returns>
    public IReadOnlyList<ScriptInfo> Load(DistributedSettings settings)
    {
        string? xml = settings.GetString(SettingName, defaultValue: null);
        IReadOnlyList<ScriptInfo> scripts = LoadFromXmlString(xml);
        return scripts;
    }

    /// <summary>
    ///  Saves the provided script definitions to the settings.
    /// </summary>
    /// <param name="settings">The settings to save to.</param>
    /// <param name="scripts">The scripts to save.</param>
    public void Save(DistributedSettings settings, IReadOnlyList<ScriptInfo> scripts)
    {
        string? xml;
        if (scripts.Count == 0)
        {
            xml = null;
        }
        else
        {
            StringWriter sw = new();
            _serializer.Serialize(sw, scripts);
            xml = sw.ToString();
        }

        settings.SetString(SettingName, xml);
    }

    /// <summary>
    ///  Loads script information from an XML string.
    /// </summary>
    /// <param name="xmlString">The XML string to load from.</param>
    /// <returns>A list of script information.</returns>
    /// <exception cref="UserExternalOperationException">Thrown when there is an error during deserialization.</exception>
    private static IReadOnlyList<ScriptInfo> LoadFromXmlString(string? xmlString)
    {
        if (string.IsNullOrWhiteSpace(xmlString))
        {
            return [];
        }

        using StringReader stringReader = new(xmlString);
        using XmlTextReader xmlReader = new(stringReader);

        try
        {
            return (List<ScriptInfo>)_serializer.Deserialize(xmlReader)!;
        }
        catch (Exception ex)
        {
            throw new UserExternalOperationException(ex);
        }
    }
}
