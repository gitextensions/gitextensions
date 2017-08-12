using GitCommands;
using System;
using System.IO;
using System.Collections.Concurrent;

namespace GitExtensions
{

    public class DIObjectInstance<T> where T : class, new()
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
            lock (lockObj)
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
            if (fakeInstances.TryGetValue(typeof(T), out fakeObj))
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

    public class GitModuleGateway
    {
        private static DIObjectInstance<GitModuleGateway> _diInst = new DIObjectInstance<GitModuleGateway>();
        public static GitModuleGateway Inst => _diInst.Inst;

        public virtual string FindGitWorkingDir(string dirPath)
        {
            return GitModule.FindGitWorkingDir(dirPath);
        }

        public virtual bool IsValidGitWorkingDir(string dirPath)
        {
            return GitModule.IsValidGitWorkingDir(dirPath);
        }
    }

}
