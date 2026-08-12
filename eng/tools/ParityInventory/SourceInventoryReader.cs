using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GitExtensions.ParityInventory;

// parity-scaffolding: Extracts comparable functional facts from C# partials and Avalonia markup.
internal static class SourceInventoryReader
{
    public static SourceInventory Read(
        string root,
        string typeName,
        IReadOnlySet<string> englishKeys,
        bool isTwin)
    {
        string fullRoot = Path.GetFullPath(root);
        string className = typeName[(typeName.LastIndexOf('.') + 1)..];
        List<MutablePart> parts = [];
        foreach (string file in Directory.EnumerateFiles(fullRoot, "*.cs", SearchOption.AllDirectories)
                     .OrderBy(path => path, StringComparer.Ordinal))
        {
            ReadCSharp(fullRoot, file, typeName, className, isTwin, parts);
        }

        if (isTwin)
        {
            foreach (string file in Directory.EnumerateFiles(fullRoot, "*.axaml", SearchOption.AllDirectories)
                         .OrderBy(path => path, StringComparer.Ordinal))
            {
                ReadAxaml(fullRoot, file, typeName, englishKeys, parts);
            }
        }

        if (parts.Count == 0)
        {
            throw new InvalidDataException($"Type '{typeName}' was not found below '{fullRoot}'.");
        }

        return CreateInventory(root, className, englishKeys, isTwin, parts);
    }

    public static SourceInventory ReadFiles(
        string root,
        string typeName,
        IReadOnlySet<string> englishKeys,
        bool isTwin,
        IEnumerable<string> files)
    {
        string fullRoot = Path.GetFullPath(root);
        string className = typeName[(typeName.LastIndexOf('.') + 1)..];
        List<MutablePart> parts = [];
        foreach (string file in files.Select(Path.GetFullPath).Distinct(StringComparer.Ordinal)
                     .OrderBy(path => path, StringComparer.Ordinal))
        {
            if (file.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase))
            {
                if (isTwin)
                {
                    ReadAxaml(fullRoot, file, typeName, englishKeys, parts);
                }
            }
            else if (file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                ReadCSharp(fullRoot, file, typeName, className, isTwin, parts);
            }
        }

        if (parts.Count == 0)
        {
            throw new InvalidDataException($"Type '{typeName}' was not found in the supplied files.");
        }

