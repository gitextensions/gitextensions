using System.Diagnostics;
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
        IReadOnlyList<ExternalLinkDefinition>? Load(DistributedSettings settings);

        /// <summary>
        /// Saves the provided external link definitions to the settings.
        /// </summary>
        void Save(DistributedSettings settings, IReadOnlyList<ExternalLinkDefinition> definitions);
    }

    public sealed class ExternalLinksStorage : IExternalLinksStorage
    {
        private const string SettingName = "RevisionLinkDefs";

        /// <summary>
        /// Loads external link definitions from the settings.
        /// </summary>
        public IReadOnlyList<ExternalLinkDefinition>? Load(DistributedSettings settings)
        {
            var xml = settings.GetString(SettingName, null);
            return LoadFromXmlString(xml);
        }

        /// <summary>
        /// Saves the provided external link definitions to the settings.
        /// </summary>
        public void Save(DistributedSettings settings, IReadOnlyList<ExternalLinkDefinition> definitions)
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

                    XmlSerializer serializer = new(typeof(List<ExternalLinkDefinition>));
                    XmlSerializerNamespaces ns = new();
                    ns.Add(string.Empty, string.Empty);

                    XmlWriterSettings xmlWriterSettings = new()
                    {
                        Indent = true
                    };
                    using StringWriter sw = new();
                    using XmlWriter xmlWriter = XmlWriter.Create(sw, xmlWriterSettings);

                    serializer.Serialize(xmlWriter, definitions.OrderBy(x => x.Name).ToList(), ns);
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
