using GitExtensions.ParityInventory;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GitExtensions.ParityFix;

// parity-scaffolding: Restores only mechanically anchored trivia and same-part declaration order.
internal static class MechanicalFixer
{
    private static readonly string[] DeviationMarkers = ["Avalonia", "WinForms", "cross-platform", "framework", "parity-scaffolding"];

    public static FileFix Fix(string path, string originalText, string twinText, string typeName,
        IReadOnlyList<FunctionalFinding> findings, IReadOnlyList<CommentEntry> originalComments,
        IReadOnlyList<CommentEntry> twinComments, bool allowInitializers = false)
    {
        SyntaxNode original = Parse(originalText);
        SyntaxNode twin = Parse(twinText);
        List<string> unresolved = [];
        if (original.ContainsDiagnostics || twin.ContainsDiagnostics)
        {
            return new(path, twinText, 0, false, [], ["File has syntax diagnostics."], false);
        }

        TypeDeclarationSyntax? originalType = FindType(original, typeName);
        TypeDeclarationSyntax? twinType = FindType(twin, typeName);
        if (originalType is null || twinType is null)
        {
            return new(path, twinText, 0, false, [], ["Type is absent or ambiguous."], false);
        }

        List<TextChange> changes = [];
        int restored = 0;
        foreach (FunctionalFinding finding in findings.Where(item => item.Code.StartsWith("comment.", StringComparison.Ordinal))
                     .OrderBy(item => originalComments.FirstOrDefault(entry =>
                         $"comment/{entry.Part}/{entry.Anchor}/{entry.Placement}/{entry.Order}" == item.Path)?.Line ?? int.MaxValue))
        {
            CommentEntry[] entries = originalComments.Where(entry =>
                $"comment/{entry.Part}/{entry.Anchor}/{entry.Placement}/{entry.Order}" == finding.Path
                && entry.Text == finding.OriginalValue).ToArray();
            if (entries.Length != 1 || !TryComment(entries[0], finding, originalType, twinType,
                    originalText, twinText, twinComments, changes))
            {
                unresolved.Add($"{finding.Code}: {finding.Path} (no unique unchanged anchor)");
                continue;
            }

            restored++;
        }

        string commentedText;
        try
        {
            commentedText = SourceText.From(twinText).WithChanges(changes).ToString();
        }
        catch (ArgumentException)
        {
            return new(path, twinText, 0, false, [], ["Comment edits overlap; file left untouched."], false);
        }

        SyntaxNode commented = Parse(commentedText);
        if (commented.ContainsDiagnostics || !SyntaxFactory.AreEquivalent(twin, commented)
            || !DirectiveTexts(twin).SequenceEqual(DirectiveTexts(commented)))
        {
            return new(path, twinText, 0, false, [], ["Comment syntax-equivalence proof failed; file left untouched."], false);
        }

        TypeDeclarationSyntax commentedType = FindType(commented, typeName)!;
        List<string> initialized = [];
        SyntaxNode ordered = commented;
        bool orderChanged = false;
        if (findings.Any(item => item.Code == "member.order"))
        {
            MemberDeclarationSyntax[] members = commentedType.Members.ToArray();
            Dictionary<string, int> ranks = originalType.Members
                .Select((member, index) => (Key: OrderKey(member), Index: index))
                .GroupBy(item => item.Key, StringComparer.Ordinal)
                .Where(group => group.Count() == 1)
                .ToDictionary(group => group.Key, group => group.Single().Index, StringComparer.Ordinal);
            HashSet<string> unique = members.GroupBy(OrderKey, StringComparer.Ordinal)
                .Where(group => group.Count() == 1).Select(group => group.Key).ToHashSet(StringComparer.Ordinal);

            // A directive-bearing member is a barrier: regions and preprocessor branches must stay in place.
            int start = 0;
            while (start < members.Length)
            {
                if (members[start].ContainsDirectives)
                {
                    start++;
                    continue;
                }

                int end = start;
                while (end < members.Length && !members[end].ContainsDirectives)
                {
                    end++;
                }

                int[] slots = Enumerable.Range(start, end - start)
                    .Where(index => unique.Contains(OrderKey(members[index])) && ranks.ContainsKey(OrderKey(members[index])))
                    .ToArray();
                MemberDeclarationSyntax[] sorted = slots.Select(index => members[index]).OrderBy(member => ranks[OrderKey(member)]).ToArray();
                for (int index = 0; index < slots.Length; index++)
                {
                    if (members[slots[index]] != sorted[index])
                    {
                        initialized.AddRange(InitializedNames(members[slots[index]]));
                        initialized.AddRange(InitializedNames(sorted[index]));
                    }
                }

                for (int index = 0; index < slots.Length; index++)
                {
                    members[slots[index]] = sorted[index];
                }

                start = end;
            }

            initialized = initialized.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
            if (initialized.Count > 0 && !allowInitializers)
            {
                unresolved.Add("Member order requires hand review of initialized fields/properties: " + string.Join(", ", initialized));
            }
            else
            {
                orderChanged = !members.SequenceEqual(commentedType.Members);
                ordered = commented.ReplaceNode(commentedType, commentedType.WithMembers(SyntaxFactory.List(members)));
            }
        }

        SyntaxNode reparsed = Parse(ordered.ToFullString());
        bool proof = !reparsed.ContainsDiagnostics && MemberMultisetEquivalent(commented, reparsed, typeName);
        return proof
            ? new(path, ordered.ToFullString(), restored, orderChanged, initialized, unresolved, true)
            : new(path, twinText, 0, false, initialized, ["Member-multiset proof failed; file left untouched."], false);
    }

