using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ResourceManager.Translation;

namespace TranslationApp
{
    static class TranslationHelpers
    {
        public static void SaveTranslation(string languageCode, IEnumerable<TranslateItem> items, string filename)
        {
            Translation foreignTranslation = new Translation { GitExVersion = GitCommands.Settings.GitExtensionsVersionString, LanguageCode = languageCode };
            foreach (TranslateItem translateItem in items)
            {
                string value = translateItem.TranslatedValue ?? String.Empty;
                TranslationItem ti = new TranslationItem(translateItem.Name, translateItem.Property,
                                                         translateItem.NeutralValue, value);
                ti.Status = translateItem.Status;
                if (ti.Status == TranslationType.Obsolete &&
                    (String.IsNullOrEmpty(value) || String.IsNullOrEmpty(translateItem.NeutralValue)))
                    continue;
                if (string.IsNullOrEmpty(value))
                {
                    if (ti.Status == TranslationType.Translated || ti.Status == TranslationType.New)
                        ti.Status = TranslationType.Unfinished;
                }
                else
                {
                    // TODO: Support in form
                    if (ti.Status == TranslationType.Unfinished)
                        ti.Status = TranslationType.Translated;
                }
                foreignTranslation.FindOrAddTranslationCategory(translateItem.Category).AddTranslationItem(ti);
            }
            TranslationSerializer.Serialize(foreignTranslation, filename);
        }

        public static IList<Tuple<string, TranslationItem>> LoadNeutralItems()
        {
            string language = GitCommands.Settings.Translation;

            Translation neutralTranslation = new Translation();
            try
            {
                //Set language to neutral to get neutral translations
                GitCommands.Settings.Translation = "";

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
                GitCommands.Settings.Translation = language;
            }

            IList<Tuple<string, TranslationItem>> neutralItems =
                (from translationCategory in neutralTranslation.GetTranslationCategories()
                 from translationItem in translationCategory.GetTranslationItems()
                 select Tuple.Create(translationCategory.Name, translationItem)).ToList();
            return neutralItems;
        }

        public static List<TranslateItem> LoadTranslation(Translation translation, IList<Tuple<string, TranslationItem>> neutralItems)
        {
            List<TranslateItem> translateItems = new List<TranslateItem>();

            var translationItems =
                (from translationCategory in translation.GetTranslationCategories()
                 from translationItem in translationCategory.GetTranslationItems()
                 select Tuple.Create(translationCategory.Name, translationItem)).ToList();

            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var item in neutralItems)
            {
                var translateItem = new TranslateItem
                {
                    Category = item.Item1,
                    Name = item.Item2.Name,
                    Property = item.Item2.Property,
                    NeutralValue = item.Item2.Value,
                    Status = TranslationType.New
                };

                var curItem =
                    (from trItem in translationItems
                     where trItem.Item1.TrimStart('_') == item.Item1.TrimStart('_') &&
                     trItem.Item2.Name.TrimStart('_') == item.Item2.Name.TrimStart('_') &&
                     trItem.Item2.Property == item.Item2.Property
                     select trItem).FirstOrDefault();

                if (curItem != null)
                {
                    translateItem.TranslatedValue = curItem.Item2.Value;
                    if (item.Item2.CompareWithSource(curItem.Item2.Source))
                    {
                        if (!String.IsNullOrEmpty(curItem.Item2.Value))
                            translateItem.Status = TranslationType.Translated;
                        else
                            translateItem.Status = TranslationType.Unfinished;
                    }
                    else
                        translateItem.Status = TranslationType.Obsolete;
                    translationItems.Remove(curItem);
                    string source = curItem.Item2.Source ?? item.Item2.Value;
                    if (!String.IsNullOrEmpty(curItem.Item2.Value) && !dict.ContainsKey(source))
                        dict.Add(source, curItem.Item2.Value);
                }

                translateItems.Add(translateItem);
            }

            foreach (var item in translationItems)
            {
                if (String.IsNullOrEmpty(item.Item2.Value))
                    continue;

                var translateItem = new TranslateItem
                {
                    Category = item.Item1,
                    Name = item.Item2.Name,
                    Property = item.Item2.Property,
                    NeutralValue = item.Item2.Source,
                    TranslatedValue = item.Item2.Value,
                    Status = TranslationType.Obsolete
                };

                translateItems.Add(translateItem);
                if (item.Item2.Source != null && !dict.ContainsKey(item.Item2.Source))
                    dict.Add(item.Item2.Source, item.Item2.Value);
            }

            // update untranslated items
            var untranlatedItems = from trItem in translateItems
                                   where trItem.Status == TranslationType.New &&
                                   dict.ContainsKey(trItem.NeutralValue)
                                   select trItem;

            foreach (var untranlatedItem in untranlatedItems)
            {
                untranlatedItem.Status = TranslationType.Unfinished;
                untranlatedItem.TranslatedValue = dict[untranlatedItem.NeutralValue];
            }
            return translateItems;
        }
    }
}
