using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace GitCommands
{
    public class RepositoryHistory
    {
        public List<string> MostRecentRepositories = new List<string>();

        public void RemoveRecentRepository(string repo)
        {
            foreach (string recentRepository in MostRecentRepositories)
            {
                if (recentRepository.Equals(repo, StringComparison.CurrentCultureIgnoreCase))
                {
                    MostRecentRepositories.Remove(recentRepository);
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

            foreach (string recentRepository in MostRecentRepositories)
            {
                if (recentRepository.Equals(repo, StringComparison.CurrentCultureIgnoreCase))
                {
                    MostRecentRepositories.Remove(recentRepository);
                    break;
                }
            }

            MostRecentRepositories.Insert(0, repo);

            if (MostRecentRepositories.Count > 30)
            {
                MostRecentRepositories.RemoveAt(30);
            }
        }
    }
}
