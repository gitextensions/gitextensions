using System.Text.Json;

namespace GitExtensions.ParityDiff;

// parity-scaffolding: Declares tolerances for temporary parity measurements.
internal sealed record DiffConfiguration
{
    public const int CurrentSchemaVersion = 1;

    public required int SchemaVersion { get; init; }

    public required DiffTolerance Defaults { get; init; }

    public IReadOnlyDictionary<string, DiffTolerance> Components { get; init; } =
        new Dictionary<string, DiffTolerance>(StringComparer.Ordinal);

    public static DiffConfiguration Load(string path)
    {
        DiffConfiguration? configuration = JsonSerializer.Deserialize<DiffConfiguration>(
            File.ReadAllText(path),
            JsonDefaults.ReadOptions);
        if (configuration is null || configuration.SchemaVersion != CurrentSchemaVersion)
        {
            throw new InvalidDataException("The parity-diff configuration has an unsupported schema version.");
        }

        configuration.Defaults.Validate();
        foreach (DiffTolerance tolerance in configuration.Components.Values)
        {
            tolerance.Validate();
        }

        return configuration;
    }

    public DiffTolerance GetTolerance(string componentType) =>
        Components.TryGetValue(componentType, out DiffTolerance? tolerance)
            ? tolerance
            : Defaults;
}

// parity-scaffolding: Declares metric tolerances for temporary parity measurements.
internal sealed record DiffTolerance
{
    public required decimal GeometryDip { get; init; }

    public required decimal FontSizePoints { get; init; }

    public required decimal BorderWidthDip { get; init; }

    public required decimal CornerRadiusDip { get; init; }

    public required PixelTolerance Pixels { get; init; }

    public void Validate()
    {
        if (GeometryDip < 0
            || FontSizePoints < 0
            || BorderWidthDip < 0
            || CornerRadiusDip < 0)
        {
            throw new InvalidDataException("Metric tolerances cannot be negative.");
        }

        Pixels.Validate();
    }
}

// parity-scaffolding: Declares pixel tolerances for temporary parity measurements.
internal sealed record PixelTolerance
{
    public required double MinimumSsim { get; init; }

    public required double MaximumDifferentPixelFraction { get; init; }

    public required byte MaximumChannelDelta { get; init; }

    public void Validate()
    {
        if (MinimumSsim is < -1 or > 1
            || MaximumDifferentPixelFraction is < 0 or > 1)
        {
            throw new InvalidDataException("Pixel fractions and SSIM thresholds are outside their supported ranges.");
        }
    }
}
