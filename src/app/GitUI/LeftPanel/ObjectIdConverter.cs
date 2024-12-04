using System.Text.Json;
using System.Text.Json.Serialization;
using GitExtensions.Extensibility.Git;

namespace GitUI.LeftPanel;

internal class ObjectIdConverter : JsonConverter<ObjectId>
{
    public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Unexpected token type {reader.TokenType}, expected a JSON string for ObjectId.");
        }

        string? idString = reader.GetString();

        return idString is not null
            ? ObjectId.Parse(idString)
            : default;
    }

    public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
