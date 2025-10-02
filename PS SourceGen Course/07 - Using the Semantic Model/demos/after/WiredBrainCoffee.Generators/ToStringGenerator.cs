using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Text;
using WiredBrainCoffee.Generators.Model;

namespace WiredBrainCoffee.Generators;

[Generator]
public class ToStringGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => IsSyntaxTarget(node),
            transform: static (ctx, _) => GetSemanticTarget(ctx))
            .Where(static (target) => target is not null);

        context.RegisterSourceOutput(classes,
            static (ctx, source) => Execute(ctx, source));

        context.RegisterPostInitializationOutput(
            static (ctx) => PostInitializationOutput(ctx));
    }

    private static bool IsSyntaxTarget(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDeclarationSyntax
            && classDeclarationSyntax.AttributeLists.Count > 0;
    }

    private static ClassToGenerate? GetSemanticTarget(
        GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        var attributeSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(
            "WiredBrainCoffee.Generators.GenerateToStringAttribute");

        if (classSymbol is not null
            && attributeSymbol is not null)
        {
            foreach (var attributeData in classSymbol.GetAttributes())
            {
                if (attributeSymbol.Equals(attributeData.AttributeClass,
                    SymbolEqualityComparer.Default))
                {
                    var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                    var className = classSymbol.Name;
                    var propertyNames = new List<string>();

                    foreach (var memberSymbol in classSymbol.GetMembers())
                    {
                        if (memberSymbol.Kind == SymbolKind.Property
                            && memberSymbol.DeclaredAccessibility == Accessibility.Public)
                        {
                            propertyNames.Add(memberSymbol.Name);
                        }
                    }

                    return new ClassToGenerate(namespaceName, className, propertyNames);
                }
            }
        }

        return null;
    }

    private static void PostInitializationOutput(
        IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("WiredBrainCoffee.Generators.GenerateToStringAttribute.g.cs",
            @"namespace WiredBrainCoffee.Generators
{
    internal class GenerateToStringAttribute : System.Attribute { }
}");
    }

    private static Dictionary<string, int> _countPerFileName = new();

    private static void Execute(SourceProductionContext context,
        ClassToGenerate? classToGenerate)
    {
        if (classToGenerate is null)
        {
            return;
        }

        var namespaceName = classToGenerate.NamespaceName;
        var className = classToGenerate.ClassName;
        var fileName = $"{namespaceName}.{className}.g.cs";

        if (_countPerFileName.ContainsKey(fileName))
        {
            _countPerFileName[fileName]++;
        }
        else
        {
            _countPerFileName.Add(fileName, 1);
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.Append($@"// Generation count: {_countPerFileName[fileName]}
namespace {namespaceName}
{{
    partial class {className}
    {{
        public override string ToString()
        {{
            return $""");

        var first = true;
        foreach (var propertyName in classToGenerate.PropertyNames)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                stringBuilder.Append("; ");
            }

            stringBuilder.Append($"{propertyName}:{{{propertyName}}}");
        }

        stringBuilder.Append($@""";
        }}
    }}
}}
");

        context.AddSource(fileName, stringBuilder.ToString());
    }
}
