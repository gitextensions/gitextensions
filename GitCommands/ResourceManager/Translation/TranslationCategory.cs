using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ResourceManager.Translation
{
    public class TranslationCategory
    {
        public TranslationCategory()
        {
            translationItems = new List<TranslationItem>();
        }

        public TranslationCategory(string name)
        {
            this.name = name;
            translationItems = new List<TranslationItem>();
        }

        private string name;
        [XmlAttribute]
        public string Name 
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public List<TranslationItem> translationItems;

        public void AddTranslationItem(TranslationItem translationItem)
        {
            if (string.IsNullOrEmpty(translationItem.Name))
                throw new InvalidOperationException("Cannot add translationitem without name");

            translationItems.Add(translationItem);
        }

        public bool HasTranslationItem(string name, string property)
        {
            return translationItems.Exists(t => t.Name == name && t.Property == property);
        }

        public TranslationItem GetTranslationItem(string name, string property)
        {
            return translationItems.Find(t => t.Name == name && t.Property == property);
        }

        public List<TranslationItem> GetTranslationItems()
        {
            return translationItems;
        }
    }
}
