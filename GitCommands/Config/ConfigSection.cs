﻿using System;
using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;

namespace GitCommands.Config
{
    /// <summary>
    ///   ConfigSection
    ///   Sections can be defined as:
    ///   [section "subsection"] (subsection is case sensitive)
    ///   or
    ///   [section.subsection] (subsection is case insensitive)
    ///   
    ///   Case insensitive sections are deprecated. Dot separated subsections are treated
    ///   as case insensitive only when loaded from config file. Dot separated subsections
    ///   added from code, are treated as case sensitive.
    /// </summary>
    public class ConfigSection : IConfigSection
    {
        private readonly IDictionary<string, IList<string>> _configKeys;


        internal ConfigSection(string name, bool forceCaseSensitive)
        {
            _configKeys = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);

            if (name.Contains("\"")) //[section "subsection"] case sensitive
            {
                SectionName = name.Substring(0, name.IndexOf('\"')).Trim();
                SubSection = name.Substring(name.IndexOf('\"') + 1, name.LastIndexOf('\"') - name.IndexOf('\"') - 1);
                SubSectionCaseSensitive = true;
            }
            else if (!name.Contains("."))
            {
                SectionName = name.Trim(); // [section] case sensitive
                SubSectionCaseSensitive = true;
            }
            else
            {
                //[section.subsection] case insensitive
                var subSectionIndex = name.IndexOf('.');

                if (subSectionIndex < 1)
                    throw new Exception("Invalid section name: " + name);

                SectionName = name.Substring(0, subSectionIndex).Trim();
                SubSection = name.Substring(subSectionIndex + 1).Trim();
                SubSectionCaseSensitive = false;
            }
            if (forceCaseSensitive)
                SubSectionCaseSensitive = true;
        }

        public string SectionName { get; set; }
        public string SubSection { get; set; }
        public bool SubSectionCaseSensitive { get; set; }

        public static string FixPath(string path)
        {
            if (path.StartsWith("\\\\")) //for using unc paths -> these need to be backward slashes
                return path;

            return path.ToPosixPath();
        }

        public IDictionary<string, IList<string>> AsDictionary()
        {
            return _configKeys.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
        }

        public bool HasValue(string key)
        {
            return _configKeys.ContainsKey(key);
        }

        public void SetValue(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
                _configKeys.Remove(key);
            else
                _configKeys[key] = new List<string> { value };
        }

        public void SetPathValue(string setting, string value)
        {
            SetValue(setting, FixPath(value));
        }

        public void AddValue(string key, string value)
        {
            if (!_configKeys.ContainsKey(key))
                _configKeys[key] = new List<string>();

            _configKeys[key].Add(value);
        }

        public string GetValue(string key)
        {
            return GetValue(key, string.Empty);
        }

        public string GetValue(string key, string defaultValue)
        {
            if (_configKeys.TryGetValue(key, out var list))
            {
                if (list.Count > 0)
                    return list[list.Count-1];
            }

            return defaultValue;
        }

        public IList<string> GetValues(string key)
        {
            return _configKeys.ContainsKey(key) ? _configKeys[key] : new List<string>();
        }

        public override string ToString()
        {
            string result = "[" + SectionName;
            if (!SubSection.IsNullOrEmpty())
            {
                var escSubSection = SubSection.Replace("\"", "\\\"");
                escSubSection = escSubSection.Replace("\\", "\\\\");

                if (!SubSectionCaseSensitive)
                    escSubSection = escSubSection.ToLower();
                result = result + " \"" + escSubSection + "\"";
            }
            result = result + "]";
            return result;
        }

        public bool Equals(IConfigSection other)
        {
            StringComparison sc;
            if (SubSectionCaseSensitive)
                sc = StringComparison.Ordinal;
            else
                sc = StringComparison.OrdinalIgnoreCase;

            return string.Equals(SectionName, other.SectionName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(SubSection, other.SubSection, sc);
        }
    }

    public static class ConfigSectionExt
    {
        public static bool GetValueAsBool(this IConfigSection section, string name, bool defaultValue)
        {
            bool result = defaultValue;

            if (section.HasValue(name))
            {
                string value = section.GetValue(name);
                bool.TryParse(value, out result);
            }

            return result;
        }

        public static void SetValueAsBool(this IConfigSection section, string name, bool value)
        {
            section.SetValue(name, value.ToString());
        }
    }
}