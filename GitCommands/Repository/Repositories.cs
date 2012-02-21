using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace GitCommands.Repository
{
    public static class Repositories
    {
        private static RepositoryHistory _repositoryHistory;
        private static BindingList<RepositoryCategory> _repositoryCategories;

        public static RepositoryHistory RepositoryHistory
        {
            get
            {
                if (_repositoryHistory == null)
                {
                    object setting = Settings.GetValue<string>("history", null);
                    if (setting != null)
                    {
                        _repositoryHistory = DeserializeHistoryFromXml(setting.ToString());
                        if (_repositoryHistory != null)
                            AssignRepositoryHistoryFromCategories(null);
                    }
                }

                return _repositoryHistory ?? (_repositoryHistory = new RepositoryHistory());
            }
            private set
            {
                _repositoryHistory = value;
            }

        }

        private static void AssignRepositoryHistoryFromCategories(string path)
        {
            foreach (Repository repo in RepositoryHistory.Repositories)
            {
                if (path == null || path.Equals(repo.Path, StringComparison.CurrentCultureIgnoreCase))
                {
                    Repository catRepo = FindFirstCategoryRepository(repo.Path);
                    if (catRepo != null)
                        repo.Assign(catRepo);
                }
            }        
        }

        private static Repository FindFirstCategoryRepository(string path)
        {
            foreach (RepositoryCategory category in Repositories.RepositoryCategories)
            {
                foreach (Repository repo in category.Repositories)
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
                    object setting = Settings.GetValue<string>("repositories", null);
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
                using (var stringReader = new StringReader(xml))
                using (var xmlReader = new XmlTextReader(stringReader))
                {
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
                using (var stringReader = new StringReader(xml))
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    var obj = serializer.Deserialize(xmlReader) as RepositoryHistory;
                    if (obj != null)
                    {
                        history = obj;
                        history.SetIcon();
                    }
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
                Settings.SetValue("history", Repositories.SerializeHistoryIntoXml(_repositoryHistory));
            if (_repositoryCategories != null)
                Settings.SetValue("repositories", SerializeRepositories(_repositoryCategories));
        }

        public static void AddCategory(string title)
        {
            RepositoryCategories.Add(new RepositoryCategory { Description = title });
        }

        public static void AddMostRecentRepository(string repo)
        {
            RepositoryHistory.AddMostRecentRepository(repo);
            AssignRepositoryHistoryFromCategories(repo);
        }
    }
}