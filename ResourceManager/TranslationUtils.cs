using System.Collections.Generic;

namespace ResourceManager
{
    public static class TranslationUtils
    {
        public static IEnumerable<(string name, object item)> GetObjFields(object obj, string objName)
        {
            return Xliff.TranslationUtl.GetObjFields(obj, objName);
        }

        public static void AddTranslationItem(string category, object obj, string property, ITranslation translation)
        {
            Xliff.TranslationUtl.AddTranslationItem(category, obj, property, translation);
        }

        public static void AddTranslationItemsFromFields(string category, object obj, ITranslation translation)
        {
            if (!string.IsNullOrEmpty(category))
            {
                Xliff.TranslationUtl.AddTranslationItemsFromFields(category, obj, translation);
            }
        }

        public static void AddTranslationItemsFromList(string category, ITranslation translation, IEnumerable<(string name, object item)> items)
        {
            Xliff.TranslationUtl.AddTranslationItemsFromList(category, translation, items);
        }

        public static void TranslateProperty(string category, object obj, string property, ITranslation translation)
        {
            Xliff.TranslationUtl.TranslateProperty(category, obj, property, translation);
        }

        public static void TranslateItemsFromList(string category, ITranslation translation, IEnumerable<(string name, object item)> items)
        {
            Xliff.TranslationUtl.TranslateItemsFromList(category, translation, items);
        }

        public static void TranslateItemsFromFields(string category, object obj, ITranslation translation)
        {
            if (!string.IsNullOrEmpty(category))
            {
                Xliff.TranslationUtl.TranslateItemsFromFields(category, obj, translation);
            }
        }
    }
}
