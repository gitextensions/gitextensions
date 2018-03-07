using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ResourceManager.Xliff
{
    /// <summary>Serializes and deserialize a <see cref="TranslationFile"/>.</summary>
    public static class TranslationSerializer
    {
        public static void Serialize(TranslationFile translation, string path)
        {
            using (TextWriter tw = new StreamWriter(path, false))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TranslationFile));
                serializer.Serialize(tw, translation);
            }
        }

        public static TranslationFile Deserialize(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(TranslationFile));
            TextReader stringReader = null;
            try
            {
                stringReader = new StreamReader(path);
                using (XmlTextReader xmlReader = new XmlTextReader(stringReader))
                {
                    stringReader = null;
                    return (TranslationFile)serializer.Deserialize(xmlReader);
                }
            }
            finally
            {
                stringReader?.Dispose();
            }
        }
    }
}
