using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Xml;

namespace GitCommands
{
    public enum RepositoryCategoryType
    {
        Repositories,
        RssFeed
    }

    public class RepositoryCategory
    {
        public RepositoryCategory()
        {
            Repositories.AllowNew = true;
            Repositories.RaiseListChangedEvents = false;
        }


        public RepositoryCategory(string description)
        {
            this.Description = description;
            Repositories.AllowNew = true;
            Repositories.RaiseListChangedEvents = false;
        }

        public virtual void SetIcon()
        {
            foreach (Repository recentRepository in Repositories)
            {
                if (CategoryType == RepositoryCategoryType.RssFeed)
                    recentRepository.RepositoryType = RepositoryType.RssFeed;
            }
        }


        private BindingList<Repository> repositories;
        public BindingList<Repository> Repositories
        {
            get
            {
                if (repositories == null)
                    repositories = new BindingList<Repository>();
                return repositories;
            }
            set
            {
                repositories = value;
            }
        }

        public string Description { get; set; }

        public string RssFeedUrl { get; set; }

        public RepositoryCategoryType CategoryType { get; set; }

        public void DownloadRssFeed()
        {
            try
            {
                // Create a new XmlTextReader from the specified URL (RSS feed)
                XmlTextReader rssReader = new XmlTextReader(RssFeedUrl);
                XmlDocument rssDoc = new XmlDocument();

                // Load the XML content into a XmlDocument
                rssDoc.Load(rssReader);

                //Clear old entries
                Repositories.Clear();

                // Loop for the <rss> or (atom) <feed> tag
                for (int r = 0; r < rssDoc.ChildNodes.Count; r++)
                {
                    // If it is the (atom) feed tag
                    if (rssDoc.ChildNodes[r].Name == "feed")
                    {
                        
                        handleFeedTag(rssDoc, r);
                    }

                    // If it is the rss tag
                    if (rssDoc.ChildNodes[r].Name == "rss")
                    {                        
                        handleRSSTag(rssDoc, r);
                    }
                }
            }
            catch (Exception ex)
            {
                Repositories.Clear();

                Repository repository = new Repository();
                repository.Title = "Error loading rssfeed from :" + RssFeedUrl;
                repository.Description = ex.Message;
                repository.Path = RssFeedUrl;
                repository.RepositoryType = RepositoryType.RssFeed;
                Repositories.Add(repository);
            }
        }

        private void handleRSSTag(XmlDocument rssDoc, int r)
        {
            // <rss> tag found
            XmlNode nodeRss = rssDoc.ChildNodes[r];

            // Loop for the <channel> tag
            for (int c = 0; c < nodeRss.ChildNodes.Count; c++)
            {
                // If it is the channel tag
                if (nodeRss.ChildNodes[c].Name != "channel")
                    continue;
                // <channel> tag found
                XmlNode nodeChannel = nodeRss.ChildNodes[c];

                // Set the labels with information from inside the nodes
                /*string title = nodeChannel["title"].InnerText;
                                string link = nodeChannel["link"].InnerText;
                                string description = nodeChannel["description"].InnerText;*/

                //loop through all items
                for (int i = 0; i < nodeChannel.ChildNodes.Count; i++)
                {
                    // If it is the item tag, then it has children tags which we will add as items to the ListView
                    if (nodeChannel.ChildNodes[i].Name != "item")
                        continue;
                    XmlNode nodeItem = nodeChannel.ChildNodes[i];

                    // Create a new row in the ListView containing information from inside the nodes
                    Repository repository = new Repository();
                    if (nodeItem["title"] != null)
                        repository.Title = nodeItem["title"].InnerText.Trim();
                    if (nodeItem["description"] != null)
                        repository.Description = nodeItem["description"].InnerText.Trim();
                    if (nodeItem["link"] != null)
                        repository.Path = nodeItem["link"].InnerText.Trim();
                    repository.RepositoryType = RepositoryType.RssFeed;
                    Repositories.Add(repository);
                }
            }
        }

        private void handleFeedTag(XmlDocument rssDoc, int r)
        {
            // <feed> tag found
            XmlNode nodeFeed = rssDoc.ChildNodes[r];

            //loop through all entries
            for (int i = 0; i < nodeFeed.ChildNodes.Count; i++)
            {
                XmlNode nodeItem = nodeFeed.ChildNodes[i];

                if (nodeItem.Name != "entry")
                    continue;
                // Create a new row in the ListView containing information from inside the nodes
                Repository repository = new Repository();
                if (nodeItem["title"] != null)
                    repository.Title = nodeItem["title"].InnerText.Trim();
                //repository.Description = nodeItem["content"].InnerText.Trim();
                if (nodeItem["link"] != null)
                    repository.Path = nodeItem["link"].Attributes["href"].Value;
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
