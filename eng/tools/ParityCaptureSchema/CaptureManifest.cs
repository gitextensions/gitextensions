using System.Text.Json.Serialization;

namespace GitExtensions.ParityCapture;

/// <summary>
///  Maps a capture request matrix to its emitted artifacts.
/// </summary>
public sealed record CaptureSetManifest
{
    public required int SchemaVersion { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>
    ///  Gets the optional UTC fixture clock, independently of the actual acquisition timestamp.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? FixtureNowUtc
    {
        get;
        init
        {
            if (value is { Kind: not DateTimeKind.Utc })
            {
                throw new InvalidDataException("The capture fixture clock must be UTC.");
            }

            field = value;
        }
    }

    public required string ToolVersion { get; init; }

    public required string Repository { get; init; }

    public required IReadOnlyList<CaptureManifestEntry> Captures { get; init; }
}

/// <summary>
///  Describes one requested capture and its artifact paths or unsupported reason.
/// </summary>
public sealed record CaptureManifestEntry
{
    public required string ComponentType { get; init; }

    public required string ThemeId { get; init; }

    public required int ScalePercent { get; init; }

    public required string State { get; init; }

    public required CaptureStateStatus Status { get; init; }

    public string? Note { get; init; }

    public CaptureDpiMode? DpiMode { get; init; }

    public required CaptureMethod CaptureMethod { get; init; }

    public string? ImageFile { get; init; }

    public string? TreeFile { get; init; }

    /// <summary>
    ///  Gets optional live factory-bracket observations, independently of the fixture formatter clock.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public CaptureConstructionObservation? ConstructionObservation
    {
        get;
        init
        {
            value?.Validate();
            field = value;
        }
    }
}
