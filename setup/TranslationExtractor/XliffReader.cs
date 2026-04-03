using System.Xml;

namespace TranslationExtractor;

/// <summary>
///  Reads translation entries from an existing XLIFF 1.0 file in the Git Extensions custom format.
/// </summary>
internal static class XliffReader
{
    /// <summary>
    ///  Reads all translation entries from an existing XLIFF file.
    /// </summary>
    /// <returns>A list of translation entries, or an empty list if the file doesn't exist.</returns>
    public static List<TranslationEntry> Read(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        List<TranslationEntry> entries = [];
        XmlDocument doc = new();
        doc.Load(filePath);

        XmlNodeList fileNodes = doc.GetElementsByTagName("file");
        foreach (XmlNode fileNode in fileNodes)
        {
            string? category = fileNode.Attributes?["original"]?.Value;
            if (category is null)
            {
                continue;
            }

            XmlNodeList transUnits = fileNode.SelectNodes(".//trans-unit") ?? EmptyNodeList();
            foreach (XmlNode transUnit in transUnits)
            {
                string? id = transUnit.Attributes?["id"]?.Value;
                if (id is null)
                {
                    continue;
                }

                string? source = transUnit.SelectSingleNode("source")?.InnerText;
                if (source is null)
                {
                    continue;
                }

                // Parse id into Name.Property
                int dotIndex = id.IndexOf('.');
                if (dotIndex < 0)
                {
                    continue;
                }

                string name = id[..dotIndex];
                string property = id[(dotIndex + 1)..];

                entries.Add(new TranslationEntry(category, name, property, source));
            }
        }

        return entries;
    }

    private static XmlNodeList EmptyNodeList()
    {
        XmlDocument empty = new();
        return empty.SelectNodes("/nonexistent")!;
    }
}
