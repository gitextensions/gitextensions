using System;
using System.Collections.Generic;

using System.Text;
using System.IO;
using System.Windows.Forms;

namespace GitCommands
{
    public class Settings
    {
        private static string pullMerge = "merge";
        public static string PullMerge
        {
            get
            {
                return pullMerge;
            }
            set
            {
                pullMerge = value;
            }
        }

        private static string smtp = "";
        public static string Smtp
        {
            get
            {
                return smtp;
            }
            set
            {
                smtp = value;
            }
        }

        private static bool autoStash = false;
        public static bool AutoStash
        {
            get
            {
                return autoStash;
            }
            set
            {
                autoStash = value;
            }
        }

        private static bool orderRevisionByDate = true;
        public static bool OrderRevisionByDate
        {
            get
            {
                return orderRevisionByDate;
            }
            set
            {
                orderRevisionByDate = value;
            }
        }

        private static string dictionary = "en-US";
        public static string Dictionary
        {
            get
            {
                return dictionary;
            }
            set
            {
                dictionary = value;
            }
        }

        private static bool showGitCommandLine = false;
        public static bool ShowGitCommandLine
        {
            get
            {
                return showGitCommandLine;
            }
            set
            {
                showGitCommandLine = value;
            }
        }

        private static bool relativeDate = true;
        public static bool RelativeDate
        {
            get
            {
                return relativeDate;
            }
            set
            {
                relativeDate = value;
            }
        }

        private static bool useFastChecks = true;
        public static bool UseFastChecks
        {
            get
            {
                return useFastChecks;
            }
            set
            {
                useFastChecks = value;
            }
        }

        private static bool showRevisionGraph = true;
        public static bool ShowRevisionGraph
        {
            get
            {
                return showRevisionGraph;
            }
            set
            {
                showRevisionGraph = value;
            }
        }

        private static bool closeProcessDialog = false;
        public static bool CloseProcessDialog
        {
            get
            {
                return closeProcessDialog;
            }
            set
            {
                closeProcessDialog = value;
            }
        }

        public static string GetDictionaryDir()
        {
            if (Application.UserAppDataRegistry.GetValue("InstallDir") != null)
                return GetInstallDir() + "\\Dictionaries\\";

            return "";
        }


        public static string GetInstallDir()
        {
            if (Application.UserAppDataRegistry.GetValue("InstallDir") != null)
                return Application.UserAppDataRegistry.GetValue("InstallDir").ToString();

            return "";
        }

        public static void SetInstallDir(string dir)
        {
            Application.UserAppDataRegistry.SetValue("InstallDir", dir);
        }


        private static bool showAllBranches = true;
        public static bool ShowAllBranches
        {
            get
            {
                return showAllBranches;
            }
            set
            {
                showAllBranches = value;
            }
        }


        private static string gitDir = "";
        public static string GitDir 
        {
            get
            {
                return gitDir;
}
            set
            {
                gitDir = value;
                if (gitDir.Length > 0 && gitDir[gitDir.Length - 1] != '\\')
                    gitDir += "\\";
                gitDir = gitDir.Replace("\\\\", "\\");
                gitDir = gitDir.Replace("//", "/");

            }
        }

        private static string gitBinDir = "";
        public static string GitBinDir
        {
            get
            {
                return gitBinDir;
            }
            set
            {
                gitBinDir = value;
                if (gitBinDir.Length > 0 && gitBinDir[gitBinDir.Length - 1] != '\\')
                    gitBinDir += "\\";

                if (!string.IsNullOrEmpty(gitBinDir))
                    Environment.SetEnvironmentVariable("path", Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.Process) + ";" + gitBinDir, EnvironmentVariableTarget.Process);

            }
        }

        private static int maxCommits = 2000;
        public static int MaxCommits
        {
            get
            {
                return maxCommits;
            }
            set
            {
                maxCommits = value;
            }
        }

        public static bool ValidWorkingDir()
        {
            return ValidWorkingDir(WorkingDir);
        }

