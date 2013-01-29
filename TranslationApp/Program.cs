using ResourceManager.Translation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TranslationApp
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 1)
                Application.Run(new FormTranslate());
            else if (args.Length == 2 && args[1] == "update")
            {
                // TODO: Update all translations
            }
            else if (args.Length == 2 && args[1] == "status")
                ShowStatus();
        }

        private static void ShowStatus()
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

            using (var stream = File.CreateText("statistic.csv"))
            {
                stream.WriteLine(string.Format("{0};{1};{2};{3}", "Language", "Percent", "TranslatedItems", "TotalItems"));
                var translationsNames = Translator.GetAllTranslations();
                foreach (var name in translationsNames)
                {
                    Translation translation = Translator.GetTranslation(name);
                    List<TranslateItem> translateItems;
                    LoadTranslation(translation, neutralItems, out translateItems);
                    int translatedCount = translateItems.Count(translateItem => translateItem.Status != TranslationType.Obsolete && 
                        !string.IsNullOrEmpty(translateItem.TranslatedValue));
                    stream.WriteLine(string.Format("{0};{1:F}%;{2};{3}", name, 100.0f * translatedCount / neutralItems.Count, translatedCount, neutralItems.Count));
                }
            }
        }

        private static void LoadTranslation(Translation translation, IList<Tuple<string, TranslationItem>> neutralItems,
            out List<TranslateItem> translateItems)
        {
            translateItems = new List<TranslateItem>();

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
        }
    }
}