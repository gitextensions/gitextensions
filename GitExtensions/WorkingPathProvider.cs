using GitCommands;
using System.IO;

namespace GitExtensions
{
    public class WorkingPathProvider
    {
        public class Exterior
        {
            //ad hoc variables (without a dedicated Gateway)
            public bool StartWithRecentWorkingDir = AppSettings.StartWithRecentWorkingDir;
            //There should be a global instance of AppSettings that could be mocked.
            public string RecentWorkingDir = AppSettings.RecentWorkingDir;
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
                //Get DirectoryGateway from the global injected instance, needs to call before each test StaticDI.ClearInstances();
                if (DirectoryGateway.Inst.DirectoryExists(dirArg))
                    workingDir = GitModuleGateway.Inst.FindGitWorkingDir(dirArg);
                else
                {
                    workingDir = Path.GetDirectoryName(dirArg);
                    workingDir = GitModuleGateway.Inst.FindGitWorkingDir(workingDir);
                }

                //Do not add this working directory to the recent repositories. It is a nice feature, but it
                //also increases the startup time
                //if (Module.ValidWorkingDir())
                //    Repositories.RepositoryHistory.AddMostRecentRepository(Module.WorkingDir);
            }

            if (args.Length <= 1 && string.IsNullOrEmpty(workingDir) && Ext.StartWithRecentWorkingDir)
            {
                if (GitModuleGateway.Inst.IsValidGitWorkingDir(Ext.RecentWorkingDir))
                    workingDir = Ext.RecentWorkingDir;
            }

            if (string.IsNullOrEmpty(workingDir))
            {
                //Get DirectoryGateway from Ext.Directory
                string findWorkingDir = GitModuleGateway.Inst.FindGitWorkingDir(DirectoryGateway.Inst.CurrentDirectory);
                if (GitModuleGateway.Inst.IsValidGitWorkingDir(findWorkingDir))
                    workingDir = findWorkingDir;
            }

            return workingDir;
        }


    }

}
