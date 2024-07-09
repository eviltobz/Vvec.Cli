using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Vvec.Cli.SourceGenerator
{
    [Generator]
    public class CommandRegistrationGenerator : IIncrementalGenerator
    {
        private static string debugString = "";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource(
                    "EntryPoint.g.cs",
                    SourceText.From(EntryPointSource.Source, Encoding.UTF8));
            });

            IncrementalValuesProvider<ClassDeclarationSyntax?> classDeclarations =
                context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (s, _) => s is ClassDeclarationSyntax c,
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(static x => x is not null);

            // Combine the class declarations with the `Compilation`
            IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses
                = context.CompilationProvider.Combine(classDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses,
                static (spc, source) => Execute(source.Item1, source.Item2, spc));

        }

        private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext thingy)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)thingy.Node;

            var bases = classDeclarationSyntax?.BaseList?.Types ?? new SeparatedSyntaxList<BaseTypeSyntax>();

            // This is a bit vague as we've not got the fully qualified names of the interfaces, so could do
            // with being tightened up
            if (bases.Any(b => b.ToString() == "ISubCommand" || b.ToString() == "ISubCommandAsync"))
                return classDeclarationSyntax;

            return null;
        }

        static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
        {
            if (classes.IsDefaultOrEmpty)
            {
                return;
            }

            IEnumerable<ClassDeclarationSyntax> distinctClasses = classes.Distinct();

            var implementor = new CommandRegistrationGeneratorImpl(compilation, context.CancellationToken);
            List<ClassToRegister> classesToRegister = implementor.GetTypesToGenerate(distinctClasses);

            if (classesToRegister.Any())
            {
                string result = RegistrationSourceGenerationHelper.GenerateClassStuff(classesToRegister);
                context.AddSource("CommandRegistration.g.cs", SourceText.From(result, Encoding.UTF8));
            }
        }
    }

    internal class CommandRegistrationGeneratorImpl
    {
        private readonly Compilation compilation;
        private readonly CancellationToken ct;
        private readonly INamedTypeSymbol argAttributeSymbol;
        private readonly INamedTypeSymbol optAttributeSymbol;
        private readonly INamedTypeSymbol[] knownCliAttributes;

        public CommandRegistrationGeneratorImpl(Compilation compilation, CancellationToken ct)
        {
            this.ct = ct;
            this.compilation = compilation;

            argAttributeSymbol = compilation.GetTypeByMetadataName("Vvec.Cli.Arguments.ArgAttribute")!;
            optAttributeSymbol = compilation.GetTypeByMetadataName("Vvec.Cli.Arguments.OptAttribute")!;
            knownCliAttributes = new[] { argAttributeSymbol, optAttributeSymbol };
        }


        internal List<ClassToRegister> GetTypesToGenerate(IEnumerable<ClassDeclarationSyntax> classes)
        {
            var classesToRegister = new List<ClassToRegister>();

            foreach (ClassDeclarationSyntax clss in classes)
            {
                ct.ThrowIfCancellationRequested();

                SemanticModel semanticModel = compilation.GetSemanticModel(clss.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(clss) is not INamedTypeSymbol symbol)
                {
                    continue;
                }

                string symbolName = symbol.ToString();

                (CliModifier[] args, string debuggery) = GetModifiers(clss, compilation, symbol);

                classesToRegister.Add(new ClassToRegister(symbolName, args, debuggery));
            }

            return classesToRegister;
        }

        private (CliModifier[], string) GetModifiers(ClassDeclarationSyntax clss, Compilation compilation, INamedTypeSymbol symbol)
        {
            var members = symbol.GetMembers()
                .Where(m => m.Kind == SymbolKind.Property && m.GetAttributes().Any(a => knownCliAttributes.Contains(a.AttributeClass))) // == "ArgAttribute"))
                .Cast<IPropertySymbol>()
                .ToArray();

            string debuggery = null;
            var arguments = new CliModifier[members.Length];

            debuggery = "Attributed Members: ";
            for (int i = 0; i < members.Length; i++)
            {
                var member = members[i];
                var attr = member.GetAttributes().Single(a => knownCliAttributes.Contains(a.AttributeClass));
                // Should prolly try to add some error message if we have multiple CLI attributes on a member
                if (attr.AttributeClass == argAttributeSymbol)
                {
                    var cliName = (string)attr.ConstructorArguments[0].Value;
                    var cliDescription = (string)attr.ConstructorArguments[1].Value;
                    arguments[i] = new Argument(member.Name, member.Type.ToString(), cliName, cliDescription);
                }
                else
                {
                    var cliFlag = (char)attr.ConstructorArguments[0].Value;
                    var cliName = (string)attr.ConstructorArguments[1].Value;
                    var cliDescription = (string)attr.ConstructorArguments[2].Value;
                    arguments[i] = new Option(member.Name, member.Type.ToString(), cliFlag, cliName, cliDescription);
                }
            }

            return (arguments, debuggery);
        }
    }
}
