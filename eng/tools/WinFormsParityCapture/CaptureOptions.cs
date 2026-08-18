using System.Globalization;

namespace WinFormsParityCapture;

internal enum CaptureCommand
{
    Capture,
    Validate,
    Worker
}

internal sealed record CaptureOptions
{
    public required CaptureCommand Command { get; init; }

    public string? PlanPath { get; init; }

    public string? RepositoryPath { get; init; }

    public string? OutputPath { get; init; }

    public string? ManifestPath { get; init; }

    public string? ComponentType { get; init; }

    public string? ThemeId { get; init; }

    public int? ScalePercent { get; init; }

    public CaptureMonitor? Monitor { get; init; }

    public string? DpiMode { get; init; }

    public string? WorkerResultPath { get; init; }

    public string? StateId { get; init; }

    public IReadOnlySet<string> Components { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    public IReadOnlySet<string> Themes { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlySet<int> Scales { get; init; } = new HashSet<int>();

    public bool RoundTrip { get; init; }

    public bool RequireResolvedArgb { get; init; }

    public static CaptureOptions Parse(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help")
        {
            throw new CaptureHelpException();
        }

        CaptureCommand command = args[0] switch
        {
            "capture" => CaptureCommand.Capture,
            "validate" => CaptureCommand.Validate,
            "--worker" => CaptureCommand.Worker,
            _ => throw new ArgumentException($"Unknown command '{args[0]}'.")
        };

        Dictionary<string, string> values = new(StringComparer.Ordinal);
        HashSet<string> switches = new(StringComparer.Ordinal);
        for (int i = 1; i < args.Length; i++)
        {
            string argument = args[i];
            if (argument is "--round-trip" or "--require-resolved-argb")
            {
                switches.Add(argument);
                continue;
            }

            if (!argument.StartsWith("--", StringComparison.Ordinal) || i + 1 >= args.Length)
            {
                throw new ArgumentException($"Option '{argument}' requires a value.");
            }

            values[argument] = args[++i];
        }

        return new CaptureOptions
        {
            Command = command,
            PlanPath = GetValue("--plan"),
            RepositoryPath = GetValue("--repository"),
            OutputPath = GetValue("--output"),
            ManifestPath = GetValue("--manifest"),
            ComponentType = GetValue("--component"),
            ThemeId = GetValue("--theme"),
            ScalePercent = ParseNullableInt("--scale"),
            Monitor = ParseMonitor(GetValue("--monitor")),
            DpiMode = GetValue("--dpi-mode"),
            WorkerResultPath = GetValue("--worker-result"),
            StateId = GetValue("--state"),
            Components = ParseSet(GetValue("--components"), StringComparer.Ordinal),
            Themes = ParseSet(GetValue("--themes"), StringComparer.OrdinalIgnoreCase),
            Scales = ParseIntSet(GetValue("--scales")),
            RoundTrip = switches.Contains("--round-trip"),
            RequireResolvedArgb = switches.Contains("--require-resolved-argb")
        };

        string? GetValue(string key) => values.GetValueOrDefault(key);

        int? ParseNullableInt(string key)
        {
            string? value = GetValue(key);
            return value is null
                ? null
                : int.Parse(value, NumberStyles.None, CultureInfo.InvariantCulture);
        }
    }

    public static void WriteHelp(TextWriter writer)
    {
        writer.WriteLine("""
            Captures the real Git Extensions WinForms UI for parity comparison.

            Usage:
              WinFormsParityCapture capture --plan <capture-plan.json> --repository <throwaway-repo> --output <directory>
                  [--components <type,...>] [--themes <id,...>] [--scales <100,...>]

              WinFormsParityCapture validate --manifest <manifest.json>
                  [--round-trip] [--require-resolved-argb]

            The repository must be outside the current working tree. The public capture
            command copies its runtime to a disposable directory before loading AppSettings;
            the worker therefore cannot read or write the user's Git Extensions profile.
            """);
    }

    private static IReadOnlySet<int> ParseIntSet(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new HashSet<int>();
        }

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => int.Parse(item, NumberStyles.None, CultureInfo.InvariantCulture))
            .ToHashSet();
    }

    private static CaptureMonitor? ParseMonitor(string? value)
    {
        if (value is null)
        {
            return null;
        }

        int[] parts = value.Split(',', StringSplitOptions.TrimEntries)
            .Select(item => int.Parse(item, NumberStyles.Integer, CultureInfo.InvariantCulture))
            .ToArray();
        if (parts.Length != 6)
        {
            throw new ArgumentException("--monitor requires x,y,width,height,dpiX,dpiY.");
        }

        return new CaptureMonitor(parts[0], parts[1], parts[2], parts[3], parts[4], parts[5]);
    }

    private static IReadOnlySet<string> ParseSet(string? value, StringComparer comparer) =>
        string.IsNullOrWhiteSpace(value)
            ? new HashSet<string>(comparer)
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet(comparer);
}

internal sealed class CaptureHelpException : Exception;
