using System.Text.Json;

namespace GitExtensions.ParityInventory;

// parity-scaffolding: Applies the functional inventory to every source mapping without hiding unsupported shapes.
internal static class InventorySweepRunner
{
    public static InventorySweepResult Run(SweepOptions options)
    {
        IReadOnlyList<PortMapMapping> mappings = ReadPortMap(options.PortMapFile);
        Dictionary<string, TypeWork> workByType = new(StringComparer.Ordinal);
        Dictionary<string, IReadOnlyList<string>> mappingTypes = new(StringComparer.Ordinal);
        foreach (PortMapMapping mapping in mappings)
        {
            string sourceFile = Path.GetFullPath(mapping.Source);
            IReadOnlyList<string> typeNames = SourceInventoryReader.DiscoverTopLevelClassNames(sourceFile);
            mappingTypes[mapping.Source] = typeNames;
            foreach (string typeName in typeNames)
            {
                if (!workByType.TryGetValue(typeName, out TypeWork? work))
                {
                    work = new TypeWork(typeName);
                    workByType.Add(typeName, work);
                }

                work.Mappings.Add(mapping);
                work.OriginalFiles.Add(sourceFile);
                if (File.Exists(mapping.Twin))
                {
                    work.TwinFiles.Add(Path.GetFullPath(mapping.Twin));
                }
            }
        }

        AddDiscoveredParts(options.OriginalRoot, "*.cs", workByType, isTwin: false);
        AddDiscoveredParts(options.TwinRoot, "*.cs", workByType, isTwin: true);
        AddDiscoveredParts(options.TwinRoot, "*.axaml", workByType, isTwin: true);

        string outputFile = Path.GetFullPath(options.OutputFile);
        string outputDirectory = Path.GetDirectoryName(outputFile)
            ?? throw new InvalidDataException($"Output file '{outputFile}' has no parent directory.");
        Directory.CreateDirectory(outputDirectory);
        Dictionary<string, SweepTypeResult> typeResults = new(StringComparer.Ordinal);
        Dictionary<string, InventoryReport> reports = new(StringComparer.Ordinal);
        foreach (TypeWork work in workByType.Values.OrderBy(item => item.TypeName, StringComparer.Ordinal))
        {
            if (work.TwinFiles.Count == 0 || work.Mappings.All(IsLinkedExact))
            {
                continue;
            }

            string className = work.TypeName[(work.TypeName.LastIndexOf('.') + 1)..];
            IReadOnlySet<string> englishKeys = EnglishCatalog.Read(options.TranslationsFile, className);
            try
            {
                SourceInventory original = SourceInventoryReader.ReadFiles(
                    options.OriginalRoot,
                    work.TypeName,
                    englishKeys,
                    isTwin: false,
                    work.OriginalFiles);
                SourceInventory twin = SourceInventoryReader.ReadFiles(
                    options.TwinRoot,
                    work.TypeName,
                    englishKeys,
                    isTwin: true,
                    work.TwinFiles);
                IReadOnlyList<FunctionalFinding> findings = InventoryComparer.Compare(original, twin);
                InventoryReport report = CreateReport(work.TypeName, original, twin, findings);
                string relativeReport = $"types/{Sanitize(work.TypeName)}.functional-findings.json";
                string reportFile = Path.Combine(
                    outputDirectory,
                    relativeReport.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(reportFile)!);
                File.WriteAllText(reportFile, InventoryJson.Serialize(report));
                reports.Add(work.TypeName, report);
                typeResults.Add(work.TypeName, new SweepTypeResult
                {
                    TypeName = work.TypeName,
                    Report = relativeReport,
                    OriginalPartCount = original.Parts.Count,
                    TwinPartCount = twin.Parts.Count,
                    FindingCount = findings.Count,
                    FindingsByCategory = report.Summary.FindingsByCategory
                });
            }
            catch (InvalidDataException)
            {
                // The per-mapping result below records this type as unsupported.
            }
        }

        List<SweepMappingResult> mappingResults = [];
        foreach (PortMapMapping mapping in mappings)
        {
            IReadOnlyList<string> typeNames = mappingTypes[mapping.Source];
            bool linkedExact = IsLinkedExact(mapping);
            SweepTypeResult[] analyzedTypes = typeNames
                .Where(typeResults.ContainsKey)
                .Select(typeName => typeResults[typeName])
                .ToArray();
            (string analysisStatus, string? note) = GetAnalysisStatus(
                mapping,
                typeNames,
                analyzedTypes,
                linkedExact);
            mappingResults.Add(new SweepMappingResult
            {
                Source = NormalizePath(mapping.Source),
                Twin = NormalizePath(mapping.Twin),
                PortMapStatus = mapping.Status,
                AnalysisStatus = analysisStatus,
                TypeNames = typeNames,
                FindingCount = analyzedTypes.Sum(type => type.FindingCount),
                Evidence = analyzedTypes.Select(type => type.Report).Order(StringComparer.Ordinal).ToArray(),
                Note = note
            });
        }

        FunctionalFinding[] allFindings = reports.Values.SelectMany(report => report.Findings).ToArray();
        InventorySweepResult result = new()
        {
            SchemaVersion = InventorySweepResult.CurrentSchemaVersion,
            AnalyzedCommit = options.AnalyzedCommit,
            PortMap = NormalizePath(options.PortMapFile),
            Summary = new InventorySweepSummary
            {
                MappingCount = mappings.Count,
                FormerParityCount = mappings.Count(mapping => mapping.Status == "parity"),
                LinkedExactCount = mappingResults.Count(mapping => mapping.AnalysisStatus == "linkedExact"),
                AnalyzedTypeCount = typeResults.Count,
                UnsupportedMappingCount = mappingResults.Count(mapping => mapping.AnalysisStatus == "unsupported"),
                FindingCount = allFindings.Length,
                FindingsByCategory = allFindings.GroupBy(finding => finding.Category, StringComparer.Ordinal)
                    .OrderBy(group => group.Key, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal)
            },
            Mappings = mappingResults.OrderBy(mapping => mapping.Source, StringComparer.Ordinal).ToArray(),
            Types = typeResults.Values.OrderBy(type => type.TypeName, StringComparer.Ordinal).ToArray(),
            OutputFile = outputFile
        };
        File.WriteAllText(outputFile, InventoryJson.Serialize(result));
        return result;
    }

