using GitCommands;
using System;
using System.IO;
using System.Collections.Concurrent;

namespace GitExtensions
{

    internal class DIObjectInstance<T> where T : class, new()
    {
        private static object lockObj = new object();
        private T _Inst;
        public T Inst
        {
            get
            {
                if (_Inst == null)
                {
                    lock (lockObj)
                    {
                        if (_Inst == null)
                        {
                            _Inst = StaticDI.CreateInstance<T>(ClearInst);
                        }
                    }
                }

                return _Inst;
            }
        }

        private void ClearInst()
        {
            lock(lockObj)
            {
                _Inst = null;
            }            
        }
    }

    public interface IInstanceFactory
    {
        T CreateInstance<T>() where T : class, new();
    }

    public class NewInstanceFactory : IInstanceFactory
    {
        public T CreateInstance<T>() where T : class, new()
        {
            return new T();
        }
    }

    public class StaticDI
    {
        private static ConcurrentBag<Action> clearActions = new ConcurrentBag<Action>();
        private static ConcurrentDictionary<Type, object> fakeInstances = new ConcurrentDictionary<Type, object>();

        public static T CreateInstance<T>(Action clearAction) where T : class, new()
        {
            clearActions.Add(clearAction);
            object fakeObj;
            if(fakeInstances.TryGetValue(typeof(T), out fakeObj))
            {
                return (T)fakeObj;
            }

            return InstanceFactory.CreateInstance<T>();
        }

        public static IInstanceFactory InstanceFactory = new NewInstanceFactory();
        
        public static void ClearInstances()
        {
            Action clearAction;
            while (clearActions.TryTake(out clearAction))
            {
                clearAction();
            }
        }

        public static void RegisterFakeInstance<T>(T instacne)
        {
            fakeInstances[typeof(T)] = instacne;
        }

    }

    public class DirectoryGateway
    {
        private static DIObjectInstance<DirectoryGateway> _diInst = new DIObjectInstance<DirectoryGateway>();
        public static DirectoryGateway Inst => _diInst.Inst;

        public virtual bool DirectoryExists(string dirPath)
        {
            return Directory.Exists(dirPath);
        }

        public virtual string CurrentDirectory => Directory.GetCurrentDirectory();
    }


    public class WorkingPathProvider
    {
        public class Exterior
        {
            //ad hoc variables (without a dedicated Gateway)
            public bool StartWithRecentWorkingDir = AppSettings.StartWithRecentWorkingDir;
            //There should be a global instance of AppSettings that could be mocked.
            public string RecentWorkingDir = AppSettings.RecentWorkingDir;
            //the same happens here
            public Func<string, string> FindGitWorkingDir = (dirPath) => GitModule.FindGitWorkingDir(dirPath);
            public Func<string, bool> IsValidGitWorkingDir = (dirPath) => GitModule.IsValidGitWorkingDir(dirPath);
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
                //Get DirectoryGateway from Ext.Directory
                string findWorkingDir = Ext.FindGitWorkingDir(DirectoryGateway.Inst.CurrentDirectory);
                if (Ext.IsValidGitWorkingDir(findWorkingDir))
                    workingDir = findWorkingDir;
            }

            return workingDir;
        }


    }

}
