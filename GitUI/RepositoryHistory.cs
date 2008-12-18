using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public static class RepositoryHistory
    {

        public static List<string> MostRecentRepositories = new List<string>();

        public static void AddMostRecentRepository(string repo)
        {
            if (MostRecentRepositories.IndexOf(repo) > -1)
                return;

            MostRecentRepositories.Insert(0, repo);

            if (MostRecentRepositories.Count > 8)
            {
                MostRecentRepositories.RemoveAt(5);
            }
        }
    }
}
