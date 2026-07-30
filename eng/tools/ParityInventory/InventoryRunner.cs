using System.Text.Json;
using System.Text.Json.Serialization;

namespace GitExtensions.ParityInventory;

// parity-scaffolding: Runs source extraction, comparison, and deterministic report emission.
internal static class InventoryRunner
{
    public static InventoryReport Run(InventoryOptions options)
    {
        string className = options.TypeName[(options.TypeName.LastIndexOf('.') + 1)..];
        IReadOnlySet<string> englishKeys = EnglishCatalog.Read(options.TranslationsFile, className);
        SourceInventory original = SourceInventoryReader.Read(
            options.OriginalRoot,
            options.TypeName,
            englishKeys,
            isTwin: false);
        SourceInventory twin = SourceInventoryReader.Read(
            options.TwinRoot,
            options.TypeName,
            englishKeys,
            isTwin: true);
        InventoryComparison comparison = InventoryComparer.Compare(original, twin);
        IReadOnlyList<FunctionalFinding> findings = comparison.Findings;
        InventoryReport report = new()
        {
            SchemaVersion = InventoryReport.CurrentSchemaVersion,
            TypeName = options.TypeName,
            Original = original,
            Twin = twin,
            Summary = new InventorySummary
            {
                FindingCount = findings.Count,
                FindingsByCategory = findings
                    .GroupBy(finding => finding.Category, StringComparer.Ordinal)
                    .OrderBy(group => group.Key, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal),
                AdaptedCommentCount = comparison.AdaptedComments.Count
            },
            Findings = findings,
            AdaptedComments = comparison.AdaptedComments
        };

        string output = Path.GetFullPath(options.OutputFile);
        Directory.CreateDirectory(
            Path.GetDirectoryName(output)
            ?? throw new InvalidDataException($"Output file '{output}' has no parent directory."));
        File.WriteAllText(output, InventoryJson.Serialize(report));
        return report;
    }
}
