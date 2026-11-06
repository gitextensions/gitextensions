using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GitExtensions.ParityInventory;

// parity-scaffolding: Matches every declaration without collapsing overloads or guessing ambiguous pairs.
internal static class MemberMatcher
{
    public static IReadOnlyList<MemberMatch> Match(IEnumerable<MemberEntry> original, IEnumerable<MemberEntry> twin, IReadOnlyDictionary<string, string>? expectedParts = null)
    {
        ILookup<string, MemberEntry> leftGroups = original.ToLookup(InventoryComparer.MemberGroupKey, StringComparer.Ordinal);
        ILookup<string, MemberEntry> rightGroups = twin.ToLookup(InventoryComparer.MemberGroupKey, StringComparer.Ordinal);
        Dictionary<MemberEntry, Shape> shapes = [];
        List<MemberMatch> result = [];
        foreach (string groupKey in leftGroups.Select(group => group.Key).Concat(rightGroups.Select(group => group.Key)).Distinct(StringComparer.Ordinal))
        {
            List<MemberEntry> left = leftGroups[groupKey].ToList();
            List<MemberEntry> right = rightGroups[groupKey].ToList();
            bool overloaded = left.Count > 1 || right.Count > 1;
            List<(MemberEntry? Original, MemberEntry? Twin)> pairs = [];
            if (expectedParts is not null)
            {
                MatchUnique(member => Canonical(member.Signature) + "@" + expectedParts.GetValueOrDefault(member.Part, member.Part),
                    member => Canonical(member.Signature) + "@" + member.Part);
            }

            MatchUnique(member => Canonical(member.Signature));
            foreach (IGrouping<string, MemberEntry> identical in left.GroupBy(member => Canonical(member.Signature), StringComparer.Ordinal).ToArray())
            {
                MemberEntry[] matching = right.Where(member => Canonical(member.Signature) == identical.Key).ToArray();
                foreach ((MemberEntry source, MemberEntry target) in identical.Zip(matching))
                {
                    pairs.Add((source, target));
                    left.Remove(source);
                    right.Remove(target);
                }
            }

            MatchUnique(member => GetShape(member).Identity);
            MatchUnique(member => GetShape(member).Names);
            MatchUnique(member => GetShape(member).Arity);
            if (left.Count == 1 && right.Count == 1)
            {
                pairs.Add((left[0], right[0]));
                left.Clear();
                right.Clear();
            }

            pairs.AddRange(left.Select(member => ((MemberEntry?)member, (MemberEntry?)null)));
            pairs.AddRange(right.Select(member => ((MemberEntry?)null, (MemberEntry?)member)));
            Dictionary<string, int> used = new(StringComparer.Ordinal);
            foreach ((MemberEntry? source, MemberEntry? target) in pairs)
            {
                MemberEntry member = source ?? target!;
                string key = overloaded ? OverloadKey(member) : groupKey;
                int occurrence = used.GetValueOrDefault(key);
                used[key] = occurrence + 1;
                result.Add(new(occurrence == 0 ? key : $"{key}@{member.Part}#{member.Order}:{occurrence}", source, target));
            }

            void MatchUnique(Func<MemberEntry, string> keySelector, Func<MemberEntry, string>? twinKeySelector = null)
            {
                Dictionary<string, MemberEntry> candidates = right.GroupBy(twinKeySelector ?? keySelector, StringComparer.Ordinal)
                    .Where(group => group.Count() == 1).ToDictionary(group => group.Key, group => group.Single(), StringComparer.Ordinal);
                foreach (IGrouping<string, MemberEntry> group in left.GroupBy(keySelector, StringComparer.Ordinal)
                             .Where(group => group.Count() == 1).ToArray())
                {
                    if (candidates.TryGetValue(group.Key, out MemberEntry? match))
                    {
                        MemberEntry source = group.Single();
                        pairs.Add((source, match));
                        left.Remove(source);
                        right.Remove(match);
                    }
                }
            }
        }

        return result;

        Shape GetShape(MemberEntry member)
        {
            if (!shapes.TryGetValue(member, out Shape? shape))
            {
                shapes[member] = shape = ReadShape(member);
            }

            return shape;
        }
    }

    public static string OverloadKey(MemberEntry member)
    {
        Shape shape = ReadShape(member);
        return InventoryComparer.MemberGroupKey(member) + shape.Suffix;
    }

    private static Shape ReadShape(MemberEntry member)
    {
        if (member.Kind is not ("method" or "constructor" or "operator" or "conversion" or "indexer"))
        {
            return new(member.Signature, member.Signature, member.Signature, string.Empty);
        }

        string container = member.Kind == "constructor" ? member.Name : "__Member";
        MemberDeclarationSyntax declaration = CSharpSyntaxTree.ParseText($"class {container} {{ {member.Signature} {{ }} }}")
            .GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First().Members.Single();
        SeparatedSyntaxList<ParameterSyntax> parameters = declaration switch
        {
            BaseMethodDeclarationSyntax method => method.ParameterList.Parameters,
            IndexerDeclarationSyntax indexer => indexer.ParameterList.Parameters,
            _ => throw new InvalidDataException($"Cannot read overload signature '{member.Signature}'.")
        };
        MethodDeclarationSyntax? ordinary = declaration as MethodDeclarationSyntax;
        string generic = ordinary?.TypeParameterList?.NormalizeWhitespace().ToString() ?? string.Empty;
        string prefix = $"{ordinary?.TypeParameterList?.Parameters.Count ?? 0}:";
        string qualifier = ordinary?.ExplicitInterfaceSpecifier?.NormalizeWhitespace().ToString()
            ?? (declaration as IndexerDeclarationSyntax)?.ExplicitInterfaceSpecifier?.NormalizeWhitespace().ToString()
            ?? string.Empty;
        if (declaration is ConstructorDeclarationSyntax constructor && constructor.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            qualifier = "static";
        }
        else if (declaration is ConversionOperatorDeclarationSyntax conversion)
        {
            qualifier = (conversion.CheckedKeyword.IsKind(SyntaxKind.CheckedKeyword) ? "checked " : string.Empty)
                + conversion.Type.NormalizeWhitespace().ToString();
        }
        else if (declaration is OperatorDeclarationSyntax operation && operation.CheckedKeyword.IsKind(SyntaxKind.CheckedKeyword))
        {
            qualifier = "checked";
        }

        prefix += qualifier + ":";
        string parameterList = declaration is IndexerDeclarationSyntax indexed
            ? indexed.ParameterList.NormalizeWhitespace().ToString()
            : ((BaseMethodDeclarationSyntax)declaration).ParameterList.NormalizeWhitespace().ToString();
        return new(
            prefix + string.Join("|", parameters.Select(parameter =>
                parameter.Modifiers + ":" + parameter.Type?.NormalizeWhitespace().ToString())),
            prefix + string.Join("|", parameters.Select(parameter => parameter.Identifier.ValueText)),
            prefix + parameters.Count,
            (qualifier.Length == 0 ? string.Empty : $"[{qualifier}]") + generic + parameterList);
    }

    private static string Canonical(string signature) =>
        string.Join("|", SyntaxFactory.ParseTokens(signature).Select(token => $"{token.RawKind}:{token.Text}"));

    private sealed record Shape(string Identity, string Names, string Arity, string Suffix);
}

// parity-scaffolding: Preserves both sides of an individual overload for findings and exact-pin validation.
internal sealed record MemberMatch(string Key, MemberEntry? Original, MemberEntry? Twin);
