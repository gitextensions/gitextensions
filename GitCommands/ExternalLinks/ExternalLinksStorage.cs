using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using GitCommands.Settings;
using JetBrains.Annotations;

namespace GitCommands.ExternalLinks
{
    public interface IExternalLinksStorage
    {
        /// <summary>
        /// Loads external link definitions from the settings.
        /// </summary>
        IReadOnlyList<ExternalLinkDefinition> Load(RepoDistSettings settings);

        /// <summary>
        /// Saves the provided external link definitions to the settings.
        /// </summary>
        void Save(RepoDistSettings settings, IReadOnlyList<ExternalLinkDefinition> definitions);
    }

    public sealed class ExternalLinksStorage : IExternalLinksStorage
    {
        private const string SettingName = "RevisionLinkDefs";

        /// <summary>
        /// Loads external link definitions from the settings.
        /// </summary>
        public IReadOnlyList<ExternalLinkDefinition> Load(RepoDistSettings settings)
        {
            var cachedSettings = new RepoDistSettings(null, settings.SettingsCache);
            var xml = cachedSettings.GetString(SettingName, null);
            return LoadFromXmlString(xml);
        }

        /// <summary>
        /// Saves the provided external link definitions to the settings.
        /// </summary>
        public void Save(RepoDistSettings settings, IReadOnlyList<ExternalLinkDefinition> definitions)
        {
            try
            {
                string xml;
                if (definitions.Count == 0)
                {
                    xml = null;
                }
                else
                {
                    foreach (var definition in definitions)
                    {
                        definition.RemoveEmptyFormats();
                    }

                    var sw = new StringWriter();
                    var serializer = new XmlSerializer(typeof(List<ExternalLinkDefinition>));
                    var ns = new XmlSerializerNamespaces();
                    ns.Add(string.Empty, string.Empty);
                    serializer.Serialize(sw, definitions.OrderBy(x => x.Name).ToList(), ns);
                    xml = sw.ToString();
                }

                var cachedSettings = new RepoDistSettings(null, settings.SettingsCache);
                cachedSettings.SetString(SettingName, xml);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // TODO: refactor and outsource to the centralised SettingsSerialiser implementations.
        [CanBeNull]
        private static IReadOnlyList<ExternalLinkDefinition> LoadFromXmlString(string xmlString)
        {
            if (string.IsNullOrWhiteSpace(xmlString))
            {
                return Array.Empty<ExternalLinkDefinition>();
            }

            try
            {
                var serializer = new XmlSerializer(typeof(List<ExternalLinkDefinition>));
                using (var stringReader = new StringReader(xmlString))
                {
                    using (var xmlReader = new XmlTextReader(stringReader))
                    {
                        return serializer.Deserialize(xmlReader) as List<ExternalLinkDefinition>;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return Array.Empty<ExternalLinkDefinition>();
        }
    }
}