namespace GitExtensions.ParityInventory;

// parity-scaffolding: Defines the deterministic ledger-generation boundary for the P0 baseline.
internal sealed record LedgerOptions
{
    public required string PortMapFile { get; init; }

    public required string FunctionalReport { get; init; }

    public required string VisualReport { get; init; }

    public required string ReferenceManifest { get; init; }

    public required string AnalyzedCommit { get; init; }

    public required string VerifiedOn { get; init; }

    public required string LedgerOutput { get; init; }

    public required string BaselineOutput { get; init; }

    public static LedgerOptions Parse(string[] args)
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

        return new LedgerOptions
        {
            PortMapFile = Require(values, "--portmap"),
            FunctionalReport = Require(values, "--functional"),
            VisualReport = Require(values, "--visual"),
            ReferenceManifest = Require(values, "--reference"),
            AnalyzedCommit = Require(values, "--analyzed-commit"),
            VerifiedOn = Require(values, "--verified-on"),
            LedgerOutput = Require(values, "--ledger-output"),
            BaselineOutput = Require(values, "--baseline-output")
        };
    }

    private const string Usage =
        "Usage: ParityInventory ledger --portmap <portmap.json> --functional <functional-findings.json> "
        + "--visual <findings.json> --reference <P0.1 manifest.json> "
        + "--analyzed-commit <sha> --verified-on <yyyy-MM-dd> "
        + "--ledger-output <parity-ledger.json> --baseline-output <baseline-report.json>";

    private static string Require(IReadOnlyDictionary<string, string> values, string name) =>
        values.TryGetValue(name, out string? value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException($"Missing {name}.{Environment.NewLine}{Usage}");
}
