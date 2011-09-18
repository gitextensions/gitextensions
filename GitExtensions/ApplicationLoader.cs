using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitUI;
using GitCommands;
using System.IO;

namespace GitExtensions
{
    public static class ApplicationLoader
    {
        private static bool isLoaded = false;
        
        public static void Load()
        {
            if (isLoaded)
                return;

            FormSplash.Show("Load settings");
            Settings.LoadSettings();
            if (Settings.RunningOnWindows())
            {
                //Quick HOME check:
                FormSplash.SetAction("Check home path");
                FormFixHome.CheckHomePath();
            }
            //Register plugins
            FormSplash.SetAction("Load plugins");
            PluginLoader.LoadAsync();

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
                    FormSplash.SetAction("Check settings");
                    using (var settings = new FormSettings())
                    {
                        if (!settings.CheckSettings())
                        {
                            FormSettings.AutoSolveAllSettings();
                            GitUICommands.Instance.StartSettingsDialog();
                        }
                    }
                }
            }
            catch
            {
                // TODO: remove catch-all
            }

            if (string.IsNullOrEmpty(Settings.WorkingDir))
            {
                string findWorkingDir = GitCommandHelpers.FindGitWorkingDir(Directory.GetCurrentDirectory());
                if (Settings.ValidWorkingDir(findWorkingDir))
                    Settings.WorkingDir = findWorkingDir;
            }

            FormSplash.Hide();

            isLoaded = true;
        }
    }
}
