using System.Text.Json.Serialization;

namespace GitExtensions.ParityInventory;

// parity-scaffolding: Defines the committed proof ledger consumed by all later parity tranches.
internal sealed record ParityLedger
{
    public const int CurrentSchemaVersion = 1;

    [JsonPropertyName("$schema")]
    public required string Schema { get; init; }

    public required int SchemaVersion { get; init; }

    public required string AnalyzedCommit { get; init; }

    public required IReadOnlyList<string> AxisOrder { get; init; }

    public required IReadOnlyList<LedgerComponent> Components { get; init; }
}

// parity-scaffolding: Records the six-axis proof state for one unambiguous portmap mapping.
internal sealed record LedgerComponent
{
    public required string Source { get; init; }

    public required string Twin { get; init; }

    public required bool Complete { get; init; }

    public required LedgerAxes Axes { get; init; }
}

// parity-scaffolding: Names every parity axis explicitly so omissions cannot deserialize silently.
internal sealed record LedgerAxes
{
    public required AxisEvidence Structural { get; init; }

    public required AxisEvidence Functional { get; init; }

    public required AxisEvidence Visual { get; init; }

    public required AxisEvidence ThemingAndColor { get; init; }

    public required AxisEvidence BehavioralState { get; init; }

    public required AxisEvidence Platform { get; init; }
}

// parity-scaffolding: Carries evidence provenance and the measured state for one parity axis.
internal sealed record AxisEvidence
{
    public required string Status { get; init; }

    public required string VerifiedOn { get; init; }

    public required string Commit { get; init; }

    public required IReadOnlyList<string> Evidence { get; init; }

    public required IReadOnlyList<string> Platforms { get; init; }

    public required int FindingCount { get; init; }

    public required string Note { get; init; }
}

// parity-scaffolding: Summarizes the first real baseline without turning findings into parity claims.
internal sealed record BaselineReport
{
    public required int SchemaVersion { get; init; }

    public required string AnalyzedCommit { get; init; }

    public required string VerifiedOn { get; init; }

    public required BaselineInputs Inputs { get; init; }

    public required BaselineSummary Summary { get; init; }

    public required IReadOnlyList<string> CoverageGaps { get; init; }
}

// parity-scaffolding: Identifies the exact evidence artifacts aggregated by the baseline.
internal sealed record BaselineInputs
{
    public required string PortMap { get; init; }

    public required string FunctionalReport { get; init; }

    public required string VisualReport { get; init; }

    public required string ReferenceManifest { get; init; }

    public required string PairedReferenceManifest { get; init; }

    public required string PairedCandidateManifest { get; init; }

    public required string Ledger { get; init; }
}

// parity-scaffolding: Reports measured coverage and downgrade counts for PLAN bookkeeping.
internal sealed record BaselineSummary
{
    public required int MappingCount { get; init; }

    public required int FormerParityCount { get; init; }

    public required int CompleteLedgerEntryCount { get; init; }

    public required int FunctionalTypeCount { get; init; }

    public required int FunctionalFindingCount { get; init; }

    public required int UnsupportedFunctionalMappingCount { get; init; }

    public required int VisualRequestCount { get; init; }

    public required int ReferenceRequestCount { get; init; }

    public required int CapturedReferenceCount { get; init; }

    public required int UnsupportedReferenceCount { get; init; }

    public required int ReferenceTypeCount { get; init; }

    public required int ComparedVisualCaptureCount { get; init; }

    public required int UnavailableVisualCaptureCount { get; init; }

    public required int VisualFindingCount { get; init; }

    public required int VisuallyComparedTypeCount { get; init; }
}
