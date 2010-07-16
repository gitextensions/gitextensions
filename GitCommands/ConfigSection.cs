using System;
using System.Collections.Generic;
using System.Text;

namespace GitCommands
{
    //sections can be defined as:
    //[section "subsection"] (subsection is case senstive)
    //or
    //[section.subsection] (subsection is case insenstive)
    public class ConfigSection
    {
        public IDictionary<string, string> Keys { get; set; }

        public string SectionName { get; set; }
        public string SubSection { get; set; }

        public ConfigSection(string name)
        {
            Keys = new Dictionary<string, string>();

            if (name.Contains("\"")) //[section "subsection"] case sensitive
            {
                SectionName = name.Substring(0, name.IndexOf('\"')).Trim();
                SubSection = name.Substring(name.IndexOf('\"')+1, name.LastIndexOf('\"') - name.IndexOf('\"')-1);
            }
            else if (!name.Contains("."))
            {
                SectionName = name.Trim(); // [section] case sensitive
            }
            else
            {
                int subSectionIndex = name.IndexOf('.');

                if (subSectionIndex < 1)
                    throw new Exception("Invalid section name: " + name);

                SectionName = name.Substring(0, subSectionIndex).Trim();
                SubSection = name.Substring(subSectionIndex+1).Trim();
            }
        }

        public void SetValue(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
                Keys.Remove(key);
            else
                Keys[key] = value;
        }

        public string GetValue(string key)
        {
            if (Keys.ContainsKey(key))
                return Keys[key];

            return string.Empty;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(SubSection))
                return string.Concat("[", SectionName, "]");
            else
                return string.Concat("[", SectionName, " \"", SubSection, "\"]");
        }
    }
}
