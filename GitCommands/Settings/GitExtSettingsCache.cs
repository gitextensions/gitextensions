using System;
using System.Text;
using System.Xml;

namespace GitCommands.Settings
{
    public class GitExtSettingsCache : FileSettingsCache
    {
        private readonly XmlSerializableDictionary<string, string> _encodedNameMap = new XmlSerializableDictionary<string, string>();

        public GitExtSettingsCache(string settingsFilePath, bool autoSave = true)
            : base(settingsFilePath, autoSave)
        {
        }

        public static GitExtSettingsCache FromCache(string settingsFilePath)
        {
            var createSettingsCache = new Lazy<GitExtSettingsCache>(
                () => new GitExtSettingsCache(settingsFilePath, autoSave: true));

            return FromCache(settingsFilePath, createSettingsCache);
        }

        public static GitExtSettingsCache Create(string settingsFilePath, bool allowCache = true)
        {
            if (allowCache)
            {
                return FromCache(settingsFilePath);
            }
            else
            {
                return new GitExtSettingsCache(settingsFilePath, autoSave: false);
            }
        }

        protected override void ClearImpl()
        {
            _encodedNameMap.Clear();
        }

        protected override void WriteSettings(string fileName)
        {
            using (var xtw = new XmlTextWriter(fileName, Encoding.UTF8) { Formatting = Formatting.Indented })
            {
                xtw.WriteStartDocument();
                xtw.WriteStartElement("dictionary");
                _encodedNameMap.WriteXml(xtw);
                xtw.WriteEndElement();
            }
        }

        protected override void ReadSettings(string fileName)
        {
            var readerSettings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                CheckCharacters = false
            };

            using (var xr = XmlReader.Create(fileName, readerSettings))
            {
                try
                {
                    _encodedNameMap.ReadXml(xr);
                }
                catch (Exception e)
                {
                    throw new Exception($"Exception reading XML file \"{fileName}\": {e.Message}", e);
                }
            }
        }

        protected override void SetValueImpl(string key, string value)
        {
            if (value == null)
            {
                _encodedNameMap.Remove(key);
            }
            else
            {
                _encodedNameMap[key] = value;
            }
        }

        protected override string GetValueImpl(string key)
        {
            _encodedNameMap.TryGetValue(key, out var value);
            return value;
        }
    }
}
