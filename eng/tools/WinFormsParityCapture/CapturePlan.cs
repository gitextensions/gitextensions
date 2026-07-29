using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinFormsParityCapture;

internal sealed record CapturePlan
{
    public required int SchemaVersion { get; init; }

    public required string SettingsProfile { get; init; }

    public required IReadOnlyList<int> Scales { get; init; }

    public required IReadOnlyList<CaptureThemePlan> Themes { get; init; }

    public required IReadOnlyList<CaptureComponentPlan> Components { get; init; }

    public static CapturePlan Load(string path)
    {
        CapturePlan? plan = JsonSerializer.Deserialize<CapturePlan>(
            File.ReadAllText(path),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });

        if (plan is null || plan.SchemaVersion != 1)
        {
            throw new InvalidDataException("The capture plan is missing or has an unsupported schema version.");
        }

        return plan;
    }
}

internal sealed record CaptureThemePlan
{
    public required string Id { get; init; }

    public required string Kind { get; init; }

    public required string File { get; init; }

    public required bool IsBuiltin { get; init; }
}

internal sealed record CaptureComponentPlan
{
    public required string TypeName { get; init; }

    public required IReadOnlyList<CaptureStatePlan> States { get; init; }
}

internal sealed record CaptureStatePlan
{
    public required string Id { get; init; }

    public required CaptureStateKind Kind { get; init; }

    public string? TargetField { get; init; }
}

internal enum CaptureStateKind
{
    Normal,
    Focus,
    Disabled,
    Checked,
    Expanded,
    Hover,
    Pressed,
    MenuOpen
}

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
