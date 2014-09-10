using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;


namespace GitCommands.Settings
{
    public class GitExtSettingsCache : FileSettingsCache
    {
        private readonly XmlSerializableDictionary<string, string> EncodedNameMap = new XmlSerializableDictionary<string, string>();

        public GitExtSettingsCache(string aSettingsFilePath, bool autoSave = true)
            : base(aSettingsFilePath, autoSave)
        {         
        }

        public static GitExtSettingsCache FromCache(string aSettingsFilePath)
        {
            Lazy<GitExtSettingsCache> createSettingsCache = new Lazy<GitExtSettingsCache>(() =>
                {
                    return new GitExtSettingsCache(aSettingsFilePath, true);
                });

            return FileSettingsCache.FromCache(aSettingsFilePath, createSettingsCache);
        }

        public static GitExtSettingsCache Create(string aSettingsFilePath, bool allowCache = true)
        {
            if (allowCache)
                return FromCache(aSettingsFilePath);
            else
                return new GitExtSettingsCache(aSettingsFilePath, false);
        }

        protected override void ClearImpl()
        {
            base.ClearImpl();
            EncodedNameMap.Clear();
        }

        protected override void WriteSettings(string fileName)
        {
            using (System.Xml.XmlTextWriter xtw = new System.Xml.XmlTextWriter(fileName, Encoding.UTF8))
            {
                xtw.Formatting = Formatting.Indented;
                xtw.WriteStartDocument();
                xtw.WriteStartElement("dictionary");

                EncodedNameMap.WriteXml(xtw);
                xtw.WriteEndElement();
            }
        }

        protected override void ReadSettings(string fileName)
        {
            XmlReaderSettings rSettings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                CheckCharacters = false
            };

            using (System.Xml.XmlReader xr = XmlReader.Create(fileName, rSettings))
            {
                EncodedNameMap.ReadXml(xr);
            }
        }

        protected override void SetValueImpl(string key, string value)
        {
            if (value == null)
            {
                EncodedNameMap.Remove(key);
            }
            else
            {
                EncodedNameMap[key] = value;
            }
        }

        protected override string GetValueImpl(string key)
        {
            string value = null;
            EncodedNameMap.TryGetValue(key, out value);
            return value;
        }

    }
}
