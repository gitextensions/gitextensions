using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using GitUI;
using Microsoft.VisualStudio.Threading;

namespace GitCommands.Repository
{
    public static class Repositories
    {
        private static AsyncLazy<RepositoryHistory> _repositoryHistory;
        private static RepositoryHistory _remoteRepositoryHistory;

        public static Task<RepositoryHistory> LoadRepositoryHistoryAsync()
        {
            if (_repositoryHistory != null)
            {
                return _repositoryHistory.GetValueAsync();
            }

            _repositoryHistory = new AsyncLazy<RepositoryHistory>(() => Task.Run(() => LoadRepositoryHistory()), ThreadHelper.JoinableTaskFactory);
            return _repositoryHistory.GetValueAsync();
        }

        private static RepositoryHistory LoadRepositoryHistory()
        {
            int size = AppSettings.RecentRepositoriesHistorySize;
            object setting = AppSettings.GetString("history", null);
            if (setting == null)
            {
                return new RepositoryHistory(size);
            }

            RepositoryHistory repositoryHistory = DeserializeHistoryFromXml(setting.ToString());
            if (repositoryHistory == null)
            {
                return new RepositoryHistory(size);
            }

            repositoryHistory.MaxCount = size;

            // migration from old version (move URL history to _remoteRepositoryHistory)
            if (AppSettings.GetString("history remote", null) == null)
            {
                _remoteRepositoryHistory = new RepositoryHistory(size);
                foreach (Repository repo in repositoryHistory.Repositories)
                {
                    if (PathUtil.IsUrl(repo.Path))
                    {
                        repo.Path = repo.Path.ToPosixPath();
                        _remoteRepositoryHistory.AddRepository(repo);
                    }
                }

                foreach (Repository repo in _remoteRepositoryHistory.Repositories)
                {
                    repositoryHistory.RemoveRepository(repo);
                }
            }

            return repositoryHistory;
        }

        public static RepositoryHistory RepositoryHistory
        {
            get
            {
                if (_repositoryHistory?.IsValueFactoryCompleted ?? false)
                {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                    return _repositoryHistory.GetValueAsync().Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
                }

                return ThreadHelper.JoinableTaskFactory.Run(() => LoadRepositoryHistoryAsync());
            }
        }

        public static RepositoryHistory RemoteRepositoryHistory
        {
            get
            {
                if (_repositoryHistory != null && !_repositoryHistory.IsValueFactoryCompleted)
                {
                    ThreadHelper.JoinableTaskFactory.Run(() => _repositoryHistory.GetValueAsync());
                }

                int size = AppSettings.RecentRepositoriesHistorySize;
                if (_remoteRepositoryHistory == null)
                {
                    object setting = AppSettings.GetString("history remote", null);
                    if (setting != null)
                    {
                        _remoteRepositoryHistory = DeserializeHistoryFromXml(setting.ToString());
                        _remoteRepositoryHistory.MaxCount = size;
                    }
                }

                return _remoteRepositoryHistory ?? (_remoteRepositoryHistory = new RepositoryHistory(size));
            }
        }

        private static string SerializeRepositories(BindingList<RepositoryCategory> categories)
        {
            try
            {
                var sw = new StringWriter();
                var serializer = new XmlSerializer(typeof(BindingList<RepositoryCategory>));
                serializer.Serialize(sw, categories);
                return sw.ToString();
            }
            catch
            {
                return null;
            }
        }

        private static BindingList<RepositoryCategory> DeserializeRepositories(string xml)
        {
            BindingList<RepositoryCategory> repositories = null;
            StringReader stringReader = null;
            try
            {
                var serializer = new XmlSerializer(typeof(BindingList<RepositoryCategory>));
                stringReader = new StringReader(xml);
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    if (serializer.Deserialize(xmlReader) is BindingList<RepositoryCategory> repos)
                    {
                        repositories = new BindingList<RepositoryCategory>();
                        foreach (var repositoryCategory in repos)
                        {
                            repositoryCategory.SetIcon();
                            repositories.Add(repositoryCategory);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            finally
            {
                stringReader?.Dispose();
            }

            return repositories;
        }

        // TODO: make it internal to test, before refactoring it out
        internal static string SerializeHistoryIntoXml(RepositoryHistory history)
        {
            try
            {
                var sw = new StringWriter();
                var serializer = new XmlSerializer(typeof(RepositoryHistory));
                serializer.Serialize(sw, history);
                return sw.ToString();
            }
            catch
            {
                return null;
            }
        }

        // TODO: make it internal to test, before refactoring it out
        internal static RepositoryHistory DeserializeHistoryFromXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return null;
            }

            RepositoryHistory history = null;
            try
            {
                var serializer = new XmlSerializer(typeof(RepositoryHistory));
                StringReader stringReader = null;
                try
                {
                    stringReader = new StringReader(xml);
                    using (var xmlReader = new XmlTextReader(stringReader))
                    {
                        stringReader = null;
                        if (serializer.Deserialize(xmlReader) is RepositoryHistory obj)
                        {
                            history = obj;
                            history.SetIcon();
                        }
                    }
                }
                finally
                {
                    stringReader?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return history;
        }

        public static void SaveSettings()
        {
            if (_repositoryHistory != null)
            {
                AppSettings.SetString("history", SerializeHistoryIntoXml(RepositoryHistory));
            }

            if (_remoteRepositoryHistory != null)
            {
                AppSettings.SetString("history remote", SerializeHistoryIntoXml(_remoteRepositoryHistory));
            }

            // TODO: address later to provide a migration path
            //  if (_repositoryCategories != null)
            //  {
            //      AppSettings.SetString("repositories", SerializeRepositories(_repositoryCategories));
            //  }
        }

        public static void AddMostRecentRepository(string repo)
        {
            if (PathUtil.IsUrl(repo))
            {
                RemoteRepositoryHistory.AddMostRecentRepository(repo);
            }
            else
            {
                RepositoryHistory.AddMostRecentRepository(repo);
            }
        }
    }
}