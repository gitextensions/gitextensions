using System.Collections.Generic;

namespace ResourceManager
{
    public static class TranslationUtils
    {
        public static IEnumerable<(string name, object item)> GetObjFields(object obj, string objName)
        {
            return Xliff.TranslationUtil.GetObjFields(obj, objName);
        }

        public static void AddTranslationItem(string category, object obj, string property, ITranslation translation)
        {
            Xliff.TranslationUtil.AddTranslationItem(category, obj, property, translation);
        }

        public static void AddTranslationItemsFromFields(string category, object obj, ITranslation translation)
        {
            if (!string.IsNullOrEmpty(category))
            {
                Xliff.TranslationUtil.AddTranslationItemsFromFields(category, obj, translation);
            }
        }

        public static void AddTranslationItemsFromList(string category, ITranslation translation, IEnumerable<(string name, object item)> items)
        {
            Xliff.TranslationUtil.AddTranslationItemsFromList(category, translation, items);
        }

        public static void TranslateProperty(string category, object obj, string property, ITranslation translation)
        {
            Xliff.TranslationUtil.TranslateProperty(category, obj, property, translation);
        }

        public static void TranslateItemsFromList(string category, ITranslation translation, IEnumerable<(string name, object item)> items)
        {
            Xliff.TranslationUtil.TranslateItemsFromList(category, translation, items);
        }

        public static void TranslateItemsFromFields(string category, object obj, ITranslation translation)
        {
            if (!string.IsNullOrEmpty(category))
            {
                Xliff.TranslationUtil.TranslateItemsFromFields(category, obj, translation);
            }
        }
    }
}
