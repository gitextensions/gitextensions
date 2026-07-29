using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitExtensions.ParityDiff;

// parity-scaffolding: Provides deterministic JSON for temporary parity measurements.
internal static class JsonDefaults
{
    public static JsonSerializerOptions ReadOptions { get; } = CreateReadOptions();

    public static JsonSerializerOptions WriteOptions { get; } = CreateWriteOptions();

    private static JsonSerializerOptions CreateReadOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }

    private static JsonSerializerOptions CreateWriteOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}
