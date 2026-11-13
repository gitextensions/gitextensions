using System.Text.Json;
using GitExtensions.ParityCapture;

namespace GitExtensions.ParityDiff;

// parity-scaffolding: Compares only explicitly declared, framework-neutral resolved-color roles.
internal static class ColorRoleRunner
{
    public static ColorRoleResult Run(ColorRoleOptions options)
    {
        ColorRoleCatalog catalog = ColorRoleCatalog.Load(options.RoleCatalog);
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
        List<ColorRoleCaptureComparison> comparisons = [];
        foreach (CaptureKey key in keys)
        {
            referenceEntries.TryGetValue(key, out CaptureManifestEntry? referenceEntry);
            candidateEntries.TryGetValue(key, out CaptureManifestEntry? candidateEntry);
            comparisons.Add(Compare(
                key,
                referenceEntry,
                candidateEntry,
                referenceDirectory,
                candidateDirectory,
                catalog.Roles));
        }

        ColorRoleResult result = new()
        {
            SchemaVersion = ColorRoleResult.CurrentSchemaVersion,
            ReferenceManifest = NormalizePath(options.ReferenceManifest),
            CandidateManifest = NormalizePath(options.CandidateManifest),
            RoleCatalog = NormalizePath(options.RoleCatalog),
            Roles = catalog.Roles,
            Summary = new ColorRoleSummary
            {
                RequestCount = keys.Length,
                ComparedCaptureCount = comparisons.Count(comparison => comparison.Status == "compared"),
                UnavailableCaptureCount = comparisons.Count(comparison => comparison.Status != "compared"),
                DeclaredRoleCount = catalog.Roles.Count,
                RoleComparisonCount = comparisons.Sum(comparison => comparison.RoleComparisonCount),
                MatchCount = comparisons.Sum(comparison => comparison.MatchCount),
                FindingCount = comparisons.Sum(comparison => comparison.Findings.Count)
            },
            Captures = comparisons
        };

        Directory.CreateDirectory(options.OutputDirectory);
        string json = JsonSerializer.Serialize(result, JsonDefaults.WriteOptions) + Environment.NewLine;
        File.WriteAllText(Path.Combine(options.OutputDirectory, "color-findings.json"), json);
        File.WriteAllText(Path.Combine(options.OutputDirectory, "color-report.md"), ColorRoleReportWriter.Write(result));
        return result;
    }

