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

        public List<TranslationCategory> translationCategories;

        public void AddTranslationCategory(TranslationCategory translationCategory)
        {
            if (string.IsNullOrEmpty(translationCategory.Name))
                new InvalidOperationException("Cannot add translationCategory without name");

            translationCategories.Add(translationCategory);
        }

        public bool HasTranslationCategory(string name)
        {
            return translationCategories.Exists(t => t.Name == name);
        }

        public TranslationCategory GetTranslationCategory(string name)
        {
            return translationCategories.Find(t => t.Name == name);
        }

        public List<TranslationCategory> GetTranslationCategories()
        {
            return translationCategories;
        }

    }
}
