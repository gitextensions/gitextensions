using System.Configuration;
using System.Diagnostics;
using GitCommands;
using GitCommands.Utils;
using GitExtUtils.GitUI;
using GitUI;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.Infrastructure.Telemetry;
using GitUI.NBugReports;
using GitUI.Theming;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitExtensions
{
    internal static class Program
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            // If an error happens before we had a chance to init the environment information
            // the call to GetInformation() from BugReporter.ShowNBug() will fail.
            // There's no perf hit calling Initialise() multiple times.
            UserEnvironmentInformation.Initialise(ThisAssembly.Git.Sha, ThisAssembly.Git.IsDirty);

            AppSettings.SetDocumentationBaseUrl(ThisAssembly.Git.Branch);

            ThemeModule.Load();
#if SUPPORT_THEME_HOOKS
            Application.ApplicationExit += (s, e) => ThemeModule.Unload();

            SystemEvents.UserPreferenceChanged += (s, e) =>
            {
                // Whenever a user changes monitor scaling (e.g. 100%->125%) unload and
                // reload the theme, and repaint all forms
                if (e.Category == UserPreferenceCategory.Desktop || e.Category == UserPreferenceCategory.VisualStyle)
                {
                    ThemeModule.ReloadWin32ThemeData();
                    foreach (Form form in Application.OpenForms)
                    {
                        form.BeginInvoke((MethodInvoker)(() => form.Invalidate()));
                    }
                }
            };
#endif

            HighDpiMouseCursors.Enable();

            try
            {
                DiagnosticsClient.Initialize(ThisAssembly.Git.IsDirty);

                // If you want to suppress the BugReportInvoker when debugging and exit quickly, uncomment the condition:
                ////if (!Debugger.IsAttached)
                {
                    AppDomain.CurrentDomain.UnhandledException += (s, e) => BugReportInvoker.Report((Exception)e.ExceptionObject, e.IsTerminating);
                    Application.ThreadException += (s, e) => BugReportInvoker.Report(e.Exception, isTerminating: false);
                }
            }
            catch (TypeInitializationException tie)
            {
                // is this exception caused by the configuration?
                if (tie.InnerException is not null
                    && tie.InnerException.GetType()
                        .IsSubclassOf(typeof(ConfigurationException)))
                {
                    HandleConfigurationException((ConfigurationException)tie.InnerException);
                }
            }

            AppTitleGenerator.Initialise(ThisAssembly.Git.Sha, ThisAssembly.Git.Branch);

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

            ManagedExtensibility.Initialise(new[]
                {
                    typeof(GitUI.GitExtensionsForm).Assembly,
                    typeof(GitCommands.GitModule).Assembly,
                    typeof(ResourceManager.GitPluginBase).Assembly
                },
                AppSettings.UserPluginsPath);

            AppSettings.LoadSettings();

            if (EnvUtils.RunningOnWindows())
            {
                WebBrowserEmulationMode.SetBrowserFeatureControl();
                FormFixHome.CheckHomePath();
            }

            if (string.IsNullOrEmpty(AppSettings.Translation))
            {
                using FormChooseTranslation formChoose = new();
                formChoose.ShowDialog();
            }

            AppSettings.TelemetryEnabled ??= MessageBox.Show(
                null,
                ResourceManager.TranslatedStrings.TelemetryPermissionMessage,
                ResourceManager.TranslatedStrings.TelemetryPermissionCaption,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes;

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
                        GitUICommands uiCommands = new("");
                        CommonLogic commonLogic = new(uiCommands.Module);
                        CheckSettingsLogic checkSettingsLogic = new(commonLogic);
                        SettingsPageHostMock fakePageHost = new(checkSettingsLogic);
                        using var checklistSettingsPage = SettingsPageBase.Create<ChecklistSettingsPage>(fakePageHost);
                        if (!checklistSettingsPage.CheckSettings())
                        {
                            if (!checkSettingsLogic.AutoSolveAllSettings() || !checklistSettingsPage.CheckSettings())
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

            if (EnvUtils.RunningOnWindows())
            {
                MouseWheelRedirector.Active = true;
            }

            GitUICommands commands = new(GetWorkingDir(args));

            if (args.Length <= 1)
            {
                commands.StartBrowseDialog(owner: null);
            }
            else
            {
                // if we are here args.Length > 1

                // Avoid replacing the ExitCode eventually set while parsing arguments,
                // i.e. assume -1 and afterwards, only set it to 0 if no error is indicated.
                Environment.ExitCode = -1;
                if (commands.RunCommand(args))
                {
                    Environment.ExitCode = 0;
                }
            }

            AppSettings.SaveSettings();
        }

        private static string? GetWorkingDir(string[] args)
        {
            string? workingDir = null;

            if (args.Length >= 3)
            {
                // there is bug in .net
                // while parsing command line arguments, it unescapes " incorrectly
                // https://github.com/gitextensions/gitextensions/issues/3489
                string dirArg = args[2].TrimEnd('"');
                if (!string.IsNullOrWhiteSpace(dirArg))
                {
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
            }

            if (args.Length <= 1 && workingDir is null && AppSettings.StartWithRecentWorkingDir)
            {
                if (GitModule.IsValidGitWorkingDir(AppSettings.RecentWorkingDir))
                {
                    workingDir = AppSettings.RecentWorkingDir;
                }
            }

            if (args.Length > 1 && workingDir is null)
            {
                // If no working dir is yet found, try to find one relative to the current working directory.
                // This allows the `fileeditor` command to discover repository configuration which is
                // required for core.commentChar support.
                workingDir = GitModule.TryFindGitWorkingDir(Environment.CurrentDirectory);
            }

            return workingDir;
        }

        /// <summary>
        /// Used in the rare event that the configuration file for the application is corrupted.
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

                        if (MessageBox.Show(messageContent, "Configuration Error",
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                        {
                            if (localSettingsPath.TryDeleteDirectory(out string? errorMessage))
                            {
                                // Restart Git Extensions with the same arguments after old config is deleted?
                                if (DialogResult.OK.Equals(MessageBox.Show(string.Format("Files have been deleted.{0}{0}Would you like to attempt to restart Git Extensions?", Environment.NewLine), "Configuration Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Question)))
                                {
                                    var args = Environment.GetCommandLineArgs();
                                    Process p = new() { StartInfo = { FileName = args[0] } };
                                    if (args.Length > 1)
                                    {
                                        args[0] = "";
                                        p.StartInfo.Arguments = string.Join(" ", args);
                                    }

                                    p.Start();
                                }
                            }
                            else
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
            TaskDialogPage page = new()
            {
                Heading = ResourceManager.TranslatedStrings.GitExecutableNotFound,
                Icon = TaskDialogIcon.Error,
                Buttons = { TaskDialogButton.Cancel },
                AllowCancel = true,
                SizeToContent = true
            };
            TaskDialogCommandLinkButton btnFindGitExecutable = new(ResourceManager.TranslatedStrings.FindGitExecutable);
            TaskDialogCommandLinkButton btnInstallGitInstructions = new(ResourceManager.TranslatedStrings.InstallGitInstructions);
            page.Buttons.Add(btnFindGitExecutable);
            page.Buttons.Add(btnInstallGitInstructions);

            TaskDialogButton result = TaskDialog.ShowDialog(page);
            if (result == btnFindGitExecutable)
            {
                using OpenFileDialog dialog = new() { Filter = @"git.exe|git.exe|git.cmd|git.cmd" };
                if (dialog.ShowDialog(null) == DialogResult.OK)
                {
                    AppSettings.GitCommandValue = dialog.FileName;
                }

                return CheckSettingsLogic.SolveGitCommand();
            }

            if (result == btnInstallGitInstructions)
            {
                OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/wiki/Application-Dependencies#git");
                return false;
            }

            return false;
        }
    }
}
