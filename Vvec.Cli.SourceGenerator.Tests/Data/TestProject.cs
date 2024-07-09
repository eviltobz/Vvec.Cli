using System.Reflection;
using Microsoft.CodeAnalysis;
using Vvec.Cli.Arguments;

namespace ZeroIoC.Tests.Data
{
    public static class TestProject
    {
        public const string ProgramCs = @"
using System;
using Vvec.Cli;
using Vvec.Cli.Arguments;

namespace TestProject 
{
    // place to replace

    class Program
    {
        static void Main(string[] args)
        {
            //var entryPoint = new EntryPoint(args, ""CLI tool for testing my CLI library."");
            //return entryPoint
                //.Group(""Interesting Functionality"")
                //.Register<BasicCommand>()
                //.Execute();
        }
    }



public class BasicCommand : ISubCommand
{

    public BasicCommand()
    {
    }

    public static string Name => ""basic"";

    public static string Description => ""basic command for testing"";

    public void Execute()
    {
        // Do nothing
    }
}


}
";

        static TestProject()
        {
            var workspace = new AdhocWorkspace();
            Project = workspace
                .AddProject("TestProject", LanguageNames.CSharp)
                .WithMetadataReferences(GetReferences())
                .AddDocument("Program.cs", ProgramCs).Project;
        }

        public static Project Project { get; }

        private static MetadataReference[] GetReferences()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return new MetadataReference[]
            {
                MetadataReference.CreateFromFile(assemblies.Single(a => a.GetName().Name == "netstandard").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Initialiser).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.CommandLine").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
            };
        }
    }
}