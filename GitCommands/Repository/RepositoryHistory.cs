using System;
using System.IO;
using System.Xml.Serialization;

namespace GitCommands.Repository
{
    public class RepositoryHistory : RepositoryCategory
    {
        public RepositoryHistory(int maxCount)
        {
            Description = "Recent Repositories";
            MaxCount = maxCount;
        }

        public RepositoryHistory()
            : this(0)
        {
        }


        private int _maxCount;

        [XmlIgnore]
        public int MaxCount
        {
            get
            {
                return _maxCount;
            }
            set
            {
                _maxCount = value;
                while (_maxCount > 0 && Repositories.Count > _maxCount)
                {
                    Repositories.RemoveAt(_maxCount);
                }
            }
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
            if (string.IsNullOrEmpty(repo))
                return;

            repo = repo.Trim();

            if (string.IsNullOrEmpty(repo))
                return;

            if (!Repository.PathIsUrl(repo))
            {
                repo = repo.ToNativePath().EnsureTrailingPathSeparator();
            }

            Repository.RepositoryAnchor anchor = Repository.RepositoryAnchor.None;
            foreach (var recentRepository in Repositories)
            {
                if (!recentRepository.Path.Equals(repo, StringComparison.CurrentCultureIgnoreCase))
                    continue;
                anchor = recentRepository.Anchor;
                Repositories.Remove(recentRepository);
                break;
            }

            var repository = new Repository(repo, null, null) {
                RepositoryType = RepositoryType.History,
                Anchor = anchor
            };
            Repositories.Insert(0, repository);

            while (MaxCount > 0 && Repositories.Count > MaxCount)
            {
                Repositories.RemoveAt(MaxCount);
            }
        }
    }
}