using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TranslationExtractor;

/// <summary>
///  Extracts translatable control properties from WinForms Designer.cs files.
///  Handles: Text, ToolTipText, AccessibleName, AccessibleDescription, Caption, Title,
///  HeaderText (DataGridView columns), SetToolTip() calls, and form title.
/// </summary>
internal static class DesignerExtractor
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
    ///  Properties that are translatable via <c>[LocalizableProperties]</c> attribute
    ///  on specific control types (e.g. WatermarkComboBox).
    /// </summary>
    private static readonly HashSet<string> AdditionalTranslatableProperties = new(StringComparer.Ordinal)
    {
        "Watermark",
    };

    /// <summary>
    ///  Scans a .Designer.cs file for translatable control property assignments.
    /// </summary>
    /// <param name="filePath">Path to the .Designer.cs file.</param>
    /// <param name="sourceText">The source code text.</param>
    /// <param name="className">The class name (category) for the translation entries.</param>
    public static IEnumerable<TranslationEntry> Extract(string filePath, string sourceText, string className)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText, path: filePath);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

        // Find the InitializeComponent method
        MethodDeclarationSyntax? initMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == "InitializeComponent");

        if (initMethod is null)
        {
            yield break;
        }

        foreach (StatementSyntax statement in initMethod.Body?.Statements ?? [])
        {
            if (statement is not ExpressionStatementSyntax exprStatement)
            {
                continue;
            }

            // Handle: this.control.Property = "value";
            // Handle: control.Property = "value";
            // Handle: this.Text = "value";       (form title → $this.Text)
            // Handle: Text = "value";             (form title → $this.Text)
            if (exprStatement.Expression is AssignmentExpressionSyntax assignment)
            {
                foreach (TranslationEntry entry in ExtractFromAssignment(assignment, className))
                {
                    yield return entry;
                }

                continue;
            }

            // Handle: tooltip.SetToolTip(control, "text");
            // Handle: control.Items.AddRange(new object[] { "item0", "item1" });
            if (exprStatement.Expression is InvocationExpressionSyntax invocation)
            {
                foreach (TranslationEntry entry in ExtractFromToolTipInvocation(invocation, className))
                {
                    yield return entry;
                }

                foreach (TranslationEntry entry in ExtractFromItemsAddRange(invocation, className))
                {
                    yield return entry;
                }
            }
        }
    }

    private static IEnumerable<TranslationEntry> ExtractFromAssignment(
        AssignmentExpressionSyntax assignment, string className)
    {
        string? text = ExtractAssignedStringValue(assignment.Right);
        if (text is null || !ContainsLetter(text))
        {
            yield break;
        }

        (string controlName, string propertyName)? parsed = ParsePropertyAccess(assignment.Left);
        if (parsed is null)
        {
            yield break;
        }

        string controlName2 = parsed.Value.controlName;
        string propertyName2 = parsed.Value.propertyName;

        // Skip _NO_TRANSLATE_ fields
        if (controlName2.StartsWith("_NO_TRANSLATE_", StringComparison.Ordinal))
        {
            yield break;
        }

        if (!TranslatableProperties.Contains(propertyName2) && !AdditionalTranslatableProperties.Contains(propertyName2))
        {
            yield break;
        }

        yield return new TranslationEntry(className, controlName2, propertyName2, text);
    }

    private static (string ControlName, string PropertyName)? ParsePropertyAccess(ExpressionSyntax left)
    {
        if (left is MemberAccessExpressionSyntax memberAccess)
        {
            string propertyName = memberAccess.Name.Identifier.Text;

            // this.control.Property or control.Property
            if (memberAccess.Expression is MemberAccessExpressionSyntax innerAccess)
            {
                // this.control.Property → control name is innerAccess.Name
                string controlName = innerAccess.Name.Identifier.Text;
                return (controlName, propertyName);
            }

            if (memberAccess.Expression is ThisExpressionSyntax)
            {
                // this.Text → form title, category = $this
                return ("$this", propertyName);
            }

            if (memberAccess.Expression is IdentifierNameSyntax identifier)
            {
                // control.Property → control name is identifier
                return (identifier.Identifier.Text, propertyName);
            }
        }

        // Bare "Text = ..." inside InitializeComponent → form title
        if (left is IdentifierNameSyntax id && TranslatableProperties.Contains(id.Identifier.Text))
        {
            return ("$this", id.Identifier.Text);
        }

        return null;
    }

    private static IEnumerable<TranslationEntry> ExtractFromToolTipInvocation(
        InvocationExpressionSyntax invocation, string className)
    {
        // tooltip.SetToolTip(control, "text") or this.tooltip.SetToolTip(control, "text")
        if (invocation.Expression is not MemberAccessExpressionSyntax methodAccess)
        {
            yield break;
        }

        if (methodAccess.Name.Identifier.Text != "SetToolTip")
        {
            yield break;
        }

        if (invocation.ArgumentList.Arguments.Count != 2)
        {
            yield break;
        }

        // Get the tooltip component name
        string tooltipName = GetTooltipName(methodAccess.Expression);
        if (tooltipName.StartsWith("_NO_TRANSLATE_", StringComparison.Ordinal))
        {
            yield break;
        }

        // Get the control name (first argument)
        string? controlName = GetControlNameFromArgument(invocation.ArgumentList.Arguments[0].Expression);
        if (controlName is null || controlName.StartsWith("_NO_TRANSLATE_", StringComparison.Ordinal))
        {
            yield break;
        }

        // Get the text (second argument)
        string? text = TranslationStringExtractor.ExtractStringValue(invocation.ArgumentList.Arguments[1].Expression);
        if (text is null || !ContainsLetter(text))
        {
            yield break;
        }

        // The property in the xlf is the tooltip field name, e.g. "PushToRemote.toolTip1"
        yield return new TranslationEntry(className, controlName, tooltipName, text);
    }

    private static string GetTooltipName(ExpressionSyntax expression)
    {
        return expression switch
        {
            MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
            IdentifierNameSyntax id => id.Identifier.Text,
            _ => "tooltip"
        };
    }

    private static string? GetControlNameFromArgument(ExpressionSyntax expr)
    {
        return expr switch
        {
            // this.controlName or just controlName
            MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
            IdentifierNameSyntax id => id.Identifier.Text,
            _ => null
        };
    }

    private static string? ExtractAssignedStringValue(ExpressionSyntax expr)
    {
        return TranslationStringExtractor.ExtractStringValue(expr);
    }

    /// <summary>
    ///  Extracts translatable items from <c>control.Items.AddRange(new object[] { "item0", "item1" })</c>.
    /// </summary>
    private static IEnumerable<TranslationEntry> ExtractFromItemsAddRange(
        InvocationExpressionSyntax invocation, string className)
    {
        // Match: control.Items.AddRange(...) or this.control.Items.AddRange(...)
        if (invocation.Expression is not MemberAccessExpressionSyntax methodAccess)
        {
            yield break;
        }

        if (methodAccess.Name.Identifier.Text != "AddRange")
        {
            yield break;
        }

        // Navigate up: Items.AddRange → control.Items
        if (methodAccess.Expression is not MemberAccessExpressionSyntax itemsAccess)
        {
            yield break;
        }

        if (itemsAccess.Name.Identifier.Text != "Items")
        {
            yield break;
        }

        // Get control name
        string? controlName = itemsAccess.Expression switch
        {
            MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
            IdentifierNameSyntax id => id.Identifier.Text,
            _ => null,
        };

        if (controlName is null || controlName.StartsWith("_NO_TRANSLATE_", StringComparison.Ordinal))
        {
            yield break;
        }

        // Extract string items from the array argument
        if (invocation.ArgumentList.Arguments.Count != 1)
        {
            yield break;
        }

        ExpressionSyntax arg = invocation.ArgumentList.Arguments[0].Expression;

        // new object[] { "item0", "item1" }
        if (arg is not ArrayCreationExpressionSyntax arrayCreation)
        {
            // Could also be ImplicitArrayCreationExpressionSyntax
            if (arg is ImplicitArrayCreationExpressionSyntax implicitArray)
            {
                foreach (TranslationEntry entry in ExtractItemsFromInitializer(implicitArray.Initializer, controlName, className))
                {
                    yield return entry;
                }
            }

            yield break;
        }

        if (arrayCreation.Initializer is not null)
        {
            foreach (TranslationEntry entry in ExtractItemsFromInitializer(arrayCreation.Initializer, controlName, className))
            {
                yield return entry;
            }
        }
    }

    private static IEnumerable<TranslationEntry> ExtractItemsFromInitializer(
        InitializerExpressionSyntax initializer, string controlName, string className)
    {
        int index = 0;
        foreach (ExpressionSyntax expr in initializer.Expressions)
        {
            string? text = TranslationStringExtractor.ExtractStringValue(expr);
            if (text is not null && ContainsLetter(text))
            {
                yield return new TranslationEntry(className, controlName, $"Item{index}", text);
            }

            index++;
        }
    }

    private static bool ContainsLetter(string text)
    {
        return text.Any(char.IsLetter);
    }
}
