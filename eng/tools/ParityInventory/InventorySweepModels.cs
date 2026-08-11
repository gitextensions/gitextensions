namespace GitExtensions.ParityInventory;

// parity-scaffolding: Defines the deterministic aggregate contract for a functional baseline sweep.
internal sealed record InventorySweepResult
{
    public const int CurrentSchemaVersion = 2;

    public required int SchemaVersion { get; init; }

    public required string AnalyzedCommit { get; init; }

    public required string PortMap { get; init; }

    public required InventorySweepSummary Summary { get; init; }

    public required IReadOnlyList<SweepMappingResult> Mappings { get; init; }

    public required IReadOnlyList<SweepTypeResult> Types { get; init; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string OutputFile { get; init; } = string.Empty;
}

// parity-scaffolding: Summarizes batch source coverage without implying runtime parity.
internal sealed record InventorySweepSummary
{
    public required int MappingCount { get; init; }

    public required int FormerParityCount { get; init; }

    public required int LinkedExactCount { get; init; }

    public required int AnalyzedTypeCount { get; init; }

    public required int UnsupportedMappingCount { get; init; }

    public required int FindingCount { get; init; }

    public required IReadOnlyDictionary<string, int> FindingsByCategory { get; init; }

    public required int AdaptedCommentCount { get; init; }
}

// parity-scaffolding: Records how one portmap source-to-twin mapping entered the sweep.
internal sealed record SweepMappingResult
{
    public required string Source { get; init; }

    public required string Twin { get; init; }

    public required string PortMapStatus { get; init; }

    public required string AnalysisStatus { get; init; }

    public required IReadOnlyList<string> TypeNames { get; init; }

    public required int FindingCount { get; init; }

    public required IReadOnlyList<string> Evidence { get; init; }

    public string? Note { get; init; }
}

// parity-scaffolding: Summarizes one type-level P0.4 report within a full sweep.
internal sealed record SweepTypeResult
{
    public required string TypeName { get; init; }

    public required string Report { get; init; }

    public required int OriginalPartCount { get; init; }

    public required int TwinPartCount { get; init; }

    public required int FindingCount { get; init; }

    public required IReadOnlyDictionary<string, int> FindingsByCategory { get; init; }

    public required int AdaptedCommentCount { get; init; }
}
