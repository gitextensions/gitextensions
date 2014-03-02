using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ResourceManager.Xliff
{
    /// <summary>Provides a translation for a specific language.</summary>
    [XmlRoot("xliff")]
    public class Translation : ITranslation
    {
        public Translation()
        {
            Version = "1.0";
        }

        public Translation(string gitExVersion, string languageCode)
            : this()
        {
            GitExVersion = gitExVersion;
            _languageCode = languageCode;
        }

        private string _languageCode;

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("GitExVersion")]
        public string GitExVersion { get; set; }

        [XmlElement(ElementName = "file")]
        public List<TranslationCategory> translationCategories = new List<TranslationCategory>();

        public TranslationCategory FindOrAddTranslationCategory(string translationCategory)
        {
            TranslationCategory tc = GetTranslationCategory(translationCategory);
            if (tc == null)
            {
                tc = new TranslationCategory(translationCategory, "en", _languageCode);
                AddTranslationCategory(tc);
            }
            return tc;
        }

        public void AddTranslationCategory(TranslationCategory translationCategory)
        {
            if (string.IsNullOrEmpty(translationCategory.Name))
                new InvalidOperationException("Cannot add translationCategory without name");

            translationCategories.Add(translationCategory);
        }

        public bool HasTranslationCategory(string name)
        {
            return translationCategories.Exists(t => t.Name.TrimStart('_') == name.TrimStart('_'));
        }

        public TranslationCategory GetTranslationCategory(string name)
        {
            return translationCategories.Find(t => t.Name.TrimStart('_') == name.TrimStart('_'));
        }

        public List<TranslationCategory> GetTranslationCategories()
        {
            return translationCategories;
        }

        public void Sort()
        {
            translationCategories.Sort();
            foreach(TranslationCategory tc in translationCategories)
                tc.Body.GetTranslationItems().Sort();
        }

        public void AddTranslationItem(string category, string item, string property, string neutralValue)
        {
            FindOrAddTranslationCategory(category).Body.AddTranslationItemIfNotExist(new TranslationItem(item, property, neutralValue));
        }

        public string TranslateItem(string category, string item, string property, string defaultValue)
        {
            TranslationCategory tc = GetTranslationCategory(category);
            if (tc == null)
                return defaultValue;
            TranslationItem ti = tc.Body.GetTranslationItem(item, property);
            return ti == null || string.IsNullOrEmpty(ti.Value) ? defaultValue : ti.Value;
        }
    }
}
