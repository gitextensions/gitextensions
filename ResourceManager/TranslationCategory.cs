using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace ResourceManager.Translation
{
    [DebuggerDisplay("{name}")]
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

        public void AddTranslationItemIfNotExist(TranslationItem translationItem)
        {
            if (string.IsNullOrEmpty(translationItem.Name))
                throw new InvalidOperationException("Cannot add translationitem without name");

            TranslationItem ti = GetTranslationItem(translationItem.Name, translationItem.Property);
            if (ti == null)
                translationItems.Add(translationItem);
            else
                Debug.Assert(ti.Value == translationItem.Value);
        }

        public bool HasTranslationItem(string name, string property)
        {
            return translationItems.Exists(t => t.Name.TrimStart('_') == name.TrimStart('_') && t.Property == property);
        }

        public TranslationItem GetTranslationItem(string name, string property)
        {
            return translationItems.Find(t => t.Name.TrimStart('_') == name.TrimStart('_') && t.Property == property);
        }

        public List<TranslationItem> GetTranslationItems()
        {
            return translationItems;
        }
    }
}
