using System.Text.RegularExpressions;

namespace GitExtensions.ParityInventory;

// parity-scaffolding: Classifies anchored comment preservation separately from other source facts.
internal static class CommentInventoryComparer
{
    public static InventoryComparison Compare(SourceInventory original, SourceInventory twin)
    {
        IndexedComment[] originalComments = original.Comments
            .Select((comment, index) => new IndexedComment(index, comment))
            .ToArray();
        IndexedComment[] twinComments = twin.Comments
            .Select((comment, index) => new IndexedComment(index, comment))
            .ToArray();
        Dictionary<CommentGroupKey, IndexedComment[]> originalGroups = originalComments
            .GroupBy(comment => new CommentGroupKey(
                GetComparablePart(comment.Entry.Part),
                comment.Entry.Anchor,
                comment.Entry.Placement))
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(comment => comment.Entry.Order).ToArray());
        Dictionary<CommentGroupKey, IndexedComment[]> twinGroups = twinComments
            .GroupBy(comment => new CommentGroupKey(
                GetComparablePart(comment.Entry.Part),
                comment.Entry.Anchor,
                comment.Entry.Placement))
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(comment => comment.Entry.Order).ToArray());

        List<CommentPair> changed = [];
        List<IndexedComment> unmatchedOriginal = [];
        List<CommentAdaptation> adaptations = [];
        HashSet<int> usedTwin = [];
        foreach ((CommentGroupKey key, IndexedComment[] originalGroup) in originalGroups
                     .OrderBy(pair => pair.Key.Part, StringComparer.Ordinal)
                     .ThenBy(pair => pair.Key.Anchor, StringComparer.Ordinal)
                     .ThenBy(pair => pair.Key.Placement, StringComparer.Ordinal))
        {
            IndexedComment[] twinGroup = twinGroups.TryGetValue(key, out IndexedComment[]? value)
                ? value
                : [];
            foreach (AlignmentStep step in Align(originalGroup, twinGroup))
            {
                switch (step.Kind)
                {
                    case AlignmentKind.Exact:
                        usedTwin.Add(step.Twin!.Index);
                        break;
                    case AlignmentKind.Adapted:
                        usedTwin.Add(step.Twin!.Index);
                        adaptations.Add(NewAdaptation(step.Original!.Entry, step.Twin.Entry));
                        break;
                    case AlignmentKind.Changed:
                        usedTwin.Add(step.Twin!.Index);
                        changed.Add(new CommentPair(step.Original!, step.Twin));
                        break;
                    case AlignmentKind.Deleted:
                        unmatchedOriginal.Add(step.Original!);
                        break;
                    case AlignmentKind.Inserted:
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown alignment step '{step.Kind}'.");
                }
            }
        }

        List<FunctionalFinding> findings = [];
        foreach (CommentPair pair in changed)
        {
            IndexedComment? drifted = FindUnusedExact(pair.Original, twinComments, usedTwin);
            if (drifted is not null)
            {
                usedTwin.Remove(pair.Twin.Index);
                usedTwin.Add(drifted.Index);
                findings.Add(NewDriftedFinding(pair.Original.Entry, drifted.Entry));
            }
            else
            {
                findings.Add(NewChangedFinding(pair.Original.Entry, pair.Twin.Entry));
            }
        }

        foreach (IndexedComment originalComment in unmatchedOriginal)
        {
            IndexedComment? drifted = FindUnusedExact(originalComment, twinComments, usedTwin);
            if (drifted is not null)
            {
                usedTwin.Add(drifted.Index);
                findings.Add(NewDriftedFinding(originalComment.Entry, drifted.Entry));
            }
            else
            {
                findings.Add(NewMissingFinding(originalComment.Entry));
            }
        }

        return new InventoryComparison
        {
            Findings = findings
                .OrderBy(finding => finding.Code, StringComparer.Ordinal)
                .ThenBy(finding => finding.Path, StringComparer.Ordinal)
                .ThenBy(finding => finding.OriginalValue, StringComparer.Ordinal)
                .ToArray(),
            AdaptedComments = adaptations
                .OrderBy(adaptation => adaptation.Path, StringComparer.Ordinal)
                .ThenBy(adaptation => adaptation.OriginalText, StringComparer.Ordinal)
                .ToArray()
        };
    }

    private static IReadOnlyList<AlignmentStep> Align(
        IReadOnlyList<IndexedComment> original,
        IReadOnlyList<IndexedComment> twin)
    {
        int[,] costs = new int[original.Count + 1, twin.Count + 1];
        AlignmentKind[,] operations = new AlignmentKind[original.Count + 1, twin.Count + 1];
        for (int originalIndex = 1; originalIndex <= original.Count; originalIndex++)
        {
            costs[originalIndex, 0] = originalIndex * 2;
            operations[originalIndex, 0] = AlignmentKind.Deleted;
        }

        for (int twinIndex = 1; twinIndex <= twin.Count; twinIndex++)
        {
            costs[0, twinIndex] = twinIndex * 2;
            operations[0, twinIndex] = AlignmentKind.Inserted;
        }

        for (int originalIndex = 1; originalIndex <= original.Count; originalIndex++)
        {
            for (int twinIndex = 1; twinIndex <= twin.Count; twinIndex++)
            {
                AlignmentKind diagonalKind = Classify(
                    original[originalIndex - 1].Entry,
                    twin[twinIndex - 1].Entry);
                int diagonalCost = costs[originalIndex - 1, twinIndex - 1] + GetCost(diagonalKind);
                int deletedCost = costs[originalIndex - 1, twinIndex] + 2;
                int insertedCost = costs[originalIndex, twinIndex - 1] + 2;
                if (diagonalCost <= deletedCost && diagonalCost <= insertedCost)
                {
                    costs[originalIndex, twinIndex] = diagonalCost;
                    operations[originalIndex, twinIndex] = diagonalKind;
                }
                else if (deletedCost <= insertedCost)
                {
                    costs[originalIndex, twinIndex] = deletedCost;
                    operations[originalIndex, twinIndex] = AlignmentKind.Deleted;
                }
                else
                {
                    costs[originalIndex, twinIndex] = insertedCost;
                    operations[originalIndex, twinIndex] = AlignmentKind.Inserted;
                }
            }
        }

        List<AlignmentStep> reversed = [];
        int i = original.Count;
        int j = twin.Count;
        while (i > 0 || j > 0)
        {
            AlignmentKind operation = operations[i, j];
            switch (operation)
            {
                case AlignmentKind.Exact:
                case AlignmentKind.Adapted:
                case AlignmentKind.Changed:
                    reversed.Add(new AlignmentStep(operation, original[--i], twin[--j]));
                    break;
                case AlignmentKind.Deleted:
                    reversed.Add(new AlignmentStep(operation, original[--i], null));
                    break;
                case AlignmentKind.Inserted:
                    reversed.Add(new AlignmentStep(operation, null, twin[--j]));
                    break;
                default:
                    throw new InvalidOperationException($"Comment alignment stopped at ({i}, {j}).");
            }
        }

        reversed.Reverse();
        return reversed;
    }

    private static AlignmentKind Classify(CommentEntry original, CommentEntry twin)
    {
        if (original.Kind == twin.Kind && original.Text == twin.Text)
        {
            return AlignmentKind.Exact;
        }

        return original.Kind == twin.Kind
               && NormalizeFrameworkNames(original.Text) == NormalizeFrameworkNames(twin.Text)
            ? AlignmentKind.Adapted
            : AlignmentKind.Changed;
    }

    private static int GetCost(AlignmentKind kind) =>
        kind switch
        {
            AlignmentKind.Exact => 0,
            AlignmentKind.Adapted => 1,
            AlignmentKind.Changed => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

    private static IndexedComment? FindUnusedExact(
        IndexedComment original,
        IEnumerable<IndexedComment> twins,
        IReadOnlySet<int> usedTwin) =>
        twins.FirstOrDefault(twin =>
            !usedTwin.Contains(twin.Index)
            && twin.Entry.Kind == original.Entry.Kind
            && twin.Entry.Text == original.Entry.Text);

    private static string NormalizeFrameworkNames(string text)
    {
        string normalized = Regex.Replace(
            text,
            @"\b(?:System\.Windows\.Forms|Avalonia\.Controls)\b",
            "<ui-framework>",
            RegexOptions.CultureInvariant);
        normalized = Regex.Replace(
            normalized,
            @"\b(?:Windows Forms|WinForms|Avalonia)\b",
            "<ui-framework>",
            RegexOptions.CultureInvariant);
        normalized = Regex.Replace(
            normalized,
            @"\b(?:FormClosingEventArgs|WindowClosingEventArgs)\b",
            "<closing-event-args>",
            RegexOptions.CultureInvariant);
        return Regex.Replace(
            normalized,
            @"\b(?:Form|Window)\b",
            "<window>",
            RegexOptions.CultureInvariant);
    }

    private static CommentAdaptation NewAdaptation(CommentEntry original, CommentEntry twin) =>
        new()
        {
            Path = GetPath(original),
            OriginalPart = original.Part,
            OriginalLine = original.Line,
            TwinPart = twin.Part,
            TwinLine = twin.Line,
            OriginalText = original.Text,
            TwinText = twin.Text
        };

    private static FunctionalFinding NewMissingFinding(CommentEntry original) =>
        new()
        {
            Category = "comments",
            Code = "comment.missing",
            Path = GetPath(original),
            Message = $"Original comment at {original.Part}:{original.Line} is missing from its anchored twin member.",
            OriginalValue = original.Text,
            TwinValue = null
        };

    private static FunctionalFinding NewChangedFinding(CommentEntry original, CommentEntry twin) =>
        new()
        {
            Category = "comments",
            Code = "comment.changed",
            Path = GetPath(original),
            Message = $"Comment at {original.Part}:{original.Line} was altered at {twin.Part}:{twin.Line}.",
            OriginalValue = original.Text,
            TwinValue = twin.Text
        };

    private static FunctionalFinding NewDriftedFinding(CommentEntry original, CommentEntry twin) =>
        new()
        {
            Category = "comments",
            Code = "comment.drifted",
            Path = GetPath(original),
            Message = $"Comment at {original.Part}:{original.Line} drifted to {twin.Part}:{twin.Line} ({twin.Anchor}, {twin.Placement}).",
            OriginalValue = original.Text,
            TwinValue = twin.Text
        };

    private static string GetPath(CommentEntry comment) =>
        $"comment/{comment.Part}/{comment.Anchor}/{comment.Placement}/{comment.Order}";

    private static string GetComparablePart(string part) =>
        part.EndsWith(".axaml.cs", StringComparison.Ordinal)
            ? $"{part[..^".axaml.cs".Length]}.cs"
            : part;

    private enum AlignmentKind
    {
        None,
        Exact,
        Adapted,
        Changed,
        Deleted,
        Inserted
    }

    private sealed record IndexedComment(int Index, CommentEntry Entry);

    private sealed record CommentGroupKey(string Part, string Anchor, string Placement);

    private sealed record CommentPair(IndexedComment Original, IndexedComment Twin);

    private sealed record AlignmentStep(
        AlignmentKind Kind,
        IndexedComment? Original,
        IndexedComment? Twin);
}
