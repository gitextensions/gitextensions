using GitExtensions.ParityCapture;

namespace GitExtensions.ParityDiff;

// parity-scaffolding: Records strict framework-neutral color-role comparison output.
internal sealed record ColorRoleResult
{
    public const int CurrentSchemaVersion = 1;

    public required int SchemaVersion { get; init; }

    public required string ReferenceManifest { get; init; }

    public required string CandidateManifest { get; init; }

    public required string RoleCatalog { get; init; }

    public required IReadOnlyList<ColorRoleDefinition> Roles { get; init; }

    public required ColorRoleSummary Summary { get; init; }

    public required IReadOnlyList<ColorRoleCaptureComparison> Captures { get; init; }
}

// parity-scaffolding: Summarizes strict framework-neutral color-role comparison output.
internal sealed record ColorRoleSummary
{
    public required int RequestCount { get; init; }

    public required int ComparedCaptureCount { get; init; }

    public required int UnavailableCaptureCount { get; init; }

    public required int DeclaredRoleCount { get; init; }

    public required int RoleComparisonCount { get; init; }

    public required int MatchCount { get; init; }

    public required int FindingCount { get; init; }
}

// parity-scaffolding: Records one capture's strict framework-neutral color-role comparison.
internal sealed record ColorRoleCaptureComparison
{
    public required CaptureKey Key { get; init; }

    public required string Status { get; init; }

    public CaptureStateStatus? ReferenceStatus { get; init; }

    public CaptureStateStatus? CandidateStatus { get; init; }

    public string? ReferenceNote { get; init; }

    public string? CandidateNote { get; init; }

    public required int RoleComparisonCount { get; init; }

    public required int MatchCount { get; init; }

    public required IReadOnlyList<ColorRoleFinding> Findings { get; init; }
}

// parity-scaffolding: Localizes one strict framework-neutral color-role finding.
internal sealed record ColorRoleFinding
{
    public required string Code { get; init; }

    public required string Role { get; init; }

    public string? Meaning { get; init; }

    public required string Message { get; init; }

    public string? ReferenceValue { get; init; }

    public string? CandidateValue { get; init; }
}
