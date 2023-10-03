using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace System.Windows.Forms.Generators;

[Generator(LanguageNames.CSharp)]
internal class FormDefaultConstructorGenerator : IIncrementalGenerator
{
    private static void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<string?> candidateTypes)
    {
        if (candidateTypes.IsEmpty)
        {
            return;
        }

        foreach (string? candidateType in candidateTypes)
        {
            if (string.IsNullOrEmpty(candidateType))
            {
                continue;
            }

            Debug.WriteLine($"candidateType: {candidateType}");

            bool generateDefaultCtor = true;
            bool containsInitializeComponent = false;
            string? @namespace = null;
            string typeName = candidateType!;

            if (compilation.GetSymbolsWithName(candidateType!, SymbolFilter.Type).FirstOrDefault() is INamedTypeSymbol typeSymbol)
            {
                Debug.WriteLine($"symbol: {candidateType}");

                if (typeSymbol.InstanceConstructors.Length > 0)
                {
                    foreach (IMethodSymbol ctor in typeSymbol.InstanceConstructors)
                    {
                        if (ctor.Parameters.Length == 0)
                        {
                            generateDefaultCtor = false;
                            break;
                        }
                    }

                    if (generateDefaultCtor)
                    {
                        generateDefaultCtor = InheritsFrom(typeSymbol);

                        if (generateDefaultCtor)
                        {
                            @namespace = typeSymbol.ContainingNamespace.ToString();
                            containsInitializeComponent = typeSymbol.MemberNames.Contains("InitializeComponent");

                            if (typeSymbol.IsGenericType)
                            {
                                typeName = $"{candidateType}<{string.Join(", ", typeSymbol.TypeArguments)}>";
                            }
                        }
                    }
                }
            }

            if (generateDefaultCtor)
            {
                string ctorBody = containsInitializeComponent ? "InitializeComponent();" : "";
                Debug.WriteLine($"Generating .ctor for {candidateType}");
                string? code = $@"
namespace {@namespace}
{{
    partial class {typeName}
    {{
#pragma warning disable CS0618 // Type or member is obsolete
        [System.Obsolete(""For VS designer and translation test only. Do not remove."")]
        public {candidateType}()
        {{
            {ctorBody}
        }}
#pragma warning restore CS0618 // Type or member is obsolete
    }}
}}
";
                if (code is not null)
                {
                    context.AddSource($"{candidateType}_ctor.g.cs", code);
                }
            }
        }

        static bool InheritsFrom(INamedTypeSymbol symbol)
        {
            while (true)
            {
                if (symbol.ToString() == "System.Windows.Forms.Form")
                {
                    return true;
                }

                if (symbol.ToString() == "System.Windows.Forms.UserControl")
                {
                    return true;
                }

                if (symbol.BaseType is not null)
                {
                    symbol = symbol.BaseType;
                    continue;
                }

                break;
            }

            return false;
        }
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<string?> syntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (syntaxNode, _) => IsSupportedSyntaxNode(syntaxNode),
            transform: static (generatorSyntaxContext, _) => GetTypeName(generatorSyntaxContext.Node));

        IncrementalValueProvider<(Compilation, ImmutableArray<string?>)> compilationAndTypes
            = context.CompilationProvider.Combine(syntaxProvider.Collect());

        context.RegisterSourceOutput(
            compilationAndTypes,
            (context, source) => Execute(context, source.Item1, source.Item2));
    }

    private static string? GetTypeName(SyntaxNode node)
    {
        string? typeName = null;

        if (node is ClassDeclarationSyntax classSyntax)
        {
            typeName = classSyntax.Identifier.ToString();
        }

        Debug.WriteLine($"typeName: {typeName}");
        return typeName;
    }

    public static bool IsSupportedSyntaxNode(SyntaxNode syntaxNode)
    {
#pragma warning disable SA1513 // Closing brace should be followed by blank line
        if (syntaxNode is ClassDeclarationSyntax
            {
                BaseList: BaseListSyntax
                {
                    Types.Count: > 0
                }
            })
        {
            return true;
        }
#pragma warning restore SA1513 // Closing brace should be followed by blank line

        return false;
    }
}
