using System.Collections.Immutable;
using System.Linq;
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

        private static string debuggery = "";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Add the marker attribute to the compilation
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

            // Generate the source using the compilation and enums
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

        //static bool IsSyntaxTargetForGeneration(SyntaxNode node) =>
        //    node is EnumDeclarationSyntax m && m.AttributeLists.Count > 0;

        //static EnumDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        //{
        //    // we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
        //    var enumDeclarationSyntax = (EnumDeclarationSyntax)context.Node;

        //    // loop through all the attributes on the method
        //    foreach (AttributeListSyntax attributeListSyntax in enumDeclarationSyntax.AttributeLists)
        //    {
        //        foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
        //        {
        //            if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
        //            {
        //                // weird, we couldn't get the symbol, ignore it
        //                continue;
        //            }

        //            INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
        //            string fullName = attributeContainingTypeSymbol.ToDisplayString();

        //            // Is the attribute the [EnumExtensions] attribute?
        //            if (fullName == "Vvec.Sg.EnumExtensionsAttribute")
        //            {
        //                // return the enum
        //                return enumDeclarationSyntax;
        //            }
        //        }
        //    }

        //    // we didn't find the attribute we were looking for
        //    return null;
        //}

        static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
        {
            if (classes.IsDefaultOrEmpty)
            {
                // nothing to do yet
                return;
            }

            // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
            IEnumerable<ClassDeclarationSyntax> distinctClasses = classes.Distinct();

            // Convert each EnumDeclarationSyntax to an EnumToGenerate
            var implementor = new CommandRegistrationGeneratorImpl(compilation, context.CancellationToken);
            //List<ClassToRegister> classesToRegister = GetTypesToGenerate(compilation, distinctClasses, context.CancellationToken);
            List<ClassToRegister> classesToRegister = implementor.GetTypesToGenerate(distinctClasses);

            // If there were errors in the EnumDeclarationSyntax, we won't create an
            // EnumToGenerate for it, so make sure we have something to generate
            if (classesToRegister.Any())
            {
                // generate the source code and add it to the output
                string result = RegistrationSourceGenerationHelper.GenerateClassStuff(classesToRegister);
                context.AddSource("ClassStuff.g.cs", SourceText.From(result, Encoding.UTF8));
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


            // Create a list to hold our output
            var classesToRegister = new List<ClassToRegister>();
            // Get the semantic representation of our marker attribute
            //INamedTypeSymbol? enumAttribute = compilation.GetTypeByMetadataName("Vvec.Sg.EnumExtensionsAttribute");

            //if (enumAttribute == null)
            //{
            //    // If this is null, the compilation couldn't find the marker attribute type
            //    // which suggests there's something very wrong! Bail out..
            //    return enumsToGenerate;
            //}

            foreach (ClassDeclarationSyntax clss in classes)
            {
                // stop if we're asked to
                ct.ThrowIfCancellationRequested();

                // Get the semantic representation of the enum syntax
                SemanticModel semanticModel = compilation.GetSemanticModel(clss.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(clss) is not INamedTypeSymbol symbol)
                {
                    // something went wrong, bail out
                    continue;
                }

                // Get the full type name of the enum e.g. Colour,
                // or OuterClass<T>.Colour if it was nested in a generic type (for example)
                string symbolName = symbol.ToString();

                // Get all the members in the enum
                //ImmutableArray<ISymbol> enumMembers = symbol.GetMembers();
                //var members = new List<string>(enumMembers.Length);

                //// Get all the fields from the enum, and add their name to the list
                //foreach (ISymbol member in enumMembers)
                //{
                //    if (member is IFieldSymbol field && field.ConstantValue is not null)
                //    {
                //        members.Add(member.Name);
                //    }
                //}

                //var bases = clss?.BaseList?.Types;
                //var members = bases is null
                //    ? new List<string>()
                //    : clss!.BaseList!.Types.Select(b => b.ToFullString()).ToList();
                //var members = new List<string>() { "meh" };

                (CliModifier[] args, string debuggery) = GetModifiers(clss, compilation, symbol);

                //Argument[] args = new Argument[0];

                //var members = symbol.GetMembers().Where(m => m.GetAttributes().Any());
                //string debuggery = null;
                //if (members.Any())
                //{
                //    debuggery = "Attributed Members: ";
                //    foreach (var member in members)
                //    {
                //        var cliAttributes = member.GetAttributes().Where(a => knownCliAttributes.Contains(a.AttributeClass)).ToArray();
                //        if (cliAttributes.Any())
                //        {
                //            debuggery += "\n  " + member.Name + " [";
                //            foreach (var attr in cliAttributes)
                //            {
                //                debuggery += attr.AttributeClass.ToString() + " ";
                //            }
                //            debuggery += member.ToString() + " ]";
                //        }
                //    }
                //}

                classesToRegister.Add(new ClassToRegister(symbolName, args, null));
            }

            return classesToRegister;
        }

        private (CliModifier[], string) GetModifiers(ClassDeclarationSyntax clss, Compilation compilation, INamedTypeSymbol symbol)
        {
            //SemanticModel semanticModel = compilation.GetSemanticModel(clss.SyntaxTree);
            //if (semanticModel.GetDeclaredSymbol(clss) is not INamedTypeSymbol symbol)

            var members = symbol.GetMembers()
                .Where(m => m.Kind == SymbolKind.Property && m.GetAttributes().Any(a => knownCliAttributes.Contains(a.AttributeClass))) // == "ArgAttribute"))
                .Cast<IPropertySymbol>()
                .ToArray();

            string debuggery = null;
            var arguments = new CliModifier[members.Length];

            //var members = clss?.Members ?? new SyntaxList<MemberDeclarationSyntax>();
            //var members = symbol.GetMembers().Where(m => m.GetAttributes().Any());
            debuggery = "Attributed Members: ";
            for (int i = 0; i < members.Length; i++)
            {
                var member = members[i];
                var attr = member.GetAttributes().Single(a => knownCliAttributes.Contains(a.AttributeClass));
                // Should prolly try to add some error message if we have multiple CLI attributes on a member
                //debuggery += "\n  " + member.Name + " [";
                //debuggery += attr.AttributeClass.ToString() + " ";
                //debuggery += member.ToString() + " ]";
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

            debuggery = "DEBUGGERY:";
            {
                var tobz = "meh";
                debuggery += tobz;
            }
            {
                var tobz = "narf";
                debuggery += tobz;
            }


            //var args = members.Select(m => new Argument(m.Name, m.Type.ToString())).ToArray();
            return (arguments, debuggery);
        }

    }
}
