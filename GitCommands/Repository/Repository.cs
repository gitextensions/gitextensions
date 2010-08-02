using System.Xml.Serialization;

namespace GitCommands.Repository
{
    public class Repository
    {
        public Repository()
        {
        }

        public Repository(string path, string description, string title)
        {
            Path = path;
            Description = description;
            Title = title;
            RepositoryType = RepositoryType.Repository;
        }

        public string Title { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }

        [XmlIgnore]
        public RepositoryType RepositoryType { get; set; }
    }
}