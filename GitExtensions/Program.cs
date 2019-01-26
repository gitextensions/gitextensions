using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using GitExtUtils.GitUI;
using GitUI;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

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

            HighDpiMouseCursors.Enable();

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

                if (!Debugger.IsAttached)
                {
                    AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
                    Application.ThreadException += NBug.Handler.ThreadException;
                }
            }
            catch (TypeInitializationException tie)
            {
                // is this exception caused by the configuration?
                if (tie.InnerException != null
                    && tie.InnerException.GetType()
                        .IsSubclassOf(typeof(ConfigurationException)))
                {
                    HandleConfigurationException((ConfigurationException)tie.InnerException);
                }
            }

            // This is done here so these values can be used in the GitGui project but this project is the authority of the values.
            UserEnvironmentInformation.Initialise(ThisAssembly.Git.Sha, ThisAssembly.Git.IsDirty);

            // NOTE we perform the rest of the application's startup in another method to defer
            // the JIT processing more types than required to configure NBug.
            // In this way, there's more chance we can handle startup exceptions correctly.
            RunApplication();
        }

        private static void RunApplication()
        {
            string[] args = Environment.GetCommandLineArgs();

            // This form created to obtain UI synchronization context only
            using (new Form())
            {
                // Store the shared JoinableTaskContext
                ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
            }

            AppSettings.LoadSettings();
            if (EnvUtils.RunningOnWindows())
            {
                WebBrowserEmulationMode.SetBrowserFeatureControl();
                FormFixHome.CheckHomePath();
            }

            if (string.IsNullOrEmpty(AppSettings.Translation))
            {
                using (var formChoose = new FormChooseTranslation())
                {
                    formChoose.ShowDialog();
                }
            }

            try
            {
                // Ensure we can find the git command to execute,
                // unless we are being instructed to uninstall,
                // or AppSettings.CheckSettings is set to false.
                if (!(args.Length >= 2 && args[1] == "uninstall"))
                {
                    if (!CheckSettingsLogic.SolveGitCommand())
                    {
                        if (!LocateMissingGit())
                        {
                            Environment.Exit(-1);
                            return;
                        }
                    }

                    if (AppSettings.CheckSettings)
                    {
                        var uiCommands = new GitUICommands("");
                        var commonLogic = new CommonLogic(uiCommands.Module);
                        var checkSettingsLogic = new CheckSettingsLogic(commonLogic);
                        var fakePageHost = new SettingsPageHostMock(checkSettingsLogic);
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
            }
            catch
            {
                // TODO: remove catch-all
            }

            if (EnvUtils.RunningOnWindows())
            {
                MouseWheelRedirector.Active = true;
            }

            var commands = new GitUICommands(GetWorkingDir(args));

            if (args.Length <= 1)
            {
                commands.StartBrowseDialog();
            }
            else
            {
                // if we are here args.Length > 1
                commands.RunCommand(args);
            }

            AppSettings.SaveSettings();
        }

        [CanBeNull]
        private static string GetWorkingDir(string[] args)
        {
            string workingDir = null;

            if (args.Length >= 3)
            {
                // there is bug in .net
                // while parsing command line arguments, it unescapes " incorectly
                // https://github.com/gitextensions/gitextensions/issues/3489
                string dirArg = args[2].TrimEnd('"');

                if (!Directory.Exists(dirArg))
                {
                    dirArg = Path.GetDirectoryName(dirArg);
                }

                workingDir = GitModule.TryFindGitWorkingDir(dirArg);

                if (Directory.Exists(workingDir))
                {
                    workingDir = Path.GetFullPath(workingDir);
                }

                // Do not add this working directory to the recent repositories. It is a nice feature, but it
                // also increases the startup time
                ////if (Module.ValidWorkingDir())
                ////   Repositories.RepositoryHistory.AddMostRecentRepository(Module.WorkingDir);
            }

            if (args.Length <= 1 && workingDir == null && AppSettings.StartWithRecentWorkingDir)
            {
                if (GitModule.IsValidGitWorkingDir(AppSettings.RecentWorkingDir))
                {
                    workingDir = AppSettings.RecentWorkingDir;
                }
            }

            if (workingDir == null)
            {
                // If no working dir is yet found, try to find one relative to the current working directory.
                // This allows the `fileeditor` command to discover repository configuration which is
                // required for core.commentChar support.
                workingDir = GitModule.TryFindGitWorkingDir(Environment.CurrentDirectory);
            }

            return workingDir;
        }

        /// <summary>
        /// Used in the rare event that the configuration file for the application is corrupted
        /// </summary>
        private static void HandleConfigurationException(ConfigurationException ce)
        {
            bool exceptionHandled = false;
            try
            {
                // perhaps this should be checked for if it is null
                var in3 = ce.InnerException.InnerException;

                // saves having to have a reference to System.Xml just to check that we have an XmlException
                if (in3.GetType().Name == "XmlException")
                {
                    var localSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GitExtensions");

                    // assume that if we are having this error and the installation is not a portable one then the folder will exist.
                    if (Directory.Exists(localSettingsPath))
                    {
                        string messageContent = string.Format("There is a problem with the user.xml configuration file.{0}{0}The error message was: {1}{0}{0}The configuration file is usually found in: {2}{0}{0}Problems with configuration can usually be solved by deleting the configuration file. Would you like to delete the file?", Environment.NewLine, in3.Message, localSettingsPath);

                        if (DialogResult.Yes.Equals(MessageBox.Show(messageContent, "Configuration Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2)))
                        {
                            try
                            {
                                Directory.Delete(localSettingsPath, true); // deletes all application settings not just for this instance - but should work

                                // Restart GitExtensions with the same arguments after old config is deleted?
                                if (DialogResult.OK.Equals(MessageBox.Show(string.Format("Files have been deleted.{0}{0}Would you like to attempt to restart GitExtensions?", Environment.NewLine), "Configuration Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Information)))
                                {
                                    var args = Environment.GetCommandLineArgs();
                                    var p = new System.Diagnostics.Process { StartInfo = { FileName = args[0] } };
                                    if (args.Length > 1)
                                    {
                                        args[0] = "";
                                        p.StartInfo.Arguments = string.Join(" ", args);
                                    }

                                    p.Start();
                                }
                            }
                            catch (IOException)
                            {
                                MessageBox.Show(string.Format("Could not delete all files and folders in {0}!", localSettingsPath), "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }

                    // assuming that there is no localSettingsPath directory in existence we probably have a portable installation.
                    else
                    {
                        string messageContent = string.Format("There is a problem with the application settings XML configuration file.{0}{0}The error message was: {1}{0}{0}Problems with configuration can usually be solved by deleting the configuration file.", Environment.NewLine, in3.Message);
                        MessageBox.Show(messageContent, "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    exceptionHandled = true;
                }
            }
            finally
            {
                // if we fail in this somehow at least this message might get somewhere
                if (!exceptionHandled)
                {
                    MessageBox.Show(ce.ToString(), "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                Environment.Exit(1);
            }
        }

        private static bool LocateMissingGit()
        {
            int dialogResult = PSTaskDialog.cTaskDialog.ShowCommandBox(Title: "Error",
                                                                        MainInstruction: Strings.GitExecutableNotFound,
                                                                        Content: null,
                                                                        ExpandedInfo: null,
                                                                        Footer: null,
                                                                        VerificationText: null,
                                                                        CommandButtons: $"{Strings.FindGitExecutable}|{Strings.InstallGitInstructions}",
                                                                        ShowCancelButton: true,
                                                                        MainIcon: PSTaskDialog.eSysIcons.Error,
                                                                        FooterIcon: PSTaskDialog.eSysIcons.Warning);
            switch (dialogResult)
            {
                case 0:
                    {
                        using (var dialog = new OpenFileDialog
                        {
                            Filter = @"git.exe|git.exe|git.cmd|git.cmd",
                        })
                        {
                            if (dialog.ShowDialog(null) == DialogResult.OK)
                            {
                                AppSettings.GitCommandValue = dialog.FileName;
                            }

                            if (CheckSettingsLogic.SolveGitCommand())
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                case 1:
                    {
                        Process.Start(@"https://github.com/gitextensions/gitextensions/wiki/Application-Dependencies#git");
                        return false;
                    }

                default:
                    {
                        return false;
                    }
            }
        }
    }
}