using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ResourceManager;
using ResourceManager.Xml;
using TranslationUtl = ResourceManager.Xml.TranslationUtl;
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
                (from translationCategory in neutralTranslation.GetTranslationCategories()
                 from translationItem in translationCategory.GetTranslationItems()
                 select new TranslationItemWithCategory(translationCategory.Name, translationItem)).ToList();
            return neutralItems;
        }

        public static List<TranslationItemWithCategory> LoadTranslation(Translation translation, IEnumerable<TranslationItemWithCategory> neutralItems)
        {
            List<TranslationItemWithCategory> translateItems = new List<TranslationItemWithCategory>();

            var oldTranslationItems =
                (from translationCategory in translation.GetTranslationCategories()
                 from translationItem in translationCategory.GetTranslationItems()
                 select new TranslationItemWithCategory(translationCategory.Name, translationItem)).ToList();

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var item in neutralItems)
            {
                var curItem =
                    (from trItem in oldTranslationItems
                     where trItem.Category.TrimStart('_') == item.Category.TrimStart('_') &&
                     trItem.Name.TrimStart('_') == item.Name.TrimStart('_') &&
                     trItem.Property == item.Property
                     select trItem).FirstOrDefault();

                if (curItem == null)
                {
                    curItem = item.Clone();
                    translateItems.Add(curItem);
                    continue;
                }

                oldTranslationItems.Remove(curItem);
                curItem.Category = item.Category;
                curItem.Name = item.Name;
                if (curItem.Status == TranslationType.Translated || curItem.Status == TranslationType.Obsolete)
                {
                    string source = curItem.NeutralValue ?? item.NeutralValue;
                    if (!String.IsNullOrEmpty(curItem.TranslatedValue) && !dict.ContainsKey(source))
                        dict.Add(source, curItem.TranslatedValue);
                }
                if ((curItem.Status == TranslationType.Translated || curItem.Status == TranslationType.Obsolete) && 
                    !curItem.IsSourceEqual(item.NeutralValue))
                    curItem.Status = TranslationType.Unfinished;
                else if (curItem.Status == TranslationType.Obsolete && curItem.IsSourceEqual(item.NeutralValue))
                    curItem.Status = TranslationType.Translated;
                if (curItem.Status == TranslationType.Unfinished)
                {
                    if (!String.IsNullOrEmpty(curItem.TranslatedValue) &&
                        curItem.OldNeutralValue == null && !curItem.IsSourceEqual(item.NeutralValue))
                        curItem.OldNeutralValue = curItem.NeutralValue;
                }
                else
                    curItem.OldNeutralValue = null;
                curItem.NeutralValue = item.NeutralValue;
                translateItems.Add(curItem);
            }

            foreach (var item in oldTranslationItems)
            {
                if ((item.Status == TranslationType.Translated || item.Status == TranslationType.Obsolete) &&
                    !String.IsNullOrEmpty(item.TranslatedValue) && 
                    item.NeutralValue != null && !dict.ContainsKey(item.NeutralValue))
                {
                    dict.Add(item.NeutralValue, item.TranslatedValue);

                    item.Status = TranslationType.Obsolete;
                    item.OldNeutralValue = null;
                    translateItems.Add(item);
                }
            }

            // update untranslated items
            var untranlatedItems = 
                from trItem in translateItems
                where (trItem.Status == TranslationType.New || trItem.Status == TranslationType.Unfinished &&
                String.IsNullOrEmpty(trItem.TranslatedValue)) && dict.ContainsKey(trItem.NeutralValue)
                select trItem;

            foreach (var untranlatedItem in untranlatedItems)
            {
                untranlatedItem.TranslatedValue = dict[untranlatedItem.NeutralValue];
                if (untranlatedItem.TranslatedValue.IndexOfAny(new[] { ' ', '\t', '\n' }) != -1)
                    untranlatedItem.Status = TranslationType.Translated;
                else
                    untranlatedItem.Status = TranslationType.Unfinished;
            }
            return translateItems;
        }

        public static void SaveTranslation(string languageCode, IEnumerable<TranslationItemWithCategory> items, string filename)
        {
            var foreignTranslation = new Xliff.Translation(GitCommands.AppSettings.GitExtensionsVersionString, languageCode);
            foreach (TranslationItemWithCategory translateItem in items)
            {
                if (translateItem.Status == TranslationType.Obsolete)
                    continue;

                var item = translateItem.GetTranslationItem();
                string value = item.Value;
                if (item.Status != TranslationType.Translated)
                    value = "";

                var ti = new Xliff.TranslationItem(item.Name, item.Property, item.Source, value);
                ti.Value = ti.Value ?? String.Empty;
                foreignTranslation.FindOrAddTranslationCategory(translateItem.Category).Body.AddTranslationItem(ti);
            }
            Xliff.TranslationSerializer.Serialize(foreignTranslation, filename);
        }
    }
}
