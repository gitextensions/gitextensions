using System;
using System.Collections.Generic;
using System.Linq;
using ResourceManager;
using ResourceManager.Xliff;
using TranslationUtl = ResourceManager.Xliff.TranslationUtl;
using Xliff = ResourceManager.Xliff;

namespace TranslationApp
{
    static class TranslationHelpers
    {
        public static IList<TranslationItemWithCategory> LoadNeutralItems()
        {
            Translation neutralTranslation = new Translation();
            try
            {
                //Set language to neutral to get neutral translations
                GitCommands.AppSettings.CurrentTranslation = "";

                List<Type> translatableTypes = TranslationUtl.GetTranslatableTypes();
                foreach (Type type in translatableTypes)
                {
                    ITranslate obj = TranslationUtl.CreateInstanceOfClass(type) as ITranslate;
                    if (obj != null)
                        obj.AddTranslationItems(neutralTranslation);
                }
            }
            finally
            {
                neutralTranslation.Sort();

                //Restore translation
                GitCommands.AppSettings.CurrentTranslation = null;
            }

            IList<TranslationItemWithCategory> neutralItems =
                (from translationCategory in neutralTranslation.TranslationCategories
                 from translationItem in translationCategory.Body.TranslationItems
                 select new TranslationItemWithCategory(translationCategory.Name, translationItem)).ToList();
            return neutralItems;
        }

        public static List<TranslationItemWithCategory> LoadTranslation(Translation translation, IEnumerable<TranslationItemWithCategory> neutralItems)
        {
            List<TranslationItemWithCategory> translateItems = new List<TranslationItemWithCategory>();

            var oldTranslationItems =
                (from translationCategory in translation.TranslationCategories
                 from translationItem in translationCategory.Body.TranslationItems
                 select new TranslationItemWithCategory(translationCategory.Name, translationItem)).ToList();

            var dict = new Dictionary<string, string>();
            foreach (var item in neutralItems)
            {
                var curItems = oldTranslationItems.Where(
                        trItem => trItem.Category.TrimStart('_') == item.Category.TrimStart('_') &&
                                  trItem.Name.TrimStart('_') == item.Name.TrimStart('_') &&
                                  trItem.Property == item.Property);
                var curItem = curItems.FirstOrDefault();

                if (curItem == null)
                {
                    curItem = item.Clone();
                    translateItems.Add(curItem);
                    continue;
                }

                oldTranslationItems.Remove(curItem);
                curItem.Category = item.Category;
                curItem.Name = item.Name;

                string source = curItem.NeutralValue ?? item.NeutralValue;
                if (!String.IsNullOrEmpty(curItem.TranslatedValue) && !dict.ContainsKey(source))
                    dict.Add(source, curItem.TranslatedValue);

                // Source text changed
                if (!curItem.IsSourceEqual(item.NeutralValue) &&
                    (!String.IsNullOrEmpty(curItem.TranslatedValue) && !curItem.IsSourceEqual(item.NeutralValue)))
                {
                    curItem.TranslatedValue = "";
                }
                curItem.NeutralValue = item.NeutralValue;
                translateItems.Add(curItem);
            }

            foreach (var item in oldTranslationItems)
            {
                // Obsolete should be added only to dictionary
                if (!String.IsNullOrEmpty(item.TranslatedValue) && 
                    item.NeutralValue != null && !dict.ContainsKey(item.NeutralValue))
                {
                    dict.Add(item.NeutralValue, item.TranslatedValue);
                }
            }

            // update untranslated items
            var untranlatedItems = 
                from trItem in translateItems
                where (String.IsNullOrEmpty(trItem.TranslatedValue)) && dict.ContainsKey(trItem.NeutralValue)
                select trItem;

            foreach (var untranlatedItem in untranlatedItems)
            {
                untranlatedItem.TranslatedValue = dict[untranlatedItem.NeutralValue];
            }
            return translateItems;
        }

        public static void SaveTranslation(string languageCode, IEnumerable<TranslationItemWithCategory> items, string filename)
        {
            var foreignTranslation = new Translation(GitCommands.AppSettings.GitExtensionsVersionString, languageCode);
            foreach (TranslationItemWithCategory translateItem in items)
            {
                var item = translateItem.GetTranslationItem();

                var ti = new TranslationItem(item.Name, item.Property, item.Source, item.Value);
                ti.Value = ti.Value ?? String.Empty;
                foreignTranslation.FindOrAddTranslationCategory(translateItem.Category).Body.AddTranslationItem(ti);
            }
            TranslationSerializer.Serialize(foreignTranslation, filename);
        }
    }
}
