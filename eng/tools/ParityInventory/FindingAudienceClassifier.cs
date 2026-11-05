using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GitExtensions.ParityInventory;

// parity-scaffolding: Partitions findings without suppressing test support or product differences.
internal static class FindingAudienceClassifier
{
    public static InventoryComparison Classify(
        SourceInventory original,
        SourceInventory twin,
        InventoryComparison comparison)
    {
        MemberEntry[] members = original.Members.Concat(twin.Members).ToArray();
        HashSet<string> testMembers = members.Where(IsTestOnlyMember)
            .Select(member => "member/" + InventoryComparer.MemberKey(member))
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> testComments = original.Comments.Concat(twin.Comments)
            .Where(comment => IsTestOnlyAnchor(comment.Anchor, members))
            .Select(comment => $"comment/{comment.Part}/{comment.Anchor}/{comment.Placement}/{comment.Order}")
            .ToHashSet(StringComparer.Ordinal);

        return comparison with
        {
            Findings = comparison.Findings.Select(ClassifyFinding).ToArray(),
            DependentFindings = comparison.DependentFindings.Select(dependent => dependent with
            {
                Finding = ClassifyFinding(dependent.Finding)
            }).ToArray()
        };

        FunctionalFinding ClassifyFinding(FunctionalFinding finding) => finding with
        {
            IsTestOnly = testComments.Contains(finding.Path)
                || testMembers.Contains(finding.Path)
                || testMembers.Any(path => finding.Path.StartsWith(path + "/", StringComparison.Ordinal))
        };
    }

    private static bool IsTestOnlyMember(MemberEntry member) => IsTestScope(member.ContainingType)
        || (member.Kind is "class" or "struct" or "interface" or "record" && member.Name == "TestAccessor")
        || IsAccessorEntryPoint(member);

    private static bool IsTestScope(string? containingType) =>
        containingType?.Split('.').Contains("TestAccessor", StringComparer.Ordinal) == true;

    private static bool IsAccessorEntryPoint(MemberEntry member)
    {
        if (member.Kind != "method" || member.Name != "GetTestAccessor")
        {
            return false;
        }

        int separator = member.Signature.IndexOf(" GetTestAccessor", StringComparison.Ordinal);
        if (separator < 0)
        {
            return false;
        }

        TypeSyntax type = SyntaxFactory.ParseTypeName(member.Signature[..separator]);
        if (type is NullableTypeSyntax nullable)
        {
            type = nullable.ElementType;
        }

        return type switch
        {
            SimpleNameSyntax simple => simple.Identifier.ValueText == "TestAccessor",
            QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText == "TestAccessor",
            AliasQualifiedNameSyntax alias => alias.Name.Identifier.ValueText == "TestAccessor",
            _ => false
        };
    }

    private static bool IsTestOnlyAnchor(string anchor, IReadOnlyList<MemberEntry> members) => anchor.Split('/')
        .Any(part => part is "class:TestAccessor" or "struct:TestAccessor" or "interface:TestAccessor")
        || (anchor.StartsWith("method:GetTestAccessor(", StringComparison.Ordinal)
            && members.Any(IsAccessorEntryPoint));
}
