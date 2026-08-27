using System.Text.Json;

namespace GitExtensions.ParityInventory;

// parity-scaffolding: Loads exact, reviewed framework adaptations without providing wildcard suppression.
internal sealed class ReviewedFrameworkDeviationManifest
{
    private const int CurrentSchemaVersion = 2;

    private readonly IReadOnlyList<ReviewedFrameworkDeviationEntry> _entries;

    private ReviewedFrameworkDeviationManifest(IReadOnlyList<ReviewedFrameworkDeviationEntry> entries)
    {
        _entries = entries;
    }

    public static ReviewedFrameworkDeviationManifest Empty { get; } = new([]);

    public static ReviewedFrameworkDeviationManifest Read(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Empty;
        }

        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        JsonElement root = document.RootElement;
        int schemaVersion = root.GetProperty("schemaVersion").GetInt32();
        if (schemaVersion != CurrentSchemaVersion)
        {
            throw new InvalidDataException(
                $"Framework-adaptation manifest '{path}' has schema {schemaVersion}; expected {CurrentSchemaVersion}.");
        }

        List<ReviewedFrameworkDeviationEntry> entries = [];
        foreach (JsonElement element in root.GetProperty("deviations").EnumerateArray())
        {
            ReviewedFrameworkDeviationEntry entry = new(
                Require(element, "typeName"),
                Require(element, "code"),
                Require(element, "path"),
                Optional(element, "originalPart"),
                Optional(element, "originalAccessibility"),
                Optional(element, "originalSignature"),
                Require(element, "twinPart"),
                Require(element, "twinAccessibility"),
                Require(element, "twinSignature"),
                Require(element, "rationale"));
            if ((entry.Code != "member.extra" && entry.Code != "member.signature")
                || !entry.Path.StartsWith("member/", StringComparison.Ordinal)
                || (entry.Code == "member.signature" && !entry.Path.EndsWith("/signature", StringComparison.Ordinal)))
            {
                throw new InvalidDataException(
                    $"Reviewed adaptation '{entry.Identity}' is not an exact supported member finding.");
            }

            if (entry.Code == "member.signature"
                && (entry.OriginalPart is null
                    || entry.OriginalAccessibility is null
                    || entry.OriginalSignature is null))
            {
                throw new InvalidDataException(
                    $"Reviewed signature adaptation '{entry.Identity}' must pin the original member.");
            }

            entries.Add(entry);
        }

        string? duplicate = entries.GroupBy(entry => entry.Identity, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1)?.Key;
        if (duplicate is not null)
        {
            throw new InvalidDataException($"Framework-adaptation manifest contains duplicate '{duplicate}'.");
        }

        return new ReviewedFrameworkDeviationManifest(entries);
    }

    public InventoryComparison Apply(
        string typeName,
        SourceInventory original,
        SourceInventory twin,
        InventoryComparison comparison,
        ISet<string>? appliedEntries = null)
    {
        List<FunctionalFinding> findings = [.. comparison.Findings];
        List<AcceptedFrameworkDeviation> deviations = [.. comparison.AcceptedFrameworkDeviations];
        foreach (ReviewedFrameworkDeviationEntry entry in _entries.Where(entry => entry.TypeName == typeName))
        {
            FunctionalFinding[] matches = findings.Where(finding =>
                finding.Category == "members"
                && finding.Code == entry.Code
                && finding.Path == entry.Path).ToArray();
            if (matches.Length != 1)
            {
                throw new InvalidDataException(
                    $"Reviewed adaptation '{entry.Identity}' is stale: expected one live finding, found {matches.Length}.");
            }

            string memberKey = entry.Path["member/".Length..];
            if (entry.Code == "member.signature")
            {
                memberKey = memberKey[..^"/signature".Length];
            }

            MemberEntry[] members = twin.Members.Where(member => $"{member.Kind}:{member.Name}" == memberKey).ToArray();
            if (members.Length != 1)
            {
                throw new InvalidDataException(
                    $"Reviewed adaptation '{entry.Identity}' is ambiguous: expected one twin member, found {members.Length}.");
            }

            MemberEntry member = members[0];
            if (member.Part != entry.TwinPart
                || member.Accessibility != entry.TwinAccessibility
                || member.Signature != entry.TwinSignature)
            {
                throw new InvalidDataException(
                    $"Reviewed adaptation '{entry.Identity}' drifted. Expected '{entry.TwinPart}' / "
                    + $"'{entry.TwinAccessibility}' / '{entry.TwinSignature}', found '{member.Part}' / "
                    + $"'{member.Accessibility}' / '{member.Signature}'.");
            }

            MemberEntry? originalMember = null;
            if (entry.Code == "member.signature")
            {
                MemberEntry[] originalMembers = original.Members
                    .Where(candidate => $"{candidate.Kind}:{candidate.Name}" == memberKey)
                    .ToArray();
                if (originalMembers.Length != 1)
                {
                    throw new InvalidDataException(
                        $"Reviewed adaptation '{entry.Identity}' is ambiguous: expected one original member, found {originalMembers.Length}.");
                }

                originalMember = originalMembers[0];
                if (originalMember.Part != entry.OriginalPart
                    || originalMember.Accessibility != entry.OriginalAccessibility
                    || originalMember.Signature != entry.OriginalSignature)
                {
                    throw new InvalidDataException(
                        $"Reviewed adaptation '{entry.Identity}' source drifted. Expected '{entry.OriginalPart}' / "
                        + $"'{entry.OriginalAccessibility}' / '{entry.OriginalSignature}', found '{originalMember.Part}' / "
                        + $"'{originalMember.Accessibility}' / '{originalMember.Signature}'.");
                }
            }

            findings.Remove(matches[0]);
            deviations.Add(new AcceptedFrameworkDeviation
            {
                Category = "members",
                Code = entry.Code,
                Path = entry.Path,
                OriginalPart = originalMember?.Part ?? "(no WinForms member)",
                TwinPart = member.Part,
                Rationale = entry.Rationale,
                OriginalValue = originalMember is null ? null : $"{originalMember.Accessibility} {originalMember.Signature}",
                TwinValue = $"{member.Accessibility} {member.Signature}"
            });
            appliedEntries?.Add(entry.Identity);
        }

        return comparison with
        {
            Findings = findings,
            AcceptedFrameworkDeviations = deviations
                .OrderBy(deviation => deviation.Category, StringComparer.Ordinal)
                .ThenBy(deviation => deviation.Code, StringComparer.Ordinal)
                .ThenBy(deviation => deviation.Path, StringComparer.Ordinal)
                .ToArray()
        };
    }

    public void ValidateAllApplied(IReadOnlySet<string> appliedEntries)
    {
        string[] unused = _entries.Select(entry => entry.Identity)
            .Where(identity => !appliedEntries.Contains(identity))
            .ToArray();
        if (unused.Length > 0)
        {
            throw new InvalidDataException(
                $"Framework-adaptation manifest contains unapplied entries: {string.Join(", ", unused)}.");
        }
    }

    private static string Require(JsonElement element, string propertyName)
    {
        string? value = element.GetProperty(propertyName).GetString();
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidDataException($"Framework-adaptation property '{propertyName}' is required.");
    }

    private static string? Optional(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private sealed record ReviewedFrameworkDeviationEntry(
        string TypeName,
        string Code,
        string Path,
        string? OriginalPart,
        string? OriginalAccessibility,
        string? OriginalSignature,
        string TwinPart,
        string TwinAccessibility,
        string TwinSignature,
        string Rationale)
    {
        public string Identity => $"{TypeName}/{Code}/{Path}";
    }
}
