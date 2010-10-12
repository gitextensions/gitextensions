using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
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
            using (var formSplash = new FormSplash())
            {
                formSplash.Show();
                formSplash.SetAction("Load settings");
                Settings.LoadSettings();
                if (Settings.RunningOnWindows())
                {
                    //Quick HOME check:
                    formSplash.SetAction("Check home path");
                    FormFixHome.CheckHomePath();
                }
                //Register plugins
                formSplash.SetAction("Load plugins");
                PluginLoader.Load();

                try
                {
                    if (Application.UserAppDataRegistry == null ||
                        Application.UserAppDataRegistry.GetValue("checksettings") == null ||
                        !Application.UserAppDataRegistry.GetValue("checksettings").ToString().Equals("false", StringComparison.OrdinalIgnoreCase) ||
                        string.IsNullOrEmpty(Settings.GitCommand))
                    {
                        formSplash.SetAction("Check settings");
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
            }

            if (args.Length >= 3)
            {
                if (Directory.Exists(args[2]))
                    Settings.WorkingDir = args[2];

                if (string.IsNullOrEmpty(Settings.WorkingDir))
                {
                    if (args[2].Contains(Settings.PathSeperator.ToString()))
                        Settings.WorkingDir = args[2].Substring(0, args[2].LastIndexOf(Settings.PathSeperator));
                }

                if (Settings.ValidWorkingDir())
                    Repositories.RepositoryHistory.AddMostRecentRepository(Settings.WorkingDir);
            }

            if (string.IsNullOrEmpty(Settings.WorkingDir))
                Settings.WorkingDir = Directory.GetCurrentDirectory();

            if (args.Length <= 1)
                GitUICommands.Instance.StartBrowseDialog();
            else  // if we are here args.Length > 1
                RunCommand(args);

            Settings.SaveSettings();
        }

        private static void RunCommand(string[] args)
        {
            if (args.Length > 1)
            {
                switch (args[1])
                {
                    case "mergeconflicts":
                        GitUICommands.Instance.StartResolveConflictsDialog();
                        return;
                    case "gitbash":
                        GitCommands.GitCommands.RunBash();
                        return;
                    case "gitignore":
                        GitUICommands.Instance.StartEditGitIgnoreDialog();
                        return;
                    case "remotes":
                        GitUICommands.Instance.StartRemotesDialog();
                        return;
                    case "blame":
                        if (args.Length > 2)
                        {
                            // Remove working dir from filename. This is to prevent filenames that are too
                            // long while there is room left when the workingdir was not in the path.
                            string fileName = args[2].Replace(Settings.WorkingDir, "").Replace('\\', '/');

                            GitUICommands.Instance.StartBlameDialog(fileName);
                        }
                        else
                            MessageBox.Show("Cannot open blame, there is no file selected.", "Blame");
                        return;
                    case "browse":
                        GitUICommands.Instance.StartBrowseDialog(GetParameterOrEmptyStringAsDefault(args, "-filter"));
                        return;
                    case "add":
                    case "addfiles":
                        GitUICommands.Instance.StartAddFilesDialog();
                        return;
                    case "apply":
                    case "applypatch":
                        GitUICommands.Instance.StartApplyPatchDialog();
                        return;
                    case "branch":
                        GitUICommands.Instance.StartCreateBranchDialog();
                        return;
                    case "checkout":
                    case "checkoutbranch":
                        GitUICommands.Instance.StartCheckoutBranchDialog();
                        return;
                    case "checkoutrevision":
                        GitUICommands.Instance.StartCheckoutRevisionDialog();
                        return;
                    case "init":
                        if (args.Length > 2)
                            GitUICommands.Instance.StartInitializeDialog(args[2]);
                        else
                            GitUICommands.Instance.StartInitializeDialog();
                        return;
                    case "clone":
                        GitUICommands.Instance.StartCloneDialog();
                        return;
                    case "commit":
                        GitUICommands.Instance.StartCommitDialog();
                        return;
                    case "filehistory":
                        if (args.Length > 2)
                        {
                            //Remove working dir from filename. This is to prevent filenames that are too
                            //long while there is room left when the workingdir was not in the path.
                            string fileName = args[2].Replace(Settings.WorkingDir, "").Replace('\\', '/');

                            GitUICommands.Instance.StartFileHistoryDialog(fileName);
                        }
                        else
                            MessageBox.Show("Cannot open file history, there is no file selected.", "File history");
                        return;
                    case "fileeditor":
                        if (args.Length > 2)
                        {
                            using (var formEditor = new FormEditor(args[2]))
                            {
                                formEditor.ShowDialog();
                            }
                        }
                        else
                            MessageBox.Show("Cannot open file editor, there is no file selected.", "File editor");
                        return;
                    case "formatpatch":
                        GitUICommands.Instance.StartFormatPatchDialog();
                        return;
                    case "pull":
                        GitUICommands.Instance.StartPullDialog();
                        return;
                    case "push":
                        GitUICommands.Instance.StartPushDialog();
                        return;
                    case "settings":
                        GitUICommands.Instance.StartSettingsDialog();
                        return;
                    case "viewdiff":
                        GitUICommands.Instance.StartCompareRevisionsDialog();
                        return;
                    case "rebase":
                        GitUICommands.Instance.StartRebaseDialog(null);
                        return;
                    case "merge":
                        GitUICommands.Instance.StartMergeBranchDialog(null);
                        return;
                    case "cherry":
                        GitUICommands.Instance.StartCherryPickDialog();
                        return;
                    case "revert":
                        Application.Run(new FormRevert(args[2]));
                        return;
                    case "tag":
                        GitUICommands.Instance.StartCreateTagDialog();
                        return;
                    case "about":
                        Application.Run(new AboutBox());
                        return;
                    case "stash":
                        GitUICommands.Instance.StartStashDialog();
                        return;
                    default:
                        Application.Run(new FormCommandlineHelp());
                        return;
                }
            }
        }

        private static string GetParameterOrEmptyStringAsDefault(string[] args, string paramName)
        {
            foreach (string arg in args)
                if (arg.StartsWith(paramName + "="))
                    return args[2].Replace(paramName + "=", "");

            return string.Empty;
        }
    }
}