    private static (string Status, string? Note) GetAnalysisStatus(
        PortMapMapping mapping,
        IReadOnlyList<string> typeNames,
        IReadOnlyList<SweepTypeResult> analyzedTypes,
        bool linkedExact)
    {
        if (!File.Exists(mapping.Source))
        {
            return ("unsupported", "Original source file is missing.");
        }

        if (linkedExact)
        {
            return ("linkedExact", "The twin compiles this exact original source through an MSBuild Link.");
        }

        if (!File.Exists(mapping.Twin))
        {
            return ("unsupported", "Twin path is not a physical file and is not recorded as linked unchanged.");
        }

        if (typeNames.Count == 0)
        {
            return ("unsupported", "P0.4 inventories classes; this mapping declares no top-level class.");
        }

        if (analyzedTypes.Count != typeNames.Count)
        {
            return ("unsupported", "At least one original class could not be resolved in the mapped twin files.");
        }

        return ("analyzed", null);
    }

    private static InventoryReport CreateReport(
        string typeName,
        SourceInventory original,
        SourceInventory twin,
        IReadOnlyList<FunctionalFinding> findings) =>
        new()
        {
            SchemaVersion = InventoryReport.CurrentSchemaVersion,
            TypeName = typeName,
            Original = original,
            Twin = twin,
            Summary = new InventorySummary
            {
                FindingCount = findings.Count,
                FindingsByCategory = findings.GroupBy(finding => finding.Category, StringComparer.Ordinal)
                    .OrderBy(group => group.Key, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal)
            },
            Findings = findings
        };

    private static IReadOnlyList<PortMapMapping> ReadPortMap(string path)
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        List<PortMapMapping> mappings = [];
        foreach (JsonProperty property in document.RootElement.EnumerateObject())
        {
            if (property.NameEquals("//"))
            {
                continue;
            }

            JsonElement value = property.Value;
            string status = value.GetProperty("status").GetString()
                ?? throw new InvalidDataException($"Portmap entry '{property.Name}' has no status.");
            if (status is "unported" or "windowsOnly")
            {
                continue;
            }

            mappings.Add(new PortMapMapping(
                property.Name,
                value.GetProperty("twin").GetString()
                    ?? throw new InvalidDataException($"Portmap entry '{property.Name}' has no twin."),
                status,
                value.TryGetProperty("notes", out JsonElement notes) ? notes.GetString() : null));
        }

        return mappings.OrderBy(mapping => mapping.Source, StringComparer.Ordinal).ToArray();
    }

    private static bool IsLinkedExact(PortMapMapping mapping) =>
        Path.GetFullPath(mapping.Source).Equals(
            Path.GetFullPath(mapping.Twin),
            StringComparison.OrdinalIgnoreCase)
        || (!File.Exists(mapping.Twin)
            && mapping.Notes?.Contains("Linked unchanged", StringComparison.OrdinalIgnoreCase) == true);

    private static void AddDiscoveredParts(
        string root,
        string pattern,
        IReadOnlyDictionary<string, TypeWork> workByType,
        bool isTwin)
    {
        foreach (string file in Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories))
        {
            foreach (string typeName in SourceInventoryReader.DiscoverTopLevelClassNames(file))
            {
                if (!workByType.TryGetValue(typeName, out TypeWork? work))
                {
                    continue;
                }

                if (isTwin)
                {
                    work.TwinFiles.Add(Path.GetFullPath(file));
                }
                else
                {
                    work.OriginalFiles.Add(Path.GetFullPath(file));
                }
            }
        }
    }

    private static string Sanitize(string value) =>
        string.Concat(value.Select(character => char.IsLetterOrDigit(character) || character is '.' or '-'
            ? character
            : '_'));

    private static string NormalizePath(string path) => path.Replace('\\', '/');

    // parity-scaffolding: Accumulates the mapped partial files for one class-level comparison.
    private sealed class TypeWork(string typeName)
    {
        public string TypeName { get; } = typeName;

        public List<PortMapMapping> Mappings { get; } = [];

        public HashSet<string> OriginalFiles { get; } = new(StringComparer.Ordinal);

        public HashSet<string> TwinFiles { get; } = new(StringComparer.Ordinal);
    }

    // parity-scaffolding: Represents the subset of portmap data required by the source sweep.
    private sealed record PortMapMapping(
        string Source,
        string Twin,
        string Status,
        string? Notes);
}
