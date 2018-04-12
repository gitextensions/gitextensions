using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.VisualStudio.Threading;

namespace GitCommands.Repository
{
    public static class RepositoryManager
    {
        private const string KeyRecentHistory = "history";
        private const string KeyRemoteHistory = "history remote";

        private static readonly IRepositoryStorage RepositoryStorage = new RepositoryStorage();

        public static async Task<RepositoryHistory> LoadRepositoryHistoryAsync()
        {
            await TaskScheduler.Default;

            int size = AppSettings.RecentRepositoriesHistorySize;
            var repositoryHistory = new RepositoryHistory(size);

            var history = RepositoryStorage.Load(KeyRecentHistory);
            if (history == null)
            {
                return repositoryHistory;
            }

            repositoryHistory.Repositories = new BindingList<Repository>(history.ToList());
            return repositoryHistory;
        }

        public static Task RemoveRepositoryHistoryAsync(Repository repository)
        {
            // TODO:
            return Task.CompletedTask;
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
            // if (_repositoryHistory != null)
            // {
            //     AppSettings.SetString(KeyRecentHistory, SerializeHistoryIntoXml(RepositoryHistory));
            // }

            // if (_repositoryRemoteHistory != null)
            // {
            //     AppSettings.SetString(KeyRemoteHistory, SerializeHistoryIntoXml(RemoteRepositoryHistory));
            // }

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
                ////RemoteRepositoryHistory.AddMostRecentRepository(repo);
            }
            else
            {
                ////RepositoryHistory.AddMostRecentRepository(repo);
            }
        }

        public static async Task<RepositoryHistory> LoadRepositoryRemoteHistoryAsync()
        {
            await TaskScheduler.Default;

            int size = AppSettings.RecentRepositoriesHistorySize;
            var repositoryHistory = new RepositoryHistory(size);

            var history = RepositoryStorage.Load(KeyRemoteHistory);
            if (history == null)
            {
                return repositoryHistory;
            }

            repositoryHistory.Repositories = new BindingList<Repository>(history.ToList());
            return repositoryHistory;
        }

        public static void AdjustRecentHistorySize(int recentRepositoriesHistorySize)
        {
            // TODO:
        }

        public static async Task SaveRepositoryHistoryAsync(RepositoryHistory repositoryHistory)
        {
            await TaskScheduler.Default;
            RepositoryStorage.Save(KeyRecentHistory, repositoryHistory.Repositories);
        }

        public static async Task SaveRepositoryRemoteHistoryAsync(RepositoryHistory repositoryHistory)
        {
            await TaskScheduler.Default;
            RepositoryStorage.Save(KeyRemoteHistory, repositoryHistory.Repositories);
        }
    }
}
