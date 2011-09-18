using System;
using System.IO;
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
                //if (Settings.ValidWorkingDir())
                //    Repositories.RepositoryHistory.AddMostRecentRepository(Settings.WorkingDir);
            }

            ApplicationLoader.Load();

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
            Dictionary<string, string> arguments = new Dictionary<string, string>();

            for (int i = 2; i < args.Length; i++)
            {
                if (args[i].StartsWith("--") && i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    arguments.Add(args[i].TrimStart('-'), args[++i]);
                else
                    if (args[i].StartsWith("--"))
                        arguments.Add(args[i].TrimStart('-'), null);
            }

            if (args.Length > 1)
            {
                switch (args[1])
                {
                    case "mergetool":
                    case "mergeconflicts":
                        if (!arguments.ContainsKey("quiet") || GitCommandHelpers.InTheMiddleOfConflictedMerge())
                            GitUICommands.Instance.StartResolveConflictsDialog();
                        
                        return;
                    case "gitbash":
                        GitCommandHelpers.RunBash();
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
                    case "cleanup":
                        new FormCleanupRepository().ShowDialog();
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
                        Commit(arguments);
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
                                if (formEditor.ShowDialog() == DialogResult.Cancel)
                                    System.Environment.ExitCode = -1;
                            }
                        }
                        else
                            MessageBox.Show("Cannot open file editor, there is no file selected.", "File editor");
                        return;
                    case "formatpatch":
                        GitUICommands.Instance.StartFormatPatchDialog();
                        return;
                    case "pull":
                        Pull(arguments);
                        return;
                    case "push":
                        Push(arguments);
                        return;
                    case "settings":
                        GitUICommands.Instance.StartSettingsDialog();
                        return;
                    case "viewdiff":
                        GitUICommands.Instance.StartCompareRevisionsDialog();
                        return;
                    case "rebase":
                        {
                            string branch = null;
                            if (arguments.ContainsKey("branch"))
                                branch = arguments["branch"];
                            GitUICommands.Instance.StartRebaseDialog(branch);
                            return;
                        }
                    case "merge":
                        {
                            string branch = null;
                            if (arguments.ContainsKey("branch"))
                                branch = arguments["branch"];
                            GitUICommands.Instance.StartMergeBranchDialog(branch);
                            return;
                        }
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
                    case "synchronize":
                        Commit(arguments);
                        Pull(arguments);
                        Push(arguments);
                        return;
                    case "openrepo":
                        if (args.Length > 2)
                        {
                            if (File.Exists(args[2]))
                            {
                                string path = File.ReadAllText(args[2]);
                                if (Directory.Exists(path))
                                {
                                    Settings.WorkingDir = path;
                                }
                            }
                        }

                        GitUICommands.Instance.StartBrowseDialog();
                        return;
                    default:
                        Application.Run(new FormCommandlineHelp());
                        return;
                }
            }
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
            if (arguments.ContainsKey("merge"))
                Settings.PullMerge = "merge";
            if (arguments.ContainsKey("rebase"))
                Settings.PullMerge = "rebase";
            if (arguments.ContainsKey("fetch"))
                Settings.PullMerge = "fetch";
            if (arguments.ContainsKey("autostash"))
                Settings.AutoStash = true;
            GitUICommands.Instance.StartPullDialog(arguments.ContainsKey("quiet"));
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