        return CreateInventory(root, className, englishKeys, isTwin, parts);
    }

    public static IReadOnlyList<string> DiscoverTopLevelTypeNames(string file)
    {
        if (!File.Exists(file))
        {
            return [];
        }

        if (file.EndsWith(".axaml", StringComparison.OrdinalIgnoreCase))
        {
            XDocument document = XDocument.Load(file);
            XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
            string? className = (string?)document.Root?.Attribute(x + "Class");
            return string.IsNullOrWhiteSpace(className) ? [] : [className];
        }

        if (!file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        SyntaxTree tree = CSharpSyntaxTree.ParseText(
            File.ReadAllText(file),
            new CSharpParseOptions(LanguageVersion.Preview));
        return tree.GetCompilationUnitRoot().DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Where(IsSupportedTypeDeclaration)
            .Where(declaration => !declaration.Ancestors().OfType<TypeDeclarationSyntax>().Any())
            .Select(GetTypeName)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
    }

    private static SourceInventory CreateInventory(
        string root,
        string className,
        IReadOnlySet<string> englishKeys,
        bool isTwin,
        List<MutablePart> parts)
    {
        bool hasDesignerPart = parts.Any(part =>
            string.Equals(
                Path.GetFileName(part.Path),
                $"{className}.Designer.cs",
                StringComparison.Ordinal));
        List<TranslationKeyEntry> keys = parts.SelectMany(part => part.TranslationKeys)
            .GroupBy(entry => (entry.Key, entry.Origin))
            .Select(group => group.First() with { InEnglishCatalog = englishKeys.Contains(group.Key.Key) })
            .OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .ThenBy(entry => entry.Origin, StringComparer.Ordinal)
            .ToList();

        return new SourceInventory
        {
            Root = NormalizePath(root),
            Parts = parts.OrderBy(part => part.Path, StringComparer.Ordinal)
                .Select(part => new SourcePart
                {
                    Path = part.Path,
                    ExpectedTwinPath = !isTwin ? GetExpectedTwinPath(part.Path, className, hasDesignerPart) : null
                })
                .ToArray(),
            Members = parts.SelectMany(part => part.Members)
                .OrderBy(entry => entry.Part, StringComparer.Ordinal).ThenBy(entry => entry.Order).ToArray(),
            EventWiring = parts.SelectMany(part => part.EventWiring)
                .OrderBy(entry => entry.Part, StringComparer.Ordinal)
                .ThenBy(entry => entry.Target, StringComparer.Ordinal)
                .ThenBy(entry => entry.Event, StringComparer.Ordinal)
                .ThenBy(entry => entry.Handler, StringComparer.Ordinal)
                .ToArray(),
            EventHandlers = parts.SelectMany(part => part.EventHandlers).Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal).ToArray(),
            Menus = parts.SelectMany(part => part.Menus)
                .OrderBy(entry => entry.Part, StringComparer.Ordinal)
                .ThenBy(entry => entry.Parent, StringComparer.Ordinal)
                .ThenBy(entry => entry.Order)
                .ThenBy(entry => entry.Name, StringComparer.Ordinal)
                .ToArray(),
            HotkeyCommandIds = parts.SelectMany(part => part.HotkeyCommandIds).Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            Settings = parts.SelectMany(part => part.Settings)
                .GroupBy(entry => (entry.Part, entry.Key, entry.Access))
                .Select(group => group.First())
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .ThenBy(entry => entry.Access, StringComparer.Ordinal)
                .ThenBy(entry => entry.Part, StringComparer.Ordinal)
                .ToArray(),
            TranslationStrings = parts.SelectMany(part => part.TranslationStrings)
                .OrderBy(entry => entry.Name, StringComparer.Ordinal)
                .ThenBy(entry => entry.Part, StringComparer.Ordinal)
                .ToArray(),
            TranslationKeys = keys,
            Comments = parts.SelectMany(part => part.Comments)
                .OrderBy(entry => entry.Part, StringComparer.Ordinal)
                .ThenBy(entry => entry.Line)
                .ThenBy(entry => entry.Anchor, StringComparer.Ordinal)
                .ThenBy(entry => entry.Placement, StringComparer.Ordinal)
                .ThenBy(entry => entry.Order)
                .ToArray()
        };
    }

    private static void ReadCSharp(
        string root,
        string file,
        string typeName,
        string className,
        bool isTwin,
        List<MutablePart> parts)
    {
        string text = File.ReadAllText(file);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(text, new CSharpParseOptions(LanguageVersion.Preview));
        CompilationUnitSyntax unit = tree.GetCompilationUnitRoot();
        foreach (TypeDeclarationSyntax declaration in unit.DescendantNodes().OfType<TypeDeclarationSyntax>()
                     .Where(IsSupportedTypeDeclaration))
        {
            if (!MatchesType(declaration, typeName))
            {
                continue;
            }

            string relativePath = NormalizePath(Path.GetRelativePath(root, file));
            MutablePart part = new(relativePath);
            ExtractMembers(declaration, part);
            ExtractEventWiring(declaration, part);
            ExtractMenus(declaration, part);
            ExtractHotkeys(declaration, part);
            ExtractSettings(declaration, part);
            ExtractTranslationStrings(declaration, className, part);
            ExtractDesignerTranslationKeys(declaration, className, part);
            ExtractComments(declaration, tree, part);
            parts.Add(part);
        }
    }

    private static void ExtractComments(
        TypeDeclarationSyntax declaration,
        SyntaxTree tree,
        MutablePart part)
    {
        foreach (SyntaxTrivia trivia in declaration.DescendantTrivia(descendIntoTrivia: true)
                     .Where(IsComment)
                     .OrderBy(item => item.SpanStart))
        {
            MemberDeclarationSyntax? member = trivia.Token.Parent?.AncestorsAndSelf()
                .OfType<MemberDeclarationSyntax>()
                .FirstOrDefault(candidate =>
                    candidate != declaration
                    && candidate.AncestorsAndSelf().Contains(declaration));
            string anchor = member is null ? "$type" : GetCommentAnchor(declaration, member);
            string placement = member switch
            {
                null => "leading",
                _ when trivia.Span.End <= member.SpanStart => "leading",
                _ when trivia.SpanStart >= member.Span.End => "trailing",
                _ => "body"
            };
            int order = part.Comments.Count(comment =>
                comment.Anchor == anchor && comment.Placement == placement);
            int line = tree.GetLineSpan(trivia.Span).StartLinePosition.Line + 1;
            part.Comments.Add(new CommentEntry
            {
                Part = part.Path,
                Anchor = anchor,
                Placement = placement,
                Order = order,
                Kind = GetCommentKind(trivia),
                Line = line,
                Text = NormalizeCommentText(trivia)
            });
        }
    }

    private static bool IsComment(SyntaxTrivia trivia) =>
        trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
        || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
        || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
        || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);

    private static string GetCommentAnchor(
        TypeDeclarationSyntax declaration,
        MemberDeclarationSyntax member)
    {
        return string.Join(
            "/",
            member.AncestorsAndSelf()
                .OfType<MemberDeclarationSyntax>()
                .TakeWhile(candidate => candidate != declaration)
                .Reverse()
                .Select(GetMemberAnchor));
    }

    private static string GetMemberAnchor(MemberDeclarationSyntax member) =>
        member switch
        {
            FieldDeclarationSyntax field => $"field:{string.Join(",", field.Declaration.Variables.Select(variable => variable.Identifier.ValueText))}",
            EventFieldDeclarationSyntax eventField => $"event:{string.Join(",", eventField.Declaration.Variables.Select(variable => variable.Identifier.ValueText))}",
            PropertyDeclarationSyntax property => $"property:{property.Identifier.ValueText}",
            IndexerDeclarationSyntax indexer => $"indexer:this{Normalize(indexer.ParameterList)}",
            EventDeclarationSyntax eventDeclaration => $"event:{eventDeclaration.Identifier.ValueText}",
            MethodDeclarationSyntax method => $"method:{method.Identifier.ValueText}{Normalize(method.ParameterList)}",
            ConstructorDeclarationSyntax constructor => $"constructor:{constructor.Identifier.ValueText}{Normalize(constructor.ParameterList)}",
            DestructorDeclarationSyntax destructor =>
                $"destructor:{destructor.Identifier.ValueText}{Normalize(destructor.ParameterList)}",
            OperatorDeclarationSyntax operatorDeclaration =>
                $"operator:{operatorDeclaration.OperatorToken.ValueText}{Normalize(operatorDeclaration.ParameterList)}",
            ConversionOperatorDeclarationSyntax conversion =>
                $"conversion:{conversion.ImplicitOrExplicitKeyword.ValueText}:"
                + $"{Normalize(conversion.Type)}{Normalize(conversion.ParameterList)}",
            EnumDeclarationSyntax enumDeclaration => $"enum:{enumDeclaration.Identifier.ValueText}",
            ClassDeclarationSyntax nestedClass => $"class:{nestedClass.Identifier.ValueText}",
            StructDeclarationSyntax nestedStruct => $"struct:{nestedStruct.Identifier.ValueText}",
            InterfaceDeclarationSyntax nestedInterface => $"interface:{nestedInterface.Identifier.ValueText}",
            DelegateDeclarationSyntax nestedDelegate =>
                $"delegate:{nestedDelegate.Identifier.ValueText}{Normalize(nestedDelegate.ParameterList)}",
            _ => member.Kind().ToString()
        };

    private static string GetCommentKind(SyntaxTrivia trivia) =>
        trivia.Kind() switch
        {
            SyntaxKind.SingleLineCommentTrivia => "singleLine",
            SyntaxKind.MultiLineCommentTrivia => "multiLine",
            SyntaxKind.SingleLineDocumentationCommentTrivia => "xmlDoc",
            SyntaxKind.MultiLineDocumentationCommentTrivia => "xmlDoc",
            _ => throw new InvalidOperationException($"Trivia '{trivia.Kind()}' is not a comment.")
        };

    private static string NormalizeCommentText(SyntaxTrivia trivia)
    {
        string text = trivia.ToFullString().Replace("\r\n", "\n", StringComparison.Ordinal);
        string[] lines = text.Split('\n');
        for (int index = 0; index < lines.Length; index++)
        {
            string line = lines[index].Trim();
            if (line.StartsWith("///", StringComparison.Ordinal))
            {
                line = line[3..];
            }
            else if (line.StartsWith("//", StringComparison.Ordinal))
            {
                line = line[2..];
            }
            else
            {
                line = line.TrimStart('/', '*').TrimEnd('/', '*');
            }

            lines[index] = line.Trim();
        }

        return Regex.Replace(
            string.Join(" ", lines.Where(line => line.Length > 0)),
            @"\s+",
            " ").Trim();
    }

    private static bool MatchesType(TypeDeclarationSyntax declaration, string typeName) =>
        string.Equals(GetTypeName(declaration), typeName, StringComparison.Ordinal);

    private static bool IsSupportedTypeDeclaration(TypeDeclarationSyntax declaration) =>
        declaration is ClassDeclarationSyntax or StructDeclarationSyntax or InterfaceDeclarationSyntax;

    private static string GetTypeName(TypeDeclarationSyntax declaration)
    {
        string namespaceName = string.Join(
            ".",
            declaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>()
                .Reverse()
                .Select(item => item.Name.ToString()));
        string candidate = string.IsNullOrEmpty(namespaceName)
            ? declaration.Identifier.ValueText
            : $"{namespaceName}.{declaration.Identifier.ValueText}";
        return candidate;
    }

    private static void ExtractMembers(TypeDeclarationSyntax declaration, MutablePart part)
    {
        int order = 0;
        foreach (MemberDeclarationSyntax member in declaration.Members)
        {
            string accessibility = GetAccessibility(member.Modifiers);
            switch (member)
            {
                case FieldDeclarationSyntax field:
                    foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
                    {
                        part.Members.Add(NewMember(part.Path, order++, "field", variable.Identifier.ValueText,
                            accessibility, $"{field.Declaration.Type} {variable.Identifier.ValueText}"));
                    }

                    break;
                case EventFieldDeclarationSyntax eventField:
                    foreach (VariableDeclaratorSyntax variable in eventField.Declaration.Variables)
                    {
                        part.Members.Add(NewMember(part.Path, order++, "event", variable.Identifier.ValueText,
                            accessibility, $"{eventField.Declaration.Type} {variable.Identifier.ValueText}"));
                    }

                    break;
                case PropertyDeclarationSyntax property:
                    part.Members.Add(NewMember(part.Path, order++, "property", property.Identifier.ValueText,
                        accessibility, $"{property.Type} {property.Identifier.ValueText}"));
                    break;
                case MethodDeclarationSyntax method:
                    part.Members.Add(NewMember(part.Path, order++, "method", method.Identifier.ValueText,
                        accessibility, $"{method.ReturnType} {method.Identifier}{Normalize(method.ParameterList)}"));
                    break;
                case ConstructorDeclarationSyntax constructor:
                    part.Members.Add(NewMember(part.Path, order++, "constructor", constructor.Identifier.ValueText,
                        accessibility, $"{constructor.Identifier}{Normalize(constructor.ParameterList)}"));
                    break;
                case EnumDeclarationSyntax enumDeclaration:
                    part.Members.Add(NewMember(part.Path, order++, "enum", enumDeclaration.Identifier.ValueText,
                        accessibility, enumDeclaration.Identifier.ValueText));
                    break;
                case ClassDeclarationSyntax nestedClass:
                    part.Members.Add(NewMember(part.Path, order++, "class", nestedClass.Identifier.ValueText,
                        accessibility, nestedClass.Identifier.ValueText));
                    break;
            }
        }
    }

    private static MemberEntry NewMember(
        string part,
        int order,
        string kind,
        string name,
        string accessibility,
        string signature) =>
        new()
        {
            Part = part,
            Order = order,
            Kind = kind,
            Name = name,
            Accessibility = accessibility,
            Signature = Normalize(signature)
        };

    private static void ExtractEventWiring(TypeDeclarationSyntax declaration, MutablePart part)
    {
        foreach (AssignmentExpressionSyntax assignment in declaration.DescendantNodes()
                     .OfType<AssignmentExpressionSyntax>()
                     .Where(item => item.IsKind(SyntaxKind.AddAssignmentExpression)))
        {
            if (assignment.Left is not MemberAccessExpressionSyntax member)
            {
                continue;
            }

            string handler = assignment.Right switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
                MemberAccessExpressionSyntax access => access.Name.Identifier.ValueText,
                AnonymousFunctionExpressionSyntax => "<lambda>",
                _ => Normalize(assignment.Right)
            };
            part.EventWiring.Add(new EventWireEntry
            {
                Part = part.Path,
                Target = Normalize(member.Expression),
                Event = member.Name.Identifier.ValueText,
                Handler = handler
            });
            if (handler != "<lambda>")
            {
                part.EventHandlers.Add(handler);
            }
        }

        foreach (MethodDeclarationSyntax method in declaration.Members.OfType<MethodDeclarationSyntax>())
        {
            if (method.ParameterList.Parameters.Count == 2
                && method.ParameterList.Parameters[1].Type?.ToString().EndsWith("EventArgs", StringComparison.Ordinal) == true)
            {
                part.EventHandlers.Add(method.Identifier.ValueText);
            }
        }
    }

    private static void ExtractMenus(TypeDeclarationSyntax declaration, MutablePart part)
    {
        HashSet<string> menuNames = declaration.DescendantNodes().OfType<FieldDeclarationSyntax>()
            .Where(field =>
            {
                string type = field.Declaration.Type.ToString();
                return type.EndsWith("ToolStripMenuItem", StringComparison.Ordinal)
                    || type.EndsWith("ContextMenuStrip", StringComparison.Ordinal)
                    || type.EndsWith("MenuItem", StringComparison.Ordinal)
                    || type.EndsWith("ContextMenu", StringComparison.Ordinal);
            })
            .SelectMany(field => field.Declaration.Variables)
            .Select(variable => variable.Identifier.ValueText)
            .ToHashSet(StringComparer.Ordinal);

        foreach (InvocationExpressionSyntax invocation in declaration.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax call
                || call.Expression is not MemberAccessExpressionSyntax collection
                || collection.Name.Identifier.ValueText is not ("Items" or "DropDownItems")
                || call.Name.Identifier.ValueText is not ("Add" or "AddRange" or "Insert"))
            {
                continue;
            }

            string parent = Normalize(collection.Expression);
            List<string> children = ExtractCollectionItems(invocation).ToList();
            for (int index = 0; index < children.Count; index++)
            {
                string child = children[index];
                part.Menus.Add(new MenuEntry
                {
                    Part = part.Path,
                    Parent = parent,
                    Order = index,
                    Name = child,
                    Kind = child.Contains("separator", StringComparison.OrdinalIgnoreCase) ? "separator" : "item"
                });
            }
        }
    }

    private static IEnumerable<string> ExtractCollectionItems(InvocationExpressionSyntax invocation)
    {
        foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
        {
            foreach (ExpressionSyntax expression in FlattenCollection(argument.Expression))
            {
                if (expression is IdentifierNameSyntax identifier)
                {
                    yield return identifier.Identifier.ValueText;
                }
                else if (expression is MemberAccessExpressionSyntax member)
                {
                    yield return member.Name.Identifier.ValueText;
                }
            }
        }
    }

    private static IEnumerable<ExpressionSyntax> FlattenCollection(ExpressionSyntax expression) =>
        expression switch
        {
            ArrayCreationExpressionSyntax array when array.Initializer is not null => array.Initializer.Expressions,
            ImplicitArrayCreationExpressionSyntax array => array.Initializer.Expressions,
            CollectionExpressionSyntax collection => collection.Elements
                .OfType<ExpressionElementSyntax>().Select(element => element.Expression),
            _ => [expression]
        };

    private static void ExtractHotkeys(TypeDeclarationSyntax declaration, MutablePart part)
    {
        foreach (MethodDeclarationSyntax method in declaration.Members.OfType<MethodDeclarationSyntax>()
                     .Where(item => item.Identifier.ValueText.Contains("Command", StringComparison.Ordinal)))
        {
            foreach (SwitchLabelSyntax label in method.DescendantNodes().OfType<SwitchLabelSyntax>())
            {
                if (label is CaseSwitchLabelSyntax caseLabel)
                {
                    part.HotkeyCommandIds.Add(Normalize(caseLabel.Value));
                }
            }
        }

        foreach (InvocationExpressionSyntax invocation in declaration.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            string methodName = invocation.Expression switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
                MemberAccessExpressionSyntax access => access.Name.Identifier.ValueText,
                _ => string.Empty
            };
            if (methodName is not ("ExecuteCommand" or "GetShortcutKeys"))
            {
                continue;
            }

            foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
            {
                part.HotkeyCommandIds.Add(Normalize(argument.Expression));
            }
        }
    }

    private static void ExtractSettings(TypeDeclarationSyntax declaration, MutablePart part)
    {
        HashSet<string> settingsSources = declaration.DescendantNodes()
            .OfType<VariableDeclarationSyntax>()
            .Where(item => IsSettingsSourceType(item.Type))
            .SelectMany(item => item.Variables)
            .Select(item => item.Identifier.ValueText)
            .Concat(declaration.DescendantNodes().OfType<ParameterSyntax>()
                .Where(item => IsSettingsSourceType(item.Type))
                .Select(item => item.Identifier.ValueText))
            .ToHashSet(StringComparer.Ordinal);

        foreach (MemberAccessExpressionSyntax access in declaration.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
        {
            if (access.Expression is IdentifierNameSyntax identifier
                && identifier.Identifier.ValueText == "AppSettings")
            {
                part.Settings.Add(new SettingEntry
                {
                    Part = part.Path,
                    Key = access.Name.Identifier.ValueText,
                    Access = IsWritten(access) ? "write" : "read"
                });
            }
        }

        foreach (InvocationExpressionSyntax invocation in declaration.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax call)
            {
                continue;
            }

            string receiver = call.Expression switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
                MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
                _ => string.Empty
            };
            bool appSettingsCall = receiver == "AppSettings";
            bool settingsSourceCall = settingsSources.Contains(receiver);
            string method = call.Name.Identifier.ValueText;
            if ((!appSettingsCall && !settingsSourceCall)
                || (!method.StartsWith("Get", StringComparison.Ordinal)
                    && !method.StartsWith("Set", StringComparison.Ordinal)))
            {
                continue;
            }

            ArgumentSyntax? keyArgument = invocation.ArgumentList.Arguments.FirstOrDefault();
            if (keyArgument is null)
            {
                continue;
            }

            part.Settings.Add(new SettingEntry
            {
                Part = part.Path,
                Key = $"{method}:{Normalize(keyArgument.Expression)}",
                Access = method.StartsWith("Set", StringComparison.Ordinal) ? "write" : "read"
            });
        }
    }

    private static bool IsSettingsSourceType(TypeSyntax? type) =>
        type?.ToString().TrimEnd('?').EndsWith("ISettingsSource", StringComparison.Ordinal) == true;

    private static bool IsWritten(SyntaxNode node)
    {
        SyntaxNode? current = node;
        while (current.Parent is MemberAccessExpressionSyntax parent)
        {
            current = parent;
        }

        return (current.Parent is AssignmentExpressionSyntax assignment && assignment.Left == current)
            || (current.Parent is PrefixUnaryExpressionSyntax prefix
                && (prefix.IsKind(SyntaxKind.PreIncrementExpression)
                    || prefix.IsKind(SyntaxKind.PreDecrementExpression)))
            || (current.Parent is PostfixUnaryExpressionSyntax postfix
                && (postfix.IsKind(SyntaxKind.PostIncrementExpression)
                    || postfix.IsKind(SyntaxKind.PostDecrementExpression)));
    }

    private static void ExtractTranslationStrings(
        TypeDeclarationSyntax declaration,
        string className,
        MutablePart part)
    {
        foreach (FieldDeclarationSyntax field in declaration.Members.OfType<FieldDeclarationSyntax>()
                     .Where(item => item.Declaration.Type.ToString().EndsWith("TranslationString", StringComparison.Ordinal)))
        {
            foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
            {
                part.TranslationStrings.Add(new TranslationStringEntry
                {
                    Part = part.Path,
                    Name = variable.Identifier.ValueText,
                    Initializer = Normalize(variable.Initializer?.Value)
                });
                part.TranslationKeys.Add(NewTranslationKey(
                    $"{variable.Identifier.ValueText}.Text",
                    $"{part.Path}:{className}.{variable.Identifier.ValueText}"));
            }
        }
    }

    private static void ExtractDesignerTranslationKeys(
        TypeDeclarationSyntax declaration,
        string className,
        MutablePart part)
    {
        if (!part.Path.EndsWith(".Designer.cs", StringComparison.Ordinal))
        {
            return;
        }

        foreach (AssignmentExpressionSyntax assignment in declaration.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (assignment.Left is not MemberAccessExpressionSyntax access
                || access.Name.Identifier.ValueText is not ("Text" or "HeaderText"))
            {
                continue;
            }

            string control = access.Expression switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
                MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
                _ => string.Empty
            };
            if (!string.IsNullOrEmpty(control))
            {
                part.TranslationKeys.Add(NewTranslationKey(
                    $"{control}.Text",
                    $"{part.Path}:{className}.{control}"));
            }
        }
    }

    private static void ReadAxaml(
        string root,
        string file,
        string typeName,
        IReadOnlySet<string> englishKeys,
        List<MutablePart> parts)
    {
        XDocument document;
        try
        {
            document = XDocument.Load(file, LoadOptions.SetLineInfo);
        }
        catch (Exception exception) when (exception is System.Xml.XmlException or IOException)
        {
            throw new InvalidDataException($"Could not parse AXAML '{file}': {exception.Message}", exception);
        }

        XElement? rootElement = document.Root;
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        if (rootElement is null
            || !string.Equals((string?)rootElement.Attribute(x + "Class"), typeName, StringComparison.Ordinal))
        {
            return;
        }

        string relativePath = NormalizePath(Path.GetRelativePath(root, file));
        MutablePart part = new(relativePath);
        Dictionary<XElement, string> menuNames = [];
        foreach (XElement element in rootElement.DescendantsAndSelf())
        {
            string? name = (string?)element.Attribute(x + "Name");
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            string kind = element.Name.LocalName;
            part.Members.Add(NewMember(part.Path, part.Members.Count, "control", name, "private", kind));
            string? translatedProperty = element.Attribute("Header") is not null
                || element.Attribute("Content") is not null
                || element.Attribute("Text") is not null
                    ? "Text"
                    : element.Attribute("Watermark") is not null ? "Watermark" : null;
            if (translatedProperty is not null)
            {
                part.TranslationKeys.Add(new TranslationKeyEntry
                {
                    Key = $"{name}.{translatedProperty}",
                    Origin = $"{part.Path}:{kind}",
                    InEnglishCatalog = englishKeys.Contains($"{name}.{translatedProperty}")
                });
            }

            if (kind is "ContextMenu" or "MenuItem")
            {
                menuNames[element] = name;
            }
        }

        foreach ((XElement element, string parent) in menuNames)
        {
            int order = 0;
            foreach (XElement child in element.Elements()
                         .Where(child => child.Name.LocalName is "MenuItem" or "Separator"))
            {
                string? childName = (string?)child.Attribute(x + "Name");
                if (string.IsNullOrWhiteSpace(childName))
                {
                    childName = $"<{child.Name.LocalName.ToLowerInvariant()}:{order}>";
                }

                part.Menus.Add(new MenuEntry
                {
                    Part = part.Path,
                    Parent = parent,
                    Order = order++,
                    Name = childName,
                    Kind = child.Name.LocalName == "Separator" ? "separator" : "item"
                });
            }
        }

        parts.Add(part);
    }

    private static TranslationKeyEntry NewTranslationKey(string key, string origin) =>
        new()
        {
            Key = key,
            Origin = origin,
            InEnglishCatalog = false
        };

    private static string GetExpectedTwinPath(string path, string className, bool hasDesignerPart)
    {
        string fileName = Path.GetFileName(path);
        string directory = NormalizePath(Path.GetDirectoryName(path) ?? string.Empty);
        string twinFileName = fileName switch
        {
            var name when name == $"{className}.cs" && hasDesignerPart => $"{className}.axaml.cs",
            var name when name == $"{className}.Designer.cs" => $"{className}.axaml",
            _ => fileName
        };
        return string.IsNullOrEmpty(directory) ? twinFileName : $"{directory}/{twinFileName}";
    }

    private static string GetAccessibility(SyntaxTokenList modifiers)
    {
        string[] accessibility = modifiers
            .Where(token => token.Kind() is
                SyntaxKind.PublicKeyword
                or SyntaxKind.PrivateKeyword
                or SyntaxKind.ProtectedKeyword
                or SyntaxKind.InternalKeyword)
            .Select(token => token.ValueText)
            .ToArray();
        return accessibility.Length == 0 ? "private" : string.Join(" ", accessibility);
    }

    private static string Normalize(SyntaxNode? node) => node is null ? string.Empty : Normalize(node.ToString());

    private static string Normalize(string value) =>
        string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string NormalizePath(string path) => path.Replace('\\', '/');

    // parity-scaffolding: Accumulates facts while one source part is being visited.
    private sealed class MutablePart(string path)
    {
        public string Path { get; } = path;

        public List<MemberEntry> Members { get; } = [];

        public List<EventWireEntry> EventWiring { get; } = [];

        public List<string> EventHandlers { get; } = [];

        public List<MenuEntry> Menus { get; } = [];

        public List<string> HotkeyCommandIds { get; } = [];

        public List<SettingEntry> Settings { get; } = [];

        public List<TranslationStringEntry> TranslationStrings { get; } = [];

        public List<TranslationKeyEntry> TranslationKeys { get; } = [];

        public List<CommentEntry> Comments { get; } = [];
    }
}
