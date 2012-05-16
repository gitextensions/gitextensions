using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GitCommands.Config
{
    public class ConfigFile
    {
        private static readonly Regex RegParseIsSection =
            new Regex(
                @"(?<IsSection>
                        ^\s*\[(?<SectionName>[^\]]+)?\]\s*$
                  )",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase |
                RegexOptions.IgnorePatternWhitespace
                );

        private static readonly Regex RegParseIsKey =
            new Regex(
                @"(?<IsKeyValue>
                        ^\s*(?<Key>[^(\s*\=\s*)]+)?\s*\=\s*(?<Value>[\d\D]*)$
                   )",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase |
                RegexOptions.IgnorePatternWhitespace
                );

        private readonly string _fileName;
        private readonly IList<ConfigSection> _sections;

        public ConfigFile(string fileName)
        {
            _sections = new List<ConfigSection>();

            _fileName = fileName;
            try
            {
                Load();
            }
            catch (Exception ex)
            {
                ex.Data.Add(GetType().Name + ".Load", "Could not load config file: " + _fileName);
                throw;
            }
        }

        public IList<ConfigSection> GetConfigSections()
        {
            return _sections;
        }

        private void Load()
        {
            if (string.IsNullOrEmpty(Path.GetFileName(_fileName)) || !File.Exists(_fileName))
                return;

            FindSections(File.ReadAllLines(_fileName, Settings.AppEncoding));
        }

        private void FindSections(IEnumerable<string> fileLines)
        {
            ConfigSection configSection = null;

            foreach (var line in fileLines)
            {
                var m = RegParseIsSection.Match(line);
                if (m.Success) //this line is a section
                {
                    var name = m.Groups["SectionName"].Value;

                    configSection = new ConfigSection(name);
                    _sections.Add(configSection);
                }
                else
                {
                    m = RegParseIsKey.Match(line);
                    if (m.Success) //this line is a key
                    {
                        var key = m.Groups["Key"].Value;
                        var value = m.Groups["Value"].Value;

                        if (configSection == null)
                            throw new Exception(
                                string.Format("Key {0} in configfile {1} is not in a section.", key, _fileName));

                        configSection.AddValue(key, value);
                    }
                }
            }
        }

        public void Save()
        {
            var configFileContent = new StringBuilder();

            foreach (var section in _sections)
            {
                //Skip empty sections
                if (section.Keys.Count == 0)
                    continue;

                configFileContent.AppendLine(section.ToString());

                foreach (var key in section.Keys)
                {
                    foreach (var value in key.Value)
                    {
                        configFileContent.AppendLine(string.Concat("\t", key.Key, " = ", value));
                    }
                }
            }


            try
            {
                FileInfoExtensions
                    .MakeFileTemporaryWritable(_fileName,
                                       x =>
                                       File.WriteAllText(_fileName, configFileContent.ToString(), Settings.AppEncoding));
            }
            catch (Exception ex)
            {
                ExceptionUtils.ShowException(ex, false);
            }
        }

        private void SetStringValue(string setting, string value)
        {
            var keyIndex = setting.LastIndexOf('.');

            if (keyIndex < 0 && keyIndex == setting.Length)
                throw new Exception("Invalid setting name: " + setting);

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
            SetStringValue(setting, ConfigSection.EscapeString(value));
        }

        private void AddStringValue(string setting, string value)
        {
            var keyIndex = setting.LastIndexOf('.');

            if (keyIndex < 0 && keyIndex == setting.Length)
                throw new Exception("Invalid setting name: " + setting);

            var configSectionName = setting.Substring(0, keyIndex);
            var keyName = setting.Substring(keyIndex + 1);

            FindOrCreateConfigSection(configSectionName).AddValue(keyName, value);
        }

        public void AddValue(string setting, string value)
        {
            AddStringValue(setting, value);
        }

        public void AddPathValue(string setting, string value)
        {
            AddStringValue(setting, ConfigSection.EscapeString(value));
        }

        public bool HasValue(string setting)
        {
            var keyIndex = setting.LastIndexOf('.');

            if (keyIndex < 0 && keyIndex == setting.Length)
                throw new Exception("Invalid setting name: " + setting);

            var configSectionName = setting.Substring(0, keyIndex);
            var keyName = setting.Substring(keyIndex + 1);

            var configSection = FindConfigSection(configSectionName);
            return configSection != null && configSection.GetValue(keyName) != string.Empty;
        }

        public bool HasConfigSection(string configSectionName)
        {
            var configSection = FindConfigSection(configSectionName);
            if (configSection != null)
                return true;
            else
                return false;
        }

        private string GetStringValue(string setting)
        {
            var keyIndex = setting.LastIndexOf('.');

            if (keyIndex < 0 && keyIndex == setting.Length)
                throw new Exception("Invalid setting name: " + setting);

            var configSectionName = setting.Substring(0, keyIndex);
            var keyName = setting.Substring(keyIndex + 1);

            var configSection = FindConfigSection(configSectionName);

            if (configSection == null)
                return string.Empty;

            return configSection.GetValue(keyName);
        }

        public string GetValue(string setting)
        {
            return GetStringValue(setting);
        }

        public string GetPathValue(string setting)
        {
            return ConfigSection.UnescapeString(GetStringValue(setting));
        }

        public IList<string> GetValues(string setting)
        {
            var keyIndex = setting.LastIndexOf('.');

            if (keyIndex < 0 && keyIndex == setting.Length)
                throw new Exception("Invalid setting name: " + setting);

            var configSectionName = setting.Substring(0, keyIndex);
            var keyName = setting.Substring(keyIndex + 1);

            var configSection = FindConfigSection(configSectionName);

            if (configSection == null)
                return new List<string>();

            return configSection.GetValues(keyName);
        }

        public void RemoveSetting(string setting)
        {
            var keyIndex = setting.LastIndexOf('.');

            if (keyIndex < 0 && keyIndex == setting.Length)
                throw new Exception("Invalid setting name: " + setting);

            var configSectionName = setting.Substring(0, keyIndex);
            var keyName = setting.Substring(keyIndex + 1);

            var configSection = FindConfigSection(configSectionName);

            if (configSection == null)
                return;

            configSection.SetValue(keyName, null);
        }

        private ConfigSection FindOrCreateConfigSection(string name)
        {
            var result = FindConfigSection(name);
            if (result == null)
            {
                result = new ConfigSection(name);
                _sections.Add(result);
            }
            
            return result;
        }

        public void RemoveConfigSection(string configSectionName)
        {
            var configSection = FindConfigSection(configSectionName);

            if (configSection == null)
                return;

            _sections.Remove(configSection);
        }

        private ConfigSection FindConfigSection(string name)
        {
            var configSectionToFind = new ConfigSection(name);

            foreach (var configSection in _sections)
            {
                if (configSectionToFind.Equals(configSection))
                    return configSection;
            }
            return null;
        }
    }
}