    private static ColorRoleCaptureComparison Compare(
        CaptureKey key,
        CaptureManifestEntry? referenceEntry,
        CaptureManifestEntry? candidateEntry,
        string referenceDirectory,
        string candidateDirectory,
        IReadOnlyList<ColorRoleDefinition> roles)
    {
        if (referenceEntry?.Status != CaptureStateStatus.Captured
            || candidateEntry?.Status != CaptureStateStatus.Captured)
        {
            return new ColorRoleCaptureComparison
            {
                Key = key,
                Status = "unavailable",
                ReferenceStatus = referenceEntry?.Status,
                CandidateStatus = candidateEntry?.Status,
                ReferenceNote = referenceEntry?.Note,
                CandidateNote = candidateEntry?.Note,
                RoleComparisonCount = 0,
                MatchCount = 0,
                Findings = []
            };
        }

        CaptureDocument reference = LoadDocument(referenceDirectory, referenceEntry);
        CaptureDocument candidate = LoadDocument(candidateDirectory, candidateEntry);
        IReadOnlyDictionary<string, IReadOnlyList<string>> referenceRoles = ExtractRoles(reference);
        IReadOnlyDictionary<string, IReadOnlyList<string>> candidateRoles = ExtractRoles(candidate);
        Dictionary<string, ColorRoleDefinition> definitions = roles.ToDictionary(role => role.Id, StringComparer.Ordinal);
        List<ColorRoleFinding> findings = [];
        int comparisonCount = 0;
        int matchCount = 0;

        foreach (string undeclared in referenceRoles.Keys.Concat(candidateRoles.Keys)
                     .Distinct(StringComparer.Ordinal)
                     .Except(definitions.Keys, StringComparer.Ordinal)
                     .Order(StringComparer.Ordinal))
        {
            findings.Add(new ColorRoleFinding
            {
                Code = "color.roleUndeclared",
                Role = undeclared,
                Message = "A capture emitted a semantic color role with no framework-neutral catalog definition.",
                ReferenceValue = JoinValues(referenceRoles, undeclared),
                CandidateValue = JoinValues(candidateRoles, undeclared)
            });
        }

        foreach (ColorRoleDefinition role in roles)
        {
            referenceRoles.TryGetValue(role.Id, out IReadOnlyList<string>? referenceValues);
            candidateRoles.TryGetValue(role.Id, out IReadOnlyList<string>? candidateValues);
            if (referenceValues is null || candidateValues is null)
            {
                findings.Add(new ColorRoleFinding
                {
                    Code = "color.roleMissing",
                    Role = role.Id,
                    Meaning = role.Meaning,
                    Message = "The declared semantic color role was not emitted by both captures.",
                    ReferenceValue = JoinValues(referenceValues),
                    CandidateValue = JoinValues(candidateValues)
                });
                continue;
            }

            if (referenceValues.Count != 1 || candidateValues.Count != 1)
            {
                findings.Add(new ColorRoleFinding
                {
                    Code = "color.roleAmbiguous",
                    Role = role.Id,
                    Meaning = role.Meaning,
                    Message = "A semantic color role resolved to more than one ARGB value in one capture.",
                    ReferenceValue = JoinValues(referenceValues),
                    CandidateValue = JoinValues(candidateValues)
                });
                continue;
            }

            comparisonCount++;
            if (string.Equals(referenceValues[0], candidateValues[0], StringComparison.Ordinal))
            {
                matchCount++;
                continue;
            }

            findings.Add(new ColorRoleFinding
            {
                Code = "color.roleMismatch",
                Role = role.Id,
                Meaning = role.Meaning,
                Message = "The semantic color role resolved to different ARGB values.",
                ReferenceValue = referenceValues[0],
                CandidateValue = candidateValues[0]
            });
        }

        return new ColorRoleCaptureComparison
        {
            Key = key,
            Status = "compared",
            ReferenceStatus = referenceEntry.Status,
            CandidateStatus = candidateEntry.Status,
            RoleComparisonCount = comparisonCount,
            MatchCount = matchCount,
            Findings = findings
        };
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> ExtractRoles(CaptureDocument document)
    {
        Dictionary<string, HashSet<string>> roles = new(StringComparer.Ordinal);
        foreach (CaptureSurface surface in document.Surfaces)
        {
            Visit(surface.Root);
        }

        return roles.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<string>)pair.Value.Order(StringComparer.Ordinal).ToArray(),
            StringComparer.Ordinal);

        void Visit(CaptureNode node)
        {
            foreach ((string name, string value) in node.Colors.Additional.Where(
                         pair => pair.Key.StartsWith("semantic.", StringComparison.Ordinal)))
            {
                if (!roles.TryGetValue(name, out HashSet<string>? values))
                {
                    values = new HashSet<string>(StringComparer.Ordinal);
                    roles.Add(name, values);
                }

                values.Add(value);
            }

            foreach (CaptureColumn column in node.Columns)
            {
                foreach ((string name, string value) in column.Colors.Additional.Where(
                             pair => pair.Key.StartsWith("semantic.", StringComparison.Ordinal)))
                {
                    if (!roles.TryGetValue(name, out HashSet<string>? values))
                    {
                        values = new HashSet<string>(StringComparer.Ordinal);
                        roles.Add(name, values);
                    }

                    values.Add(value);
                }
            }

            foreach (CaptureNode child in node.Children)
            {
                Visit(child);
            }
        }
    }

    private static CaptureDocument LoadDocument(string directory, CaptureManifestEntry entry)
    {
        if (entry.TreeFile is null)
        {
            throw new InvalidDataException($"Captured manifest entry '{entry.ComponentType}' has no tree file.");
        }

        string path = Path.GetFullPath(Path.Combine(directory, entry.TreeFile));
        return CaptureJson.Deserialize(File.ReadAllText(path));
    }

    private static string? JoinValues(
        IReadOnlyDictionary<string, IReadOnlyList<string>> roles,
        string name) =>
        roles.TryGetValue(name, out IReadOnlyList<string>? values) ? JoinValues(values) : null;

    private static string? JoinValues(IReadOnlyList<string>? values) =>
        values is null ? null : string.Join(",", values);

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
