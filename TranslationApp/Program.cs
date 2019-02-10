using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands.Utils;
using GitUI;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace TranslationApp
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (EnvUtils.RunningOnWindows())
            {
                NBug.Settings.UIMode = NBug.Enums.UIMode.Full;

                // Uncomment the following after testing to see that NBug is working as configured
                NBug.Settings.ReleaseMode = true;
                NBug.Settings.ExitApplicationImmediately = false;
                NBug.Settings.WriteLogToDisk = true;
                NBug.Settings.MaxQueuedReports = 10;
                NBug.Settings.StopReportingAfter = 90;
                NBug.Settings.SleepBeforeSend = 30;
                NBug.Settings.StoragePath = NBug.Enums.StoragePath.WindowsTemp;

                if (!Debugger.IsAttached)
                {
                    AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
                    Application.ThreadException += NBug.Handler.ThreadException;
                }
            }

            // This form created for obtain UI synchronization context only
            using (new Form())
            {
                // Store the shared JoinableTaskContext
                ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
            }

            // required for translation
            PluginRegistry.Initialize();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 1)
            {
                Application.Run(new FormTranslate());
            }
            else if (args.Length == 2 && args[1] == "update")
            {
                UpdateAllTranslations();
            }
            else if (args.Length == 2 && args[1] == "status")
            {
                ShowStatus();
            }
        }

        private static void UpdateAllTranslations()
        {
            using (new WaitCursorScope())
            {
                // we will be instantiating a number of forms using their default .ctors
                // this would lead to InvalidOperationException thrown in GitModuleForm()
                // set the flag that will stop this from happening
                GitModuleForm.IsUnitTestActive = true;

                var neutralItems = TranslationHelpers.LoadNeutralItems();
                string filename = Path.Combine(Translator.GetTranslationDir(), "English.xlf");
                TranslationHelpers.SaveTranslation(null, neutralItems, filename);

                var translationsNames = Translator.GetAllTranslations();
                foreach (var name in translationsNames)
                {
                    var translation = Translator.GetTranslation(name);
                    var translateItems = TranslationHelpers.LoadTranslation(translation, neutralItems);
                    filename = Path.Combine(Translator.GetTranslationDir(), name + ".xlf");
                    TranslationHelpers.SaveTranslation(translation.First().Value.TargetLanguage, translateItems, filename);
                }
            }
        }

        private static void ShowStatus()
        {
            using (new WaitCursorScope())
            {
                var neutralItems = TranslationHelpers.LoadNeutralItems();

                var translationsNames = Translator.GetAllTranslations();
                var list = new List<KeyValuePair<string, int>>();
                foreach (var name in translationsNames)
                {
                    var translation = Translator.GetTranslation(name);
                    var translateItems = TranslationHelpers.LoadTranslation(translation, neutralItems);
                    int translatedCount = translateItems
                        .Sum(p => p.Value.Count(translateItem => !string.IsNullOrEmpty(translateItem.TranslatedValue)));
                    list.Add(new KeyValuePair<string, int>(name, translatedCount));
                }

                using (var stream = File.CreateText("statistic.csv"))
                {
                    stream.WriteLine("{0};{1};{2};{3}", "Language", "Percent", "TranslatedItems", "TotalItems");
                    foreach (var (language, translatedItems) in list.OrderByDescending(item => item.Value))
                    {
                        stream.WriteLine(
                            "{0};{1:F}%;{2};{3}", language, 100.0f * translatedItems / neutralItems.Count, translatedItems,
                            neutralItems.Count);
                    }
                }
            }
        }
    }
}