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
            foreach (TypeDeclarationSyntax nested in originalType.Members.OfType<TypeDeclarationSyntax>())
            {
                string nestedName = SourceInventoryReader.GetTypeName(nested);
                if (FindType(ordered, nestedName) is null)
                {
                    continue;
                }

                FileFix nestedFix = Fix(path, originalText, ordered.ToFullString(), nestedName,
                    findings.Where(item => item.Code == "member.order").ToArray(), [], [], allowInitializers);
                initialized.AddRange(nestedFix.InitializedFields.Select(name =>
                    name.StartsWith(nestedName + ".", StringComparison.Ordinal) ? name : nestedName + "." + name));
                unresolved.AddRange(nestedFix.Unresolved);
                if (!nestedFix.ProofPassed)
                {
                    return new(path, twinText, 0, false, initialized, unresolved, false);
                }

                orderChanged |= nestedFix.OrderChanged;
                ordered = Parse(nestedFix.Text);
            }

            commented = ordered;
            commentedType = FindType(commented, typeName)!;
            MemberDeclarationSyntax[] members = commentedType.Members.ToArray();
            IReadOnlyList<MemberEntry> primaryMembers = SourceInventoryReader.GetPrimaryMembers(originalType);
            Dictionary<string, int> ranks = originalType.Members
                .Select((member, index) => (Key: OrderKey(member), Index: index + primaryMembers.Count))
                .Concat(primaryMembers.Select(member => (Key: $"{member.Kind}:{member.Name}", Index: member.Order)))
                .GroupBy(item => item.Key, StringComparer.Ordinal)
                .Where(group => group.Count() == 1)
                .ToDictionary(group => group.Key, group => group.Single().Index, StringComparer.Ordinal);
            HashSet<string> unique = members.GroupBy(OrderKey, StringComparer.Ordinal)
                .Where(group => group.Count() == 1).Select(group => group.Key).ToHashSet(StringComparer.Ordinal);

            HashSet<SyntaxTrivia> movableWarnings = MovableWarningDirectives(commented, commentedType);

            // Regions, conditional branches and warning scopes outside a member remain barriers.
            int start = 0;
            while (start < members.Length)
            {
                if (HasOrderBarrier(members[start], movableWarnings))
                {
                    start++;
                    continue;
                }

                int end = start;
                while (end < members.Length && !HasOrderBarrier(members[end], movableWarnings))
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
                orderChanged |= !members.SequenceEqual(commentedType.Members);
                ordered = commented.ReplaceNode(commentedType, commentedType.WithMembers(SyntaxFactory.List(members)));
            }
        }

        SyntaxNode reparsed = Parse(ordered.ToFullString());
        bool proof = !reparsed.ContainsDiagnostics && MemberMultisetEquivalent(Parse(commentedText), reparsed, typeName);
        return proof
            ? new(path, ordered.ToFullString(), restored, orderChanged, initialized, unresolved, true)
            : new(path, twinText, 0, false, initialized, ["Member-multiset proof failed; file left untouched."], false);
    }

    internal static bool MemberMultisetEquivalent(SyntaxNode before, SyntaxNode after, string typeName)
    {
        TypeDeclarationSyntax? left = FindType(before, typeName);
        TypeDeclarationSyntax? right = FindType(after, typeName);
        return left is not null && right is not null
            && OrderDirectiveTexts(before, left).SequenceEqual(OrderDirectiveTexts(after, right))
            && CanonicalMembers(left).SequenceEqual(CanonicalMembers(right))
            && SyntaxFactory.AreEquivalent(before.ReplaceNode(left, left.WithMembers(default)), after.ReplaceNode(right, right.WithMembers(default)));
    }

    private static IEnumerable<string> CanonicalMembers(TypeDeclarationSyntax type) => type.Members
        .Select(member => member is TypeDeclarationSyntax nested
            ? nested.WithMembers(default).ToFullString() + string.Join("\0", CanonicalMembers(nested))
            : member.ToFullString())
        .Order(StringComparer.Ordinal);

    private static bool HasOrderBarrier(MemberDeclarationSyntax member, HashSet<SyntaxTrivia> movableWarnings) =>
        member.DescendantTrivia().Any(trivia => trivia.IsDirective && !movableWarnings.Contains(trivia));

    private static IEnumerable<string> OrderDirectiveTexts(SyntaxNode root, TypeDeclarationSyntax type)
    {
        HashSet<SyntaxTrivia> movableWarnings = MovableWarningDirectives(root, type);
        return root.DescendantTrivia().Where(trivia => trivia.IsDirective && !movableWarnings.Contains(trivia))
            .Select(trivia => trivia.ToFullString());
    }

    private static HashSet<SyntaxTrivia> MovableWarningDirectives(SyntaxNode root, TypeDeclarationSyntax type)
    {
        HashSet<SyntaxTrivia> movable = [];
        foreach (MemberDeclarationSyntax member in type.Members)
        {
            SyntaxTrivia[] directives = member.DescendantTrivia().Where(trivia => trivia.IsDirective).ToArray();
            if (member is not BaseMethodDeclarationSyntax || directives.Length == 0 || directives.Length % 2 != 0
                || directives.Any(trivia => !member.Span.Contains(trivia.Span)))
            {
                continue;
            }

            bool balanced = true;
            for (int index = 0; index < directives.Length; index += 2)
            {
                if (directives[index].GetStructure() is not PragmaWarningDirectiveTriviaSyntax disable
                    || directives[index + 1].GetStructure() is not PragmaWarningDirectiveTriviaSyntax restore
                    || !disable.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword)
                    || !restore.DisableOrRestoreKeyword.IsKind(SyntaxKind.RestoreKeyword)
                    || disable.ErrorCodes.Count == 0
                    || !disable.ErrorCodes.Select(code => code.ToString()).SequenceEqual(restore.ErrorCodes.Select(code => code.ToString())))
                {
                    balanced = false;
                    break;
                }
            }

            if (balanced)
            {
                movable.UnionWith(directives);
            }
        }

        // An ambient warning directive could make a local restore change the following member's scope.
        return root.DescendantTrivia().Any(trivia => trivia.GetStructure() is PragmaWarningDirectiveTriviaSyntax && !movable.Contains(trivia))
            ? [] : movable;
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
        if (candidates.Length == 0)
        {
            string relaxed = RelaxAnchor(entry.Anchor);
            candidates = twinType.DescendantNodes().OfType<MemberDeclarationSyntax>()
                .Where(member => RelaxAnchor(SourceInventoryReader.GetCommentAnchor(twinType, member)) == relaxed).ToArray();
            if (originalType.DescendantNodes().OfType<MemberDeclarationSyntax>()
                    .Count(member => RelaxAnchor(SourceInventoryReader.GetCommentAnchor(originalType, member)) == relaxed) != 1)
            {
                return false;
            }
        }

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

        SyntaxToken sourceToken;
        SyntaxToken targetToken;
        bool after = false;
        if (entry.Placement == "leading")
        {
            sourceToken = sourceMember.GetFirstToken();
            targetToken = target.GetFirstToken();
        }
        else if (unchangedMember)
        {
            SyntaxToken[] sourceTokens = sourceMember.DescendantTokens().ToArray();
            int next = Array.FindIndex(sourceTokens, token => token.SpanStart >= source.FullSpan.End);
            if (next < 0)
            {
                return false;
            }

            sourceToken = sourceTokens[next];
            targetToken = target.DescendantTokens().ElementAt(next);
        }
        else
        {
            if (entry.Placement != "body" || finding.Code == "comment.changed")
            {
                return false;
            }

            if (!TryStatementAnchor(source, sourceMember, target, out sourceToken, out targetToken, out after))
            {
                return false;
            }
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

        if (!Standalone(originalText, source.FullSpan.Start))
        {
            return false;
        }

        SyntaxTrivia[] sourceGroup = after ? [] : sourceToken.LeadingTrivia.Where(SourceInventoryReader.IsComment).ToArray();
        SyntaxTrivia[] targetGroup = after ? [] : targetToken.LeadingTrivia.Where(SourceInventoryReader.IsComment).ToArray();
        if (targetGroup.Any(comment => !sourceGroup.Any(candidate =>
                SourceInventoryReader.NormalizeCommentText(candidate) == SourceInventoryReader.NormalizeCommentText(comment))))
        {
            return false;
        }

        int indentPosition = after
            ? targetToken.Parent!.AncestorsAndSelf().OfType<StatementSyntax>().First().GetFirstToken().SpanStart
            : targetToken.SpanStart;
        int position = LineStart(twinText, indentPosition);
        if (!Standalone(twinText, indentPosition))
        {
            return false;
        }

        string indent = twinText[position..indentPosition];
        if (after)
        {
            int newline = twinText.IndexOf('\n', targetToken.Span.End);
            if (newline < 0 || !twinText[targetToken.Span.End..newline].All(char.IsWhiteSpace))
            {
                return false;
            }

            position = newline + 1;
        }

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

        string text = Reindent(source.ToFullString(), originalText, source.FullSpan.Start, twinText, indentPosition).TrimEnd('\r', '\n');
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

    private static bool TryStatementAnchor(SyntaxTrivia comment, MemberDeclarationSyntax sourceMember,
        MemberDeclarationSyntax targetMember, out SyntaxToken sourceToken, out SyntaxToken targetToken, out bool after)
    {
        sourceToken = default;
        targetToken = default;
        after = false;
        StatementSyntax[] sourceStatements = Statements(sourceMember);
        StatementSyntax[] targetStatements = Statements(targetMember);
        string[] commentContext = comment.Token.Parent?.AncestorsAndSelf().TakeWhile(node => node != sourceMember)
            .Where(node => node.Span.Contains(comment.Span) && IsContext(node)).Select(ContextKey).ToArray() ?? [];
        foreach (StatementSyntax statement in sourceStatements
                     .OrderBy(node => node.Span.Contains(comment.Span) ? 0 : Math.Min(Math.Abs(node.SpanStart - comment.FullSpan.End), Math.Abs(node.Span.End - comment.FullSpan.Start)))
                     .ThenBy(node => node.Span.Length))
        {
            StatementSyntax[] matches = targetStatements.Where(node => SyntaxFactory.AreEquivalent(statement, node)).ToArray();
            if (matches.Length != 1 || sourceStatements.Count(node => SyntaxFactory.AreEquivalent(statement, node)) != 1
                || (!statement.Span.Contains(comment.Span) && !commentContext.SequenceEqual(StatementContext(statement, sourceMember)))
                || !StatementContext(statement, sourceMember).SequenceEqual(StatementContext(matches[0], targetMember)))
            {
                continue;
            }

            after = statement.Span.End <= comment.FullSpan.Start;
            if (after)
            {
                sourceToken = statement.GetLastToken();
                targetToken = matches[0].GetLastToken();
                return true;
            }

            SyntaxToken[] tokens = statement.DescendantTokens().ToArray();
            int next = Array.FindIndex(tokens, token => token.SpanStart >= comment.FullSpan.End);
            if (next >= 0)
            {
                sourceToken = tokens[next];
                targetToken = matches[0].DescendantTokens().ElementAt(next);
                return true;
            }
        }

        return false;
    }

    private static StatementSyntax[] Statements(MemberDeclarationSyntax member) => member.DescendantNodes()
        .OfType<StatementSyntax>().Where(node => node is not BlockSyntax
            && !node.Ancestors().TakeWhile(ancestor => ancestor != member).OfType<MemberDeclarationSyntax>().Any()).ToArray();

    private static IEnumerable<string> StatementContext(StatementSyntax statement, MemberDeclarationSyntax member) =>
        statement.Ancestors().TakeWhile(node => node != member).Where(IsContext).Select(ContextKey);

    private static bool IsContext(SyntaxNode node) => node is StatementSyntax and not BlockSyntax
        or ElseClauseSyntax or CatchClauseSyntax or FinallyClauseSyntax or SwitchSectionSyntax or AnonymousFunctionExpressionSyntax;

    private static string ContextKey(SyntaxNode node) => node.RawKind + ":"
        + (node is LocalFunctionStatementSyntax local ? local.Identifier.ValueText : string.Empty)
        + string.Concat(node.ChildNodes()
            .Where(child => child is not (StatementSyntax or ElseClauseSyntax or CatchClauseSyntax or FinallyClauseSyntax or SwitchSectionSyntax))
            .SelectMany(child => child.DescendantTokens()).Select(token => token.Text));

    private static string RelaxAnchor(string anchor) => string.Join('/', anchor.Split('/').Select(segment =>
        segment.Contains('(') ? segment[..segment.IndexOf('(')] : segment));

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
