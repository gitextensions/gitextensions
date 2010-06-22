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
                string[] path = name.Replace("\"", " ").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (path.Length != 2)
                    throw new Exception("Invalid section name: " + name);

                SectionName = path[0].Trim();
                SubSection = path[1].Trim();
            }
            else if (!name.Contains("."))
            {
                SectionName = name.Trim(); // [section] case sensitive
            }
            else
            {
                string[] path = name.Split('.');

                if (path.Length > 2)
                    throw new Exception("Invalid section name: " + name);

                SectionName = path[0].Trim();
                SubSection = path[1].Trim();
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
