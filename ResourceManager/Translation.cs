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

    }
}
