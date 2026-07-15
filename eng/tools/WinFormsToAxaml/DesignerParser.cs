using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WinFormsToAxaml;

/// <summary>
///  Parses a Windows Forms <c>.Designer.cs</c> file: field declarations give the control
///  types, the statements of <c>InitializeComponent</c> give properties, events, layout
///  styles, and the containment tree.
/// </summary>
internal static class DesignerParser
{
    public static DesignerForm Parse(string designerSource)
    {
        CompilationUnitSyntax unit = CSharpSyntaxTree.ParseText(designerSource).GetCompilationUnitRoot();

        ClassDeclarationSyntax classDeclaration = unit.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        string namespaceName = unit.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? "";

        // The field declarations at the bottom of the class map field name → control type.
        Dictionary<string, string> fieldTypes = [];
        foreach (FieldDeclarationSyntax field in classDeclaration.Members.OfType<FieldDeclarationSyntax>())
        {
            string typeName = field.Declaration.Type.ToString();
            foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
            {
                fieldTypes[variable.Identifier.Text] = typeName;
            }
        }

        MethodDeclarationSyntax initializeComponent = classDeclaration.Members.OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(method => method.Identifier.Text == "InitializeComponent")
            ?? throw new InvalidDataException($"{classDeclaration.Identifier.Text} has no InitializeComponent method; not a Windows Forms Designer file.");

        ControlNode root = new("this", "Form");
        Dictionary<string, ControlNode> nodes = [];
        List<ControlNode> ordered = [];

        ControlNode GetOrCreateNode(string name)
        {
            if (name == "this")
            {
                return root;
            }

            if (!nodes.TryGetValue(name, out ControlNode? node))
            {
                // Used but never created in InitializeComponent: a container inherited from
                // the base form (e.g. MainPanel/ControlsPanel of GitExtensionsDialog).
                node = new ControlNode(name, fieldTypes.GetValueOrDefault(name, "Panel")) { IsInherited = true };
                nodes[name] = node;
                ordered.Add(node);
            }

            return node;
        }

        foreach (StatementSyntax statement in initializeComponent.Body!.Statements)
        {
            // Containers that need no field access are local variables of InitializeComponent
            // ("TableLayoutPanel tableLayoutPanel1;"); treat them like fields.
            if (statement is LocalDeclarationStatementSyntax { Declaration: { } localDeclaration })
            {
                string localType = localDeclaration.Type.ToString();
                foreach (VariableDeclaratorSyntax variable in localDeclaration.Variables)
                {
                    fieldTypes[variable.Identifier.Text] = localType;
                    if (variable.Initializer?.Value is ObjectCreationExpressionSyntax localCreation)
                    {
                        ControlNode node = new(variable.Identifier.Text, Strip(localCreation.Type.ToString()));
                        nodes[variable.Identifier.Text] = node;
                        ordered.Add(node);
                    }
                }

                continue;
            }

            if (statement is not ExpressionStatementSyntax { Expression: ExpressionSyntax expression })
            {
                continue;
            }

            switch (expression)
            {
                case AssignmentExpressionSyntax assignment when assignment.IsKind(SyntaxKind.SimpleAssignmentExpression):
                    ParseAssignment(assignment);
                    break;

                case AssignmentExpressionSyntax assignment when assignment.IsKind(SyntaxKind.AddAssignmentExpression):
                    ParseEventSubscription(assignment);
                    break;

                case InvocationExpressionSyntax invocation:
                    ParseInvocation(invocation);
                    break;

                default:
                    root.Unmapped.Add(Strip(statement.ToString()));
                    break;
            }
        }

        return new DesignerForm
        {
            Namespace = namespaceName,
            ClassName = classDeclaration.Identifier.Text,
            Root = root,
            AllControls = ordered,
        };

        void ParseAssignment(AssignmentExpressionSyntax assignment)
        {
            (string? target, string? property) = SplitTarget(assignment.Left);
            if (target is null)
            {
                root.Unmapped.Add(Strip(assignment.ToString()));
                return;
            }

            string value = Strip(assignment.Right.ToString());

            if (property is null)
            {
                // "name = new Type();" — creation when the field is a control; otherwise a
                // form-level property assignment like "AcceptButton = cmdOk".
                if (assignment.Right is ObjectCreationExpressionSyntax creation && fieldTypes.ContainsKey(target))
                {
                    if (!nodes.TryGetValue(target, out ControlNode? node))
                    {
                        node = new ControlNode(target, Strip(creation.Type.ToString()));
                        nodes[target] = node;
                        ordered.Add(node);
                    }
                    else
                    {
                        node.TypeName = Strip(creation.Type.ToString());
                        node.IsInherited = false;
                    }
                }
                else
                {
                    root.Properties[target] = value;
                }

                return;
            }

            if (property.Contains('.'))
            {
                // Nested property chain like "cmdOk.FlatAppearance.BorderSize = 0".
                GetOrCreateNode(target).Unmapped.Add(Strip(assignment.ToString()));
                return;
            }

            GetOrCreateNode(target).Properties[property] = value;
        }

        void ParseEventSubscription(AssignmentExpressionSyntax assignment)
        {
            (string? target, string? eventName) = SplitTarget(assignment.Left);
            if (target is null || eventName is null)
            {
                root.Unmapped.Add(Strip(assignment.ToString()));
                return;
            }

            // Handler is either a method group ("cmdOk_Click") or "new EventHandler(this.cmdOk_Click)".
            string handler = assignment.Right is ObjectCreationExpressionSyntax { ArgumentList.Arguments: [{ } argument] }
                ? Strip(argument.ToString())
                : Strip(assignment.Right.ToString());
            handler = handler[(handler.LastIndexOf('.') + 1)..];

            ControlNode node = target == "this" ? root : GetOrCreateNode(target);
            node.Events.Add((eventName, handler));
        }

        void ParseInvocation(InvocationExpressionSyntax invocation)
        {
            string invocationText = Strip(invocation.Expression.ToString());
            SeparatedSyntaxList<ArgumentSyntax> arguments = invocation.ArgumentList.Arguments;

            // Layout-machinery calls carry no information for the AXAML scaffold.
            if (invocationText.EndsWith(".SuspendLayout") || invocationText.EndsWith(".ResumeLayout")
                || invocationText.EndsWith(".PerformLayout") || invocationText.EndsWith(".BeginInit")
                || invocationText.EndsWith(".EndInit") || invocationText.EndsWith(".SetChildIndex")
                || invocationText is "SuspendLayout" or "ResumeLayout" or "PerformLayout")
            {
                return;
            }

            if (invocationText.EndsWith("Controls.Add") && !invocationText.EndsWith(".SetChildIndex"))
            {
                string parentName = invocationText[..^"Controls.Add".Length].TrimEnd('.');
                ControlNode parent = parentName.Length == 0 || parentName == "this" ? root : GetOrCreateNode(parentName);
                ControlNode child = GetOrCreateNode(Strip(arguments[0].ToString()));
                child.Parent = parent;
                parent.Children.Add(child);

                // TableLayoutPanel: Controls.Add(child, column, row).
                if (arguments.Count == 3
                    && int.TryParse(arguments[1].ToString(), out int column)
                    && int.TryParse(arguments[2].ToString(), out int row))
                {
                    child.GridColumn = column;
                    child.GridRow = row;
                }

                return;
            }

            if (invocationText.EndsWith("Controls.AddRange"))
            {
                string parentName = invocationText[..^"Controls.AddRange".Length].TrimEnd('.');
                ControlNode parent = parentName.Length == 0 || parentName == "this" ? root : GetOrCreateNode(parentName);
                if (arguments is [{ Expression: BaseObjectCreationExpressionSyntax { Initializer.Expressions: { } elements } }])
                {
                    foreach (ExpressionSyntax element in elements)
                    {
                        ControlNode child = GetOrCreateNode(Strip(element.ToString()));
                        child.Parent = parent;
                        parent.Children.Add(child);
                    }
                }

                return;
            }

            if (invocationText.EndsWith(".ColumnStyles.Add") || invocationText.EndsWith(".RowStyles.Add"))
            {
                bool isColumn = invocationText.EndsWith(".ColumnStyles.Add");
                string panelName = invocationText[..^(isColumn ? ".ColumnStyles.Add" : ".RowStyles.Add").Length];
                ControlNode panel = GetOrCreateNode(panelName);
                string definition = ParseTableLayoutStyle(arguments[0].Expression);
                (isColumn ? panel.ColumnStyles : panel.RowStyles).Add(definition);
                return;
            }

            if (invocationText.EndsWith(".SetColumnSpan") || invocationText.EndsWith(".SetRowSpan"))
            {
                ControlNode child = GetOrCreateNode(Strip(arguments[0].ToString()));
                if (int.TryParse(arguments[1].ToString(), out int span))
                {
                    if (invocationText.EndsWith(".SetColumnSpan"))
                    {
                        child.ColumnSpan = span;
                    }
                    else
                    {
                        child.RowSpan = span;
                    }
                }

                return;
            }

            if (invocationText.EndsWith(".SetToolTip") && arguments.Count == 2)
            {
                ControlNode child = GetOrCreateNode(Strip(arguments[0].ToString()));
                child.ToolTipText = ParseStringLiteral(arguments[1].Expression);
                return;
            }

            // Menus and tool strips: Items.AddRange(new ToolStripItem[] { ... }).
            if (invocationText.EndsWith(".Items.AddRange") || invocationText.EndsWith(".DropDownItems.AddRange"))
            {
                bool isDropDown = invocationText.EndsWith(".DropDownItems.AddRange");
                string parentName = invocationText[..^(isDropDown ? ".DropDownItems.AddRange" : ".Items.AddRange").Length];
                ControlNode parent = GetOrCreateNode(parentName);
                if (arguments is [{ Expression: BaseObjectCreationExpressionSyntax { Initializer.Expressions: { } items } }])
                {
                    foreach (ExpressionSyntax item in items)
                    {
                        ControlNode child = GetOrCreateNode(Strip(item.ToString()));
                        child.Parent = parent;
                        parent.Children.Add(child);
                    }
                }

                return;
            }

            // Anything else (SetStyle, custom helpers, …) is preserved as a TODO comment.
            (string? target, _) = SplitTarget(invocation.Expression);
            ControlNode owner = target is not null && nodes.ContainsKey(target) ? nodes[target] : root;
            owner.Unmapped.Add(Strip(invocation.ToString()));
        }
    }

