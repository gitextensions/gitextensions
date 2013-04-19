using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using GitUI;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;

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

            if (!EnvUtils.IsMonoRuntime())
            {
                NBug.Settings.UIMode = NBug.Enums.UIMode.Full;

                // Uncomment the following after testing to see that NBug is working as configured
                NBug.Settings.ReleaseMode = true;
                NBug.Settings.ExitApplicationImmediately = false;
                NBug.Settings.WriteLogToDisk = true;
                NBug.Settings.MaxQueuedReports = 10;
                NBug.Settings.StopReportingAfter = 90;
                NBug.Settings.SleepBeforeSend = 30;
                NBug.Settings.StoragePath = "WindowsTemp";
                
                AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
                Application.ThreadException += NBug.Handler.ThreadException;
            }

            string[] args = Environment.GetCommandLineArgs();
            FormSplash.ShowSplash();
            //Store here SynchronizationContext.Current, because later sometimes it can be null
            //see http://stackoverflow.com/questions/11621372/synchronizationcontext-current-is-null-in-continuation-on-the-main-ui-thread
            GitUIExtensions.UISynchronizationContext = SynchronizationContext.Current;
            Application.DoEvents();

            Settings.LoadSettings();
            if (EnvUtils.RunningOnWindows())
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
                    Settings.GetBool("checksettings", true) ||
                    string.IsNullOrEmpty(Settings.GitCommand))
                {
                    FormSplash.SetAction("Checking settings...");
                    Application.DoEvents();

                    GitUICommands uiCommands = new GitUICommands(string.Empty);
                    var commonLogic = new CommonLogic(uiCommands.Module);
                    var checkSettingsLogic = new CheckSettingsLogic(commonLogic, uiCommands.Module);
                    using (var checklistSettingsPage = new ChecklistSettingsPage(commonLogic, checkSettingsLogic, uiCommands.Module, null))
                    {
                        if (!checklistSettingsPage.CheckSettings())
                        {
                            checkSettingsLogic.AutoSolveAllSettings();
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

            if (EnvUtils.RunningOnWindows())
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
                    workingDir = GitModule.FindGitWorkingDir(args[2]);
                else
                {
                    workingDir = Path.GetDirectoryName(args[2]);
                    workingDir = GitModule.FindGitWorkingDir(workingDir);
                }

                //Do not add this working dir to the recent repositories. It is a nice feature, but it
                //also increases the startup time
                //if (Module.ValidWorkingDir())
                //    Repositories.RepositoryHistory.AddMostRecentRepository(Module.WorkingDir);
            }

            if (args.Length <= 1 && string.IsNullOrEmpty(workingDir) && Settings.StartWithRecentWorkingDir)
            {
                if (GitModule.IsValidGitWorkingDir(Settings.RecentWorkingDir))
                    workingDir = Settings.RecentWorkingDir;
            }

            if (string.IsNullOrEmpty(workingDir))
            {
                string findWorkingDir = GitModule.FindGitWorkingDir(Directory.GetCurrentDirectory());
                if (GitModule.IsValidGitWorkingDir(findWorkingDir))
                    workingDir = findWorkingDir;
            }

            return workingDir;
        }
    }
}