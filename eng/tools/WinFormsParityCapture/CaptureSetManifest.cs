using GitExtensions.ParityCapture;

namespace WinFormsParityCapture;

internal sealed record CaptureSetManifest
{
    public required int SchemaVersion { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public required string ToolVersion { get; init; }

    public required string Repository { get; init; }

    public required IReadOnlyList<CaptureManifestEntry> Captures { get; init; }
}

internal sealed record CaptureManifestEntry
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
}

internal sealed record CaptureWorkerResult
{
    public required IReadOnlyList<CaptureManifestEntry> Captures { get; init; }
}

internal readonly record struct CaptureMonitor(int X, int Y, int Width, int Height, int DpiX, int DpiY)
{
    public int ScalePercent => (int)Math.Round(DpiX * 100.0 / 96.0);

    public override string ToString() => $"{X},{Y},{Width},{Height},{DpiX},{DpiY}";
}
