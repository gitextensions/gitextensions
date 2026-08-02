using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitExtensions.ParityCapture;

/// <summary>
///  Defines the framework-neutral capture matrix consumed by both emitters.
/// </summary>
public sealed record CapturePlan
{
    public required int SchemaVersion { get; init; }

    public required string SettingsProfile { get; init; }

    public required IReadOnlyList<int> Scales { get; init; }

    public required IReadOnlyList<CaptureThemePlan> Themes { get; init; }

    public required IReadOnlyList<CaptureComponentPlan> Components { get; init; }

    /// <summary>
    ///  Loads and validates a capture plan.
    /// </summary>
    public static CapturePlan Load(string path)
    {
        CapturePlan? plan = JsonSerializer.Deserialize<CapturePlan>(
            File.ReadAllText(path),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });

        if (plan is null || plan.SchemaVersion != CaptureDocument.CurrentSchemaVersion)
        {
            throw new InvalidDataException("The capture plan is missing or has an unsupported schema version.");
        }

        if (plan.Scales.Count == 0
            || plan.Scales.Any(scale => scale is not (100 or 125 or 150 or 200))
            || plan.Themes.Count == 0
            || plan.Components.Count == 0)
        {
            throw new InvalidDataException("The capture plan does not define a supported scale, theme, and component matrix.");
        }

        return plan;
    }
}

/// <summary>
///  Describes one theme in a capture matrix.
/// </summary>
public sealed record CaptureThemePlan
{
    public required string Id { get; init; }

    public required string Kind { get; init; }

    public required string File { get; init; }

    public required bool IsBuiltin { get; init; }
}

/// <summary>
///  Describes one component and its requested states.
/// </summary>
public sealed record CaptureComponentPlan
{
    public required string TypeName { get; init; }

    public IReadOnlyDictionary<string, string> TextValues { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public required IReadOnlyList<CaptureStatePlan> States { get; init; }
}

/// <summary>
///  Describes one requested component state.
/// </summary>
public sealed record CaptureStatePlan
{
    public required string Id { get; init; }

    public required CaptureStateKind Kind { get; init; }

    public string? TargetField { get; init; }
}

/// <summary>
///  Identifies a state operation shared by the WinForms and Avalonia drivers.
/// </summary>
public enum CaptureStateKind
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
