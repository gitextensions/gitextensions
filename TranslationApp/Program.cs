using System;
using System.IO;
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
            {
                // TODO: Generate information with translation status
            }
        }
    }
}