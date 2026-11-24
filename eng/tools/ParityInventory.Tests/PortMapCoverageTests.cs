using System.Text.Json;
using System.Xml.Linq;
using AwesomeAssertions;
using NUnit.Framework;

namespace GitExtensions.ParityInventory.Tests;

// parity-scaffolding: Prevents original GitUI source files from silently falling out of the parity inventory.
[TestFixture]
[Category("P0_7")]
public sealed class PortMapCoverageTests
{
    private static readonly string[] MappingStatuses = ["scaffolded", "functional", "parity"];
    private static readonly string[] ClassificationStatuses = ["unported", "windowsOnly"];

    [Test]
    public void Repository_portmap_should_classify_every_GitUI_CSharp_source()
    {
        RepositoryInventory inventory = RepositoryInventory.Read();

        string[] missing = inventory.OriginalSources
            .Except(inventory.MappedSources, StringComparer.Ordinal)
            .Except(inventory.LinkedSources, StringComparer.Ordinal)
            .Except(inventory.PhysicalTwinSources, StringComparer.Ordinal)
            .Except(inventory.ClassifiedSources, StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        missing.Should().BeEmpty("every original must be ported, linked, or explicitly classified");
    }

    [Test]
    public void Repository_portmap_should_use_the_non_twin_contract()
    {
        RepositoryInventory inventory = RepositoryInventory.Read();

        foreach (PortMapEntry property in inventory.PortMapEntries)
        {
            JsonElement entry = property.Value;
            string status = entry.GetProperty("status").GetString()!;
            if (MappingStatuses.Contains(status, StringComparer.Ordinal))
            {
                entry.TryGetProperty("twin", out JsonElement twin).Should().BeTrue();
                twin.GetString().Should().NotBeNullOrWhiteSpace();
                entry.TryGetProperty("basedOn", out JsonElement basedOn).Should().BeTrue();
                basedOn.GetString().Should().MatchRegex("^[0-9a-f]{40}$");
                continue;
            }

            ClassificationStatuses.Should().Contain(status);
            entry.TryGetProperty("twin", out _).Should().BeFalse();
            entry.TryGetProperty("basedOn", out _).Should().BeFalse();
            if (status == "windowsOnly")
            {
                entry.GetProperty("justification").GetString().Should().NotBeNullOrWhiteSpace();
                entry.GetProperty("substitute").GetString().Should().NotBeNullOrWhiteSpace();
            }
            else
            {
                entry.GetProperty("plannedIn").GetString().Should().MatchRegex("^P[1-9][0-9]*\\.[0-9]+[a-z]?$");
            }
        }
    }

    [Test]
    public void Repository_portmap_non_twin_entries_should_name_existing_originals()
    {
        RepositoryInventory inventory = RepositoryInventory.Read();

        inventory.ClassifiedSources.Should().BeSubsetOf(inventory.OriginalSources);
        inventory.ClassifiedSources.Intersect(inventory.MappedSources, StringComparer.Ordinal)
            .Should().BeEmpty();
        inventory.ClassifiedSources.Intersect(inventory.LinkedSources, StringComparer.Ordinal)
            .Should().BeEmpty();
        inventory.ClassifiedSources.Intersect(inventory.PhysicalTwinSources, StringComparer.Ordinal)
            .Should().BeEmpty();
    }

    private sealed class RepositoryInventory
    {
        private RepositoryInventory(
            IReadOnlySet<string> originalSources,
            IReadOnlySet<string> mappedSources,
            IReadOnlySet<string> linkedSources,
            IReadOnlySet<string> physicalTwinSources,
            IReadOnlySet<string> classifiedSources,
            IReadOnlyList<PortMapEntry> portMapEntries)
        {
            OriginalSources = originalSources;
            MappedSources = mappedSources;
            LinkedSources = linkedSources;
            PhysicalTwinSources = physicalTwinSources;
            ClassifiedSources = classifiedSources;
            PortMapEntries = portMapEntries;
        }

        public IReadOnlySet<string> OriginalSources { get; }

        public IReadOnlySet<string> MappedSources { get; }

        public IReadOnlySet<string> LinkedSources { get; }

        public IReadOnlySet<string> PhysicalTwinSources { get; }

        public IReadOnlySet<string> ClassifiedSources { get; }

        public IReadOnlyList<PortMapEntry> PortMapEntries { get; }

        public static RepositoryInventory Read()
        {
            string root = FindRepositoryRoot();
            string originalRoot = Path.Combine(root, "src", "app", "GitUI");
            string twinRoot = Path.Combine(root, "src", "app", "GitUI.Avalonia");
            HashSet<string> originalSources = Directory.EnumerateFiles(
                    originalRoot,
                    "*.cs",
                    SearchOption.AllDirectories)
                .Where(path => !HasBuildDirectory(path))
                .Select(path => Normalize(Path.GetRelativePath(root, path)))
                .ToHashSet(StringComparer.Ordinal);
            using JsonDocument portMap = JsonDocument.Parse(
                File.ReadAllText(Path.Combine(root, "eng", "avalonia", "portmap.json")));
            PortMapEntry[] entries = portMap.RootElement.EnumerateObject()
                .Where(property => !property.NameEquals("//"))
                .Select(property => new PortMapEntry(property.Name, property.Value.Clone()))
                .ToArray();
            HashSet<string> mappedSources = entries
                .Where(property => property.Value.TryGetProperty("twin", out _))
                .Select(property => property.Name)
                .ToHashSet(StringComparer.Ordinal);
            HashSet<string> classifiedSources = entries
                .Where(property => ClassificationStatuses.Contains(
                    property.Value.GetProperty("status").GetString(),
                    StringComparer.Ordinal))
                .Select(property => property.Name)
                .ToHashSet(StringComparer.Ordinal);

            return new RepositoryInventory(
                originalSources,
                mappedSources,
                ReadLinkedSources(root, originalRoot),
                ReadPhysicalTwinSources(root, originalSources, twinRoot),
                classifiedSources,
                entries);
        }

        private static HashSet<string> ReadLinkedSources(string root, string originalRoot)
        {
            string projectFile = Path.Combine(root, "src", "app", "GitUI.Avalonia", "GitUI.Avalonia.csproj");
            XDocument project = XDocument.Load(projectFile);
            return project.Descendants("Compile")
                .Select(element => element.Attribute("Include")?.Value)
                .Where(include => !string.IsNullOrWhiteSpace(include))
                .Select(include => Path.GetFullPath(ToPlatformPath(include!), Path.GetDirectoryName(projectFile)!))
                .Where(path => path.StartsWith(originalRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                .Select(path => Normalize(Path.GetRelativePath(root, path)))
                .ToHashSet(StringComparer.Ordinal);
        }

        private static HashSet<string> ReadPhysicalTwinSources(
            string root,
            IEnumerable<string> originalSources,
            string twinRoot)
        {
            HashSet<string> sources = new(StringComparer.Ordinal);
            foreach (string source in originalSources)
            {
                string relative = Normalize(Path.GetRelativePath(
                    "src/app/GitUI",
                    source));
                string samePath = Path.Combine(twinRoot, relative);
                string axaml = relative.EndsWith(".Designer.cs", StringComparison.Ordinal)
                    ? Path.Combine(twinRoot, relative[..^".Designer.cs".Length] + ".axaml")
                    : string.Empty;
                string codeBehind = Path.Combine(twinRoot, relative[..^".cs".Length] + ".axaml.cs");
                if (File.Exists(samePath) || File.Exists(axaml) || File.Exists(codeBehind))
                {
                    sources.Add(source);
                }
            }

            return sources;
        }

        private static bool HasBuildDirectory(string path)
        {
            string normalized = Normalize(path);
            return normalized.Contains("/bin/", StringComparison.Ordinal)
                || normalized.Contains("/obj/", StringComparison.Ordinal);
        }

        private static string ToPlatformPath(string path) => path
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);

        private static string Normalize(string path) => path.Replace('\\', '/');
    }

    private sealed record PortMapEntry(string Name, JsonElement Value);

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
