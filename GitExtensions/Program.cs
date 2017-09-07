using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using GitUI;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using System.Threading.Tasks;

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
                try
                {
                    NBug.Settings.UIMode = NBug.Enums.UIMode.Full;

                    // Uncomment the following after testing to see that NBug is working as configured
                    NBug.Settings.ReleaseMode = true;
                    NBug.Settings.ExitApplicationImmediately = false;
                    NBug.Settings.WriteLogToDisk = false;
                    NBug.Settings.MaxQueuedReports = 10;
                    NBug.Settings.StopReportingAfter = 90;
                    NBug.Settings.SleepBeforeSend = 30;
                    NBug.Settings.StoragePath = NBug.Enums.StoragePath.WindowsTemp;

                    AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
                    Application.ThreadException += NBug.Handler.ThreadException;

                }
                catch (TypeInitializationException tie)
                {
                    // is this exception caused by the configuration?
                    if (tie.InnerException != null
                        && tie.InnerException.GetType()
                            .IsSubclassOf(typeof(System.Configuration.ConfigurationException)))
                    {
                        HandleConfigurationException((System.Configuration.ConfigurationException)tie.InnerException);
                    }
                }
            }

            string[] args = Environment.GetCommandLineArgs();
            FormSplash.ShowSplash();
            //Store here SynchronizationContext.Current, because later sometimes it can be null
            //see http://stackoverflow.com/questions/11621372/synchronizationcontext-current-is-null-in-continuation-on-the-main-ui-thread
            GitUIExtensions.UISynchronizationContext = SynchronizationContext.Current;
            AsyncLoader.DefaultContinuationTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Application.DoEvents();

            AppSettings.LoadSettings();
            if (EnvUtils.RunningOnWindows())
            {
              WebBrowserEmulationMode.SetBrowserFeatureControl();

              //Quick HOME check:
                FormSplash.SetAction("Checking home path...");
                Application.DoEvents();

                FormFixHome.CheckHomePath();
            }
            //Register plugins
            FormSplash.SetAction("Loading plugins...");
            Application.DoEvents();

            if (string.IsNullOrEmpty(AppSettings.Translation))
            {
                using (var formChoose = new FormChooseTranslation())
                {
                    formChoose.ShowDialog();
                }
            }

            try
            {
                if (!(args.Length >= 2 && args[1].Equals("uninstall"))
                    && (AppSettings.CheckSettings 
                    || string.IsNullOrEmpty(AppSettings.GitCommandValue)
                    || !File.Exists(AppSettings.GitCommandValue)))
                {
                    FormSplash.SetAction("Checking settings...");
                    Application.DoEvents();

                    GitUICommands uiCommands = new GitUICommands(string.Empty);
                    var commonLogic = new CommonLogic(uiCommands.Module);
                    var checkSettingsLogic = new CheckSettingsLogic(commonLogic);
                    ISettingsPageHost fakePageHost = new SettingsPageHostMock(checkSettingsLogic);
                    using (var checklistSettingsPage = SettingsPageBase.Create<ChecklistSettingsPage>(fakePageHost))
                    {
                        if (!checklistSettingsPage.CheckSettings())
                        {
                            if (!checkSettingsLogic.AutoSolveAllSettings())
                            {
                                uiCommands.StartSettingsDialog();
                            }
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

            AppSettings.SaveSettings();
        }

        private static string GetWorkingDir(string[] args)
        {
            string workingDir = string.Empty;
            if (args.Length >= 3)
            {
                //there is bug in .net
                //while parsing command line arguments, it unescapes " incorectly
                //https://github.com/gitextensions/gitextensions/issues/3489
                string dirArg = args[2].TrimEnd('"');
                if (Directory.Exists(dirArg))
                    workingDir = GitModule.FindGitWorkingDir(dirArg);
                else
                {
                    workingDir = Path.GetDirectoryName(dirArg);
                    workingDir = GitModule.FindGitWorkingDir(workingDir);
                }

                if (Directory.Exists(workingDir))
                    workingDir = Path.GetFullPath(workingDir);

                //Do not add this working directory to the recent repositories. It is a nice feature, but it
                //also increases the startup time
                //if (Module.ValidWorkingDir())
                //    Repositories.RepositoryHistory.AddMostRecentRepository(Module.WorkingDir);
            }

            if (args.Length <= 1 && string.IsNullOrEmpty(workingDir) && AppSettings.StartWithRecentWorkingDir)
            {
                if (GitModule.IsValidGitWorkingDir(AppSettings.RecentWorkingDir))
                    workingDir = AppSettings.RecentWorkingDir;
            }

            if (string.IsNullOrEmpty(workingDir))
            {
                string findWorkingDir = GitModule.FindGitWorkingDir(Directory.GetCurrentDirectory());
                if (GitModule.IsValidGitWorkingDir(findWorkingDir))
                    workingDir = findWorkingDir;
            }

            return workingDir;
        }

        /// <summary>
        /// Used in the rare event that the configuration file for the application is corrupted
        /// </summary>
        /// <param name="ce"></param>
        private static void HandleConfigurationException(System.Configuration.ConfigurationException ce)
        {
            bool exceptionHandled = false;
            try
            {
                // perhaps this should be checked for if it is null
                var in3 = ce.InnerException.InnerException;

                // saves having to have a reference to System.Xml just to check that we have an XmlException
                if (in3.GetType().Name.Equals("XmlException"))
                {
                    var localSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GitExtensions");

                    //assume that if we are having this error and the installation is not a portable one then the folder will exist.
                    if (Directory.Exists(localSettingsPath))
                    {
                        string messageContent = String.Format("There is a problem with the user.xml configuration file.{0}{0}The error message was: {1}{0}{0}The configuration file is usually found in: {2}{0}{0}Problems with configuration can usually be solved by deleting the configuration file. Would you like to delete the file?", Environment.NewLine, in3.Message, localSettingsPath);

                        if (DialogResult.Yes.Equals(MessageBox.Show(messageContent, "Configuration Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2)))
                        {
                            try
                            {
                                Directory.Delete(localSettingsPath, true); //deletes all application settings not just for this instance - but should work
                                //Restart GitExtensions with the same arguments after old config is deleted?
                                if (DialogResult.OK.Equals(MessageBox.Show(String.Format("Files have been deleted.{0}{0}Would you like to attempt to restart GitExtensions?", Environment.NewLine), "Configuration Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Information)))
                                {
                                    var args = Environment.GetCommandLineArgs();
                                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                                    p.StartInfo.FileName = args[0];
                                    if (args.Length > 1)
                                    {
                                        args[0] = "";
                                        p.StartInfo.Arguments = String.Join(" ", args);
                                    }
                                    p.Start();
                                }
                            }
                            catch (IOException)
                            {
                                MessageBox.Show(String.Format("Could not delete all files and folders in {0}!", localSettingsPath), "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    //assuming that there is no localSettingsPath directory in existence we probably have a portable installation.
                    else
                    {
                        string messageContent = String.Format("There is a problem with the application settings XML configuration file.{0}{0}The error message was: {1}{0}{0}Problems with configuration can usually be solved by deleting the configuration file.", Environment.NewLine, in3.Message);
                        MessageBox.Show(messageContent, "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    exceptionHandled = true;
                }
            }
            finally // if we fail in this somehow at least this message might get somewhere
            {
                if (!exceptionHandled)
                {
                    MessageBox.Show(ce.ToString(), "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                System.Environment.Exit(1);
            }
        }

    }
}