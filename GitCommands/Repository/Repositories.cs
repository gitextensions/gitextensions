using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace GitCommands.Repository
{
    public static class Repositories
    {
        private static RepositoryHistory _repositoryHistory;
        private static BindingList<RepositoryCategory> _repositoryCategories;

        //This property is used to determine if repository history needs to be saved
        public static bool RepositoryHistoryLoaded
        {
            get
            {
                return _repositoryHistory != null;
            }
        }

        public static RepositoryHistory RepositoryHistory
        {
            get
            {
                if (_repositoryHistory == null)
                {
                    object setting = Settings.GetValue<string>("history", null);
                    if (setting != null)
                    {
                        DeserializeHistoryFromXml(setting.ToString());
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

        public static void AssignRepositoryHistoryFromCategories(string path)
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

        public static Repository FindFirstCategoryRepository(string path)
        {
            foreach (RepositoryCategory category in Repositories.RepositoryCategories)
            {
                foreach (Repository repo in category.Repositories)
                    if (repo.Path.Equals(path, StringComparison.CurrentCultureIgnoreCase))
                        return repo;
            }
            return null;        
        }

        //This property is used to determine if repository history needs to be saved
        public static bool RepositoryCategoriesLoaded
        {
            get
            {
                return _repositoryCategories != null;
            }
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
                        DeserializeRepositories(setting.ToString());
                    }

                }

                return _repositoryCategories ?? (_repositoryCategories = new BindingList<RepositoryCategory>());
            }
            private set
            {
                _repositoryCategories = value;
            }
        }

        public static string SerializeRepositories()
        {
            try
            {
                var sw = new StringWriter();
                var serializer = new XmlSerializer(typeof(BindingList<RepositoryCategory>));
                serializer.Serialize(sw, RepositoryCategories);
                return sw.ToString();
            }
            catch
            {
                return null;
            }
        }

        public static void DeserializeRepositories(string xml)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(BindingList<RepositoryCategory>));
                using (var stringReader = new StringReader(xml))
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    var obj = serializer.Deserialize(xmlReader) as BindingList<RepositoryCategory>;
                    if (obj != null)
                    {
                        RepositoryCategories = obj;

                        foreach (var repositoryCategory in RepositoryCategories)
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
        }

        public static string SerializeHistoryIntoXml()
        {
            try
            {
                var sw = new StringWriter();
                var serializer = new XmlSerializer(typeof(RepositoryHistory));
                serializer.Serialize(sw, RepositoryHistory);
                return sw.ToString();
            }
            catch
            {
                return null;
            }
        }

        public static void DeserializeHistoryFromXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return;

            try
            {
                var serializer = new XmlSerializer(typeof(RepositoryHistory));
                using (var stringReader = new StringReader(xml))
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    var obj = serializer.Deserialize(xmlReader) as RepositoryHistory;
                    if (obj != null)
                    {
                        RepositoryHistory = obj;
                        RepositoryHistory.SetIcon();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
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