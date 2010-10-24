using System;

namespace GitCommands.Repository
{
    public class RepositoryHistory : RepositoryCategory
    {
        public RepositoryHistory()
        {
            Description = "Recent Repositories";
        }

        public override void SetIcon()
        {
            foreach (var recentRepository in Repositories)
            {
                recentRepository.RepositoryType = RepositoryType.History;
            }
        }

        public void RemoveRecentRepository(string repo)
        {
            foreach (var recentRepository in Repositories)
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

            repo = repo.Replace(Settings.PathSeparatorWrong, Settings.PathSeparator);
            if (!repo.EndsWith(Settings.PathSeparator.ToString()) &&
                !repo.StartsWith("http", StringComparison.CurrentCultureIgnoreCase) &&
                !repo.StartsWith("git", StringComparison.CurrentCultureIgnoreCase) &&
                !repo.StartsWith("ssh", StringComparison.CurrentCultureIgnoreCase))
                repo += Settings.PathSeparator;

            foreach (var recentRepository in Repositories)
            {
                if (!recentRepository.Path.Equals(repo, StringComparison.CurrentCultureIgnoreCase))
                    continue;
                Repositories.Remove(recentRepository);
                break;
            }

            var repository = new Repository(repo, null, null) {RepositoryType = RepositoryType.History};
            Repositories.Insert(0, repository);

            if (Repositories.Count > 30)
            {
                Repositories.RemoveAt(30);
            }
        }
    }
}