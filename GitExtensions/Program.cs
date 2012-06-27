using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Repository;
using GitUI;
using System.Collections.Generic;

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
                    FormSplash.SetAction("Checking settings...");
                    Application.DoEvents();

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

            if (args.Length >= 3)
            {
                if (Directory.Exists(args[2]))
                    Settings.WorkingDir = args[2];

                if (string.IsNullOrEmpty(Settings.WorkingDir))
                {
                    if (args[2].Contains(Settings.PathSeparator.ToString()))
                        Settings.WorkingDir = args[2].Substring(0, args[2].LastIndexOf(Settings.PathSeparator));
                }

                //Do not add this working dir to the recent repositories. It is a nice feature, but it
                //also increases the startup time
                //if (Settings.Module.ValidWorkingDir())
                //    Repositories.RepositoryHistory.AddMostRecentRepository(Settings.WorkingDir);
            }

            if (args.Length <= 1 && string.IsNullOrEmpty(Settings.WorkingDir) && Settings.StartWithRecentWorkingDir)
            {
                if (GitModule.ValidWorkingDir(Settings.RecentWorkingDir))
                    Settings.WorkingDir = Settings.RecentWorkingDir;
            }

            if (string.IsNullOrEmpty(Settings.WorkingDir))
            {
                string findWorkingDir = GitModule.FindGitWorkingDir(Directory.GetCurrentDirectory());
                if (GitModule.ValidWorkingDir(findWorkingDir))
                    Settings.WorkingDir = findWorkingDir;
            }

            FormSplash.HideSplash();

            if (Settings.RunningOnWindows())
                MouseWheelRedirector.Active = true;

            if (args.Length <= 1)
            {
                GitUICommands.Instance.StartBrowseDialog();
            }
            else  // if we are here args.Length > 1
            {
                RunCommand(args);
            }

            Settings.SaveSettings();
        }

        private static void RunCommand(string[] args)
        {
            var arguments = InitializeArguments(args);

            if (args.Length <= 1)
                return;

            if (args[1].Equals("blame") && args.Length <= 2)
            {
                MessageBox.Show("Cannot open blame, there is no file selected.", "Blame");
                return;
            }
            if (args[1].Equals("filehistory") && args.Length <= 2)
            {
                MessageBox.Show("Cannot open file history, there is no file selected.", "File history");
                return;
            }
            if (args[1].Equals("fileeditor") && args.Length <= 2)
            {
                MessageBox.Show("Cannot open file editor, there is no file selected.", "File editor");
                return;
            }

            RunCommandBasedOnArgument(args, arguments);
        }

        // Please update FormCommandlineHelp if you add or change commands
        private static void RunCommandBasedOnArgument(string[] args, Dictionary<string, string> arguments)
        {
            switch (args[1])
            {
                case "about":
                    Application.Run(new AboutBox());
                    return;
                case "add":
                case "addfiles":
                    GitUICommands.Instance.StartAddFilesDialog();
                    return;
                case "apply":
                case "applypatch":
                    GitUICommands.Instance.StartApplyPatchDialog();
                    return;
                case "blame":       // file
                    RunBlameCommand(args);
                    return;
                case "branch":
                    GitUICommands.Instance.StartCreateBranchDialog();
                    return;
                case "browse":      // [path] [-filter]
                    RunBrowseCommand(args);
                    return;
                case "checkout":
                case "checkoutbranch":
                    GitUICommands.Instance.StartCheckoutBranchDialog();
                    return;
                case "checkoutrevision":
                    GitUICommands.Instance.StartCheckoutRevisionDialog();
                    return;
                case "cherry":
                    GitUICommands.Instance.StartCherryPickDialog();
                    return;
                case "cleanup":
                    using (var form = new FormCleanupRepository())
                        form.ShowDialog();
                    return;
                case "clone":       // [path]
                    RunCloneCommand(args);
                    return;
                case "commit":      // [--quiet]
                    Commit(arguments);
                    return;
                case "filehistory": // [filename]
                    RunFileHistoryCommand(args);
                    return;
                case "fileeditor":  // [filename]
                    RunFileEditorCommand(args);
                    return;
                case "formatpatch":
                    GitUICommands.Instance.StartFormatPatchDialog();
                    return;
                case "gitbash":
                    Settings.Module.RunBash();
                    return;
                case "gitignore":
                    GitUICommands.Instance.StartEditGitIgnoreDialog();
                    return;
                case "init":        // [path]
                    RunInitCommand(args);
                    return;
                case "merge":       // [--branch name]
                    RunMergeCommand(arguments);
                    return;
                case "mergeconflicts": // [--quiet]
                case "mergetool":
                    RunMergeToolOrConflictCommand(arguments);
                    return;
                case "openrepo":    // [file]
                    RunOpenRepoCommand(args);
                    return;
                case "pull":        //  [--rebase] [--merge] [--fetch] [--quiet]
                    Pull(arguments);
                    return;
                case "push":        // [--quiet]
                    Push(arguments);
                    return;
                case "rebase":      // [--branch name]
                    RunRebaseCommand(arguments);
                    return;
                case "remotes":
                    GitUICommands.Instance.StartRemotesDialog();
                    return;
                case "revert":      // [filename]
                    Application.Run(new FormRevert(args[2]));
                    return;
                case "searchfile":
                    RunSearchFileCommand();
                    return;
                case "settings":
                    GitUICommands.Instance.StartSettingsDialog();
                    return;
                case "stash":
                    GitUICommands.Instance.StartStashDialog();
                    return;
                case "synchronize": // [--rebase] [--merge] [--fetch] [--quiet]
                    RunSynchronizeCommand(arguments);
                    return;
                case "tag":
                    GitUICommands.Instance.StartCreateTagDialog();
                    return;
                case "viewdiff":
                    GitUICommands.Instance.StartCompareRevisionsDialog();
                    return;
                default:
                    if (args[1].StartsWith("git://") || args[1].StartsWith("http://") || args[1].StartsWith("https://"))
                    {
                        GitUICommands.Instance.StartCloneDialog(null, args[1], true);
                        return;
                    }
                    if (args[1].StartsWith("github-windows://openRepo/"))
                    {
                        GitUICommands.Instance.StartCloneDialog(null, args[1].Replace("github-windows://openRepo/", ""), true);
                        return;
                    }
                    break;
            }
            Application.Run(new FormCommandlineHelp());
        }

        private static void RunMergeCommand(Dictionary<string, string> arguments)
        {
            string branch = null;
            if (arguments.ContainsKey("branch"))
                branch = arguments["branch"];
            GitUICommands.Instance.StartMergeBranchDialog(branch);
        }

        private static void RunSearchFileCommand()
        {
            var searchWindow = new SearchWindow<string>(FindFileMatches);
            Application.Run(searchWindow);
            Console.WriteLine(Settings.WorkingDir + searchWindow.SelectedItem);
        }

        private static void RunBrowseCommand(string[] args)
        {
            if (args.Length > 2)
            {
                if (Directory.Exists(args[2]))
                {
                    Settings.WorkingDir = args[2];
                }
            }

            GitUICommands.Instance.StartBrowseDialog(GetParameterOrEmptyStringAsDefault(args, "-filter"));
        }

        private static void RunOpenRepoCommand(string[] args)
        {
            if (args.Length > 2)
            {
                if (File.Exists(args[2]))
                {
                    string path = File.ReadAllText(args[2]).Trim().Split(new char[] {'\n'}, 1).FirstOrDefault();
                    if (Directory.Exists(path))
                    {
                        Settings.WorkingDir = path;
                    }
                }
            }

            GitUICommands.Instance.StartBrowseDialog(GetParameterOrEmptyStringAsDefault(args, "-filter"));
        }

        private static void RunSynchronizeCommand(Dictionary<string, string> arguments)
        {
            Commit(arguments);
            Pull(arguments);
            Push(arguments);
        }

        private static void RunRebaseCommand(Dictionary<string, string> arguments)
        {
            string branch = null;
            if (arguments.ContainsKey("branch"))
                branch = arguments["branch"];
            GitUICommands.Instance.StartRebaseDialog(branch);
        }

        private static void RunFileEditorCommand(string[] args)
        {
            using (var formEditor = new FormEditor(args[2]))
            {
                if (formEditor.ShowDialog() == DialogResult.Cancel)
                    System.Environment.ExitCode = -1;
            }
        }

        private static void RunFileHistoryCommand(string[] args)
        {
            //Remove working dir from filename. This is to prevent filenames that are too
            //long while there is room left when the workingdir was not in the path.
            string fileHistoryFileName = args[2].Replace(Settings.WorkingDir, "").Replace('\\', '/');

            GitUICommands.Instance.StartFileHistoryDialog(fileHistoryFileName);
        }

        private static void RunCloneCommand(string[] args)
        {
            if (args.Length > 2)
                GitUICommands.Instance.StartCloneDialog(args[2]);
            else
                GitUICommands.Instance.StartCloneDialog();
        }

        private static void RunInitCommand(string[] args)
        {
            if (args.Length > 2)
                GitUICommands.Instance.StartInitializeDialog(args[2]);
            else
                GitUICommands.Instance.StartInitializeDialog();
        }

        private static void RunBlameCommand(string[] args)
        {
            // Remove working dir from filename. This is to prevent filenames that are too
            // long while there is room left when the workingdir was not in the path.
            string filenameFromBlame = args[2].Replace(Settings.WorkingDir, "").Replace('\\', '/');
            GitUICommands.Instance.StartBlameDialog(filenameFromBlame);
        }

        private static void RunMergeToolOrConflictCommand(Dictionary<string, string> arguments)
        {
            if (!arguments.ContainsKey("quiet") || Settings.Module.InTheMiddleOfConflictedMerge())
                GitUICommands.Instance.StartResolveConflictsDialog();
        }

        private static Dictionary<string, string> InitializeArguments(string[] args)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();

            for (int i = 2; i < args.Length; i++)
            {
                if (args[i].StartsWith("--") && i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    arguments.Add(args[i].TrimStart('-'), args[++i]);
                else if (args[i].StartsWith("--"))
                    arguments.Add(args[i].TrimStart('-'), null);
            }
            return arguments;
        }

        private static IList<string> FindFileMatches(string name)
        {
            var candidates = Settings.Module.GetFullTree("HEAD");

            string nameAsLower = name.ToLower();

            return candidates.Where(fileName => fileName.ToLower().Contains(nameAsLower)).ToList();
        }

        private static void Commit(Dictionary<string, string> arguments)
        {
            GitUICommands.Instance.StartCommitDialog(arguments.ContainsKey("quiet"));
        }

        private static void Push(Dictionary<string, string> arguments)
        {
            GitUICommands.Instance.StartPushDialog(arguments.ContainsKey("quiet"));
        }

        private static void Pull(Dictionary<string, string> arguments)
        {
            UpdateSettingsBasedOnArguments(arguments);
            GitUICommands.Instance.StartPullDialog(arguments.ContainsKey("quiet"));
        }

        private static void UpdateSettingsBasedOnArguments(Dictionary<string, string> arguments)
        {
            if (arguments.ContainsKey("merge"))
                Settings.PullMerge = Settings.PullAction.Merge;
            if (arguments.ContainsKey("rebase"))
                Settings.PullMerge = Settings.PullAction.Rebase;
            if (arguments.ContainsKey("fetch"))
                Settings.PullMerge = Settings.PullAction.Fetch;
            if (arguments.ContainsKey("autostash"))
                Settings.AutoStash = true;
        }

        private static string GetParameterOrEmptyStringAsDefault(string[] args, string paramName)
        {
            for ( int i = 2; i < args.Length; i++ )
            {
                string arg = args[i];
                if (arg.StartsWith(paramName + "="))
                {
                    return arg.Replace(paramName + "=", "");
                }
            }

            return string.Empty;
        }
    }
}