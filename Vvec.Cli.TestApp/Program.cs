using System.CommandLine;
using System.Linq;
using Vvec.Cli.Arguments;
using Vvec.Cli.UI;

var entryPoint = new EntryPoint(args, "CLI tool for testing my CLI library.");
return entryPoint
    .Group("Interesting Functionality")
    .Register<ShowColoursCommand>()
    .Register<FurtleArgsCommand>()
    .Register<RunSourceGeneratorCommand>()
    .Group("Long help text stuff")
    .Register<FurtleCommand>()
    .Register<NoDescriptionCommand>()
    .Register<VeryLongCommand>()
    .Execute();



