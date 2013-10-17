using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ResourceManager.Translation
{
    /// <summary>Provides a translation for a specific language.</summary>
    public class Translation
    {
        public Translation()
        {
            translationCategories = new List<TranslationCategory>();
        }

        [XmlAttribute("GitExVersion")]
        public string GitExVersion { get; set; }

        public string LanguageCode { get; set; }

        public List<TranslationCategory> translationCategories;

        public TranslationCategory FindOrAddTranslationCategory(string translationCategory)
        {
            TranslationCategory tc = GetTranslationCategory(translationCategory);
            if (tc == null)
            {
                tc = new TranslationCategory(translationCategory);
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
                tc.GetTranslationItems().Sort();
        }

        public void AddTranslationItem(string category, string item, string property, string neutralValue)
        {
            FindOrAddTranslationCategory(category).AddTranslationItemIfNotExist(new TranslationItem(item, property, neutralValue));
        }

        public string TranslateItem(string category, string item, string property, string defaultValue)
        {
            TranslationCategory tc = GetTranslationCategory(category);
            if (tc == null)
                return defaultValue;
            TranslationItem ti = tc.GetTranslationItem(item, property);
            return ti == null || string.IsNullOrEmpty(ti.Value) ? defaultValue : ti.Value;
        }
    }
}
