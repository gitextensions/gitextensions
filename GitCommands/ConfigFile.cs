using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace GitCommands
{
    public class ConfigFile
    {
        IList<ConfigSection> sections;
        private string fileName;

        public ConfigFile(string fileName)
        {
            sections = new List<ConfigSection>();

            this.fileName = fileName;

            Load();
        }

        private void Load()
        {
            if (!File.Exists(fileName))
                return;

            string[] fileLines = File.ReadAllLines(fileName, Settings.Encoding);

            ConfigSection configSection = null;

            foreach (string line in fileLines)
            {
                Match m = regParseIsSection.Match(line);
                if (m.Success) //this line is a section
                {
                    string name = m.Groups["SectionName"].Value;

                    configSection = new ConfigSection(name);
                    this.sections.Add(configSection);
                }
                else
                {
                    m = regParseIsKey.Match(line);
                    if (m.Success) //this line is a key
                    {
                        string key = m.Groups["Key"].Value;
                        string value = unescapeString(m.Groups["Value"].Value);

                        if (configSection == null)
                            throw new Exception("Key " + key + " in configfile " + fileName + " is not in a section.");

                        configSection.SetValue(key, value);
                    }
                }

            }
        }

        public void Save()
        {
            StringBuilder configFileContent = new StringBuilder();

            foreach (ConfigSection section in sections)
            {
                //Skip empty sections
                if (section.Keys.Count == 0)
                    continue;

                configFileContent.AppendLine(section.ToString());

                foreach (KeyValuePair<string, string> key in section.Keys)
                {
                    configFileContent.AppendLine(string.Concat("\t", key.Key, " = ", escapeString(key.Value)));
                }
            }

            File.WriteAllText(fileName, configFileContent.ToString(), Settings.Encoding);
        }

        public void SetValue(string setting, string value)
        {
            int keyIndex = setting.LastIndexOf('.');

            if (keyIndex < 0 && keyIndex == setting.Length)
                throw new Exception("Invalid setting name: " + setting);


            string configSectionName = setting.Substring(0, keyIndex);
            string keyName = setting.Substring(keyIndex+1);

            FindOrCreateConfigSection(configSectionName).SetValue(keyName, value);
        }

        public string GetValue(string setting)
        {
            int keyIndex = setting.LastIndexOf('.');

            if (keyIndex < 0 && keyIndex == setting.Length)
                throw new Exception("Invalid setting name: " + setting);

            string configSectionName = setting.Substring(0, keyIndex);
            string keyName = setting.Substring(keyIndex+1);

            ConfigSection configSection = FindConfigSection(configSectionName);

            if (configSection == null)
                return string.Empty;

            return configSection.GetValue(keyName);
        }

        public void RemoveSetting(string setting)
        {
            int keyIndex = setting.LastIndexOf('.');

            if (keyIndex < 0 && keyIndex == setting.Length)
                throw new Exception("Invalid setting name: " + setting);

            string configSectionName = setting.Substring(0, keyIndex);
            string keyName = setting.Substring(keyIndex + 1);

            ConfigSection configSection = FindConfigSection(configSectionName);

            if (configSection == null)
                return;

            configSection.SetValue(keyName, null);
        }

        private ConfigSection FindOrCreateConfigSection(string name)
        {
            ConfigSection configSectionToFind = new ConfigSection(name);

            foreach (ConfigSection configSection in sections)
            {
                if (configSection.SectionName == configSectionToFind.SectionName &&
                    configSection.SubSection == configSectionToFind.SubSection)
                    return configSection;
            }
            sections.Add(configSectionToFind);
            return configSectionToFind;
        }

        private ConfigSection FindConfigSection(string name)
        {
            ConfigSection configSectionToFind = new ConfigSection(name);

            foreach (ConfigSection configSection in sections)
            {
                if (configSection.SectionName == configSectionToFind.SectionName &&
                    configSection.SubSection == configSectionToFind.SubSection)
                    return configSection;
            }
            return null;
        }

        private string unescapeString(string value)
        {
            // The .gitconfig escapes some character sequences
            return value.Replace("\\\"", "\"");
        }

        private static string escapeString(string path)
        {
            // The .gitconfig escapes some character sequences
            path = path.Replace("\"", "$QUOTE$");

            path = path.Trim();

            if (!path.StartsWith("\\\\"))
                path = path.Replace('\\', '/');

            return path.Replace("$QUOTE$", "\\\"");
        }

        static readonly Regex regParseIsSection =
                                        new Regex
                                        (
                                            @"(?<IsSection>
                                                        ^\s*\[(?<SectionName>[^\]]+)?\]\s*$
                                                    )
                                                    ",
                                            RegexOptions.Compiled |
                                            RegexOptions.IgnoreCase |
                                            RegexOptions.IgnorePatternWhitespace
                                        );
        static readonly Regex regParseIsKey =
                                        new Regex
                                        (
                                            @"(?<IsKeyValue>
                                                       ^\s*(?<Key>[^(\s*\=\s*)]+)?\s*\=\s*(?<Value>[\d\D]*)$
                                                    )",
                                            RegexOptions.Compiled |
                                            RegexOptions.IgnoreCase |
                                            RegexOptions.IgnorePatternWhitespace
                                        );
    }
}
