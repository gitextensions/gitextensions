using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace GitUI.CommandsDialogs.SettingsDialog.Toolbars;

// XML serializer for toolbar configuration data.
// Uses DataContractSerializer for compatibility with [DataContract] attributes.
//
// The XML comes from the settings file, which the user or any other process can edit, so reading
// it is bounded on every axis: an oversized setting is rejected before it is copied into memory,
// and the parser is given explicit quotas instead of the unlimited defaults it would otherwise use.
internal static class ToolbarXmlSerializer
{
    // A full layout (the built-in toolbars plus custom ones, with every item) stays in the tens of
    // kilobytes. Reject anything substantially larger before Encoding.GetBytes duplicates it.
    private const int MaxXmlLength = 256 * 1024;

    // DataContractSerializer defaults to int.MaxValue, letting a crafted payload allocate objects
    // until the process runs out of memory. The layout is a few lists of small records.
    private const int MaxItemsInObjectGraph = 20_000;

    // ReadObject(Stream) would create a reader with XmlDictionaryReaderQuotas.Max, i.e. no limit
    // on nesting, element length or array length.
    private static readonly XmlDictionaryReaderQuotas _readerQuotas = new()
    {
        MaxDepth = 32,
        MaxStringContentLength = 16 * 1024,
        MaxArrayLength = 16 * 1024,
        MaxBytesPerRead = 16 * 1024,
        MaxNameTableCharCount = 16 * 1024
    };

    /// <typeparam name="T">Type of object to serialize (must have [DataContract] attribute)</typeparam>
    /// <param name="obj">Object to serialize</param>
    /// <returns>XML string representation</returns>
    public static string Serialize<T>(T? obj) where T : class
    {
        if (obj is null)
        {
            return string.Empty;
        }

        DataContractSerializer serializer = new(typeof(T));
        using MemoryStream stream = new();

        // Use a UTF-8 encoding WITHOUT a byte-order mark: XmlWriter writes the encoding preamble
        // (BOM) to the stream, and OmitXmlDeclaration does not suppress it. With Encoding.UTF8 the
        // resulting string would start with a stray U+FEFF that gets persisted into the settings.
        using XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Indent = true,
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            OmitXmlDeclaration = true
        });

        serializer.WriteObject(writer, obj);
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <typeparam name="T">Type of object to deserialize (must have [DataContract] attribute)</typeparam>
    /// <param name="xml">XML string to deserialize</param>
    /// <returns>Deserialized object, or <see langword="null"/> if the setting is absent or unreadable</returns>
    public static T? Deserialize<T>(string xml) where T : class
    {
        if (string.IsNullOrWhiteSpace(xml) || xml.Length > MaxXmlLength)
        {
            return null;
        }

        try
        {
            DataContractSerializer serializer = new(typeof(T), new DataContractSerializerSettings
            {
                MaxItemsInObjectGraph = MaxItemsInObjectGraph
            });

            // CreateTextReader neither accepts a DTD nor resolves external entities, so a settings
            // file cannot pull in outside content or blow up through entity expansion.
            using XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(xml), _readerQuotas);

            return (T?)serializer.ReadObject(reader);
        }
        catch (Exception ex) when (ex is XmlException or SerializationException or FormatException or OverflowException)
        {
            // A malformed or foreign setting must not prevent the application from starting;
            // the caller falls back to a default (empty) configuration.
            Trace.WriteLine(ex.Message);
            return null;
        }
    }
}
