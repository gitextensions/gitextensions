namespace GitExtensions.ParityInventory;

// parity-scaffolding: Defines the batch inventory boundary used by the initial parity baseline.
internal sealed record SweepOptions
{
    public required string PortMapFile { get; init; }

    public required string OriginalRoot { get; init; }

    public required string TwinRoot { get; init; }

    public required string TranslationsFile { get; init; }

    public required string AnalyzedCommit { get; init; }

    public required string OutputFile { get; init; }

    public static SweepOptions Parse(string[] args)
    {
        Dictionary<string, string> values = new(StringComparer.Ordinal);
        for (int index = 1; index < args.Length; index += 2)
        {
            if (index + 1 >= args.Length || !args[index].StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException(Usage);
            }

            values[args[index]] = args[index + 1];
        }

        return new SweepOptions
        {
            PortMapFile = Require(values, "--portmap"),
            OriginalRoot = Require(values, "--original-root"),
            TwinRoot = Require(values, "--twin-root"),
            TranslationsFile = Require(values, "--translations"),
            AnalyzedCommit = Require(values, "--analyzed-commit"),
            OutputFile = Require(values, "--output")
        };
    }

    private const string Usage =
        "Usage: ParityInventory sweep --portmap <portmap.json> --original-root <path> "
        + "--twin-root <path> --translations <English.xlf> --analyzed-commit <sha> "
        + "--output <functional-findings.json>";

    private static string Require(IReadOnlyDictionary<string, string> values, string name) =>
        values.TryGetValue(name, out string? value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException($"Missing {name}.{Environment.NewLine}{Usage}");
}
