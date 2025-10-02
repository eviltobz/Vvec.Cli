using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace WiredBrainCoffee.Generators;

[Generator]
public class ToStringGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classes = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node);

        context.RegisterSourceOutput(classes,
            static (ctx, source) => Execute(ctx, source));
    }

    private static void Execute(SourceProductionContext context,
        ClassDeclarationSyntax classDeclarationSyntax)
    {
        if (classDeclarationSyntax.Parent
            is BaseNamespaceDeclarationSyntax namespaceDeclarationSyntax)
        {
            var namespaceName = namespaceDeclarationSyntax.Name.ToString();
            var className = classDeclarationSyntax.Identifier.Text;
            var fileName = $"{namespaceName}.{className}.g.cs";

            var stringBuilder = new StringBuilder();
            stringBuilder.Append($@"namespace {namespaceName}
{{
    partial class {className}
    {{
        public override string ToString()
        {{
            return $""");

            var first = true;
            foreach (var memberDeclarationSyntax in classDeclarationSyntax.Members)
            {
                if (memberDeclarationSyntax
                    is PropertyDeclarationSyntax propertyDeclarationSyntax
                    && propertyDeclarationSyntax.Modifiers.Any(SyntaxKind.PublicKeyword))
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        stringBuilder.Append("; ");
                    }
                    var propertyName = propertyDeclarationSyntax.Identifier.Text;
                    stringBuilder.Append($"{propertyName}:{{{propertyName}}}");
                }
            }

            stringBuilder.Append($@""";
        }}
    }}
}}
");

            context.AddSource(fileName, stringBuilder.ToString());
        }
    }
}
