using System.Text.Json;

namespace GitExtensions.ParityInventory;

// parity-scaffolding: Joins measured source and visual coverage into the initial six-axis ledger.
internal static class LedgerRunner
{
    private static readonly string[] AxisOrder =
    [
        "structural",
        "functional",
        "visual",
        "themingAndColor",
        "behavioralState",
        "platform"
    ];

    public static ParityLedger Run(LedgerOptions options)
    {
        using JsonDocument functionalDocument = JsonDocument.Parse(File.ReadAllText(options.FunctionalReport));
        using JsonDocument visualDocument = JsonDocument.Parse(File.ReadAllText(options.VisualReport));
        using JsonDocument referenceDocument = JsonDocument.Parse(File.ReadAllText(options.ReferenceManifest));
        JsonElement functionalRoot = functionalDocument.RootElement;
        JsonElement visualRoot = visualDocument.RootElement;
        Dictionary<string, JsonElement> typeResults = functionalRoot.GetProperty("types")
            .EnumerateArray()
            .ToDictionary(
                item => item.GetProperty("typeName").GetString()!,
                item => item.Clone(),
                StringComparer.Ordinal);
        Dictionary<string, VisualTypeSummary> visualTypes = ReadVisualTypes(visualRoot);
        Dictionary<string, ReferenceTypeSummary> referenceTypes = ReadReferenceTypes(referenceDocument.RootElement);
        string functionalEvidence = NormalizePath(options.FunctionalReport);
        string visualEvidence = NormalizePath(options.VisualReport);
        string referenceEvidence = NormalizePath(options.ReferenceManifest);
        List<LedgerComponent> components = [];
        foreach (JsonElement mapping in functionalRoot.GetProperty("mappings").EnumerateArray())
        {
            string source = mapping.GetProperty("source").GetString()!;
            string twin = mapping.GetProperty("twin").GetString()!;
            string analysisStatus = mapping.GetProperty("analysisStatus").GetString()!;
            string[] typeNames = mapping.GetProperty("typeNames").EnumerateArray()
                .Select(item => item.GetString()!)
                .ToArray();
            Dictionary<string, int> functionalCategories = SumCategories(typeNames, typeResults);
            int structureFindings = GetCount(functionalCategories, "structure")
                + GetCount(functionalCategories, "members");
            int functionalFindings = functionalCategories
                .Where(pair => pair.Key is not "structure")
                .Sum(pair => pair.Value);
            VisualTypeSummary[] mappedVisual = typeNames.Where(visualTypes.ContainsKey)
                .Select(typeName => visualTypes[typeName])
                .ToArray();
            ReferenceTypeSummary[] mappedReference = typeNames.Where(referenceTypes.ContainsKey)
                .Select(typeName => referenceTypes[typeName])
                .ToArray();
            bool isView = twin.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase)
                || twin.EndsWith(".axaml.cs", StringComparison.OrdinalIgnoreCase);
            AxisEvidence structural = CreateStructural(
                options,
                functionalEvidence,
                analysisStatus,
                structureFindings);
            AxisEvidence functional = CreateFunctional(
                options,
                functionalEvidence,
                analysisStatus,
                functionalFindings);
            AxisEvidence visual = CreateVisual(
                options,
                visualEvidence,
                referenceEvidence,
                mappedVisual,
                mappedReference,
                isView,
                category: null,
                "visual layout has not been captured for this mapped view");
            AxisEvidence theming = CreateVisual(
                options,
                visualEvidence,
                referenceEvidence,
                mappedVisual,
                mappedReference,
                isView,
                category: "color",
                "theme/color behavior has not been captured for this mapped view");
            AxisEvidence behavioral = CreateVisual(
                options,
                visualEvidence,
                referenceEvidence,
                mappedVisual,
                mappedReference,
                isView: true,
                category: "state",
                "the complete interaction state matrix has not been exercised");
            AxisEvidence platform = NewAxis(
                options,
                "unverified",
                [],
                [],
                0,
                "Windows and WSL headless build gates do not prove Windows, Wayland, X11, Flatpak, and macOS runtime parity.");
            LedgerAxes axes = new()
            {
                Structural = structural,
                Functional = functional,
                Visual = visual,
                ThemingAndColor = theming,
                BehavioralState = behavioral,
                Platform = platform
            };
            components.Add(new LedgerComponent
            {
                Source = source,
                Twin = twin,
                Complete = IsComplete(axes),
                Axes = axes
            });
        }

        ParityLedger ledger = new()
        {
            Schema = "parity-ledger.schema.json",
            SchemaVersion = ParityLedger.CurrentSchemaVersion,
            AnalyzedCommit = options.AnalyzedCommit,
            AxisOrder = AxisOrder,
            Components = components.OrderBy(component => component.Source, StringComparer.Ordinal).ToArray()
        };
        string ledgerOutput = Path.GetFullPath(options.LedgerOutput);
        Directory.CreateDirectory(Path.GetDirectoryName(ledgerOutput)!);
        File.WriteAllText(ledgerOutput, InventoryJson.Serialize(ledger));

