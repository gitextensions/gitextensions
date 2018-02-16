using System;
using System.Collections.Generic;
using ResourceManager.Xliff;

namespace ResourceManager
{
    public static class TranslationUtils
    {
        public static IEnumerable<Tuple<string, object>> GetObjFields(object obj, string objName)
        {
            return TranslationUtl.GetObjFields(obj, objName);
        }

        public static void AddTranslationItem(string category, object obj, string property, ITranslation translation)
        {
            TranslationUtl.AddTranslationItem(category, obj, property, translation);
        }

        public static void AddTranslationItemsFromFields(string category, object obj, ITranslation translation)
        {
            if (!string.IsNullOrEmpty(category))
                TranslationUtl.AddTranslationItemsFromFields(category, obj, translation);
        }

        public static void AddTranslationItemsFromList(string category, ITranslation translation, IEnumerable<Tuple<string, object>> items)
        {
            TranslationUtl.AddTranslationItemsFromList(category, translation, items);
        }

        public static void TranslateProperty(string category, object obj, string property, ITranslation translation)
        {
            TranslationUtl.TranslateProperty(category, obj, property, translation);
        }

        public static void TranslateItemsFromList(string category, ITranslation translation, IEnumerable<Tuple<string, object>> items)
        {
            TranslationUtl.TranslateItemsFromList(category, translation, items);
        }

        public static void TranslateItemsFromFields(string category, object obj, ITranslation translation)
        {
            if (!string.IsNullOrEmpty(category))
                TranslationUtl.TranslateItemsFromFields(category, obj, translation);
        }
    }
}