        public static bool ValidWorkingDir(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                return false;

            if (Directory.Exists(dir + "\\" + ".git"))
                return true;

            if (!dir.Contains(".git") &&
                Directory.Exists(dir + "\\" + "info") &&
                Directory.Exists(dir + "\\" + "objects") &&
                Directory.Exists(dir + "\\" + "refs")
                )
                return true;

            return false;
        }

        public static bool IsBareRepository()
        {
            if (Directory.Exists(WorkingDir + "\\" + ".git"))
                return false;

            return true;
        }

        public static string WorkingDirGitDir()
        {
            if (Directory.Exists(WorkingDir + "\\" + ".git"))
                return WorkingDir + "\\" + ".git";

            return WorkingDir;
        }


        private static string workingdir;
        public static string WorkingDir
        {
            get
            {
                return workingdir;
            }
            set
            {
                workingdir = GitCommands.FindGitWorkingDir(value.Trim());
            }
        }

        private static readonly CommandLogger _gitLog = new CommandLogger();
        public static CommandLogger GitLog
        {
            get
            {
                return _gitLog;
            }
        }

        private static string plink = "";
        public static string Plink
        {
            get
            {
                return plink;
            }
            set
            {
                plink = value;
            }
        }

        private static string puttygen = "";
        public static string Puttygen
        {
            get
            {
                return puttygen;
            }
            set
            {
                puttygen = value;
            }
        }

        private static string pageant = "";
        public static string Pageant
        {
            get
            {
                return pageant;
            }
            set
            {
                pageant = value;
            }
        }

        private static bool autoStartPageant = true;
        public static bool AutoStartPageant
        {
            get
            {
                return autoStartPageant;
            }
            set
            {
                autoStartPageant = value;
            }
        }


        public static void SaveSettings()
        {
            try
            {
                for (int n = 0; n < RepositoryHistory.MostRecentRepositories.Count; n++)
                {
                    Application.UserAppDataRegistry.SetValue("dir" + n.ToString(), RepositoryHistory.MostRecentRepositories[n]);
                }
                Application.UserAppDataRegistry.SetValue("maxcommits", Settings.MaxCommits);
                Application.UserAppDataRegistry.SetValue("gitdir", Settings.GitDir);
                Application.UserAppDataRegistry.SetValue("gitbindir", Settings.GitBinDir);
                Application.UserAppDataRegistry.SetValue("showallbranches", Settings.ShowAllBranches);
                Application.UserAppDataRegistry.SetValue("closeprocessdialog", Settings.CloseProcessDialog);
                Application.UserAppDataRegistry.SetValue("showrevisiongraph", Settings.ShowRevisionGraph);
                Application.UserAppDataRegistry.SetValue("orderrevisiongraphbydate", Settings.OrderRevisionByDate);

                Application.UserAppDataRegistry.SetValue("showgitcommandline", Settings.ShowGitCommandLine);
                Application.UserAppDataRegistry.SetValue("usefastchecks", Settings.UseFastChecks);
                Application.UserAppDataRegistry.SetValue("relativedate", Settings.RelativeDate);

                Application.UserAppDataRegistry.SetValue("gitssh", GitCommands.GetSsh());
                Application.UserAppDataRegistry.SetValue("pullmerge", Settings.PullMerge);

                Application.UserAppDataRegistry.SetValue("autostash", Settings.AutoStash);

                Application.UserAppDataRegistry.SetValue("plink", Settings.Plink);
                Application.UserAppDataRegistry.SetValue("puttygen", Settings.Puttygen);
                Application.UserAppDataRegistry.SetValue("pageant", Settings.Pageant);

                Application.UserAppDataRegistry.SetValue("smtp", Settings.Smtp);

                Application.UserAppDataRegistry.SetValue("dictionary", Settings.Dictionary);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load settigns.\n\n" + ex.Message);
            }
        }