        JsonElement functionalSummary = functionalRoot.GetProperty("summary");
        JsonElement visualSummary = visualRoot.GetProperty("summary");
        BaselineReport baseline = new()
        {
            SchemaVersion = 1,
            AnalyzedCommit = options.AnalyzedCommit,
            VerifiedOn = options.VerifiedOn,
            Inputs = new BaselineInputs
            {
                PortMap = NormalizePath(options.PortMapFile),
                FunctionalReport = functionalEvidence,
                VisualReport = visualEvidence,
                ReferenceManifest = referenceEvidence,
                PairedReferenceManifest = visualRoot.GetProperty("referenceManifest").GetString()!,
                PairedCandidateManifest = visualRoot.GetProperty("candidateManifest").GetString()!,
                Ledger = NormalizePath(options.LedgerOutput)
            },
            Summary = new BaselineSummary
            {
                MappingCount = components.Count,
                FormerParityCount = functionalSummary.GetProperty("formerParityCount").GetInt32(),
                CompleteLedgerEntryCount = components.Count(component => component.Complete),
                FunctionalTypeCount = functionalSummary.GetProperty("analyzedTypeCount").GetInt32(),
                FunctionalFindingCount = functionalSummary.GetProperty("findingCount").GetInt32(),
                UnsupportedFunctionalMappingCount =
                    functionalSummary.GetProperty("unsupportedMappingCount").GetInt32(),
                ReferenceRequestCount = referenceTypes.Values.Sum(type => type.RequestCount),
                CapturedReferenceCount = referenceTypes.Values.Sum(type => type.CapturedCount),
                UnsupportedReferenceCount = referenceTypes.Values.Sum(type => type.UnsupportedCount),
                ReferenceTypeCount = referenceTypes.Count,
                VisualRequestCount = visualSummary.GetProperty("requestCount").GetInt32(),
                ComparedVisualCaptureCount = visualSummary.GetProperty("comparedCaptureCount").GetInt32(),
                UnavailableVisualCaptureCount = visualSummary.GetProperty("unavailableCaptureCount").GetInt32(),
                VisualFindingCount = visualSummary.GetProperty("findingCount").GetInt32(),
                VisuallyComparedTypeCount = visualTypes.Count
            },
            CoverageGaps =
            [
                "Strict paired visual evidence currently covers only GitUI.ScriptsEngine.FormFilePrompt.",
                "The 106-view Avalonia inventory is broader than the WinForms state-plan coverage; uncaptured views remain unverified.",
                "P0.4 is class-oriented; non-class and non-physical mappings remain explicit unsupported inventory entries.",
                "WSL headless regression builds do not satisfy Wayland, X11, Flatpak, or macOS platform-axis verification."
            ]
        };
        string baselineOutput = Path.GetFullPath(options.BaselineOutput);
        Directory.CreateDirectory(Path.GetDirectoryName(baselineOutput)!);
        File.WriteAllText(baselineOutput, InventoryJson.Serialize(baseline));
        return ledger;
    }

    private static AxisEvidence CreateStructural(
        LedgerOptions options,
        string evidence,
        string analysisStatus,
        int findings) =>
        analysisStatus switch
        {
            "linkedExact" => NewAxis(options, "verified", [evidence], [], 0,
                "The exact original source is compiled into the twin through an MSBuild Link."),
            "analyzed" when findings > 0 => NewAxis(options, "gap", [evidence], [], findings,
                "The P0.4 source inventory found structural/member differences."),
            "analyzed" => NewAxis(options, "verified", [evidence], [], 0,
                "The P0.4 inventory found no structural/member differences in its measured surface."),
            _ => NewAxis(options, "unverified", [evidence], [], 0,
                "This mapping could not be represented by the class-oriented P0.4 inventory.")
        };

    private static AxisEvidence CreateFunctional(
        LedgerOptions options,
        string evidence,
        string analysisStatus,
        int findings) =>
        analysisStatus switch
        {
            "linkedExact" => NewAxis(options, "verified", [evidence], [], 0,
                "The exact original implementation is compiled into the twin."),
            "analyzed" when findings > 0 => NewAxis(options, "gap", [evidence], [], findings,
                "The P0.4 functional inventory found member/event/menu/hotkey/settings/translation differences."),
            "analyzed" => NewAxis(options, "partial", [evidence], [], 0,
                "Static inventory is clean, but runtime success/error/async behavior is not fully exercised."),
            _ => NewAxis(options, "unverified", [evidence], [], 0,
                "This mapping is outside the class-oriented P0.4 inventory.")
        };

    private static AxisEvidence CreateVisual(
        LedgerOptions options,
        string evidence,
        string referenceEvidence,
        IReadOnlyList<VisualTypeSummary> visualTypes,
        IReadOnlyList<ReferenceTypeSummary> referenceTypes,
        bool isView,
        string? category,
        string missingNote)
    {
        if (visualTypes.Count == 0)
        {
            if (referenceTypes.Count > 0)
            {
                return NewAxis(
                    options,
                    "partial",
                    [referenceEvidence],
                    ["windows"],
                    referenceTypes.Sum(type => type.UnsupportedCount),
                    "A WinForms reference matrix exists, but no paired Avalonia comparison proves this axis.");
            }

            return isView
                ? NewAxis(options, "unverified", [], [], 0, missingNote)
                : NewAxis(options, "notApplicable", [], [], 0, "This source mapping does not declare UI chrome.");
        }

        int findings = category is null
            ? visualTypes.Sum(type => type.FindingCount)
            : visualTypes.Sum(type => GetCount(type.FindingsByCategory, category));
        return NewAxis(
            options,
            findings > 0 ? "gap" : "partial",
            [evidence],
            ["windows"],
            findings,
            findings > 0
                ? "The paired capture report contains findings for this component."
                : "The measured capture subset is clean, but the full required matrix is not complete.");
    }

    private static AxisEvidence NewAxis(
        LedgerOptions options,
        string status,
        IReadOnlyList<string> evidence,
        IReadOnlyList<string> platforms,
        int findingCount,
        string note) =>
        new()
        {
            Status = status,
            VerifiedOn = options.VerifiedOn,
            Commit = options.AnalyzedCommit,
            Evidence = evidence,
            Platforms = platforms,
            FindingCount = findingCount,
            Note = note
        };

    private static bool IsComplete(LedgerAxes axes) =>
        new[]
        {
            axes.Structural,
            axes.Functional,
            axes.Visual,
            axes.ThemingAndColor,
            axes.BehavioralState,
            axes.Platform
        }.All(axis => axis.Status is "verified" or "notApplicable");

    private static Dictionary<string, int> SumCategories(
        IEnumerable<string> typeNames,
        IReadOnlyDictionary<string, JsonElement> types)
    {
        Dictionary<string, int> result = new(StringComparer.Ordinal);
        foreach (string typeName in typeNames)
        {
            if (!types.TryGetValue(typeName, out JsonElement type))
            {
                continue;
            }

            foreach (JsonProperty category in type.GetProperty("findingsByCategory").EnumerateObject())
            {
                result[category.Name] = GetCount(result, category.Name) + category.Value.GetInt32();
            }
        }

        return result;
    }

    private static Dictionary<string, VisualTypeSummary> ReadVisualTypes(JsonElement root)
    {
        Dictionary<string, VisualTypeSummary> result = new(StringComparer.Ordinal);
        foreach (JsonElement capture in root.GetProperty("captures").EnumerateArray())
        {
            string typeName = capture.GetProperty("key").GetProperty("componentType").GetString()!;
            if (!result.TryGetValue(typeName, out VisualTypeSummary? summary))
            {
                summary = new VisualTypeSummary();
                result.Add(typeName, summary);
            }

            foreach (JsonElement finding in capture.GetProperty("findings").EnumerateArray())
            {
                string category = finding.GetProperty("category").GetString()!;
                summary.FindingCount++;
                summary.FindingsByCategory[category] =
                    GetCount(summary.FindingsByCategory, category) + 1;
            }
        }

        return result;
    }

    private static Dictionary<string, ReferenceTypeSummary> ReadReferenceTypes(JsonElement root)
    {
        Dictionary<string, ReferenceTypeSummary> result = new(StringComparer.Ordinal);
        foreach (JsonElement capture in root.GetProperty("captures").EnumerateArray())
        {
            string typeName = capture.GetProperty("componentType").GetString()!;
            if (!result.TryGetValue(typeName, out ReferenceTypeSummary? summary))
            {
                summary = new ReferenceTypeSummary();
                result.Add(typeName, summary);
            }

            summary.RequestCount++;
            string status = capture.GetProperty("status").GetString()!;
            if (status == "captured")
            {
                summary.CapturedCount++;
            }
            else if (status == "unsupported")
            {
                summary.UnsupportedCount++;
            }
        }

        return result;
    }

    private static int GetCount(IReadOnlyDictionary<string, int> values, string key) =>
        values.TryGetValue(key, out int count) ? count : 0;

    private static string NormalizePath(string path) => path.Replace('\\', '/');

    // parity-scaffolding: Accumulates visual findings for one captured component type.
    private sealed class VisualTypeSummary
    {
        public int FindingCount { get; set; }

        public Dictionary<string, int> FindingsByCategory { get; } = new(StringComparer.Ordinal);
    }

    // parity-scaffolding: Accumulates WinForms-only reference coverage for one component.
    private sealed class ReferenceTypeSummary
    {
        public int RequestCount { get; set; }

        public int CapturedCount { get; set; }

        public int UnsupportedCount { get; set; }
    }
}
