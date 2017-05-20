using System;
using System.Xml.Serialization;

namespace GitCommands.Repository
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

        [XmlIgnore]
        public bool IsRemote => PathIsUrl(Path);

        public string Path
        {
            get
            {
                return _path ?? string.Empty;
            }
            set
            {
                _path = value;
            }
        }


        public override string ToString()
        {
            return Path + " ("+ Anchor.ToString() +")";
        }


        // TODO: doesn't belong here
        public static bool PathIsUrl(string path)
        {
            return !String.IsNullOrEmpty(path) &&
                (path.StartsWith("http", StringComparison.CurrentCultureIgnoreCase) ||
                 path.StartsWith("git", StringComparison.CurrentCultureIgnoreCase) ||
                 path.StartsWith("ssh", StringComparison.CurrentCultureIgnoreCase));
        }
    }
}