    internal static bool MemberMultisetEquivalent(SyntaxNode before, SyntaxNode after, string typeName)
    {
        TypeDeclarationSyntax? left = FindType(before, typeName);
        TypeDeclarationSyntax? right = FindType(after, typeName);
        return left is not null && right is not null
            && DirectiveTexts(before).SequenceEqual(DirectiveTexts(after))
            && left.Members.Select(member => member.ToFullString()).Order(StringComparer.Ordinal)
                .SequenceEqual(right.Members.Select(member => member.ToFullString()).Order(StringComparer.Ordinal))
            && SyntaxFactory.AreEquivalent(before.ReplaceNode(left, left.WithMembers(default)), after.ReplaceNode(right, right.WithMembers(default)));
    }

    private static bool TryComment(CommentEntry entry, FunctionalFinding finding,
        TypeDeclarationSyntax originalType, TypeDeclarationSyntax twinType,
        string originalText, string twinText, IReadOnlyList<CommentEntry> twinComments, List<TextChange> changes)
    {
        SyntaxTrivia[] originals = originalType.DescendantTrivia().Where(SourceInventoryReader.IsComment)
            .Where(trivia => SourceInventoryReader.NormalizeCommentText(trivia) == entry.Text
                && trivia.SyntaxTree!.GetLineSpan(trivia.Span).StartLinePosition.Line + 1 == entry.Line).ToArray();
        if (originals.Length != 1)
        {
            return false;
        }

        SyntaxTrivia source = originals[0];
        MemberDeclarationSyntax? sourceMember = Owner(source, originalType);
        if (sourceMember is null || SourceInventoryReader.GetCommentAnchor(originalType, sourceMember) != entry.Anchor)
        {
            return false;
        }

        MemberDeclarationSyntax[] candidates = twinType.DescendantNodes().OfType<MemberDeclarationSyntax>()
            .Where(member => SourceInventoryReader.GetCommentAnchor(twinType, member) == entry.Anchor).ToArray();
        if (candidates.Length != 1)
        {
            return false;
        }

        MemberDeclarationSyntax target = candidates[0];

        // Changed comments may explain a framework deviation; only an unchanged member permits restoration.
        bool unchangedMember = SyntaxFactory.AreEquivalent(sourceMember, target);
        if (finding.Code == "comment.changed" && !SyntaxFactory.AreEquivalent(originalType, twinType))
        {
            return false;
        }

        if (finding.Code == "comment.changed" && DeviationMarkers.Any(marker => finding.TwinValue?.Contains(marker, StringComparison.OrdinalIgnoreCase) == true))
        {
            return false;
        }

        SyntaxNode sourceBoundary = sourceMember;
        SyntaxNode targetBoundary = target;
        if (!unchangedMember)
        {
            if (entry.Placement != "body" || finding.Code == "comment.changed")
            {
                return false;
            }

            SyntaxToken nextToken = sourceMember.DescendantTokens().FirstOrDefault(token => token.SpanStart >= source.FullSpan.End);
            StatementSyntax? statement = nextToken.Parent?.AncestorsAndSelf().TakeWhile(node => node != sourceMember)
                .OfType<StatementSyntax>().LastOrDefault(node => node is not BlockSyntax);
            if (statement is null || !sourceMember.Span.Contains(statement.Span))
            {
                return false;
            }

            StatementSyntax[] matchingStatements = TopLevelStatements(target)
                .Where(node => SyntaxFactory.AreEquivalent(statement, node)).ToArray();
            if (matchingStatements.Length != 1 || TopLevelStatements(sourceMember)
                    .Count(node => SyntaxFactory.AreEquivalent(statement, node)) != 1)
            {
                return false;
            }

            sourceBoundary = statement;
            targetBoundary = matchingStatements[0];
        }

        SyntaxTrivia? removed = null;
        if (finding.Code is "comment.changed" or "comment.drifted")
        {
            CommentEntry[] matches = twinComments.Where(comment => comment.Text == finding.TwinValue
                && (finding.Code == "comment.drifted" || comment.Anchor == entry.Anchor)).ToArray();
            if (matches.Length != 1)
            {
                return false;
            }

            SyntaxTrivia[] triviaMatches = twinType.DescendantTrivia().Where(SourceInventoryReader.IsComment)
                .Where(trivia => SourceInventoryReader.NormalizeCommentText(trivia) == matches[0].Text
                    && trivia.SyntaxTree!.GetLineSpan(trivia.Span).StartLinePosition.Line + 1 == matches[0].Line).ToArray();
            if (triviaMatches.Length != 1)
            {
                return false;
            }

            removed = triviaMatches[0];
            if (finding.Code == "comment.changed")
            {
                string[] sourceReferences = source.GetStructure()?.DescendantNodes().OfType<XmlCrefAttributeSyntax>()
                    .Select(attribute => attribute.Cref.ToString()).ToArray() ?? [];
                string[] targetReferences = removed.Value.GetStructure()?.DescendantNodes().OfType<XmlCrefAttributeSyntax>()
                    .Select(attribute => attribute.Cref.ToString()).ToArray() ?? [];
                if (!sourceReferences.SequenceEqual(targetReferences))
                {
                    return false;
                }

                changes.Add(new TextChange(removed.Value.FullSpan, Reindent(source.ToFullString(), originalText, source.FullSpan.Start, twinText, removed.Value.FullSpan.Start)));
                return true;
            }
        }

        if (twinComments.Any(comment => comment.Anchor == entry.Anchor && comment.Text == entry.Text))
        {
            return false;
        }

        SyntaxToken[] sourceTokens = sourceBoundary.DescendantTokens().ToArray();
        SyntaxToken[] targetTokens = targetBoundary.DescendantTokens().ToArray();
        int next = Array.FindIndex(sourceTokens, token => token.SpanStart >= source.FullSpan.End);
        if (next < 0 || next >= targetTokens.Length || !Standalone(originalText, source.FullSpan.Start))
        {
            return false;
        }

        SyntaxToken targetToken = targetTokens[next];
        SyntaxTrivia[] sourceGroup = sourceTokens[next].LeadingTrivia.Where(SourceInventoryReader.IsComment).ToArray();
        SyntaxTrivia[] targetGroup = targetToken.LeadingTrivia.Where(SourceInventoryReader.IsComment).ToArray();
        if (targetGroup.Any(comment => !sourceGroup.Any(candidate =>
                SourceInventoryReader.NormalizeCommentText(candidate) == SourceInventoryReader.NormalizeCommentText(comment))))
        {
            return false;
        }

        int position = LineStart(twinText, targetToken.SpanStart);
        if (!Standalone(twinText, targetToken.SpanStart))
        {
            return false;
        }

        string indent = twinText[position..targetToken.SpanStart];
        foreach (SyntaxTrivia following in sourceGroup.Where(comment => comment.FullSpan.Start > source.FullSpan.Start))
        {
            SyntaxTrivia[] matching = targetGroup.Where(comment =>
                SourceInventoryReader.NormalizeCommentText(comment) == SourceInventoryReader.NormalizeCommentText(following)).ToArray();
            if (matching.Length > 1)
            {
                return false;
            }

            if (matching.Length == 1)
            {
                if (!Standalone(twinText, matching[0].FullSpan.Start))
                {
                    return false;
                }

                position = LineStart(twinText, matching[0].FullSpan.Start);
                break;
            }
        }

        string text = Reindent(source.ToFullString(), originalText, source.FullSpan.Start, twinText, targetToken.SpanStart).TrimEnd('\r', '\n');
        if (removed is { } drifted)
        {
            if (!Standalone(twinText, drifted.FullSpan.Start))
            {
                return false;
            }

            int removeStart = LineStart(twinText, drifted.FullSpan.Start);
            int removeEnd = drifted.FullSpan.End;
            if (removeEnd == 0 || twinText[removeEnd - 1] != '\n')
            {
                int newline = twinText.IndexOf('\n', removeEnd);
                if (newline >= 0)
                {
                    removeEnd = newline + 1;
                }
            }

            changes.Add(new TextChange(TextSpan.FromBounds(removeStart, removeEnd), string.Empty));
        }

        string separator = BlankPreviousLine(originalText, LineStart(originalText, source.FullSpan.Start))
            && !BlankPreviousLine(twinText, position) ? "\r\n" : string.Empty;
        changes.Add(new TextChange(new TextSpan(position, 0), separator + indent + text + "\r\n"));
        return true;
    }

