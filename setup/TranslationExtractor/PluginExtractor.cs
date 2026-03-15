using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TranslationExtractor;

/// <summary>
///  Extracts translatable entries from plugin source files.
///  Handles: Description property (from GitPluginBase), ISetting Caption fields, and TranslationString fields.
/// </summary>
internal static class PluginExtractor
{
    /// <summary>
    ///  Scans a plugin C# file for:
    ///  1. The plugin's Name (which becomes the Description entry)
    ///  2. ISetting fields (StringSetting, BoolSetting, etc.) with their Caption
    ///  3. TranslationString fields (delegated to TranslationStringExtractor)
    /// </summary>
    public static IEnumerable<TranslationEntry> Extract(string filePath, string sourceText)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText, path: filePath);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

        foreach (ClassDeclarationSyntax classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            if (!InheritsFromGitPluginBase(classDecl))
            {
                continue;
            }

            string className = classDecl.Identifier.Text;

            // 1. Find the Name assignment in constructor → becomes Description.Text
            string? pluginName = FindPluginName(classDecl);
            if (pluginName is not null && ContainsLetter(pluginName))
            {
                yield return new TranslationEntry(className, "Description", "Text", pluginName);
            }

            // 2. Find ISetting fields
            foreach (TranslationEntry entry in ExtractSettingFields(classDecl, className))
            {
                yield return entry;
            }

            // 3. TranslationString fields are handled by TranslationStringExtractor
            // (they'll be found when the file is also processed by TranslationStringExtractor)
        }
    }

    private static bool InheritsFromGitPluginBase(ClassDeclarationSyntax classDecl)
    {
        if (classDecl.BaseList is null)
        {
            return false;
        }

        return classDecl.BaseList.Types.Any(t =>
        {
            string typeName = t.Type.ToString();
            return typeName is "GitPluginBase"
                or "GitExtensions.Extensibility.Plugins.GitPluginBase";
        });
    }

    private static string? FindPluginName(ClassDeclarationSyntax classDecl)
    {
        // Look in constructors for: Name = "...";
        foreach (ConstructorDeclarationSyntax ctor in classDecl.Members.OfType<ConstructorDeclarationSyntax>())
        {
            if (ctor.Body is null)
            {
                continue;
            }

            foreach (StatementSyntax statement in ctor.Body.Statements)
            {
                if (statement is not ExpressionStatementSyntax exprStmt)
                {
                    continue;
                }

                if (exprStmt.Expression is not AssignmentExpressionSyntax assignment)
                {
                    continue;
                }

                string propertyName = assignment.Left switch
                {
                    IdentifierNameSyntax id => id.Identifier.Text,
                    MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
                    _ => ""
                };

                if (propertyName == "Name")
                {
                    return TranslationStringExtractor.ExtractStringValue(assignment.Right);
                }
            }
        }

        return null;
    }

    private static readonly HashSet<string> SettingTypes = new(StringComparer.Ordinal)
    {
        "StringSetting",
        "BoolSetting",
        "NumberSetting",
        "ChoiceSetting",
        "PseudoSetting",
    };

    private static IEnumerable<TranslationEntry> ExtractSettingFields(ClassDeclarationSyntax classDecl, string className)
    {
        foreach (FieldDeclarationSyntax field in classDecl.Members.OfType<FieldDeclarationSyntax>())
        {
            string typeName = field.Declaration.Type.ToString();

            // Handle generic types like NumberSetting<int>
            bool isSetting = SettingTypes.Any(s => typeName.StartsWith(s, StringComparison.Ordinal));
            if (!isSetting)
            {
                continue;
            }

            foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
            {
                string? caption = ExtractSettingCaption(variable);
                if (caption is not null && ContainsLetter(caption))
                {
                    yield return new TranslationEntry(className, variable.Identifier.Text, "Caption", caption);
                }
            }
        }
    }

    private static string? ExtractSettingCaption(VariableDeclaratorSyntax variable)
    {
        if (variable.Initializer?.Value is ObjectCreationExpressionSyntax creation)
        {
            return ExtractCaptionFromArgs(creation.ArgumentList);
        }

        if (variable.Initializer?.Value is ImplicitObjectCreationExpressionSyntax implicitCreation)
        {
            return ExtractCaptionFromArgs(implicitCreation.ArgumentList);
        }

        return null;
    }

    /// <summary>
    ///  Extracts the caption from ISetting constructor arguments.
    ///  2-arg overload: (name, defaultValue) — caption is the name (first arg).
    ///  3+ arg overload: (name, caption, defaultValue) — caption is the second arg.
    /// </summary>
    private static string? ExtractCaptionFromArgs(ArgumentListSyntax? argumentList)
    {
        if (argumentList is null || argumentList.Arguments.Count == 0)
        {
            return null;
        }

        // 3+ args: (name, caption, defaultValue, ...) — caption is second arg
        if (argumentList.Arguments.Count >= 3)
        {
            return TranslationStringExtractor.ExtractStringValue(argumentList.Arguments[1].Expression);
        }

        // 2 args: (name, defaultValue) — caption defaults to name (first arg)
        return TranslationStringExtractor.ExtractStringValue(argumentList.Arguments[0].Expression);
    }

    private static bool ContainsLetter(string text)
    {
        return text.Any(char.IsLetter);
    }
}
