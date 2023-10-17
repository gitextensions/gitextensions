using System.Xml;
using System.Xml.Serialization;
using GitCommands.Settings;

namespace GitUI.ScriptsEngine
{
    public interface IUserScriptsStorage
    {
        /// <summary>
        /// Loads external link definitions from the settings.
        /// </summary>
        IReadOnlyList<ScriptInfo> Load(DistributedSettings settings);

        /// <summary>
        /// Saves the provided external link definitions to the settings.
        /// </summary>
        void Save(DistributedSettings settings, IReadOnlyList<ScriptInfo> scripts);
    }

    internal sealed class UserScriptsStorage : IUserScriptsStorage
    {
        private static readonly XmlSerializer _serializer = new(typeof(List<ScriptInfo>));
        private const string SettingName = "ownScripts";

        /// <summary>
        /// Loads external link definitions from the settings.
        /// </summary>
        public IReadOnlyList<ScriptInfo>? Load(DistributedSettings settings)
        {
            var xml = settings.GetString(SettingName, null);
            var scripts = LoadFromXmlString(xml);
            return scripts;
        }

        /// <summary>
        /// Saves the provided external link definitions to the settings.
        /// </summary>
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

        // TODO: refactor and outsource to the centralised SettingsSerializer implementations.
        private static IReadOnlyList<ScriptInfo> LoadFromXmlString(string? xmlString)
        {
            if (string.IsNullOrWhiteSpace(xmlString))
            {
                return Array.Empty<ScriptInfo>();
            }

            using StringReader stringReader = new(xmlString);
            using XmlTextReader xmlReader = new(stringReader);
            return (List<ScriptInfo>)_serializer.Deserialize(xmlReader);
        }
    }
}
