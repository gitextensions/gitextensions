namespace GitExtensions.ParityInventory;

// parity-scaffolding: Produces concrete source-parity findings from two extracted inventories.
internal static class InventoryComparer
{
    public static InventoryComparison Compare(SourceInventory original, SourceInventory twin)
    {
        FrameworkComparisonInputs frameworkInputs = FrameworkDeviationClassifier.Classify(original, twin);
        SourceInventory comparableOriginal = frameworkInputs.Original;
        SourceInventory comparableTwin = frameworkInputs.Twin;
        List<FunctionalFinding> findings = [];
        CompareParts(comparableOriginal, comparableTwin, findings);
        CompareSet(comparableOriginal.Members, comparableTwin.Members, MemberKey, "members", "member", findings);
        CompareMemberDetails(comparableOriginal, comparableTwin, findings);
        CompareMemberOrder(comparableOriginal, comparableTwin, findings);
        CompareSet(comparableOriginal.EventWiring, comparableTwin.EventWiring, EventKey, "events", "event.wiring", findings);
        CompareSet(comparableOriginal.EventHandlers, comparableTwin.EventHandlers, value => value, "events", "event.handler", findings);
        CompareMenuSequences(comparableOriginal.Menus, comparableTwin.Menus, findings);
        CompareSet(comparableOriginal.HotkeyCommandIds, comparableTwin.HotkeyCommandIds, value => value, "hotkeys", "hotkey.command", findings);
        CompareSet(comparableOriginal.Settings, comparableTwin.Settings, SettingKey, "settings", "setting", findings);
        CompareSet(comparableOriginal.TranslationStrings, comparableTwin.TranslationStrings, item => item.Name,
            "translations", "translation.string", findings);
        CompareSet(comparableOriginal.TranslationKeys, comparableTwin.TranslationKeys, item => item.Key,
            "translations", "translation.key", findings);
        InventoryComparison commentComparison = CommentInventoryComparer.Compare(comparableOriginal, comparableTwin);
        findings.AddRange(commentComparison.Findings);

        HashSet<string> originalTranslationKeys = comparableOriginal.TranslationKeys
            .Select(item => item.Key)
            .ToHashSet(StringComparer.Ordinal);
        foreach (TranslationKeyEntry entry in comparableTwin.TranslationKeys.Where(item =>
                     !item.InEnglishCatalog && !originalTranslationKeys.Contains(item.Key)))
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
            DependentFindings = [],
            AdaptedComments = commentComparison.AdaptedComments,
            AcceptedFrameworkDeviations = frameworkInputs.Deviations
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
        HashSet<string> uniqueOriginalKeys = original.Members
            .Where(member => !IsGeneratedMarkupField(member))
            .GroupBy(MemberKey, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> uniqueTwinKeys = twin.Members
            .Where(member => !IsGeneratedMarkupField(member))
            .GroupBy(MemberKey, StringComparer.Ordinal)
            .Where(group => group.Count() == 1)
            .Select(group => group.Key)
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> comparableKeys = uniqueOriginalKeys
            .Where(uniqueTwinKeys.Contains)
            .ToHashSet(StringComparer.Ordinal);
        Dictionary<string, int> originalOrder = original.Members
            .Where(member => comparableKeys.Contains(MemberKey(member)))
            .Select((member, order) => (Key: MemberKey(member), Order: order))
            .ToDictionary(item => item.Key, item => item.Order, StringComparer.Ordinal);
        Dictionary<string, int> expectedTwinPartOrder = original.Parts
            .Select((part, order) => (Part: part.ExpectedTwinPath
                ?? throw new InvalidDataException("Original source part is missing its expected twin path."), Order: order))
            .GroupBy(item => item.Part, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Min(item => item.Order), StringComparer.Ordinal);
        Dictionary<string, int> twinOrder = twin.Members
            .Where(member => comparableKeys.Contains(MemberKey(member)))
            .OrderBy(member => expectedTwinPartOrder.GetValueOrDefault(member.Part, int.MaxValue))
            .ThenBy(member => member.Part, StringComparer.Ordinal)
            .ThenBy(member => member.Order)
            .Select((member, order) => (Key: MemberKey(member), Order: order))
            .ToDictionary(item => item.Key, item => item.Order, StringComparer.Ordinal);
        foreach ((string key, int order) in originalOrder)
        {
            if (twinOrder[key] != order)
            {
                findings.Add(NewFinding(
                    "members",
                    "member.order",
                    $"member/{key}",
                    $"Member '{key}' appears at relative order {order} in the original and {twinOrder[key]} in the twin.",
                    order.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    twinOrder[key].ToString(System.Globalization.CultureInfo.InvariantCulture)));
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

            if (!MemberSignaturesMatch(originalMember, twinMember))
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

    private static void CompareMenuSequences(
        IReadOnlyList<MenuEntry> original,
        IReadOnlyList<MenuEntry> twin,
        List<FunctionalFinding> findings)
    {
        string[] parents = original.Select(item => item.Parent)
            .Concat(twin.Select(item => item.Parent))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(parent => parent, StringComparer.Ordinal)
            .ToArray();
        foreach (string parent in parents)
        {
            MenuEntry[] originalItems = original.Where(item => item.Parent == parent)
                .OrderBy(item => item.Order).ToArray();
            MenuEntry[] twinItems = twin.Where(item => item.Parent == parent)
                .OrderBy(item => item.Order).ToArray();
            int[,] lengths = new int[originalItems.Length + 1, twinItems.Length + 1];
            for (int originalIndex = originalItems.Length - 1; originalIndex >= 0; originalIndex--)
            {
                for (int twinIndex = twinItems.Length - 1; twinIndex >= 0; twinIndex--)
                {
                    lengths[originalIndex, twinIndex] = MenuIdentity(originalItems[originalIndex]) == MenuIdentity(twinItems[twinIndex])
                        ? lengths[originalIndex + 1, twinIndex + 1] + 1
                        : Math.Max(lengths[originalIndex + 1, twinIndex], lengths[originalIndex, twinIndex + 1]);
                }
            }

            int source = 0;
            int target = 0;
            while (source < originalItems.Length || target < twinItems.Length)
            {
                if (source < originalItems.Length
                    && target < twinItems.Length
                    && MenuIdentity(originalItems[source]) == MenuIdentity(twinItems[target]))
                {
                    source++;
                    target++;
                }
                else if (target < twinItems.Length
                         && (source == originalItems.Length
                             || lengths[source, target + 1] > lengths[source + 1, target]))
                {
                    MenuEntry item = twinItems[target++];
                    findings.Add(NewFinding(
                        "menus",
                        "menu.item.extra",
                        $"menu.item/{MenuKey(item)}",
                        $"Twin has extra menu item '{MenuKey(item)}'.",
                        null,
                        Format(item)));
                }
                else
                {
                    MenuEntry item = originalItems[source++];
                    findings.Add(NewFinding(
                        "menus",
                        "menu.item.missing",
                        $"menu.item/{MenuKey(item)}",
                        $"Original menu item '{MenuKey(item)}' is missing from the twin.",
                        Format(item),
                        null));
                }
            }
        }
    }

    private static string MenuIdentity(MenuEntry item) => $"{item.Kind}:{item.Name}";

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

    private static bool IsGeneratedMarkupField(MemberEntry item) =>
        item.Kind == "field" && item.Part.EndsWith(".axaml", StringComparison.Ordinal);

    private static bool MemberSignaturesMatch(MemberEntry original, MemberEntry twin)
    {
        if (string.Equals(
                NormalizeSignatureForComparison(original.Signature),
                NormalizeSignatureForComparison(twin.Signature),
                StringComparison.Ordinal))
        {
            return true;
        }

        // AXAML stores the CLR namespace in an xmlns declaration, while XElement.LocalName only
        // exposes the control type. Compare the same local type identity for generated fields.
        return IsGeneratedMarkupField(twin)
            && string.Equals(
                UnqualifyFieldType(original.Signature),
                UnqualifyFieldType(twin.Signature),
                StringComparison.Ordinal);
    }

    private static string NormalizeSignatureForComparison(string signature) =>
        signature.Replace("( ", "(", StringComparison.Ordinal)
            .Replace(" )", ")", StringComparison.Ordinal)
            .Replace(" ,", ",", StringComparison.Ordinal);

    private static string UnqualifyFieldType(string signature)
    {
        int separator = signature.LastIndexOf(' ');
        if (separator < 0)
        {
            return signature;
        }

        string type = signature[..separator];
        int namespaceSeparator = type.LastIndexOf('.');
        return $"{type[(namespaceSeparator + 1)..]}{signature[separator..]}";
    }

    private static string EventKey(EventWireEntry item) =>
        $"{item.Target}.{NormalizeEventName(item.Event)}->{item.Handler}";

    private static string NormalizeEventName(string eventName) =>
        eventName switch
        {
            // Framework constraint: these Avalonia events are the direct lifecycle equivalents
            // of the WinForms events used by ported handlers.
            "SelectedIndexChanged" or "SelectionChanged" => "selectionChanged",
            "Resize" or "SizeChanged" => "sizeChanged",
            "Enter" or "GotFocus" => "focusEntered",
            "Leave" or "LostFocus" => "focusLeft",
            _ => eventName
        };

    private static string MenuKey(MenuEntry item) => $"{item.Parent}/{item.Order}:{item.Name}";

    private static string SettingKey(SettingEntry item) => $"{item.Access}:{item.Key}";

    private static string Format<T>(T value) => value?.ToString() ?? string.Empty;
}
