using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI;

namespace GitExtensions
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

            string[] args = Environment.GetCommandLineArgs();
            FormSplash.ShowSplash();
            Application.DoEvents();

            Settings.LoadSettings();
            if (Settings.RunningOnWindows())
            {
                //Quick HOME check:
                FormSplash.SetAction("Checking home path...");
                Application.DoEvents();

                FormFixHome.CheckHomePath();
            }
            //Register plugins
            FormSplash.SetAction("Loading plugins...");
            Application.DoEvents();

            if (string.IsNullOrEmpty(Settings.Translation))
            {
                using (var formChoose = new FormChooseTranslation())
                {
                    formChoose.ShowDialog();
                }
            }

            try
            {
                if (Application.UserAppDataRegistry == null ||
                    Settings.GetValue<string>("checksettings", null) == null ||
                    !Settings.GetValue<string>("checksettings", null).ToString().Equals("false", StringComparison.OrdinalIgnoreCase) ||
                    string.IsNullOrEmpty(Settings.GitCommand))
                {
                    FormSplash.SetAction("Checking settings...");
                    Application.DoEvents();

                    GitUICommands uiCommands = new GitUICommands(string.Empty);
                    using (var settings = new FormSettings(uiCommands))
                    {
                        if (!settings.CheckSettings())
                        {
                            settings.AutoSolveAllSettings();
                            uiCommands.StartSettingsDialog();
                        }
                    }
                }
            }
            catch
            {
                // TODO: remove catch-all
            }


            FormSplash.HideSplash();

            if (Settings.RunningOnWindows())
                MouseWheelRedirector.Active = true;

            GitUICommands uCommands = new GitUICommands(GetWorkingDir(args));

            if (args.Length <= 1)
            {
                uCommands.StartBrowseDialog();
            }
            else  // if we are here args.Length > 1
            {
                uCommands.RunCommand(args);
            }

            Settings.SaveSettings();
        }

        private static string GetWorkingDir(string[] args)
        {
            string workingDir = string.Empty;
            if (args.Length >= 3)
            {
                if (Directory.Exists(args[2]))
                    workingDir = args[2];

                if (string.IsNullOrEmpty(workingDir))
                {
                    if (args[2].Contains(Settings.PathSeparator.ToString()))
                        workingDir = args[2].Substring(0, args[2].LastIndexOf(Settings.PathSeparator));
                }

                //Do not add this working dir to the recent repositories. It is a nice feature, but it
                //also increases the startup time
                //if (Module.ValidWorkingDir())
                //    Repositories.RepositoryHistory.AddMostRecentRepository(Module.WorkingDir);
            }

            if (args.Length <= 1 && string.IsNullOrEmpty(workingDir) && Settings.StartWithRecentWorkingDir)
            {
                if (GitModule.ValidWorkingDir(Settings.RecentWorkingDir))
                    workingDir = Settings.RecentWorkingDir;
            }

            if (string.IsNullOrEmpty(workingDir))
            {
                string findWorkingDir = GitModule.FindGitWorkingDir(Directory.GetCurrentDirectory());
                if (GitModule.ValidWorkingDir(findWorkingDir))
                    workingDir = findWorkingDir;
            }

            return workingDir;
        }
    }
}