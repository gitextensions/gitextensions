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
            TranslationCategories = new List<TranslationCategory>();
        }

        public Translation(string gitExVersion, string languageCode)
            : this()
        {
            GitExVersion = gitExVersion;
            _languageCode = languageCode;
        }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("GitExVersion")]
        public string GitExVersion { get; set; }

        private string _languageCode;
        [XmlAttribute("LanguageCode")]
        public string LanguageCode { get { return _languageCode; } }

        [XmlElement(ElementName = "file")]
        public List<TranslationCategory> TranslationCategories { get; set; }

        public TranslationCategory FindOrAddTranslationCategory(string translationCategory)
        {
            TranslationCategory tc = GetTranslationCategory(translationCategory);
            if (tc == null)
            {
                tc = new TranslationCategory(translationCategory, "en");
                AddTranslationCategory(tc);
            }
            return tc;
        }

        public void AddTranslationCategory(TranslationCategory translationCategory)
        {
            if (string.IsNullOrEmpty(translationCategory.Name))
                new InvalidOperationException("Cannot add translationCategory without name");

            TranslationCategories.Add(translationCategory);
        }

        public TranslationCategory GetTranslationCategory(string name)
        {
            return TranslationCategories.Find(t => t.Name.TrimStart('_') == name.TrimStart('_'));
        }

        public void Sort()
        {
            TranslationCategories.Sort();
            foreach(TranslationCategory tc in TranslationCategories)
                tc.Body.TranslationItems.Sort();
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
