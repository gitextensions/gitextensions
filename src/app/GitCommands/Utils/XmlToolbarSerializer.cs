using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace GitCommands.Utils;

// XML serializer for toolbar configuration data.
// Uses DataContractSerializer for compatibility with [DataContract] attributes.
public static class XmlToolbarSerializer
{
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
    /// <returns>Deserialized object or null if deserialization fails</returns>
    public static T? Deserialize<T>(string xml) where T : class
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return null;
        }

        try
        {
            DataContractSerializer serializer = new(typeof(T));
            using MemoryStream stream = new(Encoding.UTF8.GetBytes(xml));
            return (T?)serializer.ReadObject(stream);
        }
        catch (Exception)
        {
            // If deserialization fails, try to deserialize from JSON for backward compatibility
            return TryDeserializeFromJson<T>(xml);
        }
    }

    // Tries to deserialize from JSON format for backward compatibility with existing settings.
    private static T? TryDeserializeFromJson<T>(string data) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(data);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