    /// <summary>
    ///  Splits an assignment target into (control name, property name): "cmdOk.Text" →
    ///  ("cmdOk", "Text"); "Text" → ("this", "Text"); "cmdOk" → ("cmdOk", null).
    ///  "this." prefixes are dropped; deeper chains keep the rest in the property part.
    /// </summary>
    private static (string? Target, string? Property) SplitTarget(ExpressionSyntax expression)
    {
        string text = Strip(expression.ToString());

        int dot = text.IndexOf('.');
        if (dot < 0)
        {
            // Bare identifier: a field ("cmdOk = new Button()") or a form property ("Text = …").
            // The caller decides using the field table; report as a name-only target unless it
            // is a well-known form property (starts with an uppercase letter and is assigned,
            // which the caller resolves via fieldTypes).
            return (text, null);
        }

        return (text[..dot], text[(dot + 1)..]);
    }

    /// <summary>"new ColumnStyle(SizeType.Percent, 50F)" → "50*"; Absolute → pixels; default/AutoSize → "Auto".</summary>
    private static string ParseTableLayoutStyle(ExpressionSyntax expression)
    {
        if (expression is not BaseObjectCreationExpressionSyntax creation
            || creation.ArgumentList is null
            || creation.ArgumentList.Arguments.Count == 0)
        {
            return "Auto";
        }

        SeparatedSyntaxList<ArgumentSyntax> arguments = creation.ArgumentList.Arguments;
        string sizeType = arguments[0].ToString();
        string size = arguments.Count > 1 ? arguments[1].ToString().TrimEnd('F', 'f') : "";

        return sizeType switch
        {
            "SizeType.Percent" or "System.Windows.Forms.SizeType.Percent" => TrimDecimal(size) + "*",
            "SizeType.Absolute" or "System.Windows.Forms.SizeType.Absolute" => TrimDecimal(size),
            _ => "Auto",
        };

        static string TrimDecimal(string value)
            => value.EndsWith(".0") ? value[..^2] : value;
    }

    private static string? ParseStringLiteral(ExpressionSyntax expression)
        => expression is LiteralExpressionSyntax { Token.Value: string text } ? text : Strip(expression.ToString());

    /// <summary>Removes "this." prefixes so old- and new-style Designer files parse alike.</summary>
    private static string Strip(string text)
        => text.Replace("this.", "");
}
