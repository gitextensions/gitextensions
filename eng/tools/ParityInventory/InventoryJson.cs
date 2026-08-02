using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitExtensions.ParityInventory;

// parity-scaffolding: Keeps every functional inventory artifact byte-stable across operating systems.
internal static class InventoryJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, Options).Replace("\r\n", "\n", StringComparison.Ordinal) + "\n";
}
