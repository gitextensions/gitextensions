using System.IO;
using System.Xml;
using Microsoft.Extensions.Configuration;

namespace GitCommands.Settings
{
    internal sealed class GitExtensionConfigurationProvider : FileConfigurationProvider
    {
        public GitExtensionConfigurationProvider(GitExtensionConfigurationSource source)
            : base(source)
        {
        }

        public override void Load(Stream stream)
        {
            var readerSettings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                CheckCharacters = false
            };

            using var reader = XmlReader.Create(stream, readerSettings);

            var encodedNameMap = new XmlSerializableDictionary<string, string>();

            encodedNameMap.ReadXml(reader);

            Data = encodedNameMap;
        }
    }
}
