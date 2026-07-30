using System.Text.Json.Serialization;

namespace GitExtensions.ParityInventory;

// parity-scaffolding: Supplies the stable interchange contract consumed by later parity tooling.
internal sealed record InventoryReport
{
    public const int CurrentSchemaVersion = 2;

    public required int SchemaVersion { get; init; }

    public required string TypeName { get; init; }

    public required SourceInventory Original { get; init; }

    public required SourceInventory Twin { get; init; }

    public required InventorySummary Summary { get; init; }

    public required IReadOnlyList<FunctionalFinding> Findings { get; init; }

    public required IReadOnlyList<CommentAdaptation> AdaptedComments { get; init; }
}

// parity-scaffolding: Describes one side of a source-level parity comparison.
internal sealed record SourceInventory
{
    public required string Root { get; init; }

    public required IReadOnlyList<SourcePart> Parts { get; init; }

    public required IReadOnlyList<MemberEntry> Members { get; init; }

    public required IReadOnlyList<EventWireEntry> EventWiring { get; init; }

    public required IReadOnlyList<string> EventHandlers { get; init; }

    public required IReadOnlyList<MenuEntry> Menus { get; init; }

    public required IReadOnlyList<string> HotkeyCommandIds { get; init; }

    public required IReadOnlyList<SettingEntry> Settings { get; init; }

    public required IReadOnlyList<TranslationStringEntry> TranslationStrings { get; init; }

    public required IReadOnlyList<TranslationKeyEntry> TranslationKeys { get; init; }

    public required IReadOnlyList<CommentEntry> Comments { get; init; }
}

// parity-scaffolding: Records a source partial and its expected corresponding path.
internal sealed record SourcePart
{
    public required string Path { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExpectedTwinPath { get; init; }
}

// parity-scaffolding: Records a declared class member in lexical order.
internal sealed record MemberEntry
{
    public required string Part { get; init; }

    public required int Order { get; init; }

    public required string Kind { get; init; }

    public required string Name { get; init; }

    public required string Accessibility { get; init; }

    public required string Signature { get; init; }
}

// parity-scaffolding: Records a source event subscription.
internal sealed record EventWireEntry
{
    public required string Part { get; init; }

    public required string Target { get; init; }

    public required string Event { get; init; }

    public required string Handler { get; init; }
}

// parity-scaffolding: Records a menu node and its ordered parent relationship.
internal sealed record MenuEntry
{
    public required string Part { get; init; }

    public required string Parent { get; init; }

    public required int Order { get; init; }

    public required string Name { get; init; }

    public required string Kind { get; init; }
}

// parity-scaffolding: Records a settings key and whether code reads or writes it.
internal sealed record SettingEntry
{
    public required string Part { get; init; }

    public required string Key { get; init; }

    public required string Access { get; init; }
}

// parity-scaffolding: Records a TranslationString field and initializer.
internal sealed record TranslationStringEntry
{
    public required string Part { get; init; }

    public required string Name { get; init; }

    public required string Initializer { get; init; }
}

// parity-scaffolding: Records a translation key inferred from code or markup and its catalog state.
internal sealed record TranslationKeyEntry
{
    public required string Key { get; init; }

    public required string Origin { get; init; }

    public required bool InEnglishCatalog { get; init; }
}

// parity-scaffolding: Anchors one normalized C# comment to the member whose reasoning it describes.
internal sealed record CommentEntry
{
    public required string Part { get; init; }

    public required string Anchor { get; init; }

    public required string Placement { get; init; }

    public required int Order { get; init; }

    public required string Kind { get; init; }

    public required int Line { get; init; }

    public required string Text { get; init; }
}

// parity-scaffolding: Records a conservative framework-name adaptation without calling it a gap.
internal sealed record CommentAdaptation
{
    public required string Path { get; init; }

    public required string OriginalPart { get; init; }

    public required int OriginalLine { get; init; }

    public required string TwinPart { get; init; }

    public required int TwinLine { get; init; }

    public required string OriginalText { get; init; }

    public required string TwinText { get; init; }
}

// parity-scaffolding: Carries both parity gaps and legitimate comment adaptations.
internal sealed record InventoryComparison
{
    public required IReadOnlyList<FunctionalFinding> Findings { get; init; }

    public required IReadOnlyList<CommentAdaptation> AdaptedComments { get; init; }
}

// parity-scaffolding: Summarizes a deterministic inventory comparison.
internal sealed record InventorySummary
{
    public required int FindingCount { get; init; }

    public required IReadOnlyDictionary<string, int> FindingsByCategory { get; init; }

    public required int AdaptedCommentCount { get; init; }
}

// parity-scaffolding: Describes one concrete functional gap between original and twin.
internal sealed record FunctionalFinding
{
    public required string Category { get; init; }

    public required string Code { get; init; }

    public required string Path { get; init; }

    public required string Message { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OriginalValue { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TwinValue { get; init; }
}
