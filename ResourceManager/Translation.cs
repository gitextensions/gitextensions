using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceManager.Translation
{
    public class Translation
    {
        public Translation()
        {
            translationCategories = new List<TranslationCategory>();
        }

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
            translationCategories.Sort((tc1, tc2) => tc1.Name.CompareTo(tc2.Name));
            foreach(TranslationCategory tc in translationCategories)
                tc.GetTranslationItems().Sort((tc1, tc2) => {
                    var val = tc1.Name.CompareTo(tc2.Name);
                    if (val == 0) val = tc1.Property.CompareTo(tc2.Property);
                    return val; });
        }

    }
}