    private static string Reindent(string text, string source, int sourcePosition, string target, int targetPosition)
    {
        int sourceIndent = sourcePosition - LineStart(source, sourcePosition);
        string targetIndent = new(' ', targetPosition - LineStart(target, targetPosition));
        return string.Join("\r\n", text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n')
            .Select((line, index) => index == 0 || line.Length == 0 ? line : targetIndent + line[Math.Min(sourceIndent, line.TakeWhile(char.IsWhiteSpace).Count())..]));
    }

    private static MemberDeclarationSyntax? Owner(SyntaxTrivia trivia, TypeDeclarationSyntax type) =>
        trivia.Token.Parent?.AncestorsAndSelf().OfType<MemberDeclarationSyntax>().FirstOrDefault(member => member != type);

    private static IEnumerable<StatementSyntax> TopLevelStatements(MemberDeclarationSyntax member) =>
        member.DescendantNodes().OfType<StatementSyntax>().Where(node => node is not BlockSyntax
            && !node.Ancestors().TakeWhile(ancestor => ancestor != member).OfType<StatementSyntax>().Any(ancestor => ancestor is not BlockSyntax));

    private static TypeDeclarationSyntax? FindType(SyntaxNode root, string name) =>
        root.DescendantNodes().OfType<TypeDeclarationSyntax>().SingleOrDefault(type => SourceInventoryReader.GetTypeName(type) == name);

