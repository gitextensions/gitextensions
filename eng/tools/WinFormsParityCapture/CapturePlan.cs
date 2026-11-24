using System.Text.Json;

namespace WinFormsParityCapture;

internal sealed record CaptureSettingsProfile
{
    public required string UiFontFamily { get; init; }

    public required float UiFontSizePoints { get; init; }

    public required string FixedFontFamily { get; init; }

    public required float FixedFontSizePoints { get; init; }

    public required IReadOnlyDictionary<string, string> AppSettings { get; init; }

    public static CaptureSettingsProfile Load(string path)
    {
        CaptureSettingsProfile? profile = JsonSerializer.Deserialize<CaptureSettingsProfile>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return profile ?? throw new InvalidDataException("The capture settings profile is empty.");
    }
}
