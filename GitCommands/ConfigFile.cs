using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GitCommands
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

            Load();
        }

        private void Load()
        {
            if (!File.Exists(_fileName))
                return;

            var fileLines = File.ReadAllLines(_fileName, Settings.Encoding);

            ConfigSection configSection = null;

            foreach (var line in fileLines)
            {
                processLine(line, configSection);
            }
        }

        private void processLine(string line, ConfigSection configSection)
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
                    var value = UnescapeString(m.Groups["Value"].Value);

                    if (configSection == null)
                        throw new Exception("Key " + key + " in configfile " + _fileName + " is not in a section.");

                    configSection.SetValue(key, value);
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
                    configFileContent.AppendLine(string.Concat("\t", key.Key, " = ", EscapeString(key.Value)));
                }
            }


            try
            {
                FileInfoExtensions
                    .TemporayMakeFileWriteable(_fileName,
                                       x =>
                                       File.WriteAllText(_fileName, configFileContent.ToString(), Settings.Encoding));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SetValue(string setting, string value)
        {
            var keyIndex = setting.LastIndexOf('.');

            if (keyIndex < 0 && keyIndex == setting.Length)
                throw new Exception("Invalid setting name: " + setting);


            var configSectionName = setting.Substring(0, keyIndex);
            var keyName = setting.Substring(keyIndex + 1);

            FindOrCreateConfigSection(configSectionName).SetValue(keyName, value);
        }

        public string GetValue(string setting)
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
            var configSectionToFind = new ConfigSection(name);

            foreach (var configSection in _sections)
            {
                if (configSection.SectionName == configSectionToFind.SectionName &&
                    configSection.SubSection == configSectionToFind.SubSection)
                    return configSection;
            }
            _sections.Add(configSectionToFind);
            return configSectionToFind;
        }

        private ConfigSection FindConfigSection(string name)
        {
            var configSectionToFind = new ConfigSection(name);

            foreach (var configSection in _sections)
            {
                if (configSection.SectionName == configSectionToFind.SectionName &&
                    configSection.SubSection == configSectionToFind.SubSection)
                    return configSection;
            }
            return null;
        }

        private static string UnescapeString(string value)
        {
            // The .gitconfig escapes some character sequences -> 
            // \" = "
            // \\ = \
            return value.Replace("\\\"", "\"").Replace("\\\\", "\\");
        }

        private static string EscapeString(string path)
        {
            // The .gitconfig escapes some character sequences
            path = path.Replace("\"", "$QUOTE$");

            path = path.Trim();

            if (path.StartsWith("\\\\")) //for using unc paths -> these need to be backward slashes
                path = path.Replace("\\", "\\\\");
            else //for directories -> git only supports forward slashes
                path = path.Replace('\\', '/');

            return path.Replace("$QUOTE$", "\\\"");
        }
    }
}