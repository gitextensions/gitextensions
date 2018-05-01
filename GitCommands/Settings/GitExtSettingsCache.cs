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
                () => new GitExtSettingsCache(settingsFilePath, true));

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
                return new GitExtSettingsCache(settingsFilePath, false);
            }
        }

        protected override void ClearImpl()
        {
            _encodedNameMap.Clear();
        }

        protected override void WriteSettings(string fileName)
        {
            using (XmlTextWriter xtw = new XmlTextWriter(fileName, Encoding.UTF8))
            {
                xtw.Formatting = Formatting.Indented;
                xtw.WriteStartDocument();
                xtw.WriteStartElement("dictionary");

                _encodedNameMap.WriteXml(xtw);
                xtw.WriteEndElement();
            }
        }

        protected override void ReadSettings(string fileName)
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                CheckCharacters = false
            };

            using (XmlReader xr = XmlReader.Create(fileName, readerSettings))
            {
                _encodedNameMap.ReadXml(xr);
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
