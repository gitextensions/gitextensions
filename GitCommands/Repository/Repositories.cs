using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GitCommands.Repository
{
    public static class Repositories
    {
        private static Task<RepositoryHistory> _repositoryHistory;
        private static RepositoryHistory _remoteRepositoryHistory;
        private static BindingList<RepositoryCategory> _repositoryCategories;

        public static Task<RepositoryHistory> LoadRepositoryHistoryAsync()
        {
            if (_repositoryHistory != null)
                return _repositoryHistory;
            _repositoryHistory = Task.Factory.StartNew(() => LoadRepositoryHistory());
            return _repositoryHistory;
        }

        private static RepositoryHistory LoadRepositoryHistory()
        {
            var repositoryHistory = new RepositoryHistory();
            object setting = Settings.GetString("history", null);
            if (setting == null)
            {
                repositoryHistory = new RepositoryHistory();
                return repositoryHistory;
            }

            repositoryHistory = DeserializeHistoryFromXml(setting.ToString());
            if (repositoryHistory != null)
            {
                AssignRepositoryHistoryFromCategories(repositoryHistory, null);

                // migration from old version (move URL history to _remoteRepositoryHistory)
                if (Settings.GetString("history remote", null) == null)
                {
                    _remoteRepositoryHistory = new RepositoryHistory();
                    foreach (Repository repo in repositoryHistory.Repositories)
                    {
                        if (repo.IsRemote)
                        {
                            repo.Path = repo.Path.Replace('\\', '/');
                            _remoteRepositoryHistory.AddRepository(repo);
                        }
                    }
                    foreach (Repository repo in _remoteRepositoryHistory.Repositories)
                    {
                        repositoryHistory.RemoveRepository(repo);
                    }
                }
            }

            return repositoryHistory ?? new RepositoryHistory();
        }

        public static RepositoryHistory RepositoryHistory
        {
            get
            {
                if (_repositoryHistory == null)
                    LoadRepositoryHistoryAsync();
                return _repositoryHistory.Result;
            }
        }

        public static RepositoryHistory RemoteRepositoryHistory
        {
            get
            {
                if (_repositoryHistory != null && _repositoryHistory.Status == TaskStatus.Running)
                    _repositoryHistory.Wait();
                if (_remoteRepositoryHistory == null)
                {
                    object setting = Settings.GetString("history remote", null);
                    if (setting != null)
                    {
                        _remoteRepositoryHistory = DeserializeHistoryFromXml(setting.ToString());
                    }
                }

                return _remoteRepositoryHistory ?? (_remoteRepositoryHistory = new RepositoryHistory());
            }
            private set
            {
                _remoteRepositoryHistory = value;
            }
        }

        private static void AssignRepositoryHistoryFromCategories(RepositoryHistory repositoryHistory, string path)
        {
            foreach (Repository repo in repositoryHistory.Repositories)
            {
                if (path == null || path.Equals(repo.Path, StringComparison.CurrentCultureIgnoreCase))
                {
                    Repository catRepo = FindFirstCategoryRepository(repo.Path);
                    if (catRepo != null)
                        repo.Assign(catRepo);
                }
            }        
        }

        private static void AssignRepositoryHistoryFromCategories(string path)
        {
            AssignRepositoryHistoryFromCategories(RepositoryHistory, path);
        }

        private static Repository FindFirstCategoryRepository(string path)
        {
            foreach (Repository repo in RepositoryCategories.SelectMany(category => category.Repositories))
            {
                if (repo.Path != null && repo.Path.Equals(path, StringComparison.CurrentCultureIgnoreCase))
                    return repo;
            }
            return null;        
        }

        public static BindingList<RepositoryCategory> RepositoryCategories
        {
            get
            {
                if (_repositoryCategories == null)
                {
                    object setting = Settings.GetString("repositories", null);
                    if (setting != null)
                    {
                        _repositoryCategories = DeserializeRepositories(setting.ToString());
                    }
                }

                return _repositoryCategories ?? (_repositoryCategories = new BindingList<RepositoryCategory>());
            }
            private set
            {
                _repositoryCategories = value;
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
            try
            {
                var serializer = new XmlSerializer(typeof(BindingList<RepositoryCategory>));
                StringReader stringReader = null;
                try
                {
                    stringReader = new StringReader(xml);
                    using (var xmlReader = new XmlTextReader(stringReader))
                    {
                        stringReader = null;
                        repositories = serializer.Deserialize(xmlReader) as BindingList<RepositoryCategory>;
                        if (repositories != null)
                        {
                            foreach (var repositoryCategory in repositories)
                            {
                                repositoryCategory.SetIcon();
                            }
                        }
                    }
                }
                finally
                {
                    if (stringReader != null)
                        stringReader.Dispose();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            return repositories;
        }

        private static string SerializeHistoryIntoXml(RepositoryHistory history)
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

        private static RepositoryHistory DeserializeHistoryFromXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return null;

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
                        var obj = serializer.Deserialize(xmlReader) as RepositoryHistory;
                        if (obj != null)
                        {
                            history = obj;
                            history.SetIcon();
                        }
                    }
                }
                finally
                {
                    if (stringReader != null)
                        stringReader.Dispose();
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
                Settings.SetString("history", Repositories.SerializeHistoryIntoXml(_repositoryHistory.Result));
            if (_remoteRepositoryHistory != null)
                Settings.SetString("history remote", Repositories.SerializeHistoryIntoXml(_remoteRepositoryHistory));
            if (_repositoryCategories != null)
                Settings.SetString("repositories", SerializeRepositories(_repositoryCategories));
        }

        public static void AddCategory(string title)
        {
            RepositoryCategories.Add(new RepositoryCategory { Description = title });
        }

        public static void AddMostRecentRepository(string repo)
        {
            if (Repository.PathIsUrl(repo))
            {
                RemoteRepositoryHistory.AddMostRecentRepository(repo);
            }
            else
            {
                RepositoryHistory.AddMostRecentRepository(repo);
                AssignRepositoryHistoryFromCategories(repo);
            }
        }
    }
}