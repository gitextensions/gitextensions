namespace GitExtensions.ParityDiff;

// parity-scaffolding: Configures strict resolved-color role comparison across capture frameworks.
internal sealed record ColorRoleOptions
{
    public required string ReferenceManifest { get; init; }

    public required string CandidateManifest { get; init; }

    public required string RoleCatalog { get; init; }

    public required string OutputDirectory { get; init; }

    public static ColorRoleOptions Parse(IReadOnlyList<string> args)
    {
        if (args.Count == 0 || !string.Equals(args[0], "compare-colors", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Usage: ParityDiff compare-colors --reference <manifest> --candidate <manifest> "
                + "--roles <color-roles.json> --output <directory>");
        }

        Dictionary<string, string> values = new(StringComparer.Ordinal);
        for (int index = 1; index < args.Count; index += 2)
        {
            if (index + 1 >= args.Count || !args[index].StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Missing value for option '{args[index]}'.");
            }

            values.Add(args[index], args[index + 1]);
        }

        return new ColorRoleOptions
        {
            ReferenceManifest = GetRequired("--reference"),
            CandidateManifest = GetRequired("--candidate"),
            RoleCatalog = GetRequired("--roles"),
            OutputDirectory = GetRequired("--output")
        };

        string GetRequired(string name) =>
            values.TryGetValue(name, out string? value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : throw new ArgumentException($"Required option '{name}' was not supplied.");
    }
}
