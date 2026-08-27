namespace GitExtensions.ParityInventory;

// parity-scaffolding: Keeps cascaded evidence visible while presenting its missing member as the actionable root.
internal static class DependentFindingClassifier
{
    public static InventoryComparison Classify(SourceInventory original, InventoryComparison comparison)
    {
        Dictionary<string, string> missingMembers = comparison.Findings
            .Where(finding => finding.Code == "member.missing")
            .ToDictionary(finding => finding.Path["member/".Length..], finding => finding.Path, StringComparer.Ordinal);
        Dictionary<string, CommentEntry> comments = original.Comments.ToDictionary(GetCommentPath, StringComparer.Ordinal);
        List<FunctionalFinding> findings = [];
        List<DependentFinding> dependentFindings = [.. comparison.DependentFindings];
        foreach (FunctionalFinding finding in comparison.Findings)
        {
            string? memberKey = GetMissingMemberKey(finding, comments, missingMembers);
            if (memberKey is not null)
            {
                dependentFindings.Add(new DependentFinding
                {
                    RootPath = missingMembers[memberKey],
                    Finding = finding
                });
            }
            else
            {
                findings.Add(finding);
            }
        }

        return comparison with
        {
            Findings = findings,
            DependentFindings = dependentFindings
                .OrderBy(finding => finding.RootPath, StringComparer.Ordinal)
                .ThenBy(finding => finding.Finding.Code, StringComparer.Ordinal)
                .ThenBy(finding => finding.Finding.Path, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static string? GetMissingMemberKey(
        FunctionalFinding finding,
        IReadOnlyDictionary<string, CommentEntry> comments,
        IReadOnlyDictionary<string, string> missingMembers)
    {
        if (finding.Code == "comment.missing" && comments.TryGetValue(finding.Path, out CommentEntry? comment))
        {
            return comment.Anchor.Split('/').Reverse()
                .Select(GetMemberKey)
                .FirstOrDefault(missingMembers.ContainsKey);
        }

        if (finding.Code == "event.handler.missing")
        {
            return ExistingKey($"method:{finding.Path["event.handler/".Length..]}", missingMembers);
        }

        if (finding.Code == "event.wiring.missing")
        {
            int handlerStart = finding.Path.LastIndexOf("->", StringComparison.Ordinal);
            return handlerStart < 0
                ? null
                : ExistingKey($"method:{finding.Path[(handlerStart + 2)..]}", missingMembers);
        }

        return null;
    }

    private static string GetMemberKey(string anchor)
    {
        int parameters = anchor.IndexOf('(');
        return parameters < 0 ? anchor : anchor[..parameters];
    }

    private static string? ExistingKey(string key, IReadOnlyDictionary<string, string> missingMembers) =>
        missingMembers.ContainsKey(key) ? key : null;

    private static string GetCommentPath(CommentEntry comment) =>
        $"comment/{comment.Part}/{comment.Anchor}/{comment.Placement}/{comment.Order}";
}
