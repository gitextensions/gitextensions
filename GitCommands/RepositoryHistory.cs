using System;

namespace GitCommands
{
    public class RepositoryHistory : RepositoryCategory
    {
        public RepositoryHistory()
        {
            Description = "Recent Repositories";
        }

        public override void SetIcon()
        {
            foreach (Repository recentRepository in Repositories)
            {
                recentRepository.RepositoryType = RepositoryType.History;
            }
        }

        public void RemoveRecentRepository(string repo)
        {
            foreach (Repository recentRepository in Repositories)
            {
                if (!recentRepository.Path.Equals(repo, StringComparison.CurrentCultureIgnoreCase))
                    continue;
                Repositories.Remove(recentRepository);
                break;
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
                if (!recentRepository.Path.Equals(repo, StringComparison.CurrentCultureIgnoreCase))
                    continue;
                Repositories.Remove(recentRepository);
                break;
            }

            Repository repository = new Repository(repo, null, null) {RepositoryType = RepositoryType.History};
            Repositories.Insert(0, repository);

            if (Repositories.Count > 30)
            {
                Repositories.RemoveAt(30);
            }
        }
    }
}
