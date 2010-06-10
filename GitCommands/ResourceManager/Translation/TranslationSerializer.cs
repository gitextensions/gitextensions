using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace ResourceManager.Translation
{
    public class TranslationSerializer
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
            using (TextReader stringReader = new StreamReader(path))
            using (XmlTextReader xmlReader = new XmlTextReader(stringReader))
            {
                return (Translation)serializer.Deserialize(xmlReader);

            }
        }
    }
}
