using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace GitCommands
{
    public static class RepositoryHistory
    {

        public static List<string> MostRecentRepositories = new List<string>();

        public static void AddMostRecentRepository(string repo)
        {
            repo = repo.Trim();

            if (MostRecentRepositories.IndexOf(repo) > -1)
            {
                MostRecentRepositories.Remove(repo);
            }

            MostRecentRepositories.Insert(0, repo);

            if (MostRecentRepositories.Count > 13)
            {
                MostRecentRepositories.RemoveAt(13);
            }
        }
    }
}
