﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GitUIPluginInterfaces;

namespace GitCommands.Config
{
    public class ConfigFile
    {
        public string FileName { get; }

        public bool Local { get; }

        public ConfigFile(string fileName, bool aLocal)
        {
            ConfigSections = new List<IConfigSection>();
            Local = aLocal;

            FileName = fileName;
            try
            {
                Load();
            }
            catch (Exception ex)
            {
                ex.Data.Add(GetType().Name + ".Load", "Could not load config file: " + FileName);
                throw;
            }
        }

        public IList<IConfigSection> ConfigSections { get; }

        public IEnumerable<IConfigSection> GetConfigSections(string sectionName)
        {
            return ConfigSections.Where(section => section.SectionName.Equals(sectionName, StringComparison.OrdinalIgnoreCase));
        }

        private static Encoding GetEncoding()
        {
            return GitModule.SystemEncoding;
        }

        private void Load()
        {
            if (string.IsNullOrEmpty(Path.GetFileName(FileName)) || !File.Exists(FileName))
                return;

            ConfigFileParser parser = new ConfigFileParser(this);
            parser.Parse();
        }

        public static readonly char[] CommentChars = { ';', '#' };

        public void LoadFromString(string str)
        {
            ConfigFileParser parser = new ConfigFileParser(this);
            parser.Parse(str);
        }


        public static string EscapeValue(string value)
        {
            value = value.Replace("\\", "\\\\");
            value = value.Replace("\"", "\\\"");
            value = value.Replace("\n", "\\n");
            value = value.Replace("\t", "\\t");

            if (value.IndexOfAny(CommentChars) != -1 || !value.Trim().Equals(value))
                value = value.Quote();

            return value;
        }

        public void Save()
        {
            Save(FileName);
        }

        public string GetAsString()
        {
            var configFileContent = new StringBuilder();

            foreach (var section in ConfigSections)
            {
                var keys = section.AsDictionary();
                //Skip empty sections
                if (keys.Count == 0)
                {
                    continue;
                }
                configFileContent.Append(section);
                configFileContent.Append(Environment.NewLine);

                foreach (var key in keys)
                {
                    foreach (var value in key.Value)
                    {
                        configFileContent.AppendLine(string.Concat("\t", key.Key, " = ", EscapeValue(value)));
                    }
                }
            }

            return configFileContent.ToString();
        }

        public void Save(string fileName)
        {
            try
            {
                FileInfoExtensions.MakeFileTemporaryWritable(fileName, x => File.WriteAllText(fileName, GetAsString(), GetEncoding()));
            }
            catch (Exception ex)
            {
                ExceptionUtils.ShowException(ex, false);
            }
        }

        private void SetStringValue(string setting, string value)
        {
            var keyIndex = FindAndCheckKeyIndex(setting);

            var configSectionName = setting.Substring(0, keyIndex);
            var keyName = setting.Substring(keyIndex + 1);

            FindOrCreateConfigSection(configSectionName).SetValue(keyName, value);
        }

        public void SetValue(string setting, string value)
        {
            SetStringValue(setting, value);
        }

        public void SetPathValue(string setting, string value)
        {
            SetStringValue(setting, ConfigSection.FixPath(value));
        }

        public bool HasValue(string setting)
        {
            var keyIndex = FindAndCheckKeyIndex(setting);

            var configSectionName = setting.Substring(0, keyIndex);
            var keyName = setting.Substring(keyIndex + 1);

            var configSection = FindConfigSection(configSectionName);
            return configSection != null && configSection.GetValue(keyName) != string.Empty;
        }

        private static int FindAndCheckKeyIndex(string setting)
        {
            var keyIndex = FindKeyIndex(setting);

            if (keyIndex < 0 || keyIndex == setting.Length)
                throw new Exception("Invalid setting name: " + setting);

            return keyIndex;
        }

        private static int FindKeyIndex(string setting)
        {
            return setting.LastIndexOf('.');
        }

