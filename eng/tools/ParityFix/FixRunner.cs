using System.Text;
using System.Text.Json;
using GitExtensions.ParityInventory;

namespace GitExtensions.ParityFix;

// parity-scaffolding: Consumes reviewed type reports, rejects stale input, and writes only proved twin files.
internal static class FixRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static FixRun Run(string inventoryDirectory, IReadOnlySet<string> selectedTypes, string twinRoot,
        bool apply = false, bool allowInitializers = false)
    {
        InventoryReport[] reports = Directory.EnumerateFiles(inventoryDirectory, "*.json")
            .Order(StringComparer.Ordinal)
            .Select(path => JsonSerializer.Deserialize<InventoryReport>(File.ReadAllText(path), JsonOptions)
                ?? throw new InvalidDataException($"Empty inventory: {path}"))
            .Where(report => selectedTypes.Contains(report.TypeName)).ToArray();
        if (reports.Length == 0)
        {
            throw new InvalidDataException("No inventory reports matched the selected types.");
        }

        twinRoot = Path.GetFullPath(twinRoot);

        // Validate every snapshot before proposing or applying any file, including files shared by types.
        foreach (InventoryReport report in reports)
        {
            bool withinExplicitRoot = report.Twin.Parts.Count > 0 && report.Twin.Parts.All(part =>
                IsWithin(twinRoot, Path.GetFullPath(Path.Combine(report.Twin.Root, part.Path))));
            if (report.SchemaVersion != InventoryReport.CurrentSchemaVersion
                || (Path.GetFullPath(report.Twin.Root) != twinRoot && !withinExplicitRoot))
            {
                throw new InvalidDataException($"Unsupported schema or twin root for {report.TypeName}.");
            }

            ValidateSnapshot(report.Original, report.TypeName, false);
            ValidateSnapshot(report.Twin, report.TypeName, true);
        }

        Dictionary<string, string> texts = new(StringComparer.Ordinal);
        HashSet<string> failedPaths = new(StringComparer.Ordinal);
        List<FileFix> results = [];
        int selectedFindings = 0;
        int commentFindings = 0;
        foreach (InventoryReport report in reports)
        {
            FunctionalFinding[] mechanical = report.Findings.Where(finding => finding.Code is
                "comment.missing" or "comment.changed" or "comment.drifted" or "member.order").ToArray();
            selectedFindings += mechanical.Length;
            commentFindings += mechanical.Count(finding => finding.Code.StartsWith("comment.", StringComparison.Ordinal));
            foreach (SourcePart part in report.Original.Parts)
            {
                string path = Path.GetFullPath(Path.Combine(report.Twin.Root, part.ExpectedTwinPath ?? part.Path));
                if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || !File.Exists(path)
                    || !IsWithin(twinRoot, path))
                {
                    continue;
                }

                string originalPath = Path.GetFullPath(Path.Combine(report.Original.Root, part.Path));
                if (originalPath == path)
                {
                    throw new InvalidDataException("The original file cannot be a write target.");
                }

                FunctionalFinding[] applicable = mechanical.Where(finding => finding.Code == "member.order"
                    || finding.Path.StartsWith($"comment/{part.Path}/", StringComparison.Ordinal)).ToArray();
                if (applicable.Length == 0 || failedPaths.Contains(path))
                {
                    continue;
                }

                string text = texts.GetValueOrDefault(path) ?? File.ReadAllText(path);
                FileFix result = MechanicalFixer.Fix(path, File.ReadAllText(originalPath), text, report.TypeName,
                    applicable, report.Original.Comments.Where(comment => comment.Part == part.Path).ToArray(),
                    report.Twin.Comments, allowInitializers);
                if (!result.ProofPassed)
                {
                    failedPaths.Add(path);
                    results.RemoveAll(previous => previous.Path == path);
                    result = result with { Text = File.ReadAllText(path) };
                }

                results.Add(result);
                texts[path] = result.Text;
            }
        }

        if (apply)
        {
            foreach ((string path, string text) in texts)
            {
                if (File.ReadAllText(path) != text)
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    bool bom = bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble);
                    File.WriteAllText(path, text, new UTF8Encoding(bom));
                }
            }
        }

        // Relative-order findings are measured by a fresh sweep, not inferred from the number of moved nodes.
        return new FixRun(results, selectedFindings, commentFindings, results.Sum(result => result.Unresolved.Count));
    }

    internal static bool IsWithin(string root, string path) =>
        !Path.IsPathRooted(Path.GetRelativePath(root, path))
        && !Path.GetRelativePath(root, path).Split(Path.DirectorySeparatorChar).Contains("..");

    private static void ValidateSnapshot(SourceInventory snapshot, string typeName, bool isTwin)
    {
        SourceInventory current = SourceInventoryReader.ReadFiles(snapshot.Root, typeName, new HashSet<string>(), isTwin,
            snapshot.Parts.Select(part => Path.Combine(snapshot.Root, part.Path)));
        if (InventoryJson.Serialize(snapshot.Members) != InventoryJson.Serialize(current.Members)
            || InventoryJson.Serialize(snapshot.Comments) != InventoryJson.Serialize(current.Comments))
        {
            throw new InvalidDataException($"Stale inventory for {typeName} below {snapshot.Root}; run a reviewed sweep first.");
        }
    }
}
