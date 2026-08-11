using System.Text.Json;
using AwesomeAssertions;
using NUnit.Framework;

namespace GitExtensions.ParityInventory.Tests;

// parity-scaffolding: Guards the initial six-axis ledger and its deterministic source sweep.
[TestFixture]
[Category("P0_5")]
public sealed class ParityBaselineTests
{
    [Test]
    public void Sweep_should_record_analyzed_linked_and_unsupported_mappings()
    {
        using BaselineFixture fixture = new();

        InventorySweepResult result = InventorySweepRunner.Run(fixture.Options);

        result.Summary.MappingCount.Should().Be(4);
        result.Summary.AnalyzedTypeCount.Should().Be(2);
        result.Summary.LinkedExactCount.Should().Be(1);
        result.Summary.UnsupportedMappingCount.Should().Be(1);
        result.Mappings.Select(mapping => mapping.AnalysisStatus)
            .Should().BeEquivalentTo("analyzed", "analyzed", "linkedExact", "unsupported");
    }

    [Test]
    public void Sweep_should_write_byte_identical_aggregate_for_identical_inputs()
    {
        using BaselineFixture fixture = new();

        InventorySweepRunner.Run(fixture.Options);
        byte[] first = File.ReadAllBytes(fixture.Options.OutputFile);
        InventorySweepRunner.Run(fixture.Options);
        byte[] second = File.ReadAllBytes(fixture.Options.OutputFile);

        second.Should().Equal(first);
    }

    [Test]
    public void Repository_ledger_should_cover_every_portmap_mapping_with_six_axes()
    {
        string root = FindRepositoryRoot();
        using JsonDocument portMap = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(root, "eng", "avalonia", "portmap.json")));
        using JsonDocument ledger = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(root, "eng", "avalonia", "parity-ledger.json")));
        JsonElement[] mappings = portMap.RootElement.EnumerateObject()
            .Where(property => !property.NameEquals("//")
                && property.Value.TryGetProperty("twin", out _))
            .Select(property => property.Value)
            .ToArray();
        JsonElement[] components = ledger.RootElement.GetProperty("components").EnumerateArray().ToArray();

        components.Should().HaveCount(mappings.Length);
        components.Select(component => component.GetProperty("source").GetString())
            .Should().OnlyHaveUniqueItems();
        foreach (JsonElement component in components)
        {
            JsonElement axes = component.GetProperty("axes");
            axes.EnumerateObject().Select(property => property.Name).Should().Equal(
                "structural",
                "functional",
                "visual",
                "themingAndColor",
                "behavioralState",
                "platform");
            foreach (JsonProperty axis in axes.EnumerateObject())
            {
                axis.Value.GetProperty("verifiedOn").GetString().Should().NotBeNullOrWhiteSpace();
                axis.Value.GetProperty("commit").GetString().Should().MatchRegex("^[0-9a-f]{40}$");
                axis.Value.GetProperty("evidence").ValueKind.Should().Be(JsonValueKind.Array);
                axis.Value.GetProperty("platforms").ValueKind.Should().Be(JsonValueKind.Array);
            }
        }
    }

    [Test]
    public void Repository_portmap_should_not_claim_parity_without_complete_ledger()
    {
        string root = FindRepositoryRoot();
        using JsonDocument portMap = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(root, "eng", "avalonia", "portmap.json")));
        using JsonDocument ledger = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(root, "eng", "avalonia", "parity-ledger.json")));
        Dictionary<string, bool> completeness = ledger.RootElement.GetProperty("components")
            .EnumerateArray()
            .ToDictionary(
                component => component.GetProperty("source").GetString()!,
                component => component.GetProperty("complete").GetBoolean(),
                StringComparer.Ordinal);

        foreach (JsonProperty mapping in portMap.RootElement.EnumerateObject()
                     .Where(property => !property.NameEquals("//")))
        {
            if (mapping.Value.GetProperty("status").GetString() == "parity")
            {
                completeness[mapping.Name].Should().BeTrue(
                    $"{mapping.Name} may claim parity only with complete six-axis evidence");
            }
        }
    }

    [Test]
    public void Ledger_schema_should_fix_axis_order_and_evidence_contract()
    {
        string schemaPath = Path.Combine(
            FindRepositoryRoot(),
            "eng",
            "avalonia",
            "parity-ledger.schema.json");
        using JsonDocument schema = JsonDocument.Parse(File.ReadAllText(schemaPath));
        JsonElement component = schema.RootElement.GetProperty("$defs").GetProperty("component");
        JsonElement axis = schema.RootElement.GetProperty("$defs").GetProperty("axis");

        component.GetProperty("properties").GetProperty("axes").GetProperty("required")
            .EnumerateArray().Select(item => item.GetString()).Should().Equal(
                "structural",
                "functional",
                "visual",
                "themingAndColor",
                "behavioralState",
                "platform");
        axis.GetProperty("required").EnumerateArray().Select(item => item.GetString())
            .Should().Contain("verifiedOn", "commit", "evidence", "platforms");
    }

    private static string FindRepositoryRoot([System.Runtime.CompilerServices.CallerFilePath] string path = "")
    {
        DirectoryInfo? directory = new(Path.GetDirectoryName(path)!);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GitExtensions.Avalonia.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException($"Could not find the repository root from '{path}'.");
    }
}