        public bool HasConfigSection(string configSectionName)
        {
            return FindConfigSection(configSectionName) != null;
        }

        public string GetValue(string setting, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(setting))
                throw new ArgumentNullException();

            var keyIndex = FindAndCheckKeyIndex(setting);

            var configSectionName = setting.Substring(0, keyIndex);
            var keyName = setting.Substring(keyIndex + 1);

            var configSection = FindConfigSection(configSectionName);

            if (configSection == null)
                return defaultValue;

            return configSection.GetValue(keyName, defaultValue);
        }

        public string GetPathValue(string setting)
        {
            return GetValue(setting);
        }

        public IList<string> GetValues(string setting)
        {
            var keyIndex = FindAndCheckKeyIndex(setting);

            var configSectionName = setting.Substring(0, keyIndex);
            var keyName = setting.Substring(keyIndex + 1);

            var configSection = FindConfigSection(configSectionName);

            if (configSection == null)
                return new List<string>();

            return configSection.GetValues(keyName);
        }

        public void RemoveSetting(string setting)
        {
            var keyIndex = FindAndCheckKeyIndex(setting);

            var configSectionName = setting.Substring(0, keyIndex);
            var keyName = setting.Substring(keyIndex + 1);

            var configSection = FindConfigSection(configSectionName);

            configSection?.SetValue(keyName, null);
        }

        public IConfigSection FindOrCreateConfigSection(string name)
        {
            var result = FindConfigSection(name);
            if (result == null)
            {
                result = new ConfigSection(name, true);
                ConfigSections.Add(result);
            }

            return result;
        }

        public void AddConfigSection(IConfigSection configSection)
        {
            if (FindConfigSection(configSection) != null)
                throw new ArgumentException("Can not add a section that already exists: " + configSection.SectionName);

            ConfigSections.Add(configSection);
        }

        public void RemoveConfigSection(string configSectionName)
        {
            var configSection = FindConfigSection(configSectionName);

            if (configSection == null)
                return;

            ConfigSections.Remove(configSection);
        }

        public void RemoveConfigSections(string configSectionName)
        {
            var toRemove = GetConfigSections(configSectionName).ToArray();
            toRemove.ForEach(section => ConfigSections.Remove(section));
        }

        public IConfigSection FindConfigSection(string name)
        {
            var configSectionToFind = new ConfigSection(name, true);

            return FindConfigSection(configSectionToFind);
        }

        private IConfigSection FindConfigSection(IConfigSection configSectionToFind)
        {
            foreach (var configSection in ConfigSections)
            {
                if (configSectionToFind.Equals(configSection))
                    return configSection;
            }
            return null;
        }

        #region ConfigFileParser

        private class ConfigFileParser
        {
            private delegate ParsePart ParsePart(char c);

            private readonly ConfigFile _configFile;
            private string _fileContent;
            private IConfigSection _section;
            private string FileName => _configFile.FileName;

            private string _key;
            //parsed char
            private int pos;
            private readonly StringBuilder token = new StringBuilder();
            private readonly StringBuilder valueToken = new StringBuilder();

            public ConfigFileParser(ConfigFile configFile)
            {
                _configFile = configFile;
            }

            public void Parse(string aFileContent = null)
            {
                _fileContent = aFileContent ?? File.ReadAllText(FileName, GetEncoding());

                ParsePart parseFunc = ReadUnknown;

                for (pos = 0; pos < _fileContent.Length; pos++)
                {
                    parseFunc = parseFunc(_fileContent[pos]);
                }

                if (_fileContent.Length > 0 && !_fileContent.EndsWith("\n"))
                {
                    parseFunc('\n');
                }
            }

            private void NewSection()
            {
                var sectionName = token.ToString();
                token.Clear();
                _section = _configFile.FindConfigSection(sectionName);
                if (_section == null)
                {
                    _section = new ConfigSection(sectionName, false);
                    _configFile.ConfigSections.Add(_section);
                }
            }

