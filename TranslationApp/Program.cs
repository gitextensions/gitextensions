using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
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
            // This form created for obtain UI synchronization context only
            using (new Form())
            {
                // Store the shared JoinableTaskContext
                ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
            }

            // required for translation
            PluginRegistry.Initialize();

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
}