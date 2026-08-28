using System.Text.Json;

namespace GitExtensions.ParityInventory;

// parity-scaffolding: Loads exact, reviewed framework adaptations without providing wildcard suppression.
internal sealed class ReviewedFrameworkDeviationManifest
{
    private const int CurrentSchemaVersion = 4;

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
                Optional(element, "category"),
                Optional(element, "originalPart"),
                Optional(element, "originalAccessibility"),
                Optional(element, "originalSignature"),
                Optional(element, "originalValue"),
                Optional(element, "twinPart"),
                Optional(element, "twinAccessibility"),
                Optional(element, "twinSignature"),
                Optional(element, "twinValue"),
                Require(element, "rationale"));
            if (entry.IsMemberDeviation)
            {
                ValidateMemberEntry(entry);
            }
            else
            {
                ValidateExactFindingEntry(entry);
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
            if (!entry.IsMemberDeviation)
            {
                ApplyExactFinding(entry, original, findings, deviations, appliedEntries);
                continue;
            }

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

            SourceInventory memberInventory = entry.Code == "member.missing" ? original : twin;
            MemberEntry[] members = memberInventory.Members
                .Where(member => $"{member.Kind}:{member.Name}" == memberKey)
                .ToArray();
            if (members.Length != 1)
            {
                throw new InvalidDataException(
                    $"Reviewed adaptation '{entry.Identity}' is ambiguous: expected one "
                    + $"{(entry.Code == "member.missing" ? "original" : "twin")} member, found {members.Length}.");
            }

            MemberEntry member = members[0];
            string? expectedPart = entry.Code == "member.missing" ? entry.OriginalPart : entry.TwinPart;
            string? expectedAccessibility = entry.Code == "member.missing"
                ? entry.OriginalAccessibility
                : entry.TwinAccessibility;
            string? expectedSignature = entry.Code == "member.missing"
                ? entry.OriginalSignature
                : entry.TwinSignature;
            if (member.Part != expectedPart
                || member.Accessibility != expectedAccessibility
                || member.Signature != expectedSignature)
            {
                throw new InvalidDataException(
                    $"Reviewed adaptation '{entry.Identity}' drifted. Expected '{expectedPart}' / "
                    + $"'{expectedAccessibility}' / '{expectedSignature}', found '{member.Part}' / "
                    + $"'{member.Accessibility}' / '{member.Signature}'.");
            }

            MemberEntry? originalMember = entry.Code == "member.missing" ? member : null;
            MemberEntry? twinMember = entry.Code == "member.missing" ? null : member;
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
                TwinPart = twinMember?.Part ?? "(no Avalonia member)",
                Rationale = entry.Rationale,
                OriginalValue = originalMember is null ? null : $"{originalMember.Accessibility} {originalMember.Signature}",
                TwinValue = twinMember is null ? null : $"{twinMember.Accessibility} {twinMember.Signature}"
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

    private static void ValidateMemberEntry(ReviewedFrameworkDeviationEntry entry)
    {
        if (!entry.Path.StartsWith("member/", StringComparison.Ordinal)
            || (entry.Code == "member.signature" && !entry.Path.EndsWith("/signature", StringComparison.Ordinal)))
        {
            throw new InvalidDataException(
                $"Reviewed adaptation '{entry.Identity}' is not an exact supported member finding.");
        }

        bool requiresOriginal = entry.Code is "member.missing" or "member.signature";
        bool requiresTwin = entry.Code is "member.extra" or "member.signature";
        bool hasOriginal = entry.OriginalPart is not null
            || entry.OriginalAccessibility is not null
            || entry.OriginalSignature is not null;
        bool hasTwin = entry.TwinPart is not null
            || entry.TwinAccessibility is not null
            || entry.TwinSignature is not null;
        if ((requiresOriginal
             && (entry.OriginalPart is null
                || entry.OriginalAccessibility is null
                || entry.OriginalSignature is null))
            || (requiresTwin
                && (entry.TwinPart is null
                    || entry.TwinAccessibility is null
                    || entry.TwinSignature is null))
            || (!requiresOriginal && hasOriginal)
            || (!requiresTwin && hasTwin))
        {
            throw new InvalidDataException(
                $"Reviewed member adaptation '{entry.Identity}' must pin every present member.");
        }

        if (entry.Code == "member.missing" && !IsGeneratedDesignerMember(entry))
        {
            throw new InvalidDataException(
                $"Reviewed missing-member adaptation '{entry.Identity}' is not generated WinForms Designer lifecycle state.");
        }
    }

    private static bool IsGeneratedDesignerMember(ReviewedFrameworkDeviationEntry entry) =>
        entry.OriginalPart!.EndsWith(".Designer.cs", StringComparison.Ordinal)
        && (entry.Path, entry.OriginalAccessibility, entry.OriginalSignature) is
            ("member/field:components", "private", "System.ComponentModel.IContainer components")
            or ("member/method:Dispose", "protected", "void Dispose(bool disposing)")
            or ("member/method:InitializeComponent", "private", "void InitializeComponent()");

    private static void ValidateExactFindingEntry(ReviewedFrameworkDeviationEntry entry)
    {
        bool isDesignerComment = entry.Category == "comments"
            && entry.Code == "comment.missing"
            && entry.Path.StartsWith("comment/", StringComparison.Ordinal)
            && entry.Path.Contains(".Designer.cs/", StringComparison.Ordinal)
            && entry.OriginalPart?.EndsWith(".Designer.cs", StringComparison.Ordinal) == true
            && entry.OriginalValue is not null
            && entry.TwinPart is not null
            && entry.TwinValue is null;
        bool isMissingEventWire = entry.Category == "events"
            && entry.Code == "event.wiring.missing"
            && entry.Path.StartsWith("event.wiring/", StringComparison.Ordinal)
            && entry.OriginalPart is not null
            && entry.OriginalValue is not null
            && entry.TwinPart is null
            && entry.TwinValue is null;
        bool isExtraEventWire = entry.Category == "events"
            && entry.Code == "event.wiring.extra"
            && entry.Path.StartsWith("event.wiring/", StringComparison.Ordinal)
            && entry.OriginalPart is null
            && entry.OriginalValue is null
            && entry.TwinPart is not null
            && entry.TwinValue is not null;
        bool isExtraEventHandler = entry.Category == "events"
            && entry.Code == "event.handler.extra"
            && entry.Path.StartsWith("event.handler/", StringComparison.Ordinal)
            && entry.OriginalPart is null
            && entry.OriginalValue is null
            && entry.TwinPart is null
            && entry.TwinValue is not null;
        if (!isDesignerComment && !isMissingEventWire && !isExtraEventWire && !isExtraEventHandler)
        {
            throw new InvalidDataException(
                $"Reviewed adaptation '{entry.Identity}' is not an exact supported generated-comment or event finding.");
        }
    }

    private static void ApplyExactFinding(
        ReviewedFrameworkDeviationEntry entry,
        SourceInventory original,
        List<FunctionalFinding> findings,
        List<AcceptedFrameworkDeviation> deviations,
        ISet<string>? appliedEntries)
    {
        FunctionalFinding[] matches = findings.Where(finding =>
            finding.Category == entry.Category
            && finding.Code == entry.Code
            && finding.Path == entry.Path
            && finding.OriginalValue == entry.OriginalValue
            && finding.TwinValue == entry.TwinValue).ToArray();
        if (matches.Length != 1)
        {
            throw new InvalidDataException(
                $"Reviewed adaptation '{entry.Identity}' is stale or drifted: expected one exact live finding, found {matches.Length}.");
        }

        if (entry.Code == "comment.missing")
        {
            SourcePart[] sourceParts = original.Parts.Where(part => part.Path == entry.OriginalPart).ToArray();
            if (sourceParts.Length != 1 || sourceParts[0].ExpectedTwinPath != entry.TwinPart)
            {
                throw new InvalidDataException(
                    $"Reviewed adaptation '{entry.Identity}' source or expected twin part drifted.");
            }
        }
        else if (entry.Code == "event.wiring.missing"
                 && !ValuePinsPart(entry.OriginalValue!, entry.OriginalPart!))
        {
            throw new InvalidDataException($"Reviewed adaptation '{entry.Identity}' original part drifted.");
        }
        else if (entry.Code == "event.wiring.extra"
                 && !ValuePinsPart(entry.TwinValue!, entry.TwinPart!))
        {
            throw new InvalidDataException($"Reviewed adaptation '{entry.Identity}' twin part drifted.");
        }

        findings.Remove(matches[0]);
        deviations.Add(new AcceptedFrameworkDeviation
        {
            Category = entry.Category!,
            Code = entry.Code,
            Path = entry.Path,
            OriginalPart = entry.OriginalPart ?? "(no WinForms source fact)",
            TwinPart = entry.TwinPart ?? "(no Avalonia source fact)",
            Rationale = entry.Rationale,
            OriginalValue = entry.OriginalValue,
            TwinValue = entry.TwinValue
        });
        appliedEntries?.Add(entry.Identity);
    }

    private static bool ValuePinsPart(string value, string part) =>
        value.StartsWith($"EventWireEntry {{ Part = {part}, ", StringComparison.Ordinal);

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
        string? Category,
        string? OriginalPart,
        string? OriginalAccessibility,
        string? OriginalSignature,
        string? OriginalValue,
        string? TwinPart,
        string? TwinAccessibility,
        string? TwinSignature,
        string? TwinValue,
        string Rationale)
    {
        public string Identity => $"{TypeName}/{Code}/{Path}";

        public bool IsMemberDeviation => Code is "member.missing" or "member.extra" or "member.signature";
    }
}
