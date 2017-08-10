using GitCommands;
using System;
using System.IO;

namespace GitExtensions
{
    public class WorkingPathProvider
    {
        public class Exterior
        {
            public Func<string, bool> DirectoryExists = (dirPath) => Directory.Exists(dirPath);
            public Func<string, string> FindGitWorkingDir = (dirPath) => GitModule.FindGitWorkingDir(dirPath);
            public bool StartWithRecentWorkingDir = AppSettings.StartWithRecentWorkingDir;
            public string RecentWorkingDir = AppSettings.RecentWorkingDir;
            public Func<string, bool> IsValidGitWorkingDir = (dirPath) => GitModule.IsValidGitWorkingDir(dirPath);
            public string CurrentDirectory = Directory.GetCurrentDirectory();
        }

        private Exterior Ext;

        public WorkingPathProvider()
            : this(new Exterior())
        { }

        public WorkingPathProvider(Exterior anExt)
        {
            Ext = anExt;
        }

        public string GetWorkingDir(string[] args)
        {
            string workingDir = string.Empty;
            if (args.Length >= 3)
            {
                //there is bug in .net
                //while parsing command line arguments, it unescapes " incorectly
                //https://github.com/gitextensions/gitextensions/issues/3489
                string dirArg = args[2].TrimEnd('"');
                if (Ext.DirectoryExists(dirArg))
                    workingDir = Ext.FindGitWorkingDir(dirArg);
                else
                {
                    workingDir = Path.GetDirectoryName(dirArg);
                    workingDir = Ext.FindGitWorkingDir(workingDir);
                }

                //Do not add this working directory to the recent repositories. It is a nice feature, but it
                //also increases the startup time
                //if (Module.ValidWorkingDir())
                //    Repositories.RepositoryHistory.AddMostRecentRepository(Module.WorkingDir);
            }

            if (args.Length <= 1 && string.IsNullOrEmpty(workingDir) && Ext.StartWithRecentWorkingDir)
            {
                if (Ext.IsValidGitWorkingDir(Ext.RecentWorkingDir))
                    workingDir = Ext.RecentWorkingDir;
            }

            if (string.IsNullOrEmpty(workingDir))
            {
                string findWorkingDir = Ext.FindGitWorkingDir(Ext.CurrentDirectory);
                if (Ext.IsValidGitWorkingDir(findWorkingDir))
                    workingDir = findWorkingDir;
            }

            return workingDir;
        }


    }
}
