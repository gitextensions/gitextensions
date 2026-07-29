namespace GitExtensions.ParityInventory;

// parity-scaffolding: Defines the command-line boundary for temporary source inventory runs.
internal sealed record InventoryOptions
{
    public required string OriginalRoot { get; init; }

    public required string TwinRoot { get; init; }

    public required string TypeName { get; init; }

    public required string TranslationsFile { get; init; }

    public required string OutputFile { get; init; }

    public static InventoryOptions Parse(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help")
        {
            throw new ArgumentException(Usage);
        }

        int offset = args[0].Equals("compare", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        Dictionary<string, string> values = new(StringComparer.Ordinal);
        for (int index = offset; index < args.Length; index += 2)
        {
            if (index + 1 >= args.Length || !args[index].StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException(Usage);
            }

            values[args[index]] = args[index + 1];
        }

        return new InventoryOptions
        {
            OriginalRoot = Require(values, "--original-root"),
            TwinRoot = Require(values, "--twin-root"),
            TypeName = Require(values, "--type"),
            TranslationsFile = Require(values, "--translations"),
            OutputFile = Require(values, "--output")
        };
    }

    private const string Usage =
        "Usage: ParityInventory compare --original-root <path> --twin-root <path> "
        + "--type <namespace.class> --translations <English.xlf> --output <functional-findings.json>";

    private static string Require(IReadOnlyDictionary<string, string> values, string name) =>
        values.TryGetValue(name, out string? value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException($"Missing {name}.{Environment.NewLine}{Usage}");
}
