using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using GitCommands.Settings;

namespace GitCommands.ExternalLinks
{
    public interface IExternalLinksStorage
    {
        /// <summary>
        /// Loads external link definitions from the settings.
        /// </summary>
        IReadOnlyList<ExternalLinkDefinition>? Load(RepoDistSettings settings);

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
        public IReadOnlyList<ExternalLinkDefinition>? Load(RepoDistSettings settings)
        {
            var xml = settings.GetString(SettingName, null);
            return LoadFromXmlString(xml);
        }

        /// <summary>
        /// Saves the provided external link definitions to the settings.
        /// </summary>
        public void Save(RepoDistSettings settings, IReadOnlyList<ExternalLinkDefinition> definitions)
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

                    StringWriter sw = new();
                    XmlSerializer serializer = new(typeof(List<ExternalLinkDefinition>));
                    XmlSerializerNamespaces ns = new();
                    ns.Add(string.Empty, string.Empty);
                    serializer.Serialize(sw, definitions.OrderBy(x => x.Name).ToList(), ns);
                    xml = sw.ToString();
                }

                settings.SetString(SettingName, xml);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // TODO: refactor and outsource to the centralised SettingsSerializer implementations.
        private static IReadOnlyList<ExternalLinkDefinition>? LoadFromXmlString(string? xmlString)
        {
            if (string.IsNullOrWhiteSpace(xmlString))
            {
                return Array.Empty<ExternalLinkDefinition>();
            }

            try
            {
                XmlSerializer serializer = new(typeof(List<ExternalLinkDefinition>));
                using StringReader stringReader = new(xmlString);
                using XmlTextReader xmlReader = new(stringReader);
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