            private void NewKey()
            {
                _key = token.ToString().Trim();
                token.Clear();

                if (_section == null)
                    throw new Exception(
                        string.Format("Key {0} in configfile {1} is not in a section.", _key, FileName));
            }

            private void NewValue()
            {
                token.Append(valueToken.ToString().Trim());
                valueToken.Clear();

                string value = token.ToString();

                if (_key.IsNullOrEmpty())
                    throw new Exception(
                        string.Format("Value {0} for empty key in configfile {1}.", value, FileName));

                _section.AddValue(_key, value);

                _key = null;
            }

            private bool _escapedSection;
            private bool _quotedStringInSection;

            private ParsePart ReadSection(char c)
            {
                if (_escapedSection)
                {
                    switch (c)
                    {
                        case '\\':
                        case '"':
                            token.Append(c);
                            break;
                        case 't':
                            token.Append('\t');
                            break;
                        default:
                            throw new Exception("Invalid escape character: " + Regex.Escape(c.ToString()));
                    }
                    _escapedSection = false;
                    return ReadSection;
                }

                // closing square bracket not in quoted section lead to start new section
                if(c == ']' && !_quotedStringInSection)
                {
                    NewSection();
                    return ReadUnknown;
                }

                if (c == '"')
                    _quotedStringInSection = !_quotedStringInSection;

                switch (c)
                {
                    case '\\':
                        _escapedSection = true;
                        return ReadSection;
                    default:
                        token.Append(c);
                        return ReadSection;
                }
            }

            private ParsePart ReadComment(char c)
            {
                switch (c)
                {
                    case '\n':
                        //check for line continuation
                        if (token.Length > 0 && token[token.Length - 1] == '\\')
                        {
                            token.Remove(token.Length - 1, 1);
                            return ReadComment;
                        }
                        else
                            return ReadUnknown;
                    default:
                        token.Append(c);
                        return ReadComment;
                }
            }

            private ParsePart ReadKey(char c)
            {
                switch (c)
                {
                    case '=':
                        NewKey();
                        return ReadValue;
                    case '\n':
                        NewKey();
                        token.Append("true");
                        NewValue();
                        return ReadUnknown;
                    default:
                        token.Append(c);
                        return ReadKey;
                }
            }

            private bool _quotedValue;
            private bool _escapedValue;

            private ParsePart ReadValue(char c)
            {
                if (_escapedValue)
                {
                    switch (c)
                    {
                        case '\\':
                        case '"':
                            valueToken.Append(c);
                            break;
                        case 't':
                            valueToken.Append('\t');
                            break;
                        case 'n':
                            valueToken.Append('\n');
                            break;
                        case '\r':
                            return ReadValue;
                        case '\n':
                            //line continuation
                            break;

                        default:
                            throw new Exception("Invalid escape character: " + Regex.Escape(c.ToString()));
                    }
                    _escapedValue = false;
                    return ReadValue;
                }

                switch (c)
                {
                    case '\\':
                        _escapedValue = true;
                        return ReadValue;
                    case '"':
                        if (_quotedValue)
                        {
                            token.Append(valueToken);
                        }
                        else
                        {
                            token.Append(valueToken.ToString().Trim());
                        }
                        valueToken.Clear();
                        _quotedValue = !_quotedValue;
                        return ReadValue;
                    case ';':
                    case '#':
                        if (_quotedValue)
                        {
                            valueToken.Append(c);
                            return ReadValue;
                        }
                        NewValue();
                        return ReadComment;
                    case '\r':
                        return ReadValue;
                    case '\n':
                        NewValue();
                        return ReadUnknown;
                    default:
                        valueToken.Append(c);
                        return ReadValue;
                }
            }

            private ParsePart ReadUnknown(char c)
            {
                token.Clear();

                switch (c)
                {
                    case '[':
                        return ReadSection;
                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                        return ReadUnknown;
                    case ';':
                    case '#':
                        return ReadComment;
                    default:
                        token.Append(c);
                        return ReadKey;
                }
            }

        }
        #endregion
    }
}
