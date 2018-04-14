using System;

namespace GitCommands.UserRepositoryHistory
{
    [Serializable]
    public class Repository
    {
        private string _path;

        public enum RepositoryAnchor
        {
            MostRecent,
            LessRecent,
            None
        }

        // required by XmlSerializer
        private Repository()
        {
            Anchor = RepositoryAnchor.None;
        }

        public Repository(string path)
            : this()
        {
            Path = path;
        }

        public RepositoryAnchor Anchor { get; set; }

        public string Category { get; set; }

        public string Path
        {
            get => _path ?? string.Empty;
            set => _path = value;
        }

        public override string ToString()
        {
            return Path + " (" + Anchor + ")";
        }
    }
}