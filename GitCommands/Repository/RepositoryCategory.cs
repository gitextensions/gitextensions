using System;
using System.ComponentModel;
using System.Xml;

namespace GitCommands.Repository
{
    public class RepositoryCategory
    {
        private BindingList<Repository> _repositories;

        public RepositoryCategory()
        {
            Repositories.AllowNew = true;
            Repositories.RaiseListChangedEvents = false;
        }


        public RepositoryCategory(string description)
        {
            Description = description;
            Repositories.AllowNew = true;
            Repositories.RaiseListChangedEvents = false;
        }

        public BindingList<Repository> Repositories
        {
            get { return _repositories ?? (_repositories = new BindingList<Repository>()); }
            set { _repositories = value; }
        }

        public string Description { get; set; }

        public string RssFeedUrl { get; set; }

        public RepositoryCategoryType CategoryType { get; set; }

        public virtual void SetIcon()
        {
            foreach (var recentRepository in Repositories)
            {
                if (CategoryType == RepositoryCategoryType.RssFeed)
                    recentRepository.RepositoryType = RepositoryType.RssFeed;
            }
        }

        public void DownloadRssFeed()
        {
            try
            {
                // Create a new XmlTextReader from the specified URL (RSS feed)
                var rssReader = new XmlTextReader(RssFeedUrl);
                var rssDoc = new XmlDocument();

                // Load the XML content into a XmlDocument
                rssDoc.Load(rssReader);

                //Clear old entries
                Repositories.Clear();

                // Loop for the <rss> or (atom) <feed> tag
                for (var r = 0; r < rssDoc.ChildNodes.Count; r++)
                {
                    // If it is the (atom) feed tag
                    if (rssDoc.ChildNodes[r].Name == "feed")
                    {
                        HandleFeedTag(rssDoc, r);
                    }

                    // If it is the rss tag
                    if (rssDoc.ChildNodes[r].Name == "rss")
                    {
                        HandleRssTag(rssDoc, r);
                    }
                }
            }
            catch (Exception ex)
            {
                Repositories.Clear();

                var repository = new Repository
                                     {
                                         Title = "Error loading rssfeed from :" + RssFeedUrl,
                                         Description = ex.Message,
                                         Path = RssFeedUrl,
                                         RepositoryType = RepositoryType.RssFeed
                                     };
                Repositories.Add(repository);
            }
        }

        private void HandleRssTag(XmlNode rssDoc, int r)
        {
            // <rss> tag found
            var nodeRss = rssDoc.ChildNodes[r];

            // Loop for the <channel> tag
            for (var c = 0; c < nodeRss.ChildNodes.Count; c++)
            {
                // If it is the channel tag
                if (nodeRss.ChildNodes[c].Name != "channel")
                    continue;
                // <channel> tag found
                var nodeChannel = nodeRss.ChildNodes[c];

                // Set the labels with information from inside the nodes
                /*string title = nodeChannel["title"].InnerText;
                                string link = nodeChannel["link"].InnerText;
                                string description = nodeChannel["description"].InnerText;*/

                //loop through all items
                for (var i = 0; i < nodeChannel.ChildNodes.Count; i++)
                {
                    // If it is the item tag, then it has children tags which we will add as items to the ListView
                    if (nodeChannel.ChildNodes[i].Name != "item")
                        continue;
                    var nodeItem = nodeChannel.ChildNodes[i];

                    // Create a new row in the ListView containing information from inside the nodes
                    var repository = new Repository();
                    var title = nodeItem["title"];
                    if (title != null)
                        repository.Title = title.InnerText.Trim();
                    var description = nodeItem["description"];
                    if (description != null)
                        repository.Description = description.InnerText.Trim();
                    var link = nodeItem["link"];
                    if (link != null)
                        repository.Path = link.InnerText.Trim();
                    repository.RepositoryType = RepositoryType.RssFeed;
                    Repositories.Add(repository);
                }
            }
        }

        private void HandleFeedTag(XmlNode rssDoc, int r)
        {
            // <feed> tag found
            var nodeFeed = rssDoc.ChildNodes[r];

            //loop through all entries
            for (var i = 0; i < nodeFeed.ChildNodes.Count; i++)
            {
                var nodeItem = nodeFeed.ChildNodes[i];

                if (nodeItem.Name != "entry")
                    continue;
                // Create a new row in the ListView containing information from inside the nodes
                var repository = new Repository();
                var title = nodeItem["title"];
                if (title != null)
                    repository.Title = title.InnerText.Trim();
                //repository.Description = nodeItem["content"].InnerText.Trim();
                var link = nodeItem["link"];
                if (link != null)
                    repository.Path = link.Attributes["href"].Value;
                repository.RepositoryType = RepositoryType.RssFeed;
                Repositories.Add(repository);
            }
        }

        public void RemoveRepository(Repository repository)
        {
            Repositories.Remove(repository);
        }


        public void AddRepository(Repository repo)
        {
            Repositories.Add(repo);
        }
    }
}