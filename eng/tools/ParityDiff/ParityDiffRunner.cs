using System.Text.Json;
using GitExtensions.ParityCapture;

namespace GitExtensions.ParityDiff;

// parity-scaffolding: Runs the temporary capture-comparison pipeline.
internal static class ParityDiffRunner
{
    public static ParityDiffResult Run(DiffOptions options)
    {
        DiffConfiguration configuration = DiffConfiguration.Load(options.ConfigurationFile);
        CaptureSetManifest reference = LoadManifest(options.ReferenceManifest);
        CaptureSetManifest candidate = LoadManifest(options.CandidateManifest);
        Dictionary<CaptureKey, CaptureManifestEntry> referenceEntries = Index(reference);
        Dictionary<CaptureKey, CaptureManifestEntry> candidateEntries = Index(candidate);
        string referenceDirectory = GetManifestDirectory(options.ReferenceManifest);
        string candidateDirectory = GetManifestDirectory(options.CandidateManifest);

        CaptureKey[] keys = referenceEntries.Keys
            .Concat(candidateEntries.Keys)
            .Distinct()
            .Order()
            .ToArray();
        List<CaptureComparison> comparisons = [];
        foreach (CaptureKey key in keys)
        {
            referenceEntries.TryGetValue(key, out CaptureManifestEntry? referenceEntry);
            candidateEntries.TryGetValue(key, out CaptureManifestEntry? candidateEntry);
            DiffTolerance tolerance = configuration.GetTolerance(key.ComponentType);
            comparisons.Add(CaptureComparer.Compare(
                key,
                referenceEntry,
                candidateEntry,
                referenceDirectory,
                candidateDirectory,
                tolerance));
        }

        IReadOnlyList<ParityFinding> findings = comparisons.SelectMany(comparison => comparison.Findings).ToArray();
        ParityDiffResult result = new()
        {
            SchemaVersion = ParityDiffResult.CurrentSchemaVersion,
            ReferenceManifest = NormalizePath(options.ReferenceManifest),
            CandidateManifest = NormalizePath(options.CandidateManifest),
            Summary = new ParityDiffSummary
            {
                RequestCount = keys.Length,
                ComparedCaptureCount = comparisons.Count(comparison => comparison.Status == "compared"),
                UnavailableCaptureCount = comparisons.Count(comparison => comparison.Status != "compared"),
                FindingCount = findings.Count,
                FindingsByCategory = findings
                    .GroupBy(finding => finding.Category, StringComparer.Ordinal)
                    .OrderBy(group => group.Key, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal)
            },
            Captures = comparisons
        };

        Directory.CreateDirectory(options.OutputDirectory);
        string json = JsonSerializer.Serialize(result, JsonDefaults.WriteOptions) + Environment.NewLine;
        File.WriteAllText(Path.Combine(options.OutputDirectory, "findings.json"), json);
        File.WriteAllText(Path.Combine(options.OutputDirectory, "report.md"), HumanReportWriter.Write(result));
        return result;
    }

    private static string GetManifestDirectory(string manifestPath) =>
        Path.GetDirectoryName(Path.GetFullPath(manifestPath))
        ?? throw new InvalidDataException($"Manifest '{manifestPath}' does not have a parent directory.");

    private static Dictionary<CaptureKey, CaptureManifestEntry> Index(CaptureSetManifest manifest)
    {
        if (manifest.SchemaVersion != CaptureDocument.CurrentSchemaVersion)
        {
            throw new InvalidDataException($"Unsupported manifest schema version {manifest.SchemaVersion}.");
        }

        Dictionary<CaptureKey, CaptureManifestEntry> entries = [];
        foreach (CaptureManifestEntry entry in manifest.Captures)
        {
            CaptureKey key = new()
            {
                ComponentType = entry.ComponentType,
                ThemeId = entry.ThemeId,
                ScalePercent = entry.ScalePercent,
                State = entry.State
            };
            if (!entries.TryAdd(key, entry))
            {
                throw new InvalidDataException($"Manifest contains duplicate capture key '{key}'.");
            }
        }

        return entries;
    }

    private static CaptureSetManifest LoadManifest(string path) =>
        JsonSerializer.Deserialize<CaptureSetManifest>(File.ReadAllText(path), JsonDefaults.ReadOptions)
        ?? throw new InvalidDataException($"Manifest '{path}' is empty.");

    private static string NormalizePath(string path) => path.Replace(Path.DirectorySeparatorChar, '/');
}
