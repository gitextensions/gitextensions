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
            // required for translation
            GitUI.PluginLoader.Load();
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 1)
                Application.Run(new FormTranslate());
            else if (args.Length == 2 && args[1] == "update")
            {
                UpdateAllTranslations();
            }
            else if (args.Length == 2 && args[1] == "status")
                ShowStatus();
        }

        private static void UpdateAllTranslations()
        {
            Cursor.Current = Cursors.WaitCursor;
            var neutralItems = TranslationHelpers.LoadNeutralItems();

            var translationsNames = Translator.GetAllTranslations();
            foreach (var name in translationsNames)
            {
                Translation translation = Translator.GetTranslation(name);
                List<TranslationItemWithCategory> translateItems = TranslationHelpers.LoadTranslation(translation, neutralItems);
                string filename = Path.Combine(Translator.GetTranslationDir(), name + ".xml");
                TranslationHelpers.SaveTranslation(translation.LanguageCode, translateItems, filename);
            }
            Cursor.Current = Cursors.Default;
        }

        private static void ShowStatus()
        {
            Cursor.Current = Cursors.WaitCursor;
            var neutralItems = TranslationHelpers.LoadNeutralItems();

            var translationsNames = Translator.GetAllTranslations();
            var list = new List<KeyValuePair<string, int>>();
            foreach (var name in translationsNames)
            {
                Translation translation = Translator.GetTranslation(name);
                List<TranslationItemWithCategory> translateItems = TranslationHelpers.LoadTranslation(translation, neutralItems);
                int translatedCount = translateItems.Count(translateItem => translateItem.Status != TranslationType.Obsolete &&
                    !string.IsNullOrEmpty(translateItem.TranslatedValue));
                list.Add(new KeyValuePair<string, int>(name, translatedCount));
            }
            using (var stream = File.CreateText("statistic.csv"))
            {
                stream.WriteLine(string.Format("{0};{1};{2};{3}", "Language", "Percent", "TranslatedItems", "TotalItems"));
                foreach (var item in list.OrderByDescending(item => item.Value))
                    stream.WriteLine(string.Format("{0};{1:F}%;{2};{3}", item.Key, 
                        100.0f * item.Value / neutralItems.Count, item.Value, neutralItems.Count));
            }
            Cursor.Current = Cursors.Default;
        }

    }
}