using System.Xml;
using System.Xml.Serialization;

namespace GitUI.LeftPanel
{
    public class CachedFavorites
    {
        private static readonly XmlSerializer _serializer = new(typeof(string[]), new XmlRootAttribute("RepoFavorites"));

        public HashSet<string> Favorites { get; } = [];

        public string Location { get; set; }

        private string FavConfigFile => Path.Combine(Location, "repofav");
        public void Load()
        {
            if (File.Exists(FavConfigFile))
            {
                try
                {
                    using StringReader reader = new(File.ReadAllText(FavConfigFile));
                    string[] deserialize = (string[])_serializer.Deserialize(reader);

                    if (deserialize != null)
                    {
                        foreach (string item in deserialize)
                        {
                            Favorites.Add(item);
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        public void Save()
        {
            if (Favorites.Any())
            {
                try
                {
                    XmlWriterSettings xmlWriterSettings = new() { Indent = true };
                    using StringWriter sw = new();

                    // Create empty namespaces to remove xmlns:xsi and xmlns:xsd
                    XmlSerializerNamespaces namespaces = new();
                    namespaces.Add("", ""); // Add an empty namespace

                    using XmlWriter xmlWriter = XmlWriter.Create(sw, xmlWriterSettings);

                    _serializer.Serialize(xmlWriter, Favorites.ToArray(), namespaces);
                    File.WriteAllText(FavConfigFile, sw.ToString());
                }
                catch
                {
                    // ignore
                }
            }
        }

        public bool Contains(string match)
        {
            return Favorites.Contains(match);
        }

        public void Add(string key)
        {
            if (Favorites.Add(key))
            {
                Save();
            }
        }

        public void Remove(string key)
        {
            if (Favorites.Remove(key))
            {
                Save();
            }
        }
    }
}
