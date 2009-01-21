using System;
using System.Collections.Generic;

using System.Text;
using System.IO;
using System.Windows.Forms;

namespace GitCommands
{
    public class Settings
    {
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

        private static string workingdir;
        public static string WorkingDir
        {
            get
            {
                return workingdir;
            }
            set
            {
                workingdir = GitCommands.FindGitWorkingDir(value);
            }
        }

        public static string GitLog { get; set; }

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
    }
}
