using GitExtensions.ParityCapture;

namespace GitExtensions.ParityDiff;

// parity-scaffolding: Records temporary parity-comparison output.
internal sealed record ParityDiffResult
{
    public const int CurrentSchemaVersion = 1;

    public required int SchemaVersion { get; init; }

    public required string ReferenceManifest { get; init; }

    public required string CandidateManifest { get; init; }

    public required ParityDiffSummary Summary { get; init; }

    public required IReadOnlyList<CaptureComparison> Captures { get; init; }
}

// parity-scaffolding: Summarizes temporary parity-comparison output.
internal sealed record ParityDiffSummary
{
    public required int RequestCount { get; init; }

    public required int ComparedCaptureCount { get; init; }

    public required int UnavailableCaptureCount { get; init; }

    public required int FindingCount { get; init; }

    public required IReadOnlyDictionary<string, int> FindingsByCategory { get; init; }
}

// parity-scaffolding: Records one temporary capture comparison.
internal sealed record CaptureComparison
{
    public required CaptureKey Key { get; init; }

    public required string Status { get; init; }

    public CaptureStateStatus? ReferenceStatus { get; init; }

    public CaptureStateStatus? CandidateStatus { get; init; }

    public string? ReferenceNote { get; init; }

    public string? CandidateNote { get; init; }

    public PixelMetrics? Pixels { get; init; }

    public required IReadOnlyList<ParityFinding> Findings { get; init; }
}

// parity-scaffolding: Joins temporary captures across frameworks.
internal sealed record CaptureKey : IComparable<CaptureKey>
{
    public required string ComponentType { get; init; }

    public required string ThemeId { get; init; }

    public required int ScalePercent { get; init; }

    public required string State { get; init; }

    public int CompareTo(CaptureKey? other)
    {
        if (other is null)
        {
            return 1;
        }

        int component = string.Compare(ComponentType, other.ComponentType, StringComparison.Ordinal);
        if (component != 0)
        {
            return component;
        }

        int theme = string.Compare(ThemeId, other.ThemeId, StringComparison.Ordinal);
        if (theme != 0)
        {
            return theme;
        }

        int scale = ScalePercent.CompareTo(other.ScalePercent);
        return scale != 0 ? scale : string.Compare(State, other.State, StringComparison.Ordinal);
    }

    public override string ToString() => $"{ComponentType}|{ThemeId}|{ScalePercent}|{State}";
}

// parity-scaffolding: Localizes one temporary parity finding.
internal sealed record ParityFinding
{
    public required string Category { get; init; }

    public required string Code { get; init; }

    public required string Path { get; init; }

    public required string Message { get; init; }

    public string? ReferenceValue { get; init; }

    public string? CandidateValue { get; init; }

    public string? Delta { get; init; }

    public string? Tolerance { get; init; }
}

// parity-scaffolding: Records temporary image-comparison metrics.
internal sealed record PixelMetrics
{
    public required int ReferenceWidth { get; init; }

    public required int ReferenceHeight { get; init; }

    public required int CandidateWidth { get; init; }

    public required int CandidateHeight { get; init; }

    public required double Ssim { get; init; }

    public required double DifferentPixelFraction { get; init; }

    public required int MaximumChannelDelta { get; init; }

    public required double MeanAbsoluteChannelDelta { get; init; }
}
