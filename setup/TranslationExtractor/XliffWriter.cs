using System.Xml;

namespace TranslationExtractor;

/// <summary>
///  Writes translation entries in the custom XLIFF 1.0 format used by Git Extensions.
///  Produces output matching the existing English.xlf / English.Plugins.xlf format.
/// </summary>
internal static class XliffWriter
{
    /// <summary>
    ///  Writes the translation entries to an XLIFF file in the Git Extensions custom format.
    /// </summary>
    /// <param name="entries">All translation entries to write.</param>
    /// <param name="outputPath">The output file path.</param>
    public static void Write(IEnumerable<TranslationEntry> entries, string outputPath)
    {
        // Group by category (class name), sorted alphabetically
        IOrderedEnumerable<IGrouping<string, TranslationEntry>> categories = entries
            .GroupBy(e => e.Category, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal);

        XmlWriterSettings settings = new()
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\r\n",
            Encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
        };

        string? dir = Path.GetDirectoryName(outputPath);
        if (dir is not null)
        {
            Directory.CreateDirectory(dir);
        }

        using FileStream stream = File.Create(outputPath);
        using XmlWriter writer = XmlWriter.Create(stream, settings);

        writer.WriteStartDocument();

        // <xliff version="1.0" GitExVersion="">
        writer.WriteStartElement("xliff");
        writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
        writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
        writer.WriteAttributeString("version", "1.0");
        writer.WriteAttributeString("GitExVersion", "");

        foreach (IGrouping<string, TranslationEntry> category in categories)
        {
            // <file datatype="plaintext" original="ClassName" source-language="en">
            writer.WriteStartElement("file");
            writer.WriteAttributeString("datatype", "plaintext");
            writer.WriteAttributeString("original", category.Key);
            writer.WriteAttributeString("source-language", "en");

            // <body>
            writer.WriteStartElement("body");

            // Sort items alphabetically by ID within each category
            foreach (TranslationEntry entry in category.OrderBy(e => e.Id, StringComparer.Ordinal))
            {
                // <trans-unit id="fieldName.Property">
                writer.WriteStartElement("trans-unit");
                writer.WriteAttributeString("id", entry.Id);

                // <source>English text</source>
                writer.WriteElementString("source", entry.Source);

                // <target />
                writer.WriteStartElement("target");
                writer.WriteEndElement();

                writer.WriteEndElement(); // </trans-unit>
            }

            writer.WriteEndElement(); // </body>
            writer.WriteEndElement(); // </file>
        }

        writer.WriteEndElement(); // </xliff>
        writer.WriteEndDocument();
    }
}
