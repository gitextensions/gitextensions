﻿using System;
using System.Text;
using System.Xml;

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

            return FromCache(aSettingsFilePath, createSettingsCache);
        }

        public static GitExtSettingsCache Create(string aSettingsFilePath, bool allowCache = true)
        {
            if (allowCache)
                return FromCache(aSettingsFilePath);
            return new GitExtSettingsCache(aSettingsFilePath, false);
        }

        protected override void ClearImpl()
        {
            EncodedNameMap.Clear();
        }

        protected override void WriteSettings(string fileName)
        {
            using (XmlTextWriter xtw = new XmlTextWriter(fileName, Encoding.UTF8))
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

            using (XmlReader xr = XmlReader.Create(fileName, rSettings))
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
            EncodedNameMap.TryGetValue(key, out var value);
            return value;
        }
    }
}