    private static string OrderKey(MemberDeclarationSyntax member)
    {
        string anchor = SourceInventoryReader.GetMemberAnchor(member);
        int parameters = anchor.IndexOf('(');
        return parameters < 0 ? anchor : anchor[..parameters];
    }

    private static IEnumerable<string> InitializedNames(MemberDeclarationSyntax member) => member switch
    {
        FieldDeclarationSyntax field => field.Declaration.Variables.Where(variable => variable.Initializer is not null).Select(variable => variable.Identifier.ValueText),
        EventFieldDeclarationSyntax field => field.Declaration.Variables.Where(variable => variable.Initializer is not null).Select(variable => variable.Identifier.ValueText),
        PropertyDeclarationSyntax { Initializer: not null } property => [property.Identifier.ValueText],
        _ => []
    };

    private static int LineStart(string text, int position) => position == 0 ? 0 : text.LastIndexOf('\n', position - 1) + 1;

    private static bool Standalone(string text, int position) => text[LineStart(text, position)..position].All(char.IsWhiteSpace);

    private static bool BlankPreviousLine(string text, int position) => position == 0
        || text[LineStart(text, position - 1)..position].All(char.IsWhiteSpace);

    private static IEnumerable<string> DirectiveTexts(SyntaxNode node) => node.DescendantTrivia()
        .Where(trivia => trivia.GetStructure() is DirectiveTriviaSyntax).Select(trivia => trivia.ToFullString());

    private static SyntaxNode Parse(string text) => CSharpSyntaxTree.ParseText(text, new CSharpParseOptions(LanguageVersion.Preview)).GetRoot();
}
