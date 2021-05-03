using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using GitExtUtils;
using GitUIPluginInterfaces;

namespace GitCommands.ExternalLinks
{
    public interface IExternalLinksStorage
    {
        /// <summary>
        /// Loads external link definitions from the settings.
        /// </summary>
        IReadOnlyList<ExternalLinkDefinition>? Load(ISettingsSource settingsSource);

        /// <summary>
        /// Saves the provided external link definitions to the settings.
        /// </summary>
        void Save(ISettingsSource settingsSource, IReadOnlyList<ExternalLinkDefinition> definitions);
    }

    public sealed class ExternalLinksStorage : IExternalLinksStorage
    {
        private const string SettingName = "RevisionLinkDefs";

        /// <summary>
        /// Loads external link definitions from the settings.
        /// </summary>
        public IReadOnlyList<ExternalLinkDefinition>? Load(ISettingsSource settingsSource)
        {
            var xml = settingsSource.GetString(SettingName, null);
            return LoadFromXmlString(xml);
        }

        /// <summary>
        /// Saves the provided external link definitions to the settings.
        /// </summary>
        public void Save(ISettingsSource settingsSource, IReadOnlyList<ExternalLinkDefinition> definitions)
        {
            try
            {
                string? xml;
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

                settingsSource.SetString(SettingName, xml);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // TODO: refactor and outsource to the centralised SettingsSerialiser implementations.
        private static IReadOnlyList<ExternalLinkDefinition>? LoadFromXmlString(string? xmlString)
        {
            if (Strings.IsNullOrWhiteSpace(xmlString))
            {
                return Array.Empty<ExternalLinkDefinition>();
            }

            try
            {
                var serializer = new XmlSerializer(typeof(List<ExternalLinkDefinition>));
                using var stringReader = new StringReader(xmlString);
                using var xmlReader = new XmlTextReader(stringReader);
                return serializer.Deserialize(xmlReader) as List<ExternalLinkDefinition>;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return Array.Empty<ExternalLinkDefinition>();
        }
    }
}
