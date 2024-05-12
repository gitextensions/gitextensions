using System.Xml;
using System.Xml.Serialization;

namespace GitExtensions.Extensibility.Translations.Xliff;

/// <summary>Serializes and deserialize a <see cref="TranslationFile"/>.</summary>
public static class TranslationSerializer
{
    public static void Serialize(TranslationFile translation, string path)
    {
        XmlWriterSettings xmlWriterSettings = new()
        {
            Indent = true
        };
        XmlSerializer serializer = new(typeof(TranslationFile));
        using XmlWriter xmlWriter = XmlWriter.Create(path, xmlWriterSettings);
        serializer.Serialize(xmlWriter, translation);
    }

    public static TranslationFile? Deserialize(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        XmlSerializer serializer = new(typeof(TranslationFile));
        TextReader? stringReader = null;
        try
        {
            stringReader = new StreamReader(path);
            using XmlTextReader xmlReader = new(stringReader);
            stringReader = null;
            return (TranslationFile)serializer.Deserialize(xmlReader);
        }
        finally
        {
            stringReader?.Dispose();
        }
    }
}
