using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace GitCommands
{
    public class RepositoryHistory : RepositoryCategory
    {
        public RepositoryHistory()
        {
            Description = "Recent Repositories";
            //Repositories.ListChanged += new System.ComponentModel.ListChangedEventHandler(Repositories_ListChanged);
        }

        public override void Repositories_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            if (e.ListChangedType == System.ComponentModel.ListChangedType.ItemAdded)
            {
                Repositories[e.NewIndex].RepositoryType = RepositoryType.History;
            }
            OnListChanged(this, e);
        }

        public void RemoveRecentRepository(string repo)
        {
            foreach (Repository recentRepository in Repositories)
            {
                if (recentRepository.Path.Equals(repo, StringComparison.CurrentCultureIgnoreCase))
                {
                    Repositories.Remove(recentRepository);
                    break;
                }
            }
        }

        public void AddMostRecentRepository(string repo)
        {
            repo = repo.Trim();

            if (string.IsNullOrEmpty(repo))
                return;

            repo = repo.Replace('/', '\\');
            if (!repo.EndsWith("\\") && 
                !repo.StartsWith("http", StringComparison.CurrentCultureIgnoreCase) &&
                !repo.StartsWith("git", StringComparison.CurrentCultureIgnoreCase) &&
                !repo.StartsWith("ssh", StringComparison.CurrentCultureIgnoreCase))
                repo += "\\";

            foreach (Repository recentRepository in Repositories)
            {
                if (recentRepository.Path.Equals(repo, StringComparison.CurrentCultureIgnoreCase))
                {
                    Repositories.Remove(recentRepository);
                    break;
                }
            }

            Repository repository = new Repository(repo, null, null);
            repository.RepositoryType = RepositoryType.History;
            Repositories.Insert(0, repository);

            if (Repositories.Count > 30)
            {
                Repositories.RemoveAt(30);
            }
        }
    }
}
