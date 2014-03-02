using System;
using System.Collections.Generic;

namespace ResourceManager
{
    public static class TranslationUtl
    {
        public static IEnumerable<Tuple<string, object>> GetObjProperties(object obj, string objName)
        {
            return Xml.TranslationUtl.GetObjProperties(obj, objName);
        }

        public static void AddTranslationItemsFromFields(string category, object obj, ITranslation translation)
        {
            Xml.TranslationUtl.AddTranslationItemsFromFields(category, obj, translation);
        }

        public static void AddTranslationItemsFromList(string category, ITranslation translation, IEnumerable<Tuple<string, object>> items)
        {
            Xml.TranslationUtl.AddTranslationItemsFromList(category, translation, items);
        }

        public static void TranslateItemsFromList(string category, ITranslation translation, IEnumerable<Tuple<string, object>> items) 
        {
            Xml.TranslationUtl.TranslateItemsFromList(category, translation, items);
        }

        public static void TranslateItemsFromFields(string category, object obj, ITranslation translation)
        {
            Xml.TranslationUtl.TranslateItemsFromFields(category, obj, translation);
        }
    }
}
