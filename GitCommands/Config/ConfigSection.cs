using System;
using System.Collections.Generic;

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
    public class ConfigSection
    {
        internal ConfigSection(string name, bool forceCaseSensitive)
        {
            Keys = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);

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

        internal IDictionary<string, IList<string>> Keys { get; set; }
        public string SectionName { get; set; }
        public string SubSection { get; set; }
        public bool SubSectionCaseSensitive { get; set; }

        internal static string UnescapeString(string value)
        {
            // The .gitconfig escapes some character sequences -> 
            // \" = "
            // \\ = \
            return value.Replace("\\\"", "\"").Replace("\\\\", "\\");
        }

        internal static string EscapeString(string path)
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

        public void SetValue(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
                Keys.Remove(key);
            else
                Keys[key] = new List<string> { value };
        }

        public void SetPathValue(string setting, string value)
        {
            SetValue(setting, EscapeString(value));
        }

        public void AddValue(string key, string value)
        {
            if (!Keys.ContainsKey(key))
                Keys[key] = new List<string>();

            Keys[key].Add(value);
        }

        public string GetValue(string key)
        {
            return Keys.ContainsKey(key) && Keys[key].Count > 0 ? Keys[key][0] : string.Empty;
        }

        public string GetPathValue(string setting)
        {
            return UnescapeString(GetValue(setting));
        }

        public IList<string> GetValues(string key)
        {
            return Keys.ContainsKey(key) ? Keys[key] : new List<string>();
        }

        public override string ToString()
        {
            string result = "[" + SectionName;
            if (!SubSection.IsNullOrEmpty())
                if (SubSectionCaseSensitive)
                    result = result + " \"" + SubSection + "\"";
                else
                    result = result + "." + SubSection;
            result = result + "]";
            return result;
        }

        public bool Equals(ConfigSection other)
        {
            StringComparison sc;
            if (SubSectionCaseSensitive)
                sc = StringComparison.Ordinal;
            else
                sc = StringComparison.OrdinalIgnoreCase;

            return
                string.Equals(SectionName, other.SectionName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(SubSection, other.SubSection, sc);
        }
    }
}