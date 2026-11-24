using GitExtensions.ParityCapture;

namespace WinFormsParityCapture;

internal sealed record CaptureWorkerResult
{
    public required IReadOnlyList<CaptureManifestEntry> Captures { get; init; }
}

internal readonly record struct CaptureMonitor(int X, int Y, int Width, int Height, int DpiX, int DpiY)
{
    public int ScalePercent => (int)Math.Round(DpiX * 100.0 / 96.0);

    public override string ToString() => $"{X},{Y},{Width},{Height},{DpiX},{DpiY}";
}
