using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TranslationExtractor;

/// <summary>
///  Extracts <c>TranslationString</c> field declarations from C# source files (non-Designer).
///  Produces entries like: category=ClassName, name=fieldName, property="Text", source="English text".
/// </summary>
internal static class TranslationStringExtractor
{
    /// <summary>
    ///  Scans a C# source file for <c>TranslationString</c> field declarations.
    /// </summary>
    public static IEnumerable<TranslationEntry> Extract(string filePath, string sourceText)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText, path: filePath);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

        foreach (ClassDeclarationSyntax classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            string className = classDecl.Identifier.Text;

            foreach (FieldDeclarationSyntax field in classDecl.Members.OfType<FieldDeclarationSyntax>())
            {
                foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
                {
                    if (variable.Initializer?.Value is not ObjectCreationExpressionSyntax creation)
                    {
                        // Also handle implicit object creation: new("text")
                        if (variable.Initializer?.Value is not ImplicitObjectCreationExpressionSyntax implicitCreation)
                        {
                            continue;
                        }

                        if (!IsTranslationStringField(field))
                        {
                            continue;
                        }

                        string? text = ExtractStringArgument(implicitCreation.ArgumentList);
                        if (text is not null && ContainsLetter(text))
                        {
                            string fieldName = variable.Identifier.Text;
                            yield return new TranslationEntry(className, fieldName, "Text", text);
                        }

                        continue;
                    }

                    // Explicit: new TranslationString("text")
                    if (!IsTranslationStringType(creation.Type) && !IsTranslationStringField(field))
                    {
                        continue;
                    }

                    string? sourceText2 = ExtractStringArgument(creation.ArgumentList);
                    if (sourceText2 is not null && ContainsLetter(sourceText2))
                    {
                        string fieldName = variable.Identifier.Text;
                        yield return new TranslationEntry(className, fieldName, "Text", sourceText2);
                    }
                }
            }
        }
    }

    private static bool IsTranslationStringField(FieldDeclarationSyntax field)
    {
        // Check if the type is TranslationString
        string typeName = field.Declaration.Type.ToString();
        return typeName is "TranslationString" or "ResourceManager.TranslationString"
            or "GitExtensions.Extensibility.Translations.TranslationString";
    }

    private static bool IsTranslationStringType(TypeSyntax type)
    {
        string typeName = type.ToString();
        return typeName is "TranslationString" or "ResourceManager.TranslationString"
            or "GitExtensions.Extensibility.Translations.TranslationString";
    }

    private static string? ExtractStringArgument(ArgumentListSyntax? argumentList)
    {
        if (argumentList is null || argumentList.Arguments.Count == 0)
        {
            return null;
        }

        ExpressionSyntax expr = argumentList.Arguments[0].Expression;
        return ExtractStringValue(expr);
    }

    internal static string? ExtractStringValue(ExpressionSyntax expr)
    {
        switch (expr)
        {
            case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression):
                return literal.Token.ValueText;

            case InterpolatedStringExpressionSyntax interpolated:
                return ExtractInterpolatedString(interpolated);

            // Handle string concatenation: "part1" + "part2"
            case BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.AddExpression):
            {
                string? left = ExtractStringValue(binary.Left);
                string? right = ExtractStringValue(binary.Right);
                if (left is not null && right is not null)
                {
                    return left + right;
                }

                return null;
            }

            // Handle verbatim strings @"..."
            case LiteralExpressionSyntax verbatimLiteral when verbatimLiteral.Token.IsVerbatimStringLiteral():
                return verbatimLiteral.Token.ValueText;

            // Handle raw string literals
            case LiteralExpressionSyntax rawLiteral when rawLiteral.IsKind(SyntaxKind.Utf8StringLiteralExpression):
                return rawLiteral.Token.ValueText;

            // Handle well-known constant member accesses (Environment.NewLine, etc.)
            case MemberAccessExpressionSyntax memberAccess:
                return ResolveKnownConstant(memberAccess);

            // Handle parenthesized expressions
            case ParenthesizedExpressionSyntax paren:
                return ExtractStringValue(paren.Expression);

            default:
                return null;
        }
    }

    /// <summary>
    ///  Resolves well-known constant member accesses to their string values.
    /// </summary>
    private static string? ResolveKnownConstant(MemberAccessExpressionSyntax memberAccess)
    {
        string fullName = memberAccess.ToString();
        return fullName switch
        {
            "Environment.NewLine" => "\n",
            _ => null,
        };
    }

    /// <summary>
    ///  Extracts string value from interpolated strings where all interpolations
    ///  are resolvable constants (e.g. Environment.NewLine).
    ///  Returns null if any interpolation contains non-resolvable expressions.
    /// </summary>
    private static string? ExtractInterpolatedString(InterpolatedStringExpressionSyntax interpolated)
    {
        System.Text.StringBuilder sb = new();
        foreach (InterpolatedStringContentSyntax content in interpolated.Contents)
        {
            switch (content)
            {
                case InterpolatedStringTextSyntax text:
                    sb.Append(text.TextToken.ValueText);
                    break;

                case InterpolationSyntax interpolation:
                    string? resolved = ExtractStringValue(interpolation.Expression);
                    if (resolved is null)
                    {
                        return null;
                    }

                    sb.Append(resolved);
                    break;

                default:
                    return null;
            }
        }

        return sb.ToString();
    }

    private static bool ContainsLetter(string text)
    {
        return text.Any(char.IsLetter);
    }
}
