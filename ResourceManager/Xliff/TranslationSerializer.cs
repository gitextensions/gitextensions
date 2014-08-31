using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ResourceManager.Xliff
{
    /// <summary>Serializes and deserialize a <see cref="Translation"/>.</summary>
    public static class TranslationSerializer
    {
        public static void Serialize(Translation translation, string path)
        {
            using (TextWriter tw = new StreamWriter(path, false))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Translation));
                serializer.Serialize(tw, translation);
            }
        }

        public static Translation Deserialize(string path)
        {
            if (!File.Exists(path))
                return null;

            XmlSerializer serializer = new XmlSerializer(typeof(Translation));
            TextReader stringReader = null;
            try
            {
                stringReader = new StreamReader(path);
                using (XmlTextReader xmlReader = new XmlTextReader(stringReader))
                {
                    stringReader = null;
                    return (Translation)serializer.Deserialize(xmlReader);
                }
            }
            finally
            {
                if (stringReader != null)
                    stringReader.Dispose();
            }
        }
    }
}
