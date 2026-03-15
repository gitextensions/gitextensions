using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TranslationExtractor;

/// <summary>
///  Extracts translatable properties from object initializers in regular C# files.
///  Handles patterns like: <c>new ToolStripMenuItem { Name = "toolbarsMenuItem", Text = "Toolbars" }</c>
///  where controls with translatable properties (Text, ToolTipText, etc.) are created dynamically.
/// </summary>
internal static class ObjectInitializerExtractor
{
    private static readonly HashSet<string> TranslatableProperties = new(StringComparer.Ordinal)
    {
        "Text",
        "ToolTipText",
        "ToolTipTitle",
        "AccessibleDescription",
        "AccessibleName",
        "Caption",
        "Title",
        "HeaderText",
    };

    /// <summary>
    ///  Scans a C# source file for object initializers that set translatable properties,
    ///  and for field declarations where the constructor's first arg is the Text property
    ///  (e.g., <c>new ToolStripMenuItem("Collapse all", Images.CollapseAll)</c>).
    /// </summary>
    public static IEnumerable<TranslationEntry> Extract(string filePath, string sourceText)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText, path: filePath);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

        foreach (ClassDeclarationSyntax classDecl in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            string className = classDecl.Identifier.Text;

            // Pattern 1: Object creation with initializer { Name = "x", Text = "y" }
            foreach (ObjectCreationExpressionSyntax creation in classDecl.DescendantNodes()
                .OfType<ObjectCreationExpressionSyntax>())
            {
                if (creation.Initializer is not null)
                {
                    foreach (TranslationEntry entry in ExtractFromInitializer(creation, className))
                    {
                        yield return entry;
                    }
                }
            }

            // Pattern 2: Field = new ToolStripMenuItem("text", image) — first arg is Text
            foreach (FieldDeclarationSyntax field in classDecl.Members.OfType<FieldDeclarationSyntax>())
            {
                foreach (TranslationEntry entry in ExtractFromConstructorArg(field, className))
                {
                    yield return entry;
                }
            }
        }
    }

    private static IEnumerable<TranslationEntry> ExtractFromInitializer(
        ObjectCreationExpressionSyntax creation, string className)
    {
        // Try to find the Name property in the initializer
        string? controlName = GetNameFromInitializer(creation.Initializer!);
        if (controlName is null || controlName.StartsWith("_NO_TRANSLATE_", StringComparison.Ordinal))
        {
            yield break;
        }

        // Extract translatable properties from initializer
        foreach (AssignmentExpressionSyntax assignment in creation.Initializer!.Expressions
            .OfType<AssignmentExpressionSyntax>())
        {
            if (assignment.Left is not IdentifierNameSyntax propName)
            {
                continue;
            }

            string propertyName = propName.Identifier.Text;
            if (!TranslatableProperties.Contains(propertyName))
            {
                continue;
            }

            string? text = TranslationStringExtractor.ExtractStringValue(assignment.Right);
            if (text is not null && text.Any(char.IsLetter))
            {
                yield return new TranslationEntry(className, controlName, propertyName, text);
            }
        }
    }

    private static readonly HashSet<string> ControlTypesWithTextConstructor = new(StringComparer.Ordinal)
    {
        "ToolStripMenuItem",
        "ToolStripButton",
        "ToolStripDropDownButton",
        "ToolStripSplitButton",
        "ToolStripLabel",
    };

    private static IEnumerable<TranslationEntry> ExtractFromConstructorArg(
        FieldDeclarationSyntax field, string className)
    {
        foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
        {
            if (variable.Initializer?.Value is not ObjectCreationExpressionSyntax creation)
            {
                continue;
            }

            // Check if the type is a control type whose first constructor arg is Text
            string typeName = creation.Type.ToString();
            if (!ControlTypesWithTextConstructor.Any(t => typeName.EndsWith(t, StringComparison.Ordinal)))
            {
                continue;
            }

            if (creation.ArgumentList is null || creation.ArgumentList.Arguments.Count == 0)
            {
                continue;
            }

            string? text = TranslationStringExtractor.ExtractStringValue(creation.ArgumentList.Arguments[0].Expression);
            if (text is null || !text.Any(char.IsLetter))
            {
                continue;
            }

            string fieldName = variable.Identifier.Text;
            if (fieldName.StartsWith("_NO_TRANSLATE_", StringComparison.Ordinal))
            {
                continue;
            }

            yield return new TranslationEntry(className, fieldName, "Text", text);
        }
    }

    private static string? GetNameFromInitializer(InitializerExpressionSyntax initializer)
    {
        foreach (AssignmentExpressionSyntax assignment in initializer.Expressions
            .OfType<AssignmentExpressionSyntax>())
        {
            if (assignment.Left is IdentifierNameSyntax id && id.Identifier.Text == "Name")
            {
                return TranslationStringExtractor.ExtractStringValue(assignment.Right);
            }
        }

        return null;
    }
}
