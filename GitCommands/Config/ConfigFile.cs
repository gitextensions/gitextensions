using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Config
{
    public class ConfigFile
    {
        private static Encoding GetEncoding() => GitModule.SystemEncoding;
        public static readonly char[] CommentChars = { ';', '#' };

        private readonly List<IConfigSection> _configSections = new List<IConfigSection>();

        public string FileName { get; }
        public bool Local { get; }

        public ConfigFile(string fileName, bool local)
        {
            Local = local;
            FileName = fileName;

            try
            {
                if (!string.IsNullOrEmpty(Path.GetFileName(FileName)) && File.Exists(FileName))
                {
                    new ConfigFileParser(this).Parse();
                }
            }
            catch (Exception ex)
            {
                ex.Data.Add(GetType().Name + ".Load", "Could not load config file: " + FileName);
                throw;
            }
        }

        public IReadOnlyList<IConfigSection> ConfigSections => _configSections;

        public IEnumerable<IConfigSection> GetConfigSections(string sectionName)
        {
            return ConfigSections.Where(section => section.SectionName.Equals(sectionName, StringComparison.OrdinalIgnoreCase));
        }

        public void LoadFromString(string str)
        {
            new ConfigFileParser(this).Parse(str);
        }

        public static string EscapeValue(string value)
        {
            value = value.Replace("\\", "\\\\");
            value = value.Replace("\"", "\\\"");
            value = value.Replace("\n", "\\n");
            value = value.Replace("\t", "\\t");

            if (value.IndexOfAny(CommentChars) != -1 || value.Trim() != value)
            {
                value = value.Quote();
            }

            return value;
        }

        public void Save()
        {
            Save(FileName);
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

        public string GetAsString()
        {
            var configFileContent = new StringBuilder();

            foreach (var section in ConfigSections)
            {
                var dic = section.AsDictionary();

                // Skip empty sections
                if (dic.Count == 0)
                {
                    continue;
                }

                configFileContent.Append(section);
                configFileContent.Append(Environment.NewLine);

                foreach (var (key, values) in dic)
                {
                    foreach (var value in values)
                    {
                        configFileContent.AppendLine(string.Concat("\t", key, " = ", EscapeValue(value)));
                    }
                }
            }

            return configFileContent.ToString();
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
            {
                throw new Exception("Invalid setting name: " + setting);
            }

            return keyIndex;
        }

        private static int FindKeyIndex(string setting)
        {
            return setting.LastIndexOf('.');
        }

        public bool HasConfigSection(string configSectionName)
        {
            var configSection = FindConfigSection(configSectionName);
            return configSection != null;
        }

        private string GetStringValue(string setting)
        {
            return GetValue(setting, string.Empty);
        }

        public string GetValue(string setting)
        {
            return GetValue(setting, string.Empty);
        }

        public string GetValue(string setting, string defaultValue)
        {
            if (string.IsNullOrEmpty(setting))
            {
                throw new ArgumentNullException();
            }

            var keyIndex = FindAndCheckKeyIndex(setting);

            var configSectionName = setting.Substring(0, keyIndex);
            var keyName = setting.Substring(keyIndex + 1);

            var configSection = FindConfigSection(configSectionName);

            if (configSection == null)
            {
                return defaultValue;
            }

            return configSection.GetValue(keyName, defaultValue);
        }

        public string GetPathValue(string setting)
        {
            return GetStringValue(setting);
        }

        public IReadOnlyList<string> GetValues(string setting)
        {
            var keyIndex = FindAndCheckKeyIndex(setting);

            var configSectionName = setting.Substring(0, keyIndex);
            var keyName = setting.Substring(keyIndex + 1);

            var configSection = FindConfigSection(configSectionName);

            if (configSection == null)
            {
                return Array.Empty<string>();
            }

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
                _configSections.Add(result);
            }

            return result;
        }

        public void AddConfigSection(IConfigSection configSection)
        {
            if (FindConfigSection(configSection) != null)
            {
                throw new ArgumentException("Can not add a section that already exists: " + configSection.SectionName);
            }

            _configSections.Add(configSection);
        }

        public void RemoveConfigSection(string configSectionName)
        {
            var configSection = FindConfigSection(configSectionName);

            if (configSection == null)
            {
                return;
            }

            _configSections.Remove(configSection);
        }

        [CanBeNull]
        public IConfigSection FindConfigSection(string name)
        {
            var configSectionToFind = new ConfigSection(name, true);

            return FindConfigSection(configSectionToFind);
        }

        [CanBeNull]
        private IConfigSection FindConfigSection(IConfigSection configSectionToFind)
        {
            return ConfigSections.FirstOrDefault(configSectionToFind.Equals);
        }

        #region ConfigFileParser

        private class ConfigFileParser
        {
            private delegate ParsePart ParsePart(char c);

            private readonly ConfigFile _configFile;
            private string _fileContent;
            private IConfigSection _section;
            private string _key;
            private string FileName => _configFile.FileName;

            // parsed char
            private int _pos;
            private readonly StringBuilder _token = new StringBuilder();
            private readonly StringBuilder _valueToken = new StringBuilder();

            public ConfigFileParser(ConfigFile configFile)
            {
                _configFile = configFile;
            }

            public void Parse(string fileContent = null)
            {
                _fileContent = fileContent ?? File.ReadAllText(FileName, GetEncoding());

                ParsePart parseFunc = ReadUnknown;

                for (_pos = 0; _pos < _fileContent.Length; _pos++)
                {
                    parseFunc = parseFunc(_fileContent[_pos]);
                }

                if (_fileContent.Length > 0 && !_fileContent.EndsWith("\n"))
                {
                    parseFunc('\n');
                }
            }

            private void NewSection()
            {
                var sectionName = _token.ToString();
                _token.Clear();
                _section = _configFile.FindConfigSection(sectionName);
                if (_section == null)
                {
                    _section = new ConfigSection(sectionName, false);
                    _configFile._configSections.Add(_section);
                }
            }

            private void NewKey()
            {
                _key = _token.ToString().Trim();
                _token.Clear();

                if (_section == null)
                {
                    throw new Exception($"Key {_key} in config file {FileName} is not in a section.");
                }
            }

            private void NewValue()
            {
                _token.Append(_valueToken.ToString().Trim());
                _valueToken.Clear();

                string value = _token.ToString();

                if (_key.IsNullOrEmpty())
                {
                    throw new Exception($"Value {value} for empty key in config file {FileName}.");
                }

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
                            _token.Append(c);
                            break;
                        case 't':
                            _token.Append('\t');
                            break;
                        default:
                            throw new Exception("Invalid escape character: " + Regex.Escape(c.ToString()));
                    }

                    _escapedSection = false;
                    return ReadSection;
                }
                else
                {
                    // closing square bracket not in quoted section lead to start new section
                    if (c == ']' && !_quotedStringInSection)
                    {
                        NewSection();
                        return ReadUnknown;
                    }

                    if (c == '"')
                    {
                        _quotedStringInSection = !_quotedStringInSection;
                    }

                    switch (c)
                    {
                        case '\\':
                            _escapedSection = true;
                            return ReadSection;
                        default:
                            _token.Append(c);
                            return ReadSection;
                    }
                }
            }

            private ParsePart ReadComment(char c)
            {
                switch (c)
                {
                    case '\n':
                        // check for line continuation
                        if (_token.Length > 0 && _token[_token.Length - 1] == '\\')
                        {
                            _token.Remove(_token.Length - 1, 1);
                            return ReadComment;
                        }
                        else
                        {
                            return ReadUnknown;
                        }

                    default:
                        _token.Append(c);
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
                        _token.Append("true");
                        NewValue();
                        return ReadUnknown;
                    default:
                        _token.Append(c);
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
                            _valueToken.Append(c);
                            break;
                        case 't':
                            _valueToken.Append('\t');
                            break;
                        case 'n':
                            _valueToken.Append('\n');
                            break;
                        case '\r':
                            return ReadValue;
                        case '\n':
                            // line continuation
                            break;

                        default:
                            throw new Exception("Invalid escape character: " + Regex.Escape(c.ToString()));
                    }

                    _escapedValue = false;
                    return ReadValue;
                }
                else
                {
                    switch (c)
                    {
                        case '\\':
                            _escapedValue = true;
                            return ReadValue;
                        case '"':
                            _token.Append(_quotedValue
                                ? _valueToken.ToString()
                                : _valueToken.ToString().Trim());
                            _valueToken.Clear();
                            _quotedValue = !_quotedValue;
                            return ReadValue;
                        case ';':
                        case '#':
                            if (_quotedValue)
                            {
                                _valueToken.Append(c);
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
                            _valueToken.Append(c);
                            return ReadValue;
                    }
                }
            }

            private ParsePart ReadUnknown(char c)
            {
                _token.Clear();

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
                        _token.Append(c);
                        return ReadKey;
                }
            }
        }
        #endregion
    }
}
