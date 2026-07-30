namespace GitExtensions.ParityInventory;

// parity-scaffolding: Produces concrete source-parity findings from two extracted inventories.
internal static class InventoryComparer
{
    public static InventoryComparison Compare(SourceInventory original, SourceInventory twin)
    {
        List<FunctionalFinding> findings = [];
        CompareParts(original, twin, findings);
        CompareSet(original.Members, twin.Members, MemberKey, "members", "member", findings);
        CompareMemberDetails(original, twin, findings);
        CompareMemberOrder(original, twin, findings);
        CompareSet(original.EventWiring, twin.EventWiring, EventKey, "events", "event.wiring", findings);
        CompareSet(original.EventHandlers, twin.EventHandlers, value => value, "events", "event.handler", findings);
        CompareSet(original.Menus, twin.Menus, MenuKey, "menus", "menu.item", findings);
        CompareSet(original.HotkeyCommandIds, twin.HotkeyCommandIds, value => value, "hotkeys", "hotkey.command", findings);
        CompareSet(original.Settings, twin.Settings, SettingKey, "settings", "setting", findings);
        CompareSet(original.TranslationStrings, twin.TranslationStrings, item => item.Name,
            "translations", "translation.string", findings);
        CompareSet(original.TranslationKeys, twin.TranslationKeys, item => item.Key,
            "translations", "translation.key", findings);
        InventoryComparison commentComparison = CommentInventoryComparer.Compare(original, twin);
        findings.AddRange(commentComparison.Findings);

        foreach (TranslationKeyEntry entry in twin.TranslationKeys.Where(item => !item.InEnglishCatalog))
        {
            findings.Add(NewFinding(
                "translations",
                "translation.not-in-english",
                $"translation/{entry.Key}",
                $"Twin emits translation key '{entry.Key}' which is absent from English.xlf.",
                null,
                entry.Origin));
        }

        return new InventoryComparison
        {
            Findings = findings
                .OrderBy(finding => finding.Category, StringComparer.Ordinal)
                .ThenBy(finding => finding.Code, StringComparer.Ordinal)
                .ThenBy(finding => finding.Path, StringComparer.Ordinal)
                .ThenBy(finding => finding.OriginalValue, StringComparer.Ordinal)
                .ThenBy(finding => finding.TwinValue, StringComparer.Ordinal)
                .ToArray(),
            AdaptedComments = commentComparison.AdaptedComments
        };
    }

    private static void CompareParts(
        SourceInventory original,
        SourceInventory twin,
        List<FunctionalFinding> findings)
    {
        HashSet<string> twinPaths = twin.Parts.Select(part => part.Path).ToHashSet(StringComparer.Ordinal);
        foreach (SourcePart part in original.Parts)
        {
            string expected = part.ExpectedTwinPath
                ?? throw new InvalidDataException("Original source part is missing its expected twin path.");
            if (!twinPaths.Contains(expected))
            {
                findings.Add(NewFinding(
                    "structure",
                    "partial.missing",
                    $"part/{expected}",
                    $"Original partial '{part.Path}' has no twin at '{expected}'.",
                    part.Path,
                    null));
            }
        }
    }

