using System;
using System.Collections.Generic;

using System.Text;
using System.IO;

namespace GitCommands
{
    public class Settings
    {
        public static string GitDir 
        {
            get
            {
                return "";
                //return @"c:\msysgit\bin\";
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
            return Directory.Exists(WorkingDir + "\\" + ".git");
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
    }
}
