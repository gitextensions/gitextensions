using BugReporter;
using GitCommands;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Extensibility.Translations.Xliff;
using GitUI;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

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
            // This form created for obtain UI synchronization context only
            using (new Form())
            {
                // Store the shared JoinableTaskContext
                ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
            }

            // Force load into the appdomain
            using (BugReportForm dummy = new())
            {
            }

            ManagedExtensibility.Initialise();

            // Required for translation
            PluginRegistry.InitializeAll();

            // We will be instantiating a number of forms using their default constructors.
            // This would lead to InvalidOperationException thrown in GitModuleForm().
            // Set the flag that will stop this from happening.
            GitModuleForm.IsUnitTestActive = true;

            AppSettings.Font = SystemFonts.MessageBoxFont;

            IDictionary<string, List<TranslationItemWithCategory>> neutralItems = TranslationHelpers.LoadNeutralItems();
            string filename = Path.Combine(Translator.GetTranslationDir(), "English.xlf");
            TranslationHelpers.SaveTranslation(null, neutralItems, filename);

            string[] translationsNames = Translator.GetAllTranslations();
            foreach (string name in translationsNames)
            {
                IDictionary<string, TranslationFile> translation = Translator.GetTranslation(name);
                IDictionary<string, List<TranslationItemWithCategory>> translateItems = TranslationHelpers.LoadTranslation(translation, neutralItems);
                filename = Path.Combine(Translator.GetTranslationDir(), name + ".xlf");
                TranslationHelpers.SaveTranslation(translation.First().Value.TargetLanguage, translateItems, filename);
            }
        }
    }
}