    private static void CompareMemberOrder(
        SourceInventory original,
        SourceInventory twin,
        List<FunctionalFinding> findings)
    {
        Dictionary<string, int> twinOrder = twin.Members
            .GroupBy(MemberKey, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single().Order, StringComparer.Ordinal);
        foreach (MemberEntry member in original.Members)
        {
            string key = MemberKey(member);
            if (twinOrder.TryGetValue(key, out int order) && order != member.Order)
            {
                findings.Add(NewFinding(
                    "members",
                    "member.order",
                    $"member/{key}",
                    $"Member '{key}' appears at order {member.Order} in the original and {order} in the twin.",
                    member.Order.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    order.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            }
        }
    }

    private static void CompareMemberDetails(
        SourceInventory original,
        SourceInventory twin,
        List<FunctionalFinding> findings)
    {
        Dictionary<string, string> expectedTwinParts = original.Parts.ToDictionary(
            part => part.Path,
            part => part.ExpectedTwinPath
                ?? throw new InvalidDataException("Original source part is missing its expected twin path."),
            StringComparer.Ordinal);
        Dictionary<string, MemberEntry> twinMembers = twin.Members
            .GroupBy(MemberKey, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);
        foreach (IGrouping<string, MemberEntry> group in original.Members.GroupBy(MemberKey, StringComparer.Ordinal)
                     .Where(group => group.Count() == 1))
        {
            MemberEntry originalMember = group.Single();
            if (!twinMembers.TryGetValue(group.Key, out MemberEntry? twinMember))
            {
                continue;
            }

            string expectedPart = expectedTwinParts[originalMember.Part];
            if (!string.Equals(expectedPart, twinMember.Part, StringComparison.Ordinal))
            {
                findings.Add(NewFinding(
                    "members",
                    "member.partial",
                    $"member/{group.Key}/part",
                    $"Member '{group.Key}' is declared in a different partial.",
                    originalMember.Part,
                    twinMember.Part));
            }

            if (!string.Equals(originalMember.Accessibility, twinMember.Accessibility, StringComparison.Ordinal))
            {
                findings.Add(NewFinding(
                    "members",
                    "member.accessibility",
                    $"member/{group.Key}/accessibility",
                    $"Member '{group.Key}' has different accessibility.",
                    originalMember.Accessibility,
                    twinMember.Accessibility));
            }

            if (!string.Equals(originalMember.Signature, twinMember.Signature, StringComparison.Ordinal))
            {
                findings.Add(NewFinding(
                    "members",
                    "member.signature",
                    $"member/{group.Key}/signature",
                    $"Member '{group.Key}' has a different signature.",
                    originalMember.Signature,
                    twinMember.Signature));
            }
        }
    }

    private static void CompareSet<T>(
        IEnumerable<T> original,
        IEnumerable<T> twin,
        Func<T, string> keySelector,
        string category,
        string codePrefix,
        List<FunctionalFinding> findings)
    {
        Dictionary<string, T> originalItems = original.GroupBy(keySelector, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        Dictionary<string, T> twinItems = twin.GroupBy(keySelector, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        foreach ((string key, T value) in originalItems)
        {
            if (!twinItems.ContainsKey(key))
            {
                findings.Add(NewFinding(
                    category,
                    $"{codePrefix}.missing",
                    $"{codePrefix}/{key}",
                    $"Original {codePrefix.Replace('.', ' ')} '{key}' is missing from the twin.",
                    Format(value),
                    null));
            }
        }

        foreach ((string key, T value) in twinItems)
        {
            if (!originalItems.ContainsKey(key))
            {
                findings.Add(NewFinding(
                    category,
                    $"{codePrefix}.extra",
                    $"{codePrefix}/{key}",
                    $"Twin has extra {codePrefix.Replace('.', ' ')} '{key}'.",
                    null,
                    Format(value)));
            }
        }
    }

    private static FunctionalFinding NewFinding(
        string category,
        string code,
        string path,
        string message,
        string? originalValue,
        string? twinValue) =>
        new()
        {
            Category = category,
            Code = code,
            Path = path,
            Message = message,
            OriginalValue = originalValue,
            TwinValue = twinValue
        };

    private static string MemberKey(MemberEntry item) => $"{item.Kind}:{item.Name}";

    private static string EventKey(EventWireEntry item) => $"{item.Target}.{item.Event}->{item.Handler}";

    private static string MenuKey(MenuEntry item) => $"{item.Parent}/{item.Order}:{item.Name}";

    private static string SettingKey(SettingEntry item) => $"{item.Access}:{item.Key}";

    private static string Format<T>(T value) => value?.ToString() ?? string.Empty;
}
