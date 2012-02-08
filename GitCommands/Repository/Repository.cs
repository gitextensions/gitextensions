using System.Xml.Serialization;

namespace GitCommands.Repository
{
    public class Repository
    {
        public enum RepositoryAnchor
        {
            MostRecent,
            LessRecent,
            None
        }


        public Repository()
        {
            Anchor = RepositoryAnchor.None;
        }

        public Repository(string path, string description, string title)
            : this()
        {
            Path = path;
            Description = description;
            Title = title;
            RepositoryType = RepositoryType.Repository;
        }

        public string Title { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        RepositoryAnchor _Anchor;
        public RepositoryAnchor Anchor
        {
            get { return _Anchor; }
            set { 
            _Anchor = value;
        } }

        [XmlIgnore]
        public RepositoryType RepositoryType { get; set; }

        public void Assign(Repository source)
        {
            Path = source.Path;
            Title = source.Title;
            Description = source.Description;
            RepositoryType = source.RepositoryType;
        }

        public override string  ToString()
        {
            return Path + " ("+Anchor+")";
        }
    }
}