// parity-scaffolding: Supplies a minimal mixed-shape portmap for batch inventory tests.
internal sealed class BaselineFixture : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "gitextensions-parity-baseline-tests",
        Guid.NewGuid().ToString("N"));

    public BaselineFixture()
    {
        string original = Path.Combine(_root, "original");
        string twin = Path.Combine(_root, "twin");
        Directory.CreateDirectory(original);
        Directory.CreateDirectory(twin);
        string originalWidget = Write(original, "Widget.cs", "namespace Sample; public partial class Widget { private int value; }");
        string twinWidget = Write(twin, "Widget.axaml.cs", "namespace Sample; public partial class Widget { private int value; }");
        string linked = Write(original, "Linked.cs", "namespace Sample; public sealed class Linked { }");
        string interfaceSource = Write(original, "IThing.cs", "namespace Sample; public interface IThing { }");
        string interfaceTwin = Write(twin, "IThing.cs", "namespace Sample; public interface IThing { }");
        string delegateSource = Write(original, "Callback.cs", "namespace Sample; public delegate void Callback();");
        string delegateTwin = Write(twin, "Callback.cs", "namespace Sample; public delegate void Callback();");
        string missingLinkedTwin = Path.Combine(twin, "Linked.cs");
        string portMap = $$"""
            {
              "{{Normalize(originalWidget)}}": {
                "twin": "{{Normalize(twinWidget)}}",
                "status": "functional",
                "basedOn": "0000000000000000000000000000000000000000"
              },
              "{{Normalize(linked)}}": {
                "twin": "{{Normalize(missingLinkedTwin)}}",
                "status": "parity",
                "basedOn": "0000000000000000000000000000000000000000",
                "notes": "Linked unchanged."
              },
              "{{Normalize(interfaceSource)}}": {
                "twin": "{{Normalize(interfaceTwin)}}",
                "status": "functional",
                "basedOn": "0000000000000000000000000000000000000000"
              },
              "{{Normalize(delegateSource)}}": {
                "twin": "{{Normalize(delegateTwin)}}",
                "status": "functional",
                "basedOn": "0000000000000000000000000000000000000000"
              },
              "{{Normalize(Path.Combine(original, "Planned.cs"))}}": {
                "status": "unported",
                "plannedIn": "P2.1"
              },
              "{{Normalize(Path.Combine(original, "Native.cs"))}}": {
                "status": "windowsOnly",
                "justification": "The fixture models a platform-specific source.",
                "substitute": "The fixture's portable implementation."
              }
            }
            """;
        File.WriteAllText(Path.Combine(_root, "portmap.json"), portMap);
        File.WriteAllText(
            Path.Combine(_root, "English.xlf"),
            "<xliff><file original=\"Widget\"><body /></file></xliff>");
        Options = new SweepOptions
        {
            PortMapFile = Path.Combine(_root, "portmap.json"),
            OriginalRoot = original,
            TwinRoot = twin,
            TranslationsFile = Path.Combine(_root, "English.xlf"),
            AnalyzedCommit = new string('0', 40),
            OutputFile = Path.Combine(_root, "output", "functional-findings.json")
        };
    }

    public SweepOptions Options { get; }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private static string Write(string root, string relativePath, string content)
    {
        string path = Path.Combine(root, relativePath);
        File.WriteAllText(path, content);
        return path;
    }

    private static string Normalize(string path) => path.Replace('\\', '/');
}