        public static void LoadSettings()
        {
            try
            {
                if (Application.UserAppDataRegistry.GetValue("maxcommits") != null)
                {
                    int result;
                    if (int.TryParse(Application.UserAppDataRegistry.GetValue("maxcommits").ToString(), out result) == true)
                    {
                        Settings.MaxCommits = result;
                    }
                }


                if (Application.UserAppDataRegistry.GetValue("pullmerge") != null) Settings.PullMerge = Application.UserAppDataRegistry.GetValue("pullmerge").ToString();
                if (Application.UserAppDataRegistry.GetValue("gitssh") != null) GitCommands.SetSsh(Application.UserAppDataRegistry.GetValue("gitssh").ToString());
                if (Application.UserAppDataRegistry.GetValue("plink") != null) Settings.Plink = Application.UserAppDataRegistry.GetValue("plink").ToString();
                if (Application.UserAppDataRegistry.GetValue("puttygen") != null) Settings.Puttygen = Application.UserAppDataRegistry.GetValue("puttygen").ToString();
                if (Application.UserAppDataRegistry.GetValue("pageant") != null) Settings.Pageant = Application.UserAppDataRegistry.GetValue("pageant").ToString();

                if (Application.UserAppDataRegistry.GetValue("dictionary") != null) Settings.Dictionary = Application.UserAppDataRegistry.GetValue("dictionary").ToString();
                if (Application.UserAppDataRegistry.GetValue("smtp") != null) Settings.Smtp = Application.UserAppDataRegistry.GetValue("smtp").ToString();

                if (Application.UserAppDataRegistry.GetValue("autostash") != null) Settings.AutoStash = Application.UserAppDataRegistry.GetValue("autostash").ToString() == "True";

                if (Application.UserAppDataRegistry.GetValue("relativedate") != null) Settings.RelativeDate = Application.UserAppDataRegistry.GetValue("relativedate").ToString() == "True";
                if (Application.UserAppDataRegistry.GetValue("usefastchecks") != null) Settings.UseFastChecks = Application.UserAppDataRegistry.GetValue("usefastchecks").ToString() == "True";
                if (Application.UserAppDataRegistry.GetValue("showgitcommandline") != null) Settings.ShowGitCommandLine = Application.UserAppDataRegistry.GetValue("showgitcommandline").ToString() == "True";
                if (Application.UserAppDataRegistry.GetValue("showrevisiongraph") != null) Settings.ShowRevisionGraph = Application.UserAppDataRegistry.GetValue("showrevisiongraph").ToString() == "True";
                if (Application.UserAppDataRegistry.GetValue("closeprocessdialog") != null) Settings.CloseProcessDialog = Application.UserAppDataRegistry.GetValue("closeprocessdialog").ToString() == "True";
                if (Application.UserAppDataRegistry.GetValue("showallbranches") != null) Settings.ShowAllBranches = Application.UserAppDataRegistry.GetValue("showallbranches").ToString() == "True";
                if (Application.UserAppDataRegistry.GetValue("orderrevisiongraphbydate") != null) Settings.OrderRevisionByDate = Application.UserAppDataRegistry.GetValue("orderrevisiongraphbydate").ToString() == "True";
                if (Application.UserAppDataRegistry.GetValue("gitdir") != null) Settings.GitDir = Application.UserAppDataRegistry.GetValue("gitdir").ToString();
                if (Application.UserAppDataRegistry.GetValue("gitbindir") != null) Settings.GitBinDir = Application.UserAppDataRegistry.GetValue("gitbindir").ToString();
                if (Application.UserAppDataRegistry.GetValue("dir13") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir13").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir12") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir12").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir11") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir11").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir10") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir10").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir9") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir9").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir8") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir8").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir7") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir7").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir6") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir6").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir5") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir5").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir4") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir4").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir3") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir3").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir2") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir2").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir1") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir1").ToString());
                if (Application.UserAppDataRegistry.GetValue("dir0") != null) RepositoryHistory.AddMostRecentRepository(Application.UserAppDataRegistry.GetValue("dir0").ToString());


            }
            catch
            {
            }
        }
    }
}
